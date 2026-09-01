using System.Diagnostics;
using Ft8Sharp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Ldpc;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Message;

/// <summary>
/// Step 2's criterion 2 in one measurement — all four named categories of message round-tripped
/// across a large generated corpus — and criterion 3 re-taken with a cache in the picture.
/// </summary>
/// <remarks>
/// <para>
/// <b>Criterion 3 is re-taken rather than inherited, and the reason is exact.</b> Unit 207 closed it
/// on three zeros over a million random patterns, the third of which was <em>no decode returned for
/// an unresolvable callsign</em>. The cache changes that count by construction: patterns refused last
/// night can resolve tonight. So the fuzz runs twice, cold and warm, and the third count means a
/// different thing in each — with no cache it must still be zero, and with a full one it becomes
/// <em>no decode carrying a call the cache never stored</em>, with the resolved calls counted
/// separately as the correct outcome they are.
/// </para>
/// <para>
/// <b>Round-trip is defined here as three separate legs, because with a cache in the picture the
/// word is ambiguous in the direction of a false pass.</b> A corpus that only round-trips the call
/// carried in full has not touched the cache at all and would look exactly like a closed criterion.
/// The three are reported separately and never summed.
/// </para>
/// <para>
/// <b>None of this is evidence that the encoding agrees with upstream.</b> A packer and an unpacker
/// that agree are inverses and nothing more. What settles it is step 3's bit-identical symbol
/// comparison, and <b>that comparison must include a message carrying a hashed callsign</b> or the
/// hash goes unsettled into step 4.
/// </para>
/// </remarks>
public class Ft8Step2ClosingCorpusTests
{
    private readonly ITestOutputHelper _output;

    public Ft8Step2ClosingCorpusTests(ITestOutputHelper output) => _output = output;

    /// <summary>The seed every corpus in this class is generated from. Stated in the report.</summary>
    private const int Seed = 20871;

    private const int StandardCorpus = 200_000;
    private const int TextCorpus = 100_000;
    private const int NonstandardCorpus = 100_000;
    private const int FuzzSize = 1_000_000;

    /// <summary>How many callsigns the warm fuzz's cache is filled with before it is frozen.</summary>
    private const int WarmCacheFill = Ft8CallsignCache.DefaultCapacity;

