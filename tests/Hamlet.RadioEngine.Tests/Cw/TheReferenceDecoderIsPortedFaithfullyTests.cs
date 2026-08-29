using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The ported reference behaves as `cwdecoder.py` behaves, and refuses where it
/// refuses.
/// </summary>
/// <remarks>
/// <para>**THESE PIN THE REFERENCE'S OWN BEHAVIOUR RATHER THAN NEW JUDGEMENT**
/// (Tim's ruling of 2026-08-28). A port is only a port if it agrees with its
/// source, so every figure below was read off `cwdecoder.py` running on the same
/// audio rather than decided here.</para>
/// <para>**MEASURED ACROSS THE WHOLE CORPUS WHEN THE PORT LANDED**: on all
/// forty-four captures the two agree on the acquired pitch, on whether a clock
/// fits, on the gate's contrast, on the character count and on the transcript
/// character for character. The only differences anywhere were the fitted dah on
/// `cw-2026-08-22-032012` and the fitted dit on `cw-2026-08-28-005218`, each by
/// under a millisecond, which is floating-point accumulation order rather than a
/// difference in behaviour.</para>
/// </remarks>
public sealed class TheReferenceDecoderIsPortedFaithfullyTests
{
    private readonly ITestOutputHelper _output;

    public TheReferenceDecoderIsPortedFaithfullyTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Acquisition finds the net on the operator's own capture.</summary>
    /// <remarks>
    /// **THE WORK ORDER ASKED FOR 429 HZ AND THAT NUMBER CANNOT COME OUT OF
    /// `acquire_tone`.** Its grid is 300 to 900 in steps of 25, so its answer is
    /// always a multiple of 25, and on this capture it is **425**. The 430 the
    /// reference prints is a different quantity: the median of the fine
    /// tracker's seven offsets over the loud hops, which lands on 430. Both are
    /// pinned here because the two were being confused.
    /// </remarks>
    [Fact]
    public void AcquisitionFindsTheNetAndTheTrackerRefinesIt()
    {
        var audio = Capture("cw-2026-08-28-004844");
        var x = AsDoubles(audio);
        var mask = CwReferenceDecoder.MuteMask(x, audio.SampleRate);
        var acquired = CwReferenceDecoder.AcquireTone(x, audio.SampleRate, mask);

        _output.WriteLine($"acquire_tone -> {acquired}");

        Assert.Equal(425, acquired!.Value, 9);

        var read = CwReferenceDecoder.Run(audio.Samples, audio.SampleRate);

        _output.WriteLine(
            $"run -> tone {read.ToneHz:0} Hz, dit {read.DitMilliseconds:0} ms, "
            + $"dah {read.DahMilliseconds:0} ms, {read.WordsPerMinute:0.0} WPM");

        Assert.Equal(430, read.ToneHz, 0);
    }

    /// <summary>The net reads, and it reads what the reference reads.</summary>
    /// <remarks>
    /// Taken from `cwdecoder.py` run on this file, not composed here. The
    /// operator has confirmed the net: `TUES AUG 25`, `BRUCE`, `NR 230`.
    /// </remarks>
    [Fact]
    public void TheNetReadsExactlyAsTheReferenceReadsIt()
    {
        var audio = Capture("cw-2026-08-28-004844");
        var read = CwReferenceDecoder.Run(audio.Samples, audio.SampleRate);

        _output.WriteLine(read.Text);

        Assert.Null(read.Refusal);
        Assert.Equal(57, read.DitMilliseconds, 0);
        Assert.Equal(164, read.DahMilliseconds, 0);
        Assert.Equal(21, read.ContrastDb, 0);

        Assert.Equal(
            "K I L O T U E S A U G 2 5 K C 9 U C Q R ET 8 8 <BT> B R U C E <AR> "
            + "N R 2 3 0 C",
            read.Text);
    }

