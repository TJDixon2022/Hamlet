# What the mark looks like, and what nobody looked at — work instruction 285, task 5

**The honest half of this unit.** The order asks what the mark renders as at each
place it now appears, and says: *do not claim a size looks right if nothing looked at
it.* **Nothing looked at any of it.** Everything below is either a measurement of the
geometry or a measurement off a realized visual tree; **no pixel was ever drawn and
inspected**, and the reason is recorded rather than worked around.

---

## Why nothing looked

The tests run on **Avalonia's headless drawing backend**
(`AvaloniaHeadlessPlatformOptions`, `BindingHealthTests.cs:20`). It composes a visual
tree and rasterises nothing. Measured, not assumed:

- `RenderTargetBitmap.CopyPixels` throws **“CopyPixels is not supported for this
  bitmap type”**.
- A first attempt used the saved PNG's compressed length as a proxy for “did anything
  draw”, and **reported the full mark as blank when it is not** — a guess wearing a
  measurement's clothes, removed rather than tuned.

Real pixels want `Avalonia.Headless.Skia`. **A new package is a dependency decision
with a licence and a supply chain attached** — Tim's under §0.4, and this order did
not ask for one.

---

## What was measured

| Place | Verified how | Result |
|---|---|---|
| Both files parse | Loaded through `SvgMark` | **26 shapes** in the full mark, **4** in the small |
| Every shape has ink and a size | Walked the loaded shapes | **0 of 30** with no brush, no pen, or no extent |
| Full mark against its viewBox | Geometry union vs. declared box | **18.5 of 260 outside, at the bottom** |
| Small mark against its viewBox | Same | **15.5 of 68 outside, at the top** |
| The About window draws it | Realized visual tree | `HamletMark`, **161 × 140 px**, source `DrawingImage` |
| The About window without it | Element renamed, test re-run | *no Image named HamletMark … Images found: [NotTheMark]* |
| The icon builds | Rasterised through `AppIcon` | **256 × 256** |
| The About character ceiling | `HowMuchTheApplicationSaysTests` | **726 of 850**, unchanged |

---

## The About window

**161 × 140 px**, left of the wordmark, top-aligned, with the tagline and the mission
paragraph to its right. That clears the 150 px the order asks for so the faceplate
detail reads.

**The theme question needs no dark variant.** `App.axaml` sets
`RequestedThemeVariant="Light"` and HM-DEC-012 rules the application is a light theme
with colour rather than dark mode, so the parchment plate never lands on a dark
ground. **If that ruling ever changes, the mark wants looking at again** — a
`#E8E2D6` plate on a dark background would read as a bright rectangle.

**No caption.** The mark is not labelled; the wordmark beside it is the name.

---

## The window and the taskbar

**There was no icon before this.** `Hamlet.App.csproj` sets no `<ApplicationIcon>`
and no window set `Icon`, so both showed Avalonia's default.

`AppIcon` rasterises the small mark at **256 px** at startup and `MainWindow` sets
`Window.Icon` from it. **Null where the platform cannot raster, and the window opens
anyway**: a window that refused to open because it could not draw a picture of itself
would be the worst trade available.

**Not verified:** that the taskbar shows it. A headless run has no taskbar, and this
machine is not the one it would be looked at on.

---

## What survives at icon sizes

**Arithmetic, not a look.** The mark is 68 units across, so a pixel at size *N* is
68/*N* units.

| Size | One pixel | plate outline (3.4) | whip (4.0) | feather outline (3.0) |
|---:|---:|---:|---:|---:|
| **16 px** | 4.3 units | **0.80 px** | **0.94 px** | **0.71 px** |
| 32 px | 2.1 units | 1.60 px | 1.88 px | 1.41 px |
| 48 px | 1.4 units | 2.40 px | 2.82 px | 2.12 px |
| 256 px | 0.3 units | 12.80 px | 15.06 px | 11.29 px |

**At 16 px every stroke in the drawing is thinner than a pixel.** The icon at that
size is a dark disc with a suggestion on it rather than a plate and a quill. At 32 the
strokes become lines; at 48 they are comfortable.

**That is a statement about the drawing and not about how it looks**, which is a
different claim and one nothing here can make.

---

## The two defects, reported and not repaired

Work instruction 285: *do not redesign the mark; report what will not render and say
what you would change.*

### The full mark loses the bottom edge of the faceplate

Ink runs to y = 278.5 against a viewBox ending at 260. **18.5 units, about 7 per cent
of its height** — the lower border of the plate, stroke included.

**What I would change:** the viewBox to `0 0 320 280`, which shows the drawing whole
and moves nothing. It changes the mark's aspect ratio from 1.23 to 1.14, which is a
composition decision and therefore his.

### The small mark loses half the feather

Ink starts at y = −15.5 against a viewBox starting at 0. **15.5 of 68 units, a little
over half the quill** — and the quill is the joke.

**This one is worse than a trim**, because it is the part that makes the mark a quill
rather than a whip, and it is gone at every size the icon is ever drawn at.

**What I would change:** nothing that is not a redesign. `viewBox="0 -16 68 84"` shows
it all but makes the mark 68 × 84, and an icon has to be square. Squaring it means
re-placing the circle, the plate and the quill inside the box — **which is composing
the mark again, and that is Tim's.**

**Both figures are pinned in `TheMarksRenderTests`**, so whichever way he rules, one of
those numbers changes and the test goes red.

---

## What nothing verified

- **The taskbar.** No taskbar in a headless run.
- **The window title-bar icon at 16 px.** Computed above, never seen.
- **Whether the About window's layout looks balanced** with the mark beside the text.
- **Whether the mark reads as a radio with a quill** at any size. That is the whole
  point of it, and it is the one thing this unit cannot say a word about.
