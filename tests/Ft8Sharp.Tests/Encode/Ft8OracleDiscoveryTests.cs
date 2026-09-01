using Ft8Sharp.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// What upstream's generator actually emits, established by running it rather than by reading its
/// source — because the comparison's whole shape depends on the answer.
/// </summary>
/// <remarks>
/// <para>
/// Three possibilities and they lead three different places. A tone sequence on stdout is the
/// direct channel and answers step 3's second criterion cleanly. A WAV and nothing else leaves
/// demodulation as the only route. A payload or a codeword alongside is free, and upgrades the
/// first criterion from a syndrome check against our own tables to a byte-for-byte comparison
/// against upstream's own bits.
/// </para>
/// <para>
/// <b>Shapes are asserted; values are not committed.</b> What upstream's binary produces is read at
/// run time and never enters this repository. Counts and lengths are facts about the <em>form</em>
/// of its output, which is what a reader of the report is entitled to.
/// </para>
/// </remarks>
public class Ft8OracleDiscoveryTests
{
    private readonly ITestOutputHelper _output;

    public Ft8OracleDiscoveryTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// Whether the executable runs at all, asked the cheapest way there is: with no arguments, so
    /// it prints its own usage and stops long before it reaches any signal buffer.
    /// </summary>
    /// <remarks>
    /// This separates two very different faults that both show up as a non-zero exit. A binary that
    /// cannot print its usage is a bad build. A binary that prints its usage and then dies while
    /// generating a waveform is a good build meeting a limit of the platform it was linked for, and
    /// only the second of those leaves the port's evidence within reach.
    /// </remarks>
    [RequiresOracleFact]
    public void TheGeneratorPrintsItsUsageWithNoArguments()
    {
        var run = Ft8Oracle.Invoke(wavToMeasure: null);

        _output.WriteLine($"exit code : {run.ExitCode} (0x{run.ExitCode:X8})");
        _output.WriteLine($"stdout    : {run.StandardOutput.Length} chars");

        Assert.Contains("Usage", run.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("gen_ft8", run.StandardOutput, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// What the generator does with a message it should encode, recorded rather than asserted, so
    /// the finding survives in the suite whichever way this machine's build goes.
    /// </summary>
    /// <remarks>
    /// <b>This test passes either way on purpose.</b> Its job is to say what happened, and the gate
    /// that decides whether the comparison runs is
    /// <see cref="RequiresWorkingOracleFactAttribute"/>. Asserting a zero exit here would paint the
    /// whole project red for a fault in somebody else's build, which is not this library's news to
    /// carry.
    /// </remarks>
    [RequiresOracleFact]
    public void WhatTheGeneratorDoesWithARealMessageIsRecorded()
    {
        var (state, detail) = Ft8Oracle.ProbeUsability();

        _output.WriteLine($"executable : {Ft8Oracle.ExecutablePath}");
        _output.WriteLine($"usability  : {state}");
        _output.WriteLine($"detail     : {detail}");

        Assert.NotEqual(Ft8Oracle.Usability.CloneAbsent, state);
    }

    [RequiresWorkingOracleFact]
    public void TheGeneratorRunsAndSaysWhatItEmits()
    {
        var run = Ft8Oracle.Generate("CQ K1ABC FN42");

        _output.WriteLine($"exit code : {run.ExitCode}");
        _output.WriteLine($"wav bytes : {run.WavBytes}");
        _output.WriteLine($"stderr    : {(run.StandardError.Trim().Length == 0 ? "(empty)" : "present")}");

        var lines = run.StandardOutput.Replace("\r", string.Empty).Split('\n');
        _output.WriteLine($"stdout lines: {lines.Length}");
        for (var i = 0; i < lines.Length; i++)
        {
            _output.WriteLine($"  [{i,2}] {Ft8Oracle.Shape(lines[i])}");
        }

        var readTones = Ft8Oracle.TryReadTones(
            run.StandardOutput,
            Ft8SymbolEncoder.SymbolCount,
            out var tones);

        _output.WriteLine($"a tone line of exactly {Ft8SymbolEncoder.SymbolCount} symbols: {readTones}");
        _output.WriteLine($"wrote a WAV: {run.WavBytes > 0} ({run.WavBytes} bytes)");

        Assert.Equal(0, run.ExitCode);
        Assert.True(readTones, "upstream printed no tone sequence this parser could read");
        Assert.Equal(Ft8SymbolEncoder.SymbolCount, tones.Length);
    }

    /// <summary>
    /// Whether the generator also prints a payload or a codeword, which is what would upgrade
    /// criterion 1 from a syndrome check against our own tables to a byte-for-byte comparison.
    /// </summary>
    [RequiresWorkingOracleFact]
    public void WhetherTheGeneratorAlsoEmitsAPayloadOrACodeword()
    {
        var run = Ft8Oracle.Generate("CQ K1ABC FN42");

        foreach (var label in new[] { "payload", "codeword", "crc", "message" })
        {
            var found = Ft8Oracle.TryReadHexAfterLabel(run.StandardOutput, label, out var bytes);
            _output.WriteLine($"a hex run labelled '{label}': {found}" + (found ? $", {bytes.Length} bytes" : string.Empty));
        }
    }

    /// <summary>
    /// Whether the generator will encode a message whose callsign travels as a hash — unit 208's
    /// carried-forward debt, now on its third unit, and the one leg the corpus alone cannot settle.
    /// </summary>
    [RequiresWorkingOracleFact]
    public void TheGeneratorIsAskedForAHashedCallsignMessage()
    {
        var run = Ft8Oracle.Generate("PJ4/K1ABC W9XYZ");

        _output.WriteLine($"exit code : {run.ExitCode}");
        _output.WriteLine($"wav bytes : {run.WavBytes}");

        var readTones = Ft8Oracle.TryReadTones(
            run.StandardOutput,
            Ft8SymbolEncoder.SymbolCount,
            out _);
        _output.WriteLine($"a tone line came back: {readTones}");
    }
}
