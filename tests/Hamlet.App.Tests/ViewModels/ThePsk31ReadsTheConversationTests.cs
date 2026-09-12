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
/// the parser - and the CQ filter, the operator's side, the fade, the country and the quill are
/// the FT8 code, asked of it.</para>
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

    /// <summary>**Assertion 1: the CQ filter selects exactly the rows whose latest message is a CQ.**</summary>
    /// <remarks>
    /// **ALL EIGHT CHANNELS AT ONCE**, stepped a line at a time together, so *exactly* is asked of a
    /// list holding every kind the corpus has side by side.
    /// </remarks>
    [Fact]
    public void TheCqFilterSelectsExactlyTheRowsWhoseLatestMessageIsACq()
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
                var wantCq = latest?.ExpectedKind == Psk31LineKind.Cq;
                var shown = model.DigitalVisibleDecodes.Contains(row);
                var key = (latest?.Kind ?? "no message") + (shown ? " in" : " out");

                tally[key] = tally.GetValueOrDefault(key) + 1;

                Assert.True(
                    wantCq == shown,
                    carrier.Transcript.Name + " after line " + step + ": latest " + (latest?.Kind ?? "none")
                    + ", in the CQ filter " + shown);

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
            Assert.DoesNotContain(row, model.DigitalVisibleDecodes);
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
        Assert.DoesNotContain(parsed, model.DigitalVisibleDecodes);
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
            ("src/Hamlet.RadioEngine/Contacts/NudgeSet.cs", "a70dca061116989295c9ac248e1a606650831bb2fc3a3a1c1df73548c14d903b"),
            ("src/Hamlet.App/ViewModels/NudgeWords.cs", "1767bd4ebceec60a7e240515e61bac2e51ec2ca0575d40f14eb7e110eaf84fa4"),
            // **MOVED BY §R16, NOT DROPPED** (work instruction 325 task 4). Tim
            // ruled on 2026-09-11 that the quill has two forms and no cap, so this
            // file gained `AchievementMarkForm`, the door's amber, and a ring that
            // a counter does not draw. The pin exists to catch an *incidental*
            // change to a control these rows reuse; a change made under a ruling
            // is the one case where re-pinning is the right answer, and the reason
            // is written here so the next reader can tell the two apart.
            ("src/Hamlet.App/Controls/AchievementMarkControl.cs", "944ce75f4b572e0e4a6318ab0068ec86ef43a60326e2b18ff6ea15e419def0d1"),
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
        foreach (var body in new[]
        {
            "public bool HasWorkedBefore => _workedBefore.Length > 0;",
            "public double RowOpacity => HasWorkedBefore || HeardNotReadable ? 0.55 : 1.0;",
        })
        {
            var count = rowSource.Split(body).Length - 1;

            _output.WriteLine(body + " found " + count + " time(s)");

            Assert.Equal(1, count);
        }
    }

    /// <summary>**Assertion 8: no click on a PSK31 row reaches a send path.**</summary>
    /// <remarks>
    /// **THE FT8 LEDGER IS GIVEN W1AW FIRST**, from an FT8 CQ, so the right-click menu and the
    /// conversation card would both exist for a PSK31 row from W1AW if the guard were missing.
    /// </remarks>
    [Fact]
    public void NoClickOnAPsk31RowReachesASendPath()
    {
        var corpus = Psk31Corpus.Load();
        var model = Panel();

        model.CardsNowForTests = DateTime.UtcNow;

        var ft8Row = model.AddDecodeRowForTests(
            "021115", "-09", "0.2", "1240", "CQ W1AW FN31", Slot("02:11:15"), heardOnHz: 14_074_000);

        _output.WriteLine("an FT8 row from W1AW has a menu: " + (model.SendMenuFor(ft8Row) is not null));

        Assert.NotNull(model.SendMenuFor(ft8Row));

        var carriers = Carriers(corpus);
        var longest = carriers.Max(c => c.Transcript.Lines.Count);
        var looked = 0;

        for (var step = 1; step <= longest; step++)
        {
            Show(model, carriers, step);

            foreach (var row in model.DigitalDecodes.Where(r => r.IsTextOnly).ToList())
            {
                // **NO FT8 MENU AND NO LOG ON ANY PSK31 ROW**, which is what this assertion
                // has always been for: the FT8 options are 77-bit message shapes and none of
                // them is a thing to send on PSK31.
                Assert.Null(model.SendMenuFor(row));
                Assert.False(model.CanLogRow(row));

                // **AND THE ONLY THING A ROW EVER OFFERS IS ONE ANSWER, ON A CERTAIN CQ**
                // (§R12, work instruction 323 task 3). This asserted the flyout was always
                // null, which is the shut door; the door is open from unit 323 and answering
                // a station calling CQ is step 4's criterion. A row that is not a certain CQ
                // still offers nothing at all.
                var flyout = MainWindow.SendFlyoutFor(model, row);

                if (model.Psk31CqOn(row) is null)
                {
                    Assert.Null(flyout);
                }
                else
                {
                    var item = Assert.Single(flyout!.Items.OfType<MenuItem>());

                    Assert.Same(model.AnswerPsk31Command, item.Command);
                    Assert.Same(row, item.CommandParameter);
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
}
