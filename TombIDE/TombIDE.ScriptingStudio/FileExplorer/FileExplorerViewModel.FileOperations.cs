#nullable enable

using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic.FileIO;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Windows.Interop;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.Shared.SharedClasses;
using TombLib.Forms.ViewModels;
using TombLib.Forms.Views;
using Nickelony.IDEKit.Workspace.Documents;

namespace TombIDE.ScriptingStudio.FileExplorer;

/// <summary>
/// Implements file and folder creation, deletion, rename, open, and workspace operations.
/// </summary>
public sealed partial class FileExplorerViewModel
{
	public string? CreateNewFile()
	{
		string? targetDirectory = GetTargetDirectoryPath();

		if (string.IsNullOrWhiteSpace(targetDirectory))
			return null;

		string suggestedName = "untitled" + GetDefaultFileExtension();

		while (true)
		{
			string? requestedName = PromptForName(_localizationService["NewFileTitle"], _localizationService["EnterFileName"], suggestedName);

			if (requestedName is null)
				return null;

			string finalName = EnsureDefaultExtension(requestedName);
			string filePath = Path.Combine(targetDirectory, finalName);

			if (File.Exists(filePath))
			{
				_messageService.ShowError(_localizationService["FileAlreadyExistsMessage"], _localizationService["ErrorTitle"]);
				suggestedName = requestedName;
				continue;
			}

			try
			{
				string relativePath = filePath.StartsWith(RootDirectoryPath, StringComparison.OrdinalIgnoreCase)
					? filePath[RootDirectoryPath.Length..]
					: filePath;
				string content = $"{CommentPrefix} FILE: {relativePath}{Environment.NewLine}";

				if (_documentManager is not null)
				{
					WorkspaceDocumentManagerOpenResult openResult = _documentManager
						.OpenAsync(filePath, GetWorkspaceOpenOptions())
						.GetAwaiter()
						.GetResult();
					if (openResult.Snapshot is not WorkspaceDocumentSnapshot snapshot)
						throw new IOException(openResult.Failure?.Message ?? "The file could not be opened.");

					WorkspaceDocumentManagerMutationResult replacement = _documentManager
						.ReplaceAsync(new WorkspaceDocumentReplaceRequest(
							new(snapshot.DocumentKey, snapshot.DocumentId, snapshot.Version),
							content,
							new TextFileFormat(
								TextEncodingKind.Utf8,
								false,
								TextNewlineStyle.Lf)))
						.GetAwaiter()
						.GetResult();
					if (replacement.Snapshot is not WorkspaceDocumentSnapshot replacementSnapshot)
						throw new IOException("The file content could not be prepared.");

					WorkspaceDocumentManagerCommitResult commit = _documentManager
						.CommitAsync(
							new WorkspaceDocumentCommitRequest(
								new(replacementSnapshot.DocumentKey, replacementSnapshot.DocumentId, replacementSnapshot.Version),
								replacementSnapshot.OnDiskStamp))
						.GetAwaiter()
						.GetResult();
					if (commit.Outcome != WorkspaceDocumentCommitOutcome.Committed
						|| commit.Views.Outcome != WorkspaceDocumentViewSynchronizationOutcome.Synchronized)
						throw new IOException(commit.StoreResult?.Failure?.Message ?? "The file could not be saved.");
				}
				else
				{
					File.WriteAllText(filePath, content);
				}

				UpdateFileList();
				FileOpened?.Invoke(this, new FileOpenedEventArgs(filePath));
				return filePath;
			}
			catch (Exception ex) when (ex is ArgumentException or IOException or NotSupportedException or UnauthorizedAccessException)
			{
				_messageService.ShowError(ex.Message, _localizationService["ErrorTitle"]);
				suggestedName = requestedName;
			}
		}
	}

	public void CreateNewFolder()
	{
		string? targetDirectory = GetTargetDirectoryPath();

		if (string.IsNullOrWhiteSpace(targetDirectory))
			return;

		string suggestedName = _localizationService["DefaultFolderName"];

		while (true)
		{
			string? requestedName = PromptForName(_localizationService["NewFolderTitle"], _localizationService["EnterFolderName"], suggestedName);

			if (requestedName is null)
				return;

			string folderPath = Path.Combine(targetDirectory, requestedName);

			if (Directory.Exists(folderPath))
			{
				_messageService.ShowError(_localizationService["FolderAlreadyExistsMessage"], _localizationService["ErrorTitle"]);
				suggestedName = requestedName;
				continue;
			}

			try
			{
				Directory.CreateDirectory(folderPath);
				UpdateFileList();
				return;
			}
			catch (Exception ex) when (ex is ArgumentException or IOException or NotSupportedException or UnauthorizedAccessException)
			{
				_messageService.ShowError(ex.Message, _localizationService["ErrorTitle"]);
				suggestedName = requestedName;
			}
		}
	}

