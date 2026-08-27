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

# Work instruction 029 — the tab owns the canvas

**ISSUED: 2026-08-27. A fresh order, not an amendment.**

**Six tasks; task 6 is the drop. No decoder file is touched, and the engine's
failing set being byte-identical at the end is the proof.**

## Why this unit exists

**The unit's number: one click, everything gone.**

Tim photographed unit 1.11.25's screen. **Clicking Digital and returning to CW
leaves the workspace blank** — no Send, no Receive, nothing. The unit asserted
that CW's workspace is the same object on return and that assertion passed, so
**the objects survive and stop being shown**. The effective-visibility fix that
unit made for Send reaches the panels and not the container: whatever hides the
CW workspace on a tab change is not undone on the way back.

**And on a fresh start, where the panels do appear, nothing joins them to the
tab.** Send and Receive float below the tab strip with no boundary of any kind.
Tim: *"It doesn't look like the tab owns the workspace. We need a containing
boundary to show what's happening… everything down from the tabs is the working
canvas. Make that obvious."*

**Two smaller faults are in the same photograph.** The block
`7.030 MHz · yours to use / 97.305(a)` renders **twice** — once inside the
neighborhood map where it belongs, and again as a loose card beneath it. And the
`recent · places you have been · forget this place` row sits between the header
and the tabs.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and
report any mismatch, including where the work succeeded anyway.

**Expected state: 28 failing of 1841 in the engine as the stable set; 493 of 493
in the app. Seven timing intermittents exist.** Do not chase any; diff which
tests moved and never trust a total.

**`AppSettings.UseJointDecoder` and `AppSettings.ShowKeyingSweep` both ship false
and stay false.**

**Do not verify by headless hit-testing.** Unit 1.11.13's rule stands: assert the
geometry that causes the fault. **And unit 1.11.25's lesson goes with it: assert
what the operator sees, not a control's own property** — a panel's `IsVisible`
stays true inside a hidden container, which is precisely how a blank workspace
passed its test.

**`CLAUDE_CODE.md` is at version 1.6.** Read its own section count.

