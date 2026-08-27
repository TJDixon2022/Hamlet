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

# Work instruction 030 — a green suite over a dead screen

**ISSUED: 2026-08-27. A fresh order, not an amendment.**

**Five tasks; task 5 is the drop. No decoder file is touched, and the engine's
failing set being byte-identical at the end is the proof.**

## Why this unit exists

**The unit's number: two units, two faults, two green suites.**

**Unit 1.11.25 asserted the CW workspace was the same object after a tab change.
It was. The screen was blank.** The fault lived in the tab strip's binding —
`ConverterParameter={Binding}`, which Avalonia never resolves — and the test
could not reach it, because **it set `OperatingMode` on the view model instead of
pressing the tab.** A test that drives the view model cannot see a broken
control.

**Unit 1.11.24 shipped the Receive panel with no background at all**, for two
units, because `HmPanelBrush` was never defined in `App.axaml`. **Avalonia leaves
an unfound `StaticResource` as no brush rather than failing** — no build error,
no runtime error. `BindingHealthTests` catches an unresolved *binding* and not an
unresolved *resource*.

**Both suites were green. Both faults reached the operator's screen.** He found
each of them by looking at it.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and
report any mismatch, including where the work succeeded anyway.

**Tim has built the Send panel himself since unit 1.11.26** — the transmit
button, Send and Clear beside the title, Clear coloured as an action, and the
macro explanations. **Read what is there before touching anything near it, and do
not modify it.** If a task below appears to require a change inside the Send
panel, **stop and report** rather than editing his work.

**Expected state: 28 failing of 1841 in the engine as the stable set; the app
suite at 500 or whatever his own work has made it.** Seven timing intermittents
exist. Do not chase any; diff which tests moved and never trust a total.

**`AppSettings.UseJointDecoder` and `AppSettings.ShowKeyingSweep` both ship false
and stay false.**

**`CLAUDE_CODE.md` is at version 1.6.** Read its own section count.

**`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150, nor Tim's
rulings of 2026-08-25/26/27.**

## Rulings in force

**Tim's ruling, 2026-08-27: a view-level test acts through the control.** It
presses the button; it does not set the property the button would have set.
**A test that drives the view model cannot see a broken control**, and two units
running proved it by passing over faults he then found by looking at the screen.
This is the same shape of rule as unit 1.11.13's *assert the geometry that causes
the fault*, which has held since.

**The rule is enforced by a test, not by prose.** A rule that lives only in a
document is one a session reads at minute zero and has forgotten by task six.

**Untouched:** everything Tim built in the Send panel; the band plan, the
neighborhood map, the radio panel, the divider; the workspace boundary and the
tab strip as unit 1.11.26 left them; every decoder behaviour.

**Rejected already, do not revisit:** bringing back the canvas, the tray, the
preset bar or the layout namer; placeholder text in Digital or Voice; asserting a
control's own `IsVisible` where effective visibility is what the operator sees.

## Status cadence

Per §4.5: after each task, before the next, update `PROJECT_STATUS.md` —
`STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is
moving. Same every ten minutes while a task runs.

## The tasks

### Task 1 — find every view test that drives the view model

Inventory the app suite. For each test that exercises view behaviour, report
**whether it acts through a control or sets a property directly**, with file and
line, and group them: those that must change under the ruling, those that are
genuinely about the view model and are not view tests at all, and any that cannot
be driven through a control at all.

**Report the third group carefully.** A view behaviour that no control can reach
is either dead behaviour or a missing control, and which one it is matters.

Build and run; record the baseline by diffing which tests fail.

### Task 2 — make the tests act through the controls

Convert the first group. Each test presses, clicks or types where the operator
would, and asserts what the operator sees — effective visibility and render
bounds, never a control's own property.

**Report any test that changes from passing to failing under conversion.** That
is not a regression; **it is a fault the old test was covering**, and it is the
most valuable thing this unit can produce. Fix it if the fix is contained; if it
is not, **report the fault with file and line and leave it**, because a named
fault beats a rushed change.

### Task 3 — the rule enforces itself

A test that fails when a view test sets a view-model property where a control
exists to do it. **Report exactly what it can and cannot detect** — a rule
enforced by a heuristic is worth what the heuristic is worth, and the next
session needs to know which.

If it cannot be enforced by a test at all, **say so plainly and say why**, and
propose where the rule should live instead. **Do not ship a check that looks like
enforcement and is not.**

### Task 4 — resources resolve, or the suite fails

A test that walks every `StaticResource` and `DynamicResource` key referenced in
the application's XAML and asserts each one resolves.

`HmPanelBrush` was missing for two units, in a suite of five hundred tests that
includes a binding-health test, and nothing said so. **Report how many keys are
referenced, how many resolve, and name any that do not** — there may be others
sitting silently.

### Task 5 — the recent-places row's home *(the drop candidate)*

Unit 1.11.26 removed it from between the header and the tabs and reported it has
no home: not CW, not Digital, not Voice. **The control is unreferenced in the
tree and still tested**, with `ABANDONED_WIDGETS.md` describing what it did.

**Report what it would take to put it in the header** — beside the band plan, the
neighborhood and the radio — and what that would cost in space at the default
width. **Report only; place nothing.** Where it goes is Tim's.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

Every decoder question: admission, the six axis families, the gate, the squelch,
the joint decoder, the constrained margin, the tracker, the meter, the integrator
width, the whole-file second pass, the reference and port difference, the
short-character bias, `001520`'s quadrillions, `013347`'s 17.2 million. Also:
**the Send panel Tim built**; the engine code behind the abandoned widgets; the
phrasebook's arrival and the absent-widget news; HM-DEC-086's supersession
record; `CHANGELOG.md`; the seven intermittents; the Avalonia geometry offset;
HM-OPEN-057; HM-OPEN-059.

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not touch any decoder file.** The byte-identical failing set is the claim.
- **Do not modify the Send panel.** Tim built it. If a task seems to require it,
  stop and report.
- **Do not delete a test to make it comply.** A test converted is a test kept.
- **Do not ship a check that looks like enforcement and is not.**
- **Do not place the recent-places row anywhere.** Task 5 reports.

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — read the file's own section count — to
`output.md` at the repository root, overwritten and printed.

**Section 3 leads with the count: how many view tests drove the view model, how
many were converted, and how many faults the conversion uncovered** — with each
fault named. **Section 2 says whether anything the operator sees changed**, which
for a unit about tests should be nothing except any fault the conversion found.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Twenty-one inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's rulings of 2026-08-25/26/27, including the one this unit acts under.**
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
16. **HM-DEC-086's supersession needs a `DECISIONS.md` record** (1.11.25).
17. **The phrasebook's arrival and the absent-widget news are gone** (1.11.25).
18. **Engine code behind the abandoned widgets is unreachable from the screen**
    (1.11.25).
19. **Where the recent-places row belongs** (1.11.26) — task 5 reports the cost.
20. **An unresolved `StaticResource` fails silently** (1.11.26) — task 4 closes
    the harness gap.
21. **`013347` returns a likelihood ratio of 17.2 million**, with `001520`'s
    quadrillions. Parked, raised once.

Still open: **three fixtures at accepted cost**; **the reference and port
integrator difference**; **an unmeasured pitch costs `N4L`**; **the six-hertz
window disagreement**; **the short-character bias**; **the Avalonia geometry
offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.26**; **the whole-file second
pass**; **the squelch has no axis**; **the three morning captures of
2026-08-26**.

**If you finish every task, stop and report. Do not start the next unit.**
