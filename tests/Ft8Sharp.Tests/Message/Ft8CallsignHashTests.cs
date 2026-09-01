using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Message;

/// <summary>
/// The three callsign hashes: leg B of their provenance, and the structural properties that are
/// cheap to assert and worth asserting.
/// </summary>
/// <remarks>
/// <b>None of this is evidence that the hash agrees with upstream's.</b> An independent computation
/// agreeing with the library catches an ordinary porting slip and nothing more; a misreading of the
/// pin made twice survives it. Leg A —
/// <see cref="UpstreamCallsignHashProvenanceTests"/> — reads the pin's own scalars by machine, and
/// step 3's bit-identical symbol comparison against upstream is what finally settles it, provided
/// that comparison includes a message carrying a hashed callsign.
/// </remarks>
public class Ft8CallsignHashTests
{
    private readonly ITestOutputHelper _output;

    public Ft8CallsignHashTests(ITestOutputHelper output) => _output = output;

    /// <summary>The seed every corpus in this class is generated from. Stated in the report.</summary>
    private const int Seed = 20826;

    /// <summary>How many callsigns leg B is run over.</summary>
    private const int CorpusSize = 100_000;

    /// <summary>
    /// Leg B: a second computation of all three hashes, written from the pin and not calling the
    /// library, agrees with the library over a large seeded corpus across every shape.
    /// </summary>
    [Fact]
    public void AnIndependentComputationAgreesOverASeededCorpus()
    {
        var random = new Random(Seed);
        var agreed = 0;
        var refusedByBoth = 0;
        var disagreed = 0;
        var shapes = new int[CallsignCorpus.ShapeCount];

        for (var i = 0; i < CorpusSize; i++)
        {
            var shape = i % CallsignCorpus.ShapeCount;
            shapes[shape]++;
            var call = CallsignCorpus.Generate(random, shape, out _);

            var ours = Ft8CallsignHash.TryCompute(call, out var a22, out var a12, out var a10);
            var theirs = HashCheck.TryCompute(call, out var b22, out var b12, out var b10);

            Assert.Equal(theirs, ours);
            if (!ours)
            {
                refusedByBoth++;
                continue;
            }

            if (a22 == b22 && a12 == b12 && a10 == b10)
            {
                agreed++;
            }
            else
            {
                disagreed++;
            }
        }

        _output.WriteLine($"seed                     : {Seed}");
        _output.WriteLine($"callsigns                : {CorpusSize} across {CallsignCorpus.ShapeCount} shapes");
        _output.WriteLine($"agreed on all three      : {agreed}");
        _output.WriteLine($"refused by both          : {refusedByBoth}");
        _output.WriteLine($"disagreed                : {disagreed}");

        Assert.Equal(0, disagreed);
        Assert.Equal(CorpusSize, agreed + refusedByBoth);

        // Every shape actually appeared, so "across every shape" is a measurement rather than an
        // intention.
        Assert.All(shapes, n => Assert.True(n > 0));
    }

    /// <summary>
    /// The relationship between the three widths, asserted rather than assumed.
    /// </summary>
    /// <remarks>
    /// <b>This is a structural fact and it is the reason one stored value answers three lookups.</b>
    /// The narrow hashes are not separate functions of the callsign: they are the wide one with its
    /// low bits dropped. Stated as a relationship, never as a value.
    /// </remarks>
    [Fact]
    public void TheNarrowHashesAreTruncationsOfTheWideOne()
    {
        var random = new Random(Seed);

        for (var i = 0; i < 20_000; i++)
        {
            var call = CallsignCorpus.Generate(random, i % CallsignCorpus.ShapeCount, out _);
            if (!Ft8CallsignHash.TryCompute(call, out var h22, out var h12, out var h10))
            {
                continue;
            }

            Assert.Equal(h22 >> Ft8CallsignHash.Shift12, h12);
            Assert.Equal(h22 >> Ft8CallsignHash.Shift10, h10);

            // And therefore the ten is a truncation of the twelve, which is what makes the probe
            // chain the same chain whichever width is being looked up.
            Assert.Equal(h12 >> (Ft8CallsignHash.Shift10 - Ft8CallsignHash.Shift12), h10);

            Assert.True(h22 <= Ft8CallsignHash.Mask22);
            Assert.True(h12 < (1u << Ft8CallsignHash.Bits12));
            Assert.True(h10 < (1u << Ft8CallsignHash.Bits10));
        }

        _output.WriteLine(
            "structural: the 12-bit hash is the 22-bit hash shifted down, the 10-bit hash is the "
            + "22-bit hash shifted further, and the 10 is therefore a truncation of the 12.");
    }

