using System.Numerics;
using System.Text;

namespace Hamlet.RadioEngine.Psk31;

/// <summary>
/// **One PSK31 channel: audio at one offset in, characters out.**
/// </summary>
/// <remarks>
/// <para>**IT KNOWS NOTHING ABOUT TABS, RADIOS OR PANELS** (§0.1). Samples and an
/// offset in hertz go in; text comes out as it completes. Whether those samples came
/// from a sound card, a WAV or a test is not its business, and finding other signals
/// across the passband is a later step's.</para>
/// <para>**WHAT PSK31 IS ON THE WIRE.** Binary phase-shift keying at 31.25 baud - 256
/// samples a bit at 8 kHz - carrying text in <see cref="Varicode"/>. The keying is
/// **differential**: a `0` bit is a phase reversal and a `1` bit is no change, so the
/// receiver compares each symbol with the one before and never needs absolute phase.
/// Idle is a run of `0`s, which is continuous reversals, and it carries no characters.
/// The transmitter shapes each symbol with a raised cosine so the envelope passes
/// through zero at every reversal, which is what keeps the signal 31 Hz wide.</para>
/// <para>**SO THE CHAIN IS**: mix down at the offset to a complex baseband; average
/// over one bit, which is a matched filter for that pulse; find the symbol centres from
/// where the envelope is strongest; take one sample a bit; multiply each by the
/// conjugate of the last; the sign of the real part is the bit; split on `00`; look the
/// pieces up.</para>
/// <para>**AND TWO THINGS A REAL SIGNAL NEEDS.** **AFC**, because a carrier drifts and
/// nobody tunes to the exact hertz - squaring the differential product removes the
/// modulation and leaves twice the frequency error. And **a squelch**, because this
/// same machinery run on noise produces characters: the author's unsquelched reference
/// decoder emitted **112 of them from thirty seconds of Gaussian noise**. For a text
/// mode the squelch **is** §0.0 - a character Hamlet was not sure of is not shown, and
/// there is no dimmed maybe and no question mark in its place.</para>
/// </remarks>
public sealed class Psk31Demodulator
{
    /// <summary>Symbols a second, which is the mode's whole name.</summary>
    public const double Baud = 31.25;

    /// <summary>**The squelch rule, in one sentence.**</summary>
    /// <remarks>
    /// **IT MEASURES HOW BPSK-SHAPED THE SIGNAL IS, NOT HOW LOUD IT IS.** Loudness
    /// cannot tell a station from a burst of noise at the same level, and PSK31 is
    /// worked at levels where the noise is often louder in the passband than the signal
    /// is. What separates them is *shape*: on real keying the differential product lands
    /// on the real axis, plus or minus, because that is what BPSK is; on noise its phase
    /// is uniform. So the measure is how much of the product's magnitude is real,
    /// averaged over recent symbols. **Uniform phase gives 2/pi, which is 0.637; clean
    /// keying gives 1.0.**
    /// </remarks>
    public const string SquelchRule =
        "the mean of |Re(d)| over the mean of |d|, where d is each symbol times the "
        + "conjugate of the one before, taken over a rolling window: 1.0 on clean "
        + "keying and 0.637 on uniform noise phase. Characters are emitted only while "
        + "it is at or above " + nameof(SquelchQuality) + ".";

    /// <summary>**The squelch number.**</summary>
    /// <remarks>
    /// <para>**0.90, AND HERE IS WHY THAT NUMBER.** The two ends of the measure are
    /// 0.637 for noise and 1.0 for clean keying, so the whole usable range is a third of
    /// a unit wide. 0.90 sits about four fifths of the way up it: far enough above noise
    /// that a burst cannot hold the gate open for the eight or ten bits a character
    /// takes, and far enough below 1.0 that a real signal with noise on it stays
    /// through.</para>
    /// <para>**IT IS CHOSEN FROM THE ARITHMETIC AND CONFIRMED BY THE FIXTURES**, not
    /// tuned until the tests passed. The noise-only fixture must produce nothing and
    /// the -3 dB fixture must still read - if a number could not do both, the answer
    /// would be to say so rather than to split the difference and ship a decoder that
    /// invents text on a quiet band.</para>
    /// </remarks>
    public const double SquelchQuality = 0.90;

