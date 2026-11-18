using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using TombLib.WPF;
using TombLib.WPF.Controls.Rendering;
using TombLib.WPF.Dialogs;
using TombLib.WPF.ViewModels;
using TombLib.WPF.Views;

namespace TombLib.Test;

[TestClass]
public class DialogServiceTests
{
    [TestMethod]
    public void RegistryCreatesSharedAndApplicationViews()
    {
        StaTest.Run(() => {
            var registry = new DialogViewRegistry();
            var applicationView = CreateWindow();
            registry.Register<ModalModel>(() => applicationView);
            var input = ConfigureOffscreen(registry.Create(typeof(InputBoxWindowViewModel)));
            var geometry = ConfigureOffscreen(registry.Create(typeof(GeometryIOSettingsWindowViewModel)));

            Assert.IsInstanceOfType(input, typeof(InputBoxWindow));
            Assert.IsInstanceOfType(geometry, typeof(GeometryIOSettingsWindow));
            Assert.AreSame(applicationView, registry.Create(typeof(ModalModel)));
            input.Close();
            geometry.Close();
            applicationView.Close();
        });
    }

    [TestMethod]
    public void ModalResultClosesOnceAndReleasesNotifications()
    {
        StaTest.Run(() => {
            var model = new ModalModel();
            var registry = new DialogViewRegistry();
            int subscriptionsWhileOpen = 0;
            registry.Register<ModalModel>(() => {
                var window = CreateWindow();
                window.HookModalAutoClose();
                window.Loaded += (_, _) => window.Dispatcher.BeginInvoke(new Action(() => {
                    subscriptionsWhileOpen = model.SubscriberCount;
                    model.DialogResult = true;
                }));
                return window;
            });
            var service = new DialogService(registry);

            Assert.AreEqual(true, service.ShowDialog(new ModalModel(), model));
            Assert.AreEqual(1, subscriptionsWhileOpen);
            Assert.AreEqual(0, model.SubscriberCount);
        });
    }

    [TestMethod]
    public void ModalOwnerIsResolvedThroughDockedViewDataContext()
    {
        StaTest.Run(() => {
            var ownerModel = new ModalModel();
            var owner = CreateWindow();
            owner.Content = new Grid { Children = { new UserControl { DataContext = ownerModel } } };
            owner.Show();

            try
            {
                var model = new ModalModel();
                var registry = new DialogViewRegistry();
                Window? observedOwner = null;
                registry.Register<ModalModel>(() => {
                    var window = CreateWindow();
                    window.Loaded += (_, _) => window.Dispatcher.BeginInvoke(new Action(() => {
                        observedOwner = window.Owner;
                        model.DialogResult = true;
                    }));
                    return window;
                });

                Assert.AreEqual(true, new DialogService(registry).ShowDialog(ownerModel, model));
                Assert.AreSame(owner, observedOwner);
            }
            finally
            {
                owner.Close();
            }
        });
    }

    [TestMethod]
    public void ReplacingDataContextDetachesPreviousModalModel()
    {
        StaTest.Run(() => {
            var previous = new ModalModel();
            var current = new ModalModel();
            var window = CreateWindow();
            window.DataContext = previous;
            window.HookModalAutoClose();
            window.Show();
            window.DataContext = new object();

            Assert.AreEqual(0, previous.SubscriberCount);
            previous.DialogResult = true;
            Assert.IsTrue(window.IsVisible);
            window.DataContext = current;
            Assert.AreEqual(1, current.SubscriberCount);
            current.DialogResult = false;
            Assert.IsFalse(window.IsVisible);
            Assert.AreEqual(0, current.SubscriberCount);
        });
    }

    [TestMethod]
    public void ServiceCloseClosesItsModalWindow()
    {
        StaTest.Run(() => {
            var model = new ModalModel();
            var registry = new DialogViewRegistry();
            var service = new DialogService(registry);
            registry.Register<ModalModel>(() => {
                var window = CreateWindow();
                window.Loaded += (_, _) => window.Dispatcher.BeginInvoke(new Action(() => service.Close(model)));
                return window;
            });

            Assert.AreEqual(false, service.ShowDialog(new ModalModel(), model));
            Assert.AreEqual(0, model.SubscriberCount);
        });
    }

    [TestMethod]
    public void ClosingAnUnknownModelPreservesOtherWindows()
    {
        StaTest.Run(() => {
            var owner = CreateWindow();
            owner.DataContext = new ModalModel();
            owner.Show();

            try
            {
                new DialogService(new DialogViewRegistry()).Close(new ModalModel());
                Assert.IsTrue(owner.IsVisible);
            }
            finally
            {
                owner.Close();
            }
        });
    }

    [TestMethod]
    public void NativeViewportHandleResolvesItsOwningWpfWindow()
    {
        StaTest.Run(() => {
            using var viewport = new RenderingPanel();
            var owner = CreateWindow();
            owner.Content = viewport;
            owner.Show();

            try
            {
                Assert.AreSame(owner, WPFUtils.GetOwnerWindow(viewport.Handle));
            }
            finally
            {
                owner.Close();
            }
        });
    }

    private static Window CreateWindow() => ConfigureOffscreen(new Window { Width = 100.0, Height = 80.0 });

    private static Window ConfigureOffscreen(Window window)
    {
        window.Left = -30000.0;
        window.Top = -30000.0;
        window.ShowActivated = false;
        window.ShowInTaskbar = false;
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        return window;
    }

    private sealed class ModalModel : IModalDialogViewModel
    {
        private PropertyChangedEventHandler? _propertyChanged;
        private bool? _dialogResult;

        public int SubscriberCount { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged
        {
            add { _propertyChanged += value; SubscriberCount++; }
            remove { _propertyChanged -= value; SubscriberCount--; }
        }

        public bool? DialogResult
        {
            get => _dialogResult;
            set
            {
                _dialogResult = value;
                _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DialogResult)));
            }
        }
    }
}
