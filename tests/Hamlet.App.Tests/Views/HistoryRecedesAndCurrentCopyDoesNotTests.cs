using Avalonia.Controls.Documents;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Current copy is bright, everything older recedes, and nothing is deleted.
/// </summary>
/// <remarks>
/// <para>**THE EYE HAD NOTHING TO LAND ON.** The night of 2026-08-25 ended with
/// a transcript whose first hundred characters were soup decoded two minutes
/// earlier, at full strength, sitting above three correctly-read callsign
/// tokens. Everything on the instrument was equally bright, so the operator
/// could not see that Hamlet had read `WB8SC`, `SKSK` and `KE8P` for him.</para>
/// <para>**NOTHING HERE HIT-TESTS**, per unit 1.11.13's rule: the headless
/// geometry offset is still unexplained, and three visible faults have already
/// hidden behind a green hit test. What is asserted is the ink each character is
/// actually drawn with, which is the fault itself rather than a proxy for it.</para>
/// </remarks>
public sealed class HistoryRecedesAndCurrentCopyDoesNotTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the inks are printed.</param>
    public HistoryRecedesAndCurrentCopyDoesNotTests(ITestOutputHelper output)
        => _output = output;

    private static CwCharacter Letter(string text)
        => new(text, CwConfidence.High, 1, ".-", 20, 18, TimeSpan.Zero);

    /// <summary>Feed a terminal some settled characters and let it draw.</summary>
    private static CwTerminalControl Filled(int characters)
    {
        var transcript = new CwTranscript();
        var terminal = new CwTerminalControl { Transcript = transcript };

        for (var i = 0; i < characters; i++)
        {
            // Enough distinct letters that runs break naturally rather than
            // becoming one long run the boundary can never fall inside.
            transcript.Settle(Letter(((char)('A' + (i % 26))).ToString()));
        }

        terminal.Draw();

        return terminal;
    }

    private static IReadOnlyList<Run> Runs(CwTerminalControl terminal)
        => terminal.Inlines?.OfType<Run>().ToList() ?? new List<Run>();

    private static int Length(IEnumerable<Run> runs)
        => runs.Sum(r => r.Text?.Length ?? 0);

    /// <remarks>
    /// <para>Proves the ruling of 2026-08-26 at its narrowest: everything before
    /// the most recent stretch is drawn in a receded ink and the stretch itself
    /// is not.</para>
    /// <para>The boundary is drawn at a run rather than inside one, so a little
    /// more than <see cref="CwTranscript.RecentCharacters"/> stays bright — the
    /// assertion is that the bright tail covers the recent stretch and that
    /// something older than it has receded, which is what the operator sees.</para>
    /// </remarks>
    [AvaloniaFact]
    public void EverythingBeforeTheRecentStretchRecedes()
    {
        var terminal = Filled(CwTranscript.RecentCharacters * 3);
        var runs = Runs(terminal);

        Assert.True(runs.Count > 1, "the transcript drew as a single run");

        var bright = runs
            .Where(r => ReferenceEquals(
                r.Foreground, InstrumentPalette.For(CwConfidence.High)))
            .ToList();

        var receded = runs
            .Where(r => ReferenceEquals(
                r.Foreground, InstrumentPalette.HistoryFor(CwConfidence.High)))
            .ToList();

        _output.WriteLine(
            $"{runs.Count} runs, {Length(bright)} characters bright, "
            + $"{Length(receded)} receded, recent stretch is "
            + $"{CwTranscript.RecentCharacters}");

        Assert.True(
            Length(receded) > 0,
            "nothing receded, so old copy is still as bright as current copy");

        Assert.True(
            Length(bright) >= CwTranscript.RecentCharacters,
            $"only {Length(bright)} characters are bright and the recent stretch "
            + $"is {CwTranscript.RecentCharacters}");

        // **THE BRIGHT TAIL IS THE TAIL.** Every receded run comes before every
        // bright one; history receding out of order would be worse than none.
        var firstBright = runs
            .Select((run, index) => (run, index))
            .First(pair => ReferenceEquals(
                pair.run.Foreground, InstrumentPalette.For(CwConfidence.High)))
            .index;

        Assert.True(
            runs.Skip(firstBright).All(r => !ReferenceEquals(
                r.Foreground, InstrumentPalette.HistoryFor(CwConfidence.High))),
            "a receded run sits after a bright one");
    }

    /// <remarks>
    /// Proves nothing is deleted. Dimming and trimming are different things and
    /// only the second removes text; the operator can still read and select every
    /// character of the history, which is the whole point of receding it rather
    /// than dropping it.
    /// </remarks>
    [AvaloniaFact]
    public void NothingIsDeleted()
    {
        const int written = CwTranscript.RecentCharacters * 3;

        var terminal = Filled(written);

        _output.WriteLine(
            $"{written} characters written, {Length(Runs(terminal))} on the screen");

        Assert.Equal(written, Length(Runs(terminal)));
    }

    /// <remarks>
    /// Proves a short transcript is left alone. Where everything is current copy
    /// there is no history to push back, and receding some of it would invent a
    /// distinction the operator cannot act on.
    /// </remarks>
    [AvaloniaFact]
    public void AShortTranscriptIsAllCurrentCopy()
    {
        var terminal = Filled(CwTranscript.RecentCharacters / 2);
        var runs = Runs(terminal);

        _output.WriteLine($"{Length(runs)} characters, {runs.Count} runs");

        Assert.True(
            runs.All(r => !ReferenceEquals(
                r.Foreground, InstrumentPalette.HistoryFor(CwConfidence.High))),
            "part of a transcript shorter than the recent stretch has receded");
    }

    /// <remarks>
    /// <para>Proves §0.6 survives the change: history keeps each confidence's own
    /// hue rather than collapsing them together. A placeholder is still amber and
    /// an uncertain character still the dimmer green once they are old, so
    /// receding cannot become a fourth confidence state.</para>
    /// <para>And receded ink is not the surface: a character nobody can read is
    /// deleted in everything but name.</para>
    /// </remarks>
    [AvaloniaFact]
    public void RecededInkKeepsItsOwnHueAndStaysReadable()
    {
        var inks = new[]
        {
            CwConfidence.High, CwConfidence.Low, CwConfidence.Unreadable,
        }
        .Select(c => ((SolidColorBrush)InstrumentPalette.HistoryFor(c)).Color)
        .ToList();

        foreach (var (confidence, ink) in new[]
        {
            CwConfidence.High, CwConfidence.Low, CwConfidence.Unreadable,
        }.Zip(inks))
        {
            _output.WriteLine($"{confidence,-10} receded to {ink}");
        }

        Assert.Equal(inks.Count, inks.Distinct().Count());

        foreach (var ink in inks)
        {
            Assert.NotEqual(InstrumentPalette.Surface, ink);
        }
    }
}
