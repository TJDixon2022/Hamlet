using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE SEQUENCE READER SHIPS ON** (the owner's ruling on HM-DEC-295, 2026-10-27): <see cref="CwRules.SequenceReader"/> is on by
/// default and is the only path to the printer. These hold what it reads at the ship that no other guard holds: the far side of a
/// QSO, <c>two-station</c>, at 27 of 27, and the easy receiver fixtures at 90 of 90, each read and scored as the survey reads it
/// (<see cref="TheCwSurveyTests.Truthful"/>). **HM-DEC-304** (work instruction 599, the owner's ruling of 2026-11-06): the learned reader
/// ships on, so these read the chain as it now ships and hold what it reads: <c>two-station</c> 27 of 27, unchanged, and the easy fixtures
/// 87 of 90 (were 90). <see cref="TheReaderIsOnByDefault"/> holds the sequence reader's own switch, which is unchanged; the learned reader's
/// switch decides which of the two is built.
/// </summary>
public sealed class TheSequenceReaderShipsTests(ITestOutputHelper output)
{
    private static string Receiver(string name) => Path.Combine(CwFixtures.Folder, "receiver", name + ".wav");

    [Fact]
    public void TheReaderIsOnByDefault() => Assert.True(CwRules.On(CwRules.SequenceReader), "the sequence reader is not on by default");

    [Fact]
    public void TheFarSideReadsWhole()
    {
        var wav = Receiver("two-station");
        var row = TheCwSurveyTests.Truthful("receiver fixtures: ungraded", wav, TheCwSurveyTests.SheetText(wav), "its sheet", 600, 500);

        output.WriteLine($"two-station: `{row.Read}`, {row.Right} of {row.OutOf}");
        Assert.Equal(27, row.OutOf);
        Assert.Equal(27, row.Right);
    }

    [Fact]
    public void TheEasyFixturesReadWhole()
    {
        var total = 0;
        var outOf = 0;

        foreach (var wav in Directory.GetFiles(Path.Combine(CwFixtures.Folder, "receiver"), "*-easy.wav").Order())
        {
            var row = TheCwSurveyTests.Truthful("receiver fixtures: easy", wav, TheCwSurveyTests.SheetText(wav), "its sheet", 600, 500);

            total += row.Right ?? 0;
            outOf += row.OutOf ?? 0;
            output.WriteLine($"{Path.GetFileNameWithoutExtension(wav)}: `{row.Read}`, {row.Right} of {row.OutOf}");
        }

        Assert.Equal(90, outOf);
        Assert.True(total >= 87, $"the easy fixtures read {total} of 90, under the learned reader's 87");
    }
}
