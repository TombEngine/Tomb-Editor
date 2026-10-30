#nullable enable

using System;
using System.Windows;
using System.Windows.Input;

namespace TombLib.WPF.Features.AnimatedTextures
{
    public partial class AnimatedTexturesWindow : Window
    {
        private AnimatedTexturesWindowViewModel? _viewModel;

        public AnimatedTexturesWindow()
        {
            InitializeComponent();
            this.HookModalAutoClose();
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
            Closed += OnClosed;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Detach();

            _viewModel = e.NewValue as AnimatedTexturesWindowViewModel;
            if (_viewModel == null)
                return;

            _viewModel.InputRequested += OnInputRequested;
            _viewModel.TextureMap.MouseDown += OnTextureMapMouseDown;
        }

        // Attach the externally-created texture map only once the window has its HwndSource, so the
        // hosted control (and the window) get the correct per-monitor DPI context. Attaching it earlier
        // (in DataContextChanged, before the window is shown) corrupted the popup coordinate transform.
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null && !ReferenceEquals(textureMapContainer.Child, _viewModel.TextureMap))
                textureMapContainer.Child = _viewModel.TextureMap;

            Dispatcher.BeginInvoke(new Action(() => _viewModel?.ResetMapView()), System.Windows.Threading.DispatcherPriority.Loaded);

            // Make sure the first available set is selected on first show. The VM ctor already
            // does this, but binding evaluation order can leave the ComboBox with no selection
            // when ItemsSource resolves after SelectedItem.
            if (_viewModel != null && _viewModel.SelectedSet == null && _viewModel.Sets.Count > 0)
                _viewModel.SelectedSet = _viewModel.Sets[0];
        }

        // The VM cannot show windows itself, so it raises a request and the view answers it here.
        private void OnInputRequested(object? sender, TextInputRequest e)
        {
            var viewModel = new InputDialogViewModel(e.Title, e.Label, e.Value);
            var dialog = new InputDialog { Owner = this, DataContext = viewModel };
            if (dialog.ShowDialog() == true)
                e.Accept(viewModel.Value);
        }

        private void OnTextureMapMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2 &&
                _viewModel?.AddFrameCommand.CanExecute(null) == true)
            {
                _viewModel.AddFrameCommand.Execute(null);
            }
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            _viewModel?.OnWindowClosed();
            Detach();
            textureMapContainer.Child = null;
        }

        private void Detach()
        {
            if (_viewModel == null)
                return;

            _viewModel.InputRequested -= OnInputRequested;
            _viewModel.TextureMap.MouseDown -= OnTextureMapMouseDown;
        }
    }
}
