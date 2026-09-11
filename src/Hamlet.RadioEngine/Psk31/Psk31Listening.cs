namespace Hamlet.RadioEngine.Psk31;

/// <summary>
/// **Where Hamlet listens for PSK31, while it listens in one place.**
/// </summary>
/// <remarks>
/// <para>**ONE CHANNEL AT ONE OFFSET IS WHAT STEP 1 BUILT** (work instruction 314). The
/// *hear everyone* step finds signals across the passband and gives each one its own
/// demodulator; until then there is a single spot, and where it is has to be written
/// down somewhere both the code and the sentence on the panel can read it.</para>
/// <para>**A THOUSAND HERTZ ABOVE THE DIAL, WHICH IS AN AUDIO OFFSET AND NOT A
/// FREQUENCY** (§0.1). The engine is told samples and an offset; what dial those samples
/// came from is the shell's fact, and the cited band row is what answers it. With the
/// dial on the row's own 14.070.000 that puts this at 14.071.000, and on any other band
/// it puts it a thousand hertz above whatever that band's row says.</para>
/// <para>**WHY A THOUSAND.** PSK31 activity sits in the first two or three kilohertz
/// above the calling frequency, and a thousand is comfortably inside a receiver's
/// passband at any filter setting a data mode is worked with. It is a starting place
/// rather than a measurement, and the step that finds signals for itself will not need
/// it.</para>
/// </remarks>
public static class Psk31Listening
{
    /// <summary>How far above the dial the one channel listens, in hertz.</summary>
    public const double OffsetHz = 1000;
}
