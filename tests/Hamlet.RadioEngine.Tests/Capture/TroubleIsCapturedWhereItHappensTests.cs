using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Capture;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Capture;

/// <summary>
/// **TROUBLE, CAPTURED WHERE IT HAPPENS** (work instruction 549, task 2, HM-DEC-253), on a fake clock and fake audio at
/// 100 samples a second. No recording is read.
/// </summary>
public sealed class TroubleIsCapturedWhereItHappensTests : IDisposable
{
    private const int Rate = 100;

    // Wednesday 2026-10-07 18:00 UTC, 13:00 US Central: no W1AW session.
    private static readonly DateTime Quiet = new(2026, 10, 7, 18, 0, 0, DateTimeKind.Utc);

    private readonly ITestOutputHelper _output;
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "hamlet-549-" + Guid.NewGuid().ToString("N"));
    private DateTime _now = Quiet;
    private long _index;

    public TroubleIsCapturedWhereItHappensTests(ITestOutputHelper output) => _output = output;

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    [Fact]
    public void FourSendersHeldTenSecondsFiresOnceThenWaitsOutItsCooldown()
    {
        var auto = Auto();

        // Nine seconds of five senders, then three: nothing.
        Run(auto, seconds: 9, Listening() with { SendersHeld = 5 });
        Run(auto, seconds: 1, Listening() with { SendersHeld = 3 });
        Assert.Empty(auto.TroubleFolders);

        // Ten seconds of four: it fires, once, at the tenth.
        Run(auto, seconds: 11, Listening() with { SendersHeld = 4 });
        var name = Assert.Single(auto.TroubleFolders);
        Assert.EndsWith("-senders", name, StringComparison.Ordinal);

        // Held on, nine more minutes: still one.
        Run(auto, seconds: 9 * 60, Listening() with { SendersHeld = 6 }, audio: false);
        Assert.Single(auto.TroubleFolders);

        // Past ten minutes from the first: the second.
        Run(auto, seconds: 61, Listening() with { SendersHeld = 6 }, audio: false);
        Assert.Equal(2, auto.TroubleFolders.Count);

        var sheet = File.ReadAllText(Path.Combine(_folder, name, "capture.txt"));
        _output.WriteLine(sheet);
        Assert.Contains("trigger    four or more senders held for ten seconds  (4 held now, since 18:00:11 UTC)", sheet);
        Assert.Contains("fired      2026-10-07 18:00:21 UTC", sheet);
    }

    [Fact]
    public void AnyAudioLostFiresOnceThenWaitsOutItsCooldown()
    {
        var auto = Auto();

        Run(auto, seconds: 30, Listening());
        Assert.Empty(auto.TroubleFolders);

        Run(auto, seconds: 1, Listening() with { AudioLostMilliseconds = 50 });
        Assert.EndsWith("-audio-lost", Assert.Single(auto.TroubleFolders), StringComparison.Ordinal);

        // More lost inside the cooldown saves nothing more; more lost after it does.
        Run(auto, seconds: 5 * 60, Listening() with { AudioLostMilliseconds = 400 }, audio: false);
        Assert.Single(auto.TroubleFolders);
        Run(auto, seconds: 5 * 60, Listening() with { AudioLostMilliseconds = 450 }, audio: false);
        Assert.Single(auto.TroubleFolders);
        Run(auto, seconds: 1, Listening() with { AudioLostMilliseconds = 500 }, audio: false);
        Assert.Equal(2, auto.TroubleFolders.Count);
    }

    [Fact]
    public void SixStrayLettersInTenSecondsFireAndWordsOfThemDoNot()
    {
        var auto = Auto();

        Run(auto, seconds: 5, Listening());

        // ANTENNA MEANTIME: fourteen letters of one or two elements in two words, inside ten seconds. Not stray.
        foreach (var word in new[] { "ANTENNA", "MEANTIME", "IN", "A", "MEAN", "TIME" })
        {
            Word(auto, word);
        }

        Run(auto, seconds: 1, Listening());
        Assert.Empty(auto.TroubleFolders);

        // E I S A N T E E: seven alone, S among them, inside ten seconds. Six count; S has three elements and does not.
        foreach (var letter in new[] { "E", "I", "S", "A", "N", "T", "E", "E" })
        {
            Word(auto, letter);
            Run(auto, seconds: 1, Listening());
        }

        Assert.EndsWith("-stray-letters", Assert.Single(auto.TroubleFolders), StringComparison.Ordinal);

        // Another burst inside ten minutes is not saved.
        foreach (var letter in new[] { "E", "I", "T", "A", "N", "T", "E", "E" })
        {
            Word(auto, letter);
            Run(auto, seconds: 1, Listening());
        }

        Assert.Single(auto.TroubleFolders);
    }

    [Fact]
    public void TheSavedAudioIsTheFiveMinutesBeforeTheTrigger()
    {
        var auto = Auto();

        // Seven minutes of audio, then a loss.
        Run(auto, seconds: 7 * 60, Listening());
        Run(auto, seconds: 1, Listening() with { AudioLostMilliseconds = 20 });

        var wav = WavAudio.Read(Path.Combine(_folder, Assert.Single(auto.TroubleFolders), "capture.wav"));

        Assert.Equal(300 * Rate, wav.Samples.Length);

        for (var i = 0; i < wav.Samples.Length; i++)
        {
            Assert.Equal(Quantized(Ramp(_index - wav.Samples.Length + i)), wav.Samples[i], 4);
        }
    }

    [Fact]
    public void NoTroubleCaptureDuringAScan()
    {
        var auto = Auto();

        Run(auto, seconds: 60, Listening() with { Scanning = true, SendersHeld = 7, AudioLostMilliseconds = 300 });
        Assert.Empty(auto.TroubleFolders);

        // The scan ends: the loss during it is taken as seen, and the senders must hold ten seconds afresh.
        Run(auto, seconds: 9, Listening() with { SendersHeld = 7, AudioLostMilliseconds = 300 });
        Assert.Empty(auto.TroubleFolders);
        Run(auto, seconds: 2, Listening() with { SendersHeld = 7, AudioLostMilliseconds = 300 });
        Assert.EndsWith("-senders", Assert.Single(auto.TroubleFolders), StringComparison.Ordinal);
    }

    [Fact]
    public void DuringAW1awCaptureATriggerIsNotedOnItsSheet()
    {
        var auto = Auto();

        // Wednesday's slow code practice, 13:00 to 14:00 UTC, on 40 m.
        _now = new DateTime(2026, 10, 7, 13, 10, 0, DateTimeKind.Utc);
        var onW1aw = Listening() with { FrequencyHz = 7_047_500 };

        Run(auto, seconds: 5, onW1aw);
        Run(auto, seconds: 12, onW1aw with { SendersHeld = 5 });
        Run(auto, seconds: 2, onW1aw with { SendersHeld = 5, FrequencyHz = 7_030_000 });

        Assert.Empty(auto.TroubleFolders);

        var w1aw = Assert.Single(Directory.GetDirectories(_folder));
        var sheet = File.ReadAllText(Path.Combine(w1aw, "piece-01.txt"));

        Assert.Contains("trigger    13:10:16 UTC  four or more senders held for ten seconds", sheet);
        Assert.Contains("(noted here instead of saving again)", sheet);
    }

    private static AutoCaptureConditions Listening()
        => new(Listening: true, InCw: true, Scanning: false, FrequencyHz: 7_030_000, FilterWidthHz: 500);

    private static float Ramp(long i) => (i % 1000 / 1000f) - 0.5f;

    private static float Quantized(float s) => (float)Math.Round(s * short.MaxValue) / short.MaxValue;

    private static CwCharacter Letter(string text)
        => new(text, CwConfidence.High, 1, MorseAlphabet.All.First(p => p.Value == text).Key, double.NaN, 18, TimeSpan.Zero);

    private static CwCharacter Gap()
        => new(MorseAlphabet.WordGap, CwConfidence.High, 1, string.Empty, double.NaN, 18, TimeSpan.Zero);

    private CwAutoCapture Auto() => new(_folder, W1awMorseFrequencies.Default, () => _now, threaded: false);

    // A word as the gate prints it: the word gap first, then its letters.
    private void Word(CwAutoCapture auto, string word)
    {
        auto.Character(Gap());

        foreach (var c in word)
        {
            auto.Character(Letter(c.ToString()));
        }
    }

    // A second at a time: the audio for it, then the tick.
    private void Run(CwAutoCapture auto, int seconds, AutoCaptureConditions conditions, bool audio = true)
    {
        var buffer = new float[Rate];

        for (var s = 0; s < seconds; s++)
        {
            if (audio)
            {
                for (var i = 0; i < Rate; i++)
                {
                    buffer[i] = Ramp(_index + i);
                }

                auto.Hear(_index, Rate, buffer);
                _index += Rate;
            }

            _now = _now.AddSeconds(1);
            auto.Tick(conditions);
        }
    }
}
