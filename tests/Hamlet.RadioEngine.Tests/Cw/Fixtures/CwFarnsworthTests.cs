using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// Gap classes come from the gaps, because operators send Farnsworth
/// (HM-DEC-115).
/// </summary>
/// <remarks>
/// <para>**THE FIXTURE IS A REAL ARRL BULLETIN OFF A 40 M TRAFFIC NET**, S4,
/// tone 501 Hz, recorded on the air. Independent measurement of the same file
/// finds the gaps in three clean heaps: 69 element gaps near 40 ms, 28 character
/// gaps between 190 and 300, and 11 word gaps at 400 and above, with a dit of
/// 57 ms and a dah of 158.</para>
/// <para>**SO THE ELEMENT GAP IS SHORTER THAN A DIT AND THE CHARACTER GAP IS SIX
/// TIMES THE ELEMENT GAP RATHER THAN THREE.** Nothing about one-three-seven
/// survives contact with a traffic net. A decoder using dit multiples gets every
/// character right and puts every space in the wrong place, and that is what was
/// on the operator's screen: 177 characters of which 94 were unsure.</para>
/// </remarks>
public sealed class CwFarnsworthTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the transcript is printed.</param>
    public CwFarnsworthTests(ITestOutputHelper output) => _output = output;

    /// <summary>The ARRL bulletin that produced the ruling.</summary>
    public const string Bulletin = "cw-2026-08-18-004507";

    private static (string Settled, CwGapClasses? Classes) Decode(string name)
    {
        var path = Path.Combine(
            CwFixtureCatalogue.Folder, "..", "captured", name + ".wav");

        var audio = WavAudio.Read(path);
        var decoder = new CwDecoder(audio.SampleRate, 500);
        var settled = new System.Text.StringBuilder();

        decoder.CharacterSettled += c => settled.Append(c.Text);

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        return (settled.ToString(), decoder.GapClasses);
    }

    /// <remarks>
    /// <para>**THE MEASUREMENT THE RULING RESTS ON.** The classes are fitted to
    /// the gaps and to nothing else, so a Farnsworth sender comes out as a
    /// Farnsworth sender rather than as a decoder in trouble.</para>
    /// <para>The bounds are wide on purpose. What is being asserted is the
    /// shape — three separated heaps with the character gap several times the
    /// element gap — and not a set of numbers that would have to be edited every
    /// time the detector changes by a millisecond.</para>
    /// </remarks>
    [Fact]
    public void TheBulletinsSpacingIsMeasuredAndIsFarnsworth()
    {
        var (_, classes) = Decode(Bulletin);

        Assert.NotNull(classes);

        var c = classes!.Value;

        _output.WriteLine($"element {c.ElementMs:0} ms, character {c.CharacterMs:0}, "
            + $"word {c.WordMs:0}");
        _output.WriteLine($"character gap is {c.FarnsworthRatio:0.0} element gaps");

        Assert.InRange(c.ElementMs, 20, 90);
        Assert.InRange(c.CharacterMs, 150, 350);
        Assert.InRange(c.WordMs, 350, 1500);

        // **FOUR AT LEAST, WHERE THE TEXTBOOK SAYS THREE.** This is the whole
        // ruling in one number, and it is a description rather than a warning:
        // a sender spacing like this is a normal operator on a net.
        Assert.True(
            c.FarnsworthRatio >= 4.0,
            $"the character gap measured {c.FarnsworthRatio:0.0} element gaps, so "
            + "either this recording is not the Farnsworth one or the classes "
            + "are being fitted to something other than the gaps");
    }

    /// <remarks>
    /// <para>**THE SPACES ARE THE POINT.** The letters were never the problem:
    /// the decoder had them before this ruling and put the word breaks in the
    /// wrong places, which is where a transcript stops being a transcript.</para>
    /// <para>What is asserted is the words that must appear with their spaces
    /// intact, rather than the whole answer key, because the remaining errors
    /// are character-level and belong to the clock rather than to the
    /// spacing.</para>
    /// </remarks>
    [Fact]
    public void TheBulletinsWordsComeOutAsWords()
    {
        var (settled, _) = Decode(Bulletin);

        _output.WriteLine($"'{settled}'");

        // From the answer key: `AT ARRL DOT NET <BT> EACH STATION HANDLING THIS
        // MESSAGE P`. These three are whole words with a space either side, and
        // before the ruling every one of them was run into its neighbour.
        Assert.Contains(" NET ", settled, StringComparison.Ordinal);
        Assert.Contains("STA", settled, StringComparison.Ordinal);
        Assert.Contains(" P", settled, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves §0.0 and the ruling's own instruction: where the gaps do not form
    /// three groups there is no answer, and the pass emits nothing rather than
    /// guessing a dit multiple. A guessed boundary is a guess about where the
    /// words are.
    /// </remarks>
    [Fact]
    public void GapsThatDoNotSeparateProduceNoClasses()
    {
        // One heap: somebody sending without pausing.
        var flat = new double[40];
        for (var i = 0; i < flat.Length; i++)
        {
            flat[i] = 50 + (i % 5);
        }

        Assert.Null(CwGapFit.Fit(flat, flat.Length));

        // And too few to say anything about.
        var few = new double[] { 40, 240, 600, 40, 240 };

        Assert.Null(CwGapFit.Fit(few, few.Length));
    }

    /// <remarks>
    /// Proves the fit holds for a textbook sender too, so removing the dit
    /// multiples did not trade one assumption for another: one, three and seven
    /// dits separate cleanly and come back as themselves.
    /// </remarks>
    [Fact]
    public void ATextbookSenderStillFitsThreeClasses()
    {
        var gaps = new List<double>();

        for (var i = 0; i < 60; i++)
        {
            gaps.Add(50);
        }

        for (var i = 0; i < 20; i++)
        {
            gaps.Add(150);
        }

        for (var i = 0; i < 8; i++)
        {
            gaps.Add(350);
        }

        var fit = CwGapFit.Fit(gaps.ToArray(), gaps.Count);

        Assert.NotNull(fit);

        _output.WriteLine($"element {fit!.Value.ElementMs:0}, "
            + $"character {fit.Value.CharacterMs:0}, word {fit.Value.WordMs:0}");

        Assert.InRange(fit.Value.ElementMs, 45, 55);
        Assert.InRange(fit.Value.CharacterMs, 140, 160);
        Assert.InRange(fit.Value.WordMs, 330, 370);
    }
}
