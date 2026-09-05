using Ft8Sharp.Tests.Dsp;
using Xunit;

namespace Ft8Sharp.Tests.Fixtures;

/// <summary>
/// <b>The committed example is exactly what the code that built it builds today.</b>
/// </summary>
/// <remarks>
/// <para>
/// A generated artefact that nothing regenerates is a file whose provenance decays: six units from
/// now nobody can say whether it still corresponds to the code that made it, and the only honest
/// answer is <em>we would have to rebuild it to know</em>. So this rebuilds it, every run.
/// </para>
/// <para>
/// <b>To regenerate it after a deliberate change</b>, set <c>HAMLET_WRITE_FT8_EXAMPLE=1</c>:
/// <c>dotnet test tests/Ft8Sharp.Tests -e HAMLET_WRITE_FT8_EXAMPLE=1 --filter Ft8ExampleFixtureTests</c>.
/// That is the pattern <c>Ft8TableGenerationTests</c> already uses for the checked-in tables, and it
/// keeps the generator and the check the same code.
/// </para>
/// </remarks>
public class Ft8ExampleFixtureTests
{
    private const string WriteVariable = "HAMLET_WRITE_FT8_EXAMPLE";

    [Fact]
    public void TheCommittedExampleIsWhatTheGeneratorBuilds()
    {
        var (wav, fixture) = Ft8ExampleFixture.Build();
        var text = Ft8ExampleFixture.FileText(fixture);

        if (Environment.GetEnvironmentVariable(WriteVariable) is { Length: > 0 })
        {
            Directory.CreateDirectory(Ft8ExampleFixture.Directory);
            File.WriteAllBytes(Ft8ExampleFixture.CommittedCapturePath, wav);
            File.WriteAllText(Ft8ExampleFixture.CommittedFixturePath, text);
        }

        Assert.True(
            File.Exists(Ft8ExampleFixture.CommittedCapturePath),
            $"The example capture is not committed at {Ft8ExampleFixture.CommittedCapturePath}. "
            + $"Regenerate it with {WriteVariable}=1.");

        Assert.True(
            File.Exists(Ft8ExampleFixture.CommittedFixturePath),
            $"The example fixture is not committed at {Ft8ExampleFixture.CommittedFixturePath}. "
            + $"Regenerate it with {WriteVariable}=1.");

        // The audio is compared by digest rather than by array, because the digest is the thing the
        // fixture actually asserts about it and a byte-by-byte diff of 360044 bytes helps nobody.
        Assert.Equal(
            Ft8CaptureFixture.HashOfBytes(wav),
            Ft8CaptureFixture.HashOf(Ft8ExampleFixture.CommittedCapturePath));

        Assert.Equal(
            text.Replace("\r\n", "\n"),
            File.ReadAllText(Ft8ExampleFixture.CommittedFixturePath).Replace("\r\n", "\n"));
    }

    [Fact]
    public void TheCommittedExampleReadsBackAndItsCaptureIsThere()
    {
        var fixture = Ft8CaptureFixture.Read(Ft8ExampleFixture.CommittedFixturePath);

        Assert.Equal(Ft8CaptureFixture.CurrentFormat, fixture.FormatVersion);
        Assert.Equal(Ft8ExampleFixture.CaptureName, fixture.CaptureName);
        Assert.Equal(Ft8ExampleFixture.Utc, fixture.Utc);
        Assert.Equal(Ft8CaptureFixture.ProvenanceExample, fixture.Provenance);
        Assert.Equal(3, fixture.Rows.Count);

        // Refusals 1 and 2 do NOT fire: the capture is beside it and its hash is the recorded one.
        Assert.Equal(Ft8ExampleFixture.CommittedCapturePath, fixture.RequireCapture());
    }

    /// <summary>
    /// <b>The example's own rows are what the ladder put into the audio</b> — same messages, same
    /// frequencies, same offsets. If this drifts, the file has stopped being ground truth.
    /// </summary>
    [Fact]
    public void TheExampleRowsAreTheTruthTheLadderPutIn()
    {
        var (_, fixture) = Ft8ExampleFixture.Build();

        Assert.Equal([1000.0, 1500.0, 2000.0], fixture.Rows.Select(r => r.FrequencyHz));
        Assert.Equal(
            [0.48, 0.64, 0.80],
            fixture.Rows.Select(r => Math.Round(r.DtSeconds, 2)));
        Assert.All(fixture.Rows, r => Assert.NotEmpty(r.Message));
        Assert.All(fixture.Rows, r => Assert.Equal(r.Message, ReferenceRecording.Normalise(r.Message)));

        // The delivered ratio is near the commanded one but is not asserted equal to it: the ladder's
        // own distinction between requested and delivered is the whole reason the row carries the
        // delivered figure.
        Assert.All(
            fixture.Rows,
            r => Assert.InRange(r.SnrDb, Ft8ExampleFixture.RungDecibels - 1.0, Ft8ExampleFixture.RungDecibels + 1.0));
    }
}
