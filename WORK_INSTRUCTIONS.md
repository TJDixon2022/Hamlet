# Work instruction 495 - one W1AW button

**Hand run. One small unit.** Unit 494 put seven buttons on the CW tab. The owner wants one.

**His words, 2026-09-29, and they are the whole unit:** *"Ugly. We only need one button - it turns
to the current band."*

**No test against a recording, a fixture, a floor or copied telemetry** (R96). A headless test
driving synthetic values written in the test itself is allowed; nothing read from disk. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
  MUST EXIST:      CW_REQUIREMENTS.md
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln
  root             C:\Source\HamLet

If all five are not as stated, refuse: reply with only the path you are in,
which checks failed, and "wrong project - nothing done."

If all five hold, say "Hamlet confirmed" and continue.
```

---

## 1. Rules

- Take `SESSION.lock` through `tools\arbiter\lock.bat take`, release it at the end. Write nothing
  to `RUN_LEDGER.md`. Touch nothing under `tools\arbiter\`. Tick nothing in `PHASE_PLAN.md`.
- One `dotnet test` invocation per line, filtered, with a `timeout`. Never background and poll.
  The app line loses names to the dispatcher loop; re-run once, count neither way.
- Apostrophes in quoted heredocs break; `;`, `rm` and `git rm` are refused; Python cannot run
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit495-<name>.sh`
  and are not committed.
- **Nothing that keys or transmits.** The button tunes the receiver and sets CW, by the same path
  unit 494 built.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. The change

**One button on the CW tab, where unit 494's row is**, replacing the seven and the *Listen to
W1AW* heading.

- **Its label names the band it will tune**: `W1AW on 40 m`, `W1AW on 20 m`, and so on, from the
  band the radio is on now. It follows the dial: change band and the label changes with it.
- **A press tunes to W1AW's Morse frequency on the band the radio is on**, and sets CW - the same
  `TuneTo` and `SetModeAsync` path unit 494 built, and the same mode-follow hold. Nothing else
  changes.
- **On a band W1AW does not send Morse on, or one Hamlet cannot honestly tune** - 12 m, 30 m, 6 m
  as unit 494 found it, or anything not in `w1aw-morse.json` - the button says so and is not
  pressable: **`W1AW not on 30 m`**, or the plainest words that fit. **Never a false label** and
  never a press that goes nowhere (§0.0).
- **The hover keeps unit 494's text**: the frequency it will tune to, that W1AW is the ARRL's
  headquarters station, what it sends and at what speeds, and that the times are the ARRL's
  schedule and are not shown here.
- **Keep everything else unit 494 built:** `data/bands/w1aw-morse.json` with all nine rows,
  `W1awMorseFrequencies`, the tune command, the mode-follow hold. **Only the seven buttons and
  their heading go.**
- **It sits in the CW tab's send column** where the row was, one button high, and **moves no
  panel's position or size** (R101).
- **The hover registry** `EveryControlSaysWhatItDoesTests` lists the one button and no longer the
  seven.

**This also settles two things unit 494 raised**, and neither needs the owner:
- **Greyed or pressable:** with one button the question becomes concrete - it is not pressable
  only when **W1AW does not send on this band**, which is a fact about W1AW, not about the
  licence. **Licence never disables it**, since listening is never restricted (HM-DEC-029); the
  hover still says when sending Morse there is not covered.
- **The CW tab's text ceiling:** unit 494 pushed `HowMuchTheApplicationSaysTests` from 585 to 663
  against a ceiling of 550. One button and no heading should bring it back near 585. **Report the
  number.** It was red before unit 484 and is not this unit's to fix.

**Watch it fail first**, headless:

1. On 40 m the button reads `W1AW on 40 m` and a press asks for 7.0475 MHz and CW, one mode write,
   nothing keyed, no setting written.
2. On 20 m the same button reads `W1AW on 20 m` and asks for 14.0475.
3. On a band W1AW does not send Morse on, the button says so and is not pressable.
4. The button is on the CW tab and nowhere else, there is exactly one of it, and with and without
   it no panel's position or size differs.

---

## 3. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 495 - STEP 11`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 495.
- Patch-bump `Directory.Build.props`.
- **Append to HM-DEC-199 in `DECISIONS.md`** - do not edit it - a dated note that the owner ruled
  one button following the dial, and that it is not pressable only where W1AW does not send.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling.

---

## 4. Report

Section 2, for the owner, in plain words: rebuild, go to the CW tab, and there is one button that
says which band it will take you to. Press it and the radio tunes to W1AW there in CW. Change
band and the button changes with it.

Section 1: what changed, file by file, and that the build and the app line are green. Section 3:
the four cases, and the CW tab's text count. Section 4: anything left, a line each.
