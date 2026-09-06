using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 252, task 1: `CQ` and `mine` are independent toggles, both
/// can be on at once, and `everything` is the state where neither is.
/// </summary>
/// <remarks>
/// <para>**THIS SUPERSEDES UNIT 251'S THREE EXCLUSIVE CHOICES** (Tim's ruling,
/// 2026-09-06). An enum could not carry the state he wants most evenings: the
/// calls he could answer AND his own traffic, at the same time.</para>
/// <para>**THE PERSISTED CHOICE IS CARRIED FORWARD RATHER THAN RESET.** A
/// settings file written by unit 251 names one exclusive filter; §6.1's second
/// exception says a rename that changes a stored key ships with a migration and a
/// test proving an existing profile survives it, and
/// <see cref="AUnit251FileKeepsTheChoiceItWasLeftOn"/> is that test.</para>
/// </remarks>
public sealed class TheDecodedListFiltersByCategoryTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rows and the summary are printed.</param>
    public TheDecodedListFiltersByCategoryTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>`CQ` alone keeps the calls to anyone and nothing else.</summary>
    [Fact]
    public void CqKeepsEveryCallToAnyone()
    {
        var model = WithRows(mine: "KD9ABC");

        model.ShowsCqOnly = true;

        Print(model);

        Assert.True(Wanted(model, "CQ TA3MPK KM39"));
        Assert.True(Wanted(model, "CQ DX EA3QQ JN11"));

        Assert.False(Wanted(model, "KE9COB N5CH R+14"));
        Assert.False(Wanted(model, "KD9ABC W4WTM -07"));
        Assert.False(Wanted(model, "TNX FER QSO OM"));
    }

    /// <summary>`mine` alone keeps his traffic, sent and received.</summary>
    /// <remarks>
    /// **EITHER FIELD, BECAUSE IT IS HIS TRAFFIC AND NOT HIS INBOX.** A contact
    /// is two sides, and a list of what was said to him with his own half missing
    /// is half a conversation.
    /// </remarks>
    [Fact]
    public void MineKeepsWhatHeSentAsWellAsWhatHeWasSent()
    {
        var model = WithRows(mine: "KD9ABC");

        model.ShowsMine = true;

        Print(model);

        // Addressed to him.
        Assert.True(Wanted(model, "KD9ABC W4WTM -07"));

        // Sent by him. This is the half unit 251 could not see, because its
        // predicate read the to-field alone.
        Assert.True(Wanted(model, "W4WTM KD9ABC R-11"));

        Assert.False(Wanted(model, "CQ TA3MPK KM39"));
        Assert.False(Wanted(model, "KE9COB N5CH R+14"));
    }

    /// <summary>
    /// Both on is the union: every CQ, plus his own traffic, and nothing else.
    /// </summary>
    /// <remarks>
    /// **THE STATE THE ENUM COULD NOT HOLD, AND THE REASON THIS TASK EXISTS**
    /// (Tim's ruling, 2026-09-06: *when both are on, that is all he wants to
    /// see*). It is the union rather than the intersection, which would be his
    /// own callsign addressed to `CQ` and is empty on every real band.
    /// </remarks>
    [Fact]
    public void BothOnShowsTheCqsAndHisOwnTrafficAndNothingElse()
    {
        var model = WithRows(mine: "KD9ABC");

        model.ShowsCqOnly = true;
        model.ShowsMine = true;

        Print(model);

        Assert.True(Wanted(model, "CQ TA3MPK KM39"));
        Assert.True(Wanted(model, "CQ DX EA3QQ JN11"));
        Assert.True(Wanted(model, "KD9ABC W4WTM -07"));
        Assert.True(Wanted(model, "W4WTM KD9ABC R-11"));

        // Somebody else's contact, and free text with no fields at all. Neither
        // toggle asked for either.
        Assert.False(Wanted(model, "KE9COB N5CH R+14"));
        Assert.False(Wanted(model, "TNX FER QSO OM"));

        Assert.False(model.ShowsEverything);
    }

    /// <summary>Neither on is `everything`, and it is the fresh-file state.</summary>
    [Fact]
    public void NeitherToggleIsEverything()
    {
        var model = WithRows(mine: "KD9ABC");

        Assert.True(model.ShowsEverything);
        Assert.False(model.ShowsCqOnly);
        Assert.False(model.ShowsMine);

        foreach (var row in model.DigitalDecodes)
        {
            Assert.True(Wanted(model, row.Message));
        }
    }

    /// <summary>Pressing `everything` clears both toggles.</summary>
    /// <remarks>
    /// **IT IS NOT A THIRD CHOICE THAT COULD DISAGREE WITH THEM.** `everything`
    /// is what neither toggle being on already is, so the button that appears to
    /// select it clears both and `ShowsEverything` is derived rather than stored.
    /// </remarks>
    [Fact]
    public void EverythingClearsBothToggles()
    {
        var model = WithRows(mine: "KD9ABC");

        model.ShowsCqOnly = true;
        model.ShowsMine = true;

        model.ShowEveryDecodeCommand.Execute(null);

        Assert.False(model.ShowsCqOnly);
        Assert.False(model.ShowsMine);
        Assert.True(model.ShowsEverything);

        // And pressing it again is a no-op rather than an error, because the
        // control is never disabled (§0.5.1).
        model.ShowEveryDecodeCommand.Execute(null);

        Assert.True(model.ShowsEverything);
    }

    /// <summary>The two toggles do not turn each other off.</summary>
    [Fact]
    public void TheTogglesAreIndependentOfEachOther()
    {
        var model = WithRows(mine: "KD9ABC");

        model.ToggleDecodedCqCommand.Execute(null);
        Assert.True(model.ShowsCqOnly);
        Assert.False(model.ShowsMine);

        model.ToggleDecodedMineCommand.Execute(null);

        // The whole point: turning `mine` on did not turn `CQ` off.
        Assert.True(model.ShowsCqOnly);
        Assert.True(model.ShowsMine);

        model.ToggleDecodedCqCommand.Execute(null);

        Assert.False(model.ShowsCqOnly);
        Assert.True(model.ShowsMine);
    }

    /// <summary>Both toggles are remembered between evenings, separately.</summary>
    [Fact]
    public void BothTogglesSurviveTheSettingsFile()
    {
        var settings = new AppSettings();
        var model = new MainWindowViewModel(settings, null);

        model.ShowsCqOnly = true;
        model.ShowsMine = true;

        Assert.True(settings.DecodedShowCq);
        Assert.True(settings.DecodedShowMine);

        var path = Path.Combine(
            Path.GetTempPath(),
            "hamlet-unit252-filter-" + Guid.NewGuid().ToString("N") + ".json");

        try
        {
            SettingsStore.SaveTo(settings, path);

            var reloaded = SettingsStore.LoadFrom(path);
            var reopened = new MainWindowViewModel(reloaded, null);

            _output.WriteLine(
                "reopened on : CQ=" + reopened.ShowsCqOnly
                + " mine=" + reopened.ShowsMine);

            Assert.True(reopened.ShowsCqOnly);
            Assert.True(reopened.ShowsMine);
            Assert.False(reopened.ShowsEverything);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>A unit 251 settings file keeps the filter it was left on.</summary>
    /// <remarks>
    /// **§6.1's SECOND EXCEPTION, WHICH IS THE ONE THAT BITES.** Unit 251 shipped
    /// `DecodedFilter` as one string. Dropping it would take an operator who had
    /// left the panel on `CQ only` back to `everything` on his next launch with
    /// nothing on screen to say why, and a settings reset that looks like the app
    /// forgetting him is exactly what that clause was written about.
    /// </remarks>
    [Theory]
    [InlineData("CqOnly", true, false)]
    [InlineData("Mine", false, true)]
    [InlineData("Everything", false, false)]
    [InlineData("OnlyTheInterestingOnes", false, false)]
    public void AUnit251FileKeepsTheChoiceItWasLeftOn(
        string legacy, bool cq, bool mine)
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "hamlet-unit251-legacy-" + Guid.NewGuid().ToString("N") + ".json");

        try
        {
            // Written the way unit 251 wrote it: the one key, and neither of the
            // two this unit reads.
            File.WriteAllText(
                path,
                "{ \"DecodedFilter\": \"" + legacy + "\" }");

            var reloaded = SettingsStore.LoadFrom(path);
            var reopened = new MainWindowViewModel(reloaded, null);

            _output.WriteLine(
                legacy + " -> CQ=" + reopened.ShowsCqOnly
                + " mine=" + reopened.ShowsMine);

            Assert.Equal(cq, reopened.ShowsCqOnly);
            Assert.Equal(mine, reopened.ShowsMine);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// A file that already carries the two toggles is not overwritten by the old
    /// key.
    /// </summary>
    /// <remarks>
    /// **OTHERWISE TURNING A FILTER OFF WOULD NOT STICK.** An operator who
    /// deliberately turned `CQ` back off would have his own settings file turn it
    /// on again at every launch, which is worse than the reset the migration
    /// exists to prevent.
    /// </remarks>
    [Fact]
    public void TheMigrationDoesNotFightAFileThatAlreadyHasTheToggles()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "hamlet-unit252-nofight-" + Guid.NewGuid().ToString("N") + ".json");

        try
        {
            File.WriteAllText(
                path,
                "{ \"DecodedFilter\": \"CqOnly\", "
                + "\"DecodedShowCq\": false, \"DecodedShowMine\": false }");

            var reopened = new MainWindowViewModel(SettingsStore.LoadFrom(path), null);

            Assert.False(reopened.ShowsCqOnly);
            Assert.True(reopened.ShowsEverything);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Work instruction 252, task 2: a filtered row is off the list, and the
    /// summary says how many.
    /// </summary>
    /// <remarks>
    /// **THE SUMMARY IS THE WHOLE DEFENCE AGAINST §0.0 HERE.** A filter that
    /// removes can make a busy band look like a quiet one, and a picture binds as
    /// hard as a sentence (HM-DEC-092). Every state below is asserted with both
    /// numbers, because a shown-count without a hidden-count is the fault.
    /// </remarks>
    [Fact]
    public void TheSummaryCountsWhatIsShownAndWhatIsHidden()
    {
        var model = WithRows(mine: "KD9ABC");

        // Neither toggle: six heard, six shown, nothing hidden, and no "hidden"
        // clause at all because there is nothing to warn about.
        Assert.Equal(6, model.DigitalShownCount);
        Assert.Equal(0, model.DigitalHiddenCount);
        Assert.Equal(6, model.DigitalVisibleDecodes.Count);
        Assert.DoesNotContain("hidden", model.DigitalDecodedSummary);
        _output.WriteLine("neither : " + model.DigitalDecodedSummary);

        model.ShowsCqOnly = true;

        Assert.Equal(2, model.DigitalShownCount);
        Assert.Equal(4, model.DigitalHiddenCount);
        Assert.Equal(2, model.DigitalVisibleDecodes.Count);
        Assert.Contains("2 shown", model.DigitalDecodedSummary);
        Assert.Contains("4 hidden by CQ", model.DigitalDecodedSummary);
        _output.WriteLine("CQ      : " + model.DigitalDecodedSummary);

        model.ShowsCqOnly = false;
        model.ShowsMine = true;

        Assert.Equal(2, model.DigitalShownCount);
        Assert.Equal(4, model.DigitalHiddenCount);
        Assert.Contains("4 hidden by mine", model.DigitalDecodedSummary);
        _output.WriteLine("mine    : " + model.DigitalDecodedSummary);

        model.ShowsCqOnly = true;

        // Both: the two CQs and his two, so two are held back.
        Assert.Equal(4, model.DigitalShownCount);
        Assert.Equal(2, model.DigitalHiddenCount);
        Assert.Contains("2 hidden by CQ and mine", model.DigitalDecodedSummary);
        _output.WriteLine("both    : " + model.DigitalDecodedSummary);

        // **THE WHOLE TABLE NEVER SHRANK.** The band was as busy under every one
        // of those states, and the panel still knows it.
        Assert.Equal(6, model.DigitalDecodes.Count);
    }

    /// <summary>A row the toggles do not want never reaches the visible list.</summary>
    /// <remarks>
    /// **AND THAT IS HOW THE SCROLL POSITION IS LEFT ALONE.** `FollowingScroll`
    /// watches the bound collection, so a filtered-out row raising no change on it
    /// is the mechanism — the view has nothing to follow because nothing happened
    /// to the list it is drawing. This asserts the collection did not move, which
    /// is the fact the scroll behaviour rests on and one a headless test can
    /// state without a window.
    /// </remarks>
    [Fact]
    public void AFilteredRowArrivingDoesNotDisturbTheList()
    {
        var model = WithRows(mine: "KD9ABC");

        model.ShowsCqOnly = true;

        var before = model.DigitalVisibleDecodes.ToArray();
        var changed = 0;

        model.DigitalVisibleDecodes.CollectionChanged += (_, _) => changed++;

        // Somebody else's contact, which `CQ` did not ask for.
        model.AddDecodeRowForTests(
            "214150", "-13", "0.3", "1620", "W1ABC K4XYZ RR73");

        Assert.Equal(0, changed);
        Assert.Equal(before, model.DigitalVisibleDecodes);

        // It was still heard, and the summary says so rather than swallowing it.
        Assert.Equal(7, model.DigitalDecodes.Count);
        Assert.Equal(5, model.DigitalHiddenCount);

        // And a row it did ask for arrives on the list, so the silence above is
        // the filter working rather than the table having stopped.
        model.AddDecodeRowForTests(
            "214150", "-08", "0.2", "980", "CQ K4XYZ FM18");

        Assert.Equal(1, changed);
        Assert.Contains(
            model.DigitalVisibleDecodes, r => r.Message == "CQ K4XYZ FM18");
    }

    /// <summary>
    /// The visible list keeps the whole list's order, in both directions.
    /// </summary>
    /// <remarks>
    /// **NEWEST-FIRST IS WHERE THIS COULD GO WRONG.** A new row does not go at
    /// either end of the table then — it goes after the rows already in its own
    /// slot — so the visible list's insert position has to be counted rather than
    /// assumed.
    /// </remarks>
    [Fact]
    public void TheVisibleListKeepsTheWholeListsOrder()
    {
        foreach (var newestFirst in new[] { true, false })
        {
            var model = WithRows(mine: "KD9ABC");

            model.DigitalNewestFirst = newestFirst;
            model.ShowsCqOnly = true;

            model.AddDecodeRowForTests(
                "214150", "-08", "0.2", "980", "CQ K4XYZ FM18");
            model.AddDecodeRowForTests(
                "214150", "-13", "0.3", "1620", "W1ABC K4XYZ RR73");
            model.AddDecodeRowForTests(
                "214150", "-15", "0.1", "1100", "CQ DX VK3ABC QF22");

            var expected = model.DigitalDecodes
                .Where(r => DecodedFilterRule.IsCallToAnyone(r.Addressee))
                .ToArray();

            _output.WriteLine(
                "newest first " + newestFirst + " : "
                + string.Join(" | ", model.DigitalVisibleDecodes.Select(r => r.Message)));

            Assert.Equal(expected, model.DigitalVisibleDecodes);
        }
    }

    /// <summary>Clearing empties both lists.</summary>
    [Fact]
    public void ClearEmptiesTheVisibleListToo()
    {
        var model = WithRows(mine: "KD9ABC");

        model.ShowsCqOnly = true;

        model.ClearDigitalDecodesCommand.Execute(null);

        Assert.Empty(model.DigitalDecodes);
        Assert.Empty(model.DigitalVisibleDecodes);
        Assert.Equal(0, model.DigitalShownCount);
        Assert.Equal(0, model.DigitalHiddenCount);
    }

    /// <summary>
    /// Work instruction 252, task 3: his compound and portable calls are his.
    /// </summary>
    /// <remarks>
    /// <para>**THE RULE IS: STRIP THE SLASHES AND COMPARE THE LONGEST PIECES.**
    /// FT8 puts the prefix or the suffix on the other side of a `/`, so a compound
    /// call is his base call plus one more piece and never his base call with
    /// letters welded onto it.</para>
    /// <para>**WHICH IS WHY IT IS NOT A PREFIX TEST, AND `W1ABCD` IS THE CASE
    /// THAT PROVES IT.** A prefix match would make `W1ABCD` his, and `W1ABCD` is
    /// somebody else entirely — a filter claiming another station's traffic as his
    /// is the fault §0.0 exists for, wearing a helpful face.</para>
    /// <para>**AND IT WORKS BOTH WAYS ROUND**, because he may type `W4/W1ABC` into
    /// settings while operating away from home.</para>
    /// </remarks>
    [Theory]
    // The plain case, and case does not matter.
    [InlineData("W1ABC", "W1ABC", true)]
    [InlineData("W1ABC", "w1abc", true)]
    // Portable and compound suffixes.
    [InlineData("W1ABC", "W1ABC/P", true)]
    [InlineData("W1ABC", "W1ABC/M", true)]
    [InlineData("W1ABC", "W1ABC/QRP", true)]
    // A prefix, which is the other side of the slash.
    [InlineData("W1ABC", "W4/W1ABC", true)]
    [InlineData("W1ABC", "VP2E/W1ABC", true)]
    // He is the one operating portable, so the stored call carries the piece.
    [InlineData("W4/W1ABC", "W1ABC", true)]
    [InlineData("W1ABC/P", "W1ABC", true)]
    [InlineData("W4/W1ABC", "W1ABC/P", true)]
    // **AND THE ONES THAT ARE NOT HIM.** A longer call that merely starts the
    // same way is a different station, and so is a shorter one.
    [InlineData("W1ABC", "W1ABCD", false)]
    [InlineData("W1ABC", "W1AB", false)]
    [InlineData("W1ABC", "W1ABCD/P", false)]
    [InlineData("W1ABC", "KD9ABC", false)]
    [InlineData("W1ABC", "", false)]
    public void HisPortableAndCompoundCallsAreHis(
        string stored, string heard, bool his)
    {
        var got = DecodedFilterRule.IsSameStation(heard, stored);

        _output.WriteLine(
            "stored [" + stored + "] heard [" + heard + "] -> "
            + (got ? "his" : "somebody else"));

        Assert.Equal(his, got);

        // And it reaches the filter the same way round in either field.
        Assert.Equal(
            his,
            DecodedFilterRule.Wants(false, true, heard, "W4WTM", stored));
        Assert.Equal(
            his,
            DecodedFilterRule.Wants(false, true, "W4WTM", heard, stored));
    }

    /// <summary>
    /// With no callsign on file, `mine` holds everything back and says so.
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS A CHANGE FROM UNIT 251 AND IT IS DELIBERATE.** While rows
    /// were dimmed, `mine` with no callsign matched everything — it dimmed nothing
    /// and said so, which was the safe answer then. It is the wrong answer now
    /// that the filter removes: matching everything would make `mine` do something
    /// other than what the control says, and with `CQ` also on it would quietly
    /// turn *both* into *everything*, which Tim's ruling forbids in as many
    /// words.</para>
    /// <para>**THE §0.0 HAZARD IS CARRIED BY THE TWO THINGS ON SCREEN INSTEAD**:
    /// the hidden count, which says the band is as busy as it was, and the amber
    /// note, which names the missing callsign and the screen it is typed on. An
    /// empty list under a line reading `0 shown · 6 hidden by mine` is not a quiet
    /// band, and it cannot be read as one.</para>
    /// </remarks>
    [Fact]
    public void MineWithNoCallsignHoldsEverythingBackAndSaysSo()
    {
        var model = WithRows(mine: "");

        Assert.False(model.HasDigitalFilterNote);

        model.ShowsMine = true;

        _output.WriteLine("note    : " + model.DigitalFilterNote);
        _output.WriteLine("summary : " + model.DigitalDecodedSummary);

        Assert.Empty(model.DigitalVisibleDecodes);
        Assert.Equal(0, model.DigitalShownCount);
        Assert.Equal(6, model.DigitalHiddenCount);

        // **THE BAND IS STILL ON THE RECORD**, which is what makes the empty list
        // readable rather than misleading.
        Assert.Equal(6, model.DigitalDecodes.Count);
        Assert.Contains("0 shown", model.DigitalDecodedSummary);
        Assert.Contains("6 hidden by mine", model.DigitalDecodedSummary);

        // And the note is up, naming what is missing and where it is fixed.
        Assert.True(model.HasDigitalFilterNote);
        Assert.Contains("does not know your callsign", model.DigitalFilterNote);
        Assert.Contains("Settings", model.DigitalFilterNote);

        // The toggle is still on and still available: it was not disabled and it
        // did not silently fall back to something else (§0.5.1).
        Assert.True(model.ShowsMine);
    }

    /// <summary>Typing the callsign in makes `mine` start working.</summary>
    /// <remarks>
    /// **THE POINTER HAS TO BE TRUE.** The note sends him to Settings, under
    /// Operator, and that is the field this writes.
    /// </remarks>
    [Fact]
    public void SettingTheCallsignMakesMineStartMatching()
    {
        var settings = new AppSettings();
        var model = new MainWindowViewModel(settings, null);

        model.AddDecodeRowForTests("214135", "-09", "0.2", "1240", "KD9ABC W4WTM -07");
        model.AddDecodeRowForTests("214135", "-11", "0.2", "1290", "CQ TA3MPK KM39");

        model.ShowsMine = true;

        Assert.Empty(model.DigitalVisibleDecodes);

        settings.Operator.Callsign = "KD9ABC";

        // The panel re-reads the callsign the next time the filter runs, which is
        // what pressing the toggle does.
        model.ShowsMine = false;
        model.ShowsMine = true;

        Assert.False(model.HasDigitalFilterNote);
        Assert.Single(model.DigitalVisibleDecodes);
        Assert.Equal(
            "KD9ABC W4WTM -07", model.DigitalVisibleDecodes[0].Message);
    }

    /// <summary>The predicate itself, over the shapes the two fields take.</summary>
    /// <remarks>
    /// **THE `CQ POTA` ROW IS THE ONE THAT MATTERS**, and it takes two files to
    /// answer: `Ft8Vocabulary.Split` joins `CQ POTA W5LST EM33` into the single
    /// addressee `CQ POTA`, and `IsCallToAnyone` tests what that produced. The
    /// instruction requires that behaviour to survive this unit and it does.
    /// </remarks>
    [Theory]
    // CQ alone.
    [InlineData(true, false, "CQ", "TA3MPK", true)]
    [InlineData(true, false, "CQ DX", "EA3QQ", true)]
    [InlineData(true, false, "CQ POTA", "W5LST", true)]
    [InlineData(true, false, "cq", "TA3MPK", true)]
    [InlineData(true, false, "KD9ABC", "W4WTM", false)]
    [InlineData(true, false, "", "", false)]
    // mine alone, either field.
    [InlineData(false, true, "KD9ABC", "W4WTM", true)]
    [InlineData(false, true, "W4WTM", "KD9ABC", true)]
    [InlineData(false, true, "kd9abc", "W4WTM", true)]
    [InlineData(false, true, "CQ", "TA3MPK", false)]
    [InlineData(false, true, "KE9COB", "N5CH", false)]
    [InlineData(false, true, "", "", false)]
    // Both, which is the union.
    [InlineData(true, true, "CQ", "TA3MPK", true)]
    [InlineData(true, true, "KD9ABC", "W4WTM", true)]
    [InlineData(true, true, "KE9COB", "N5CH", false)]
    // Neither, which is everything.
    [InlineData(false, false, "", "", true)]
    [InlineData(false, false, "KE9COB", "N5CH", true)]
    public void ThePredicateReadsBothFields(
        bool cq, bool mine, string to, string from, bool wanted)
    {
        var got = DecodedFilterRule.Wants(cq, mine, to, from, "KD9ABC");

        _output.WriteLine(
            "CQ=" + cq + " mine=" + mine
            + " / [" + to + "] [" + from + "] -> "
            + (got ? "shown" : "held back"));

        Assert.Equal(wanted, got);
    }

    /// <summary>Whether a row the panel heard is on the list the operator sees.</summary>
    /// <remarks>
    /// **IT READS THE VISIBLE COLLECTION AND ASSERTS THE ROW IS STILL ON THE
    /// WHOLE ONE.** The second half is what keeps a filter from being confused
    /// with a decoder that stopped: the row was heard either way, and only its
    /// place on the table is in question.
    /// </remarks>
    private static bool Wanted(MainWindowViewModel model, string message)
    {
        Assert.Contains(model.DigitalDecodes, r => r.Message == message);

        return model.DigitalVisibleDecodes.Any(r => r.Message == message);
    }

    private void Print(MainWindowViewModel model)
    {
        foreach (var row in model.DigitalDecodes)
        {
            var shown = model.DigitalVisibleDecodes.Contains(row);

            _output.WriteLine((shown ? "  shown " : "  hidden ") + row.Message);
        }

        _output.WriteLine("summary : " + model.DigitalDecodedSummary);
    }

    /// <summary>Six rows covering every shape the predicate has to read.</summary>
    private static MainWindowViewModel WithRows(string mine)
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = mine;

        var model = new MainWindowViewModel(settings, null);

        var rows = new[]
        {
            // A call to anyone, and a call to anyone with a direction on it.
            ("CQ TA3MPK KM39", "-11"),
            ("CQ DX EA3QQ JN11", "-17"),

            // Two other stations: neither a CQ nor his.
            ("KE9COB N5CH R+14", "-04"),

            // Addressed to him, and sent by him.
            ("KD9ABC W4WTM -07", "-09"),
            ("W4WTM KD9ABC R-11", "-13"),

            // Free text: no three fields at all, so no addressee to match.
            ("TNX FER QSO OM", "-06"),
        };

        foreach (var (message, snr) in rows)
        {
            model.AddDecodeRowForTests("214135", snr, "0.2", "1240", message);
        }

        return model;
    }
}
