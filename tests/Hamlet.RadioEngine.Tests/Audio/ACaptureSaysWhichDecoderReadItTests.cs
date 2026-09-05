using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Work instruction 249, task 3: a capture says which decoder read it.
/// </summary>
/// <remarks>
/// <para>**WITHOUT IT, EVERY CAPTURE FROM TONIGHT ONWARD IS UNATTRIBUTABLE.**
/// From unit 249 there is more than one decoder this project might have used,
/// and a candidate count means a different thing depending on which produced it.
/// Six sidecars from 2026-09-03 are readable today only because they recorded
/// their own conditions.</para>
/// <para>**AN UNRECORDED DECODER SAYS SO RATHER THAN NAMING THE PORT** (§0.0).
/// The port was the only one for a year, so defaulting to it would look
/// harmless and would put a false attribution in the one record that exists to
/// settle attribution.</para>
/// </remarks>
public sealed class ACaptureSaysWhichDecoderReadItTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lines are printed.</param>
    public ACaptureSaysWhichDecoderReadItTests(ITestOutputHelper output)
        => _output = output;

    private static DateTime EndedAt { get; } =
        new(2026, 9, 3, 21, 6, 30, DateTimeKind.Utc);

    private static ClockOffset Measured { get; } =
        new(0, new DateTime(2026, 9, 3, 21, 0, 0, DateTimeKind.Utc));

    /// <summary>Every slot the reader produces names its decoder and stages.</summary>
    [Fact]
    public void EverySlotNamesTheDecoderThatReadIt()
    {
        var audio = Fixture();

        Assert.NotNull(audio);

        var heard = Ft8Reader.Read(audio, EndedAt, Measured);

        Assert.NotEmpty(heard.Slots);

        foreach (var slot in heard.Slots)
        {
            _output.WriteLine(slot.SlotStartUtc.ToString("HH:mm:ss")
                + "  ->  " + slot.Decoder);

            Assert.True(slot.Decoder.IsRecorded, "a slot did not name its decoder");
            Assert.Equal("Ft8Sharp.Deep", slot.Decoder.Name);
            Assert.True(slot.Decoder.FineSync, "fine sync is not recorded as on");
            Assert.True(
                slot.Decoder.OrderedStatistics,
                "ordered statistics is not recorded as on");
        }
    }

    /// <summary>The sidecar carries it, in words.</summary>
    [Fact]
    public void TheSidecarSaysWhichDecoderReadIt()
    {
        var audio = Fixture();

        Assert.NotNull(audio);

        var heard = Ft8Reader.Read(audio, EndedAt, Measured);

        var sheet = DigitalCaptureSheet.Compose(
            EndedAt,
            audio.Duration.TotalSeconds,
            audio.SampleRate,
            RigState.Empty,
            Measured,
            EndedAt,
            "20 m FT8",
            null,
            census: heard.Slots,
            refusal: heard.Refusal);

        var line = sheet.Split('\n')
            .FirstOrDefault(l => l.StartsWith("decoder", StringComparison.Ordinal));

        _output.WriteLine("the sidecar says:");
        _output.WriteLine("  " + line);

        Assert.NotNull(line);

        Assert.Contains("Ft8Sharp.Deep", line, StringComparison.Ordinal);
        Assert.Contains("fine sync", line, StringComparison.Ordinal);
        Assert.Contains("ordered statistics", line, StringComparison.Ordinal);
    }

    /// <summary>A census nobody stamped says so rather than naming a decoder.</summary>
    [Fact]
    public void AnUnrecordedDecoderIsSaidToBeUnrecorded()
    {
        var unstamped = new Ft8SlotCensus(
            EndedAt, 4, 2, 1, 0, 0, Array.Empty<int>(), 48_000);

        _output.WriteLine("unstamped  -> " + unstamped.Decoder);
        _output.WriteLine("port       -> " + Ft8DecoderIdentity.Port);
        _output.WriteLine("deep, both -> "
            + new Ft8DecoderIdentity("Ft8Sharp.Deep", true, true));
        _output.WriteLine("deep, one  -> "
            + new Ft8DecoderIdentity("Ft8Sharp.Deep", true, false));

        Assert.False(unstamped.Decoder.IsRecorded);
        Assert.Equal("not recorded", unstamped.Decoder.ToString());

        // The port has no stages, so it names itself and stops.
        Assert.Equal("Ft8Sharp", Ft8DecoderIdentity.Port.ToString());

        Assert.Equal(
            "Ft8Sharp.Deep with fine sync and ordered statistics",
            new Ft8DecoderIdentity("Ft8Sharp.Deep", true, true).ToString());

        Assert.Equal(
            "Ft8Sharp.Deep with fine sync",
            new Ft8DecoderIdentity("Ft8Sharp.Deep", true, false).ToString());

        Assert.Equal(
            "Ft8Sharp.Deep with ordered statistics",
            new Ft8DecoderIdentity("Ft8Sharp.Deep", false, true).ToString());
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
