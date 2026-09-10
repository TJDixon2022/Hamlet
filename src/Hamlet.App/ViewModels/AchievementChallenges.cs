using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Solar;

namespace Hamlet.App.ViewModels;

/// <summary>
/// **The handful of standing targets, which are visible whether or not they are
/// earned.**
/// </summary>
/// <remarks>
/// <para>**§3.4: THESE ARE THE *GO AND TRY THIS* HALF AND HIDING THEM DEFEATS
/// THEM.** A record is *look what you have collected* and would be discouraging as a
/// wall of blanks, so it unlocks. A challenge exists to be aimed at, so it stands.
/// **They are the only cards on this screen that may appear unearned**, and there
/// are eight of them rather than fifty.</para>
/// <para>**§3.5 IS WHY THEY EXIST AT ALL.** *Write challenges that push toward
/// things a new operator would not know to try... each challenge is a nudge toward
/// something worth knowing, and the `i` hover carries the why. That sentence is the
/// actual product. The card is the reason he reads it.* Every hover below teaches
/// one true thing about radio, and a challenge whose hover taught nothing would fail
/// §5 question 4 and would not belong here.</para>
/// <para>**§4: NOTHING SHAMES.** Each reads as an invitation. There is no streak he
/// broke, no elapsed silence, and no card whose purpose is to name a thing he has
/// not done. **Where he has nothing to measure against, the progress line says that
/// rather than showing a zero**, because a zero is a score and this is not one.</para>
/// <para>**§5 QUESTION 1: EVERY ONE IS COMPUTED FROM THE LOG ALONE.** The one that
/// reaches past it is the grey line, which needs sunrise and sunset - and those come
/// from `SolarClock`, computed from the operator's own coordinates, which is
/// arithmetic rather than data Hamlet does not have.</para>
/// </remarks>
public static class AchievementChallenges
{
    /// <summary>The distance ladder, in miles.</summary>
    /// <remarks>
    /// <para>**ONE RUNG SHOWS AT A TIME AND IT IS THE NEXT ONE.** Five cards reading
    /// *past 500*, *past 1,000*, *past 3,000*, *past 5,000* and *past 10,000* is
    /// four things he has not done and one he is aiming at; one card reading the
    /// next rung is a target (§3.3, §2).</para>
    /// <para>**THE RUNGS ARE WHAT THE BANDS ACTUALLY DO.** A few hundred miles is
    /// ground wave and near skip; a thousand is one hop; three thousand crosses an
    /// ocean; five is most of the way round a continent and back; ten thousand is
    /// the far side of the world, which on HF is a real thing rather than a
    /// fantasy.</para>
    /// </remarks>
    private static readonly int[] Miles = { 500, 1_000, 3_000, 5_000, 10_000 };

    /// <summary>The faint-signal ladder, in decibels.</summary>
    /// <remarks>
    /// **-21 dB IS NOT AN ARBITRARY FLOOR.** It is about where FT8 stops decoding at
    /// all, which the sensitivity work in this repository measured against a
    /// published figure, so the last rung is genuinely the edge rather than a round
    /// number.
    /// </remarks>
    private static readonly int[] Decibels = { -10, -15, -18, -21 };

    /// <summary>Build the standing targets from the log.</summary>
    /// <param name="log">What the contact log holds.</param>
    /// <param name="operatorGrid">The operator's own locator, for the grey line.</param>
    /// <returns>Eight cards, earned or not.</returns>
    /// <exception cref="ArgumentNullException">There is no log.</exception>
    public static IReadOnlyList<AchievementCard> For(
        AchievementLog log, string? operatorGrid = null)
    {
        ArgumentNullException.ThrowIfNull(log);

        return new[]
        {
            Distance(log),
            Faint(log),
            NewBand(log),
            LowBandAfterDark(log, operatorGrid),
            GreyLine(log, operatorGrid),
            NewContinent(log),
            Grids(log),
            BusyDay(log),
        };
    }