    /// <summary>
    /// No clock fits noise, and that is where the refusal lives.
    /// </summary>
    /// <remarks>
    /// **THE REFUSAL IS STRUCTURAL AND NOT A THRESHOLD ANYBODY CHOSE**
    /// (HM-DEC-120). Six units were spent looking for a number that separates a
    /// station from an empty band; `fit_clock` never asks the question. Marks
    /// that do not form two lengths produce no clock, and nothing downstream runs
    /// without one.
    /// </remarks>
    [Fact]
    public void NoClockFitsNoise()
    {
        var random = new Random(20260828);
        var marks = new double[60];

        for (var i = 0; i < marks.Length; i++)
        {
            // A continuum rather than two lengths, which is what a gate
            // chattering on noise produces.
            marks[i] = 40 + (random.NextDouble() * 160);
        }

        Assert.Null(CwReferenceDecoder.FitClock(marks));
    }

    /// <summary>Too few marks is not a clock either.</summary>
    [Fact]
    public void EightMarksAreNeededBeforeAClockIsFittedAtAll()
    {
        double[] few = [60, 180, 60, 180, 60, 180, 60];

        Assert.Null(CwReferenceDecoder.FitClock(few));
    }

    /// <summary>A textbook fist fits, and a speed outside four to forty does not.</summary>
    /// <remarks>
    /// The dit sanity range stays because 4 to 40 words a minute is a fact about
    /// people rather than an assumption about their spacing.
    /// </remarks>
    [Fact]
    public void ATextbookFistFitsAndAnImpossibleSpeedDoesNot()
    {
        var textbook = Fist(60, 180);
        var fitted = CwReferenceDecoder.FitClock(textbook);

        Assert.NotNull(fitted);
        Assert.Equal(60, fitted!.Value.Dit, 0);

        // A dit of 10 ms is 120 words a minute; nobody sends that.
        Assert.Null(CwReferenceDecoder.FitClock(Fist(10, 30)));
    }

    /// <summary>
    /// The heavy fist the ratio band refuses is admitted by the scatter test.
    /// </summary>
    /// <remarks>
    /// <para>**THE RATIO BAND WAS 2.5 TO 3.8 AND IT REFUSED A REAL STATION.**
    /// `cw-2026-08-17-134712` holds a fist sending a dah of 4.24 dits,
    /// adjudicated as HM-DEC-144. A judge that cannot read a fist the radio has
    /// heard is not independent, it is wrong.</para>
    /// <para>**AND `well_separated` IS A WIDENING THAT CAN ONLY ACCEPT.** It was
    /// tried first as a replacement for the band and measured: at five decibels
    /// the marks scatter enough that fast-working fell from 58 % to nothing. The
    /// scatter test is right about shape and wrong about noise, the band is the
    /// other way round, so the reference keeps both — and so does this port.</para>
    /// </remarks>
    [Fact]
    public void AHeavyFistIsAdmittedByScatterWhereTheRatioBandRefusesIt()
    {
        // 4.24 dits to the dah, which is HM-DEC-144's measured fist.
        var heavy = Fist(56, 238);
        var (c1, c2) = CwReferenceDecoder.TwoMeans(heavy);

        _output.WriteLine($"ratio {c2 / c1:0.00}, outside the 2.5-3.8 band");

        Assert.True(c2 / c1 > 3.8, "this fist is supposed to be outside the band");
        Assert.True(
            CwReferenceDecoder.WellSeparated(heavy, c1, c2),
            "the scatter test refused a fist whose two lengths are clean");

        Assert.NotNull(CwReferenceDecoder.FitClock(heavy));

        // **AND THE WIDENING IS ACQUISITION-ONLY.** On the slow-fist re-read it
        // must not replace a working clock, which is what took farnsworth-light
        // from 100 % to 73 %.
        Assert.Null(CwReferenceDecoder.FitClock(heavy, acquiring: false));
    }

