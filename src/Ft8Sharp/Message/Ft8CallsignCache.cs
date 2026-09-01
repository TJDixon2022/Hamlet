using System;

namespace Ft8Sharp.Message;

/// <summary>
/// The rolling cache of callsigns this library has heard, which is what lets a message that names a
/// station only by the hash of its call be read at all.
/// </summary>
/// <remarks>
/// <para>
/// <b>Ported from the pinned clone</b>, where the table itself is not in the library: <c>ft8/message.c</c>
/// declares a two-entry function-pointer interface — one to store a call by its hash, one to look a
/// call up by it — and calls that interface without implementing it. The implementation lives in the
/// clone's own decoder application, and that is what the slot arithmetic, the probe stride, the
/// duplicate check and the ageing below are ported from. Its capacity and stride are asserted
/// against the pin by machine.
/// </para>
/// <para>
/// <b>Everything else in this message layer is a pure function. This is not, and that is why it was
/// held back.</b> A cache remembers callsigns across messages, so it has three failure modes nothing
/// else here has, and all three are answered deliberately rather than by default:
/// </para>
/// <list type="number">
///   <item><description>
///     <b>A miss refuses.</b> A hash this cache has never seen resolves to nothing at all — no
///     placeholder, no hash rendered as digits, no partially decoded message. Upstream writes a
///     literal <c>&lt;...&gt;</c> into the field on a miss and returns the message anyway; this
///     library refuses the whole message. That is <c>CLAUDE.md</c> §0.0 / HM-DEC-009 and it is the
///     rule this whole type turns on.
///   </description></item>
///   <item><description>
///     <b>A collision refuses.</b> Two different callsigns can hash to the same 22, 12 or 10-bit
///     value; a 12-bit hash has only four thousand values and there are far more callsigns than
///     that. Upstream stores both and its lookup returns whichever it reaches first — a real,
///     plausible, entirely wrong callsign, presented with no mark of doubt on it. This lookup finds
///     <em>every</em> stored call that matches at the requested width and refuses where there is
///     more than one. See <see cref="TryLookup"/>.
///   </description></item>
///   <item><description>
///     <b>It is state, so it is constructible.</b> There is no static instance of this type anywhere
///     in this library and no default one hiding behind a property. Every caller that wants a cache
///     is handed one, every test makes its own, and a test that wants a cold cache gets a cold one
///     regardless of what ran before it. A corpus result that depended on test ordering would not be
///     a measurement.
///   </description></item>
/// </list>
/// <para>
/// <b>Not thread-safe, deliberately.</b> A decoder owns its cache for the length of a decode cycle.
/// Sharing one across threads is the caller's problem to solve with a lock, and hiding a lock in
/// here would cost every single-threaded caller for a case none of them has.
/// </para>
/// </remarks>
public sealed class Ft8CallsignCache
{
    /// <summary>The number of slots the pin's own implementation uses. Asserted against it by machine.</summary>
    public const int DefaultCapacity = 256;

    /// <summary>
    /// The multiplier that turns the top ten bits of a hash into a starting slot. Upstream's, and
    /// asserted against it by machine.
    /// </summary>
    public const int ProbeStride = 23;

    /// <summary>The shortest thing this cache will accept as a callsign.</summary>
    /// <remarks>
    /// Upstream applies the same bound at each of its own call sites rather than in the table — its
    /// basecall packer refuses anything shorter, and its 58-bit unpacker checks the length before it
    /// stores. Applied once here instead of three times, which is the same behaviour in one place.
    /// </remarks>
    public const int MinimumCallsignLength = 3;

    private readonly string?[] _callsigns;
    private readonly uint[] _hashes;
    private readonly int[] _ages;

    /// <summary>A cache of the pin's own size.</summary>
    public Ft8CallsignCache()
        : this(DefaultCapacity)
    {
    }

