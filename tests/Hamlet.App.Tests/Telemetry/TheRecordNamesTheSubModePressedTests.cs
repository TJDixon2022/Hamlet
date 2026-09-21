using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Headless.XUnit;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Tests.Psk31;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// Work instruction 374 task 3, criterion 2.1: **`cq_pressed` and every send event carry the
/// sub-mode the press was made under - PSK31, Olivia, FT8, FT4 - never a mapped family.**
/// </summary>
/// <remarks>
/// <para>**THE FAULT, AND IT WAS TIM'S OWN TWO EVENINGS** (§R35, seen 2026-09-11 and 2026-09-14).
/// `cq_pressed` wrote `detail: Ft8` while PSK31 was the mode he had pressed. The cause is that the
/// four operator-action sites wrote `_digitalMode.ToString()`, and `DigitalMode` has two members:
/// `DigitalModeFor` answers `Ft4` for the string `FT4` and `Ft8` for everything else - PSK31 and
/// Olivia with it. That mapping is correct for a grid, a cutter and a decoder, and wrong as a name
/// for what somebody pressed. **Measured before it was repaired**
/// (`Unit374TraceTests.WhatTheRecordSaysUnderEachLabel`): PSK31 wrote `Ft8`, Olivia wrote `Ft8`,
/// FT8 wrote `Ft8` and FT4 wrote `Ft4`, and the other three actions wrote the same.</para>
/// <para>**WHAT IS ASSERTED, AND WHY IT IS ONE TYPE AND NOT FOUR** (§R14). One rule is on trial -
/// the record names the sub-mode the press was made under - and it is the same rule at all four
/// sites, so it is one type with the sites as its rows. The rule is asserted from a fixture: a real
/// view model on a real chip press, its telemetry read back off the file it wrote.</para>
/// <para>**AND THE SEND EVENTS WERE MEASURED RATHER THAN EDITED** (work instruction 374 §6's second
/// ruling). Under PSK31 and Olivia the press leaves by `SendUnslotted`, above the three slotted
/// writes, and the unslotted path already records `Psk31` and `Olivia` by name; the slotted sites
/// are reached only under FT8 and FT4, where the family **is** the sub-mode.
/// <see cref="TheSendPathNamesTheModeTheSendWasMadeIn"/> is what holds that true, so a later change
/// that routed PSK31 down the slotted path would fail here rather than quietly write `Ft8` again.
/// </para>
/// <para>**NOTHING PERSONAL** (HM-DEC-018 §2.1). Every name here sweeps the whole file for the
/// operator's callsign, the other station's callsign and the words that were typed, and finds none
/// of them.</para>
/// <para>**NOTHING REACHES A DEVICE AND NOTHING IS ON THE AIR** (CLAUDE.md §0.2). The send path
/// ends at a `FakePort` and a `FakeSink`, which is where it ends in every app test; no sample of
/// composed audio, no `Arm` site and no `PttOn` site is touched by what this unit changed, which is
/// a string written into a record.</para>
/// </remarks>
public sealed class TheRecordNamesTheSubModePressedTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the record is printed.</param>
    public TheRecordNamesTheSubModePressedTests(ITestOutputHelper output) => _output = output;

    /// <summary>The strip's four labels, which is what criterion 2.1 means by the sub-mode.</summary>
    public static TheoryData<string> Labels => new() { "PSK31", "Olivia", "FT8", "FT4" };

    /// <summary>The station whose lines are fed in, and the words typed at him.</summary>
    private const string TypedAtHim = "tnx fer the call";

    private const long On20m = 14_070_000;
    private const double HisHz = 1234;

    /// <summary>
    /// **With each label chosen, the CQ press writes that label in `detail`** - and never `Ft8`
    /// for a mode that is not FT8.
    /// </summary>
    /// <param name="label">One of the strip's four labels.</param>
    [AvaloniaTheory]
    [MemberData(nameof(Labels))]
    public void TheCqPressWritesTheLabelTheOperatorPressed(string label)
    {
        var lines = Session(label, model => model.SendCallToAnyoneCommand.Execute(null));
        var press = Assert.Single(lines, l => Action(l) == "cq_pressed");

        _output.WriteLine(label + ": " + press);

        Assert.Equal(label, Field(press, "detail"));

        // **AND THE MODE FIELD IS STILL THE OPERATING MODE**, which is what it has always been and
        // is a different question from which sub-mode the press was made under.
        Assert.Equal("Digital", Field(press, "mode"));

        NothingPersonal(lines);
    }

    /// <summary>
    /// **`Ft8` never appears as the detail of an operator action under a mode that is not FT8.**
    /// </summary>
    /// <remarks>
    /// This is the fault's own shape rather than its repair: `Ft8` in `detail` under PSK31 is what
    /// was on Tim's two evenings, and the two-member enum is still in the file for the grid, the
    /// cutter and the decoder to use, so the way back to it is one careless line.
    /// </remarks>
    /// <param name="label">One of the strip's four labels.</param>
    [AvaloniaTheory]
    [MemberData(nameof(Labels))]
    public void NoOperatorActionNamesTheMappedFamilyUnderAModeThatIsNotFt8(string label)
    {
        var lines = Session(label, model =>
        {
            model.SendCallToAnyoneCommand.Execute(null);

            Hear(model);

            if (model.DigitalDecodes.FirstOrDefault() is not { } row)
            {
                return;
            }

            model.AnswerPsk31Command.Execute(row);
            model.OpenPsk31CardCommand.Execute(row);

            if (model.DigitalCards.FirstOrDefault() is not { } card)
            {
                return;
            }

            card.TypedText = TypedAtHim;
            model.SendTypedPsk31Command.Execute(card);
        });

        var actions = lines.Where(l => Action(l).Length > 0).ToList();

        Assert.NotEmpty(actions);

        foreach (var line in actions)
        {
            var action = Action(line);
            var detail = Field(line, "detail");

            _output.WriteLine("  " + label.PadRight(8) + action.PadRight(24) + "detail=" + detail);

            // **THE FOUR SITES CRITERION 2.1 NAMES**, each of which must say what he pressed. The
            // other actions on this path - `send_requested`, `stop_pressed` - carry a character
            // count or a reason in `detail` and are not sub-mode fields at all.
            if (action is "cq_pressed" or "psk31_answer_pressed"
                or "psk31_typed_pressed" or "psk31_card_opened")
            {
                Assert.Equal(label, detail);
            }

            if (!string.Equals(label, "FT8", StringComparison.Ordinal))
            {
                Assert.NotEqual("Ft8", detail);
            }
        }

        NothingPersonal(lines);
    }

    /// <summary>
    /// **Every send event carries the mode the send was made in**, measured per path rather than
    /// asserted from a reading of the file.
    /// </summary>
    /// <remarks>
    /// **THE SLOTTED SITES ARE UNREACHABLE HERE AND THAT IS THE ASSERTION.** `SendMessage` sends
    /// PSK31 and Olivia down `SendUnslotted` and returns, above the three writes that carry
    /// `_digitalMode.ToString()` - so under these two labels the record's send stages come from the
    /// unslotted path, which names the mode itself. If a later change routed either mode down the
    /// slotted path, `detail` would read `Ft8` and this name would say so.
    /// </remarks>
    /// <param name="label">PSK31 or Olivia - the two labels that map to the wrong family.</param>
    [AvaloniaTheory]
    [InlineData("PSK31", "Psk31")]
    [InlineData("Olivia", "Olivia")]
    public void TheSendPathNamesTheModeTheSendWasMadeIn(string label, string composedAs)
    {
        var lines = Session(label, model =>
        {
            model.SendCallToAnyoneCommand.Execute(null);
            Settle(model);
        });

        var stages = lines
            .Where(l => Field(l, "event") == "send_stage")
            .ToList();

        foreach (var line in stages)
        {
            _output.WriteLine(
                "  " + label.PadRight(8) + "stage=" + Field(line, "stage")
                + "  detail=" + Field(line, "detail"));
        }

        var composed = Assert.Single(stages, l => Field(l, "stage") == "composed");

        // **WHAT WAS COMPOSED WAS COMPOSED IN THE MODE HE PRESSED**, not in the family it maps to.
        Assert.Equal(composedAs, Field(composed, "detail"));
        Assert.NotEqual("Ft8", Field(composed, "detail"));

        // **AND THE READ-BACK REFUSAL NEVER FIRED**, because it is on the slotted path and this
        // press did not take it. A line here would be the slotted sites becoming reachable.
        Assert.DoesNotContain(
            lines, l => Field(l, "event") == "send_refused_after_read_back");

        NothingPersonal(lines);
    }

    /// <summary>
    /// **The decoder line still names the decoder that started, and that is deliberate.**
    /// </summary>
    /// <remarks>
    /// Work instruction 374 §6's second ruling, in full: `DigitalDecoderStarted` does not say what
    /// the operator pressed, it says **which decoder started**, and under PSK31 the decoder that
    /// starts genuinely is FT8's. Renaming it would put a lie in the file to take one out. This
    /// name exists so that the exception is written down as an assertion rather than as a comment
    /// somebody removes.
    /// </remarks>
    [AvaloniaFact]
    public void TheDecoderLineNamesWhichDecoderStartedAndNotWhatWasPressed()
    {
        var lines = Session("PSK31", model => model.SendCallToAnyoneCommand.Execute(null));

        foreach (var line in lines.Where(l => Field(l, "event") == "digital_decoder_started"))
        {
            _output.WriteLine("  " + line);

            Assert.Equal("Ft8", Field(line, "mode"));
        }

        // **AND THE PRESS BESIDE IT SAYS PSK31**, which is the whole of the distinction: one line
        // says what he asked for and the other says what ran.
        var press = Assert.Single(lines, l => Action(l) == "cq_pressed");

        Assert.Equal("PSK31", Field(press, "detail"));
    }

    /// <summary>
    /// **With nothing pressed, the record says `unknown` and not `Ft8`** - nothing chosen is not
    /// FT8, and writing the family for it is the same lie in smaller print (§0.0).
    /// </summary>
    [AvaloniaFact]
    public void WithNothingChosenThePressSaysUnknown()
    {
        var lines = Session(null, model => model.SendCallToAnyoneCommand.Execute(null));
        var press = Assert.Single(lines, l => Action(l) == "cq_pressed");

        _output.WriteLine("nothing chosen: " + press);

        Assert.Equal(StartupSnapshot.Unknown, Field(press, "detail"));

        NothingPersonal(lines);
    }

    /// <summary>**No callsign and no typed word reaches the record** (HM-DEC-018 §2.1).</summary>
    private void NothingPersonal(IReadOnlyList<string> lines)
    {
        var forbidden = new[] { Psk31Corpus.Load().Operator, "W1AW", TypedAtHim, "FN00DJ" };

        foreach (var word in forbidden)
        {
            Assert.DoesNotContain(
                lines, l => l.Contains(word, StringComparison.OrdinalIgnoreCase));
        }

        _output.WriteLine(
            "  swept " + lines.Count + " lines for " + string.Join(", ", forbidden)
            + " and found none");
    }

    // -----------------------------------------------------------------------------------------

    /// <summary>One panel on one label, one act, its telemetry read back.</summary>
    /// <param name="label">The label to press, or null to press none.</param>
    /// <param name="act">What the operator does.</param>
    private static IReadOnlyList<string> Session(string? label, Action<MainWindowViewModel> act)
    {
        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit374-submode-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(folder);

        try
        {
            using var telemetry = new JsonlTelemetry(folder, "1.13.61", _ => true);

            var settings = new AppSettings { ReconnectOnStartup = false };

            settings.Operator.Callsign = Psk31Corpus.Load().Operator;
            settings.Operator.GridSquare = "FN00DJ";
            settings.Operator.OperatorName = "Tim";
            settings.Operator.Location = "Trafford PA";
            settings.Operator.LicenseClass = LicenseClass.General;

            var model = new MainWindowViewModel(settings, telemetry)
            {
                OperatingMode = "Digital",
                DigitalNewestFirst = false,
            };

            model.SelectedBand = model.Bands.First(
                b => b.Band.LowHz <= On20m && b.Band.HighHz >= On20m);
            model.FrequencyHz = On20m;

            model.UseWorkedBeforeForTests(
                new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));

            if (label is not null)
            {
                // **THE PRESS ITSELF, NOT THE BACKING FIELD.** `ChosenDigitalMode` is assigned in
                // exactly one place in the application and this is that place.
                model.ChooseDigitalModeCommand.Execute(label);
            }

            model.UseRigPortForTests(new FakePort());
            model.UseArmedSendForTests(
                new Ft8ArmedSend(new Ft8TransmitSequence(new FakePort(), new FakeSink())));

            act(model);

            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            telemetry.Dispose();

            return Directory.GetFiles(folder, "*.jsonl")
                .SelectMany(File.ReadAllLines)
                .ToList();
        }
        finally
        {
            try
            {
                Directory.Delete(folder, recursive: true);
            }
            catch (IOException)
            {
                // Not this test's business.
            }
        }
    }

    /// <summary>Hands the panel one heard station, so a row and a card are reachable.</summary>
    private static void Hear(MainWindowViewModel model)
    {
        var corpus = Psk31Corpus.Load();

        var his = corpus.Transcripts
            .Single(t => t.Name == "01-textbook")
            .Lines
            .Where(l => !string.Equals(l.Frm, corpus.Operator, StringComparison.OrdinalIgnoreCase))
            .Take(1)
            .Select(l => l.Text + "\n");

        model.ShowPsk31ChannelsForTests(
            new[] { new Psk31Channel(1, HisHz, 10.0, string.Concat(his)) });
    }

    /// <summary>Lets an unslotted send finish; the click fires it and does not wait (§R10).</summary>
    private static void Settle(MainWindowViewModel model)
    {
        for (var tries = 0; tries < 400 && model.HasSomethingToStop; tries++)
        {
            System.Threading.Thread.Sleep(10);
        }

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
    }

    private static string Action(string line)
        => line.Contains("operator_action", StringComparison.Ordinal) ? Field(line, "action") : "";

    /// <summary>One string field out of one jsonl line.</summary>
    private static string Field(string line, string name)
    {
        var key = "\"" + name + "\":\"";
        var at = line.IndexOf(key, StringComparison.Ordinal);

        if (at < 0)
        {
            return "";
        }

        var from = at + key.Length;
        var to = line.IndexOf('"', from);

        return to < 0 ? "" : line[from..to];
    }
}