    /// <summary>Go further than you have gone.</summary>
    private static AchievementCard Distance(AchievementLog log)
    {
        var best = log.Contacts.Where(c => c.Miles is not null)
            .Select(c => c.Miles!.Value)
            .DefaultIfEmpty(0)
            .Max();

        var target = Miles.FirstOrDefault(m => best < m);
        var earned = target == 0;

        return new AchievementCard(
            key: "challenge-distance",
            kind: AchievementKind.Challenge,
            title: earned
                ? "Past 10,000 miles"
                : "First past "
                  + target.ToString("N0", CultureInfo.InvariantCulture) + " miles",
            figure: "",
            station: "",
            detail: "Distance on HF is not about power. A signal that goes a few "
                + "hundred miles went out and came straight back down off a layer "
                + "of the upper atmosphere, and one that goes ten thousand did that "
                + "several times over, bouncing between the sky and the sea. **The "
                + "band and the hour decide which of those is possible**, far more "
                + "than your transmitter does, which is why a hundred watts can "
                + "reach the far side of the world on the right evening and not "
                + "reach the next state on the wrong one.",
            earned: earned,
            progress: best <= 0
                ? "No contact has carried a grid square yet, so there is nothing to "
                  + "measure this against."
                : "Best so far " + GridPath.DescribeMiles(best) + ".")
        {
            Glyph = AchievementGlyph.Distance,

            // **NO GRID SQUARE ANYWHERE IN THE LOG IS NOT THE SAME AS ZERO MILES.**
            // One is unmeasured and the other is a measurement, and drawing 0% for
            // the first would be a figure with nothing behind it.
            HasRing = best > 0,
            Ring = earned || target == 0 ? 1 : (double)best / target,
            Target = earned
                ? "done"
                : target.ToString("N0", CultureInfo.InvariantCulture) + " mi",
            RingMeans = earned
                ? "Past the last rung on the ladder."
                : best <= 0
                    ? "No contact has carried a grid square, so there is no distance "
                      + "to measure."
                    : GridPath.DescribeMiles(best) + " of "
                      + target.ToString("N0", CultureInfo.InvariantCulture)
                      + ", so " + GridPath.DescribeMiles(target - best)
                      + " to go. The ring is miles covered and not how likely the "
                      + "rest is, and the next thousand is much harder than the "
                      + "last.",
        };
    }

    /// <summary>Be heard when almost nothing of you arrives.</summary>
    private static AchievementCard Faint(AchievementLog log)
    {
        var reports = log.Contacts
            .Where(c => c.ReportReceived is not null)
            .Select(c => c.ReportReceived!.Value)
            .ToList();

        var best = reports.Count == 0 ? (int?)null : reports.Min();

        var target = Decibels.FirstOrDefault(d => best is null || best > d);
        var earned = target == 0;

        return new AchievementCard(
            key: "challenge-faint",
            kind: AchievementKind.Challenge,
            title: earned
                ? "Heard below -21 dB"
                : "First report below " + target + " dB",
            figure: "",
            station: "",
            detail: "A signal report in FT8 is how far your signal was above the "
                + "noise where he was sitting, and it is usually a minus number "
                + "because the noise is louder than you are. **The decoder reads "
                + "down to about -21 dB, which is well below what an ear can hear "
                + "at all** - the arithmetic finds the pattern where a person would "
                + "hear only hiss. A report near that means almost none of you "
                + "arrived and the far end read you anyway.",
            earned: earned,
            progress: best is null
                ? "No contact has carried a report yet, so there is nothing to "
                  + "measure this against."
                : "Faintest so far " + AchievementCard.Signed(best.Value) + " dB.")
        {
            Glyph = AchievementGlyph.Faint,

            // **COUNTED FROM 0 dB, WHERE YOUR SIGNAL AND THE NOISE ARE THE SAME
            // SIZE**, which is a real place on the scale rather than a chosen one. A
            // report above it is not partway to anything, so it reads as nothing
            // covered rather than as a negative fraction.
            HasRing = best is not null,
            Ring = earned || target == 0
                ? 1
                : Math.Clamp((double)-(best ?? 0) / -target, 0, 1),
            Target = earned ? "done" : target + " dB",
            RingMeans = earned
                ? "Past the last rung on the ladder."
                : best is null
                    ? "No contact has carried a report, so there is nothing to "
                      + "measure."
                    : AchievementCard.Signed(best.Value) + " dB of " + target
                      + " dB, counting down from 0 dB, where your signal and the "
                      + "noise are the same size. It is decibels covered and not "
                      + "how likely the rest is: every one of them is harder than "
                      + "the one before.",
        };
    }

