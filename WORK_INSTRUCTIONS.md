# Work instruction 574 - A found weak station prints dits: Ken at 425 Hz

**Hand run. Three tasks, commit per task. Task 1 is a measurement and always ships as a report;
task 2 ships only on the condition in §4; task 3 is small and droppable.** This zip carries one
recording, placed by the owner's order of 2026-10-19 (*"put it in"*):

```
tests\fixtures\cw\captured\cw-2026-10-19-141708.wav   300 s, 48 kHz, 7.0519 MHz, 40 m
tests\fixtures\cw\captured\cw-2026-10-19-141708.txt   its sheet
```

It is the first live capture with the weak end on (`weak on`, 1.13.256, AGC SLOW). **It is not
added to either scoreboard.** It is a trace recording with no truth: nobody wrote down what Ken
sent. Add it to the survey's list as a file without a truth, like the old captures, and to
`.gitattributes` as the other captures are.

**What is in it.** A QSO on 7.0519. KB2UZK, a 13 WPM hand at 500 Hz, S7, reads on the screen like
a log entry: `GM KEN NICE TO MEET U … RPRT UR 559 … MOBILE STN … QTH IS … CHICOPEE MA … PARKED IN
MY MOBILE <BT> … DE KB2UZK`. **The other side, Ken, sits at about 425 Hz, 75 Hz below, and keys
from about 272 s to the end of the file.** Measured by hand through a lane at 425 Hz with a 100 ms
dit, he is a real station at about 24 dB of contrast. The sheet agrees: `bank … best 425 Hz shape
0.59 at a 100 ms dit`. **What printed for him was** `EIE E EIET EI I I E SE I DE MM TT XF N IE E
EI E E EI E E NEE ETO` **- a stray burst, which fired the other capture.** A station the bank
stood on, handed on, and printed as dits.

**This is the same thing the 5 dB CQs show on replay** (`N0 IL N0N L I K` for `N0CALL N0CALL K`:
letters losing elements), now on a real signal with a recording. The CQs have a truth; Ken does
not. **The CQs are the yardstick; Ken is the specimen.**

**Standing rules** (HM-DEC-276): one setting per unit, no side variants; do not stop to ask -
take the choice that keeps both boards and name it in section 4; a run stopped for memory is run
once more alone, then reported as not measured.

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**Nothing keys or transmits. Nothing is written to the radio.** The one recording above is added;
no other is added, removed or promoted.

**Numbering.** This is unit 574, ruling HM-DEC-278. If taken, use the next free and say so.
(HM-DEC-269 stays held for unit 565.)

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      docs\cw-scoreboard.md
  MUST EXIST:      tests\fixtures\cw\captured\cw-2026-10-19-141708.wav
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
- One `dotnet test` invocation per line, filtered, with a `timeout`. The survey line gets 1800 s.
- Apostrophes in quoted heredocs break; `;` is refused; Python cannot run here; `-m` more than
  once for a multi-line commit. Scripts go in `.run-unit\unit574-<name>.sh`, not committed.
