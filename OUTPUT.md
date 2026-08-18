# 1. What Claude did

**STATE.** Development computer, `C:\Source\HamLet`, Claude Code surface (§9.5).
**Branch: `main`, and nowhere else** (§9.5.1). The prompt claimed `PROJECT: Hamlet`
and the tree confirms it: `CLAUDE.md`'s header reads `Project: Hamlet`, the
solution is `Hamlet.sln`, the namespaces are `Hamlet.*`. Gate passed. **Nothing in
this report is evidence about the radio** (HM-DEC-093), and **nothing in this
session transmitted anything**: every keying call in every test lands on a fake
that records what would have gone out.

**Nothing was recorded under §12.1.** On this order that is the rule rather than a
coincidence — almost all of it touches §0.2 — and two questions are in section 4.

**All six phases completed. Nothing was dropped.**

## The resume premise, corrected

The prompt said phases 1 and 2 had committed. **They had not: nothing from this
order was committed at all.** `git log` topped out at `44b93ed`, the previous
session's report, and both new engine files were untracked. What was true is that
`AutoCallAnswers.cs` had landed complete at 310 lines, brace-balanced, and
`AutoCall.cs` at 729 — and that the engine did **not** compile, because
`AutoCall.cs` named an event-args type that does not exist
(`RigFrequencyChangedEventArgs`; it is `FrequencyChangedEventArgs`). That was the
first thing fixed.

**One correction to what had landed.** The second response tier constructed its
verdict positionally as `Stop: false` and then overrode it with an object
initializer setting `Stop = true`. It compiled and it read as a contradiction, so
it is now written out with named arguments and a comment saying which half is which.

## Phase 1 — the keyer, and the abort first

**Most of what this phase asks for already existed and is already proved**, which
is worth saying rather than rebuilding: `KeyerCwSender` sends by command 17,
`Abort()` is same-thread and awaits nothing, and four existing tests cover it —
`TheAbortStopsASendAlreadyInFlight`, `TheAbortIsSafeWhenThereIsNothingToStop`,
`TheStopFrameIsCommand17CarryingFf` and `TheTransmitterHasNoWayToKeyWithoutBeingAsked`.
Break-in off and break-in unread are already separate refusals with separate
sentences.

**What is new is validation at edit time.** `AutoCallSettings.Refusal` names the
fault where the operator can see it: an over-long message says its own character
count and the keyer's limit, an empty one says Hamlet does not write one for him, a
round too short to fit the message says so, and a round count nobody would run
unattended is refused at both ends.

**A longer call is refused rather than split, and that is the design question the
order said to raise rather than decide.** It is section 4's second item.
`CQ CQ DE KC3QIS KC3QIS K` is 24 characters against the keyer's 30, so it fits —
but not by much.

**Host-timed keying on DTR or RTS is not built and the reason is in the code**, so
the next session does not re-open it: it makes a PC responsible for continuous
control of a transmitter it cannot guarantee it will be alive to release.

## Phase 2 — the cycle

`AutoCaller` in the engine. Thirty seconds and ten rounds are the ruled defaults
and there is a test pinning them. **The message is the operator's own text, there
is no default anywhere, and there is a test asserting the panel opens with none.**

**The listen window does not run during transmit.** Command 17 returns as soon as
the radio acknowledges — milliseconds — while the radio keys for as long as the
message takes, so the cycle waits out `CwDuration.Of(message, keyerSpeed)` plus a
quarter of a second of transmit-receive recovery before it listens. That is
comfortably past the 24 ms of T/R hang and the guard's own 150 ms hold measured on
this radio (HM-DEC-095). Listening earlier hears the operator's own transmission as
a muted receiver and reads the slivers between his own elements as somebody
answering.

**Every transmission is logged** with its timestamp, the frequency the radio was
on, the message and the round number, and the panel draws it.

## Phase 3 — response detection, two tiers plus the tracker

**QSO-shaped text stops and claims an answer**, in order of what it establishes:
his own callsign coming back, `DE` and a callsign-shaped token, a closing word, a
repeat. **Confident text Hamlet cannot read stops and claims nothing.** Those are
different claims and they carry different sentences (§0.0). Dim text does not stop
it at all, because a window of letters the decoder would not stand behind happens
all evening on a fading band.

**A callsign-shaped verdict names no callsign** (HM-DEC-073), and a placeholder
splits a token rather than welding two fragments into one — there is a test that
feeds it `W1■AW` and asserts it is not read as the operator's own callsign coming
back.

**And the tracker's own move is used, which the order specifically asked for.** A
follow means the survey found keying at a pitch that is not the one it was reading,
decided on three seconds of mark-length structure rather than on a classifier
having enough letters, and it arrives sooner than text can. **Evidence and never a
verdict**: on its own it stops the cycle without claiming an answer, and beside two
confident characters it means what four would have meant alone. **A follow and
never a refinement** (HM-DEC-123) — counting refinements would stop the cycle every
time the survey preferred its neighbouring bin.

