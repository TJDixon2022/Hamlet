using System.Diagnostics;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Message;

/// <summary>
/// The rolling cache: what it does with a miss, what it does with a collision, and the three legs of
/// the round trip at the smallest scope that shows each one.
/// </summary>
/// <remarks>
/// <b>Every test here builds its own cache.</b> There is no shared instance and no fixture, so no
/// test can see a callsign another test stored and no result depends on the order xunit happened to
/// pick. That is the isolation property the cache was designed for and it is asserted directly
/// rather than assumed.
/// </remarks>
public class Ft8CallsignCacheTests
{
    private readonly ITestOutputHelper _output;

    public Ft8CallsignCacheTests(ITestOutputHelper output) => _output = output;

    private const int Seed = 20841;

    /// <summary>
    /// A hash the cache has never seen resolves to no text at all — at the field, at the message,
    /// and through the dispatcher.
    /// </summary>
    /// <remarks>
    /// <b>This is the assertion the whole unit turns on</b> (HM-DEC-009). Upstream writes a literal
    /// placeholder into the field on a miss and hands back the message with it in. There is no
    /// placeholder here, no numeric field dressed as a call, and no partial message.
    /// </remarks>
    [Fact]
    public void AMissRefusesAtEveryLevel()
    {
        var cold = new Ft8CallsignCache();

        // At the cache.
        Assert.Equal(Ft8CacheLookup.NotFound, cold.TryLookup(Ft8CallsignHashWidth.Bits22, 12345, out var nothing));
        Assert.Equal(string.Empty, nothing);
        Assert.Equal(Ft8CacheLookup.NotFound, cold.TryLookup(Ft8CallsignHashWidth.Bits12, 1234, out nothing));
        Assert.Equal(string.Empty, nothing);
        Assert.Equal(Ft8CacheLookup.NotFound, cold.TryLookup(Ft8CallsignHashWidth.Bits10, 123, out nothing));
        Assert.Equal(string.Empty, nothing);

        // At the field. Every value in the hashed sub-range refuses against a cold cache, which is
        // the property unit 207 measured across the whole range and which the cache must not weaken.
        for (uint offset = 0; offset < 4096; offset++)
        {
            var value = Ft8CallsignField.TokenRangeSize + (offset * 1024);
            if (value >= Ft8CallsignField.TokenRangeSize + Ft8CallsignField.HashRangeSize)
            {
                break;
            }

            var result = Ft8CallsignField.TryUnpack(value, false, 1, cold, out var text, out var type);
            Assert.Equal(Ft8FieldResult.UnresolvedCallsign, result);
            Assert.Equal(string.Empty, text);
            Assert.Equal(Ft8FieldType.Unknown, type);
        }

        // At the message and through the dispatcher, on real bits: a standard message naming a
        // non-standard station, packed against a cache that knew the call, read by one that does not.
        // The non-standard call is the addressed station rather than the transmitting one, because a
        // CQ addressed to a non-standard call is refused by this message type upstream and here — it
        // is what the type built in task 5 exists for.
        var warm = new Ft8CallsignCache();
        Span<byte> message = stackalloc byte[Ft8StandardMessage.MessageBytes];
        Assert.Equal(
            Ft8PackResult.Ok,
            Ft8StandardMessage.TryPack("PJ4/KA1ABC", "W9XYZ", "FN42", warm, message));

        Assert.Equal(Ft8DecodeStatus.UnresolvedCallsign, Ft8StandardMessage.TryUnpack(message, cold, out var fields));
        Assert.Equal(default, fields);

        var refused = Ft8MessageDecoder.Decode(message, cold);
        Assert.False(refused.Decoded);
        Assert.Equal(Ft8DecodeStatus.UnresolvedCallsign, refused.Status);
        Assert.Equal(string.Empty, refused.Text);
        Assert.Equal(default, refused.Fields);

        // And a null cache is a cold cache, so unit 207's seam is exactly where it was.
        Assert.Equal(Ft8DecodeStatus.UnresolvedCallsign, Ft8StandardMessage.TryUnpack(message, null, out _));
        Assert.False(Ft8MessageDecoder.Decode(message).Decoded);

        _output.WriteLine("a miss writes no text at the cache, the field, the message or the dispatcher.");
    }

