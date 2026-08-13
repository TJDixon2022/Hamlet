namespace Hamlet.RadioEngine.Civ;

/// <summary>
/// CI-V protocol constants for the IC-7300.
/// </summary>
/// <remarks>
/// VERIFICATION STATUS (CLAUDE.md §4, HM-OPEN-002): every value here is
/// carried from general knowledge of Icom's CI-V protocol and is UNVERIFIED
/// against the IC-7300 Full Manual. Verified values will cite the vendored
/// manual pages in data/vendor/. Do not trust these bytes on a live rig
/// until that citation exists.
/// </remarks>
public static class CivConstants
{
    /// <summary>Frame preamble byte, sent twice. [unverified]</summary>
    public const byte Preamble = 0xFE;

    /// <summary>Frame terminator. [unverified]</summary>
    public const byte EndOfMessage = 0xFD;

    /// <summary>IC-7300 factory-default CI-V address. Menu-configurable on
    /// the radio; must match HM-OPEN-003 station config. [unverified]</summary>
    public const byte DefaultRadioAddress = 0x94;

    /// <summary>Conventional controller (this app) address. [unverified]</summary>
    public const byte DefaultControllerAddress = 0xE0;

    /// <summary>Broadcast address used by the radio's transceive
    /// notifications. [unverified]</summary>
    public const byte BroadcastAddress = 0x00;

    /// <summary>Unsolicited transceive frequency report. [unverified]</summary>
    public const byte CmdTransceiveFrequency = 0x00;

    /// <summary>Read operating frequency. [unverified]</summary>
    public const byte CmdReadFrequency = 0x03;

    /// <summary>Set operating frequency. [unverified]</summary>
    public const byte CmdSetFrequency = 0x05;

    /// <summary>Send CW message text (radio keys it). Believed limited to 30
    /// characters per message; 0xFF aborts. NOT used until verified —
    /// transmit paths bind hardest under §0.2. [unverified]</summary>
    public const byte CmdSendCwMessage = 0x17;

    /// <summary>Spectrum scope data family (HM-DEC-005). [unverified]</summary>
    public const byte CmdScope = 0x27;

    /// <summary>Positive acknowledgement from the radio. [unverified]</summary>
    public const byte ResultOk = 0xFB;

    /// <summary>Negative acknowledgement from the radio. [unverified]</summary>
    public const byte ResultNg = 0xFA;
}
