UNIT: 039 — stopped at task 4 of 7 — 2026-08-28 13:29

## 1. What Claude did

**Stopped. Tasks 1, 2, 3 and 4 are done. Tasks 5, 6 and 7 were not started, and
that was my sizing call rather than a blocker.**

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed all four checks — `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does. **Hamlet confirmed.** Branch `main`
throughout, four commits, all pushed, none refused. Version 1.12.1 to 1.12.2.

**Nothing here is evidence about the radio.** No rig was connected. **Nothing
transmitted and no rig setting was written.**

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

**Why it stopped where it did.** Task 5 wires the capture press "exactly the way
the CW one does", and the CW press turns out to be entangled with
`CwCaseRoster` — a roster whose whole purpose is scoring the CW decoder, and
whose every row asserts the operator heard a station. **Reusing it from the
Digital tab would put digital presses in the CW score**, and separating them is a
change inside the CW capture path, which this order forbids touching. That is a
real question rather than a five-minute job, and it is in section 4.

**Tasks 6 and 7 follow task 5's audio path**, and I judged it better to stop with
the render fault fixed and measured than to start a slot cutter I could not
finish carefully. **The order named task 7 as its drop, so dropping 5 and 6 as
well is a sizing decision the owner did not make**, and §8 requires it reported
as one. This is it.

## 2. What the owner should expect

**The waterfall was drawing the shape of the radio's own filter, not the band.**

The floor it measured brightness against was **the twenty-fifth percentile across
the whole 200–3000 Hz display span**. With `FIL2` selected, most of that span is
outside what the radio passes — so the floor sat in the stopband, tens of
decibels below the noise *inside* the passband, and everything in band saturated.
**The hard vertical edge was the filter skirt.** It was a real feature of the
radio, drawn as if it were a signal.

**It now measures each bin against its own recent quiet level**, which removes
the filter shape from the picture entirely. A bin is bright because it is louder
than it usually is, not because it sits where the receiver happens to pass.

**Measured on his own recordings, before and after:**

| capture | saturated before | saturated after |
|---|---|---|
| `cw-2026-08-20-014854`, **holds nothing** | **12.3 %** | **0.0 %** |
| `cw-2026-08-17-013347`, holds `VA3VRR` | 9.8 % | 1.2 % |
| `cw-2026-08-28-004844`, reads a bulletin | 16.1 % | 4.3 % |

**The capture press still does nothing**, and that is now the only lying control
on a tab with live values on it. Section 4 says what stands in the way.

**The clock now actually asks.** It was displaying `clock not checked yet` because
unit 038 built the display and nothing ever queried. It queries at startup and
every ten minutes.

| | before | after |
|---|---|---|
| engine | 28 failing, not measured since 038 | **29 of 1914 — see below** |
| app | 509 of 509 | **509 of 509** |

## 3. What you should see

### The engine run 038 owed, and its one surprise

**1914 tests, 1885 passed, 29 failed, 22 minutes.** That is **one more than the
stable 28**, and the order says report it before anything else.

The extra is **`AConfirmedModeWriteFoldsTheDataVariantTooAsync`**, which **passes
three runs out of three in isolation**. It is a CI-V mode-write test; unit 038
added four audio files and touched nothing on that path. **It is one of the seven
known intermittents, not a regression.**

### The render fault, reproduced before anything was changed

The order requires the fault to reproduce before task 3 changes a line, and it
did — **on real radio audio, not synthesised**:

> `cw-2026-08-20-014854` **holds no station** and drew **12.3 % of its bins
> saturated**, with neighbouring bins stepping **226 of 255**. That is band noise
> painted as signal.

### The fix, and the second statistic it needed

**The floor is per bin and tracked over time.** It falls fast — a bin that goes
quiet is noise and should be followed at once — and rises very slowly, so a
station cannot chase its own floor up and erase itself. At about twelve frames a
second the rise constant is minutes, and a fifteen-second FT8 transmission stays
bright for all of it.

**That alone was not enough and the measurement said so.** A floor that follows
the level down quickly settles near the *minimum* of the noise, not its typical
level, so noise sat several decibels above its own floor and the picture read
mid-grey everywhere — **only 15 % of an empty recording was dark.** So a second
slow average holds how far each bin usually sits above its own floor, and
subtracting it puts the display zero at the noise's ordinary level. **Dark now
means indistinguishable from this bin's own noise**, and neither number is tuned
to make the picture look better — both come from the audio.

**After: 0.0 % saturated on the empty recording, median byte 74 of 255.**

### Two of my own test choices were wrong, and are corrected rather than moved

**"The share of bins under 40 of 255" was a threshold I invented.** It is not a
fact about a picture, and moving it to pass would be fitting the test to the
answer. **The assertion is the median byte now** — where the whole picture sits.

**And the tone test used a perfectly constant tone.** A per-bin floor is *meant*
to absorb something perfectly steady; that is what removes the filter shape. So
the test was asking the new design to fail at its own purpose. **The tone now
arrives partway through, the way a real signal does, and lands at 1500 Hz.**

**This is a real property and it is stated rather than buried**: a dead-constant
carrier will fade from this waterfall. Anything that starts, stops or fades
stands out — and FT8 keys every fifteen seconds, so FT8 shows.

**Determinism across chunk sizes 4096 and 997 is intact**, unit 038's property
and not traded.

### The second defect, and what it does to the CW picture

The digital waterfall never bound `FrequencyHz`, so `Render` drew its tuning
marker at the property default of **7.03 MHz** — thousands of widths off the
right edge of a 200–3000 Hz band. It is RF hertz on an audio spectrum.

**The marker is now drawn only when it falls inside the band. The CW picture is
unchanged, and that is checked rather than assumed**: there the band edges come
from the selected band and the frequency from the dial, so the marker is inside
the span whenever the radio is tuned within the band it is showing — which is the
only case in which it was ever visible.

### The clock

**At startup and every ten minutes**, and the interval is reasoned. A PC clock
the operating system already disciplines does not wander measurably in ten
minutes, and the answer is only needed to place fifteen-second boundaries. **More
often is a request a minute to somebody else's volunteer-run pool** for a number
that has not changed (HM-DEC-024); less often means an evening runs on one
reading taken before the radio warmed up.

Off the UI thread, its own timeout, and every failure returns unknown rather than
zero. **A failure does not erase an earlier good reading** — a network that drops
for one poll has not made the last measurement untrue, only older, and the age is
already on the strip.

### Where the order and the tree differ

- **Everything the order carried from 038's report was accurate**, including the
  transform figures, the multicast tap, and the noise-floor description.
- **The engine baseline was 28 and is 29**, above.
- **The CW capture press is not separable as the order assumes**, below.

## 4. What's blocking us

**The capture press cannot be wired the CW way without putting digital presses
into the CW decoder's score.**

Ruling asked for:

> **The digital capture press gets its own record, or the CW roster gains a
> column saying which tab a press came from.** The CW press does not only write a
> WAV and a sidecar: it calls `MarkCase`, which appends to `CwCaseRoster` — and
> that roster exists to score the CW decoder, with every row asserting the
> operator heard a station there.
>
> **A digital press routed through it would be counted as a CW case Hamlet failed
> to read**, which corrupts the one measurement the CW work is judged on. **And
> separating them means changing the CW capture path**, which this order forbids
> touching and which unit 036's residue makes a bad thing to disturb.
>
> **What was rejected:** wiring the press to write only the WAV and sidecar and
> skipping the roster — that is not "exactly the way the CW one does", and the
> difference would be invisible in the code and load-bearing in the score.

**This is the whole of what stopped task 5**, and tasks 6 and 7 sit behind it.

---

**Tasks 6 and 7 were dropped and only 7 was the named drop.**

Task 6, the slot cutter, is not blocked by anything — it is pure arithmetic over
`Ft8Slots`, which is built and tested. **I stopped rather than start it with
enough budget left to finish it carefully**, which is a sizing decision you did
not make. **It is a short unit on its own.**

---

**A dead-constant carrier fades from the digital waterfall.**

Not a fault and not a question, but it should be on the record before somebody
reports it as one. It is the cost of removing the receiver's filter shape per
bin, and FT8 keys every fifteen seconds so FT8 is unaffected.

### Asks still outstanding

**Carried forward per HM-DEC-139 and HM-DEC-140, and deliberately not restated.**
The order parks the whole CW stream and the carried asks, and says *both halves
are required*. **The thirty-one asks from unit 1.11.34's list stand unchanged**,
none touched by this unit.

**Carried from unit 038, still open:**

1. **`ft8_lib` cannot be built here** — no C toolchain — and the wrap-or-write
   choice. **This order rules it: the decoder is written in C#.** The closing
   text for HM-OPEN-004 is in unit 038's report.

**New this unit:**

2. **The capture press and the CW roster**, above — the one that stopped task 5.
3. **Tasks 6 and 7 dropped beyond the named drop**, above.
4. **A constant carrier fades from the digital waterfall**, above.

**Closed this unit:** the engine run 038 owed — 29 of 1914, the extra a CI-V
intermittent that passes alone. **Why the waterfall did not match the radio** —
a floor measured across the display span instead of per bin, so the receiver's
filter shape saturated the picture and its skirt read as a seam. **Whether the
clock ever asked** — it did not, and now it does.
