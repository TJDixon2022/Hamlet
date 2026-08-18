using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Whether a CQ was answered, and what a message may be before it goes out
/// (phases 1 and 3).
/// </summary>
/// <remarks>
/// <para>**BIASED TOWARD STOPPING, AND THAT BIAS IS THE POINT.** The cost of a
/// false stop is that the operator presses start again. The cost of a missed
/// answer is Hamlet transmitting a CQ over the top of somebody's reply, under his
/// callsign, on a frequency they are both trying to use.</para>
/// <para>Nothing here transmits or touches a radio: characters in, a verdict
/// out (§5).</para>
/// </remarks>
public sealed class AutoCallAnswerTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the verdicts are printed.</param>
    public AutoCallAnswerTests(ITestOutputHelper output) => _output = output;

    private const string MyCall = "CQ CQ DE W1AW W1AW K";

    private static IReadOnlyList<CwCharacter> Heard(string text, double score = 0.95)
        => text.Select(c => c == ' '
                ? new CwCharacter(
                    MorseAlphabet.WordGap, CwConfidence.High, 1, "", 25, 18, TimeSpan.Zero)
                : c == '■'
                    ? new CwCharacter(
                        MorseAlphabet.Unreadable, CwConfidence.Unreadable, 0, "..",
                        25, 18, TimeSpan.Zero)
                    : new CwCharacter(
                        c.ToString(),
                        score >= 0.7 ? CwConfidence.High : CwConfidence.Low,
                        score, ".-", 25, 18, TimeSpan.Zero))
            .ToList();

    private AutoCallAnswer Judge(string text, double score = 0.95)
    {
        var answer = AutoCallAnswers.Judge(Heard(text, score), MyCall);

        _output.WriteLine($"'{text}' -> stop {answer.Stop}, answer {answer.IsAnswer}"
            + $", {answer.Confidence:0.00}: {answer.Why}");

        return answer;
    }

    // ---- the first tier: shaped like an answer ---------------------------

    /// <remarks>
    /// <para>**THE STRONGEST THING THIS CAN SEE.** Nobody else on the band has a
    /// reason to send this operator's callsign, so it coming back is as close to
    /// proof of an answer as a decoder gets.</para>
    /// </remarks>
    [Fact]
    public void HisOwnCallsignComingBackIsAnAnswer()
    {
        var answer = Judge("W1AW DE K2ABC");

        Assert.True(answer.Stop);
        Assert.True(answer.IsAnswer);
        Assert.Contains("W1AW", answer.Why, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>`DE` and a callsign-shaped token is somebody naming themselves
    /// straight after a call went out.</para>
    /// <para>**AND IT NAMES NO CALLSIGN** (HM-DEC-073). Stopping a transmitter
    /// does not need every character solid; putting a name on screen does, and
    /// this puts none there.</para>
    /// </remarks>
    [Fact]
    public void AStationNamingItselfIsAnAnswerAndIsNotNamed()
    {
        var answer = Judge("QRZ DE K2ABC");

        Assert.True(answer.Stop);
        Assert.True(answer.IsAnswer);
        Assert.DoesNotContain("K2ABC", answer.Why, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Somebody handing it back — `K`, `R`, `73` — is what the end of a
    /// transmission aimed at you sounds like.
    /// </remarks>
    [Theory]
    [InlineData("PSE K")]
    [InlineData("R R")]
    [InlineData("TU 73")]
    public void SomebodyHandingItBackIsAnAnswer(string text)
    {
        var answer = Judge(text);

        Assert.True(answer.Stop);
        Assert.True(answer.IsAnswer);
    }

    /// <remarks>
    /// A run of characters coming round twice is what a station calling you
    /// repeatedly sounds like, and repetition is the one structure that survives
    /// a decode too poor to read: two bad readings of the same thing are bad in
    /// the same way.
    /// </remarks>
    [Fact]
    public void TheSameRunTwiceIsAnAnswer()
    {
        var answer = Judge("XQPZ XQPZ");

        Assert.True(answer.Stop);
        Assert.True(answer.IsAnswer);
    }

    // ---- the second tier: heard, and not an answer -----------------------

    /// <remarks>
    /// <para>**THESE ARE DIFFERENT CLAIMS AND MUST LOOK DIFFERENT** (§0.0). Four
    /// confident characters that mean nothing to Hamlet say the frequency is not
    /// empty and say nothing whatever about whether anybody replied. It stops —
    /// calling over a station that is transmitting is the failure this feature has
    /// to avoid — and it does not claim an answer.</para>
    /// </remarks>
    [Fact]
    public void ConfidentTextThatIsNotAnAnswerStopsWithoutClaimingOne()
    {
        var answer = Judge("MNQX WXYZ");

        Assert.True(answer.Stop);
        Assert.False(answer.IsAnswer);
        Assert.Contains("could not make anything of", answer.Why, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>**AND DIM TEXT DOES NOT STOP IT.** A window of letters the decoder
    /// would not stand behind is a signal Hamlet could not read, which happens all
    /// evening on a fading band. A cycle that stopped on every one would never
    /// finish a round, and the operator would learn to ignore the stop.</para>
    /// </remarks>
    [Fact]
    public void TextTheDecoderWouldNotStandBehindDoesNotStopIt()
    {
        var answer = Judge("MNQX WXYZ", score: 0.25);

        Assert.False(answer.Stop);
        Assert.False(answer.IsAnswer);
    }

    /// <remarks>
    /// Two or three confident letters is what a decoder produces out of noise, so
    /// unrecognized text needs a few before it counts.
    /// </remarks>
    [Fact]
    public void ACoupleOfLettersIsNotEnoughToStopIt()
    {
        Assert.False(Judge("MN").Stop);
    }

    /// <remarks>
    /// Nothing heard is not an answer, and it is not a stop either.
    /// </remarks>
    [Fact]
    public void AnEmptyWindowIsNotAnAnswer()
    {
        var answer = AutoCallAnswers.Judge(Array.Empty<CwCharacter>(), MyCall);

        Assert.False(answer.Stop);
        Assert.False(answer.IsAnswer);
    }

    /// <remarks>
    /// <para>**A HOLE IN A WORD IS NOT A WORD** (§0.0). A placeholder splits the
    /// token rather than welding what is either side of it into something nobody
    /// sent, which is how a callsign gets manufactured out of two fragments.</para>
    /// </remarks>
    [Fact]
    public void APlaceholderDoesNotWeldTwoFragmentsIntoACallsign()
    {
        // W1 and AW either side of a character the decoder could not resolve.
        // Welded that would read as the operator's own callsign coming back.
        var answer = AutoCallAnswers.Judge(Heard("W1■AW"), MyCall);

        _output.WriteLine($"stop {answer.Stop}, answer {answer.IsAnswer}: {answer.Why}");

        Assert.False(answer.IsAnswer);
    }

    /// <remarks>
    /// <para>Proves the callsign is read out of what the operator asked Hamlet to
    /// send, so the two can never disagree. What matters is the call that actually
    /// went on the air, and the message is the only thing that knows it.</para>
    /// </remarks>
    [Theory]
    [InlineData("CQ CQ DE W1AW W1AW K", "W1AW")]
    [InlineData("CQ DE KC3QIS K", "KC3QIS")]
    [InlineData("CQ DE VA3VRR/QRP K", "VA3VRR/QRP")]
    [InlineData("CQ CQ CQ K", "")]
    [InlineData("", "")]
    public void TheOwnCallsignComesOutOfHisOwnMessage(string message, string expected)
    {
        var found = AutoCallAnswers.OwnCallsign(message);

        _output.WriteLine($"'{message}' -> '{found}'");

        Assert.Equal(expected, found);
    }

    // ---- phase 1: what a message may be, at edit time --------------------

    /// <remarks>
    /// <para>**THE OPERATOR'S OWN CALL, WHICH FITS BY FIVE CHARACTERS.**
    /// `CQ CQ DE KC3QIS KC3QIS K` is 24, and the keyer takes 30. That is inside
    /// and not by much, which is the reason the length is checked where he can see
    /// it rather than discovered on the air.</para>
    /// </remarks>
    [Fact]
    public void HisOwnCallFitsInOneKeyerMessage()
    {
        var settings = new AutoCallSettings("CQ CQ DE KC3QIS KC3QIS K");

        _output.WriteLine($"{CwMessage.Clean(settings.Message).Length} characters, "
            + $"refusal '{settings.Refusal}'");

        Assert.True(settings.IsUsable);
        Assert.Equal("", settings.Refusal);
    }

    /// <remarks>
    /// <para>**A MESSAGE TOO LONG FAILS AT EDIT TIME AND SAYS THE COUNT.** The
    /// radio's keyer takes thirty characters in one message; a longer one would go
    /// out cut short, under his callsign, and the first he would know of it is
    /// somebody asking him to repeat.</para>
    /// <para>It is refused rather than split, which is a deliberate difference
    /// from the single-send path: splitting a repeating call across two keyer
    /// messages puts a gap of unknown length in the middle of every round, and how
    /// that should work is a design question rather than something to assume.
    /// </para>
    /// </remarks>
    [Fact]
    public void AMessageTooLongForTheKeyerIsRefusedWithItsCount()
    {
        var settings = new AutoCallSettings(
            "CQ CQ CQ DE KC3QIS KC3QIS KC3QIS PSE K");

        _output.WriteLine($"refusal: {settings.Refusal}");

        Assert.False(settings.IsUsable);
        Assert.Contains("38", settings.Refusal, StringComparison.Ordinal);
        Assert.Contains("30", settings.Refusal, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>**AND AN EMPTY MESSAGE IS REFUSED RATHER THAN FILLED IN.** No session
    /// may ever invent the content of a transmission that goes out under his
    /// callsign, so there is no default and the refusal says so.</para>
    /// </remarks>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!!!")]
    public void AnEmptyMessageIsRefusedAndNothingIsWrittenForHim(string text)
    {
        var settings = new AutoCallSettings(text);

        _output.WriteLine($"'{text}' -> {settings.Refusal}");

        Assert.False(settings.IsUsable);
        Assert.Contains("does not write one for you", settings.Refusal, StringComparison.Ordinal);
    }

    /// <remarks>
    /// A round shorter than the message does not leave time for it to finish
    /// before the next one is due, so the cycle would call over its own tail.
    /// </remarks>
    [Fact]
    public void ARoundTooShortForTheMessageIsRefused()
    {
        var settings = new AutoCallSettings(MyCall, IntervalSeconds: 3);

        _output.WriteLine($"refusal: {settings.Refusal}");

        Assert.False(settings.IsUsable);
    }

    /// <remarks>
    /// Ruled defaults, pinned here so a later edit has to be deliberate: thirty
    /// seconds a round and ten rounds, which is five minutes of calling.
    /// </remarks>
    [Fact]
    public void TheRuledDefaultsAreThirtySecondsAndTenRounds()
    {
        var settings = new AutoCallSettings(MyCall);

        Assert.Equal(30, settings.IntervalSeconds);
        Assert.Equal(10, settings.MaxRounds);
        Assert.True(settings.IsUsable);
    }

    /// <remarks>
    /// A round count nobody would run unattended is refused at both ends: none at
    /// all, and an hour of calling.
    /// </remarks>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public void ARoundCountNobodyWouldRunUnattendedIsRefused(int rounds)
    {
        var settings = new AutoCallSettings(MyCall, MaxRounds: rounds);

        _output.WriteLine($"{rounds} rounds -> {settings.Refusal}");

        Assert.False(settings.IsUsable);
    }
}
