using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Deep.Tests;

/// <summary>
/// <b>The sibling runs the port's loop itself, and with OSD off it returns what the port returns.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>This stopped being trivial at unit 246.</b> Until then the sibling delegated, and a delegating
/// decoder returning what it delegates to needs no experiment. It now reproduces
/// <c>Ft8SlotDecoder.Decode(Ft8Waterfall)</c> stage for stage through the port's public members, so
/// that an ordered statistics stage has a place to sit, and the identity is a real claim about two
/// pieces of code rather than about one called twice. These tests hold the parameters a caller hands
/// this type to reaching the port unaltered, and a whole <see cref="Ft8SlotResult"/> - all five counts
/// and every message, in order - to coming back the same.
/// </para>
/// <para>
/// The identity over the ladder, the committed capture and the reference recordings is measured in
/// <c>tests/Ft8Sharp.Tests</c>, where the harness and the fixtures live. This file is the sibling's
/// own suite and stands on audio it synthesises itself.
/// </para>
/// </remarks>
public class Ft8DeepSlotDecoderTests(ITestOutputHelper output)
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>A clean transmission of one free-text message, laid into a whole slot.</summary>
    private static float[] CleanSlot(string text)
    {
        Span<byte> message = stackalloc byte[Ft8Payload.MessageBytes];
        var packed = Ft8FreeText.TryPackText(text, message);
        Assert.Equal(Ft8PackResult.Ok, packed);

        var symbols = Ft8SymbolEncoder.Encode(message);
        return Ft8Waveform.SynthesizeSlot(symbols, Rate);
    }

    [Fact]
    public void TheParametersHandedInReachThePortUnaltered()
    {
        var geometry = new Ft8WaterfallGeometry();
        var search = new Ft8SyncSearch();
        var deep = new Ft8DeepSlotDecoder(geometry, search, messageLimit: 7, maxIterations: 11);

        Assert.Same(geometry, deep.Geometry);
        Assert.Same(geometry, deep.Port.Geometry);
        Assert.Equal(7, deep.MessageLimit);
        Assert.Equal(7, deep.Port.MessageLimit);
        Assert.Equal(11, deep.MaxIterations);
        Assert.Equal(11, deep.Port.MaxIterations);
        Assert.Equal(search.CandidateLimit, deep.CandidateLimit);
        Assert.Equal(search.MinimumScore, deep.MinimumScore);
    }

    /// <summary>
    /// <b>The refusals are the port's, with the port's own wording.</b> A second copy of a refusal is
    /// a copy that drifts, so this type does not check what it is about to hand over.
    /// </summary>
    [Fact]
    public void TheRefusalsAreThePortsAndAreNotReimplementedHere()
    {
        var negativeLimit = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Ft8DeepSlotDecoder(messageLimit: -1));
        Assert.Contains("cannot be negative", negativeLimit.Message, StringComparison.Ordinal);

        var negativeIterations = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Ft8DeepSlotDecoder(maxIterations: -1));
        Assert.Contains("cannot be negative", negativeIterations.Message, StringComparison.Ordinal);

        Assert.Throws<ArgumentNullException>(() => new Ft8DeepSlotDecoder((Ft8SlotDecoder)null!));
        Assert.Throws<ArgumentNullException>(
            () => new Ft8DeepSlotDecoder().Decode((Ft8Waterfall)null!));

        output.WriteLine(negativeLimit.Message);
    }

    /// <summary>
    /// <b>The whole result comes back across the seam, not just the texts.</b> A comparison on text
    /// alone passes while the counts differ, and the counts are what steps 2, 3 and 4 will be read on.
    /// </summary>
    [Fact]
    public void OnCleanAudioTheWholeResultIsThePortsResult()
    {
        var samples = CleanSlot("HAMLET 245");

        var port = new Ft8SlotDecoder();
        var deep = new Ft8DeepSlotDecoder();

        var fromPort = port.Decode(samples);
        var fromDeep = deep.Decode(samples);

        output.WriteLine("count                 Ft8Sharp   Ft8Sharp.Deep");
        output.WriteLine($"candidates          {fromPort.CandidateCount,10} {fromDeep.CandidateCount,15}");
        output.WriteLine($"parity satisfied    {fromPort.ParitySatisfiedCount,10} {fromDeep.ParitySatisfiedCount,15}");
        output.WriteLine($"checksum passed     {fromPort.ChecksumPassedCount,10} {fromDeep.ChecksumPassedCount,15}");
        output.WriteLine($"became text         {fromPort.BecameTextCount,10} {fromDeep.BecameTextCount,15}");
        output.WriteLine($"duplicates          {fromPort.DuplicateCount,10} {fromDeep.DuplicateCount,15}");
        output.WriteLine($"messages            {fromPort.Messages.Count,10} {fromDeep.Messages.Count,15}");

        Assert.Equal(fromPort.CandidateCount, fromDeep.CandidateCount);
        Assert.Equal(fromPort.ParitySatisfiedCount, fromDeep.ParitySatisfiedCount);
        Assert.Equal(fromPort.ChecksumPassedCount, fromDeep.ChecksumPassedCount);
        Assert.Equal(fromPort.BecameTextCount, fromDeep.BecameTextCount);
        Assert.Equal(fromPort.DuplicateCount, fromDeep.DuplicateCount);
        Assert.Equal(fromPort.Messages.Count, fromDeep.Messages.Count);

        for (var i = 0; i < fromPort.Messages.Count; i++)
        {
            Assert.Equal(fromPort.Messages[i].Text, fromDeep.Messages[i].Text, StringComparer.Ordinal);
            Assert.Equal(fromPort.Messages[i].Candidate, fromDeep.Messages[i].Candidate);
            Assert.Equal(
                fromPort.Messages[i].FrequencyHz(port.Geometry),
                fromDeep.Messages[i].FrequencyHz(deep.Geometry),
                12);
            Assert.Equal(
                fromPort.Messages[i].TimeSeconds(port.Geometry),
                fromDeep.Messages[i].TimeSeconds(deep.Geometry),
                12);
        }

        // The audio is a clean transmission of a message the encoder packed, so the message is
        // expected back. If this ever goes red it is a finding about the decoder, not about the seam.
        Assert.Contains("HAMLET 245", fromDeep.Texts);
    }

    /// <summary>
    /// <b>Both overloads reach the same loop</b>, which matters because the waterfall one is where the
    /// OSD stage sits and the samples one is what the scoreboard's seat calls.
    /// </summary>
    [Fact]
    public void BothOverloadsReachTheSameLoop()
    {
        var samples = CleanSlot("HAMLET 245");
        var deep = new Ft8DeepSlotDecoder();
        var waterfall = new Ft8Monitor(deep.Geometry).Analyse(samples);

        var fromWaterfall = deep.Decode(waterfall);
        var fromSamples = deep.Decode(samples);

        Assert.Equal(fromSamples.CandidateCount, fromWaterfall.CandidateCount);
        Assert.Equal(fromSamples.BecameTextCount, fromWaterfall.BecameTextCount);
        Assert.Equal(fromSamples.Texts, fromWaterfall.Texts);
    }

    /// <summary>
    /// <b>What the sibling holds, named one by one, and changed deliberately rather than
    /// discovered.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Unit 245 left a tripwire here and unit 246 has walked into it on purpose.</b> The
    /// assertion this replaces was <c>Assert.Single(types)</c> plus a refusal of any type whose name
    /// contained "Osd" or "Ordered", and its stated reason was that the unit landing ordered
    /// statistics decoding must come here and change it, rather than find out afterwards that step
    /// 1's claim - that the sibling changes no behaviour - had quietly stopped being true.
    /// </para>
    /// <para>
    /// <b>It has stopped being true, and here is the whole of what stopped it.</b> The sibling now
    /// runs the port's per-candidate loop itself rather than delegating, and it carries the ordered
    /// statistics stage that loop exists to host. The list below is exhaustive and is asserted
    /// exhaustively, so the next unit that adds a type has to come here too.
    /// </para>
    /// <para>
    /// <b>Unit 247 is that next unit, and it came here on purpose three times.</b> Task 2 added the
    /// soft combiner — <c>Ft8DeepCombineWeighting</c> and <c>Ft8DeepSoftCombiner</c>; task 3 added the
    /// pairing rule and its budget — <c>Ft8DeepCombineSettings</c> and <c>Ft8DeepCombineCounts</c>;
    /// task 4 added the decoder that carries them across slots — <c>Ft8DeepHearing</c> and
    /// <c>Ft8DeepRepeatDecoder</c>. <b>This test going red is the tripwire working; the list is
    /// rewritten by the unit that changed the assembly and never by the unit that discovers it
    /// afterwards.</b>
    /// </para>
    /// <para>
    /// <b>Unit 248 came here on purpose too, and added seven.</b> Task 2 added the downconversion and
    /// the extractor that reads a candidate below the waterfall's grid —
    /// <c>Ft8DeepBasebandSettings</c>, <c>Ft8DeepBaseband</c> and <c>Ft8DeepBasebandExtractor</c>;
    /// task 3 added the search that finds the position instead of being told it —
    /// <c>Ft8DeepFineSyncSettings</c>, <c>Ft8DeepFineSync</c> and <c>Ft8DeepFineSyncResult</c>; task 4
    /// added the counts that say what it did — <c>Ft8DeepFineSyncCounts</c>. <b>This list was
    /// rewritten by unit 248 deliberately and is not a test that broke.</b>
    /// </para>
    /// </remarks>
    [Fact]
    public void TheSiblingHoldsExactlyTheseTypesAndTheListIsAssertedWhole()
    {
        var types = typeof(Ft8DeepSlotDecoder).Assembly.GetTypes();

        output.WriteLine("Types in Ft8Sharp.Deep:");
        foreach (var type in types)
        {
            output.WriteLine($"  {type.FullName}");
        }

        Assert.Equal(
            new[]
            {
                typeof(Ft8DeepBaseband),
                typeof(Ft8DeepBasebandExtractor),
                typeof(Ft8DeepBasebandSettings),
                typeof(Ft8DeepCombineCounts),
                typeof(Ft8DeepCombineSettings),
                typeof(Ft8DeepCombineWeighting),
                typeof(Ft8DeepFineSync),
                typeof(Ft8DeepFineSyncCounts),
                typeof(Ft8DeepFineSyncResult),
                typeof(Ft8DeepFineSyncSettings),
                typeof(Ft8DeepHearing),
                typeof(Ft8DeepOrderedStatistics),
                typeof(Ft8DeepOsdCounts),
                typeof(Ft8DeepOsdResult),
                typeof(Ft8DeepOsdSettings),
                typeof(Ft8DeepRepeatDecoder),
                typeof(Ft8DeepSlotDecoder),
                typeof(Ft8DeepSoftCombiner),
            },
            types.OrderBy(t => t.FullName, StringComparer.Ordinal).ToArray());
    }

    /// <summary>
    /// <b>Off is the default, and off is what the scoreboard's second column is built on.</b>
    /// </summary>
    /// <remarks>
    /// If this ever goes red, <c>Ft8DeepIdentityTests</c> has silently stopped comparing the port
    /// against an exact reproduction and started comparing it against an OSD run, and every
    /// difference between the scoreboard's columns stops being attributable to one named change.
    /// </remarks>
    [Fact]
    public void OrderedStatisticsIsOffUnlessItIsAskedFor()
    {
        Assert.Null(new Ft8DeepSlotDecoder().Osd);
        Assert.Null(new Ft8DeepSlotDecoder(new Ft8SlotDecoder()).Osd);

        var on = new Ft8DeepSlotDecoder(osd: new Ft8DeepOsdSettings(2));
        Assert.NotNull(on.Osd);
        Assert.Equal(2, on.Osd!.Order);

        var negative = Assert.Throws<ArgumentOutOfRangeException>(() => new Ft8DeepOsdSettings(-1));
        var tooHigh = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Ft8DeepOsdSettings(Ft8DeepOsdSettings.MaximumOrder + 1));

        output.WriteLine(negative.Message);
        output.WriteLine(tooHigh.Message);
    }

    /// <summary>
    /// <b>The stage runs where belief propagation gave up, and its three counts are kept beside the
    /// port's five.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The audio is one clean transmission buried in enough noise that most of the candidates the
    /// search returns are places where nothing was sent. Those are the candidates belief propagation
    /// refuses, and they are exactly what OSD is offered - which is the point: <b>most of what this
    /// stage is handed is noise, and the port refusing nearly all of it is the ordinary case</b>.
    /// </para>
    /// <para>
    /// <b>Nothing here asserts a decode rate.</b> The ladder is where rates are measured. What is
    /// asserted is the wiring: OSD is asked once per refused candidate, produces exactly one codeword
    /// each time, spends the re-encodings its order costs, and never accepts anything itself.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheStageIsOfferedEveryCandidateBeliefPropagationRefusedAndNoOthers()
    {
        var clean = CleanSlot("HAMLET 245");
        var random = new Random(246);
        var noisy = new float[clean.Length];
        for (var i = 0; i < clean.Length; i++)
        {
            noisy[i] = clean[i] + (float)((random.NextDouble() - 0.5) * 4.0);
        }

        var off = new Ft8DeepSlotDecoder();
        var fromOff = off.Decode(noisy);

        Assert.Equal(default, off.LastOsd);

        var on = new Ft8DeepSlotDecoder(osd: new Ft8DeepOsdSettings(1));
        var fromOn = on.Decode(noisy);
        var counts = on.LastOsd;

        output.WriteLine($"candidates                {fromOn.CandidateCount}");
        output.WriteLine($"reached parity, OSD off   {fromOff.ParitySatisfiedCount}");
        output.WriteLine($"offered to OSD            {counts.Offered}");
        output.WriteLine($"codewords OSD produced    {counts.Produced}");
        output.WriteLine($"of those, the PORT took   {counts.Accepted}");
        output.WriteLine($"re-encodings spent        {counts.Reencodings}");

        // Offered is exactly the candidates the port refused on parity, and no others.
        Assert.Equal(fromOff.CandidateCount - fromOff.ParitySatisfiedCount, counts.Offered);
        Assert.Equal(counts.Offered, counts.Produced);
        Assert.Equal(counts.Offered * 92L, counts.Reencodings);
        Assert.InRange(counts.Accepted, 0, counts.Produced);
        Assert.True(counts.Offered > 0, "no candidate was refused, so this measures nothing.");
    }

    /// <summary>The default iteration count is the port's, not a second copy of the number.</summary>
    [Fact]
    public void TheDefaultsAreThePortsDefaults()
    {
        var deep = new Ft8DeepSlotDecoder();

        Assert.Equal(LdpcDecoder.DefaultMaxIterations, deep.MaxIterations);
        Assert.Equal(Ft8SlotDecoder.DefaultMessageLimit, deep.MessageLimit);
    }
}
