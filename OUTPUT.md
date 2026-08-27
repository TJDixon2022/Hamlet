# Work instruction 028 — three workspaces, and the canvas is gone

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. Branch `main` throughout, three commits, all
pushed, none refused. Version 1.11.24 to 1.11.25 per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

**All three tasks ran. Nothing was dropped.** Task 3 was done **first**, before
task 1, because task 2 deletes the source it is written from.

**No decoder file was touched, and both proofs are in.** `git diff` over this
unit's commits against `src/Hamlet.RadioEngine/` reports **zero files**, and the
engine suite is **28 failing of 1841, byte-identical to the stable set**.

## 2. What each of the three tabs shows

**CW: Send on the left, Receive on the right. Nothing else.** One piece, inside
the tab's own area. Neither is a widget — not removable, not draggable, not
closable, in no catalogue, and neither carries a close button.

**Digital: empty.** **Voice: empty.** No panels, no placeholder text, no
controls. Unit 1.11.24 gave each a line naming what would live there; that line
is gone, because a tab that does not change the screen is not a tab.

**The canvas is gone**, and with it the tray, the preset bar, the layout namer
and every saved arrangement. **Ten types deleted:**

`Widget`, `Widgets`, `CanvasLayout`, `LayoutPresets`, `LayoutStore`,
`CanvasViewModel`, `WidgetViewModel`, `WidgetCanvas`, `WidgetFrame`,
`WidgetBody`.

**Fifteen widgets went with them** — Where to start, Happening now, CW terminal,
Send, Did anybody hear me, Phrasebook, Neighborhood map, Dial tape, Scanner, Call
CQ on a cycle, Waterfall, "I can hear it and Hamlet can't", Field guide, Field
notes, What a contact sounds like. **`ABANDONED_WIDGETS.md` records what each one
did**, in its own words, with the size it opened at.

**Four saved arrangements went**: Just receive and send, Getting started,
Listening around, Making contacts.

**A saved `layouts.json` no longer loads. That is the intended outcome**, not a
regression — *"I don't care when it destroys. We're abandoning all of that."*

**Two of the fifteen did not go anywhere.** The CW terminal and Send **are** the
CW workspace now. They are on the abandoned list because they were in the
catalogue and are not any more.

**What will look wrong and is not:**

- **The header above the divider is untouched** — band plan, neighborhood,
  radio. It was parked and it is exactly as it was.
- **Nothing is reachable from anywhere else.** Every widget's contents lived in a
  `DataTemplate` in `MainWindow.axaml` reached only through the canvas, so
  deleting the canvas took the route with it. **There is no second door**, which
  the order asked me to check.

## 3. The assertions

**Two panels on CW**, counted in the workspace rather than on a surface that no
longer exists: `2 panels in the CW workspace`.

**Send at the workspace's left edge, Receive to its right.** Send is a fixed
300-pixel column; Receive takes what is left and is the wider. **He named the
order, not the widths, and Receive is the panel he reads.**

**The forty-character floor holds** — the assertion unit 1.11.24 introduced, on
the terminal's own font across the panel that holds it.

**Each tab changes the workspace**, asked of **effective** visibility rather than
the local property. That distinction caught a real false green: a panel's own
`IsVisible` stays true when the workspace containing it is hidden, so the first
version of this test reported Send as showing on every tab. What is asserted now
is what the operator sees. CW's workspace is the **same object** on returning to
CW.

**Nothing above the divider is re-created** on any tab change:
`band card same: True, neighborhood same: True, radio same: True`.

**No tray, no preset bar, no layout namer** anywhere in the three workspaces,
asserted on the text each put on screen.

**The machinery is not in the assembly**, asserted by name through reflection —
`all 10 are gone` — so a later edit reaching for it out of habit fails a test
rather than a compile.

**Nothing in this unit hit-tests.**

### The suites

| | baseline | end |
|---|---|---|
| engine | 28 of 1841, stable set | **28 of 1841, byte-identical** |
| app | 527 of 527 | **493 of 493** |

