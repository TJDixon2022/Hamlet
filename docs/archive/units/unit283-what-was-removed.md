# What this unit moved, and where it went — work instruction 283, task 5

The fourth of these, after units 280, 281 and 282. **Removing words can remove
facts, and the only defence is a list somebody can read.**

---

## The number this unit is judged by

**The Digital tab, which is the one he operates from.**

| | Chars |
|---|---:|
| Before, byline excluded | **1,201** |
| After | **1,118** |
| The line that left it | **304**, on the branch his own radio takes |

The 83-character difference is the branch the idle fixture happened to be showing.
**The 304-character branch is the one he has been reading**, and it is gone from the
strip on every tab, not only this one.

| Tab | Before | After |
|---|---:|---:|
| CW | 611 | 528 |
| Digital | 1,201 | 1,118 |
| Voice | 601 | 518 |

---

## The paragraph was never on the status bar

**Units 280, 281 and 282 all swept that surface. This is not on it.**

`LinkCheckLine` is drawn at `MainWindow.axaml:2530`, in the **top strip**, under the
frequency readout — shared chrome, above all three workspaces, permanently visible on
every tab. Units 280 and 281 measured it and printed it; neither recognised it,
because it was reading 83 characters in the fixture and 304 in his shack.

**Five branches, measured through `LinkSelfCheck.Describe` rather than quoted:**

| Chars | Branch | Now |
|---:|---|---|
| **304** | the radio does not announce its own changes | **on the mark** |
| 102 | the radio announces and the frequency is fresh | on the mark |
| 83 | Hamlet is keeping up by asking | on the mark |
| ~130 | the frequency on screen is stale | **speaks**, trimmed to its first sentence |
| 94 | nothing has been heard from the radio yet | **speaks** |

**304 is the branch his own radio takes.** `CivTransceive` is off, and HM-DEC-138
measured 5,499 frames in sixty-one seconds on that radio with `inboundTransceive`
zero.

---

## Moved to hover, nothing lost

| Was | Chars | Now on screen | Where the words went |
|---|---:|---|---|
| *Your radio is not announcing its own changes, so Hamlet asks it where it is several times a second instead. That keeps the screen honest and it costs a little of the cable. Turning CI-V Transceive on at the radio would let it simply say so, which is quicker and quieter, and it is your setting to change.* | 304 | nothing | `LinkCheckMark`, a Measurement mark in the strip |
| *Your radio tells Hamlet the moment you touch the dial, so what you see here is where you actually are.* | 102 | nothing | the same mark |
| *Hamlet is keeping up with your radio, asking it where it is several times a second.* | 83 | nothing | the same mark |

**The advice about CI-V Transceive is asserted by phrase in the test**, because it is
the one clause of the 304 that tells him something he could act on, and losing it
would be losing a fact rather than a sentence.

---

## Kept in words, and why

| Kept | Why |
|---|---|
| *The frequency on screen is about a minute old, so treat it as where the radio was rather than where it is.* | **The reason this class exists.** He turned the dial, Hamlet followed thirty seconds later, and the number was drawn confidently four times a second while being a minute old |
| *Hamlet has not heard where the radio is yet, so the frequency is blank rather than guessed at.* | The display is blank about the one number every other surface trusts, and saying so is §0.0 |

**The stale branch is trimmed, not moved.** Its last sentence — *Hamlet is still
asking* — is narration and goes to the mark; the part that says the number is old
stays on the screen. **`Concern` is decided from the same inputs in the same place**
as `Headline`, not cut out of it by a caller, so what he hovers and what he is shown
cannot come to disagree (§0) — the shape `ReceiverSetupVoice.Admissions` already uses.

---

## A fact removed

**None.** Nothing was deleted. Every branch is behind the mark, including the two that
also speak, because a hover that sometimes says nothing teaches somebody not to
bother hovering.

---

## The application, measured again

Same harness, same settled fixture as unit 282, **so these two totals are
comparable** — which unit 282's and unit 281's were not.

| Screen | Unit 282 | Now |
|---|---:|---:|
| SettingsWindow | 1,628 | 1,628 |
| RigDiagnosticsWindow | 1,346 | 1,346 |
| MainWindow — Digital tab | 1,201 | **1,118** |
| AboutWindow | 726 | 726 |
| LogContactWindow | 716 | 716 |
| MainWindow — CW tab | 611 | **528** |
| MainWindow — Voice tab | 601 | **518** |
| FavoritesWindow | 280 | 280 |
| ContactLogWindow | 246 | 246 |
| DecisionLogWindow | 196 | 196 |
| **TOTAL** | **7,551** | **7,302** |

**249 characters off the application**, all of it from one line drawn three times.

**And the Digital tab is now capped in two states**, at rest and working, because a
ceiling caps a state and not only a surface. The working state measures **1,087** —
*less* than the idle 1,118 — because the empty-state explanations go away once there
is traffic and everything composed at run time is now on a hover.

---

## What the sweep looked at, and what it cannot see

**533 strings across eleven surfaces**; thirteen are a hundred characters or more.
**One is of the shape this phase keeps finding**: `DigitalReadiness` composes its
219-character training-radio line across concatenated fragments, so a whole-phrase
search returns nothing. **It is a boundary statement and correctly visible** —
nothing off the air can reach the decoder — and it shows only on the training radio.

**What no sweep here can see is a state the fixture never enters.** It draws no send
refusal, no licence refusal, no scan, no capture refusal, no receive offer, and
nothing from the receive-help panel HM-OPEN-087 leaves unreachable. **That is the same
limit that let three paragraphs through**, and naming it is more use than another
clean bill of health.
