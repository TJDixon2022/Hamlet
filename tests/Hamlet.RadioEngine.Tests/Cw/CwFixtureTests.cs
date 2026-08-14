using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The committed recordings, decoded off disk the way a captured failure would
/// be (HM-DEC-007, HM-DEC-048).
/// </summary>
/// <remarks>
/// The tests above this one generate their audio in memory, which proves the
/// decoder. These prove the evidence trail: the file on disk is the audio it
/// claims to be, it survives the WAV round trip, and decoding it gives what the
/// generator was asked for. §0.0.1 wants a wrong decode to arrive with its
/// input attached, and that is worth nothing if the attachment does not decode
/// the same way.
/// </remarks>
public sealed class CwFixtureTests
{
    public static TheoryData<string> FixtureNames()
    {
        var data = new TheoryData<string>();

        foreach (var fixture in CwFixtures.All)
        {
            data.Add(fixture.Name);
        }

        return data;
    }

    /// <remarks>
    /// Proves every recording is where it says it is. A fixture folder that
    /// silently emptied would leave every test below passing over nothing,
    /// which is the failure mode of every test that walks a directory.
    /// </remarks>
    [Fact]
    public void EveryFixtureIsOnDisk()
    {
        Assert.True(
            Directory.Exists(CwFixtures.Folder),
            $"no fixture folder at {CwFixtures.Folder}");

        Assert.NotEmpty(CwFixtures.All);

        foreach (var fixture in CwFixtures.All)
        {
            Assert.True(
                File.Exists(CwFixtures.PathOf(fixture)),
                $"missing fixture {fixture.Name}");
        }
    }

    /// <remarks>
    /// THE DRIFT GUARD. Every file is regenerated from the request beside it and
    /// compared byte for byte. Regenerating on purpose is a deliberate act; a
    /// fixture that changed quietly, because somebody touched the synthesizer
    /// or the noise generator, would take every assertion resting on it along
    /// with it and nothing would say so (§5).
    /// </remarks>
    [Theory]
    [MemberData(nameof(FixtureNames))]
    public void EveryFixtureIsStillTheAudioItWasGeneratedFrom(string name)
    {
        var fixture = CwFixtures.All.Single(f => f.Name == name);
        var regenerated = CwSignal.Generate(fixture.Request);
        var committed = CwFixtures.Read(fixture);

        Assert.Equal(regenerated.SampleRate, committed.SampleRate);
        Assert.Equal(regenerated.Samples.Length, committed.Samples.Length);

        // Sixteen-bit quantization is the only difference there should be.
        for (var i = 0; i < regenerated.Samples.Length; i++)
        {
            Assert.True(
                Math.Abs(regenerated.Samples[i] - committed.Samples[i]) < 1.0 / short.MaxValue,
                $"{name} differs at sample {i}");
        }
    }

    /// <remarks>
    /// Proves the clean recordings decode off disk to exactly what was sent,
    /// with the decoder sure about all of it. This is the same claim the
    /// in-memory tests make, made again through a file, because the file is what
    /// a bug report will carry.
    /// </remarks>
    [Theory]
    [InlineData("clean-12wpm")]
    [InlineData("clean-18wpm")]
    [InlineData("clean-25wpm")]
    public void TheCleanRecordingsDecodeExactly(string name)
    {
        var fixture = CwFixtures.All.Single(f => f.Name == name);
        var result = CwDecodeHarness.Decode(CwFixtures.Read(fixture));

        Assert.Equal(fixture.Sent, result.Text);
        Assert.InRange(
            result.WordsPerMinute,
            fixture.WordsPerMinute - 1,
            fixture.WordsPerMinute + 1);

        Assert.All(result.Letters, c => Assert.Equal(CwConfidence.High, c.Confidence));
    }

    /// <remarks>
    /// Proves the prosign recording comes back with prosigns in it rather than
    /// with the letters they are made of.
    /// </remarks>
    [Fact]
    public void TheProsignRecordingDecodesItsProsigns()
    {
        var fixture = CwFixtures.All.Single(f => f.Name == "prosigns-18wpm");
        var result = CwDecodeHarness.Decode(CwFixtures.Read(fixture));

        Assert.Contains("<BT>", result.Text, StringComparison.Ordinal);
        Assert.Contains("<SK>", result.Text, StringComparison.Ordinal);
        Assert.Empty(CwAlignment.ConfidentMistakes(result.Characters, fixture.Sent));
    }

    /// <remarks>
    /// THE CENTRAL CLAIM, made against every recording including the damaged
    /// ones. A degraded fixture is allowed to come out patchy, dimmed, or with
    /// characters missing entirely. It is not allowed to come out clean and
    /// wrong, because the person reading it will conclude the fault is theirs
    /// (§0.0).
    /// </remarks>
    [Theory]
    [MemberData(nameof(FixtureNames))]
    public void NothingTheDecoderWasSureOfIsWrong(string name)
    {
        var fixture = CwFixtures.All.Single(f => f.Name == name);
        var result = CwDecodeHarness.Decode(CwFixtures.Read(fixture));

        Assert.Empty(CwAlignment.ConfidentMistakes(result.Characters, fixture.Sent));
    }

    /// <remarks>
    /// Proves the recordings still say something. A decoder that met every
    /// impairment by going silent would pass the confident-mistakes test
    /// perfectly and be useless on the band it is meant for, so each fixture
    /// carries the share it has to give back and the reason that share is what
    /// it is.
    /// </remarks>
    [Theory]
    [MemberData(nameof(FixtureNames))]
    public void EveryRecordingGivesBackTheShareItShould(string name)
    {
        var fixture = CwFixtures.All.Single(f => f.Name == name);
        var result = CwDecodeHarness.Decode(CwFixtures.Read(fixture));

        var wanted = CwAlignment.SymbolCount(fixture.Sent);
        var got = result.Letters.Count(c => !c.IsUnreadable);

        Assert.True(
            got >= wanted * fixture.ReadableShare,
            $"{name} gave back {got} of {wanted}, short of the "
            + $"{fixture.ReadableShare:P0} it has to manage");
    }

    /// <remarks>
    /// Proves the whole set stays small enough to live in the repository. A
    /// fixture folder is only worth having if nobody is tempted to delete it,
    /// and this is the number that would make somebody tempted.
    /// </remarks>
    [Fact]
    public void TheWholeSetStaysSmallEnoughToCommit()
    {
        var bytes = CwFixtures.All
            .Select(f => new FileInfo(CwFixtures.PathOf(f)).Length)
            .Sum();

        Assert.True(
            bytes < 3 * 1024 * 1024,
            $"the fixtures now come to {bytes / 1024 / 1024.0:0.0} MB");
    }
}
