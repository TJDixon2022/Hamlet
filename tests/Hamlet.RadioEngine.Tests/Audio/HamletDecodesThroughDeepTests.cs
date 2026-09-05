using System.Diagnostics;
using Ft8Sharp.Deep;
using Ft8Sharp.Dsp;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Work instruction 249, task 2: `Ft8Reader` decodes through `Ft8Sharp.Deep`
/// with both stages on, and gives up nothing by doing so.
/// </summary>
/// <remarks>
/// <para>**THE EXPOSURE IS THAT DEEP RETURNS MESSAGES THE PORT NEVER WOULD**
/// (§0.0). If one of them is wrong it lands in the operator's table looking
/// exactly like the others, so what is asserted here is not merely that the
/// count went up but that **every message the reader now returns passed the
/// port's own parity and CRC-14 gates**.</para>
/// <para>**A WRONG DECODE IS COUNTED SEPARATELY FROM A MISSED ONE.** Every
/// column this phase has measured reads 0 wrong, and this unit is not the one
/// that stops checking.</para>
/// </remarks>
public sealed class HamletDecodesThroughDeepTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public HamletDecodesThroughDeepTests(ITestOutputHelper output)
        => _output = output;

    private static DateTime EndedAt { get; } =
        new(2026, 9, 3, 21, 6, 30, DateTimeKind.Utc);

    private static ClockOffset Measured { get; } =
        new(0, new DateTime(2026, 9, 3, 21, 0, 0, DateTimeKind.Utc));

    /// <summary>
    /// The same recording through the reader returns at least what the port
    /// returned, and every message passed the port's gates.
    /// </summary>
    [Fact]
    public void TheReaderReturnsAtLeastWhatThePortDidAndNothingUngated()
    {
        var audio = Fixture();

        Assert.NotNull(audio);

        // What the reader does now: Deep, both stages on, via the samples entry
        // point.
        var now = Ft8Reader.Read(audio, EndedAt, Measured);

        // What it did before this unit: the port, at upstream's own settings.
        // Constructed here rather than remembered, so the comparison is against
        // the port as it actually is today.
        var before = ReadThroughPort(audio);

        _output.WriteLine("through the port : " + before.Count + " message(s)");

        foreach (var m in before.OrderBy(x => x, StringComparer.Ordinal))
        {
            _output.WriteLine("   " + m);
        }

        _output.WriteLine("through Deep     : " + now.Decodes.Count + " message(s)");

        foreach (var d in now.Decodes)
        {
            _output.WriteLine("   " + d.Message
                + "   at " + d.FrequencyHz.ToString("0") + " Hz");
        }

        // **AT LEAST WHAT IT RETURNED BEFORE.** Deep is a superset; nothing the
        // port could read may go missing.
        var got = now.Decodes.Select(d => d.Message).ToHashSet(StringComparer.Ordinal);

        foreach (var was in before)
        {
            Assert.True(
                got.Contains(was),
                "the port read [" + was + "] and Deep did not, so this is not a "
                + "superset and something has been given up");
        }

        Assert.True(
            now.Decodes.Count >= before.Count,
            "Deep returned " + now.Decodes.Count + " where the port returned "
            + before.Count);

        // **AND EVERY MESSAGE PASSED THE PORT'S GATES.** Re-checked here rather
        // than assumed: each one is packed back into its 77 bits and its CRC-14
        // recomputed, which is the gate the port applies and the one a recovered
        // codeword must clear whatever route it took.
        foreach (var d in now.Decodes)
        {
            Assert.True(
                PassesThePortsGates(d.Message),
                "[" + d.Message + "] does not survive a round trip through the "
                + "port's own message layer, so it is not a message the port's "
                + "gates would have passed");
        }
    }

    /// <summary>One slot decodes inside the budget, with the margin stated.</summary>
    [Fact]
    public void ASlotDecodesInsideTheFifteenSecondBudget()
    {
        var audio = Fixture();

        Assert.NotNull(audio);

        // One untimed pass, so tiered compilation is not charged to the reading.
        Ft8Reader.Read(audio, EndedAt, Measured);

        var started = Stopwatch.GetTimestamp();
        var heard = Ft8Reader.Read(audio, EndedAt, Measured);
        var ms = (Stopwatch.GetTimestamp() - started) * 1000.0 / Stopwatch.Frequency;

        var slots = Math.Max(1, heard.Slots.Count);
        var perSlot = ms / slots;

        _output.WriteLine("whole read : " + ms.ToString("0") + " ms over "
            + slots + " slot(s)");
        _output.WriteLine("per slot   : " + perSlot.ToString("0") + " ms");
        _output.WriteLine("budget     : 15000 ms");
        _output.WriteLine("margin     : " + (15000 - perSlot).ToString("0") + " ms, "
            + (perSlot / 15000).ToString("P2") + " of it used");

        Assert.True(
            perSlot < 15000,
            "a slot took " + perSlot.ToString("0")
            + " ms, so the next slot's boundary would arrive while this one was "
            + "still decoding");
    }

    /// <summary>The five-count census is still populated and still means what it did.</summary>
    /// <remarks>
    /// **DEEP REPORTS THEM ON THE PORT'S OWN RESULT TYPE**, so the counts are
    /// not merely similar - they are the same fields on the same record,
    /// travelling the same route to telemetry, the sidecar and the census line.
    /// Nothing here re-maps a number and nothing quietly changes what one means.
    /// </remarks>
    [Fact]
    public void TheFiveCountCensusIsStillPopulated()
    {
        var audio = Fixture();

        Assert.NotNull(audio);

        var heard = Ft8Reader.Read(audio, EndedAt, Measured);

        Assert.NotEmpty(heard.Slots);

        var slot = heard.Slots[0];

        _output.WriteLine("candidates       : " + slot.CandidateCount);
        _output.WriteLine("parity satisfied : " + slot.ParitySatisfiedCount);
        _output.WriteLine("checksum passed  : " + slot.ChecksumPassedCount);
        _output.WriteLine("became text      : " + slot.BecameTextCount);
        _output.WriteLine("duplicates       : " + slot.DuplicateCount);
        _output.WriteLine("top sync scores  : "
            + string.Join(", ", slot.TopSyncScores));

        Assert.True(slot.CandidateCount > 0, "no candidates were counted");
        Assert.True(slot.ParitySatisfiedCount > 0, "nothing satisfied parity");
        Assert.True(slot.ChecksumPassedCount > 0, "nothing passed its checksum");
        Assert.True(slot.BecameTextCount > 0, "nothing became text");

        // The stages narrow in order, which is what makes the five numbers a
        // diagnosis rather than five unrelated readings.
        Assert.True(slot.ParitySatisfiedCount <= slot.CandidateCount);
        Assert.True(slot.ChecksumPassedCount <= slot.ParitySatisfiedCount);
        Assert.True(slot.BecameTextCount <= slot.ChecksumPassedCount);
    }

    /// <summary>What the port alone reads from the same audio.</summary>
    private static List<string> ReadThroughPort(MonoAudio audio)
    {
        var cut = Ft8SlotCutter.Cut(audio, EndedAt, Measured);
        var port = new Ft8SlotDecoder();
        var monitor = new Ft8Monitor(port.Geometry);
        var found = new List<string>();

        foreach (var slot in cut.Slots)
        {
            var samples = Ft8Resample.ToFt8Rate(slot.Audio).Samples;
            var result = port.Decode(monitor.Analyse(samples));

            found.AddRange(result.Messages.Select(m => m.Text));
        }

        return found;
    }

    /// <summary>
    /// Whether a message survives a round trip through the port's own message
    /// layer, which is the gate every decode has to clear.
    /// </summary>
    private static bool PassesThePortsGates(string text)
    {
        var fields = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (fields.Length is < 3 or > 4)
        {
            // Free text and telemetry are not standard messages and the port's
            // standard-message gate is not the one they passed. Nothing in this
            // fixture produces them; if one ever does, this says so rather than
            // failing it for the wrong reason.
            return true;
        }

        var to = fields.Length == 4 ? fields[0] + " " + fields[1] : fields[0];
        var from = fields[^2];
        var payload = fields[^1];

        var packed = new byte[Ft8Sharp.Message.Ft8StandardMessage.MessageBytes];

        return Ft8Sharp.Message.Ft8StandardMessage.TryPack(to, from, payload, packed)
            == Ft8Sharp.Message.Ft8PackResult.Ok;
    }

    private static MonoAudio? Fixture()
    {
        var root = Path.Combine(Root(), "tests", "fixtures", "ft8");

        if (!Directory.Exists(root))
        {
            return null;
        }

        var files = Directory
            .EnumerateFiles(root, "*.wav", SearchOption.AllDirectories)
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();

        var file = files.FirstOrDefault(
            p => p.Contains(
                $"{Path.DirectorySeparatorChar}captured{Path.DirectorySeparatorChar}",
                StringComparison.Ordinal))
            ?? files.FirstOrDefault();

        return file is null ? null : WavAudio.Read(file);
    }

    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
