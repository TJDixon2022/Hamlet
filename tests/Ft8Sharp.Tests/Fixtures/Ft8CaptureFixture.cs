using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Ft8Sharp.Tests.Dsp;

namespace Ft8Sharp.Tests.Fixtures;

/// <summary>
/// <b>Something is wrong with a capture fixture, and the run stops.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>There is no quiet path out of this type.</b> Every refusal in
/// <see cref="Ft8CaptureFixture"/> throws one of these and none of them returns a skip, a warning,
/// a zero-row result or an empty list. That is the whole point of the exit this code closes:
/// <c>PHASE_PLAN.md</c> step 0 says <em>a stale fixture silently measures the wrong thing</em>, and
/// silence is the failure mode, not the fault it hides.
/// </para>
/// <para>
/// <b>The message always names three things</b> — the fixture, the capture it is about, and what was
/// wrong — because a session six units from now sees this line and nothing else, and "hash mismatch"
/// without a file name is a message that starts an investigation rather than ending one.
/// </para>
/// </remarks>
internal sealed class Ft8FixtureException : Exception
{
    internal Ft8FixtureException(string fixture, string capture, string what)
        : base($"FT8 capture fixture \"{fixture}\" (capture \"{capture}\"): {what}")
    {
        Fixture = fixture;
        Capture = capture;
        What = what;
    }

    /// <summary>The fixture file this is about.</summary>
    internal string Fixture { get; }

    /// <summary>The capture it names, or <c>(unknown)</c> if it did not get that far.</summary>
    internal string Capture { get; }

    /// <summary>What was wrong, in words.</summary>
    internal string What { get; }
}

/// <summary>One message a decoder returned for a capture: the plan's row of message, frequency, dt and SNR.</summary>
/// <param name="SnrDb">Signal-to-noise ratio in decibels, as the producing decoder reported it.</param>
/// <param name="DtSeconds">Time offset of the transmission within the slot, in seconds.</param>
/// <param name="FrequencyHz">Audio frequency of the transmission in the passband.</param>
/// <param name="Message">
/// The message, already through <see cref="ReferenceRecording.Normalise"/>. Never empty.
/// </param>
internal sealed record Ft8FixtureRow(double SnrDb, double DtSeconds, double FrequencyHz, string Message)
{
    /// <summary>The line this row is written as. Round-trips through <see cref="Ft8CaptureFixture.Parse"/>.</summary>
    public override string ToString() =>
        string.Format(
            CultureInfo.InvariantCulture,
            "ROW  {0,6:F1}  {1,6:F2}  {2,6:F0}  {3}",
            SnrDb,
            DtSeconds,
            FrequencyHz,
            Message);
}

