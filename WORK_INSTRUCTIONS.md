STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.

---

# Work instruction 047 — the owned settings, and one contract for every mode

**ISSUED: 2026-08-29. A fresh order, not an amendment. Follows unit 046.**

**Six tasks; task 6 is the drop.**

## Why this unit exists

**Two conversations are now building against the same radio.** This one works CW
at night; another works FT8 in daylight. **Each of them changes settings the other
depends on**, and the way those changes are made today guarantees they will fight:
each mode writes its own ad-hoc set of deltas, so whichever ran last wins on
whatever it happened to touch, and nothing states what happens to a setting a mode
never mentions.

**That is not a bug to find. It is a missing contract.**

Last night showed the cost of the same gap on one mode. The attenuator sat at
**20 dB** while a station faded S4 → S1 → S0, and then sat **off** while the front
end read `overloading` at S9+10. Both wrong, in opposite directions, and Hamlet
read `Overflow` in both cases and said nothing.

**This unit builds the contract, fills in CW's row, and reports every gap it
finds. It does not invent values for modes it does not own.**

## The contract

**One owned-settings list. Every neighborhood row states a value for every setting
on that list, or states explicitly that the setting is the operator's own choice.
Nothing is implicitly left alone.**

That is the whole design, and everything below follows from it:

- **Switching CW → FT8 → CW lands in exactly the same place every time**, because
  the second mode restores what the first changed rather than leaving whatever it
  did not think about.
- **Two conversations cannot write conflicting partial deltas**, because a row is
  complete or it is reported as incomplete.
- **A setting that is nobody's business is said to be nobody's business**, which is
  different from being forgotten.

**A row that does not yet state a value for an owned setting leaves that setting
alone and the gap is reported.** It is not an error and it is not filled in by
guesswork — the FT8 rows belong to another conversation and this unit must not
write them.

## What Hamlet owns, and what it does not

**Owned** — these are consequences of the operator's stated intent to work a mode
in a place, and Hamlet sets them:

mode and data flag; filter slot and width; auto notch; manual notch; noise
blanker; noise reduction; AGC; preamp; attenuator; RF gain; squelch; scope span.

**Not owned, and this is deliberate:**

- **CW pitch** (`14 09`). Moving it changes what the operator hears in the
  headphones. That is his ear, not a receive condition. **Left alone.**
- **AF level.** Same reason.
- **Break-in.** A transmit setting. §0.2 keeps this unit off it entirely, and the
  manual's footnote 2 makes PC text become transmitted CW while break-in is on,
  so it is the last thing that should be touched by an automatic write.
- **Noise blanker level, noise reduction level, notch position.** They only matter
  when their function is on, and Hamlet turns those off. Setting a level for a
  disabled function is noise in the write log.

## CW's row, decided, with the reason for each

**These are Tim's, given 2026-08-29. Record them in the file beside the values, in
the file's own voice, because unit 040 established that a row carries its reason
as text.**

| setting | CW | why |
|---|---|---|
| mode | CW, data off | the mode the neighborhood is worked in |
| filter | FIL2, 500 Hz | comfortable to listen to and standard practice for CW |
| **auto notch** | **off** | **it hunts steady carriers and removes them, and a keyed CW signal is a steady carrier. It will eat the thing we are trying to read.** |
| manual notch | off | same reason, under the operator's hand rather than automatic |
| noise blanker | off | an impulse gate that chops keying edges |
| noise reduction | off | built for speech; it mangles the envelope the decoder measures |
| AGC | FAST | standard for CW; it tracks keying rather than pumping across it |
| RF gain | 100% | anything less throws away signal the decoder needs |
| squelch | open | a gate that closes between elements is fatal to a decoder |
| **attenuator** | **driven by the overflow reading** | off unless the front end reads `overloading`. Last night was 20 dB on at S0 and off at S9+10 while overloading — both wrong, and Hamlet read the answer both times |
| **preamp** | **from the band** | off at 40 m and below, where the noise floor is the limit and a preamp only raises noise with signal; on above |
| scope span | narrow enough to show the neighborhood | ±100 kHz renders a 3 kHz block as seven pixels, which is what cost an hour on 2026-08-28 |

