# Work instruction 300 — the achievement mark, and the screen it opens

**READ IN THIS ORDER.** The number below is the whole unit in one line: **the mark
stays lit until he opens the screen, and nothing else clears it.**

A. **The phase goal — FT4 works exactly the way FT8 does.** This unit advanced no
   step of it. It built the achievement mark in the status bar and re-presented the
   achievements screen, neither of which is FT4.

B. **Step 4 and its exit criteria** — pressing FT4 tunes and decodes; the panel, the
   conversation, the ring, the filters, the tooltips, the ledger and the right-click
   menu all working unchanged; one click, one transmission; a whole exchange from one
   right click at the bench. **None was measured tonight.** Step 4 stays `partial`,
   and its second criterion now describes a status bar with a fourth thing on it.

C. **The report last, and section 4 raises 3 items.** None blocks the next unit. One
   is an inherited red measured and recorded rather than repaired, one is the ask
   carried since unit 299, and one is a limitation re-tested rather than inherited.

```
UNIT:       300 — complete at task 6 of 6, none dropped — 2026-09-09 21:32
PHASE GOAL: FT4 works exactly the way FT8 does.
UNIT GOAL:  A green quill in the status bar orbits when something new is earned and
            opens the achievements screen, and that screen reads as a collection
            rather than a list.
ADVANCED:   no — no phase step moved. Step 4 stays partial: nothing tuned, nothing
            transmitted, no exchange run at the bench.
NUMBER:     how long the mark stays lit, and what clears it
            IT STAYS LIT UNTIL HE OPENS THE ACHIEVEMENTS SCREEN. There is no other
            path that clears it, and it is written to settings.json, so it survives
            closing Hamlet and reopening it a week later.
            THE MOTION IS A SEPARATE THING AND IT LASTS 30 SECONDS — ten turns of
            three. When it ends the ring and the fill stay exactly as they were.
            Watched failing: with the orbit's end also clearing the flag, the mark
            reports itself dark with nothing having been opened.
DRIFT:      4 consecutive units without advance  (was 3, carried from unit 299)
```

---

## 1. What Claude did

**Complete. Six tasks of six, none dropped, including the named drop candidate.**
Development machine, prompt claimed `PROJECT: Hamlet`, branch `main`, three commits,
all pushed. Root version 1.12.262 to **1.12.263**, bumped once. **No file under
`src/Ft8Sharp/` was touched**, and nothing here reaches a send path.

**Nothing in this report is evidence about the radio.** Nothing was tuned, nothing
was transmitted, and no audio was captured.

**Nothing was recorded to `DECISIONS.md`.** The four judgements this unit made on
its own authority are in `PHASE_OUTCOME.md`'s entry and are listed below; none of
them supersedes a ruling and none weighs two costs.

### Task 1 — the mark at rest

**The shipped mark was measured before anything was built, and it refused to load.**
`SvgMark.Shapes` threw `FormatException: Invalid color string: 'context-stroke'`.
That is unit 285's rule working exactly as written — the loader refuses a paint it
does not understand rather than drawing it wrongly — and unit 286's precedent
decides what to do about it: **the mark is approved and the loader is not.** So
`SvgMark` now understands `currentColor`, `context-fill` and `context-stroke` as
deferrals: they come back `null`, the stroke keeps its width, and the caller paints.
The quill loads as 2 shapes, one path and one line, both with a deferred pen.

**The mark is a `Button` wrapping the drawn control and not a bare control.** That is
unit 299's finding applied: `HintMarkControl` draws a ring with no fill, so its
middle is not a hit target. Measured in the realized window, the button is
**26 × 26 px** with the mark 20 × 20 inside it, and its command is the same
`OpenAchievementsCommand` the menu uses. **There is no second achievements window.**

**The status bar's ceiling was confirmed rather than assumed.** The CW tab reads
**528 of 650** and the Voice tab **518 of 650**, both exactly the figures the rows
were set from — the icon added no characters, because its only words are a tooltip
and no ceiling counts hover text. (The Digital tab is over its ceiling; it was over
before this unit started. Section 4.)

### Task 2 — the orbit, when something is new

**`AchievementsUnseen` is set where a badge is raised and cleared where the screen is
opened, and nowhere else.** It is persisted in `settings.json`. It rides the badge
event, so unit 278's silent-seeding rule already keeps a first look at an existing
log quiet — measured: fourteen imported contacts light nothing, and one contact on a
band he has never worked lights it.

**It is cleared before the dialog is shown rather than after it closes.** He has
looked the moment it opens, and a dialog that threw on the way up would otherwise
leave the mark lit for something he had already seen.

