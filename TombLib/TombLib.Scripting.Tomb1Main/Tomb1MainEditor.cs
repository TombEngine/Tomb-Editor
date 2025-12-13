#nullable enable

using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Objects;
using TombLib.Scripting.Tomb1Main.Objects;
using TombLib.Scripting.Tomb1Main.Parsers;
using TombLib.Scripting.Tomb1Main.Services;
using TombLib.Scripting.Tomb1Main.Services.Implementations;
using TombLib.Scripting.Tomb1Main.Utils;
using TombLib.Scripting.Workers;

namespace TombLib.Scripting.Tomb1Main
{
	public sealed class Tomb1MainEditor : TextEditorBase
	{
		public override string DefaultFileExtension => ".json5";

		private bool _suppressBracketAutospacing;
		private DocumentLine? _cachedLine;
		private readonly ErrorDetectionWorker _errorDetectionWorker;

		private readonly IGameflowSchemaService _schemaService;
		private readonly IGameflowAutocompleteService _autocompleteService;
		private readonly IGameflowHoverService _hoverService;
		private readonly ITextAnalysisService _textAnalysisService;
		private readonly IAutocompleteManager _autocompleteManager;

		public Tomb1MainEditor(Version engineVersion) : this(engineVersion, false)
		{ }

		public Tomb1MainEditor(Version engineVersion, bool isTR2) : base(engineVersion)
		{
			// Initialize schema services based on game version
			string schemaFileName = isTR2 ? "tr2.gameflow.schema.json" : "tr1.gameflow.schema.json";
			string schemaPath = Path.Combine(DefaultPaths.ResourcesDirectory, "TRX", schemaFileName);

			_schemaService = new GameflowSchemaService(schemaPath);
			GameflowSchemaService.Instance = _schemaService; // TODO: Remove singleton usage

			_autocompleteService = new GameflowAutocompleteService(_schemaService);
			_hoverService = new GameflowHoverService(_schemaService);
			_textAnalysisService = new TextAnalysisService();
			_autocompleteManager = new AutocompleteManager();

			// Initialize error detection
			_errorDetectionWorker = new ErrorDetectionWorker(new ErrorDetector(), EngineVersion, new TimeSpan(500));
			_errorDetectionWorker.RunWorkerCompleted += ErrorWorker_RunWorkerCompleted;

			// Initialize other properties
			BindEventMethods();
			CommentPrefix = "//";
		}

		private void BindEventMethods()
		{
			TextArea.TextEntering += TextArea_TextEntering;
			TextArea.TextEntered += TextEditor_TextEntered;
			TextChanged += TextEditor_TextChanged;
			TextArea.TextView.MouseHover += TextView_MouseHover;
		}

		#region Event handlers

		private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
		{
			// Handle Ctrl+Space autocomplete
			if (AutocompleteEnabled && IsCtrlSpaceInput(e.Text, Keyboard.Modifiers))
			{
				HandleCtrlSpaceAutocomplete(e);
				return;
			}

			// Handle bracket autospacing
			if (e.Text == "\n" && CaretOffset > 0 && CaretOffset < Document.TextLength)
			{
				char? prevChar = GetPreviousChar();
				char? nextChar = GetNextChar();

				if (prevChar.HasValue && nextChar.HasValue)
				{
					char prev = prevChar.Value;
					char next = nextChar.Value;

					if ((prev == '{' && next == '}') || (prev == '[' && next == ']'))
						_suppressBracketAutospacing = true;
				}
			}
		}

		private void TextEditor_TextEntered(object sender, TextCompositionEventArgs e)
		{
			if (AutocompleteEnabled)
			{
				bool completionWindowIsOpen = _completionWindow is not null;

				if (completionWindowIsOpen)
					UpdateExistingCompletionWindow();
				else
					HandleAutocompleteOnTextEntered(e);
			}

			HandleBracketAutospacing();
		}

		private void TextEditor_TextChanged(object? sender, EventArgs e)
		{
			if (LiveErrorUnderlining)
				_errorDetectionWorker.RunErrorCheckOnIdle(Text);
		}

		private void TextView_MouseHover(object? sender, MouseEventArgs e)
		{
			int hoveredOffset = GetOffsetFromPoint(e.GetPosition(this));
			string? hoverInfo = _hoverService.GetHoverInfo(Document, hoveredOffset);

			if (!string.IsNullOrEmpty(hoverInfo))
				ShowToolTip(hoverInfo);
		}

		private void ErrorWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Result is null)
				return;

