# Work instruction 027 — clear the CW tab

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. Branch `main` throughout, three commits, all
pushed, none refused. Version 1.11.23 to 1.11.24 per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

**All three tasks ran, including the drop.**

**No decoder file was touched, and both proofs are in.** `git diff` over this
unit's commits against `src/Hamlet.RadioEngine/` reports **zero files**, and the
engine suite is **28 failing of 1841, byte-identical to the stable set**.

### The two things the order could not see — both confirmed

1. **Unit 1.11.23's header is there.** The band plan full width, the neighborhood
   hosted from `widget.map` at `MainWindow.axaml:2273`, the radio beside it, and
   the divider. The photograph was cropped above it.
2. **The neighborhood in the photograph was the second copy.** The same template
   was in the header and still in the tray, which is the duplicate unit 1.11.23
   reported in its own section 4.

## 2. What the CW tab now contains

**Receive on the left and Send on the right. Nothing else.**

**Receive is a panel now rather than whatever space was left over.** That was the
fault: unit 1.11.23 made the canvas itself the Receive side, and the canvas still
carried the whole previous arrangement, so Receive got the remainder — a column
narrow enough to render `wh at the rad io is he ari ng`.

**Every widget that was on the CW canvas left it, and none was deleted:**

| widget | where it is now |
|---|---|
| Neighborhood map | **in the header**, permanent above the divider — and **out of the tray**, by ruling |
| Dial tape | in the tray, far left |
| Waterfall | in the tray, far left |
| I can hear it and Hamlet can't | in the tray, far left |
| Scanner | in the tray, far left |
| CW terminal | **it is Receive now**, permanent on the CW tab |
| everything else in the catalogue | in the tray, far left — **fourteen offered** |

**A first run starts from a new arrangement, "Just receive and send", with
nothing out.** The furnished arrangement is still on the preset bar under its own
name, one press away, and every other arrangement is unchanged.

**Digital and Voice each say one line** naming what will live there, and carry no
controls.

**What will look wrong and is not:**

- **His existing `layouts.json` still restores whatever he had out.** The change
  affects what a fresh profile gets and adds a one-press way to clear it; it does
  not reach into a saved arrangement. **Press "Just receive and send" on the bar
  and the tab is clear.**
- **The canvas is invisible until something is dragged out.** It takes no room
  while it holds nothing, which is why two panels is what he sees.

## 3. The width assertions

**Receive is wide enough to read**, measured in characters of the terminal's own
face rather than in pixels, because the fault was that text had nowhere to go:

| window | Receive | terminal | **characters to a line** |
|---|---|---|---|
| 1200 px | 672 px | 614 px | **61** |
| 1400 px | 872 px | 814 px | **81** |

Against the photographed **one or two**. The assertion floor is forty.

**Two panels in the operating area and no widgets**: `0 widgets out, 14 in the
tray`.

**Receive against Send**: Receive 872 px, Send 280 px at 1400. Receive is the
wider, they do not overlap, and Send is present on CW and absent on Digital.

**The tab strip still begins at Receive's left edge** — tabs x=222, operating
area x=222.

**The header is not re-created on a mode change**, by reference identity:

```
band card same: True, neighborhood same: True, radio same: True
```

**The band row is intact**: every label renders inside its own card, and the
badge's first clipping ancestor is `MainWindow` — nothing between clips it.

**Nothing in this unit hit-tests.**

### The suites

| | baseline | end |
|---|---|---|
| engine | 28 of 1841, stable set | **28 of 1841, byte-identical** |
| app | 520 of 520 | **527 of 527** |

**Sixteen app tests went red on the way and every one was a test pinning
behaviour this ruling changed** — a furnished first run, the preset list, the
tray's contents, and a canvas that starts with widgets on it to manipulate. Each
was updated to say what is now true, with the reason at the site. None was
deleted.

### Where the instruction and the tree disagree

- **`CLAUDE_CODE.md` is at 1.5**, as the order states. Confirmed.
- **The tray already sat at the far left** and needed no move.
- **`AppSettings.UseJointDecoder` and `ShowKeyingSweep` both ship false**,
  untouched.
- **`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150**, nor
  Tim's rulings of 2026-08-25/26/27.

## 4. What's blocking us

**HM-DEC-086 says nobody ever starts on an empty canvas, and this unit changes
what a first run gets. The reconciliation is written down rather than assumed.**

Ruling asked for:

> **HM-DEC-086's "nobody ever starts on an empty canvas" is untouched. What that
> ruling forbids is an empty rectangle beside a list of things to drag — a puzzle
> handed to somebody who came here to talk on the radio. A first run now lands on
> four panels: the band plan, the neighborhood and the radio above the divider,
> and Receive and Send below it. What is empty is the canvas layer, not the
> screen.**

The test that pinned the old behaviour now says this in those terms and carries
the reasoning. **If the reading is wrong, the ruling to make is that a first run
should still furnish the canvas**, and this unit's default is what changes.

---

**The terminal is a permanent panel and is still in the tray, which is the same
duplicate the neighborhood had.**

The ruling names the map as *"the one widget that leaves the catalogue"*, and it
did not anticipate that Receive would be built from the terminal widget — because
the order specified the tab's contents rather than how they would be assembled.
**So the terminal can be dragged out and the same panel appears twice**, exactly
as the map did before today.

**It is left exactly as it is**, because the order forbids removing a widget and
names one exception. **The ask is whether the terminal takes the same route the
map took.**

*Rejected: taking it out on my own judgement.* The prohibition is explicit and the
exception is named as singular.

---

**The neighborhood left the tray but stayed in the catalogue, and that is a
deliberate half-measure.**

Removing `Widgets.Map` from `Widgets.All` was built first and breaks two things:
`Widgets.Lookup` stops resolving it, so a saved layout naming it becomes an empty
box with a question mark, and `EveryPresetPlacesOnlyRealWidgets` goes red because
`Listening around` placed it. **So it leaves the tray and the presets, and stays
where `Lookup` can find it.** That satisfies the ruling's words — *"comes out of
the tray"* — and is worth stating because "leaves the catalogue" would have meant
something stronger and more destructive.

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
16. **Per-mode widget placement needs a stored-format ruling** (1.11.23).
17. **There is a Send button that does not send** (1.11.23) — parked by this
    order and unruled.
18. **HM-DEC-086 and the first run**, above.
19. **The terminal is permanent and still in the tray**, above.
20. **The neighborhood left the tray, not the catalogue**, above.
21. **`013347` returns a likelihood ratio of 17.2 million**, with `001520`'s
    quadrillions. Parked, raised once.

New this unit: **the HM-DEC-086 reconciliation**, above; **the terminal's
duplicate**, above; **the neighborhood's half-measure**, above.

Closed this unit: **the CW tab**, cleared to Receive and Send with the width
measured at 61 and 81 characters a line. **The neighborhood's duplicate** from
1.11.23. **The two empty tabs**, each given a line.

Still open: **the lock's mixed help**; **three fixtures at accepted cost**; **the
reference and port integrator difference**; **an unmeasured pitch costs `N4L`**;
**the six-hertz window disagreement**; **the short-character bias**; **the
Avalonia geometry offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.24**; **the
whole-file second pass**; **the squelch has no axis**; **the three morning
captures of 2026-08-26**; **seven timing intermittents, none of which fired
today**.
