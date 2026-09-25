using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Avalonia.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Tests.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 316 task 4: **PSK31 rows become stations, through code that does not change.**
/// </summary>
/// <remarks>
/// <para>**EACH TRANSCRIPT IS ONE CHANNEL**, its text growing a line at a time exactly as a
/// listener would list it, and handed to the panel through the path the tick takes. The row's
/// station is whatever the latest complete message on it says - cut by the splitter and read by
/// the parser - and the operator's side, the fade, the country and the quill are the FT8 code,
/// asked of it. **The CQ filter is not asked of it since work instruction 337**: the squelch is
/// the only gate on a PSK31 row.</para>
/// <para>**THE OPERATOR'S CALLSIGN IS SET IN SETTINGS**, from the corpus's own `operator` field,
/// and not in anything under test.</para>
/// <para>**COMPUTED, NOT SEEN, AND WRITTEN, NOT RECORDED.** Nothing here looks at a pixel, and
/// the corpus was typed (FACT-004).</para>
/// </remarks>
public sealed class ThePsk31ReadsTheConversationTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public ThePsk31ReadsTheConversationTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Assertion 1: the CQ toggle holds no PSK31 row back, and each row still reads its latest message.**</summary>
    /// <remarks>
    /// <para>**REWRITTEN BY WORK INSTRUCTION 337 TASK 1, NOT DELETED.** Unit 316 asserted the CQ
    /// filter selected exactly the rows whose latest message was a CQ - which is the rule that held
    /// a carrier with 262 characters and no turnover off the screen all evening on 2026-09-12. The
    /// later ruling is PSK31 plan R9 as that instruction states it: **the squelch is the only gate
    /// on a PSK31 row**. What is kept is the part that was never about the filter: every row reads
    /// its latest message as the corpus says, and `CQ DX` is a call to anyone.</para>
    /// <para>**ALL EIGHT CHANNELS AT ONCE**, stepped a line at a time together, so the list holds
    /// every kind the corpus has side by side.</para>
    /// </remarks>
    [Fact]
    public void TheCqToggleHoldsNoPsk31RowBack()
    {
        var corpus = Psk31Corpus.Load();
        var model = Panel();
        var carriers = Carriers(corpus);
        var longest = carriers.Max(c => c.Transcript.Lines.Count);
        var tally = new SortedDictionary<string, int>(StringComparer.Ordinal);

        model.ShowsCqOnly = true;

        for (var step = 1; step <= longest; step++)
        {
            Show(model, carriers, step);

            foreach (var carrier in carriers)
            {
                var row = RowOf(model, carrier);
                var latest = carrier.LatestMessage(step);
                // **A ROW FOR HIM IS HIS SIDE'S QUESTION AND NOT THE TOGGLE'S** - and his side
                // follows one station, so with eight channels up it is assertion 2 that asks it.
                var forHim = latest is not null
                    && string.Equals(latest.Addressee, corpus.Operator, StringComparison.OrdinalIgnoreCase);
                var shown = model.DigitalVisibleDecodes.Contains(row);
                var key = (latest?.Kind ?? "no message") + (forHim ? " for him" : shown ? " in" : " out");

                tally[key] = tally.GetValueOrDefault(key) + 1;

                Assert.True(
                    forHim || shown,
                    carrier.Transcript.Name + " after line " + step + ": latest " + (latest?.Kind ?? "none")
                    + ", held back by the CQ toggle");

                if (latest is not null)
                {
                    Assert.Equal(latest.ExpectedKind, row.Reading?.Kind);
                }
            }

            _output.WriteLine("after line " + step + ", the CQ filter holds: "
                + string.Join(", ", model.DigitalVisibleDecodes.Select(r => r.Sender + " (" + r.Addressee + ") at " + r.Hz)));
        }

        foreach (var (key, count) in tally)
        {
            _output.WriteLine("  " + key.PadRight(18) + count + " row-steps");
        }

        // **`CQ DX` IS A CQ.**
        Assert.Contains(model.DigitalVisibleDecodes, r => r.Addressee == "CQ DX");
    }

    /// <summary>**Assertion 2: a row addressed to the operator is on his side and not in the left list.**</summary>
    /// <remarks>
    /// **ONE CHANNEL AT A TIME**, so his side has one station on it and a row being there is not
    /// confused with a second caller waiting.
    /// </remarks>
    [Fact]
    public void ARowAddressedToTheOperatorIsOnHisSideAndNotInTheLeftList()
    {
        var corpus = Psk31Corpus.Load();

        foreach (var carrier in Carriers(corpus))
        {
            var model = Panel();

            for (var step = 1; step <= carrier.Transcript.Lines.Count; step++)
            {
                model.ShowPsk31ChannelsForTests(new[] { carrier.Through(step) });

                var row = RowOf(model, carrier);
                var latest = carrier.LatestMessage(step);
                var forHim = latest is not null
                    && string.Equals(latest.Addressee, corpus.Operator, StringComparison.OrdinalIgnoreCase);
                var onHisSide = model.DigitalMineDecodes.Contains(row);
                var onTheLeft = model.DigitalVisibleDecodes.Contains(row);

                _output.WriteLine(carrier.Transcript.Name + " after line " + step + ": "
                    + (row.Sender.Length > 0 ? row.Sender : "(no sender)") + " > " + (row.Addressee.Length > 0 ? row.Addressee : "(none)")
                    + (onHisSide ? ", his side" : "") + (onTheLeft ? ", left list" : ""));

                Assert.True(forHim == onHisSide, carrier.Transcript.Name + " after line " + step + ": for him " + forHim + ", on his side " + onHisSide);
                Assert.True(onHisSide != onTheLeft, carrier.Transcript.Name + " after line " + step + ": on both sides or neither");

                if (carrier.Transcript.Name == "04-not-for-me")
                {
                    Assert.False(onHisSide);
                }
            }
        }
    }

    /// <summary>**Assertion 3: an uncertain parse says so, as a member and as a word.**</summary>
    [Fact]
    public void AnUncertainParseSaysSoAsAMemberAndAWord()
    {
        var corpus = Psk31Corpus.Load();

        foreach (var carrier in Carriers(corpus))
        {
            var model = Panel();

            for (var step = 1; step <= carrier.Transcript.Lines.Count; step++)
            {
                model.ShowPsk31ChannelsForTests(new[] { carrier.Through(step) });

                var row = RowOf(model, carrier);
                var latest = carrier.LatestMessage(step);

                if (latest is null)
                {
                    Assert.False(row.IsGuess);
                    Assert.Equal("", row.ReadingWord);
                    continue;
                }

                if (!latest.Certain)
                {
                    _output.WriteLine(carrier.Transcript.Name + " after line " + step + ": IsGuess " + row.IsGuess
                        + ", word [" + row.ReadingWord + "], caption [" + row.Caption + "]");

                    Assert.True(row.IsGuess, carrier.Transcript.Name + " after line " + step + " is shown as certain");
                    Assert.True(row.HasReadingWord);
                }

                if (row.Reading is { IsCertain: true })
                {
                    Assert.Equal("", row.ReadingWord);
                }
            }
        }

        // **`05-garbled` LINE 3, BY NAME.**
        var garbled = Carriers(corpus).Single(c => c.Transcript.Name == "05-garbled");
        var panel = Panel();

        panel.ShowPsk31ChannelsForTests(new[] { garbled.Through(3) });

        var third = RowOf(panel, garbled);

        _output.WriteLine("05-garbled line 3: sender " + third.Sender + ", IsGuess " + third.IsGuess
            + ", word [" + third.ReadingWord + "], caption [" + third.Caption + "]");

        Assert.Equal("K3ABC", third.Sender);
        Assert.True(third.IsGuess);
        Assert.Equal("guess", third.ReadingWord);
        Assert.Contains("guess", third.Caption, StringComparison.Ordinal);
    }

    /// <summary>**Assertion 4: a row with no speaker has no sender, no addressee, no country, and is not a CQ.**</summary>
    /// <remarks>
    /// <para>**NO SPLIT MESSAGE HAS NO SPEAKER**, and that is a finding about the splitter: a
    /// message ends only on a turnover after `de` and a clean callsign, and that callsign is the
    /// speaker. So a row reaches this state by carrying no finished message - text with no
    /// turnover, or a CQ whose own call is damaged - and both are fed through the panel.</para>
    /// <para>**AND A PARSE WITH NO SPEAKER IS PUT ON A ROW BY HAND**, the one thing not built
    /// through the splitter here, to show the row's members hold when the parser says unknown.</para>
    /// </remarks>
    [Fact]
    public void ARowWithNoSpeakerHasNoSenderNoAddresseeNoCountryAndIsNotACq()
    {
        var corpus = Psk31Corpus.Load();
        var garbled = corpus.Transcripts.Single(t => t.Name == "05-garbled");
        var model = Panel();

        model.ShowsCqOnly = true;

        var noTurnover = garbled.Lines[3].Text + "\n";
        const string damagedCq = "CQ CQ CQ de K3A~C K3A~C pse K\n";

        model.ShowPsk31ChannelsForTests(new[]
        {
            new Psk31Channel(1, 800, 10, noTurnover),
            new Psk31Channel(2, 1300, 10, damagedCq),
        });

        foreach (var row in model.DigitalDecodes.Where(r => r.IsTextOnly).ToList())
        {
            _output.WriteLine("[" + row.Message.TrimEnd() + "]: sender [" + row.Sender + "], addressee ["
                + row.Addressee + "], hover [" + row.SenderHelp + "], word [" + row.ReadingWord + "]");

            Assert.Null(row.Reading);
            Assert.Equal("", row.Sender);
            Assert.Equal("", row.Addressee);
            Assert.Equal("Who sent it.", row.SenderHelp);
            Assert.Null(DxccPrefixes.EntityOf(row.Sender));

            // **ON THE LEFT LIST WITH THE CQ TOGGLE ON** (work instruction 337 task 1): not a CQ,
            // and still shown, because the squelch is the only gate on a PSK31 row. Unit 316
            // asserted it was held back.
            Assert.Contains(row, model.DigitalVisibleDecodes);
            Assert.DoesNotContain(row, model.DigitalMineDecodes);
        }

        // **AND THE ROW STILL SHOWS ITS TEXT.**
        Assert.Contains(model.DigitalDecodes, r => r.Message == noTurnover);

        var reading = Psk31ExchangeParser.Read(garbled.Lines[3].Text, corpus.Operator);

        Assert.Null(reading.Speaker);

        var parsed = new DigitalDecodeRow(
            "121500", "+10", DigitalDecodeRow.NotMeasured, "1700", garbled.Lines[3].Text,
            IsTextOnly: true, Reading: reading);

        model.DigitalDecodes.Add(parsed);

        _output.WriteLine("parsed with no speaker: sender [" + parsed.Sender + "], addressee [" + parsed.Addressee
            + "], word [" + parsed.ReadingWord + "]");

        Assert.Equal("", parsed.Sender);
        Assert.Equal("", parsed.Addressee);
        Assert.Equal("unknown", parsed.ReadingWord);
        Assert.Equal("Who sent it.", parsed.SenderHelp);
        Assert.Contains(parsed, model.DigitalVisibleDecodes);
    }

    /// <summary>**Assertion 5: a Costa Rican station calling CQ gets what it would get on FT8.**</summary>
    [Fact]
    public void ACostaRicanStationCallingCqGetsTheSameEntityFadeAndQuillAsOnFt8()
    {
        var nudges = Log("W9ZZZ");

        var ft8 = Panel(nudges, mode: "FT8");
        var ft8Row = ft8.AddDecodeRowForTests(
            "021115", "-09", "0.2", "1240", "CQ TI2ABC EK70", Slot("02:11:15"), heardOnHz: 14_074_000);

        var psk31 = Panel(nudges);

        psk31.ShowsCqOnly = true;
        psk31.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(1, 1000, 10, "CQ CQ CQ de TI2ABC TI2ABC K\n") });

        var row = psk31.DigitalDecodes.Single(r => r.IsTextOnly);

        foreach (var (name, shown) in new[] { ("FT8  ", ft8Row), ("PSK31", row) })
        {
            _output.WriteLine(name + ": sender " + shown.Sender + ", entity " + DxccPrefixes.EntityOf(shown.Sender)
                + ", hover [" + shown.SenderHelp + "], nudge " + shown.Nudge + " [" + shown.NudgeTip + "], lift "
                + shown.RowLift + ", opacity " + shown.RowOpacity);
        }

        Assert.Equal("TI2ABC", row.Sender);
        Assert.Equal(ft8Row.Sender, row.Sender);
        Assert.NotNull(DxccPrefixes.EntityOf(row.Sender));
        Assert.Equal(DxccPrefixes.EntityOf(ft8Row.Sender), DxccPrefixes.EntityOf(row.Sender));
        Assert.Equal(ft8Row.SenderHelp, row.SenderHelp);
        Assert.NotEqual(NudgeKind.None, row.Nudge);
        Assert.Equal(ft8Row.Nudge, row.Nudge);
        Assert.Equal(ft8Row.NudgeTip, row.NudgeTip);
        Assert.Equal(ft8Row.IsNudged, row.IsNudged);
        Assert.Equal(ft8Row.RowLift, row.RowLift);
        Assert.Equal(ft8Row.RowOpacity, row.RowOpacity);
        Assert.Contains(row, psk31.DigitalVisibleDecodes);
        Assert.Equal("", row.ReadingWord);
    }

    /// <summary>**Assertion 6: worked-fade dims a PSK31 row from a worked station to the same 0.55.**</summary>
    [Fact]
    public void WorkedFadeDimsAPsk31RowToTheSameOpacity()
    {
        var worked = new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase)
        {
            ["TI2ABC"] = new AdifContact { Call = "TI2ABC", StartedUtc = new DateTime(2026, 9, 7, 1, 0, 0, DateTimeKind.Utc) },
        };

        var ft8 = Panel(Log("W9ZZZ"), worked, "FT8");
        var ft8Row = ft8.AddDecodeRowForTests(
            "021115", "-09", "0.2", "1240", "CQ TI2ABC EK70", Slot("02:11:15"), heardOnHz: 14_074_000);

        var psk31 = Panel(Log("W9ZZZ"), worked);

        psk31.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(1, 1000, 10, "CQ CQ CQ de TI2ABC TI2ABC K\n") });

        var row = psk31.DigitalDecodes.Single(r => r.IsTextOnly);

        _output.WriteLine("FT8 opacity " + ft8Row.RowOpacity + " [" + ft8Row.WorkedTip + "], PSK31 opacity "
            + row.RowOpacity + " [" + row.WorkedTip + "]");

        Assert.Equal(0.55, row.RowOpacity);
        Assert.Equal(ft8Row.RowOpacity, row.RowOpacity);
        Assert.Equal(ft8Row.WorkedTip, row.WorkedTip);

        // **AND A ROW ALREADY ON THE LIST FADES THE MOMENT THE LOG GAINS THE ENTRY.**
        var later = Panel(Log("W9ZZZ"));

        later.ShowPsk31ChannelsForTests(new[] { new Psk31Channel(1, 1000, 10, "CQ CQ CQ de TI2ABC TI2ABC K\n") });

        Assert.Equal(1.0, later.DigitalDecodes.Single(r => r.IsTextOnly).RowOpacity);

        later.UseWorkedBeforeForTests(worked);

        Assert.Equal(0.55, later.DigitalDecodes.Single(r => r.IsTextOnly).RowOpacity);
    }

    /// <summary>**Assertion 7: the code these rows reuse is unchanged, measured.**</summary>
    /// <remarks>
    /// <para>**THE HASHES ARE OF THE FILES AT THE TASK 1 COMMIT, `2a9d9ed`**, with carriage returns
    /// removed so a checkout's line endings cannot fail it. `git diff 2a9d9ed..HEAD` over the same
    /// files is printed in the report.</para>
    /// <para>**AND THE TWO ROW MEMBERS BY THEIR BODIES**, since the file they live in does change:
    /// each must still be there exactly once, character for character.</para>
    /// </remarks>
    [Fact]
    public void TheCodeTheseRowsReuseIsUnchanged()
    {
        var root = Psk31Corpus.Root();

        foreach (var (file, sha) in new[]
        {
            ("src/Hamlet.RadioEngine/Explore/DxccPrefixes.cs", "f5ebf765a62defe87c0e25bc88c4a005ac3614d02dd8b51bb9a5d6a432351ef2"),
            // **MOVED BY UNIT 327 TASK 3, NOT DROPPED.** `NudgeSet` gained `Explain`,
            // which is `WouldOpen` with the continent and the count attached for the
            // popup, and `NudgeWords` gained the two lines that popup says. Both
            // changes are under a ruling - Tim, 2026-09-12, that the mark must answer
            // a click - and re-pinning is the right answer for a ruled change. **The
            // door still names nothing**, which assertion 5 of
            // `TheMarkIsSeenAndClickedTests` checks by behaviour against every
            // continent in the cited table rather than by this hash.
            //
            // **MOVED AGAIN BY UNIT 427 TASK 1, UNDER HM-DEC-180.** `WouldOpen` and `Explain`
            // take the grid he sent, and a grid that contradicts his prefix opens nothing
            // (Tim, 2026-09-23). Asserted by behaviour in `TheGridBeatsThePrefixTests`.
            ("src/Hamlet.RadioEngine/Contacts/NudgeSet.cs", "e7f0026f00b6afcac02818ba9ba4d951ac692413105683264fb1622ecd206080"),
            ("src/Hamlet.App/ViewModels/NudgeWords.cs", "98d85db8cf9691895b8f1922ff55547dfb0a8cd65025af8839c1090e1942828e"),

            // **AND `AchievementMarkControl.cs` IS NO LONGER PINNED AT ALL** (§R14,
            // and work instruction 327 sections 5 and 10 name this pin by file).
            // It moved for unit 325 and again for unit 327, which is a pin that
            // tracks the file rather than guarding it: **a hash proves no criterion**
            // (§R14), and a pin that is re-stamped every time the file is worked on
            // has stopped being a check and become a chore that fails the next unit
            // for doing what it was told to do. What these rows actually need of that
            // control is asserted by behaviour - the two forms, the two colors, the
            // ring, the size - in `TheMarkIsSeenAndClickedTests`, `Unit300MarkTests`,
            // `Unit301MarkSizeTests` and `Unit303OptionBTests`.
        })
        {
            var bytes = File.ReadAllBytes(Path.Combine(root, file)).Where(b => b != (byte)'\r').ToArray();
            var actual = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

            _output.WriteLine(file + " " + actual + (actual == sha ? " unchanged" : " CHANGED from " + sha));

            Assert.Equal(sha, actual);
        }

        var rowSource = File.ReadAllText(Path.Combine(root, "src", "Hamlet.App", "ViewModels", "DigitalDecodeRow.cs"));

        // **THE OPACITY LINE GAINED A SECOND REASON TO FADE IN UNIT 324**, and the pin is
        // moved rather than dropped. What unit 279 fixed is the number and the mechanism -
        // recede at 0.55, never grey - and both are still here; what is new is that a
        // carrier Hamlet can hear and cannot read fades the same way, which assertion 6
        // above and `ThePsk31CarrierLivesTests` both check by behaviour rather than by
        // source text. A worked-before row is still 0.55 and that is asserted, not pinned.
        // **AND A THIRD IN WORK INSTRUCTION 355 TASK 2**, moved again rather than dropped: a PSK31
        // row whose carrier went is kept and fades the same way (Tim, 2026-09-14), checked by
        // behaviour in `ThePsk31RowStaysTests`. The number and the mechanism are unchanged.
        foreach (var body in new[]
        {
            "public bool HasWorkedBefore => _workedBefore.Length > 0;",
            "public double RowOpacity => HasWorkedBefore || HeardNotReadable || Ended ? 0.55 : 1.0;",
        })
        {
            var count = rowSource.Split(body).Length - 1;

            _output.WriteLine(body + " found " + count + " time(s)");

            Assert.Equal(1, count);
        }
    }

    /// <summary>**Assertion 8: no FT8 message is ever on a PSK31 row, and what the row offers is R39's seven.**</summary>
    /// <remarks>
    /// <para>**THE FT8 LEDGER IS GIVEN W1AW FIRST**, from an FT8 CQ, so the right-click menu and
    /// the conversation card would both exist for a PSK31 row from W1AW if the guard were
    /// missing.</para>
    /// <para>**REWRITTEN BY WORK INSTRUCTION 378 TASK 2 UNDER §R12, AND IT ASSERTS MORE THAN IT
    /// REPLACED.** What it asserted about the menu was the one-item door unit 323 opened: null
    /// on every row that was not a certain CQ, and a single item bound to `AnswerPsk31Command`
    /// where it was. R39 (Tim, 2026-09-21, ruled C) replaces both - a right-click on ANY row
    /// that names a station offers the seven lines from `data/psk31/canned.json`. **Nothing here
    /// is loosened**: `Assert.Null(model.SendMenuFor(row))` is untouched and still the first
    /// thing asked of every row, `CanLogRow` is untouched, and the Answer item's command and
    /// parameter are still asserted by identity on exactly the rows that carried them.</para>
    /// <para>**GROWN AGAIN BY WORK INSTRUCTION 383 TASK 3 UNDER §R12, AND IT ASSERTS MORE THAN
    /// IT REPLACED.** Criterion 7.2 asks that every canned send write `macro: canned`, and the
    /// three macro rows wrote their own token because nothing downstream could tell a macro
    /// pressed off the list from the same macro pressed on the card. The Answer item now carries
    /// the command that marks the press, so where this asserted TWO identities it asserts THREE
    /// and a type: the marking command on the item, `AnswerPsk31Command` itself inside the press,
    /// the row itself inside the press, and that the press is NOT `SendCannedPsk31Command` -
    /// which is the thing unit 378's reasoning and §R1's gate forbid. **The send is still made by
    /// the command it was always made by.**</para>
    /// <para>**AND FOUR THINGS ARE ASSERTED THAT NEVER WERE**: that a row naming a station has
    /// a menu where it had none - five of unit 378's eleven traced rows were in that state - that
    /// the seven arrive in the file's own order with R39's labels, that no item on a PSK31 row
    /// carries `SendMessageCommand` so no FT8 message can ever be on one, and that nothing is
    /// greyed or disabled, an absent line being a note with no command instead.</para>
    /// </remarks>
    [Fact]
    public void NoFt8MessageIsOnAPsk31RowAndWhatItOffersIsTheCannedSeven()
    {
        var corpus = Psk31Corpus.Load();
        var model = Panel();
        var canned = model.CannedLines;

        Assert.True(canned.IsUsable, canned.Problem ?? "the shipped canned lines did not read");

        model.CardsNowForTests = DateTime.UtcNow;

        var ft8Row = model.AddDecodeRowForTests(
            "021115", "-09", "0.2", "1240", "CQ W1AW FN31", Slot("02:11:15"), heardOnHz: 14_074_000);

        _output.WriteLine("an FT8 row from W1AW has a menu: " + (model.SendMenuFor(ft8Row) is not null));

        Assert.NotNull(model.SendMenuFor(ft8Row));

        var carriers = Carriers(corpus);
        var longest = carriers.Max(c => c.Transcript.Lines.Count);
        var looked = 0;
        var named = 0;
        var offered = 0;
        var notACqAndOffered = 0;
        var held = 0;

        for (var step = 1; step <= longest; step++)
        {
            Show(model, carriers, step);

            foreach (var row in model.DigitalDecodes.Where(r => r.IsTextOnly).ToList())
            {
                // **NO FT8 MENU AND NO LOG ON ANY PSK31 ROW**, which is what this assertion
                // has always been for: the FT8 options are 77-bit message shapes and none of
                // them is a thing to send on PSK31. **UNCHANGED BY UNIT 378.**
                Assert.Null(model.SendMenuFor(row));
                Assert.False(model.CanLogRow(row));

                // **THE RIGHT-CLICK OPENS THE CARD FIRST AND BUILDS THE MENU AFTERWARDS**
                // (R29, `OnDecodedRowContextRequested`). That order is Tim's, the card is what
                // the report and confirm lines are offered off, so the menu measured here is
                // the menu a right-click actually produces rather than half of one.
                model.OpenPsk31CardCommand.Execute(row);

                var flyout = MainWindow.SendFlyoutFor(model, row);
                var station = model.Psk31StationOn(row);

                if (station is null)
                {
                    // **A ROW THAT NAMES NOBODY STILL OFFERS NO SEND LINE**, and that has not
                    // changed: free text and a damaged callsign are not stations to send to.
                    //
                    // **REWRITTEN UNDER R12 BY WORK INSTRUCTION 387 TASK 4 (criterion 10.2).**
                    // It asserted `Assert.Null(flyout)`; R46(b), 2026-09-22, off Tim's own
                    // screen, says a right-click on ANY decoded row opens the menu. The later
                    // ruling wins, and what it opens on these rows is exactly `Capture` and
                    // *make a card anyway* - **asserted here to be two lines and neither of
                    // them a send**, so the thing this assertion protected is protected harder.
                    Assert.NotNull(flyout);

                    var bare = flyout!.Items.OfType<MenuItem>().ToList();

                    Assert.Equal(2, bare.Count);
                    Assert.StartsWith("Capture", bare[0].Header as string ?? "", StringComparison.Ordinal);
                    Assert.StartsWith("Make a card anyway", bare[1].Header as string ?? "", StringComparison.Ordinal);
                    Assert.All(bare, item => Assert.NotSame(model.SendMessageCommand, item.Command));

                    looked++;

                    continue;
                }

                named++;

                // **EVERY ROW THAT NAMES A STATION HAS A MENU** (criterion 7.1). Before unit
                // 378 this was null on every row that was not a certain CQ.
                Assert.NotNull(flyout);

                var items = flyout!.Items.OfType<MenuItem>().ToList();

                offered++;

                if (model.Psk31CqOn(row) is null)
                {
                    notACqAndOffered++;
                }

                // **REWRITTEN UNDER R12 BY WORK INSTRUCTION 385 TASK 2 (criterion 9.2).** It
                // asserted the seven on every row that names a station. While that station is
                // mid-over - his carrier up and nothing handed back - the menu is now one note
                // saying he is still sending, because this menu greys nothing by its own ruling
                // of 2026-09-06 and a note cannot be hit. **What it guards now is both halves**:
                // the seven where he is not sending, the one note where he is, and never a
                // pressable line on top of a man mid-over.
                // **AND SINCE R46(b) EVERY MENU CARRIES A TAIL OF TWO** (criterion 10.2, work
                // instruction 387 task 4): `Capture` and *make a card anyway*, on every decoded
                // row, behind a rule because neither is a send. The send lines are what comes
                // before them and they are counted below without it.
                Assert.Equal(2, TheTail(items));

                var sendLines = items.Count - 2;

                if (model.HisCarrierIsLive(station))
                {
                    held++;

                    // **A NOTE IS A MENU ITEM WITH NO COMMAND AND NO HIT TEST**, which is this
                    // menu's own way of saying a thing is not on offer.
                    Assert.Equal(1, sendLines);

                    var note = items[0];

                    Assert.Null(note.Command);
                    Assert.False(note.IsHitTestVisible);
                    Assert.Contains(
                        Ft8ContactCard.HeIsStillSending,
                        note.Header as string ?? "",
                        StringComparison.Ordinal);

                    continue;
                }

                // **THE SEVEN, IN THE FILE'S OWN ORDER, WITH R39's LABELS.**
                Assert.Equal(canned.Lines.Count, sendLines);

                for (var at = 0; at < sendLines; at++)
                {
                    var header = items[at].Header as string ?? "";

                    Assert.StartsWith(canned.Lines[at].Label, header, StringComparison.Ordinal);

                    // **NOTHING IS GREYED, HIDDEN OR DISABLED** (ruled 2026-09-06) **EXCEPT THE
                    // LINES R46(b) NAMES** - the ones that cannot be sent because Hamlet does not
                    // know the operator's own callsign, which are drawn grey and carry the word
                    // (work instruction 387 section 6 ruling 2(a) item 1). Either way the line
                    // cannot be hit and either way it says why.
                    if (items[at].Command is null)
                    {
                        Assert.True(
                            header.Contains(" - not offered: ", StringComparison.Ordinal)
                            || (!items[at].IsEnabled
                                && header.Contains("callsign", StringComparison.OrdinalIgnoreCase)),
                            "[" + header + "] carries no command but is neither a note saying why "
                            + "nor a line drawn grey for want of his callsign.");

                        Assert.False(items[at].IsHitTestVisible && items[at].IsEnabled);
                    }
                    else
                    {
                        Assert.Equal(canned.Lines[at].Label, header);
                        Assert.True(items[at].IsEnabled);
                        Assert.True(items[at].IsHitTestVisible);
                    }

                    // **NO FT8 MESSAGE IS EVER ON ONE OF THESE ITEMS**, which is the other half
                    // of `SendMenuFor` being null: `SendMessageCommand` is the entry point that
                    // arms an FT8 message, and nothing on a PSK31 row may carry it.
                    Assert.NotSame(model.SendMessageCommand, items[at].Command);
                }

                // **THE ANSWER LINE, ON A CERTAIN CQ, STILL REACHES THE COMMAND AND THE
                // PARAMETER IT ALWAYS DID** - by identity, both of them - and §R1's certainty
                // gate is intact: on a row that is not a certain CQ that line is a note saying
                // so. **Since unit 383 it reaches them one step in** (criterion 7.2, work
                // instruction 383 section 6 ruling 2 item 3): the item carries the command that
                // MARKS a press as having come off the canned list, and that press carries
                // `AnswerPsk31Command` itself with the row itself. The send is still made by
                // `AnswerPsk31Command`, and nothing on this menu routes a macro row through
                // `SendCannedPsk31Command`, which is asserted here too.
                var answer = items[0];

                if (model.Psk31CqOn(row) is not null)
                {
                    Assert.Same(model.SendCannedMacroPsk31Command, answer.Command);

                    var press = Assert.IsType<Psk31CannedMacroPress>(answer.CommandParameter);

                    Assert.Same(model.AnswerPsk31Command, press.Send);
                    Assert.Same(row, press.Parameter);
                    Assert.NotSame(model.SendCannedPsk31Command, press.Send);
                }
                else
                {
                    Assert.Null(answer.Command);
                }

                looked++;
            }

            // **A W1AW CARD OFFERS ONLY WHAT THE ENGINE OFFERED, AND NEVER AN FT8 MESSAGE**
            // (§R12, work instruction 323 task 3; narrowed once already as unit 320's item
            // 39). It read *no action at all*, which was the shut door. What matters is that
            // the FT8 ledger already knows W1AW, so a card that reached the FT8 send options
            // would offer to arm an FT8 transmission on a PSK31 frequency - and that is what
            // cannot happen: every action on a PSK31 card is `Psk31Offer`'s macro. **When the
            // Log appears is not asserted here** - this panel is fed both halves of each
            // transcript as though every line had been heard, so several of these exchanges
            // are finished; `ThePsk31ExchangeTests` drives the Log from clicks (§R14).
            foreach (var card in model.DigitalCards.Where(c => c.IsPsk31))
            {
                // **THE OTHER WAY ROUND IS NOT ASSERTED**, and that is a finding rather than
                // a gap: with no name or location in Settings the report cannot be built at
                // all, so a card can offer `Report` and still carry no message - which is
                // exactly this panel, whose operator profile has neither.
                Assert.True(
                    card.ActionMessage.Length == 0 || card.Offered != Psk31Macro.None,
                    card.Callsign + " carries a message it was never offered");
            }
        }

        _output.WriteLine("PSK31 rows right-clicked: " + looked + ", cards on the panel: " + model.DigitalCards.Count
            + ", send line [" + model.DigitalSendLine + "]");

        _output.WriteLine("rows naming a station: " + named + ", of those with a menu: " + offered
            + ", of those NOT a certain CQ and offered anyway: " + notACqAndOffered);

        // **THE CRITERION'S OWN NUMBER.** Every row that names a station has a menu, and some of
        // them are not certain CQs - which is the whole of what unit 378 changed here. Both are
        // asserted rather than printed, so a later unit that narrows the menu back to CQ rows
        // turns this red.
        Assert.Equal(named, offered);
        Assert.True(notACqAndOffered > 0, "no row that names a station without being a certain CQ was offered a menu");

        // **NOBODY CLICKED, SO NOTHING IS IN FLIGHT AND NOTHING IS BEING SENT** (§0.2).
        // **This read `CanAnswerRowsForTests` too** - `CanTransmitIn`, the shut door - and
        // that assertion is rewritten out under §R12 by the unit that opens the door: the
        // mode being able to send is not the same claim as a row offering a send, and the
        // loop above has just proved the second over every row on the panel.
        Assert.False(model.HasSomethingToStop);
        Assert.DoesNotContain("Sending", model.DigitalSendLine, StringComparison.Ordinal);
    }

    /// <summary>One transcript carried on one channel, a line at a time.</summary>
    private sealed class Carrier
    {
        public Carrier(int id, double hz, Psk31CorpusTranscript transcript)
        {
            Id = id;
            Hz = hz;
            Transcript = transcript;
        }

        public int Id { get; }

        public double Hz { get; }

        public Psk31CorpusTranscript Transcript { get; }

        public string HzCell => Hz.ToString("0", CultureInfo.InvariantCulture);

        /// <summary>The channel as a listener would list it after this many lines.</summary>
        public Psk31Channel Through(int lines)
            => new(Id, Hz, 10.0, string.Concat(Transcript.Lines.Take(lines).Select(l => l.Text + "\n")));

        /// <summary>The last line up to here that handed the turn over, or null.</summary>
        public Psk31CorpusLine? LatestMessage(int lines)
            => Transcript.Lines.Take(lines).LastOrDefault(l => l.Turnover);
    }

    private static List<Carrier> Carriers(Psk31Corpus corpus)
        => corpus.Transcripts.Select((t, i) => new Carrier(i + 1, 500 + (i * 250), t)).ToList();

    private static void Show(MainWindowViewModel model, List<Carrier> carriers, int step)
        => model.ShowPsk31ChannelsForTests(carriers.Select(c => c.Through(Math.Min(step, c.Transcript.Lines.Count))).ToList());

    private static DigitalDecodeRow RowOf(MainWindowViewModel model, Carrier carrier)
        => model.DigitalDecodes.Single(r => r.IsTextOnly && r.Hz == carrier.HzCell);

    private static MainWindowViewModel Panel(
        NudgeSet? nudges = null, Dictionary<string, AdifContact>? worked = null, string mode = "PSK31")
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Psk31Corpus.Load().Operator;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(worked ?? new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.UseNudgeSetForTests(nudges ?? Log());
        model.ChooseDigitalModeCommand.Execute(mode);

        return model;
    }

    /// <summary>A position in which exactly these stations have been worked, from the cited table.</summary>
    private static NudgeSet Log(params string[] workedCallsigns)
    {
        var entities = workedCallsigns
            .Select(DxccPrefixes.EntityOf)
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .ToList();

        return new NudgeSet(entities, entities.Select(DxccContinents.Of));
    }

    private static DateTime Slot(string at)
        => DateTime.ParseExact(
            "2026-09-11 " + at, "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

    /// <summary>
    /// **How many of the last items are R46(b)'s two** - `Capture` and *make a card anyway*,
    /// which every decoded row carries since work instruction 387 task 4 (criterion 10.2).
    /// </summary>
    /// <param name="items">The menu's items, in order.</param>
    /// <returns>Two where both are there, so the caller can assert it.</returns>
    /// <remarks>
    /// **IT COUNTS RATHER THAN TRIMMING**, so a menu that lost one of the two fails on the count
    /// instead of quietly being read as one send line longer.
    /// </remarks>
    private static int TheTail(IReadOnlyList<MenuItem> items)
    {
        if (items.Count < 2)
        {
            return items.Count;
        }

        var capture = (items[^2].Header as string ?? "")
            .StartsWith("Capture", StringComparison.Ordinal);
        var card = (items[^1].Header as string ?? "")
            .StartsWith("Make a card anyway", StringComparison.Ordinal);

        return (capture ? 1 : 0) + (card ? 1 : 0);
    }
}
