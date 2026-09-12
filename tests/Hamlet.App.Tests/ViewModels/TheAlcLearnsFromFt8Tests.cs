using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 325 task 6, `PHASE_PLAN.md` §R15: **the ALC reference is
/// learned from FT8 rather than read off a meter by the operator.**
/// </summary>
/// <remarks>
/// <para>**THE NUMBER THAT KEPT STEP 4 PARTIAL.** Unit 323 judged the meter
/// against 128, which it had got by halving the range of a different meter - the
/// transmit power *setting* - and unit 324 removed the invention and left the
/// reading unjudged, because the manual gives this meter's scale, 0 to 120, and
/// does not say where its zone ends on it. Tim ruled on 2026-09-11 that neither
/// an invented figure nor a question to the operator is wanted: **FT8 already
/// transmits cleanly through this radio**, so what a good send reads here is
/// something Hamlet can measure.</para>
/// <para>**THE MARGIN IS 15, ONE EIGHTH OF THE SCALE**, stated by this unit and
/// Tim's to overrule, with the argument for it written on
/// `MainWindowViewModel.Psk31AlcMargin`.</para>
/// <para>**NO RADIO ON THIS MACHINE** (FACT-006, FACT-004). There is no `CivRead`
/// caller that fills <see cref="RigField.Alc"/> today, so every reading here is
/// handed in through the same seam unit 324 built. **Nothing is keyed, nothing is
/// opened and no sound is made.**</para>
/// </remarks>
public sealed class TheAlcLearnsFromFt8Tests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every reading and verdict is printed.</param>
    public TheAlcLearnsFromFt8Tests(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-alc-learn-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
    }

    /// <summary>Removes the telemetry folder.</summary>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch (IOException)
        {
            // A left-over temp folder is not a test failure.
        }
    }

    /// <summary>**A fed FT8 send at 70 sets the reference to 70, with provenance.**</summary>
    [Fact]
    public void AnFt8SendAtSeventySetsTheReferenceWithItsProvenance()
    {
        var lines = Run(model =>
        {
            Assert.Null(model.Psk31AlcReference);
            Assert.False(model.HasPsk31AlcReference);

            model.LearnTheAlcForTests(Reading(70), "FT8");

            var reference = model.Psk31AlcReference;

            _output.WriteLine("reference : " + reference);
            _output.WriteLine("line      : " + model.Psk31AlcReferenceLine);

            Assert.NotNull(reference);
            Assert.Equal(70, reference!.Reading);
            Assert.Equal("FT8", reference.Mode);
            Assert.Contains("learned from FT8 send at", reference.Provenance);
            Assert.True(model.HasPsk31AlcReference);
        });

        var learned = Events(lines, "psk31_alc_reference").Single().GetProperty("data");

        Assert.Equal(70, learned.GetProperty("alc").GetDouble());
        Assert.Equal("FT8", learned.GetProperty("mode").GetString());
        Assert.Equal(CivAlc.FullScale, learned.GetProperty("scaleTop").GetDouble());
        Assert.Equal(15, learned.GetProperty("margin").GetDouble());
        Assert.NotEqual(
            JsonValueKind.Null, learned.GetProperty("takenUtc").ValueKind);
    }

    /// <summary>**A PSK31 send at 80 against a reference of 70 is not judged hot.**</summary>
    /// <remarks>
    /// **70 PLUS THE MARGIN OF 15 IS 85**, so 80 is inside it. The sentence still
    /// prints both numbers, because a verdict with no measurement behind it is
    /// what this whole path exists to avoid.
    /// </remarks>
    [Fact]
    public void AtEightyAgainstSeventyItIsNotHot()
    {
        var lines = Run(model =>
        {
            model.LearnTheAlcForTests(Reading(70), "FT8");

            model.Psk31AlcForTests = Reading(80);
            model.ReadTheAlcForTests();

            _output.WriteLine("80 vs 70+15 : " + model.Psk31AlcLine);

            Assert.True(model.HasPsk31AlcLine);
            Assert.DoesNotContain("driven harder", model.Psk31AlcLine, StringComparison.Ordinal);
            Assert.Contains("80", model.Psk31AlcLine, StringComparison.Ordinal);
            Assert.Contains("70", model.Psk31AlcLine, StringComparison.Ordinal);
        });

        var data = Events(lines, "psk31_send_alc").Single().GetProperty("data");

        Assert.True(data.GetProperty("judged").GetBoolean());
        Assert.False(data.GetProperty("pastTheZone").GetBoolean());
        Assert.Equal(80, data.GetProperty("alc").GetDouble());
        Assert.Equal(70, data.GetProperty("reference").GetDouble());
        Assert.Equal(15, data.GetProperty("margin").GetDouble());
        Assert.Equal(85, data.GetProperty("zone").GetDouble());
        Assert.Equal("FT8", data.GetProperty("referenceMode").GetString());
    }

    /// <summary>**At 90 it is hot, and the sentence and the event fire.**</summary>
    [Fact]
    public void AtNinetyAgainstSeventyItIsHotAndSaysSo()
    {
        var lines = Run(model =>
        {
            model.LearnTheAlcForTests(Reading(70), "FT8");

            model.Psk31AlcForTests = Reading(90);
            model.ReadTheAlcForTests();

            _output.WriteLine("90 vs 70+15 : " + model.Psk31AlcLine);

            Assert.Contains(
                "driven harder", model.Psk31AlcLine, StringComparison.Ordinal);

            // **§R11's OWN ACTION, AND ONLY THAT ONE.** Hamlet asks him to turn the
            // drive down; it does not do it for him.
            Assert.Contains(
                "turn the transmit drive above down one step",
                model.Psk31AlcLine,
                StringComparison.OrdinalIgnoreCase);
        });

        var alc = Events(lines, "psk31_send_alc").Single();
        var data = alc.GetProperty("data");

        _output.WriteLine(alc.ToString());

        Assert.True(data.GetProperty("judged").GetBoolean());
        Assert.True(data.GetProperty("pastTheZone").GetBoolean());
        Assert.Equal(90, data.GetProperty("alc").GetDouble());
        Assert.Equal(70, data.GetProperty("reference").GetDouble());
        Assert.Equal(15, data.GetProperty("margin").GetDouble());

        // A verdict against the operator's signal is a warning and is filed as one.
        Assert.Equal("warn", alc.GetProperty("level").GetString());
    }

    /// <summary>
    /// **With no FT8 send observed, a PSK31 send at 120 is reported and not judged.**
    /// </summary>
    /// <remarks>
    /// **THIS IS THE ONE THAT MATTERS MOST** (§0.0, §R15: *Hamlet never invents
    /// one*). 120 is the top of the meter, so if anything were ever going to be
    /// judged hot without a reference it would be this. It is reported and nothing
    /// is claimed about it.
    /// </remarks>
    [Fact]
    public void WithNoReferenceEvenTheTopOfTheScaleIsNotJudged()
    {
        var lines = Run(model =>
        {
            Assert.Null(model.Psk31AlcReference);

            model.Psk31AlcForTests = Reading(120);
            model.ReadTheAlcForTests();

            _output.WriteLine("120, no reference : " + model.Psk31AlcLine);

            Assert.True(model.HasPsk31AlcLine);
            Assert.Contains("120", model.Psk31AlcLine, StringComparison.Ordinal);
            Assert.DoesNotContain(
                "driven harder", model.Psk31AlcLine, StringComparison.Ordinal);

            // **AND IT SAYS WHAT WOULD MAKE IT JUDGE**, so the silence is a state
            // he can change rather than a limitation he has to discover.
            Assert.Contains("FT8", model.Psk31AlcLine, StringComparison.Ordinal);
        });

        var data = Events(lines, "psk31_send_alc").Single().GetProperty("data");

        Assert.True(data.GetProperty("measured").GetBoolean());
        Assert.Equal(120, data.GetProperty("alc").GetDouble());
        Assert.False(data.GetProperty("judged").GetBoolean());
        Assert.False(data.GetProperty("pastTheZone").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("zone").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("reference").ValueKind);

        // Nothing was learned, so nothing was written down as learned.
        Assert.Empty(Events(lines, "psk31_alc_reference"));
    }

    /// <summary>**The reference carries its age** (HM-DEC-111).</summary>
    [Fact]
    public void TheReferenceCarriesItsAge()
    {
        Run(model =>
        {
            var taken = DateTime.UtcNow.AddMinutes(-4);

            model.LearnTheAlcForTests(Reading(70, taken), "FT4");

            var reference = model.Psk31AlcReference!;

            _output.WriteLine("line : " + reference.Line(DateTime.UtcNow));

            Assert.Equal(taken, reference.TakenUtc);
            Assert.Contains("FT4", reference.Provenance, StringComparison.Ordinal);
            Assert.Contains(
                "4 minutes ago",
                reference.Line(DateTime.UtcNow),
                StringComparison.Ordinal);

            // Seconds under the minute, because the poll's cadence supports that
            // and nothing finer.
            Assert.Contains(
                "12 s ago",
                reference.Line(taken.AddSeconds(12)),
                StringComparison.Ordinal);
        });
    }

    /// <summary>**The highest reading wins, and a lower one never lowers it.**</summary>
    /// <remarks>
    /// **THE QUESTION IS *WHAT DOES THIS RADIO READ WHEN IT IS HAPPY*.** A
    /// reference that fell to the quietest send of the evening would call every
    /// ordinary one hot, which is the cry-wolf failure the margin also guards.
    /// </remarks>
    [Fact]
    public void TheHighestReadingIsTheReference()
    {
        Run(model =>
        {
            model.LearnTheAlcForTests(Reading(70), "FT8");
            model.LearnTheAlcForTests(Reading(55), "FT8");

            Assert.Equal(70, model.Psk31AlcReference!.Reading);

            model.LearnTheAlcForTests(Reading(82), "FT4");

            _output.WriteLine("after 70, 55, 82 : " + model.Psk31AlcReference);

            Assert.Equal(82, model.Psk31AlcReference!.Reading);
            Assert.Equal("FT4", model.Psk31AlcReference.Mode);
        });
    }

    /// <summary>**An unknown reading teaches nothing and claims nothing.**</summary>
    /// <remarks>
    /// **THIS IS THIS MACHINE'S ACTUAL STATE** (FACT-006). There is no radio here,
    /// so the poll holds nothing; an FT8 send on this machine learns no reference
    /// and writes no line saying it did.
    /// </remarks>
    [Fact]
    public void AnUnknownReadingLearnsNothing()
    {
        var lines = Run(model =>
        {
            model.LearnTheAlcForTests(null, "FT8");
            model.LearnTheAlcForTests(
                RigValue.Unknown(RigField.Alc, "no reply"), "FT8");

            _output.WriteLine("after two unknowns : " + model.Psk31AlcReferenceLine);

            Assert.Null(model.Psk31AlcReference);
            Assert.False(model.HasPsk31AlcReference);

            // The line is never blank: a missing line and a line saying all is
            // well are the same picture and only one of them is true.
            Assert.NotEmpty(model.Psk31AlcReferenceLine);
            Assert.Contains(
                "no reference", model.Psk31AlcReferenceLine, StringComparison.Ordinal);
        });

        Assert.Empty(Events(lines, "psk31_alc_reference"));
    }

    /// <summary>**The margin is one eighth of the scale and is derived from it.**</summary>
    /// <remarks>
    /// **WRITTEN AS A FRACTION RATHER THAN AS 15** (§0's generate-from-a-source-of-
    /// truth rule). A typed 15 would quietly become a different fraction of a
    /// different range the day the scale moved, which is exactly how unit 323's
    /// 128 came about.
    /// </remarks>
    [Fact]
    public void TheMarginIsOneEighthOfTheScale()
    {
        _output.WriteLine(
            "scale " + MainWindowViewModel.Psk31AlcScaleTop
            + ", margin " + MainWindowViewModel.Psk31AlcMargin);

        Assert.Equal(CivAlc.FullScale, MainWindowViewModel.Psk31AlcScaleTop);
        Assert.Equal(15, MainWindowViewModel.Psk31AlcMargin);
        Assert.Equal(
            MainWindowViewModel.Psk31AlcScaleTop / 8,
            MainWindowViewModel.Psk31AlcMargin);
    }

    private static RigValue Reading(double level, DateTime? atUtc = null)
        => RigValue.Known(
            RigField.Alc,
            level,
            CivAlc.Describe((int)level),
            atUtc ?? DateTime.UtcNow,
            "15 13");

    private List<string> Run(Action<MainWindowViewModel> what)
    {
        using (var telemetry = new JsonlTelemetry(_folder, "325", _ => true))
        {
            var settings = new AppSettings { ReconnectOnStartup = false };

            settings.Operator.Callsign = "KC3QIS";
            settings.Operator.GridSquare = "FN00DJ";

            var model = new MainWindowViewModel(settings, telemetry)
            {
                OperatingMode = "Digital",
                TapForTests = new AudioTap(),
                ClockOffset = new ClockOffset(0.033, DateTime.UtcNow),
            };

            model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

            what(model);
        }

        return Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines).ToList();
    }

    private static List<JsonElement> Events(IEnumerable<string> lines, string name)
        => lines
            .Where(l => l.Contains("\"" + name + "\"", StringComparison.Ordinal))
            .Select(l => JsonDocument.Parse(l).RootElement)
            .Where(e => e.GetProperty("event").GetString() == name)
            .ToList();
}
