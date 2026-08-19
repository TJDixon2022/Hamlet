namespace Hamlet.RadioEngine.Rig;

/// <summary>
/// How the CI-V conversation itself is going (HM-DEC-092).
/// </summary>
/// <param name="PortName">Which port, or "" when nothing is open.</param>
/// <param name="BaudRate">The rate Hamlet opened it at, or 0.</param>
/// <param name="Sent">Commands sent since the link came up.</param>
/// <param name="Answered">How many of those the radio answered.</param>
/// <param name="Unanswered">How many it did not.</param>
/// <param name="LastUnansweredCommand">
/// The command byte of the most recent unanswered one, or null.
/// </param>
/// <param name="LastUnansweredUtc">When that was, or null.</param>
/// <param name="Inbound">
/// Every frame the reader completed, counted before anything filtered it.
/// </param>
/// <param name="InboundFromRadio">
/// How many of those came from the radio's own address.
/// </param>
/// <param name="InboundBroadcast">
/// How many were addressed to nobody in particular — destination `00`, which is
/// what the radio uses to announce a change the operator made at the front panel.
/// </param>
/// <param name="InboundTransceive">
/// How many carried a transceive command byte: `00` for the dial, `01` for the
/// mode knob.
/// </param>
/// <param name="InboundScope">
/// How many carried the scope's own command byte, `27`. **The one that can drown
/// the others**: a waveform sweep is eleven frames of about fifty bytes and they
/// arrive continuously once the radio is asked for them, on the same cable as
/// everything else.
/// </param>
/// <param name="InboundBytes">How many bytes those frames carried in total.</param>
/// <param name="LastInboundUtc">When the last frame of any kind arrived, or null.</param>
/// <param name="LastBroadcastUtc">When the last broadcast arrived, or null.</param>
/// <remarks>
/// <para>**THE DIAGNOSTICS SCREEN READ FORTY VALUES AND SAID NOTHING ABOUT THE
/// CONVERSATION CARRYING THEM.** Five settings were written one evening, all five
/// reported as failed for want of an answer, and at least two of them had
/// actually taken effect. The operator was being told things about his own radio
/// that were not true, and a visible count of unanswered commands would have
/// shown it in a glance.</para>
/// <para>**AND IT MATTERS BEYOND ANY ONE COMMAND ON THIS STATION.** Radio
/// frequency energy from the operator's own transmissions knocks USB devices off
/// the bus: the mouse and keyboard drop and come back. The CI-V link is on that
/// same bus. A link that stops answering during a send is expected here until
/// ferrites are fitted, and "the radio stopped answering while you were
/// transmitting" is a diagnosis nobody reaches on their own.</para>
/// </remarks>
public readonly record struct CivLinkHealth(
    string PortName,
    int BaudRate,
    long Sent,
    long Answered,
    long Unanswered,
    byte? LastUnansweredCommand,
    DateTime? LastUnansweredUtc,
    long Inbound = 0,
    long InboundFromRadio = 0,
    long InboundBroadcast = 0,
    long InboundTransceive = 0,
    long InboundScope = 0,
    long InboundBytes = 0,
    DateTime? LastInboundUtc = null,
    DateTime? LastBroadcastUtc = null)
{
    /// <summary>Nothing known.</summary>
    public static CivLinkHealth Unknown { get; } = new("", 0, 0, 0, 0, null, null);

    /// <summary>
    /// True once the radio has volunteered at least one change.
    /// </summary>
    /// <remarks>
    /// <para>**THE QUESTION NOTHING IN THIS APPLICATION COULD ANSWER.** Whether
    /// the radio pushes its own changes decides whether the frequency on screen
    /// follows the dial in a hundred milliseconds or in thirty seconds, and the
    /// only instrument pointed at it was a telemetry field that says "read" for
    /// a broadcast and "read" for a poll. So a session counted zero broadcasts
    /// and concluded the path was dead, on a vocabulary that returns zero for a
    /// working one.</para>
    /// <para>Counted at the reader, before the dispatcher, before any address or
    /// command test, so a frame that arrives and is then discarded is still
    /// visible here. **Null rather than false when nothing has arrived at all**:
    /// a link that has had no traffic and a radio that is not broadcasting are
    /// different facts (HM-DEC-050).</para>
    /// </remarks>
    public bool? IsRadioBroadcasting
        => Inbound == 0 ? null : InboundTransceive > 0;

    /// <summary>
    /// What share of the traffic is the scope's, or null before anything came.
    /// </summary>
    /// <remarks>
    /// **A CABLE CARRIES ONE CONVERSATION.** The scope's waveform output was
    /// asked for automatically at connect from the build that introduced it
    /// (HM-DEC-092), and the acknowledgement that would have said whether the
    /// radio took it could not be read back (HM-OPEN-042) — so Hamlet has been
    /// reporting that write as failed without knowing. If it succeeded, this is
    /// where it shows: a flood of `27` frames between every other answer, on a
    /// link the dial's own announcements share.
    /// </remarks>
    public double? ScopeShare
        => Inbound == 0 ? null : (double)InboundScope / Inbound;

    /// <summary>The share of commands that came back, or null before any went.</summary>
    public double? AnsweredShare => Sent == 0 ? null : (double)Answered / Sent;

    /// <summary>True when the link is answering everything it is asked.</summary>
    public bool IsHealthy => Unanswered == 0;

    /// <summary>
    /// The rate the scope's data output requires (p. 19-7, footnote 4).
    /// </summary>
    /// <remarks>
    /// One of the two preconditions on `27 11`. **This one Hamlet does not have
    /// to ask the radio about**, because it opened the port itself and knows what
    /// rate it opened it at. The other, the CI-V USB Port setting, it cannot read
    /// today (HM-OPEN-013).
    /// </remarks>
    public const int ScopeOutputBaudRate = 115_200;

    /// <summary>True when the link is fast enough for the scope's output.</summary>
    /// <remarks>
    /// Null rather than false when no port is open, because not having looked and
    /// having looked and found it wrong are different facts (HM-DEC-050).
    /// </remarks>
    public bool? FastEnoughForScope
        => BaudRate == 0 ? null : BaudRate >= ScopeOutputBaudRate;
}
