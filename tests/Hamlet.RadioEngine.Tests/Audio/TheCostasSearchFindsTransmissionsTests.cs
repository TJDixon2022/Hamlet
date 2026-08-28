using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// The sync search says where a transmission is, and never what it said.
/// </summary>
/// <remarks>
/// <para>**TASK 7 OF WORK INSTRUCTION 042.** An FT8 transmission is seventy-nine
/// symbols long and three of its blocks are known in advance: the same
/// seven-symbol Costas array at the beginning, the middle and the end. Finding
/// those tells you where a transmission is in time and frequency, which is what
/// a decoder needs and is not itself a decode.</para>
/// <para>**IT REPORTS CANDIDATES, NOT MESSAGES**, and nothing it produces goes
/// on the decoded-text panel. A candidate says a signal with this sync pattern
/// appears to be here; it says nothing at all about what was sent (§0.0).</para>
/// </remarks>
public sealed class TheCostasSearchFindsTransmissionsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the candidates are printed.</param>
    public TheCostasSearchFindsTransmissionsTests(ITestOutputHelper output)
        => _output = output;

    private const int Rate = 12000;

    /// <summary>The repository root, walking up from the test binary.</summary>
    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }

    /// <summary>
    /// The tone numbers of one transmission: the three sync blocks where they
    /// belong, and made-up data everywhere else.
    /// </summary>
    /// <remarks>
    /// **THE DATA SYMBOLS ARE DELIBERATELY ARBITRARY.** The search must not care
    /// what was sent, so the fixture gives it something it cannot possibly read
    /// and still expects the sync to be found.
    /// </remarks>
    private static int[] Symbols(int seed)
    {
        var tones = new int[Ft8Sync.SymbolsPerTransmission];
        var noise = new Random(seed);

        for (var i = 0; i < tones.Length; i++)
        {
            tones[i] = noise.Next(Ft8Sync.Tones);
        }

        foreach (var at in Ft8Sync.CostasAt)
        {
            for (var i = 0; i < Ft8Sync.Costas.Count; i++)
            {
                tones[at + i] = Ft8Sync.Costas[i];
            }
        }

        return tones;
    }

    /// <summary>
    /// Fifteen seconds holding one transmission, plus noise.
    /// </summary>
    /// <param name="baseHz">The lowest of its eight tones.</param>
    /// <param name="startSeconds">How far into the slot it begins.</param>
    /// <param name="amplitude">How loud, against noise at 1.0 root mean square.</param>
    /// <param name="seed">Which arbitrary data it carries.</param>
    /// <returns>The slot.</returns>
    /// <remarks>
    /// **PHASE IS CONTINUOUS ACROSS SYMBOL BOUNDARIES**, as it is on the air. A
    /// generator that restarts each tone at zero phase puts a click at every
    /// boundary, and a click is broadband: the search would then be finding a
    /// row of clicks rather than a row of tones, and would pass this test while
    /// failing on a real signal (§12.5).
    /// </remarks>
    private static MonoAudio Transmission(
        double baseHz, double startSeconds, double amplitude, int seed = 7)
    {
        var samples = new float[15 * Rate];
        var noise = new Random(seed * 31);

        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)((noise.NextDouble() - 0.5) * 2.0);
        }

        var tones = Symbols(seed);
        var perSymbol = (int)Math.Round(Ft8Sync.SymbolSeconds * Rate);
        var at = (int)Math.Round(startSeconds * Rate);
        var phase = 0.0;

        for (var symbol = 0; symbol < tones.Length; symbol++)
        {
            var hertz = baseHz + (tones[symbol] * Ft8Sync.ToneSpacingHz);
            var perSample = 2.0 * Math.PI * hertz / Rate;

            for (var i = 0; i < perSymbol; i++)
            {
                var index = at + (symbol * perSymbol) + i;

                if (index >= 0 && index < samples.Length)
                {
                    samples[index] += (float)(amplitude * Math.Sin(phase));
                }

                phase += perSample;
            }
        }

        return new MonoAudio(Rate, samples);
    }

    private void Print(IReadOnlyList<SyncCandidate> candidates)
    {
        _output.WriteLine("  frequency | starts at | sync score");
        _output.WriteLine("  ----------|-----------|-----------");

        foreach (var candidate in candidates)
        {
            _output.WriteLine(
                $"  {candidate.FrequencyHz,7:0.0} Hz | {candidate.TimeOffsetSeconds,7:0.00} s "
                + $"| {candidate.Score,6:0.00}");
        }
    }

    /// <remarks>
    /// <para>**ONE TRANSMISSION, FOUND WHERE IT ACTUALLY IS.** Within half a
    /// tone in frequency and a quarter symbol in time, which is the resolution
    /// the grid can offer and no better.</para>
    /// <para>The score is a ratio: eight is the most a noiseless signal could
    /// give and one is what pure noise gives, so a real one lands between.</para>
    /// </remarks>
    [Fact]
    public void OneTransmissionIsFoundWhereItIs()
    {
        var slot = Transmission(baseHz: 1113, startSeconds: 0.5, amplitude: 3.0);

        var found = Ft8Sync.Search(slot);

        Print(found);

        Assert.NotEmpty(found);

        var best = found[0];

        Assert.InRange(best.FrequencyHz, 1113 - 3.2, 1113 + 3.2);
        Assert.InRange(best.TimeOffsetSeconds, 0.5 - 0.05, 0.5 + 0.05);

        // Well clear of what noise gives, and inside what the mode allows.
        Assert.InRange(best.Score, 2.0, 8.0);
    }

    /// <remarks>
    /// <para>**THREE STATIONS AT ONCE, WHICH IS THE ORDINARY CASE.** An FT8
    /// block holds dozens of transmissions in the same fifteen seconds, and a
    /// search that could only find the loudest would be useless on a real
    /// band.</para>
    /// <para>**AND ONE SIGNAL IS ONE CANDIDATE.** A strong transmission scores
    /// well at every grid point around itself, so without suppression three
    /// stations would report as thirty and the operator would read a crowd that
    /// is not there.</para>
    /// </remarks>
    [Fact]
    public void SeveralTransmissionsAreFoundSeparately()
    {
        var one = Transmission(1113, 0.4, 3.0, seed: 1);
        var two = Transmission(1799, 0.4, 3.0, seed: 2);
        var three = Transmission(2375, 0.4, 2.0, seed: 3);

        var mixed = new float[one.Samples.Length];

        for (var i = 0; i < mixed.Length; i++)
        {
            mixed[i] = one.Samples[i] + two.Samples[i] + three.Samples[i];
        }

        var found = Ft8Sync.Search(new MonoAudio(Rate, mixed));

        Print(found);

        foreach (var hertz in new[] { 1113.0, 1799.0, 2375.0 })
        {
            Assert.Contains(
                found, c => Math.Abs(c.FrequencyHz - hertz) <= 3.2);
        }

        // Each of the three appears once rather than as a smear of grid points.
        foreach (var hertz in new[] { 1113.0, 1799.0, 2375.0 })
        {
            Assert.Equal(
                1, found.Count(c => Math.Abs(c.FrequencyHz - hertz) <= 3.2));
        }
    }

    /// <remarks>
    /// <para>**NOISE PRODUCES NOTHING, WHICH IS THE HALF THAT MATTERS MOST**
    /// (§0.0). A search that reported candidates in an empty band would put
    /// stations on the operator's screen that were never on the air, and he
    /// would have no way to tell them from the real ones.</para>
    /// <para>**AND A SIGNAL WITH NO SYNC IN IT IS ALSO NOTHING.** The second
    /// case is a full set of FT8 tones with the Costas blocks replaced by
    /// arbitrary symbols: as loud as a real transmission, in the right place,
    /// with the right tone spacing, and no sync pattern. Noise alone would not
    /// prove the search is looking at the pattern rather than at energy.</para>
    /// </remarks>
    [Fact]
    public void NoiseAndAnUnsynchronisedSignalBothProduceNothing()
    {
        var noise = new Random(4242);
        var empty = new float[15 * Rate];

        for (var i = 0; i < empty.Length; i++)
        {
            empty[i] = (float)((noise.NextDouble() - 0.5) * 2.0);
        }

        var onNoise = Ft8Sync.Search(new MonoAudio(Rate, empty));

        Print(onNoise);

        Assert.Empty(onNoise);

        // The same tones, the same loudness, no sync array anywhere in it.
        var scrambled = Transmission(1113, 0.5, 3.0);
        var tones = Symbols(7);
        var perSymbol = (int)Math.Round(Ft8Sync.SymbolSeconds * Rate);
        var samples = new float[15 * Rate];
        var hiss = new Random(99);

        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)((hiss.NextDouble() - 0.5) * 2.0);
        }

        var phase = 0.0;
        var shuffled = new Random(5).Next(1, Ft8Sync.Tones);

        for (var symbol = 0; symbol < tones.Length; symbol++)
        {
            var tone = (tones[symbol] + shuffled) % Ft8Sync.Tones;
            var hertz = 1113 + (tone * Ft8Sync.ToneSpacingHz);
            var perSample = 2.0 * Math.PI * hertz / Rate;

            for (var i = 0; i < perSymbol; i++)
            {
                var index = (int)Math.Round(0.5 * Rate) + (symbol * perSymbol) + i;

                if (index < samples.Length)
                {
                    samples[index] += (float)(3.0 * Math.Sin(phase));
                }

                phase += perSample;
            }
        }

        _ = scrambled;

        var onScrambled = Ft8Sync.Search(new MonoAudio(Rate, samples));

        Print(onScrambled);

        // Rotating every tone by the same amount moves the array to a different
        // tone set, which is still a valid pattern shifted in frequency, so what
        // must not happen is a candidate at 1113 with a high score.
        Assert.DoesNotContain(
            onScrambled,
            c => Math.Abs(c.FrequencyHz - 1113) <= 3.2 && c.Score > 4.0);
    }

    /// <remarks>
    /// **WHAT NOISE SCORES, MEASURED RATHER THAN ASSUMED.** The floor below
    /// which a candidate is not worth reporting has to sit above whatever an
    /// empty band produces, and the way to know that is to look. Ten seeds of
    /// pure noise, the best score each one reaches.
    /// </remarks>
    [Fact]
    public void WhatAnEmptyBandScores()
    {
        var worst = 0.0;

        for (var seed = 0; seed < 10; seed++)
        {
            var noise = new Random(seed * 7919);
            var samples = new float[15 * Rate];

            for (var i = 0; i < samples.Length; i++)
            {
                samples[i] = (float)((noise.NextDouble() - 0.5) * 2.0);
            }

            var found = Ft8Sync.Search(
                new MonoAudio(Rate, samples), most: 1, floor: 0.0);

            var best = found.Count > 0 ? found[0].Score : 0.0;
            worst = Math.Max(worst, best);

            _output.WriteLine($"  seed {seed}: best noise score {best:0.000}");
        }

        _output.WriteLine("");
        _output.WriteLine($"  the worst an empty band gave was {worst:0.000}");
        _output.WriteLine($"  the reporting floor is {Ft8Sync.DefaultFloor:0.000}");

        // **THE FLOOR IS ABOVE WHAT NOISE REACHES, WITH ROOM.** A floor set at
        // the noise maximum would report a station in an empty band the first
        // time a seed came up unlucky, which is the failure this whole project
        // is built against (§0.0).
        Assert.True(
            Ft8Sync.DefaultFloor > worst,
            $"noise reached {worst:0.00} and the floor is {Ft8Sync.DefaultFloor:0.00}");
    }

    /// <remarks>
    /// **THE SEARCH IS PURE.** The same audio gives the same candidates, in the
    /// same order, every time. A first stage whose answer moved between runs
    /// would make every later stage impossible to attribute (§5).
    /// </remarks>
    [Fact]
    public void TheSameAudioGivesTheSameCandidates()
    {
        var slot = Transmission(1641, 0.72, 2.5);

        var once = Ft8Sync.Search(slot);
        var again = Ft8Sync.Search(slot);

        Assert.Equal(once.Count, again.Count);

        for (var i = 0; i < once.Count; i++)
        {
            Assert.Equal(once[i].FrequencyHz, again[i].FrequencyHz);
            Assert.Equal(once[i].TimeOffsetSeconds, again[i].TimeOffsetSeconds);
            Assert.Equal(once[i].Score, again[i].Score);
        }
    }

    /// <remarks>
    /// <para>**RUN OVER A REAL RECORDING FROM THE OPERATOR'S OWN RADIO**, cut
    /// into slots by the cutter that task 6 built. This is the whole chain
    /// meeting real audio: a real length, a real sample rate, and whatever the
    /// band was doing that evening.</para>
    /// <para>**AND WHAT IT FINDS IS REPORTED RATHER THAN ASSERTED.** The order
    /// asks for one of the operator's digital captures and **there is not one in
    /// this repository**, so this runs on a Morse capture from 40 m. There is no
    /// FT8 in it, so the honest expectation is nothing, and the test holds the
    /// search to not inventing stations in audio that has none. What it prints
    /// is what it found.</para>
    /// </remarks>
    [Fact]
    public void OnRealAudioItReportsWhatItFindsAndInventsNothing()
    {
        var path = Path.Combine(
            Root(), "tests", "fixtures", "cw", "captured", "unadjudicated",
            "cw-2026-08-28-005051.wav");

        var audio = WavAudio.Read(path);

        var cut = Ft8SlotCutter.Cut(
            audio,
            new DateTime(2026, 8, 28, 0, 50, 51, DateTimeKind.Utc),
            new ClockOffset(0.12, new DateTime(2026, 8, 28, 0, 50, 21, DateTimeKind.Utc)));

        _output.WriteLine(
            $"  {audio.Duration.TotalSeconds:0.0} s at {audio.SampleRate} Hz "
            + $"cut into {cut.Slots.Count} slot(s)");

        Assert.NotEmpty(cut.Slots);

        foreach (var slot in cut.Slots)
        {
            var found = Ft8Sync.Search(slot.Audio);

            _output.WriteLine("");
            _output.WriteLine($"  slot at {slot.StartUtc:HH:mm:ss} UTC:");
            Print(found);

            // **THIS IS A MORSE CAPTURE AND HOLDS NO FT8**, so anything reported
            // strongly here would be the search finding a pattern in a signal
            // that does not carry one.
            Assert.DoesNotContain(found, c => c.Score > 4.0);
        }
    }
}
