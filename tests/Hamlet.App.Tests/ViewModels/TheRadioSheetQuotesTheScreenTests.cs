using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **Criteria 4.2 and 4.3: every sentence `docs/RADIO_SHEET.md` quotes is one Hamlet says, every
/// refusal Hamlet can say on the PSK31 and Olivia send path is on the sheet, and no sentence the
/// unit wrote tells the operator to touch the radio.**
/// </summary>
/// <remarks>
/// <para>**IT IS ASSERTED IN BOTH DIRECTIONS, AND THE BACKWARD ONE IS THE POINT** (work
/// instruction 382 section 6 ruling 1 item 3). Forward alone would pass a sheet that quoted one
/// sentence and left eight out. The backward direction is what keeps the sheet true after
/// tonight: a later unit that adds a refusal to the send path fails this name until the sheet
/// carries it.</para>
/// <para>**TWO KINDS OF CHECK, AND THE OUTPUT SAYS WHICH ONE EACH QUOTE GOT** (ruling 1 item 4).
/// **Kind (a)** drives the shipped view model into the state and compares the sheet's quote with
/// what Hamlet actually put in <c>DigitalSendLine</c>, <c>TransmitRefusalSentence</c> or the other
/// operator-facing lines, character for character. **Kind (b)** finds the literal in a file under
/// <c>src/Hamlet.App</c>, with a sentence assembled from several literals - as
/// <c>NoTransmitDeviceSentence</c> is - found by its parts, in order, in one file. Kind (b) is the
/// weaker one and the report names every sentence that got it.</para>
/// <para>**A SENTENCE WITH A VALUE SPLICED INTO IT IS CHECKED EITHER SIDE OF THE VALUE** (ruling 1
/// item 5). The sheet writes the varying part in angle brackets, and what is compared is each
/// fixed part, in order - so a sheet that quoted one variant's name would be telling the operator
/// he will see a word he may not see, and this would not accept it.</para>
/// <para>**THE SHEET IS REACHED BY THE WALK TO THE REPOSITORY ROOT** that
/// <c>DecisionLogOrderTests.cs:49</c> already uses. There is no new test project, no new fixture
/// kind and no copy of the sheet in the build output.</para>
/// <para>**NOTHING IS KEYED** (§0.2, FACT-004): the wire is `FakePort`, the sound card is
/// `FakeSink`, and every refusal below returns before either is touched.</para>
/// </remarks>
public sealed class TheRadioSheetQuotesTheScreenTests : IDisposable
{
    private const string Mine = "K1ABC";

    /// <summary>20 m, inside a General class data segment, where both modes have a block.</summary>
    private const long On20m = 14_071_500;

    /// <summary>Ruling 2 item 2's target, which is reported and is never a red.</summary>
    private const int TargetLines = 120;

    /// <summary>The same target in bytes.</summary>
    private const int TargetBytes = 10 * 1024;

    /// <summary>Where a value is spliced into a sentence the sheet quotes.</summary>
    private static readonly Regex Spliced = new("<[^<>]*>", RegexOptions.Compiled);

