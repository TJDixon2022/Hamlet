using System.Globalization;
using System.Reflection;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **WHAT THE PANEL SHOWS WITH THE LEARNED READER ON** (work instruction 598, task 1): the learned reader's vector played through the tab's
/// own chain and scope feed, ticked twenty times a second on the audio's clock, and each second the three things the panel shows: the
/// scope's tone and decoding lines, the terminal's speed as the main window composes it from <see cref="CwDecoder.WordsPerMinute"/>, and
/// the hold-still light's words from <see cref="CwHearingViewModel"/>. It asserts nothing and is on no line.
/// </summary>
[Trait("Category", "Survey")]
public sealed class TheLearnedReaderOnThePanelProbe(ITestOutputHelper output)
{
    private const double TickMs = 50;

    /// <remarks>The vector, 12 s at 579 Hz, then 4 s of quiet noise so the learned reader commits its last letters.</remarks>
    [Fact]
    public void ThePanelWhileTheLearnedReaderPrints()
    {
        output.WriteLine("SWITCH OFF");
        Play(learned: false);
        output.WriteLine("SWITCH ON");
        Play(learned: true);
    }

    private void Play(bool learned)
    {
        using var chosen = learned ? CwRules.Choose(CwRules.LearnedReader) : null;
        var audio = WavAudio.Read(Path.Combine(RepositoryRoot(), "tests", "fixtures", "cwnet", "cwnet-vector.wav"));
        var random = new Random(598);
        var samples = audio.Samples.Concat(Enumerable.Range(0, 4 * audio.SampleRate).Select(_ => (float)((random.NextDouble() - 0.5) * 0.004))).ToArray();

        using var chain = new CwChain(audio.SampleRate);
        var decoder = chain.Decoder;
        var envelope = chain.Detector;
        var feed = new CwScopeFeed();
        var hearing = new CwHearingViewModel();
        var start = new DateTime(2026, 11, 5, 22, 0, 0, DateTimeKind.Utc);
        var printed = new System.Text.StringBuilder();

        DateTime Now() => start + decoder.Heard;

        decoder.CharacterSettled += c =>
        {
            printed.Append(c.Text);
            feed.Settle(c, decoder.Heard, Now());
        };

        envelope.SetPassband(600, 500);

        var perTick = (int)(audio.SampleRate * TickMs / 1000);
        var frame = CwScopeFrame.Empty;
        var lastLine = string.Empty;

        for (var at = 0; at + perTick <= samples.Length; at += perTick)
        {
            chain.Process(new AudioChunk(at, audio.SampleRate, samples.AsSpan(at, perTick)));
            frame = feed.Tick(envelope, envelope.Reading, decoder.RunsPrintingHz, frame, scopeQuiet: false, Now());
            hearing.ObserveScope(frame);

            var wpm = decoder.WordsPerMinute ?? 0;
            var speed = wpm > 0 ? $"{wpm} WPM" : "(blank)";
            var line = $"tone `{frame.ToneLine}` | `{frame.MixingLine}` | speed {speed} | light `{hearing.ShapeLightWords}`";

            if (line != lastLine || (at + perTick) % audio.SampleRate < perTick)
            {
                output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"  {(at + perTick) / (double)audio.SampleRate,5:0.00} s: {line} | printed `{printed}`"));
                lastLine = line;
            }
        }

        decoder.Flush();
        output.WriteLine($"  at the end: printed `{printed}`");
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);

        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "src", "Hamlet.App")))
        {
            directory = directory.Parent;
        }

        return directory!.FullName;
    }
}
