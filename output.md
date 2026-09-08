# Work instruction 285 — the logo goes into the application

```
UNIT:      285
TASKS:     6 of 6, none dropped
NUMBER:    sizes the mark was verified at, and by what —
           161 x 140 px in About (realized visual tree)
           256 x 256 px as an icon (rasterised through AppIcon)
           16 / 32 / 48 / 256 px stroke widths (arithmetic, nothing looked)
           0 px verified by eye. Nothing rendered a pixel anybody saw.
ADVANCED:  no
DRIFT:     7 consecutive units without advance, carried from unit 283
VERSION:   1.12.199 -> 1.12.205
BRANCH:    main, pushed
```

---

## 1. What Claude did

**Surface: Claude Code, on the development computer, on `main`.** The prompt claimed
`PROJECT: Hamlet` and the tree confirmed it — `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` present, `CoreHMI.sln` and
`MURC.sln` absent, `Hamlet.sln` at the root. **Nothing here is evidence about the
radio.**

**Nothing was recorded under §12.1.** No shell refusals.

### Task 1 — the files, and a defect in both of them

**`src/Hamlet.App/Assets/`**, registered as `AvaloniaResource`. **Not `/data`**: that
folder holds cited facts the application reads and reasons from, and a logo is neither
cited nor reasoned from. It is a resource of the shell, and the shell is the only
thing that may know what a picture is (§0.1).

**Both parse and both draw** — 26 shapes and 4. **And both run outside their own
viewBox**, measured through the loader rather than argued from the spec:

| File | Ink | viewBox | Outside |
|---|---|---|---|
| `hamlet-logo.svg` | 7.5, 5.75 → 312.5, 278.5 | 320 × 260 | **18.5 below** |
| `hamlet-mark-small.svg` | 0, −15.5 → 68, 68 | 68 × 68 | **15.5 above** |

SVG clips to the viewBox, so that ink is lost in a browser, in this application and in
an icon alike. **Reported and not redesigned.** Both figures are pinned in the test.

`SvgMark` reads the files rather than transcribing them, so **the file Tim approved is
the file that renders**. It is deliberately narrow — the seven things these two files
use — and **throws on anything else**, because a mark that silently loses an element is
worse than one that fails to load. **No new dependency**: Avalonia's geometry parser
already speaks the SVG path mini-language, which was the only hard part.

### Task 2 — the mark in About

**161 × 140 px**, measured off the realized window, left of the wordmark. Clears the
150 the order asks for. **It sits with the name rather than replacing it. No caption.**

**One thing had to be right first.** An `Image` scales its source's bounds, and a
drawing's bounds are its *ink* — so the mark would have been laid out from its ink and
clipped somewhere other than a browser clips it, and any description of how it looks
would have been about a rendering nobody else gets. `Load` now puts an invisible
rectangle over the viewBox in front of the shapes, so **the box is what gets scaled.**

**The theme question needs no dark variant.** `App.axaml` sets
`RequestedThemeVariant="Light"` and HM-DEC-012 rules the application a light theme with
colour rather than dark mode. If that ruling changes, the mark wants looking at again.

**Ceiling confirmed rather than assumed**: About holds **726 of 850**, unchanged.

### Task 3 — the window and the taskbar

**There was no icon before this.** `Hamlet.App.csproj` sets no `<ApplicationIcon>` and
no window set `Icon`.

`AppIcon` rasterises the small mark at **256 px** at startup — the largest size Windows
asks for, so every smaller one is a downscale — and `MainWindow` sets `Window.Icon`.
**Null where the platform cannot raster, and the window opens anyway.**

### Task 4 — the test, watched failing on absence

**It had not actually been watched.** It was written before the mark was placed and
first run after, where it went red on the *size* assertion — a different red from the
one the order asks to see. Renaming the element and re-running gives it:

```
no Image named HamletMark on the realized About window. Images found: [NotTheMark]
```

Restored, six green. **Recorded rather than left as an assumption**, because a test
believed to have been watched and never watched is worth less than one nobody claims
for.

