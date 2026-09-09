# What this unit changed — work instruction 286, tasks 4 and 5

The sixth of these, after units 280 to 285.

---

## The number

**The seconds the ring shows with the turn unknown.**

| | |
|---|---:|
| Before | **none** — a bare `?` |
| After | **the seconds to the next boundary**, 1 to 15 |

Measured: 7 s into a slot with nothing heard from anybody, the ring reads **8** and
still reports the turn as unknown.

---

## Task 4 — what the marks render as, and what was verified

**Nothing was verified by looking. The limit unit 285 recorded still holds, and it
was re-checked rather than assumed.**

The tests run on **Avalonia's headless drawing backend**
(`BindingHealthTests.cs:20`). It composes a visual tree and rasterises nothing:
`RenderTargetBitmap.CopyPixels` throws *“CopyPixels is not supported for this bitmap
type”*. Whether a rasterising harness is worth a package is **on the asks queue and
is Tim's**, and this order says not to add one.

### What was measured, and how

| Claim | Verified how | Result |
|---|---|---|
| Both files load | Through `SvgMark` | **35 shapes** full, **8** small |
| Every shape has ink and a size | Walked the loaded shapes | **0 of 43** blank |
| Full mark inside its viewBox | Ink union vs. declared box | **18, 5.75 → 358.25, 348** in 380 × 360 — **inside** |
| Small mark inside its viewBox | Same | fills **64 × 64** exactly |
| About draws it | Realized visual tree | `HamletMark`, **170 × 161 px** |
| The icon builds | Rasterised through `AppIcon` | **256 × 256** |
| `SvgMark` refused something | Ran it | **`<text>`**, with the exception quoted below |

### What each renders as, at each size

**The full mark, in About: 170 × 161 px.** The transceiver display in the parchment
bezel — `USB-D`, `FIL1`, `RX`, `UTC`, `7.074.0` in amber, the S-meter, and the quill
as the antenna. **35 shapes, all of them inside the box.** Seven of the 35 are `<text>`
and now draw as glyph outlines.

**The small mark, as the icon.** 8 shapes: a rust tile, a cream faceplate, a dark
display and knob, the whip, the vane, and two lines of barbs. **Arithmetic on the
strokes, not a look** — the mark is 64 units across:

| Size | One pixel | whip (5.5) | quill spine (2.2) | barbs (1.8) |
|---:|---:|---:|---:|---:|
| **16 px** | 4.0 units | **1.38 px** | 0.55 px *sub-pixel* | 0.45 px *sub-pixel* |
| 32 px | 2.0 units | 2.75 px | 1.10 px | 0.90 px *sub-pixel* |
| 48 px | 1.3 units | 4.13 px | 1.65 px | 1.35 px |
| 256 px | 0.3 units | 22.00 px | 8.80 px | 7.20 px |

**At 16 px the whip survives and the barbs do not.** That is the useful thing about
this drawing and it is why it is a sibling rather than a shrink: **it leans on filled
shapes where the old one leaned on outlines**, so the tile, the faceplate, the display,
the knob and the vane have no stroke to lose at all. The old mark's three strokes were
all sub-pixel at 16.

### What nothing verified

- **The taskbar.** No taskbar in a headless run.
- **The title-bar icon at 16 px.** Computed above, never seen.
- **Whether the About layout looks balanced** with a 170 px mark beside the text.
- **Whether the display reads as a transceiver** at any size, and whether the quill
  reads as a quill. **That is the whole point of the mark and this unit cannot say a
  word about it.**

---

## Task 5 — every string and assertion this unit changed

### The ring

| Was | Now | Where |
|---|---|---|
| `TurnRingCount` returned `"?"` for `NoClock`, `NoStationYet`, or a null count | Returns the count where one exists; `"?"` only where none does | `MainWindowViewModel` |
| The unknown hover ended at *…no turn to work out.* | Gains *The next slot starts in 8 seconds.* | `TurnRingTip` + `NextSlotSentence` |

**Nothing left a screen.** The count was never on screen to lose, and the hover gained
a sentence rather than trading one.

**One change was made and reverted**: `Ft8Turn.Read` briefly carried the count into the
`Stopped` state too, on a literal reading of *always*. That collided with unit 277's
`StoppedEarlySaysSoRatherThanPretendingItRan`, which states that there is nothing left
running to count. **Reverted; raised for a ruling rather than pushed through.**

### The badge

| Added | What |
|---|---|
| `BadgeAward` | A record: the count, the ranks crossed, the heading, what it says, what the next costs |
| `MainWindowViewModel.BadgeEarned` | An event raised once per crossing, after the record is written |
| `BadgeWindow` | The notice, `ShowActivated="False"`, not in the taskbar, gone in 8 s |

**Nothing unit 278 built was changed.** The thresholds, the once-per-rank rule, the
naming of every rank a jump passes, and the silent seeding on a first look are all
untouched and all re-asserted.

**What it says at 10**, quoted:

> **yellow belt**
> That is 10 contacts logged.
> 15 more and the ring turns orange.

**At 0 → 26**: *That is 10 and 25 contacts logged.*, headed **orange belt**.

### The marks

| Was | Now |
|---|---|
| `hamlet-logo.svg`, 320 × 260, ink 18.5 outside the bottom | 380 × 360, ink inside with margin |
| `hamlet-mark-small.svg`, 68 × 68, ink 15.5 outside the top | 64 × 64, ink exactly inside |
| `SvgMark` threw on `<text>` | Draws it as glyph outlines |
| About's mark box 164 × 140 | 170 × 162, renders 170 × 161 |

### The assertions

| Was | Now | Why |
|---|---|---|
| `Assert.Equal(18.5, …)` and `Assert.Equal(15.5, …)` — the old clipping pinned | Both marks asserted **inside** their box | The property was worth keeping; the numbers described drawings that are gone |
| `UnknownShowsAQuestionMarkAndNotAZero` | Replaced by three: the count with the turn unknown, the count from corrected UTC, and the hover | It was right about `NoClock` and wrong about `NoStationYet`. Its red said so: *Expected "?" / Actual "8"* |
| `WhatSurvivesOfTheSmallMarkAtIconSizes` — strokes 3.4, 4.0, 3.0 across 68 units | Strokes 5.5, 2.2, 1.8 across 64 | **It would have gone on passing while describing a drawing no longer in the tree.** A stale fact that stays green is the harder kind to catch |

---

## A fact that left a screen

**None.** Nothing was deleted from any surface. The ring gained a number, the hover
gained a sentence, the badge gained a window, and the marks were replaced by drawings
that show more of themselves than the ones before.
