using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;

namespace Hamlet.RadioEngine.Scan;

/// <summary>What the ear has heard so far in a catch.</summary>
/// <param name="Light">The light now.</param>
/// <param name="ShapeSeen">Whether the light has gone green - a shape found, or reading - at any time in the catch.</param>
/// <param name="SilentSeconds">Seconds since the last mark stood, or since the catch began where none has.</param>
/// <param name="Letters">Letters printed so far.</param>
/// <param name="HeardSeconds">Seconds of audio heard in the catch.</param>
public readonly record struct CatchSense(CwShapeLight Light, bool ShapeSeen, double SilentSeconds, int Letters, double HeardSeconds);

/// <summary>One sender the gate held in a catch.</summary>
/// <param name="PitchHz">Its pitch.</param>
/// <param name="ShapeScore">How much it sounded like code.</param>
/// <param name="Marks">Marks stood.</param>
/// <param name="Printed">Whether it was the one printed.</param>
public sealed record CatchStation(double PitchHz, double ShapeScore, int Marks, bool Printed);

/// <summary>One letter printed in a catch, and when its last mark ended, in seconds from the catch's start.</summary>
/// <param name="Seconds">When.</param>
/// <param name="Text">The letter, a prosign, or the placeholder.</param>
public sealed record CatchLetter(double Seconds, string Text);

/// <summary>One change of the light in a catch: from when, in seconds from the catch's start, and to what.</summary>
/// <param name="Seconds">When.</param>
/// <param name="Light">The light's words.</param>
public sealed record CatchLight(double Seconds, string Light);

/// <summary>Everything a catch heard.</summary>
/// <param name="Stations">Every sender the gate held.</param>
/// <param name="Text">The text printed, a space at each word end.</param>
/// <param name="Letters">Each letter and its time.</param>
/// <param name="Lights">The light's states over the stay.</param>
/// <param name="SampleRate">The audio's rate, which the WAV is written at.</param>
/// <param name="Seconds">How long it heard.</param>
public sealed record CatchHeard(
    IReadOnlyList<CatchStation> Stations,
    string Text,
    IReadOnlyList<CatchLetter> Letters,
    IReadOnlyList<CatchLight> Lights,
    int SampleRate,
    double Seconds);

/// <summary>
/// **THE SCAN'S OWN EAR** (work instruction 540, HM-DEC-244): it records a catch's audio whole and reads it through the
/// same detector and gate the terminal reads through, fresh for each catch, so the scan knows whether a shape formed
/// and what was printed without touching what the terminal reads.
/// </summary>
/// <remarks>
/// <para>**THE TERMINAL'S DECODER IS NOT TOUCHED.** The ear subscribes to the same audio and reads it again: what
/// Hamlet shows on the CW tab is exactly what it was, and every catch starts with no sender held from the last place.</para>
/// <para>**THE WAV IS THE AUDIO AS HEARD**, at the source's own rate: 48 kHz from the IC-7300's USB codec, so a catch
/// replays through any later build exactly as this one heard it.</para>
/// </remarks>
public sealed class CwCatchEar : IDisposable
{
    private readonly IAudioSource _source;
    private readonly double _pitchHz;
    private readonly double _widthHz;
    private readonly object _gate = new();
    private readonly List<float[]> _audio = new();
    private readonly List<CatchLetter> _letters = new();
    private readonly List<CatchLight> _lights = new();
    private readonly System.Text.StringBuilder _text = new();

    private CwChain? _chain;
    private long _sequence;
    private long _samples;
    private int _rate;
    private bool _listening;
    private bool _shapeSeen;
    private double _lastMarkSeconds = double.NegativeInfinity;
    private CwShapeLight _light = CwShapeLight.Listening;

