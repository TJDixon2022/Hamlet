namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// Which name the terminal gives a pattern that has both a prosign name and a
/// punctuation name (HM-REQ-072).
/// </summary>
/// <remarks>
/// <para>NAMING, NOT A CLAIM ABOUT THE SIGNAL (§0.0). <c>-...-</c> is the same
/// sound whether the sender meant "BT" or "=", and <c>.-.-.</c> the same for
/// "AR" and "+", so the decoder emits one symbol for each and this only chooses
/// what it is called on the screen. `CW_SPEC.md` 6.2 names exactly those two
/// patterns as two-named.</para>
/// </remarks>
public enum CwProsignNaming
{
    /// <summary><c>&lt;BT&gt;</c> and <c>&lt;AR&gt;</c>, which is what the terminal has always shown.</summary>
    Prosign,

    /// <summary><c>=</c> and <c>+</c>.</summary>
    Punctuation,
}
