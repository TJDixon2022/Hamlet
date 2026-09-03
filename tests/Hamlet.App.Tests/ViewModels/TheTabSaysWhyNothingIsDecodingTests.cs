using Avalonia.Headless.XUnit;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// When nothing is decoding and something is wrong, the Digital tab says which
/// thing. When nothing is wrong it says nothing at all.
/// </summary>
/// <remarks>
/// <para>**THE STATE OF THE ART ON FT8 IS THAT FAILURES ARE ALMOST NEVER THE
/// DECODER.** They are clock drift, the wrong audio device, or the radio in the
/// wrong mode. Hamlet already knew all of those and said none of them in one
/// place, so an operator looking at an empty table could not tell a quiet band
/// from a wrong setup, and finding out by elimination is how a morning gets
/// spent.</para>
/// <para>**THE CONTROL IS THE TEST THAT MATTERS.** Five conditions right
/// produces no line, because a readiness line that always says something is one
/// the operator stops reading.</para>
/// <para>**NOTHING HERE OPENS A WINDOW, A SOUND CARD OR A SERIAL PORT.** Every
/// condition is driven at the view model's own seam: the spectrum source is
/// constructed in memory, the clock offset is a value, the rig state goes
/// through `ApplyRigState`, and the map is the real one for 20 m.</para>
/// </remarks>
public sealed class TheTabSaysWhyNothingIsDecodingTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each line is printed.</param>
    public TheTabSaysWhyNothingIsDecodingTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Where FT8 gathers on 40 m.</summary>
    /// <remarks>
    /// **40 m BECAUSE IT IS THE BAND A FRESH VIEW MODEL OPENS ON**, and
    /// `OnFrequencyHzChanged` clamps the dial to the selected band's map. A
    /// first draft of these tests used 14.074 without changing band, every
    /// frequency was silently clamped to the top of 40 m, and the control passed
    /// because the map had no block there rather than because the dial was in a
    /// digital one. `Ready` now asserts the dial landed where it was put, which
    /// is §12.5: a fixture built from the same misunderstanding as the code
    /// proves nothing.
    /// </remarks>
    private const long Ft8OnForty = 7_074_000;

    /// <summary>Well down in the Morse end of the same band.</summary>
    private const long MorseOnForty = 7_020_000;

    private static readonly DateTime Now =
        new(2026, 9, 2, 14, 20, 0, DateTimeKind.Utc);

    /// <summary>A clock that has been checked and found to match UTC.</summary>
    private static ClockOffset Measured => new(0.02, Now);

    /// <summary>
    /// **NOTHING IS LISTENING OUTRANKS EVERYTHING**, because every condition
    /// below it is a question about audio that does not exist.
    /// </summary>
    [AvaloniaFact]
    public void NothingListeningIsSaidFirstAndOutranksTheRest()
    {
        var model = Ready();

        // Wrong in three other ways as well, and none of them is what is said.
        model.DigitalSpectrum = null;
        model.ClockOffset = new ClockOffset(4.2, Now);
        model.FrequencyHz = MorseOnForty;

        Print(model);

        Assert.True(model.HasDigitalReadiness);
        Assert.StartsWith(
            "nothing is listening yet", model.DigitalReadinessLine,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// **THE TRAINING RADIO IS SAID BEFORE THE CLOCK**, because no amount of
    /// clock accuracy will put a real station into audio Hamlet made itself.
    /// </summary>
    [AvaloniaFact]
    public void ASimulatedSourceIsSaidBeforeTheClock()
    {
        var model = Ready();

        model.DigitalSpectrum?.Dispose();
        model.DigitalSpectrum = new AudioSpectrumSource(48000, simulated: true);
        model.ClockOffset = new ClockOffset(4.2, Now);

        Print(model);

        Assert.StartsWith(
            "this is the training radio", model.DigitalReadinessLine,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "clock", model.DigitalReadinessLine, StringComparison.Ordinal);
    }

    /// <summary>
    /// **AN OFFSET NOBODY HAS MEASURED MEANS NO SLOTS ARE CUT AT ALL**, which is
    /// the strongest true statement any of the five conditions can make.
    /// </summary>
    /// <remarks>
    /// `ClockIsConcerning` is false here, so this case is not one the
    /// instruction's own third condition would have caught. It is folded into
    /// the same slot because `Ft8SlotCutter` refuses to cut on an unmeasured
    /// offset rather than guessing where the minute falls, and a readiness line
    /// silent about that is silent about the case it exists for.
    /// </remarks>
    [AvaloniaFact]
    public void AnUncheckedClockSaysNothingIsBeingCutIntoSlots()
    {
        var model = Ready();

        model.ClockOffset = ClockOffset.Unknown;

        Print(model);

        Assert.False(model.ClockIsConcerning);
        Assert.StartsWith(
            "the clock has not been checked", model.DigitalReadinessLine,
            StringComparison.Ordinal);
        Assert.Contains(
            "nothing is being cut into slots", model.DigitalReadinessLine,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// **HOW FAR OUT, SPOKEN, AND NO CLAIM THIS PROGRAM CANNOT SUPPORT.**
    /// </summary>
    /// <remarks>
    /// **THE INSTRUCTION'S OWN EXAMPLE SENTENCE IS NOT WRITTEN**, and this
    /// asserts that it is not. `Ft8SlotCutter` cuts on the measured offset
    /// rather than on the machine's own minute, so Hamlet stays aligned to true
    /// UTC with a clock that is out, and telling the operator that nothing will
    /// decode until he fixes it would send him to repair the one thing that was
    /// already handled (§0.0).
    /// </remarks>
    [AvaloniaFact]
    public void AClockThatIsOutSaysHowFarAndDoesNotOverstateIt()
    {
        var model = Ready();

        model.ClockOffset = new ClockOffset(4.2, Now);

        Print(model);

        Assert.True(model.ClockIsConcerning);
        Assert.Contains(
            "about 4 seconds slow", model.DigitalReadinessLine,
            StringComparison.Ordinal);
        Assert.Contains(
            "inside about a second", model.DigitalReadinessLine,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "nothing will decode", model.DigitalReadinessLine,
            StringComparison.Ordinal);
    }

    /// <summary>**A CLOCK RUNNING AHEAD IS SAID AS FAST.**</summary>
    [AvaloniaFact]
    public void AClockRunningAheadIsSaidAsFast()
    {
        var model = Ready();

        model.ClockOffset = new ClockOffset(-1.4, Now);

        Print(model);

        Assert.Contains(
            "about a second and a half fast", model.DigitalReadinessLine,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// **AN UNCONFIRMED READ IS NOT A FAULT** (HM-DEC-056). A radio that has not
    /// said which mode it is in is reported as unknown rather than as wrong.
    /// </summary>
    [AvaloniaFact]
    public void AnUnreadModeIsSaidAsUnknownAndNotAsWrong()
    {
        var model = Ready();

        // Everything the radio might have said, unsaid.
        model.ApplyRigState(RigState.Empty);

        Print(model);

        Assert.Null(model.RigState.Mode);
        Assert.Null(model.RigState.DataVariant);
        Assert.Contains(
            "unknown rather than wrong", model.DigitalReadinessLine,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// **A RADIO IN MORSE IS NAMED**, in the radio's own word for it, rather
    /// than described as not being in a data mode.
    /// </summary>
    [AvaloniaFact]
    public void AModeThatIsNotTheUpperSidebandIsNamed()
    {
        var model = Ready();

        model.ApplyRigState(Mode(CivMode.Cw, dataVariant: false));

        Print(model);

        Assert.Contains(
            "the radio is in CW", model.DigitalReadinessLine,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// **PLAIN USB AND USB-D ARE DIFFERENT RADIOS** (work instruction 041), and
    /// the line says which one it is looking at.
    /// </summary>
    [AvaloniaFact]
    public void PlainUpperSidebandIsSaidAsNotTheDataSetting()
    {
        var model = Ready();

        model.ApplyRigState(Mode(CivMode.Usb, dataVariant: false));

        Print(model);

        Assert.Contains(
            "not on the data setting", model.DigitalReadinessLine,
            StringComparison.Ordinal);
        Assert.Contains(
            "USB-D", model.DigitalReadinessLine, StringComparison.Ordinal);
    }

    /// <summary>
    /// **THE MAP ALREADY KNOWS WHERE THE DIGITAL MODES GATHER**, and the line
    /// reads it rather than carrying a frequency of its own.
    /// </summary>
    [AvaloniaFact]
    public void ADialOutsideTheDigitalBlocksNamesWhereItIs()
    {
        var model = Ready();

        model.FrequencyHz = MorseOnForty;

        var here = model.Neighborhoods.First(n => n.Contains(MorseOnForty));
        _output.WriteLine($"  dial in [{here.Name}] family {here.Family}");
        _output.WriteLine($"  dial at {model.FrequencyHz} Hz, {model.Neighborhoods.Count} blocks");
        _output.WriteLine($"  mode {model.RigState.Mode} data {model.RigState.DataVariant}");
        _output.WriteLine($"  clock [{model.ClockOffsetLine}] listening {model.DigitalSpectrum is not null}");

        Print(model);

        Assert.NotEqual(ModeFamily.Digital, here.Family);
        Assert.Contains(
            here.Name, model.DigitalReadinessLine, StringComparison.Ordinal);
        Assert.Contains(
            "digital", model.DigitalReadinessLine, StringComparison.Ordinal);
    }

    /// <summary>
    /// **A FREQUENCY THE MAP HAS NO BLOCK FOR SAYS NOTHING** (HM-DEC-009). Not
    /// knowing where you are is not evidence that you are in the wrong place,
    /// and it is the same ruling that makes an unread mode unknown rather than
    /// wrong.
    /// </summary>
    [AvaloniaFact]
    public void ADialTheMapHasNoBlockForIsNotCalledWrong()
    {
        var model = Ready();

        model.Neighborhoods = Array.Empty<Neighborhood>();

        Print(model);

        Assert.Equal(DigitalReadiness.Nothing, model.DigitalReadinessLine);
    }

    /// <summary>
    /// **THE CONTROL, AND IT IS THE ONE THAT MATTERS.** Five conditions right
    /// produces no line at all, because a quiet band is not a fault and the
    /// decoded panel's own idle line already covers it.
    /// </summary>
    [AvaloniaFact]
    public void AllFiveRightProducesNoLineAtAll()
    {
        var model = Ready();

        var here = model.Neighborhoods.First(n => n.Contains(model.FrequencyHz));

        _output.WriteLine($"  listening   {model.DigitalSpectrum is not null}");
        _output.WriteLine($"  simulated   {model.DigitalSpectrum?.IsSimulated}");
        _output.WriteLine($"  clock       [{model.ClockOffsetLine}]");
        _output.WriteLine($"  mode        {model.RigState.Mode} data {model.RigState.DataVariant}");
        _output.WriteLine($"  dial        {model.FrequencyHz} Hz, in [{here.Name}] family {here.Family}");

        Print(model);

        // **THE CONTROL HAS TO PASS FOR THE RIGHT REASON** (§12.5). Every one of
        // the five is checked here as a value before the line is asked, because
        // a fixture that is quietly wrong in some sixth way would produce the
        // same empty string and prove nothing.
        Assert.NotNull(model.DigitalSpectrum);
        Assert.False(model.DigitalSpectrum!.IsSimulated);
        Assert.True(model.ClockOffset.IsKnown);
        Assert.False(model.ClockIsConcerning);
        Assert.Equal(CivMode.Usb, model.RigState.Mode);
        Assert.True(model.RigState.DataVariant);
        Assert.Equal(ModeFamily.Digital, here.Family);

        Assert.Equal(DigitalReadiness.Nothing, model.DigitalReadinessLine);
        Assert.False(model.HasDigitalReadiness);

        // **AND THE TABLE UNDERNEATH IS STILL EMPTY**, which is exactly the
        // state this control is about: nothing decoded, nothing wrong, nothing
        // said beyond the panel's own idle line.
        Assert.Empty(model.DigitalDecodes);
        Assert.Equal(DigitalIdleText.Decoded, model.DigitalDecodedIdle);
    }

    /// <summary>
    /// **WHAT THE LINE SAYS ON THIS MACHINE RIGHT NOW**, on a view model built
    /// the way the application builds one, before anything has been started.
    /// </summary>
    [AvaloniaFact]
    public void AFreshViewModelSaysNothingIsListening()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        _output.WriteLine($"  neighborhoods {model.Neighborhoods.Count}");
        _output.WriteLine($"  dial          {model.FrequencyHz} Hz");
        Print(model);

        Assert.True(model.HasDigitalReadiness);
        Assert.StartsWith(
            "nothing is listening yet", model.DigitalReadinessLine,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// **THE STRIP IS BOUND AND THE WHOLE ROW GOES WHEN THERE IS NOTHING TO
    /// SAY.** Read as text, because a property nothing binds is a property the
    /// operator never sees, and that is the failure every panel on this tab has
    /// had at least once.
    /// </summary>
    [AvaloniaFact]
    public void TheStripIsOnTheTabAndIsHiddenByItsOwnEmptiness()
    {
        var markup = DigitalWorkspaceMarkup();

        foreach (var binding in new[]
        {
            "x:Name=\"DigitalReadinessStrip\"",
            "{Binding DigitalReadinessLine}",
            "IsVisible=\"{Binding HasDigitalReadiness}\"",
        })
        {
            _output.WriteLine(
                $"  {binding,-46} "
                + $"{(markup.Contains(binding, StringComparison.Ordinal) ? "there" : "MISSING")}");

            Assert.Contains(binding, markup, StringComparison.Ordinal);
        }

        // **AND IT DID NOT DISPLACE THE IDLE LINES** (HM-DEC-021). They are the
        // owner's own words and this unit adds to them rather than replacing
        // them.
        Assert.Contains(
            "{Binding DigitalDecodedIdle}", markup, StringComparison.Ordinal);
        Assert.Contains(
            "{Binding DigitalSayingIdle}", markup, StringComparison.Ordinal);
    }

    /// <summary>A view model with all five conditions right.</summary>
    /// <returns>The model, ready for one thing to be broken in it.</returns>
    private static MainWindowViewModel Ready()
    {
        var model = new MainWindowViewModel(new AppSettings(), null)
        {
            DigitalSpectrum = new AudioSpectrumSource(48000, simulated: false),
            ClockOffset = Measured,
        };

        model.Neighborhoods = NeighborhoodPlan.WithEdges(model.SelectedBand.Band);
        model.FrequencyHz = Ft8OnForty;
        model.ApplyRigState(Mode(CivMode.Usb, dataVariant: true));

        // **THE DIAL LANDED WHERE IT WAS PUT.** The setter clamps to the
        // selected band, so a fixture that never checked this would be asking
        // its questions at whatever frequency the clamp chose.
        Assert.Equal(Ft8OnForty, model.FrequencyHz);

        return model;
    }

    /// <summary>A rig state carrying one mode and its data flag.</summary>
    /// <param name="mode">The mode the radio reports.</param>
    /// <param name="dataVariant">Whether the data flag is on.</param>
    /// <returns>The state.</returns>
    private static RigState Mode(CivMode mode, bool dataVariant)
        => RigState.Empty.With(new[]
        {
            RigValue.Known(
                RigField.Mode, (double)mode, CivValues.Name(mode),
                Now, "CI-V 04"),
            RigValue.Known(
                RigField.DataMode, dataVariant ? 1 : 0,
                dataVariant ? "on" : "off", Now, "CI-V 1A 06"),
        });

    /// <summary>Print the line under test, whatever it turns out to be.</summary>
    /// <param name="model">The view model.</param>
    private void Print(MainWindowViewModel model)
        => _output.WriteLine(
            model.DigitalReadinessLine.Length == 0
                ? "  line [] (nothing said)"
                : $"  line [{model.DigitalReadinessLine}]");

    /// <summary>The whole Digital workspace's markup.</summary>
    private static string DigitalWorkspaceMarkup()
    {
        var markup = File.ReadAllText(
            Path.Combine(Root(), "src", "Hamlet.App", "Views", "MainWindow.axaml"));

        var from = markup.IndexOf(
            "x:Name=\"DigitalWorkspace\"", StringComparison.Ordinal);
        var to = markup.IndexOf(
            "x:Name=\"VoiceWorkspace\"", StringComparison.Ordinal);

        Assert.InRange(from, 0, int.MaxValue);
        Assert.InRange(to, from, int.MaxValue);

        return markup[from..to];
    }

    /// <summary>The repository root, walking up from the test binary.</summary>
    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
