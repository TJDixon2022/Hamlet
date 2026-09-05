namespace Ft8Sharp.Deep.Tests;

/// <summary>
/// Finds the repository root by walking up from the test binaries to the directory holding
/// <c>Hamlet.sln</c>.
/// </summary>
/// <remarks>
/// <b>Throws rather than skipping or passing when it cannot find it</b>, which is
/// <c>Ft8SharpBoundaryTests.LocateFt8SharpProject</c>'s rule and is kept for the same reason: a guard
/// that quietly goes green when it cannot find its subject is worse than no guard at all, because it
/// reads green forever.
/// </remarks>
internal static class RepositoryRoot
{
    /// <summary>The directory holding <c>Hamlet.sln</c>.</summary>
    public static string Locate()
    {
        var searched = new List<string>();
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            searched.Add(dir.FullName);
            if (File.Exists(Path.Combine(dir.FullName, "Hamlet.sln")))
            {
                return dir.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            "Could not find the repository root (no directory containing Hamlet.sln) walking up from "
            + $"{AppContext.BaseDirectory}. Searched: {string.Join(", ", searched)}. These tests "
            + "cannot check files they cannot locate, and will not pass without checking them.");
    }

    /// <summary>The directory holding <c>Ft8Sharp.Deep.csproj</c>.</summary>
    public static string SiblingDirectory() => Path.Combine(Locate(), "src", "Ft8Sharp.Deep");

    /// <summary>The sibling's project file.</summary>
    public static string SiblingProject() =>
        Path.Combine(SiblingDirectory(), "Ft8Sharp.Deep.csproj");
}
