using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 314 task 4: **pressing PSK31 shows text arriving.**
/// </summary>
/// <remarks>
/// <para>**THE DEMODULATOR BEHIND THE TAB, FED FROM THE SAME AUDIO THE FT8 DECODER
/// USES.** Unit 314 built one channel at one spot, 1000 Hz above the dial; since work
/// instruction 315 there is one channel per carrier the search finds, and the clean
/// fixture has one carrier, so everything here still holds of its one row. Text lands on
/// the decoded-text panel as characters arrive rather than a message at a time, which is
/// what a mode with no slots needs.</para>
/// <para>**THE TAB STAYS INERT FOR SENDING.** Nothing here reconnects the send path, and
/// `ThePsk31TabIsInertTests` is re-run after this to say so.</para>
/// <para>**COMPUTED, NOT SEEN.** A fixture is pushed through the real audio tap and the
/// real tick, and what is read back is the view model. Nothing here looks at a pixel,
/// and nothing here is evidence about the radio (FACT-006).</para>
/// </remarks>
public sealed class ThePsk31PanelHearsTests
{
    private const string HisCall = "KC3QIS";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the text is printed.</param>
    public ThePsk31PanelHearsTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The panel accumulates the QSO text from the clean fixture.**</summary>
    [Fact]
    public void ThePanelAccumulatesTheQsoText()
    {
        var model = Listening();

        var audio = WavAudio.Read(Fixture("psk31-clean-1000hz.wav"));

        _output.WriteLine("fixture: " + audio.Samples.Length + " samples at "
            + audio.SampleRate + " Hz, "
            + audio.Duration.TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture)
            + " s");

        Play(model, audio);

        var text = PanelText(model);

        _output.WriteLine("rows on the panel : " + model.DigitalDecodes.Count);
        _output.WriteLine("characters        : " + text.Length);
        _output.WriteLine("text              : " + text.Replace("\r", "", StringComparison.Ordinal)
            .Replace("\n", " / ", StringComparison.Ordinal));

        var cer = ErrorRate(text, Reference());

        _output.WriteLine("CER               : "
            + cer.ToString("0.0000", CultureInfo.InvariantCulture));

        // **ONE LINE PER SIGNAL**, and there is one signal.
        Assert.Single(model.DigitalDecodes);

        Assert.True(
            cer <= 0.01,
            "the panel read CER " + cer.ToString("0.0000", CultureInfo.InvariantCulture));
    }

    /// <summary>**Text arrives as characters do, not all at the end.**</summary>
    /// <remarks>
    /// **A MODE WITH NO SLOTS HAS NOTHING TO WAIT FOR.** If the panel only filled when
    /// the audio ran out, it would be a slotted decoder wearing a streaming one's
    /// clothes, and on a live band it would show nothing at all.
    /// </remarks>
    [Fact]
    public void TextArrivesAsCharactersDo()
    {
        var model = Listening();

        var audio = WavAudio.Read(Fixture("psk31-clean-1000hz.wav"));

        var half = audio.Samples.Length / 2;

        Play(model, new MonoAudio(audio.SampleRate, audio.Samples[..half]));

        var midway = PanelText(model);

        _output.WriteLine("half way : " + midway.Length + " characters");
        _output.WriteLine("           " + midway.Replace("\r", "", StringComparison.Ordinal)
            .Replace("\n", " / ", StringComparison.Ordinal));

        Assert.True(
            midway.Length > 40,
            "only " + midway.Length + " characters had arrived half way through");

        Play(model, new MonoAudio(audio.SampleRate, audio.Samples[half..]));

        var whole = PanelText(model);

        _output.WriteLine("at the end: " + whole.Length + " characters");

        Assert.True(whole.Length > midway.Length, "nothing arrived in the second half");

        Assert.StartsWith(midway, whole, StringComparison.Ordinal);
    }

    /// <summary>**The line says where it is listening.**</summary>
    /// <remarks>
    /// **REWRITTEN BY WORK INSTRUCTION 315 TASK 3, NOT DELETED.** Unit 314's line named one
    /// spot a thousand hertz above the dial and said there was only one, which was true of
    /// what that step built. The listener now searches the whole passband, so what is
    /// protected is the same thing turned round: the line says how widely it listens, and
    /// names no fixed offset that would suggest it looks anywhere less.
    /// </remarks>
    [Fact]
    public void TheLineSaysWhereItIsListening()
    {
        var model = Listening();

        var line = model.DigitalModeStripLine;

        _output.WriteLine("line : " + line);

        Assert.DoesNotContain("cannot read", line, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("passband", line, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("one spot", line, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("1000", line, StringComparison.Ordinal);
    }

    /// <summary>**Nothing on the panel can be answered.**</summary>
    [Fact]
    public void NothingOnThePanelCanBeAnswered()
    {
        var model = Listening();

        var audio = WavAudio.Read(Fixture("psk31-clean-1000hz.wav"));

        Play(model, new MonoAudio(audio.SampleRate, audio.Samples[..(audio.Samples.Length / 4)]));

        _output.WriteLine("rows            : " + model.DigitalDecodes.Count);
        _output.WriteLine("can be answered : " + model.CanAnswerRowsForTests);

        Assert.NotEmpty(model.DigitalDecodes);
        Assert.False(model.CanAnswerRowsForTests);

        // **AND THE SEND DOOR STILL REFUSES.**
        model.SendCallToAnyoneCommand.Execute(null);

        _output.WriteLine("send line       : " + model.DigitalSendLine);

        Assert.Contains("PSK31", model.DigitalSendLine, StringComparison.Ordinal);
    }

    /// <summary>Push audio through the real tap and the real tick.</summary>
    /// <remarks>
    /// **IN LUMPS, THE WAY A SOUND CARD GIVES IT.** Quarter-second chunks with a tick
    /// between them, so what is exercised is the streaming path rather than one call
    /// with the whole recording in it.
    /// </remarks>
    private static void Play(MainWindowViewModel model, MonoAudio audio)
    {
        var chunk = audio.SampleRate / 4;

        for (var at = 0; at < audio.Samples.Length; at += chunk)
        {
            var count = Math.Min(chunk, audio.Samples.Length - at);

            model.TapForTests!.Take(
                audio.Samples.AsSpan(at, count), audio.SampleRate);

            model.LookForASlotForTests();
        }
    }

    private static string PanelText(MainWindowViewModel model)
        => string.Concat(model.DigitalDecodes.Select(r => r.Message));

    private MainWindowViewModel Listening()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            TapForTests = new AudioTap(),
            ClockOffset = new ClockOffset(0.033, DateTime.UtcNow),
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        model.ChooseDigitalModeCommand.Execute("PSK31");

        return model;
    }

    private static double ErrorRate(string got, string want)
    {
        var a = got.Trim();
        var b = want.Trim();

        if (b.Length == 0)
        {
            return a.Length == 0 ? 0 : 1;
        }

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

        return (double)previous[b.Length] / b.Length;
    }

    private static string Reference()
        => Encoding.Latin1.GetString(File.ReadAllBytes(Fixture("qso-text.txt")));

    private static string Fixture(string file)
        => Path.Combine(Root(), "assets", "fixtures", file);

    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
