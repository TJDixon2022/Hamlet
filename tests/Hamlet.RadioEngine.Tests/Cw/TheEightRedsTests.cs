using System.Reflection;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A printer, not a test: the audio behind the eight reds open at unit 401's
/// closing line of `docs\unit239-failing-set.txt`, decoded and printed so each
/// red can be traced to a file and a line (work instruction 402, task 1).
/// </summary>
/// <remarks>
/// <para>**IT ASSERTS NOTHING** and is on no carry-forward line. Group A is the
/// speed behind #24 and #41, group B the easy tier behind #43 to #45 on all three
/// events, group C the operator's own transmission behind #42, group D the share
/// behind #6 and #15. The samples since the last discontinuity are private to
/// `CwDecoder` and are read by reflection here, in the printer only.</para>
/// </remarks>
public sealed class TheEightRedsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the printer.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public TheEightRedsTests(ITestOutputHelper output) => _output = output;

    private static readonly FieldInfo Discontinuity = typeof(CwDecoder).GetField(
        "_samplesAtDiscontinuity", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly FieldInfo LastSample = typeof(CwDecoder).GetField(
        "_lastSample", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private void Poll(CwDecoder decoder, string label, int[] reasons)
    {
        var last = decoder.Reading;
        var since = (long)LastSample.GetValue(decoder)! - (long)Discontinuity.GetValue(decoder)!;
        var wpm = (int)Math.Round(last.WordsPerMinute);

        if (last.Text.Length == 0)
        {
            reasons[0]++;
        }
        else if (decoder.SpeedIsReacquiring)
        {
            reasons[1]++;
        }
        else if (wpm < CwDecoder.SlowestPlausibleWpm || wpm > CwDecoder.FastestPlausibleWpm)
        {
            reasons[2]++;
        }

        _output.WriteLine(
            $"A | {label} | text {last.Text.Length} | last-wpm {last.WordsPerMinute:0.00} "
            + $"| wpm {decoder.WordsPerMinute?.ToString() ?? "null"} "
            + $"| reacquiring {decoder.SpeedIsReacquiring} | retunes {decoder.Tracker.Retunes} "
            + $"| follows {decoder.Tracker.Follows} | since {since / (double)decoder.SampleRate:0.00} s "
            + $"| tone {decoder.Tracker.ToneHz:0}");
    }

    /// <summary>Group A: #24's real request and #41's two-station file.</summary>
    [Fact]
    public void GroupASpeed()
    {
        var real = CwSignal.Generate(new CwSignalRequest(
            "CQ DE W1AW K", WordsPerMinute: 18, ToneHz: 620,
            Amplitude: 0.5, NoiseAmplitude: 0.02, Seed: 5));

        foreach (var (name, audio) in new[]
        {
            ("#24", real),
            ("#41", (MonoAudio)WavAudio.Read(Path.Combine(
                CwFixtureCatalogue.Folder, CwFixtureCatalogue.TwoStationName + ".wav"))),
            ("exchange-easy", (MonoAudio)WavAudio.Read(Path.Combine(
                CwFixtureCatalogue.Folder, "exchange-easy.wav"))),
        })
        {
            var decoder = new CwDecoder(audio.SampleRate, 600);
            var reasons = new int[3];
            var polls = 0;
            var chunk = audio.SampleRate / 4;

            for (var at = 0; at < audio.Samples.Length; at += chunk)
            {
                var take = Math.Min(chunk, audio.Samples.Length - at);

                decoder.Process(new AudioChunk(
                    at, audio.SampleRate, audio.Samples.AsSpan(at, take)));

                polls++;
                Poll(decoder, $"{name} {at / (double)audio.SampleRate:0.00} s", reasons);
            }

            decoder.Flush();
            Poll(decoder, $"{name} flushed", new int[3]);

            _output.WriteLine(
                $"A | {name} | {audio.Samples.Length / (double)audio.SampleRate:0.0} s of audio "
                + $"| polls {polls} | null for text empty {reasons[0]}, reacquiring {reasons[1]}, "
                + $"outside bounds {reasons[2]} | reading '{decoder.Reading.Text}'");
        }
    }

    /// <summary>Group B: the easy tier on the three events.</summary>
    [Theory]
    [InlineData("coverage-easy")]
    [InlineData("exchange-easy")]
    [InlineData("tightfist-easy")]
    public void GroupBEasyTier(string name)
    {
        var recipe = CwFixtureCatalogue.All.Single(r => r.Name == name);
        var sent = recipe.Text.Replace("^", "", StringComparison.Ordinal)
            .Replace(" ", "", StringComparison.Ordinal).ToUpperInvariant();
        var expected = recipe.Text.Replace(CwFixtureCatalogue.RunUp, "", StringComparison.Ordinal)
            .Replace("^", "", StringComparison.Ordinal)
            .Replace(" ", "", StringComparison.Ordinal).ToUpperInvariant();

        var audio = WavAudio.Read(Path.Combine(CwFixtureCatalogue.Folder, name + ".wav"));
        var decoder = new CwDecoder(audio.SampleRate, 600);

        var decoded = new List<CwCharacter>();
        var settled = new List<CwCharacter>();
        IReadOnlyList<CwCharacter> edge = [];

        decoder.CharacterDecoded += decoded.Add;
        decoder.CharacterSettled += settled.Add;
        decoder.LeadingEdge += e => edge = e.ToList();

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        foreach (var (label, list) in new[]
        {
            ("CharacterDecoded", (IReadOnlyList<CwCharacter>)decoded),
            ("LeadingEdge last", edge),
            ("CharacterSettled", settled),
        })
        {
            var letters = list.Where(c => !c.IsWordGap).ToList();
            var got = string.Concat(letters.Select(c => c.Text))
                .Replace("<", "", StringComparison.Ordinal)
                .Replace(">", "", StringComparison.Ordinal);
            var strangers = letters.Where(c => !c.IsUnreadable && c.Text.Length == 1)
                .Count(c => !sent.Contains(c.Text[0], StringComparison.Ordinal));

            _output.WriteLine(
                $"B | {name} | {label} | {letters.Count} characters | unreadable "
                + $"{letters.Count(c => c.IsUnreadable)} | strangers {strangers} "
                + $"| ends {got.EndsWith(expected, StringComparison.Ordinal)} | '{got}' against '{expected}'");
        }
    }

    /// <summary>Group C: the operator's own transmission.</summary>
    [Fact]
    public void GroupCOwnTransmit()
    {
        var audio = WavAudio.Read(Path.Combine(CwFixtureCatalogue.Folder, "qsk-preamble.wav"));
        var decoder = new CwDecoder(audio.SampleRate, 600);

        var decoded = new List<CwCharacter>();
        var settled = new List<CwCharacter>();
        var edgeInside = new HashSet<string>();
        var blocked = new List<(double From, double To)>();
        double? from = null;

        decoder.CharacterDecoded += decoded.Add;
        decoder.CharacterSettled += settled.Add;
        decoder.LeadingEdge += e =>
        {
            foreach (var c in e.Where(c => c.At.TotalSeconds < 13))
            {
                edgeInside.Add($"{c.Text}@{c.At.TotalSeconds:0.000}");
            }
        };

        var chunk = audio.SampleRate / 50;

        for (var at = 0; at < audio.Samples.Length; at += chunk)
        {
            var take = Math.Min(chunk, audio.Samples.Length - at);

            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan(at, take)));

            var now = at / (double)audio.SampleRate;

            if (decoder.Tracker.Guard.IsBlocked && from is null)
            {
                from = now;
            }
            else if (!decoder.Tracker.Guard.IsBlocked && from is { } f)
            {
                blocked.Add((f, now));
                from = null;
            }
        }

        decoder.Flush();

        _output.WriteLine(
            $"C | own transmit {decoder.Report.OwnTransmitSeconds:0.0} s | guard spans "
            + $"{blocked.Count}, first {blocked.FirstOrDefault().From:0.00} s, last ends "
            + $"{blocked.LastOrDefault().To:0.00} s, blocked total {blocked.Sum(b => b.To - b.From):0.0} s");

        foreach (var (label, list) in new[] { ("CharacterDecoded", decoded), ("CharacterSettled", settled) })
        {
            var inside = list.Where(c => !c.IsWordGap && c.At.TotalSeconds < 13).ToList();

            _output.WriteLine(
                $"C | {label} | {list.Count(c => !c.IsWordGap)} in all | {inside.Count} before 13 s | "
                + string.Join(" ", inside.Select(c => $"{c.Text}@{c.At.TotalSeconds:0.00}")));
        }

        _output.WriteLine($"C | LeadingEdge | {edgeInside.Count} distinct before 13 s");

        var settledKeys = settled.Select(c => $"{c.Text}@{c.At.TotalSeconds:0.000}").ToHashSet();
        var reachesNeither = decoded.Where(c => c.At.TotalSeconds < 13)
            .Select(c => $"{c.Text}@{c.At.TotalSeconds:0.000}")
            .Count(k => !edgeInside.Contains(k) && !settledKeys.Contains(k));

        _output.WriteLine($"C | counted characters reaching neither LeadingEdge nor CharacterSettled: {reachesNeither}");
    }

    /// <summary>Group D: #6's and #15's cases, per seed.</summary>
    [Theory]
    [InlineData(25, 18.0, "")]
    [InlineData(12, 18.0, "VVV ")]
    public void GroupDShare(int wordsPerMinute, double snrDb, string prefix)
    {
        const string call = "CQ CQ DE N0CALL N0CALL K";
        var message = prefix + call;
        var expected = call.Count(c => c != ' ');

        foreach (var seed in new[] { 7919, 104729, 15485863 })
        {
            var result = CwDecodeHarness.Decode(new CwSignalRequest(
                message, WordsPerMinute: wordsPerMinute, ToneHz: 640, Amplitude: 0.5,
                NoiseAmplitude: CwSensitivity.NoiseFor(snrDb), Seed: seed));

            var matches = CwAlignment.Align(result.Characters, message);
            var share = (double)matches.Count(m => m.Kind == CwMatchKind.Correct
                && !m.Decoded.IsWordGap && m.Expected != "V") / expected;

            var kinds = new StringBuilder();
            foreach (var m in matches.Where(m => !m.Decoded.IsWordGap))
            {
                kinds.Append(m.Kind switch
                {
                    CwMatchKind.Correct => m.Decoded.Text,
                    CwMatchKind.Wrong => $"[{m.Decoded.Text}/{m.Expected}]",
                    _ => $"(+{m.Decoded.Text})",
                });
            }

            _output.WriteLine(
                $"D | {wordsPerMinute} wpm {snrDb:0} dB {(prefix.Length == 0 ? "bare" : "run-up")} "
                + $"| seed {seed} | share {share:0.00} | wpm {result.WordsPerMinute} "
                + $"| settled '{result.Text}' | kinds {kinds}");
        }
    }
}