	[RelayCommand(CanExecute = nameof(CanCreateItems))]
	private void CreateNewFileRequested()
		=> CreateNewFile();

	[RelayCommand(CanExecute = nameof(CanCreateItems))]
	private void CreateNewFolderRequested()
		=> CreateNewFolder();

	[RelayCommand(CanExecute = nameof(CanDeleteSelectedItem))]
	private void DeleteSelectedItem()
	{
		if (SelectedItem is null)
			return;

		string message = SelectedItem.IsDirectory
			? _localizationService.Format("DeleteFolderMessage", SelectedItem.Name)
			: _localizationService.Format("DeleteFileMessage", SelectedItem.Name);

		bool confirmed = _messageService.ShowConfirmation(
			message,
			_localizationService["DeleteTitle"],
			defaultValue: false,
			isRisky: true);

		if (!confirmed)
			return;

		try
		{
			if (SelectedItem.IsDirectory && _documentManager is not null)
			{
				if (!PrepareRetainedDocumentsForDelete(_documentManager.Documents.GetSnapshotsUnderDirectory(SelectedItem.FullPath)))
					return;

				WorkspaceDocumentManagerDirectoryDeleteResult result = _documentManager
					.DeleteDirectoryAsync(new WorkspaceDocumentDirectoryDeleteRequest(
						SelectedItem.FullPath))
					.GetAwaiter()
					.GetResult();
				if (result.Outcome != WorkspaceDocumentDirectoryDeleteOutcome.Deleted
					|| result.Views.Outcome != WorkspaceDocumentViewSynchronizationOutcome.Synchronized)
					throw new IOException(result.StoreResult?.Failure?.Message ?? "The folder could not be deleted.");
			}
			else if (SelectedItem.IsDirectory)
				FileSystem.DeleteDirectory(SelectedItem.FullPath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
			else if (_documentManager is not null)
			{
				WorkspaceDocumentSnapshot snapshot = OpenWorkspaceSnapshot(SelectedItem.FullPath);
				if (!PrepareRetainedDocumentsForDelete([snapshot]))
					return;

				snapshot = OpenWorkspaceSnapshot(SelectedItem.FullPath);

				WorkspaceDocumentManagerDeleteResult result = _documentManager
					.DeleteAsync(
						new WorkspaceDocumentDeleteRequest(
							new(snapshot.DocumentKey, snapshot.DocumentId, snapshot.Version),
							snapshot.OnDiskStamp))
					.GetAwaiter()
					.GetResult();
				if (result.Outcome != WorkspaceDocumentDeleteOutcome.Deleted
					|| result.Views.Outcome != WorkspaceDocumentViewSynchronizationOutcome.Synchronized)
					throw new IOException(result.StoreResult?.Failure?.Message ?? "The file could not be deleted.");
			}
			else
				FileSystem.DeleteFile(SelectedItem.FullPath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);

			UpdateFileList();
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
		{
			_messageService.ShowError(ex.Message, _localizationService["ErrorTitle"]);
		}
	}

	[RelayCommand(CanExecute = nameof(CanOpenInExplorer))]
	private void OpenInExplorer()
	{
		if (SelectedItem is null)
			return;

		SharedMethods.OpenInExplorer(SelectedItem.FullPath);
	}

	[RelayCommand(CanExecute = nameof(CanOpenSelectedItem))]
	private void OpenSelectedItem()
	{
		if (SelectedItem is null || SelectedItem.IsDirectory)
			return;

		FileOpened?.Invoke(this, new FileOpenedEventArgs(SelectedItem.FullPath));
	}

	[RelayCommand(CanExecute = nameof(CanOpenSelectedItem))]
	private void OpenSourceView()
	{
		if (SelectedItem is null || SelectedItem.IsDirectory)
			return;

		FileOpened?.Invoke(this, FileOpenedEventArgs.CreateSourceView(SelectedItem.FullPath));
	}

	[RelayCommand(CanExecute = nameof(CanRenameSelectedItem))]
	private void RenameSelectedItem()
	{
		if (SelectedItem is null)
			return;

		string initialName = SelectedItem.IsDirectory
			? Path.GetFileName(SelectedItem.FullPath)
			: Path.GetFileNameWithoutExtension(SelectedItem.FullPath);

		string? parentDirectory = Path.GetDirectoryName(SelectedItem.FullPath);

		if (string.IsNullOrWhiteSpace(parentDirectory))
			return;

		while (true)
		{
			string? requestedName = PromptForName(_localizationService["RenameTitle"], _localizationService["EnterNewName"], initialName);

			if (requestedName is null || string.Equals(requestedName, initialName, StringComparison.Ordinal))
				return;

			string targetName = SelectedItem.IsDirectory
				? requestedName
				: requestedName + Path.GetExtension(SelectedItem.FullPath);

			string newPath = Path.Combine(parentDirectory, targetName);

			bool isCaseOnlyRename = string.Equals(
				SelectedItem.FullPath,
				newPath,
				StringComparison.OrdinalIgnoreCase)
				&& !string.Equals(SelectedItem.FullPath, newPath, StringComparison.Ordinal);
			if (!isCaseOnlyRename && (File.Exists(newPath) || Directory.Exists(newPath)))
			{
				_messageService.ShowError(_localizationService["ItemAlreadyExistsMessage"], _localizationService["ErrorTitle"]);
				initialName = requestedName;
				continue;
			}

			try
			{
				if (SelectedItem.IsDirectory && _documentManager is not null)
				{
					WorkspaceDocumentManagerDirectoryRenameResult result = _documentManager
						.RenameDirectoryAsync(new WorkspaceDocumentDirectoryRenameRequest(SelectedItem.FullPath, newPath))
						.GetAwaiter()
						.GetResult();
					if (result.Outcome != WorkspaceDocumentDirectoryRenameOutcome.Renamed
						|| result.Views.Outcome != WorkspaceDocumentViewSynchronizationOutcome.Synchronized)
						throw new IOException(result.StoreResult?.Failure?.Message ?? "The folder could not be renamed.");
				}
				else if (SelectedItem.IsDirectory)
					Directory.Move(SelectedItem.FullPath, newPath);
				else if (_documentManager is not null)
				{
					WorkspaceDocumentSnapshot snapshot = OpenWorkspaceSnapshot(SelectedItem.FullPath);
					WorkspaceDocumentManagerRenameResult result = _documentManager
						.RenameAsync(
							new WorkspaceDocumentRenameRequest(
								new(snapshot.DocumentKey, snapshot.DocumentId, snapshot.Version),
								snapshot.OnDiskStamp,
								newPath))
						.GetAwaiter()
						.GetResult();
					if (result.Outcome != WorkspaceDocumentRenameOutcome.Renamed
						|| result.Views.Outcome != WorkspaceDocumentViewSynchronizationOutcome.Synchronized)
						throw new IOException(result.StoreResult?.Failure?.Message ?? "The file could not be renamed.");
				}
				else
					File.Move(SelectedItem.FullPath, newPath);

				UpdateFileList();
				return;
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				_messageService.ShowError(ex.Message, _localizationService["ErrorTitle"]);
				initialName = requestedName;
			}
		}
	}

	private bool CanCreateItems()
		=> !string.IsNullOrWhiteSpace(RootDirectoryPath) && Directory.Exists(RootDirectoryPath);

	private bool CanDeleteSelectedItem()
		=> SelectedItem is not null && IsModifiableItem(SelectedItem);

	private bool CanOpenInExplorer()
		=> SelectedItem is not null;

	private bool CanOpenSelectedItem()
		=> SelectedItem?.IsFile is true && IsSupportedFileFormat(SelectedItem.FullPath);

	private bool CanRenameSelectedItem()
		=> SelectedItem is not null && IsModifiableItem(SelectedItem);

	private WorkspaceDocumentSnapshot OpenWorkspaceSnapshot(string path)
	{
			WorkspaceDocumentManagerOpenResult openResult = _documentManager!
			.OpenAsync(path, GetWorkspaceOpenOptions())
			.GetAwaiter()
			.GetResult();
		return openResult.Snapshot
			?? throw new IOException(openResult.Failure?.Message ?? "The file could not be opened.");
	}

	private bool PrepareRetainedDocumentsForDelete(IReadOnlyList<WorkspaceDocumentSnapshot> snapshots)
	{
		if (_documentManager is null)
			return true;

		foreach (WorkspaceDocumentSnapshot snapshot in snapshots)
		{
			if (!snapshot.IsDirty)
				continue;

			DialogResult choice = _messageService.ShowConfirmation(
				_localizationService.Format("UnsavedChangesMessage", snapshot.DisplayPath),
				_localizationService["UnsavedChangesTitle"],
				DialogResult.Yes,
				DialogResult.No,
				DialogResult.Cancel,
				DialogResult.Yes);

			WorkspaceDocumentManagerOpenResult openResult = _documentManager
				.OpenAsync(snapshot.DisplayPath, GetWorkspaceOpenOptions())
				.GetAwaiter()
				.GetResult();
			if (openResult.Snapshot is not WorkspaceDocumentSnapshot currentSnapshot)
				return false;

			if (choice == DialogResult.Yes)
			{
				WorkspaceDocumentManagerCommitResult commit = _documentManager
					.CommitAsync(new WorkspaceDocumentCommitRequest(
						new(currentSnapshot.DocumentKey, currentSnapshot.DocumentId, currentSnapshot.Version),
						currentSnapshot.OnDiskStamp))
					.GetAwaiter()
					.GetResult();
				if (commit.Outcome != WorkspaceDocumentCommitOutcome.Committed
					|| commit.Views.Outcome != WorkspaceDocumentViewSynchronizationOutcome.Synchronized)
					return false;
			}
			else if (choice == DialogResult.No)
			{
				WorkspaceDocumentManagerMutationResult discard = _documentManager
					.DiscardAsync(new WorkspaceDocumentDiscardRequest(
						new(currentSnapshot.DocumentKey, currentSnapshot.DocumentId, currentSnapshot.Version)))
					.GetAwaiter()
					.GetResult();
				if (discard.Outcome is not (WorkspaceDocumentMutationOutcome.Changed or WorkspaceDocumentMutationOutcome.NoChange))
					return false;
			}
			else
				return false;
		}

		return true;
	}

	private static WorkspaceDocumentOpenOptions GetWorkspaceOpenOptions()
		=> new(
			TextEncodingKind.Utf8,
			new TextFileFormat(TextEncodingKind.Utf8, false, TextNewlineStyle.Lf));

	private string EnsureDefaultExtension(string fileName)
	{
		if (!string.IsNullOrEmpty(Path.GetExtension(fileName)))
			return fileName;

		string defaultExtension = GetDefaultFileExtension();
		return string.IsNullOrEmpty(defaultExtension)
			? fileName
			: fileName + defaultExtension;
	}

	private string GetDefaultFileExtension()
	{
		foreach (string pattern in GetFilterPatterns())
		{
			if (string.Equals(pattern, "*.*", StringComparison.Ordinal))
				return string.Empty;

			if (pattern.StartsWith("*.", StringComparison.Ordinal))
				return pattern[1..];
		}

		return string.Empty;
	}

	private string? GetTargetDirectoryPath()
	{
		if (SelectedItem is null)
			return string.IsNullOrWhiteSpace(RootDirectoryPath) ? null : RootDirectoryPath;

		if (SelectedItem.IsDirectory)
			return SelectedItem.FullPath;

		return Path.GetDirectoryName(SelectedItem.FullPath);
	}

	private bool IsModifiableItem(FileExplorerItemViewModel item)
	{
		if (string.Equals(item.FullPath, RootDirectoryPath, StringComparison.OrdinalIgnoreCase))
			return false;

		if (!item.IsFile)
			return true;

		string scriptPath = Path.Combine(RootDirectoryPath, "script.txt");
		string defaultLanguagePath = Path.Combine(RootDirectoryPath, "english.txt");

		return !string.Equals(item.FullPath, scriptPath, StringComparison.OrdinalIgnoreCase)
			&& !string.Equals(item.FullPath, defaultLanguagePath, StringComparison.OrdinalIgnoreCase);
	}

	private string? PromptForName(string title, string label, string initialValue)
	{
		string currentValue = initialValue;

		while (true)
		{
			var inputBox = new InputBoxWindowViewModel(
				title: title,
				label: label,
				placeholder: currentValue);

			bool? dialogResult = ShowInputBoxDialog(inputBox);

			if (dialogResult is not true)
				return null;

			string sanitizedValue = PathHelper.RemoveIllegalPathSymbols(inputBox.Value).Trim();

			if (!string.IsNullOrWhiteSpace(sanitizedValue))
				return sanitizedValue;

			_messageService.ShowError(_localizationService["InvalidNameMessage"], _localizationService["ErrorTitle"]);
			currentValue = inputBox.Value;
		}
	}

	private bool? ShowInputBoxDialog(InputBoxWindowViewModel inputBox)
	{
		try
		{
			return _dialogService.ShowDialog(this, inputBox);
		}
		catch (ViewNotRegisteredException)
		{
			var window = new InputBoxWindow { DataContext = inputBox };
			PropertyChangedEventHandler? propertyChangedHandler = null;

			propertyChangedHandler = (_, e) =>
			{
				if (e.PropertyName == nameof(InputBoxWindowViewModel.DialogResult) && inputBox.DialogResult.HasValue)
					window.DialogResult = inputBox.DialogResult;
			};

			inputBox.PropertyChanged += propertyChangedHandler;

			try
			{
				if (_dialogOwnerProvider.GetOwner() is { } owner)
					new WindowInteropHelper(window).Owner = owner.Handle;

				return window.ShowDialog();
			}
			finally
			{
				inputBox.PropertyChanged -= propertyChangedHandler;
			}
		}
	}

	private void RunOnUiThread(Action action)
	{
		if (System.Windows.Application.Current?.Dispatcher is null || System.Windows.Application.Current.Dispatcher.CheckAccess())
		{
			action();
			return;
		}

		System.Windows.Application.Current.Dispatcher.BeginInvoke(action);
	}
}
