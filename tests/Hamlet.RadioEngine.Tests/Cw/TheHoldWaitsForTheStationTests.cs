using System.Globalization;
using System.Text.RegularExpressions;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE HOLD'S CLOCK WAITS FOR THE STATION, AND THE LANE THAT FITS IS HANDED ON** (work instruction 579, HM-DEC-283). Every hold
/// the handover makes on the five working receiver fixtures and Ken's two overs: the lane's dit against the station's, when it was
/// held, when the station first printed near it, and when and why it was let go. Read with the grid stepping aside
/// (<see cref="CwRules.GridStepsAside"/>) chosen, as shipped and with the two corrections (<see cref="CwRules.HoldWaitsAndFits"/>).
/// </summary>
public sealed class TheHoldWaitsForTheStationTests(ITestOutputHelper output)
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>One hold: its pitch and dit, when it began, the first letter printed near it, and how it ended.</summary>
    internal sealed record Hold(double PitchHz, double DitSeconds, double HeldAt, double FirstLetterAt, double LetGoAt, string How);

    /// <summary>Reads audio through the chain, collecting every hold from the handover trace and every letter with its pitch.</summary>
    internal static (List<Hold> Holds, List<(double At, double PitchHz, string Text)> Letters) Holds(float[] samples, int rate, double pitch, double width)
    {
        using var chain = new CwChain(rate);
        var gate = chain.Decoder.Runs;
        var trace = new List<string>();
        var letters = new List<(double, double, string)>();
        var chunk = rate / 100;

        chain.Detector.SetPassband(pitch, width);
        chain.Detector.HandoverTrace = trace;
        gate.RunRead += (c, run) => letters.Add((run[^1].ToSeconds, run.Average(m => m.PitchHz), c.Text));

        for (var at = 0; at + chunk <= samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, rate, samples.AsSpan(at, chunk)));
        }

        chain.Decoder.Flush();

        var end = samples.Length / (double)rate;
        var holds = new List<Hold>();
        (double Hz, double Dit, double At)? open = null;

        void Close(double at, string how)
        {
            if (open is { } o)
            {
                var first = letters.Where(l => l.Item1 >= o.At && l.Item1 <= at + 0.5 && Math.Abs(l.Item2 - o.Hz) <= CwEnvelopeDetector.HandoverHz).Select(l => l.Item1).DefaultIfEmpty(double.NaN).First();

                holds.Add(new Hold(o.Hz, o.Dit, o.At, first, at, how));
                open = null;
            }
        }

        foreach (var line in trace)
        {
            var held = Regex.Match(line, @"^([\d.]+) s holds (\d+) Hz at a (\d+) ms dit(.*)$");
            var letGo = Regex.Match(line, @"^([\d.]+) s let go (\d+) Hz(.*)$");
            var refit = Regex.Match(line, @"^([\d.]+) s refits (\d+) Hz to a (\d+) ms dit(.*)$");

            if (held.Success)
            {
                Close(double.Parse(held.Groups[1].Value, Inv), "another hold");
                open = (double.Parse(held.Groups[2].Value, Inv), double.Parse(held.Groups[3].Value, Inv) / 1000, double.Parse(held.Groups[1].Value, Inv));
            }
            else if (refit.Success && open is { } o)
            {
                var at = double.Parse(refit.Groups[1].Value, Inv);

                Close(at, $"refitted to {refit.Groups[3].Value} ms");
                open = (o.Hz, double.Parse(refit.Groups[3].Value, Inv) / 1000, at);
            }
            else if (letGo.Success)
            {
                var how = letGo.Groups[3].Value.Trim();

                Close(double.Parse(letGo.Groups[1].Value, Inv), how.Length == 0 ? "its sequence stopped standing, or the grid took it back, or a carrier" : how);
            }
        }

        Close(end, "held to the end");

        return (holds, letters);
    }

    /// <summary>The station's dit from a fixture sheet's speed.</summary>
    private static double Dit(string name)
        => 1.2 / double.Parse(File.ReadAllLines(Path.ChangeExtension(TheGridStepsAsideTests.Fixture(name), ".txt")).First(l => l.StartsWith("wpm", StringComparison.Ordinal))[3..].Trim(), Inv);

    private void Report(string name, List<Hold> holds, double stationDit, double from, double to)
    {
        output.WriteLine($"## {name}");
        output.WriteLine("| held at | pitch | lane dit | against the station's | first letter near it | let go | by |");
        output.WriteLine("|---|---|---|---|---|---|---|");

        foreach (var h in holds.Where(h => h.LetGoAt >= from && h.HeldAt <= to))
        {
            output.WriteLine(string.Create(Inv, $"| {h.HeldAt:0.00} s | {h.PitchHz:0} | {h.DitSeconds * 1000:0} ms | {h.DitSeconds / stationDit:0.00} | {(double.IsNaN(h.FirstLetterAt) ? "none" : $"{h.FirstLetterAt:0.00} s, {h.FirstLetterAt - h.HeldAt:0.0} s in")} | {h.LetGoAt:0.00} s, {h.LetGoAt - h.HeldAt:0.0} s held | {h.How} |"));
        }

        output.WriteLine(string.Empty);
    }

    /// <remarks>
    /// Work instruction 579, section 2: every hold on the working fixtures and Ken's two overs, with the grid stepping aside, as
    /// shipped and with the two corrections. Asserts nothing.
    /// </remarks>
    /// <param name="corrected">Whether the hold waits for the station and the lane that fits is handed on.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EveryHold(bool corrected)
    {
        using var rules = corrected ? CwRules.Choose(CwRules.GridStepsAside, CwRules.HoldWaitsAndFits) : CwRules.Choose(CwRules.GridStepsAside);

        output.WriteLine(corrected ? "the grid steps aside, the hold waits and the lane that fits is handed on" : "the grid steps aside, the hold as shipped");

        foreach (var name in TheGridStepsAsideTests.Working)
        {
            var audio = WavAudio.Read(TheGridStepsAsideTests.Fixture(name));
            var (holds, letters) = Holds(audio.Samples, audio.SampleRate, 600, 500);

            Report($"`{name}`, station dit {Dit(name) * 1000:0} ms; printed `{string.Concat(letters.Select(l => l.Text))}`", holds, Dit(name), 0, double.PositiveInfinity);
        }

        var ken = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(TheFoundWeakStationTests.Ken));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(TheFoundWeakStationTests.Ken);
        var (kenHolds, kenLetters) = Holds(ken.Samples, ken.SampleRate, pitch, width);

        foreach (var (from, to) in new[] { (80.0, 100.0), (272.0, 300.0) })
        {
            var near = string.Concat(kenLetters.Where(l => l.At >= from && l.At <= to + 1 && Math.Abs(l.PitchHz - 430) <= 45).Select(l => l.Text));

            Report(string.Create(Inv, $"Ken, {from:0} to {to:0} s, his dit about 93 ms by hand; printed at his pitch `{near}`"), kenHolds, 0.093, from, to);
        }
    }

    /// <remarks>
    /// Work instruction 579, section 2: what the sender gate makes of a held lane's marks, every half second through a working
    /// fixture with the two corrections and the grid stepping aside: the marks handed on near the pitch with their lengths, and each
    /// sender near it with its qualifying tests. Asserts nothing.
    /// </remarks>
    /// <param name="name">The working fixture.</param>
    [Theory]
    [InlineData("coverage-working")]
    [InlineData("prosigns-working")]
    public void WhatTheGateMakesOfTheHeldLane(string name)
    {
        using var rules = CwRules.Choose(CwRules.GridStepsAside, CwRules.HoldWaitsAndFits);
        var audio = WavAudio.Read(TheGridStepsAsideTests.Fixture(name));
        var rate = audio.SampleRate;
        using var chain = new CwChain(rate);
        var gate = chain.Decoder.Runs;
        var chunk = rate / 100;
        long sequence = 0;

        chain.Detector.SetPassband(600, 500);

        for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, rate, audio.Samples.AsSpan(at, chunk)));

            if ((at / chunk) % 50 != 49)
            {
                continue;
            }

            var now = (at + chunk) / (double)rate;
            var batch = chain.Detector.MarksSince(sequence);
            var near = batch.Marks.Where(m => Math.Abs(m.PitchHz - 615) <= 50).ToList();

            if (batch.Marks.Count > 0)
            {
                sequence = batch.Marks.Max(m => m.Sequence);
            }

            var senders = gate.SenderTests.Where(s => Math.Abs(s.PitchHz - 615) <= 50)
                .Select(s => string.Create(Inv, $"{s.PitchHz:0} Hz {s.Marks} marks, keyed runs {s.KeyedRuns}, two kinds {(s.TwoKinds ? "yes" : "no")}, letter gap {(s.LetterGap ? "yes" : "no")}, kinds held {(s.KindsHeld ? "yes" : "no")}, sure {(s.Sure ? "yes" : "no")}, shape {s.Shape:0.00}"));

            output.WriteLine(string.Create(Inv, $"{now:0.0} s: held {(double.IsNaN(chain.Detector.WeakStationPitchHz) ? "none" : chain.Detector.WeakStationPitchHz.ToString("0", Inv))}; marks near: {string.Join(" ", near.Select(m => $"{m.PitchHz:0}/{(m.ToSeconds - m.FromSeconds) * 1000:0}"))}; senders: {string.Join("; ", senders)}"));
        }
    }

    /// <remarks>
    /// Work instruction 579, section 2: how long the station took to print its first letter after its lane was held, on the four
    /// 5 dB CQs and the working fixtures, with the corrections and the grid stepping aside: the figure the bound is set from.
    /// Asserts nothing.
    /// </remarks>
    [Fact]
    public void HowLongAStationTakesToPrint()
    {
        using var rules = CwRules.Choose(CwRules.GridStepsAside, CwRules.HoldWaitsAndFits);
        var files = TheWeakEndTests.Files().Where(f => f.Name.Contains("-5db", StringComparison.Ordinal)).Select(f => f.Wav).Concat(TheGridStepsAsideTests.Working.Select(TheGridStepsAsideTests.Fixture));
        var waits = new List<double>();

        foreach (var file in files)
        {
            var audio = WavAudio.Read(file);
            var (holds, _) = Holds(audio.Samples, audio.SampleRate, 600, 500);
            var printed = holds.Where(h => !double.IsNaN(h.FirstLetterAt)).Select(h => h.FirstLetterAt - h.HeldAt).ToList();

            waits.AddRange(printed);
            output.WriteLine(string.Create(Inv, $"`{Path.GetFileNameWithoutExtension(file)}`: {holds.Count} holds, first letter after {string.Join(", ", printed.Select(p => p.ToString("0.0", Inv)))} s; never printed in {holds.Count - printed.Count}, held {string.Join(", ", holds.Where(h => double.IsNaN(h.FirstLetterAt)).Select(h => (h.LetGoAt - h.HeldAt).ToString("0.0", Inv)))} s"));
        }

        output.WriteLine(string.Create(Inv, $"longest wait to a first letter: {waits.DefaultIfEmpty(double.NaN).Max():0.0} s over {waits.Count} holds that printed"));
    }
}