    /// <summary>
    /// All four categories criterion 2 names, in one measurement, with the fourth category's three
    /// legs reported separately.
    /// </summary>
    [Fact]
    public void AllFourCategoriesOfCriterionTwoRoundTrip()
    {
        var clock = Stopwatch.StartNew();

        var standard = StandardCategory();
        var freeText = FreeTextCategory();
        var telemetry = TelemetryCategory();
        var nonstandard = NonstandardCategory();

        clock.Stop();

        _output.WriteLine($"seed : {Seed}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("CRITERION 2 — the four named categories");
        _output.WriteLine($"{"category",-26}{"corpus",-12}{"round-tripped",-16}failures");
        _output.WriteLine($"{"standard",-26}{StandardCorpus,-12}{standard.Passed,-16}{standard.Failed}");
        _output.WriteLine($"{"free text",-26}{TextCorpus,-12}{freeText.Passed,-16}{freeText.Failed}");
        _output.WriteLine($"{"telemetry",-26}{TextCorpus,-12}{telemetry.Passed,-16}{telemetry.Failed}");
        _output.WriteLine(
            $"{"non-standard callsign",-26}{NonstandardCorpus,-12}{nonstandard.FullCall,-16}{nonstandard.Failed}");
        _output.WriteLine(string.Empty);
        _output.WriteLine(
            $"of the standard corpus, {standard.PrefixCollisions} came back as a different call: upstream's two "
            + "prefix work-arounds collide with calls spelled the way each work-around spells its own "
            + "compressed form. Upstream's wire format, not this port's, and counted apart rather than "
            + "as a pass.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("the fourth category's THREE LEGS, which are not summed:");
        _output.WriteLine($"    full call round-tripped, no cache needed        : {nonstandard.FullCall}");
        _output.WriteLine($"    hashed and resolved through a warm cache        : {nonstandard.HashedResolved}");
        _output.WriteLine($"    refused by a cold cache with no text written    : {nonstandard.ColdRefused}");
        _output.WriteLine($"    of those, whose own two calls share a 12-bit hash : {nonstandard.OwnCollision}");
        _output.WriteLine($"    skipped, too long for the 58-bit field          : {nonstandard.Skipped}");
        _output.WriteLine($"    FAILURES on any leg                             : {nonstandard.Failed}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"total wall clock : {clock.ElapsedMilliseconds} ms");

        Assert.Equal(0, standard.Failed);
        Assert.Equal(0, freeText.Failed);
        Assert.Equal(0, telemetry.Failed);
        Assert.Equal(0, nonstandard.Failed);

        Assert.Equal(StandardCorpus, standard.Passed + standard.PrefixCollisions);
        Assert.Equal(TextCorpus, freeText.Passed);
        Assert.Equal(TextCorpus, telemetry.Passed);

        // Every leg of the fourth category was actually exercised, so "three legs" is a measurement
        // rather than an intention. The cold-cache leg must match the hashed leg exactly: the same
        // bits, and the only difference is what the receiver had heard.
        Assert.True(nonstandard.FullCall > 0);
        Assert.True(nonstandard.HashedResolved > 0);
        Assert.Equal(nonstandard.HashedResolved, nonstandard.ColdRefused);
    }

    /// <summary>
    /// Criterion 3 re-taken: a million random 77-bit patterns through the dispatcher, cold and warm.
    /// </summary>
    [Fact]
    public void TheFuzzIsRetakenColdAndWarm()
    {
        // Cold: no cache at all, which is the strictest reading of cold — it has heard nothing and
        // cannot warm up part-way through the run, so the third count stays unambiguous.
        var cold = Fuzz(null, null);

        // Warm: a cache filled to its capacity before the run and therefore frozen, since a full
        // cache stores nothing further. Its contents are exactly the set below for the whole run,
        // which is what makes "a call the cache never stored" a checkable claim rather than a
        // moving target.
        var warmCache = new Ft8CallsignCache();
        var stored = new HashSet<string>(StringComparer.Ordinal);
        var filler = new Random(Seed + 5);
        while (stored.Count < WarmCacheFill)
        {
            var call = CallsignCorpus.Generate(filler, stored.Count % CallsignCorpus.ShapeCount, out _);
            if (warmCache.Save(call) == Ft8CacheStore.Stored)
            {
                stored.Add(call);
            }
        }

        Assert.Equal(warmCache.Capacity, warmCache.Count);
        var warm = Fuzz(warmCache, stored);

        // The cache did not change during the run, so the third count below is about a fixed set.
        Assert.Equal(warmCache.Capacity, warmCache.Count);

        _output.WriteLine($"corpus size : {FuzzSize}   seed: {Seed}");
        _output.WriteLine($"warm cache  : {stored.Count} callsigns, full and therefore frozen for the run");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"count",-52}{"cold",-14}warm");
        _output.WriteLine($"{"exceptions",-52}{cold.Exceptions,-14}{warm.Exceptions}");
        _output.WriteLine($"{"decodes returned for a type not built",-52}{cold.UnbuiltTypeDecodes,-14}{warm.UnbuiltTypeDecodes}");
        _output.WriteLine(
            $"{"decodes carrying a call the cache never stored",-52}{cold.UnknownCallDecodes,-14}{warm.UnknownCallDecodes}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"decoded in all",-52}{cold.Decoded,-14}{warm.Decoded}");
        _output.WriteLine(
            $"{"of those, carrying a call resolved from a hash",-52}{cold.ResolvedFromHash,-14}{warm.ResolvedFromHash}");
        _output.WriteLine($"{"refused",-52}{FuzzSize - cold.Decoded,-14}{FuzzSize - warm.Decoded}");

        // Cold: all three zero, which is unit 207's result unchanged.
        Assert.Equal(0, cold.Exceptions);
        Assert.Equal(0, cold.UnbuiltTypeDecodes);
        Assert.Equal(0, cold.UnknownCallDecodes);
        Assert.Equal(0, cold.ResolvedFromHash);

        // Warm: the first two zero, and the third zero as well — every call that came out of a hash
        // was one the cache was holding. The resolved calls are counted separately as the correct
        // outcome they are, and there are some, which is what shows the warm run is a different run.
        Assert.Equal(0, warm.Exceptions);
        Assert.Equal(0, warm.UnbuiltTypeDecodes);
        Assert.Equal(0, warm.UnknownCallDecodes);
        Assert.True(warm.ResolvedFromHash > 0, "the warm run resolved nothing, so it measured nothing the cold run did not.");
        Assert.True(warm.Decoded > cold.Decoded);
    }

    /// <summary>
    /// The end-to-end path, extended by the non-standard-callsign type: text, packed, wrapped in the
    /// container with its checksum, encoded, every parity check verified, read back and unpacked to
    /// the same text.
    /// </summary>
    [Fact]
    public void EveryBuiltTypeMakesTheWholeRoundTripThroughTheEncoder()
    {
        var random = new Random(Seed + 9);
        var carried = 0;
        var nonstandardCarried = 0;

        var message = new byte[Ft8Payload.MessageBytes];
        var payload = new byte[Ft8Payload.PayloadBytes];
        var readBack = new byte[Ft8Payload.MessageBytes];
        var codeword = new byte[LdpcEncoder.CodewordBytes];

        for (var i = 0; i < 2000; i++)
        {
            string expected;
            Ft8CallsignCache receiver;

            if (i % 2 == 0)
            {
                // The standard message, as unit 207 carried it.
                var callDe = StandardCallsign(random);
                var extra = GridOrReport(random, i);

                // Upstream's prefix work-arounds make a handful of calls come back as a different
                // call. Measured where it belongs, in the corpus above; not this test's subject.
                if (IsPrefixWorkaround(callDe))
                {
                    continue;
                }

                Assert.Equal(Ft8PackResult.Ok, Ft8StandardMessage.TryPack("CQ", callDe, extra, null, message));
                expected = string.IsNullOrEmpty(extra) ? $"CQ {callDe}" : $"CQ {callDe} {extra}";
                receiver = new Ft8CallsignCache(4);
            }
            else
            {
                // The type this unit added, through a warm cache. The addressed station is named by
                // twelve bits and nothing else, and the receiver has heard it.
                var hashed = StandardCallsign(random);
                var inFull = NonstandardCallsign(random);
                var extra = (i % 6) switch { 1 => "RRR", 3 => "RR73", _ => "73" };

                var transmitter = new Ft8CallsignCache(4);
                if (Ft8NonstandardMessage.TryPack(hashed, inFull, extra, transmitter, message)
                    != Ft8PackResult.Ok)
                {
                    continue;
                }

                receiver = new Ft8CallsignCache(4);
                receiver.Save(hashed);
                expected = $"<{hashed}> {inFull} {extra}";
                nonstandardCarried++;
            }

            Assert.Equal(0, message[9] & 0x07);

            Ft8Payload.Create(message, payload);
            LdpcEncoder.Encode(payload, codeword);

            var bits = LdpcCheck.UnpackMsbFirst(codeword, Ft8Tables.LdpcN);
            var failing = LdpcCheck.FailingCount(
                LdpcCheck.SyndromeFromNm(bits, Ft8Tables.LdpcNm, Ft8Tables.LdpcNumRows));
            Assert.Equal(0, failing);

            Assert.True(Ft8Payload.TryRead(payload, readBack));

            var result = Ft8MessageDecoder.Decode(readBack, receiver);
            Assert.True(result.Decoded, $"[{expected}] did not come back.");
            Assert.Equal(expected, result.Text);
            carried++;
        }

        _output.WriteLine(
            $"messages carried whole through pack, CRC, encode, all 83 parity checks, read and unpack : {carried}");
        _output.WriteLine($"    of those, non-standard-callsign messages through a warm cache        : {nonstandardCarried}");

        Assert.True(carried > 0);
        Assert.True(nonstandardCarried > 0);
    }

    private (int Passed, int Failed, int PrefixCollisions) StandardCategory()
    {
        var random = new Random(Seed);
        var passed = 0;
        var failed = 0;
        var prefixCollisions = 0;
        var message = new byte[Ft8StandardMessage.MessageBytes];

        for (var i = 0; i < StandardCorpus; i++)
        {
            var callTo = i % 4 == 0 ? "CQ" : StandardCallsign(random);
            var callDe = StandardCallsign(random);
            var extra = GridOrReport(random, i);

            if (Ft8StandardMessage.TryPack(callTo, callDe, extra, null, message) != Ft8PackResult.Ok)
            {
                failed++;
                continue;
            }

            var result = Ft8MessageDecoder.Decode(message, null);
            var expected = string.IsNullOrEmpty(extra) ? $"{callTo} {callDe}" : $"{callTo} {callDe} {extra}";
            if (result.Decoded && result.Text == expected)
            {
                passed++;
                continue;
            }

            // Upstream's two prefix work-arounds collide with calls spelled the way each work-around
            // spells its own compressed form, so those calls come back as the call the work-around is
            // for. Known item 15 of this unit's instruction, unit 207's measurement, and upstream's
            // wire format rather than a defect in this port. Counted apart, never as a pass.
            if (result.Decoded && (IsPrefixWorkaround(callTo) || IsPrefixWorkaround(callDe)))
            {
                prefixCollisions++;
                continue;
            }

            failed++;
        }

        return (passed, failed, prefixCollisions);
    }

    /// <summary>
    /// Whether a callsign is spelled the way one of upstream's prefix work-arounds spells its own
    /// compressed form, and therefore packs to the same integer as the call that work-around is for.
    /// </summary>
    private static bool IsPrefixWorkaround(string callsign) =>
        callsign.StartsWith("3D0", StringComparison.Ordinal)
        || (callsign.Length >= 2 && callsign[0] == 'Q' && Ft8Text.IsLetter(callsign[1]));

    private (int Passed, int Failed) FreeTextCategory()
    {
        var random = new Random(Seed + 1);
        var passed = 0;
        var failed = 0;
        var message = new byte[Ft8Payload.MessageBytes];

        for (var i = 0; i < TextCorpus; i++)
        {
            var text = FreeText(random);
            if (Ft8FreeText.TryPackText(text, message) != Ft8PackResult.Ok)
            {
                failed++;
                continue;
            }

            var result = Ft8MessageDecoder.Decode(message, null);

            // Upstream pads to the full width with spaces and trims them off again, so the two are
            // not distinguishable. Unit 207 recorded that; the comparison is against the trimmed
            // text for the same reason it was then.
            if (result.Decoded && result.Text == text.Trim())
            {
                passed++;
            }
            else
            {
                failed++;
            }
        }

        return (passed, failed);
    }

    private (int Passed, int Failed) TelemetryCategory()
    {
        var random = new Random(Seed + 2);
        var passed = 0;
        var failed = 0;
        var message = new byte[Ft8Payload.MessageBytes];
        var body = new byte[9];
        var readBack = new byte[9];

        for (var i = 0; i < TextCorpus; i++)
        {
            random.NextBytes(body);

            // The body is 71 bits, so the top bit of the first byte is not part of it.
            body[0] &= 0x7F;

            if (i == 0)
            {
                Array.Clear(body);
            }
            else if (i == 1)
            {
                Array.Fill(body, (byte)0xFF);
                body[0] &= 0x7F;
            }

            if (Ft8FreeText.TryPackTelemetry(body, message) != Ft8PackResult.Ok)
            {
                failed++;
                continue;
            }

            var result = Ft8MessageDecoder.Decode(message, null);
            Ft8FreeText.UnpackTelemetry(message, readBack);

            if (result.Decoded && readBack.AsSpan().SequenceEqual(body))
            {
                passed++;
            }
            else
            {
                failed++;
            }
        }

        return (passed, failed);
    }

    private (int FullCall, int HashedResolved, int ColdRefused, int OwnCollision, int Skipped, int Failed)
        NonstandardCategory()
    {
        var random = new Random(Seed + 3);
        var fullCall = 0;
        var hashedResolved = 0;
        var coldRefused = 0;
        var ownCollision = 0;
        var skipped = 0;
        var failed = 0;

        var message = new byte[Ft8NonstandardMessage.MessageBytes];

        for (var i = 0; i < NonstandardCorpus; i++)
        {
            var inFull = NonstandardCallsign(random);
            var hashed = StandardCallsign(random);
            var extra = (i % 4) switch { 0 => string.Empty, 1 => "RRR", 2 => "RR73", _ => "73" };

            if (inFull.Length > Ft8NonstandardMessage.CallLength)
            {
                skipped++;
                continue;
            }

            // LEG ONE — the full call. A general call names nobody, so nothing is hashed and no
            // cache is needed at either end.
            if (Ft8NonstandardMessage.TryPack("CQ", inFull, string.Empty, null, message) != Ft8PackResult.Ok)
            {
                failed++;
                continue;
            }

            var plain = Ft8MessageDecoder.Decode(message, null);
            if (!plain.Decoded || plain.Text != $"CQ {inFull}")
            {
                failed++;
                continue;
            }

            fullCall++;

            // LEG TWO — the hashed leg, which is the one that matters. A second, different message
            // names a station by twelve bits alone, and a receiver that has heard that station
            // reads it back as that station.
            var transmitter = new Ft8CallsignCache(4);
            if (Ft8NonstandardMessage.TryPack(hashed, inFull, extra, transmitter, message) != Ft8PackResult.Ok)
            {
                failed++;
                continue;
            }

            var warm = new Ft8CallsignCache(4);
            warm.Save(hashed);
            var resolved = Ft8MessageDecoder.Decode(message, warm);

            // The two calls in this message may themselves share a 12-bit hash. That case is
            // counted, because it is the case upstream's ordering gets wrong: it stores this
            // message's own spelled-out call before it resolves the hashed one, so its lookup
            // answers with the call the message is already carrying. The lookup here happens first,
            // against what the receiver knew before the message arrived, so the case behaves like
            // every other and is counted only as an observation.
            Ft8CallsignHash.TryCompute(hashed, out _, out var hashedTwelve, out _);
            Ft8CallsignHash.TryCompute(inFull, out _, out var inFullTwelve, out _);
            if (hashedTwelve == inFullTwelve && hashed != inFull)
            {
                ownCollision++;
            }

            var expected = extra.Length == 0 ? $"<{hashed}> {inFull}" : $"<{hashed}> {inFull} {extra}";
            if (!resolved.Decoded || resolved.Text != expected)
            {
                failed++;
                continue;
            }

            hashedResolved++;

            // LEG THREE — the cold cache. The very same bits, a receiver that has never heard the
            // addressed station, and no answer at all.
            var cold = new Ft8CallsignCache(4);
            var refused = Ft8MessageDecoder.Decode(message, cold);
            if (refused.Decoded || refused.Text.Length != 0 || refused.Status != Ft8DecodeStatus.UnresolvedCallsign)
            {
                failed++;
                continue;
            }

            coldRefused++;
        }

        return (fullCall, hashedResolved, coldRefused, ownCollision, skipped, failed);
    }

    private FuzzResult Fuzz(Ft8CallsignCache? cache, HashSet<string>? stored)
    {
        var random = new Random(Seed + 4);
        var message = new byte[Ft8MessageDecoder.MessageBytes];
        var result = default(FuzzResult);

        for (var i = 0; i < FuzzSize; i++)
        {
            random.NextBytes(message);

            Ft8DecodeResult decoded;
            try
            {
                decoded = Ft8MessageDecoder.Decode(message, cache);
            }
            catch (Exception ex)
            {
                result.Exceptions++;
                _output.WriteLine($"pattern {i} threw {ex.GetType().Name}: {ex.Message}");
                continue;
            }

            if (!decoded.Decoded)
            {
                Assert.Equal(string.Empty, decoded.Text);
                continue;
            }

            result.Decoded++;

            if (!Ft8MessageTypes.IsSupported(decoded.Type))
            {
                result.UnbuiltTypeDecodes++;
                continue;
            }

            // A call that came out of a hash rather than out of the bits is the one this counts.
            // It arrives in angle brackets, which is upstream's own mark for exactly that.
            foreach (var field in new[] { decoded.Fields.CallTo, decoded.Fields.CallDe })
            {
                if (field is null || field.Length < 3 || field[0] != '<' || field[^1] != '>')
                {
                    continue;
                }

                result.ResolvedFromHash++;
                var inner = field[1..^1];
                if (stored is null || !stored.Contains(inner))
                {
                    result.UnknownCallDecodes++;
                }
            }
        }

        return result;
    }

    private struct FuzzResult
    {
        public int Exceptions;
        public int UnbuiltTypeDecodes;
        public int UnknownCallDecodes;
        public int Decoded;
        public int ResolvedFromHash;
    }

    /// <summary>A callsign of one of the four shapes the 28-bit field can pack as a basecall.</summary>
    private static string StandardCallsign(Random random) =>
        CallsignCorpus.Generate(random, random.Next(CallsignCorpus.StandardShapeCount), out _);

    /// <summary>A callsign of one of the six shapes that need the hash.</summary>
    private static string NonstandardCallsign(Random random) =>
        CallsignCorpus.Generate(
            random,
            CallsignCorpus.StandardShapeCount
            + random.Next(CallsignCorpus.ShapeCount - CallsignCorpus.StandardShapeCount),
            out _);

    /// <summary>A grid square, a signal report, one of the three tokens, or nothing.</summary>
    private static string GridOrReport(Random random, int i) => (i % 5) switch
    {
        0 => string.Empty,
        1 => $"{(char)('A' + random.Next(18))}{(char)('A' + random.Next(18))}{random.Next(10)}{random.Next(10)}",
        2 => "RRR",
        3 => "RR73",
        _ => (random.Next(2) == 0 ? "+" : "-") + random.Next(1, 31).ToString("00"),
    };

    /// <summary>Up to thirteen characters of the free-text alphabet.</summary>
    private static string FreeText(Random random)
    {
        const string Alphabet = " 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ+-./?";
        var length = random.Next(1, 14);
        var text = new char[length];
        for (var i = 0; i < length; i++)
        {
            text[i] = Alphabet[random.Next(Alphabet.Length)];
        }

        return new string(text);
    }
}
