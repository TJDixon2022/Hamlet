namespace Hamlet.RadioEngine.Cw;

/// <summary>Why a transmission ended.</summary>
public enum TransmissionEnd
{
    /// <summary>It has not ended.</summary>
    Running,

    /// <summary>The radio stopped keying and stayed stopped.</summary>
    Finished,

    /// <summary>The operator stopped it.</summary>
    Stopped,

    /// <summary>
    /// The clock ran out without the transmit line ever being seen.
    /// </summary>
    /// <remarks>
    /// Not a failure. It is what happens on a radio whose transmit line Hamlet
    /// cannot read, where the computed duration is the only thing there is.
    /// </remarks>
    Expected,
}

/// <summary>
/// One transmission, from the press to the last dah (HM-DEC-085).
/// </summary>
/// <remarks>
/// <para>**THE LATCH, AND THE REASON THERE IS ONE.** The send buttons have gone
/// disabled and re-enabled dozens of times inside a single eighteen-second
/// message, because the panel was reading the transmit line and under full
/// break-in that line drops between every dit. Two attempts to stop it shipped
/// and neither did.</para>
/// <para>**THE SECOND ATTEMPT IS THE INSTRUCTIVE ONE: it latched, it passed its
/// tests, and it failed on the radio.** It latched on the send call, and command
/// `17` hands up to thirty characters to the radio's own keyer and returns about
/// thirteen milliseconds later. The radio then keys on its own for the next
/// eighteen seconds. So the latch released at 13 ms and gave the panel straight
/// back to the flapping line, and every test passed because no test crossed that
/// boundary. **Handing the message over is not the transmission.**</para>
/// <para>So this watches two things instead. The **computed duration** says when
/// it should be over, which is arithmetic and is known before the first dit
/// (<see cref="CwDuration"/>). The **hold-off** says when it actually was: the
/// transmit line quiet for longer than any silence the message itself could
/// contain. The arithmetic drives what the operator reads; the hold-off decides
/// when the state ends, so stopping early ends it early.</para>
/// <para>**No clock of its own.** Every method takes the time, so a whole
/// eighteen-second transmission with the line flapping twenty times runs in a
/// test in microseconds and comes out the same every time (§5.4).</para>
/// </remarks>
public sealed class TransmissionWatch
{
    private DateTime _startedUtc;
    private DateTime _lastKeyedUtc;
    private TimeSpan _expected;
    private TimeSpan _holdOff;
    private bool _sawKeying;

    /// <summary>True from the press until the radio has finished sending.</summary>
    /// <remarks>
    /// **This changes exactly twice per transmission.** Anything that makes it
    /// change more often is the bug (HM-DEC-085).
    /// </remarks>
    public bool IsSending { get; private set; }

    /// <summary>How it ended, or <see cref="TransmissionEnd.Running"/>.</summary>
    public TransmissionEnd Outcome { get; private set; } = TransmissionEnd.Running;

    /// <summary>What is going out.</summary>
    public string Message { get; private set; } = "";

    /// <summary>How long the whole message should take.</summary>
    public TimeSpan Expected => _expected;

    /// <summary>How long the radio actually keyed, once it has finished.</summary>
    /// <remarks>
    /// **The number that was wrong.** It used to be measured across the send call
    /// and came out at a hundredth of a second for an eighteen-second
    /// transmission, and that figure reached the operator as "the radio keyed for
    /// 0 seconds" (HM-DEC-085). Completion means the radio finished sending, not
    /// that the bytes were accepted.
    /// </remarks>
    public TimeSpan Elapsed { get; private set; }

    /// <summary>True when the radio was seen keying at any point.</summary>
    public bool Keyed => _sawKeying;

    /// <summary>How far in, for a progress display.</summary>
    /// <param name="nowUtc">The time.</param>
    /// <returns>Zero to one, and one once it has finished.</returns>
    public double Progress(DateTime nowUtc)
    {
        if (!IsSending)
        {
            return Outcome == TransmissionEnd.Running ? 0 : 1;
        }

        if (_expected <= TimeSpan.Zero)
        {
            return 0;
        }

        return Math.Clamp((nowUtc - _startedUtc) / _expected, 0, 1);
    }

    /// <summary>How much longer, roughly.</summary>
    /// <param name="nowUtc">The time.</param>
    /// <returns>Never negative, and zero once it has finished.</returns>
    /// <remarks>
    /// Clamped at zero rather than going negative, because a radio keying past
    /// its computed time is a slower keyer than was read and not a countdown that
    /// has gone wrong. The hold-off is what ends the state; this only ever
    /// describes it.
    /// </remarks>
    public TimeSpan Remaining(DateTime nowUtc)
    {
        if (!IsSending)
        {
            return TimeSpan.Zero;
        }

        var left = _expected - (nowUtc - _startedUtc);

        return left < TimeSpan.Zero ? TimeSpan.Zero : left;
    }

    /// <summary>How long it has been going.</summary>
    /// <param name="nowUtc">The time.</param>
    /// <returns>Time since the press, or the final figure once finished.</returns>
    public TimeSpan So(DateTime nowUtc)
        => IsSending ? nowUtc - _startedUtc : Elapsed;

