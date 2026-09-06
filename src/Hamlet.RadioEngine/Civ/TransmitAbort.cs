using Hamlet.RadioEngine.Transport;

namespace Hamlet.RadioEngine.Civ;

/// <summary>One of the abort's two writes, and what became of it.</summary>
/// <param name="Frame">The frame that was built, whether or not it got out.
/// Carried so the caller can log it verbatim (§0.0.1).</param>
/// <param name="Written">True if the transport took the bytes without
/// complaint.</param>
/// <param name="Failure">Why it did not, or null.</param>
public sealed record AbortAttempt(CivFrame Frame, bool Written, string? Failure);

/// <summary>
/// What the abort did — both halves, whether or not either worked.
/// </summary>
/// <remarks>
/// **IT RETURNS SOMETHING BECAUSE A SILENT ABORT CANNOT BE DIAGNOSED.**
/// <c>Ic7300Rig.AbortCw</c> returns void and swallows its exception, so an abort
/// that did nothing and an abort that worked leave the same trace, which is
/// nothing (§0.0.1). Every caller from step 3 onward logs this.
/// </remarks>
public sealed record AbortRecord(AbortAttempt CwStop, AbortAttempt PttOff)
{
    /// <summary>True if either frame got out. Neither half needs the other.</summary>
    public bool AnythingReachedTheRadio => CwStop.Written || PttOff.Written;
}

/// <summary>
/// Stop transmitting. Now, on this thread, waiting for nothing (§0.2).
/// </summary>
/// <remarks>
/// <para>**TWO FRAMES, EACH TRIED ON ITS OWN, NEITHER DEPENDING ON THE OTHER.**
/// CI-V <c>17 FF</c> stops a keyer message the radio is sending itself
/// (p. 19-11); CI-V <c>1C 00 00</c> puts the radio back into receive (p. 19-7).
/// They stop different things. The stop code means nothing to a transmission
/// keyed by PTT and audio, and the PTT byte does not reach into the radio's own
/// keyer, so **the fallback is not a retry** — it is the other half of the
/// answer, and it goes out whether or not the first one landed.</para>
/// <para>**No wait of any kind is on this path.** The one seam it uses is
/// <see cref="ISerialPort.Write(ReadOnlySpan{byte})"/>, which returns void and
/// goes straight at the port rather than at the base stream, because the moment
/// somebody wants a transmitter to stop is the moment they cannot afford to be
/// queued behind whatever the ordinary command gate is already holding.
/// <c>TheAbortFiresFromEveryStateTests.NothingOnTheAbortPathWaitsForAnything</c>
/// fails if that changes.</para>
/// <para>**It cannot be switched off.** Static, no fields, no properties, no
/// boolean parameter and no branch in the body: there is nowhere for a flag to
/// live. <c>NoFlagCanTurnTheAbortOff</c> fails if one appears.</para>
/// <para>**It never throws.** A closed port, a disposed transport, a pulled USB
/// lead and a driver that refuses the write all end the same way: the attempt is
/// recorded as failed, the other half is still tried, and nothing propagates.
/// An abort that could fail is not an abort.</para>
/// <para>**Nothing in this repository calls it.** Unit 253 built it before any
/// code exists that can key a transmitter, which is the whole point of step 1.
/// </para>
/// </remarks>
public static class TransmitAbort
{
    /// <summary>Stop transmitting.</summary>
    /// <param name="port">The CI-V transport, in whatever state it is in.</param>
    /// <param name="radioAddress">The radio's CI-V address.</param>
    /// <param name="controllerAddress">This application's CI-V address.</param>
    /// <returns>What both halves did.</returns>
    public static AbortRecord Fire(
        ISerialPort port,
        byte radioAddress = CivConstants.DefaultRadioAddress,
        byte controllerAddress = CivConstants.DefaultControllerAddress)
    {
        var cwStop = new CivFrame(
            radioAddress, controllerAddress, CivConstants.CmdSendCwMessage,
            new[] { CivConstants.CwStopByte });

        var pttOff = new CivFrame(
            radioAddress, controllerAddress, CivConstants.CmdTransceiverControl,
            new[] { CivConstants.SubPtt, CivConstants.PttOff });

        // Both, in order, unconditionally. The second is not reached for
        // because the first failed -- it is reached for because it is the other
        // half of stopping, and reading the first one's outcome to decide would
        // be the condition this path is not allowed to have.
        var stopped = Attempt(port, cwStop);
        var unkeyed = Attempt(port, pttOff);

        return new AbortRecord(stopped, unkeyed);
    }

    /// <summary>One write, on this thread, swallowing whatever comes back.</summary>
    private static AbortAttempt Attempt(ISerialPort port, CivFrame frame)
    {
        try
        {
            port.Write(frame.ToWireBytes());
            return new AbortAttempt(frame, true, null);
        }
        catch (Exception ex)
        {
            // Nothing here is worth taking the application down for, least of
            // all on the path somebody reaches for when something has already
            // gone wrong. The message is kept so the record can say what
            // happened rather than that nothing did.
            return new AbortAttempt(frame, false, ex.Message);
        }
    }
}
