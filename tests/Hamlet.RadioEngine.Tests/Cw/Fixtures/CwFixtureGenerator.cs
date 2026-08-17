using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Training;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// How one rebuilt fixture is put together, in the terms a receiver imposes.
/// </summary>
/// <param name="Name">The file name, without extension.</param>
/// <param name="Text">What is sent. Never a real callsign (HM-OPEN-018).</param>
/// <param name="DitMilliseconds">How long a dit is.</param>
/// <param name="DahMilliseconds">
/// How long a dah is. Separate from the dit rather than three times it, because
/// real fists are not textbook and the one this repository measured is not.
/// </param>
/// <param name="ElementGapMilliseconds">The silence inside a character.</param>
/// <param name="CharacterGapMilliseconds">The silence between characters.</param>
/// <param name="WordGapMilliseconds">The silence between words.</param>
/// <param name="SignalToNoiseDb">
/// How far the keyed tone stands over the noise, measured inside the receiver's
/// own passband rather than across the whole audio band.
/// </param>
/// <param name="ToneHz">Where the note sits.</param>
/// <param name="DriftHz">How far the note wanders, either side.</param>
/// <param name="QsbHz">How fast the signal fades, or zero for steady.</param>
/// <param name="QsbDepthDb">How deep the fade goes.</param>
/// <param name="PreambleSeconds">
/// How much of the operator's own full-break-in transmission comes first, as the
/// receiver hears it: element-patterned mutes rather than silence.
/// </param>
/// <param name="Seed">Seeds the noise, so the file is the same every time.</param>
public readonly record struct CwFixtureRecipe(
    string Name,
    string Text,
    double DitMilliseconds = 105,
    double DahMilliseconds = 283,
    double ElementGapMilliseconds = 65,
    double CharacterGapMilliseconds = 130,
    double WordGapMilliseconds = 280,
    double SignalToNoiseDb = 15,
    double ToneHz = 615,
    double DriftHz = 3,
    double QsbHz = 0,
    double QsbDepthDb = 0,
    double PreambleSeconds = 0,
    int Seed = 20260817)
{
    /// <summary>The sending speed these element lengths imply.</summary>
    public double WordsPerMinute => 1200.0 / DitMilliseconds;
}

/// <summary>
/// Builds CW fixtures out of what a receiver actually delivers (HM-OPEN-018,
/// HM-DEC-096).
/// </summary>
/// <remarks>
/// <para>**THE OLD FIXTURES ENCODE A PHYSICAL IMPOSSIBILITY AND THAT IS WHY THE
/// REFERENCE CHAIN SCORES NOTHING ON THEM.** They are tone or exact digital
/// silence. Run the validated decoder against them and it reports twenty percent
/// of `clean-12wpm` as active audio, eleven percent of `clean-18wpm`, and **zero
/// percent of `clean-25wpm`**, in which it finds no tone at all. It is not
/// broken: any transmit-mute guard reads digital silence as a muted receiver,
/// because on a real radio nothing else produces it.</para>
/// <para>**A REAL RECEIVER NEVER HANDS OVER DIGITAL SILENCE.** Between elements
/// there is band noise. During the operator's own transmission on full break-in
/// there is the codec's residue, which the captures put near minus eighty-two
/// decibels rather than at nothing. A fixture without a noise floor cannot
/// exercise the guard, the threshold fit, or the refusals, which between them are
/// most of what this decoder is.</para>
/// <para>This is the second time in this repository. The scope parser and the
/// fixtures that certified it were green for months while the instrument
/// discarded every frame (HM-DEC-094, §12.5). **When a test passes and the
/// instrument disagrees, suspect the fixture.**</para>
/// <para>Everything here is deterministic: a seeded generator, no clock, no
/// recorded audio, no hand-tuning. A fixture regenerated next year is the same
/// file, which is what lets a test assert that it is (§5).</para>
/// </remarks>
public static class CwFixtureGenerator
{
    /// <summary>Samples per second of every generated fixture.</summary>
    /// <remarks>
    /// Eight thousand, matching the existing fixtures and the rate the reference
    /// chain's own figures are quoted at. A CW note lives under a kilohertz.
    /// </remarks>
    public const int SampleRate = 8_000;