## Phase 4 — arming, stopping, the dead man

**Arm is a distinct step from start and start is not offered until it has
happened.** What the operator consents to is a transmission repeating under his
callsign while he may not be watching, so consent is an act against displayed
facts: the message, the frequency, the rounds and how many minutes that is,
break-in, the power as a percentage and never a wattage (HM-DEC-082), and whether
the radio has finished saying what it is set to. **What Hamlet has not read, it
says it has not read**, in each of those lines separately.

**The stop is in the pinned strip beside the scanner's**, because a stop for a
transmitter inside a widget is a stop the operator can lose while his callsign goes
on calling. **Escape does the same thing from anywhere**, handled in the window,
because the one keystroke that stops a transmitter has to work whatever has focus.
A green line in the same strip says it is transmitting.

**The dead man re-reads break-in and transmit status from the radio before every
round.** `RigStateMonitor.RefreshAsync` writes `RigValue.Unknown` for a read that
came back empty *and* for one that threw because the port is closed, so one test —
is the field known — covers a quiet radio and a pulled cable alike.

**Mutually exclusive with the scanner, checked from both sides.** Refusing in one
direction only leaves whichever the operator pressed second to win. The two ask
each other rather than tracking each other, so a stale copy cannot let Hamlet
transmit mid-tune.

**Two real faults were found by testing rather than by reading, and both are worth
naming:**

- **The dead man rejected a live radio.** Its first form asked whether the
  reading's timestamp had advanced. Two reads inside one clock tick then read as
  silence and stopped a perfectly working cycle — and the check duplicated one the
  monitor already makes properly. A guard that fires on a working radio is not a
  safer guard, it is a broken one.
- **The dial-touched baseline swallowed the move that matters.** Seeded from the
  first frequency event, the operator reaching for the dial during the first
  transmission arrived as that first event and was consumed as "where the dial is".
  The cycle ran to its round limit with the radio somewhere nobody had checked. It
  is now seeded at arm time from the reading the cycle already requires to be known
  and fresh.

**And one reporting fault.** An internal guard tripping calls the same `Stop` the
operator's button calls, so testing that flag first reported that *he* had stopped
a cycle the dial actually stopped. The cycle halted correctly either way; what was
wrong was the record, and a record that names the wrong reason is worth nothing on
the evening it is needed. The specific cause now wins.

## Phase 5 — every interlock, proved by breaking it

Seventeen tests, each breaking exactly one thing and asserting exactly its own
stop: break-in off at arm; break-in going off mid-cycle; transmit status stuck on;
the PTT pressed while listening; the dial moved; a stale reading; an unanswered
dead-man read; **the serial link failing outright mid-cycle**; the send itself
refused; the `Populated` gate; the scanner running; the operator stopping it; a
response detected; the round limit; and that the stop code goes out on every exit
including the ordinary one.

**The one HM-DEC-098 names specifically is there**: the read throws, which is what
a closed port does, the cycle stops, and the stop code goes out on the way past
where it reaches nothing quietly — because an abort that could fail is not an abort.

**And the tripwire.** Every path above runs to completion and the test asserts the
rig underneath never had `SendCwAsync` called on it at all, so no test in the suite
can key anything even if the sender is later rewired.

## Phase 6 — HM-DEC-129 recorded

`prosigns-easy` is out of `TheEasyTierIsReadWhole`'s theory, **with the whole reason
on the test's own face** so nobody reads it as a bar being lowered: the bar was
ruled for a loud clean signal read wrongly, and a message whose opening is gone
before the detector has found it is a different claim — one **no real station
produces**, because a CQ repeats.

**The fixture is not edited.** It still asserts `TheProsignsArriveAsProsigns` and
its edge and working tiers, and it passes all of them. HM-OPEN-033 is marked
scheduled rather than closed, with a paragraph saying that it now has **no red test
behind it while remaining the largest single defect in the decoder** — which is the
kind that stays unfixed for a year. HM-OPEN-034, the 350 hertz hole, is noted
beside it.

# 2. What Tim should expect

- **Build succeeds, no warnings.**
- **1902 tests, 2 failing.** 1469 of 1470 in the engine, 431 of 432 in the app.
  **Fifty-seven tests are new**, and thirty of those are the safety suite and the
  answer classifier.
- **Three failures become two**, and both that remain are the settled positions
  the order named as not-work: `ClearingTheTranscriptLeavesTheDecoderAlone` and
  `TheBulletinDecodesToItsAnswerKey`. **There is nothing red in the transmit path.**
- **What will look wrong and is not.** A new widget appears in the tray, "Call CQ
  on a cycle", and it opens with an empty message box and every fact unread until a
  radio is connected — that is the design, not a broken panel. `prosigns-easy` no
  longer appears in the easy-tier theory; that is HM-DEC-129 and the reason is on
  the test.