### Task 5 — what nobody looked at

`docs/unit285-the-mark.md`. Eight things measured with their method; **four listed as
not verified.**

### Task 6 — the outcome entry

Appended by `tools\arbiter\outcome-append.bat`, exit 0, as `UNIT 285 - STEP E`.

---

## 2. What Tim should expect

**Hamlet has a face.**

Open **Tools ▸ About** and the transceiver sits to the left of the wordmark at about
160 px — the LCD, the S-meter trace, the VFO knob with its detents, the three band
buttons with one lit, and the quill coming up off the top edge. The name is still
beside it; the mark did not replace it, and nothing labels it.

**The window and the taskbar carry the small mark** instead of Avalonia's default
feather.

### What will look wrong, and two of them are

- **The bottom edge of the faceplate is missing** in the About window. That is not a
  layout fault: the drawing runs 18.5 units past the bottom of its own viewBox, and
  every renderer cuts it there. **About 7 per cent of its height.**
- **The icon is missing half the quill.** The small mark draws 15.5 of its 68 units
  above its own box, so the feather is cut roughly in half — **at every size, in the
  taskbar and the title bar both.** That is the part that makes it a quill rather than
  a whip.
- **At 16 px it is a dark disc with a suggestion on it.** Every stroke in the drawing
  is thinner than one pixel at that size. Computed, not seen.

**Both are reported rather than fixed, because the mark is yours.** What I would change
is in §3.

### The build and the tests

Build clean, no warnings. **This unit ran no suite** (HM-DEC-155): only the tests it
wrote, plus `BindingHealthTests` because new markup went into a window.

| Class | Result |
|---|---|
| `TheMarksRenderTests` | 6 of 6 — new |
| `HowMuchTheApplicationSaysTests` | 5 of 5 |
| `BindingHealthTests` | 1 of 1 |
| **Total** | **12 of 12** |

Inherited reds untouched: `HM-OPEN-088`'s ten, the CW set, the `Ft8Sharp.Deep`
tripwire.

**Six commits, all on `main`, all pushed**, 1.12.199 → 1.12.205. Nothing uncommitted.

---

## 3. What we should do next

### Where the files went, and why

**`src/Hamlet.App/Assets/`**, as `AvaloniaResource`. The tree had **no convention for
images at all** — `/data` holds cited data embedded with a `LogicalName`, and every one
of those files is something the application reads and reasons from. A logo is neither.
It is a resource of the shell, it is only ever drawn, and Avalonia's own convention for
a drawable resource is exactly this. **The engine never learns it exists** (§0.1).

### What the mark renders as in About

**161 × 140 px**, top-aligned to the left of the wordmark, tagline and mission
paragraph. **One theme only** — the application is `Light` by ruling, so the parchment
plate never lands on a dark ground and no dark variant is needed. **The bottom edge of
the faceplate is cut**, by 18.5 of 260.

### What it looks like at 16 px, and whether anything looked

**Nothing looked. Not at 16, not at 32, not at 48, not in the taskbar, not in About.**

The tests run on Avalonia's headless *drawing* backend, which composes a visual tree
and rasterises nothing. `CopyPixels` throws *“CopyPixels is not supported for this
bitmap type”*, and a first attempt that used the saved PNG's compressed length as a
proxy **reported the full mark as blank when it is not** — removed rather than tuned.
Real pixels want `Avalonia.Headless.Skia`, and **a package is a dependency decision**.

What can be said is arithmetic. The mark is 68 units across:

| Size | One pixel | plate outline | whip | feather outline |
|---:|---:|---:|---:|---:|
| **16 px** | 4.3 units | **0.80 px** | **0.94 px** | **0.71 px** |
| 32 px | 2.1 units | 1.60 px | 1.88 px | 1.41 px |
| 48 px | 1.4 units | 2.40 px | 2.82 px | 2.12 px |

**At 16 every stroke is sub-pixel.** At 32 they become lines. At 48 they are
comfortable.

