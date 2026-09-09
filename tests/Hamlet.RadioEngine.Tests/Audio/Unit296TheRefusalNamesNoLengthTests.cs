using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// <b>Work instruction 296 task 2a — every refusal sentence on the operator's decode path names
/// the slot and never how long one is.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>THE BREAKAGE THIS WOULD HAVE CAUGHT, NAMED RATHER THAN IMPLIED</b>
/// (<c>PHASE_PLAN.md</c> requires the naming). <see cref="Ft8Reader.NoWholeSlot"/> read
/// <i>there is not a whole fifteen-second slot in what was kept</i>. It reaches the operator's own
/// screen — <c>Ft8Heard.Refusal</c> becomes the mode strip line and the decoded summary — and on
/// FT4, whose slot is 7.5 seconds, it named a length twice the one the application was cutting on,
/// in the one sentence whose whole job is to say that nothing was cut at all. Unit 290 corrected
/// exactly this fault two sentences below it in <see cref="Ft8SlotCutter"/> and unit 292 corrected
/// it in <c>AudioArrival</c>; this constant was missed and five units passed over it. Nothing in the
/// suite failed, because a sentence that is merely wrong compiles and decodes just as well as one
/// that is right. <b>That is the class of fault §0.0 exists for, and it is caught by reading the
/// words rather than by exercising the path.</b>
/// </para>
/// <para>
/// <b>WHY THE RULE IS NO LENGTH AT ALL RATHER THAN THE RIGHT LENGTH.</b> These are <c>const</c>
/// strings on static types. The slot length belongs to the running mode and is decided per call;
/// nothing reachable from a constant knows which mode the audio was captured on, so a sentence
/// naming 7.5 would be wrong on FT8 exactly as often as the old one was wrong on FT4. Naming the
/// <em>slot</em>, which is what was being cut, is true on both grids.
/// </para>
/// <para>
/// <b>AND IT IS NOT THE PARKED FIGURE.</b> The 4.48-against-5.04 disagreement is about how long a
/// <em>transmission</em> is and is with the owner. A slot is not a transmission, and this test
/// forbids a length of either reaching a screen, so neither answer to that question can be smuggled
/// on to one here.
/// </para>
/// </remarks>
public class Unit296TheRefusalNamesNoLengthTests(ITestOutputHelper output)
{
    /// <summary>
    /// <b>The units of time. A refusal on this path may not name one at all</b>, spelled or
    /// abbreviated.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>The rule is the unit and not the number, and the difference is the whole of getting this
    /// test right.</b> <c>Ft8SlotCutter.TooShort</c> reads <i>shorter than one whole
    /// transmission</i>: <c>one</c> there counts transmissions and says nothing about how long one
    /// is, and a test that refused it would be refusing an honest sentence. What made
    /// <c>NoWholeSlot</c> wrong was <c>fifteen-second</c> — a number bound to a unit of time — and
    /// splitting on the hyphen leaves <c>second</c> standing on its own where this list finds it.
    /// </para>
    /// <para>
    /// A digit is caught separately, because <c>7.5 s</c> would name a length with no spelled unit
    /// in it and would be the same fault arriving by another door — and it is the parked figure's
    /// door in particular.
    /// </para>
    /// </remarks>
    private static readonly string[] TimeUnits =
    [
        "second", "seconds", "s", "sec", "secs",
        "minute", "minutes", "min", "mins",
        "millisecond", "milliseconds", "ms",
    ];

    /// <summary>Every refusal constant an operator can meet on the decode path, with its name.</summary>
    public static TheoryData<string, string> TheOperatorsRefusals =>
        new()
        {
            { nameof(Ft8Reader.NoWholeSlot), Ft8Reader.NoWholeSlot },
            { nameof(Ft8SlotCutter.NoOffset), Ft8SlotCutter.NoOffset },
            { nameof(Ft8SlotCutter.TooShort), Ft8SlotCutter.TooShort },
        };

    /// <summary>
    /// <b>No refusal on the operator's decode path names a length, in digits or in words.</b>
    /// </summary>
    [Theory]
    [MemberData(nameof(TheOperatorsRefusals))]
    public void ARefusalSentenceNamesNoSlotLength(string name, string sentence)
    {
        output.WriteLine($"{name}:");
        output.WriteLine($"  \"{sentence}\"");

        // WHOLE WORDS AND NOT SUBSTRINGS. "seconds" lives inside nothing here, but "s" does -
        // a substring search for it would refuse every sentence in the language. The hyphen is a
        // separator because "fifteen-second" is exactly the shape that got through.
        var words = sentence
            .Split([' ', ',', '.', '-', ';', ':'], StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word.ToLowerInvariant())
            .ToList();

        var found = words
            .Where(word => TimeUnits.Contains(word) || word.Any(char.IsDigit))
            .ToList();

        Assert.True(
            found.Count == 0,
            $"{name} reads \"{sentence}\" and names {string.Join(", ", found)}. A refusal on the "
            + "operator's decode path is a const on a static type and cannot know which mode's "
            + "slot it is talking about, so it must name no length at all. NoWholeSlot said "
            + "\"fifteen-second\" for five units while FT4 was cutting 7.5 second slots, and the "
            + "sentence's whole job is to say that nothing was cut.");
    }

    /// <summary>
    /// <b>The sentence still says what it is refusing about</b> — a refusal that named no length
    /// and no slot either would pass the test above and tell the operator nothing.
    /// </summary>
    [Fact]
    public void TheNoWholeSlotSentenceStillNamesTheSlotItWasCutting()
    {
        output.WriteLine($"NoWholeSlot: \"{Ft8Reader.NoWholeSlot}\"");

        Assert.Contains("slot", Ft8Reader.NoWholeSlot, StringComparison.Ordinal);
        Assert.Contains("nothing to decode", Ft8Reader.NoWholeSlot, StringComparison.Ordinal);
        Assert.DoesNotContain("fifteen", Ft8Reader.NoWholeSlot, StringComparison.OrdinalIgnoreCase);
    }
}
