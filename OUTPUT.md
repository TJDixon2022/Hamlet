# Work instruction 030 — a green suite over a dead screen

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. Branch `main` throughout, five commits, all
pushed, none refused. Version 1.11.26 to 1.11.27 per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

**All five tasks ran, including the drop. Nothing was left.**

**No decoder file was touched, and both proofs are in.** `git diff` over this
unit's commits against `src/Hamlet.RadioEngine/` reports **zero files**, and the
engine suite is **28 failing of 1841, byte-identical to the stable set**.

### One mismatch, and it is the first thing to say

**Tim's Send panel work is not in this tree.** The order says he has built it
himself since unit 1.11.26 — the transmit button, Send and Clear beside the
title, Clear coloured as an action, the macro explanations — and to read it
before touching anything near it.

**What is in `MainWindow.axaml` is exactly what unit 1.11.24 shipped**: the title
`Send`, a text box, four buttons reading CQ, RST, 73 and Clear, and the line
about nothing leaving the radio. No transmit button, no Send button, no macro
explanations. The last commit before this session is my own report from 1.11.26.

**Nothing near it was modified**, so the prohibition is honoured either way — but
if his work was meant to be in the zip, it did not arrive.

## 2. What the operator sees

**Nothing changed on the screen**, which for a unit about tests is the right
answer. No app source file was edited at all: the five commits are one governance
file, three test files added and two converted, and a version bump.

**The conversion uncovered no fault.** The order says a test that goes from
passing to failing under conversion is the most valuable thing this unit could
produce, and none did. **The fault the conversion would have caught was fixed
last unit** — unit 1.11.26 repaired the tab strip's binding — so the value here
is prospective rather than immediate, and that is stated rather than dressed up.

**What did change is what the suite can see.** Two guards that did not exist:
view tests must act through controls, and every resource key must resolve.

## 3. The count

**Seven writes across two files drove the view model where a control existed.
All seven converted. No test went red under conversion.**

| file | writes | what they bypassed |
|---|---|---|
| `TheTabsAndTheWorkspacesTests` | 4 | the CW / Digital / Voice tab strip |
| `TheSendPanelComposesAndDoesNotKeyTests` | 2 mode + 4 commands | the tab strip, and the CQ / RST / 73 / Clear buttons |

**Group two — genuinely about the view model, not view tests.** Every remaining
`model.` reference in the view tests is a read or an assertion —
`Assert.Equal(model.OperatingMode, checkedTabs[0])` and the like. Reading state to
check it against the screen is the point; only writing it bypasses the control.

**Group three — behaviour no control can reach, and it is one thing.**
`TheFollowedSentenceReachesTheScreenTests` sets `model.ListeningAfresh`. There is
no control for it and there should not be: it is set from
`_decoder.ListeningAfresh`, which is **state arriving from the radio**. A test
must arrange it, and arranging state the app observes is not bypassing anything.
**It is neither dead behaviour nor a missing control.**

**Two improvements the conversion forced, both worth naming:**

- **The Send tests now read the text box the operator reads**, not the property
  behind it. A property that is right behind a box not bound to it is no use to
  him.
- **The button helper searches inside the Send panel**, not the whole window.
  `Clear` is not a unique word on this screen, and a helper that takes whichever
  button visual order reaches first is a test that works by luck. It passed
  before I scoped it — by luck.

### Task 3 — the guard, and what it is worth

**It catches** a test that builds a `MainWindow` and then writes one of four
properties a control owns: `OperatingMode`, `SendText`, `IsBestChance`,
`IsWhatsNew`. That is the exact shape of the fault that got through twice.
**12 view test files scanned, no offences.**

**It does not catch** a property nobody has added to the list, an assignment
split over two lines, a command invoked through `SomeCommand.Execute` rather than
by writing a property, or a control driven by a method call.

**It cannot catch the general case at all.** Deciding whether a control exists
for a given property is a question about the XAML, and answering it properly
means resolving bindings — which is what the application does at run time and
what a text search cannot do. **So it is a named-property guard and not a proof,
and the class doc says exactly that.**

**A second test proves it can go red**, because a guard that cannot fail enforces
nothing.

**And it skips one file, its own** — which it discovered by reporting its own
self-test data as a breach on the first run. A check that fails on its own
fixtures is a check nobody keeps.

### Task 4 — the resources

**35 resource keys referenced, 35 resolve. None missing.**

So `HmPanelBrush` was the only one, and nothing else was sitting silently. The
check reads the application's markup, collects every `{StaticResource}` and
`{DynamicResource}` key, and asks **the real window** — not
`Application.Current` — because a key can live in a control's own resources or a
merged dictionary, and the window is what the operator looks at. A second test
proves a made-up key does not resolve, so the search is not finding something for
everything.

**It does not cover** a key built in code at run time, or one named only inside a
control theme in a library this application does not own.

### Task 5 — what the recent-places row would cost

Measured at both widths, report only. Nothing was placed.

| | 1400 px | 1200 px |
|---|---|---|
| header, bands to tabs | **295 px tall** | **327 px tall** |
| workspace below the tabs | 518 px | 486 px |
| neighborhood | x=31, w=778 | x=31, w=578 |
| radio | x=851, w=520 | x=651, w=520 |
| gap between them | **42 px** | **42 px** |

