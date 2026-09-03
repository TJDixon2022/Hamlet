using Hamlet.RadioEngine.Audio;
using Xunit;

namespace Hamlet.Tests.Shared;

/// <summary>
/// One of upstream's off-air recordings, found on this machine at run time.
/// </summary>
/// <param name="Name">The path inside the clone's WAV folder, slashes forward.</param>
/// <param name="WavPath">Where the audio is, in the clone.</param>
/// <param name="ExpectedPath">
/// The list upstream's own decoder wrote beside it, which may not exist.
/// </param>
/// <remarks>
/// <para>**NOTHING HERE IS OURS AND NOTHING HERE IS COPIED.** These are roughly
/// 21 MB of somebody else's off-air audio — a real antenna, a real band, a real
/// ionosphere — read from <c>C:\Source\ft8_lib</c> at run time under the plan's
/// ruling that they never enter this repository. This type opens them where they
/// are; it writes nothing and copies nothing.</para>
/// <para>**AND ABSENCE IS A SKIP, NEVER A FAILURE.** A fresh clone of Hamlet does
/// not have them, and a test suite that goes red because somebody else's 21 MB is
/// missing is a suite nobody can run.</para>
/// </remarks>
internal sealed record OffAirRecording(
    string Name, string WavPath, string ExpectedPath)
{
    /// <summary>Whether upstream wrote a decode list beside this recording.</summary>
    public bool HasExpectedList => File.Exists(ExpectedPath);

    /// <summary>How many messages that list claims. Zero when there is none.</summary>
    public int ExpectedCount => ExpectedMessages().Count;

    /// <summary>The audio, at whatever rate the clone holds it.</summary>
    public MonoAudio Read() => WavAudio.Read(WavPath);

    /// <summary>
    /// The message texts upstream's list claims, in the order the file gives them.
    /// </summary>
    /// <remarks>
    /// <para>**THE NORMALISATION IS THE ONE <c>Ft8Sharp.Tests</c> ALREADY USES**,
    /// restated rather than shared because that copy is internal to that assembly:
    /// the message is everything after the first tilde, which is where upstream's
    /// own <c>printf</c> puts it; leading and trailing space goes; and where the
    /// remainder carries a run of two or more spaces the text is what lies to the
    /// left of it, because some of these lists were post-processed to carry a
    /// country annotation upstream's format string does not emit.</para>
    /// <para>**NOTHING ELSE IS STRIPPED.** No case folding, no brackets, and
    /// <c>RR73</c> and <c>RRR</c> stay different messages — laundering those is how
    /// a comparison stops meaning anything. Here the comparison is a witness rather
    /// than a gate in any case, so there is nothing to be gained by loosening it
    /// and a measurement to be lost.</para>
    /// </remarks>
    public IReadOnlyList<string> ExpectedMessages()
    {
        if (!HasExpectedList)
        {
            return Array.Empty<string>();
        }

        var messages = new List<string>();

        foreach (var raw in File.ReadAllLines(ExpectedPath))
        {
            var tilde = raw.IndexOf('~', StringComparison.Ordinal);

            if (tilde < 0)
            {
                continue;
            }

            var text = Normalise(raw[(tilde + 1)..]);

            if (text.Length > 0)
            {
                messages.Add(text);
            }
        }

        return messages;
    }

    /// <summary>The one normalisation, applied to both sides of any comparison.</summary>
    /// <param name="text">A line's worth of message.</param>
    /// <returns>The message.</returns>
    public static string Normalise(string text)
    {
        var trimmed = text.Trim();

        for (var i = 0; i + 1 < trimmed.Length; i++)
        {
            if (trimmed[i] == ' ' && trimmed[i + 1] == ' ')
            {
                return trimmed[..i].TrimEnd();
            }
        }

        return trimmed;
    }
}

