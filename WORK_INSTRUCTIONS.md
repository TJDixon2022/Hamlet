# Work instruction 286 - the ring counts down, the badge announces itself, and Hamlet gets a face

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

**Tim's rulings of 2026-09-05, HM-DEC-155.**

**1. A unit runs no test suite.** **Only the unit tests it constructs or rewrites in
this work instruction**, filtered by exact name, foregrounded, with a stated
timeout. **An unfiltered `dotnet test` on any project is forbidden.**

**2. Never background a command and poll for it.** The watchdog fires after twelve
minutes with no status write.

`dotnet build` is allowed, foregrounded, with a timeout.

**Tool fact, twelve units old:** this shell will not carry a quoted heredoc
containing an apostrophe, and it collapses a doubled backslash inside one. Use
script files.

---

## `ADVANCED` and `DRIFT`

Every bench step of this phase is closed; steps D and E are Tim at his radio.
**`ADVANCED: no` by construction.**

```
DRIFT:  8 consecutive units without advance, carried from unit 285.
```

---

## Why this unit exists

**Three things the operator asked for, 2026-09-08.**

**One. The ring never counts down until somebody has transmitted.** Unit 277 ruled
that **whose turn it is** is unknown before a station has spoken, which is right -
parity is derived from what the other station sent and guessing would send him into
their slot. **But the slot clock is not the turn.** FT8 boundaries land on `:00`,
`:15`, `:30` and `:45` whatever anybody is doing.

**The author let the turn rule suppress the countdown too.** They are different
facts and only one of them is ever unknown. His words: *"Nobody's responded, but I
want to know how long I have till the next transmit cycle. That might make me
decide, oh, I'll do a CQ or I'll respond to this guy."*

**Right now the empty For you panel shows a bare `?` and no number**, which is the
one place he most needs the clock, because it is where he is deciding whether to
call.

**Two. A badge should announce itself.** Unit 278 built the thresholds and shows
progress inside the log window. **He is at 8 contacts.** At 10 he wants a dialog
saying what he has earned. **The author previously ruled against a dialog and he has
overruled it**; what remains is that it must not cost him a contact.

**Three. Hamlet has a face, and unit 285's drawing had two defects.** Both files ran
outside their own viewBox - the full mark by 18.5 units, the small one by 15.5,
cutting half the quill at every icon size. **Both are redrawn and both are fixed**,
verified by rasterising and reading the ink's bounding box rather than by reading
the numbers.

```
UNIT GOAL:    The ring always shows the seconds, a badge says so when it is earned
              without costing him a contact, and Hamlet has a face at every size it
              is drawn.
ADVANCES:     nothing.
```

---

## What ships with this instruction

**`assets/hamlet-logo.svg`** - 380 x 360. The transceiver display in the
application's parchment bezel: `USB-D`, `FIL1`, `RX`, `UTC`, the frequency in amber,
the S-meter with cream bars and rust past S9 - and the quill as the antenna, with a
spine and barbs branching off it **inside** the vane.

**`assets/hamlet-mark-small.svg`** - 64 x 64. A rust tile, a cream faceplate with a
dark display and knob, and the quill. **Not a shrink of the full mark**: a faithful
reduction made the feather a sliver, so the small one is a sibling drawn to survive
16 px.

**Both were rasterised and looked at**, at 380, 256, 48, 32 and 16 px. **The full
mark's ink sits at 18,5 to 359,348 in a 380 x 360 box** - inside, with margin.

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
Report them; do not repair the instruction.

- **Unit 285 built `SvgMark`**, which reads these files rather than transcribing
  them, is deliberately narrow, and **throws on anything it does not know**. **The
  two new files may use a construct it does not handle** - `<text>`, plain `<line>`,
  many `<rect>`s. **Check before assuming they load, and report what it refused.**
- **Unit 285 built `AppIcon`**, rasterising the small mark at 256 px at startup, and
  set `Window.Icon`. **The files change; the mechanism should not need to.**
- **Unit 285 pinned both files' clipping defects in `TheMarksRenderTests`.** Those
  assertions describe the old drawings and **will go red.** **Update them to the new
  geometry** - the ink inside the box - rather than deleting them.
- **Unit 277 built the turn ring** in four states, deriving parity from what the
  other station has sent.
- **Unit 278 built the badge thresholds** - 10, 25, 50, 100, 500, 1000, 2000, 5000,
  10000 - and unit 281 made them belt ranks ending gold. **The count is every logged
  contact.** Unit 278 also made the first look at the log **seed the level silently**,
  so a fresh install does not announce nine badges at once. **Do not undo that.**
- Root version after unit 285 was **1.12.205**. **Read it, do not assume.**

Known reds, inherited, **never chased**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list tripwire;
`HM-OPEN-088`'s ten.

---

## Rulings in force

**Tim's, 2026-09-08:**

- **The ring always shows the seconds to the next slot**, including where the For
  you panel is empty and the turn is unknown.
- **A dialog announces a badge when it is earned**, naming what was earned.
- **Rust for the tray icon.** **The approved marks ship with this order and are not
  to be redesigned.**

**Standing:**

- **One click, one transmission.** **The ring counts down and does nothing else** -
  it arms nothing, cancels nothing, and reaching zero sends nothing. **A countdown
  that fires is automatic sequencing in disguise and this phase forbids it.**
- **§0.0 and HM-DEC-092.** Never present a guess as a decode. **The turn stays
  unknown when it is unknown**; only the count is added.