    /// <summary>The transmission starts.</summary>
    /// <param name="message">Exactly what is going out.</param>
    /// <param name="wordsPerMinute">The keyer speed, or 0 when it was not read.</param>
    /// <param name="nowUtc">The time of the press.</param>
    /// <remarks>
    /// Called when the send is handed to the radio, which is the last moment the
    /// state is certainly true. Everything after this is worked out.
    /// </remarks>
    public void Begin(string message, int wordsPerMinute, DateTime nowUtc)
    {
        Message = message ?? "";
        _startedUtc = nowUtc;
        _lastKeyedUtc = nowUtc;
        _expected = CwDuration.Of(Message, wordsPerMinute);
        _holdOff = CwDuration.Silence(wordsPerMinute);
        _sawKeying = false;
        Elapsed = TimeSpan.Zero;
        Outcome = TransmissionEnd.Running;
        IsSending = true;
    }

    /// <summary>
    /// One look at the transmit line.
    /// </summary>
    /// <param name="isTransmitting">
    /// Whether the radio is keyed right now, or null when that cannot be read.
    /// </param>
    /// <param name="nowUtc">The time.</param>
    /// <returns>True when this observation ended the transmission.</returns>
    /// <remarks>
    /// <para>**A LOW READING IS NOT AN ENDING.** It is one sample of a line that
    /// drops between every element.</para>
    /// <para>**AND THE SAMPLING IS WORSE THAN THAT, WHICH IS THE MEASUREMENT THAT
    /// SHAPED THIS.** The rig state is read about four times a second, and a dit
    /// at twenty words a minute is sixty milliseconds, so the samples do not so
    /// much watch the keying as beat against it. Replaying a real CQ through the
    /// real key pattern at the real poll rate, the longest stretch with no sample
    /// catching the key down is **a second and a half**, in the middle of the
    /// message. There is no hold-off both short enough to be useful and long
    /// enough to survive that.</para>
    /// <para>So the arithmetic is the floor and the line may only ever extend it.
    /// The transmission is not over before it is computed to be over, and it is
    /// not over while the line is still being seen. That is the opposite way
    /// round from watching for an ending, and it is the way round that cannot
    /// blink: a missed sample costs nothing, and a seen one only ever holds the
    /// state open longer.</para>
    /// <para>What that gives up is an early ending. If the radio stops on its own
    /// the panel stays busy until the computed time is up, a few seconds at
    /// worst, and the operator's own stop ends it on the spot through
    /// <see cref="Stop"/>. A panel that is busy slightly too long is a small
    /// wrongness; a panel that flickers thirty times a message is the complaint
    /// this ruling exists to close.</para>
    /// <para>Where the line cannot be read at all, the arithmetic is all there
    /// is, and that ends the state as <see cref="TransmissionEnd.Expected"/>
    /// rather than <see cref="TransmissionEnd.Finished"/>, because Hamlet worked
    /// it out and did not watch it happen (§0.0).</para>
    /// </remarks>
    public bool Observe(bool? isTransmitting, DateTime nowUtc)
    {
        if (!IsSending)
        {
            return false;
        }

        if (isTransmitting == true)
        {
            _sawKeying = true;
            _lastKeyedUtc = nowUtc;

            return false;
        }

        // The floor: never before the message can possibly have finished.
        var floor = _startedUtc + _expected;

        if (_sawKeying)
        {
            var quiet = _lastKeyedUtc + _holdOff;

            return nowUtc >= (quiet > floor ? quiet : floor)
                && Finish(nowUtc, TransmissionEnd.Finished);
        }

        // Never seen keying: either the radio has not started yet, or its
        // transmit line is not readable on this radio. Wait out the arithmetic
        // and a hold-off past it before giving up on seeing anything.
        return nowUtc >= floor + _holdOff
            && Finish(nowUtc, TransmissionEnd.Expected);
    }

    /// <summary>The operator stopped it.</summary>
    /// <param name="nowUtc">The time.</param>
    /// <returns>True when this ended a running transmission.</returns>
    /// <remarks>
    /// Immediate and unconditional. The abort path may not wait for a hold-off,
    /// a poll or anything else (§0.2).
    /// </remarks>
    public bool Stop(DateTime nowUtc)
        => IsSending && Finish(nowUtc, TransmissionEnd.Stopped);

    /// <summary>End it, once, and record how long it really took.</summary>
    private bool Finish(DateTime nowUtc, TransmissionEnd outcome)
    {
        // The keying ended when the line was last seen down, or when the
        // arithmetic says it must have, whichever is later. Not when the hold-off
        // that confirmed it expired: reporting the wait as part of the
        // transmission would overstate every send by the best part of a second.
        Elapsed = outcome switch
        {
            TransmissionEnd.Stopped => nowUtc - _startedUtc,
            TransmissionEnd.Finished when _lastKeyedUtc - _startedUtc > _expected
                => _lastKeyedUtc - _startedUtc,
            _ => _expected,
        };

        if (Elapsed < TimeSpan.Zero)
        {
            Elapsed = TimeSpan.Zero;
        }

        Outcome = outcome;
        IsSending = false;

        return true;
    }
}