    /// <summary>Try a band you have not been on.</summary>
    /// <remarks>
    /// **IT NAMES ONE BAND AND NOT THE SIX HE HAS NOT WORKED** (§3.3, §2). A list of
    /// six is a list of things he has not done; one is somewhere to go tonight.
    /// </remarks>
    private static AchievementCard NewBand(AchievementLog log)
    {
        var worked = log.Bands
            .Select(AdifLog.BandDisplayNameFor)
            .ToList();

        var next = HfBands.Names.FirstOrDefault(
            n => !worked.Contains(n, StringComparer.OrdinalIgnoreCase));

        var earned = next is null;

        return new AchievementCard(
            key: "challenge-new-band",
            kind: AchievementKind.Challenge,
            title: earned
                ? "Every band Hamlet offers, worked"
                : "A contact on " + next,
            figure: "",
            station: "",
            detail: "**Each band is a different world at the same hour.** The higher "
                + "ones - 20 m, 15 m, 10 m - need the sun on the path and go quiet "
                + "after dark; the lower ones - 80 m, 40 m - are noisy and short in "
                + "daylight and open up at night over long distances. Working a new "
                + "band is not more of the same: it is a different set of people at "
                + "a different set of distances, and the quickest way to learn what "
                + "the ionosphere is actually doing.",
            earned: earned,
            progress: worked.Count == 0
                ? "No band worked yet."
                : worked.Count == 1
                    ? "One band worked so far: " + worked[0] + "."
                    : worked.Count + " bands worked so far: "
                      + string.Join(", ", worked) + ".")
        {
            Glyph = AchievementGlyph.Band,

            // **THERE IS NO HALF-WORKED BAND** (work instruction 300 task 4). Any
            // fraction here would have to be invented: bands worked over bands
            // offered measures something this card is not about, since the card is
            // about one contact on one band he has not been on.
            HasRing = earned,
            Ring = earned ? 1 : 0,
            Target = earned ? "done" : next ?? "",
            RingMeans = earned
                ? "Every band Hamlet offers has a contact on it."
                : "There is no half of this to be in, so there is no percentage. "
                  + next + " is the nearest one you have not been on.",
        };
    }

    /// <summary>Work a low band after the sun has gone.</summary>
    /// <remarks>
    /// **§3.5's OWN EXAMPLE**: *40 m at night is a different world from 20 m in the
    /// afternoon.* It is computed from the record's own UTC and the operator's own
    /// coordinates, so nothing is guessed.
    /// </remarks>
    private static AchievementCard LowBandAfterDark(
        AchievementLog log, string? operatorGrid)
    {
        var here = OperatorLocation.FromGrid(operatorGrid);

        var done = here is not null && log.Contacts.Any(
            c => IsLowBand(c.Band) && c.StartedUtc is { } at && IsDark(here.Value, at));

        return new AchievementCard(
            key: "challenge-low-band-night",
            kind: AchievementKind.Challenge,
            title: "A contact on 80 m or 40 m after dark",
            figure: "",
            station: "",
            detail: "**Daylight thickens a layer of the atmosphere that soaks up low "
                + "frequencies, and after dark it thins.** That is the whole reason "
                + "40 m is a short, noisy band in the afternoon and a long-distance "
                + "band at midnight, and why the people you hear on it at those two "
                + "hours are completely different. It is the single easiest piece of "
                + "propagation to go and prove to yourself.",
            earned: done,
            progress: here is null
                ? "Hamlet needs your grid square in Settings before it can work out "
                  + "when the sun goes down where you are."
                : done
                    ? "You have done this one."
                    : "Nothing on 80 m or 40 m after dark yet.")
        {
            Glyph = AchievementGlyph.Night,
            HasRing = done,
            Ring = done ? 1 : 0,
            Target = done ? "done" : "80 m 40 m",
            RingMeans = done
                ? "You have done this one."
                : here is null
                    ? "Hamlet needs your grid square before it knows when your sun "
                      + "goes down, so there is nothing to measure."
                    : "You have either done this or you have not, so there is no "
                      + "percentage to show.",
        };
    }