- **§0.6.** Colour is never the only carrier. **The unknown ring keeps its dashed
  stroke** so it still reads as four states and not three.
- **Text only where he hovers**, and a fault speaks unasked.
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

### Task 1 - the ring always counts down

**This is the goal task.**

- **The ring drains and shows the seconds to the next slot boundary, always** -
  including in an empty For you panel where the turn is unknown. **The bare `?` with
  no number goes.**
- **The turn stays unknown when it is unknown.** Unit 277's rule is untouched: no
  parity is guessed. **The dashed ring says the turn is unknown; the number says the
  clock is not.**
- **The clock is `Ft8Slots.TrueUtc` and the measured offset**, not a second timing
  source.
- **It counts down and does nothing else.**
- **The hover says what the count is** - *next slot in 7 seconds* - rather than a
  claim about whose it is.
- Test, watched failing first: with no station having transmitted, the ring shows a
  count **and** the turn still reads unknown; the count comes from corrected UTC.

### Task 2 - a badge announces itself

**He is at 8 contacts. The next one it can fire on is 10.**

- **When a threshold is crossed, a dialog appears naming what was earned** - the
  rank and the count.
- **It must not cost him a contact.** He is often mid-exchange with fourteen seconds
  to reply. **It takes no keyboard focus, blocks no click, and does not close the
  right-click menu or the Send controls.** If it cannot be made non-blocking, **say
  so and say what it would take** rather than shipping one that steals focus.
- **It fires once per threshold**, on the crossing, and never again for that rank.
- **It never fires on first launch**, however many contacts the log already holds.
  **Unit 278 built that seeding deliberately** - do not undo it.
- **Crossing more than one threshold at once names them all.** Unit 278's rule:
  nine to twenty-six earns 10 **and** 25, not only the highest.
- Test, watched failing first: crossing 10 shows the dialog once; the same count
  again shows nothing; a fresh log at 40 contacts shows nothing on first look;
  crossing 9 to 26 names both.

### Task 3 - the new marks replace the old

- **Both files from `assets/` replace the ones unit 285 installed**, in the same
  place, by the same mechanism.
- **`SvgMark` may refuse a construct it does not know.** **Report exactly what it
  refused** and extend it narrowly, or say plainly what cannot be drawn. **Do not
  redraw the mark to suit the loader** - the mark is approved and the loader is not.
- **`TheMarksRenderTests` pins the old clipping figures and will go red.** **Update
  the assertions to the new geometry** - that the ink is inside the box - rather
  than removing them. **The assertion that a mark stays inside its own viewBox is
  worth keeping; it is the defect this replaces.**
- **The About window and the icon keep the placements unit 285 built.**

### Task 4 - what the marks look like where they are used

- **Report what each renders as** at 16, 32, 48 and 256 px and in About.
- **Say which you verified and how, and which you could not.** Unit 285 could verify
  none, because the headless backend composes without rasterising and `CopyPixels`
  throws. **If that is still true, say so** - do not compute a stroke width and call
  it a look.
- **Whether a rasterising harness is worth a package is on the asks queue and is
  Tim's.** Do not add one.

### Task 5 - what changed, listed

**Named drop candidate.**

- Every string and every assertion this unit changed, appended to the log units 280
  to 285 keep.
- **If a fact left a screen without arriving on a hover, say so under its own
  heading.**

### Task 6 - the outcome entry

- **Append this unit's entry to `PHASE_OUTCOME.md`** through
  `tools\arbiter\outcome-append.bat`. **If the shell refuses, append with the
  file-editing tools in the format the existing entries use** and say so.

---

## Parked - do not touch, do not raise

**The whole asks queue**: the three pixels, `dt` and `hz` on the mine list, the
repeat fold, the fade's second carrier, counting subjects, the fade on a bubble,
`HM-OPEN-087`, the hover ring's findability, `AboutWindow`'s terseness,
`HM-OPEN-088`, three admissions on the simulated radio, and whether a rasterising
harness is worth a package. **None of it is this unit's.**

Also parked: FT4, the send path, the abort, the composer, the log's fields, the
ceilings, Settings. **Anything in `src/Ft8Sharp/`.**

---

## What not to do

- **Do not let the ring transmit, arm, or cancel anything.**
- **Do not guess whose turn it is.** Only the count is added.
- **Do not let the badge dialog take focus or block a click.**
- **Do not fire a badge on first launch.**
- **Do not redraw the marks.** Report what will not load.
- **Do not delete the viewBox assertions.** Update them.
- **Do not add a rasterising package.**
- **Do not compute a size and call it verified.**
- **Do not touch `src/Ft8Sharp/`.**
- **Do not run a test suite.**
- **Do not background a command and poll for it.**
- **Do not report `ADVANCED: yes`.**

---

## Committing and pushing

Commit and push each task before starting the next. Bump the root version's patch by
one. **`Ft8Sharp` does not move.**

---

## Reporting

`output.md` at the repository root, overwritten, four sections per
`CLAUDE_CODE.md` §8.

**NUMBER: the seconds the ring shows with the turn unknown, before and after.** It
showed none.

**Section 3 leads with four things:**

1. **The ring with the turn unknown**, showing a count and still reading unknown.
2. **The badge dialog at 10**, quoted - and what it does at 9 to 26.
3. **What `SvgMark` refused**, if anything, and what was done about it.
4. **What was verified by looking**, and what was not.

**Section 2 says what he will see**: the clock is there whether or not anybody has
spoken, the tenth contact says so, and the taskbar is not ugly.

Write `output.md`, then stop.
