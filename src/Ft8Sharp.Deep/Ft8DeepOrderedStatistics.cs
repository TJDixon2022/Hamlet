using System;
using System.Numerics;
using Ft8Sharp.Dsp;
using Ft8Sharp.Ldpc;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>Ordered statistics decoding of the FT8 (174, 91) LDPC code: re-order the bits by reliability,
/// re-encode from the most reliable ones, and search low-weight patterns among them.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>THE SOURCE, CITED AT THE POINT OF USE.</b> M. P. C. Fossorier and S. Lin, <i>Soft-decision
/// decoding of linear block codes based on ordered statistics</i>, IEEE Transactions on Information
/// Theory, volume 41 number 5, September 1995, pages 1379-1396. The protocol's own description is
/// S. Franke K9AN, B. Somerville G4WJS and J. Taylor K1JT, <i>The FT4 and FT8 Communication
/// Protocols</i>, QEX, July/August 2020. <b>Nothing here came from WSJT-X's source or from
/// <c>ft4_ft8_public/</c>, neither of which was read.</b> <c>ft8_lib</c>, which
/// <c>Ft8Sharp</c> is a port of, has no ordered statistics decoder at all. See
/// <c>src/Ft8Sharp.Deep/porting-notes.md</c>.
/// </para>
/// <para>
/// <b>Why it can reach what belief propagation cannot.</b> At -21 dB the hard decisions carry about
/// 31 errors in 174 against a code whose iterative recovery reaches zero at about 17, so belief
/// propagation gives up. But unit 246's task 1 measured, over one whole 51-trial block, that the
/// closest candidate's errors are not spread evenly: the median trial has about six of them inside
/// the 91 most reliable positions and the rest below. Re-encoding from the reliable end throws the
/// low ones away for free, and the search only has to cover what is left.
/// </para>
/// <para>
/// <b>WHAT THIS TYPE DOES NOT DO, AND MUST NOT.</b> It does not decide that a message is real. It
/// computes no checksum, applies no threshold and returns no confidence. It hands back the codeword
/// with the smallest soft distance it found, and whether that is a message is settled by the port's
/// own parity gate and CRC-14 gate in <c>Ft8CodewordDecoder</c> and nowhere else - <c>CLAUDE.md</c>
/// §0.0.
/// </para>
/// <para>
/// <b>It never throws on a signal.</b> It is called once per candidate that belief propagation
/// refused, which at -21 dB is most of up to 140 candidates a slot, nearly all of them noise. Ratios
/// that are all equal, all zero, infinite or not a number all produce an answer rather than an
/// exception; only a wrong-sized span or an out-of-range order is refused, and those are caller
/// mistakes rather than bad signals.
/// </para>
/// <para>
/// <b>One instance is not thread-safe.</b> It holds the scratch buffers a slot's worth of decoding
/// needs so that a per-candidate call allocates nothing; build one per decoder, which is what
/// <see cref="Ft8DeepSlotDecoder"/> does.
/// </para>
/// </remarks>
public sealed class Ft8DeepOrderedStatistics
{
    /// <summary>The codeword length, 174. The port's own <c>LdpcDecoder.CodewordBits</c>.</summary>
    public const int CodewordBits = LdpcDecoder.CodewordBits;

    /// <summary>
    /// The dimension, 91: the message and its checksum, and therefore the size of the most reliable
    /// basis. The port's own <c>LdpcEncoder.PayloadBits</c>.
    /// </summary>
    public const int BasisBits = LdpcEncoder.PayloadBits;

    /// <summary>64-bit words needed to hold a codeword: 3, holding 174 used bits and 18 spare.</summary>
    private const int Words = 3;

    /// <summary>
    /// <b>The generator, read off the port's own encoder rather than unpacked from a table.</b>
    /// </summary>
    /// <remarks>
    /// The code is systematic in its first 91 bits, so row <c>i</c> of the 91 by 174 generator is
    /// exactly what <c>LdpcEncoder.Encode</c> returns for the payload with bit <c>i</c> set and every
    /// other bit clear. <c>Ft8Tables.LdpcGenerator</c> is not used: its 83 rows are the parity checks
    /// in upstream's own packing, and a mistake in re-deriving G from them would poison every decode
    /// while looking exactly like an algorithm that does not work.
    /// <c>Ft8Unit246CeilingTests.TheGeneratorReadOffTheEncoderReproducesEveryEncodeItIsCheckedAgainst</c>
    /// checks this construction against the port for several hundred random payloads.
    /// </remarks>
    private static readonly ulong[] Generator = BuildGenerator();

