using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TombLib.Scripting.UI.Navigation;

namespace TombLib.Tests;

[TestClass]
public class TextDefinitionTriggerControllerTests
{
	[TestMethod]
	public void TryHandleKeyDownAsync_F12AndSuccessfulNavigation_HandlesEventAndUsesCaretOffset()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(owner);

			try
			{
				int requestedOffset = -1;
				var controller = new TextDefinitionTriggerController(
					owner,
					_ => -1,
					(offset, cancellationToken) =>
					{
						requestedOffset = offset;
						return Task.FromResult(true);
					});

				var eventArgs = new KeyEventArgs(
					Keyboard.PrimaryDevice,
					PresentationSource.FromVisual(hostWindow)!,
					0,
					Key.F12)
				{
					RoutedEvent = Keyboard.KeyDownEvent
				};

				bool handled = controller.TryHandleKeyDownAsync(eventArgs, 42).GetAwaiter().GetResult();

				Assert.IsTrue(handled);
				Assert.IsTrue(eventArgs.Handled);
				Assert.AreEqual(42, requestedOffset);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryHandleKeyDownAsync_F12AndFailedNavigation_DoesNotHandleEvent()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(owner);

			try
			{
				var controller = new TextDefinitionTriggerController(
					owner,
					_ => -1,
					(offset, cancellationToken) => Task.FromResult(false));

				var eventArgs = new KeyEventArgs(
					Keyboard.PrimaryDevice,
					PresentationSource.FromVisual(hostWindow)!,
					0,
					Key.F12)
				{
					RoutedEvent = Keyboard.KeyDownEvent
				};

				bool handled = controller.TryHandleKeyDownAsync(eventArgs, 42).GetAwaiter().GetResult();

				Assert.IsFalse(handled);
				Assert.IsFalse(eventArgs.Handled);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryHandleKeyDownAsync_ThrowingNavigation_ReturnsFalseWithoutEscaping()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(owner);

			try
			{
				var controller = new TextDefinitionTriggerController(
					owner,
					_ => -1,
					(offset, cancellationToken) => throw new InvalidOperationException("Navigation failed."));

				var eventArgs = new KeyEventArgs(
					Keyboard.PrimaryDevice,
					PresentationSource.FromVisual(hostWindow)!,
					0,
					Key.F12)
				{
					RoutedEvent = Keyboard.KeyDownEvent
				};

				bool handled = controller.TryHandleKeyDownAsync(eventArgs, 42).GetAwaiter().GetResult();

				Assert.IsFalse(handled);
				Assert.IsFalse(eventArgs.Handled);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryHandleKeyDownAsync_AlreadyHandled_ReturnsFalseWithoutNavigating()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(owner);

			try
			{
				bool navigated = false;
				var controller = new TextDefinitionTriggerController(
					owner,
					_ => -1,
					(offset, cancellationToken) =>
					{
						navigated = true;
						return Task.FromResult(true);
					});

				var eventArgs = new KeyEventArgs(
					Keyboard.PrimaryDevice,
					PresentationSource.FromVisual(hostWindow)!,
					0,
					Key.F12)
				{
					RoutedEvent = Keyboard.KeyDownEvent,
					Handled = true
				};

				bool handled = controller.TryHandleKeyDownAsync(eventArgs, 42).GetAwaiter().GetResult();

				// The WPF convention: an event that is already handled must not trigger navigation.
				Assert.IsFalse(handled);
				Assert.IsFalse(navigated);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryHandlePointerNavigationAsync_ControlLeftClick_HandlesEventAndUsesHoveredOffset()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(owner);

			try
			{
				int requestedOffset = -1;
				var controller = new TextDefinitionTriggerController(
					owner,
					_ => 21,
					(offset, cancellationToken) =>
					{
						requestedOffset = offset;
						return Task.FromResult(true);
					},
					getModifiers: () => ModifierKeys.Control);

				MouseButtonEventArgs eventArgs = CreateLeftClickArgs();

				bool handled = controller.TryHandlePointerNavigationAsync(eventArgs).GetAwaiter().GetResult();

				Assert.IsTrue(handled);
				Assert.IsTrue(eventArgs.Handled);
				Assert.AreEqual(21, requestedOffset);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryHandlePointerNavigationAsync_LeftClickWithoutControl_ReturnsFalseWithoutNavigating()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();

			bool navigated = false;
			var controller = new TextDefinitionTriggerController(
				owner,
				_ => 21,
				(offset, cancellationToken) =>
				{
					navigated = true;
					return Task.FromResult(true);
				},
				getModifiers: () => ModifierKeys.None);

			MouseButtonEventArgs eventArgs = CreateLeftClickArgs();

			bool handled = controller.TryHandlePointerNavigationAsync(eventArgs).GetAwaiter().GetResult();

			Assert.IsFalse(handled);
			Assert.IsFalse(navigated);
			Assert.IsFalse(eventArgs.Handled);
		});
	}

	[TestMethod]
	public void TryHandlePointerNavigationAsync_PointerOutsideDocument_ReturnsFalse()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(owner);

			try
			{
				bool navigated = false;
				var controller = new TextDefinitionTriggerController(
					owner,
					_ => -1,
					(offset, cancellationToken) =>
					{
						navigated = true;
						return Task.FromResult(true);
					},
					getModifiers: () => ModifierKeys.Control);

				MouseButtonEventArgs eventArgs = CreateLeftClickArgs();

				bool handled = controller.TryHandlePointerNavigationAsync(eventArgs).GetAwaiter().GetResult();

				// A pointer that maps to no document offset must not navigate.
				Assert.IsFalse(handled);
				Assert.IsFalse(navigated);
				Assert.IsFalse(eventArgs.Handled);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryHandlePointerNavigationAsync_AlreadyHandled_ReturnsFalseWithoutNavigating()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();

			bool navigated = false;
			var controller = new TextDefinitionTriggerController(
				owner,
				_ => 21,
				(offset, cancellationToken) =>
				{
					navigated = true;
					return Task.FromResult(true);
				},
				getModifiers: () => ModifierKeys.Control);

			MouseButtonEventArgs eventArgs = CreateLeftClickArgs();
			eventArgs.Handled = true;

			bool handled = controller.TryHandlePointerNavigationAsync(eventArgs).GetAwaiter().GetResult();

			// The WPF convention: an event that is already handled must not trigger navigation.
			Assert.IsFalse(handled);
			Assert.IsFalse(navigated);
		});
	}