/// <summary>
/// <b>A committed capture fixture: which audio, taken when, hashing to what, and what a decoder
/// returned for it — message by message.</b> The format is documented in prose at
/// <c>docs/ft8-capture-fixture-format.md</c> and that document is the specification; this type is
/// its reader and its writer.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why a file and not a program.</b> There is no WSJT-X on the development machine and no unit may
/// assume one. So WSJT-X enters this project as a file Tim commits from the shack, and every session
/// afterwards scores against what it actually returned for that exact audio.
/// </para>
/// <para>
/// <b>The four refusals, each with its own test.</b> The capture is absent
/// (<see cref="RequireCapture"/>); the capture is there and its hash does not match (same); a row or
/// a header line is malformed (<see cref="Parse"/>); and the provenance is not a real WSJT-X run and
/// the caller asked to <em>score</em> (<see cref="RequireScorable"/>). <b>A refusal that is not
/// tested is a refusal that will not happen</b>, so all four are watched by
/// <c>Ft8CaptureFixtureTests</c>.
/// </para>
/// <para>
/// <b>The message comparison is not this type's to invent.</b> Every message read here goes through
/// <see cref="ReferenceRecording.Normalise"/>, called and not re-implemented, for the same reason the
/// harness extends the ladder rather than replacing it: a second copy of a normalisation rule drifts,
/// and it drifts silently.
/// </para>
/// </remarks>
internal sealed record Ft8CaptureFixture(
    int FormatVersion,
    string CaptureName,
    string Utc,
    string Sha256,
    int SampleRate,
    string Provenance,
    string Generator,
    IReadOnlyList<Ft8FixtureRow> Rows,
    string FixturePath)
{
    /// <summary>The only format version this reader knows.</summary>
    internal const int CurrentFormat = 1;

    /// <summary><b>The rows are a real WSJT-X run's output.</b> The only provenance that may be scored against.</summary>
    internal const string ProvenanceWsjtx = "wsjtx";

    /// <summary><b>The rows came from something else and make no claim about WSJT-X.</b> Readable, never scorable.</summary>
    internal const string ProvenanceExample = "example";

    /// <summary>The extension that distinguishes this from CW's <c>.txt</c> state sidecar.</summary>
    internal const string Extension = ".fixture.txt";

    /// <summary>Every header key, all of them required. An unknown key is a refusal, not a comment.</summary>
    private static readonly string[] Keys =
    [
        "format", "capture", "utc", "sha256", "sampleRate", "provenance", "generator",
    ];

    /// <summary>Where the capture should be: beside the fixture, same folder.</summary>
    internal string CapturePath =>
        Path.Combine(Path.GetDirectoryName(Path.GetFullPath(FixturePath)) ?? ".", CaptureName);

    /// <summary>What a message this fixture claims looks like, for a set comparison.</summary>
    internal IReadOnlyList<string> Messages => Rows.Select(r => r.Message).ToArray();

    /// <summary>Whether a claim may be scored against this fixture at all.</summary>
    internal bool IsRealWsjtxRun => string.Equals(Provenance, ProvenanceWsjtx, StringComparison.Ordinal);

    /// <summary>Reads one fixture off disk. <b>Refuses a malformed one; does not look at the capture yet.</b></summary>
    internal static Ft8CaptureFixture Read(string path)
    {
        if (!File.Exists(path))
        {
            throw new Ft8FixtureException(
                path,
                "(unknown)",
                "the fixture file itself is not there, so there is nothing to read. A path that does "
                + "not exist is never an empty fixture.");
        }

        return Parse(File.ReadAllText(path), path);
    }

    /// <summary>
    /// The parse, split out so every malformation can be watched against text built in a test rather
    /// than against files that would have to be manufactured on disk to be broken.
    /// </summary>
    internal static Ft8CaptureFixture Parse(string text, string fixturePath)
    {
        var headers = new Dictionary<string, string>(StringComparer.Ordinal);
        var rows = new List<Ft8FixtureRow>();
        var lines = text.Replace("\r\n", "\n").Split('\n');

        // The capture name is wanted in every refusal message below, including the ones raised
        // before it has been read, so it is tracked as the parse goes rather than looked up at the end.
        var capture = "(unknown)";

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            var number = i + 1;

            if (line.Length == 0 || line[0] == '#')
            {
                continue;
            }

            if (line.StartsWith("ROW", StringComparison.Ordinal)
                && (line.Length == 3 || char.IsWhiteSpace(line[3])))
            {
                rows.Add(ParseRow(line, number, fixturePath, capture));
                continue;
            }

            // A bare known key is an EMPTY VALUE, not an unreadable line. The two produce different
            // messages and the difference is worth keeping: "the generator header is empty" sends
            // whoever reads it somewhere useful, and "this line makes no sense" does not.
            var split = line.IndexOfAny([' ', '\t']);
            var key = split < 0 ? line : line[..split];
            var value = split < 0 ? string.Empty : line[split..].Trim();

            if (split < 0 && !Keys.Contains(key, StringComparer.Ordinal))
            {
                throw new Ft8FixtureException(
                    fixturePath,
                    capture,
                    $"line {number} is \"{line}\", which is neither a comment, a \"key value\" header "
                    + "nor a ROW. A line this reader does not understand is a refusal and never a "
                    + "line it skips.");
            }

            if (!Keys.Contains(key, StringComparer.Ordinal))
            {
                throw new Ft8FixtureException(
                    fixturePath,
                    capture,
                    $"line {number} carries the header key \"{key}\", which is not one of "
                    + $"{string.Join(", ", Keys)}. An unrecognised key is a fixture written by "
                    + "something this reader does not understand.");
            }

            if (!headers.TryAdd(key, value))
            {
                throw new Ft8FixtureException(
                    fixturePath,
                    capture,
                    $"line {number} repeats the header key \"{key}\", which already read "
                    + $"\"{headers[key]}\". Which one is meant cannot be guessed.");
            }

            if (string.Equals(key, "capture", StringComparison.Ordinal))
            {
                capture = value;
            }
        }

        foreach (var key in Keys)
        {
            if (!headers.ContainsKey(key))
            {
                throw new Ft8FixtureException(
                    fixturePath,
                    capture,
                    $"there is no \"{key}\" header. All of {string.Join(", ", Keys)} are required; a "
                    + "fixture missing one reads cleanly and is missing the field that made it "
                    + "trustworthy.");
            }
        }

        var format = RequireInt(headers["format"], "format", fixturePath, capture);
        if (format != CurrentFormat)
        {
            throw new Ft8FixtureException(
                fixturePath,
                capture,
                $"it declares format {format} and this reader knows only format {CurrentFormat}. A "
                + "reader that does not know a version refuses rather than reading it optimistically.");
        }

        if (capture.Length == 0
            || capture.IndexOfAny([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, ':']) >= 0)
        {
            throw new Ft8FixtureException(
                fixturePath,
                capture,
                $"the capture name \"{capture}\" is empty or carries a path. It must be a bare file "
                + "name, resolved beside the fixture, so a fixture can never point outside its own "
                + "folder.");
        }

        var utc = headers["utc"];
        if (!DateTimeOffset.TryParseExact(
                utc,
                "yyyy-MM-dd'T'HH:mm:ss'Z'",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out _))
        {
            throw new Ft8FixtureException(
                fixturePath,
                capture,
                $"the utc header reads \"{utc}\", which is not yyyy-MM-ddTHH:mm:ssZ. The shack is "
                + "UTC-04:00 and a local time here would put one evening's captures on two dates.");
        }

        var sha = headers["sha256"];
        if (sha.Length != 64 || !sha.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))
        {
            throw new Ft8FixtureException(
                fixturePath,
                capture,
                $"the sha256 header reads \"{sha}\", which is not 64 lower-case hexadecimal "
                + "characters. It is the digest of the capture file's bytes and nothing shorter will "
                + "do: a truncated hash is a hash that can be told apart from a check.");
        }

        var rate = RequireInt(headers["sampleRate"], "sampleRate", fixturePath, capture);
        if (rate <= 0)
        {
            throw new Ft8FixtureException(
                fixturePath, capture, $"the sampleRate header reads {rate}, which is not a rate.");
        }

        var provenance = headers["provenance"];
        if (!string.Equals(provenance, ProvenanceWsjtx, StringComparison.Ordinal)
            && !string.Equals(provenance, ProvenanceExample, StringComparison.Ordinal))
        {
            // NOT DEFAULTED, EITHER WAY. Defaulting to "example" silently discards a real
            // measurement; defaulting to "wsjtx" silently promotes a fabricated one. Neither is safe,
            // so an unrecognised provenance is refused here and the caller has to look at the file.
            throw new Ft8FixtureException(
                fixturePath,
                capture,
                $"the provenance header reads \"{provenance}\" and the only values this reader "
                + $"accepts are \"{ProvenanceWsjtx}\" and \"{ProvenanceExample}\". It is not defaulted "
                + "in either direction, because one default throws away a real measurement and the "
                + "other promotes a fabricated one.");
        }

        if (headers["generator"].Length == 0)
        {
            throw new Ft8FixtureException(
                fixturePath,
                capture,
                "there is no \"generator\" header value - the key is there and the value is empty. It "
                + "has to name what actually produced the rows, "
                + "because provenance alone says which kind of thing it was and not which thing.");
        }

        if (rows.Count == 0)
        {
            throw new Ft8FixtureException(
                fixturePath,
                capture,
                "it carries no ROW lines. A fixture with no rows is not a measurement that found "
                + "nothing - it is a fixture that would score every decoder as having missed "
                + "nothing, forever.");
        }

        return new Ft8CaptureFixture(
            format, capture, utc, sha, rate, provenance, headers["generator"], rows, fixturePath);
    }

    /// <summary>
    /// <b>Refusal 1 and refusal 2.</b> Finds the capture, checks its hash, and hands back its path.
    /// </summary>
    /// <remarks>
    /// <b>The two are separate messages on purpose.</b> "The file is not there" and "the file is not
    /// the one this was measured on" send whoever reads them to different places, and merging them
    /// into one <em>could not use the capture</em> costs exactly the information that was worth
    /// having.
    /// </remarks>
    internal string RequireCapture()
    {
        var path = CapturePath;

        if (!File.Exists(path))
        {
            throw new Ft8FixtureException(
                FixturePath,
                CaptureName,
                $"the capture it names is not at {path}. A fixture whose capture is absent is a hard "
                + "failure and is not the same thing as having no fixtures at all - an empty "
                + "captured/ folder is this machine's expected state (SHACK_FACTS.md FACT-004) and "
                + "passes cleanly.");
        }

        var actual = HashOf(path);
        if (!string.Equals(actual, Sha256, StringComparison.Ordinal))
        {
            throw new Ft8FixtureException(
                FixturePath,
                CaptureName,
                $"the capture at {path} hashes to {actual} and this fixture records {Sha256}. THE "
                + "AUDIO IS NOT THE AUDIO THESE ROWS WERE MEASURED ON. Scoring against it would "
                + "quietly measure the wrong thing and report a number that looks exactly like a "
                + "good one.");
        }

        return path;
    }

    /// <summary>
    /// <b>Refusal 4.</b> Reading an example fixture is fine; scoring a claim against one is not.
    /// </summary>
    /// <param name="what">What the caller is trying to do, named in the message.</param>
    internal void RequireScorable(string what)
    {
        if (IsRealWsjtxRun)
        {
            return;
        }

        throw new Ft8FixtureException(
            FixturePath,
            CaptureName,
            $"{what} was refused because its provenance is \"{Provenance}\" and not "
            + $"\"{ProvenanceWsjtx}\". Its rows are \"{Generator}\" - they are not what WSJT-X "
            + "returned for this audio, and a score against them would be a measurement against a "
            + "claim nobody ever made. Reading this fixture is allowed; scoring against it is not.");
    }

    /// <summary>The SHA-256 of a file's bytes, lower-case hex, all 64 characters.</summary>
    /// <remarks>
    /// <b>Of the file, not of the samples.</b> CW's sidecar carries a 12-character fingerprint over
    /// the float array in memory (HM-DEC-090), which nothing recomputes and nobody outside this
    /// repository could. This one is recomputable by anyone holding the <c>.wav</c>, with any tool,
    /// on any machine - <c>certutil -hashfile x.wav SHA256</c> or <c>sha256sum x.wav</c> - which is
    /// what makes it a check rather than a label.
    /// </remarks>
    internal static string HashOf(string path) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    /// <summary>The same digest over bytes already in hand.</summary>
    internal static string HashOfBytes(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    /// <summary>
    /// <b>The whole file, as text, ready to be written.</b> Round-trips through
    /// <see cref="Parse"/> and there is no other writer.
    /// </summary>
    /// <param name="preamble">Comment lines, written with their <c>#</c>, before the headers.</param>
    internal string ToFileText(IReadOnlyList<string>? preamble = null)
    {
        var text = new StringBuilder();

        text.Append("# Hamlet FT8 capture fixture - format ").Append(FormatVersion).Append('\n');
        foreach (var line in preamble ?? [])
        {
            text.Append(line.Length == 0 ? "#" : "# " + line).Append('\n');
        }

        text.Append('\n');
        text.Append("format      ").Append(FormatVersion.ToString(CultureInfo.InvariantCulture)).Append('\n');
        text.Append("capture     ").Append(CaptureName).Append('\n');
        text.Append("utc         ").Append(Utc).Append('\n');
        text.Append("sha256      ").Append(Sha256).Append('\n');
        text.Append("sampleRate  ").Append(SampleRate.ToString(CultureInfo.InvariantCulture)).Append('\n');
        text.Append("provenance  ").Append(Provenance).Append('\n');
        text.Append("generator   ").Append(Generator).Append('\n');
        text.Append('\n');

        foreach (var row in Rows)
        {
            text.Append(row).Append('\n');
        }

        return text.ToString();
    }

    private static Ft8FixtureRow ParseRow(string line, int number, string fixturePath, string capture)
    {
        // Message last, unquoted, because it contains spaces; so the split is bounded at four and the
        // remainder is the message whole. A row with fewer than five tokens is short a field and is
        // refused rather than filled in.
        var parts = line.Split((char[]?)null, 5, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 5)
        {
            throw new Ft8FixtureException(
                fixturePath,
                capture,
                $"line {number} is \"{line}\", which has {parts.Length} fields where a ROW needs five: "
                + "ROW, snrDb, dt, freqHz, then the message.");
        }

        var snr = RequireDouble(parts[1], "snrDb", line, number, fixturePath, capture);
        var dt = RequireDouble(parts[2], "dt", line, number, fixturePath, capture);
        var hz = RequireDouble(parts[3], "freqHz", line, number, fixturePath, capture);

        // The same normalisation ReferenceRecordings applies to upstream's own lists, CALLED and not
        // re-implemented: trim, cut at the first run of two or more spaces, and nothing else. No case
        // folding, no bracket stripping, and RR73 and RRR stay different messages.
        var message = ReferenceRecording.Normalise(parts[4]);
        if (message.Length == 0)
        {
            throw new Ft8FixtureException(
                fixturePath,
                capture,
                $"line {number} is \"{line}\", whose message is empty once normalised. A row with no "
                + "message is a row that would match nothing and be counted as a miss by every "
                + "decoder forever.");
        }

        return new Ft8FixtureRow(snr, dt, hz, message);
    }

    private static double RequireDouble(
        string token, string field, string line, int number, string fixturePath, string capture)
    {
        if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        throw new Ft8FixtureException(
            fixturePath,
            capture,
            $"line {number} is \"{line}\", whose {field} field reads \"{token}\" and is not a number. "
            + "A parser that guessed here is how a wrong figure reaches a report.");
    }

    private static int RequireInt(string token, string field, string fixturePath, string capture)
    {
        if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        throw new Ft8FixtureException(
            fixturePath,
            capture,
            $"the {field} header reads \"{token}\" and is not a whole number.");
    }
}

