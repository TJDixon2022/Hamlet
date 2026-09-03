using Ft8Sharp.Dsp;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Unit 227 task 3, the controls — and they come first because an instrument that has never been
/// shown working is not an instrument.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>Either of these can stop the night, and that is deliberate.</b> The comparison in
/// <see cref="Unit227MeasurementTests"/> is only worth reading if upstream's decoder demonstrably
/// works here and demonstrably hears a signal this library made. Running the measurement first and
/// the controls afterwards would let a broken harness produce a number that looked like a verdict.
/// </para>
/// </remarks>
public class Unit227ControlTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    /// <summary>
    /// <b>Control one: upstream reads its own generator. Twelve messages, and it must return
    /// twelve.</b>
    /// </summary>
    /// <remarks>
    /// <b>If this fails the harness is wrong rather than either decoder.</b> Nothing this project
    /// wrote is anywhere in the path — upstream's generator writes the file and upstream's decoder
    /// reads it — so a shortfall here is the wiring, the parser or the build, and never a finding
    /// about sensitivity.
    /// </remarks>
    [RequiresWorkingDecoderFact]
    public void UpstreamReadsItsOwnGenerator()
    {
        var messages = Ft8Step6Ladder.Population()
            .Where(e => e.Text is { Length: > 0 })
            .Take(12)
            .ToArray();

        _output.WriteLine("UNIT 227 TASK 3a — CONTROL ONE: upstream's generator into upstream's decoder");
        _output.WriteLine($"  generator : {Ft8Oracle.ResolvedExecutablePath}");
        _output.WriteLine($"  decoder   : {Ft8Decoder.ExecutablePath}");
        _output.WriteLine($"  messages  : {messages.Length}");
        _output.WriteLine(string.Empty);

        var returned = 0;
        var wrong = 0;
        foreach (var entry in messages)
        {
            var written = Ft8Oracle.GenerateKeepingWav(entry.Text!);
            try
            {
                var run = Ft8Decoder.Decode(written.WavPath);
                var got = run.Lines.Any(l => string.Equals(l.Text, entry.Text, StringComparison.Ordinal));
                wrong += run.Lines.Count(l => !string.Equals(l.Text, entry.Text, StringComparison.Ordinal));
                if (got)
                {
                    returned++;
                }

                _output.WriteLine(
                    $"  {(got ? "back" : "LOST")}  {entry.Text,-13}  {run.Lines.Count} line(s)"
                    + (run.Lines.Count > 0 ? $"  [{run.Lines[0].Raw.Trim()}]" : string.Empty));
            }
            finally
            {
                WavFile.DeleteQuietly(written.WavPath);
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  CONTROL ONE: {returned} of {messages.Length} returned, WRONG {wrong}");

        Assert.Equal(messages.Length, returned);
    }

    /// <summary>
    /// <b>Control two: upstream's decoder reads a signal this library made — and this is
    /// <c>HM-OPEN-065</c>'s own question, unasked since unit 210.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Step 3's nice-to-pass criterion 3 reads <i>audio synthesis produces a signal the reference
    /// decoder decodes</i>. Unit 212 met all four of step 3's must-pass criteria and could not ask
    /// this one, because no decoder existed on this machine; the debt was recorded as
    /// <c>HM-OPEN-065</c> and has been carried for seventeen units. <b>It is asked here as an open
    /// issue being discharged and not as step 3 being reopened.</b>
    /// </para>
    /// <para>
    /// <b>Noiseless, because the question is whether the waveform is decodable at all</b> and not
    /// how well it survives. Nine million samples agreeing with upstream's own generator to one
    /// count is what this replaces — strong evidence that the waveform is <em>identical</em>, and
    /// never evidence that anything can demodulate it.
    /// </para>
    /// </remarks>
    [RequiresWorkingDecoderFact]
    public void UpstreamReadsASignalThisLibraryMade()
    {
        var population = Ft8Step6Ladder.Population();

        _output.WriteLine("UNIT 227 TASK 3b — CONTROL TWO: this library's synthesis into upstream's decoder");
        _output.WriteLine("  HM-OPEN-065, and step 3's nice-to-pass criterion 3, asked for the first time.");
        _output.WriteLine($"  decoder    : {Ft8Decoder.ExecutablePath}");
        _output.WriteLine($"  population : {population.Count} messages, noiseless");
        _output.WriteLine($"  placed at  : {Unit227Paired.OnGridHz:F2} Hz, offset {Unit227Paired.AlignedOffset}");
        _output.WriteLine(string.Empty);

        var anything = 0;
        var exact = 0;
        var wrong = 0;
        var lost = new List<string>();

        foreach (var entry in population)
        {
            var (clean, _) = SearchFixture.OneSignal(
                Unit227Paired.Rate, entry, Unit227Paired.OnGridHz, Unit227Paired.AlignedOffset);
            var expected = Ft8MessageDecoder.Decode(entry.Message).Text;

            var path = Path.Combine(Path.GetTempPath(), $"ft8-unit227-3b-{Guid.NewGuid():N}.wav");
            try
            {
                WavFile.Write(path, clean, Unit227Paired.Rate);
                var run = Ft8Decoder.Decode(path);

                if (run.Lines.Count > 0)
                {
                    anything++;
                }

                if (run.Lines.Any(l => string.Equals(l.Text, expected, StringComparison.Ordinal)))
                {
                    exact++;
                }
                else
                {
                    lost.Add($"{entry.Label} — transmitted '{expected}', upstream printed "
                        + $"{run.Lines.Count} line(s)"
                        + (run.Lines.Count > 0 ? $": {string.Join(" | ", run.Lines.Select(l => l.Text))}" : string.Empty));
                }

                wrong += run.Lines.Count(l => !string.Equals(l.Text, expected, StringComparison.Ordinal));
            }
            finally
            {
                WavFile.DeleteQuietly(path);
            }
        }

        _output.WriteLine($"  came back at all      : {anything} of {population.Count}");
        _output.WriteLine($"  EXACT TRANSMITTED TEXT: {exact} of {population.Count}");
        _output.WriteLine($"  WRONG                 : {wrong}");
        _output.WriteLine(string.Empty);

        if (lost.Count > 0)
        {
            _output.WriteLine("  not returned with the exact transmitted text:");
            foreach (var line in lost)
            {
                _output.WriteLine($"    {line}");
            }

            _output.WriteLine(string.Empty);
        }

        _output.WriteLine(
            exact == population.Count
                ? "  HM-OPEN-065 IS DISCHARGED: upstream's own decoder reads what this library "
                  + "synthesizes, on every message of the population."
                : "  HM-OPEN-065 IS NOT DISCHARGED WHOLE. The shortfall is named above and is a "
                  + "finding larger than tonight's comparison — reported, not run past.");

        Assert.Equal(population.Count, exact);
    }
}
