using Hamlet.App.Controls;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Training;

namespace Hamlet.App.ViewModels;

/// <summary>One "hear it" button on a field-guide card.</summary>
/// <param name="Label">Button text, e.g. "12 WPM" or "Mistuned".</param>
/// <param name="Request">What to generate and play.</param>
public sealed record ModeSampleButton(string Label, AudioSampleRequest Request);

/// <summary>
/// A field-guide card: the mode's story, its animated fingerprint, and the
/// samples a newcomer can listen to.
/// </summary>
/// <remarks>
/// <para>The buttons are per-mode rather than one generic "play", because the
/// useful comparison differs by mode (HM-DEC-027). CW gets three speeds,
/// which is how somebody finds the speed they can actually copy and the
/// groundwork FG-002 needs. SSB gets tuned and mistuned side by side, because
/// hearing those two back to back is the fastest way to learn what the tuning
/// knob is for — the field guide already promises "duck talk until tuned",
/// and this is that sentence made audible.</para>
/// </remarks>
public sealed class ModeCardViewModel
{
    /// <summary>Wraps a field-guide entry.</summary>
    /// <param name="mode">The entry.</param>
    public ModeCardViewModel(ModeInfo mode)
    {
        Mode = mode;
        Samples = BuildSamples(mode);
    }

    /// <summary>The field-guide entry.</summary>
    public ModeInfo Mode { get; }

    /// <summary>Samples offered for this mode.</summary>
    public IReadOnlyList<ModeSampleButton> Samples { get; }

    /// <summary>True when there is anything to listen to.</summary>
    public bool HasSamples => Samples.Count > 0;

    /// <summary>
    /// The mode family's colors, from the one palette every surface reads
    /// (HM-DEC-032).
    /// </summary>
    /// <remarks>
    /// This is what makes the map's legend worth reading. Somebody who learns
    /// "lavender means digital" from the band map finds the same lavender here
    /// against FT8 and RTTY, and the color stops being decoration on both
    /// surfaces at once.
    /// </remarks>
    public ModeColors Colors => ModePalette.For(Mode.Family);

    /// <summary>
    /// The family named in words — "Morse", "Digital", "Voice".
    /// </summary>
    /// <remarks>
    /// The second carrier. A chip that is only a color tells somebody with a
    /// color vision deficiency nothing at all (§0.6).
    /// </remarks>
    public string FamilyLabel => Colors.Label;

    private static IReadOnlyList<ModeSampleButton> BuildSamples(ModeInfo mode)
    {
        var training = ModeFingerprintControl.ModeFor(mode.Signature);

        return training switch
        {
            TrainingMode.Cw => new[]
            {
                new ModeSampleButton("12 WPM", new AudioSampleRequest(TrainingMode.Cw, 12)),
                new ModeSampleButton("18 WPM", new AudioSampleRequest(TrainingMode.Cw, 18)),
                new ModeSampleButton("25 WPM", new AudioSampleRequest(TrainingMode.Cw, 25)),
            },
            TrainingMode.Ssb => new[]
            {
                new ModeSampleButton("Tuned", new AudioSampleRequest(TrainingMode.Ssb)),
                new ModeSampleButton(
                    "Mistuned", new AudioSampleRequest(TrainingMode.Ssb, Mistuned: true)),
            },
            _ => new[]
            {
                new ModeSampleButton("Hear it", new AudioSampleRequest(training)),
            },
        };
    }
}
