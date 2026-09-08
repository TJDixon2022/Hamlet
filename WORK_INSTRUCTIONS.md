# Work instruction 281 - text only where he hovers, and a belt for the count

```
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
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## THE TWO RULES THAT KILLED SESSIONS

**Tim's rulings of 2026-09-05, HM-DEC-155. Not this unit's to weigh.**

**1. A unit runs no test suite.** **A unit may run only the unit test it
constructs or rewrites in that work instruction**, filtered by exact name, in the
foreground, with a stated timeout of a few minutes. **An unfiltered `dotnet test`
on any project is forbidden.**

**2. Never background a command and poll for it.** Sessions were killed by the
watchdog sitting in `until grep -q "exited with code" ...; do sleep 15; done`
against a twelve-minute watchdog.

`dotnet build` is allowed, foregrounded, with a timeout.

---

## THE TOOL RULE

**This session's shell may refuse calls.** A refused shell call is a signal to
reach for the other tool, not to stop. Record every refusal verbatim. **Nothing in
this unit halts the loop.**

**Eight units have paid for these two tool facts:** this shell will not carry a
quoted heredoc containing an apostrophe, and it collapses a doubled backslash
inside one. Use script files and the file-editing tools.

---

## `ADVANCED` and `DRIFT`

**Every bench step of this phase is closed.** Steps D and E are Tim at his own
radio. **Every unit from here reports `ADVANCED: no` by construction.** Report it
honestly; the counter is measuring the plan's shape and that is the author's
problem.

```
DRIFT:  3 consecutive units without advance, carried from unit 280.
```

---

## Asks still outstanding

Carried inbound per HM-DEC-139.

1. **Two issues of one work-instruction number.** Unit 271, and 252 before it.
   **The author's error.** No unit action.
2. **`PM95` reads *southern Japan***, unit 271. **Not a defect.**
3. **`HM-OPEN-083` and `HM-OPEN-084`.** In `OPEN_ISSUES.md` by HM-DEC-140.
4. **Three pixels.** **Waiting on Tim.**
5. **`dt` and `hz` were never on the mine list.** **Waiting on Tim.**
6. **Where an outcome entry goes when the unit it corrects has none.** Unit 276,
   in the tree. **The ruling wants Tim's eye.**
7. **`PHASE_OUTCOME.md` is written by a tool no unit is told to run.** Named in
   task 8; **the general question is the author's.**
8. **Where the repeat fold stops**, unit 277, at `0c359cc`. **Still open.**
9. **Whether a faded row needs a second carrier of its meaning**, unit 279. **The
   hover carries it in words**; a glyph as well is Tim's.
10. **Whether counting subjects is a new assertion**, unit 279, at `4d91bfe`.
    **The ruling wants Tim's eye.**
11. **Whether the fade is still obvious now the row is a bubble**, unit 280. **Not
    measured.** Interacts with 9.
12. **Whether `ReceiveAdvice` is in scope for the same ruling.** Unit 280 asked
    and did not sweep it, correctly - every task named the Digital tab. **Tim's
    answer, 2026-09-08: yes, and the rule is app-wide.** **Task 2 discharges it.**

---

## Why this unit exists

**Unit 280 removed 838 characters from the Digital tab and the paragraph Tim was
complaining about is still on his screen**, running the full width, with his
contact count crammed at the end of it.

**That is the author's error, not the session's.** The order said the AGC prose was
*in the status bar*. It is not: it is `ReceiveAdvice`, **18 KB of it**, feeding a
Receive-help widget on the canvas. Unit 280 reported the mismatch and refused to
take on a third panel on its own reading of a ruling, which §12.6 is exactly for.
**Naming one tab was the mistake. The ruling was always app-wide.**

**And the ruling has sharpened.** Tim, 2026-09-08:

> **"I want clean visual screens with text only where I, the user, intentionally
> hover."**

Not *less* text. **None, unless he asked for it.** That takes out things the last
unit's mockups kept - `contacts` beside a number, `6 to 10` beside a bar, *click a
reply* beside a ring, `sent` under a bubble. **The number, the bar, the ring and
the alignment carry it; the words go on hover.**

**One exception, and it is the only one: a fault speaks unasked.** A clock that is
wrong, a slot that refused, a station Hamlet cannot place, a device that is not
there. Those are not decoration and they say so without being hovered. **Anything
merely working is silent.**

**And the count becomes a belt.** Tim, 2026-09-08: a coloured ring that rises with
the rank, martial-arts order, **and gold at ten thousand - go for the gold.**

```
PHASE GOAL:   Hamlet works stations on the air.
UNIT GOAL:    Nothing on any screen is a sentence unless it is a fault or he
              hovered for it, and his contact count wears a rank he can see from
              across the room.