**The two states differ by shape.** At rest: no ring, hollow vane. Lit: a ring, a
filled vane, and a bead going round it. **Watched failing**, with `OnTick`'s settle
branch also clearing `IsNew`: `after the orbit settles: IsNew=False IsOrbiting=False`
with nothing having been opened.

### Task 3 — the screen becomes a collection

**Presentation only. Nothing changed about what is visible or what unlocks.**

*Your best* is the `Everything` scope as tiles, four across. **That tab came off the
strip below** rather than being drawn twice.

*Places* carry `worked of total`, a bar that is those same two numbers, the countries
named as small tiles, and the prefixes to look for inside a group already opened.

*Go and try* carries a ring, the figure remaining, and its own small glyph — drawn
from primitives, never from a font, because a glyph that renders as a box on one
machine is not a carrier of anything.

**The CW, PSK31 and Voice note is untouched**, and the header still counts only what
he has opened.

### Task 4 — the percentages, made honest

**Every ring says on its face what its number is a percentage of.** Not on the hover:
a hover is read by somebody who already wondered, and the person this protects is the
one who glanced at 63% and did not wonder at all. Every ring and its meaning is in
section 3 below.

**Where there is no linear measure there is no percentage.** A band is worked or it
is not; there is no part of a continent to have reached. Those rings are **dashed,
with the target inside them and no number anywhere on them**. So is the distance ring
when no contact in the log has carried a grid square — **nothing measured is not the
same as measured zero.**

**A ring he has finished says `done` rather than `100%`**, so no full ring invites
the others to be read as the same kind of number.

**Watched failing**, with the band challenge given a ring: it draws `0%` for a target
that has no fraction to be part of.

### Task 5 — what it looks like, and what nobody looked at

**Named drop candidate, not dropped.** Section 3 carries the figures. The short
version: **nothing in this repository looked at a pixel**, re-tested rather than
inherited, and the measurement caught a defect this unit had introduced.

### Task 6 — the outcome entry

Appended through `tools\arbiter\outcome-append.bat`, which resolved the unit number
from the instruction's own heading and filed it as **`UNIT 300 - STEP 4`** with no
correction by hand. **That is three units' ask closed** — units 297, 298 and 299 each
had to renumber the heading afterwards, and this heading carries `300`.

### The four things decided on this session's authority

1. **`SvgMark` understands a deferred paint.** Unit 286's precedent, one way.
2. **The `Everything` tab came off the tab strip**, because its records are the tiles
   at the top now and a screen that says a thing twice reads as two facts. Nothing
   became invisible and nothing unlocks differently.
3. **The two `AchievementsWindow` ceiling rows were re-measured**, 412 → 158 and
   443 → 153, with the reason in the row's own comment. Part of that drop was already
   there before this unit: measured at its starting commit, 149 and 144.
4. **An earned ring says `done`.**

---

## 2. What the owner should expect

**A green feather in the bottom bar, on the left of the contact badge.** Hover it and
it says which of its two states it is in. Click it and the achievements open — the
same window the menu opens.

**When you earn something, it stirs.** A bead goes round it for half a minute and
then stops, and the feather stays green and ringed until you open the screen. **If
you were looking at the radio the whole time you have missed nothing**: close Hamlet,
come back next week, and it is still marked.

**The screen behind it looks like a collection now.** Your best across the top as
tiles — `3,400 miles / Furthest so far / EI4GNB · Ireland` — then your places with a
bar and the countries in them, then the things to go and try, each with a ring.

**What will look wrong and is not:**

- **Some rings are dashed with a word in them instead of a percentage.** That is
  deliberate and it is the most careful thing in this unit. There is no half-worked
  band, so there is no number to put there.
- **The `Everything` tab is gone from the strip.** Its records are the tiles at the
  top; nothing was removed.
- **The achievements screen says fewer words than it did.** The prose lines under the
  targets became rings with the figure remaining in them.
- **The character-ceiling test is red on two of five.** Both are the Digital tab and
  both were red at the commit this unit opened on. Section 4.

**Build:** succeeded, 0 warnings, 0 errors. **Tests:** 13 constructed in this
instruction across four classes, **13 of 13 green**, each run filtered by exact name
and foregrounded. **No suite was run** (HM-DEC-155). The gates re-run because this
unit touched their surfaces: `BindingHealthTests` green,
`EveryResourceKeyResolvesTests` green, `Unit298UnlockTests` 14 of 14 green,
`Unit298ScreenDrawsTests` 2 of 2 green after following the renamed heading,
`HowMuchTheApplicationSaysTests` 3 of 5 — the same 2 red as at the starting commit.

