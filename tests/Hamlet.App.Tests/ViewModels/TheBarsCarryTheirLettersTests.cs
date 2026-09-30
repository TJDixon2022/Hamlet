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
/// <para>**SINCE WORK INSTRUCTION 485 (R97)** no level trace is drawn, a letter is drawn only over
/// blocks, and nothing is drawn while the detector says no keying; the tests below were
/// rewritten to say so.</para>
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
    /// Proves the picture since R97 (work instruction 485): four blocks for C, C over its own span,
    /// and the two lines of words; no level trace.
    /// </remarks>
    [Fact]
    public void FourBarsForCWithCOverThem()
    {
        var (frame, items) = KeyedC(settle: true);

        foreach (var item in items)
        {
            _output.WriteLine($"{item.Kind,-9} {item.X,7:0.0} {item.X2,7:0.0}  {item.Text}");
        }

        var bars = items.Where(i => i.Kind == CwScopeItemKind.Bar).ToList();
        var letter = Assert.Single(items, i => i.Kind == CwScopeItemKind.Letter);

        Assert.DoesNotContain(items, i => i.Kind == CwScopeItemKind.Listening);
        Assert.Equal(4, bars.Count);
        Assert.Equal("C", letter.Text);
        Assert.InRange((letter.X + letter.X2) / 2, bars[0].X, bars[^1].X2);
        Assert.All(
            items,
            i => Assert.Contains(i.Kind, new[] { CwScopeItemKind.Bar, CwScopeItemKind.Letter, CwScopeItemKind.Tone, CwScopeItemKind.Mixing }));
        Assert.Equal(4, frame.Training.Bars.Count);
    }

    /// <remarks>
    /// Proves nothing heard draws nothing: no bar, no letter, and the small word
    /// "listening".
    /// </remarks>
    [Fact]
    public void NoBarsDrawNothingButListening()
    {
        var frame = CwScopeFrame.Empty with { Training = new CwTrainingGraph().Frame(DateTime.UtcNow) };
        var items = CwScopeControl.Items(frame, Width);

        Assert.DoesNotContain(items, i => i.Kind is CwScopeItemKind.Bar or CwScopeItemKind.Letter);
        Assert.Contains(items, i => i.Kind == CwScopeItemKind.Listening && i.Text == "listening");
    }

    /// <remarks>
    /// Proves a letter the terminal printed is drawn even where no block stands beneath it (work
    /// instruction numbered 508, run as unit 509, R100, R106, HM-DEC-213). It replaces
    /// `ALetterWithNoBlocksBeneathItIsNotDrawn`, which work instruction 485 wrote under R97: R102
    /// took the detector's keying and blocks off what is emitted, and the scroll's letters are now
    /// the terminal's, so a letter it printed is on the scroll whatever the detector's blocks say.
    /// </remarks>
    [Fact]
    public void APrintedLetterWithNoBlockBeneathItIsStillDrawn()
    {
        var graph = new CwTrainingGraph();
        var now = new DateTime(2026, 9, 28, 17, 15, 0, DateTimeKind.Utc);

        graph.Settle(Character("E", ".", TimeSpan.FromSeconds(9), 12), TimeSpan.FromSeconds(10), now);

        var frame = CwScopeFrame.Empty with
        {
            Reading = CwEnvelopeReading.None with { Keying = true },
            Training = graph.Frame(now),
        };

        Assert.Empty(frame.Training.Bars);
        Assert.Single(frame.Training.Letters);
        Assert.Single(CwScopeControl.Items(frame, Width), i => i.Kind == CwScopeItemKind.Letter && i.Text == "E");
    }

    /// <remarks>
    /// Proves the hovers: over a block its length and dit or dah, over the empty plot the scope's
    /// own words, over a letter where it came from and how sure the decoder was.
    /// </remarks>
    [Fact]
    public void TheHoverSaysABarsLengthAndWhereALetterCameFrom()
    {
        var (frame, items) = KeyedC(settle: true);
        var dah = items.First(i => i.Kind == CwScopeItemKind.Bar);

        var barTip = CwScopeControl.TipAt(frame, Width, new Avalonia.Point((dah.X + dah.X2) / 2, CwScopeControl.BarTop + 2));
        var plotTip = CwScopeControl.TipAt(frame, Width, new Avalonia.Point((dah.X + dah.X2) / 2, CwScopeControl.TraceTop + 10));

        var letter = items.Single(i => i.Kind == CwScopeItemKind.Letter);
        var letterTip = CwScopeControl.TipAt(frame, Width, new Avalonia.Point((letter.X + letter.X2) / 2, CwScopeControl.LetterTop + 4));

        _output.WriteLine($"bar: {barTip}; plot: {plotTip}; letter: {letterTip}");

        Assert.StartsWith("dah, ", barTip, StringComparison.Ordinal);
        // Since work instruction 502 the length is followed by the shape score, or by the words for none.
        Assert.Matches(@" ms, (shape \d\.\d\d of 1|not handed out as a mark)$", barTip);
        Assert.Equal(CwHearingViewModel.ScopeTip, plotTip);
        Assert.Equal("C: " + CwScopeControl.LetterTipWords + "; sure, 93% likely right", letterTip);
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
