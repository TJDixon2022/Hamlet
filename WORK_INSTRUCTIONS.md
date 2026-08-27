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

# Work instruction 027 — clear the CW tab

**ISSUED: 2026-08-27. A fresh order, not an amendment.**

**Three tasks; task 3 is the drop. No decoder file is touched, and the engine's
failing set being byte-identical at the end is the proof.**

## Why this unit exists

**The unit's number: one letter per line.**

Tim photographed the CW tab after unit 1.11.23. **Receive is rendering
`wh at the rad io is he ari ng` one or two letters to a line**, because it was
squeezed into a narrow column while the tab still carried the whole previous
canvas — the neighborhood map, the dial tape, the waterfall, the advice panel,
all stacked down the left of it. His verdict: **"this is a mess… It needs to be
clear and only show the send receive parts for now."**

**The fault is in the previous order, not the session that executed it.** Unit
1.11.23's instruction said what to add to the CW tab and never said what the tab
should contain, so the work landed on top of the existing arrangement instead of
replacing it. **This order states the contents.**

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and
report any mismatch, including where the work succeeded anyway.

**Two things this order assumes and cannot see. Report both before task 1, and
if either is wrong, say so and build to what is actually there rather than to
this description.**

1. **That unit 1.11.23's header exists above the tab strip** — the band plan
   full width, then the neighborhood on the left and the radio on the right,
   then the divider. The photograph is cropped and shows only "Listening around
   / Making contacts", a layout namer, and the tabs.
2. **That the neighborhood in the photograph is a second copy dragged onto the
   canvas**, the duplicate unit 1.11.23 reported in its section 4 — the same
   `widget.map` template appearing twice because it is in the header and still
   in the tray.

**Expected state: 28 failing of 1841 in the engine as the stable set; 520 of 520
in the app. Seven timing intermittents exist.** Do not chase any; diff which
tests moved and never trust a total.

**`AppSettings.UseJointDecoder` and `AppSettings.ShowKeyingSweep` both ship
false and stay false.**

**Do not verify by headless hit-testing.** Unit 1.11.13's rule stands: assert the
geometry that causes the fault — visual-tree order, render bounds, clipping
ancestors, reference identity — never that a point reaches a control.

**`CLAUDE_CODE.md` is at version 1.5** per unit 1.11.23's correction; read its own
section count. **`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141,
150, nor Tim's rulings of 2026-08-25/26/27.**

## Rulings in force

**Tim's ruling, 2026-08-27, in his words: "Remove all widgets for now. Leave them
on the far left side."**

- **The CW tab's operating area contains Receive and Send. Nothing else.**
- **Every widget currently on the CW canvas is removed from it** — the
  neighborhood map, the dial tape, the waterfall, the advice panel, and anything
  else there. **None is deleted.** Each remains available in the tray on the far
  left, to be dragged out when he wants it.
- **The neighborhood map comes out of the tray**, because it is a header panel
  now and a second copy on the canvas is the duplicate unit 1.11.23 flagged.
  **This is the one widget that leaves the catalogue.**

**Tim's ruling, same date: Receive gets the room.** It was unreadable at one
letter per line. **Receive is the wider of the two panels and its text wraps at
whole words.**

**HM-DEC-141's wavelength proportions, HM-DEC-148's advisory precedent, and every
decoder behaviour are untouched.** The pitch controls stay off, per unit
1.11.23.

**Rejected already, do not revisit:** deleting any widget other than the
neighborhood's tray entry; modifying the neighborhood panel itself; wiring Send
to the transmitter (§0.2, HM-DEC-098 — the interlocks have never been watched
firing into a dummy load); re-enabling the sweep panel or the joint decoder.

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is
moving. Same every ten minutes while a task runs.

## The tasks

### Task 1 — clear the canvas, keep the tray

Remove every widget from the CW tab's operating area. **Assert that the tray on
the far left still offers each one**, by name, and that dragging one out still
works.

**Take the neighborhood map out of the tray**, per the ruling — it is in the
header and must not be duplicable onto the canvas.

**Report what was on the CW canvas before this task**, by name, so the removal is
on the record and nothing vanishes unnoticed.

Build and run; record the baseline by diffing which tests fail.

### Task 2 — Receive and Send, sized to be read

The operating area holds exactly two panels: **Receive on the left, wider; Send
on the right, narrower.**

**Assert from render bounds, at the application's default width and at a
narrower one:**

- the operating area contains **two** panels and no others;
- **Receive is wider than Send**;
- **Receive's text wraps at word boundaries and no line of decoded text is
  narrower than forty characters** at the default width — the photographed
  failure was one or two characters to a line, and a width assertion is the only
  thing that will keep it from recurring;
- the tab strip still begins at Receive's left edge;
- the header above the divider is **not re-created** when the mode changes —
  reference identity, as unit 1.11.23 asserted it.

**If the header is not present**, per the check above, **report that first and
build the operating area anyway** — it is the part Tim can see is wrong.

### Task 3 — Digital and Voice *(the drop candidate)*

Those tabs are empty and look unfinished. **Give each a single line naming what
will live there and nothing else.** No controls, no placeholder panels.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

Every decoder question: admission, the six axis families, the gate, the squelch,
the joint decoder, the constrained margin, the tracker, the meter, the integrator
width, the whole-file second pass, the reference and port difference, the
short-character bias, `001520`'s quadrillions, `013347`'s 17.2 million. Also:
per-mode widget placement and the layout store's format; condensing the tray;
`CHANGELOG.md`; the seven intermittents; the Avalonia geometry offset;
HM-OPEN-057; HM-OPEN-059; **the Send button that does not send** — unruled and
left exactly as it is.

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not touch any decoder file.** The byte-identical failing set is the claim.
- **Do not delete a widget.** They leave the canvas and stay in the tray. The
  neighborhood's tray entry is the single exception, by ruling.
- **Do not modify the neighborhood panel.**
- **Do not put anything else in the CW operating area.** Two panels.
- **Do not wire Send to the transmitter.**
- **Do not verify by headless hit-testing.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 2 leads with what the CW tab now contains**, and names every widget
that left it and where it went. **Section 3 leads with the width assertions on
Receive** — panels in the operating area, Receive against Send, and the minimum
decoded line length — because one letter per line is the fault this unit exists
to fix.

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
   one, and the operator's assertion is no longer the way round it.
6. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named.**
10. **The keying meter** — its measurement found a station its verdict denied.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **The gate opens on everything, including two empty recordings.**
13. **The joint cutter cannot find word gaps on a compressed fist** — HM-DEC-115
    arriving a second time, and **still unruled.**
14. **The constrained margin is bounded and still does not separate.**
15. **Four fixtures are absent and five acceptance lines were unmeasurable.**
16. **Per-mode widget placement needs a stored-format ruling.**
17. **The Digital and Voice tabs are empty** — task 3 gives them a line.
18. **There is a Send button that does not send.**
19. **A mutable static in the decode path cannot be measured under xUnit.**
20. **`013347` returns a likelihood ratio of 17.2 million**, with `001520`'s
    quadrillions.

Closed by this unit if it lands: **the neighborhood's duplicate** — out of the
tray, one copy in the header.

Still open: **three fixtures at accepted cost**; **the reference and port
integrator difference**; **an unmeasured pitch costs `N4L`**; **the six-hertz
window disagreement**; **the short-character bias**; **the Avalonia geometry
offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.23**; **the whole-file second
pass**; **the squelch has no axis**; **the three morning captures of
2026-08-26**.

**If you finish every task, stop and report. Do not start the next unit.**
