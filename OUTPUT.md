# Work instruction 029 — the tab owns the canvas

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. Branch `main` throughout, six commits, all
pushed, none refused. Version 1.11.25 to 1.11.26 per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

**All six tasks ran, including the drop. Nothing was left.**

**No decoder file was touched, and both proofs are in.** `git diff` over this
unit's commits against `src/Hamlet.RadioEngine/` reports **zero files**, and the
engine suite is **28 failing of 1841, byte-identical to the stable set**.

## 2. What the operator sees

**CW shows Send on the left and Receive on the right, and it still does after
pressing every tab twice round.** That was the fault: one click on Digital and
back left the screen blank, permanently.

**The tab strip now sits on the top edge of a bordered region that runs down over
the whole working area.** The selected tab merges into it — same fill, same edge
colour, no border along the bottom where they meet, overlapping by a pixel so no
hairline shows through. The unselected tabs keep all four edges and read as
separate things you could press. **The region is the same rectangle on all three
tabs** — 16, 450, 1368 by 398 — so it reads as a space rather than as three
panels.

**Digital and Voice are empty**, on every visit, with the CW workspace off the
screen and nothing of their own in its place.

**The frequency block renders once.** `7.030 MHz · yours to use · 97.305(a)` was
appearing twice — inside the neighborhood map where it belongs, and again as a
loose card beneath it. The loose one is gone.

**The `recent · places you have been · forget this place` row is out of the strip
between the header and the tabs.** It is not deleted: `FavoritesViewModel`,
`RecentPlaces` and their commands are all still in the tree and still tested.
`ABANDONED_WIDGETS.md` records what it did.

**One thing changed that nobody asked for and it is worth knowing.** Receive has
been drawing with **no background at all** since unit 1.11.24, because
`HmPanelBrush` was never defined — see section 4. It has a white surface now,
which is what HM-DEC-012 says a panel body is.

## 3. Task 1's answer, and the round trip

**What hid the workspace, with the line:** `MainWindow.axaml`, the tab strip's
item template. Each tab's `IsChecked` was two-way bound through
`ModeTabConverter` with the tab's own name passed as
**`ConverterParameter={Binding}`** — and **a converter parameter cannot itself be
a binding in Avalonia.** It never resolved.

**Both directions were broken, and the second one is what the operator saw:**

- `Convert` compared the selected mode against an unresolved parameter and
  returned false for all three tabs. **Measured: a fresh window showed
  `CW=False, Digital=False, Voice=False`.**
- `ConvertBack` read the same parameter as null and wrote it to `OperatingMode`.
  **Measured: the first press of any tab set the mode to `""`**, after which
  `IsCwMode`, `IsDigitalMode` and `IsVoiceMode` were all false, every workspace
  was hidden, and no further press recovered it.

```
fresh                  mode="CW"      cw.Effective=True
after pressing Digital mode=""        cw.Effective=False
after pressing Voice   mode=""        cw.Effective=False
after pressing CW      mode=""        cw.Effective=False
```

**It is not the container's visibility, a template recreated without its content,
or a binding that fails to re-evaluate.** The binding evaluated correctly every
time, on a value that had been destroyed.

**Why unit 1.11.25's test passed, and there are two reasons.** It asserted the
workspace is the same object on return, **and it is** — the container survived
and stopped being shown, so object identity was true over a blank screen. But the
deeper reason is that **the test set `OperatingMode` on the view model directly
and never pressed a button**, so the fault, which lives entirely in the strip's
binding, could not be reached. **A test that drives the view model cannot see a
broken control.**

**The fix**: selection is state on the tab now. Each tab is a `ModeTabViewModel`
with its own `IsSelected`, bound straight through, and the view model keeps the
three in step from either end. The converter is deleted.

**The round trip, twice, pressing the buttons:**

| | mode | Send | Receive |
|---|---|---|---|
| fresh | CW | 300 × 177 | 1058 × 401 |
| back on CW, lap 1 | CW | 300 × 177 | 1058 × 401 |
| back on CW, lap 2 | CW | 300 × 177 | 1058 × 401 |

Effective visibility and non-zero render bounds, never a control's own
`IsVisible`. **Exactly one tab is checked at every point and it is the one
showing.**

### The boundary

| | |
|---|---|
| strip bottom / boundary top, 1400 px | **450 / 450** |
| strip bottom / boundary top, 1200 px | **478 / 478** |
| boundary, all three tabs | **16, 450, 1368 × 398 — same object** |
| selected tab | fill `White`, border `1,1,1,0` |
| unselected tabs | fill `#ffedeae1`, border `1,1,1,1` |

