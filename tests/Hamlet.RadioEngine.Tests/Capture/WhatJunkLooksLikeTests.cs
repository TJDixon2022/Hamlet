using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Capture;

/// <summary>
/// **WHY THE JUNK FIGURE IS WHAT IT IS** (work instruction 549, task 2): how many letters of one or two elements - E, T, I,
/// A, N, M - ordinary English and Q-code text print, and how the junk W1AW's lost audio printed compares. Text only: no
/// recording is read.
/// </summary>
public sealed class WhatJunkLooksLikeTests
{
    /// <summary>Plain English: the Gettysburg Address, public domain.</summary>
    internal const string English =
        "FOUR SCORE AND SEVEN YEARS AGO OUR FATHERS BROUGHT FORTH ON THIS CONTINENT A NEW NATION CONCEIVED IN LIBERTY AND "
        + "DEDICATED TO THE PROPOSITION THAT ALL MEN ARE CREATED EQUAL NOW WE ARE ENGAGED IN A GREAT CIVIL WAR TESTING WHETHER "
        + "THAT NATION OR ANY NATION SO CONCEIVED AND SO DEDICATED CAN LONG ENDURE WE ARE MET ON A GREAT BATTLE FIELD OF THAT "
        + "WAR WE HAVE COME TO DEDICATE A PORTION OF THAT FIELD AS A FINAL RESTING PLACE FOR THOSE WHO HERE GAVE THEIR LIVES "
        + "THAT THAT NATION MIGHT LIVE IT IS ALTOGETHER FITTING AND PROPER THAT WE SHOULD DO THIS";

    /// <summary>A bulletin's kind of English, as W1AW sends it: the scoreboard's W1AW reference and words like it.</summary>
    internal const string Bulletin =
        "PE II AND TYPE IV RADIO EMISSIONS HOWEVER THIS CME IS EXPECTED TO ARRIVE AT EARTH LATE ON THE SEVENTH "
        + "THE SOLAR FLUX INDEX IS EXPECTED TO REMAIN NEAR ONE HUNDRED FORTY AND THE PLANETARY A INDEX NEAR TEN "
        + "AT ARRL DOT NET EACH STATION HANDLING THIS MESSAGE PLEASE ANTENNA MAINTENANCE IN THE MEANTIME";

    /// <summary>A contact's Q-code and abbreviations.</summary>
    internal const string Qso =
        "CQ CQ CQ DE KC4ZGP KC4ZGP K KC4ZGP DE W1AW TNX FER CALL UR RST 599 599 QTH NEWINGTON CT NAME ED ED HW CPY BK "
        + "R R TNX ED UR 579 QTH NC NAME TIM TIM RIG IC7300 ANT DIPOLE WX SUNNY TEMP 57 BK FB TIM TNX FER QSO 73 ES GL SK EE";

    /// <summary>What W1AW's bulletin printed with one 50 ms chunk lost a second (work instruction 548), and what it should have.</summary>
    internal const string Junk = "TYAEE IEA RADIO EMII EEIOTS";

    private readonly ITestOutputHelper _output;

    public WhatJunkLooksLikeTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void ShortLettersInTenSecondsAndInARow()
    {
        foreach (var (name, text) in new[] { ("English", English), ("bulletin", Bulletin), ("QSO", Qso), ("junk", Junk), ("junk's clean text", "TYPE IV RADIO EMISSIONS") })
        {
            var letters = text.Count(c => c != ' ');
            var shortOnes = text.Count(c => c != ' ' && Elements(c) <= 2);

            _output.WriteLine($"{name}: {shortOnes} of {letters} letters are one or two elements ({100.0 * shortOnes / letters:0}%); "
                + $"longest run of them, spaces not breaking it: {LongestRun(text)}; "
                + $"six or more in a row: {Runs(text, 6)} times");

            foreach (var wpm in new[] { 18, 25, 35 })
            {
                _output.WriteLine($"    at {wpm} WPM the most in any 10 s: {MostInWindow(text, wpm, 10)}");
            }
        }

        // **NEITHER SHAPE TELLS JUNK FROM ENGLISH**: six in any ten seconds fires on plain English at a bulletin's speed, and
        // six in a row fires on English and Q-code text as often as on the junk. Letters standing alone do (the next test).
        Assert.True(MostInWindow(English, 18, 10) >= 6);
        Assert.True(Runs(English, 6) >= 1);
        Assert.True(Runs(Qso, 6) >= 1);
        Assert.True(Runs(Junk, 6) >= 1);
    }