    /// <summary>
    /// The three legs of the round trip, on a standard message carrying a 22-bit hashed callsign.
    /// </summary>
    /// <remarks>
    /// The non-standard-callsign message's own three legs are asserted beside its packer; this is the
    /// same three properties one level down, on the type unit 207 already built, because the cache
    /// changed what that type can do too.
    /// </remarks>
    [Fact]
    public void TheThreeLegsHoldForAHashedCallsignInAStandardMessage()
    {
        const string Nonstandard = "PJ4/KA1ABC";

        // Leg one, the full call: a message with no hashed field in it needs no cache at all.
        Span<byte> plain = stackalloc byte[Ft8StandardMessage.MessageBytes];
        Assert.Equal(Ft8PackResult.Ok, Ft8StandardMessage.TryPack("CQ", "W9XYZ", "FN42", null, plain));
        var plainDecode = Ft8MessageDecoder.Decode(plain, null);
        Assert.True(plainDecode.Decoded);
        Assert.Equal("CQ W9XYZ FN42", plainDecode.Text);

        // Leg two, the hashed leg: pack against a cache that learns the call, then read the bits
        // with a cache that has heard it, and get the call back.
        var transmitter = new Ft8CallsignCache();
        Span<byte> hashed = stackalloc byte[Ft8StandardMessage.MessageBytes];
        Assert.Equal(
            Ft8PackResult.Ok,
            Ft8StandardMessage.TryPack(Nonstandard, "W9XYZ", "FN42", transmitter, hashed));

        var receiver = new Ft8CallsignCache();
        Assert.Equal(Ft8CacheStore.Stored, receiver.Save(Nonstandard));

        var warmDecode = Ft8MessageDecoder.Decode(hashed, receiver);
        Assert.True(warmDecode.Decoded);
        Assert.Equal($"<{Nonstandard}> W9XYZ FN42", warmDecode.Text);
        Assert.Equal(Ft8FieldType.Callsign, warmDecode.Fields.CallToType);

        // Leg three, the cold cache: the same bits, a receiver that has never heard the station,
        // and no answer at all.
        var stranger = new Ft8CallsignCache();
        var coldDecode = Ft8MessageDecoder.Decode(hashed, stranger);
        Assert.False(coldDecode.Decoded);
        Assert.Equal(Ft8DecodeStatus.UnresolvedCallsign, coldDecode.Status);
        Assert.Equal(string.Empty, coldDecode.Text);

        _output.WriteLine("full call, hashed through a warm cache, and the same bits refused by a cold one.");
    }

    /// <summary>
    /// A decode teaches the cache the callsigns it read, which is how a receiver comes to be able to
    /// resolve a hash it has never been told directly.
    /// </summary>
    [Fact]
    public void ADecodeFillsTheCacheItReadsFrom()
    {
        var receiver = new Ft8CallsignCache();
        Assert.Equal(0, receiver.Count);

        // First transmission: the station spells its call out in full.
        Span<byte> spelled = stackalloc byte[Ft8StandardMessage.MessageBytes];
        Assert.Equal(Ft8PackResult.Ok, Ft8StandardMessage.TryPack("CQ", "W9XYZ", "FN42", null, spelled));
        Assert.True(Ft8MessageDecoder.Decode(spelled, receiver).Decoded);
        Assert.True(receiver.Contains("W9XYZ"));

        // Second transmission, from somebody else, naming that station by its hash alone. The
        // receiver was never told the call; it remembered it.
        var caller = new Ft8CallsignCache();
        Assert.Equal(Ft8CacheStore.Stored, caller.Save("W9XYZ", out var hash22, out _, out _));

        Span<byte> byHash = stackalloc byte[Ft8StandardMessage.MessageBytes];
        Assert.Equal(Ft8PackResult.Ok, Ft8StandardMessage.TryPack("K1ABC", "PJ4/KA1ABC", "RRR", caller, byHash));

        // Prove the hash really is in the bits rather than the call.
        Assert.True(hash22 <= Ft8CallsignHash.Mask22);

        var second = Ft8MessageDecoder.Decode(byHash, receiver);
        Assert.False(second.Decoded);
        Assert.Equal(Ft8DecodeStatus.UnresolvedCallsign, second.Status);

        // Once the receiver hears that station name itself, the same bits read.
        Assert.Equal(Ft8CacheStore.Stored, receiver.Save("PJ4/KA1ABC"));
        var third = Ft8MessageDecoder.Decode(byHash, receiver);
        Assert.True(third.Decoded);
        Assert.Equal("K1ABC <PJ4/KA1ABC> RRR", third.Text);
    }