ADVANCES:     nothing. Every bench step is closed and this unit says so plainly.
```

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
Report them; do not repair the instruction.

- **Unit 280 measured 1,367 -> 529 characters** of permanently-visible text on the
  Digital tab, off the realized window rather than from source, because the whole
  workspace holds ten literal strings totalling 71 characters. **Use the same
  method.**
- **`ReceiveAdvice` is about 18 KB** and surfaces in a collapsible Receive-help
  widget on the canvas, **not the status bar.**
- **Unit 280 built the turn ring** in four states with two- and three-word
  captions, and **kept two strings on the Send block on the author's instruction**:
  the clamp line and the boundary statement about Windows' volume and the radio's
  input gain.
- **Unit 278 put the count in the status bar; unit 280 made it 20 point** with the
  badge progress beside it.
- **The operator's grid is `FN00DJ`**, marked `verified` in Settings from
  callook.info, 2026-08-14.
- **`docs/unit280-what-was-removed.md`** lists what that unit moved and kept. **Read
  it before removing anything, so a string is not moved twice or a kept one
  re-justified.**
- Root version after unit 280 was **1.12.173**. **Read it, do not assume.**

Known reds, inherited, **never chased**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list
tripwire.

---

## Rulings in force

**Tim's, 2026-09-08:**

- **Text only where he intentionally hovers.** App-wide, every screen.
- **A fault speaks unasked.** The single exception.
- **The count is a belt**: white, yellow, orange, green, blue, purple, brown, red,
  black, **gold**, at 0, 10, 25, 50, 100, 500, 1000, 2000, 5000, 10000.
- **`ReceiveAdvice` is in scope.**

**Standing:**

- **§0.6. Colour is never the only carrier of meaning.** **The belt is a flourish
  on top of a number**: the count and the progress must read in grayscale without
  it. **Do not make the rank the only way to know anything.**
- **§0.0 and HM-DEC-092.** **Removing words must not remove a fact.** A boundary
  statement moved to hover is preserved; a boundary statement deleted is a fact
  lost. **Task 7 is the check.**
- **One click, one transmission.** Nothing here transmits, arms or cancels.
- **HM-DEC-012 and §0.5.** Family colour is text colour only; a bar is never
  filled with it. **The belt ring is a border, not a fill.**
- **§0.5.1 and HM-DEC-087.** Grey is reserved for a control that cannot be used.
- **§0.1.** The engine is not told that tabs exist.
- **`Ft8Sharp` is a faithful MIT port and nothing changes a line of it.**

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, and
`NOTE` saying what is moving inside the task. The same every ten minutes while a
task is running. **Use the file-editing tools if the shell refuses.**

---

## Tasks

### Task 1 - every permanent string in the application

**Reading and measuring only. Remove nothing in this task.**

- **Every string permanently visible on any screen**, not the Digital tab alone.
  Measure off the realized window as unit 280 did; **a source count finds nothing
  and then flatters the result.**
- **For each, classify it:** a fact worth keeping visible, a fault, or advice.
- **Report the total, per screen**, so task 7 can measure the same thing again.
- **Read `docs/unit280-what-was-removed.md` first**, so nothing is moved twice or a
  kept string re-argued.

### Task 2 - `ReceiveAdvice` and everything like it

**This is the goal task and it is the one unit 280 could not reach.**

- **Advice moves behind an icon with the text on hover.** `ReceiveAdvice`'s 18 KB
  is the largest instance and it is not the only one; task 1 found the rest.
- **The icon says what kind of thing it holds** - a tip, a measurement, a boundary
  - and the words appear only on hover.
- **Nothing is deleted.** Every sentence still exists in the application, one
  hover away. **A sentence removed is a fact lost and task 7 catches it.**
- **A fault is not advice.** A clock that is wrong, a slot that refused, a device
  that is not there: **those stay visible and stay in words**, unhovered.

### Task 3 - text only where he hovers

**Apply the rule to what units 278, 279 and 280 built, including strings a previous
order told them to keep.**

- **The turn ring's captions go.** *click a reply*, *yours is next*, *nothing heard
  yet* - the ring, its colour, its dash pattern and the count carry all four
  states. **On hover, the full sentence.**
- **`sent` under a bubble goes.** The alignment says it.
- **`contacts` and `6 to 10` go** from beside the count; task 4 replaces them.
- **The Send block keeps only facts**: the message, the time, the level, whether it
  clipped. **The clamp line and the Windows-volume boundary statement move to the
  icon** - the author told unit 280 to keep them visible and that was wrong. They
  are facts and hover preserves them.
- **The two column headers on the left list stay.** `utc snr message` labels
  columns that exist; that is not prose.

### Task 4 - the belt

- **Ten ranks**, in order: **white 0, yellow 10, orange 25, green 50, blue 100,
  purple 500, brown 1000, red 2000, black 5000, gold 10000.**
- **A ring around the count**, a border and never a fill (HM-DEC-012).
- **The number stays large and stays the primary carrier** (§0.6). **In grayscale
  the count and the progress must still read**; only the rank is lost, and that is
  the decoration.
- **The progress bar stays**, without its `6 to 10` label.
- **On hover**: the rank, the next colour, and how many contacts away. *Six more
  contacts and the ring turns yellow at 10.*
- **One list, one place.** Thresholds and colours in a single table, not scattered.
- Test, watched failing first: each of the ten counts yields its own rank; a
  grayscale render still shows the number and the bar; the ring is a border.

### Task 5 - the sender tooltip is not about him

**Observed 2026-09-08, on his own CQ**: *KC3QIS is calling anyone. He is in United
States of America, in grid FN00, and Hamlet needs your own grid square in Settings
before it can say how far away that is.*

- **A message he sent gets no sender tooltip.** He knows who sent it.
- **Shorten the entity name.** `United States of America` reads long where every
  other entity reads short.
- Test: a sent bubble has no sender tooltip; a received one does.

### Task 6 - the grid that is there and is not found

**The tooltip says Hamlet needs his grid square. It is `FN00DJ`, verified in
Settings from callook.info on 2026-08-14.**

- **Find why the distance calculation cannot reach it**, with file and line.
- **This is the third time a feature has failed to reach a value that was plainly
  present** - the menu on the wrong list, the Log item gated where it could never
  fire, and now this. **Say whether it is the same shape.**
- **Fix it**, and **if it cannot be fixed here, say exactly why** rather than
  leaving the tooltip asserting a setting is missing when it is not.
- Test, watched failing first: with a grid in Settings, a received message's
  tooltip carries a distance and a bearing and does not mention Settings at all.

### Task 7 - what was removed, listed and measured

- **A list of every string this unit deleted or moved**, with where it went,
  appended to `docs/unit280-what-was-removed.md` or beside it as this unit's own.
- **If a fact was lost rather than a sentence shortened, say so under its own
  heading.** Unit 280 did exactly this for the band leaving the worked-station
  hover.
- **Measure the total again**, per screen, against task 1's figure.

### Task 8 - the outcome entry

- **Append this unit's entry to `PHASE_OUTCOME.md`** through
  `tools\arbiter\outcome-append.bat`. **If the shell refuses, append with the
  file-editing tools in the format the existing entries use** and say so.
- **Do not back-fill units 273, 274 or 275.**

---

## Parked - do not touch, do not raise

- **The three pixels**, **`dt` and `hz` on the mine list**, **where the repeat fold
  stops**, **whether the fade needs a glyph**, **whether the fade still reads on a
  bubble**. All Tim's.
- **FT4.** On hold.
- **Editing or deleting a log record**, awards for anything but the count,
  uploading anywhere.
- **The send path, the abort, the composer, the waiting-stations row.**
- **Anything in `src/Ft8Sharp/`.**
- The OSD re-encoding count, `ReusableWindow`, `ProcessDelayForTests`, the tap's
  owner, the waterfall's first row, unit 237's Extensible conclusion, work
  instruction 231's four tree items, `validate-output.bat`'s permitted-spellings
  bug, the 101.33 ms pulse above 6 kHz, the CW decoder and its inherited reds.

---

## What not to do

- **Do not delete a sentence.** Move it to hover.
- **Do not hide a fault behind a hover.** Faults speak unasked.
- **Do not make the belt rank the only carrier of anything** (§0.6).
- **Do not fill the belt ring.** It is a border.
- **Do not remove a column header that labels a real column.**
- **Do not leave a tooltip claiming a setting is missing when it is present.**
- **Do not touch `src/Ft8Sharp/`.**
- **Do not run a test suite.** Only the tests you write or rewrite here, filtered,
  foregrounded, with a timeout.
- **Do not background a command and poll for it.**
- **Do not ship a placeholder token in a reported number.**
- **Do not report `ADVANCED: yes`.**

---

## Committing and pushing

Commit and push each task before starting the next. Bump the root version's patch
by one from whatever the tree carries. **`Ft8Sharp` does not move.**

---

## Reporting

`output.md` at the repository root, overwritten, four sections per
`CLAUDE_CODE.md` §8 with the header block above section 1.

**NUMBER: characters of permanently-visible text across the whole application,
before and after** - not the Digital tab alone. That was the author's error and the
figure should measure the ruling rather than one screen.

**Section 3 leads with four things:**

1. **The status bar and the Send block as they now render**, with every string that
   left named and where it went.
2. **The ten belt ranks**, and what the count looks like in grayscale.
3. **The four ring states with no captions**, and how they are still four.
4. **A received message's tooltip carrying a distance**, quoted - the grid found.

**Section 2 says what he will see change on his screen**, in his own terms: clean
screens, and words only where he asks for them.

**Carry the asks queue outbound, and carry `DRIFT` forward.**

Write `output.md`, then stop. Do not start the next unit.
