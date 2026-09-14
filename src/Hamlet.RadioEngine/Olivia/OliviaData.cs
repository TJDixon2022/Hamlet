using Hamlet.RadioEngine.Rsid;

namespace Hamlet.RadioEngine.Olivia;

/// <summary>
/// **The two cited files the Olivia mode stands on, read once, with a sentence where either
/// could not be read.**
/// </summary>
/// <remarks>
/// <para>**READ AT STARTUP, AND NEVER GUESSED** (`PHASE_PLAN.md` step 0 criterion 0.4). Both
/// files are embedded in the engine the way `us-neighborhoods.json` is, so they ship with the
/// application, and <see cref="Current"/> reads them the first time anything asks. A file
/// that is missing or malformed leaves its value null and puts a sentence in
/// <see cref="Problem"/> naming the file and what was wrong; nothing fills the gap.</para>
/// <para>**THE ENGINE NEVER LEARNS TABS EXIST** (§0.1). This says what the files hold and
/// whether they could be read. Where the sentence is shown is the shell's business.</para>
/// </remarks>
public sealed class OliviaData
{
    /// <summary>Resource name of the embedded calling table.</summary>
    public const string CallingResourceName = "Hamlet.RadioEngine.Data.Bands.olivia-calling.json";

    /// <summary>Resource name of the embedded RSID codes.</summary>
    public const string RsidResourceName = "Hamlet.RadioEngine.Data.Rsid.rsid-codes.json";

    private static readonly Lazy<OliviaData> Shared = new(LoadEmbedded);

    private OliviaData(OliviaCallingTable? calling, RsidCodes? rsid, string? problem)
    {
        Calling = calling;
        Rsid = rsid;
        Problem = problem;
    }

    /// <summary>What the shipped files hold.</summary>
    public static OliviaData Current => Shared.Value;

    /// <summary>The calling table, or null where it could not be read.</summary>
    public OliviaCallingTable? Calling { get; }

    /// <summary>The RSID codes, or null where they could not be read.</summary>
    public RsidCodes? Rsid { get; }

    /// <summary>
    /// One sentence per file that could not be read, or null where both were.
    /// </summary>
    public string? Problem { get; }

    /// <summary>Read the two files from their text.</summary>
    /// <param name="callingJson">The calling table's contents, or null where the file is missing.</param>
    /// <param name="rsidJson">The RSID codes' contents, or null where the file is missing.</param>
    /// <returns>What could be read, and a sentence about what could not.</returns>
    /// <remarks>
    /// **ONE BAD FILE DOES NOT TAKE THE OTHER WITH IT.** They are separate facts: a broken
    /// RSID table leaves the calling spot usable, and the sentence names only the file that
    /// failed.
    /// </remarks>
    public static OliviaData Read(string? callingJson, string? rsidJson)
    {
        var problems = new List<string>();

        OliviaCallingTable? calling = null;

        if (callingJson is null)
        {
            problems.Add(
                "Hamlet's Olivia calling table, " + OliviaCallingTable.FilePath
                + ", is missing from this build, so there is no Olivia spot to tune to and none is guessed.");
        }
        else
        {
            try
            {
                calling = OliviaCallingTable.Parse(callingJson);
            }
            catch (InvalidDataException error)
            {
                problems.Add(
                    "Hamlet could not read its Olivia calling table, " + OliviaCallingTable.FilePath
                    + ", because " + error.Message
                    + ", so there is no Olivia spot to tune to and none is guessed.");
            }
        }

        RsidCodes? rsid = null;

        if (rsidJson is null)
        {
            problems.Add(
                "Hamlet's RSID codes, " + RsidCodes.FilePath
                + ", are missing from this build, so no mode announcement can be named.");
        }
        else
        {
            try
            {
                rsid = RsidCodes.Parse(rsidJson);
            }
            catch (InvalidDataException error)
            {
                problems.Add(
                    "Hamlet could not read its RSID codes, " + RsidCodes.FilePath
                    + ", because " + error.Message
                    + ", so no mode announcement can be named.");
            }
        }

        return new OliviaData(
            calling,
            rsid,
            problems.Count == 0 ? null : string.Join(" ", problems));
    }

    private static OliviaData LoadEmbedded()
        => Read(Embedded(CallingResourceName), Embedded(RsidResourceName));

    private static string? Embedded(string resourceName)
    {
        using var stream = typeof(OliviaData).Assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            return null;
        }

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