    /// <summary>
    /// A collision at each of the three widths, found by search over this project's own generated
    /// callsigns, and refused rather than answered.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>The decision this test asserts, in full.</b> Where two distinct callsigns in the cache
    /// share the hash being looked up, the cache returns nothing. Upstream stores both and returns
    /// whichever its probe chain reaches first, which is a real, plausible, entirely wrong callsign
    /// presented with no mark of doubt on it — the one output HM-DEC-009 forbids. Refusing costs a
    /// decode that upstream would have shown; showing it costs the operator a contact with a station
    /// that was never on the air, and a wrong callsign in a log is worse than a gap in one. Recorded
    /// beside unit 207's five divergences in <c>porting-notes.md</c>.
    /// </para>
    /// <para>
    /// <b>Both calls in each pair are this project's own data</b>, produced by
    /// <see cref="CallsignCorpus"/>, so they may be named in a report. Nothing here came out of the
    /// pinned clone.
    /// </para>
    /// </remarks>
    [Fact]
    public void ACollisionIsRefusedRatherThanAnswered()
    {
        var clock = Stopwatch.StartNew();
        var calls = CallsignCorpus.Distinct(Seed, 200_000);

        var pairs = new Dictionary<Ft8CallsignHashWidth, (string First, string Second)>();
        var seen22 = new Dictionary<uint, string>();
        var seen12 = new Dictionary<uint, string>();
        var seen10 = new Dictionary<uint, string>();
        var scanned = 0;

        foreach (var call in calls)
        {
            scanned++;
            if (!Ft8CallsignHash.TryCompute(call, out var h22, out var h12, out var h10))
            {
                continue;
            }

            Record(seen22, Ft8CallsignHashWidth.Bits22, h22, call);
            Record(seen12, Ft8CallsignHashWidth.Bits12, h12, call);
            Record(seen10, Ft8CallsignHashWidth.Bits10, h10, call);

            if (pairs.Count == 3)
            {
                break;
            }
        }

        clock.Stop();

        _output.WriteLine($"seed                       : {Seed}");
        _output.WriteLine($"callsigns walked           : {scanned} of {calls.Count} generated");
        _output.WriteLine($"search time                : {clock.ElapsedMilliseconds} ms");

        foreach (var width in new[]
                 {
                     Ft8CallsignHashWidth.Bits22, Ft8CallsignHashWidth.Bits12, Ft8CallsignHashWidth.Bits10,
                 })
        {
            Assert.True(pairs.ContainsKey(width), $"no colliding pair was found at {width}.");
            var (first, second) = pairs[width];
            _output.WriteLine($"colliding pair at {width,-6}   : {first} and {second}");

            Assert.NotEqual(first, second);

            // One of them alone resolves. That is the point: refusing is a property of the
            // ambiguity, not a blanket refusal of everything hashed.
            var one = new Ft8CallsignCache();
            Assert.Equal(Ft8CacheStore.Stored, one.Save(first));
            Assert.Equal(Ft8CacheLookup.Found, one.TryLookup(width, HashAt(width, first), out var resolved));
            Assert.Equal(first, resolved);

            // Both of them together, and the cache refuses rather than picking one.
            var both = new Ft8CallsignCache();
            Assert.Equal(Ft8CacheStore.Stored, both.Save(first));
            Assert.Equal(Ft8CacheStore.Stored, both.Save(second));
            Assert.Equal(Ft8CacheLookup.Ambiguous, both.TryLookup(width, HashAt(width, first), out var refused));
            Assert.Equal(string.Empty, refused);

            // And the same hash looked up from the other call's side, which is the same hash.
            Assert.Equal(HashAt(width, first), HashAt(width, second));
            Assert.Equal(Ft8CacheLookup.Ambiguous, both.TryLookup(width, HashAt(width, second), out refused));
            Assert.Equal(string.Empty, refused);
        }

        void Record(Dictionary<uint, string> seen, Ft8CallsignHashWidth width, uint hash, string call)
        {
            if (pairs.ContainsKey(width))
            {
                return;
            }

            if (seen.TryGetValue(hash, out var earlier))
            {
                if (!string.Equals(earlier, call, StringComparison.Ordinal))
                {
                    pairs[width] = (earlier, call);
                }
            }
            else
            {
                seen[hash] = call;
            }
        }
    }

