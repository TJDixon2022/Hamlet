using System;

namespace Ft8Sharp.Message;

/// <summary>
/// The two small fields at the top of every 77-bit message that say what the other bits mean.
/// </summary>
/// <remarks>
/// <para>
/// <b>Ported from <c>ft8/message.c</c> in the pinned clone</b>, functions <c>ftx_message_get_i3</c>,
/// <c>ftx_message_get_n3</c> and <c>ftx_message_get_type</c>. The primary selector sits in the last
/// byte of the message; the secondary one, which only means anything when the primary is zero, is
/// assembled from bits in the last two bytes.
/// </para>
/// <para>
/// <b>Every combination of the two has a defined answer and none of them throws.</b> There are
/// <see cref="CombinationCount"/> of them and the tests walk all of them, because a type field is
/// small enough to finish rather than sample. A combination this library has not built decodes to
/// <see cref="Ft8MessageType.Unknown"/>, which the dispatcher turns into a refusal — never into a
/// message.
/// </para>
/// <para>
/// <b>One upstream oddity is inherited rather than repaired, and it is reported.</b> The pin's own
/// enumeration declares a contest type for one of the secondary codes, and the pin's own type
/// function does not return it: that code falls through to the unknown branch. This port does the
/// same thing, because the code that goes on the air is the type function's and not the
/// enumeration's. <see cref="Ft8MessageType.Contesting"/> therefore exists and is never produced,
/// which is exactly the state upstream is in.
/// </para>
/// </remarks>
public static class Ft8MessageTypes
{
    /// <summary>The width of the primary type selector.</summary>
    public const int PrimaryBits = 3;

    /// <summary>The width of the secondary selector, which only applies under primary code zero.</summary>
    public const int SecondaryBits = 3;

    /// <summary>How many values the primary selector can take.</summary>
    public const int PrimaryCount = 1 << PrimaryBits;

    /// <summary>How many values the secondary selector can take.</summary>
    public const int SecondaryCount = 1 << SecondaryBits;

    /// <summary>
    /// Every combination of the two selectors: the secondary applies under one primary code only,
    /// so the total is that code's fan-out plus the remaining primary codes.
    /// </summary>
    public const int CombinationCount = SecondaryCount + (PrimaryCount - 1);

    /// <summary>The primary code under which the secondary selector applies.</summary>
    public const int PrimaryFreeTextFamily = 0;

    /// <summary>The primary code of a standard message with no suffix or a <c>/R</c> suffix.</summary>
    public const int PrimaryStandard = 1;

    /// <summary>The primary code of a standard message carrying a <c>/P</c> suffix.</summary>
    public const int PrimaryStandardWithP = 2;

    /// <summary>The primary selector of a packed message.</summary>
    /// <remarks>Never throws: any ten bytes have one.</remarks>
    public static int Primary(ReadOnlySpan<byte> message) => (message[9] >> 3) & 0x07;

    /// <summary>The secondary selector of a packed message, which means nothing unless the primary is zero.</summary>
    /// <remarks>Never throws: any ten bytes have one.</remarks>
    public static int Secondary(ReadOnlySpan<byte> message) =>
        ((message[8] << 2) & 0x04) | ((message[9] >> 6) & 0x03);

    /// <summary>The message type a packed message declares itself to be.</summary>
    public static Ft8MessageType TypeOf(ReadOnlySpan<byte> message) =>
        TypeOf(Primary(message), Secondary(message));

    /// <summary>
    /// The message type a pair of selector codes names, for every pair including the ones the
    /// protocol leaves undefined.
    /// </summary>
    /// <remarks>
    /// <b>Total over every pair of integers</b>, not just the in-range ones: an out-of-range code
    /// answers <see cref="Ft8MessageType.Unknown"/> rather than throwing, so that a caller sweeping
    /// the whole combination space needs no bounds of its own.
    /// </remarks>
    public static Ft8MessageType TypeOf(int primary, int secondary) => primary switch
    {
        PrimaryFreeTextFamily => secondary switch
        {
            0 => Ft8MessageType.FreeText,
            1 => Ft8MessageType.DxPedition,
            2 => Ft8MessageType.EuVhfContest,
            3 or 4 => Ft8MessageType.ArrlFieldDay,
            5 => Ft8MessageType.Telemetry,

            // Codes 6 and 7 fall through here in the pin as well, and 6 is the one the pin's own
            // enumeration names a contest type for. Inherited, not repaired.
            _ => Ft8MessageType.Unknown,
        },
        PrimaryStandard or PrimaryStandardWithP => Ft8MessageType.Standard,
        3 => Ft8MessageType.ArrlRttyRoundup,
        4 => Ft8MessageType.NonstandardCallsign,
        5 => Ft8MessageType.WwrofContest,
        _ => Ft8MessageType.Unknown,
    };

    /// <summary>Whether this library can turn a message of this type into text.</summary>
    /// <remarks>
    /// <b>The list is short on purpose and the answer for everything else is no.</b> A type that is
    /// not built is refused as unsupported, which is a correct answer; returning a decode for it
    /// would not be.
    /// </remarks>
    public static bool IsSupported(Ft8MessageType type) => type switch
    {
        Ft8MessageType.Standard => true,
        Ft8MessageType.FreeText => true,
        Ft8MessageType.Telemetry => true,
        _ => false,
    };
}

/// <summary>The message types the protocol defines, named as the pinned clone names them.</summary>
public enum Ft8MessageType
{
    /// <summary>Thirteen characters of free text.</summary>
    FreeText,

    /// <summary>DXpedition mode.</summary>
    DxPedition,

    /// <summary>The European VHF contest exchange carried under the free-text family.</summary>
    EuVhfContest,

    /// <summary>ARRL Field Day.</summary>
    ArrlFieldDay,

    /// <summary>Eighteen hexadecimal digits of telemetry.</summary>
    Telemetry,

    /// <summary>
    /// Contesting. Declared by the pin's enumeration and never returned by the pin's type
    /// function, and never returned by this library either.
    /// </summary>
    Contesting,

    /// <summary>Two callsigns and a grid, report or token — most of what is on the band.</summary>
    Standard,

    /// <summary>ARRL RTTY Roundup.</summary>
    ArrlRttyRoundup,

    /// <summary>One non-standard callsign and one hashed one.</summary>
    NonstandardCallsign,

    /// <summary>The WWROF contest exchange.</summary>
    WwrofContest,

    /// <summary>A selector combination the protocol does not define.</summary>
    Unknown,
}
