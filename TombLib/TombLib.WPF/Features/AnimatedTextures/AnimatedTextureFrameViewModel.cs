#nullable enable

using System.Numerics;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombLib.WPF.Features.AnimatedTextures
{
    /// <summary>
    /// Row wrapper for an <see cref="AnimatedTextureFrame"/> in the frames grid. The model does not
    /// implement INotifyPropertyChanged, so edits (grid cells or <c>UpdateFrameCommand</c>) would leave
    /// the dependent read-only columns (thumbnail, area, edges) stale; this wrapper writes through to
    /// the model and raises the change notifications for them.
    /// </summary>
    public sealed class AnimatedTextureFrameViewModel : ObservableObject
    {
        public AnimatedTextureFrameViewModel(AnimatedTextureFrame model)
        {
            Model = model;
        }

        public AnimatedTextureFrame Model { get; }

        public Texture Texture
        {
            get => Model.Texture;
            set
            {
                if (Model.Texture == value)
                    return;
                Model.Texture = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Thumbnail));
            }
        }

        public int Repeat
        {
            get => Model.Repeat;
            set
            {
                if (Model.Repeat == value)
                    return;
                Model.Repeat = value; // The model clamps to >= 1.
                OnPropertyChanged();
            }
        }

        public Vector2 TexCoord0
        {
            get => Model.TexCoord0;
            set
            {
                if (Model.TexCoord0 == value)
                    return;
                Model.TexCoord0 = value;
                OnTexCoordChanged(nameof(TexCoord0));
            }
        }

        public Vector2 TexCoord1
        {
            get => Model.TexCoord1;
            set
            {
                if (Model.TexCoord1 == value)
                    return;
                Model.TexCoord1 = value;
                OnTexCoordChanged(nameof(TexCoord1));
            }
        }

        public Vector2 TexCoord2
        {
            get => Model.TexCoord2;
            set
            {
                if (Model.TexCoord2 == value)
                    return;
                Model.TexCoord2 = value;
                OnTexCoordChanged(nameof(TexCoord2));
            }
        }

        public Vector2 TexCoord3
        {
            get => Model.TexCoord3;
            set
            {
                if (Model.TexCoord3 == value)
                    return;
                Model.TexCoord3 = value;
                OnTexCoordChanged(nameof(TexCoord3));
            }
        }

        public RectangleInt2 Area => Model.Area;

        public ImageSource? Thumbnail => AnimatedTexturesWindowViewModel.RenderFrame(Model, 32);

        private void OnTexCoordChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
            OnPropertyChanged(nameof(Area));
            OnPropertyChanged(nameof(Thumbnail));
        }
    }
}