    /// <summary>
    /// A collision refuses through the whole stack, not just at the cache.
    /// </summary>
    [Fact]
    public void ACollisionRefusesTheWholeMessage()
    {
        var (first, second) = FindPair(Ft8CallsignHashWidth.Bits22);

        // A message naming the first station by its hash, packed by somebody who knows only it.
        var transmitter = new Ft8CallsignCache();
        Span<byte> message = stackalloc byte[Ft8StandardMessage.MessageBytes];
        Assert.Equal(Ft8PackResult.Ok, Ft8StandardMessage.TryPack(first, "W9XYZ", "FN42", transmitter, message));

        // A receiver that has heard only the first station reads it.
        var narrow = new Ft8CallsignCache();
        narrow.Save(first);
        var resolved = Ft8MessageDecoder.Decode(message, narrow);
        Assert.True(resolved.Decoded);
        Assert.Equal($"<{first}> W9XYZ FN42", resolved.Text);

        // A receiver that has heard both cannot tell which is on the air, and says so by saying
        // nothing. The same bits, a fuller cache, and a refusal rather than a coin toss.
        //
        // THIS IS THE CASE THE WHOLE UNIT WAS MOST AT RISK OF GETTING WRONG. A cache that knows
        // more here produces less, and that is the correct direction: the extra knowledge is what
        // reveals that the answer was never certain.
        var ambiguous = new Ft8CallsignCache();
        ambiguous.Save(first);
        ambiguous.Save(second);
        var refused = Ft8MessageDecoder.Decode(message, ambiguous);
        Assert.False(refused.Decoded);
        Assert.Equal(Ft8DecodeStatus.UnresolvedCallsign, refused.Status);
        Assert.Equal(string.Empty, refused.Text);

        _output.WriteLine($"{first} and {second} share a 22-bit hash; a cache holding both decodes neither.");
    }

    /// <summary>Two caches share nothing, which is what makes a corpus reproducible.</summary>
    [Fact]
    public void CachesAreIsolatedFromOneAnother()
    {
        var a = new Ft8CallsignCache();
        var b = new Ft8CallsignCache();

        Assert.Equal(Ft8CacheStore.Stored, a.Save("PJ4/KA1ABC", out var hash, out _, out _));
        Assert.Equal(1, a.Count);
        Assert.Equal(0, b.Count);

        Assert.Equal(Ft8CacheLookup.Found, a.TryLookup(Ft8CallsignHashWidth.Bits22, hash, out _));
        Assert.Equal(Ft8CacheLookup.NotFound, b.TryLookup(Ft8CallsignHashWidth.Bits22, hash, out var nothing));
        Assert.Equal(string.Empty, nothing);

        // And there is no static one to leak through. Nothing in the library exposes a cache it did
        // not receive as an argument.
        Assert.Empty(typeof(Ft8CallsignCache).GetProperties(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static));
        Assert.DoesNotContain(
            typeof(Ft8CallsignCache).GetFields(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static),
            f => f.FieldType == typeof(Ft8CallsignCache));
    }

    /// <summary>Storing the same call twice is one entry, and it resets the call's age.</summary>
    [Fact]
    public void TheSameCallStoredTwiceIsOneEntry()
    {
        var cache = new Ft8CallsignCache();
        Assert.Equal(Ft8CacheStore.Stored, cache.Save("W9XYZ"));
        Assert.Equal(Ft8CacheStore.AlreadyPresent, cache.Save("W9XYZ"));
        Assert.Equal(1, cache.Count);
    }

    /// <summary>What the cache declines to remember at all.</summary>
    [Fact]
    public void TheCacheRefusesWhatCannotBeACallsign()
    {
        var cache = new Ft8CallsignCache();

        Assert.Equal(Ft8CacheStore.TooShort, cache.Save(null!));
        Assert.Equal(Ft8CacheStore.TooShort, cache.Save(string.Empty));
        Assert.Equal(Ft8CacheStore.TooShort, cache.Save("W9"));
        Assert.Equal(Ft8CacheStore.NotHashable, cache.Save("w9xyz"));
        Assert.Equal(Ft8CacheStore.NotHashable, cache.Save("<W9XYZ>"));
        Assert.Equal(0, cache.Count);

        // A hash wider than the width it claims to be is not the hash of anything stored.
        cache.Save("W9XYZ");
        Assert.Equal(
            Ft8CacheLookup.NotFound,
            cache.TryLookup(Ft8CallsignHashWidth.Bits12, 1u << Ft8CallsignHash.Bits12, out var nothing));
        Assert.Equal(string.Empty, nothing);
    }

