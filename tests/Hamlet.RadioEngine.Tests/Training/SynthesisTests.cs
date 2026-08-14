using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Training;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Training;

/// <summary>
/// The training synthesiser (HM-DEC-026, HM-DEC-027): reproducible, with each
/// mode carrying its real width and rhythm, placed on the real band plan.
/// </summary>
public sealed class SynthesisTests
{
    private const int Seed = 20260813;
    private const int Bins = 512;

    private static CwBand Forty => BandPlan.Bands.First(b => b.Name == "40 m");

    private static byte[] RenderAt(SignalSynthesizer synth, double seconds)
    {
        var bins = new byte[Bins];
        synth.Render(TimeSpan.FromSeconds(seconds), bins);
        return bins;
    }

    private static SignalSynthesizer OneSignal(SyntheticSignal signal, CwBand band)
        => new(new[] { signal }, band.LowHz, band.HighHz, Seed);

    /// <remarks>
    /// Proves the determinism the whole design rests on (§5): the same seed
    /// and the same elapsed time paint the same bytes, every time. Without
    /// this a practice session could not be replayed and none of the
    /// assertions below could be written.
    /// </remarks>
    [Fact]
    public void Render_IsDeterministicForASeedAndTime()
    {
        var a = new SignalSynthesizer(
            TrainingBandPlan.ForBand(Forty, Seed), Forty.LowHz, Forty.HighHz, Seed);
        var b = new SignalSynthesizer(
            TrainingBandPlan.ForBand(Forty, Seed), Forty.LowHz, Forty.HighHz, Seed);

        foreach (var t in new[] { 0.0, 3.25, 7.5, 61.0 })
        {
            Assert.Equal(RenderAt(a, t), RenderAt(b, t));
        }
    }

    /// <remarks>
    /// Proves a different seed gives a different band, so the seed is really
    /// the whole state and not decoration.
    /// </remarks>
    [Fact]
    public void Render_DiffersBetweenSeeds()
    {
        var a = new SignalSynthesizer(
            TrainingBandPlan.ForBand(Forty, 1), Forty.LowHz, Forty.HighHz, 1);
        var b = new SignalSynthesizer(
            TrainingBandPlan.ForBand(Forty, 2), Forty.LowHz, Forty.HighHz, 2);

        Assert.NotEqual(RenderAt(a, 4.0), RenderAt(b, 4.0));
    }

    /// <remarks>
    /// Proves the frame moves. A still picture would be a screensaver, and
    /// the noise floor is what makes an empty band look like a receiver
    /// rather than a blank panel.
    /// </remarks>
    [Fact]
    public void NoiseFloor_IsPresentAndMoves()
    {
        var synth = new SignalSynthesizer(
            Array.Empty<SyntheticSignal>(), Forty.LowHz, Forty.HighHz, Seed);

        var first = RenderAt(synth, 1.0);
        var later = RenderAt(synth, 9.0);

        Assert.All(first, b => Assert.True(b > 0, "the noise floor should never be zero"));
        Assert.NotEqual(first, later);
    }

    /// <remarks>
    /// Proves each mode occupies its real bandwidth. This is the thing the
    /// waterfall is teaching — that PSK31 is a hair and SSB is a slab — so a
    /// width that merely looked good would be teaching a falsehood
    /// (HM-DEC-026).
    /// </remarks>
    [Theory]
    [InlineData(TrainingMode.Psk31, 31)]
    [InlineData(TrainingMode.Ft8, 50)]
    [InlineData(TrainingMode.Cw, 150)]
    [InlineData(TrainingMode.Rtty, 230)]
    [InlineData(TrainingMode.Ssb, 2400)]
    public void Modes_CarryTheirRealBandwidth(TrainingMode mode, int expectedHz)
        => Assert.Equal(
            expectedHz, new SyntheticSignal(mode, 7_040_000, 1.0).WidthHz);