**Pushed:** three commits to `main`, nothing uncommitted that belongs to this unit.

---

## 3. What you should see

### 1. The mark in both states, and how they differ without colour

|  | at rest | something new |
|---|---|---|
| ring | **absent** | present, 1.6 px |
| bead | absent | present, going round |
| vane | **hollow** | **filled** |
| shapes drawn | **2** | **4** |
| ink | muted grey `#6E6E66` | decode green `#3B6D11` |

**The difference is a count of shapes, not a hue.** Print it in greyscale and one has
a circle round it and a solid body and the other has neither. Measured by running the
control's own `Render` and recording what came out of it — at 16, 24 and 32 px, in
both states, the resting mark emits 2 drawings and fills none of them, and the lit
one emits 4 and fills two.

### 2. The screen's *Your best* and *Places* as they now render

From the real window, two contacts in the log:

```
Your best
  3,400 miles          Furthest so far                       EI4GNB · Ireland
  -9 dB                Faintest you have been heard so far   EI4GNB · Ireland
  -14 dB               Faintest you have heard so far        EI4GNB · Ireland
  10 September 2026    First so far                          W1ABC · United States
  2 contacts           Busiest day so far                    10 September 2026
  02:00 UTC            Busiest hour so far                   2 contacts

North America        1 of 45 worked   [bar]   the United States  1 contact
Europe               1 of 63 worked   [bar]   Ireland            1 contact
```

**The figure is said once.** The line under a tile is the callsign and the country,
never the distance again — unit 299's caption fault kept out of a smaller frame.

**The bar and the words are the same two numbers**, and the denominator is what
Hamlet can recognise rather than the publication's own total, which is the smaller
claim and the honest direction to be wrong in.

### 3. Every ring's number and what it is a percentage of

Measured against a four-contact log reaching 1,900 miles with a -19 dB report:

| ring | reads | what the number is of |
|---|---|---|
| First past 3,000 miles | **64%** | **miles covered.** *1,900 miles of 3,000, so 1,100 miles to go. The ring is miles covered and not how likely the rest is, and the next thousand is much harder than the last.* |
| First report below -21 dB | **90%** | **decibels covered, counted down from 0 dB**, where a signal and the noise are the same size. *It is decibels covered and not how likely the rest is: every one of them is harder than the one before.* |
| A contact on 80 m | **dashed, `80 m`** | **nothing.** *There is no half of this to be in, so there is no percentage.* |
| A contact on 80 m or 40 m after dark | **`done`** | already held. |
| A contact on the grey line | **dashed, `1 hr`** | **nothing.** *You have either done this or you have not.* |
| A contact on a new continent | **dashed, `one more`** | **nothing.** *There is no part of a continent to have reached.* |
| 5 grid squares worked | **80%** | **a count of squares.** *4 squares of 5, so 1 to go.* |
| 5 contacts in one day | **60%** | **a count of contacts.** *3 contacts of 5 in one day, so 2 more in a single sitting.* |

**And one more, which is the case worth reading twice.** Given a log whose contacts
carry no grid square at all, the distance ring goes **dashed with no number**, not to
0%: *No contact has carried a grid square, so there is no distance to measure.*

The places bar is the ninth measured figure: **countries worked over countries Hamlet
can recognise**, both whole numbers, with `1 of 45 worked` in words beside it.

### 4. What was verified by looking, and what was not

**Nothing was verified by looking. Nothing in this repository can rasterise, and that
was re-tested tonight rather than inherited from units 285 and 286.**

The wall is the same and its symptom is worth writing down, because it is not what
those units reported: `RenderTargetBitmap.Render` and `Save` **both return without
complaint and no file appears on disk.** It fails silently rather than throwing, so a
session that only checked for an exception would conclude it had a picture.

**What was measured instead**, and it is stronger than reading the renderer's
arithmetic back out of the renderer: `Render` was run at each size into a recording
context and what it emitted was captured.

```
16 px, at rest        : 2 drawings   vane 2.2 px pen, no fill   spine 1.7 px pen
16 px, something new  : 4 drawings   ring 14.0 across at 1.6    bead 3.5 filled
                                     vane 2.2 px pen, FILLED    spine 1.7 px pen
24 px, at rest        : 2 drawings
24 px, something new  : 4 drawings   ring 22.0 across           bead 5.3 filled
32 px, at rest        : 2 drawings
32 px, something new  : 4 drawings   ring 30.0 across           bead 7.0 filled
```

