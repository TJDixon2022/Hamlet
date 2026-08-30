using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The captures that read cleanly keep reading cleanly, checked one at a time and
/// never as an average.
/// </summary>
/// <remarks>
/// <para>**AN AVERAGE CAN RISE WHILE THE EASY CASES COLLAPSE, AND THAT IS WHAT
/// HAPPENED** (work instruction 053, tasks 1 and 2). Precision went 0.858 to
/// 0.888 across units 050 and 051 while the operator's experience of the
/// application got worse, and the bisect found why at `95a5e06`: on
/// `cw-2026-08-17-013347` the text went from **55 named characters and 3 blocks
/// to 9 named and 49 blocks** — five per cent blocks to eighty-four — **and the
/// precision did not move at all.**</para>
/// <para>**THE REASON IT DID NOT MOVE IS THE SHAPE OF THE MEASUREMENT.** That
/// capture's adjudicated truth is `VA3VRR`, six characters. `VA3VRR` survived, so
/// six of six were still correct and nothing else was scored. Everything the
/// decoder emits outside a six-character truth is invisible to `CwAccuracy` — it
/// is neither correct nor a substitution — so turning fifty characters into blocks
/// cost nothing in the number and everything on the screen.</para>
/// <para>**SO THIS TEST HAS TWO HALVES AND ONLY THE SECOND WOULD HAVE CAUGHT
/// IT.** The first is the floor the order asks for: a capture reading at 1.000
/// keeps reading at 1.000. **That half passes at `95a5e06` and at every commit
/// before and after it**, which is exactly the failure being fixed. The second
/// counts how much of what reaches the screen is a named character rather than a
/// block, per capture, and that half is the one with teeth.</para>
/// <para>**IT MAY NOT BE MODIFIED TO ACCOMMODATE A CHANGE**, in the same form as
/// `TheSilencePropertyIsLockedTests`. A change that raises the corpus average and
/// turns one of these to blocks fails here, and that is the whole point of it
/// existing separately from the average.</para>
/// </remarks>
public sealed class TheCleanReadsStayCleanTests
{
    private readonly ITestOutputHelper _output;

    public TheCleanReadsStayCleanTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>One capture that reads cleanly, and what it must keep doing.</summary>
    /// <param name="Name">The recording, under `tests/fixtures/cw`.</param>
    /// <param name="Truth">What was adjudicated, and who by, in the ruling.</param>
    /// <param name="Ruling">Who says so.</param>
    /// <param name="NamedFloor">
    /// **The fewest named characters this capture has ever put on the screen.** Not
    /// a target and not an answer key: a floor, measured at head on 2026-08-30, and
    /// it may rise and may not fall.
    /// </param>
    public readonly record struct CleanRead(
        string Name, string Truth, string Ruling, int NamedFloor);

    /// <summary>Every capture reading at 1.000, with what it must keep doing.</summary>
    /// <remarks>
    /// **THE FLOORS ARE MEASURED AT HEAD AND THEY ARE DELIBERATELY NOT THE
    /// PRE-SQUELCH NUMBERS.** `013347` put 55 named characters on the screen before
    /// `95a5e06` and puts 9 after. Setting the floor at 55 would assert that the
    /// squelch must be reverted, which is Tim's ruling and not this test's
    /// (§12.5, and the order forbids building a fix here). Setting it at 9 locks
    /// in what is true today so it cannot quietly get worse while somebody watches
    /// an average.
    /// </remarks>
    public static IReadOnlyList<CleanRead> All { get; } = new[]
    {
        new CleanRead(
            "captured/unadjudicated/cw-2026-08-24-012403",
            "DE KD0UN KD0UN K", "work instruction 011", 20),
        new CleanRead(
            "captured/unadjudicated/cw-2026-08-18-003758",
            "AA4MP/4 QNIK", "HM-DEC-126", 42),
        new CleanRead(
            "captured/cw-2026-08-17-013347", "VA3VRR", "HM-DEC-145", 9),
    };

    /// <summary>Each capture, for the theories below.</summary>
    public static TheoryData<CleanRead> Reads()
    {
        var data = new TheoryData<CleanRead>();

        foreach (var read in All)
        {
            data.Add(read);
        }

        return data;
    }