    private readonly ulong[] _rows = new ulong[BasisBits * Words];
    private readonly int[] _pivots = new int[BasisBits];
    private readonly int[] _order = new int[CodewordBits];
    private readonly float[] _magnitude = new float[CodewordBits];
    private readonly byte[] _hard = new byte[CodewordBits];
    private readonly ulong[] _hardWords = new ulong[Words];
    private readonly ulong[] _current = new ulong[Words];
    private readonly ulong[] _best = new ulong[Words];

    private float _bestDistance;
    private long _reencodings;

    /// <summary>
    /// <b>The most reliable basis of the last decode: the 91 codeword positions the answer was
    /// re-encoded from, in reliability order.</b>
    /// </summary>
    /// <remarks>
    /// These are the first 91 positions in <c>|ratio|</c> order whose generator columns are
    /// independent, which is not always the first 91 positions outright - a dependent column is
    /// stepped over and the basis reaches one place further down. Exposed so a test can check that
    /// there are 91 of them and that they are distinct, which is the one property the elimination
    /// must have for every input it is ever handed.
    /// </remarks>
    public ReadOnlySpan<int> MostReliableBasis => _pivots;

    /// <summary>
    /// <b>Finds the codeword of smallest soft distance from a set of ratios, at a given order.</b>
    /// </summary>
    /// <param name="ratios">
    /// <see cref="CodewordBits"/> log-likelihood ratios in the port's convention: <b>positive means
    /// the bit is more likely 1</b>.
    /// </param>
    /// <param name="order">
    /// Fossorier and Lin's λ: how many of the 91 basis positions may be flipped, 0 to
    /// <see cref="Ft8DeepOsdSettings.MaximumOrder"/>. <b>Order λ can only reach the transmitted
    /// codeword when the basis carries at most λ errors</b>, which is what makes the basis error
    /// distribution, and not the total error count, the thing that decides whether this helps.
    /// </param>
    /// <param name="codeword">
    /// <see cref="CodewordBits"/> bytes, one per bit, written in full. <b>Always a codeword of the
    /// code</b>, because it is built as a linear combination of the rows of a row-reduced generator.
    /// </param>
    /// <returns>The soft distance reached and what it cost in re-encodings.</returns>
    /// <exception cref="ArgumentException">Either span is the wrong length.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The order is negative or above the bound.</exception>
    /// <remarks>
    /// <para>
    /// The five steps are Fossorier and Lin's, in their order:
    /// </para>
    /// <list type="number">
    /// <item>order the positions by <c>|ratio|</c>, most reliable first;</item>
    /// <item>find the most reliable basis - Gaussian elimination over GF(2) on the generator with its
    /// columns visited in that order, taking the first 91 that are independent, giving a generator
    /// systematic on those 91 positions;</item>
    /// <item>order 0: take the hard decision on those 91 positions and re-encode;</item>
    /// <item>order λ: flip every subset of the basis of size 1 to λ and re-encode each;</item>
    /// <item>rank by soft distance - the sum of <c>|ratio|</c> over the positions where the re-encoded
    /// codeword disagrees with the hard decision - and keep the smallest.</item>
    /// </list>
    /// <para>
    /// <b>Ties keep the first found</b>, and the enumeration is in a fixed order, so the same ratios
    /// give the same codeword in every process. A decoder whose answer depended on iteration order
    /// would not be measurable on a ladder.
    /// </para>
    /// </remarks>
    public Ft8DeepOsdResult Decode(ReadOnlySpan<float> ratios, int order, Span<byte> codeword)
    {
        if (ratios.Length != CodewordBits)
        {
            throw new ArgumentException(
                $"Ordered statistics decoding takes {CodewordBits} ratios, one per codeword bit, and "
                + $"a span of {ratios.Length} was given.",
                nameof(ratios));
        }

        if (codeword.Length != CodewordBits)
        {
            throw new ArgumentException(
                $"The codeword buffer is {CodewordBits} bytes, one per bit, and a span of "
                + $"{codeword.Length} was given.",
                nameof(codeword));
        }

        if (order < 0 || order > Ft8DeepOsdSettings.MaximumOrder)
        {
            throw new ArgumentOutOfRangeException(
                nameof(order),
                order,
                $"An order is 0 to {Ft8DeepOsdSettings.MaximumOrder}. Clamping instead of refusing "
                + "would report a measurement of an order nobody asked for.");
        }

        // STEP 1 -- reliability. The hard decision is the port's own rule, l > 0, so a ratio of
        // exactly zero reads as a zero rather than as a one. A NaN magnitude sorts to the bottom
        // rather than throwing, which is what "never throws on a signal" costs.
        for (var i = 0; i < CodewordBits; i++)
        {
            var ratio = ratios[i];
            _hard[i] = ratio > 0.0f ? (byte)1 : (byte)0;
            var magnitude = Math.Abs(ratio);
            _magnitude[i] = float.IsNaN(magnitude) ? 0.0f : magnitude;
            _order[i] = i;
        }

        SortByReliability();
        PackHard();

        // STEP 2 -- the most reliable basis.
        Eliminate();

        // STEP 3 -- order 0. The basis bits are the hard decisions at the pivot positions, and
        // re-encoding is the exclusive-or of the rows those bits select. Working in the difference
        // from the hard decision rather than in the codeword itself makes step 5 a popcount walk.
        Array.Clear(_current);
        for (var r = 0; r < BasisBits; r++)
        {
            if (_hard[_pivots[r]] != 0)
            {
                XorRow(_current, r);
            }
        }

        for (var w = 0; w < Words; w++)
        {
            _current[w] ^= _hardWords[w];
        }

        _bestDistance = SoftDistance(_current);
        _current.CopyTo(_best, 0);
        _reencodings = 1;

        // STEP 4 -- order λ. Flipping basis position r toggles its bit, which adds that row.
        if (order > 0)
        {
            Search(1, 0, order);
        }

        // The answer, back out of difference space: best = codeword XOR hard, so codeword = best
        // XOR hard.
        for (var i = 0; i < CodewordBits; i++)
        {
            var bit = (_best[i >> 6] >> (i & 63)) & 1UL;
            codeword[i] = (byte)(bit ^ (ulong)_hard[i]);
        }

        return new Ft8DeepOsdResult(_bestDistance, _reencodings);
    }

