#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TombLib.Services;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.WPF;
using TombLib.WPF.Dialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace WadTool.Features.Dialogs.SpriteSequenceEditor;

/// <summary>
/// WPF port of <c>FormSpriteSequenceEditor</c>: edits the sprites of a <see cref="WadSpriteSequence"/>
/// (add/remove/reorder/replace/export, alignment fields). All edits happen on a working copy;
/// on OK they are written back to <see cref="SpriteSequence"/> and unsaved changes are flagged
/// on the tool, mirroring the dialog. The caller raises <c>WadChanged</c> afterwards.
/// </summary>
public partial class SpriteSequenceEditorWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly WadToolClass _tool;
    private readonly ILocalizationService _localization;
    private string _currentPath;

    // Texture-keyed like the legacy _imageCache: each sprite texture is converted to a WPF bitmap only once.
    private readonly Cache<WadTexture, ImageSource> _imageCache = new(1024, texture => CreateImage(texture.Image));

    public WadSpriteSequence SpriteSequence { get; }
    public Wad2 Wad { get; }

    public ObservableCollection<SpriteRowViewModel> Sprites { get; } = new();

    public IReadOnlyList<WadSprite.HorizontalAlignment> HorizontalAlignments { get; } =
        (WadSprite.HorizontalAlignment[])Enum.GetValues(typeof(WadSprite.HorizontalAlignment));
    public IReadOnlyList<WadSprite.VerticalAlignment> VerticalAlignments { get; } =
        (WadSprite.VerticalAlignment[])Enum.GetValues(typeof(WadSprite.VerticalAlignment));

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private string _title = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveUpCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveDownCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExportCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReplaceSpriteCommand))]
    [NotifyCanExecuteChangedFor(nameof(RecalculateAlignmentCommand))]
    private SpriteRowViewModel? _selectedSprite;

    // Legacy defaults: cmbHorAdj.SelectedIndex = 1 (Center), cmbVerAdj.SelectedIndex = 2 (Bottom), nudScale = 1.
    [ObservableProperty] private WadSprite.HorizontalAlignment _selectedHorizontalAlignment = WadSprite.HorizontalAlignment.Center;
    [ObservableProperty] private WadSprite.VerticalAlignment _selectedVerticalAlignment = WadSprite.VerticalAlignment.Bottom;
    [ObservableProperty] private double _scale = 1.0;

    public bool HasSelection => SelectedSprite is not null;

    public SpriteSequenceEditorWindowViewModel(WadToolClass tool, Wad2 wad, WadSpriteSequence spriteSequence,
        ILocalizationService? localizationService = null)
    {
        _localization = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        SpriteSequence = spriteSequence;
        Wad = wad;

        _tool = tool;
        _currentPath = wad.FileName;

        Title = string.Format(_localization["Title"], spriteSequence.Id.ToString(wad.GameVersion));

        foreach (WadSprite sprite in spriteSequence.Sprites)
            Sprites.Add(new SpriteRowViewModel(sprite, Sprites.Count, GetImage));

        // Refresh initially: like the legacy form, preselect the first sprite.
        if (Sprites.Count > 0)
            SelectedSprite = Sprites[0];
    }

    [RelayCommand]
    private void AddSprites()
    {
        List<WadSprite>? newSprites = AskForSpriteFiles();
        if (newSprites is null || newSprites.Count == 0)
            return;

        foreach (WadSprite sprite in newSprites)
            Sprites.Add(new SpriteRowViewModel(sprite, Sprites.Count, GetImage));

        // Legacy DarkDataGridViewControls ends up with the last added row selected and visible.
        SelectedSprite = Sprites[^1];
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Delete(IList? selectedItems)
    {
        foreach (SpriteRowViewModel row in RowsFrom(selectedItems))
            Sprites.Remove(row);

        Renumber();
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void MoveUp(IList? selectedItems)
    {
        // Same multi-selection algorithm as the legacy DarkDataGridViewControls up button.
        List<int> indices = RowsFrom(selectedItems).Select(row => Sprites.IndexOf(row)).ToList();

        int lastItemIndex = 0;
        foreach (int index in indices)
        {
            if (index == lastItemIndex++)
                continue;

            Sprites.Move(index - 1, index);
        }

        Renumber();
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void MoveDown(IList? selectedItems)
    {
        // Same multi-selection algorithm as the legacy DarkDataGridViewControls down button.
        List<int> indices = RowsFrom(selectedItems).Select(row => Sprites.IndexOf(row)).ToList();
        indices.Reverse();

        int lastItemIndex = Sprites.Count - 1;
        foreach (int index in indices)
        {
            if (index == lastItemIndex--)
                continue;

            Sprites.Move(index + 1, index);
        }

        Renumber();
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void ReplaceSprite()
    {
        if (SelectedSprite is null)
            return;

        var fileDialog = new OpenFileDialog();
        fileDialog.Filter = ImageC.FileExtensions.GetFilter();
        fileDialog.Multiselect = false;
        ApplyCurrentPath(fileDialog);
        fileDialog.Title = _localization["ReplaceDialogTitle"];

        bool? dialogResult = fileDialog.ShowDialog(Owner);
        _currentPath = fileDialog.FileName;
        if (dialogResult != true)
            return;

        try
        {
            var sprite = new WadSprite { Texture = new WadTexture(ImageC.FromFile(fileDialog.FileName)) };
            sprite.RecalculateAlignment();
            sprite.Texture.Image.SetColorDataForTransparentPixels(new ColorC(0, 0, 0));
            SelectedSprite.Replace(sprite);

            // Legacy resets the adjustment controls after a replace.
            Scale = 1.0;
            SelectedHorizontalAlignment = WadSprite.HorizontalAlignment.Center;
            SelectedVerticalAlignment = WadSprite.VerticalAlignment.Bottom;
        }
        catch (Exception exc)
        {
            logger.Error(exc, "Unable to open file '" + fileDialog.FileName + "'.");
            UserDialogs.Show(Owner,
                string.Format(_localization["UnableToLoadSpriteMessage"], fileDialog.FileName, exc),
                _localization["UnableToLoadSpriteTitle"],
                DialogButtons.OK, DialogIcon.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Export(IList? selectedItems)
    {
        List<SpriteRowViewModel> rows = RowsFrom(selectedItems);
        if (rows.Count == 0)
            return;

        var fileDialog = new SaveFileDialog();
        fileDialog.Filter = ImageC.SaveFileFileExtensions.GetFilter(true);
        if (!string.IsNullOrWhiteSpace(_currentPath))
            try
            {
                fileDialog.InitialDirectory = Path.GetDirectoryName(_currentPath);
                fileDialog.FileName = "Untitled";
            }
            catch { }
        fileDialog.Title = _localization["ExportDialogTitle"];
        fileDialog.AddExtension = true;

        bool? dialogResult = fileDialog.ShowDialog(Owner);
        _currentPath = fileDialog.FileName;
        if (dialogResult != true)
            return;

        try
        {
            foreach (SpriteRowViewModel row in rows)
            {
                string fileName = fileDialog.FileName;
                if (rows.Count > 1)
                    fileName = Path.Combine(Path.GetDirectoryName(fileName)!,
                        Path.GetFileNameWithoutExtension(fileName) + Sprites.IndexOf(row).ToString("0000") + Path.GetExtension(fileName));
                row.Sprite.Texture.Image.SaveToFile(fileName);
            }
        }
        catch (Exception exc)
        {
            logger.Error(exc, "Unable to save file '" + fileDialog.FileName + "'.");
            UserDialogs.Show(Owner,
                string.Format(_localization["UnableToSaveSpriteMessage"], exc),
                _localization["UnableToSaveSpriteTitle"],
                DialogIcon.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void RecalculateAlignment()
        => SelectedSprite?.RecalculateAlignment(SelectedHorizontalAlignment, SelectedVerticalAlignment, (float)Scale);

    [RelayCommand]
    private void Confirm()
    {
        // Update data
        SpriteSequence.Sprites.Clear();
        SpriteSequence.Sprites.AddRange(Sprites.Select(row => row.Sprite));

        _tool.ToggleUnsavedChanges();

        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel() => DialogResult = false;

    /// <summary>Ports the legacy <c>newObject()</c> callback, including the retry/ignore/abort loop.</summary>
    private List<WadSprite>? AskForSpriteFiles()
    {
        var fileDialog = new OpenFileDialog();
        fileDialog.Filter = ImageC.FileExtensions.GetFilter();
        fileDialog.Multiselect = true;
        ApplyCurrentPath(fileDialog);
        fileDialog.Title = _localization["ImportDialogTitle"];

        bool? dialogResult = fileDialog.ShowDialog(Owner);
        _currentPath = fileDialog.FileName;
        if (dialogResult != true)
            return null;

        // Load sprites
        var sprites = new List<WadSprite>();
        foreach (string fileName in fileDialog.FileNames)
        {
            while (true)
            {
                try
                {
                    var newSprite = new WadSprite { Texture = new WadTexture(ImageC.FromFile(fileName)) };
                    newSprite.RecalculateAlignment();
                    newSprite.Texture.Image.SetColorDataForTransparentPixels(new ColorC(0, 0, 0));
                    sprites.Add(newSprite);
                    break;
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Unable to open file '" + fileName + "'.");
                    DialogChoice result = UserDialogs.Show(Owner,
                        string.Format(_localization["UnableToLoadSpriteMessage"], fileName, exc),
                        _localization["UnableToLoadSpriteTitle"],
                        fileDialog.FileNames.Length == 1 ? DialogButtons.RetryCancel : DialogButtons.AbortRetryIgnore,
                        DialogIcon.Error,
                        fileDialog.FileNames.Length == 1 ? DialogDefaultButton.Button2 : DialogDefaultButton.Button1);

                    if (result == DialogChoice.Ignore)
                        break; // Skip this file, keep the rest.
                    if (result == DialogChoice.Retry)
                        continue;

                    return null; // Cancel/Abort: like the legacy form, nothing is added at all.
                }
            }
        }

        return sprites;
    }

    private void ApplyCurrentPath(OpenFileDialog fileDialog)
    {
        if (string.IsNullOrWhiteSpace(_currentPath))
            return;

        try
        {
            fileDialog.InitialDirectory = Path.GetDirectoryName(_currentPath);
            fileDialog.FileName = Path.GetFileName(_currentPath);
        }
        catch { }
    }

    /// <summary>
    /// Resolves the rows a command should act on: the grid's multi-selection if provided,
    /// otherwise the single selected row; always in grid order.
    /// </summary>
    private List<SpriteRowViewModel> RowsFrom(IList? selectedItems)
    {
        List<SpriteRowViewModel> rows = selectedItems?.OfType<SpriteRowViewModel>().ToList() ?? new List<SpriteRowViewModel>();

        if (rows.Count == 0 && SelectedSprite is not null)
            rows.Add(SelectedSprite);

        return rows.OrderBy(row => Sprites.IndexOf(row)).ToList();
    }

    private void Renumber()
    {
        for (int i = 0; i < Sprites.Count; i++)
            Sprites[i].Index = i;
    }

    private ImageSource GetImage(WadTexture texture) => _imageCache[texture];

    /// <summary>Same ImageC-to-frozen-bitmap conversion as TombLib.WPF (TextureMapBase, animated textures preview).</summary>
    private static ImageSource CreateImage(ImageC image)
    {
        var bitmap = new WriteableBitmap(image.Width, image.Height, 96, 96, PixelFormats.Bgra32, null);
        bitmap.WritePixels(new Int32Rect(0, 0, image.Width, image.Height), image.ToByteArray(), image.Width * 4, 0);
        bitmap.Freeze();
        return bitmap;
    }

    private static IWindowOwner Owner => WPFUtils.GetWindowOwner();
}
