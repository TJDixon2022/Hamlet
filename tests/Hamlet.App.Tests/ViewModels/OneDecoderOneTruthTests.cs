using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// One decoder, one truth: the terminal's letters are the letters the scroll draws over the
/// blocks (work instruction 493, R106, HM-DEC-198).
/// </summary>
/// <remarks>
/// **SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96). Wired as the tab wires it: the
/// decoder given the detector's marks, every settled character to the terminal and to the scope's
/// feed, the scope ticked every hundred milliseconds on the audio's clock. The terminal's text is
/// every settled letter; the scroll's is every letter the scope ever drew over its blocks.
/// </remarks>
public sealed class OneDecoderOneTruthTests
{
    private const int Rate = 8000;
    private const int Chunk = 80;
    private const string Call = "CQ CQ DE N0CALL N0CALL K";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each case's two surfaces are printed.</param>
    public OneDecoderOneTruthTests(ITestOutputHelper output) => _output = output;

    private static float[] Station(string text, int wpm, double hz, double amplitude, double noise, double lead, int seed)
        => CwSignal.Generate(new CwSignalRequest(
            text, WordsPerMinute: wpm, ToneHz: hz, SampleRate: Rate, Amplitude: amplitude,
            NoiseAmplitude: noise, LeadInSeconds: lead, TailSeconds: 3, Seed: seed)).Samples;

    private static float[] CleanCall() => Station(Call, 23, 625, 0.5, 0.04, 3, 490);

    /// <summary>The call with a 40 ms burst 8 dB under it in every gap between letters, at 550 to 675 Hz.</summary>
    private static float[] CallWithBlips()
    {
        var samples = CleanCall();
        var keyed = Station(Call, 23, 625, 0.5, 0, 3, 490);
        var pitches = new[] { 575.0, 650, 600, 675, 550 };
        var burst = (int)(0.040 * Rate);
        var quietRun = 0;
        var placed = 0;

        for (var i = 0; i < keyed.Length; i++)
        {
            if (Math.Abs(keyed[i]) > 1e-4)
            {
                if (quietRun >= (int)(0.150 * Rate) && i > (int)(3.2 * Rate))
                {
                    var middle = i - (quietRun / 2) - (burst / 2);
                    var hz = pitches[placed++ % pitches.Length];

                    for (var k = 0; k < burst; k++)
                    {
                        samples[middle + k] += (float)(0.2 * Math.Sin(Math.PI * k / burst) * Math.Sin(2 * Math.PI * hz * k / Rate));
                    }
                }

                quietRun = 0;
            }
            else
            {
                quietRun++;
            }
        }

        return samples;
    }

    private static float[] NoiseAlone() => CwSignal.Generate(new CwSignalRequest(
        " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: 0.3, LeadInSeconds: 15, TailSeconds: 15, Seed: 491)).Samples;

    private static float[] TwoStations()
    {
        var a = Station(Call, 23, 625, 0.5, 0.04, 3, 490);
        var b = Station("TEST DE W1AW K", 18, 825, 0.3, 0, 3.4, 491);
        var mixed = new float[Math.Max(a.Length, b.Length)];

        for (var i = 0; i < mixed.Length; i++)
        {
            mixed[i] = (i < a.Length ? a[i] : 0) + (i < b.Length ? b[i] : 0);
        }

        return mixed;
    }

    /// <summary>The terminal's letters and the scroll's, in time order, with no spaces; and the terminal's text as shown.</summary>
    internal static (string Terminal, string Scroll, string Shown) Surfaces(float[] samples)
    {
        var detector = new CwEnvelopeDetector(Rate);
        var decoder = new CwDecoder(Rate) { DetectorMarks = detector.MarksSince };
        var feed = new CwScopeFeed();
        var start = new DateTime(2026, 9, 29, 13, 35, 0, DateTimeKind.Utc);
        var terminal = new List<CwCharacter>();
        var drawn = new Dictionary<(string, DateTime), CwGraphLetter>();
        var frame = CwScopeFrame.Empty;

        DateTime Now() => start + decoder.Heard;

        decoder.CharacterSettled += c =>
        {
            terminal.Add(c);
            feed.Settle(c, decoder.Heard, Now());
        };

        void Tick()
        {
            frame = feed.Tick(detector, detector.Reading, decoder.RunsPrintingHz, frame, scopeQuiet: false, Now());

            foreach (var l in CwScopeControl.DrawnLetters(frame))
            {
                drawn[(l.Text, l.StartUtc)] = l;
            }
        }

        for (var at = 0; at + Chunk <= samples.Length; at += Chunk)
        {
            decoder.Process(new AudioChunk(at, Rate, samples.AsSpan(at, Chunk)));
            detector.Process(samples.AsSpan(at, Chunk));

            if (at / Chunk % 10 == 0)
            {
                Tick();
            }
        }

        decoder.Flush();
        Tick();

        static string Shown(string text, CwConfidence confidence)
            => confidence == CwConfidence.Unreadable ? MorseAlphabet.Unreadable : text;

        var letters = terminal.Where(c => !c.IsWordGap).ToList();

        return (
            string.Concat(letters.Select(c => Shown(c.Text, c.Confidence))),
            string.Concat(drawn.Values.OrderBy(l => l.StartUtc).Select(l => Shown(l.Text, l.Confidence))),
            string.Join(' ', string.Concat(terminal.Select(c => c.Text)).Split(' ', StringSplitOptions.RemoveEmptyEntries)));
    }

    private (string Terminal, string Scroll, string Shown) Case(string name, float[] samples)
    {
        var s = Surfaces(samples);

        _output.WriteLine($"{name}: terminal `{s.Shown}`; terminal letters `{s.Terminal}`; scroll letters `{s.Scroll}`");

        return s;
    }

    /// <remarks>Case 1: the clean call. Terminal and scroll identical, reading the call.</remarks>
    [Fact]
    public void TheCleanCall()
    {
        var s = Case("clean call", CleanCall());

        Assert.Equal(Call, s.Shown);
        Assert.Equal(s.Terminal, s.Scroll);
    }

    /// <remarks>Case 2: the call with bursts in every gap. Identical, reading the call whole.</remarks>
    [Fact]
    public void TheCallWithBursts()
    {
        var s = Case("call with bursts", CallWithBlips());

        Assert.Equal(Call, s.Shown);
        Assert.Equal(s.Terminal, s.Scroll);
    }

    /// <remarks>Case 3: loud noise, no station. The terminal is empty, and so is the scroll.</remarks>
    [Fact]
    public void LoudNoiseAlone()
    {
        var s = Case("noise alone", NoiseAlone());

        Assert.Equal(string.Empty, s.Terminal);
        Assert.Equal(s.Terminal, s.Scroll);
    }

    /// <remarks>Case 4: a lone dit, and a lone dah. The terminal is empty.</remarks>
    /// <param name="text">E for the dit, T for the dah.</param>
    [Theory]
    [InlineData("E")]
    [InlineData("T")]
    public void ALoneDitOrDah(string text)
    {
        var s = Case($"a lone {text}", Station(text, 23, 625, 0.5, 0.04, 3, 492));

        Assert.Equal(string.Empty, s.Terminal);
        Assert.Equal(s.Terminal, s.Scroll);
    }

    /// <remarks>Case 5: the call plus a second station 200 Hz away. Both surfaces read the printed station, identically.</remarks>
    [Fact]
    public void TwoStations200HzApart()
    {
        var s = Case("two stations", TwoStations());

        Assert.Equal(Call, s.Shown);
        Assert.Equal(s.Terminal, s.Scroll);
    }
}