- **The one layout holds**: nothing on screen moves.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`. Status on the tree's
  own triggers; the phase count as `PHASE_STATUS.md` holds it; the asks list carried forward.
- The recording is committed in the first commit, with its sheet, before any code.

---

## 2. Task 1 - the trace, always shipped

**Ken's over, 272 to 300 s of the recording, through the chain as shipped**, one table and one
paragraph:

1. **His lane:** which lane stood on him (pitch, dit size), from what second, its shape, its
   contrast median and p10 over the over, and when it was handed on.
2. **His marks, as the lane found them**, in order, with lengths in ms, and the gaps between
   them; then the same marks **as the pattern gate kinded them** (dit, dah, inside gap, letter
   gap, word gap) and the two clusters it held (centres, and the √3 jump between them or not).
3. **What the reader printed**, mark by mark against the kinds, so each `E`, `I`, `T` is traced
   to the marks that made it.
4. **His marks by hand**, through a lane opened at his true pitch (find it to 5 Hz) with a 100 ms
   dit: the same mark list, at the level rule (up at 0.6 of contrast, down at 0.4, half a dit at
   least); the lengths sorted, so the eye can see whether there are two clusters or a smear.
   The owner's own measurement by hand found lengths from 35 to 410 ms with no clean gap between
   dits and dahs - say whether the tree's lane sees the same.
5. **The same four rows for `cq-18wpm-5db`**, which has a truth, so the two can be compared on
   one page: what its lane finds, how the gate kinds it, what prints, and the by-hand marks.

**The paragraph:** where the letters are lost - the lane (marks that are not there), the level
rule (dahs cut into pieces by the lane's own noise), the gate (two clusters not found, so the
kinds are wrong), or the reader - and whether Ken and the CQ fail at the same place. **Name one
rule** that the table supports, or say none does.

## 3. Task 2 - the one rule, if the table names it

Candidates, in order; **only the one the table names is built**; the others are not tried:

- **The lane's mark end is held through its own noise:** a lane's mark ends only when the level
  has stayed under 0.4 for a quarter of the lane's dit (the bank already does this for 0.6 - say
  whether the same hold applies at 0.4, and if not, apply it), and a dip shorter than that inside
  a mark does not end it.
- **The lane's clusters are its own, not the grid's:** a station handed on from a lane is kinded
  on the lane's marks alone, with the gate's memory cleared of any grid pieces at that pitch, so
  the two clusters form from his marks and not from the grid's 20 to 35 ms splinters.
- **The level lines on a lane sit on its own noise:** up and down at a set number of spreads
  above the lane's key-up floor rather than at 0.4 and 0.6 of contrast, where the contrast is
  low.

**Ships only if:**

- the **four 5 dB CQs' total rises** (from 13 + 14 + 16 + 19 = 62 of 84) and none falls under
  half;
- **Ken's over** prints fewer lone one- and two-element words per 10 s than as shipped, and at
  least one word of three or more letters that was not there before (reported, since he has no
  truth);
- **both boards hold:** 297 stands or rises, 2111 stands or rises, wrong and invented do not
  rise on either;
- **the hard limits hold:** noise prints nothing, the carrier at no more than one of twenty
  seeds, the first recording whole, nothing in silences, no old capture above six strays per
  10 s.

If it fails, say which condition, by how much, which file; leave the rule behind a switch off by
default; keep the trace.

## 4. Task 3 - splinters are not senders (droppable)

The sheet's `senders` line for this capture reads `500 Hz shape 0.02, 445 marks; 500 Hz shape
0.00, 276 marks` - KB2UZK's own splinters from the grid, held as senders, and they are what
tripped the `four or more senders held` capture trigger. **A sequence of shape under 0.1 is not a
held sender for the trigger's count or the sheet's line**; it is listed, if at all, as `splinters
at 500 Hz, 445 marks`. Nothing about reading changes; both boards as at HEAD. Drop if time is
short.

---

## 5. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 574 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 574.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-278**, headline *A found weak station prints dits*,
  quoting the owner's order to keep the recording, naming where the letters are lost, the rule
  shipped or that none was, and the figures before and after (the CQs, Ken's strays, the boards).
- `docs\cw-scoreboard.md`: a 574 row in each table; a line under the tables naming the recording
  as a trace recording with no truth, not scored.
- `docs\cw-survey-2026-10-09.md`: the recording in the unadjudicated group (rerun the survey).
- `CW_SPEC.md` §2: one sentence if a rule shipped.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling beyond HM-DEC-278.

---

## 6. Report

Section 2, for the owner, in plain words, leading with **whether a weak station found by the
bank now prints letters instead of dits and what it cost** - Ken's over before and after, the
four CQs before and after, the boards - then where the letters were being lost, in two
sentences. Section 1: per task. **Section 3: the trace tables for Ken and the CQ side by side;
Ken's over as printed before and after; the four CQs; both scoreboards' rows.** Section 4:
anything left, a line each.
