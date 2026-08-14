using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// What Hamlet may say about the radio's settings, and the much longer list of
/// what it may not (HM-DEC-050).
/// </summary>
public sealed class RigObservationTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);

    private static RigState With(params (RigField Field, double Number, string Text)[] values)
        => RigState.Empty.With(
            values.Select(v => RigValue.Known(v.Field, v.Number, v.Text, Now, "test")));

    /// <summary>
    /// Every state worth putting an observation in front of, for the sweep.
    /// </summary>
    public static TheoryData<string> Situations()
    {
        var data = new TheoryData<string>();

        foreach (var name in Cases.Keys)
        {
            data.Add(name);
        }

        return data;
    }

    private static readonly Dictionary<string, RigState> Cases = new()
    {
        ["wide filter in CW"] = With(
            (RigField.Mode, (int)CivMode.Cw, "CW"),
            (RigField.FilterBandwidth, 3000, "3 kHz")),

        ["noise blanker on"] = With((RigField.NoiseBlanker, 1, "on")),

        ["attenuator and preamp"] = With(
            (RigField.Attenuator, 20, "20 dB"),
            (RigField.Preamp, 1, "preamp 1")),

        ["squelch shut"] = With(
            (RigField.SquelchStatus, 0, "closed"),
            (RigField.Squelch, 40, "40%")),

        ["IF instead of audio"] = With((RigField.AccUsbOutputSelect, 1, "IF")),

        ["everything at once"] = With(
            (RigField.Mode, (int)CivMode.Cw, "CW"),
            (RigField.FilterBandwidth, 3600, "3.6 kHz"),
            (RigField.NoiseBlanker, 1, "on"),
            (RigField.Attenuator, 20, "20 dB"),
            (RigField.Preamp, 2, "preamp 2"),
            (RigField.SquelchStatus, 0, "closed"),
            (RigField.AccUsbOutputSelect, 1, "IF")),
    };

    /// <remarks>
    /// THE ONE THAT WOULD HAVE ENDED THE HALF HOUR. The filter was wide open and
    /// nobody could see it. Hamlet may say so, because it read the number and it
    /// understands the mechanism.
    /// </remarks>
    [Fact]
    public void AWideFilterInMorseIsWorthSayingOutLoud()
    {
        var said = RigObservations.For(Cases["wide filter in CW"]);

        var line = Assert.Single(said);
        Assert.Contains("3 kHz", line, StringComparison.Ordinal);
        Assert.Contains("decoder", line, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves a narrow filter in Morse produces nothing. An observation that
    /// fired whatever the setting would be noise, and the operator would learn
    /// to skip past it.
    /// </remarks>
    [Fact]
    public void ANarrowFilterInMorseIsNotWorthMentioning()
    {
        var narrow = With(
            (RigField.Mode, (int)CivMode.Cw, "CW"),
            (RigField.FilterBandwidth, 500, "500 Hz"));

        Assert.Empty(RigObservations.For(narrow));
    }

    /// <remarks>
    /// Proves the same wide filter says nothing in a voice mode, where two and a
    /// half kilohertz is simply what a voice needs. The observation is about a
    /// mismatch between two readings and not about a number being large.
    /// </remarks>
    [Fact]
    public void AWideFilterInAVoiceModeIsJustAVoiceFilter()
    {
        var voice = With(
            (RigField.Mode, (int)CivMode.Usb, "USB"),
            (RigField.FilterBandwidth, 2400, "2.4 kHz"));

        Assert.Empty(RigObservations.For(voice));
    }

    /// <remarks>
    /// NOTHING IS SAID FROM A VALUE THAT WAS NOT READ (§0.0). An observation
    /// resting on an assumed setting would be a confident guess wearing the
    /// clothes of helpfulness, and this is the subsystem where that would be
    /// easiest to excuse.
    /// </remarks>
    [Fact]
    public void NothingIsSaidAboutSettingsNobodyHasRead()
    {
        Assert.Empty(RigObservations.For(RigState.Empty));

        // A mode with no width read, and a width with no mode read: neither is
        // enough on its own.
        Assert.Empty(RigObservations.For(With((RigField.Mode, (int)CivMode.Cw, "CW"))));
        Assert.Empty(RigObservations.For(With((RigField.FilterBandwidth, 3000, "3 kHz"))));
    }

    /// <remarks>
    /// THE HONESTY SWEEP. Every observation Hamlet can produce, checked against
    /// the phrases that would turn a statement about a value it read into an
    /// instruction about somebody's radio. This session reads and does not
    /// write, and the copy has to hold that line as firmly as the code does.
    /// </remarks>
    [Theory]
    [MemberData(nameof(Situations))]
    public void NoObservationEverTellsTheOperatorWhatToChange(string situation)
    {
        var banned = new[]
        {
            // Instructions.
            "you should", "you need to", "you must", "try ", "turn it", "turn the",
            "turn down", "turn up", "set it", "set the", "change the", "switch it",
            "narrow the", "widen the", "adjust", "fix ", "correct the", "make sure",
            "recommend", "suggest", "consider ",

            // Fault, which a setting somebody chose on purpose is not.
            "wrong", "incorrect", "mistake", "problem with", "fault", "broken",
            "misconfigured", "badly", "too high", "too low", "should be",

            // Claims about the world outside the numbers Hamlet read.
            "conditions are", "propagation", "the ionosphere", "band is dead",
            "your antenna", "the antenna",
        };

        var offenders = new List<string>();

        foreach (var line in RigObservations.For(Cases[situation]))
        {
            foreach (var phrase in banned)
            {
                if (line.Contains(phrase, StringComparison.OrdinalIgnoreCase))
                {
                    offenders.Add($"\"{phrase}\" in: {line}");
                }
            }
        }

        Assert.Empty(offenders);
    }

    /// <remarks>
    /// Proves the sweep is reading something. A banned-phrase test over an empty
    /// set passes forever and proves nothing, which is the failure mode of every
    /// test shaped like this one.
    /// </remarks>
    [Fact]
    public void TheSweepIsActuallyReadingObservations()
    {
        var all = Cases.Values.SelectMany(RigObservations.For).ToList();

        Assert.True(all.Count >= 5, $"only {all.Count} observations exist");
        Assert.All(all, line => Assert.True(line.Length > 80, $"too short: {line}"));

        // And it can see a phrase it would have to reject.
        Assert.Contains("try ", "you could try narrowing it", StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the observations are written in the project voice (§0.7,
    /// HM-DEC-040): connected speech rather than a stack of clipped facts, at
    /// most one em dash in a passage, and no shouting.
    /// </remarks>
    [Fact]
    public void TheObservationsAreWrittenInTheProjectVoice()
    {
        foreach (var line in Cases.Values.SelectMany(RigObservations.For).Distinct())
        {
            Assert.True(line.Count(c => c == '—') <= 1, $"two em dashes: {line}");
            Assert.True(line.Count(c => c == '.') >= 2, $"a single clipped fact: {line}");
            Assert.DoesNotContain("!", line, StringComparison.Ordinal);
        }
    }

    /// <remarks>
    /// Proves several observations can stand together without repeating
    /// themselves, since a badly set-up radio produces more than one.
    /// </remarks>
    [Fact]
    public void SeveralObservationsCanStandTogether()
    {
        var said = RigObservations.For(Cases["everything at once"]);

        Assert.Equal(5, said.Count);
        Assert.Equal(said.Count, said.Distinct().Count());
    }
}