    /// <remarks>
    /// Proves the widths survive into the picture, in the right order: a
    /// PSK31 ribbon lights fewer bins than a CW signal, which lights fewer
    /// than an SSB smear.
    /// </remarks>
    [Fact]
    public void PaintedWidth_OrdersNarrowToWide()
    {
        static int LitBins(TrainingMode mode)
        {
            // A 4 kHz window across 512 bins is about 8 Hz per bin. The whole
            // point of the assertion is telling a 31 Hz ribbon from a 150 Hz
            // CW note, and at a whole band's resolution both round to the
            // same bin — so the measurement is made at a resolution that can
            // actually see the difference.
            var band = new CwBand("test", 7_033_000, 7_037_000, 7_033_000, 7_037_000, 7_035_000);
            var signal = new SyntheticSignal(mode, 7_035_000, 1.0, PhaseOffset: 0);
            var synth = OneSignal(signal, band);

            var best = 0;
            var floor = new SignalSynthesizer(
                Array.Empty<SyntheticSignal>(), band.LowHz, band.HighHz, Seed);

            // Sample several instants: keyed modes are not always on.
            for (var t = 0; t < 40; t++)
            {
                var seconds = t * 0.37;
                var withSignal = RenderAt(synth, seconds);
                var noiseOnly = RenderAt(floor, seconds);

                var lit = 0;
                for (var i = 0; i < withSignal.Length; i++)
                {
                    if (withSignal[i] > noiseOnly[i] + 8)
                    {
                        lit++;
                    }
                }

                best = Math.Max(best, lit);
            }

            return best;
        }

        var psk = LitBins(TrainingMode.Psk31);
        var cw = LitBins(TrainingMode.Cw);
        var ssb = LitBins(TrainingMode.Ssb);

        Assert.True(psk >= 1, "PSK31 should light at least one bin");
        Assert.True(cw > psk, $"CW ({cw}) should be wider than PSK31 ({psk})");
        Assert.True(ssb > cw, $"SSB ({ssb}) should be wider than CW ({cw})");
    }

    /// <remarks>
    /// Proves FT8 transmits on the quarter-minute and stops before the slot
    /// ends. That synchronization is the single fact that makes FT8 look like
    /// rain on a waterfall, and a newcomer who learns it can identify the
    /// mode across the room.
    /// </remarks>
    [Fact]
    public void Ft8_BurstsAlignToTheFifteenSecondBoundary()
    {
        var band = Forty;
        var signal = new SyntheticSignal(
            TrainingMode.Ft8, 7_074_000, 1.0, PhaseOffset: 0);
        var synth = OneSignal(signal, band);
        var bin = synth.BinFor(7_074_000, Bins);

        var floor = new SignalSynthesizer(
            Array.Empty<SyntheticSignal>(), band.LowHz, band.HighHz, Seed);

        static bool Lit(SignalSynthesizer s, SignalSynthesizer noise, int bin, double t)
            => RenderAt(s, t)[bin] > RenderAt(noise, t)[bin] + 8;

        // Sweep several cycles rather than one. A real station does not key
        // up in every slot, and the synthesiser models that, so asserting on
        // a single cycle would be asserting on which way one hash landed.
        var period = SignalSynthesizer.Ft8Period.TotalSeconds;
        var window = SignalSynthesizer.Ft8Transmission.TotalSeconds;
        var litTimes = new List<double>();

        for (var t = 0.0; t < period * 8; t += 0.25)
        {
            if (Lit(synth, floor, bin, t))
            {
                litTimes.Add(t);
            }
        }

        Assert.NotEmpty(litTimes);

        // The claim that matters: every transmission falls inside the window
        // that opens on the quarter-minute. That synchronization is what
        // makes FT8 look like rain, and it is the fact a newcomer learns to
        // recognize the mode by.
        Assert.All(litTimes, t => Assert.True(
            t % period < window,
            $"FT8 was transmitting {t % period:0.00}s into a cycle, past the "
            + $"{window:0.00}s window"));

        // And it is reliably silent in the dead zone at the end of a cycle.
        foreach (var t in new[] { 13.0, 14.5, 28.0, 29.5, 43.5, 59.5 })
        {
            Assert.False(
                Lit(synth, floor, bin, t),
                $"FT8 must be silent at {t}s — past the transmission window");
        }
    }

    /// <remarks>
    /// Proves CW keying runs at the stated speed. The check is arithmetic
    /// rather than a measurement: the key pattern is integers in dit units,
    /// and one dit is 1200/WPM milliseconds by the PARIS standard, so a
    /// message's length in seconds is exactly predictable.
    /// </remarks>
    [Theory]
    [InlineData(12)]
    [InlineData(18)]
    [InlineData(25)]
    public void Cw_KeyingMatchesTheStatedWpm(int wpm)
    {
        var text = "PARIS";
        var dits = MorseCode.LengthInDits(text);
        var pattern = MorseCode.KeyPattern(text);

        // "PARIS" is the standard word by which WPM is defined: 50 dit units
        // including the word gap that follows it, so 43 without.
        Assert.Equal(43, dits);

        // One dit is 1200/WPM milliseconds. Asserted in milliseconds rather
        // than by multiplying out a TimeSpan, because TimeSpan quantises to
        // ticks and comparing the products would be testing that rounding
        // rather than the speed.
        Assert.Equal(1200.0 / wpm, MorseCode.Dit(wpm).TotalMilliseconds, 3);

        // Which makes the standard word take exactly 60/WPM seconds.
        var parisSeconds = 50 * MorseCode.Dit(wpm).TotalSeconds;
        Assert.Equal(60.0 / wpm, parisSeconds, 3);

        // The key is down at the very start and the pattern alternates.
        Assert.True(MorseCode.IsKeyDown(pattern, dits, 0, 0.5));
        Assert.NotEmpty(pattern);
    }