**The attenuator and preamp rules are conditional, not constants**, and the
condition is a value Hamlet already reads. State the condition in the row.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
Trust the tree over this order everywhere they differ.

**This author does not know what unit 042 landed** and has not seen its report.
**Task 1 must establish what is written automatically today** before anything
changes.

From unit 046's report: app **519 passing, 0 failing**; engine **28 failing of
1990** excluding `TheGateHasItsOwnWindowNowTests` (HM-OPEN-061); corpus **yield
0.763, precision 0.761**. **`TheSilencePropertyIsLockedTests` exists and may not be
modified.**

**Record both suites and the corpus score before task 2.** **The corpus score must
not move in this unit** — nothing here touches the decoder.

## Rulings in force

**Transcribed with what was rejected. Do not re-argue either.**

**Tim's rulings:**

> **Hamlet sets whatever the radio needs for the mode. The operator does not touch
> the radio.**

> **One owned-settings list, and every mode's row states a value for every setting
> on it.**
>
> Rejected: per-mode deltas — whichever mode ran last wins on whatever it happened
> to touch, and two conversations building against one radio then overwrite each
> other silently.

> **CW's values are as tabled above.** Auto notch off is the one that matters most.

> **The CW pitch and the AF level are the operator's.** Moving them changes what he
> hears, which is a different class of write from a receive condition.

> **Do not break the silence property.**

**Standing rulings this unit is bound by:**

- **HM-DEC-084** — **settings are consequences of intent, never things the operator
  operates. No screen may carry a control corresponding one-to-one with a radio
  setting.** Read before write, read back after, announced, undoable, and **unknown
  stays unknown**. No byte written that is not cited.
- **HM-DEC-056** — the operator's own hand wins and suspends the automation
  visibly until the next band change re-arms it; a flip waits for the dial to
  settle so crossing three neighborhoods in one drag produces one change; the
  status line says what changed and why in the app's voice.
- **HM-DEC-050 / §0.5** — no rig-control panel.
- **§0** — generate from the source of truth; no constants sprinkled through code.
- **§0.0.1** — every write goes out through the same gate and the same trace as
  every read, so a session log carries it verbatim with its timestamp.
- **§0.2 / HM-DEC-008** — **no transmit work of any kind. Nothing goes near
  break-in, and `1C 01` value `02` starts a tuning cycle that transmits.**

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` — `STATE`,
`TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is moving
inside the task. Same every ten minutes while a task runs.

## The tasks

### Task 1 — every write Hamlet makes on its own initiative

**Report before changing anything. Say what you find rather than confirming a
list.**

- **Every automatic write in the tree**: command, sub-command, value, **what
  triggers it**, whether it is read back, and whether the operator's hand suspends
  it. Name each with file and line.
- **Every write reachable at all**, automatic or operator-triggered, so the two are
  distinguishable.
- **Any write that cannot be read back.** `16 65` IP+ is already excluded for that
  reason (HM-DEC-084). **A second one is a finding.**
- **Whether anything can reach the transmitter.** Break-in is on, and the manual's
  footnote 2 makes PC text become CW in that state. **§0.2 says no keying; assert
  it with a test rather than believing it.**
- **What unit 042 landed** on the digital rows, if anything.

### Task 2 — the owned list, and the completeness report

- **Define the owned-settings list** as data, not as scattered code: the twelve
  above, each with its CI-V command from `CLAUDE.md` §4 and its citation.
- **A neighborhood row may state a value, or state `operator's choice`.** Both are
  answers. **Absent is neither**, and absent leaves the setting alone.
- **A test walks every neighborhood row and reports coverage** — which settings
  each row states, which it defers, which are absent. **Absent is reported, not
  failed**, because the FT8 rows belong to another conversation and this unit must
  not write them.
- **The report carries that coverage table.** It is what the other conversation
  needs to fill its side in without collision.

### Task 3 — CW's row