    /// <summary>The low edge of the receiver's passband, in hertz.</summary>
    /// <remarks>
    /// Three hundred and fifty to eight hundred and seventy, which is the FIL2
    /// five hundred hertz filter as the captures show it: everything outside is
    /// down about thirty decibels and everything inside is the band.
    /// </remarks>
    public const double PassbandLowHz = 350;

    /// <summary>The high edge of the receiver's passband, in hertz.</summary>
    public const double PassbandHighHz = 870;

    /// <summary>How far the out-of-band floor sits below the passband.</summary>
    public const double OutOfBandDropDb = 30;

    /// <summary>
    /// What the audio measures during the operator's own transmission.
    /// </summary>
    /// <remarks>
    /// <para>**MINUS EIGHTY-TWO, AND NEVER ZERO.** The real captures bottom out
    /// there, because the radio stops the audio while the codec carries on
    /// streaming, so what arrives is the converter's own residue. Exact zero is
    /// the defect being removed and it sits a hundred and fifty decibels away
    /// from anything a receiver does.</para>
    /// <para>**THE BRIEF ASKS FOR MINUS NINETY AND MINUS NINETY WOULD NOT WORK**,
    /// which is worth recording rather than quietly correcting. The figure comes
    /// from `CW_RECEIVE_BRIEF.md` §4, written before the guard had a lower bound.
    /// The guard now treats anything at or below
    /// <see cref="CwTransmitGuard.SilenceBelowDbfs"/>, which is minus ninety, as a
    /// file with nothing in it rather than as a muted receiver — precisely so the
    /// noiseless fixtures stop being read as one long transmission. A preamble
    /// generated at minus ninety therefore sits exactly on that boundary and
    /// fails to exercise the guard it exists to exercise.</para>
    /// <para>Minus eighty-two is the measurement, it is what the captures show,
    /// and it sits squarely inside the band the guard calls a mute.</para>
    /// </remarks>
    public const double MuteDbfs = -82;

    /// <summary>Transmit-receive changeover time, in seconds.</summary>
    public const double HangSeconds = 0.024;

    /// <summary>Rise and fall of each keyed element, in seconds.</summary>
    /// <remarks>Five milliseconds. A square edge clicks across the band and
    /// teaches a decoder a transient no transmitter produces.</remarks>
    private const double EdgeSeconds = 0.005;

    /// <summary>Silence before the message, in seconds.</summary>
    private const double LeadInSeconds = 1.0;

    /// <summary>Silence after it, in seconds.</summary>
    private const double TailSeconds = 1.5;

    /// <summary>How long the drift takes to go round once, in seconds.</summary>
    private const double DriftSeconds = 10;

