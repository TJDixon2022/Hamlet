# What this unit moved, and where it went — work instruction 284, task 4

The fifth of these, after units 280, 281, 282 and 283. **Removing words can remove
facts, and the only defence is a list somebody can read.**

**This unit moved one thing.** Five orders aimed at this paragraph and every one
missed, because each was told where the text was and each location was wrong. This
one was given a reproduction instead.

---

## The number

**The bar, in the state his screenshot shows** — simulated radio, Digital tab, 20 m,
14.074 MHz.

| | Chars |
|---|---:|
| The bar, before | **590** |
| The bar, after | **184** |
| The hover | **596** — every word, both kinds |

And the whole Digital tab in that same state, which is not the idle tab five units
measured:

| | Chars |
|---|---:|
| The tab, before | **1,873** |
| The tab, after | **1,467** |

**406 characters off the bar**, all of it advice, none of it deleted.

---

## The five clauses, and which kind each is

Composed by `ReceiverSetupVoice.Say` from the FT8 row of
`mode-receiver-conditions.json` plus the block's own width.

| Chars | Clause | Kind | Outcome |
|---:|---|---|---|
| 60 | *I could not read the noise blanker, so I have not touched it.* | **admission** | `NotRead` |
| 62 | *I could not read the noise reduction, so I have not touched it.* | **admission** | `NotRead` |
| 58 | *I could not read the auto notch, so I have not touched it.* | **admission** | `NotRead` |
| 222 | *The AGC usually wants to be slow here, because dozens of stations transmit together here and the gain would ride up and down under the loudest of them, and that is not settled well enough for me to change it on your radio.* | **advice** | `SpokenOnly` |
| 188 | *Your scope span wants to be 3 kHz across, because a scope showing a couple of hundred kilohertz draws the whole block about seven pixels wide, and that is one I cannot set from here.* | **advice** | `SpokenOnly` |

**180 characters of admission and 410 of advice, in one string.** That is why no
sweep could move it: moving the advice took the admissions with it, and clearing the
line hid a fault.

---

## Moved to hover, nothing lost

| Was | Chars | Now on screen | Where the words went |
|---|---:|---|---|
| *The AGC usually wants to be slow here, because…* | 222 | nothing | `StatusTipMark`, the status line's own hover |
| *Your scope span wants to be 3 kHz across, because…* | 188 | nothing | the same mark |

**Both are asserted by phrase on the hover**, so a moved sentence cannot become a
deleted one without a test going red.

---

## Kept in words, and why

| Kept | Why |
|---|---|
| All three *I could not read the …* clauses | **Hamlet tried and failed.** The operator is left not knowing where a control is, and a fault speaks unasked — unit 282's rule, untouched here |

---

## Where the line is drawn, and why there

**On whether Hamlet tried.**

- `NotConfirmed` and `NotRead` are **attempts that came back with nothing.** He is
  left not knowing where a control is, and only Hamlet can tell him.
- `SpokenOnly` leaves him knowing exactly where everything is. It names a change
  Hamlet **could** make and has **decided not to**, and says why. Hamlet did not try
  and fail; it chose not to try.

**One rule, in one place.** `ReceiverSetupVoice.Admissions` decides, `Say` still
composes all five kinds, and nothing downstream cuts a finished string up — a caller
that slices a sentence is the same fault one layer along.

---

## A fact removed

**None.** Nothing was deleted. Every clause is on the hover, admissions included,
because a hover that sometimes says nothing teaches somebody not to bother hovering.

---

## What the test does that five units' tests did not

**It stands the application up.** Connects the simulated radio through the view
model's own command, selects 20 m, sets the dial to 14.074, and drives the tune-in —
then asks the realized bar what it drew. Every earlier test handed a string to a
seam, and every one of them was green while this paragraph was on his screen.

**Two fixture faults were found on the way and are recorded rather than quietly
fixed**, because each made the bar say something else and each would have read as a
clean result:

- **The connect command is a toggle** and the panel starts connected, so pressing it
  disconnected. The bar read `Disconnected` and nothing was composed.
- **`OnFrequencyHzChanged` clamps the operator's own tuning to the band map on
  screen**, so setting 14.074 while the panel sat on 40 m landed on 40 m. Nothing was
  composed there either.

**And the assertion does not depend on today's wording.** Every advice clause joins
its reason with *because* and neither admission shape has one, so **the bar may not
contain the word at all**, and every sentence on it must open with one of the two
admission shapes. The 590-character bar recorded before the split carries *because*
twice.
