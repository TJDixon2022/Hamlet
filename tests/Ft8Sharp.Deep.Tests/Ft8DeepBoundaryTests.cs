using System.Reflection;
using System.Xml.Linq;
using Ft8Sharp.Dsp;
using Xunit;

namespace Ft8Sharp.Deep.Tests;

/// <summary>
/// <b>The seam, asserted from the sibling's side: the arrow points one way and only one way.</b>
/// </summary>
/// <remarks>
/// <para>
/// <c>Ft8Sharp.Tests.Ft8SharpBoundaryTests</c> guards the port from the port's side - it reads
/// <c>src/Ft8Sharp/Ft8Sharp.csproj</c> off disk and refuses any <c>ProjectReference</c> or
/// <c>PackageReference</c> at all, and it has been watched refusing one. These tests guard the same
/// seam from this side, and they check the thing that project file cannot: what the two BUILT
/// assemblies actually reference.
/// </para>
/// <para>
/// <b>Both directions matter and they matter differently.</b> The sibling referencing the port is the
/// arrangement working - if it ever stopped, the two scoreboard columns would be measuring two
/// unrelated things while still printing side by side. The port referencing the sibling is the
/// failure that destroys the port's publishability, and it is the one this file exists for.
/// </para>
/// </remarks>
public class Ft8DeepBoundaryTests
{
    private static Assembly Sibling => typeof(Ft8DeepSlotDecoder).Assembly;

    private static Assembly Port => typeof(Ft8SlotDecoder).Assembly;

    /// <summary>
    /// <b>The seam is real and not a coincidence</b>: the sibling's built assembly references
    /// <c>Ft8Sharp</c>, so the decoder behind it is the port and not a copy of the port.
    /// </summary>
    [Fact]
    public void TheSiblingsBuiltAssemblyReferencesFt8Sharp()
    {
        var referenced = Sibling
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .ToList();

        Assert.True(
            referenced.Contains("Ft8Sharp", StringComparer.Ordinal),
            $"{Sibling.GetName().Name} references {referenced.Count} assemblies and Ft8Sharp is not "
                + "among them: " + string.Join(", ", referenced) + ". The sibling is supposed to "
                + "delegate to the port; an assembly that does not reference it is not delegating to "
                + "it, whatever the source says.");
    }

    /// <summary>
    /// <b>The direction that would destroy the port's publishability.</b> <c>Ft8Sharp</c> is built to
    /// be lifted out of this repository and published on its own under MIT; a reference to a GPL-3.0
    /// sibling would end that, and it would end it silently.
    /// </summary>
    [Fact]
    public void ThePortsBuiltAssemblyDoesNotReferenceTheSibling()
    {
        var arrivals = Port
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .Where(n => n.StartsWith("Ft8Sharp.Deep", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            arrivals.Count == 0,
            $"{Port.GetName().Name} references {arrivals.Count} Ft8Sharp.Deep assembly/assemblies: "
                + string.Join(", ", arrivals) + ". Nothing of the sibling may reach into the port, "
                + "however it got there. The port is the instrument this phase measures against and "
                + "it is separately publishable under MIT.");
    }

    /// <summary>
    /// <b>And no Hamlet reaches the port through here either.</b> The sibling arrives in the same
    /// solution as a desktop application, and a transitive arrival is exactly the failure
    /// <c>Ft8SharpBoundaryTests.NoHamletAssemblyArrives</c> was written for.
    /// </summary>
    [Fact]
    public void NoHamletAssemblyArrivesInEitherAssembly()
    {
        foreach (var assembly in new[] { Port, Sibling })
        {
            var arrivals = assembly
                .GetReferencedAssemblies()
                .Select(a => a.Name ?? string.Empty)
                .Where(n => n.StartsWith("Hamlet", StringComparison.OrdinalIgnoreCase))
                .ToList();

            Assert.True(
                arrivals.Count == 0,
                $"{assembly.GetName().Name} references {arrivals.Count} Hamlet assembly/assemblies: "
                    + string.Join(", ", arrivals) + ".");
        }
    }

    /// <summary>
    /// <b>One <c>ProjectReference</c>, to the port, and no <c>PackageReference</c> at all.</b> Read
    /// off disk, the way the port's own guard reads its project file, because a declaration is a
    /// different failure from an arrival and the two need different nets.
    /// </summary>
    [Fact]
    public void TheSiblingDeclaresOneProjectReferenceAndNoPackages()
    {
        var projectPath = RepositoryRoot.SiblingProject();
        Assert.True(File.Exists(projectPath), $"No project file at {projectPath}.");

        var project = XDocument.Load(projectPath);

        // MSBuild's default namespace is absent in SDK-style projects, but do not rely on that:
        // match on local name so a namespaced project still counts.
        var projectReferences = Includes(project, "ProjectReference");
        var packageReferences = Includes(project, "PackageReference");

        Assert.True(
            projectReferences.Count == 1,
            $"{projectPath} declares {projectReferences.Count} ProjectReference element(s): "
                + string.Join(", ", projectReferences) + ". Ft8Sharp.Deep references Ft8Sharp and "
                + "nothing else.");

        Assert.EndsWith(
            @"Ft8Sharp\Ft8Sharp.csproj",
            projectReferences[0].Replace('/', '\\'),
            StringComparison.OrdinalIgnoreCase);

        Assert.True(
            packageReferences.Count == 0,
            $"{projectPath} declares {packageReferences.Count} PackageReference element(s): "
                + string.Join(", ", packageReferences) + ". Ft8Sharp.Deep carries no third-party "
                + "runtime dependency.");
    }

    private static List<string> Includes(XDocument project, string localName) =>
        project.Descendants()
            .Where(e => e.Name.LocalName == localName)
            .Select(e => e.Attribute("Include")?.Value ?? "(no Include attribute)")
            .ToList();
}