    /// <summary>
    /// Join two stations into one recording, one after the other (HM-DEC-104).
    /// </summary>
    /// <param name="name">What to call the result.</param>
    /// <param name="first">The station that starts.</param>
    /// <param name="second">The station that answers.</param>
    /// <param name="betweenSeconds">The silence between them.</param>
    /// <returns>The audio and a sidecar describing both halves.</returns>
    /// <remarks>
    /// <para>**JOINED ACROSS A GAP AND NEVER MID-CHARACTER**, so the seam is a
    /// signal rather than an artifact a decoder could learn. Each segment is
    /// generated complete, with its own keying envelope and its own noise, and
    /// the join is a stretch of band between them exactly as it would be on the
    /// air when one station stops and another starts.</para>
    /// <para>This is the situation an answered call actually produces, and it is
    /// the first committed test of four capabilities that were built on rulings
    /// alone: clock loss on a discontinuity, the previous clock kept as a
    /// candidate, tracker switching on keying structure, and the speed-change
    /// annotation.</para>
    /// </remarks>
    public static (MonoAudio Audio, string Sidecar) Join(
        string name,
        CwFixtureRecipe first,
        CwFixtureRecipe second,
        double betweenSeconds = 1.5)
    {
        var (one, _) = Generate(first);
        var (two, _) = Generate(second);

        var between = (int)Math.Round(betweenSeconds * SampleRate);
        var joined = new float[one.Samples.Length + between + two.Samples.Length];

        one.Samples.CopyTo(joined.AsSpan());

        // The band carries on between them. Silence here would be a seam the
        // decoder could find, and a receiver never delivers one.
        var gapNoise = new float[between];
        ShapedNoise(gapNoise, first.Seed ^ 0x51D3);
        gapNoise.CopyTo(joined.AsSpan(one.Samples.Length));

        two.Samples.CopyTo(joined.AsSpan(one.Samples.Length + between));

        var audio = new MonoAudio(SampleRate, joined);
        var handover = (double)(one.Samples.Length + between) / SampleRate;

        var text = new StringBuilder();

        text.AppendLine($"name          {name}");
        text.AppendLine("generated     tests/Hamlet.RadioEngine.Tests/Cw/Fixtures");
        text.AppendLine($"sampleRate    {SampleRate}");
        text.AppendLine($"seconds       {audio.Duration.TotalSeconds:0.00}");
        text.AppendLine();
        text.AppendLine($"text          {first.Text} {second.Text}");
        text.AppendLine($"stations      2, joined across {betweenSeconds:0.0} s of band");
        text.AppendLine($"handover      {handover:0.00} s");
        text.AppendLine();
        text.AppendLine($"first         {first.Text}");
        text.AppendLine($"  toneHz      {first.ToneHz:0} Hz");
        text.AppendLine($"  wpm         {first.WordsPerMinute:0.0}");
        text.AppendLine($"  dit         {first.DitMilliseconds:0} ms");
        text.AppendLine($"  snrDb       {first.SignalToNoiseDb:0.0} dB");
        text.AppendLine();
        text.AppendLine($"second        {second.Text}");
        text.AppendLine($"  toneHz      {second.ToneHz:0} Hz");
        text.AppendLine($"  wpm         {second.WordsPerMinute:0.0}");
        text.AppendLine($"  dit         {second.DitMilliseconds:0} ms");
        text.AppendLine($"  snrDb       {second.SignalToNoiseDb:0.0} dB");
        text.AppendLine();
        text.AppendLine($"peak          {AudioTap.PeakOf(audio):0.0} dBFS");

        return (audio, text.ToString());
    }

    /// <summary>Generate one fixture.</summary>
    /// <param name="recipe">What to build.</param>
    /// <returns>The audio and the sidecar describing it.</returns>
    public static (MonoAudio Audio, string Sidecar) Generate(CwFixtureRecipe recipe)
    {
        var keying = KeyEdges(recipe, out var messageStart);

        var totalSeconds = keying.Length > 0
            ? keying[^1] + TailSeconds
            : messageStart + TailSeconds;

        var count = Math.Max(1, (int)Math.Round(totalSeconds * SampleRate));
        var samples = new float[count];

        // **THE BAND FIRST, THEN THE STATION ON TOP OF IT.** A receiver hands over
        // noise the whole time and a signal some of the time, which is the order
        // these have to be built in for the signal-to-noise figure to mean what it
        // says.
        var noiseRms = ShapedNoise(samples, recipe.Seed);

        // A tone whose root-mean-square sits the requested distance over the
        // noise already in the passband.
        var amplitude = Math.Sqrt(2) * noiseRms * Math.Pow(10, recipe.SignalToNoiseDb / 20);

        RenderKeying(samples, keying, recipe, amplitude);

        var mutes = recipe.PreambleSeconds > 0
            ? ApplyPreambleMutes(samples, recipe)
            : Array.Empty<(double Start, double End)>();

        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = Math.Clamp(samples[i], -1f, 1f);
        }

        var audio = new MonoAudio(SampleRate, samples);

