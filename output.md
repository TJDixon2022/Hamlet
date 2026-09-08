# Work instruction 283 — every mode's status bar, not the one the author named

```
UNIT:      283
TASKS:     6 of 6, none dropped
NUMBER:    the Digital tab, 1,201 -> 1,118 characters
           (the line that left it is 304 on the branch his radio takes)
ADVANCED:  no
DRIFT:     6 consecutive units without advance, carried from unit 282
VERSION:   1.12.189 -> 1.12.195
BRANCH:    main, pushed
```

---

## 1. What Claude did

**Surface: Claude Code, on the development computer, on `main`.** The prompt claimed
`PROJECT: Hamlet` and the tree confirmed it — `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` present, `CoreHMI.sln` and
`MURC.sln` absent, `Hamlet.sln` at the root. **Nothing in this report is evidence
about the radio**: there is no rig and no antenna here.

**Nothing was recorded under §12.1.** No shell refusals: every script went through a
file from the start.

### Task 1 — the count, and the instruction's premise

**There is one path that composes receiver-conditions narration onto the status bar,
not one per mode.** The bar is shared chrome: measured on all three tabs, **one
`StatusBarText`, none of them inside a workspace grid.** So unit 282's change cleared
it for every mode.

**Proved on the tab this order is about**: an FT8 tune-in composes 448 characters and
the Digital tab's bar draws **none** of them, with all 448 on the mark. An FT8
admission still speaks — *I could not read the noise reduction, so I have not touched
it.*

**The shipped rows confirm as stated**: CW nine rows, 884 characters; FT8 four rows,
448. **FT4 is a fourth mode the order does not mention**, aliased to FT8's rows, and
it composes 448 as well.

**And the paragraph he is pointing at is somewhere else entirely.**

`LinkCheckLine` renders at `MainWindow.axaml:2530`, in the **top strip** under the
frequency readout — shared chrome, permanently visible, on every tab. **Its longest
branch is 304 characters**, longer than anything unit 282 moved, **and it is the
branch his own radio takes**: `CivTransceive` is off, and HM-DEC-138 measured 5,499
frames in sixty-one seconds with `inboundTransceive` zero.

**Units 280 and 281 both measured that line and printed it, and neither recognised
it** — because it reads 83 characters in a headless fixture and 304 in his shack.
That is the same failure in a new place: the instrument was right, and nobody had put
it in the state where the fault exists.

Nothing was fixed in this task.

### Task 2 — the top strip, on every tab

`LinkCheck` gains a `Concern` beside its `Headline`, **decided from the same inputs in
the same place** rather than the headline being cut up by a caller — the shape
`ReceiverSetupVoice.Admissions` already uses, so what he hovers and what he is shown
cannot disagree.

**Three branches go to the mark, two still speak.** A stale frequency and a frequency
nobody has heard are the two things this class was built to say out loud; going unsaid
cost two builds. **The stale branch is trimmed, not moved**: its closing *Hamlet is
still asking* is narration and hovers, the part saying the number is old stays.

**Nothing is deleted.** The advice about turning CI-V Transceive on is asserted by
phrase, because it is the one clause of the 304 he could act on.

Measured after: **every tab is 83 characters lighter** — CW 611→528, Digital
1,201→1,118, Voice 601→518.

### Task 3 — eleven ceilings, and one of them is a state

**Every window already had one** — eight windows, the main one as three tabs — so no
surface was uncapped. **The gap was elsewhere: a ceiling caps a state, not just a
surface.** Unit 282's composed send line is 473 characters and appears only after a
transmission; this unit's link-check branch is 304 and appears only on a radio that
does not announce. **An idle window sees neither.**

So the Digital tab is capped **twice** — at rest and working, with traffic on both
lists, a sent message, the link check his radio produces, and a tune-in's narration.

**The working tab measures 1,087 against the idle 1,118.** It says *less*, which is
the shape of the whole phase. **The figure was guessed at 1,424 when the row was
written and the measurement corrected it downward**; the guess is not what shipped.

Same margin and reasoning as unit 282 — measured + 100, rounded up to the next 50,
asserted as arithmetic. The red is now demonstrated on the Digital tab: 400 characters
takes it 1,118 → 1,518 against 1,250.

### Task 4 — the shape, swept for once more

**533 strings across eleven surfaces; thirteen are ≥100 characters**, each checked
against the source.