### Then

1. **Look at it.** Run the app, open About, look at the taskbar. Everything above is
   geometry, and the one question that matters — does it read as a radio with a quill
   — is the one nothing here can answer.
2. **Rule on the two clipping defects** (§4).

---

## 4. What's blocking us

Nothing blocks the next unit.

### The full mark loses the bottom edge of its faceplate

**Ruling asked for:** the drawing runs to y = 278.5 against a viewBox ending at 260.
**18.5 units, about 7 per cent.**

**What I would change:** the viewBox to `0 0 320 280`. It shows the drawing whole and
moves nothing. **It changes the aspect ratio from 1.23 to 1.14**, which is a
composition decision and therefore yours.

**Rejected:** doing it. The order says report and do not redesign, and a viewBox is
part of the mark.

### The small mark loses half the quill

**Ruling asked for:** ink starts at y = −15.5 against a box starting at 0. **15.5 of 68
units — a little over half the feather, at every size the icon is ever drawn.**

**This one is worse than a trim**, because the feather is the joke. An icon that is a
disc, a plate outline and half a stub is not the mark you approved.

**What I would change: nothing that is not a redesign.** `viewBox="0 -16 68 84"` shows
it all but makes the mark 68 × 84, and an icon has to be square. Squaring it means
re-placing the circle, the plate and the quill inside the box — **which is composing
the mark again.**

**Rejected:** guessing at a new composition. Both figures are pinned in the test, so
whichever way you rule, one of them changes and it goes red.

### Whether a rasterising test harness is worth a package

**Ruling asked for:** whether to add `Avalonia.Headless.Skia` so tests can look at
pixels.

**Why it is worth asking:** this unit could not verify a single thing about how the
mark *looks*, and neither will the next one. **The same gap let three paragraphs
through on the text side** — a harness that composes but does not draw cannot see a
class of fault. Against that: a package is a licence, a supply chain and a slower test
run, and this is the first unit that has ever wanted one.

**Rejected:** adding it unilaterally. §0.4.

### Asks still outstanding

Carried forward per HM-DEC-139. **This order parks all of it**; listed so the queue
survives.

1. **Two issues of one work-instruction number.** Unit 271, and 252 before it. **The
   author's error.**
2. **`PM95` reads *southern Japan***, unit 271. **Not a defect.**
3. **`HM-OPEN-083` and `HM-OPEN-084`.** In `OPEN_ISSUES.md` by HM-DEC-140.
4. **Three pixels.** **Waiting on Tim.**
5. **`dt` and `hz` were never on the mine list.** **Waiting on Tim.**
6. **Where an outcome entry goes when the unit it corrects has none.** **Tim's.**
7. **`PHASE_OUTCOME.md` is written by a tool no unit is told to run.** **The author's.**
   Unit 284's order had no outcome task and did not append; this one did.
8. **Where the repeat fold stops**, unit 277.
9. **Whether a faded row needs a second carrier of its meaning**, unit 279.
10. **Whether counting subjects is a new assertion**, unit 279.
11. **Whether the fade is still obvious now the row is a bubble**, unit 280.
12. **`HM-OPEN-087`** — thirteen unreferenced `widget.*` templates. **Still Tim's.**
13. **Whether a 14-pixel hover ring is findable**, units 281–283. **Still unseen.**
14. **Whether `AboutWindow` is in scope for the terseness ruling**, unit 281.
15. **What `SenderHelp` is for**, unit 281. `HM-OPEN-088`. **Ten inherited reds.**
16. **Whether an order should name a behaviour rather than a file and line**, unit 283.
    **Answered in practice by unit 284**, which named a reproduction and found the
    thing in one unit.
17. **Whether three admissions on the simulated radio is right**, unit 284.
18. **The full mark's viewBox**, unit 285. **New.**
19. **The small mark's clipped quill**, unit 285. **New, and the more serious of the
    two.**
20. **Whether a rasterising test harness is worth a package**, unit 285. **New.**
