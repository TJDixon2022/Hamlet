using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 286, task 2: a badge announces itself, and cannot cost a contact.
/// </summary>
/// <remarks>
/// <para>**TIM OVERRULED THE AUTHOR ON 2026-09-08.** A dialog appears naming what was
/// earned. **He is at 8 contacts and the next it can fire on is 10.**</para>
/// <para>**WHAT UNIT 278 BUILT IS UNTOUCHED**: the thresholds, the once-per-rank rule,
/// the naming of every rank a jump passes, and the silent seeding on a first look at a
/// log that already holds contacts. Every one of those is asserted here as well as the
/// dialog, because the dialog is the thing most likely to break them.</para>
/// </remarks>
public sealed class TheBadgeAnnouncesItselfTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the awards are printed.</param>
    public TheBadgeAnnouncesItselfTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Crossing ten announces once; the same count again says nothing.**</summary>
    /// <remarks>
    /// Watched failing first: before the event existed nothing was raised at all, and
    /// this could not compile.
    /// </remarks>
    [AvaloniaFact]
    public void CrossingTenAnnouncesOnceAndNotAgain()
    {
        // Seeded at 8, which is where he actually is.
        var panel = Panel(seededAt: 8);

        var awards = Watch(panel);

        panel.AnnounceBadgesForTests(10);

        _output.WriteLine("at 10 : " + Describe(awards));

        Assert.Single(awards);
        Assert.Equal(10, awards[0].Count);
        Assert.Equal(new[] { 10 }, awards[0].Crossed);

        // The same count again, and the same rank: nothing more to say.
        panel.AnnounceBadgesForTests(10);
        panel.AnnounceBadgesForTests(11);

        _output.WriteLine("at 10 and 11 again : " + Describe(awards));

        Assert.Single(awards);
    }

    /// <summary>**What it says at ten, quoted.**</summary>
    [AvaloniaFact]
    public void WhatItSaysAtTen()
    {
        var award = new BadgeAward(10, new[] { 10 });

        _output.WriteLine(award.Heading);
        _output.WriteLine(award.Says);
        _output.WriteLine(award.Next);

        Assert.Equal("yellow belt", award.Heading);
        Assert.Equal("That is 10 contacts logged.", award.Says);
        Assert.Equal("15 more and the ring turns orange.", award.Next);
    }

    /// <summary>**Nine to twenty-six names both, not only the higher.**</summary>
    /// <remarks>
    /// Unit 278's rule. A quiet evening on FT8 puts him past two at once, and
    /// reporting only the top one would swallow a milestone he actually reached.
    /// </remarks>
    [AvaloniaFact]
    public void NineToTwentySixNamesBoth()
    {
        var panel = Panel(seededAt: 0);

        var awards = Watch(panel);

        panel.AnnounceBadgesForTests(26);

        _output.WriteLine(Describe(awards));

        Assert.Single(awards);
        Assert.Equal(new[] { 10, 25 }, awards[0].Crossed);
        Assert.Equal("That is 10 and 25 contacts logged.", awards[0].Says);
        Assert.Equal("orange belt", awards[0].Heading);
    }

    /// <summary>**A fresh log at forty announces nothing on the first look.**</summary>
    /// <remarks>
    /// Unit 278 built that seeding deliberately, and this order says not to undo it.
    /// Hamlet installed beside a log it was not there for must not congratulate him
    /// for badges it never saw him earn.
    /// </remarks>
    [AvaloniaFact]
    public void AFreshLogAtFortyAnnouncesNothing()
    {
        var panel = Panel(seededAt: null);

        var awards = Watch(panel);

        panel.AnnounceBadgesForTests(40);

        _output.WriteLine("first look at 40 : " + Describe(awards));

        Assert.Empty(awards);

        // And the next real crossing does fire, so seeding silences the past and
        // not the future.
        panel.AnnounceBadgesForTests(50);

        _output.WriteLine("then 50 : " + Describe(awards));

        Assert.Single(awards);
        Assert.Equal(new[] { 50 }, awards[0].Crossed);
    }

    /// <summary>**The notice takes no focus and blocks nothing.**</summary>
    /// <remarks>
    /// <para>**THE ONE THING THAT HAS TO BE TRUE OF THE WINDOW.** He is often
    /// mid-exchange with fourteen seconds to reply, and a window that activates closes
    /// an open right-click menu — which is exactly how a dialog would cost him the
    /// contact it is congratulating him for.</para>
    /// <para>Asserted on the window rather than reasoned about, because
    /// `ShowActivated` is the whole mechanism and a default is easy to lose.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheNoticeTakesNoFocusAndBlocksNothing()
    {
        var notice = new BadgeWindow { DataContext = new BadgeAward(10, new[] { 10 }) };

        _output.WriteLine("ShowActivated : " + notice.ShowActivated);
        _output.WriteLine("ShowInTaskbar : " + notice.ShowInTaskbar);
        _output.WriteLine("Topmost       : " + notice.Topmost);
        _output.WriteLine("stays         : " + BadgeWindow.Stays.TotalSeconds + " s");

        Assert.False(
            notice.ShowActivated,
            "the notice activates, so it would take his focus and close an open menu");

        Assert.False(notice.ShowInTaskbar, "the notice is a notice, not a place");

        // **AND IT LEAVES ON ITS OWN**, so it is never a click he did not ask for.
        Assert.InRange(BadgeWindow.Stays.TotalSeconds, 4, 15);
    }

    /// <summary>**And it draws what it was given.**</summary>
    [AvaloniaFact]
    public void TheNoticeDrawsTheRankAndTheCount()
    {
        var notice = new BadgeWindow { DataContext = new BadgeAward(10, new[] { 10 }) };

        notice.Show();
        HowMuchTheApplicationSaysTests.Pump(notice);

        var shown = HowMuchTheApplicationSaysTests.Shown(notice).ToList();

        foreach (var t in shown)
        {
            _output.WriteLine("  " + t);
        }

        Assert.Contains("yellow belt", shown);
        Assert.Contains("That is 10 contacts logged.", shown);
        Assert.Contains("10", shown);

        notice.Close();
    }

    /// <summary>A panel whose badge level has been seeded.</summary>
    /// <param name="seededAt">
    /// The highest rank already announced, or null for a log never looked at.
    /// </param>
    /// <returns>The panel.</returns>
    /// <remarks>
    /// **THE SEED IS WHAT UNIT 278 STORES**, so a null here is a genuine first look
    /// and not a fixture pretending to be one.
    /// </remarks>
    private static MainWindowViewModel Panel(int? seededAt)
    {
        // Nothing on disk, so the count comes only from what the test hands over.
        File.WriteAllText(ContactLogStore.LogPath, AdifLog.Header("1.12.206"));

        var settings = HowMuchTheApplicationSaysTests.Settled();

        settings.ContactBadgeAnnounced = seededAt ?? -1;

        return new MainWindowViewModel(settings, null) { OperatingMode = "Digital" };
    }

    /// <summary>Collect every award a panel raises.</summary>
    /// <param name="panel">The panel.</param>
    /// <returns>The list, which fills as awards arrive.</returns>
    private static List<BadgeAward> Watch(MainWindowViewModel panel)
    {
        var awards = new List<BadgeAward>();

        panel.BadgeEarned += (_, award) => awards.Add(award);

        return awards;
    }

    /// <summary>The awards, for the printout.</summary>
    /// <param name="awards">What was raised.</param>
    /// <returns>Something readable.</returns>
    private static string Describe(IReadOnlyList<BadgeAward> awards)
        => awards.Count == 0
            ? "nothing"
            : string.Join(
                " | ",
                awards.Select(a =>
                    a.Count.ToString(CultureInfo.InvariantCulture) + " crossing ["
                    + string.Join(", ", a.Crossed) + "] " + a.Heading));
}
