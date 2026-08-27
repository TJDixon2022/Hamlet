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
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.

---

# Work instruction 028 — three workspaces, and the canvas is gone

**ISSUED: 2026-08-27. A fresh order, not an amendment.**

**Three tasks; task 3 is the drop. No decoder file is touched, and the engine's
failing set being byte-identical at the end is the proof.**

## Why this unit exists

**The unit's number: two CW terminals, one screen.**

Tim photographed the CW tab after unit 1.11.24. Receive and Send are in place at
the top — **and the entire old canvas is still beneath them**, restored from his
saved layout: a second neighborhood map, the dial tape, the waterfall, **a second
CW terminal**, and the advice panel. Above them sits the preset bar and the
layout namer; down the left is the "Add to the canvas" column.

**Two failures, both in the previous order rather than in the session that
executed it.**

1. **It said to move widgets to the tray and forbade deleting any.** That
   protected his saved arrangements, which is not what he asked for. His words:
   *"I don't care when it destroys. We're abandoning all of that."*
2. **It gave Digital and Voice "a single line naming what will live there."**
   That is decoration, not behaviour. **A tab that does not change the screen is
   not a tab**, and the previous order specified it that way.

**And Send is not part of the CW workspace at all** — it sits outside the tab's
own area, to the right of the canvas, which is why it reads as detached. Tim:
*"I want the CW campus to be a single piece… They're not widgets. They're not
removable. They're permanent parts of the CW workspace. Makes no sense to have a
CW workspace without at least those two elements."*

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and
report any mismatch, including where the work succeeded anyway.

**Expected state: 28 failing of 1841 in the engine as the stable set; 527 of 527
in the app. Seven timing intermittents exist.** Do not chase any; diff which
tests moved and never trust a total.

**`AppSettings.UseJointDecoder` and `AppSettings.ShowKeyingSweep` both ship false
and stay false.**

**Do not verify by headless hit-testing.** Unit 1.11.13's rule stands: assert the
geometry that causes the fault — visual-tree order, render bounds, clipping
ancestors, reference identity — never that a point reaches a control.

**`CLAUDE_CODE.md` is at version 1.6.** Read its own section count.