    /// <summary>The gate refuses a window with less than six decibels of contrast.</summary>
    /// <remarks>
    /// **THIS IS THE STRUCTURAL REFUSAL'S FIRST HALF**, before any clock is
    /// fitted: a window whose two centres are closer than six decibels gets no
    /// threshold at all, so the key stays up right through it.
    /// </remarks>
    [Fact]
    public void TheGateRefusesAWindowWithTooLittleContrast()
    {
        const int Hops = 600;

        var t = new double[Hops];
        var flat = new double[Hops];
        var loud = new double[Hops];
        var active = new bool[Hops];

        for (var i = 0; i < Hops; i++)
        {
            t[i] = i * 0.010;
            active[i] = true;

            // Three decibels of swing: real structure, and not enough of it.
            flat[i] = i % 20 < 10 ? -60 : -57;

            // Twenty decibels of the same structure.
            loud[i] = i % 20 < 10 ? -60 : -40;
        }

        var (quietKey, quietContrast) = CwReferenceDecoder.Gate(t, flat, active);
        var (loudKey, loudContrast) = CwReferenceDecoder.Gate(t, loud, active);

        _output.WriteLine(
            $"3 dB swing: {quietKey.Count(k => k)} hops keyed, "
            + $"contrast {quietContrast.Max():0.0}");
        _output.WriteLine(
            $"20 dB swing: {loudKey.Count(k => k)} hops keyed, "
            + $"contrast {loudContrast.Max():0.0}");

        Assert.All(quietKey, k => Assert.False(k));
        Assert.Contains(loudKey, k => k);
    }

    /// <summary>
    /// What the reference does on the two captures that hold nothing, measured
    /// rather than hoped for.
    /// </summary>
    /// <remarks>
    /// <para>**ONE OF THE TWO SILENCE CONTROLS IS NOT SILENT UNDER THE
    /// REFERENCE, AND THAT IS THE REFERENCE'S BEHAVIOUR RATHER THAN A PORTING
    /// BUG.** `cwdecoder.py` and this port agree on it character for character.
    /// On `cw-2026-08-20-014935` a tone is found at 590 Hz and no clock fits, so
    /// it emits nothing, which is exactly the structural refusal the port was
    /// made for. On `cw-2026-08-20-014854` a clock **does** fit and eighteen
    /// characters come out of a capture the suite has always called HOLDS
    /// NOTHING: `■ ■■I M YOY■KB A NB ■A IM`.</para>
    /// <para>**IT IS PINNED HERE BECAUSE IT DECIDES WHETHER THE SETTING MAY
    /// DEFAULT ON** (HM-DEC-120, §0.0). A decoder that puts letters on a dead
    /// frequency is the one thing the operator asked to stop, so this goes red
    /// the moment either capture's behaviour moves, in either direction — the
    /// good change and the bad change both deserve a look.</para>
    /// </remarks>
    [Theory]
    [InlineData("cw-2026-08-20-014935", 0)]
    [InlineData("cw-2026-08-20-014854", 18)]
    public void TheSilenceControlsBehaveAsMeasured(string name, int expected)
    {
        var audio = Capture(name);
        var read = CwReferenceDecoder.Run(audio.Samples, audio.SampleRate);
        var letters = read.Characters.Count(c => c.Text != MorseAlphabet.WordGap);

        _output.WriteLine(
            $"{name}: {letters} characters, refusal {read.Refusal}, "
            + $"text {read.Text}");

        Assert.Equal(expected, letters);
    }

    /// <summary>Digital silence is refused rather than read.</summary>
    [Fact]
    public void AnAllZeroBufferIsRefused()
    {
        var read = CwReferenceDecoder.Run(new float[48_000 * 5], 48_000);

        Assert.NotNull(read.Refusal);
        Assert.Empty(read.Characters);
    }

    /// <summary>A fist of alternating dits and dahs, with a little jitter.</summary>
    private static double[] Fist(double dit, double dah)
    {
        var random = new Random(20260828);
        var marks = new double[40];

        for (var i = 0; i < marks.Length; i++)
        {
            var length = i % 3 == 2 ? dah : dit;

            marks[i] = length * (1 + ((random.NextDouble() - 0.5) * 0.04));
        }

        return marks;
    }

    private static MonoAudio Capture(string name)
    {
        var direct = Path.Combine(CapturedSignalTests.Folder, name + ".wav");

        return WavAudio.Read(File.Exists(direct)
            ? direct
            : Path.Combine(
                CapturedSignalTests.Folder, "unadjudicated", name + ".wav"));
    }

    private static double[] AsDoubles(MonoAudio audio)
    {
        var x = new double[audio.Samples.Length];

        for (var i = 0; i < x.Length; i++)
        {
            x[i] = audio.Samples[i];
        }

        return x;
    }
}
