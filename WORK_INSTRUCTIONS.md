# Work instruction 530 - one front end, and the last four faults on the owner's recording

**Hand run. One unit, four tasks, commit per task, drop from the back. Task 1 is the unit.**

Earlier unit numbers are not cited in this order: each part of the tree is described by what it
does.

**R88 stays lifted for `tests\fixtures\cw\captured\cw-2026-10-02-200157.wav` alone** (the owner,
2026-10-02). No other recording is read. Synthetic hops written in a test are allowed. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line. **Every
existing reading case reads as at HEAD or better**, or the report says what changed and why.

**Numbering.** This is unit 530, ruling HM-DEC-234. If taken, use the next free and say so.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwSenderLane.cs
  MUST EXIST:      tests\fixtures\cw\captured\cw-2026-10-02-200157.wav
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
  once for a multi-line commit. Scripts go in `.run-unit\unit530-<name>.sh`, not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- **Tag HEAD `before-one-front-end`** before any engine change, pushed.
- **Nothing is tuned to the recording.** Every figure has a reason from what a keyed tone or a hand
  does.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. Where it stands

**The sender's own window is in** (`CwSenderLane`): once a sender stands it is mixed at its own
pitch and low-passed to its dit. On the owner's recording a dah's top wobbles 1.5 dB through it
against 3.4 to 3.7 dB on the 25 Hz grid. **The recording now reads
`F ER C H AT<BT> BEST MSV 73 <SK> KC4ZGP`** against the sent
`FER CHAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`. Four faults are left, and the tree still has **two
front ends**.

---

## 3. Task 1 - one front end, pitch as a result

The tree finds marks two ways: **the grid path**, shipped, finding bars bin by bin on 25 Hz bins
and taking a mark's pitch by walking to the louder neighbouring bin; and **shape-first**, switched
off, finding rectangles in time across the passband and taking pitch as the energy's centroid. The
sender's window now measures a standing station well on either. **The owner wants one front end.**

- **A mark's pitch is the energy's centroid over the mark**, on whichever path finds it. The walk to
  the louder neighbouring bin retires as a pitch decision.
- **Then compare the two paths**, each with the sender's window, on every reading case and the
  recording. **If shape-first reads every case as well or better, it becomes the only front end**
  and the grid path's decision code comes out. **Otherwise shape-first comes out** and the grid path
  is the only one, keeping the centroid pitch.
- **One front end remains either way**; the tag holds the other. Report which, and the case that
  decided it.

## 4. Task 2 - the sender is not let go before it has finished

**`DEWA` is lost.** The reader lets a sender go when its shape score falls under 0.1, and this
hand's letter gaps - 2 to 5.7 dits with no jump to its word gaps - score nought for tightness, so it
is released before its last word.

**The question the last report asked, answered here as a CW question from the pattern, not raised
to the owner: yes.** A sender's letter-gap tightness is scored on **its own letter cluster**, split
from its word cluster where the word line is drawn - not on all its gaps together. **And the
straight key's first pick**, which moved to `SKCCDE` when this was tried, **is fixed on its own**:
find why the first pick moved and say.

Green: the recording prints `DEWA`; the straight key reads `SKCC DE` in every condition.

## 5. Task 3 - the 7 before V

`--...` with one 120 ms gap inside it reads `M S`. The reader's line between a gap inside a letter
and one between letters sits at 100 to 114 ms, **leaning toward the tight gaps inside letters**
(70 to 82 ms) because it is drawn from their centre without the letter gaps' spread. **Draw it as
the boundary weighted by both clusters' spreads**, as the dit-or-dah line already is. Green: `7V`.

## 6. Task 4 - the first gaps of a transmission

`F ER C H AT`: the first gaps are judged before any letter cluster exists. **Until the sender has
shown its own letter gaps, a gap under three of its gaps inside letters is inside the letter** -
Morse's own 1:3 - and nothing is spaced. Green: `FER CHAT`. **And the bench helpers that never tell
the detector what it prints** - `TheLetterGapHoldsTests`, `WhichGateTurnsAwayW1awTests` and any
other - **wire the printed pitch as the app does**, so the window is tested where it runs; report
every reading that changes. **Drop candidate.**

---

## 7. The test

**`TheOwnersRecordingReads`**: letters `FERCHAT<BT>BEST7V73<SK>KC4ZGPDEWA`, spaces ignored, no space
inside `KC4ZGP`. **Red today.** Report the full text with its spaces. Plus the strength table at 8,
10, 12, 16 and 24 dB with the bench's 1 dB AGC and the filter; the between-bin cases; noise, 30 s
and 3 minutes, standing nothing.

---

## 8. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 530 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 530.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-234**, headline *One front end; a sender is held to its
  last word*, naming which front end remains and the tag that holds the other.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling.

---

## 9. Report

Section 2, for the owner, in plain words: rebuild; what the recording reads now beside what was
sent; which front end is now the only one; that a station is no longer dropped before its last
word. Section 1: per task, what was found and changed. **Section 3: the recording's text first, then
the front-end decision and its cases, then the strength table, then the existing cases.**
Section 4: anything left, a line each.