    /// <summary>A cache of a stated size.</summary>
    /// <param name="capacity">How many callsigns it can hold at once.</param>
    /// <exception cref="ArgumentOutOfRangeException">The capacity is not positive.</exception>
    public Ft8CallsignCache(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "A cache holds at least one call.");
        }

        Capacity = capacity;
        _callsigns = new string?[capacity];
        _hashes = new uint[capacity];
        _ages = new int[capacity];
    }

    /// <summary>How many callsigns this cache can hold at once.</summary>
    public int Capacity { get; }

    /// <summary>How many callsigns it is holding now.</summary>
    public int Count { get; private set; }

    /// <summary>
    /// Hashes a callsign and remembers it, so that a later message naming it only by that hash can
    /// be read.
    /// </summary>
    /// <param name="callsign">The callsign, already trimmed and upper-cased.</param>
    /// <returns>What happened, including the two ways it can decline.</returns>
    public Ft8CacheStore Save(string callsign) => Save(callsign, out _, out _, out _);

    /// <summary>
    /// Hashes a callsign, remembers it, and hands back all three of its hashes.
    /// </summary>
    /// <param name="callsign">The callsign, already trimmed and upper-cased.</param>
    /// <param name="hash22">Its 22-bit hash, written whenever the call could be hashed at all.</param>
    /// <param name="hash12">Its 12-bit hash, written whenever the call could be hashed at all.</param>
    /// <param name="hash10">Its 10-bit hash, written whenever the call could be hashed at all.</param>
    /// <remarks>
    /// <b>The hashes are written even when the call was not stored.</b> A packer needs the hash in
    /// order to put it on the air whether or not this particular cache had room for the call, and
    /// the two questions are separate.
    /// </remarks>
    public Ft8CacheStore Save(string callsign, out uint hash22, out uint hash12, out uint hash10)
    {
        hash22 = 0;
        hash12 = 0;
        hash10 = 0;

        if (callsign is null || callsign.Length < MinimumCallsignLength)
        {
            return Ft8CacheStore.TooShort;
        }

        if (!Ft8CallsignHash.TryCompute(callsign, out hash22, out hash12, out hash10))
        {
            return Ft8CacheStore.NotHashable;
        }

        var slot = SlotFor(hash10);
        for (var probe = 0; probe < Capacity; probe++)
        {
            var at = (slot + probe) % Capacity;
            var occupant = _callsigns[at];

            if (occupant is null)
            {
                _callsigns[at] = callsign;
                _hashes[at] = hash22;
                _ages[at] = 0;
                Count++;
                return Ft8CacheStore.Stored;
            }

            if (_hashes[at] == hash22 && string.Equals(occupant, callsign, StringComparison.Ordinal))
            {
                // The same call again. Upstream resets its age here, which is what makes the cache
                // roll: a station that keeps transmitting keeps its place.
                _ages[at] = 0;
                return Ft8CacheStore.AlreadyPresent;
            }
        }

        // Upstream's loop has no bound and spins forever on a full table. Bounded here, and the
        // caller is told rather than hung. A call that could not be stored is simply one this cache
        // has not heard, which the lookup already refuses correctly.
        return Ft8CacheStore.Full;
    }

    /// <summary>
    /// Looks a callsign up by one of its three hashes.
    /// </summary>
    /// <param name="width">Which of the three hashes <paramref name="hash"/> is.</param>
    /// <param name="hash">The hash, as it came off the air.</param>
    /// <param name="callsign">
    /// The callsign, written only on <see cref="Ft8CacheLookup.Found"/>. Empty on both refusals, so
    /// a caller that ignores the answer has nothing to display rather than something that looks like
    /// one.
    /// </param>
    /// <remarks>
    /// <para>
    /// <b>Every occupied slot is examined, not just the probe chain, and this is a deliberate
    /// divergence.</b> Upstream walks from the computed slot and stops at the first empty one, which
    /// is correct while nothing has ever been removed — but its own ageing pass punches holes in the
    /// table, and a hole can hide the second half of a colliding pair behind it. Hiding the second
    /// half is exactly the case where a wrong callsign would be returned confidently, so the scan
    /// does not stop early. It costs a walk of at most <see cref="Capacity"/> slots and it makes the
    /// refusal below depend on the table's contents rather than on the order they were inserted in.
    /// </para>
    /// <para>
    /// <b>Two distinct callsigns matching one hash is a refusal, not a choice.</b> The narrow hashes
    /// are truncations of the wide one, so any two calls that collide at any width share their top
    /// ten bits and therefore their starting slot; a collision this cache is holding is a collision
    /// this cache can see. What it cannot see is a station it has never heard whose call collides
    /// with one it has — no cache can, and nothing here pretends otherwise.
    /// </para>
    /// </remarks>
    public Ft8CacheLookup TryLookup(Ft8CallsignHashWidth width, uint hash, out string callsign)
    {
        callsign = string.Empty;

        var shift = Ft8CallsignHash.ShiftFor(width);
        var bits = Ft8CallsignHash.BitsOf(width);
        if (hash >= (1u << bits))
        {
            // A hash wider than the width it claims to be is not a hash of anything.
            return Ft8CacheLookup.NotFound;
        }

        // Upstream's own slot arithmetic: whichever width is being asked for, the ten bits that
        // choose the slot are the top ten of the stored 22, which every width shares.
        var slot = SlotFor(hash >> (Ft8CallsignHash.Shift10 - shift));

        string? found = null;
        var matches = 0;

        for (var probe = 0; probe < Capacity; probe++)
        {
            var at = (slot + probe) % Capacity;
            var occupant = _callsigns[at];
            if (occupant is null)
            {
                continue;
            }

            if (((_hashes[at] & Ft8CallsignHash.Mask22) >> shift) != hash)
            {
                continue;
            }

            if (found is null)
            {
                found = occupant;
                matches = 1;
            }
            else if (!string.Equals(found, occupant, StringComparison.Ordinal))
            {
                // Two real callsigns, one hash. Either could be the station on the air and this
                // cache cannot tell which, so it says so instead of picking.
                matches++;
            }
        }

        if (found is null)
        {
            return Ft8CacheLookup.NotFound;
        }

        if (matches > 1)
        {
            return Ft8CacheLookup.Ambiguous;
        }

        callsign = found;
        return Ft8CacheLookup.Found;
    }

    /// <summary>
    /// Ages every callsign by one and forgets the ones older than <paramref name="maxAge"/>.
    /// </summary>
    /// <param name="maxAge">
    /// How many ageing passes a call may survive without being heard again. A call heard again has
    /// its age reset by <see cref="Save(string)"/>, which is what makes this cache roll rather than
    /// simply fill.
    /// </param>
    /// <remarks>
    /// <b>Upstream's own eviction, driven from outside as upstream drives it.</b> The clone calls
    /// this between decode cycles; nothing in this library calls it, because how long a station
    /// should be remembered is a decision for whatever is doing the decoding and not for the message
    /// layer. Where upstream packs the age into the unused top byte of the stored hash, this keeps it
    /// in its own array — the same behaviour without the byte-stuffing, which is a representation
    /// difference and not a behavioural one.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxAge"/> is negative.</exception>
    public void Age(int maxAge)
    {
        if (maxAge < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAge), maxAge, "An age is not negative.");
        }

        for (var at = 0; at < Capacity; at++)
        {
            if (_callsigns[at] is null)
            {
                continue;
            }

            if (_ages[at] > maxAge)
            {
                _callsigns[at] = null;
                _hashes[at] = 0;
                _ages[at] = 0;
                Count--;
            }
            else
            {
                _ages[at]++;
            }
        }
    }

    /// <summary>Forgets everything, leaving a cache indistinguishable from a new one.</summary>
    /// <remarks>
    /// Not upstream's — its table is a file-scope array with an initialiser rather than an object.
    /// It is here so that a test can take the same cache from warm to cold without constructing a
    /// second one and without depending on which construction the caller happened to make.
    /// </remarks>
    public void Clear()
    {
        Array.Clear(_callsigns);
        Array.Clear(_hashes);
        Array.Clear(_ages);
        Count = 0;
    }

    /// <summary>Whether this cache is holding a given callsign, by identity rather than by hash.</summary>
    /// <remarks>
    /// For tests and for diagnosis (§0.0.1). Nothing in the decode path uses it: a decoder only ever
    /// has a hash to go on, which is the whole difficulty.
    /// </remarks>
    public bool Contains(string callsign)
    {
        if (callsign is null)
        {
            return false;
        }

        foreach (var occupant in _callsigns)
        {
            if (string.Equals(occupant, callsign, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private int SlotFor(uint hash10) => (int)((hash10 & 0x3FFu) * ProbeStride % (uint)Capacity);
}

/// <summary>What happened when a callsign was offered to the cache.</summary>
public enum Ft8CacheStore
{
    /// <summary>It was not there and now it is.</summary>
    Stored,

    /// <summary>It was already there, and its age has been reset.</summary>
    AlreadyPresent,

    /// <summary>It holds a character the hash cannot pack, so it has no hash to be stored under.</summary>
    NotHashable,

    /// <summary>It is shorter than anything that could be a callsign.</summary>
    TooShort,

    /// <summary>
    /// The cache is full. Upstream spins forever here; this says so, and the call is simply one this
    /// cache will not be able to resolve later.
    /// </summary>
    Full,
}

/// <summary>What happened when a hash was looked up.</summary>
/// <remarks>
/// <b>Two different refusals, because they are different facts about the world</b> (§0.0.1). A miss
/// means this receiver has not heard the station name itself yet and may well hear it in the next
/// cycle. An ambiguity means two stations it <em>has</em> heard share this hash, and no further
/// listening will separate them. Both produce no text, and callers treat both the same way.
/// </remarks>
public enum Ft8CacheLookup
{
    /// <summary>Exactly one callsign in the cache has this hash, and it has been written out.</summary>
    Found,

    /// <summary>No callsign in the cache has this hash.</summary>
    NotFound,

    /// <summary>Two or more distinct callsigns in the cache have this hash, so none is returned.</summary>
    Ambiguous,
}
