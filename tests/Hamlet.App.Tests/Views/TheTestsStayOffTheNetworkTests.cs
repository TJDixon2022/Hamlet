using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Hamlet.RadioEngine.Explore;
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
/// <para>**AND NO CLIENT IS CREATED UNDER TEST, WHICH UNIT 355 REPORTED DID NOT HOLD.** That unit
/// found <c>MainWindowViewModel.BuildSources</c> constructing the POTA and SOTA sources at
/// construction, each with its own <c>HttpClient</c>, whether or not they were switched on; the
/// fixtures switched them off so nothing was ever sent, but the criterion was *no client is created*
/// and it was not met. **Unit 356 task 3 moved both behind the same seam the license lookup took**:
/// the ingredients are kept and the client is made on the first fetch.</para>
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

    /// <summary>**Building a view model makes no HTTP client, and fetching makes one.**</summary>
    /// <remarks>
    /// **BOTH HALVES, BECAUSE EITHER ALONE IS EASY TO PASS.** A source that never made a client
    /// would satisfy the first and be broken; one that made it eagerly satisfies the second and is
    /// the fault. What is asserted is the order: **none at construction, one on the first fetch.**
    /// </remarks>
    [Fact]
    public async Task BuildingAViewModelMakesNoHttpClient()
    {
        TheDefaultUnderTestIsTheNetworkDenied();

        var model = new MainWindowViewModel(Kc3qis(LicenseClass.General), null);

        var pota = model.OwnedSourcesForTests.OfType<PotaActivitySource>().Single();
        var sota = model.OwnedSourcesForTests.OfType<SotaActivitySource>().Single();

        _output.WriteLine(
            "after construction: POTA client made " + pota.ClientMadeForTests
            + ", SOTA client made " + sota.ClientMadeForTests);

        Assert.False(pota.ClientMadeForTests, "POTA made an HTTP client at construction");
        Assert.False(sota.ClientMadeForTests, "SOTA made an HTTP client at construction");

        // **AND THE APPLICATION STILL FETCHES.** A handed-in transport answers, so nothing
        // leaves this machine, and the client is made because the fetch asked for it.
        using var handler = new AnswersWithNothing();
        using var source = new PotaActivitySource("356", "KC3QIS", handler);

        Assert.False(source.ClientMadeForTests, "a source made a client before it was asked");

        _ = await source.GetSpotsAsync();

        _output.WriteLine("after one fetch: POTA client made " + source.ClientMadeForTests);

        Assert.True(source.ClientMadeForTests, "the fetch did not make a client");
    }

    /// <summary>A transport that answers every request with an empty list, so no packet leaves.</summary>
    /// <remarks>
    /// **AN EMPTY LIST AND NOT A REFUSAL.** A 503 makes `GetStringAsync` throw, and a test
    /// that asked whether a client was made would then be measuring the throw. An empty
    /// array is a fetch that succeeded and found nothing, which is what this needs.
    /// </remarks>
    private sealed class AnswersWithNothing : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]"),
            });
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
