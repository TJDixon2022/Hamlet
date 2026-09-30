# Work instruction 511 - the blocks stay, the sender's own dits count, and the old decoder retires

**Hand run. One unit, three tasks, drop from the back.** Commit per task. Task 1 is the owner's
report and is not dropped.

**No test against a recording, a fixture, a floor or copied telemetry** (R96). A headless test
driving synthetic hops written in the test itself is allowed; nothing read from disk. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line. **The
owner's report at the radio is the test.**

**Numbering.** This is unit 511 and its ruling id is HM-DEC-215. If either is taken, use the next
free one and say so.

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
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit511-<name>.sh`
  and are not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- **Every existing reading case reads exactly as at HEAD after every task**, or the report says
  what changed and why. Nothing is forced.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. Task 1 - a block on the scroll stays on the scroll (the owner's report)

**The owner, 2026-09-30, on the scroll after unit 509:** *"The letters are solid, but the bars,
the dashes and dots bars, tend to come and go."*

**Same fault as the letters, one layer down.** Unit 509 fixed the letters by drawing from a kept
list appended on the print event. The blocks are still asked of the detector every frame, and
the detector re-derives its last four seconds from its current state - which unit 507's pattern
gate keeps revising as marks stand or drop. A block there on one frame is not there on the next.

**The rule:** **a block that was drawn stays drawn** until time carries it off the left. The graph
keeps a list of blocks, appended when a mark stands, trimmed only by time, never re-derived from
the detector's live state. Same shape as unit 509's letters. **Display only; the engine does not
change.**

**Watch it fail first**, headless on the live path with synthetic hops: `CQ CQ DE N0CALL N0CALL K`
at 20 WPM, sampled every 50 ms - once a block appears it is present on every later frame until it
scrolls off. **Red today.** Then the 5 WPM Farnsworth call, the same. Then loud noise: no block.

## 3. Task 2 - the sender's own dits count (unit 510's question, answered)

Unit 510 measured why `SEPTEMBER` reads `SINHSPMBR`: a dit 6 dB down inside a letter of a strong
sender is found by the blind stage and **dropped by the pattern gate**, because 6 dB is outside the
sender's 3 dB level tolerance at that contrast. Its proposed ruling is adopted here, **as a CW
question answered from the pattern (R85), not raised to the owner:**

**Once a sender stands, a candidate at its pitch, of its dit or dah length within tolerance,
sitting inside one of its letters - between two of its marks, closer than a letter gap - is
admitted as its mark down to 6 dB under its level.** Twice the present tolerance, and only there.
Nowhere else does the tolerance change: between letters, between senders, and for a candidate at
another pitch, 3 dB stands. A second station's marks cannot join the first this way, because they
are at another pitch.

**Watch it fail first**: unit 510's case, the 24 dB call with the first dit of the first `L` 6 dB
down. **Red today**, `N0CAE IL`. Green when it reads `N0CALL`. Then unit 507's two-station case
must read as at HEAD - the quiet station's marks must not join the loud one - and both noise tests
must print nothing; report the counts.

## 4. Task 3 - the old decoder retires

**The owner, 2026-09-30:** *"We're running two decoders. We really don't need them both. Is there
a lot of dead code in the one we were developing? Seems like we're doing a lot of the work in the
shape shifting."*

**Yes.** Since unit 493 only `CwRunReader` reaches the screen. The work is in three files:
`CwEnvelopeDetector` finds bars, `CwPatternGate` decides which are marks, `CwRunReader` turns marks
into letters. Everything the shape approach replaced is still in the tree, behind switches,
feeding nothing:

- `CwProbabilisticDecoder`'s timing-only path - the lattice, the speed grid, the emission gate -
  behind `ReadsRuns`;
- `CwUnitEstimator`;
- `CwToneTracker` and `CwToneSurvey`, superseded by units 496 and 507;
- `CwKeyingMeter`'s swing test and `ConfidentSwingDb`;
- unit 489's three switches, gating nothing since 493;
- the second decoder of step 9 - `FldigiCwDecoder` under `Cw/Second/`, its calibration and
  arbitration (units 456 to 467) - which never votes;
- the mixdown path the tracker fed.

**Retire it:**

1. **First, tag HEAD** as `before-cw-cleanup`, so all of it stays in history, reachable by name.
2. **Then remove** what no longer reaches the screen or a kept test. For each file or type
   removed, one line in the report: what it was, which unit built it, which unit superseded it.
3. **Keep** whatever the surviving three files, the scope, the terminal, the capture sheet or the
   verdict row still call. If a retired piece is still called by something live, **say so and leave
   it** - do not refactor around it on the way past.
4. **Tests that only exercised retired code retire with it**, listed by name. **Tests that assert
   a reading stay**, and every one reads as at HEAD.
5. **`CW_REQUIREMENTS.md` section M** - the two-decoder rules, HM-REQ-120 to 129 - is marked
   **superseded** at its head with one paragraph saying why: the second decoder measured worse than
   ours on every real condition (unit 466), never voted, and the shape approach made the question
   moot. **The rows themselves are not deleted.**

**The build is green with warnings as errors, the app line is green, and every reading case reads
as at HEAD.** If removing something changes a reading, it was not dead: put it back and say so.

**Drop candidate:** this whole task, if time runs out, with the list of what would go stated in
section 4.

---

## 5. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 511 - STEP 12`, one paragraph naming which tasks
  landed.
- `PHASE_STATUS.md`, both copies: names 511.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-215**, headline *The blocks stay, the sender's own dits
  count, and the old decoder retires*, naming the tag and what was removed.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling.

---

## 6. Report

Section 2, for the owner, in plain words:

- rebuild;
- the blocks on the scroll no longer come and go;
- a strong station's quieter dits inside a letter are kept, so `SEPTEMBER` reads `SEPTEMBER`;
- the old decoder is gone from the tree and kept at the tag `before-cw-cleanup`; nothing about
  what reads changed.

Section 1: what changed, file by file, per task; the retired list with its one-line reasons; and
that the build, the app line and every reading case are as at HEAD. Section 3: the cases.
Section 4: anything left, a line each, and any task dropped by name.