**That measurement caught a defect this unit had introduced.** The orbiting bead was
a fixed 2.2 radius whatever the box, so in a 16 px mark it was a third of the width
of the 7 px ring it runs round — a blob rather than a bead going somewhere. It scales
with the box now, which is where 3.5, 5.3 and 7.0 come from.

**And one figure is arithmetic and is labelled as arithmetic**, because the
instruction is explicit that computing a stroke width is not looking:

```
ARITHMETIC, NOT A LOOK. Nothing below was rasterised or looked at.
 16 px box: quill 3.2 x 6.3 px, its outline 0.50 px, the spine 0.38 px
 24 px box: quill 4.7 x 9.5 px, its outline 0.74 px, the spine 0.57 px
 32 px box: quill 6.3 x 12.6 px, its outline 0.99 px, the spine 0.77 px
```

**At 16 px the quill's own outline works out to about half a device pixel and its
spine to 0.38.** Whether those render as faint lines, as solid ones, or as nothing at
all is exactly the question nothing here can answer. **The status bar draws the mark
at 20 px, so no surface that ships is at 16** — the smallest case is hypothetical
until somebody puts it in a smaller frame or looks at it on a real screen.

---

## 4. What's blocking us

**Nothing blocks the next unit.** Three items, ordered with the one blocking the most
work first.

### 1. The Digital tab is over its character ceiling, and the figure moves

**Recorded as `HM-OPEN-089`, owner `claude`, severity `slows`. Not a ruling ask** —
it is here because two of the five character-ceiling tests are red and the next unit
will see them.

Measured with this unit's changes stashed, at the commit it opened on:
`MainWindow — Digital tab holds 1281, ceiling 1250`, set from 1118, and
`Digital tab, working holds 1233, ceiling 1200`, set from 1087.
`AddingASentenceToACappedSurfaceTurnsItRed` fails for the same reason and is not a
second defect: it asserts the tab is under its ceiling, adds a sentence, and expects
that to turn it red — a tab already over cannot demonstrate anything.

**The CW tab reads 528 and the Voice tab 518, both exactly what they were set from**,
so this is the Digital tab alone.

**The part that makes it an issue rather than a re-measure:** two runs of the same
tree an hour apart read **1281 and then 1293**, twelve characters apart, while every
other row held still. Something on that tab composes text from the clock. **A ceiling
set against a moving number cannot be set correctly**, so raising the row would only
move the failure. Left alone under §12.6; whoever takes it has to find the moving
string first.

### 2. Nothing in this repository can look at a picture

Named rather than repaired, and it now has a second measurement behind it. Real
pixels want `Avalonia.Headless.Skia`, and **a new package is a dependency decision
rather than a session's** (§0.4). It is raised here because three units running have
had to write *nobody looked* into a report about something drawn, and the cost of
that is quietly compounding: this unit shipped a bead that was the wrong size at
small scales and only found it by arithmetic.

**Not a ruling ask unless you want it to be.** If a picture ever needs to be
*correct* rather than *present*, this is what it costs.

### 3. `Ft8Sharp` did not move

Stated because the instruction requires it. No file under `src/Ft8Sharp/` was read,
edited or built.

### Asks still outstanding

Carried verbatim per HM-DEC-139.

1. **The `i` hint mark's hit target.** *First made 2026-09-09, unit 299.* Unit 299
   measured it: bound, drawn at 14 by 14, carrying 444 characters — **it was never
   empty.** `HintMarkControl` is a bare `Control` drawing a ring **with no fill**, so
   the middle is not a target. The globe unit 299 added is a `Border` with a
   transparent background and is a solid 16-pixel target. **Waiting on:** Tim hovering
   the two side by side and saying whether one is easier to hit. If it is, the mark
   becomes a `Border` everywhere in one change. **Where the change already sits:**
   `src/Hamlet.App/Controls/HintMarkControl.cs`, unchanged. **This unit did not touch
   it**, and the achievement mark it built is a `Button` for exactly the reason the
   ask exists.

2. **A US state, from callook.** *First made 2026-09-08, unit 297; hit again by units
   298 and 299.* Hamlet cannot name a US state: neither the grid square nor the DXCC
   entity carries one, so every place name for a US station stops at *the United
   States*. **Waiting on:** the parked callook instruction. **Where the change already
   sits:** nowhere — nothing has been built against it, and every unit that has hit it
   has stopped at the country rather than inferring below it.

**And one that is dropped rather than carried.** Three consecutive orders shipped with
no unit number in the heading and `outcome-append.bat` had to be corrected by hand
each time. **This order's heading carries `300`, the script resolved it, and the entry
filed as `UNIT 300 - STEP 4` with nothing renumbered.** The ask is closed by the
author's own fix and is not carried forward.
