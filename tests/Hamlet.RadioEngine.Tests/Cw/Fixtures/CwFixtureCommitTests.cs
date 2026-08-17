using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// The committed fixtures are what the recipes produce (HM-OPEN-018 phase 1).
/// </summary>
/// <remarks>
/// **A GENERATED FIXTURE THAT HAS DRIFTED FROM ITS RECIPE IS A RECORDED FIXTURE
/// WITH EXTRA STEPS.** The whole advantage over an off-air capture is that
/// anybody who thinks it is wrong can change the recipe and rebuild it. That is
/// only true while the file on disk is the file the recipe makes, so this checks
/// it rather than trusting it.
/// </remarks>
public sealed class CwFixtureCommitTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the comparison is reported.</param>
    public CwFixtureCommitTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// Proves HM-OPEN-018 phase 1 and §5: what is committed is what the recipe
    /// builds, sample for sample.
    /// </remarks>
    [Theory]
    [MemberData(nameof(CwFixtureBuildTests.Names), MemberType = typeof(CwFixtureBuildTests))]
    public void TheCommittedFileIsWhatTheRecipeBuilds(string name)
    {
        var path = Path.Combine(CwFixtureCatalogue.Folder, name + ".wav");

        Assert.True(
            File.Exists(path),
            $"{name}.wav has not been generated. Run CwFixtureWriter.WriteAll.");

        var recipe = CwFixtureCatalogue.All.Single(r => r.Name == name);
        var (built, _) = CwFixtureGenerator.Generate(recipe);

        // **THE ENCODED BYTES, NOT THE SAMPLES.** A WAV carries sixteen-bit
        // integers and the generator works in floating point, so the values that
        // come back from a round trip are the quantized ones and never the
        // originals. Comparing what would be written against what is on disk is
        // both the stronger check and the one that can actually pass.
        using var encoded = new MemoryStream();
        WavAudio.Write(encoded, built);

        var expected = encoded.ToArray();
        var committed = File.ReadAllBytes(path);

        Assert.True(
            expected.Length == committed.Length,
            $"{name} is {committed.Length} bytes and its recipe builds "
            + $"{expected.Length}. Regenerate it.");

        for (var i = 0; i < expected.Length; i++)
        {
            if (expected[i] != committed[i])
            {
                Assert.Fail(
                    $"{name} differs from its recipe at byte {i}. Either the "
                    + "generator changed or the file was edited by hand.");
            }
        }
    }

    /// <remarks>
    /// <para>Proves HM-OPEN-018 phase 4, and it is the property the old fixtures
    /// lacked. **No rebuilt fixture may judge Hamlet until the validated
    /// reference chain has scored well on it.** A fixture the reference cannot
    /// decode is a bad fixture, not a Hamlet failure, and there was no way to tell
    /// those apart before.</para>
    /// <para>The score is committed data rather than something recomputed here,
    /// because running Python during a build is a dependency this repository does
    /// not otherwise have. What is enforced is that every fixture carries one and
    /// that it is good enough to judge anything by.</para>
    /// </remarks>
    [Theory]
    [MemberData(nameof(CwFixtureBuildTests.Names), MemberType = typeof(CwFixtureBuildTests))]
    public void TheReferenceHasScoredThisFixture(string name)
    {
        var path = Path.Combine(CwFixtureCatalogue.Folder, name + ".txt");

        Assert.True(File.Exists(path), $"{name}.txt is missing");

        var sidecar = File.ReadAllText(path);

        Assert.True(
            sidecar.Contains("reference", StringComparison.Ordinal),
            $"{name} carries no reference score. Run tools/score-fixtures "
            + "before letting it judge anything (HM-OPEN-018 phase 4).");

        var line = sidecar
            .Split('\n')
            .First(l => l.StartsWith("reference", StringComparison.Ordinal));

        _output.WriteLine($"{name}: {line.Trim()}");

        // **AND THE SCORE HAS TO BE ABOUT THIS FILE.** Regenerating a fixture
        // carries its score across, so that running the generator does not
        // silently disarm the gate; the size the score was taken on is what
        // separates a verdict that still applies from one about audio that no
        // longer exists.
        var stamp = sidecar
            .Split('\n')
            .FirstOrDefault(l => l.StartsWith("scoredBytes", StringComparison.Ordinal));

        Assert.True(
            stamp is not null,
            $"{name}'s score is not bound to any file. Re-run tools/score-fixtures.");

        var scored = long.Parse(stamp!.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
        var actual = new FileInfo(
            Path.Combine(CwFixtureCatalogue.Folder, name + ".wav")).Length;

        Assert.True(
            scored == actual,
            $"{name} was scored at {scored} bytes and is now {actual}. The score "
            + "describes audio that no longer exists; re-run tools/score-fixtures.");

        var recipe = CwFixtureCatalogue.All.Single(r => r.Name == name);

        // **THE EDGE TIER IS EXEMPT AND THAT IS THE POINT OF IT.** At zero
        // decibels the decoder refuses by ruling (HM-DEC-097), so a reference
        // that also declines to read it is the fixture behaving correctly rather
        // than the fixture being bad.
        if (recipe.SignalToNoiseDb <= CwFixtureCatalogue.EdgeDb)
        {
            return;
        }

        if (NotYetAdmissible.Contains(name))
        {
            return;
        }

        Assert.False(
            line.Contains("nothing", StringComparison.OrdinalIgnoreCase),
            $"{name} is above the edge tier and the reference read nothing from "
            + "it, so the fixture is wrong rather than the decoder");
    }

    /// <summary>
    /// Fixtures the reference cannot read, and which therefore may not judge
    /// Hamlet (HM-OPEN-018 phase 4).
    /// </summary>
    /// <remarks>
    /// <para>**THE GATE'S WHOLE PURPOSE IS THAT A FIXTURE THE REFERENCE CANNOT
    /// DECODE PROVES NOTHING ABOUT THE DECODER**, so anything named here is held
    /// out of every assertion about Hamlet until somebody resolves why. It is not
    /// a place to park a fixture that is inconvenient, and it has never been used
    /// as one: three entries were cleared by fixing the generator and one by
    /// fixing the reference, and none by lowering the bar.</para>
    /// <para>`tightfist-easy`, `tightfist-working` and `qsk-preamble` were held
    /// because the tight fist had been generated from a fifty millisecond
    /// window's measurement of a real station rather than from the station, so
    /// the measurement bias was applied twice (HM-DEC-101). At the station's true
    /// timing they score 100, 64 and 100.</para>
    /// <para>**THE ONE THAT REMAINS IS THE REFERENCE'S GAP CLASSIFIER RATHER
    /// THAN THE FIXTURE** (HM-DEC-103). At twenty-five words a minute `fast-easy`
    /// hands the reference a clock it fits correctly — dit 55, dah 152, ratio
    /// 2.77 — and then comes apart classifying the gaps, returning every element
    /// as its own character: `TETETTET TETETTET TEEE`.</para>
    /// <para>`fast-working` is the same message at the same speed ten decibels
    /// weaker and reads at 63 percent, which is what says the fixture is sound.
    /// The classifier takes the two largest multiplicative steps in the sorted
    /// gaps, and on a strong fast signal one stray short gap produces a step
    /// larger than the real element-to-character one, so the boundary lands below
    /// the element gaps and every one of them ends a character.</para>
    /// <para>Recorded as a discovery rather than fixed, per the work order:
    /// **fix only what is clearly a defect.** The reference not implementing its
    /// own written bandwidth rule was clearly a defect and was fixed; a
    /// classifier brittle at one corner is a finding about the control.</para>
    /// </remarks>
    public static IReadOnlySet<string> NotYetAdmissible { get; } =
        new HashSet<string>(StringComparer.Ordinal) { "fast-easy" };

    /// <remarks>
    /// Proves HM-OPEN-018 phase 4: **the held-out list is small and named.** A
    /// gate that quietly grows a list of exceptions is not a gate, so this fails
    /// if the exceptions ever outnumber the fixtures they are excepted from.
    /// </remarks>
    [Fact]
    public void TheHeldOutListIsSmallerThanTheSuiteItIsHeldOutOf()
    {
        Assert.True(
            NotYetAdmissible.Count * 3 < CwFixtureCatalogue.All.Count,
            $"{NotYetAdmissible.Count} of {CwFixtureCatalogue.All.Count} fixtures "
            + "are held out, which is no longer a gate");

        foreach (var name in NotYetAdmissible)
        {
            Assert.Contains(CwFixtureCatalogue.All, r => r.Name == name);
        }
    }
}
