using System.Reflection;
using Hamlet.App.Controls;
using Xunit.Sdk;

[assembly: Hamlet.App.Tests.TimersStopWithTheirTest]

namespace Hamlet.App.Tests;

/// <summary>
/// **A TEST'S TIMERS STOP WITH ITS TEST** (work instruction 563, HM-DEC-267). Every test that builds a
/// <c>MainWindowViewModel</c> starts its timers, the scope's at 50 ms, the dwell's and the decoder's at 250 ms and the age's at
/// a second, because Avalonia's DispatcherTimer constructor that takes a handler starts it; and nothing stopped them. A plain
/// test runs off the headless dispatcher, so its timers' ticks queued there unrun, and the next headless test's start runs
/// every leftover job before it begins: each tick it ran brought more timers due, and past five seconds of that Avalonia
/// throws "You've caused dispatcher loop" on whichever test was starting. Traced on 2026-10-08: after
/// `ThePsk31TelemetryTests` the queue held only `DispatcherTimer.FireTick`, two hundred at most, and the two headless tests
/// after it each spent five seconds failing. The second leak was the view model's settled snapshot, a five-second delay
/// nothing could cancel that posted onto whichever test's dispatcher was current: up to ninety-seven queued at once.
/// </summary>
/// <remarks>
/// Every timer the app makes, and the snapshot's delay, is recorded by <see cref="UiTimers"/>, and this stops and cancels
/// them all when each test ends, on the dispatcher for a headless test, where Avalonia's own drain runs straight after. It
/// retries nothing and skips nothing. What it leaves are Avalonia's own one-shot gesture timers, which fire once each.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class TimersStopWithTheirTestAttribute : BeforeAfterTestAttribute
{
    /// <inheritdoc/>
    public override void After(MethodInfo methodUnderTest) => UiTimers.StopAll();
}
