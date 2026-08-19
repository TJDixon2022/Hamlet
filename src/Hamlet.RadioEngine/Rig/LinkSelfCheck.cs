namespace Hamlet.RadioEngine.Rig;

/// <summary>What Hamlet can say about the conversation with the radio.</summary>
/// <param name="Headline">
/// One sentence for the operator, in his terms, or "" when nothing is connected.
/// </param>
/// <param name="IsAnnouncing">
/// True when the radio announces its own changes, false when it is known not to,
/// and null when nobody can say yet.
/// </param>
/// <param name="TracksTheDial">
/// True when the frequency on screen is being kept current one way or the other.
/// </param>
/// <param name="Detail">
/// The numbers behind the headline, for the diagnostics screen, or "".
/// </param>
public readonly record struct LinkCheck(
    string Headline, bool? IsAnnouncing, bool TracksTheDial, string Detail);

/// <summary>
/// The check Hamlet runs on its own link, and says out loud (§0.0.1).
/// </summary>
/// <remarks>
/// <para>**EVERYTHING KNOWN ABOUT THE FAILURE THAT COST TWO BUILDS LIVED IN A
/// FILE THE OPERATOR HAD TO UPLOAD.** He turned the dial, Hamlet followed thirty
/// seconds later, and the application itself said nothing at all: the number on
/// screen was drawn confidently four times a second while being a minute old. A
/// display that is current about a value that is not is §0.0 broken by omission,
/// and the app knew every piece of this and assembled none of it.</para>
/// <para>Pure, so the sentences are testable without a radio and go through
/// `VoiceTests` like every other thing the operator reads (§0.7).</para>
/// </remarks>
public static class LinkSelfCheck
{
    /// <summary>
    /// How old a frequency can be before it is worth saying so.
    /// </summary>
    /// <remarks>
    /// A second and a half, which is the window a live reading stays current for.
    /// Beyond it the number on screen is a claim about then wearing the clothes
    /// of a claim about now (HM-DEC-111).
    /// </remarks>
    public static TimeSpan FrequencyIsOldAfter => RigPollPlan.LiveFreshFor;

    /// <summary>Assemble what is known into something a person can read.</summary>
    /// <param name="link">What the link says about itself, or null.</param>
    /// <param name="state">The rig state.</param>
    /// <param name="nowUtc">The moment.</param>
    /// <param name="isConnected">Whether a radio is attached at all.</param>
    /// <returns>The check.</returns>
    public static LinkCheck Describe(
        CivLinkHealth? link, RigState state, DateTime nowUtc, bool isConnected)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (!isConnected)
        {
            return new LinkCheck("", null, false, "");
        }

        var frequency = state[RigField.Frequency];
        var age = frequency.Age(nowUtc);
        var current = frequency.IsKnown && age is { } a && a < FrequencyIsOldAfter;

        // **THREE SOURCES AND THE STRONGEST ONE WINS.** The setting says what the
        // radio was told to do, the counters say what actually arrived, and a
        // value in the model carrying broadcast provenance is the announcement
        // itself. A frame that arrived beats a setting that was read, because one
        // is the thing and the other is a claim about it.
        var announcing = state[RigField.CivTransceive] is { IsKnown: true, Number: { } n }
            ? n > 0
            : (bool?)null;

        if (link is { InboundTransceive: > 0 } || frequency.IsBroadcast)
        {
            announcing = true;
        }
        else if (announcing is null && link is { Inbound: > 0 })
        {
            announcing = false;
        }

        return new LinkCheck(
            Headline(announcing, current, age),
            announcing,
            current,
            Detail(link, frequency, age));
    }

    private static string Headline(bool? announcing, bool current, TimeSpan? age)
    {
        if (announcing is true && current)
        {
            return "Your radio tells Hamlet the moment you touch the dial, so what "
                 + "you see here is where you actually are.";
        }

        if (announcing is false)
        {
            return "Your radio is not announcing its own changes, so Hamlet asks "
                 + "it where it is several times a second instead. That keeps the "
                 + "screen honest and it costs a little of the cable. Turning CI-V "
                 + "Transceive on at the radio would let it simply say so, which "
                 + "is quicker and quieter, and it is your setting to change.";
        }

        if (current)
        {
            return "Hamlet is keeping up with your radio, asking it where it is "
                 + "several times a second.";
        }

        if (age is { } old)
        {
            return "The frequency on screen is "
                 + Spoken(old)
                 + " old, so treat it as where the radio was rather than where it "
                 + "is. Hamlet is still asking.";
        }

        return "Hamlet has not heard where the radio is yet, so the frequency is "
             + "blank rather than guessed at.";
    }

    private static string Detail(CivLinkHealth? link, RigValue frequency, TimeSpan? age)
    {
        if (link is not { } health)
        {
            return "";
        }

        var parts = new List<string>
        {
            $"{health.Inbound} frames in, {health.InboundTransceive} of them the "
            + $"radio announcing itself",
        };

        if (frequency.IsKnown)
        {
            var how = frequency.IsBroadcast ? "the radio announced it" : "Hamlet asked";
            parts.Add(age is { } a
                ? $"frequency {Spoken(a)} old, {how}"
                : $"frequency {how}");
        }

        if (health.ScopeShare is { } share && health.InboundScope > 0)
        {
            parts.Add(
                $"{share * 100:0}% of what arrives is the spectrum picture, "
                + $"{health.InboundBytes} bytes in total");
        }

        return string.Join(" · ", parts);
    }

    /// <summary>
    /// An age as somebody would say it rather than count it (§0.7).
    /// </summary>
    private static string Spoken(TimeSpan age) => age.TotalSeconds switch
    {
        < 2 => "about a second",
        < 10 => $"about {(int)Math.Round(age.TotalSeconds)} seconds",
        < 45 => "half a minute or so",
        < 90 => "about a minute",
        _ => "minutes",
    };
}
