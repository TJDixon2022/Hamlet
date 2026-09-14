using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Hamlet.App.Licensing;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Licensing;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 355 task 4: **the tests stay off the network.**
/// </summary>
/// <remarks>
/// <para>**WHY.** Unit 354 found the plain fixture's view model asking callook.info for KC3QIS's license
/// class at construction, and the answer changed what the layout tests measured: *General* landed or
/// not depending on the network. A test that depends on a web lookup is not a test.</para>
/// <para>**THE SEAM IS <see cref="ICallsignLookup"/>**, which the resolver already took: the live
/// callook.info client in the application (<see cref="LiveCallookLookup"/>), the network denied by
/// default in this test assembly, and a fixed *General* for KC3QIS handed in by both layout fixtures.
/// **Each test checks the default first**, so a red here fails before any view model can reach out.</para>
/// <para>**WHAT IS NOT ASSERTED, AND WHY.** The instruction's *no HTTP client is created under test*
/// does not hold and is not made to: <c>MainWindowViewModel.BuildSources</c> constructs the POTA and SOTA
/// sources at construction, each with its own client, whether or not they are switched on. The layout
/// fixtures switch them off, so neither sends. Reported in unit 355's section 4.</para>
/// </remarks>
public sealed class TheTestsStayOffTheNetworkTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fixture.</summary>
    /// <param name="output">Where the numbers are printed.</param>
    public TheTestsStayOffTheNetworkTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **A view model a test builds without a lookup of its own has the network denied**, and its profile
    /// resolve goes there rather than to callook.info.
    /// </summary>
    [Fact]
    public void AViewModelATestBuildsHasTheNetworkDenied()
    {
        TheDefaultUnderTestIsTheNetworkDenied();

        var settings = Kc3qis(LicenseClass.Unknown);
        var denied = new NetworkDeniedLookup();
        var model = new MainWindowViewModel(settings, null);
        var handed = new MainWindowViewModel(Kc3qis(LicenseClass.Unknown), null, denied);

        _output.WriteLine("two-argument view model's lookup: " + model.LicenseLookupForTests.GetType().Name
            + "; handed a denied lookup, it was asked " + denied.Attempts + " time(s); class stays " + handed.LicenseClass);

        Assert.IsType<NetworkDeniedLookup>(model.LicenseLookupForTests);
        Assert.Equal(1, denied.Attempts);
        Assert.Equal(LicenseClass.Unknown, handed.LicenseClass);
    }

    /// <summary>**The plain fixture constructs with the seam's fixed answer: General for KC3QIS, asked once.**</summary>
    [AvaloniaFact]
    public void ThePlainFixtureTakesGeneralFromTheFixedAnswer()
    {
        TheDefaultUnderTestIsTheNetworkDenied();

        var window = TheWorkingPanelsTests.Realized(1100, 780, null);

        try
        {
            var model = (MainWindowViewModel)window.DataContext!;
            var lookup = Assert.IsType<FixedLicenseLookup>(model.LicenseLookupForTests);

            _output.WriteLine("plain fixture: lookup " + lookup.SourceName + ", asked " + lookup.Asked + "; class " + model.LicenseClass
                + "; privilege headline [" + model.PrivilegeStatus.Headline + "]");

            Assert.Equal(1, lookup.Asked);
            Assert.Equal(LicenseClass.General, model.LicenseClass);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>**The licensed fixture takes its lookup from the fixed answer too.**</summary>
    [AvaloniaFact]
    public void TheLicensedFixtureTakesTheFixedAnswerToo()
    {
        TheDefaultUnderTestIsTheNetworkDenied();

        var window = TheTopRowTests.Realized(1100, 780, null, null);

        try
        {
            var model = (MainWindowViewModel)window.DataContext!;

            _output.WriteLine("licensed fixture: lookup " + model.LicenseLookupForTests.GetType().Name + "; class " + model.LicenseClass);

            Assert.IsType<FixedLicenseLookup>(model.LicenseLookupForTests);
            Assert.Equal(LicenseClass.General, model.LicenseClass);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **Unit 354's layout reads the same numbers on two consecutive runs** - the plain fixture at 900 x 620
    /// and 1100 x 780 and the licensed one at 1400 x 1040, realized twice over, every drawn box compared.
    /// </summary>
    [AvaloniaFact]
    public void The354LayoutReadsTheSameNumbersTwiceRunning()
    {
        TheDefaultUnderTestIsTheNetworkDenied();

        var first = Read();
        var second = Read();

        foreach (var line in first)
        {
            _output.WriteLine(line);
        }

        Assert.Equal(first, second);
    }

    /// <summary>What each window of the comparison draws, one line each.</summary>
    private static List<string> Read()
    {
        var lines = new List<string>();

        foreach (var (name, build) in new (string, Func<Window>)[]
        {
            ("plain 900 x 620", () => TheWorkingPanelsTests.Realized(900, 620, null)),
            ("plain 1100 x 780", () => TheWorkingPanelsTests.Realized(1100, 780, null)),
            ("licensed 1400 x 1040", () => TheTopRowTests.Realized(1400, 1040, null, null)),
        })
        {
            var window = build();

            try
            {
                var model = (MainWindowViewModel)window.DataContext!;
                var m = TheTopRowTests.Measure(window);
                var stop = TheTopRowTests.RectIn(TheTopRowTests.Named<Button>(window, "DigitalStopButton"), window);

                lines.Add(name + ": class " + model.LicenseClass + "; top row " + Px(m.TopRowHeight) + "; card " + Box(m.Card) + "; rig " + Box(m.Rig)
                    + "; status bar " + Box(m.StatusBar) + "; Stop " + Box(stop) + "; panels "
                    + string.Join(" | ", TheWorkingPanelsTests.Panels(window).Select(p => p.Name + " " + Box(p.Rect))));
            }
            finally
            {
                window.Close();
            }
        }

        return lines;
    }

    /// <summary>**The first line of every test here**: this assembly's default lookup is the network denied.</summary>
    private static void TheDefaultUnderTestIsTheNetworkDenied()
        => Assert.IsType<NetworkDeniedLookup>(MainWindowViewModel.DefaultLicenseLookup());

    private static AppSettings Kc3qis(LicenseClass cls)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00";

        if (cls != LicenseClass.Unknown)
        {
            settings.Operator.LicenseClass = cls;
        }

        foreach (var source in TheTopRowTests.NetworkSources)
        {
            settings.SetSourceEnabled(source, false);
        }

        return settings;
    }

    private static string Px(double value)
        => value.ToString("0.#", CultureInfo.InvariantCulture);

    private static string Box(Rect r)
        => Px(r.X) + "," + Px(r.Y) + " " + Px(r.Width) + " x " + Px(r.Height);
}
