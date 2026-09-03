namespace Hamlet.App.Telemetry;

/// <summary>
/// Why a digital capture press produced no files.
/// </summary>
/// <remarks>
/// <para>**FOUR WAYS TO PRODUCE NOTHING, AND UNTIL UNIT 234 ALL FOUR WERE
/// SILENT.** Each one left a sentence in the status bar and the next status
/// message overwrote it, so a press that refused was unrecoverable an hour
/// later. On the morning of 2026-09-03 the owner pressed this button at the
/// radio, nothing appeared, and no artefact anywhere on the machine could say
/// which of these four it had been — or whether the button had been pressed at
/// all.</para>
/// <para>**IT IS AN ENUM SO THAT THE SIGNATURE ENFORCES HM-DEC-018**, in the
/// manner unit 233 used for <c>Ft8SlotCensus</c>. The two exception paths could
/// have carried the exception's message, and an exception message from
/// <see cref="System.IO.File"/> contains a file path, and a Windows file path
/// contains a person's name. So the reason is a closed set of four values with
/// no member that can hold a character, and the call site has nothing to
/// remember.</para>
/// <para>**THE TWO EXCEPTION MEMBERS ARE NAMED FOR THEIR EXCEPTION TYPES**,
/// which is how the type name reaches the file without a string ever being
/// passed.</para>
/// </remarks>
public enum DigitalCaptureRefusal
{
    /// <summary>No audio source was open, so there was no tap to read.</summary>
    NothingIsListening,

    /// <summary>A source is open but no samples have arrived yet.</summary>
    NoAudioYet,

    /// <summary>The write failed with an <see cref="System.IO.IOException"/>.</summary>
    IOException,

    /// <summary>
    /// The write failed with an
    /// <see cref="System.UnauthorizedAccessException"/>.
    /// </summary>
    UnauthorizedAccessException,
}
