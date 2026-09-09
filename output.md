# Work instruction 286 — the ring counts down, the badge announces itself, and Hamlet gets a face

```
UNIT:      286
TASKS:     6 of 6, none dropped
NUMBER:    the seconds the ring shows with the turn unknown
           before: none, a bare ?
           after:  the seconds to the next boundary, 1 to 15
                   (7 s into a slot with nothing heard, it reads 8)
ADVANCED:  no
DRIFT:     9 consecutive units without advance, carried from unit 285
VERSION:   1.12.205 -> 1.12.210
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

### Task 1 — the ring counts down

**The engine never suppressed it.** `Ft8Turn.Read` has returned the seconds for the
no-station state since unit 277 wrote it; **the number was thrown away one layer up**,
by a view-model condition that named the state instead of asking whether a count
existed. `TurnRingCount` now reads the count, so **a question mark means there is no
count rather than no turn.**

**Unit 277's rule is untouched.** No parity is guessed, `TurnRingIsUnknown` still reads
true, the ring is still dashed, and the four states still separate without colour: **the
dash says the turn is unknown and the number says the clock is not.** Only `NoClock`
keeps the `?`, because with no measured offset there is no boundary to count to.

**One change was made and reverted.** Reading *always* literally, I extended the count
to the `Stopped` state as well. That collided with unit 277's
`StoppedEarlySaysSoRatherThanPretendingItRan`, which states there is nothing left
running to count. **Reverted and raised for a ruling** rather than pushed through.

### Task 2 — the badge announces itself

`BadgeAward` is a record the view model raises; `BadgeWindow` shows it. **That split is
what makes *what he is told and when* provable without opening a window**, and it means
nothing about a badge can reach the send path — an event cannot ask a subscriber to
transmit.

**It cannot cost him a contact, and that is one property with one mechanism.**
`ShowActivated="False"`, so the window never activates — **and a window that activates
closes an open right-click menu**, which is exactly how a dialog would cost him the
reply it is congratulating him for. `Show` not `ShowDialog`. Not in the taskbar. Gone in
eight seconds on its own, because a notice he has to dismiss is a click he did not ask
for at the moment he can least afford one. **Asserted on the window**, because a default
is easy to lose.

**Everything unit 278 built is untouched and re-asserted**: once per rank, every rank a
jump passes, and nothing on a first look at a log Hamlet was not there for.

### Task 3 — the new marks

**`SvgMark` refused the full mark**, exactly as the order predicted:

```
System.NotSupportedException : the mark uses <text>, which this loader does not
handle. It is deliberately narrow: an element it drew wrongly would be worse
than one it refused.
```

**Extended narrowly rather than the mark redrawn** — the mark is approved and the loader
is not. Text becomes glyph outlines, so the letters scale with everything else and the
mark stays one object. SVG puts the baseline at `y` and `FormattedText` puts the top
there, so the baseline distance is subtracted; without it every label sits a line low.

**Both defects are gone: 0 outside on every side.** The full mark's ink is
**18, 5.75 → 358.25, 348** in a 380 × 360 box, confirming the instruction's figure to a
quarter unit; the small mark fills its 64 × 64 exactly.

**The viewBox assertions are turned the right way round rather than deleted.** What was
worth keeping was never 18.5 and 15.5 — it was the property that a mark stays inside the
box it declares.

**And one stale fact was caught.** Unit 285's account of what survives at 16 px described
a 68-unit drawing with 3.4, 4.0 and 3.0 strokes. That drawing is gone, and the test
**would have gone on passing while describing something no longer in the tree.**

### Tasks 4 and 5 — measured, and listed

`docs/unit286-what-changed.md`. **No fact left a screen.**

### Task 6 — the outcome entry

Appended by `tools\arbiter\outcome-append.bat`, exit 0, as `UNIT 286 - STEP E`.

---

## 2. What Tim should expect

**The clock is there whether or not anybody has spoken.** Open the Digital tab with an
empty For you panel and the ring drains with a number in it — the seconds to the next
`:00`, `:15`, `:30` or `:45`. The ring is still dashed, because **Hamlet still does not
know whose slot it is and is not going to guess.** Hover it and it says so, and then
tells you the count: *…no turn to work out. The next slot starts in 8 seconds.*

**Your tenth contact says so.** A small notice appears at the bottom-right of the
window: **yellow belt**, *That is 10 contacts logged.*, *15 more and the ring turns
orange.* **It takes nothing from you** — no focus, no click, and it will not close a
right-click menu you have open. It leaves after eight seconds, or sooner if you click
it.

**The taskbar is not ugly.** The rust tile with the faceplate and the quill, and the
full transceiver in About at about 170 px.

### What will look wrong and is not

- **The ring is dashed while showing a number.** Deliberate: the dash is the turn being
  unknown, the number is the clock being known.
- **At 16 px the quill's inner detail is gone.** The whip survives at 1.38 px; the spine
  and barbs are sub-pixel. Computed, not seen.
- **A badge will not fire on a fresh install** however many contacts your log holds.
  Unit 278's rule, untouched.

### The build and the tests

Build clean, no warnings. **This unit ran no suite** (HM-DEC-155): only the tests it
wrote or rewrote, plus the neighbours its changes could disturb.

| Class | Result |
|---|---|
| `TheTurnIsARingTests` | 9 of 9 — three new, one replaced |
| `TheBadgeAnnouncesItselfTests` | 6 of 6 — new |
| `TheMarksRenderTests` | 6 of 6 — three assertions re-taken |
| `TheBeatIsDerivedNotGuessedTests` (engine) | 13 of 13 |
| `BothHalvesOfTheConversationTests` | 18 of 18 |
| `TheCountWearsARankTests` | 5 of 5 |
| `BindingHealthTests` | 1 of 1 |
| `HowMuchTheApplicationSaysTests` ceiling | 1 of 1 |
| **Total** | **46 of 46** in the app run, plus 13 in the engine |

Inherited reds untouched: `HM-OPEN-088`'s ten, the CW set, the `Ft8Sharp.Deep`
tripwire.

**Six commits, all on `main`, all pushed**, 1.12.205 → 1.12.210. Nothing uncommitted.

---

## 3. What we should do next

### The ring with the turn unknown

7 s into a slot, nothing heard from anybody, a measured offset:

```
no station : count "8"  unknown True  sweep 192°
no clock   : count "?"  unknown True
```

**A count and still unknown.** And the count is the corrected clock, not the machine's —
the same PC moment under a 4-second offset reads **4** instead of **8**.

The hover:

> Nothing has been heard on this frequency yet, so there is nobody to take a turn with
> and no turn to work out. The next slot starts in 8 seconds.

### The badge dialog at 10, and at 9 → 26

**At 10:**

> **yellow belt**
> That is 10 contacts logged.
> 15 more and the ring turns orange.

**At 0 → 26**, one notice naming both: *That is 10 and 25 contacts logged.*, headed
**orange belt**. Not only 25.

**At 10 again, and at 11:** nothing. **At a fresh log of 40 on first look:** nothing —
and then 50 does fire, so seeding silences the past and not the future.

### What `SvgMark` refused, and what was done

**`<text>`**, with the exception quoted in §1. **The loader was extended, not the mark
redrawn.** Seven of the full mark's 35 shapes are text and now draw as glyph outlines:
`USB-D`, `FIL1`, `RX`, `UTC`, `7.074`, `.0` and the S-meter's `S`.

### What was verified by looking

**Nothing.** The limit was re-checked, not assumed: the headless drawing backend
composes a visual tree and rasterises nothing, and `CopyPixels` still throws *“CopyPixels
is not supported for this bitmap type”*. Seven things were measured — shape counts, ink
against viewBox, the realized 170 × 161 in About, the 256 × 256 icon — and **four are
listed as not verified**: the taskbar, the title-bar icon at 16 px, whether the About
layout looks balanced, and **whether the display reads as a transceiver and the quill as
a quill**, which is the whole point of the mark.

### Then

1. **Run it and look.** The ring, the mark in About, the taskbar. Everything about
   appearance here is geometry.
2. **Make your tenth contact** and see whether the notice lands where it should and
   leaves when it should.
3. **Rule on the `Stopped` ring** (§4).

---

## 4. What's blocking us

Nothing blocks the next unit.

### Whether the ring should count down after a stop

**Ruling asked for:** you ruled the ring *always* shows the seconds to the next slot
boundary. Unit 277 ruled the `Stopped` state shows no count, with a test that says so in
as many words: *there is nothing left running to count.*

**Why they collide:** those two rules were about different quantities. Unit 277 meant
the stopped **transmission** has nothing left to count. The next **slot boundary** is
still fifteen seconds away and still knowable, and that is what you asked for
everywhere else.

**What I did:** extended the count to `Stopped`, watched unit 277's test go red, and
**reverted it.** Your ruling named the empty-panel case and its test criterion names
that case; overruling a deliberate prior ruling on my reading of one word is not a
session's call.

**Rejected:** changing unit 277's test to match. That is deciding the question by
editing the thing that objected.

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
8. **Where the repeat fold stops**, unit 277.
9. **Whether a faded row needs a second carrier of its meaning**, unit 279.
10. **Whether counting subjects is a new assertion**, unit 279.
11. **Whether the fade is still obvious now the row is a bubble**, unit 280.
12. **`HM-OPEN-087`** — thirteen unreferenced `widget.*` templates. **Still Tim's.**
13. **Whether a 14-pixel hover ring is findable**, units 281–283. **Still unseen.**
14. **Whether `AboutWindow` is in scope for the terseness ruling**, unit 281.
15. **What `SenderHelp` is for**, unit 281. `HM-OPEN-088`. **Ten inherited reds.**
16. **Whether an order should name a behaviour rather than a file and line**, unit 283.
    **Answered in practice by unit 284.**
17. **Whether three admissions on the simulated radio is right**, unit 284.
18. **The full mark's viewBox**, unit 285. **Closed by this unit** — the redrawn mark
    fits.
19. **The small mark's clipped quill**, unit 285. **Closed by this unit** — the redrawn
    mark fits.
20. **Whether a rasterising test harness is worth a package**, unit 285. **Still open,
    and this unit is the second that could verify nothing about appearance.**
21. **Whether the ring should count down after a stop**, unit 286. **New.**
