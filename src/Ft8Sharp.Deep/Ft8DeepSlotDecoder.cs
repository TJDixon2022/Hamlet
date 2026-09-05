using System;
using Ft8Sharp.Dsp;
using Ft8Sharp.Ldpc;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>The sibling's decode surface. Today it holds an <see cref="Ft8SlotDecoder"/> and hands back
/// exactly what that decoder returns.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>This changes no behaviour, and that is the whole of step 1.</b> <c>PHASE_PLAN.md</c> ruled the
/// seam split: <c>Ft8Sharp</c> stays a faithful MIT port of <c>ft8_lib</c>, byte-identical in
/// behaviour, and nothing in this phase changes a line of it, so that every measurement is taken
/// against something known-identical to upstream. Improvements land here instead. Tonight there are
/// none: the ladder reads two columns and they agree decode for decode, which is a statement about
/// the seam and the harness wiring costing nothing and is not a statement about hearing.
/// </para>
/// <para>
/// <b>The identity is trivially true and is asserted anyway.</b> A delegating type returns what it
/// delegates to; nobody needs an experiment to believe that. What the experiment proves is that the
/// wiring - the project reference, the <c>Available()</c> seat, the fixture scoring path - carries a
/// whole <see cref="Ft8SlotResult"/> across without dropping a count or reordering a message. That is
/// worth running once, and it is not a discovery.
/// </para>
/// <para>
/// <b>NO ABSTRACTION FOR SOMETHING THAT DOES NOT EXIST.</b> There is no OSD hook here, no stage
/// interface, no strategy, no extension point. An abstraction invented before the algorithm it is
/// meant to carry is an abstraction that will be wrong, and <c>docs/unit245-deep-seam.md</c> is what
/// the unit that takes step 2 is authored from instead - it records which of the port's stages are
/// reachable from outside the assembly and exactly what is not.
/// </para>
/// <para>
/// <b>Nothing under <c>src/Ft8Sharp/</c> is touched.</b> The port is the instrument.
/// </para>
/// </remarks>
public sealed class Ft8DeepSlotDecoder
{
    private readonly Ft8SlotDecoder _port;

    /// <summary>
    /// Builds a sibling decoder over an <see cref="Ft8SlotDecoder"/> constructed with these same
    /// parameters.
    /// </summary>
    /// <param name="geometry">The extents to analyse to. Defaults to the port's own default.</param>
    /// <param name="search">The search to find candidates with. Defaults to the port's own.</param>
    /// <param name="messageLimit">The most messages one slot returns.</param>
    /// <param name="maxIterations">How hard the correction tries per candidate.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The message limit is negative, or the iteration count is negative. <b>Thrown by the port</b>,
    /// with the port's own wording, because this constructor does not check what it is about to hand
    /// over: a second copy of a refusal is a copy that drifts.
    /// </exception>
    public Ft8DeepSlotDecoder(
        Ft8WaterfallGeometry? geometry = null,
        Ft8SyncSearch? search = null,
        int messageLimit = Ft8SlotDecoder.DefaultMessageLimit,
        int maxIterations = LdpcDecoder.DefaultMaxIterations)
        : this(new Ft8SlotDecoder(geometry, search, messageLimit, maxIterations))
    {
    }

    /// <summary>Builds a sibling decoder over a port decoder somebody else constructed.</summary>
    /// <param name="port">The decoder every call is handed to.</param>
    /// <exception cref="ArgumentNullException">The port decoder is null.</exception>
    public Ft8DeepSlotDecoder(Ft8SlotDecoder port)
    {
        ArgumentNullException.ThrowIfNull(port);
        _port = port;
    }

    /// <summary>The port decoder this one delegates to. <b>Exposed so a test can prove it does.</b></summary>
    public Ft8SlotDecoder Port => _port;

    /// <summary>The extents this decoder analyses to. The port's.</summary>
    public Ft8WaterfallGeometry Geometry => _port.Geometry;

    /// <summary>The most messages one slot returns. The port's.</summary>
    public int MessageLimit => _port.MessageLimit;

    /// <summary>How hard the correction tries per candidate. The port's.</summary>
    public int MaxIterations => _port.MaxIterations;

    /// <summary>The candidate limit the search this decoder uses will return. The port's.</summary>
    public int CandidateLimit => _port.CandidateLimit;

    /// <summary>The minimum sync score the search this decoder uses will keep. The port's.</summary>
    public int MinimumScore => _port.MinimumScore;

    /// <summary>
    /// Decodes one slot of audio. <b>Returns the port's <see cref="Ft8SlotResult"/> unchanged</b> -
    /// all five counts and every message, in the port's order.
    /// </summary>
    /// <param name="samples">The slot's audio. At least one block long.</param>
    /// <exception cref="ArgumentException">The signal is shorter than one block.</exception>
    public Ft8SlotResult Decode(ReadOnlySpan<float> samples) => _port.Decode(samples);

    /// <summary>
    /// Decodes one slot from a waterfall that has already been built. <b>Returns the port's
    /// <see cref="Ft8SlotResult"/> unchanged.</b>
    /// </summary>
    /// <param name="waterfall">The spectrogram of one slot.</param>
    /// <exception cref="ArgumentNullException">The waterfall is null.</exception>
    public Ft8SlotResult Decode(Ft8Waterfall waterfall) => _port.Decode(waterfall);
}