    /// <summary>
    /// <b>Turns a recovered codeword into ratios the port will read, so that the PORT decides whether
    /// it is a message.</b>
    /// </summary>
    /// <param name="codeword"><see cref="CodewordBits"/> bytes, one per bit.</param>
    /// <param name="ratios">
    /// <see cref="CodewordBits"/> ratios, written in full: plus or minus one in the port's convention,
    /// then put on upstream's own scale by <c>Ft8SoftSymbols.Normalise</c>, which is called rather
    /// than re-implemented.
    /// </param>
    /// <exception cref="ArgumentException">Either span is the wrong length.</exception>
    /// <remarks>
    /// <para>
    /// <b>THIS IS THE ONLY WAY ANYTHING THIS LIBRARY RECOVERS BECOMES A MESSAGE, AND IT IS §0.0.</b>
    /// The ratios go to <c>Ft8CodewordDecoder.Decode</c>; the port's belief propagation converges on
    /// an already-valid codeword in one iteration, and then the port's <b>parity gate</b> and its
    /// <b>CRC-14 gate</b> are applied exactly as they always are. A codeword this library got wrong
    /// carries a checksum that does not match its own payload, and the port refuses it in the port's
    /// own words. Nothing in <c>Ft8Sharp.Deep</c> decides that a message is real.
    /// </para>
    /// <para>
    /// <b>It is a re-derivation, not a second check.</b> There is still exactly one CRC-14 comparison
    /// in these two libraries and it is the port's.
    /// </para>
    /// <para>
    /// This is route A of <c>docs/unit245-deep-seam.md</c> §4, which unit 245 measured working before
    /// a line of OSD existed: a codeword handed back this way came through as a real
    /// <c>Ft8CodewordResult</c> carrying its text, and one with forty bits flipped came back refused.
    /// </para>
    /// </remarks>
    public static void Saturate(ReadOnlySpan<byte> codeword, Span<float> ratios)
    {
        if (codeword.Length != CodewordBits)
        {
            throw new ArgumentException(
                $"A codeword is {CodewordBits} bytes, one per bit, and a span of {codeword.Length} "
                + "was given.",
                nameof(codeword));
        }

        if (ratios.Length != CodewordBits)
        {
            throw new ArgumentException(
                $"The ratio buffer is {CodewordBits} long and a span of {ratios.Length} was given.",
                nameof(ratios));
        }

        for (var i = 0; i < CodewordBits; i++)
        {
            // Positive means the bit is more likely one, which is the port's convention throughout.
            ratios[i] = codeword[i] != 0 ? 1.0f : -1.0f;
        }

        Ft8SoftSymbols.Normalise(ratios);
    }