			ResetAllErrors();
			ApplyErrorsToLines(e.Result as List<ErrorLine>);
			TextArea.TextView.InvalidateLayer(KnownLayer.Caret);
		}

		#endregion Event handlers

		#region Text input handling

		private static bool IsCtrlSpaceInput(string inputText, ModifierKeys modifiers)
			=> inputText == " " && modifiers.HasFlag(ModifierKeys.Control);

		private static bool ShouldTriggerAutocomplete(string inputText)
			=> inputText == "\"";

		#endregion Text input handling

		#region Autocomplete handling

		private void HandleCtrlSpaceAutocomplete(TextCompositionEventArgs e)
		{
			// Only allow Ctrl+Space if caret is at end of word or in whitespace
			if (_textAnalysisService.IsValidPositionForCtrlSpaceAutocomplete(Document, CaretOffset) && _completionWindow is null)
			{
				string currentWord = _textAnalysisService.GetCurrentWordBeingTyped(Document, CaretOffset);
				TryShowCompletionWindow(currentWord);
			}

			e.Handled = true; // Prevents the space character from being inserted
		}

		private void HandleAutocompleteOnTextEntered(TextCompositionEventArgs e)
		{
			if (ShouldTriggerAutocomplete(e.Text))
			{
				if (_textAnalysisService.IsValidContextForAutocomplete(Document, CaretOffset))
				{
					string currentWord = _textAnalysisService.GetCurrentWordBeingTyped(Document, CaretOffset);
					TryShowCompletionWindow(currentWord);
				}
			}
			else
			{
				TryHandleAutocompleteOnEmptyLine();
			}
		}

		private void UpdateExistingCompletionWindow()
		{
			// Check if the current text still has valid completions
			string currentText = _textAnalysisService.GetCurrentWordBeingTyped(Document, CaretOffset);
			var autocompleteData = _autocompleteService.GetAutocompleteData();
			var matchingCompletions = _autocompleteManager.FilterCompletions(autocompleteData, currentText);

			if (matchingCompletions.Count == 0)
			{
				_completionWindow.Close();
				_completionWindow = null;
			}
		}

		private void TryHandleAutocompleteOnEmptyLine()
		{
			if (_autocompleteManager.ShouldTriggerAutocompleteOnEmptyLine(Document, CaretOffset))
			{
				string currentWord = _textAnalysisService.GetCurrentWordBeingTyped(Document, CaretOffset);
				TryShowCompletionWindow(currentWord);
			}
		}

		private bool TryShowCompletionWindow(string currentWord = "")
		{
			var autocompleteData = _autocompleteService.GetAutocompleteData();

			if (autocompleteData.Count == 0)
				return false;

			var filteredCompletions = _autocompleteManager.FilterCompletions(autocompleteData, currentWord);

			if (filteredCompletions.Count == 0)
				return false;

			InitializeCompletionWindow();
			SetCompletionWindowOffsets(currentWord);

			foreach (ICompletionData item in filteredCompletions)
				_completionWindow.CompletionList.CompletionData.Add(item);

			ShowCompletionWindow();
			return true;
		}

		private void SetCompletionWindowOffsets(string currentWord)
		{
			var (startOffset, endOffset) = _autocompleteManager.GetCompletionWindowOffsets(Document, CaretOffset, currentWord);
			_completionWindow.StartOffset = startOffset;
			_completionWindow.EndOffset = endOffset;
		}

		#endregion Autocomplete handling

		#region Text manipulation helpers

		private char? GetPreviousChar()
			=> CaretOffset > 0 ? Document.GetCharAt(CaretOffset - 1) : null;

		private char? GetNextChar()
			=> CaretOffset < Document.TextLength ? Document.GetCharAt(CaretOffset) : null;

		private void HandleBracketAutospacing()
		{
			if (!_suppressBracketAutospacing || _cachedLine is not null)
				return;

			_cachedLine = Document.GetLineByOffset(CaretOffset);

			TextArea.PerformTextInput("\n");
			CaretOffset = _cachedLine.EndOffset;
			TextArea.PerformTextInput("\t");

			_cachedLine = null;
			_suppressBracketAutospacing = false;
		}

		#endregion Text manipulation helpers

		#region Public methods

		public override void TidyCode(bool trimOnly = false)
		{
			Vector scrollOffset = TextArea.TextView.ScrollOffset;

			SelectAll();
			SelectedText = BasicCleaner.TrimEndingWhitespace(Text);
			ResetSelection();

			ScrollToHorizontalOffset(scrollOffset.X);
			ScrollToVerticalOffset(scrollOffset.Y);
		}

		public override void UpdateSettings(Bases.ConfigurationBase configuration)
		{
			var config = configuration as T1MEditorConfiguration;

			SyntaxHighlighting = new SyntaxHighlighting(config!.ColorScheme, _schemaService);

			Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Background));
			Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Foreground));

			BracesClosingString = config.AutoAddCommas ? "}," : "}";
			BracketsClosingString = config.AutoAddCommas ? "]," : "]";

			base.UpdateSettings(configuration);
		}

		public override void GoToObject(string objectName, object? identifyingObject = null)
		{
			DocumentLine objectLine = DocumentParser.FindDocumentLineOfLevel(Document, objectName);

			if (objectLine is not null)
			{
				Focus();
				ScrollToLine(objectLine.LineNumber);
				SelectLine(objectLine);
			}
		}

		#endregion Public methods
	}
}