    /// <summary>
    /// A full cache says it is full rather than spinning, and the calls it did take still resolve.
    /// </summary>
    /// <remarks>
    /// Upstream's insert loop has no bound and walks forever once every slot is taken. This is the
    /// bounded version answering instead, which is a divergence in what happens and not in what is
    /// stored: the table's contents are the same either way.
    /// </remarks>
    [Fact]
    public void AFullCacheAnswersRatherThanSpinning()
    {
        var cache = new Ft8CallsignCache(16);
        var calls = CallsignCorpus.Distinct(Seed + 7, 64);

        var stored = 0;
        var full = 0;
        foreach (var call in calls)
        {
            switch (cache.Save(call))
            {
                case Ft8CacheStore.Stored:
                    stored++;
                    break;
                case Ft8CacheStore.Full:
                    full++;
                    break;
            }
        }

        Assert.Equal(16, stored);
        Assert.True(full > 0);
        Assert.Equal(16, cache.Count);

        _output.WriteLine($"a cache of 16 took {stored} calls and refused {full} more without hanging.");
    }

    /// <summary>Ageing forgets the calls that stopped being heard and keeps the ones that did not.</summary>
    [Fact]
    public void AgeingForgetsWhatStoppedBeingHeard()
    {
        var cache = new Ft8CallsignCache();
        cache.Save("W9XYZ");
        cache.Save("K1ABC");
        Assert.Equal(2, cache.Count);

        // Three passes, hearing one of them again each time.
        for (var pass = 0; pass < 3; pass++)
        {
            cache.Age(1);
            cache.Save("W9XYZ");
        }

        Assert.True(cache.Contains("W9XYZ"));
        Assert.False(cache.Contains("K1ABC"));
        Assert.Equal(1, cache.Count);

        cache.Clear();
        Assert.Equal(0, cache.Count);
        Assert.False(cache.Contains("W9XYZ"));

        Assert.Throws<ArgumentOutOfRangeException>(() => cache.Age(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Ft8CallsignCache(0));
    }

    /// <summary>
    /// The cache stores a callsign as it was given rather than clipping it to eleven characters, and
    /// two calls that agree to eleven are two entries rather than one wrong one.
    /// </summary>
    /// <remarks>
    /// <b>A recorded divergence.</b> Upstream's table copies eleven characters into a fixed buffer,
    /// so two calls agreeing that far collapse into one entry spelled as neither of them, and a
    /// lookup then returns that. Storing what was actually heard turns the same case into an
    /// ambiguity, which refuses. The hash itself is unchanged and still reads eleven characters,
    /// because that part is on the air.
    /// </remarks>
    [Fact]
    public void TheCacheStoresTheCallItWasGiven()
    {
        var cache = new Ft8CallsignCache();
        Assert.Equal(Ft8CacheStore.Stored, cache.Save("ABCDEFGHIJKL", out var hash, out _, out _));
        Assert.Equal(Ft8CacheStore.Stored, cache.Save("ABCDEFGHIJKM"));

        Assert.True(cache.Contains("ABCDEFGHIJKL"));
        Assert.True(cache.Contains("ABCDEFGHIJKM"));
        Assert.Equal(2, cache.Count);

        // They share one hash on the air, so the cache refuses rather than answering with either.
        Assert.Equal(Ft8CacheLookup.Ambiguous, cache.TryLookup(Ft8CallsignHashWidth.Bits22, hash, out var nothing));
        Assert.Equal(string.Empty, nothing);
    }

    private static uint HashAt(Ft8CallsignHashWidth width, string call)
    {
        Ft8CallsignHash.TryCompute(call, out var h22, out var h12, out var h10);
        return width switch
        {
            Ft8CallsignHashWidth.Bits22 => h22,
            Ft8CallsignHashWidth.Bits12 => h12,
            _ => h10,
        };
    }

    /// <summary>The first pair of distinct generated callsigns that collide at the given width.</summary>
    private static (string First, string Second) FindPair(Ft8CallsignHashWidth width)
    {
        var seen = new Dictionary<uint, string>();
        foreach (var call in CallsignCorpus.Distinct(Seed, 200_000))
        {
            if (!Ft8CallsignHash.TryCompute(call, out _, out _, out _))
            {
                continue;
            }

            var hash = HashAt(width, call);
            if (seen.TryGetValue(hash, out var earlier))
            {
                if (!string.Equals(earlier, call, StringComparison.Ordinal))
                {
                    return (earlier, call);
                }
            }
            else
            {
                seen[hash] = call;
            }
        }

        throw new InvalidOperationException($"no colliding pair at {width} in the generated corpus.");
    }
}