    /// <summary>The same call gives the same hash every time, and across separate computations.</summary>
    [Fact]
    public void TheHashIsDeterministic()
    {
        var random = new Random(Seed);
        var calls = new List<string>();
        for (var i = 0; i < 500; i++)
        {
            calls.Add(CallsignCorpus.Generate(random, i % CallsignCorpus.ShapeCount, out _));
        }

        foreach (var call in calls)
        {
            Ft8CallsignHash.TryCompute(call, out var first, out _, out _);
            for (var repeat = 0; repeat < 3; repeat++)
            {
                Ft8CallsignHash.TryCompute(call, out var again, out _, out _);
                Assert.Equal(first, again);
            }
        }

        // Nothing here is instance state, so there is nothing to carry between calls. That is worth
        // asserting because the cache beside it is state, and the two must not be confused.
        Assert.True(typeof(Ft8CallsignHash).IsAbstract && typeof(Ft8CallsignHash).IsSealed);
    }

    /// <summary>
    /// Case and whitespace, measured rather than decided.
    /// </summary>
    /// <remarks>
    /// <b>This asserts what the pin does, not what would be friendlier.</b> The alphabet the hash
    /// packs against holds upper-case letters and no lower-case ones, so a lower-case call has no
    /// hash at all and is refused rather than folded. A leading or trailing space is a character in
    /// that alphabet and changes the hash, which means the caller has to have trimmed the call
    /// before it gets here — and every caller in this library has.
    /// </remarks>
    [Fact]
    public void CaseAndWhitespaceBehaveAsThePinDefinesThem()
    {
        Assert.True(Ft8CallsignHash.TryCompute("W9XYZ", out var upper, out _, out _));
        Assert.False(Ft8CallsignHash.TryCompute("w9xyz", out _, out _, out _));

        // A trailing space is not the same call. Upstream pads with the space, so a call with one
        // written into it is a call one character longer, and hashes differently.
        Assert.True(Ft8CallsignHash.TryCompute(" W9XYZ", out var leading, out _, out _));
        Assert.NotEqual(upper, leading);

        // The slash is in the alphabet, which is what lets a portable call be hashed at all.
        Assert.True(Ft8CallsignHash.TryCompute("PJ4/KA1ABC", out _, out _, out _));

        // Nothing outside the alphabet is.
        Assert.False(Ft8CallsignHash.TryCompute("W9-XYZ", out _, out _, out _));
        Assert.False(Ft8CallsignHash.TryCompute("<W9XYZ>", out _, out _, out _));

        _output.WriteLine("case-sensitive, space-significant, slash admitted, everything else refused.");
    }

    /// <summary>
    /// The eleven-character bound, and the collision it creates on the air.
    /// </summary>
    /// <remarks>
    /// <b>Inherited deliberately.</b> Upstream reads eleven characters and stops, so two calls that
    /// agree in their first eleven have one hash between them wherever they are heard. Repairing it
    /// here would make this library disagree with every station transmitting, which is the one
    /// failure the hash cannot survive.
    /// </remarks>
    [Fact]
    public void ElevenCharactersIsWhatTheHashReads()
    {
        Assert.True(Ft8CallsignHash.TryCompute("ABCDEFGHIJK", out var eleven, out _, out _));
        Assert.True(Ft8CallsignHash.TryCompute("ABCDEFGHIJKL", out var twelve, out _, out _));
        Assert.True(Ft8CallsignHash.TryCompute("ABCDEFGHIJKZZZZ", out var fifteen, out _, out _));

        Assert.Equal(eleven, twelve);
        Assert.Equal(eleven, fifteen);

        // And a character past the eleventh cannot refuse the call either, because it is not read.
        Assert.True(Ft8CallsignHash.TryCompute("ABCDEFGHIJK???", out var withJunk, out _, out _));
        Assert.Equal(eleven, withJunk);

        _output.WriteLine(
            "the hash reads eleven characters; calls agreeing to eleven share one hash, upstream and here.");
    }

