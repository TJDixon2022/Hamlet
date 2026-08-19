# 1. What Claude did

## The mechanism

**Which frame completed which read: none. The suspect is wrong, and I tested it
before building anything on it.**

`ReadAsync` passes its own command and sub-command as the expected reply
(`Ic7300Rig.cs:222`), so an acknowledgement cannot complete a read; and the
command gate serializes commands, so a read and a tune write cannot be on the
wire together in the first place. `AReadIsAnsweredOnlyByItsAnswerTests` asserts
both at the seam: an `FB` arrives while a frequency read is outstanding, the read
does not take it, and the value that lands is the one the radio actually sent.
**Those pass at `HEAD`.** HM-OPEN-042 is real and it bites writes, which do expect
an acknowledgement; it does not reach reads.

**Which code path held the stale value: `ApplyRigState`, the display-correction
block, `MainWindowViewModel.cs:2680`.**

```
if (!_rigSendPending && !_updatingFromRig && state[Frequency] is { IsKnown: true } ...)
    ApplyRigFrequency(swept);
```

`_rigSendPending` is set when the operator tunes and **cleared in `OnRigSendTick`
before the write is awaited**. So from the instant the command goes out until a
reading taken *after* it comes back, the guard is down and the model still holds
the frequency the radio was on before. This block then applies it, and the display
returns to where he had just been. **It is only ever reachable through a write**,
which is exactly the dividing line he describes: the radio's own knob has no write
and was perfect throughout.

It landed in **`ad93fb4`**, HM-DEC-109's "the swept frequency corrects the
display". Before that commit the swept value went into the model and never onto
the screen, so there was nothing to snap back.

**The thirty seconds is the second half, and it is `RigPollPlan.SessionInterval`.**
Once the display had been dragged back, only the next frequency reading could move
it forward, and in the build he ran the frequency was on the session sweep — the
same `ad93fb4`. Thirty seconds is that sweep. It is the only thirty second window
anywhere near the display: I swept the tree for constants in the twenty-five to
thirty-nine second range and the other three are a spot tag's lifetime, a spot
source's backoff and the decoder's reporting interval, none of which touch the
frequency.

## The fix

**`DialGuard`, pure and in the engine.** A reading may move the display when it
was taken after Hamlet's own last tune, or when Hamlet has not tuned at all —
which is every case of the radio's own knob, so the path that always worked is
untouched. Bounded by the write and never by a timer: a window would be too short
on a busy link and would freeze the display after it had already caught up on a
quiet one, and both are the app deciding it knows better than the radio.

`_writeInFlight` closes the round trip that `_rigSendPending` left open, and the
guard is strict at the boundary — a reading stamped at the instant of the write
crossed it on the wire, so the knife-edge goes the way that leaves the operator's
own action standing (§0.2.1).

## The diagnostics, which did not catch this either

- **`tune_written`** — the one command in the middle of every tune had no event at
  all. `tune_requested` said somebody asked and nothing said what happened, so a
  display disagreeing with the radio could not be placed on either side of it.
- **`frequency_went_backwards`** — the signature of the whole defect, in one line.
  A frequency returning to the value the tune started from, when the tune asked
  for somewhere else, is not an ordinary observation: the radio does not tune
  itself backwards. It is emitted even now that the guard stops it reaching the
  screen, because the day it fires again somebody should be able to search for it.
- **Step 6, fixed**: `scope_output_requested` logged `outcome: failed` with
  `reason: confirmed`. Last session moved the caller to the stable token and left
  this comparison on the enum's name, so a write that plainly succeeded was
  recorded as a failure. **That is my defect from two sessions ago and it is the
  same family**: a token exists so comparisons survive rewording, and it only
  works if the comparison uses it.
- The link's answer reached the operator last session and is in this build: the
  line under the readout, and the counts on the diagnostics screen.

## Recorded under §12.1

**Nothing.** HM-DEC-136 was not written, as the order directs.

# 2. What Tim should expect

**Yes: clicking a dot now moves the display and leaves it moved.** The reading
that used to drag it back is refused by name — it was taken before the tune, so it
cannot speak about where the dial is now — and the refusal ends the moment a
reading from after the write arrives, which is the next poll rather than the next
sweep.

**Why it holds rather than being a hope:** the reproduction fails without the fix
and passes with it. I disabled just the new condition and re-ran: three assertions
went red, including the operator's exact case. That is the whole of the report's
claim, and it is the mechanism rather than a symptom.

- **What it does not fix:** if your radio's answer to the tune is slow, the display
  will hold *your* new frequency during that window rather than reverting. That is
  the intended behavior. It never shows an older value as though it were current.
- **The two seam tests are `[AvaloniaFact]` now.** They started as plain tests,
  passed alone and failed in the full run, because that seam marshals to the user
  interface thread and a test without a dispatcher depends on what runs beside it.
  A flaky test about a display bug is worse than none.
- **Build succeeds, no warnings. 1,969 tests, 2 failing, both the standing decode
  baseline** — `ClearingTheTranscriptLeavesTheDecoderAlone` and
  `TheBulletinDecodesToItsAnswerKey`. I ran the suite four times to be sure the
  count is stable; one run showed a third failure and the next three did not, and
  it was not one of mine.
- **One commit, pushed to `main`. Nothing local, no branches.**
- **No radio was connected** (HM-DEC-093).

# 3. What we should do next

- Run it and click a dot.
- If it ever snaps back again, `frequency_went_backwards` names it directly and
  `tune_written` puts it either side of the command.
- The automatic `27 11`, named and left in this order, is already out of the tree:
  I removed it last session under HM-DEC-062 and reported it. `scopeShare` 0.50
  was measured on a build that still had it.

# 4. What's blocking us

Nothing is blocked. One thing you should know about the tree, and the standing
pair unchanged.

---
date: 2026-08-19
refs: HM-DEC-109, HM-DEC-050, WORK_INSTRUCTIONS.md 2026-08-19
---

**The frequency is on the live poll in the tree right now, and that is the change
HM-DEC-136 would have ruled.**

This order withdrew that ruling and says nothing supersedes HM-DEC-109 or
HM-DEC-050. Nothing in *this* order does. But the change itself shipped in the
session before, as `099de5a`, with the ruling asked for in that report and not yet
given, so the code is running ahead of the record.

You are right that it was not the fix: the snap-back is the defect and it happens
at any cadence. What the live poll does change is how long the wrong value stays
on screen once something else puts it there, which is why the two got confused.

Three ways: leave it and rule on it; revert it to the session sweep, which puts
the thirty second recovery back if any path ever writes a stale value again; or
rule that the frequency's cadence follows whether the radio announces, which is
what `SkipLiveRead` already implements underneath it.

I did not revert it in this order, because reverting a shipped change in an order
scoped to one defect is exactly the kind of unasked repair §12.6 exists to stop.

---

The two standing questions are unchanged and still yours: **whether an attended
automatic cycle may reach an antenna** (§0.2, HM-DEC-098), awaiting the interlocks
watched into the load; and **a callsign too long for one keyer send**
(HM-DEC-130), refused until the seam between two sends is measured into the load.

---

## Named and left, as the order directs

Not started: HM-OPEN-042's remaining rungs beyond step 6; the record sweep for
rulings resting on a write outcome; `DECISIONS.md` missing 096 to 133; HM-DEC-135
and §9.6; mode follow, favorites and the recent list. The automatic `27 11` is on
that list and is already gone, which is noted above rather than acted on again.
