using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Work instruction 056, task 4: where Hamlet's element stream and the reference
/// bench's diverge on `cw-2026-08-31-003229`.
/// </summary>
/// <remarks>
/// <para>**THEY DO NOT DIVERGE, AND THAT IS THE UNIT'S CENTRAL FINDING.** Over the
/// seconds the reference reads `CQ` — 13.50 to 14.72 — the two streams hold the
/// same eight marks and the same seven gaps, in the same order, with every
/// duration inside 13 milliseconds and most inside one hop. The absolute times
/// differ by a constant 48 ms, which is the group delay of a centred 33 ms Hann
/// against a centred 25 ms boxcar, and a constant offset cancels for every
/// length.</para>
/// <para>**SO NONE OF THE FOUR CAUSES THE ORDER NAMED CAN BE THE DIFFERENCE**,
/// because all four corrupt the element stream and the element stream is not
/// corrupt. Measured individually as well: the reference's integrator is 40 Hz
/// nominal against Hamlet's 45; the hold-over was swept from 12 to 40 ms in task 1
/// and moves nothing here; and `CwUnitEstimator`'s drop-without-merge discards
/// **one run in a hundred and forty** on this capture and none at all on most of
/// the corpus.</para>
/// <para>**WHAT DIVERGES IS HAMLET FROM HAMLET.** Read offline at 583.5 Hz the
/// decoder produces `CXSIT#DD # SXEIT#S # KA`, and held at 23 words a minute it
/// produces a literal `CQ SIT K8DZ`. Read through the streaming path on the same
/// audio it produces 27 named characters and 29 blocks. Same core decoder, same
/// audio, same pitch, and the callsign attempt survives the offline window and
/// dissolves in the sliding one.</para>
/// </remarks>
public sealed class WhereHamletAndTheReferenceDivergeTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the aligned streams are printed.</param>
    public WhereHamletAndTheReferenceDivergeTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// The reference bench's own runs over the CQ, transcribed from
    /// `tools/cwbench/elements.py` so this test needs no Python.
    /// </summary>
    /// <remarks>
    /// **QUOTED, NOT RECOMPUTED.** The bench shares no code with Hamlet and is not
    /// on any decode path (§12.5), so its output is evidence here in the way a
    /// capture is: a second instrument's reading, written down. Reproduce it with
    /// `elements.py` at a pitch of 583.5 over 13.4 to 15.0 seconds.
    /// </remarks>
    private static readonly (bool IsMark, double Milliseconds)[] Reference =
    [
        (true, 150), (false, 30), (true, 65), (false, 36),
        (true, 161), (false, 24), (true, 68), (false, 126),
        (true, 155), (false, 36), (true, 98), (false, 17),
        (true, 68), (false, 24), (true, 157),
    ];

    /// <summary>Where the reference's CQ begins, in seconds.</summary>
    private const double From = 13.40;

    /// <summary>Where it ends, in seconds.</summary>
    /// <remarks>
    /// **14.80 AND NOT 14.72, BECAUSE THE TWO INSTRUMENTS SIT 48 ms APART.** The
    /// reference's last mark of the CQ ends at 14.715 and Hamlet's ends at 14.770,
    /// which is the constant group-delay offset and not a disagreement. A window
    /// cut at the reference's own figure would drop Hamlet's last mark and turn a
    /// perfect agreement into a length mismatch.
    /// </remarks>
    private const double To = 14.80;

    /// <summary>The pitch both instruments were given.</summary>
    private const double ToneHz = 583.5;

    /// <summary>
    /// The two streams hold the same elements in the same order, so the element
    /// extraction is not where the reading is lost.
    /// </summary>
    [Fact]
    public void TheElementStreamsAgreeOverTheCallsign()
    {
        var mine = HamletsStream();

        _output.WriteLine("     reference            Hamlet          apart");

        Assert.Equal(Reference.Length, mine.Count);

        var worst = 0.0;

        for (var i = 0; i < Reference.Length; i++)
        {
            var apart = Math.Abs(mine[i].Milliseconds - Reference[i].Milliseconds);

            worst = Math.Max(worst, apart);

            _output.WriteLine(
                $"  {(Reference[i].IsMark ? "MARK" : "gap "),-5}"
                + $"{Reference[i].Milliseconds,5:0} ms      "
                + $"{(mine[i].IsMark ? "MARK" : "gap "),-5}{mine[i].Milliseconds,5:0} ms"
                + $"   {apart,5:0} ms");

            Assert.Equal(Reference[i].IsMark, mine[i].IsMark);
        }

        _output.WriteLine("");
        _output.WriteLine(
            $"worst disagreement {worst:0} ms, which is {worst / 5:0} hops");

        // **THREE HOPS, WHICH IS WHERE THE MEASUREMENT SITS AND NOT A TARGET.** A
        // tighter bound would assert that two window shapes agree rather than that
        // two decoders found the same elements, which is the claim being made.
        Assert.True(
            worst <= 15,
            $"the streams disagree by {worst:0} ms, which is more than three hops "
            + "— the element extraction has come apart and task 4's finding no "
            + "longer holds");
    }

    /// <summary>
    /// The clock fit's short-run floor throws away almost nothing on real audio,
    /// so its missing merge cannot be the fault either.
    /// </summary>
    /// <remarks>
    /// **UNIT 054 PROVED THE BUG IS REAL AND THIS MEASURES THAT IT DOES NOT
    /// FIRE.** `Runs` drops a run below two hops without merging the two it
    /// separated, which would corrupt every duration after it. On this capture it
    /// drops one run in a hundred and forty; on most of the corpus, none at all.
    /// The hysteresis absorbs the notches before they reach the floor.
    /// </remarks>
    [Theory]
    [InlineData("cw-2026-08-31-003229", 583.5)]
    [InlineData("cw-2026-08-18-004507", 500.8)]
    [InlineData("cw-2026-08-18-003758", 498.6)]
    public void TheShortRunFloorDiscardsAlmostNothing(string capture, double toneHz)
    {
        var audio = WavAudio.Read(CapturePath(capture));

        var envelope = CwProbabilisticDecoder.Envelope(
            audio.Samples, audio.SampleRate, toneHz);

        var (marks, gaps) = CwUnitEstimator.Elements(
            envelope, CwProbabilisticDecoder.HopMilliseconds, out var dropped);

        var total = marks.Count + gaps.Count + dropped;

        _output.WriteLine(
            $"{capture}: {dropped} dropped of {total} runs "
            + $"({100.0 * dropped / total:0.0} per cent)");

        Assert.True(
            dropped <= total / 100,
            $"{dropped} of {total} runs were dropped without merging what they "
            + "separated, which is enough to corrupt the durations after them");
    }

    /// <summary>Hamlet's own elements over the same stretch, marks and gaps.</summary>
    private static IReadOnlyList<(bool IsMark, double Milliseconds)> HamletsStream()
    {
        var audio = WavAudio.Read(CapturePath("cw-2026-08-31-003229"));

        var envelope = CwProbabilisticDecoder.Envelope(
            audio.Samples, audio.SampleRate, ToneHz);

        var read = CwProbabilisticDecoder.Decode(envelope, ToneHz, null, null, false);

        var over = new List<(bool, double)>();

        foreach (var element in read.Elements)
        {
            var at = element.StartHop * CwProbabilisticDecoder.HopMilliseconds / 1000.0;
            var milliseconds = element.Hops * CwProbabilisticDecoder.HopMilliseconds;

            if (at < From || at + (milliseconds / 1000.0) > To)
            {
                continue;
            }

            over.Add((element.IsMark, milliseconds));
        }

        return over;
    }

    /// <summary>Where a captured fixture lives.</summary>
    private static string CapturePath(string capture)
    {
        var here = AppContext.BaseDirectory;

        while (!string.IsNullOrEmpty(here))
        {
            var folder = Path.Combine(here, "tests", "fixtures", "cw", "captured");

            if (Directory.Exists(folder))
            {
                return Directory
                    .GetFiles(folder, capture + ".wav", SearchOption.AllDirectories)
                    .Single();
            }

            here = Path.GetDirectoryName(here.TrimEnd(
                Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }

        throw new DirectoryNotFoundException(
            "no captured fixtures folder above " + AppContext.BaseDirectory);
    }
}
