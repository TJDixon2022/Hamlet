namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// Signal strength in words, and the signal report every contact ends up
/// exchanging.
/// </summary>
/// <remarks>
/// <para>WHY THIS EXISTS (HM-DEC-042). "You're five by nine" is in every
/// contact ever made and nobody explains it, so a newcomer hears a number
/// pair, has no idea whether it is good news, and cannot tell whether the
/// answer they give back is a lie. The glossary entry for RST carries the
/// definition. This carries the part a definition cannot: what a given
/// figure actually means for the person about to answer.</para>
/// <para>A dB figure from a skimmer and an RST report from a person are
/// different things and are kept apart here. The skimmer measured
/// signal-to-noise with a computer. The person guessed, generously, in a
/// convention where almost everybody says 59 whatever they heard. Hamlet
/// converts between them nowhere, because a measured number dressed up as
/// somebody's opinion would be inventing a courtesy (§0.0).</para>
/// <para>Pure: a number in, a phrase out.</para>
/// </remarks>
public static class SignalReport
{
    /// <summary>At or above this many dB, a skimmer's report reads as strong.</summary>
    public const int StrongDb = 20;

    /// <summary>At or above this, it reads as fair.</summary>
    public const int FairDb = 10;

    /// <summary>Below this, it reads as weak.</summary>
    public const int WeakDb = 5;

    /// <summary>
    /// One word for a signal-to-noise figure: strong, fair, or weak.
    /// </summary>
    /// <param name="db">Signal-to-noise in dB, as a skimmer reported it.</param>
    /// <returns>"strong", "fair", "workable" or "weak".</returns>
    public static string Strength(int db) => db switch
    {
        >= StrongDb => "strong",
        >= FairDb => "fair",
        >= WeakDb => "workable",
        _ => "weak",
    };

    /// <summary>
    /// A signal-to-noise figure with its meaning attached.
    /// </summary>
    /// <param name="db">Signal-to-noise in dB.</param>
    /// <returns>e.g. "24 dB over the noise, which is strong".</returns>
    /// <remarks>
    /// The number stays. It is evidence, and the operator is entitled to see
    /// what the word was derived from (§0.0.1). The word is what makes it
    /// mean anything on the first read.
    /// </remarks>
    public static string Describe(int db)
        => $"{db} dB over the noise, which is {Strength(db)}";

    /// <summary>
    /// What a skimmer's figure means for somebody deciding whether to answer.
    /// </summary>
    /// <param name="db">Signal-to-noise in dB.</param>
    /// <returns>A sentence in plain language.</returns>
    public static string WhatItMeansForYou(int db) => db switch
    {
        >= StrongDb =>
            "That is a loud signal where the skimmer sat. It says nothing certain "
            + "about your own antenna, but a station that strong is usually the "
            + "easiest kind to answer.",

        >= FairDb =>
            "That is a comfortable signal for a machine to decode. Whether it is "
            + "comfortable for your ears depends on your antenna and your noise, "
            + "and the only way to find out is to listen.",

        >= WeakDb =>
            "That is workable rather than loud. A receiver with a good antenna will "
            + "hear it, and a compromised one may not, so it is worth a listen "
            + "before you decide.",

        _ =>
            "That is faint. The skimmer decoded it because a computer can dig "
            + "further into the noise than an ear can, so do not be surprised if "
            + "you hear nothing at all.",
    };

    /// <summary>
    /// The signal report an operator would give by voice or by key, and what
    /// the numbers mean.
    /// </summary>
    /// <remarks>
    /// Written once and shown wherever a report appears, because the whole
    /// convention is one short paragraph and nobody ever says it out loud
    /// (HM-DEC-042).
    /// </remarks>
    public const string RstExplained =
        "A signal report is three numbers, or two on voice. Readability runs one "
        + "to five for how much of it you can make out, strength one to nine for "
        + "how loud, and on Morse a third number one to nine for how clean the "
        + "tone is. So 579 means perfectly readable, moderately strong, and a "
        + "pure note. In practice a great deal of it is a polite fiction that "
        + "everybody joins in with, and if somebody hands you a 59 they usually "
        + "mean they can hear you fine. Nobody will check your arithmetic.";
}
