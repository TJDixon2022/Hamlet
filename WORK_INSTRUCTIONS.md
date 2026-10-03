# Work instruction 532 - the reader becomes a lookup table

**Hand run. One unit, five tasks, commit per task. Tasks 1 to 3 are the unit and are a move: what
is read does not change, only where it is decided. Tasks 4 and 5 are small; 5 drops from the back.**

Earlier unit numbers are not cited: each part of the tree is described by what it does.

**R88 stays lifted for `tests\fixtures\cw\captured\cw-2026-10-02-200157.wav` alone** (the owner,
2026-10-02). No other recording is read. Synthetic hops written in a test are allowed. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line.

**Numbering.** This is unit 532, ruling HM-DEC-236. If taken, use the next free and say so.

---

## 0. The project gate

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwRunReader.cs
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
  once for a multi-line commit. Scripts go in `.run-unit\unit532-<name>.sh`, not committed.
- Nothing that keys or transmits. Nothing written to the radio.
- **Tag HEAD `before-lookup-table`** before any change, pushed.
- **Tasks 1 to 3 move code; they do not change it.** After each, **every printed reading in every
  set is identical to HEAD**, character for character, and the report says so. If one changes, the
  move was not a move: find why and put it right before going on.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. Why

**The owner, 2026-10-02:** *"If you do shape right, we should be able to pass the decoder nothing
but a pattern that says dash dash dot dot space dash dot dot space dot dot dot dash. And it doesn't
have to look at anything. It should just take our pattern and turn it into characters. Trivial."*
And: *"I want to really get to this lookup table and get rid of the decoder logic."*

**Where the decisions are now.** The shape side - the detector, the sender's window, the pattern
gate - decides whether a stretch is a mark, whether a sequence is a station, a mark's pitch, and
each gap's kind. **The reader still decides four things:** whether a mark is a dit or a dah; which
standing sender to print and when to let it go; and whether a one-mark letter, `E` or `T`, prints.
After this unit it decides none of them.

---

## 3. Task 1 - dit or dah is decided in the gate

**Every mark leaves the gate labelled `.` or `-`.** The rules move from the reader to the gate
**unchanged**: the sender's two length clusters and the spread-weighted line between them, the
hand's two kinds when no clean jump shows, the split of a letter's marks against its neighbours,
and the retry over the sender's newer marks when its speed changes. The reader receives labels, not
lengths.

## 4. Task 2 - the sender and the lone letter are decided in the gate

- **Which sender is printed** - the best shape after the first word gap, held until silent for its
  word gap and a dah, released on shape only under 0.1 - **moves to the gate unchanged.** The gate
  hands the reader one sender's stream.
- **The lone-letter rule** - a one-mark letter held until a letter of two marks or more from the same
  sender confirms it, and three one-mark letters in a row dropped - **moves to the gate unchanged.**
  The gate already knows where letters end. A one-mark letter that is never confirmed never leaves
  the gate.

## 5. Task 3 - what is left is the table

**The reader takes a stream of `.`, `-`, letter-end and word-end from the gate and looks each
letter up.** It keeps the Morse table, the prosigns, printed-stays-printed, and handing each
character to the transcript and the scroll as one event. **Nothing else.**

- **A test, `TheReaderIsALookupTable`, fails if the reader's source holds a numeric literal other
  than 0 or 1, or any time, length, level, pitch or score.** Name the file it reads.
- **`TheReaderReadsSymbols`**: `--..  -..  ...-` in, `ZDV` out; `.-.-.` is `<AR>`; an unknown pattern
  is the placeholder, as now.
- Say how many lines the reader is before and after.

## 6. Task 4 - the recording test expects the sender's timing

**The owner's recording reads every letter right.** The test's expected text was the web session's
mistake: the sender paused **579 ms** between the `C` and the `H` of `CHAT`, about 7.4 of his element
gaps - longer than a word gap by Morse's 1:3:7, and longer than he leaves between some of his words.
The web session's own element list showed it (`-.-. [580] ....`) and then wrote `CHAT` because it
knew the word. Timing cannot.

**`TheOwnersRecordingReads` expects the sender's timing:**
`FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`. Its remark says why. Green.

## 7. Task 5 - noise agrees within half a bin

**The last report asked; answered here as a CW question, not raised to the owner: yes.** A mark
agrees with a sequence's pitch **within half a bin either side**, one bin's width, now that pitch is
measured to the hertz. Noise then stands nothing at all. **The three tests that measured the
single-mark gates by counting standing noise** (`MostNoiseBarsHaveNoEdges`,
`RealMarksScoreInsideTheShapeAndNoiseOutside`, `TheShapeTurnsAwayNoiseThatPassedFiveLines`)
**count candidates instead**, and say so. Every reading case as at HEAD or better; report any that
changes. **Drop candidate.**

---

## 8. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 532 - STEP 12`, one paragraph.
- `PHASE_STATUS.md`, both copies: names 532.
- Patch-bump `Directory.Build.props`.
- `CLAUDE.md` §1 index row.
- `DECISIONS.md`, newest first, **HM-DEC-236**, headline *The reader is a lookup table*, quoting
  the owner, naming the tag, and listing every decision that moved to the gate.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling.

---

## 9. Report

Section 2, for the owner, in plain words: rebuild; nothing you read changes; the reader is now a
lookup table - every decision is made where the shape is found; your recording's test passes, on
your sender's own timing. Section 1: per task, what moved where; the reader's size before and
after. **Section 3: the identical-readings check for tasks 1 to 3 first, then the recording, then
task 5's counts.** Section 4: anything left, a line each - and what still stands between this tree
and removing the old decoder.
