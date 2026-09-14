using System.Security.Cryptography;

namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// **Keeps the device's own receive audio, so a decoder can be proved against real air.**
/// </summary>
/// <remarks>
/// <para>**THE DEVICE STREAM, NOT HAMLET'S VERSION OF IT** (work instruction 344 task 1).
/// The PSK31 path resamples 48 kHz down to 8 before a demodulator sees it, and a capture
/// taken after that step can only ever prove the resampler right. What is wanted is the
/// audio as the sound card handed it over, because the question is what the radio sent
/// and not what Hamlet made of it.</para>
/// <para>**THE REASON THIS EXISTS AT ALL.** Every fixture under `assets/fixtures/` was
/// made by `reference-modem.py` on a machine with no radio (FACT-004, FACT-006). On
/// 2026-09-13 the first PSK31 row reached the screen off 7.070 and it was garbled, and
/// nothing in the tree could say why, because no audio off the air had ever been through
/// the demodulator. A hypothesis measured against synthetic audio is still a hypothesis.
/// </para>
/// <para>**IT RECORDS AND NOTHING ELSE.** No decoding, no squelch, no search, and nothing
/// on the transmit side (§0.2). Handing it samples changes no behaviour anywhere; deleting
/// the class would cost the evidence and not the receiver.</para>
/// </remarks>
public sealed class Psk31Capture
{
    /// <summary>How long a press records for.</summary>
    /// <remarks>
    /// **TWO MINUTES, THE INSTRUCTION'S FIGURE.** Long enough to hold several overs of a
    /// PSK31 exchange at its typing speed, and short enough that the file is a few
    /// megabytes rather than a download.
    /// </remarks>
    public const double Seconds = 120;

    private readonly List<float> _samples = new();

    /// <summary>The device rate the first samples arrived at.</summary>
    /// <remarks>
    /// **TAKEN FROM THE AUDIO AND NEVER ASSUMED** (§0.0). A capture written with a rate
    /// nobody measured plays back at the wrong speed and every frequency in it is a lie.
    /// </remarks>
    public int SampleRate { get; private set; }

    /// <summary>True between the press that started it and the one that ends it.</summary>
    public bool IsRunning { get; private set; }

    /// <summary>How many seconds of audio it holds.</summary>
    public double SecondsHeld
        => SampleRate <= 0 ? 0 : _samples.Count / (double)SampleRate;

    /// <summary>How many seconds it is still waiting for, never below zero.</summary>
    public double SecondsLeft => Math.Max(0, Seconds - SecondsHeld);

    /// <summary>True once it has all it asked for.</summary>
    public bool Full => SecondsHeld >= Seconds;

    /// <summary>Begin recording.</summary>
    public void Start()
    {
        _samples.Clear();
        SampleRate = 0;
        IsRunning = true;
    }

    /// <summary>Take a piece of the device stream.</summary>
    /// <param name="samples">The samples, as the device handed them over.</param>
    /// <param name="sampleRate">The rate they arrived at.</param>
    /// <returns>True while it is still recording after this piece.</returns>
    /// <remarks>
    /// **A RATE CHANGE ENDS THE CAPTURE RATHER THAN JOINING TWO RATES IN ONE FILE.**
    /// Splicing 48 kHz onto 44.1 makes a file whose second half is at the wrong pitch,
    /// which is a wrong answer dressed as evidence.
    /// </remarks>
    public bool Add(ReadOnlySpan<float> samples, int sampleRate)
    {
        if (!IsRunning || sampleRate <= 0)
        {
            return IsRunning;
        }

        if (SampleRate == 0)
        {
            SampleRate = sampleRate;
        }
        else if (SampleRate != sampleRate)
        {
            IsRunning = false;
            return false;
        }

        var room = (int)(Seconds * SampleRate) - _samples.Count;

        if (room <= 0)
        {
            IsRunning = false;
            return false;
        }

        var take = Math.Min(room, samples.Length);

        for (var i = 0; i < take; i++)
        {
            _samples.Add(samples[i]);
        }

        if (_samples.Count >= (int)(Seconds * SampleRate))
        {
            IsRunning = false;
        }

        return IsRunning;
    }

    /// <summary>Stop recording and keep what it has.</summary>
    public void Stop() => IsRunning = false;

    /// <summary>What it recorded, or null where nothing arrived.</summary>
    public MonoAudio? Audio()
        => SampleRate <= 0 || _samples.Count == 0
            ? null
            : new MonoAudio(SampleRate, _samples.ToArray());

    /// <summary>A fingerprint of a written file, so the record and the file can be matched.</summary>
    /// <param name="path">The file.</param>
    /// <returns>Lower-case hexadecimal SHA-256.</returns>
    public static string Fingerprint(string path)
    {
        using var stream = File.OpenRead(path);

        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}
