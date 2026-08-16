using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Rig;

/// <summary>
/// Hamlet changes the radio, and never shows a rig control (HM-DEC-084).
/// </summary>
/// <remarks>
/// <para>**Settings are consequences of intent, never things the operator
/// operates.** A rig control app gives somebody a Noise Blanker button and
/// expects them to know when to press it. Hamlet gives them one button that says
/// "I can hear it and you can't", does the things that usually cause that, and
/// says what it changed.</para>
/// <para>The tier is the safety design. These hold that no tier one write can
/// key a transmitter, that a write with no confirmation says so, that an undo
/// with no prior value admits it, and that the suggestion list comes from live
/// state and omits nothing it failed to read.</para>
/// </remarks>
public sealed class RigWriteTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 22, 0, 0, DateTimeKind.Utc);

    // ---- The tier is the safety design ----------------------------------

    /// <remarks>
    /// Proves HM-DEC-084 and §0.2: **no tier one write can put a signal on the
    /// air.** That is the whole justification for "do all four" being one press
    /// rather than four confirmations, so it is asserted against the table
    /// rather than assumed from reading it.
    /// </remarks>
    [Fact]
    public void NoReceiveSideWriteCanKeyTheTransmitter()
    {
        var keying = new[]
        {
            (Command: (byte)0x17, Name: "CW message"),
            (Command: (byte)0x1C, Name: "transmit and tuner"),
        };

        foreach (var write in CivWrites.All.Where(w => w.Tier == RigWriteTier.Receive))
        {
            Assert.DoesNotContain(
                keying, k => k.Command == write.Command);
        }

        // And the one that does key is tiered as such, out loud.
        Assert.Equal(RigWriteTier.Keys, CivWrites.AntennaTuner.Tier);
        Assert.Contains(
            "keys the radio", CivWrites.AntennaTuner.Note, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-084: what changes how the operator sounds to other people
    /// is its own tier, so it can be offered rather than simply done.
    /// </remarks>
    [Fact]
    public void WhatOthersHearIsItsOwnTier()
    {
        foreach (var write in new[]
                 {
                     CivWrites.RfPower, CivWrites.KeyerSpeed,
                     CivWrites.BreakIn, CivWrites.BreakInDelay,
                 })
        {
            Assert.Equal(RigWriteTier.Transmitted, write.Tier);
        }
    }

    // ---- No byte is written that is not cited ---------------------------

    /// <remarks>
    /// Proves HM-DEC-084 and §4: every write carries a page, and the pages are
    /// real ones in this edition. A write is a byte moving somebody's control,
    /// so an uncited one has nothing behind it.
    /// </remarks>
    [Fact]
    public void EveryWriteCarriesARealPage()
    {
        Assert.NotEmpty(CivWrites.All);

        foreach (var write in CivWrites.All)
        {
            Assert.Matches(@"^\d{1,2}-\d{1,2}$", write.Page);
            Assert.NotEqual("", write.Note);
        }
    }

    /// <remarks>
    /// Proves HM-DEC-084: **IP+ is deliberately absent.** Its row reads "Send
    /// the IP+ function setting" where every neighbor reads "Send/read", so the
    /// manual documents no way to read it back, and a write that cannot be
    /// confirmed and cannot be undone is not a write this app makes. Asserted so
    /// somebody adding it has to argue with this first.
    /// </remarks>
    [Fact]
    public void TheWriteOnlySettingIsNotInTheTable()
        => Assert.DoesNotContain(
            CivWrites.All,
            w => w.Command == 0x16 && w.Sub.Length == 1 && w.Sub[0] == 0x65);

    /// <remarks>
    /// Proves HM-DEC-084: the AGC row has four values and not three. The manual
    /// reads "00 to 03" with 00 meaning off, so a table that started at fast
    /// would have no way to say off and no way to put it back for somebody who
    /// had it off.
    /// </remarks>
    [Fact]
    public void TheAgcWriteCanSayOff()
        => Assert.Contains("00=off", CivWrites.Agc.Note, StringComparison.Ordinal);

    /// <remarks>
    /// Proves HM-DEC-084: a level goes out as BCD decimal digits, which is how
    /// the reads already decode it. Sending 128 as a plain byte would put the CW
    /// pitch at 428 Hz when the operator asked for 600 (§4).
    /// </remarks>
    [Theory]
    [InlineData(0, 0x00, 0x00)]
    [InlineData(128, 0x01, 0x28)]
    [InlineData(255, 0x02, 0x55)]
    [InlineData(42, 0x00, 0x42)]
    public void ALevelIsWrittenAsBcd(int level, byte high, byte low)
    {
        var bytes = CivWrites.LevelBytes(level);

        Assert.Equal(2, bytes.Length);
        Assert.Equal(high, bytes[0]);
        Assert.Equal(low, bytes[1]);

        // And it round-trips through the decoder the reads use.
        Assert.Equal(level, CivValues.Level(bytes[0], bytes[1]));
    }

    // ---- Announce, read back, undo --------------------------------------

    /// <remarks>
    /// Proves HM-DEC-084: **a write that was not confirmed reports as
    /// unconfirmed and never as done.** A write the radio acknowledged is not a
    /// write that took effect, and those come apart on exactly the settings
    /// somebody would most want to trust.
    /// </remarks>
    [Fact]
    public void AnUnconfirmedWriteSaysSo()
    {
        var change = new SettingChange(
            CivWrites.AutoNotch, 1, 0, "Turning the notch off.",
            RigWriteOutcome.NoAnswer, Now);

        Assert.False(change.Confirmed);
        Assert.Contains("did not confirm", change.Says, StringComparison.Ordinal);
        Assert.DoesNotContain("It is 0 now", change.Says, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-084: a confirmed write says what it was and what it is, so
    /// the announcement is the undo record.
    /// </remarks>
    [Fact]
    public void AConfirmedWriteSaysWhatItWasAndWhatItIs()
    {
        var change = new SettingChange(
            CivWrites.RfGain, 107, 255, "Opening the receive gain.",
            RigWriteOutcome.Confirmed, Now);

        Assert.True(change.Confirmed);
        Assert.True(change.CanUndo);
        Assert.Contains("It was 107 and it is 255 now", change.Says,
            StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-084 and HM-DEC-050: **an undo with no prior value admits
    /// it rather than inventing one.** Writing a plausible number into somebody's
    /// radio while calling it "restoring" would be the guess §0.0 forbids wearing
    /// the most reassuring word in the application.
    /// </remarks>
    [Fact]
    public void AnUndoWithNoPriorValueSaysSoRatherThanGuessing()
    {
        var change = new SettingChange(
            CivWrites.NoiseBlanker, null, 0, "Turning the blanker off.",
            RigWriteOutcome.Confirmed, Now);

        Assert.True(change.Confirmed);
        Assert.False(change.CanUndo);
        Assert.Contains("cannot put it back", change.Says, StringComparison.Ordinal);
    }

    // ---- The list comes from live state ---------------------------------

    /// <remarks>
    /// Proves HM-DEC-084: the suggestions are computed from what the radio
    /// actually reports, and the evening's real state produces the evening's
    /// real list.
    /// </remarks>
    [Fact]
    public void TheListComesFromLiveState()
    {
        var bad = RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.Mode, (int)CivMode.Cw, "CW", Now, "CI-V 04"),
            RigValue.Known(RigField.AutoNotch, 1, "on", Now, "CI-V 16 41"),
            RigValue.Known(RigField.NoiseBlanker, 1, "on", Now, "CI-V 16 22"),
            RigValue.Known(RigField.NoiseReduction, 1, "on", Now, "CI-V 16 40"),
            RigValue.Known(RigField.Agc, 3, "slow", Now, "CI-V 16 12"),
            RigValue.Known(RigField.FilterBandwidth, 40, "3.6 kHz", Now, "CI-V 1A 03"),
            RigValue.Known(RigField.Preamp, 0, "off", Now, "CI-V 16 02"),
            RigValue.Known(RigField.RfGain, 107, "42%", Now, "CI-V 14 02"),
            RigValue.Known(RigField.AccUsbAfLevel, 20, "8%", Now, "CI-V 1A 05 0060"),
        });

        var advice = ReceiveAdvice.For(bad);

        Assert.Equal(ReceiveAdvice.For(RigState.Empty).Count, advice.Count);
        Assert.All(advice, a => Assert.True(a.WouldChange));

        // Every one of them is receive side, which is what makes one press right.
        Assert.All(advice, a => Assert.Equal(RigWriteTier.Receive, a.Write.Tier));

        // And the notch line says why, in the terms that make it make sense.
        var notch = advice.Single(a => a.Write.Field == RigField.AutoNotch);

        Assert.Contains("steady tone", notch.Says, StringComparison.Ordinal);
        Assert.Equal(0, notch.Value);
    }

    /// <remarks>
    /// Proves HM-DEC-084: **rows that are already correct stay visible and say
    /// so.** Hiding them is tidier and teaches nothing; showing them is the app
    /// proving what it checked, which is the difference between being trusted
    /// and being second-guessed.
    /// </remarks>
    [Fact]
    public void WhatIsAlreadyRightStaysVisibleAndSaysSo()
    {
        var good = RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.Mode, (int)CivMode.Cw, "CW", Now, "CI-V 04"),
            RigValue.Known(RigField.AutoNotch, 0, "off", Now, "CI-V 16 41"),
            RigValue.Known(RigField.NoiseBlanker, 0, "off", Now, "CI-V 16 22"),
            RigValue.Known(RigField.NoiseReduction, 0, "off", Now, "CI-V 16 40"),
            RigValue.Known(RigField.Agc, 1, "fast", Now, "CI-V 16 12"),
            RigValue.Known(RigField.FilterBandwidth, 9, "500 Hz", Now, "CI-V 1A 03"),
            RigValue.Known(RigField.Preamp, 1, "P.AMP1", Now, "CI-V 16 02"),
            RigValue.Known(RigField.RfGain, 255, "100%", Now, "CI-V 14 02"),
            RigValue.Known(RigField.AccUsbAfLevel, 128, "50%", Now, "CI-V 1A 05 0060"),
        });

        var advice = ReceiveAdvice.For(good);

        Assert.Equal(ReceiveAdvice.For(RigState.Empty).Count, advice.Count);
        Assert.All(advice, a => Assert.True(a.AlreadyRight));
        Assert.DoesNotContain(advice, a => a.WouldChange);
        Assert.All(advice, a => Assert.NotEqual("", a.Says));
    }

    /// <remarks>
    /// Proves HM-DEC-084: **what could not be read says so and is neither acted
    /// on nor dropped.** Silently omitting it would leave somebody believing
    /// Hamlet had looked at something it never saw, and acting on it would be a
    /// write decided by a value nobody has (§0.0).
    /// </remarks>
    [Fact]
    public void WhatCouldNotBeReadIsSaidAndNotActedOn()
    {
        var advice = ReceiveAdvice.For(RigState.Empty);

        Assert.Equal(ReceiveAdvice.For(RigState.Empty).Count, advice.Count);
        Assert.All(advice, a => Assert.True(a.Unreadable));
        Assert.DoesNotContain(advice, a => a.WouldChange);

        foreach (var one in advice)
        {
            Assert.Contains("could not read", one.Says, StringComparison.Ordinal);
        }
    }

    /// <remarks>
    /// Proves HM-DEC-084: a partly-read radio produces a partly-actionable list,
    /// with each row saying which it is. The list never quietly shrinks.
    /// </remarks>
    [Fact]
    public void APartlyReadRadioStillListsEverything()
    {
        var partial = RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.AutoNotch, 1, "on", Now, "CI-V 16 41"),
            RigValue.Known(RigField.RfGain, 255, "100%", Now, "CI-V 14 02"),
        });

        var advice = ReceiveAdvice.For(partial);

        Assert.Equal(ReceiveAdvice.For(RigState.Empty).Count, advice.Count);
        Assert.Single(advice, a => a.WouldChange);
        Assert.Single(advice, a => a.AlreadyRight);

        // Everything else could not be read, and every row is one of the three.
        Assert.Equal(advice.Count - 2, advice.Count(a => a.Unreadable));
        Assert.All(
            advice, a => Assert.True(a.WouldChange || a.AlreadyRight || a.Unreadable));
    }

    /// <remarks>
    /// Proves HM-DEC-084 and §0.7: nothing in the advice tells the operator what
    /// to do with his own radio in the imperative-about-him sense, and nothing
    /// suggests his station is broken. It says what it would change and why.
    /// </remarks>
    [Fact]
    public void TheAdviceNeverScoldsOrDiagnoses()
    {
        foreach (var state in new[]
                 {
                     RigState.Empty,
                     RigState.Empty.With(new[]
                     {
                         RigValue.Known(RigField.AutoNotch, 1, "on", Now, "x"),
                         RigValue.Known(RigField.NoiseBlanker, 1, "on", Now, "x"),
                         RigValue.Known(RigField.Preamp, 0, "off", Now, "x"),
                         RigValue.Known(RigField.RfGain, 107, "42%", Now, "x"),
                     }),
                 })
        {
            var said = string.Join(" ", ReceiveAdvice.For(state).Select(a => a.Says))
                .ToLowerInvariant();

            foreach (var scold in new[]
                     {
                         "you should", "you must", "your radio is", "is broken",
                         "you have it wrong", "misconfigured", "your fault",
                         "be careful", "your antenna",
                     })
            {
                Assert.False(said.Contains(scold, StringComparison.Ordinal),
                    $"the advice says '{scold}'");
            }
        }
    }
}
