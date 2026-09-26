using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Proves HM-REQ-010 and HM-REQ-129: short bursts between a sender's letters do
/// not set the speed our decoder reads that sender at, so no letter of his is
/// printed sure and wrong (work instruction 459, task 2; PHASE_PLAN.md 9.4).
/// </summary>
/// <remarks>
/// <para>**BUILT FROM TASK 1'S EVIDENCE, NOT FROM THE CHANGE.** On
/// `unadjudicated/cw-2026-08-22-032050` the read that printed U as `A` held a
/// window whose short mark cluster sat at 20 ms and short gap cluster at 50 ms, so
/// it read at 34.3 WPM against a sender the port holds at 17; about one mark in
/// five there is a burst of 10 to 45 ms, and the sender's dits run near 80 ms and
/// dahs near 190 (`.run-unit/unit459-speedset.txt`). This case is that sender and
/// those bursts: dit 80, dah 190, element gap 50, letter and word gaps of three
/// and seven dits, 15 dB, and one 20 ms burst at the sender's pitch and level in
/// the middle of every letter gap and word gap.</para>
/// <para>**THE KEY IS EXACT BY CONSTRUCTION** and the case is never sole evidence
/// (CLAUDE.md 12.5). One construction, one seed, run as written.</para>
/// </remarks>
public sealed class TheSpeedFollowsTheSendersMarkPairsTests
{
    private const double BurstMilliseconds = 20;

    private static readonly CwFixtureRecipe Sender = new(
        Name: "pairs-15wpm-bursts",
        Text: SyntheticCq.Text,
        DitMilliseconds: 80,
        DahMilliseconds: 190,
        ElementGapMilliseconds: 50,
        CharacterGapMilliseconds: 240,
        WordGapMilliseconds: 560,
        SignalToNoiseDb: CwFixtureCatalogue.EasyDb,
        ToneHz: 615,
        DriftHz: 0,
        Seed: 20260926);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the reading is printed.</param>
    public TheSpeedFollowsTheSendersMarkPairsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The sender with a burst in the middle of every gap between letters and between words.</summary>
    internal static MonoAudio WithBursts()
    {
        var (audio, sidecar) = CwFixtureGenerator.Generate(Sender);
        var peakDbfs = double.Parse(
            sidecar.Split('\n').Single(l => l.StartsWith("toneAmplitude", StringComparison.Ordinal))
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)[1],
            CultureInfo.InvariantCulture);
        var amplitude = Math.Pow(10, peakDbfs / 20);
        var samples = audio.Samples.ToArray();
        var rate = audio.SampleRate;

        foreach (var centre in GapCentres())
        {
            var first = (int)Math.Round((centre - (BurstMilliseconds / 2000.0)) * rate);
            var length = (int)Math.Round(BurstMilliseconds / 1000.0 * rate);
            var edge = (int)Math.Round(0.005 * rate);

            for (var n = 0; n < length; n++)
            {
                var shape = n < edge ? 0.5 * (1 - Math.Cos(Math.PI * n / edge))
                    : n >= length - edge ? 0.5 * (1 - Math.Cos(Math.PI * (length - 1 - n) / edge))
                    : 1.0;
                var i = first + n;

                samples[i] = (float)Math.Clamp(
                    samples[i] + (amplitude * shape * Math.Sin(2 * Math.PI * Sender.ToneHz * i / rate)), -1.0, 1.0);
            }
        }

        return new MonoAudio(rate, samples);
    }

    /// <summary>The middle of every gap between two letters and between two words, in seconds, from the recipe's own timing.</summary>
    private static IEnumerable<double> GapCentres()
    {
        // The generator's lead-in; the recipe carries no preamble.
        var at = 1.0;
        var firstWord = true;

        foreach (var word in Sender.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!firstWord)
            {
                yield return at + (Sender.WordGapMilliseconds / 2000.0);
                at += Sender.WordGapMilliseconds / 1000.0;
            }

            firstWord = false;

            for (var c = 0; c < word.Length; c++)
            {
                if (c > 0)
                {
                    yield return at + (Sender.CharacterGapMilliseconds / 2000.0);
                    at += Sender.CharacterGapMilliseconds / 1000.0;
                }

                var pattern = MorseCode.Spell(word[c])!;

                for (var e = 0; e < pattern.Length; e++)
                {
                    if (e > 0)
                    {
                        at += Sender.ElementGapMilliseconds / 1000.0;
                    }

                    at += (pattern[e] == '.' ? Sender.DitMilliseconds : Sender.DahMilliseconds) / 1000.0;
                }
            }
        }
    }

    /// <remarks>
    /// HM-REQ-010 and HM-REQ-129: on the sender above, the decoder fed hop by hop
    /// from 600 Hz prints no sure character wrong or added against the exact key,
    /// and keeps HM-REQ-012's coverage of 0.90, so the first is not bought by
    /// printing less. Watched failing first at HEAD (work instruction 459, task 2).
    /// </remarks>
    [Fact]
    public void BurstsBetweenLettersDoNotSetTheSpeed()
    {
        var audio = WithBursts();
        var decoder = new CwDecoder(audio.SampleRate, SyntheticCq.StartingPitchHz);
        var settled = new List<CwCharacter>();

        decoder.CharacterSettled += settled.Add;

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        var reading = CwReading.Of(settled);
        var from = reading.Text.TakeWhile(char.IsWhiteSpace).Count();
        var measured = TheRequirementsAreMeasuredTests.Measure(
            Sender.Name, "synthetic", "synthetic, 15 dB, 20 ms bursts in every letter and word gap", CwKeyKind.Exact,
            settled, new[] { SyntheticCq.Whole(reading) with { Start = from } });
        var errors = measured.Stretches.Select(CwMetrics.SureErrors).ToList();
        var coverage = measured.Stretches.Select(CwMetrics.Coverage).ToList();
        var wrong = errors.Sum(e => e.SureWrong + e.SureAdded);
        var emitted = errors.Sum(e => e.SureEmitted);
        var right = coverage.Sum(c => c.SureRight);
        var sent = measured.Stretches.Sum(a => a.KeyCharacters);

        _output.WriteLine($"key  | {SyntheticCq.Text}");
        _output.WriteLine($"read | {reading.Text}");
        _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
            $"speeds | {string.Join(" ", settled.Where(c => !c.IsWordGap).Select(c => c.WordsPerMinute).Distinct())} WPM"));
        _output.WriteLine($"metric | sure wrong or added {wrong} of {emitted} sure | sure and right {right} of {sent} sent");

        Assert.True(wrong == 0, $"{wrong} sure characters wrong or added of {emitted}: `{reading.Text}`");
        Assert.True(right >= 0.90 * sent, $"coverage {right} of {sent}: `{reading.Text}`");
    }
}