**`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150, nor Tim's
rulings of 2026-08-25/26/27.**

## Rulings in force

**Tim's ruling, 2026-08-27, in his words:** *"Everything below the CW Digital and
Voice is the workspace canvas. That space is bounded by the controlling tab. It
needs to be obvious to the user."*

- **The tab strip and the workspace below it are one bounded region.** The tabs
  sit on the top edge of that boundary and the selected tab merges into it, so
  the tab is visibly the handle of the space it controls.
- **The boundary runs from the tabs to the bottom of the working area** and is
  the same region whichever tab is selected.

**Tim's ruling, same date: the duplicate card goes.** The loose
`7.030 MHz · yours to use` card beneath the neighborhood map is removed. **The
copy inside the map stays** — it belongs there.

**Tim's ruling, same date: the `recent · places you have been · forget this
place` row is removed from between the header and the tabs.** It is not deleted
from the application; **if it has no other home, report that and leave the
control in the tree unreferenced rather than destroying it** — this ruling is
about where it sits, not about whether the capability exists.

**Untouched:** the band plan, the neighborhood map itself, the radio panel, the
divider; HM-DEC-141's wavelength proportions; every decoder behaviour; the pitch
controls staying off; **CW's contents — Send left, Receive right, neither a
widget** — settled by unit 1.11.25.

**Rejected already, do not revisit:** bringing back the canvas, the tray, the
preset bar or the layout namer; putting placeholder text in Digital or Voice;
wiring Send to the transmitter (§0.2, HM-DEC-098).

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is
moving. Same every ten minutes while a task runs.

## The tasks

### Task 1 — find why returning to CW shows nothing

**Diagnose before fixing.** Report, with file and line, what hides the CW
workspace on a tab change and what should restore it. Say whether it is the
container's visibility, a binding that does not re-evaluate, a template
recreated without its content, or something else.

**Then state why unit 1.11.25's test passed.** That test asserted the workspace
is the same object on return, and it is — so name the property that is true
while the operator sees nothing. **A test that passes over a blank screen is the
finding here**, and the next task's assertion is written against it.

Build and run; record the baseline by diffing which tests fail.

### Task 2 — returning to CW shows CW

Fix what task 1 found.

**Assert what the operator sees, at the application's default width:** starting
on CW, switching to Digital, switching to Voice, and returning to CW, **Send and
Receive are effectively visible and have non-zero render bounds** at the end.
Repeat the round trip twice in the same test — a fault that only appears on the
second circuit is the kind this unit exists to catch.

**Digital and Voice remain empty** on every visit.

### Task 3 — the tab owns the canvas

Implement the boundary per the ruling: **the tab strip on the top edge of a
bordered region that extends down over the whole working area**, the selected
tab merging into it.

**Assert from render bounds:** the boundary's top edge meets the tab strip; its
left and right edges enclose the workspace; the selected tab's bottom edge and
the boundary's top edge coincide within a pixel; **the boundary is present and
the same region on all three tabs.**

**If merging the selected tab into the border cannot be done cleanly, report
exactly what fails and ship the closest thing that does not look broken** — a
tab that nearly meets the edge is worse than one that plainly sits on it.

### Task 4 — the duplicate card

Remove the loose `7.030 MHz · yours to use` card beneath the neighborhood map.
**The copy inside the map stays.**

**Assert that the string renders once**, not twice, in the whole window.

### Task 5 — the recent-places row

Remove the `recent · places you have been · forget this place` row from between
the header and the tabs.

**If the control has another home in the application, say where.** If it does
not, **leave it in the tree unreferenced and say so** — this is a placement
ruling, not a deletion.

**Assert that nothing renders between the header's divider and the tab strip.**

### Task 6 — what the removed row was for *(the drop candidate)*

One paragraph in `ABANDONED_WIDGETS.md`, beside unit 1.11.25's fifteen: what the
recent-places control did, so a decision to rebuild it starts from a description.
**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

Every decoder question: admission, the six axis families, the gate, the squelch,
the joint decoder, the constrained margin, the tracker, the meter, the integrator
width, the whole-file second pass, the reference and port difference, the
short-character bias, `001520`'s quadrillions, `013347`'s 17.2 million. Also: the
Send button that does not send; the engine code behind the abandoned widgets;
the phrasebook's arrival and the absent-widget news; `CHANGELOG.md`; the seven
intermittents; the Avalonia geometry offset; HM-OPEN-057; HM-OPEN-059; **the band
plan, the neighborhood map, the radio panel and the divider.**

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not touch any decoder file.** The byte-identical failing set is the claim.
- **Do not change CW's contents.** Send left, Receive right, settled.
- **Do not delete the recent-places control.** It moves out of that slot.
- **Do not remove the copy of the frequency block inside the neighborhood map.**
- **Do not assert a control's own visibility property** where effective
  visibility is what the operator sees.
- **Do not verify by headless hit-testing.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 3 leads with task 1's answer — what hid the workspace and why the
previous test passed over a blank screen — and then the round-trip assertion.**
**Section 2 says what the operator sees on each tab and what the boundary looks
like.**

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Twenty-one inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's rulings of 2026-08-25/26/27, including the three this unit acts under.**
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
    **the next decode question, still unruled.**
14. **The constrained margin is bounded and still does not separate** (1.11.22).
15. **Four fixtures are absent and five acceptance lines were unmeasurable**
    (1.11.22).
16. **There is a Send button that does not send** (1.11.23), unruled.
17. **HM-DEC-086's supersession needs a `DECISIONS.md` record** (1.11.25).
18. **The phrasebook's arrival and the absent-widget news are gone** (1.11.25).
19. **Engine code behind the abandoned widgets is unreachable from the screen**
    (1.11.25) — `ScanViewModel`, `HeardWatch`, `AutoCallViewModel` and their
    tests all still compile and run.
20. **Where the recent-places control belongs**, if anywhere — task 5 reports.
21. **`013347` returns a likelihood ratio of 17.2 million**, with `001520`'s
    quadrillions. Parked, raised once.

Still open: **three fixtures at accepted cost**; **the reference and port
integrator difference**; **an unmeasured pitch costs `N4L`**; **the six-hertz
window disagreement**; **the short-character bias**; **the Avalonia geometry
offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.25**; **the whole-file second
pass**; **the squelch has no axis**; **the three morning captures of
2026-08-26**.

**If you finish every task, stop and report. Do not start the next unit.**
