using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Rsid;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Tests.Psk31;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 359 task 4: **every PSK31 send begins with its announcement (1.5, and
/// `rsid_sent` of 1.6).**
/// </summary>
/// <remarks>
/// <para>**R27, TIM 2026-09-14**: *every keyboard-mode transmission Hamlet sends begins with the
/// RSID burst naming its mode and variant - Olivia, and PSK31 retroactively.*</para>
/// <para>**THE BURST IS COMPOSED IN FRONT OF THE TEXT AND NOWHERE ELSE** (§6, §R10). It rides
/// the same `UnslottedTransmission` through the same door, the same arming and the same
/// `Ft8TransmitSequence`; the counts of keying and arming sites are asserted unchanged from the
/// task 1 trace, and the text's own samples are asserted identical to the CQ hashed there.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio: a fake port and a
/// fake sink on a machine with none (FACT-004, FACT-006).</para>
/// </remarks>
public sealed class ThePsk31SendIsAnnouncedTests : IDisposable
{
    private const long On20m = 14_070_000;
    private const double HisHz = 1234;
    private const string Mine = "KC3QIS";
    private const string Grid = "FN00DJ";
    private const string AnnouncedAs = "BPSK31";
    private const double WithinHz = 5.0;

    /// <summary>
    /// **Task 1's before**: today's CQ at 1000 Hz and the default drive, hashed by
    /// `Unit359Trace` before anything was built (commit 489e9c5d).
    /// </summary>
    private static readonly Dictionary<int, string> CqBefore = new()
    {
        [12_000] = "1a26b6d6bb17e0f16b26c85d4e48075c90fb1f1a6d27f4390362e4354d7ca764",
        [48_000] = "e45bc60d6d39fd2d39768a8645f6b8f362e3f15f2a38c45dfbaa7315f4f934e1",
    };

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each reading is printed.</param>
    public ThePsk31SendIsAnnouncedTests(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(Path.GetTempPath(), "hamlet-announced-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
    }

    /// <summary>Removes the telemetry folder.</summary>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch (IOException)
        {
            // A left-over temp folder is not a test failure.
        }
    }

    /// <summary>**Loopback: a composed CQ, through the detector, is BPSK31 at the send offset.**</summary>
    /// <param name="offsetHz">Where the send goes out.</param>
    /// <param name="rate">The endpoint's rate.</param>
    [Theory]
    [InlineData(1000.0, 12_000)]
    [InlineData(1437.5, 12_000)]
    [InlineData(1000.0, 48_000)]
    public void AComposedCqReadsBackAsBpsk31AtTheSendOffset(double offsetHz, int rate)
    {
        var composed = Psk31Modulator.Compose(Psk31Macros.Cq(Mine), rate, offsetHz, Ft8Composer.DefaultDrivePeak);
        var heard = Hear(composed.Samples, rate);

        _output.WriteLine(
            $"CQ at {offsetHz} Hz, {rate} Hz: {composed.Seconds:0.000} s, announced {composed.AnnouncedCode?.ToString(CultureInfo.InvariantCulture) ?? "no"}; "
            + (heard.Count == 0 ? "detected none" : string.Join("; ", heard.Select(Say))));

        Assert.Equal(Codes.CodeOf(AnnouncedAs), composed.AnnouncedCode);

        var one = Assert.Single(heard);

        Assert.Equal(Codes.CodeOf(AnnouncedAs), one.Code);
        Assert.InRange(one.CenterHz, offsetHz - WithinHz, offsetHz + WithinHz);
    }

    /// <summary>**The samples after the burst are today's CQ, to the byte, and the samples before it are the burst.**</summary>
    /// <param name="rate">The endpoint's rate.</param>
    [Theory]
    [InlineData(12_000)]
    [InlineData(48_000)]
    public void TheSamplesAfterTheBurstAreTodaysCqToTheByte(int rate)
    {
        var code = Codes.CodeOf(AnnouncedAs)!.Value;
        var composed = Psk31Modulator.Compose(Psk31Macros.Cq(Mine), rate, 1000, Ft8Composer.DefaultDrivePeak);
        var burst = RsidBurst.LengthInSamples(Codes, rate);

        Assert.True(composed.Samples.Length > burst, "the send is no longer than its burst");

        var after = Hash(composed.Samples.AsSpan(burst));

        _output.WriteLine($"{rate} Hz: {composed.Samples.Length} samples, burst {burst}, after the burst sha256 {after}");
        _output.WriteLine($"         task 1's before sha256 {CqBefore[rate]}");

        Assert.Equal(CqBefore[rate], after);
        Assert.Equal(
            RsidBurst.Samples(Codes, code, 1000, rate, Ft8Composer.DefaultDrivePeak),
            composed.Samples.AsSpan(0, burst).ToArray());
    }

    /// <summary>**The other three macros and a typed line begin with the burst too, composed in one place.**</summary>
    [Fact]
    public void TheOtherMacrosAndATypedLineBeginWithTheBurst()
    {
        const int Rate = 12_000;

        var code = Codes.CodeOf(AnnouncedAs)!.Value;
        var burst = RsidBurst.Samples(Codes, code, HisHz, Rate, Ft8Composer.DefaultDrivePeak);

        var texts = new (string Name, string Text)[]
        {
            ("answer", Psk31Macros.Answer("W1AW", Mine)),
            ("report", Psk31Macros.Report("W1AW", Mine, "Tim", "Trafford PA", Grid)),
            ("confirm", Psk31Macros.Confirm("W1AW", Mine)),
            ("typed", Psk31Macros.Typed("W1AW", Mine, "hello from the bench")),
        };

        foreach (var (name, text) in texts)
        {
            var composed = Psk31Modulator.Compose(text, Rate, HisHz, Ft8Composer.DefaultDrivePeak);
            var heard = Hear(composed.Samples.AsSpan(0, Math.Min(composed.Samples.Length, 3 * Rate)).ToArray(), Rate);

            _output.WriteLine(
                $"{name,-8} {composed.Seconds,7:0.000} s, announced {composed.AnnouncedCode?.ToString(CultureInfo.InvariantCulture) ?? "no"}; "
                + (heard.Count == 0 ? "detected none" : string.Join("; ", heard.Select(Say))));

            Assert.Equal(code, composed.AnnouncedCode);
            Assert.True(composed.Samples.Length > burst.Length, name + " is no longer than its burst");
            Assert.Equal(burst, composed.Samples.AsSpan(0, burst.Length).ToArray());
            Assert.Equal(
                Psk31Modulator.Modulate(text, Rate, HisHz, Ft8Composer.DefaultDrivePeak),
                composed.Samples.AsSpan(burst.Length).ToArray());

            var one = Assert.Single(heard);

            Assert.Equal(code, one.Code);
            Assert.InRange(one.CenterHz, HisHz - WithinHz, HisHz + WithinHz);
        }

        // **ONE COMPOSITION SITE ON THE SEND PATH**, which is what puts the burst on all five.
        var sites = CodeLines(Path.Combine("src", "Hamlet.App"), "Psk31Modulator.Compose(");

        _output.WriteLine("Psk31Modulator.Compose( code lines in the application: " + sites);

        Assert.Equal(1, sites);
    }

    /// <summary>**A CQ press sends the burst first, and the composition record and `rsid_sent` say so, with nothing personal.**</summary>
    [Fact]
    public void ACqPressSendsTheBurstFirstAndTheRecordsSaySo()
    {
        FakeSink sink;

        using (var telemetry = new JsonlTelemetry(_folder, "359", _ => true))
        {
            MainWindowViewModel model;

            (model, sink) = Panel(telemetry);

            model.SendCallToAnyoneCommand.Execute(null);
            Settle(model);

            _output.WriteLine("send line: " + model.DigitalSendLine);
        }

        var lines = Lines();
        var code = Codes.CodeOf(AnnouncedAs)!.Value;

        foreach (var line in lines.Where(l => l.Contains("rsid", StringComparison.OrdinalIgnoreCase)
                                              || l.Contains("psk31_send_composed", StringComparison.Ordinal)
                                              || l.Contains(TransmitRecord.EventName, StringComparison.Ordinal)))
        {
            _output.WriteLine(line);
        }

        Assert.Equal(1, sink.TimesCalled);

        var composed = Assert.Single(Events(lines, "psk31_send_composed"));
        var offset = composed.GetProperty("offsetHz").GetDouble();

        Assert.True(composed.GetProperty("announced").GetBoolean());
        Assert.Equal(code, composed.GetProperty("rsidCode").GetInt32());

        var record = Assert.Single(Events(lines, TransmitRecord.EventName));
        var rate = record.GetProperty("sampleRate").GetInt32();
        var heard = Hear(sink.LastSamples.AsSpan(0, Math.Min(sink.LastSamples.Length, 3 * rate)).ToArray(), rate);

        _output.WriteLine($"played {sink.LastSamples.Length} samples at {rate} Hz; " + string.Join("; ", heard.Select(Say)));

        var one = Assert.Single(heard);

        Assert.Equal(code, one.Code);
        Assert.InRange(one.CenterHz, offset - WithinHz, offset + WithinHz);

        var sent = Assert.Single(Events(lines, "rsid_sent"));

        Assert.Equal(code, sent.GetProperty("code").GetInt32());
        Assert.Equal("BPSK31", sent.GetProperty("mode").GetString());
        Assert.Equal("", sent.GetProperty("variant").GetString());
        Assert.Equal(offset, sent.GetProperty("centerHz").GetDouble(), 1);

        NothingPersonal(lines.Where(l => l.Contains("\"rsid_sent\"", StringComparison.Ordinal)));
    }

    /// <summary>**A typed line goes out announced too, and its record says so.**</summary>
    [Fact]
    public void ATypedLineGoesOutAnnounced()
    {
        FakeSink sink;
        string framed;

        using (var telemetry = new JsonlTelemetry(_folder, "359", _ => true))
        {
            MainWindowViewModel model;

            (model, sink) = Panel(telemetry);

            var card = CardFor(model);

            card.TypedText = "hello from the bench";
            framed = Psk31Macros.Typed(card.Callsign, Mine, card.TypedText);

            model.SendTypedPsk31Command.Execute(card);
            Settle(model);
        }

        var lines = Lines();
        var code = Codes.CodeOf(AnnouncedAs)!.Value;

        Assert.Equal(1, sink.TimesCalled);

        var composed = Assert.Single(Events(lines, "psk31_send_composed"));

        Assert.Equal("typed", composed.GetProperty("macro").GetString());
        Assert.True(composed.GetProperty("announced").GetBoolean());

        var rate = Assert.Single(Events(lines, TransmitRecord.EventName)).GetProperty("sampleRate").GetInt32();
        var heard = Hear(sink.LastSamples.AsSpan(0, Math.Min(sink.LastSamples.Length, 3 * rate)).ToArray(), rate);

        _output.WriteLine($"typed line played {sink.LastSamples.Length} samples at {rate} Hz; " + string.Join("; ", heard.Select(Say)));

        var one = Assert.Single(heard);

        Assert.Equal(code, one.Code);
        Assert.InRange(one.CenterHz, HisHz - WithinHz, HisHz + WithinHz);

        var sent = Assert.Single(Events(lines, "rsid_sent"));

        Assert.Equal(HisHz, sent.GetProperty("centerHz").GetDouble(), 1);

        NothingPersonal(lines.Where(l => l.Contains("\"rsid_sent\"", StringComparison.Ordinal)), "hello");
        Assert.True(sink.LastSamples.Length / (double)rate > Psk31Modulator.SecondsFor(framed));
    }

    /// <summary>
    /// **The typed line's *too long to send* measures the framed text alone, and the card and the
    /// gate agree just under sixty seconds and just over.**
    /// </summary>
    /// <remarks>
    /// <para>**REWRITTEN UNDER §R12** (work instruction 360 task 2). Unit 359 wrote this to guard
    /// its decision A, the burst inside the cap. Tim's R32 (a) of 2026-09-18 put the burst outside
    /// it, so the test now guards that rule: the line just under sixty seconds of text is over
    /// sixty with its burst, and it goes.</para>
    /// </remarks>
    [Fact]
    public void TheTypedLinesCardAndGateAgreeOnTheTextAlone()
    {
        var (model, sink) = Panel();
        var card = CardFor(model);
        var burstSeconds = RsidBurst.Seconds(Codes);

        // **THE LONGEST LINE WHOSE TEXT FITS, AND ONE CHARACTER MORE**, found by arithmetic.
        var n = 1;

        while (Psk31Modulator.SecondsFor(Psk31Macros.Typed(card.Callsign, Mine, new string('e', n + 1)))
               <= MainWindowViewModel.LongestTypedSeconds)
        {
            n++;
        }

        var under = Psk31Modulator.SecondsFor(Psk31Macros.Typed(card.Callsign, Mine, new string('e', n)));
        var over = Psk31Modulator.SecondsFor(Psk31Macros.Typed(card.Callsign, Mine, new string('e', n + 1)));

        _output.WriteLine(
            $"{n} characters: text {under:0.000} s, with the burst {under + burstSeconds:0.000} s; "
            + $"{n + 1} characters: text {over:0.000} s");

        Assert.True(under <= MainWindowViewModel.LongestTypedSeconds);
        Assert.True(under + burstSeconds > MainWindowViewModel.LongestTypedSeconds, "the burst would not have tipped it over");
        Assert.True(over > MainWindowViewModel.LongestTypedSeconds);

        // **JUST UNDER: THE CARD DOES NOT SAY TOO LONG, AND IT GOES.**
        card.TypedText = new string('e', n);

        _output.WriteLine("under, card says: " + card.TypedSecondsWord);

        Assert.DoesNotContain("too long to send", card.TypedSecondsWord, StringComparison.Ordinal);

        model.SendTypedPsk31Command.Execute(card);
        Settle(model);

        _output.WriteLine("under, note     : " + card.TypedNote + " | " + model.DigitalSendLine);

        Assert.Equal(1, sink.TimesCalled);
        Assert.DoesNotContain("Too long", card.TypedNote, StringComparison.Ordinal);

        // **JUST OVER: THE CARD SAYS SO, AND THE PRESS IS REFUSED.**
        card.TypedText = new string('e', n + 1);

        _output.WriteLine("over, card says : " + card.TypedSecondsWord);

        Assert.Contains("too long to send", card.TypedSecondsWord, StringComparison.Ordinal);

        model.SendTypedPsk31Command.Execute(card);
        Settle(model);

        _output.WriteLine("over, note      : " + card.TypedNote);

        Assert.Equal(1, sink.TimesCalled);
        Assert.Contains("Too long to send", card.TypedNote, StringComparison.Ordinal);
    }

    /// <summary>**The keying and arming sites are the task 1 trace's: one `PttOn` write, two `Arm(` lines.**</summary>
    [Fact]
    public void TheKeyingAndArmingSitesAreUnchanged()
    {
        var pttOn = CodeLines("src", "CivConstants.PttOn");
        var arms = CodeLines("src", "_armedSend.Arm(");

        _output.WriteLine($"CivConstants.PttOn code lines {pttOn}, _armedSend.Arm( code lines {arms}");

        Assert.Equal(1, pttOn);
        Assert.Equal(2, arms);
    }

    private static RsidCodes Codes
        => OliviaData.Current.Rsid ?? throw new InvalidOperationException(OliviaData.Current.Problem);

    private static IReadOnlyList<RsidDetection> Hear(float[] samples, int rate)
        => RsidDetector.Detect(
            Codes, new MonoAudio(rate, samples), Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz);

    private static string Say(RsidDetection d)
        => $"{d.Name} ({d.Code}) at {d.CenterHz:0.00} Hz, tones right {d.TonesRight}";

    private static string Hash(ReadOnlySpan<float> samples)
        => Convert.ToHexString(SHA256.HashData(MemoryMarshal.AsBytes(samples))).ToLowerInvariant();

    /// <summary>No callsign, grid or word of the text in the lines.</summary>
    private static void NothingPersonal(IEnumerable<string> lines, params string[] more)
    {
        var list = lines.ToList();

        Assert.NotEmpty(list);

        foreach (var personal in new[] { Mine, Grid, "EI4GNB", "W1AW" }.Concat(more))
        {
            Assert.All(list, l => Assert.DoesNotContain(personal, l, StringComparison.OrdinalIgnoreCase));
        }
    }

    private List<string> Lines()
        => Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines).ToList();

    private static List<JsonElement> Events(IEnumerable<string> lines, string name)
        => lines
            .Select(line => JsonDocument.Parse(line).RootElement)
            .Where(e => e.GetProperty("event").GetString() == name)
            .Select(e => e.GetProperty("data"))
            .ToList();

    private static int CodeLines(string under, string needle)
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        var separator = Path.DirectorySeparatorChar.ToString();

        return Directory.EnumerateFiles(Path.Combine(at!.FullName, under), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains(separator + "obj" + separator, StringComparison.Ordinal)
                        && !f.Contains(separator + "bin" + separator, StringComparison.Ordinal))
            .SelectMany(File.ReadAllLines)
            .Count(l => l.Contains(needle, StringComparison.Ordinal)
                        && !l.TrimStart().StartsWith("//", StringComparison.Ordinal)
                        && !l.TrimStart().StartsWith("*", StringComparison.Ordinal));
    }

    /// <summary>The station's card, opened the way a right-click opens it.</summary>
    private static Ft8ContactCard CardFor(MainWindowViewModel model)
    {
        var corpus = Psk31Corpus.Load();

        var his = string.Concat(corpus.Transcripts
            .Single(t => t.Name == "04-not-for-me")
            .Lines
            .Where(l => !string.Equals(l.Frm, corpus.Operator, StringComparison.OrdinalIgnoreCase))
            .Take(2)
            .Select(l => l.Text + "\n"));

        model.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(1, HisHz, 10.0, his) });

        var row = model.DigitalDecodes.Single();

        model.OpenPsk31CardCommand.Execute(row);

        return model.DigitalCards.Single(c => c.Callsign == "EI4GNB");
    }

    /// <summary>Lets a no-slot send finish; the click fires it and does not wait (§R10).</summary>
    private static void Settle(MainWindowViewModel model)
    {
        for (var tries = 0; tries < 400 && model.HasSomethingToStop; tries++)
        {
            System.Threading.Thread.Sleep(10);
        }

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
    }

    private static (MainWindowViewModel Model, FakeSink Sink) Panel(JsonlTelemetry? telemetry = null)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = Grid;
        settings.Operator.OperatorName = "Tim";
        settings.Operator.Location = "Trafford PA";
        settings.Operator.LicenseClass = LicenseClass.General;

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= On20m && b.Band.HighHz >= On20m);
        model.FrequencyHz = On20m;

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute("PSK31");

        var port = new FakePort();
        var sink = new FakeSink();

        model.UseRigPortForTests(port);
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, guard: null, telemetry)));

        return (model, sink);
    }
}
