using Avalonia.Threading;

namespace Hamlet.App.Controls;

/// <summary>
/// **EVERY TIMER THE APP STARTS ON THE SCREEN'S THREAD, KNOWN IN ONE PLACE** (work instruction 563, HM-DEC-267). Avalonia's
/// <see cref="DispatcherTimer"/> constructor that takes a handler starts the timer, and nothing in Avalonia lists the timers
/// running, so a window or a view model that is never closed leaves its timers ticking. Each is made through here, so they
/// can be named and stopped together: by the headless tests at the end of every test, where one test's timers ticking into
/// the next test's start was the dispatcher loop that failed the app's test line.
/// </summary>
internal static class UiTimers
{
    private static readonly object Gate = new();
    private static readonly List<(WeakReference<DispatcherTimer> Timer, string Owner)> Made = new();

    /// <summary>Records a timer the app made, with the name of what owns it, and returns it.</summary>
    /// <param name="timer">The timer.</param>
    /// <param name="owner">What owns it, for the record.</param>
    /// <returns>The same timer.</returns>
    public static DispatcherTimer Track(DispatcherTimer timer, string owner)
    {
        lock (Gate)
        {
            Made.RemoveAll(m => !m.Timer.TryGetTarget(out _));
            Made.Add((new WeakReference<DispatcherTimer>(timer), owner));
        }

        return timer;
    }

    /// <summary>
    /// Posts <paramref name="action"/> to the screen's thread once <paramref name="delay"/> has passed, unless
    /// <see cref="StopAll"/> runs first. A delay rather than a DispatcherTimer, because a timer does not tick under the
    /// headless harness's RunJobs; recorded, because a delay that nothing can cancel posts its work onto whichever test's
    /// dispatcher is current five seconds later (work instruction 563).
    /// </summary>
    /// <param name="delay">How long to wait.</param>
    /// <param name="action">What to post.</param>
    /// <param name="priority">The priority it is posted at.</param>
    public static void PostAfter(TimeSpan delay, Action action, DispatcherPriority priority)
    {
        var cancel = new CancellationTokenSource();

        lock (Gate)
        {
            Pending.Add(cancel);
        }

        _ = Task.Delay(delay, cancel.Token).ContinueWith(
            done =>
            {
                lock (Gate)
                {
                    Pending.Remove(cancel);
                }

                if (!done.IsCanceled)
                {
                    Dispatcher.UIThread.Post(action, priority);
                }

                cancel.Dispose();
            },
            TaskScheduler.Default);
    }

    private static readonly List<CancellationTokenSource> Pending = new();

    /// <summary>Every timer recorded that is still running, with its owner and interval.</summary>
    public static IReadOnlyList<(string Owner, TimeSpan Interval)> Running()
    {
        lock (Gate)
        {
            return Made
                .Select(m => m.Timer.TryGetTarget(out var t) && t.IsEnabled ? (m.Owner, t.Interval) : default)
                .Where(m => m.Owner is not null)
                .ToList();
        }
    }

    /// <summary>
    /// Stops every timer recorded, cancels every delayed post not yet posted, and forgets them all; returns how many timers
    /// were running.
    /// </summary>
    public static int StopAll()
    {
        List<DispatcherTimer> timers;
        List<CancellationTokenSource> pending;

        lock (Gate)
        {
            timers = Made.Select(m => m.Timer.TryGetTarget(out var t) ? t : null).OfType<DispatcherTimer>().ToList();
            Made.Clear();
            pending = Pending.ToList();
            Pending.Clear();
        }

        foreach (var cancel in pending)
        {
            try
            {
                cancel.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // It fired and was disposed between the copy and here; nothing is left to cancel.
            }
        }

        var running = 0;

        foreach (var timer in timers)
        {
            if (timer.IsEnabled)
            {
                running++;
                timer.Stop();
            }
        }

        return running;
    }
}
