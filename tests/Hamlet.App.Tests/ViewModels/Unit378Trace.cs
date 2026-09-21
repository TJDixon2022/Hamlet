using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Avalonia.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Tests.Psk31;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 378 task 1: **the before, measured, for step 7's five criteria.**
/// </summary>
/// <remarks>
/// <para>**IT BUILDS NOTHING AND CHANGES NOTHING.** It drives a real view model over a PSK31
/// fixture and an Olivia fixture and prints what the tree does today: what a right-click offers
/// row kind by row kind, what the hover says and whether a click still opens the same words,
/// which of R39's ten facts exist and where, what the mode chips and the send line actually say,
/// and what each of Tim's seven lines costs against the cap at PSK31 and at every Olivia
/// variant.</para>
/// <para>**IT IS DELIBERATELY NOT ON THE CARRY-FORWARD LIST** (R14, and the reason
/// `Unit337Measure`, `Unit344Measure` and unit 376's trace are not): almost everything in it is
/// printed rather than asserted, and a name that cannot fail teaches nothing by being run. What
/// it does assert is only that the measurement itself happened - that there were rows to look
/// at - so a silent empty run cannot be read as a finding.</para>
/// <para>**NOTHING IS KEYED AND NO PORT IS OPENED** (FACT-004). The one send it drives is an
/// Olivia CQ over `FakePort` and `FakeSink`, which is how every send test in this project has
/// run since unit 260, and it is driven only to read the sentence the panel writes afterwards.
/// </para>
/// </remarks>
public sealed class Unit378Trace : IDisposable
{
    /// <summary>The station the seven lines are framed for, and the operator.</summary>
    private const string His = "W1AW";

    private const string Mine = "KC3QIS";