**What it would take.** The row is a `recent` label, a combo box and a `forget
this place` button — one line, about 34 px tall with its margins. There are three
places it could go and each costs something:

- **A fourth row in the header**, under the privilege line. Costs 34 px of
  workspace at every width, taking the CW workspace to 484 and 452. **Cheapest to
  build and it is the one that makes the header taller**, which is what
  HM-DEC-141 spent a unit shrinking.
- **In the 42-pixel gap between the neighborhood and the radio.** Free
  vertically, and 42 px is nowhere near enough for a combo box — it would force
  the neighborhood narrower, and that panel is untouchable.
- **Inside the radio panel**, which already carries the frequency, the mode, the
  filter and the age note, and has 520 px of width at both sizes. **Costs no
  header height at 1400** because the radio column is shorter than the
  neighborhood beside it; at 1200 the two are closer and it would add about 20 px.

**The third is the only one that is nearly free, and it is also the least
obviously right** — recent places are about where you have been, and the radio
panel is about where you are. **That is the judgement, and it is Tim's.**

### The suites

| | baseline | end |
|---|---|---|
| engine | 28 of 1841, stable set | **28 of 1841, byte-identical** |
| app | 503 of 503 | **507 of 507** |

Four tests added, two files converted, none deleted. The baseline was **503, not
the 500 the order expected** — unit 1.11.26 added three header tests after its
last full run, so 500 plus three. Nothing else moved.

### Where the instruction and the tree disagree

- **Tim's Send panel work is absent**, above.
- **The app baseline was 503, not 500**, above.
- **`CLAUDE_CODE.md` is at 1.6**, as stated. Confirmed.
- **`AppSettings.UseJointDecoder` and `ShowKeyingSweep` both ship false**,
  untouched.
- **`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150**, nor
  Tim's rulings of 2026-08-25/26/27.

## 4. What's blocking us

**The rule is enforced by a heuristic, and a heuristic is worth exactly what it
can see.**

Ruling asked for:

> **The view-test rule is guarded by a named-property list and cannot be more
> than that.** Whether a control exists for a property is a question about the
> XAML that only a running application can answer, so the guard checks four
> properties by name and fails loudly when one is written. **Adding a property to
> the list when a control gains one is now part of adding the control**, and
> nothing enforces that step.

The guard is in the tree and its own doc states its edges. **What has no
enforcement is the list staying current.** A control added next month whose
property is not on the list is invisible to it, and the failure mode is silence —
the same silence that let two faults through.

*Not proposed, because it needs a ruling:* whether the list should instead be
derived, by parsing the markup for two-way bindings on view-model properties and
treating every such property as control-owned. It would be self-maintaining and
it would be a second XAML parser in the test suite.

---

**Tim's Send panel work is not in the tree, and this unit could not read it.**

The order describes a transmit button, Send and Clear beside the title, Clear
coloured as an action, and macro explanations. **None of that is in
`MainWindow.axaml`.** The panel is exactly as unit 1.11.24 shipped it.

Nothing near it was touched, so no harm was done either way — **but the next unit
should not assume that work is present**, and if it was meant to be in this zip
it did not arrive.

---

**One test passed by luck until this unit, and nothing would have caught it.**

`TheButtonsComposeWhatWouldGoOut` found its buttons by searching the whole window
for a `Content` of `"Clear"` and taking the first. It got the right one because
of visual order. **It is scoped to the Send panel now**, but the class of fault —
a test that resolves an ambiguous control by accident — has no guard, and the
view-test rule does not address it.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Twenty-three inbound.
The oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's rulings of 2026-08-25/26/27.**
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
13. **The joint cutter cannot find word gaps on a compressed fist** (1.11.22).
14. **The constrained margin is bounded and still does not separate** (1.11.22).
15. **Four fixtures are absent and five acceptance lines were unmeasurable**
    (1.11.22).
16. **There is a Send button that does not send** (1.11.23) — parked, unruled.
17. **HM-DEC-086's supersession needs a record** (1.11.25).
18. **The phrasebook's arrival and the absent-widget news are gone** (1.11.25).
19. **Engine code behind the abandoned widgets is unreachable** (1.11.25).
20. **The recent-places row has no home** (1.11.26) — **three options costed
    above**; the judgement is Tim's.
21. **The owned-property list has no enforcement of staying current**, above.
22. **Tim's Send panel work is not in the tree**, above.
23. **A test resolved an ambiguous control by accident and nothing guards that
    class**, above.

New this unit: **the guard's list needs maintaining and nothing enforces it**,
above; **the missing Send panel work**, above; **the ambiguous-control class**,
above.

Closed this unit: **view tests act through controls** — seven writes converted
and the rule enforced by a test. **Resources resolve** — 35 of 35, and
`HmPanelBrush` was the only one that ever did not. **The recent-places row's
options**, costed at both widths.

Still open: **the lock's mixed help**; **three fixtures at accepted cost**; **the
reference and port integrator difference**; **an unmeasured pitch costs `N4L`**;
**the six-hertz window disagreement**; **the short-character bias**; **the
Avalonia geometry offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.27**; **the
whole-file second pass**; **the squelch has no axis**; **the three morning
captures of 2026-08-26**; **seven timing intermittents, none of which fired
today**.