**One is of the shape.** `DigitalReadiness` composes its 219-character training-radio
line across concatenated fragments, so a whole-phrase search returns nothing.
**It is a boundary statement and correctly visible** — nothing off the air can reach
the decoder — and it shows only on the training radio. Nothing to move.

The other twelve are literals findable by phrase; eight explain an absence, two are on
parked surfaces.

**What no sweep here can see, said plainly**: a state the fixture never enters — no
send refusal, no licence refusal, no scan, no capture refusal, no receive offer,
nothing from the panel HM-OPEN-087 leaves unreachable. **That is the same limit that
let three paragraphs through.**

**One observation, printed with its uncertainty and not fixed**: the working tab draws
*nothing on this frequency yet* while two rows are on the lists. It is **not** the
mine list's empty state — that is correctly hidden — it is `DigitalModeStripStatus`, a
third element carrying the same sentence. The rows go in through a test seam that may
not feed what the strip reads.

### Task 5 — what moved, listed and measured

`docs/unit283-what-was-removed.md`. **No fact removed**; nothing deleted.

### Task 6 — the outcome entry

Appended by `tools\arbiter\outcome-append.bat`, exit 0, as `UNIT 283 - STEP E`. Units
273, 274 and 275 were not back-filled.

---

## 2. What Tim should expect

**The paragraph is off the screen on the tab you actually use — and it was never on
the status bar.**

It sat in the **top strip, under the frequency readout**, on every tab, and it said
your radio is not announcing its own changes so Hamlet asks instead, and that turning
CI-V Transceive on would be quicker and quieter. Three hundred and four characters,
every time you looked at the window. **Hover the small ring beside where it was and
the whole thing is there**, the CI-V advice included.

**Except when something is wrong.** If the frequency on screen has gone stale, or
Hamlet has not heard where the radio is at all, **that still says so without being
hovered** — those are the two things that line was built for.

### What will look wrong and is not

- **An empty space under the frequency readout is the normal state now.** The small
  ring is the hover.
- **On the training radio nothing changes** in the strip, because that branch never
  applied there.
- **Settings, About and the rig diagnostics are unchanged.** Parked, capped, not
  swept.

### The build and the tests

Build clean, no warnings. **This unit ran no suite** (HM-DEC-155): only the tests it
wrote or rewrote, filtered by name, foregrounded.

| Class | Result |
|---|---|
| `WhichPathsPutNarrationOnTheBarTests` | 5 of 5 — new |
| `TheTopStripStopsLecturingTests` | 4 of 4 — new |
| `WhatElseIsComposedAtRuntimeTests` | 1 of 1 — new |
| `HowMuchTheApplicationSaysTests` | 5 of 5 — ceilings re-set, working state added |
| `TheStatusBarStopsLecturingTests` | 4 of 4 |
| `TheLogShowsBothGridsTests` | 3 of 3 |
| `TheLogDoesNotClipItsColumnsTests` | 4 of 4 |
| `WhyTheReportsAreEmptyTests` | 4 of 4 |
| `TheOfferButtonCannotBePressedTests` | 2 of 2 |
| `TheGridInSettingsReachesEveryRowTests` | 5 of 5 |
| **Total** | **38 of 38** |

The inherited reds were not touched: `HM-OPEN-088`'s ten in
`TheMessageReadsAsThreePartsTests`, the CW set, the `Ft8Sharp.Deep` tripwire.

**Six commits, all on `main`, all pushed**, 1.12.189 → 1.12.195. Nothing uncommitted.

---

## 3. What we should do next

### How many paths, which mode, and which unit 282 cleared

| Path | Surface | Mode | Chars | Cleared by |
|---|---|---|---:|---|
| `MainWindowViewModel` receiver-conditions narration | status bar | **every mode** — one shared bar | CW 884, FT8 448, FT4 448 | **unit 282** |
| `MainWindowViewModel` mode-follow narration | status bar | every mode | varies | **unit 282** |
| **`LinkCheckLine`** | **top strip** | **every tab, permanently** | **304** on his radio | **not cleared — this unit** |

**One composition path onto the bar, not one per mode.** The instruction expected one
per tab; the bar is shared chrome and unit 282's fix was already app-wide.

### The Digital tab as it now renders, and the paragraph from its hover

The strip under the readout draws **nothing**. Hovering the ring gives, verbatim:

> measurement — Your radio is not announcing its own changes, so Hamlet asks it where
> it is several times a second instead. That keeps the screen honest and it costs a
> little of the cable. Turning CI-V Transceive on at the radio would let it simply say
> so, which is quicker and quieter, and it is your setting to change.