/// <summary>Where the committed fixtures are, and what is there today.</summary>
/// <remarks>
/// <b>Zero real fixtures is the expected state on this machine and passes cleanly.</b>
/// <c>SHACK_FACTS.md</c> FACT-004 records that the radio lives on a different computer, so nothing
/// here treats an empty <c>captured/</c> as a defect. What is a defect is a fixture that <em>names</em>
/// a capture which is not beside it, and the two cases are watched separately.
/// </remarks>
internal static class Ft8CaptureFixtures
{
    /// <summary>Real captures and the fixtures WSJT-X produced for them.</summary>
    internal const string CapturedFolder = "captured";

    /// <summary>Worked examples. Readable, never scorable.</summary>
    internal const string ExampleFolder = "example";

    /// <summary><c>tests/fixtures/ft8</c>, found by walking up to the solution file.</summary>
    internal static string Root => Path.Combine(RepositoryRoot(), "tests", "fixtures", "ft8");

    /// <summary>Every fixture in one folder, ordinal by path so the order is not the file system's.</summary>
    internal static IReadOnlyList<string> PathsIn(string folder)
    {
        var directory = Path.Combine(Root, folder);
        if (!Directory.Exists(directory))
        {
            return [];
        }

        return Directory
            .GetFiles(directory, "*" + Ft8CaptureFixture.Extension, SearchOption.TopDirectoryOnly)
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>The repository root: the folder above the tests, carrying <c>Hamlet.sln</c>.</summary>
    internal static string RepositoryRoot()
    {
        var here = new DirectoryInfo(AppContext.BaseDirectory);
        while (here is not null && !File.Exists(Path.Combine(here.FullName, "Hamlet.sln")))
        {
            here = here.Parent;
        }

        return here?.FullName
            ?? throw new InvalidOperationException(
                $"No Hamlet.sln above {AppContext.BaseDirectory}, so the repository root cannot be "
                + "found and the committed fixtures cannot be located.");
    }
}
