using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Hamlet.App.Views;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Every resource key the application's XAML names actually resolves.
/// </summary>
/// <remarks>
/// <para>**AVALONIA LEAVES AN UNFOUND `StaticResource` AS NO VALUE AT ALL**, not
/// as an error — no build failure, no exception, nothing in a log. A brush that
/// does not exist paints nothing, and a panel with no background looks like a
/// panel that was meant to have none.</para>
/// <para>**`HmPanelBrush` WAS MISSING FOR TWO UNITS.** Unit 1.11.24 wrote it for
/// the Receive panel and unit 1.11.26 for the workspace boundary, and it was
/// never in `App.axaml`. Receive drew with no background the whole time, in a
/// suite of five hundred tests that includes a binding-health test — because
/// `BindingHealthTests` catches an unresolved **binding** and not an unresolved
/// **resource**. Tim found it by looking at the screen.</para>
/// <para>**WHAT THIS CHECKS AND WHAT IT DOES NOT.** It reads the application's
/// XAML as text, collects every `{StaticResource X}` and `{DynamicResource X}`
/// key, and asks the real window whether each one resolves. It therefore covers
/// keys named in markup. It does not cover a key built in code at run time, or
/// one named only inside a control theme in a library this application does not
/// own.</para>
/// </remarks>
public sealed class EveryResourceKeyResolvesTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the count and any failures are printed.</param>
    public EveryResourceKeyResolvesTests(ITestOutputHelper output)
        => _output = output;

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

    /// <remarks>
    /// <para>Proves every key named in the application's markup resolves against
    /// the real window, which is the object that would have to find them.</para>
    /// <para>**ASKED OF THE WINDOW AND NOT OF `Application.Current`**, because a
    /// key can live in a control's own resources or in a merged dictionary the
    /// window pulls in, and the window is what the operator is looking at.</para>
    /// </remarks>
    [AvaloniaFact]
    public void EveryStaticAndDynamicResourceKeyResolves()
    {
        var app = Path.Combine(Root(), "src", "Hamlet.App");

        Assert.True(Directory.Exists(app), $"no application at {app}");

        var keys = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var file in Directory.GetFiles(app, "*.axaml", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);

            foreach (Match m in Regex.Matches(
                text, @"\{(?:Static|Dynamic)Resource\s+([A-Za-z_][\w.]*)\s*\}"))
            {
                var key = m.Groups[1].Value;

                if (!keys.TryGetValue(key, out var where))
                {
                    keys[key] = where = new List<string>();
                }

                var name = Path.GetFileName(file);

                if (!where.Contains(name))
                {
                    where.Add(name);
                }
            }
        }

        Assert.NotEmpty(keys);

        var window = new MainWindow();

        window.Show();

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var missing = new List<string>();

        foreach (var (key, where) in keys)
        {
            if (!window.TryFindResource(key, out var found) || found is null)
            {
                missing.Add($"{key} — named in {string.Join(", ", where)}");
            }
        }

        window.Close();

        _output.WriteLine(
            $"{keys.Count} resource keys referenced, "
            + $"{keys.Count - missing.Count} resolve");

        foreach (var one in missing)
        {
            _output.WriteLine("  MISSING: " + one);
        }

        Assert.True(
            missing.Count == 0,
            "these resource keys are named in the markup and resolve to nothing, "
            + "so whatever uses them draws nothing and says nothing about it: "
            + string.Join("; ", missing));
    }

    /// <remarks>
    /// Proves the check can go red, which is the part of a guard nobody checks. A
    /// key nobody has defined must not resolve, or the search is finding
    /// something for everything and the test is worthless.
    /// </remarks>
    [AvaloniaFact]
    public void AKeyThatDoesNotExistDoesNotResolve()
    {
        var window = new MainWindow();

        window.Show();

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();

        var found = window.TryFindResource(
            "HmThisBrushHasNeverExisted", out var value);

        window.Close();

        _output.WriteLine(
            $"a made-up key resolves: {found && value is not null}");

        Assert.False(found && value is not null);
    }
}