    /// <summary>Work somebody in the hour around sunrise or sunset.</summary>
    private static AchievementCard GreyLine(AchievementLog log, string? operatorGrid)
    {
        var here = OperatorLocation.FromGrid(operatorGrid);

        var done = here is not null && log.Contacts.Any(
            c => c.StartedUtc is { } at && IsGreyLine(here.Value, at));

        return new AchievementCard(
            key: "challenge-grey-line",
            kind: AchievementKind.Challenge,
            title: "A contact on the grey line",
            figure: "",
            station: "",
            detail: "**Grey line is the hour or so around sunrise and sunset, when "
                + "the layer that absorbs low frequencies in daylight has faded but "
                + "the layer that reflects them is still there.** For that window a "
                + "path along the line between day and night can carry a low-band "
                + "signal an extraordinary distance for the power involved. It moves "
                + "round the world twice a day and it is worth being at the radio "
                + "for.",
            earned: done,
            progress: here is null
                ? "Hamlet needs your grid square in Settings before it can work out "
                  + "when your sunrise and sunset are."
                : done
                    ? "You have done this one."
                    : "No contact yet inside the hour either side of your sunrise or "
                      + "sunset.")
        {
            Glyph = AchievementGlyph.GreyLine,
            HasRing = done,
            Ring = done ? 1 : 0,
            Target = done ? "done" : "1 hr",
            RingMeans = done
                ? "You have done this one."
                : here is null
                    ? "Hamlet needs your grid square before it knows when your "
                      + "sunrise and sunset are, so there is nothing to measure."
                    : "You have either done this or you have not, so there is no "
                      + "percentage to show.",
        };
    }

    /// <summary>Reach a continent you have not reached.</summary>
    private static AchievementCard NewContinent(AchievementLog log)
    {
        var worked = log.Continents.Select(DxccContinents.NameOf).ToList();
        var earned = worked.Count >= DxccContinents.Codes.Count;

        return new AchievementCard(
            key: "challenge-new-continent",
            kind: AchievementKind.Challenge,
            title: earned ? "Every continent, worked" : "A contact on a new continent",
            figure: "",
            station: "",
            detail: "**A continent is not simply further away; it is a different "
                + "path.** Crossing an ocean means the signal is reflecting off "
                + "seawater, which is a far better mirror than dry ground, and it is "
                + "why transatlantic contacts are often easier than something the "
                + "same distance overland. Which continents are open to you changes "
                + "through the day and through the eleven-year cycle of the sun.",
            earned: earned,
            progress: worked.Count == 0
                ? "No contact has resolved to a continent yet."
                : worked.Count + " so far: " + string.Join(", ", worked) + ".")
        {
            Glyph = AchievementGlyph.Continent,

            // **THE CARD IS ABOUT REACHING ONE MORE AND NOT ABOUT REACHING THEM
            // ALL**, so continents worked over continents that exist would be a
            // percentage of a different question. It stays dashed until the last
            // one lands.
            HasRing = earned,
            Ring = earned ? 1 : 0,
            Target = earned ? "done" : "one more",
            RingMeans = earned
                ? "Every continent has a contact on it."
                : worked.Count == 0
                    ? "No contact has resolved to a continent yet."
                    : "There is no part of a continent to have reached, so there is "
                      + "no percentage. So far: " + string.Join(", ", worked) + ".",
        };
    }

