using Ft8Sharp.Tests.Encode;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// The off-air recordings in the pinned clone, and the expected decode list beside each one.
/// </summary>
/// <remarks>
/// <para>
/// <b>Nothing here is committed and nothing here is ours.</b> These are roughly 21 MB of somebody
/// else's off-air audio, read from <c>C:\Source\ft8_lib</c> at run time, and the expected lists are
/// upstream's own decoder's output about its own recordings. The plan's ruling is absolute: they
/// never enter this repository. This type locates them and reads them; it copies nothing.
/// </para>
/// <para>
/// <b>Why this is the strongest instrument this phase has on the receive side.</b> Every other
/// receive measurement in the phase was taken against a signal this library synthesized itself, and
/// a port that is wrong in the same way at both ends of a round trip passes all of them. These
/// recordings were made on somebody else's antenna from real stations, and the lists were written by
/// upstream's decoder rather than by ours.
/// </para>
/// <para>
/// <b>Absent is a skip.</b> <see cref="All"/> returns nothing when the clone is not on the machine,
/// and every test over it carries <c>[RequiresReferenceCloneFact]</c>.
/// </para>
/// </remarks>
internal sealed record ReferenceRecording(
    string Name,
    string WavPath,
    string ExpectedPath,
    int SampleRate,
    int Channels,
    int BitsPerSample,
    int SampleCount)
{
    /// <summary>Whether a checked-in expected decode list sits beside this recording.</summary>
    public bool HasExpectedList => File.Exists(ExpectedPath);

    /// <summary>How long the recording runs.</summary>
    public double Seconds => (double)SampleCount / SampleRate;

    /// <summary>How many messages upstream's list says are in it. Zero when there is no list.</summary>
    public int ExpectedCount => ExpectedMessages().Count;

    /// <summary>
    /// The message texts upstream's list claims, in the order the file gives them.
    /// </summary>
    /// <remarks>
    /// <b>The normalisation is stated here and applied to both sides of every comparison.</b>
    /// <list type="number">
    /// <item>
    /// The message is everything after the first tilde, which is where upstream's own print format
    /// puts it: <c>"%02d%02d%02d %+05.1f %+4.2f %4.0f ~  %s\n"</c>.
    /// </item>
    /// <item>
    /// Leading and trailing whitespace is removed. That is the two spaces upstream's format string
    /// writes, and nothing else.
    /// </item>
    /// <item>
    /// Where the remainder contains a run of <b>two or more spaces</b>, the text is what lies to the
    /// left of it. Some of these lists carry a trailing country or continent annotation that
    /// upstream's <c>printf</c> does not emit, so those files were post-processed by something else;
    /// an FT8 message is single-space separated between tokens, so a run of two is an unambiguous
    /// boundary. <b>Nothing else is stripped</b> — no brackets, no case folding, and <c>RR73</c> and
    /// <c>RRR</c> stay different messages, because laundering those is how a comparison stops
    /// meaning anything.
    /// </item>
    /// </list>
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
            var tilde = raw.IndexOf('~');
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

    /// <summary>
    /// The one normalisation this comparison applies, used on the expected side and on this
    /// library's own decoded text alike. See the remarks on <see cref="ExpectedMessages"/>.
    /// </summary>
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

    /// <summary>The samples, as the monitor takes them: one float per sixteen-bit count.</summary>
    /// <remarks>
    /// <b>Divided by 32768, which is upstream's own <c>load_wav</c> scaling</b>, so the signal this
    /// library analyses is numerically the signal upstream's decoder analysed. Nothing is filtered,
    /// nothing is resampled and no gain is applied.
    /// </remarks>
    public float[] ReadSamples()
    {
        var contents = WavFile.Read(WavPath);
        var samples = new float[contents.Samples.Length];
        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = contents.Samples[i] / 32768.0f;
        }

        return samples;
    }
}

/// <summary>Finds the recordings. Nothing here writes to the clone.</summary>
internal static class ReferenceRecordings
{
    /// <summary>Where the recordings sit in the pin.</summary>
    public const string Directory = @"test\wav";

    /// <summary>
    /// Every recording in the pin, deepest-path-last and ordinal by path, so the order is a function
    /// of the clone and not of the file system's enumeration.
    /// </summary>
    public static IEnumerable<ReferenceRecording> All()
    {
        var root = Path.Combine(ReferenceClone.Location, Directory);
        if (!System.IO.Directory.Exists(root))
        {
            yield break;
        }

        var paths = System.IO.Directory
            .GetFiles(root, "*.wav", SearchOption.AllDirectories)
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToArray();

        foreach (var path in paths)
        {
            var contents = WavFile.Read(path);
            yield return new ReferenceRecording(
                Path.GetRelativePath(root, path).Replace('\\', '/'),
                path,
                Path.ChangeExtension(path, ".txt"),
                contents.SampleRate,
                contents.ChannelCount,
                contents.BitsPerSample,
                contents.Samples.Length);
        }
    }

    /// <summary>
    /// The recordings criterion 3 is measured on: every one that carries an expected decode list.
    /// </summary>
    public static IReadOnlyList<ReferenceRecording> WithExpectedLists() =>
        All().Where(r => r.HasExpectedList).ToArray();
}
