using System.Globalization;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Dsp;
using Ft8Sharp.Tests.Encode;

namespace Ft8Sharp.Tests.Fixtures;

/// <summary>
/// <b>The worked example: a capture this repository can legitimately produce, and a fixture whose
/// rows are ground truth about it.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>ITS ROWS ARE NOT WSJT-X'S AND ITS PROVENANCE SAYS SO.</b> There is no WSJT-X on this machine
/// and no unit may assume one, so an example fixture carrying invented WSJT-X rows would be the
/// single worst artefact this unit could leave behind - indistinguishable from a real one to every
/// session that came after, and trusted by all of them. Everything below is built from audio the
/// ladder synthesised, which means every number in it is something this repository <em>knows</em>
/// rather than something it heard, and <see cref="Ft8CaptureFixture.ProvenanceExample"/> is what it
/// is stamped with. <see cref="Ft8CaptureFixture.RequireScorable"/> refuses to score against it.
/// </para>
/// <para>
/// <b>Why it exists at all.</b> The reader in this folder needs something real to be tested against,
/// and Tim needs something to hold his first shack-generated fixture beside.
/// </para>
/// <para>
/// <b>Deterministic by construction, and that is checked rather than hoped.</b> Fixed messages, fixed
/// frequencies, fixed offsets, a fixed noise seed and a fixed timestamp, so
/// <see cref="Build"/> produces the same bytes on every machine in every process.
/// <c>Ft8ExampleFixtureTests</c> rebuilds it and compares it against what is committed, so the
/// example cannot drift away from the code that made it.
/// </para>
/// </remarks>
internal static class Ft8ExampleFixture
{
    /// <summary>The capture's file name. The fixture is the same stem plus the fixture extension.</summary>
    internal const string CaptureName = "ft8-example-244.wav";

    /// <summary>The fixture's file name, beside the capture.</summary>
    internal const string FixtureName = "ft8-example-244" + Ft8CaptureFixture.Extension;

    /// <summary>
    /// <b>A stated instant, not a clock reading.</b> A generated file that carried "now" would differ
    /// on every run and could not be checked against what is committed.
    /// </summary>
    internal const string Utc = "2026-09-04T00:00:00Z";

    /// <summary>The seed the noise is drawn from. Any fixed number would do; this one is unit 244.</summary>
    internal const int NoiseSeed = 244_001;

    /// <summary>
    /// The ratio commanded onto the samples. <b>Comfortably above the ladder's collapse</b>, because
    /// this file's job is to exercise a reader and a scorer, not to measure sensitivity - that is the
    /// ladder's job and it is a different instrument.
    /// </summary>
    internal const double RungDecibels = 5.0;

    private static readonly int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>Where each transmission is put, and which message it carries.</summary>
    /// <remarks>
    /// Three of them, at three frequencies and three different offsets, because a fixture where every
    /// station starts at the same sample and the rows all carry the same <c>dt</c> would exercise
    /// neither column.
    /// </remarks>
    private static readonly (int Index, double FrequencyHz, int SymbolsIn)[] Placements =
    [
        (0, 1000.0, 3),
        (1, 1500.0, 4),
        (2, 2000.0, 5),
    ];

    /// <summary>The comment block written above the headers, so the file explains itself in place.</summary>
    internal static IReadOnlyList<string> Preamble =>
    [
        string.Empty,
        "THIS IS AN EXAMPLE AND ITS ROWS ARE NOT WSJT-X'S.",
        string.Empty,
        "The audio beside it was synthesised by this repository's own ladder, which knows",
        "exactly what it transmitted, where it put it and at what ratio. So every row below",
        "is ground truth about a signal we built, not a decode of a signal we heard.",
        "provenance is \"example\" and Ft8CaptureFixture.RequireScorable REFUSES to score a",
        "claim against it.",
        string.Empty,
        "It exists so the reader has something to be tested against, and so Tim has something",
        "to hold his first real one beside. See docs/ft8-capture-fixture-format.md.",
        string.Empty,
        "On the snrDb column: it is the ratio actually DELIVERED onto the samples, each",
        "transmission's own power against the noise power the fixture itself mixed in. That is",
        "a figure this repository computed about audio it made. WSJT-X reports a MEASURED",
        "per-message SNR, which is not the same measurement - and is exactly why this file's",
        "provenance can never be \"wsjtx\".",
    ];

    /// <summary>The capture's bytes and the fixture that describes them, both built from scratch.</summary>
    internal static (byte[] Wav, Ft8CaptureFixture Fixture) Build()
    {
        var population = Ft8Step6Ladder.Population();
        var slot = SearchFixture.EmptySlot(Rate);
        var perSymbol = Ft8Waveform.SamplesPerSymbol(Rate);

        var placed = new List<(EncodeCorpus.Entry Entry, double Hz, int Offset, double Power)>();
        foreach (var (index, hz, symbolsIn) in Placements)
        {
            var entry = population[index];
            var offset = perSymbol * symbolsIn;
            SearchFixture.Place(slot, Rate, entry, hz, offset);
            placed.Add((entry, hz, offset, SearchFixture.TransmissionPower(Rate, entry, hz)));
        }

        // The noise level is set from the first transmission's power at the commanded rung; the three
        // are the same waveform at the same amplitude, so the other two land within rounding of it.
        // Each row then reports its OWN delivered ratio, computed after the fact, rather than the
        // commanded one - the same distinction the ladder makes between requested and delivered.
        var sigma = SignalToNoise.NoiseAmplitudeFor(placed[0].Power, RungDecibels, Rate);
        var mixed = SearchFixture.AddNoise(slot, new GaussianNoise(NoiseSeed), sigma, out var noisePower);

        var wavPath = Path.Combine(Path.GetTempPath(), $"ft8-example-244-{Guid.NewGuid():N}.wav");
        byte[] wav;
        try
        {
            WavFile.Write(wavPath, mixed, Rate);
            wav = File.ReadAllBytes(wavPath);
        }
        finally
        {
            File.Delete(wavPath);
        }

        var rows = placed
            .Select(p => new Ft8FixtureRow(
                SignalToNoise.DecibelsFor(p.Power, noisePower, Rate),
                (double)p.Offset / Rate,
                p.Hz,
                ReferenceRecording.Normalise(Ft8MessageDecoder.Decode(p.Entry.Message).Text)))
            .ToArray();

        var fixture = new Ft8CaptureFixture(
            Ft8CaptureFixture.CurrentFormat,
            CaptureName,
            Utc,
            Ft8CaptureFixture.HashOfBytes(wav),
            Rate,
            Ft8CaptureFixture.ProvenanceExample,
            string.Format(
                CultureInfo.InvariantCulture,
                "Hamlet unit 244, Ft8ExampleFixture, three synthesised transmissions at a commanded "
                + "{0:F1} dB with noise seed {1}",
                RungDecibels,
                NoiseSeed),
            rows,
            Path.Combine(Directory, FixtureName));

        return (wav, fixture);
    }

    /// <summary>Where the committed pair lives.</summary>
    internal static string Directory =>
        Path.Combine(Ft8CaptureFixtures.Root, Ft8CaptureFixtures.ExampleFolder);

    /// <summary>The committed capture.</summary>
    internal static string CommittedCapturePath => Path.Combine(Directory, CaptureName);

    /// <summary>The committed fixture.</summary>
    internal static string CommittedFixturePath => Path.Combine(Directory, FixtureName);

    /// <summary>The exact text of the committed fixture, so writing it and checking it cannot differ.</summary>
    internal static string FileText(Ft8CaptureFixture fixture) => fixture.ToFileText(Preamble);
}
