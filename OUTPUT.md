# Work instruction 026 — the operating screen

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. Branch `main` throughout, five commits, all
pushed, none refused. Version 1.11.22 to 1.11.23 per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected.

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

**All four tasks ran, including the drop.**

**No decoder file was touched, and both proofs are in.** `git diff` over this
unit's commits against `src/Hamlet.RadioEngine/` is **empty**, and the engine
suite is **28 failing of 1841, byte-identical to the stable set**.

## 2. What the screen looks like, top to bottom

1. **The Hamlet title and the line of Shakespeare.**
2. **The band plan, full width.** Card widths keep their wavelength proportions;
   the best-bet badge hangs above the row and nothing clips it.
3. **The neighborhood on the left and the radio on the right**, one row. The
   neighborhood is the wider. Measured at 1400 px: neighborhood at x=31 w=778,
   radio at x=851 w=520.
4. **The privilege line**, spanning both.
5. **A divider.** Everything above it is the same in every mode.
6. **The tray down the left, outside the tabs** — at x=25.
7. **The tab strip — CW, Digital, Voice — beginning at x=222**, which is exactly
   where the operating area begins.
8. **Inside CW: Receive on the left at 872 px wide, Send on the right at 280.**

**"Hold this pitch" is gone**, and **"I hear a station" no longer sets the decode
pitch.** It still banks the last half minute and still adds a row to tonight's
list. Tim's words were *"It shouldn't involve me randomly clicking on a button I
don't understand"*, and both controls existed only because acquisition does not
work — six families of admission statistic measured across five units, none of
which can find a station he can plainly hear. **Decoding is Hamlet's job.**

**The Send panel composes and does not key.** CQ fills the line in his own
callsign — `CQ CQ DE KC3QIS KC3QIS K` — RST gives `RST 599 599`, 73 gives
`73 TU E E`, Clear empties it. The panel says on its face that nothing leaves the
radio, and a test asserts that it says so.

**What will look wrong and is not:**

- **The neighborhood is no longer in the tray.** It is a header panel now, by
  ruling. That is the one change relocating it forced — see section 4.
- **Switching to Digital or Voice shows the canvas with no Send panel.** Those
  tabs have no contents of their own yet; the header and the tray are unchanged.
- **A widget dropped on CW is still there on Digital.** Per-mode placement was
  not built — see section 4.

## 3. The assertions

**Visual-tree order**, at 1200 and 1400 px:

| | y | x |
|---|---|---|
| band cards | **87** | 16 |
| radio | 147 | 851 |
| neighborhood | 193 | **31** |

Bands above both; neighborhood left of radio; **they do not overlap** —
neighborhood right edge 809, radio left edge 851.

**Every band label renders inside its own card.** At 1400 px each wants 40 px and
has 40 to 83; `10 m` gets exactly the 40 it asks for. At 1200 px every card gives
62.

**The badge's clipping ancestor is `MainWindow`** — nothing between the cards and
the window clips the row the badge hangs above. Named, as the order required.

**The tab strip aligns to Receive from render bounds**: tabs x=222, operating
area x=222, and the strip ends at y=474 with the area starting at y=509.

**The header is not re-created on a mode change**, asserted as reference identity
rather than presence:

```
band card same: True, neighborhood same: True, radio same: True
```

Switched CW → Digital → CW; the same three objects throughout. **That is the only
thing distinguishing "still there" from "torn down and rebuilt identically".**

**Nothing in this unit hit-tests.** Every assertion is visual-tree order, render
bounds, clipping ancestors or reference identity, per unit 1.11.13's rule.

**Send**: to the right of Receive and narrower (280 against 872); present on CW,
absent on Digital, back on CW; all four buttons compose; the panel states that
nothing leaves the radio.

### The suites

| | baseline | end |
|---|---|---|
| engine | 28 of 1841, stable set | **28 of 1841, byte-identical** |
| app | 503 of 503 | **520 of 520** |

Seventeen tests added, all passing. No intermittent fired.

### Where the instruction and the tree disagree