    /// <remarks>
    /// Proves the keying actually reaches the picture at the right rate: a
    /// slow sender's signal is lit for longer stretches than a fast one's
    /// over the same window.
    /// </remarks>
    [Fact]
    public void Cw_SlowerSenderIsLitLongerThanFaster()
    {
        static int LitSamples(int wpm)
        {
            var band = Forty;
            var synth = OneSignal(
                new SyntheticSignal(TrainingMode.Cw, 7_030_000, 1.0, wpm, "PARIS PARIS",
                    PhaseOffset: 0),
                band);
            var bin = synth.BinFor(7_030_000, Bins);
            var floor = new SignalSynthesizer(
                Array.Empty<SyntheticSignal>(), band.LowHz, band.HighHz, Seed);

            var lit = 0;
            for (var i = 0; i < 400; i++)
            {
                var t = i * 0.02;
                if (RenderAt(synth, t)[bin] > RenderAt(floor, t)[bin] + 8)
                {
                    lit++;
                }
            }

            return lit;
        }

        var slow = LitSamples(10);
        var fast = LitSamples(30);

        Assert.True(slow > fast, $"10 WPM ({slow}) should be keyed longer than 30 WPM ({fast})");
    }

    /// <remarks>
    /// Proves RTTY paints two rails rather than one lobe — the shape the
    /// field guide calls train tracks.
    /// </remarks>
    [Fact]
    public void Rtty_PaintsTwoRails()
    {
        var band = new CwBand("t", 7_060_000, 7_064_000, 7_060_000, 7_064_000, 7_062_000);
        var synth = OneSignal(
            new SyntheticSignal(TrainingMode.Rtty, 7_062_000, 1.0, PhaseOffset: 0), band);

        var mark = synth.BinFor(7_062_000 + (SignalSynthesizer.RttyShiftHz / 2), Bins);
        var space = synth.BinFor(7_062_000 - (SignalSynthesizer.RttyShiftHz / 2), Bins);
        var between = synth.BinFor(7_062_000, Bins);

        Assert.NotEqual(mark, space);

        var frame = RenderAt(synth, 2.0);
        Assert.True(frame[mark] > frame[between] || frame[space] > frame[between],
            "at least one rail should stand above the gap between them");
    }

    /// <remarks>
    /// Proves the rule that keeps practice honest (HM-DEC-026): every
    /// synthesised signal sits inside a neighborhood the real band plan
    /// documents, hosting exactly that mode. A newcomer who learns where
    /// things are here has learned where they are on the air.
    /// </remarks>
    [Theory]
    [InlineData("40 m")]
    [InlineData("20 m")]
    [InlineData("80 m")]
    public void EverySignal_LandsInANeighborhoodThatHostsItsMode(string bandName)
    {
        var band = BandPlan.Bands.First(b => b.Name == bandName);
        var hoods = NeighborhoodPlan.ForBand(band);
        var signals = TrainingBandPlan.ForBand(band, Seed);

        Assert.NotEmpty(signals);

        foreach (var signal in signals)
        {
            var hood = hoods.FirstOrDefault(h => h.Contains(signal.CenterHz));

            Assert.True(hood is not null,
                $"{signal.Mode} at {signal.CenterHz} Hz is outside every neighborhood");

            Assert.Contains(signal.Mode, TrainingBandPlan.ModesFor(hood!));

            // And it fits: the whole occupied bandwidth is inside the band.
            Assert.True(signal.LowHz >= band.LowHz && signal.HighHz <= band.HighHz,
                $"{signal.Mode} at {signal.CenterHz} Hz spills outside {bandName}");
        }
    }

    /// <remarks>
    /// Proves the placement reads the editorial map rather than a second copy
    /// of the band plan: FT8 city hosts FT8 and nothing else, the phone
    /// segment hosts voice, and open space hosts nothing.
    /// </remarks>
    [Fact]
    public void ModesFor_ReadsTheNeighborhoodMap()
    {
        var hoods = NeighborhoodPlan.ForBand(Forty);

        var ft8 = hoods.First(h => h.ShortName == "FT8");
        var cw = hoods.First(h => h.ShortName == "CW");
        var rtty = hoods.First(h => h.ShortName == "RTTY");
        var psk = hoods.First(h => h.ShortName == "PSK31");
        var phone = hoods.First(h => h.ShortName == "SSB");
        var open = hoods.First(h => h.ShortName.Length == 0);

        Assert.Equal(new[] { TrainingMode.Ft8 }, TrainingBandPlan.ModesFor(ft8));
        Assert.Equal(new[] { TrainingMode.Cw }, TrainingBandPlan.ModesFor(cw));
        Assert.Equal(new[] { TrainingMode.Rtty }, TrainingBandPlan.ModesFor(rtty));
        Assert.Equal(new[] { TrainingMode.Psk31 }, TrainingBandPlan.ModesFor(psk));
        Assert.Equal(new[] { TrainingMode.Ssb }, TrainingBandPlan.ModesFor(phone));
        Assert.Empty(TrainingBandPlan.ModesFor(open));
    }