Fill in CW's values exactly as tabled above, **each with its reason as text in the
file** (unit 040's pattern), generated from the source of truth where derivable
(§0).

- **The attenuator's rule is conditional on the live `Overflow` reading** (`15 07`),
  which Hamlet already takes. Off unless the front end reads overloading.
- **The preamp's rule is conditional on the band.** Off at 40 m and below, on
  above. State the boundary and its reasoning in the file.
- **The scope span is derived from the neighborhood's own width**, the way unit 040
  derived the passband, not typed as a number.

### Task 4 — the write, and what it says

On tuning into a block, for each owned setting the row states:

- **Read the current value first.** If it already satisfies the row, **change
  nothing** and record that it was already right.
- **Write, then read back.** A value the radio did not confirm is **unknown**, not
  assumed to be what was asked for (HM-DEC-084, HM-DEC-056).
- **Once per tune-in, then hands off.** No timer, no re-assertion, no fighting the
  knob.
- **The operator's own hand wins.** A setting he changed by hand since the last
  tune-in is left alone, and the suspension is visible.
- **Hamlet says what it changed and why**, in connected speech, only for what
  actually changed, in the app's voice (§0.7, HM-DEC-034):

> *I set the filter to 500 Hz and turned the auto notch off, because it hunts
> steady tones and Morse is a steady tone.*

**Anything Hamlet could not confirm is said as unconfirmed, not silently omitted.**

### Task 5 — the round trip, asserted

The whole point of the contract, tested:

- **CW → FT8 → CW returns to CW's stated row on every owned setting**, ten round
  trips without drift.
- **A setting a row defers to the operator is not touched by either direction.**
- **A setting no row states is not touched by either direction**, and the gap is
  in task 2's coverage table.
- **The four states of last night are fixtures**: 20 dB attenuator on a weak
  signal; no attenuator with the front end overloading; auto notch on in CW;
  scope at ±100 kHz on a 3 kHz block. **Entering the mode must correct each.**
- **`TheSilencePropertyIsLockedTests` stays green** and is not modified.

### Task 6 — the decoder's search range against the filter *(the drop candidate)*

**Measure and report. Change nothing.**

The decoder searches **400 to 1200 Hz**. A 500 Hz filter centred on a 600 Hz pitch
passes roughly **350 to 850 Hz**. **More than half the search range is outside what
the radio can hear**, which would explain both the sweep pinning at 400 Hz and the
tracker's excursion to 850 Hz on 2026-08-29.

- **Confirm the filter's actual passband** from `1A 03` and the CW pitch, rather
  than from this order's arithmetic.
- **Report, across the corpus, how often the admitted or searched pitch falls
  outside it.**
- **Change nothing.** Unit 048 is rebuilding how pitch is handled and this is
  evidence for it, not a fix.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

**The decoder** — the lattice, the confidence work, admission, the tracker. Unit
048 owns all of it.

**The FT8 and FT4 neighborhood rows** — another conversation owns them. **Report
their coverage; do not write their values.**

Also: the digital tab and its capture press; the scanner and the calling cycle;
`CHANGELOG.md`; the missing `DECISIONS.md` records; the phrasebook and the
recent-places row; the Twin PBT; the answer key's licensing; the dial-move
threshold and the transcript break's wording.

**Both halves are required: do not touch them, and do not raise them.**

A parked item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **No transmit. Nothing keys the radio. Nothing touches break-in.**
- **Do not write a value into a row this unit does not own.**
- **Do not touch the CW pitch or the AF level.**
- **Do not build a rig-control panel or a settings row** (HM-DEC-050).
- **Do not re-assert a setting after the tune-in.** Once, then hands off.
- **Do not override a control the operator's hand is holding.**
- **Do not assume a write took.** Read back, and unknown stays unknown.
- **Do not write a byte that is not cited** in `CLAUDE.md` §4.
- **Do not change the decoder.** The corpus score must be identical at the end.
- **Do not modify `TheSilencePropertyIsLockedTests`.**
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 to `output.md` at the repository root, overwritten
and printed. **Read the file's own section count and follow it.**

**The section that reports measurements leads with task 1's inventory — every
write Hamlet makes on its own initiative — and then task 2's coverage table, which
is what the other conversation needs.**

**The section that says what the owner should expect leads with this: tuning into
CW now sets the whole receive side for CW, says what it changed and why, and
switching to a digital block and back lands in the same place every time.**

**If you finish every task, stop and report. Do not start the next unit.**