    /// <summary>How far the AFC may pull the local oscillator, in hertz.</summary>
    /// <remarks>
    /// **WIDER THAN ANY DRIFT AND NARROWER THAN THE NEXT SIGNAL.** PSK31 stations sit a
    /// few tens of hertz apart at worst, and a loop allowed to wander further would walk
    /// off one station and onto another, reporting the second one's text under the
    /// first one's heading.
    /// </remarks>
    public const double AfcRangeHz = 20;

    /// <summary>How fast the timing profile forgets, per sample.</summary>
    /// <remarks>
    /// **ABOUT EIGHT SYMBOLS OF MEMORY** at 8 kHz, which is a fifth of a second. Long
    /// enough that one noisy bit cannot move the clock and short enough to follow two
    /// sound cards running at slightly different rates.
    /// </remarks>
    public const double EnergyDecay = 0.9995;

    /// <summary>How much of each symbol's frequency error the AFC takes.</summary>
    public const double AfcGain = 0.02;

    /// <summary>How many symbols the quality measure averages over.</summary>
    /// <remarks>
    /// **LONGER THAN A CHARACTER AND SHORTER THAN A WORD.** A character is eight to ten
    /// bits, so a window of thirty-two cannot be held open by one lucky character and
    /// still closes within a second of a station stopping.
    /// </remarks>
    public const int QualityWindow = 32;

    private readonly int _sampleRate;
    private readonly double _offsetHz;
    private readonly int _samplesPerSymbol;
    private readonly int _bins;
    private readonly int _samplesPerBin;

    private readonly Complex[] _period;
    private readonly double[] _binEnergy;

    private readonly StringBuilder _piece = new();
    private readonly StringBuilder _text = new();

    private double _phase;
    private double _afcHz;

    private double _runningReal;
    private double _runningImaginary;
    private int _inFilter;

    private long _at;

    /// <summary>Samples left before the next symbol is taken.</summary>
    private int _countdown;

    /// <summary>How far into the symbol period this sample is.</summary>
    private int _position;

    private Complex _previous;
    private bool _hasPrevious;

    private double _realSum;
    private double _magnitudeSum;
    private int _quality;

    private bool _open;
    private int _zeros;

