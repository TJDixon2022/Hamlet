# Work instruction 509 - the next things, in the order they pay

**Hand run. One unit, seven tasks, drop from the back.** Commit per task. If time runs out, the
later tasks are dropped and the report says which. **Tasks 1 and 2 are the owner's asks and are
not dropped.**

**No test against a recording, a fixture, a floor or copied telemetry** (R96). A headless test
driving synthetic hops written in the test itself is allowed; nothing read from disk. Verify by
building `Hamlet.sln` with warnings as errors and running the app carry-forward line. **The
owner's report at the radio is the test.**

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
  here; `-m` more than once for a multi-line commit. Scripts go in `.run-unit\unit509-<name>.sh`
  and are not committed.
- Nothing that keys or transmits. Nothing written to the radio. **Nothing reaches the network
  from the app or a test** - `TheTestsStayOffTheNetworkTests` stays green.
- **Every existing case reads exactly as at HEAD after every task**, or the report says what
  changed and why. Nothing is forced.
- `output.md` at the root, four headings exactly: `## 1. What Claude did`, `## 2. What the owner
  should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

---

## 2. Where CW stands, so the tasks are aimed

**Reading, on the air, 2026-09-30.** A 34 WPM W1AW bulletin: `AN INCLUDED WOOD FIXTURE MADE
POSITIONING AND SOLDERING THE BRASS TUBE EASY`. A hand-sent 40 m QSO: `NICELY INTO MK TOYOTA PRIUS
BOTH WITH AND WITHOUT THE HYBRID ENGINE RUNNING`. 5 WPM Farnsworth practice as words. Near zero junk
on noise. **The turnaround was the owner's rule: a signal is a shape, and noise cannot make the
pattern** (R110, R112).

**Where it is short, measured.** The bench reads a call whole from 16 dB over the noise; below that
dahs break in the envelope before any gate sees them (unit 507). Inside a strong bulletin a dit is
still sometimes lost - `SEPTEMBER` read `SINHSPMBR`. At 35 WPM on a weak signal every letter prints
as a word. A second station that stands as a sequence is found and never printed.

---

## 3. The tasks

### Task 1 - a vertical scroll bar on the terminal (the owner's ask)

The CW terminal fills and the oldest text goes out of reach. **Give it a vertical scroll bar** that
appears when the text exceeds the box, follows new text as it arrives, and stops following when
the owner scrolls up to read - resuming when he scrolls back to the bottom. Hover text on the bar
says so (§0.6). The terminal's height and the one layout do not change (R101).

**Watch it fail first**, headless: forty lines into the terminal, the bar is present, the newest
line is in view; scroll up, add ten more lines, the view holds; scroll to the bottom, it follows.

### Task 2 - copy to clipboard (the owner's ask)

**A `Copy` button beside `Clear`** that puts the terminal's whole text on the clipboard as plain
text, with the line breaks as shown and prosigns as their bracketed names. Hover says what it
copies. It is in the hover registry with the rest of the CW tab's controls.

**Watch it fail first**, headless: after a call, the clipboard holds the terminal's text.

### Task 3 - Hamlet scores itself against W1AW

**The yardstick this project has never had: real air, real key.** After a bulletin, the owner
pastes the ARRL's published text and Hamlet says how much of it was read.

- **A `Score` control on the CW tab**, near the W1AW button: a box to paste the sent text into, and
  a line that reads **`W1AW 7 PM bulletin: 94% of characters, 3 wrong, 2 missing, 1 extra`** once
  both sides exist.
- **The comparison** is the edit distance `CwScorer` already computes (unit 439), over the stretch
  of the terminal that aligns with the pasted text - the alignment is the author's, and the
  report says how the stretch is found. Punctuation and prosigns count; case does not.
- **The number is written to telemetry** as a row - `cw`, `w1aw_score` - with the schedule slot,
  the percentage and the three counts, so the record keeps every day's figure.
- **No fetching.** The owner pastes. If the app already has an outbound HTTP path that the network
  test permits, say so in section 4; do not add one.

**Watch it fail first**, headless: a driven decode of the clean call scored against its own text
reads 100%; against the same text with one letter changed reads one wrong.

### Task 4 - integrate over a dit, not a hop

**The biggest lever on weak signals.** A mark is judged hop by hop on a 10 ms window; a dit is 35
to 240 ms. **Integrating the bin's energy over a dit's length before deciding is the textbook
detector for a keyed tone in noise**, and it is the shape rule carried through: look for a block
of the expected width, not a tall enough point.

- **When the sender's dit is known** - the reader has it after two runs - the detector judges each
  candidate bar's level as the mean over a dit-length window centred on it, at the sender's pitch.
- **When it is not known**, judge as now.
- **Nothing else in the detector changes.** The proportional gates and the pattern gate read the
  integrated level in place of the hop level.

**Watch it fail first**, synthetic: unit 507's strength table - the call at 8, 12, 16 and 24 dB -
before and after. **The reason for the task is the 8 and 12 dB rows.** Report how many of the 65
marks stand at each, and what reads. Both noise tests must still print nothing; report the counts.

### Task 5 - once the sender is known, look for its marks

**Marks are found blind, then grouped. After a sequence stands, the sender's pitch, dit and level
are known.** Search for its marks with that knowledge: at its pitch, blocks of its dit or its dah,
at its level. This is what an ear does once it has the rhythm - it fills in the weak dits.

- A candidate that the blind stage missed but that fits the sender's pattern at the sender's pitch
  is a mark of that sender.
- It never invents a mark where the level is flat: the block must be there, at the sender's level
  within the tolerance, for the sender's length within the tolerance.

**Watch it fail first**, synthetic: a strong call with one dit attenuated 6 dB inside a letter.
**Red today** - report the letter it breaks; green when the dit is found by the sender's pattern
and the letter reads whole. And the strength table again.

### Task 6 - print every sequence that stands

Unit 507's pattern gate finds a second station as a second sequence; the reader prints one. **Each
sequence that stands gets its own line in the terminal, headed by its pitch** - `625 Hz:` and
`825 Hz:` - and the scroll draws each sender's blocks in its own row. The louder sender is first.

**Watch it fail first**, synthetic: unit 507's two-station case prints both stations, each whole,
each under its pitch.

### Task 7 - the word gaps at speed

At 35 WPM on a weak signal every letter prints as a word (unit 507's report). The reader's
word-gap arithmetic from units 500, 501 and 504 was set on stronger signals. **Find why the
between-letter gap reads as a word gap at 35 WPM and 10 dB** - most likely the gap-dit's smear
term outgrowing the dit at that speed - and fix it in the reader's arithmetic.

**Watch it fail first**: unit 507's 35 WPM at 10 dB, `C Q C Q D E N 0 C A L L`. Green when it reads
`CQ CQ DE N0CALL N0CALL K`. Every other speed case reads as at HEAD.

---

## 4. Record

- `PHASE_OUTCOME.md`, both copies: `## UNIT 509 - STEP 12`, one paragraph naming which tasks
  landed.
- `PHASE_STATUS.md`, both copies: names 509.
- Patch-bump `Directory.Build.props`.
- `DECISIONS.md`, newest first, **HM-DEC-212**, headline *Hamlet scores itself against W1AW, and a
  mark is judged over a dit's width*, naming the tasks that landed.
- **Touch no checkbox in `PHASE_PLAN.md`**, and add no ruling.

---

## 5. Report

Section 2, for the owner, in plain words, task by task, saying which landed and which were
dropped:

- the scroll bar and the copy button;
- how to score a bulletin: press the W1AW button, let it run, paste the ARRL text into the box,
  read the number;
- what weak stations should now do, with the bench's floor before and after;
- whether a second station now prints on its own line.

Section 1: what changed, file by file, per task, and that the build, the app line and the network
test are green. **Section 3: the strength table before and after tasks 4 and 5 at the top, then
each task's cases.** Section 4: anything left, a line each, and every dropped task by name.
