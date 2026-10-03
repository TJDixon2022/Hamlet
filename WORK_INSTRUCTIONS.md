# Work instruction 531 - the recording reads whole

**Hand run. One small unit, four tasks, commit per task. Tasks 1 and 2 are the unit; 3 and 4 drop
from the back.**

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**R88 stays lifted for `tests\fixtures\cw\captured\cw-2026-10-02-200157.wav` alone** (the owner,
2026-10-02). No other recording is read. Synthetic hops written in a test are allowed. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line. **Every
existing reading case reads as at HEAD or better**, or the report says what changed and why.

**Numbering.** This is unit 531, ruling HM-DEC-235. If taken, use the next free and say so.

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
  once for a multi-line commit. Scripts go in `.run-unit\unit531-<name>.sh`, not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- **Nothing is tuned to the recording.** Every figure comes from Morse's own 1:3:7.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. Where it stands

The owner's recording reads `F ER C H AT<BT> BEST MSV 73 <SK> KC4ZGP DEWA` against the sent
`FER CHAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`. **Every letter is right but the 7 before V.** Both
remaining faults are a line drawn from too little evidence.

---

## 3. Task 1 - the line inside a letter sits at Morse's midpoint

**The 7 before V reads `M S`.** Its one gap inside the letter is 120 ms, 1.6 dits. The line between
a gap inside a letter and a gap between letters is drawn weighted by each cluster's spread. Through
the sender's window the gaps inside letters measure as tight as the detector reads, so the
weighting pulls the line down to 1.4 dits, and the 7's gap falls on the letter side.

**The last report asked for a ruling; it is answered here as a CW question from Morse itself, not
raised to the owner: yes.** **For gaps, the line between the inside-letter cluster and the
letter cluster is their midpoint in log-length, each side weighed alike** - Morse's own 1:3 puts it
at √3, 1.73 dits. The spread-weighted boundary stays for dit and dah; it is superseded for gaps
only.

Green: `7V`. Every existing case as at HEAD or better.

## 4. Task 2 - the first seconds of a transmission

**`F ER C H AT`.** In the first seconds a sender's word line is drawn from one or two letter gaps,
so it lands too low: the 285 ms gap after the F (3.8 dits) reads as a word.

**The rule:** **until the sender's word cluster is trusted, its word line is never under Morse's
own midpoint between a letter gap and a word gap, √21 = 4.58 of its element gaps.** Once the word
cluster is trusted, the line is the sender's own, as now.

- A **Farnsworth** sender's own line sits above that and is unchanged.
- The **five-dit floor** stays as it is.
- **`KC4ZGP`** must still hold together, and the straight key still read `SKCC DE`.

Green: `FER CHAT`.

**And the recording now asserts its spacing.** `TheOwnersRecordingReads` asserts the whole text,
spaces included: **`FER CHAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`**. If it reads every letter right and a
space is still off, say which and why; do not loosen the assertion to pass it.

## 5. Task 3 - noise stands more marks since pitch went continuous

`MostNoiseBarsHaveNoEdges` is red: with each mark's pitch now measured to the hertz, thirty seconds
of loud noise stands 44 marks with the edge test on, where it stood 30. Nothing prints. **Find
why** - most likely the gate's "within one bin" agreement now groups noise marks at scattered
pitches it used to split across bins - **and fix that**, so the agreement means one bin's width
around the sequence's own pitch. Green: the test's own figure; noise prints nothing. **Drop
candidate.**

## 6. Task 4 - the random carrier prints

The random carrier, keyed with marks of random length at random gaps, now prints at 775 and 825 Hz
on the grid. A carrier passes every test on a single mark; what makes it not CW is that its gaps
form **no letter and word clusters** and its marks no steady pair of lengths. **Find which rule
lets it print** and close it from the pattern - never by a decibel. Green: it never prints, and the
clean sender beside it reads as at HEAD. **Drop candidate.**

---

## 7. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 531 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 531.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-235**, headline *A gap is judged at Morse's own midpoints
  until the sender has shown its own*, naming that it supersedes the spread-weighted gap line.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling.

---

## 8. Report

Section 2, for the owner, in plain words: rebuild; what the recording reads now beside what was
sent; that a station tuned into mid-sentence is spaced right from its first letters. Section 1: per
task, what was found and changed. **Section 3: the recording's text first, then each task's
cases, then the existing cases.** Section 4: anything left, a line each.
