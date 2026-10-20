using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE FIST BANK IS WHAT ITS RECIPES SAY** (work instruction 580, HM-DEC-284): ninety-six fists, each named and seeded once, each
/// keying the bank's text. The survey reads and scores them (<see cref="TheCwSurveyTests"/>).
/// </summary>
public sealed class TheFistBankTests(ITestOutputHelper output)
{
    /// <remarks>Every ratio, speed, signal and fade once, each name and seed its own.</remarks>
    [Fact]
    public void TheBankIsNinetySixFists()
    {
        Assert.Equal(96, CwFistBank.All.Count);
        Assert.Equal(96, CwFistBank.All.Select(f => f.Name).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(96, CwFistBank.All.Select(f => f.Seed).Distinct().Count());
    }

    /// <remarks>Each fist keys every mark of its text, each within its spread of a dit or of the recipe's dah.</remarks>
    [Fact]
    public void EachFistKeysItsText()
    {
        foreach (var fist in CwFistBank.All)
        {
            var edges = CwFistBank.Edges(fist);
            var marks = Enumerable.Range(0, edges.Length / 2).Select(i => edges[(2 * i) + 1] - edges[2 * i]).ToList();
            var dit = 1.2 / fist.Wpm;

            Assert.Equal(CwFistBank.Text.Where(c => c != ' ').Sum(c => AHandIsReadAgainstItselfTests.Morse[c].Length), marks.Count);
            Assert.All(marks, m => Assert.True(m >= 0.85 * dit - 1e-9 && m <= 1.15 * fist.Ratio * dit + 1e-9, $"{fist.Name}: a mark of {m * 1000:0} ms"));
        }
    }

    /// <remarks>The four corners read through the app's chain: the machine-like corner should read whole today. Asserts nothing.</remarks>
    [Fact]
    public void TheCornersRead()
    {
        foreach (var fist in new[] { new CwFist(3.0, 20, 20, false), new CwFist(1.6, 20, 20, false), new CwFist(3.0, 20, 5, true), new CwFist(1.6, 10, 5, true) })
        {
            var audio = CwFistBank.Audio(fist);
            var (_, text) = TheRecordingsScoreboardTests.ReadLive(audio.Samples, audio.SampleRate, 600, 500);

            output.WriteLine($"{fist.Name} ({fist.Recipe}): `{text}`, {TheRecordingsScoreboardTests.Right(text, CwFistBank.Text)} of 60");
        }
    }
}
