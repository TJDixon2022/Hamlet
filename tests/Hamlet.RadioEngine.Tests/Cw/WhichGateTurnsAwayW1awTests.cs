using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **Which gate is turning away W1AW** (work instruction 504). The owner's verdict rows of
/// 2026-09-30, 12:09 UTC, 7.0475 MHz: the meter at 725 Hz, dit 90 ms, bars saying keying, and
/// `MarksLast4s` nought on a strong, clean, correctly tuned station.
/// </summary>
/// <remarks>
/// <para>**SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK** (R96). An 18 WPM call at 725 Hz,
/// the owner's pitch, about 22 dB over white noise, read three ways: over the whole band as the
/// detector reads with no radio, over the owner's passband (CW pitch 600, filter 500, so the bins
/// run 350 to 850 Hz), and over that passband with the radio's scope pointing at 725 Hz.</para>
/// <para>Each mark gate is switched off in turn and the call's own marks counted, beside the most
/// `MarksLast4s` read on any hop and what the run reader prints.</para>
/// </remarks>
public sealed class WhichGateTurnsAwayW1awTests
{
    private const int Rate = 8000;
    private const int Chunk = 80;
    private const double Pitch = 725;
    private const string Call = "CQ CQ DE N0CALL N0CALL K";

    private static readonly Dictionary<char, string> Morse = new()
    {
        ['C'] = "-.-.", ['Q'] = "--.-", ['D'] = "-..", ['E'] = ".", ['N'] = "-.", ['0'] = "-----",
        ['A'] = ".-", ['L'] = ".-..", ['K'] = "-.-",
    };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public WhichGateTurnsAwayW1awTests(ITestOutputHelper output) => _output = output;

    private static int Sent => Call.Where(c => c != ' ').Sum(c => Morse[c].Length);

    private static float[] W1awCall(int rate = Rate) => CwSignal.Generate(new CwSignalRequest(
        Call, WordsPerMinute: 18, ToneHz: Pitch, SampleRate: rate, Amplitude: 0.5,
        NoiseAmplitude: 0.04, LeadInSeconds: 3, TailSeconds: 3, Seed: 504)).Samples;

    private sealed record Gates(string Name, Action<CwEnvelopeDetector> Off);

    private static readonly Gates[] Each =
    {
        new("all on", _ => { }),
        new("492 promptness off", d => d.MarksNeedPromptness = false),
        new("492 key-up off", d => d.MarksNeedKeyUp = false),
        new("492 one-call off", d => d.MarksNeedOneCall = false),
        new("497 edges off", d => d.MarksNeedEdges = false),
        new("498 narrowness off", d => d.MarksNeedNarrowness = false),
        new("502 shape off", d => d.MarksNeedShape = false),
        new("all off", d =>
        {
            d.MarksNeedPromptness = false;
            d.MarksNeedKeyUp = false;
            d.MarksNeedOneCall = false;
            d.MarksNeedEdges = false;
            d.MarksNeedNarrowness = false;
            d.MarksNeedShape = false;
        }),
    };

    private sealed record Result(int Own, int MostLast4s, int KeyingHops, string Text, double LowestNarrow, double Low, double High);

    private static Result Run(float[] samples, Action<CwEnvelopeDetector> off, bool passband, double? pointed, int rate = Rate)
    {
        var detector = new CwEnvelopeDetector(rate);
        var chunk = Chunk * rate / Rate;

        off(detector);

        if (passband)
        {
            detector.SetPassband(600, 500);
        }

        // Since unit 515 nothing points the detector (R114): the pointed rows read as the unpointed ones.
        _ = pointed;

        var reader = new CwSenderGate();
        var characters = new List<CwCharacter>();
        var sequence = 0L;
        var most = 0;
        var keying = 0;

        reader.CharacterRead += characters.Add;

        for (var at = 0; at + chunk <= samples.Length; at += chunk)
        {
            detector.Process(samples.AsSpan(at, chunk));

            var batch = detector.MarksSince(sequence);

            sequence = batch.Marks.Count > 0 ? batch.Marks.Max(m => m.Sequence) : sequence;
            reader.Read(batch);

            var reading = detector.Reading;

            most = Math.Max(most, reading.MarksLast4s);
            keying += reading.Keying ? 1 : 0;
        }

        reader.Flush();

        var near = detector.MarksSince(0).Marks.Where(m => Math.Abs(m.PitchHz - Pitch) <= CwSenderGate.PitchToleranceHz).ToList();
        var loudest = near.Count > 0 ? near.Max(m => m.LevelDb) : double.NaN;
        var own = near.Where(m => m.LevelDb >= loudest - 6).ToList();
        var text = string.Join(' ', string.Concat(characters.Select(c => c.Text)).Split(' ', StringSplitOptions.RemoveEmptyEntries));
        var lowestNarrow = own.Count > 0 ? own.Min(m => m.Shape?.Narrowness ?? double.NaN) : double.NaN;

        return new Result(own.Count, most, keying, text, lowestNarrow, detector.Reading.PassbandLowHz, detector.Reading.PassbandHighHz);
    }

    /// <remarks>
    /// The table: for the whole band, the owner's passband, and the passband with the scope
    /// pointing, how many of the call's marks each gate lets through with it off, the most
    /// `MarksLast4s` any hop read, and the text. Asserts that with every gate on the owner's
    /// passband hands out every mark sent - one more is a mark split in two, which the reader
    /// joins - and reads the call whole.
    /// </remarks>
    [Fact]
    public void EachGateOffInTurnAt725InTheOwnersPassband()
    {
        var samples = W1awCall();
        var rows = new Dictionary<(string, string), Result>();

        _output.WriteLine($"18 WPM call at {Pitch:0} Hz, {Sent} marks sent");

        foreach (var (label, passband, pointed) in new (string, bool, double?)[] { ("whole band", false, null), ("600/500", true, null), ("600/500 pointed 725", true, Pitch) })
        {
            foreach (var g in Each)
            {
                var r = rows[(label, g.Name)] = Run(samples, g.Off, passband, pointed);

                Print(label, g.Name, r);
            }
        }

        var whole = rows[("whole band", "all on")];
        var owners = rows[("600/500", "all on")];

        Assert.InRange(whole.Own, Sent, Sent + 1);
        Assert.InRange(owners.Own, Sent, Sent + 1);
        Assert.Equal(Call, owners.Text);
    }

    /// <remarks>
    /// What differs on the air, every gate on: the scope pointing 125 Hz off the station, at the
    /// radio's CW pitch of 600 Hz, and the audio at the sound card's 48 kHz rather than 8 kHz.
    /// Asserts only that the call still reads whole at 48 kHz; the pointed row is printed for the
    /// report, since `MarksLast4s` counts the watched bin alone.
    /// </remarks>
    [Fact]
    public void WhatDiffersOnTheAir()
    {
        var eight = W1awCall();
        var fortyEight = W1awCall(48000);
        var off = Run(eight, _ => { }, passband: true, pointed: 600);
        var fast = Run(fortyEight, _ => { }, passband: true, pointed: null, rate: 48000);

        Print("600/500 pointed 600", "all on", off);
        Print("600/500 at 48 kHz", "all on", fast);

        Assert.Equal(Call, fast.Text);
    }

    private void Print(string label, string gates, Result r)
        => _output.WriteLine(
            $"{label,-20} {gates,-20} own marks {r.Own,3} of {Sent}, most MarksLast4s {r.MostLast4s,3}, keying hops {r.KeyingHops,5}, "
            + $"lowest narrow score {r.LowestNarrow:0.00} (bins {r.Low:0}-{r.High:0} Hz), reads `{r.Text}`");
}
