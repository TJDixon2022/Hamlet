using System.Text;
using Hamlet.RadioEngine.Contacts;

namespace Hamlet.App.Settings;

/// <summary>
/// The operator's contact log, one ADI file beside his settings.
/// </summary>
/// <remarks>
/// <para>**BESIDE `settings.json`, IN `%AppData%\Hamlet`**, through
/// <see cref="SettingsStore.DataFolder"/> — the seam unit 235 added after nine
/// tests rewrote the operator's own settings. Reading the folder on every call
/// rather than capturing it once is what makes it redirectable, and it is why no
/// test can ever append to his real log.</para>
/// <para>**IT APPENDS AND IT NEVER REWRITES.** A logged contact is a record of
/// something that happened; editing one is its own decision and is parked. A
/// crash mid-write costs the record being written and nothing before it.</para>
/// <para>**THE HEADER IS WRITTEN ONCE, WHEN THE FILE IS CREATED.** An ADI file
/// carries one header; a second one appended later would be read as a record with
/// a version number in it.</para>
/// <para>**NOTHING HERE TRANSMITS.** Logging is bookkeeping and does not touch the
/// send path at all.</para>
/// </remarks>
public static class ContactLogStore
{
    /// <summary>`%AppData%\Hamlet\contacts.adi`.</summary>
    /// <remarks>
    /// **`.adi` AND NOT `.adif`.** The ADI data format's own extension, which is
    /// what a logger's import dialog filters on.
    /// </remarks>
    public static string LogPath => Path.Combine(SettingsStore.DataFolder, "contacts.adi");

    /// <summary>Append one contact, creating the file and its header if needed.</summary>
    /// <param name="contact">The contact.</param>
    /// <param name="programVersion">Hamlet's version, for the header.</param>
    /// <returns>True when it was written.</returns>
    /// <remarks>
    /// **A FAILED WRITE IS REPORTED AND NEVER THROWN PAST THE CALLER** (§8's
    /// never-throw discipline). Losing a log entry is bad; taking the application
    /// down while the radio is transmitting is worse. The caller says so on screen.
    /// </remarks>
    public static bool Append(AdifContact contact, string programVersion)
    {
        ArgumentNullException.ThrowIfNull(contact);

        try
        {
            var path = LogPath;
            var folder = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var text = new StringBuilder();

            if (!File.Exists(path))
            {
                text.Append(AdifLog.Header(programVersion));
            }

            text.Append(AdifLog.Record(contact));

            File.AppendAllText(path, text.ToString(), new UTF8Encoding(false));

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>Every contact in the log, or an empty list.</summary>
    /// <remarks>
    /// **AN UNREADABLE LOG IS AN EMPTY ONE HERE AND SAYS SO NOWHERE.** That is
    /// deliberate and narrow: the only thing reading this is the already-worked
    /// mark, and a mark that fails to appear is a station he might work twice —
    /// which he is free to do. It is not the same as a decode going missing.
    /// </remarks>
    public static IReadOnlyList<AdifContact> Read()
    {
        try
        {
            var path = LogPath;

            return File.Exists(path)
                ? AdifLog.Read(File.ReadAllText(path))
                : [];
        }
        catch (Exception)
        {
            return [];
        }
    }
}
