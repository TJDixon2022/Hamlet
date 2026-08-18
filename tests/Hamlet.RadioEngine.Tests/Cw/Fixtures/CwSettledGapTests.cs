using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// The settled pass measured against the provisional tip, on proved audio
/// (HM-DEC-102, HM-DEC-107 phase 4).
/// </summary>
/// <remarks>
/// <para>**THE SETTLED PASS EXISTS BECAUSE THE CLUSTERING GATE IS BETTER, AND ON
/// SOUND AUDIO IT IS NOT YET.** The gap was first seen on the five decibel
/// fading tier, where the reference scored only about half, so nothing had been
/// proved about Hamlet. It has since been re-measured on fixtures the reference
/// reads at 96 to 100 percent, and it survives.</para>
/// <para>**WHAT IS MEASURED HERE CHANGED ONCE, AND THE REASON MATTERS.** The
/// first version of these tests counted placeholders, and phase 4 drove that
/// share from 54 percent to nothing — by which point the settled pass was
/// emitting clean-looking letters that were wrong. **Zero placeholders with the
/// wrong letters is worse than placeholders**, because a placeholder tells the
/// truth. So what is counted now is how much of what it emits belongs to the
/// message at all (§0.0).</para>
/// </remarks>
public sealed class CwSettledGapTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the comparison is printed.</param>
    public CwSettledGapTests(ITestOutputHelper output) => _output = output;

    private sealed record Pass(
        int Characters, int Placeholders, int Confident, int Strangers)
    {
        public double PlaceholderShare
            => Characters == 0 ? 0 : (double)Placeholders / Characters;

        /// <summary>
        /// How much of what this pass emitted is not in the message at all.
        /// </summary>
        public double StrangerShare
            => Characters == 0 ? 0 : (double)Strangers / Characters;
    }

    private (Pass Tip, Pass Settled, string TipText, string SettledText) Measure(
        string name)
    {
        var recipe = CwFixtureCatalogue.All.Single(r => r.Name == name);

        var expected = recipe.Text
            .Replace("^", "", StringComparison.Ordinal)
            .Replace(" ", "", StringComparison.Ordinal)
            .ToUpperInvariant();

        var audio = WavAudio.Read(
            Path.Combine(CwFixtureCatalogue.Folder, name + ".wav"));

        var decoder = new CwDecoder(audio.SampleRate, 600);

        var tip = new List<CwCharacter>();
        var settled = new List<CwCharacter>();

        decoder.CharacterDecoded += c => tip.Add(c);
        decoder.CharacterSettled += c => settled.Add(c);

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        Pass Summarize(List<CwCharacter> characters)
        {
            var real = characters.Where(c => !c.IsWordGap).ToList();

            return new Pass(
                real.Count,
                real.Count(c => c.IsUnreadable),
                real.Count(c => c.Confidence == CwConfidence.High),
                real.Count(c => !c.IsUnreadable
                    && c.Text.Length == 1
                    && !expected.Contains(c.Text[0], StringComparison.Ordinal)));
        }

        return (
            Summarize(tip),
            Summarize(settled),
            string.Concat(tip.Select(c => c.Text)).Trim(),
            string.Concat(settled.Select(c => c.Text)).Trim());
    }

    /// <remarks>
    /// <para>Records HM-DEC-102 and HM-DEC-107 phase 4 on audio the reference
    /// reads at 96 to 100 percent, so anything it shows is a fact about Hamlet
    /// rather than about the fixture.</para>
    /// </remarks>
    [Theory]
    [InlineData("exchange-easy")]
    [InlineData("coverage-easy")]
    [InlineData("tightfist-easy")]
    public void TheSettledPassIsMeasuredAgainstTheTip(string name)
    {
        var (tip, settled, tipText, settledText) = Measure(name);

        _output.WriteLine($"{name}");
        _output.WriteLine($"  tip     {tip.Characters,3} characters, "
            + $"{tip.PlaceholderShare,4:P0} unresolved, "
            + $"{tip.StrangerShare,4:P0} not in the message   '{tipText}'");
        _output.WriteLine($"  settled {settled.Characters,3} characters, "
            + $"{settled.PlaceholderShare,4:P0} unresolved, "
            + $"{settled.StrangerShare,4:P0} not in the message   '{settledText}'");

        Assert.True(tip.Characters > 0, "the leading edge read nothing at all");
    }

    /// <remarks>
    /// <para>**THE LINE §0.0 DRAWS, AND THE ONE THING PHASE 4 MAY NOT BREAK.**
    /// Settled text is what a transcript keeps. A character it shows at full
    /// strength is a claim that this is what was sent, so a settled reading that
    /// is confidently wrong does the specific damage this whole feature exists to
    /// prevent: the operator concludes the fault is his.</para>
    /// <para>Placeholders are not counted against it. Saying "something was here
    /// and I could not tell you what" is the honest output and always
    /// available.</para>
    /// </remarks>
    [Theory]
    [InlineData("exchange-easy")]
    [InlineData("coverage-easy")]
    public void TheSettledPassShowsNoStrangersAtFullStrength(string name)
    {
        var audio = WavAudio.Read(
            Path.Combine(CwFixtureCatalogue.Folder, name + ".wav"));

        var recipe = CwFixtureCatalogue.All.Single(r => r.Name == name);

        var expected = recipe.Text
            .Replace("^", "", StringComparison.Ordinal)
            .Replace(" ", "", StringComparison.Ordinal)
            .ToUpperInvariant();

        var decoder = new CwDecoder(audio.SampleRate, 600);
        var strangers = new List<string>();
        var confident = 0;

        decoder.CharacterSettled += c =>
        {
            if (c.IsWordGap || c.IsUnreadable || c.Confidence != CwConfidence.High)
            {
                return;
            }

            confident++;

            if (c.Text.Length == 1
                && !expected.Contains(c.Text[0], StringComparison.Ordinal))
            {
                strangers.Add(c.Text);
            }
        };

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        _output.WriteLine($"{name}: {confident} settled characters at full "
            + $"strength, {strangers.Count} of them not in the message"
            + (strangers.Count == 0 ? "" : ": " + string.Join(", ", strangers)));

        // **IT REACHED ZERO, AND NOT BY THE ROUTE THE RULING EXPECTED.**
        // HM-DEC-108 added a third confidence measurement for this, on the
        // reasoning that the strangers came from marginal character boundaries.
        // The measurement is built and it moved these numbers not at all: every
        // stranger scored a boundary margin of one, because the gaps around it
        // were decisively wide.
        //
        // Tracing what was actually emitted found the real cause. Each stranger
        // was the leading dashes of the character that followed it — a lone dah
        // before D, before N, four dashes before a nought — with the real
        // character arriving whole right behind it. The gap after a window's last
        // mark was infinity, which asserts that the character certainly ended
        // there, and a window has no business asserting that: it is a view onto a
        // stream, so silence it has not seen yet is silence nobody has measured.
        //
        // The gap is measured now, and a character whose end the window did not
        // see is held for the next one, where it sits in the interior. That is
        // phase 4's own remedy for the mark-at-the-edge case applied to the
        // silence afterwards, which nothing was watching.
        //
        // The cost is real and is the reason this counts strangers rather than
        // characters: five characters come out of these fixtures where eight and
        // seven did, because a fragment is no longer published as a character.
        // **NONE, AND IT IS A BAR RATHER THAN A RATCHET** (HM-DEC-114). This
        // carried a "worst allowed" figure while the audio was unproved, which
        // was right then and is wrong now: a ratchet on a proved fixture records
        // that the decoder is still wrong without ever requiring it to stop
        // being wrong.
        Assert.True(
            strangers.Count == 0,
            $"the settled pass showed {strangers.Count} characters at full "
            + $"strength that are not in the message "
            + $"({string.Join(", ", strangers)})");
    }
}