        return (audio, Sidecar(recipe, audio, noiseRms, amplitude, mutes.Length, messageStart));
    }

    /// <summary>
    /// The instants the key changes state, from this recipe's own gap lengths.
    /// </summary>
    /// <remarks>
    /// **NOT ONE TO THREE TO SEVEN.** The station this repository recorded sends
    /// element gaps of about sixty-five milliseconds against dits of a hundred and
    /// five, which is to say **its gaps are shorter than its dits**, and character
    /// gaps of a hundred and thirty rather than the three hundred and fifteen the
    /// textbook asks for. Nothing in the old fixture set contains that shape, and
    /// it is the shape that breaks any decoder classifying gaps by counting dits.
    /// </remarks>
    private static double[] KeyEdges(CwFixtureRecipe recipe, out double messageStart)
    {
        messageStart = LeadInSeconds + recipe.PreambleSeconds;

        var edges = new List<double> { messageStart };
        var at = messageStart;
        var first = true;

        foreach (var word in recipe.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!first)
            {
                // **ADD A KEY-UP SEGMENT; DO NOT EXTEND THE LAST MARK.** Writing
                // over the last edge instead of appending one lengthens the
                // preceding dah by a whole word gap and deletes the gap
                // altogether, which runs every word into the next. The reference
                // decoder read `CQ CQ DE N0CALL N0CALL K` back as
                // `YEI YEI K MEEEE ...` until this was fixed, which is the gate
                // in phase 4 doing exactly what it exists to do.
                at += recipe.WordGapMilliseconds / 1000.0;
                edges.Add(at);
            }

            var firstCharacter = true;

            var join = false;

            foreach (var character in word)
            {
                // **A PROSIGN IS ONE CHARACTER SENT AS TWO LETTERS RUN
                // TOGETHER**, which is what the caret means here and what `^`
                // means to the radio's own keyer (Full Manual p. 19-11). Sending
                // `AR` with a character gap in it is sending A and R, which is
                // not the same thing at all.
                if (character == '^')
                {
                    join = true;
                    continue;
                }

                var pattern = MorseCode.Spell(character);

                if (pattern is null)
                {
                    continue;
                }

                if (join)
                {
                    join = false;
                    at += recipe.ElementGapMilliseconds / 1000.0;
                    edges.Add(at);

                    foreach (var element in pattern)
                    {
                        var joined = element == '.'
                            ? recipe.DitMilliseconds
                            : recipe.DahMilliseconds;

                        at += joined / 1000.0;
                        edges.Add(at);
                        at += recipe.ElementGapMilliseconds / 1000.0;
                        edges.Add(at);
                    }

                    // The trailing gap belongs to whatever comes next.
                    edges.RemoveAt(edges.Count - 1);
                    at -= recipe.ElementGapMilliseconds / 1000.0;
                    continue;
                }

                if (!firstCharacter)
                {
                    at += recipe.CharacterGapMilliseconds / 1000.0;
                    edges.Add(at);
                }

                var firstElement = true;

                foreach (var element in pattern)
                {
                    if (!firstElement)
                    {
                        at += recipe.ElementGapMilliseconds / 1000.0;
                        edges.Add(at);
                    }

                    var length = element == '.'
                        ? recipe.DitMilliseconds
                        : recipe.DahMilliseconds;

                    at += length / 1000.0;
                    edges.Add(at);

                    firstElement = false;
                }

                firstCharacter = false;
                first = false;
            }
        }

        return edges.ToArray();
    }

    /// <summary>
    /// Fill the buffer with noise shaped to the receiver's passband.
    /// </summary>
    /// <returns>The root-mean-square of the noise inside that passband.</returns>
    /// <remarks>
    /// White noise through three cascaded bandpass sections, plus a little of the
    /// unshaped noise left underneath so the out-of-band floor is thirty decibels
    /// down rather than absent. Absent is the old fixtures' mistake in a different
    /// place: a receiver's filter attenuates, it does not delete.
    /// </remarks>
    private static double ShapedNoise(float[] samples, int seed)
    {
        var state = (uint)(seed == 0 ? 1 : seed);

        double NextUniform()
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return ((state & 0xFFFFFF) + 1) / 16777217.0;
        }

        var white = new double[samples.Length];

        for (var i = 0; i < white.Length; i += 2)
        {
            var u1 = NextUniform();
            var u2 = NextUniform();
            var magnitude = Math.Sqrt(-2 * Math.Log(u1));

            white[i] = magnitude * Math.Cos(2 * Math.PI * u2);

            if (i + 1 < white.Length)
            {
                white[i + 1] = magnitude * Math.Sin(2 * Math.PI * u2);
            }
        }

        var shaped = new double[white.Length];
        Array.Copy(white, shaped, white.Length);

        var center = Math.Sqrt(PassbandLowHz * PassbandHighHz);
        var q = center / (PassbandHighHz - PassbandLowHz);

        for (var pass = 0; pass < 3; pass++)
        {
            Bandpass(shaped, center, q);
        }

        // Normalize the shaped noise to a workable level, then put the skirt back.
        var rms = Rms(shaped);

        if (rms <= 0)
        {
            return 0;
        }

        const double target = 0.02;
        var gain = target / rms;
        var skirt = target * Math.Pow(10, -OutOfBandDropDb / 20);

        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)((shaped[i] * gain) + (white[i] * skirt));
        }

        return target;
    }

    /// <summary>One biquad bandpass section, in place.</summary>
    private static void Bandpass(double[] values, double centerHz, double q)
    {
        var w0 = 2 * Math.PI * centerHz / SampleRate;
        var alpha = Math.Sin(w0) / (2 * q);

        var b0 = alpha;
        const double B1 = 0;
        var b2 = -alpha;
        var a0 = 1 + alpha;
        var a1 = -2 * Math.Cos(w0);
        var a2 = 1 - alpha;

        double x1 = 0, x2 = 0, y1 = 0, y2 = 0;

        for (var i = 0; i < values.Length; i++)
        {
            var x0 = values[i];
            var y0 = ((b0 * x0) + (B1 * x1) + (b2 * x2) - (a1 * y1) - (a2 * y2)) / a0;

            x2 = x1;
            x1 = x0;
            y2 = y1;
            y1 = y0;

            values[i] = y0;
        }
    }

    /// <summary>Write the keyed tone over the noise already in the buffer.</summary>
    private static void RenderKeying(
        float[] samples, double[] edges, CwFixtureRecipe recipe, double amplitude)
    {
        if (edges.Length < 2)
        {
            return;
        }

        var phase = 0.0;
        var segment = 0;
        var depth = recipe.QsbDepthDb;

        for (var i = 0; i < samples.Length; i++)
        {
            var t = (double)i / SampleRate;

            // **THE NOTE WANDERS, BECAUSE REAL ONES DO.** A radio warming up moves
            // a few hertz over a few seconds, and a decoder tracking a single fixed
            // bin watches it leave.
            var toneHz = recipe.ToneHz
                + (recipe.DriftHz * Math.Sin(2 * Math.PI * t / DriftSeconds));

            phase += 2 * Math.PI * toneHz / SampleRate;

            while (segment < edges.Length - 1 && t >= edges[segment + 1])
            {
                segment++;
            }

            if (segment >= edges.Length - 1 || segment % 2 != 0)
            {
                continue;
            }

            var gate = Gate(t, edges[segment], edges[segment + 1]);

            if (gate <= 0)
            {
                continue;
            }

            var fade = 1.0;

            if (recipe.QsbHz > 0 && depth > 0)
            {
                // **THE FADE SWINGS ABOUT THE STATED LEVEL RATHER THAN ONLY
                // DOWNWARD FROM IT.** Twenty-five decibels of depth taken entirely
                // below a five decibel signal puts the trough twenty decibels under
                // the noise, which does not fade the message so much as delete most
                // of it: measured, the surviving marks were separated by gaps of
                // seven hundred to two thousand seven hundred milliseconds and the
                // reference read a quarter of it.
                //
                // A depth is a peak-to-trough distance, so half of it goes each
                // way and the stated figure is the average the operator would
                // read on a meter.
                var swing = Math.Cos(2 * Math.PI * recipe.QsbHz * t);
                fade = Math.Pow(10, depth * swing / 2 / 20);
            }

            samples[i] += (float)(Math.Sin(phase) * amplitude * gate * fade);
        }
    }

    /// <summary>A raised-cosine envelope over one keyed element.</summary>
    private static double Gate(double t, double start, double end)
    {
        var rise = Math.Clamp((t - start) / EdgeSeconds, 0, 1);
        var fall = Math.Clamp((end - t) / EdgeSeconds, 0, 1);
        return 0.5 * (1 - Math.Cos(Math.PI * Math.Min(rise, fall)));
    }

    /// <summary>
    /// Cut the audio the way full break-in does, ahead of the message
    /// (HM-OPEN-018 phase 3).
    /// </summary>
    /// <returns>The muted spans, so the sidecar can say where they are.</returns>
    /// <remarks>
    /// The operator sending at twenty words a minute with his own receiver muting
    /// on every key-down. What reaches the sound card is the band, chopped into
    /// slivers by his own keying, with the changeover hanging on either side of
    /// each one. Decoded as elements those slivers spell a confident run of E and
    /// T, which is the most seductive wrong output this feature can produce.
    /// </remarks>
    private static (double Start, double End)[] ApplyPreambleMutes(
        float[] samples, CwFixtureRecipe recipe)
    {
        var dit = MorseCode.Dit(20).TotalSeconds;
        var pattern = MorseCode.KeyPattern("CQ CQ DE N0CALL K");
        var spans = new List<(double Start, double End)>();

        var at = LeadInSeconds;
        var keyDown = true;

        foreach (var units in pattern)
        {
            var length = units * dit;

            if (keyDown && at + length < LeadInSeconds + recipe.PreambleSeconds)
            {
                spans.Add((at, at + length + HangSeconds));
            }

            at += length;
            keyDown = !keyDown;

            if (at > LeadInSeconds + recipe.PreambleSeconds)
            {
                break;
            }
        }

        var floor = Math.Pow(10, MuteDbfs / 20);
        var state = (uint)(recipe.Seed ^ 0x5D5D);

        foreach (var (start, end) in spans)
        {
            var from = Math.Max(0, (int)(start * SampleRate));
            var to = Math.Min(samples.Length, (int)(end * SampleRate));

            for (var i = from; i < to; i++)
            {
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;

                // Not zero. The codec keeps streaming while the radio stops the
                // audio, so what arrives is its residue.
                samples[i] = (float)(floor * ((((state & 0xFFFF) / 32768.0) - 1) * 1.7));
            }
        }

        return spans.ToArray();
    }

    /// <summary>Root-mean-square of a buffer.</summary>
    private static double Rms(double[] values)
    {
        var sum = 0.0;

        foreach (var value in values)
        {
            sum += value * value;
        }

        return Math.Sqrt(sum / Math.Max(1, values.Length));
    }

    /// <summary>What this fixture is, written beside it.</summary>
    private static string Sidecar(
        CwFixtureRecipe recipe,
        MonoAudio audio,
        double noiseRms,
        double amplitude,
        int muteCount,
        double messageStart)
    {
        var text = new StringBuilder();

        text.AppendLine($"name          {recipe.Name}");
        text.AppendLine("generated     tests/Hamlet.RadioEngine.Tests/Cw/Fixtures");
        text.AppendLine($"seed          {recipe.Seed}");
        text.AppendLine($"sampleRate    {audio.SampleRate}");
        text.AppendLine($"seconds       {audio.Duration.TotalSeconds:0.00}");
        text.AppendLine();
        text.AppendLine($"text          {recipe.Text}");
        text.AppendLine($"messageStart  {messageStart:0.00} s");
        text.AppendLine($"dit           {recipe.DitMilliseconds:0} ms");
        text.AppendLine($"dah           {recipe.DahMilliseconds:0} ms");
        text.AppendLine($"elementGap    {recipe.ElementGapMilliseconds:0} ms");
        text.AppendLine($"characterGap  {recipe.CharacterGapMilliseconds:0} ms");
        text.AppendLine($"wordGap       {recipe.WordGapMilliseconds:0} ms");
        text.AppendLine($"wpm           {recipe.WordsPerMinute:0.0}");
        text.AppendLine();
        text.AppendLine($"toneHz        {recipe.ToneHz:0} Hz, drifting +/- {recipe.DriftHz:0} Hz");
        text.AppendLine($"snrDb         {recipe.SignalToNoiseDb:0.0} dB in the passband");
        text.AppendLine($"passband      {PassbandLowHz:0}-{PassbandHighHz:0} Hz, "
            + $"skirt {OutOfBandDropDb:0} dB down");
        text.AppendLine($"noiseRms      {20 * Math.Log10(noiseRms):0.0} dBFS");
        text.AppendLine($"toneAmplitude {20 * Math.Log10(amplitude):0.0} dBFS peak");
        text.AppendLine($"qsb           {(recipe.QsbHz > 0
            ? $"{recipe.QsbHz:0.0} Hz, {recipe.QsbDepthDb:0} dB deep"
            : "none")}");
        text.AppendLine($"preamble      {(recipe.PreambleSeconds > 0
            ? $"{recipe.PreambleSeconds:0.0} s of own-transmit mutes, {muteCount} spans, "
              + $"floor {MuteDbfs:0} dBFS"
            : "none")}");
        text.AppendLine();
        text.AppendLine($"peak          {AudioTap.PeakOf(audio):0.0} dBFS");

        return text.ToString();
    }
}
