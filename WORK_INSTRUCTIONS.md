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

# Work instruction 026 — the operating screen

**ISSUED: 2026-08-27. A fresh order, not an amendment.**

**Four tasks; task 4 is the drop. This unit is entirely about what the operator
sees. No decoder file is touched, and the engine's failing set being
byte-identical at the end is the proof.**

## Why this unit exists

**The unit's number: three panels the operator needs at once, on three
different screens.**

Tim reviewed the layout on 2026-08-27 and ruled a new one. The reasoning he
gave, in his words: **the band plan "is the essential driver for a session"**;
the neighborhood map and the radio "are independent, so that when we go from CW
to digital to voice, that part stays the same"; and the widgets "apply to all
different modes… but you drag them onto the current panel."

**And one thing comes off the screen because it was a workaround dressed as a
feature.** The pitch controls — "Hold this pitch", and the pitch behaviour
attached to "I hear a station" — exist because acquisition does not work. They
put a decoder problem in the operator's hands and asked him to press a button
whose meaning was never explained to him. **His verdict: "It shouldn't involve
me randomly clicking on a button I don't understand."** They come off the CW
panel. Decoding is Hamlet's job.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and
report any mismatch, including where the work succeeded anyway.

**Expected state: 28 failing of 1841 in the engine as the stable set; 503 of 503
in the app. Seven timing intermittents exist.** Do not chase any of them; diff
which tests moved and never trust a total.

**`AppSettings.UseJointDecoder` ships false and stays false in this unit.**
`AppSettings.ShowKeyingSweep` ships false and stays false.

**Do not verify any of this by headless hit-testing.** Unit 1.11.9 asserted this
area green by hit test while an unexplained headless-versus-real geometry offset
of about thirteen pixels hid three faults the operator could see. **Unit
1.11.13's rule stands: assert the geometry that causes the fault — visual-tree
order, render bounds, clipping ancestors — never that a point reaches a
control.**

