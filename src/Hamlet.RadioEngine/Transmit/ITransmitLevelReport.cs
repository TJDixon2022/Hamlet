namespace Hamlet.RadioEngine.Transmit;

/// <summary>
/// What a sink measured on the way out, for something that already holds one to
/// read afterwards.
/// </summary>
/// <remarks>
/// <para>**IT IS SEPARATE FROM <see cref="ITransmitAudioSink"/> ON PURPOSE, AND
/// THE SEPARATION IS THE WHOLE POINT** (work instruction 269, task 3). The
/// sequence that keys the radio talks through <c>ITransmitAudioSink</c>, and that
/// interface is not touched: it has one method because the sequence asks one
/// question. **Adding a level to it would put this reading inside the keying
/// path** - the key, the sink call, the <c>finally</c>, the abort and the stop,
/// which units 255, 261 and 263 proved and which no unit disturbs to carry a
/// number.</para>
/// <para>**IT IS READ AFTER THE BOUNDARY HAS RETURNED, WITH NOTHING KEYED.** The
/// application constructs the sink itself and can keep the reference; nothing on
/// this interface is called while a transmission is running, and nothing on it
/// can delay an unkey, fail a send or be reached from inside one. It is two
/// properties and no methods for exactly that reason.</para>
/// <para>**WHY IT HAD TO EXIST AT ALL.** Before it, the only level Hamlet could
/// show an operator after a send was the peak of the array the composer
/// produced - which is **the drive setting read back**, not a measurement - and
/// a clip count over that same array, which the composer built inside the rails.
/// Unit 265 recorded that every reader of <c>WasapiTransmitSink.PeakWritten</c>
/// and <c>ClippedSamples</c> in the repository was a test. At the radio, the
/// difference between a level he has verified and a number he typed is the whole
/// of what step D asks him to do (§0.0: uncertainty is displayed as
/// uncertainty).</para>
/// <para>**A SINK MAY NOT IMPLEMENT IT.** Nothing requires one to, so a caller
/// that finds no report must say on the screen that there is no measurement
/// rather than quietly showing the composed peak in a measurement's words.</para>
/// </remarks>
public interface ITransmitLevelReport
{
    /// <summary>The largest magnitude actually written to the endpoint.</summary>
    /// <remarks>
    /// **Measured on the way out, after clamping**, so it is what the device was
    /// handed rather than what the caller supplied. Zero until something has been
    /// played.
    /// </remarks>
    double PeakWritten { get; }

    /// <summary>How many samples had to be clamped on the way out.</summary>
    /// <remarks>
    /// **Counted, never rounded away.** Zero is the expected answer for anything
    /// this repository composes - the composer refuses a drive above full scale
    /// outright - and a number other than zero is a finding about the path
    /// between the composer and the card, not about the composer.
    /// </remarks>
    long ClippedSamples { get; }
}
