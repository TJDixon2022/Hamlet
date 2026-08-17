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
    DateTime? LastUnansweredUtc)
{
    /// <summary>Nothing known.</summary>
    public static CivLinkHealth Unknown { get; } = new("", 0, 0, 0, 0, null, null);

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
