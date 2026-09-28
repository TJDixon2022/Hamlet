using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **BARS ONLY, AND THE LETTER OVER THEM** (work instruction 480 task 3, step 12 criterion 12.2
/// rewritten, R95, HM-DEC-188).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-28**: *"I don't care when it's noise. We don't need to show that. When we
/// start to detect bars, I want to graph those. As we start to find letters, mark them in that
/// graph and put the letter over top."*</para>
/// <para>**A DRIVEN DETECTOR AND A DRIVEN SETTLE, NO RECORDING** (R88): C keyed here as a tone
/// in seeded noise, dah dit dah dit at twenty words a minute, and a settled C handed over with
/// the span that audio gave it. Headless: the items are what the control draws, and nothing
/// is drawn that is not an item.</para>
/// </remarks>
public sealed class TheBarsCarryTheirLettersTests
{
    private const int Rate = 8_000;
    private const double Width = 800;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the canvas is printed.</param>
    public TheBarsCarryTheirLettersTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// Proves the picture: four bars for C, the letter C centered over them, no trace, and
    /// nothing else on the canvas but the two lines of words in the corner.
    /// </remarks>
    [Fact]
    public void FourBarsForCHaveCAboveThemAndNothingElse()
    {
        var (frame, items) = KeyedC(settle: true);

        foreach (var item in items)
        {
            _output.WriteLine($"{item.Kind,-9} {item.X,7:0.0} {item.X2,7:0.0}  {item.Text}");
        }

        var bars = items.Where(i => i.Kind == CwScopeItemKind.Bar).ToList();
        var letter = Assert.Single(items, i => i.Kind == CwScopeItemKind.Letter);

        Assert.DoesNotContain(items, i => i.Kind == CwScopeItemKind.Trace);
        Assert.DoesNotContain(items, i => i.Kind == CwScopeItemKind.Listening);
        Assert.Equal(4, bars.Count);
        Assert.Equal("C", letter.Text);
        Assert.InRange(letter.X, bars[0].X, bars[^1].X2);
        Assert.All(
            items,
            i => Assert.Contains(i.Kind, new[] { CwScopeItemKind.Bar, CwScopeItemKind.Letter, CwScopeItemKind.Tone, CwScopeItemKind.Mixing }));
        Assert.Equal(4, frame.Training.Bars.Count);
    }

    /// <remarks>
    /// Proves silence draws nothing: no trace, no bar, no letter, and the small word
    /// "listening".
    /// </remarks>
    [Fact]
    public void NoBarsDrawNothingButListening()
    {
        var frame = CwScopeFrame.Empty with { Training = new CwTrainingGraph().Frame(DateTime.UtcNow) };
        var items = CwScopeControl.Items(frame, Width);

        Assert.DoesNotContain(items, i => i.Kind is CwScopeItemKind.Trace or CwScopeItemKind.Bar or CwScopeItemKind.Letter);
        Assert.Contains(items, i => i.Kind == CwScopeItemKind.Listening && i.Text == "listening");
    }

    /// <remarks>
    /// Proves an invention is visible: a letter the decoder settled where the detector found no
    /// bar is still written, above empty space.
    /// </remarks>
    [Fact]
    public void ALetterWithNoBarsIsStillWritten()
    {
        var graph = new CwTrainingGraph();
        var now = new DateTime(2026, 9, 28, 17, 15, 0, DateTimeKind.Utc);

        graph.Settle(Character("E", ".", TimeSpan.FromSeconds(9), 12), TimeSpan.FromSeconds(10), now);

        var frame = CwScopeFrame.Empty with { Training = graph.Frame(now) };
        var items = CwScopeControl.Items(frame, Width);

        Assert.Empty(frame.Training.Bars);
        Assert.Contains(items, i => i.Kind == CwScopeItemKind.Letter && i.Text == "E");
        Assert.DoesNotContain(items, i => i.Kind == CwScopeItemKind.Listening);
    }

    /// <remarks>
    /// Proves the hovers: over a bar its length and dit or dah, over a letter its class and
    /// confidence.
    /// </remarks>
    [Fact]
    public void TheHoverSaysABarsLengthAndALettersConfidence()
    {
        var (frame, items) = KeyedC(settle: true);
        var dah = items.First(i => i.Kind == CwScopeItemKind.Bar);
        var letter = items.Single(i => i.Kind == CwScopeItemKind.Letter);

        var barTip = CwScopeControl.TipAt(frame, Width, new Avalonia.Point((dah.X + dah.X2) / 2, CwScopeControl.BarTop + 2));
        var letterTip = CwScopeControl.TipAt(frame, Width, new Avalonia.Point(letter.X, CwScopeControl.LetterTop + 4));

        _output.WriteLine($"bar: {barTip}; letter: {letterTip}");

        Assert.StartsWith("dah, ", barTip, StringComparison.Ordinal);
        Assert.EndsWith(" ms", barTip, StringComparison.Ordinal);
        Assert.Contains("C, sure", letterTip, StringComparison.Ordinal);
    }

    private static (CwScopeFrame Frame, IReadOnlyList<CwScopeItem> Items) KeyedC(bool settle)
    {
        // Half a second of noise, then C at 20 words a minute (dah dit dah dit, 60 ms units),
        // then 300 ms of noise so the last dit has paired and dropped.
        var detector = new CwEnvelopeDetector(Rate);
        detector.SetPassband(750, 500);

        var units = new[] { (3, true), (1, false), (1, true), (1, false), (3, true), (1, false), (1, true) };
        var random = new Random(480);
        var samples = new List<float>();

        void Add(double ms, bool on)
        {
            for (var i = 0; i < ms * Rate / 1000; i++)
            {
                var n = samples.Count;
                var noise = 0.01 * ((random.NextDouble() * 2) - 1);
                samples.Add((float)((on ? 0.3 * Math.Sin(2 * Math.PI * 750 * n / Rate) : 0) + noise));
            }
        }

        Add(500, false);
        var startMs = samples.Count * 1000.0 / Rate;

        foreach (var (length, on) in units)
        {
            Add(length * 60, on);
        }

        var endMs = samples.Count * 1000.0 / Rate;
        Add(300, false);

        detector.Process(samples.ToArray());

        var now = new DateTime(2026, 9, 28, 17, 15, 0, DateTimeKind.Utc);
        var heard = TimeSpan.FromMilliseconds(samples.Count * 1000.0 / Rate);
        var graph = new CwTrainingGraph();

        graph.Update(detector.History(), detector.HopMs, now);

        if (settle)
        {
            var spanHops = (int)Math.Round((endMs - startMs) / CwProbabilisticDecoder.HopMilliseconds);
            graph.Settle(Character("C", "-.-.", TimeSpan.FromMilliseconds(endMs), spanHops), heard, now);
        }

        var frame = CwScopeFrame.From(detector.History(), detector.HopMs, detector.Reading) with
        {
            Training = graph.Frame(now),
        };

        return (frame, CwScopeControl.Items(frame, Width));
    }

    private static CwCharacter Character(string text, string pattern, TimeSpan at, int spanHops)
        => new(text, CwConfidence.High, 0.9, pattern, 20, 20, at) { SpanHops = spanHops, Probability = 0.93 };
}