    /// <summary>
    /// STEP 4's enumeration: every subset of the basis of size <paramref name="depth"/> to
    /// <c>order</c>, in a fixed order, with each pattern's soft distance ranked as it is produced.
    /// </summary>
    /// <remarks>
    /// The recursion is at most <see cref="Ft8DeepOsdSettings.MaximumOrder"/> deep and toggles one
    /// row on the way in and off on the way out, so the whole search allocates nothing and each
    /// re-encoding costs three exclusive-ors rather than a matrix multiply.
    /// </remarks>
    private void Search(int depth, int start, int order)
    {
        for (var r = start; r < BasisBits; r++)
        {
            XorRow(_current, r);
            _reencodings++;

            var distance = SoftDistance(_current);
            if (distance < _bestDistance)
            {
                _bestDistance = distance;
                _current.CopyTo(_best, 0);
            }

            if (depth < order)
            {
                Search(depth + 1, r + 1, order);
            }

            XorRow(_current, r);
        }
    }

    /// <summary>
    /// STEP 5's metric: the sum of <c>|ratio|</c> over the positions where the re-encoded codeword
    /// disagrees with the hard decision.
    /// </summary>
    /// <remarks>
    /// <paramref name="difference"/> already is that disagreement, so this is a walk over its set
    /// bits. A typical pattern has thirty to sixty of 174 set, so the walk is several times cheaper
    /// than reading all 174 positions - which is what makes order 2 affordable inside a slot.
    /// </remarks>
    private float SoftDistance(ulong[] difference)
    {
        var sum = 0.0f;
        for (var w = 0; w < Words; w++)
        {
            var bits = difference[w];
            while (bits != 0)
            {
                sum += _magnitude[(w << 6) + BitOperations.TrailingZeroCount(bits)];
                bits &= bits - 1;
            }
        }

        return sum;
    }

    /// <summary>
    /// STEP 1's ordering: <c>|ratio|</c> descending, ties broken on position ascending.
    /// </summary>
    /// <remarks>
    /// An insertion sort over 174 items, written out rather than gone through
    /// <c>Array.Sort</c> with a comparison, because ties must break the same way in every process and
    /// an introsort's order among equal keys is an implementation detail. Ratios that are all equal
    /// are exactly the degenerate case this has to be right about.
    /// </remarks>
    private void SortByReliability()
    {
        for (var i = 1; i < CodewordBits; i++)
        {
            var index = _order[i];
            var key = _magnitude[index];
            var j = i - 1;

            while (j >= 0 && _magnitude[_order[j]] < key)
            {
                _order[j + 1] = _order[j];
                j--;
            }

            _order[j + 1] = index;
        }
    }