    /// <summary>Open a channel on one signal.</summary>
    /// <param name="sampleRate">Samples a second.</param>
    /// <param name="offsetHz">Where the signal sits in the passband.</param>
    /// <exception cref="ArgumentOutOfRangeException">The rate or offset is unusable.</exception>
    public Psk31Demodulator(int sampleRate, double offsetHz)
    {
        if (sampleRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleRate));
        }

        if (offsetHz <= 0 || offsetHz >= sampleRate / 2.0)
        {
            throw new ArgumentOutOfRangeException(nameof(offsetHz));
        }

        _sampleRate = sampleRate;
        _offsetHz = offsetHz;

        // **DERIVED, NOT TYPED.** 256 at 8 kHz, and whatever it is at another rate.
        _samplesPerSymbol = (int)Math.Round(sampleRate / Baud);

        // **SIXTEEN PLACES TO PUT THE SYMBOL CENTRE**, which is a sixteenth of a bit of
        // timing error at worst - about two milliseconds here, and far inside what the
        // raised-cosine pulse tolerates.
        _bins = 16;
        _samplesPerBin = Math.Max(1, _samplesPerSymbol / _bins);

        _period = new Complex[_samplesPerSymbol];
        _binEnergy = new double[_bins];
        _countdown = _samplesPerSymbol;
    }

    /// <summary>Where the local oscillator has been pulled to, in hertz.</summary>
    /// <remarks>**A READING AND NOT A SETTING.** The AFC moves it; nothing else does.</remarks>
    public double TrackedHz => _offsetHz + _afcHz;

    /// <summary>How BPSK-shaped the last few symbols were, 0.637 to 1.0.</summary>
    public double Quality
        => _magnitudeSum > 0 ? _realSum / _magnitudeSum : 0;

    /// <summary>True while the squelch is letting characters through.</summary>
    public bool IsOpen => _open;

    /// <summary>Feed the channel some audio.</summary>
    /// <param name="samples">Samples in [-1, 1].</param>
    /// <returns>The characters that completed in this call.</returns>
    /// <remarks>
    /// **IT IS A STREAM AND THE CALLER MAY CUT IT ANYWHERE.** Nothing here depends on a
    /// block boundary, so audio arriving in whatever lumps a sound card gives decodes
    /// the same as one long array.
    /// </remarks>
    public string Add(ReadOnlySpan<float> samples)
    {
        var before = _text.Length;

        foreach (var sample in samples)
        {
            One(sample);
        }

        return _text.ToString(before, _text.Length - before);
    }

    /// <summary>One sample through the whole chain.</summary>
    private void One(float sample)
    {
        // **MIX DOWN AT WHERE THE SIGNAL IS BELIEVED TO BE.** The phase accumulator
        // carries the AFC's correction, so a drifting carrier is followed rather than
        // slid off.
        _phase -= 2 * Math.PI * TrackedHz / _sampleRate;

        if (_phase < -2 * Math.PI)
        {
            _phase += 2 * Math.PI;
        }

        var down = new Complex(
            sample * Math.Cos(_phase), sample * Math.Sin(_phase));

        // **AVERAGE OVER ONE BIT, WHICH IS THE MATCHED FILTER FOR THIS PULSE.** A
        // running sum rather than a convolution: the same answer, one add and one
        // subtract a sample instead of five hundred multiplies, and half a million
        // samples have to go through this in well under a second.
        var index = (int)(_at % _samplesPerSymbol);
        var leaving = _period[index];

        _runningReal += down.Real;
        _runningImaginary += down.Imaginary;

        if (_inFilter < _samplesPerSymbol)
        {
            _inFilter++;
        }
        else
        {
            _runningReal -= leaving.Real;
            _runningImaginary -= leaving.Imaginary;
        }

        _period[index] = down;

        var filtered = new Complex(
            _runningReal / _samplesPerSymbol, _runningImaginary / _samplesPerSymbol);

        // **WHERE THE ENVELOPE IS STRONGEST IS WHERE THE SYMBOL IS.** The pulse passes
        // through zero at every reversal, so energy gathered by position within the
        // symbol period peaks where the symbol is. **The position is measured from the
        // instant this decoder samples at**, so a peak away from zero is this decoder
        // being early or late rather than a fact about the signal.
        // **AND IT IS AVERAGED OVER SECONDS, NOT OVER ONE SYMBOL.** The first draft
        // decayed by 0.995 a sample, which is a time constant of two hundred samples -
        // less than a single bit - so the profile was the shape of whichever symbol had
        // just gone past and the clock chased the noise. Measured, that cost a readable
        // decode at +10 dB: CER 0.1976 where clean was 0.0237.
        var bin = Math.Min(_bins - 1, _position / _samplesPerBin);

        _binEnergy[bin] = (_binEnergy[bin] * EnergyDecay)
            + ((1 - EnergyDecay) * filtered.Magnitude);

        _position++;

        if (--_countdown <= 0)
        {
            // **THE CADENCE IS FIXED AND ONLY NUDGED** (see `Retime`). Taking the
            // symbol on a countdown rather than on a moving index is what makes it
            // impossible to sample a period twice or not at all - the fault the first
            // draft had, and one that loses or repeats a bit and so destroys a whole
            // character rather than damaging one.
            if (_at >= _samplesPerSymbol)
            {
                Symbol(filtered);
            }

            _countdown = _samplesPerSymbol + Retime();
            _position = 0;
        }

        _at++;
    }

    /// <summary>Lengthen or shorten the next period by one sample, or leave it.</summary>
    /// <returns>-1, 0 or +1 samples.</returns>
    /// <remarks>
    /// <para>**ONE SAMPLE AT A TIME, SO THE CLOCK WALKS RATHER THAN JUMPS.** A sampling
    /// instant that hopped across the bit period on one noisy estimate would lose a
    /// character every time it moved. One sample a symbol is thirty-one a second, which
    /// is far more than any real clock difference between two sound cards and slow
    /// enough that a single bad reading moves nothing that matters.</para>
    /// <para>**THE PEAK SHOULD BE AT POSITION ZERO**, because position is measured from
    /// where this decoder samples. A peak in the first half of the period says the
    /// energy arrives after the sample, so the period lengthens and the next sample
    /// lands later; a peak in the second half says the opposite.</para>
    /// </remarks>
    private int Retime()
    {
        var best = 0;

        for (var bin = 1; bin < _bins; bin++)
        {
            if (_binEnergy[bin] > _binEnergy[best])
            {
                best = bin;
            }
        }

        if (best == 0)
        {
            return 0;
        }

        return best < _bins / 2 ? 1 : -1;
    }

    /// <summary>One symbol: the bit, the AFC and the squelch.</summary>
    private void Symbol(Complex filtered)
    {
        if (!_hasPrevious)
        {
            _previous = filtered;
            _hasPrevious = true;

            return;
        }

        // **THE DIFFERENTIAL PRODUCT IS THE WHOLE DETECTOR.** No change leaves it on
        // the positive real axis; a reversal puts it on the negative one. Absolute
        // phase never comes into it, which is why this needs no phase lock.
        var d = filtered * Complex.Conjugate(_previous);

        _previous = filtered;

        var magnitude = d.Magnitude;

        if (magnitude <= 0)
        {
            return;
        }

        // **HOW BPSK-SHAPED IT IS**, which is the squelch (see `SquelchRule`).
        _realSum = ((_realSum * (QualityWindow - 1)) + Math.Abs(d.Real)) / QualityWindow;

        _magnitudeSum =
            ((_magnitudeSum * (QualityWindow - 1)) + magnitude) / QualityWindow;

        if (++_quality >= QualityWindow)
        {
            _open = Quality >= SquelchQuality;
        }

        // **SQUARING REMOVES THE MODULATION AND LEAVES TWICE THE ERROR.** Both of BPSK's
        // two states square to the same place, so what is left is the carrier offset -
        // and halving its angle gives the error per symbol, which is the drift.
        // **IT ONLY TRACKS WHAT IT CAN HEAR.** On noise the squared phase is uniform, so
        // a loop that ran all the time would take a random walk away from the frequency
        // the operator tuned to and then be pointing at nothing when a station did
        // start. It moves while the squelch is open and holds still otherwise.
        if (_open)
        {
            var squared = d * d;

            if (squared.Magnitude > 0)
            {
                var error = Math.Atan2(squared.Imaginary, squared.Real) / 2;

                // Radians a symbol into hertz, then a slow step towards it. **The gain
                // is small on purpose**: the drift this has to follow is twenty hertz
                // in a minute, which is a third of a hertz a second, and a loop fast
                // enough to chase symbol-to-symbol noise would put more phase error in
                // than the drift takes out.
                var hz = error * Baud / (2 * Math.PI);

                _afcHz = Math.Clamp(
                    _afcHz + (AfcGain * hz), -AfcRangeHz, AfcRangeHz);
            }
        }

        Bit(d.Real > 0 ? '1' : '0');
    }

    /// <summary>One bit into the varicode stream.</summary>
    /// <remarks>
    /// **`00` IS THE ONLY FRAMING THERE IS**, because no varicode contains it. A run of
    /// zeros longer than two is idle and carries nothing.
    /// </remarks>
    private void Bit(char bit)
    {
        if (bit == '0')
        {
            _zeros++;

            if (_zeros >= 2)
            {
                Take();
            }

            return;
        }

        // **A ONE AFTER A SINGLE ZERO IS PART OF THE CODE**, and that single zero
        // belongs in it.
        if (_zeros == 1)
        {
            _piece.Append('0');
        }

        _zeros = 0;

        // **A PIECE LONGER THAN THE LONGEST CODE IS NOT A CODE.** Without this a
        // squelch that opened on noise could gather bits for ever; with it the worst a
        // burst can do is fill one piece and have it thrown away.
        if (_piece.Length < 16)
        {
            _piece.Append('1');
        }
    }

    /// <summary>Turn the gathered piece into a character, or drop it.</summary>
    /// <remarks>
    /// **AND DROP IT WHILE THE SQUELCH IS SHUT** (§0.0). The bits are still tracked, so
    /// the decoder is in step the moment a station starts; what does not happen is text
    /// appearing on the panel that nobody sent.
    /// </remarks>
    private void Take()
    {
        if (_piece.Length == 0)
        {
            return;
        }

        var code = _piece.ToString();

        _piece.Clear();

        if (!_open)
        {
            return;
        }

        if (Varicode.ByteFor(code) is { } value)
        {
            _text.Append((char)value);
        }
    }
}