    /// <summary>Collect grid squares.</summary>
    private static AchievementCard Grids(AchievementLog log)
    {
        var count = log.Grids.Count;
        var target = new[] { 5, 25, 100, 250 }.FirstOrDefault(t => count < t);
        var earned = target == 0;

        return new AchievementCard(
            key: "challenge-grids",
            kind: AchievementKind.Challenge,
            title: earned
                ? "250 grid squares worked"
                : target.ToString(CultureInfo.InvariantCulture) + " grid squares worked",
            figure: "",
            station: "",
            detail: "**A grid square is a box about seventy miles by a hundred**, and "
                + "the four-character locator people send - `FN31`, `IO63` - names "
                + "one. Collecting them is how you find out what your station "
                + "actually covers, because they map where you can reach far better "
                + "than a count of countries does: a hundred contacts inside one "
                + "country might be four squares or forty.",
            earned: earned,
            progress: count == 0
                ? "No contact has carried a grid square yet."
                : count == 1
                    ? "1 so far."
                    : count + " so far.")
        {
            Glyph = AchievementGlyph.Grid,

            // **BOTH HALVES ARE WHOLE NUMBERS HE COULD COUNT HIMSELF**, which makes
            // this the plainest ring on the screen. It still says nothing about how
            // hard the rest are.
            HasRing = true,
            Ring = earned || target == 0 ? 1 : (double)count / target,
            Target = earned ? "done" : target.ToString(CultureInfo.InvariantCulture),
            RingMeans = earned
                ? "Two hundred and fifty squares worked."
                : count + " squares of " + target + ", so " + (target - count)
                  + " to go. It is a count of squares and not how likely the rest "
                  + "are, and the ones you have not got are mostly the far ones.",
        };
    }

    /// <summary>Have a proper session at the radio.</summary>
    private static AchievementCard BusyDay(AchievementLog log)
    {
        var best = AchievementLog.BusiestDay(log.Contacts)?.Count ?? 0;
        var target = new[] { 5, 10, 25, 50 }.FirstOrDefault(t => best < t);
        var earned = target == 0;

        return new AchievementCard(
            key: "challenge-busy-day",
            kind: AchievementKind.Challenge,
            title: earned
                ? "Fifty contacts in a day"
                : target + " contacts in one day",
            figure: "",
            station: "",
            detail: "**A run of contacts in one sitting teaches something a single "
                + "one cannot: what the band is doing while you watch it.** Stations "
                + "fade in and out, a path opens for twenty minutes and shuts, and "
                + "the same callsigns come back round. Contest weekends are the "
                + "easiest place to find that, and you do not have to enter one to "
                + "work the people in it.",
            earned: earned,
            progress: best == 0
                ? "No contact carries a date yet."
                : "Best day so far: " + best
                  + (best == 1 ? " contact." : " contacts."))
        {
            Glyph = AchievementGlyph.Day,
            HasRing = true,
            Ring = earned || target == 0 ? 1 : (double)best / target,
            Target = earned ? "done" : target.ToString(CultureInfo.InvariantCulture),
            RingMeans = earned
                ? "Fifty contacts in one day."
                : best + " contacts of " + target + " in one day, so "
                  + (target - best) + " more in a single sitting. It is a count of "
                  + "contacts and not how likely the rest are, and that depends far "
                  + "more on the band being open than on you.",
        };
    }

    /// <summary>Whether a band value is one of the two low ones.</summary>
    private static bool IsLowBand(string? band)
        => AdifLog.BandDisplayNameFor(band) is "80 m" or "40 m";

    /// <summary>Whether the sun is down where the operator is, at that moment.</summary>
    private static bool IsDark(LatLon here, DateTime utc)
    {
        var sun = SolarClock.At(here.Latitude, here.Longitude, utc);

        return sun.SunriseUtc is { } up && sun.SunsetUtc is { } down
            && (utc < up || utc > down);
    }

    /// <summary>Whether a moment is inside an hour of sunrise or sunset.</summary>
    /// <remarks>
    /// **AN HOUR EITHER SIDE, WHICH IS WHAT THE HOVER SAYS.** Grey line has no
    /// official width; the sentence on the card and the arithmetic behind it are the
    /// same figure, so the screen cannot claim one thing and count another.
    /// </remarks>
    private static bool IsGreyLine(LatLon here, DateTime utc)
    {
        var sun = SolarClock.At(here.Latitude, here.Longitude, utc);

        return Within(sun.SunriseUtc, utc) || Within(sun.SunsetUtc, utc);
    }

    private static bool Within(DateTime? moment, DateTime utc)
        => moment is { } at && Math.Abs((utc - at).TotalMinutes) <= 60;
}
