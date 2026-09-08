# Work instruction 285 - the logo goes into the application

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

**1. A unit runs no test suite.** **Only the unit test it constructs or rewrites in
this work instruction**, filtered by exact name, foregrounded, with a stated
timeout. **An unfiltered `dotnet test` on any project is forbidden.**

**2. Never background a command and poll for it.** The watchdog fires after twelve
minutes with no status write.

`dotnet build` is allowed, foregrounded, with a timeout.

**Tool fact, eleven units old:** this shell will not carry a quoted heredoc
containing an apostrophe, and it collapses a doubled backslash inside one. Use
script files.

---

## `ADVANCED` and `DRIFT`

Every bench step of this phase is closed; steps D and E are Tim at his radio.
**`ADVANCED: no` by construction.**

```
DRIFT:  6 consecutive units without advance, carried from unit 283.
```

---

## Why this unit exists

**Hamlet has no logo.** The About window carries the name in text and nothing else,
and the window and taskbar carry whatever Avalonia's default is.

**Tim approved a mark on 2026-09-08.** A transceiver faceplate - LCD, S-meter with
a needle trace, a VFO knob with detent marks and a skirt, three band buttons with
one lit, and two smaller controls - **with a quill rising from the top edge as the
antenna.** The whip becomes a feather. That is the joke and it is the point: the
name is Shakespeare and the thing is a radio.

**Two files ship with this instruction, in `assets/`:**

- **`hamlet-logo.svg`** - the full mark, 320 x 260, faceplate and quill.
- **`hamlet-mark-small.svg`** - 68 x 68, a filled circle with a plate outline and
  the quill. **The faceplate detail collapses below about 56 px**, so this is a
  different drawing rather than the same one shrunk, and it is what belongs at
  taskbar and favicon sizes.

**The palette is the application's own**: parchment `#E8E2D6`, ink `#2E2A22`, the
rust `#993C1D` of the sender field, the amber `#EF9F27` of a strong signal.

```
UNIT GOAL:    Hamlet has a logo, it is in the About window and on the window and
              taskbar, and it renders at every size it is used at.
ADVANCES:     nothing.
```

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
Report them; do not repair the instruction.

- **`AboutWindow`** carries the name, the tagline *Let me ham. The radio,
  demystified.*, a build panel and a diagnostics panel. **Find where a mark would
  sit** and say so before placing it.
- **The application's icon** is whatever `Hamlet.App.csproj` sets, if anything.
  **Report what is there now.**
- **`AboutWindow` is capped by `HowMuchTheApplicationSaysTests` at 850 characters
  and holds 726.** An image adds no characters, but **check the ceiling still
  passes** rather than assuming.
- Root version - **read it, do not assume.**

Known reds, inherited, **never chased**:
`CwAdjudicationTests.ASpeedChangeInRealisticAudio`; the 51 CW cases in
`docs/unit239-failing-set.txt`; the `Ft8Sharp.Deep.Tests` whole-type-list tripwire;
`HM-OPEN-088`'s ten in `TheMessageReadsAsThreePartsTests`.

---

## Rulings in force

- **Tim approved this mark**, 2026-09-08. **Do not redesign it.** If something
  cannot be rendered as drawn, **report it and say what you would change** rather
  than changing it.
- **§0.6.** Colour is never the only carrier of meaning. **A logo carries no
  meaning the application depends on**, so this does not constrain it - but the
  mark must be legible in both light and dark themes, and **the small mark's dark
  fill needs checking against a dark background.**
- **Text only where he hovers.** **A logo is not a caption.** Do not add a line of
  text under it explaining what it is.
- **`Ft8Sharp` is a faithful MIT port and nothing changes a line of it.**
- **Nothing in this unit transmits, arms or cancels.**

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, and
`NOTE` saying what is moving inside the task. The same every ten minutes while a
task is running. **Use the file-editing tools if the shell refuses.**

---

## Tasks

### Task 1 - the files land where the application can reach them

- **Move `assets/hamlet-logo.svg` and `assets/hamlet-mark-small.svg`** to wherever
  this application already keeps its own resources. **Follow the tree's existing
  convention** rather than inventing a folder.
- **Report where they went and why that is the right place.**
- **Confirm both parse and both render** before anything binds to them.

### Task 2 - the mark is in the About window

**This is the goal task.**

- **The full mark appears in `AboutWindow`**, at a size where the faceplate detail
  reads - **150 px or larger.**
- **It sits with the name rather than replacing it.** The name is a wordmark and
  the logo is a picture; both belong.
- **No caption.** The mark is not labelled.
- **Check it in both themes.** The parchment fill is light; on a dark background
  the plate should still read as a plate rather than a bright rectangle. **If it
  does not, say so and say what would fix it** - a dark variant is a legitimate
  answer and is Tim's to approve.
- **The character ceiling still passes.** An image adds no characters; confirm
  rather than assume.

### Task 3 - the window and the taskbar

- **The application window and the taskbar carry the small mark.**
- **Whatever format the platform needs**, produced from `hamlet-mark-small.svg`
  rather than drawn again. **Say what you produced and how.**
- **Check it at 16 px.** That is the size where a logo either survives or turns to
  mush, and it is the one nobody looks at until it is shipped.
- **Report what it looked like** at 16, 32 and 48 px.

### Task 4 - a test that the mark is there

- **Assert the About window renders the mark**, not that a file exists on disk.
  **A resource that is present and unreferenced is the fault `HM-OPEN-087` is
  about** - thirteen `widget.*` templates in this tree are declared and reachable
  by nothing.
- **Watched failing first.**
- **A test that checks a path string is not a test of a rendered image.** Assert
  the visual element is in the tree with non-zero size.

### Task 5 - what the mark looks like where it is used

**Named drop candidate.**

**Report, in words, what the mark renders as** at each place it now appears - the
About window in both themes, the window frame, the taskbar, and 16 px. **Say which
of those you could verify and which you could not**, because a headless run cannot
see a taskbar.

**Do not claim a size looks right if nothing looked at it.**

### Task 6 - the outcome entry

- **Append this unit's entry to `PHASE_OUTCOME.md`** through
  `tools\arbiter\outcome-append.bat`. **If the shell refuses, append with the
  file-editing tools in the format the existing entries use** and say so.

---

## Parked - do not touch, do not raise

**Everything.** The status-bar paragraph, the asks queue, `HM-OPEN-087`,
`HM-OPEN-088`, the ceilings, Settings, the log, the send path, FT4, the hover
ring's findability. **None of it is this unit's and this unit is not to be
widened.**

---

## What not to do

- **Do not redesign the mark.** Report what will not render and stop.
- **Do not caption the logo.**
- **Do not draw the small mark again.** Produce it from the file.
- **Do not assert a file exists and call it a test.** Assert it renders.
- **Do not claim a size looks right if nothing looked at it.**
- **Do not fix anything else.**
- **Do not touch `src/Ft8Sharp/`.**
- **Do not run a test suite.** Only the test you write here.
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

**NUMBER: the sizes the mark was verified at, and by what.**

**Section 3 leads with three things:**

1. **Where the files went** and why that is the tree's convention.
2. **What the mark renders as** in the About window, both themes.
3. **What it looks like at 16 px**, and whether anything actually looked.

**Section 2 says what he will see**: Hamlet has a face.

Write `output.md`, then stop.
