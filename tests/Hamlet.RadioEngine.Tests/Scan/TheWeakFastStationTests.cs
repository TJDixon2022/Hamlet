using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// **THE WEAK FAST STATION** (work instruction 546, task 4): a station at about 26 WPM and about 11 dB over the noise, where
/// a dit is about 45 ms, printed as a line of junk on the owner's `cw-2026-10-03-143906` while a plain decoder reads
/// fragments such as `QSY QSY DE W`. Where the chain loses it: the detector's marks, the sender's window, the gate.
/// </summary>
/// <remarks>
/// <para>R88 is lifted for the owner's twelve recordings; this reads one of them, and synthetic calls written here.</para>
/// <para>**THE SYNTHETIC LEVEL** is the tone against white noise in a 500 Hz passband: a tone of amplitude 0.5 has power
/// 0.125, white noise of standard deviation σ at 8 kHz has σ²/8 of its power in 500 Hz, so σ = 10^(-dB/20).</para>
/// </remarks>
public sealed class TheWeakFastStationTests
{
    private const string Recording = "cw-2026-10-03-143906";

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public TheWeakFastStationTests(ITestOutputHelper output) => _output = output;

    /// <remarks>Task 4: the owner's recording through the chain at the radio's own pitch and filter, beside the plain read.</remarks>
    [Fact]
    public void TheRecordingThroughTheChain()
    {
        var audio = WavAudio.Read(Cw.TheOwnersRecordingReadsTests.Wav(Recording));
        var (pitch, width) = Cw.TheRecordingsScoreboardTests.RadioState(Recording);

        Trace($"{Recording} (radio pitch {pitch:0} Hz, filter {width:0} Hz)", audio, pitch, width, 514);

        Assert.True(audio.Samples.Length > 0);
    }

    /// <remarks>Task 4: a synthetic call at 25 WPM, 600 Hz, at 10 and 12 dB in the 500 Hz passband, through the same chain.</remarks>
    /// <param name="db">The tone over the noise in the passband.</param>
    [Theory]
    [InlineData(10)]
    [InlineData(12)]
    public void ASyntheticCallAt25Wpm(int db)
    {
        var audio = CwSignal.Generate(new CwSignalRequest(
            "QSY QSY DE W1AW W1AW K QSY QSY DE W1AW W1AW K", WordsPerMinute: 25, ToneHz: 600, SampleRate: 8000, Amplitude: 0.5,
            NoiseAmplitude: Math.Pow(10, -db / 20.0), LeadInSeconds: 3, TailSeconds: 3, Seed: 5460 + db));

        Trace($"25 WPM at {db} dB", audio, 600, 500, 600);

        Assert.True(audio.Samples.Length > 0);
    }

    private void Trace(string name, MonoAudio audio, double pitchHz, double widthHz, double stationHz)
    {
        var chain = TheStrongStationsOverTests.ReadChain(audio.Samples, audio.SampleRate, pitchHz, widthHz);
        var plain = TheStrongStationsOverTests.ReadPlain(audio.Samples, audio.SampleRate, stationHz);
        var at = chain.Marks.Where(m => Math.Abs(m.PitchHz - stationHz) <= 30).OrderBy(m => m.FromSeconds).ToList();
        var lengths = at.Select(m => m.LengthMs).ToList();
        var gaps = at.Skip(1).Select((m, i) => (m.FromSeconds - at[i].ToSeconds) * 1000).Where(g => g < 400).ToList();

        _output.WriteLine(name);
        _output.WriteLine(string.Create(Invariant, $"  plain: {plain.Marks.Count} marks, dit {plain.Dit * 1000:0} ms, dah {plain.Dah * 1000:0} ms; `{plain.Text}`"));
        _output.WriteLine(string.Create(Invariant, $"  detector: {chain.Marks.Count} marks at every pitch, {at.Count} within 30 Hz of {stationHz:0} Hz"));
        _output.WriteLine($"  their lengths, ms, by count: {string.Join(" ", Enumerable.Range(0, 12).Select(b => $"{b * 20}:{lengths.Count(l => l >= b * 20 && l < (b + 1) * 20)}"))}");
        _output.WriteLine($"  their gaps under 400 ms, by count: {string.Join(" ", Enumerable.Range(0, 10).Select(b => $"{b * 40}:{gaps.Count(g => g >= b * 40 && g < (b + 1) * 40)}"))}");

        // What the plain read hears as marks that the detector stood nothing over, and the other way.
        var missed = plain.Marks.Count(p => !at.Any(m => Math.Min(m.ToSeconds, p.To) - Math.Max(m.FromSeconds, p.From) > 0));
        var extra = at.Count(m => !plain.Marks.Any(p => Math.Min(m.ToSeconds, p.To) - Math.Max(m.FromSeconds, p.From) > 0));

        _output.WriteLine($"  plain marks the detector stood nothing over: {missed} of {plain.Marks.Count}; detector marks at the tone the plain read has no mark under: {extra}");
        _output.WriteLine(string.Create(Invariant, $"  gate: senders {chain.Senders}, the most marked at {chain.PitchHz:0} Hz with shape {chain.Shape:0.000}, light green {chain.Green}; {chain.Printed.Count} marks printed in {chain.Letters.Count} letters"));

        if (chain.Printed.Count > 0)
        {
            var dits = chain.Printed.Select(p => p.Dit * 1000).Where(double.IsFinite).ToList();

            _output.WriteLine(string.Create(Invariant, $"  printed sender's dit ranged {dits.Min():0} to {dits.Max():0} ms, split {chain.Printed.Min(p => p.Split) * 1000:0} to {chain.Printed.Max(p => p.Split) * 1000:0} ms"));
        }

        _output.WriteLine($"  chain: `{chain.Text}`");
    }
}

/// <summary>Task 4's probe: the 10 dB synthetic call with one of the detector's mark tests off, to find which refuses its dahs.</summary>
public sealed class TheWeakFastStationProbeTests(ITestOutputHelper output)
{
    /// <remarks>Prints marks, dahs and what reads with the rule off; asserts only that audio was read.</remarks>
    /// <param name="rule">The rule switched off, or none.</param>
    [Theory]
    [InlineData("none")]
    [InlineData(CwRules.MarkShape)]
    [InlineData(CwRules.Edges)]
    [InlineData(CwRules.Narrowness)]
    [InlineData(CwRules.Settle)]
    [InlineData(CwRules.OwnWindow)]
    public void TheTenDecibelCallWithARuleOff(string rule)
    {
        var audio = CwSignal.Generate(new CwSignalRequest(
            "QSY QSY DE W1AW W1AW K QSY QSY DE W1AW W1AW K", WordsPerMinute: 25, ToneHz: 600, SampleRate: 8000, Amplitude: 0.5,
            NoiseAmplitude: Math.Pow(10, -10 / 20.0), LeadInSeconds: 3, TailSeconds: 3, Seed: 5470));

        using var _ = CwRules.Off(rule == "none" ? [] : [rule]);

        var chain = TheStrongStationsOverTests.ReadChain(audio.Samples, audio.SampleRate);
        var at = chain.Marks.Where(m => Math.Abs(m.PitchHz - 600) <= 30).ToList();

        output.WriteLine($"off {rule}: {at.Count} marks at the tone, {at.Count(m => m.LengthMs >= 100)} of them 100 ms or longer; reads `{chain.Text}`");

        Assert.True(audio.Samples.Length > 0);
    }
}