**`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150, nor Tim's
rulings of 2026-08-25/26/27.**

## Rulings in force

**Tim's ruling, 2026-08-27, in his words.** *"CW should come up in its own canvas
with send on the left, receive on the right, nothing else. Data should have an
empty canvas. Voice should have an empty canvas. Those tabs should work."*

- **Three workspaces below the divider: CW, Digital, Voice.** Selecting a tab
  changes what is below it and nothing above it.
- **The CW workspace is one piece: Send on the left, Receive on the right,
  nothing else.** **They are not widgets. They are not removable, not
  draggable, not closable, and they are not in any catalogue.**
- **Digital is empty. Voice is empty.** No panels, no placeholder text, no
  controls.
- **The widget canvas is removed from all three workspaces**, with everything
  that serves it: the tray, the preset bar, the layout namer and the saved
  arrangements.

**Tim's ruling, same date, on what that destroys:** *"I don't care when it
destroys. We're abandoning all of that."* **Saved layouts, presets and the
arrangement machinery are abandoned deliberately.** A saved `layouts.json` that
no longer loads is the intended outcome, not a regression.

**HM-DEC-086 — "nobody ever starts on an empty canvas" — is superseded for these
three workspaces by this ruling**, and the supersession is recorded rather than
worked around. The reasoning unit 1.11.24 offered stands and is now explicit: what
that ruling forbids is a puzzle handed to somebody who came to talk on the radio.
**CW opens on two working panels. Digital and Voice are empty because they have
nothing to do yet, and saying so honestly is better than furnishing them with
text that does nothing.**

**Untouched:** the header above the divider — band plan, neighborhood, radio — and
the divider itself; HM-DEC-141's wavelength proportions; every decoder behaviour;
the pitch controls staying off.

**Rejected already, do not revisit:** keeping the canvas anywhere in these three
workspaces; keeping Receive or Send in a catalogue; furnishing Digital or Voice
with placeholder text; wiring Send to the transmitter (§0.2, HM-DEC-098 — the
interlocks have never been watched firing into a dummy load).

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is
moving. Same every ten minutes while a task runs.

## The tasks

### Task 1 — the CW workspace, as one piece

**Send on the left, Receive on the right, inside the CW tab's own area, and
nothing else in it.**

Neither is a widget. **Remove both from the widget catalogue** — the terminal's
duplicate that unit 1.11.24 reported, and Send — so neither can be dragged out,
closed or placed twice. Neither carries a close button.

**Assert from render bounds, at the application's default width and at a narrower
one:**

- the CW workspace contains **exactly two panels**;
- **Send's left edge is at the workspace's left edge, and Receive is to its
  right** — the photographed fault was Send sitting outside the workspace
  entirely, to the right of everything;
- **no decoded line in Receive is narrower than forty characters**, the
  assertion unit 1.11.24 introduced and measured at 61 and 81;
- neither panel has a close affordance;
- the tab strip still begins at the workspace's left edge.

**Report which is the wider**, and say plainly if the ruling's left-right order
makes Receive the narrower of the two — Tim named the order, not the widths, and
Receive is the panel he reads.

Build and run; record the baseline by diffing which tests fail.

### Task 2 — the canvas is gone, and the tabs work

**Remove the widget canvas from all three workspaces**, and with it the tray, the
preset bar, the layout namer and the saved-arrangement machinery.

**Digital and Voice are empty.** Selecting either shows an empty workspace.
Selecting CW shows Send and Receive.

**Assert:**

- **each of the three tabs changes the workspace below it**, by reference
  identity — CW's two panels are present on CW, absent on Digital, absent on
  Voice, and the same objects on returning to CW;
- **nothing above the divider is re-created** on any tab change — band plan,
  neighborhood and radio, by reference identity, as unit 1.11.23 asserted them;
- **no canvas, tray, preset bar or layout namer exists anywhere in the three
  workspaces.**

**Report what was deleted, by name** — every widget definition, every preset,
every stored arrangement — so the abandonment is on the record. **Tests pinning
the removed behaviour are updated to say what is now true, with the reason at the
site, and none is deleted.**

**If any widget in the catalogue is reachable from somewhere other than these
three workspaces, say so and leave that route alone** — this ruling covers the
three workspaces, not the whole application.

### Task 3 — what the widgets were *(the drop candidate)*

**Before or as they go, record what is being abandoned**: each widget's name and
the one line describing what it did — the scanner, the phrasebook, the field
guide, "did anybody hear me", "call CQ on a cycle", and the rest. **Write it to
`ABANDONED_WIDGETS.md` at the repository root**, so that rebuilding any of them
later as a real panel starts from a list rather than from git archaeology.

**Dropped whole if time runs out, and the report says so** — the deletion still
happens in task 2; only the record is at risk.

## Parked — do not touch, do not raise

Every decoder question: admission, the six axis families, the gate, the squelch,
the joint decoder, the constrained margin, the tracker, the meter, the integrator
width, the whole-file second pass, the reference and port difference, the
short-character bias, `001520`'s quadrillions, `013347`'s 17.2 million. Also:
per-mode widget placement — **void, there is no canvas**; the Send button that
does not send; `CHANGELOG.md`; the seven intermittents; the Avalonia geometry
offset; HM-OPEN-057; HM-OPEN-059; **the header above the divider.**

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not touch any decoder file.** The byte-identical failing set is the claim.
- **Do not keep the canvas anywhere in these three workspaces**, and do not
  preserve a saved arrangement in order to be helpful. The ruling is explicit.
- **Do not leave Receive or Send in any catalogue.**
- **Do not put placeholder text in Digital or Voice.**
- **Do not touch anything above the divider.**
- **Do not wire Send to the transmitter.**
- **Do not verify by headless hit-testing.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 2 leads with what each of the three tabs shows.** **Section 3 leads
with the assertions: two panels on CW, Send at the workspace's left edge, the
forty-character floor, each tab changing the workspace, and nothing above the
divider re-created.**

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Twenty inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's rulings of 2026-08-25/26/27, including the two this unit acts under.**
5. **The tone tracker** — six axis families measured; the question is a design
   one.
6. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named** — the operator's own item
   five, still not attempted.
10. **The keying meter** — its measurement found a station its verdict denied.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **The gate opens on everything, including two empty recordings** (1.11.18).
13. **The joint cutter cannot find word gaps on a compressed fist** (1.11.22) —
    HM-DEC-115 arriving a second time, **still unruled and the next decode
    question.**
14. **The constrained margin is bounded and still does not separate** (1.11.22).
15. **Four fixtures are absent and five acceptance lines were unmeasurable**
    (1.11.22).
16. **There is a Send button that does not send** (1.11.23), unruled.
17. **A mutable static in the decode path cannot be measured under xUnit**
    (1.11.22).
18. **HM-DEC-086 is superseded for the three workspaces**, above — recorded
    rather than worked around.
19. **The widgets are abandoned**, above — task 3 records what they were.
20. **`013347` returns a likelihood ratio of 17.2 million**, with `001520`'s
    quadrillions. Parked, raised once.

Closed by this unit if it lands: **the terminal's duplicate**; **the
neighborhood's half-measure** — there is no tray to be in; **per-mode widget
placement** — void.

Still open: **three fixtures at accepted cost**; **the reference and port
integrator difference**; **an unmeasured pitch costs `N4L`**; **the six-hertz
window disagreement**; **the short-character bias**; **the Avalonia geometry
offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.24**; **the whole-file second
pass**; **the squelch has no axis**; **the three morning captures of
2026-08-26**.

**If you finish every task, stop and report. Do not start the next unit.**