    /// <summary>The half the order asks for: a clean read stays clean.</summary>
    /// <remarks>
    /// **THIS HALF WOULD NOT HAVE CAUGHT THE THING TASK 1 FOUND**, and that is
    /// said here rather than left for somebody to discover. It is kept because it
    /// is the promise the ruling makes about these three recordings, and because a
    /// capture that stops containing its callsign altogether is a different and
    /// worse failure that nothing else watches for.
    /// </remarks>
    [Theory]
    [MemberData(nameof(Reads))]
    public void EachCleanCaptureStillContainsItsTruth(CleanRead read)
    {
        var text = Settled(read.Name);

        _output.WriteLine($"{read.Name} ({read.Ruling})");
        _output.WriteLine($"  looking for \"{read.Truth}\"");
        _output.WriteLine($"  reads: {text}");

        Assert.Contains(read.Truth, text, StringComparison.Ordinal);
    }

    /// <summary>The half with teeth: the screen does not fill up with blocks.</summary>
    /// <remarks>
    /// **A BLOCK IS HONEST AND A SCREEN FULL OF THEM IS STILL A REGRESSION.**
    /// Nothing here argues a block should be a letter — §0.0 and HM-DEC-120 are
    /// untouched, and the squelch that produced them is correct about what it may
    /// assert. What this asserts is narrower and is the thing no other test
    /// watches: **however few characters Hamlet is entitled to name on these three
    /// recordings, it does not come to name fewer.**
    /// </remarks>
    [Theory]
    [MemberData(nameof(Reads))]
    public void EachCleanCaptureStillNamesAsManyCharacters(CleanRead read)
    {
        var text = Settled(read.Name);

        var named = text.Count(c => c != ' ' && c.ToString() != MorseAlphabet.Unreadable);
        var blocks = text.Count(c => c.ToString() == MorseAlphabet.Unreadable);
        var emitted = named + blocks;

        _output.WriteLine(
            $"{read.Name}: {named} named against a floor of {read.NamedFloor}, "
            + $"{blocks} blocks, "
            + $"{(emitted == 0 ? 0 : blocks * 100.0 / emitted):0} per cent blocks");

        Assert.True(
            named >= read.NamedFloor,
            $"{read.Name} names {named} characters where it named "
            + $"{read.NamedFloor}; a floor may rise and may not fall (§12.5)");
    }

    /// <summary>
    /// The floors are what head actually produces, so none of them is aspirational.
    /// </summary>
    /// <remarks>
    /// <para>**A FLOOR SET ABOVE WHAT THE TREE DOES IS A FAILING TEST DRESSED AS
    /// A PROMISE.** This proves each was measured rather than hoped for, and it is
    /// the same discipline `TheAdjudicatedReadingsKeepReadingTests` applies to its
    /// anchors.</para>
    /// <para>**IT EARNED ITS PLACE IMMEDIATELY.** The first three floors written
    /// here were counted off a console line rather than measured through this
    /// harness, and were 24, 55 and 9 against the true 20, 42 and 9. This test is
    /// what caught them.</para>
    /// </remarks>
    [Fact]
    public void EveryFloorWasMeasuredAndNotHopedFor()
    {
        foreach (var read in All)
        {
            var text = Settled(read.Name);
            var named = text.Count(
                c => c != ' ' && c.ToString() != MorseAlphabet.Unreadable);

            _output.WriteLine($"{read.Name}: floor {read.NamedFloor}, actual {named}");

            Assert.True(
                read.NamedFloor <= named,
                $"{read.Name}'s floor of {read.NamedFloor} is above the "
                + $"{named} it actually names");
        }
    }

    /// <summary>What the shipping decoder settles on for one recording.</summary>
    private static string Settled(string name)
    {
        var audio = WavAudio.Read(
            Path.Combine(CwFixtures.Folder, name.Replace('/', Path.DirectorySeparatorChar) + ".wav"));

        var decoder = new CwDecoder(audio.SampleRate, 600);
        var text = new System.Text.StringBuilder();

        decoder.CharacterSettled += c => text.Append(c.Text);

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        return text.ToString();
    }
}
