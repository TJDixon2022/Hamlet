namespace Hamlet.RadioEngine.Scan;

/// <summary>
/// **WHILE A SCAN RUNS, NOTHING IN HAMLET CAN TRANSMIT** (work instruction 540, HM-DEC-244): one lock, held for as long
/// as a scan runs, that every path keying the radio asks right before it keys.
/// </summary>
/// <remarks>
/// <para>**THE OWNER, 2026-10-04:** a scan is *"listen only, absolute."* It runs unattended, so a transmission started by
/// anything while it runs is a transmission nobody is sitting beside (§0.2).</para>
/// <para>**AT THE KEYING SITE, NOT ONLY AT THE BUTTON.** The send controls are disabled while it is held, and that is
/// the operator's half. This is the code's half: the radio's own keyer (<c>KeyerCwSender</c>) and the push-to-talk
/// sequence every audio mode keys through (<c>Ft8TransmitSequence</c>) each ask it immediately before they key, and
/// refuse while it is held. A control left enabled by mistake, or a command invoked from somewhere nobody thought of,
/// still keys nothing.</para>
/// <para>An instance, handed to both paths by whoever builds them, rather than a static: a static would make one test's
/// scan refuse another test's transmission running beside it.</para>
/// </remarks>
public sealed class ListenOnlyLock
{
    private readonly object _gate = new();
    private int _holds;

    /// <summary>True while a scan holds the lock.</summary>
    public bool IsHeld
    {
        get
        {
            lock (_gate)
            {
                return _holds > 0;
            }
        }
    }

    /// <summary>Raised when the lock is taken or let go.</summary>
    public event EventHandler? Changed;

    /// <summary>What a keying path says when it refuses, in the app's voice (§0.7).</summary>
    public const string Refusal = "A scan is running, and a scan listens only: nothing goes out until it stops.";

    /// <summary>Take the lock until the handle is disposed.</summary>
    /// <returns>The handle; disposing it twice lets go once.</returns>
    public IDisposable Hold()
    {
        lock (_gate)
        {
            _holds++;
        }

        Changed?.Invoke(this, EventArgs.Empty);

        return new Handle(this);
    }

    private void Release()
    {
        lock (_gate)
        {
            _holds = Math.Max(0, _holds - 1);
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private sealed class Handle(ListenOnlyLock owner) : IDisposable
    {
        private int _done;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _done, 1) == 0)
            {
                owner.Release();
            }
        }
    }
}