**`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150, nor
Tim's rulings of 2026-08-25/26/27.** **`CLAUDE_CODE.md` is at version 1.4.**

## Rulings in force

**Tim's layout ruling, 2026-08-27.** The operating screen, top to bottom:

1. **The band plan, full width, at the top.** Card widths keep the wavelength
   proportions of HM-DEC-141 — they are meaning, not size. The `best bet now`
   badge renders over the row rather than inside its layout flow, per unit
   1.11.13's fix, and takes no clicks.
2. **The neighborhood panel on the left and the radio panel on the far right**,
   on one row beneath the band plan. **The neighborhood panel is moved, not
   modified** — Tim: *"It looks exactly like it does now. We do not need to
   change the neighborhood."* The radio panel is the narrower of the two and
   shows what the rig reports: frequency, mode, filter, signal, preamp.
3. **A divider.** Everything above it is the same in every mode and does not
   redraw when the mode changes.
4. **The widget tray down the left, outside the tab region**, and **the tabs —
   CW, Digital, Voice — beginning at the left edge of the Receive panel.**
5. **Inside the CW tab: Receive on the left, Send on the right.** Receive is the
   wider. Both are permanent on that tab.

**Tim's ruling on the pitch controls, same date:** they come off the CW panel.
**"Hold this pitch" is removed from the panel entirely.** "I hear a station"
**keeps its capture behaviour and loses its pitch behaviour** — see task 3.

**Tim's ruling on the tray, same date:** the widgets are shared across modes and
are dragged onto whichever panel is showing. **The tray is condensed later; this
unit does not remove any widget.**

**HM-DEC-141 is untouched.** **HM-DEC-148's precedent for the advisory area is
untouched.** **No decoder behaviour changes.**

**Rejected already, do not revisit:** moving the neighborhood panel's contents;
shrinking the band cards to make room (make room around them); rebuilding the
keying meter; re-enabling the sweep panel.

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is
moving. Same every ten minutes while a task runs.

## The tasks

### Task 1 — the persistent header

Band plan at the top, full width. Neighborhood left, radio right, on the row
below it. Divider beneath.

**The neighborhood panel is relocated as a whole control. Its contents,
rendering and behaviour are not touched** — if relocating it requires any change
to the control itself, **report exactly what and why** rather than adjusting it
quietly.

**Assert, at the application's default width and at a narrower one:** the
visual-tree order is band plan, then the neighborhood-and-radio row, then the
divider; every band card's label renders inside its own card, `10 m` included;
the badge's bounds sit inside every clipping ancestor; nothing occludes anything
else. **Name the clipping ancestor of the badge in the report.**

Build and run; record the baseline by diffing which tests fail.

### Task 2 — the tabs and the tray

**The tray sits outside the tab region, down the left**, holding the existing
widgets. **The tab strip begins at the left edge of the Receive panel** — assert
that alignment from render bounds, not by eye.

Switching tabs must redraw **only** the area below the tab strip. **Assert that
the band plan, the neighborhood panel and the radio panel are not re-created
when the mode changes** — that is the whole point of the divider, and a test is
the only thing that will keep it true.

**A widget dropped on a panel stays with that mode.** Drop it on CW, switch to
Digital and back: it is still on CW and was never on Digital. **If the existing
widget machinery cannot carry per-mode placement, report that and implement one
shared arrangement**, saying plainly which was built.

### Task 3 — the pitch controls come off

**Remove "Hold this pitch" from the panel.**

**"I hear a station" keeps banking the last half minute and adding to tonight's
list. It stops setting the decode pitch.** The engine capability added in unit
1.11.21 stays in the code and stays reachable by tests; **only the panel's use
of it goes.** Nothing about admission, the tracker or the decoder changes.

**A capture taken after this task must not report an operator-asserted pitch**,
because the operator can no longer assert one from the panel. Assert it.

### Task 4 — the send panel *(the drop candidate)*

Receive is the existing terminal, relocated. **Send is new: a text line and
buttons for CQ, RST, 73 and Send.**

**It composes and displays only. It does not key the radio.** Transmit is
outside this unit and outside every unit so far; **if wiring it appears
possible, do not**, and put the ask in section 4.

**Dropped whole if time runs out, and the report says so** — the CW tab then
shows Receive alone, which is what it shows today.

## Parked — do not touch, do not raise

Every decoder question: admission, the six axis families, the gate, the squelch,
the joint decoder, the constrained margin, the tracker, the meter's rebuild, the
integrator width, `001520`'s quadrillions, `013347`'s 17.2 million, the whole-
file second pass, the reference and port difference, the short-character bias.
Also: `CHANGELOG.md`; the seven intermittents; HM-OPEN-057; HM-OPEN-059; the
Avalonia geometry offset itself; **condensing the tray**; **the Rig tab** — the
radio panel is permanent in the header and rig settings stay a tray widget.

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not touch any decoder file.** The byte-identical failing set is the
  claim.
- **Do not modify the neighborhood panel.** Move it; report anything that
  forces a change.
- **Do not shrink the band cards.** HM-DEC-141's proportions are meaning.
- **Do not verify by headless hit-testing.**
- **Do not wire Send to the transmitter.**
- **Do not remove a widget from the tray.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 2 leads with what the screen looks like top to bottom**, because that
is the deliverable. **Section 3 leads with the assertions: the visual-tree
order, the band labels inside their cards, the badge's clipping ancestor, the
tab strip's alignment to Receive, and that the header is not re-created on a
mode change.**

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Nineteen inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's rulings of 2026-08-25/26/27, including the three this unit acts
   under.**
5. **The tone tracker** — six axis families measured; the question is a design
   one, and **the operator's assertion is no longer available as the way round
   it from the panel.**
6. **The integrator width** — settled at 45 Hz, with the sharp-peak caveat.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named.**
10. **The keying meter** — behind its setting, off; the rebuild is its own unit.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
12. **The gate opens on everything, including two empty recordings.**
13. **The joint cutter cannot find word gaps on a compressed fist** —
    HM-DEC-115 arriving a second time. **Whether the cutter should fit its own
    three gap classes is Tim's and is unruled.**
14. **The constrained margin is bounded and still does not separate.**
15. **`011447` and `011514` are absent**, and `021410` does not contain the text
    three acceptance lines were written against.
16. **A mutable static in the decode path cannot be measured under xUnit.**
17. **An asserted pitch does not relax the decoder's own gate** — `014113` is
    pointed within seven hertz of its station and still emits nothing.
18. **Whether Send should ever key the transmitter** — task 4 composes only.
19. **Whether a dropped widget's placement is per-mode or shared** — task 2
    builds per-mode if the machinery allows and says which was built.

Still open: **three fixtures at accepted cost**; **the reference and port
integrator difference**; **an unmeasured pitch costs `N4L`**; **the six-hertz
window disagreement**; **the short-character bias**; **the Avalonia geometry
offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.22**; **the whole-file second
pass**; **the squelch has no axis**; **the three morning captures of
2026-08-26**.

**If you finish every task, stop and report. Do not start the next unit.**
