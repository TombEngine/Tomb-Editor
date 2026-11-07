using System.Diagnostics;
using System.Windows.Threading;
using TombLib.Input;
using TombLib.WPF.Controls.Rendering;

namespace TombLib.Test;

[TestClass]
public class MovementTimerTests
{
    [TestMethod]
    public void MovementRunsOnWpfDispatcherAndStopsAfterDisposal()
    {
        StaTest.Run(() => {
            int ticks = 0;
            var movement = new MovementTimer((_, _) => ticks++);
            Assert.IsTrue(movement.Engage(EditorKey.Up));
            PumpUntil(() => ticks > 0);

            movement.Dispose();
            int stoppedTicks = ticks;
            PumpFor(TimeSpan.FromMilliseconds(80));

            Assert.AreEqual(stoppedTicks, ticks);
            Assert.AreEqual(AnimationMode.Idle, movement.Mode);
            Assert.IsFalse(movement.Engage(EditorKey.Up));
            movement.Animate(AnimationMode.Snap, 1.0f);
            Assert.AreEqual(AnimationMode.Idle, movement.Mode);
        });
    }

    [TestMethod]
    public void AutomaticTransitionUsesSecondsForItsProgress()
    {
        StaTest.Run(() => {
            float firstProgress = 0.0f;
            MovementTimer? observedMovement = null;
            using var movement = new MovementTimer((_, _) => {
                if (observedMovement is not null && firstProgress == 0.0f)
                    firstProgress = observedMovement.MoveMultiplier;
            });
            observedMovement = movement;
            movement.Animate(AnimationMode.Snap, 1.0f);
            PumpUntil(() => firstProgress > 0.0f);

            Assert.IsTrue(movement.Animating);
            Assert.IsTrue(firstProgress > 0.0f && firstProgress < 0.005f);
        });
    }

    [DataTestMethod]
    [DataRow(0.0f)]
    [DataRow(-1.0f)]
    [DataRow(float.NaN)]
    public void InvalidDurationDoesNotStartTransition(float duration)
    {
        StaTest.Run(() => {
            using var movement = new MovementTimer((_, _) => { });
            movement.Animate(AnimationMode.Snap, duration);
            Assert.AreEqual(AnimationMode.Idle, movement.Mode);
            Assert.AreEqual(0.0f, movement.MoveMultiplier);
        });
    }

    private static void PumpUntil(Func<bool> condition)
    {
        var elapsed = Stopwatch.StartNew();
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(5) };
        timer.Tick += (_, _) => {
            if (condition() || elapsed.Elapsed > TimeSpan.FromSeconds(2))
                frame.Continue = false;
        };
        timer.Start();
        Dispatcher.PushFrame(frame);
        timer.Stop();
        Assert.IsTrue(condition(), "The dispatcher timer did not advance.");
    }

    private static void PumpFor(TimeSpan duration)
    {
        var elapsed = Stopwatch.StartNew();
        PumpUntil(() => elapsed.Elapsed >= duration);
    }

}
