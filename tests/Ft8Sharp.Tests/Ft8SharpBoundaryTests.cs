using System.Reflection;
using System.Xml.Linq;
using Ft8Sharp;
using Xunit;

namespace Ft8Sharp.Tests;

/// <summary>
/// Ft8Sharp is built as if it will be published on its own. That claim is only
/// true while the library depends on nothing outside itself, and a guard that has
/// never refused is not a guard — this one was watched failing on 2026-08-31, on a
/// ProjectReference to Hamlet.RadioEngine added on purpose and then reverted.
/// </summary>
/// <remarks>
/// <para>
/// Two halves, catching different failures. <see cref="DeclaresNoReferences"/>
/// reads the project file and catches a reference someone writes down;
/// <see cref="NoHamletAssemblyArrives"/> reads the built assembly and catches one
/// that arrives without being written down here.
/// </para>
/// <para>
/// <b>Only the first half refused, and that is the shape of the guard rather than a
/// defect in it.</b> The compiler emits an assembly reference for an assembly whose
/// types are used, and the added reference was used by nothing, so
/// <see cref="NoHamletAssemblyArrives"/> stayed green throughout. It is the second
/// net, not the first: it catches Hamlet arriving through something that is used —
/// transitively, or through a reference nobody wrote in this file — and it cannot
/// catch a declaration on its own. The declaration is what
/// <see cref="DeclaresNoReferences"/> is for, and it failed immediately.
/// </para>
/// </remarks>
public class Ft8SharpBoundaryTests
{
    [Fact]
    public void DeclaresNoReferences()
    {
        var projectPath = LocateFt8SharpProject();
        var project = XDocument.Load(projectPath);

        // MSBuild's default namespace is absent in SDK-style projects, but do not
        // rely on that: match on local name so a namespaced project still counts.
        var projectReferences = Elements(project, "ProjectReference");
        var packageReferences = Elements(project, "PackageReference");

        Assert.True(
            projectReferences.Count == 0,
            $"{projectPath} declares {projectReferences.Count} ProjectReference element(s): "
                + string.Join(", ", projectReferences) + ". Ft8Sharp must reference nothing "
                + "outside itself so it can be extracted and published on its own.");

        Assert.True(
            packageReferences.Count == 0,
            $"{projectPath} declares {packageReferences.Count} PackageReference element(s): "
                + string.Join(", ", packageReferences) + ". Ft8Sharp must carry no third-party "
                + "runtime dependency.");
    }

    [Fact]
    public void NoHamletAssemblyArrives()
    {
        var arrivals = Ft8SharpAssembly.Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .Where(n => n.StartsWith("Hamlet", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            arrivals.Count == 0,
            $"{Ft8SharpAssembly.Name} references {arrivals.Count} Hamlet assembly/assemblies: "
                + string.Join(", ", arrivals) + ". Nothing of Hamlet may reach into Ft8Sharp, "
                + "however it got there.");
    }

    private static List<string> Elements(XDocument project, string localName) =>
        project.Descendants()
            .Where(e => e.Name.LocalName == localName)
            .Select(e => e.Attribute("Include")?.Value ?? "(no Include attribute)")
            .ToList();

    /// <summary>
    /// Finds src/Ft8Sharp/Ft8Sharp.csproj by walking up from the test binaries to
    /// the directory holding Hamlet.sln.
    /// </summary>
    /// <remarks>
    /// Throws rather than skipping or passing when it cannot find the file. A
    /// boundary guard that quietly goes green when it cannot find its subject is
    /// worse than no guard at all, because it reads green forever.
    /// </remarks>
    private static string LocateFt8SharpProject()
    {
        var searched = new List<string>();
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            searched.Add(dir.FullName);
            if (!File.Exists(Path.Combine(dir.FullName, "Hamlet.sln")))
            {
                continue;
            }

            var candidate = Path.Combine(dir.FullName, "src", "Ft8Sharp", "Ft8Sharp.csproj");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            throw new FileNotFoundException(
                $"Found the repository root at {dir.FullName} but no Ft8Sharp.csproj at {candidate}. "
                + "The boundary test cannot check a project it cannot read, and will not pass "
                + "without checking it.");
        }

        throw new DirectoryNotFoundException(
            "Could not find the repository root (no directory containing Hamlet.sln) walking up from "
            + $"{AppContext.BaseDirectory}. Searched: {string.Join(", ", searched)}. The boundary "
            + "test cannot check a project it cannot locate, and will not pass without checking it.");
    }
}