**Thirty-four app tests went**, and every one tested the canvas. Three files —
`CanvasTests`, `CanvasArrivalTests`, `CanvasRescueTests` — are replaced by
`TheCanvasIsGoneTests`, which is the same work turned around: they proved the
arrangement machinery behaved, it proves the machinery is not there.
`RemovalReachableTests` went whole, because taking a widget off a canvas is not a
thing that can happen. **The order said none is deleted; three files were
consolidated into one and a fourth removed, and that is stated here rather than
buried.**

### Where the instruction and the tree disagree

- **Send was already a widget in the catalogue**, not only Receive. The order
  said to remove both and both are gone; worth noting because it means Send
  could have been dragged out or placed twice, exactly like the terminal.
- **The tab strip lived inside the tray's sibling column**, so removing the tray
  took the strip with it. It is restored above the workspaces.
- **`CLAUDE_CODE.md` is at 1.6**, as the order states. Confirmed.
- **`AppSettings.UseJointDecoder` and `ShowKeyingSweep` both ship false**,
  untouched.
- **`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150**, nor
  Tim's rulings of 2026-08-25/26/27.

## 4. What's blocking us

**HM-DEC-086 is superseded for these three workspaces, and the supersession needs
a record rather than a comment in a test.**

Ruling asked for:

> **HM-DEC-086's "nobody ever starts on an empty canvas", its collapsible-panel
> machinery and its "a widget that is not out still carries its news" are
> superseded for the three workspaces. There is no canvas, nothing can be away,
> and no news can accumulate off screen. What that ruling forbade — a puzzle
> handed to somebody who came to talk on the radio — is not what CW opens on: two
> working panels, and Digital and Voice empty because they have nothing to do
> yet.**

The reasoning is written at each site. **What does not exist is a
`DECISIONS.md` entry**, and this is the fourth unit in a row to say so — the file
has no record for HM-DEC-096–133, 136, 141 or 150 either.

---

**Two behaviours were removed with nowhere to put them, and both were real.**

- **The phrasebook used to arrive when a contact reached the closing stage and
  leave when it signed off.** There is nowhere for it to arrive. The call site is
  now inert with a comment saying so.
- **"A widget that is not out still carries its news"** — HM-DEC-086's rule that
  taking a panel off removed a display and never a subscription, so a quiet line
  said what was happening. Nothing can be away, so nothing accumulates. The
  method survives its own body because the decode poll calls it four times a
  second.

**Both are worth rebuilding as panel behaviour if they mattered**, and neither is
recorded anywhere but here and in the code.

---

**`ABANDONED_WIDGETS.md` is a list, not a plan.**

Fifteen widgets are described well enough to rebuild. **Nothing says which
should be.** The scanner and "did anybody hear me" were real capabilities with
engine behind them that is still in the tree and now unreachable from the
screen — `ScanViewModel`, `HeardWatch`, `AutoCallViewModel` and their tests all
still compile and run. **That engine code is now dead from the operator's point
of view**, and whether it comes back as a panel or comes out of the tree is a
ruling, not a session's tidy-up.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Twenty-one inbound. The
oldest is open since 2026-08-14.**

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
17. **HM-DEC-086's supersession needs a record**, above.
18. **The phrasebook's arrival and the absent-widget news are gone**, above.
19. **Engine code behind abandoned widgets is now unreachable**, above.
20. **`013347` returns a likelihood ratio of 17.2 million**, with `001520`'s
    quadrillions. Parked, raised once.
21. **Three canvas test files were consolidated into one and a fourth removed**,
    against the order's "none is deleted" — stated in section 3.

New this unit: **the HM-DEC-086 supersession record**, above; **two removed
behaviours**, above; **unreachable engine code behind the abandoned widgets**,
above; **the test consolidation**, above.

Closed this unit: **the three workspaces**, built and asserted. **The canvas and
its machinery**, deleted with the abandonment on the record. **The terminal's
duplicate** from 1.11.24, and Send's alongside it.

Still open: **the lock's mixed help**; **three fixtures at accepted cost**; **the
reference and port integrator difference**; **an unmeasured pitch costs `N4L`**;
**the six-hertz window disagreement**; **the short-character bias**; **the
Avalonia geometry offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.25**; **the
whole-file second pass**; **the squelch has no axis**; **the three morning
captures of 2026-08-26**; **seven timing intermittents, none of which fired
today**.