    /// <summary>
    /// STEP 2: Gaussian elimination over GF(2) on the generator, visiting columns in reliability
    /// order and keeping the first 91 that are independent.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>This is where a dependent column is stepped over.</b> The most reliable 91 positions are
    /// not always a basis - the code has 174 columns of rank 91, so some sets of 91 are dependent -
    /// and when one is, the elimination moves on to the next most reliable column instead. That is
    /// Fossorier and Lin's own construction and it is why the basis error count measured against the
    /// leading 91 positions is a lower bound rather than the number itself.
    /// </para>
    /// <para>
    /// <b>It cannot fail to find 91.</b> The generator has rank 91 by construction, so 91 independent
    /// columns exist and this sweep visits every column. If it ever did not, the loop would leave
    /// pivots unset and every later step would read them, so the count is asserted rather than
    /// assumed.
    /// </para>
    /// </remarks>
    private void Eliminate()
    {
        Generator.CopyTo(_rows, 0);

        var rank = 0;
        for (var c = 0; c < CodewordBits && rank < BasisBits; c++)
        {
            var column = _order[c];
            var word = column >> 6;
            var mask = 1UL << (column & 63);

            var pivot = -1;
            for (var r = rank; r < BasisBits; r++)
            {
                if ((_rows[(r * Words) + word] & mask) != 0)
                {
                    pivot = r;
                    break;
                }
            }

            if (pivot < 0)
            {
                continue;
            }

            if (pivot != rank)
            {
                for (var w = 0; w < Words; w++)
                {
                    (_rows[(rank * Words) + w], _rows[(pivot * Words) + w]) =
                        (_rows[(pivot * Words) + w], _rows[(rank * Words) + w]);
                }
            }

            for (var r = 0; r < BasisBits; r++)
            {
                if (r == rank || (_rows[(r * Words) + word] & mask) == 0)
                {
                    continue;
                }

                for (var w = 0; w < Words; w++)
                {
                    _rows[(r * Words) + w] ^= _rows[(rank * Words) + w];
                }
            }

            _pivots[rank] = column;
            rank++;
        }

        if (rank != BasisBits)
        {
            // Unreachable for this code, and left loud rather than left to corrupt a decode: a
            // generator of rank below 91 would mean the table itself had changed under this library.
            throw new InvalidOperationException(
                $"The elimination found {rank} independent columns of {BasisBits}. The FT8 generator "
                + "has rank 91 by construction, so this means the code itself is not what this "
                + "library was built against.");
        }
    }

    /// <summary>Adds one row of the row-reduced generator into a pattern.</summary>
    private void XorRow(ulong[] target, int row)
    {
        var offset = row * Words;
        for (var w = 0; w < Words; w++)
        {
            target[w] ^= _rows[offset + w];
        }
    }

    /// <summary>Packs the hard decision into words, so the difference is an exclusive-or.</summary>
    private void PackHard()
    {
        Array.Clear(_hardWords);
        for (var i = 0; i < CodewordBits; i++)
        {
            if (_hard[i] != 0)
            {
                _hardWords[i >> 6] |= 1UL << (i & 63);
            }
        }
    }

    /// <summary>
    /// The 91 by 174 generator, built once by encoding the 91 unit payloads through the port.
    /// </summary>
    private static ulong[] BuildGenerator()
    {
        var rows = new ulong[BasisBits * Words];
        var payload = new byte[LdpcEncoder.PayloadBytes];
        var codeword = new byte[LdpcEncoder.CodewordBytes];

        for (var i = 0; i < BasisBits; i++)
        {
            Array.Clear(payload);
            payload[i / 8] = (byte)(0x80u >> (i % 8));
            LdpcEncoder.Encode(payload, codeword);

            var offset = i * Words;
            for (var j = 0; j < CodewordBits; j++)
            {
                if (((codeword[j / 8] >> (7 - (j % 8))) & 1) != 0)
                {
                    rows[offset + (j >> 6)] |= 1UL << (j & 63);
                }
            }
        }

        return rows;
    }
}

/// <summary>What one ordered statistics decode reached, and what it cost.</summary>
/// <remarks>
/// <b>Neither field is an acceptance.</b> A small soft distance is not evidence that a codeword is
/// the one that was sent - the port's CRC-14 gate is the only thing that says that - and this type
/// carries no flag that could be mistaken for one.
/// </remarks>
/// <param name="SoftDistance">
/// The sum of <c>|ratio|</c> over the positions where the returned codeword disagrees with the hard
/// decision. Smaller is closer to what the ratios said.
/// </param>
/// <param name="Reencodings">
/// How many codewords were formed and ranked, including order 0's. <b>Reported rather than
/// estimated</b>, because the cost of an order is one of step 2's exit criteria.
/// </param>
public readonly record struct Ft8DeepOsdResult(float SoftDistance, long Reencodings);