/// <summary>Finds upstream's off-air recordings. Nothing here writes to the clone.</summary>
/// <remarks>
/// **THIS IS THE HAMLET-SIDE ROUTE TO THEM, AND IT DID NOT EXIST BEFORE UNIT 226.**
/// <c>Ft8Sharp.Tests</c> has reached them since step 5 through <c>ReferenceClone</c>
/// and <c>ReferenceRecordings</c>, but those types are internal to that assembly,
/// so nothing on Hamlet's side of the boundary could open one. The library has
/// heard a real band for five steps; until this file the application had not.
/// </remarks>
internal static class OffAirRecordings
{
    /// <summary>Where the pinned clone lives, outside the tree and never committed.</summary>
    private const string DefaultClone = @"C:\Source\ft8_lib";

    /// <summary>Where the recordings sit inside it.</summary>
    private const string WavFolder = @"test\wav";

    /// <summary>
    /// The clone to read. Overridable so the skip path can be <em>watched</em> on a
    /// machine that has the clone — <c>dotnet test -e FT8_LIB_PATH=&lt;nowhere&gt;</c>
    /// — rather than asserted in a comment, which is the idiom
    /// <c>Ft8Sharp.Tests</c> already uses.
    /// </summary>
    public static string CloneLocation =>
        Environment.GetEnvironmentVariable("FT8_LIB_PATH") is { Length: > 0 } configured
            ? configured
            : DefaultClone;

    /// <summary>The folder the recordings are in.</summary>
    public static string WavLocation => Path.Combine(CloneLocation, WavFolder);

    /// <summary>Whether the recordings are on this machine at all.</summary>
    public static bool Present => Directory.Exists(WavLocation);

    /// <summary>
    /// Every recording in the pin, ordinal by path, so the order is a function of
    /// the clone and not of the file system's enumeration. Empty when absent.
    /// </summary>
    public static IReadOnlyList<OffAirRecording> All()
    {
        if (!Present)
        {
            return Array.Empty<OffAirRecording>();
        }

        return Directory
            .GetFiles(WavLocation, "*.wav", SearchOption.AllDirectories)
            .OrderBy(p => p, StringComparer.Ordinal)
            .Select(p => new OffAirRecording(
                Path.GetRelativePath(WavLocation, p).Replace('\\', '/'),
                p,
                Path.ChangeExtension(p, ".txt")))
            .ToArray();
    }

    /// <summary>
    /// The recordings with the most stations in them, busiest first.
    /// </summary>
    /// <param name="count">How many to take.</param>
    /// <returns>Up to that many, busiest first, ties broken by name.</returns>
    /// <remarks>
    /// **BUSIEST BY UPSTREAM'S OWN COUNT, NOT BY OURS.** How many messages this
    /// port finds in a recording is the thing being measured, so ranking by it
    /// would pick the recordings this port happens to like. Upstream's list is
    /// written by upstream's decoder and knows nothing about us.
    /// </remarks>
    public static IReadOnlyList<OffAirRecording> Busiest(int count) =>
        All()
            .Where(r => r.HasExpectedList)
            .OrderByDescending(r => r.ExpectedCount)
            .ThenBy(r => r.Name, StringComparer.Ordinal)
            .Take(count)
            .ToArray();
}

/// <summary>A fact that needs upstream's recordings, and skips without them.</summary>
/// <remarks>
/// **SKIP RATHER THAN FAIL, UNDER THE PLAN'S OWN RULING.** The recordings are never
/// committed, so a fresh clone has none of them and every test over them must
/// report skipped. Setting <c>Skip</c> from a derived attribute is the idiom that
/// does not cost a package reference, and it is the one
/// <c>RequiresReferenceCloneFactAttribute</c> already uses in
/// <c>Ft8Sharp.Tests</c>.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequiresOffAirRecordingsFactAttribute : FactAttribute
{
    /// <summary>Creates the attribute, skipping when the clone is not here.</summary>
    public RequiresOffAirRecordingsFactAttribute()
    {
        if (!OffAirRecordings.Present)
        {
            Skip =
                $"upstream's off-air recordings are not at {OffAirRecordings.WavLocation}. "
                + "They are never committed, so this is expected on a fresh clone.";
        }
    }
}
