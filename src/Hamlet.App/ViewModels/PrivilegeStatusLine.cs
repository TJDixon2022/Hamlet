using System.Globalization;
using Hamlet.RadioEngine.Licensing;

namespace Hamlet.App.ViewModels;

/// <summary>How the status line under the band map should read.</summary>
public enum PrivilegeTone
{
    /// <summary>Nothing is claimed — the license class is not known.</summary>
    Unknown,

    /// <summary>Inside privileges. Green family.</summary>
    Yours,

    /// <summary>Outside privileges. Amber family: a caution, not an error.</summary>
    ListenOnly,
}

/// <summary>The status line under the band map, and the panel behind it.</summary>
/// <param name="Tone">Which family the line belongs to.</param>
/// <param name="Headline">
/// The frequency and the verdict, e.g. "7.030 MHz · yours to use".
/// </param>
/// <param name="Detail">The reason, in plain language.</param>
/// <param name="Reassurance">
/// The sentence about listening, shown whenever transmitting is restricted.
/// </param>
/// <param name="Citation">The paragraph behind it, or "".</param>
/// <param name="UpgradePrompt">
/// Button text such as "What would General unlock?", or "" when there is
/// nothing to offer.
/// </param>
public sealed record PrivilegeStatus(
    PrivilegeTone Tone,
    string Headline,
    string Detail,
    string Reassurance,
    string Citation,
    string UpgradePrompt);

/// <summary>
/// Writes the line under the band map: what this frequency is, for this
/// operator, right now.
/// </summary>
/// <remarks>
/// <para>THE TONE IS THE FEATURE (HM-DEC-029). The operator this serves has
/// been licensed six years and has never made a contact, and part of that is
/// a quiet fear of transmitting somewhere he is not allowed. A line that
/// scolds would make that worse. So restriction is stated as a fact and
/// immediately paired with the one thing that removes the fear: listening is
/// never restricted, anywhere, on any license.</para>
/// <para>Which is also why the amber family and not red. Being outside your
/// privileges while tuning around is not an error — it is the ordinary state
/// of most of the band for most licenses, and the app should sound like it
/// knows that.</para>
/// <para>Pure: a class, a frequency and a mode in, a line out. No clock, no
/// state (§5).</para>
/// </remarks>
public static class PrivilegeStatusLine
{
    /// <summary>The sentence that does the most work in this whole feature.</summary>
    public const string ListeningIsNeverRestricted =
        "Receiving is never restricted — any license may listen anywhere.";

    /// <summary>
    /// Build the status line.
    /// </summary>
    /// <param name="plan">The privileges plan.</param>
    /// <param name="licenseClass">The operator's class.</param>
    /// <param name="frequencyHz">Where they are tuned.</param>
    /// <param name="mode">What they would transmit here.</param>
    /// <returns>The line, and what sits behind it.</returns>
    public static PrivilegeStatus Build(
        PrivilegePlan plan,
        LicenseClass licenseClass,
        long frequencyHz,
        TransmitMode mode)
    {
        var megahertz = (frequencyHz / 1_000_000.0)
            .ToString("0.000", CultureInfo.InvariantCulture);

        if (licenseClass == LicenseClass.Unknown)
        {
            return new PrivilegeStatus(
                PrivilegeTone.Unknown,
                $"{megahertz} MHz",
                "License class unknown — set it in Settings to see your privileges.",
                ListeningIsNeverRestricted,
                "",
                "");
        }

        var verdict = plan.Evaluate(licenseClass, frequencyHz, mode);

        if (verdict.MayTransmit)
        {
            return new PrivilegeStatus(
                PrivilegeTone.Yours,
                $"{megahertz} MHz · yours to use",
                $"Your {PrivilegePlan.Describe(licenseClass)} license covers "
                + $"{PrivilegePlan.Describe(mode)} here. Call away.",
                "",
                verdict.Citation,
                "");
        }

        var upgrade = plan.UpgradeFrom(licenseClass);
        var prompt = upgrade is not null
            ? $"What would {PrivilegePlan.Describe(upgrade.Next)} unlock?"
            : "";

        return new PrivilegeStatus(
            PrivilegeTone.ListenOnly,
            $"{megahertz} MHz · listen all you like — don't transmit",
            verdict.Explanation,
            ListeningIsNeverRestricted,
            verdict.Citation,
            prompt);
    }

    /// <summary>
    /// The upgrade panel's contents — shown on click, never as permanent
    /// chrome.
    /// </summary>
    /// <param name="plan">The privileges plan.</param>
    /// <param name="licenseClass">The operator's current class.</param>
    /// <param name="band">The band on screen.</param>
    /// <returns>Lines describing what the next class up would open.</returns>
    /// <remarks>
    /// Restriction becomes motivation rather than scolding, and only when the
    /// operator asks. Permanent upgrade chrome would be a nag; a button they
    /// press is an invitation.
    /// </remarks>
    public static IReadOnlyList<string> UpgradeLadder(
        PrivilegePlan plan, LicenseClass licenseClass, RadioEngine.Bands.CwBand band)
    {
        var upgrade = plan.UpgradeFrom(licenseClass);
        if (upgrade is null)
        {
            return licenseClass == LicenseClass.Extra
                ? new[] { "You already hold every US privilege. There is nothing above this." }
                : Array.Empty<string>();
        }

        var now = plan.CoverageOf(band, licenseClass);
        var then = plan.CoverageOf(band, upgrade.Next);
        var next = PrivilegePlan.Describe(upgrade.Next);

        var lines = new List<string> { upgrade.Headline };

        lines.Add(string.Create(
            CultureInfo.InvariantCulture,
            $"On {band.Name} you can transmit on {now:P0} of the band today. "
            + $"{next} makes it {then:P0}."));

        // Name a concrete thing the upgrade buys on this band, rather than a
        // percentage nobody can picture.
        var gained = FirstGain(plan, licenseClass, upgrade.Next, band);
        if (gained is not null)
        {
            lines.Add(gained);
        }

        lines.Add(
            "Upgrading is one exam, no Morse requirement, and the question pool is public.");

        return lines;
    }

    /// <summary>
    /// The first stretch of this band the upgrade would open, described.
    /// </summary>
    private static string? FirstGain(
        PrivilegePlan plan, LicenseClass from, LicenseClass to, RadioEngine.Bands.CwBand band)
    {
        var mine = plan.SpansFor(band, from);
        var theirs = plan.SpansFor(band, to);

        if (mine.Count == 0 || theirs.Count == 0)
        {
            return null;
        }

        foreach (var span in theirs)
        {
            if (!span.MayTransmit)
            {
                continue;
            }

            // A stretch they could use that I cannot: sample its midpoint.
            var probe = span.LowHz + ((span.HighHz - span.LowHz) / 2);
            if (plan.MayTransmitAnyMode(from, probe))
            {
                continue;
            }

            var low = (span.LowHz / 1_000_000.0).ToString("0.000", CultureInfo.InvariantCulture);
            var high = (span.HighHz / 1_000_000.0).ToString("0.000", CultureInfo.InvariantCulture);

            return string.Create(
                CultureInfo.InvariantCulture,
                $"It would open {low}–{high} MHz on {band.Name}, which is closed to you now.");
        }

        return null;
    }
}