    /// <summary>Creates an ear on a source.</summary>
    /// <param name="source">The audio Hamlet hears.</param>
    /// <param name="pitchHz">The radio's CW pitch, where the detector's band is centred.</param>
    /// <param name="widthHz">The radio's filter width.</param>
    /// <param name="radio">
    /// What Hamlet knows about the radio, read on every chunk for the passband as the app reads it (work instruction 542); null
    /// to use <paramref name="pitchHz"/> and <paramref name="widthHz"/>.
    /// </param>
    public CwCatchEar(IAudioSource source, double pitchHz, double widthHz, Func<Rig.RigState>? radio = null)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _pitchHz = pitchHz;
        _widthHz = widthHz;
        _radio = radio;
        _source.SamplesReady += OnSamples;
    }

    private readonly Func<Rig.RigState>? _radio;

    /// <summary>Begin a catch: forget the last one, start recording and reading.</summary>
    public void Begin()
    {
        lock (_gate)
        {
            _audio.Clear();
            _letters.Clear();
            _lights.Clear();
            _text.Clear();
            _samples = 0;
            _sequence = 0;
            _rate = 0;
            _shapeSeen = false;
            _lastMarkSeconds = double.NegativeInfinity;
            _light = CwShapeLight.Listening;
            _chain?.Dispose();
            _chain = null;
            _lights.Add(new CatchLight(0, Words(CwShapeLight.Listening)));
            _listening = true;
        }
    }

    /// <summary>What the catch has heard so far.</summary>
    public CatchSense Sense()
    {
        lock (_gate)
        {
            var heard = _rate > 0 ? _samples / (double)_rate : 0;

            return new CatchSense(
                _light,
                _shapeSeen,
                double.IsNegativeInfinity(_lastMarkSeconds) ? heard : heard - _lastMarkSeconds,
                _letters.Count,
                heard);
        }
    }

    /// <summary>End the catch: stop recording, write the audio, and say what was heard.</summary>
    /// <param name="wavPath">Where the WAV goes, or null to write none.</param>
    /// <returns>What it heard.</returns>
    public CatchHeard End(string? wavPath)
    {
        float[] samples;
        int rate;
        CatchHeard heard;

        lock (_gate)
        {
            _listening = false;
            _chain?.Decoder.Flush();

            rate = _rate > 0 ? _rate : _source.SampleRate;
            samples = new float[_audio.Sum(a => a.Length)];

            var at = 0;

            foreach (var chunk in _audio)
            {
                chunk.CopyTo(samples, at);
                at += chunk.Length;
            }

            var reading = _chain?.Decoder.ShapeSide;

            heard = new CatchHeard(
                reading?.Senders.Select(s => new CatchStation(s.PitchHz, s.ShapeScore, s.Marks, s.Printed)).ToList() ?? [],
                _text.ToString().Trim(),
                _letters.ToList(),
                _lights.ToList(),
                rate,
                rate > 0 ? samples.Length / (double)rate : 0);

            _audio.Clear();
        }

        if (wavPath is not null)
        {
            WavAudio.Write(wavPath, new MonoAudio(rate, samples));
        }

        return heard;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _source.SamplesReady -= OnSamples;
        _chain?.Dispose();
    }

    private void OnSamples(in AudioChunk chunk)
    {
        lock (_gate)
        {
            if (!_listening)
            {
                return;
            }

            if (_chain is null)
            {
                // **THE APP'S OWN CHAIN** (work instruction 542, HM-DEC-246): the same decoder, detector, gate and reader,
                // wired as the app wires them, rather than a chain of the ear's own.
                _rate = chunk.SampleRate;
                _chain = new CwChain(_rate, _pitchHz);

                var gate = _chain.Decoder.Runs;

                gate.CharacterRead += c => _text.Append(c.Text);
                gate.RunRead += (c, run) => _letters.Add(new CatchLetter(run[^1].ToSeconds, c.Text));
            }

            // **THE PASSBAND IS THE RADIO'S**, read as the app reads it on every scope tick: its CW pitch and filter in CW, the
            // whole band otherwise. Without a view of the radio, the pitch and width the ear was given.
            if (_radio?.Invoke() is { } state)
            {
                _chain.SetPassband(state);
            }
            else
            {
                _chain.Detector.SetPassband(_pitchHz, _widthHz);
            }

            _audio.Add(chunk.Samples.ToArray());
            _samples += chunk.Samples.Length;
            _chain.Process(chunk);

            // When the last mark stood: read from the detector without taking anything from the decoder's own reading.
            var batch = _chain.Detector.MarksSince(_sequence);

            if (batch.Marks.Count > 0)
            {
                _sequence = batch.Marks.Max(m => m.Sequence);
                _lastMarkSeconds = Math.Max(_lastMarkSeconds, batch.Marks.Max(m => m.ToSeconds));
            }

            var light = _chain.Detector.Reading.ShapeLight;

            if (light != _light)
            {
                _light = light;
                _lights.Add(new CatchLight(_samples / (double)_rate, Words(light)));
            }

            // **A POSITIVE IS THE GREEN LIGHT** (work instruction 540): a sequence standing, dits and dahs made out. The amber
            // light came on within seconds on band noise and on a steady carrier in the scan's own tests, so amber alone is not
            // dits and dahs made out; it is kept in the catch's light history.
            _shapeSeen |= light is CwShapeLight.Found or CwShapeLight.Reading;
        }
    }

    /// <summary>The light's words, as the CW tab writes them.</summary>
    internal static string Words(CwShapeLight light) => light switch
    {
        CwShapeLight.Forming => "shape forming",
        CwShapeLight.Found => "shape found",
        CwShapeLight.Reading => "reading",
        _ => "listening",
    };
}