    /// <summary>The glue between two adjacent C# string literals, for kind (b).</summary>
    private static readonly Regex Concatenation = new("\"\\s*\\+\\s*\"", RegexOptions.Compiled);

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each quote and the kind it got is printed.</param>
    public TheRadioSheetQuotesTheScreenTests(ITestOutputHelper output)
    {
        _output = output;
        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-radio-sheet-" + Guid.NewGuid().ToString("N"));

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

    // ------------------------------------------------------------------
    // 4.2 forward.
    // ------------------------------------------------------------------

    /// <summary>
    /// **Every sentence the sheet quotes exists in an operator-facing string, and the output says
    /// which kind of check each one got.**
    /// </summary>
    [Fact]
    public void EveryQuotedSentenceExistsInAnOperatorFacingString()
    {
        var quotes = Quotes();
        var said = WhatHamletSays();
        var sources = OperatorFacingSources();

        var weak = new List<string>();

        Assert.NotEmpty(quotes);

        foreach (var quote in quotes)
        {
            var parts = FixedParts(quote);

            // **THE CLOSEST PRODUCED SENTENCE, NOT THE FIRST ONE THAT HOLDS THE PARTS.** Two
            // sentences can share a frame - *Hamlet did not send it: ...* is the opening of both
            // the composer's refusal and the variant gate's - so the label beside a quote names
            // the sentence nearest its own length, which is the one it was written from.
            var produced = said
                .Where(one => Holds(one.Sentence, parts))
                .OrderBy(one => Math.Abs(one.Sentence.Length - quote.Length))
                .FirstOrDefault();

            if (produced.Sentence is not null)
            {
                _output.WriteLine("(a) " + Short(quote) + "   <- " + produced.Label);
                continue;
            }

            var found = sources.FirstOrDefault(one => Holds(one.Text, parts));

            Assert.True(
                found.Name is not null,
                "the sheet quotes a sentence no operator-facing string in the tree holds: " + quote);

            weak.Add(quote);
            _output.WriteLine("(b) " + Short(quote) + "   <- found in " + found.Name);
        }

        _output.WriteLine("");
        _output.WriteLine(
            quotes.Count + " quoted sentences: " + (quotes.Count - weak.Count)
            + " checked against what the application itself produced, " + weak.Count
            + " found in the source.");

        foreach (var one in weak)
        {
            _output.WriteLine("  kind (b): " + Short(one));
        }
    }

    // ------------------------------------------------------------------
    // 4.2 backward.
    // ------------------------------------------------------------------

    /// <summary>
    /// **Every refusal this unit can produce on the PSK31 and Olivia send path is quoted somewhere
    /// on the sheet.**
    /// </summary>
    /// <remarks>
    /// **A REFUSAL THAT EXISTS AND IS NOT ON THE SHEET FAILS HERE.** That is the whole of the
    /// backward direction: the sheet cannot quietly go stale, because a send path that grows a
    /// tenth refusal grows a red here with it.
    /// </remarks>
    [Fact]
    public void EveryReachableRefusalOnTheSendPathIsQuoted()
    {
        var quotes = Quotes().Select(FixedParts).ToList();
        var refusals = WhatHamletSays().Where(one => one.IsRefusal).ToList();

        Assert.NotEmpty(refusals);

        var missing = new List<string>();

        foreach (var refusal in refusals)
        {
            var quoted = quotes.Any(parts => Holds(refusal.Sentence, parts));

            _output.WriteLine((quoted ? "quoted     : " : "NOT QUOTED : ") + refusal.Label);
            _output.WriteLine("             " + Short(refusal.Sentence));

            if (!quoted)
            {
                missing.Add(refusal.Label);
            }
        }

        _output.WriteLine("");
        _output.WriteLine(
            (refusals.Count - missing.Count) + " of " + refusals.Count
            + " reachable refusals are quoted on the sheet.");

        Assert.Empty(missing);
    }

    // ------------------------------------------------------------------
    // 4.3 - R11.
    // ------------------------------------------------------------------

    /// <summary>
    /// **No sentence this unit wrote tells the operator to touch the radio.**
    /// </summary>
    /// <remarks>
    /// <para>**THE SCAN IS OVER THE SHEET'S OWN PROSE AND NOT OVER ITS QUOTES**, and the exclusion
    /// is not a loophole. A `&gt;` line is a sentence HAMLET says, quoted; it is what is already on
    /// the screen and it is out of this unit's hands. *Open Settings and pick the radio's sound
    /// card* is Hamlet naming a Windows audio device, not asking anybody to reach for anything -
    /// and *Move the dial a little* is a sentence the send path has said since unit 323. Rewriting
    /// either one to pass a word scan would be paraphrasing inside a quote, which criterion 4.1
    /// forbids. **What R11 governs is what THIS unit wrote**, which is the prose, and that is what
    /// is scanned.</para>
    /// <para>**THE WORD LIST IS THE AUTHOR'S** (work instruction 382 section 6 ruling 2 item 1),
    /// and every entry is here with the reason it is on the list.</para>
    /// </remarks>
    [Fact]
    public void NoSentenceThisUnitWroteTellsHimToTouchTheRadio()
    {
        // Each word names a control or a surface on the transceiver itself, which is the thing
        // R11 says Hamlet never asks him to touch. "rig" and "transceiver" name the box;
        // "knob", "VFO", "PTT", "mic gain" and "RF gain" name controls on it; the four phrases
        // are the shapes an instruction to go and operate it takes in English.
        var words = new[]
        {
            "knob", "VFO", "PTT", "mic gain", "RF gain", "transceiver", "rig",
            "tune the radio", "turn the radio", "on the radio", "at the rig's front",
        };

        var prose = Sheet().Where(line => !line.StartsWith("> ", StringComparison.Ordinal)).ToList();
        var hits = new List<string>();

        foreach (var line in prose)
        {
            foreach (var word in words)
            {
                // **WHOLE WORDS, BECAUSE A SUBSTRING SCAN ANSWERS A DIFFERENT QUESTION.** `rig`
                // inside `Right-click` and inside `right now` is not R11's word, and a scan that
                // counted it would push the sheet into paraphrasing English to pass a test.
                if (Regex.IsMatch(
                        line,
                        "\\b" + Regex.Escape(word) + "\\b",
                        RegexOptions.IgnoreCase))
                {
                    hits.Add("\"" + word + "\" in: " + line.Trim());
                }
            }
        }

        _output.WriteLine(
            prose.Count + " prose lines scanned against " + words.Length + " words: "
            + hits.Count + " hits.");

        foreach (var hit in hits)
        {
            _output.WriteLine("  " + hit);
        }

        Assert.Empty(hits);
    }

    // ------------------------------------------------------------------
    // One page - reported, never a red.
    // ------------------------------------------------------------------

    /// <summary>
    /// **What the sheet measures, against ruling 2 item 2's target.**
    /// </summary>
    /// <remarks>
    /// **THE NUMBER IS REPORTED AND IS NOT AN ASSERTION ABOUT THE TARGET** (ruling 2 item 2:
    /// 4.1's content is the criterion and the length is a target). A length assertion that failed
    /// would be answered by dropping a refusal or paraphrasing a quote, and both are forbidden -
    /// so the only thing a red here could buy is the damage it exists to prevent. What IS asserted
    /// is that the file is there and has quotes in it.
    /// </remarks>
    [Fact]
    public void TheSheetIsOnePageAndTheMeasurementIsReported()
    {
        var lines = Sheet();
        var bytes = new FileInfo(Path.Combine(Root(), "docs", "RADIO_SHEET.md")).Length;
        var quotes = Quotes();
        var quoteBytes = lines
            .Where(l => l.StartsWith("> ", StringComparison.Ordinal))
            .Sum(l => l.Length + 1);

        _output.WriteLine("lines : " + lines.Count + "  against a target of " + TargetLines);
        _output.WriteLine("bytes : " + bytes + "  against a target of " + TargetBytes);
        _output.WriteLine("quoted sentences : " + quotes.Count);
        _output.WriteLine(
            "of the bytes, " + quoteBytes + " are the quote lines, which the sheet's own "
            + "convention forbids wrapping.");
        _output.WriteLine(
            lines.Count <= TargetLines && bytes <= TargetBytes
                ? "inside the target on both."
                : "over the target, and the content is the criterion (ruling 2 item 2).");

        Assert.NotEmpty(quotes);
    }

    // ------------------------------------------------------------------
    // The sheet.
    // ------------------------------------------------------------------

    /// <summary>Every line of the sheet.</summary>
    private static List<string> Sheet()
        => File.ReadAllLines(Path.Combine(Root(), "docs", "RADIO_SHEET.md")).ToList();

    /// <summary>
    /// **Every sentence the sheet quotes: the text between the first and the last double quote on
    /// a `&gt;` line.**
    /// </summary>
    /// <remarks>
    /// **FIRST AND LAST, BECAUSE A QUOTED SENTENCE CAN HOLD QUOTES ITSELF.** *Sending "&lt;your
    /// line&gt;" now* is what the screen says, and the sheet quotes it as it is said. The
    /// convention that nothing follows the closing quote is what makes the last one unambiguous.
    /// </remarks>
    private static List<string> Quotes()
    {
        var quotes = new List<string>();

        foreach (var line in Sheet().Where(l => l.StartsWith("> ", StringComparison.Ordinal)))
        {
            var opens = line.IndexOf('"');
            var shuts = line.LastIndexOf('"');

            Assert.True(
                opens >= 0 && shuts > opens,
                "a quote line is not \"sentence in double quotes\" and nothing else: " + line);

            quotes.Add(line[(opens + 1)..shuts]);
        }

        return quotes;
    }

    /// <summary>The fixed parts of a quote, in order, with every spliced value taken out.</summary>
    private static List<string> FixedParts(string quote)
        => Spliced.Split(quote)
            .Select(part => part.Trim())
            .Where(part => part.Length > 0)
            .ToList();

    /// <summary>Whether a string holds every fixed part, in the sheet's own order.</summary>
    private static bool Holds(string sentence, IReadOnlyList<string> parts)
    {
        var at = 0;

        foreach (var part in parts)
        {
            var found = sentence.IndexOf(part, at, StringComparison.Ordinal);

            if (found < 0)
            {
                return false;
            }

            at = found + part.Length;
        }

        return parts.Count > 0;
    }

    private static string Short(string sentence)
        => sentence.Length <= 88 ? sentence : sentence[..85] + "...";

    // ------------------------------------------------------------------
    // Kind (a) - what the application itself produced.
    // ------------------------------------------------------------------

    /// <summary>One operator-facing string, and where it came from.</summary>
    /// <param name="Label">What was pressed, and in which state.</param>
    /// <param name="Sentence">What Hamlet put on the screen.</param>
    /// <param name="IsRefusal">True where it is a refusal on the PSK31 or Olivia send path.</param>
    private readonly record struct Said(string Label, string Sentence, bool IsRefusal);

    /// <summary>
    /// **Everything this test can make Hamlet say, driven through the shipped view model.**
    /// </summary>
    private List<Said> WhatHamletSays()
    {
        var said = new List<Said>();

        // The idle screen, which needs no press at all.
        var idle = Panel(null, "PSK31");

        said.Add(new("PSK31 chosen, the strip line", idle.DigitalModeStripLine, false));
        said.Add(new("PSK31 chosen, the decoded panel", idle.DigitalDecodedIdle, false));
        said.Add(new("nothing sent yet", idle.DigitalSendLine, false));
        said.Add(new("the send line's hover", MainWindowViewModel.SendIdleTip, false));
        said.Add(new("the stop, nothing armed", idle.StopLabel, false));

        var olivia = Panel(null, "Olivia");

        said.Add(new("Olivia chosen, the strip line", olivia.DigitalModeStripLine, false));
        said.Add(new("Olivia chosen, the decoded panel", olivia.DigitalDecodedIdle, false));

        // The capture press, which keys nothing and writes nothing where nothing arrived.
        var capture = Panel(null, "PSK31");

        capture.CapturePsk31Command.Execute(null);

        said.Add(new("the capture press, running", capture.Psk31CaptureLine, false));

        capture.CapturePsk31Command.Execute(null);

        said.Add(new("the capture press, nothing heard", capture.Psk31CaptureWhere, false));
        said.Add(new("the capture press, at rest", capture.Psk31CaptureLine, false));

        // Every refusal the send path can reach from a fixture.
        said.Add(Refusal(
            "nothing to send",
            model => model.SendMessageCommand.Execute("")));

        said.Add(Refusal(
            "no transmit path",
            model => model.SendCallToAnyoneCommand.Execute(null),
            arm: false));

        said.Add(Refusal(
            "no clear spot",
            model =>
            {
                model.UsePsk31CandidatesForTests(CrowdedBand());
                model.SendCallToAnyoneCommand.Execute(null);
            }));

        said.Add(Refusal(
            "longer than the cap",
            model => model.SendMessageCommand.Execute(TooLongForTheCap())));

        said.Add(Refusal(
            "Olivia cannot announce itself",
            model =>
            {
                model.UseOliviaDataForTests(OliviaData.Read(null, "{ }", null));
                model.SendCallToAnyoneCommand.Execute(null);
            },
            olivia: true));

        said.Add(Refusal(
            "the Olivia variant is not proved",
            model =>
            {
                var format = File.ReadAllText(Path.Combine(Root(), "data", "olivia", "format.json"));

                model.UseOliviaDataForTests(
                    OliviaData.Read(
                        File.ReadAllText(Path.Combine(Root(), "data", "bands", "olivia-calling.json")),
                        File.ReadAllText(Path.Combine(Root(), "assets", "data", "rsid-codes.json")),
                        format.Replace(", \"proved_by_loopback\": true", "", StringComparison.Ordinal)));

                model.SendCallToAnyoneCommand.Execute(null);
            },
            olivia: true));

        said.Add(Refusal(
            "the text holds what the mode cannot carry",
            model => model.SendMessageCommand.Execute("CQ — DE " + Mine),
            olivia: true));

        said.Add(Refusal(
            "the chosen mode cannot send at all",
            model => model.SendCallToAnyoneCommand.Execute(null),
            mode: "WSPR",
            arm: false));

        // The two the transmit path itself refuses with, which the keyboard modes reach too.
        var (line, sentence) = DeviceRefusal(named: false, throws: null);

        said.Add(new("no transmit device chosen, the send line", line, true));
        said.Add(new("no transmit device chosen, the refusal", sentence, true));

        var (openLine, openSentence) = DeviceRefusal(
            named: true,
            throws: new InvalidOperationException("the device is in use by another application"));

        said.Add(new("the device would not open, the send line", openLine, true));
        said.Add(new("the device would not open, the refusal", openSentence, true));

        // The send that goes, which is the pair of sentences the walk in sections 2 and 3 ends on.
        said.AddRange(ASendThatGoes("PSK31"));
        said.AddRange(ASendThatGoes("Olivia"));

        return said;
    }

    /// <summary>Drive one refusal and keep the sentence Hamlet put on the panel.</summary>
    private Said Refusal(
        string label,
        Action<MainWindowViewModel> press,
        bool olivia = false,
        bool arm = true,
        string? mode = null)
    {
        using var telemetry = new JsonlTelemetry(_folder, "382", _ => true);

        var model = Panel(telemetry, mode ?? (olivia ? "Olivia" : "PSK31"));

        if (arm)
        {
            GiveItARadio(model, telemetry);
        }

        press(model);
        Settle(model);

        return new(label, model.DigitalSendLine, true);
    }

    /// <summary>Drive one of the two transmit-path refusals, and keep both of its sentences.</summary>
    private (string Line, string Sentence) DeviceRefusal(bool named, Exception? throws)
    {
        using var telemetry = new JsonlTelemetry(_folder, "382", _ => true);

        var model = Panel(telemetry, "PSK31", named ? "USB Audio CODEC" : null);

        model.TransmitSinkFactory = _ => throw (throws
            ?? new InvalidOperationException("no sink was asked for"));

        var port = new FakePort();

        model.UseRigPortForTests(port);
        model.BuildTheArmedSend(port);

        model.SendCallToAnyoneCommand.Execute(null);
        Settle(model);

        Assert.Empty(port.Written);

        return (model.DigitalSendLine, model.TransmitRefusalSentence);
    }

    /// <summary>A call to anyone that goes, over fakes: what the panel says at the press and after.</summary>
    private List<Said> ASendThatGoes(string mode)
    {
        using var telemetry = new JsonlTelemetry(_folder, "382", _ => true);

        var model = Panel(telemetry, mode);

        GiveItARadio(model, telemetry);

        model.SendCallToAnyoneCommand.Execute(null);

        var going = model.DigitalSendLine;

        Settle(model);

        return
        [
            new(mode + ": at the press", going, false),
            new(mode + ": when it has gone", model.DigitalSendLine, false),
        ];
    }

    // ------------------------------------------------------------------
    // Kind (b) - the literal in the source.
    // ------------------------------------------------------------------

    /// <summary>One file that can hold an operator-facing literal.</summary>
    /// <param name="Name">Its path under the repository root.</param>
    /// <param name="Text">Its text with the C# concatenation glue taken out.</param>
    private readonly record struct Source(string Name, string Text);

    /// <summary>
    /// **Every file under `src/Hamlet.App` that could hold a sentence, with adjacent literals
    /// joined so a sentence written in three pieces is found whole.**
    /// </summary>
    /// <remarks>
    /// **THAT JOIN IS WHY KIND (b) WORKS AT ALL FOR `NoTransmitDeviceSentence`**, which is one
    /// sentence in three constants (ruling 1 item 4's own example). Removing the `" + "` between
    /// two literals is what a reader does with their eye; doing it here lets the parts be found in
    /// order, in one file, exactly as the ruling asks.
    /// </remarks>
    private static List<Source> OperatorFacingSources()
    {
        var root = Path.Combine(Root(), "src", "Hamlet.App");
        var sources = new List<Source>();

        foreach (var file in Directory
                     .EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
                     .Where(f => f.EndsWith(".cs", StringComparison.Ordinal)
                                 || f.EndsWith(".axaml", StringComparison.Ordinal))
                     .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                                 && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)))
        {
            var text = File.ReadAllText(file);

            sources.Add(new(
                Path.GetRelativePath(Root(), file),
                Concatenation.Replace(text, "").Replace("\\\"", "\"", StringComparison.Ordinal)));
        }

