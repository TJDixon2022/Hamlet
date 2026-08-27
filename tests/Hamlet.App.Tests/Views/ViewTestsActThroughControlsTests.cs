using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// A view-level test presses the control; it does not set the property the
/// control would have set.
/// </summary>
/// <remarks>
/// <para>**TIM'S RULING OF 2026-08-27**, and it is enforced here rather than
/// written down somewhere, because a rule that lives only in a document is one a
/// session reads at minute zero and has forgotten by task six.</para>
/// <para>**WHAT IT IS FOR.** Unit 1.11.25 asserted the CW workspace was the same
/// object after a tab change. It was, and the screen was blank: the fault lived
/// in the tab strip's binding, and the test set `OperatingMode` on the view model
/// instead of pressing the tab, so it could not reach it. **A test that drives
/// the view model cannot see a broken control.**</para>
/// <para>**WHAT THIS CHECK IS WORTH, STATED PLAINLY.** It is a text search over
/// the view tests for assignments to properties that a control on screen owns.
/// It is a heuristic and the next session needs to know its edges:</para>
/// <para>**It catches**: a test that builds a `MainWindow` and then writes one of
/// the named properties. That is the exact shape of the fault that got through
/// twice.</para>
/// <para>**It does not catch**: a property nobody has added to the list; an
/// assignment written across two lines; a command invoked through
/// `SomeCommand.Execute` rather than by writing a property; a control driven by a
/// method call on the view model. **It cannot catch the general case at all** —
/// deciding whether a control exists for a given property is a question about the
/// XAML, and answering it properly means resolving bindings, which is what the
/// application does at run time and what a text search cannot do.</para>
/// <para>**And it skips one file, its own.** This one names the window type and
/// both offending shapes as string literals, so unskipped it reports its own
/// self-test data as a breach — which it did, on its first run.</para>
/// <para>So this is a named-property guard and not a proof. It is worth having
/// because the properties on the list are the ones a view test reaches for, and
/// it fails loudly the moment somebody reaches for one again.</para>
/// </remarks>
public sealed class ViewTestsActThroughControlsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the findings are printed.</param>
    public ViewTestsActThroughControlsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// Properties a control on screen owns, which a view test must not write.
    /// </summary>
    /// <remarks>
    /// **EACH ENTRY NAMES THE CONTROL THAT OWNS IT.** A property with no control
    /// behind it is legitimate for a test to set — state arriving from the radio
    /// has no button, and arranging it is not bypassing anything.
    /// </remarks>
    private static readonly (string Property, string Control)[] Owned =
    {
        ("OperatingMode", "the CW / Digital / Voice tab strip"),
        ("SendText", "the send line's text box"),
        ("IsBestChance", "the best-chance lens button"),
        ("IsWhatsNew", "the what's-new lens button"),
    };

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
    /// <para>Proves no view test writes a property a control owns. A view test is
    /// one that builds a `MainWindow` — if it puts the real window on the screen,
    /// it is testing the view and must act through it.</para>
    /// </remarks>
    [Fact]
    public void NoViewTestWritesAPropertyAControlOwns()
    {
        var folder = Path.Combine(
            Root(), "tests", "Hamlet.App.Tests", "Views");

        Assert.True(Directory.Exists(folder), $"no view tests at {folder}");

        var offences = new List<string>();
        var scanned = 0;

        foreach (var file in Directory.GetFiles(folder, "*.cs"))
        {
            // **THE GUARD DOES NOT SCAN ITSELF**, and finding that out was the
            // first thing it did. This file names the window type and both
            // offending shapes as string literals, so on its first run it
            // reported its own self-test data as a breach. A check that fails on
            // its own fixtures is a check nobody will keep.
            if (Path.GetFileName(file) == "ViewTestsActThroughControlsTests.cs")
            {
                continue;
            }

            var text = File.ReadAllText(file);

            // Only the files that put the real window up are view tests.
            if (!text.Contains("new MainWindow", StringComparison.Ordinal))
            {
                continue;
            }

            scanned++;

            var lines = text.Replace("\r\n", "\n").Split('\n');

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                // A comment describing the rule is not a breach of it.
                if (line.TrimStart().StartsWith("//", StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (var (property, control) in Owned)
                {
                    // `x.Property = ` and not `==`, and not a property whose name
                    // merely ends with this one.
                    if (Regex.IsMatch(line, $@"\.{property}\s*=\s*[^=]"))
                    {
                        offences.Add(
                            $"{Path.GetFileName(file)}:{i + 1} writes {property}, "
                            + $"which {control} owns — press it instead");
                    }
                }
            }
        }

        _output.WriteLine(
            $"{scanned} view test files scanned for {Owned.Length} owned properties");

        foreach (var offence in offences)
        {
            _output.WriteLine("  " + offence);
        }

        Assert.Empty(offences);
    }

    /// <remarks>
    /// <para>Proves the check can actually fail, which is the part of a guard
    /// nobody checks. A rule enforced by a test that cannot go red is a rule
    /// enforced by nothing.</para>
    /// <para>The same expression the scan uses, run over a line of the shape it
    /// is looking for.</para>
    /// </remarks>
    [Fact]
    public void TheCheckFiresOnTheShapeItIsLookingFor()
    {
        var caught = @"            model.OperatingMode = ""Digital"";";
        var allowed = @"            Assert.Equal(""CW"", model.OperatingMode);";
        var alsoAllowed = @"            if (name == model.OperatingMode)";

        Assert.Matches(@"\.OperatingMode\s*=\s*[^=]", caught);
        Assert.DoesNotMatch(@"\.OperatingMode\s*=\s*[^=]", allowed);
        Assert.DoesNotMatch(@"\.OperatingMode\s*=\s*[^=]", alsoAllowed);

        _output.WriteLine("fires on a write, quiet on a read and on a comparison");
    }
}