    [Fact]
    public void ShortLettersAmongTheLastFew()
    {
        foreach (var (name, text) in new[] { ("English", English), ("bulletin", Bulletin), ("QSO", Qso), ("junk", Junk), ("junk's clean text", "TYPE IV RADIO EMISSIONS") })
        {
            var flags = text.Where(c => c != ' ').Select(c => Elements(c) <= 2).ToList();
            var line = string.Join("; ", Enumerable.Range(6, 7).Select(k => $"{Enumerable.Range(0, Math.Max(1, flags.Count - k + 1)).Max(i => flags.Skip(i).Take(k).Count(f => f))} of {k}"));

            _output.WriteLine($"{name}: the most short letters among any consecutive letters: {line}");
        }
    }

    [Fact]
    public void StrayShortLettersInTenSeconds()
    {
        foreach (var (name, text) in new[] { ("English", English), ("bulletin", Bulletin), ("QSO", Qso), ("junk", Junk), ("stray letters", Stray) })
        {
            var line = string.Join("; ", new[] { 18, 25, 35 }.Select(wpm => $"{MostStrayInWindow(text, wpm, 10)} at {wpm} WPM"));

            _output.WriteLine($"{name}: the most lone letters of one or two elements in any 10 s: {line}");
        }

        foreach (var wpm in new[] { 18, 25, 35 })
        {
            Assert.True(MostStrayInWindow(English, wpm, 10) < 6);
            Assert.True(MostStrayInWindow(Bulletin, wpm, 10) < 6);
            Assert.True(MostStrayInWindow(Qso, wpm, 10) < 6);
        }

        Assert.True(MostStrayInWindow(Stray, 18, 10) >= 6);
    }

    /// <summary>The kind the owner reported live: stray letters, each printed on its own.</summary>
    internal const string Stray = "E I S A N T E E I N T E";

    private static int MostStrayInWindow(string text, int wpm, double seconds)
    {
        var dit = 1.2 / wpm;
        var t = 0.0;
        var times = new List<double>();

        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (var c in word)
            {
                var pattern = MorseAlphabet.All.First(p => p.Value == c.ToString()).Key;

                t += pattern.Sum(e => e == '-' ? 3 : 1) * dit + ((pattern.Length - 1) * dit) + (3 * dit);

                if (word.Length == 1 && pattern.Length <= 2)
                {
                    times.Add(t);
                }
            }

            t += 4 * dit;
        }

        return times.Count == 0 ? 0 : times.Max(end => times.Count(x => x > end - seconds && x <= end));
    }

    /// <summary>How many elements a letter has, from the Morse table.</summary>
    internal static int Elements(char c)
        => MorseAlphabet.All.FirstOrDefault(p => p.Value == c.ToString()).Key?.Length ?? 99;

    private static int LongestRun(string text)
    {
        var best = 0;
        var run = 0;

        foreach (var c in text.Where(c => c != ' '))
        {
            run = Elements(c) <= 2 ? run + 1 : 0;
            best = Math.Max(best, run);
        }

        return best;
    }

    private static int Runs(string text, int length)
    {
        var count = 0;
        var run = 0;

        foreach (var c in text.Where(c => c != ' '))
        {
            run = Elements(c) <= 2 ? run + 1 : 0;

            if (run == length)
            {
                count++;
            }
        }

        return count;
    }

    // Standard timing: a dit, a dah of three, one between elements, three between letters, seven between words.
    private static int MostInWindow(string text, int wpm, double seconds)
    {
        var dit = 1.2 / wpm;
        var t = 0.0;
        var times = new List<double>();

        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (var c in word)
            {
                var pattern = MorseAlphabet.All.First(p => p.Value == c.ToString()).Key;

                t += pattern.Sum(e => e == '-' ? 3 : 1) * dit + ((pattern.Length - 1) * dit);

                if (pattern.Length <= 2)
                {
                    times.Add(t);
                }

                t += 3 * dit;
            }

            t += 4 * dit;
        }

        return times.Count == 0 ? 0 : times.Max(end => times.Count(x => x > end - seconds && x <= end));
    }
}
