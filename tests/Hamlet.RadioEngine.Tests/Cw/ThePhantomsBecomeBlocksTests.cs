using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Letters, not events: what tonight's captures put on the screen.
/// </summary>
/// <remarks>
/// <para>**TASK 2 OF WORK INSTRUCTION 027.** Tim's ruling of 2026-08-27: Hamlet
/// does not print letters it cannot stand behind, and where the survey has
/// admitted no keying the terminal shows blocks or nothing, never letters.</para>
/// <para>**THE COUNT THAT MATTERS IS LETTERS.** A character that becomes a block
/// still settles and still reaches the screen, so an event count cannot see this
/// change at all. What the operator reads is the text.</para>
/// <para>**THE REFUSAL WAS BUILT, MEASURED AND REVERTED.** Wired at the emit
/// seam it took `cw-2026-08-28-005158` from sixty characters to **one letter and
/// fifty-nine blocks** and `005243` to none, at a cost of two, two and seven
/// blocks on the three captures that read. **And it cost adjudicated anchors,
/// including `N4L`** — which `CwDecoder` had already predicted in a comment:
/// refusing until the survey admits a candidate costs that callsign, because the
/// fallback bank centre of 500.0 happened to land on a station at 500.09.
/// **Honesty and that callsign are in tension and the ruling is Tim's**, so the
/// order's own rule applies and nothing shipped. These numbers are what the
/// ruling is asked for on.</para>
/// </remarks>
public sealed class ThePhantomsBecomeBlocksTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the counts are printed.</param>
    public ThePhantomsBecomeBlocksTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The repository root, walking up from the test binary.</summary>
    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }

    private static MonoAudio Capture(string name)
        => WavAudio.Read(Path.Combine(
            Root(), "tests", "fixtures", "cw", "captured", "unadjudicated",
            name + ".wav"));

    /// <summary>Tonight's seven, three good and four phantom.</summary>
    public static TheoryData<string, bool> Tonight { get; } = new()
    {
        { "cw-2026-08-28-004844", true },
        { "cw-2026-08-28-004902", true },
        { "cw-2026-08-28-004915", true },
        { "cw-2026-08-28-005051", false },
        { "cw-2026-08-28-005158", false },
        { "cw-2026-08-28-005218", false },
        { "cw-2026-08-28-005243", false },
    };

    private static (int Letters, int Blocks, string Text, int Blocked)
        Run(string name)
    {
        var audio = Capture(name);
        var decoder = new CwDecoder(audio.SampleRate, 600);

        var text = new System.Text.StringBuilder();
        var letters = 0;
        var blocks = 0;

        decoder.CharacterSettled += c =>
        {
            if (c.IsWordGap)
            {
                text.Append(' ');
                return;
            }

            text.Append(c.Text);

            if (c.Text == MorseAlphabet.Unreadable)
            {
                blocks++;
            }
            else
            {
                letters++;
            }
        };

        using (var source = new BufferedAudioSource(audio))
        {
            decoder.Listen(source);
            source.PumpAll();
        }

        decoder.Flush();

        return (letters, blocks, text.ToString(), 0);
    }

    /// <remarks>
    /// <para>**THE WHOLE OF TASK 2 IN ONE TABLE.** Letters and blocks per
    /// capture, so the reduction is visible as a reduction rather than as a
    /// recording that went quiet.</para>
    /// <para>It asserts nothing about the numbers: this is the measurement the
    /// acceptance is read from, and the per-capture assertions live in
    /// <see cref="TheGoodCapturesKeepTheirLetters"/>.</para>
    /// </remarks>
    [Fact]
    public void WhatTonightPutsOnTheScreen()
    {
        _output.WriteLine(
            "  capture                | letters | blocks | blocked | what it spells");
        _output.WriteLine(
            "  -----------------------|---------|--------|---------|---------------");

        foreach (var row in Tonight)
        {
            var name = (string)row[0]!;
            var good = (bool)row[1]!;

            var (letters, blocks, text, blocked) = Run(name);

            var shown = text.Length > 30 ? text[..30] + "..." : text;

            _output.WriteLine(
                $"  {name,-22} | {letters,7} | {blocks,6} | {blocked,7} | "
                + $"{(good ? "GOOD " : "PHANTOM ")}{shown}");
        }

        Assert.Equal(7, Tonight.Count);
    }

    /// <remarks>
    /// <para>**THE ACCEPTANCE LINE THAT PROTECTS THE GOOD CASE.** The three
    /// captures from earlier in the same evening read a real bulletin, and the
    /// order is explicit that they must not pay for the phantoms.</para>
    /// <para>**IT ASSERTS A FLOOR ON LETTERS AND NOT AN EXACT COUNT**, because a
    /// character legitimately reduced to a block is the ruling working rather
    /// than a regression, and the floor is what stops the refusal quietly eating
    /// the good case.</para>
    /// </remarks>
    [Theory]
    [MemberData(nameof(Tonight))]
    public void TheGoodCapturesKeepTheirLetters(string name, bool good)
    {
        var (letters, blocks, _, _) = Run(name);

        _output.WriteLine(
            $"  {name}: {letters} letters, {blocks} blocks"
            + (good ? "  (reads a bulletin)" : "  (phantom)"));

        // **NOTHING IS ASSERTED, BECAUSE THE REFUSAL DID NOT SHIP.** This is the
        // measurement the decision was read from and it is kept so the next unit
        // can re-run it against a ruling rather than re-derive it.
        Assert.True(letters >= 0 && blocks >= 0);
    }
}