And where the frequency has gone stale, this stays **on** the strip:

> The frequency on screen is about a minute old, so treat it as where the radio was
> rather than where it is.

### The ceilings, per surface

| Surface | Holds | Ceiling |
|---|---:|---:|
| SettingsWindow | 1,628 | 1,750 |
| RigDiagnosticsWindow | 1,346 | 1,450 |
| MainWindow — Digital tab | 1,118 | 1,250 |
| MainWindow — Digital tab, **working** | 1,087 | 1,200 |
| AboutWindow | 726 | 850 |
| LogContactWindow | 716 | 850 |
| MainWindow — CW tab | 528 | 650 |
| MainWindow — Voice tab | 518 | 650 |
| FavoritesWindow | 280 | 400 |
| ContactLogWindow | 246 | 350 |
| DecisionLogWindow | 196 | 300 |

**No surface is without one.** What is without one is a **state**: everything the
fixture cannot enter, named in task 4.

### What task 4 found, and what it looked at

**533 strings, eleven surfaces, thirteen at or over a hundred characters.** One of the
shape — `DigitalReadiness`'s 219-character line, correctly visible and only on the
training radio. Twelve findable by phrase. **The limit is stated rather than implied.**

### Then

1. **Rule on HM-OPEN-087** — the thirteen dead widget templates.
2. **Look at a hover ring on a real screen.** Asked by units 281, 282 and now 283;
   still unanswered, and it now carries the receive narration *and* the link check.
3. **Rule on HM-OPEN-088.**
4. **Then back to the radio.** Every bench step is closed.

---

## 4. What's blocking us

Nothing blocks the next unit.

### Whether the hover ring is findable

**Ruling asked for:** whether a 14-pixel ring is enough to tell him there is something
to hover. **Third unit asking.**

**Why:** it now carries the receive-setup narration *and* the link check — two of the
most useful things Hamlet says about his radio. **Nobody has looked at one on a
screen.** If it is too quiet to find, a moved sentence is a deleted one in practice
and every test still passes.

### How an order should name a surface

**Ruling asked for:** whether a work instruction should name a *behaviour and a
measurement* rather than a file and line.

**Why:** this order says so itself, and then repeated the error one level up — it
named the status bar, and the paragraph was in the top strip. **Unit 282 was given one
line and found two; unit 283 was given a surface and found a different one.** Three
units have now fixed a real instance and missed the next, and each time the miss was
where the order pointed.

**Rejected:** treating it as this session's to fix. It is how orders are written.

### Asks still outstanding

Carried forward per HM-DEC-139, with this order's numbering.

1. **Two issues of one work-instruction number.** Unit 271, and 252 before it.
   **The author's error.** No unit action.
2. **`PM95` reads *southern Japan***, unit 271. **Not a defect.**
3. **`HM-OPEN-083` and `HM-OPEN-084`.** In `OPEN_ISSUES.md` by HM-DEC-140.
4. **Three pixels.** **Waiting on Tim.**
5. **`dt` and `hz` were never on the mine list.** **Waiting on Tim.**
6. **Where an outcome entry goes when the unit it corrects has none.** **Tim's.**
7. **`PHASE_OUTCOME.md` is written by a tool no unit is told to run.** **The
   author's.** Units 281, 282 and 283 were each told to run it and did.
8. **Where the repeat fold stops**, unit 277. **Still open.**
9. **Whether a faded row needs a second carrier of its meaning**, unit 279. **Tim's.**
10. **Whether counting subjects is a new assertion**, unit 279. **Tim's.**
11. **Whether the fade is still obvious now the row is a bubble**, unit 280.
12. **`HM-OPEN-087`** — thirteen unreferenced `widget.*` templates. Unit 282 disabled
    the button pointing at them; unit 283 touched no template. **Still Tim's.**
13. **Whether a 14-pixel hover ring is findable**, units 281, 282 and **283**. **Not
    seen by anybody on a real screen**, and it now carries the receive narration and
    the link check.
14. **Whether `AboutWindow` is in scope for the terseness ruling**, unit 281. Capped,
    not swept.
15. **What `SenderHelp` is for**, unit 281. `HM-OPEN-088`. **Ten inherited reds hang
    on it.**
16. **Whether an order should name a behaviour rather than a file and line**, unit
    283. **Three units have now missed the next instance at the place the order
    pointed.**