	[TestMethod]
	public void TryHandlePointerNavigationAsync_AfterDisposal_ReturnsFalse()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();

			bool navigated = false;
			var controller = new TextDefinitionTriggerController(
				owner,
				_ => 21,
				(offset, cancellationToken) =>
				{
					navigated = true;
					return Task.FromResult(true);
				},
				getModifiers: () => ModifierKeys.Control);

			controller.Dispose();

			MouseButtonEventArgs eventArgs = CreateLeftClickArgs();

			bool handled = controller.TryHandlePointerNavigationAsync(eventArgs).GetAwaiter().GetResult();

			Assert.IsFalse(handled);
			Assert.IsFalse(navigated);
			Assert.IsFalse(eventArgs.Handled);
		});
	}

	[TestMethod]
	public void PublicConstructor_ThenDispose_DoesNotThrow()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();

			var controller = new TextDefinitionTriggerController(
				owner,
				_ => -1,
				(offset, cancellationToken) => Task.FromResult(false));

			controller.Dispose();
			controller.Dispose();
		});
	}

	[TestMethod]
	public void TryHandleKeyDownAsync_NonF12Key_ReturnsFalseWithoutNavigating()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(owner);

			try
			{
				bool navigated = false;
				var controller = new TextDefinitionTriggerController(
					owner,
					_ => -1,
					(offset, cancellationToken) =>
					{
						navigated = true;
						return Task.FromResult(true);
					});

				var eventArgs = new KeyEventArgs(
					Keyboard.PrimaryDevice,
					PresentationSource.FromVisual(hostWindow)!,
					0,
					Key.A)
				{
					RoutedEvent = Keyboard.KeyDownEvent
				};

				bool handled = controller.TryHandleKeyDownAsync(eventArgs, 42).GetAwaiter().GetResult();

				Assert.IsFalse(handled);
				Assert.IsFalse(eventArgs.Handled);
				Assert.IsFalse(navigated);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryHandlePointerNavigationAsync_ControlRightClick_ReturnsFalseWithoutNavigating()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();

			bool navigated = false;
			var controller = new TextDefinitionTriggerController(
				owner,
				_ => 21,
				(offset, cancellationToken) =>
				{
					navigated = true;
					return Task.FromResult(true);
				},
				getModifiers: () => ModifierKeys.Control);

			var eventArgs = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right)
			{
				RoutedEvent = Mouse.MouseDownEvent
			};

			bool handled = controller.TryHandlePointerNavigationAsync(eventArgs).GetAwaiter().GetResult();

			Assert.IsFalse(handled);
			Assert.IsFalse(eventArgs.Handled);
			Assert.IsFalse(navigated);
		});
	}

	[TestMethod]
	public void Dispose_WhileNavigationInFlight_CancelsNavigationToken()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(owner);

			try
			{
				var completion = new TaskCompletionSource<bool>();
				CancellationToken navigationToken = default;
				TextDefinitionTriggerController? controller = null;

				controller = new TextDefinitionTriggerController(
					owner,
					_ => 21,
					(offset, cancellationToken) =>
					{
						navigationToken = cancellationToken;
						return completion.Task;
					},
					getModifiers: () => ModifierKeys.Control);

				MouseButtonEventArgs eventArgs = CreateLeftClickArgs();
				Task<bool> navigation = controller.TryHandlePointerNavigationAsync(eventArgs);

				Assert.IsFalse(navigationToken.IsCancellationRequested);

				controller.Dispose();

				// Disposal signals cancellation to in-flight navigation.
				Assert.IsTrue(navigationToken.IsCancellationRequested);

				completion.TrySetResult(false);

				Assert.IsFalse(navigation.GetAwaiter().GetResult());
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryHandleKeyDownAsync_WhileNavigationRuns_MarksEventHandledBeforeAwait()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(owner);

			try
			{
				bool? handledDuringNavigation = null;
				KeyEventArgs? eventArgs = null;

				var controller = new TextDefinitionTriggerController(
					owner,
					_ => -1,
					(offset, cancellationToken) =>
					{
						handledDuringNavigation = eventArgs!.Handled;
						return Task.FromResult(true);
					});

				eventArgs = new KeyEventArgs(
					Keyboard.PrimaryDevice,
					PresentationSource.FromVisual(hostWindow)!,
					0,
					Key.F12)
				{
					RoutedEvent = Keyboard.KeyDownEvent
				};

				bool handled = controller.TryHandleKeyDownAsync(eventArgs, 42).GetAwaiter().GetResult();

				// The event is consumed during routing, before the asynchronous navigation is awaited.
				Assert.IsTrue(handled);
				Assert.IsTrue(handledDuringNavigation, "The event must be marked handled before navigation is awaited.");
				Assert.IsTrue(eventArgs.Handled);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryHandleKeyDownAsync_FailedNavigationAfterEarlyHandling_RollsBackHandledFlag()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(owner);

			try
			{
				bool? handledDuringNavigation = null;
				KeyEventArgs? eventArgs = null;

				var controller = new TextDefinitionTriggerController(
					owner,
					_ => -1,
					(offset, cancellationToken) =>
					{
						handledDuringNavigation = eventArgs!.Handled;
						return Task.FromResult(false);
					});

				eventArgs = new KeyEventArgs(
					Keyboard.PrimaryDevice,
					PresentationSource.FromVisual(hostWindow)!,
					0,
					Key.F12)
				{
					RoutedEvent = Keyboard.KeyDownEvent
				};

				bool handled = controller.TryHandleKeyDownAsync(eventArgs, 42).GetAwaiter().GetResult();

				// The early handling suppresses routing, and a failed navigation rolls the flag back.
				Assert.IsTrue(handledDuringNavigation);
				Assert.IsFalse(handled);
				Assert.IsFalse(eventArgs.Handled);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryHandleKeyDownAsync_ControlF12_ReturnsFalseWithoutNavigating()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(owner);

			try
			{
				bool navigated = false;
				var controller = new TextDefinitionTriggerController(
					owner,
					_ => -1,
					(offset, cancellationToken) =>
					{
						navigated = true;
						return Task.FromResult(true);
					},
					gestures: TextDefinitionNavigationGestures.Default,
					getModifiers: () => ModifierKeys.None);

				// The F12 gesture reads the modifiers from the event's keyboard device, not the pointer source.
				var eventArgs = new KeyEventArgs(
					new FakeKeyboardDevice(Key.F12, Key.LeftCtrl),
					PresentationSource.FromVisual(hostWindow)!,
					0,
					Key.F12)
				{
					RoutedEvent = Keyboard.KeyDownEvent
				};

				bool handled = controller.TryHandleKeyDownAsync(eventArgs, 42).GetAwaiter().GetResult();

				// F12 combined with a modifier is host policy; the controller only handles the unmodified key.
				Assert.IsFalse(handled);
				Assert.IsFalse(eventArgs.Handled);
				Assert.IsFalse(navigated);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryHandleKeyDownAsync_F12GestureDisabled_ReturnsFalseWithoutNavigating()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(owner);

			try
			{
				bool navigated = false;
				var controller = new TextDefinitionTriggerController(
					owner,
					_ => -1,
					(offset, cancellationToken) =>
					{
						navigated = true;
						return Task.FromResult(true);
					},
					gestures: TextDefinitionNavigationGestures.None);

				var eventArgs = new KeyEventArgs(
					Keyboard.PrimaryDevice,
					PresentationSource.FromVisual(hostWindow)!,
					0,
					Key.F12)
				{
					RoutedEvent = Keyboard.KeyDownEvent
				};

				// A host that binds F12 elsewhere disables the gesture instead of skipping the whole controller.
				Assert.IsFalse(controller.TryHandleKeyDownAsync(eventArgs, 42).GetAwaiter().GetResult());
				Assert.IsFalse(navigated);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	[TestMethod]
	public void TryHandlePointerNavigationAsync_ControlClickGestureDisabled_ReturnsFalseWithoutNavigating()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();

			bool navigated = false;
			var controller = new TextDefinitionTriggerController(
				owner,
				_ => 21,
				(offset, cancellationToken) =>
				{
					navigated = true;
					return Task.FromResult(true);
				},
				gestures: TextDefinitionNavigationGestures.F12,
				getModifiers: () => ModifierKeys.Control);

			MouseButtonEventArgs eventArgs = CreateLeftClickArgs();

			Assert.IsFalse(controller.TryHandlePointerNavigationAsync(eventArgs).GetAwaiter().GetResult());
			Assert.IsFalse(eventArgs.Handled);
			Assert.IsFalse(navigated);
		});
	}

	[TestMethod]
	public void TryHandlePointerNavigationAsync_DoubleClick_ReturnsFalseWithoutNavigating()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();

			bool navigated = false;
			var controller = new TextDefinitionTriggerController(
				owner,
				_ => 21,
				(offset, cancellationToken) =>
				{
					navigated = true;
					return Task.FromResult(true);
				},
				getModifiers: () => ModifierKeys.Control);

			MouseButtonEventArgs eventArgs = CreateLeftClickArgs();

			// WPF does not expose a public ClickCount setter, so the double-click state is applied through the
			// non-public setter to pin the controller's single-click-only contract.
			typeof(MouseButtonEventArgs)
				.GetProperty(nameof(MouseButtonEventArgs.ClickCount))!
				.GetSetMethod(nonPublic: true)!
				.Invoke(eventArgs, [2]);

			bool handled = controller.TryHandlePointerNavigationAsync(eventArgs).GetAwaiter().GetResult();

			// Double-clicks are deliberately left to the host.
			Assert.IsFalse(handled);
			Assert.IsFalse(eventArgs.Handled);
			Assert.IsFalse(navigated);
		});
	}

	[TestMethod]
	public void NavigateWithLogging_OperationCanceled_ReturnsFalse()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var owner = new Border();
			Window hostWindow = WPFTestHelper.ShowInHostWindow(owner);

			try
			{
				var controller = new TextDefinitionTriggerController(
					owner,
					_ => -1,
					(offset, cancellationToken) => throw new OperationCanceledException(),
					getModifiers: () => ModifierKeys.None);

				var eventArgs = new KeyEventArgs(
					Keyboard.PrimaryDevice,
					PresentationSource.FromVisual(hostWindow)!,
					0,
					Key.F12)
				{
					RoutedEvent = Keyboard.KeyDownEvent
				};

				bool handled = controller.TryHandleKeyDownAsync(eventArgs, 42).GetAwaiter().GetResult();

				// Cancellation is an expected outcome (for example after disposal), not a failure.
				Assert.IsFalse(handled);
			}
			finally
			{
				hostWindow.Close();
			}
		});
	}

	private static MouseButtonEventArgs CreateLeftClickArgs()
		=> new(Mouse.PrimaryDevice, 0, MouseButton.Left)
		{
			RoutedEvent = Mouse.MouseDownEvent
		};

	private sealed class FakeKeyboardDevice : KeyboardDevice
	{
		private readonly HashSet<Key> _downKeys;

		public FakeKeyboardDevice(params Key[] downKeys)
			: base(InputManager.Current)
		{
			_downKeys = [.. downKeys];
		}

		protected override KeyStates GetKeyStatesFromSystem(Key key)
			=> _downKeys.Contains(key) ? KeyStates.Down : KeyStates.None;
	}
}
