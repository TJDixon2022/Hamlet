using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Prints the numbers step 4 of *CW decodes again* judges a rework piece on, and asserts nothing.
/// </summary>
/// <remarks>
/// <para>Criterion 4.2 names words as well as counts: `ATEEKEND` toward `WEEKEND` on 021410,
/// `AB OVE` toward `ABOVE` on 013637. `TheCapturesThatDecodeKeepDecodingTests` prints characters,
/// elements, unsure and tone and never the text, so this type decodes the two captures exactly as
/// `EachStillProducesWhatItDid` does and prints the text beside the counts.</para>
/// <para>Two texts are printed. `TEXT` is taken the way `CwDecodeHarness` takes it, from
/// `CwDecoder.CharacterDecoded`, which re-emits the leading edge each time it is revised, so a
/// letter can appear several times. `SETTLED`, from `CwDecoder.CharacterSettled`, holds each final
/// character once and has the shape of the app's sidecar text, so its distances are the ones a
/// piece is judged on. `CwDecoder.Reading` is the last window only and is not used.</para>
/// <para>For each target word the distance is the smallest Levenshtein distance between the word
/// and any substring of the text whose length is the word's plus or minus two, printed with that
/// substring. A smaller distance is nearer. Work instruction 395, decision 8. Never on a
/// carry-forward line.</para>
/// </remarks>
public sealed class TheReworkNumbersPrinterTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Takes the output the numbers are printed to.</summary>
    /// <param name="output">The test output.</param>
    public TheReworkNumbersPrinterTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>021410 against `WEEKEND`, `THINKING` and `FLEX`.</summary>
    [Fact]
    public void TheNumbersOf021410()
        => Print("unadjudicated/cw-2026-08-25-021410", "WEEKEND", "THINKING", "FLEX");

    /// <summary>013637 against `ABOVE` and `BREEZE`.</summary>
    [Fact]
    public void TheNumbersOf013637()
        => Print("unadjudicated/cw-2026-08-25-013637", "ABOVE", "BREEZE");

    private void Print(string name, params string[] words)
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

        var decoder = new CwDecoder(audio.SampleRate, 600);
        var decoded = new StringBuilder();
        var settled = new StringBuilder();
        decoder.CharacterDecoded += c => decoded.Append(c.Text);
        decoder.CharacterSettled += c => settled.Append(c.Text);
        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        var report = decoder.Report;
        var text = decoded.ToString().Trim();
        var final = settled.ToString().Trim();

        _output.WriteLine($"{name}: {report.CharactersEmitted} characters, "
            + $"{report.ElementsSeen} elements, {report.CharactersUnsure} unsure, "
            + $"at {report.ToneHz:0} Hz");
        _output.WriteLine($"TEXT    [{text}]");
        _output.WriteLine($"SETTLED [{final}]");

        foreach (var word in words)
        {
            var (distance, nearest) = Nearest(word, final);
            _output.WriteLine($"SETTLED DISTANCE {word} {distance} [{nearest}]");
        }

        foreach (var word in words)
        {
            var (distance, nearest) = Nearest(word, text);
            _output.WriteLine($"TEXT DISTANCE {word} {distance} [{nearest}]");
        }
    }

    private static (int Distance, string Nearest) Nearest(string word, string text)
    {
        var best = int.MaxValue;
        var nearest = string.Empty;

        for (var length = Math.Max(1, word.Length - 2); length <= word.Length + 2; length++)
        {
            for (var start = 0; start + length <= text.Length; start++)
            {
                var candidate = text.Substring(start, length);
                var distance = Levenshtein(word, candidate);
                if (distance < best)
                {
                    best = distance;
                    nearest = candidate;
                }
            }
        }

        return best == int.MaxValue ? (word.Length, string.Empty) : (best, nearest);
    }

    private static int Levenshtein(string a, string b)
    {
        var previous = new int[b.Length + 1];
        var current = new int[b.Length + 1];
        for (var j = 0; j <= b.Length; j++)
        {
            previous[j] = j;
        }

        for (var i = 1; i <= a.Length; i++)
        {
            current[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                current[j] = Math.Min(
                    Math.Min(current[j - 1] + 1, previous[j] + 1),
                    previous[j - 1] + cost);
            }

            (previous, current) = (current, previous);
        }

        return previous[b.Length];
    }
}