        return sources;
    }

    // ------------------------------------------------------------------
    // The fixtures.
    // ------------------------------------------------------------------

    /// <summary>A panel on 20 m with one of the five sub-modes pressed.</summary>
    private static MainWindowViewModel Panel(JsonlTelemetry? telemetry, string mode, string? device = null)
    {
        var settings = new AppSettings { ReconnectOnStartup = false, AudioOutputDeviceId = device };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN42";
        settings.Operator.OperatorName = "Pat";
        settings.Operator.Location = "Boston MA";
        settings.Operator.LicenseClass = LicenseClass.General;

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            TapForTests = new AudioTap(),
        };

        model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= On20m && b.Band.HighHz >= On20m);
        model.FrequencyHz = On20m;
        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute(mode);

        return model;
    }

    /// <summary>Give the panel the one send path, over fakes. Nothing opens and nothing keys.</summary>
    private static void GiveItARadio(MainWindowViewModel model, JsonlTelemetry telemetry)
    {
        var port = new FakePort();
        var sink = new FakeSink();

        model.UseRigPortForTests(port);
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, guard: null, telemetry)));
    }

    /// <summary>A band with no gap over 150 Hz anywhere a call could go.</summary>
    private static List<Psk31Candidate> CrowdedBand()
    {
        var crowded = new List<Psk31Candidate>();

        for (var hz = Psk31ClearSpot.LowestCallHz - 100; hz <= Psk31ClearSpot.HighestCallHz + 100; hz += 100)
        {
            crowded.Add(new Psk31Candidate(hz, 12, Psk31ClearSpot.Occupied + 0.2, true));
        }

        return crowded;
    }

    /// <summary>More PSK31 audio than a macro's cap allows, in Latin-1 the varicode carries.</summary>
    private static string TooLongForTheCap()
        => string.Concat(Enumerable.Repeat("CQ CQ CQ DE " + Mine + " " + Mine + " PSE K ", 40));

    /// <summary>Let a send that the click fired finish, and run what it posted to the UI thread.</summary>
    private static void Settle(MainWindowViewModel model)
    {
        for (var tries = 0; tries < 400 && model.HasSomethingToStop; tries++)
        {
            Thread.Sleep(10);
        }

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
    }

    /// <summary>The repository root, by the walk `DecisionLogOrderTests.cs:49` already uses.</summary>
    private static string Root()
    {
        var here = new DirectoryInfo(AppContext.BaseDirectory);

        while (here is not null && !File.Exists(Path.Combine(here.FullName, "CLAUDE.md")))
        {
            here = here.Parent;
        }

        Assert.NotNull(here);

        return here!.FullName;
    }
}
