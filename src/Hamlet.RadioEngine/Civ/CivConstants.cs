namespace Hamlet.RadioEngine.Civ;

/// <summary>
/// CI-V protocol constants for the IC-7300.
/// </summary>
/// <remarks>
/// VERIFICATION STATUS (CLAUDE.md §4, HM-DEC-049): the frame bytes, the
/// addresses, the result codes and the commands below were read off the Full
/// Manual's section 19 and carry their page. The manual is cited and never
/// committed, because Icom permits individual use and prohibits
/// redistribution. The state reads live in <see cref="CivReads"/> with a page
/// on every row.
/// </remarks>
public static class CivConstants
{
    /// <summary>Frame preamble byte, sent twice. (p. 19-2)</summary>
    public const byte Preamble = 0xFE;

    /// <summary>Frame terminator. (p. 19-2)</summary>
    public const byte EndOfMessage = 0xFD;

    /// <summary>IC-7300 factory-default CI-V address, settable 02h to DFh on the
    /// radio; the station's own setting is HM-OPEN-003. (p. 12-10)</summary>
    public const byte DefaultRadioAddress = 0x94;

    /// <summary>Conventional controller (this app) address. (p. 19-2)</summary>
    public const byte DefaultControllerAddress = 0xE0;

    /// <summary>Broadcast address used by the radio's transceive
    /// notifications. (p. 19-2)</summary>
    public const byte BroadcastAddress = 0x00;

    /// <summary>Unsolicited transceive frequency report. (p. 19-3)</summary>
    public const byte CmdTransceiveFrequency = 0x00;

    /// <summary>
    /// Unsolicited transceive mode report: the operator turning the mode knob
    /// on the radio itself. Carries the mode byte and, usually, the filter
    /// designator. (p. 19-3)
    /// </summary>
    public const byte CmdTransceiveMode = 0x01;

    /// <summary>Read operating frequency. (p. 19-3)</summary>
    public const byte CmdReadFrequency = 0x03;

    /// <summary>Set operating frequency. (p. 19-3)</summary>
    public const byte CmdSetFrequency = 0x05;

    /// <summary>
    /// Send CW message text, which the radio keys itself. Up to 30 characters
    /// and 0xFF stops one in progress (p. 19-13); in CW mode nothing is
    /// transmitted unless TRANSMIT, an external TX switch, or Break-in is on
    /// (p. 19-8, footnote 2). NOT used yet: transmit binds hardest under §0.2
    /// and gets its own ruling.
    /// </summary>
    public const byte CmdSendCwMessage = 0x17;

    /// <summary>Spectrum scope data family (HM-DEC-005, p. 19-14).</summary>
    public const byte CmdScope = 0x27;

    /// <summary>Positive acknowledgement from the radio. (p. 19-2)</summary>
    public const byte ResultOk = 0xFB;

    /// <summary>Negative acknowledgement from the radio. (p. 19-2)</summary>
    public const byte ResultNg = 0xFA;
}