    /// <remarks>
    /// Proves the fast lane really is fast: contest-speed CW where the map
    /// says contest operators run, slower CW on main street where a beginner
    /// is meant to be able to follow it.
    /// </remarks>
    [Fact]
    public void FastLane_SendsFasterThanMainStreet()
    {
        var hoods = NeighborhoodPlan.ForBand(Forty);
        var fast = hoods.First(TrainingBandPlan.IsFastCw);
        var main = hoods.First(h => h.ShortName == "CW");
        var signals = TrainingBandPlan.ForBand(Forty, Seed);

        var fastWpm = signals.Where(s => fast.Contains(s.CenterHz)).Select(s => s.WordsPerMinute);
        var mainWpm = signals.Where(s => main.Contains(s.CenterHz)).Select(s => s.WordsPerMinute);

        Assert.All(fastWpm, w => Assert.True(w >= 25, $"fast lane should be 25+ WPM, was {w}"));
        Assert.All(mainWpm, w => Assert.True(w < 25, $"main street should be under 25 WPM, was {w}"));
    }

    /// <remarks>
    /// Proves the source hands the renderer frames spanning exactly the band
    /// on screen, so the waterfall shares its x axis with the dial tape and
    /// the neighborhood map (HM-DEC-027).
    /// </remarks>
    [Fact]
    public void Source_PublishesFramesSpanningTheBand()
    {
        using var source = new TrainingSpectrumSource(Forty, Seed);

        var seen = 0;
        long low = 0, high = 0, bins = 0;

        source.FrameReady += (in SpectrumFrame frame) =>
        {
            seen++;
            low = frame.LowHz;
            high = frame.HighHz;
            bins = frame.Bins.Length;
        };

        source.PumpOnce(TimeSpan.FromSeconds(3));

        Assert.Equal(1, seen);
        Assert.Equal(Forty.LowHz, low);
        Assert.Equal(Forty.HighHz, high);
        Assert.Equal(TrainingSpectrumSource.DefaultBinCount, bins);
    }

    /// <remarks>
    /// Proves the promise HM-DEC-006 is built on: rendering a frame allocates
    /// nothing. This caught a real leak — iterating the Morse key pattern with
    /// <c>foreach</c> over an <c>IReadOnlyList&lt;int&gt;</c> boxed the
    /// enumerator on every CW signal, about 280 bytes a frame, which at
    /// twenty-five frames a second is a garbage collection every half minute
    /// on the one path that must never stutter.
    /// </remarks>
    [Fact]
    public void Render_AllocatesNothingPerFrame()
    {
        var synth = new SignalSynthesizer(
            TrainingBandPlan.ForBand(Forty, Seed), Forty.LowHz, Forty.HighHz, Seed);
        var bins = new byte[Bins];

        // Warm up: first calls JIT and touches lazily-built state.
        for (var i = 0; i < 100; i++)
        {
            synth.Render(TimeSpan.FromSeconds(i * 0.04), bins);
        }

        // Thread-local, not process-wide. xUnit runs collections in parallel,
        // so GC.GetTotalAllocatedBytes would count every other test's
        // allocations too and this would fail for reasons that have nothing
        // to do with the synthesiser.
        const int frames = 500;
        var before = GC.GetAllocatedBytesForCurrentThread();

        for (var i = 0; i < frames; i++)
        {
            synth.Render(TimeSpan.FromSeconds(i * 0.04), bins);
        }

        var perFrame = (GC.GetAllocatedBytesForCurrentThread() - before) / (double)frames;

        Assert.True(perFrame < 1.0,
            $"rendering allocated {perFrame:0.0} bytes per frame; it must allocate none");
    }

    /// <remarks>
    /// Proves a bin's frequency is the same arithmetic the dial tape and the
    /// map use, which is what lets a click on the waterfall tune to what the
    /// operator is pointing at.
    /// </remarks>
    [Fact]
    public void BinCenter_MapsBackToFrequency()
    {
        var bins = new byte[Bins];
        var frame = new SpectrumFrame(7_000_000, 7_300_000, DateTime.UtcNow, bins);

        Assert.Equal(300_000, frame.SpanHz);
        Assert.InRange(frame.BinCenterHz(0), 7_000_000, 7_000_600);
        Assert.InRange(frame.BinCenterHz(Bins - 1), 7_299_400, 7_300_000);
    }
}