- **What to do on COM3, into a dummy load.** This is the part no test can stand in
  for and HM-DEC-098 requires it before the antenna question is even asked:
  1. Connect, put the radio in CW, **check the dummy load is on the antenna
     socket**, and open the new panel.
  2. Type your call and read the five fact lines. Turn break-in off and confirm the
     panel says so and will not arm.
  3. Break-in on, arm, start, and watch two or three rounds go out and the log fill.
  4. Then break it: press Escape mid-cycle; touch the dial mid-cycle; press the
     PTT during a listening window; turn break-in off between rounds; and **pull
     the USB cable mid-cycle**. Each should stop, and each should say which of
     those it was.
- **Nothing reached an antenna and nothing in this session transmitted at all.**
- **One flake sighting recorded honestly and incompletely.** A single combined run
  reported five app failures where every run either side reported one; four
  app-only runs and three further combined runs all gave the steady figure. **The
  grep in use at that moment printed summary lines only, so which four they were is
  not known** — recorded in HM-OPEN-024 as an unnamed sighting rather than guessed
  at.
- **Five commits, pushed to `main`.** Nothing local, no branches. The first carries
  the uncommitted `CLAUDE.md` and `CLEANUP_BRIEF.md` that were in the working tree.

# 3. What we should do next

- **Watch the interlocks fire into the load**, which is section 2's list. Nothing
  else in this feature should move until that has happened, because it is the
  precondition HM-DEC-098 sets on the antenna question.
- Rule on section 4 item one, the antenna question itself, but only afterwards.
- Rule on section 4 item two, the two-message call, which is small and blocks
  anybody whose callsign is longer than the test case.
- HM-OPEN-033, the cold-start bin choice, as its own order. It has no red test
  behind it now and it is still the largest defect in the decoder.

# 4. What's blocking us

---
date: 2026-08-18
refs: CLAUDE.md §0.2; HM-DEC-098; HM-DEC-008
---

**Whether an attended automatic cycle may reach an antenna, taken after every
interlock has been watched to fire into the dummy load.**

This is not a question this session may answer and it is not one this order
answered. It is here because the work HM-DEC-098 made the precondition is now
done and the precondition is therefore live: the cycle exists, every interlock in
phase 4 has a test that breaks something and watches exactly that stop come back,
and **the link-pulled-mid-cycle path exists and is tested before you pull the
cable rather than after**.

What is still missing is the only evidence that counts. **Reasoning about an
interlock is not seeing it work**, and seventeen passing tests against a fake that
keys nothing are reasoning with better bookkeeping. §0.2's first sentence — no
unattended transmission — stands unamended, and nothing in this order argues for
amending it.

What the ruling would need, and what section 2 asks you to produce: each of Escape,
the dial, the PTT, break-in going off, and the cable pulled, watched into the load,
each producing its own sentence rather than a generic stop. If any of them does not,
that is a finding about this code and not a reason to loosen anything.

Rejected: recording anything about the antenna under §12.1. That section puts
anything touching §0.2 with you without exception, and this is the centre of it.

---
date: 2026-08-18
refs: Full Manual p. 19-11; HM-DEC-071; CLAUDE.md §4
---

**A callsign too long for one keyer message: two messages, a shorter call, or a
refusal that stands.**

The keyer takes thirty characters in one command 17 message. `CQ CQ DE KC3QIS
KC3QIS K` is 24, so your own call fits — with six characters of headroom. A longer
call, or a third `CQ`, or `PSE K` on the end, does not.

**Today it is refused at edit time and says its own character count**, which is the
safe answer and may not be the right one. The single-send path already splits a
long message at the spaces (`CwMessage.Split`), so the machinery exists; what does
not exist is an answer to what splitting means for a *repeating* call. Handing the
keyer two messages back to back puts a gap of unknown length in the middle of every
round — unknown because the second only goes out when the radio has taken the
first, and Hamlet does not know how long the radio will hold it. A CQ with a
ragged pause in the middle of it is a worse transmission than a refusal.

Three ways, and the choice is yours:

- **Refuse, as now.** Costs nothing, and puts a ceiling on the message that some
  operators cannot get under.
- **Split, and measure the seam into the dummy load** before believing anything
  about it. It is the only one of the three that produces evidence.
- **Send the second message only after the first has finished keying**, timed from
  `CwDuration` as the cycle already times its listening window. Predictable, and it
  makes Hamlet responsible for a gap in the middle of a transmission, which is a
  smaller version of the reason host-timed keying is rejected outright.

Rejected: choosing on this session's authority. It changes what goes out under a
callsign, which §12.1 puts with you, and the safe answer is already in place so
nothing is blocked meanwhile.