- **`CLAUDE_CODE.md` is at version 1.5, not the 1.4 the order states.** It moved
  with unit 1.11.22's delivery; §8 still specifies four sections.
- **The tray was already down the left**, at column 0 of the canvas row. What was
  missing was the tab strip, not the tray's position.
- **`AppSettings.UseJointDecoder` and `ShowKeyingSweep` both ship false** and are
  untouched, as required.
- **`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141, 150**, nor
  Tim's rulings of 2026-08-25/26/27.

## 4. What's blocking us

**Relocating the neighborhood forced one change, and the order asked for it to be
named rather than made quietly.**

The neighborhood was a **canvas widget**, not a fixed panel — one of the things
the operator could drag out, put away and arrange. The ruling makes it a header
panel, which means it is no longer any of those. **Its contents, rendering and
behaviour are untouched**: the header hosts the same `widget.map` template the
canvas used, so there is one definition of that panel and not two.

**What follows and needs a ruling:** it is still listed in the tray, so the
operator can add a second copy of it to the canvas. Removing it from the tray
would be removing a widget, which this unit is forbidden to do. **Either it comes
out of the catalogue or the duplicate is accepted**, and that is a ruling rather
than a session's tidy-up.

---

**Per-mode widget placement was not built, and the order allowed either.**

The canvas carries one arrangement in `layouts.json`, keyed by widget rather than
by widget-and-mode, and giving it a mode dimension means changing the layout
store's format and its migration. **One shared arrangement was built**, which is
what the existing machinery carries: a widget dropped on CW is still there on
Digital.

*Not proposed, because it needs a ruling:* whether the layout store should carry
a mode, which is a stored-format change and therefore a migration with a test
that proves an existing profile survives (§6.1's second exception).

---

**The Digital and Voice tabs are empty, and that is visible.**

The ruling names three tabs and specifies contents for one. Switching to Digital
or Voice today shows the canvas and the tray with no Send panel and nothing
mode-specific. **It is honest and it looks unfinished**, which on a screen the
operator is about to use is worth a decision: either those tabs get a line saying
what will live there, or they are hidden until they have contents.

---

**Send exists and reaches nothing, by design and by prohibition.**

The panel composes text and the buttons fill a line. **§0.2 and HM-DEC-098 stand
untouched**: a transmit path is a separate ruling taken after every interlock has
been watched to fire into a dummy load, including the link pulled mid-cycle. The
order forbade wiring it and it is not wired. **The ask is only that the panel now
makes the gap visible** — there is a Send button on screen that does not send,
labelled as such, and that is a state worth ruling on rather than letting sit.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Twenty inbound. The
oldest is open since 2026-08-14.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150 — nor for
   Tim's rulings of 2026-08-25/26/27.**
5. **The tone tracker** — six axis families measured; the question is a design
   one, and the operator's assertion is no longer the way round it.
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
16. **The neighborhood is in the header and still in the tray**, above.
17. **Per-mode widget placement needs a stored-format ruling**, above.
18. **The Digital and Voice tabs are empty**, above.
19. **There is a Send button that does not send**, above.
20. **`013347` returns a likelihood ratio of 17.2 million**, with `001520`'s
    quadrillions. Parked, raised once.

New this unit: **the neighborhood's duplicate**, above; **per-mode placement not
built**, above; **two empty tabs**, above; **a Send button that does not send**,
above.

Closed this unit: **the operating screen**, laid out as ruled and asserted from
geometry. **The pitch controls**, off the panel with the engine capability kept.
**The send panel**, composing and keying nothing.

Still open: **the lock's mixed help**; **three fixtures at accepted cost**; **the
reference and port integrator difference**; **an unmeasured pitch costs `N4L`**;
**the six-hertz window disagreement**; **the short-character bias**; **the
Avalonia geometry offset**; **`CHANGELOG.md` at 1.9.0 against 1.11.23**; **the
whole-file second pass**; **the squelch has no axis**; **the three morning
captures of 2026-08-26**; **seven timing intermittents, none of which fired
today**.