    /// <summary>
    /// The empty call has a hash, and nothing in this library ever stores it.
    /// </summary>
    /// <remarks>
    /// Upstream's function answers for the empty string — it is eleven spaces — and this one does
    /// too, because refusing where upstream answers would be a divergence with no reason behind it.
    /// What stops an empty call reaching a cache is the cache, and that is asserted where the cache
    /// is.
    /// </remarks>
    [Fact]
    public void TheEmptyCallsignHasAHash()
    {
        Assert.True(Ft8CallsignHash.TryCompute(string.Empty, out var empty, out _, out _));
        Assert.True(Ft8CallsignHash.TryCompute("           ", out var spaces, out _, out _));
        Assert.Equal(empty, spaces);

        Assert.False(Ft8CallsignHash.TryCompute(null!, out _, out _, out _));
    }

    /// <summary>Nothing handed to the hash throws, whatever it is.</summary>
    [Fact]
    public void NothingHandedToTheHashThrows()
    {
        var random = new Random(Seed + 1);
        var buffer = new char[24];

        for (var i = 0; i < 200_000; i++)
        {
            var length = random.Next(0, buffer.Length + 1);
            for (var c = 0; c < length; c++)
            {
                buffer[c] = (char)random.Next(0, 128);
            }

            var text = new string(buffer, 0, length);
            Ft8CallsignHash.TryCompute(text, out var h22, out var h12, out var h10);

            // Whatever the answer was, the outputs are inside their widths — a refusal writes zero.
            Assert.True(h22 <= Ft8CallsignHash.Mask22);
            Assert.True(h12 < (1u << Ft8CallsignHash.Bits12));
            Assert.True(h10 < (1u << Ft8CallsignHash.Bits10));
        }

        _output.WriteLine("200000 random strings of 0 to 24 arbitrary ASCII characters: no exception.");
    }

    /// <summary>The widths and shifts answer for all three, and refuse anything that is not one.</summary>
    [Fact]
    public void EveryWidthHasAShiftAndNothingElseDoes()
    {
        Assert.Equal(0, Ft8CallsignHash.ShiftFor(Ft8CallsignHashWidth.Bits22));
        Assert.Equal(Ft8CallsignHash.Shift12, Ft8CallsignHash.ShiftFor(Ft8CallsignHashWidth.Bits12));
        Assert.Equal(Ft8CallsignHash.Shift10, Ft8CallsignHash.ShiftFor(Ft8CallsignHashWidth.Bits10));

        Assert.Equal(22, Ft8CallsignHash.BitsOf(Ft8CallsignHashWidth.Bits22));
        Assert.Equal(12, Ft8CallsignHash.BitsOf(Ft8CallsignHashWidth.Bits12));
        Assert.Equal(10, Ft8CallsignHash.BitsOf(Ft8CallsignHashWidth.Bits10));

        Assert.Throws<ArgumentOutOfRangeException>(() => Ft8CallsignHash.ShiftFor((Ft8CallsignHashWidth)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => Ft8CallsignHash.BitsOf((Ft8CallsignHashWidth)99));

        // The hashed sub-range of the 28-bit callsign field is exactly the range of the 22-bit hash.
        // Two facts ported separately, and they have to be the same fact.
        Assert.Equal(Ft8CallsignField.HashRangeSize, Ft8CallsignHash.Mask22 + 1);
    }
}
