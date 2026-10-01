# Work instruction 523 - shape-first reads everything the per-bin path reads, then it is the path

**Hand run. One unit.** Unit 522 built shape-first and shipped it off: it reads further down,
finds a dah beside a carrier, and reads every click the same - and it loses a 12 WPM fist, two
stations at once, a sender who speeds up, and the rough fists. **Those are the grouping cases the
per-bin path spent fifteen units learning.** This unit makes shape-first read every one of them,
then switches it on.

**No test against a recording, a fixture, a floor or copied telemetry** (R96). A headless test
driving synthetic hops written in the test itself is allowed; nothing read from disk. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line.

**Numbering.** This is unit 523, ruling HM-DEC-227. If taken, use the next free and say so.
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
  once for a multi-line commit. Scripts go in `.run-unit\unit523-<name>.sh`, not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- **`before-shape-first` is the tag.** Nothing is deleted; the per-bin path stays behind
  `ShapeFirst` until a later cleanup.
- **Nothing is tuned to a result.** Every figure changed has a reason from what a keyed tone or a
  hand does, stated in the report.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. The work, in unit 522's order

**With `ShapeFirst` on**, each case is red today. Take them in this order, commit per case,
and after each one every earlier case and every existing case must still read:

### 1. A 12 WPM fist reads nothing

Its dahs are partly missed and nothing stands. **Find why first**: at 12 WPM a dah is 300 ms; the
whole-band fit sweeps lengths from 25 ms to a 5 WPM dah, so the length is covered - so either
the fit's score on a fist's dah falls under the threshold (a hand's dah is not flat to a
machine's degree), or the level read over the hops inside its edges lands outside the pattern
gate's tolerance, or the span's end is placed short and the dah is read as two. **Measure which,
with the fist's dahs' fit scores and levels printed**, then fix that one thing. Green: the 12 WPM
fist at a fifth reads `CQ CQ DE N0CALL N0CALL K`.

### 2. Two stations garble

`CM CT A IE EMMCAE EL N0CALL K`. Unit 522 splits overlapping senders at two peaks a quarter of
the strongest or more, 50 Hz apart. **Find why the loud one's marks are still contaminated**:
whether the split is missed when the quiet sender keys inside a loud dah (the whole-band
rectangle then has a bump, not a second rectangle), or whether a split mark is refitted on the
wrong bins. Fix that one thing. Green: the loud sender reads whole and the quiet one stands, as
the per-bin path has it.

### 3. A sender who speeds up garbles

`… K ■H■S` where `TEST DE W1AW K` was sent at 20 WPM after 10. The standing sender's lengths are
fed to the fit; **when the sender speeds up, the fit is still trying its old lengths**, or the
pattern gate's clusters are, as unit 513 found. Fix it so the fit follows the sweep, not the
sender's history, when the two disagree. Green: the speed-change case reads whole.

### 4. The rough fists slip a letter

The 30% fist reads `N0CE LL`; the tightening fist `DEN ■CALL`. Likely the same cause as 1 at a
different speed - a hand's dah fits a rectangle less well than a machine's. If 1's fix covers it,
say so; if not, fix it. Green: both fists whole.

### 5. The two no-detection cases

`NoDetectionNoLettersTests`' cases that fail with shape-first on. **Report which and why.** If a
noise sequence stands on this path, that is a real leak and the gate's five-mark rule or the
0.2 score must catch it; if the test asserts a per-bin mechanism that no longer exists, re-pin it
and say so.

### 6. A clean sender's marks under a louder neighbour

The neighbour at 200 Hz now prints nothing instead of the carrier - better - but 47 of the clean
sender's 65 marks lie under the carrier's marks in time and only 8 are split off. **This is the
owner's crowded-band case.** The split in frequency over a span is the tool: where the loud
rectangle's span holds a second energy blob at a different centroid, that blob is its own mark
even when their time edges coincide. Green: the clean sender reads whole beside the carrier at
200 Hz. **Drop candidate**, with the count stated; the rest is not.

### 7. Switch it on

`ShapeFirst` defaults **on**. The per-hop gates' own tests - edges, narrowness, the shape gate,
which-gate-turns-W1AW-away - set `ShapeFirst = false` themselves and say so, since they test the
per-bin path. **Every existing case reads as at HEAD with shape-first on**; where one reads
better, say so with its text; where one reads worse, that case is not done and the switch stays
off.

---

## 3. What to report

**The strength table with shape-first on**, 8 to 24 dB. **The light and the gauge on the live
path** with it on: the clean call's sequence as unit 522 printed it. **Noise, 30 s and 3 min**:
nothing stands, the gauge never crosses.

---

## 4. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 523 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 523.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-227**, headline *Shape-first is the path*, naming each
  of the six cases and what was found and fixed, and that the per-bin path stays behind the
  switch at the tag.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling.

---

## 5. Report

Section 2, for the owner, in plain words:

- rebuild;
- **the shape-first front end is now what reads**: the rectangle is found in time, pitch is where
  the energy was, nothing is one click off, a station beside a carrier is found;
- weak stations read further down than before - the bench floor is in section 3;
- fists, slow senders, speed changes and two stations read as they did;
- **if anything reads worse than yesterday, that is the one thing to report back.**

Section 1: per case, what was found, what was changed and why, and that the build and the app
line are green. **Section 3: the six cases before and after, then the strength table, then the
gauge, then the existing cases.** Section 4: anything left, a line each, and whether the switch
is on.