**Merging the selected tab worked cleanly** and needed no compromise.

### The two stray blocks

`"7.030 MHz · yours to use" renders 1 time(s)`. `no recent-places row on the
screen`. `divider bottom=374, strip top=386, gap=12 px` with **nothing between
them**.

### The suites

| | baseline | end |
|---|---|---|
| engine | 28 of 1841, stable set | **28 of 1841, byte-identical** |
| app | 493 of 493 | **500 of 500** |

Ten tests added. One existing test changed: `TheTabStripBeginsWhereTheOperatingAreaBegins`
was measuring the tabs against the CW workspace, and once the boundary gained
padding the panels sat thirteen pixels in while the tabs stayed on the edge they
own. **The tabs were right and the assertion was measuring the wrong thing**; it
compares against the boundary now.

### Where the instruction and the tree disagree

- **`CLAUDE_CODE.md` is at 1.6**, as stated. Confirmed.
- **`AppSettings.UseJointDecoder` and `ShowKeyingSweep` both ship false**,
  untouched.
- **`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150**, nor
  Tim's rulings of 2026-08-25/26/27.

## 4. What's blocking us

**A brush that was never defined has been silently missing for two units, and the
class of fault matters more than the instance.**

Ruling asked for:

> **Avalonia leaves an unfound `StaticResource` as no brush at all rather than
> failing, at build or at run time.** `HmPanelBrush` was written by unit 1.11.24
> for the Receive panel and by this unit for the workspace boundary, and it was
> never in `App.axaml` — so Receive drew with no background for two units and
> nothing said so, in a suite of 500 tests that includes a binding-health test.
> **`BindingHealthTests` catches an unresolved *binding* and not an unresolved
> *resource*.** Whether it should is the ruling.

It is defined now, as the white panel surface HM-DEC-012 describes, which until
today existed only as the literal `White` inside `CollapsiblePanel`'s theme.
**The fix is one line; the gap in the harness is not.**

---

**The recent-places row has no home, which is why it was left in the tree.**

The ruling said to report if it has none, and it has none. It is not a CW, a
Digital or a Voice thing, so the three workspaces are the wrong place for it. The
header above the divider is the right kind of place and is already carrying the
band plan, the neighborhood and the radio.

**The control is unreferenced in the tree and still tested**, which is the state
the ruling asked for rather than a loose end. `ABANDONED_WIDGETS.md` describes
what it did — dwell rather than landing, twenty seconds taken from one relaxed CQ
call, two visits within 200 Hz counted as one place — so a decision to rebuild it
starts from a description.

---

**Two units running have shipped a fault that a passing test covered.**

Unit 1.11.25 asserted this area and passed over a blank screen, because it drove
the view model instead of the control. Unit 1.11.24 shipped a panel with no
background, because nothing checks that a resource resolves. **Both suites were
green and both faults reached the operator's own screen.**

*Not proposed, because it needs a ruling:* whether view-level tests should be
required to act through the controls — pressing the button, not setting the
property — the way this unit's round-trip test does. It is a rule about how tests
are written rather than a change to any of them, and unit 1.11.13's
"assert the geometry that causes the fault" is the same shape of rule.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Twenty-two inbound. The
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
17. **HM-DEC-086's supersession needs a record** (1.11.25).
18. **The phrasebook's arrival and the absent-widget news are gone** (1.11.25).
19. **Engine code behind the abandoned widgets is unreachable** (1.11.25).
20. **An unresolved `StaticResource` fails silently and nothing checks**, above.
21. **The recent-places row has no home**, above.
22. **Whether view tests must act through the controls**, above.

New this unit: **the silent resource failure**, above; **the recent-places row's
homelessness**, above; **the question of driving controls rather than view
models**, above.

Closed this unit: **the blank workspace**, diagnosed to the line and fixed. **The
boundary**, built with the selected tab merged into it. **The duplicate frequency
card** and **the recent-places row**, both out. **The record of what that row
did.**

Still open: **the lock's mixed help**; **three fixtures at accepted cost**; **the
reference and port integrator difference**; **an unmeasured pitch costs `N4L`**;
**the six-hertz window disagreement**; **the short-character bias**; **the
Avalonia geometry offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.26**; **the
whole-file second pass**; **the squelch has no axis**; **the three morning
captures of 2026-08-26**; **seven timing intermittents, none of which fired
today**.