    /// <summary>20 m, where the cited table gives an Olivia calling center and a dial.</summary>
    private const long DialOn20m = 14_071_500;

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where every table is printed.</param>
    public Unit378Trace(ITestOutputHelper output)
    {
        _output = output;
        _folder = Path.Combine(Path.GetTempPath(), "hamlet-unit378-" + Guid.NewGuid().ToString("N"));

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

    /// <summary>**The whole before, in one run: the menu, the hover, the facts, 7.5 and the seven lines.**</summary>
    [Fact]
    public void WhatTheRightClickDoesWhatTheRowKnowsAndWhatEachLineCosts()
    {
        MenuAndHoverToday();
        WhereEachFactLives();
        TheChipsAndTheSendLine();
        WhatTheSevenLinesCost();
        WhatStandsOnTheOneItemMenu();
    }

    // -------------------------------------------------------------------------
    // 1 and 2. The menu today, and the hover today.
    // -------------------------------------------------------------------------

    private void MenuAndHoverToday()
    {
        _output.WriteLine("=== 1. THE MENU TODAY, ROW KIND BY ROW KIND ===========================");
        _output.WriteLine("");

        var namesAStation = 0;
        var hasAMenu = 0;
        var looked = 0;
        var textReachableByAClick = 0;
        var textOnTheTip = 0;

        foreach (var (what, model, rows) in Panels())
        {
            _output.WriteLine("-- " + what + " ------------------------------------------------");

            foreach (var row in rows)
            {
                var station = model.Psk31StationOn(row);
                var cq = model.Psk31CqOn(row);
                var answer = model.Psk31AnswerLabelFor(row);
                var flyout = MainWindow.SendFlyoutFor(model, row);
                var items = flyout?.Items.OfType<MenuItem>().ToList() ?? new List<MenuItem>();

                looked++;

                if (station is not null)
                {
                    namesAStation++;
                }

                if (flyout is not null)
                {
                    hasAMenu++;
                }

                _output.WriteLine(
                    "  row \"" + Short(row.Message) + "\""
                    + "  kind " + (row.Reading?.Kind.ToString() ?? "no message")
                    + (row.Reading is { } r ? ", certain " + r.IsCertain : "")
                    + (row.HasVariant ? ", variant " + row.Variant : "")
                    + (row.Ended ? ", ended" : ""));

                _output.WriteLine(
                    "      StationOn [" + (station ?? "null") + "]"
                    + "  CqOn [" + (cq ?? "null") + "]"
                    + "  AnswerLabelFor [" + (answer ?? "null") + "]"
                    + "  flyout " + (flyout is null ? "NULL" : items.Count + " item(s)")
                    + "  SendMenuFor " + (model.SendMenuFor(row) is null ? "null" : "NOT NULL"));

                foreach (var item in items)
                {
                    _output.WriteLine(
                        "        item [" + (item.Header as string ?? "") + "]"
                        + " command " + (item.Command?.GetType().Name ?? "none")
                        + (ReferenceEquals(item.Command, model.AnswerPsk31Command) ? " (AnswerPsk31Command)" : "")
                        + " parameter " + (ReferenceEquals(item.CommandParameter, row) ? "the row" : item.CommandParameter?.ToString() ?? "none")
                        + " hit-testable " + item.IsHitTestVisible);
                }

                // ---- 2. THE HOVER TODAY, ON THE SAME ROW -------------------------
                var tip = row.WorkedTip;
                var whole = row.WholeMessage;

                if (row.HasWholeMessage)
                {
                    textOnTheTip++;
                }

                row.OpenTheWholeMessageCommand.Execute(null);

                var opened = row.WholeMessageIsOpen;
                var same = opened && string.Equals(row.WholeMessage, whole, StringComparison.Ordinal);

                if (same)
                {
                    textReachableByAClick++;
                }

                row.CloseTheWholeMessage();

                _output.WriteLine(
                    "      WorkedTip [" + (tip ?? "null") + "]"
                    + "  HasWholeMessage " + row.HasWholeMessage
                    + "  WholeMessage [" + Short(whole) + "]");

                _output.WriteLine(
                    "      a click on DecodedRowWholeMessage: opened " + opened
                    + ", and the box's text is the SAME STRING the tip draws: " + same);
            }

            _output.WriteLine("");
        }

        _output.WriteLine("7.1's BEFORE, counted over every row on both panels:");
        _output.WriteLine("  rows looked at                              : " + looked);
        _output.WriteLine("  rows that NAME A STATION (Psk31StationOn)   : " + namesAStation);
        _output.WriteLine("  rows that HAVE ANY MENU AT ALL              : " + hasAMenu);
        _output.WriteLine("  rows that name a station and have NO MENU   : " + (namesAStation - hasAMenu));
        _output.WriteLine("  the most items any menu has today           : 1");
        _output.WriteLine("");
        _output.WriteLine("7.3's BEFORE, the measurement section 6 ruling 2 item 3 turns on:");
        _output.WriteLine("  rows whose hover repeats the text           : " + textOnTheTip);
        _output.WriteLine("  rows where a CLICK opens the SAME string    : " + textReachableByAClick);
        _output.WriteLine(
            "  SO TAKING THE TEXT OFF THE TIP HIDES "
            + (textOnTheTip > 0 && textReachableByAClick == textOnTheTip ? "DETAIL, WHICH 0.5 ALLOWS" : "INFORMATION, WHICH 0.5 FORBIDS"));
        _output.WriteLine("");

        Assert.True(looked > 0, "the trace found no rows to measure");
    }

    // -------------------------------------------------------------------------
    // 3. What facts exist, and where.
    // -------------------------------------------------------------------------

    private void WhereEachFactLives()
    {
        _output.WriteLine("=== 3. WHAT THE ROW KNOWS, AGAINST R39's TEN FACTS ====================");
        _output.WriteLine("");

        var model = Psk31Panel(out var rows);
        var row = rows.FirstOrDefault(r => model.Psk31StationOn(r) is not null) ?? rows[0];

        model.CardsNowForTests = DateTime.UtcNow;
        model.OpenPsk31CardCommand.Execute(row);

        var card = model.DigitalCards.FirstOrDefault(c => c.IsPsk31 && c.Callsign == row.Sender);

        _output.WriteLine("measured on the row from " + (row.Sender.Length > 0 ? row.Sender : "(nobody)")
            + ", card " + (card is null ? "NOT OPENED" : "open"));
        _output.WriteLine("");

        var here = OperatorLocation.FromGrid("FN00DJ");
        var there = card?.GridValue is { Length: > 0 } g ? OperatorLocation.FromGrid(g) : null;

        Print("station", "DigitalDecodeRow.Sender (from Reading.Speaker)", row.Sender);
        Print("country", "DxccPrefixes.EntityOf(row.Sender), already said in row.SenderHelp",
            DxccPrefixes.EntityOf(row.Sender) ?? "");
        Print("grid", "Psk31Exchange.Grid on the reading; the card's Ft8CardFacts.Grid / GridValue",
            (row.Reading?.Grid ?? "") + (card is null ? "" : " | card [" + card.GridValue + "]"));
        Print("distance", "NOT ON THE ROW: Ft8ContactCard.DistanceLine over GridPath, needs both grids",
            card?.DistanceValue ?? "");
        Print("offset", "DigitalDecodeRow.Hz", row.Hz);
        Print("strength", "DigitalDecodeRow.Snr", row.Snr);
        Print("started", "Ft8CardFacts.FirstAtUtc - held by the card's facts and NOT EXPOSED as a property; "
            + "it reaches the screen only inside the card's own Slots() prose",
            "");
        Print("stopped", "DigitalDecodeRow.Ended / EndedWord is the FACT; the MOMENT is Ft8ContactCard.TimeLine (LastAtUtc)",
            row.EndedWord + (card is null ? "" : " | " + card.TimeLine));
        Print("spoke to Tim", "DigitalDecodeRow.HasWorkedBefore / WorkedTip (the ADIF log); Reading.IsForOperator",
            (row.HasWorkedBefore ? "worked before" : "") + (row.Reading?.IsForOperator == true ? " | addressed to the operator" : ""));
        Print("parser kind and certainty", "DigitalDecodeRow.Reading.Kind, .IsCertain, and ReadingWord",
            (row.Reading?.Kind.ToString() ?? "") + (row.Reading is { } rr ? ", certain " + rr.IsCertain + ", word [" + row.ReadingWord + "]" : ""));

        _output.WriteLine("");
        _output.WriteLine("  the operator's own grid the row carries : [" + row.ObserverGrid + "]");
        _output.WriteLine("  a distance could be computed from the two grids: "
            + (here is not null && there is not null));
        _output.WriteLine("");
    }

    private void Print(string fact, string where, string value)
        => _output.WriteLine(
            "  " + fact.PadRight(26)
            + (value.Trim().Length > 0 ? "HAS IT: [" + value.Trim() + "]" : "EMPTY ON THIS ROW")
            + "\n      lives in: " + where);

    // -------------------------------------------------------------------------
    // 4. 7.5's before: the chips, and the send line.
    // -------------------------------------------------------------------------

    private void TheChipsAndTheSendLine()
    {
        _output.WriteLine("=== 4. 7.5's BEFORE: THE CHIPS, AND THE SEND LINE =====================");
        _output.WriteLine("");

        foreach (var mode in new[] { "FT8", "PSK31", "Olivia" })
        {
            var model = Panel(mode);

            model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= DialOn20m && b.Band.HighHz >= DialOn20m);
            model.FrequencyHz = DialOn20m;

            _output.WriteLine("  chosen " + mode.PadRight(7) + " dial " + model.FrequencyHz + " Hz, ChosenDigitalMode [" + model.ChosenDigitalMode + "]");

            foreach (var chip in model.DigitalModeChips)
            {
                _output.WriteLine(
                    "      " + chip.Label.PadRight(7)
                    + " IsChosen " + chip.IsChosen.ToString().PadRight(5)
                    + " IsLit " + chip.IsLit.ToString().PadRight(5)
                    + " IsPlain " + chip.IsPlain.ToString().PadRight(5)
                    + " IsChosenElsewhere " + chip.IsChosenElsewhere);
            }

            _output.WriteLine("");
        }

        // ---- THE SEND LINE AFTER A REAL OLIVIA CQ, VERBATIM --------------------
        string line;

        using (var telemetry = new JsonlTelemetry(_folder, "378", _ => true))
        {
            var model = Panel("Olivia");

            model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= DialOn20m && b.Band.HighHz >= DialOn20m);
            model.FrequencyHz = DialOn20m;
            model.TapForTests = new Hamlet.RadioEngine.Audio.AudioTap();

            var port = new FakePort();
            var sink = new FakeSink();

            model.UseRigPortForTests(port);
            model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, guard: null, telemetry)));

            model.SendCallToAnyoneCommand.Execute(null);

            for (var tries = 0; tries < 2000 && model.HasSomethingToStop; tries++)
            {
                Thread.Sleep(10);
            }

            Avalonia.Threading.Dispatcher.UIThread.RunJobs();

            line = model.DigitalSendLine;

            _output.WriteLine("  an Olivia CQ was driven over FakePort and FakeSink - no port opened, nothing keyed.");
            _output.WriteLine("  the sound card was called " + sink.TimesCalled + " time(s).");
        }

        _output.WriteLine("");
        _output.WriteLine("  THE SEND LINE UNDER OLIVIA, VERBATIM:");
        _output.WriteLine("      [" + line + "]");
        _output.WriteLine("");
        _output.WriteLine("  does it say \"of PSK31\" while Olivia is chosen : "
            + line.Contains("of PSK31", StringComparison.Ordinal));
        _output.WriteLine("  does it name Olivia anywhere                  : "
            + line.Contains("Olivia", StringComparison.Ordinal));
        _output.WriteLine("");
        _output.WriteLine("  THE SENTENCES IN Psk31WentLine AND StopLine THAT NAME A MODE OR A MACRO,");
        _output.WriteLine("  read off the source at MainWindowViewModel.cs and reported, not changed:");
        _output.WriteLine("      WentLine, no run      : \"Hamlet composed the PSK31 <macro> and nothing went out: ...\"  - NAMES BOTH, and PSK31 is a literal");
        _output.WriteLine("      WentLine, cancelled   : \"Stopped: \\\"...\\\" went out for N s and the rest of it did not. ...\"  - names neither");
        _output.WriteLine("      WentLine, licence     : \"Hamlet did not send \\\"...\\\": <reason> (<citation>)\"          - names neither");
        _output.WriteLine("      WentLine, no audio    : \"Hamlet did not send \\\"...\\\": <reason>\"                       - names neither");
        _output.WriteLine("      WentLine, sent        : \"Sent \\\"...\\\" - N s of PSK31.\"                                - NAMES THE MODE, and it is a literal");
        _output.WriteLine("      StopLine, all five    : nothing in it names a mode or a macro");
        _output.WriteLine("");
    }

    // -------------------------------------------------------------------------
    // 5. What each of Tim's seven lines costs, against the cap.
    // -------------------------------------------------------------------------

    private void WhatTheSevenLinesCost()
    {
        _output.WriteLine("=== 5. THE SEVEN LINES AGAINST THE CAP ================================");
        _output.WriteLine("");
        _output.WriteLine("  framed for " + His + " de " + Mine + ", the three macros as their own composers");
        _output.WriteLine("  build them and the four texts as Psk31Macros.Typed frames them.");
        _output.WriteLine("");

        var lines = new List<(string Label, string Text)>
        {
            ("Answer him", Psk31Macros.Answer(His, Mine)),
            ("Send my report", Psk31Macros.Report(His, Mine, "Tim", "Pennsylvania", "FN00DJ")),
            ("Confirm and 73", Psk31Macros.Confirm(His, Mine)),
            ("Say again?", Psk31Macros.Typed(His, Mine, "PSE RPT ALL AFTER YOUR CALL")),
            ("Please repeat your report", Psk31Macros.Typed(His, Mine, "PSE RPT UR RST")),
            ("QRZ?", Psk31Macros.Typed(His, Mine, "QRZ? QRZ?")),
            ("73 and out", Psk31Macros.Typed(His, Mine, "73 AND THANKS FOR THE QSO")),
        };

        var variants = OliviaData.Current.Format?.Variants.Select(v => v.Name).ToList() ?? new List<string>();
        var timing = OliviaData.Current.Timing;
        var macroCap = OperatorSend.LongestUnslottedSeconds;
        var refused = new List<string>();

        _output.WriteLine("  the Olivia variants the format carries: " + string.Join(", ", variants));
        _output.WriteLine("  the timing table carries a row for   : "
            + string.Join(", ", variants.Where(v => timing?.SecondsPerCharacter.ContainsKey(v) == true)));
        _output.WriteLine("  every other variant takes the " + macroCap + " s fallback (R43, deferred, measured here and NOT repaired)");
        _output.WriteLine("");

        foreach (var (label, text) in lines)
        {
            var (clean, dropped) = Psk31Macros.Sendable(text);
            var psk31 = Psk31Modulator.SecondsFor(text);

            _output.WriteLine("  " + label.ToUpperInvariant());
            _output.WriteLine("      framed   : \"" + text + "\"");
            _output.WriteLine("      characters " + text.Length
                + ", through Sendable " + clean.Length + " kept and " + dropped + " dropped"
                + (dropped == 0 && clean.Length == text.Trim().Length ? "  (the varicode carries all of it)" : "  ** CHARACTERS WOULD BE LOST **"));
            _output.WriteLine("      PSK31    : " + psk31.ToString("0.00", CultureInfo.InvariantCulture)
                + " s against a " + macroCap + " s cap - " + (psk31 <= macroCap ? "FITS" : "REFUSED"));

            if (psk31 > macroCap)
            {
                refused.Add(label + " at PSK31, " + psk31.ToString("0.00", CultureInfo.InvariantCulture) + " s against " + macroCap + " s");
            }

            foreach (var variant in variants)
            {
                var seconds = OliviaModulator.TextSeconds(variant, text.Length);
                var cap = timing?.CapSeconds(variant, OliviaSendKind.Macro) is { } c && !double.IsNaN(c)
                    ? c
                    : macroCap;
                var fromTable = timing?.SecondsPerCharacter.ContainsKey(variant) == true;
                var fits = !double.IsNaN(seconds) && seconds <= cap;

                _output.WriteLine(
                    "      Olivia " + variant.PadRight(8)
                    + (double.IsNaN(seconds) ? "  no such variant" : seconds.ToString("0.00", CultureInfo.InvariantCulture).PadLeft(7) + " s")
                    + "  cap " + cap.ToString("0.00", CultureInfo.InvariantCulture).PadLeft(7) + " s"
                    + (fromTable ? " (timing row)   " : " (30 s FALLBACK)")
                    + "  " + (fits ? "FITS" : "REFUSED"));

                if (!fits && !double.IsNaN(seconds))
                {
                    refused.Add(label + " at Olivia " + variant + ", "
                        + seconds.ToString("0.00", CultureInfo.InvariantCulture) + " s against a "
                        + cap.ToString("0.00", CultureInfo.InvariantCulture) + " s cap"
                        + (fromTable ? "" : " (the 30 s fallback)"));
                }
            }

            _output.WriteLine("");
        }

        _output.WriteLine("  WHAT THE CAP REFUSES, WHICH IS R43's DEFERRED QUESTION MADE VISIBLE:");

        if (refused.Count == 0)
        {
            _output.WriteLine("      nothing - all seven fit at PSK31 and at every variant.");
        }

        foreach (var one in refused)
        {
            _output.WriteLine("      " + one);
        }

        _output.WriteLine("");
    }

    // -------------------------------------------------------------------------
    // 6. What stands on the one-item menu.
    // -------------------------------------------------------------------------

    private void WhatStandsOnTheOneItemMenu()
    {
        _output.WriteLine("=== 6. THE ASSERTIONS THAT STAND ON THE ONE-ITEM MENU ==================");
        _output.WriteLine("");
        _output.WriteLine("  ThePsk31ReadsTheConversationTests.NoClickOnAPsk31RowReachesASendPath");
        _output.WriteLine("      line 453  Assert.Null(model.SendMenuFor(row))                 - STAYS EXACTLY AS IT IS");
        _output.WriteLine("      line 454  Assert.False(model.CanLogRow(row))                  - untouched");
        _output.WriteLine("      line 461  var flyout = MainWindow.SendFlyoutFor(model, row)   - the read itself");
        _output.WriteLine("      line 465  Assert.Null(flyout) where Psk31CqOn(row) is null    - GOES RED: every row that");
        _output.WriteLine("                names a station gets a menu at task 2, CQ or not.");
        _output.WriteLine("      line 469  Assert.Single(flyout!.Items.OfType<MenuItem>())     - GOES RED: seven, not one.");
        _output.WriteLine("      line 471  Assert.Same(model.AnswerPsk31Command, item.Command) - the Answer row still");
        _output.WriteLine("                carries that command, but it is no longer the ONLY item.");
        _output.WriteLine("      line 472  Assert.Same(row, item.CommandParameter)             - same.");
        _output.WriteLine("      It is on the carry-forward list, so the rewrite is R12's and goes in its own commit.");
        _output.WriteLine("");
        _output.WriteLine("  TheWholeChainRunsFromOneRightClickTests");
        _output.WriteLine("      line 891  names SendFlyoutFor in a comment only - it drives the handler, not the method.");
        _output.WriteLine("      line 969  Options(flyout) = items carrying a command; line 985 TheOneThatComesNext.");
        _output.WriteLine("      IT DRIVES AN FT8 ROW, NOT A PSK31 ROW: RightClick finds a row whose Sender is his and");
        _output.WriteLine("      whose Addressee is the operator on an FT8 panel, and the item's CommandParameter is");
        _output.WriteLine("      read as a string - the FT8 SendMessageCommand shape. The PSK31 branch this unit");
        _output.WriteLine("      changes is not on its path, so it is EXPECTED TO STAY GREEN, and task 4 says whether");
        _output.WriteLine("      it did. It is not on the carry-forward list and needs a real audio endpoint.");
        _output.WriteLine("");
    }

    // -------------------------------------------------------------------------
    // The two fixtures.
    // -------------------------------------------------------------------------

    private IEnumerable<(string What, MainWindowViewModel Model, List<DigitalDecodeRow> Rows)> Panels()
    {
        var psk31 = Psk31Panel(out var psk31Rows);

        yield return ("PSK31, the eight corpus transcripts", psk31, psk31Rows);

        var olivia = OliviaPanel(out var oliviaRows);

        yield return ("OLIVIA, four channels at four variants", olivia, oliviaRows);
    }

    /// <summary>The PSK31 panel, every corpus transcript on its own carrier, whole.</summary>
    private MainWindowViewModel Psk31Panel(out List<DigitalDecodeRow> rows)
    {
        var corpus = Psk31Corpus.Load();
        var model = Panel("PSK31", corpus.Operator);

        model.ShowPsk31ChannelsForTests(corpus.Transcripts
            .Select((t, i) => new Psk31Channel(
                i + 1,
                500 + (i * 250),
                10.0 + i,
                string.Concat(t.Lines.Select(l => l.Text + "\n"))))
            .ToList());

        rows = model.DigitalDecodes.Where(r => r.IsTextOnly).ToList();

        return model;
    }

    /// <summary>The Olivia panel, four channels, so 7.4's *by construction* has a before too.</summary>
    private MainWindowViewModel OliviaPanel(out List<DigitalDecodeRow> rows)
    {
        var model = Panel("Olivia");

        model.ShowOliviaChannelsForTests(new[]
        {
            new OliviaChannel(1, "8/250", 1000, OliviaListener.FoundByRsid, 0, "CQ CQ CQ de " + His + " " + His + " pse K\n", 9, 0, false),
            new OliviaChannel(2, "16/500", 1500, OliviaListener.FoundByRsid, 0, Mine + " de K3ABC  RST 599 599  BTU " + Mine + " de K3ABC K\n", 9, 0, false),
            new OliviaChannel(3, "32/1000", 2000, OliviaListener.FoundByRsid, 0, "K9XYZ de N2DEF  R R TNX  73 73  K9XYZ de N2DEF SK\n", 9, 0, true),
            new OliviaChannel(4, "4/500", 2400, OliviaListener.FoundByRsid, 0, "CQ CQ de W4G~F W4G~F K\n", 9, 0, false),
        });

        rows = model.DigitalDecodes.Where(r => r.IsTextOnly).ToList();

        return model;
    }

    private static MainWindowViewModel Panel(string mode, string? callsign = null)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = callsign ?? Mine;
        settings.Operator.GridSquare = "FN00DJ";
        settings.Operator.OperatorName = "Tim";
        settings.Operator.Location = "Pennsylvania";
        settings.Operator.LicenseClass = Hamlet.RadioEngine.Licensing.LicenseClass.General;

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute(mode);

        return model;
    }

    private static string Short(string? text)
    {
        var one = (text ?? "").Replace("\n", " / ", StringComparison.Ordinal).Trim();

        return one.Length <= 64 ? one : one[..61] + "...";
    }
}
