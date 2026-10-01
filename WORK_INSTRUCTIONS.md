# Work instruction 520 - a fist is a sender, the best shape gets the terminal, and a neighbour does not kill a station

**Hand run. One unit, three tasks, commit per task, drop from the back.**

**No test against a recording, a fixture, a floor or copied telemetry** (R96). A headless test
driving synthetic hops written in the test itself is allowed; nothing read from disk. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line. **Every
existing reading case reads exactly as at HEAD**, or the report says what changed and why.

**Numbering.** This is unit 520, ruling HM-DEC-224. If taken, use the next free and say so.
**Unit 518 runs after this.**

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwSequenceShape.cs
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
- Apostrophes in quoted heredocs break; `;` is refused; Python cannot run here; `-m` more than
  once for a multi-line commit. Scripts go in `.run-unit\unit520-<name>.sh`, not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. Task 1 - a fist is a sender

**Unit 519 measured it:** a fist scattered by a fifth scores **0.069**; by a third, **0.000**.
Noise's best is 0.107. **A sloppy human ranks below noise.** The owner's 21:44 station on
2026-09-30 was a sloppy human and a real one, and under 519's nought rule it would print nothing.

**Why.** `CwSequenceShape`'s tightness terms reach nought at `0.25` in log-length, a hand's widest
spread (unit 513). A fist at that spread scores zero on four of eight terms, and the product is
zero.

**The rule.** A fist is still a sequence with two lengths and three gaps - wider ones. **Tightness
is scored against what a hand does, not against a machine:** a spread up to a hand's widest
scores well, and only a spread past what any hand makes scores toward nought. The figure is the
author's, stated with its reason, derived from unit 513's measured fists - not from a result.
**A machine sender still scores higher than a fist; a fist still scores well above noise.**

**Watch it fail first**: unit 513's fists at a fifth and a third, their shape scores before and
after; **both must score above noise's best (0.107) by a clear margin**, and the clean sender
above both. Noise, 30 s and 180 s, prints nothing and its best score is reported.

## 3. Task 2 - the best shape gets the terminal

**Unit 519's question, answered here as a CW question (R85), not raised to the owner:** **option
A.** When the first sender qualifies, **wait one of its word gaps** for others to qualify, then
print the best-shaped. Nothing switches mid-sentence; the first letters print a word late. **A
printed sender is held as unit 511 holds it** - until it has been silent for its word gap and a
dah; at that silence, if a better-shaped sender is standing, it takes the terminal.

**Watch it fail first**: unit 519's case 2 - a clean 10 dB sender at the filter's edge beside a
20 dB fist in the centre. **Red today**: the fist prints. Green: the clean one prints, whole.
Case 4 reports the switch time. Every other case reads as at HEAD, with the first word's delay
noted.

## 4. Task 3 - a neighbour does not kill a station

**Unit 519's case 1 at 200 Hz:** a clean 12 dB sender beside a 24 dB carrier keyed at random,
200 Hz away. The clean sender keeps **21 of 65 marks**. Not a choice fault - **the per-mark gates
lose the marks to the carrier's keying.** And this is the owner's screen this morning: stations
he heard clearly, pressed *idiot* twelve times, while the meter found their marks and the pattern
gate stood something else.

**Find which gate first.** On that case, with each per-mark gate off in turn - flatness, edges,
narrowness, the fit, the pattern gate's level tolerance - how many of the clean sender's 65 marks
survive. **Report the table.** The likely one is narrowness: the carrier 200 Hz away sits inside
the narrowness probe's reach, so a clean mark reads as broad whenever the carrier is up.

**Then fix that gate so a neighbour is a neighbour, not noise.** A probe bin that is itself a
keyed tone - its own level making bars - is not "the band beside the mark"; it is excluded from
the narrowness comparison, and the mark is judged against the band that is not another station.
If the gate is a different one, the same principle: a mark is judged against noise, never against
another sender.

**Watch it fail first**: case 1 at 200 Hz, red today at 21 of 65; green when the clean sender
keeps its marks and reads whole. Then at 150 and 100 Hz, printed. The two-station case at 200 Hz
still reads the loud one whole and stands the quiet one.

**Drop candidate:** this task, with the gate table stated even if the fix is not built.

---

## 5. What must not change

Every existing case reads exactly as at HEAD apart from the first-word delay task 2 adds: every
speed, both Farnsworth cases, the speed change, the fists, the bursts, the hesitation,
`TEST DE W1AW K`, `DE DE`, the lone and stray marks, both noise tests, the five pitches, the
drifting station, the quiet dit and dah, the strength table with the fit, the strong bulletin
identical with the fit on and off.

---

## 6. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 520 - STEP 12`, one paragraph naming what landed.
- `PHASE_STATUS.md`, both copies: names 520.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-224**, headline *A fist is a sender; the best shape gets
  the terminal after one word gap; a mark is judged against noise, never against another
  sender*, naming unit 519's measurements.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling.

---

## 7. Report

Section 2, for the owner, in plain words:

- rebuild;
- **a rough fist is a station**, ranked well above noise, and prints;
- **the cleanest station gets the terminal** - Hamlet waits one word after the first station
  appears, then picks the best; it switches only at a pause;
- **a station next to a louder one is no longer thrown away** because the loud one's keying
  bleeds into the comparison;
- nothing about letters changed.

Section 1: what changed, file by file, per task; the tightness figure and its reason; **the gate
table from task 3**; and that the build and the app line are green. **Section 3: the fist scores
against noise first, then case 2's switch, then the gate table, then the existing cases.**
Section 4: anything left, a line each, and any task dropped.
