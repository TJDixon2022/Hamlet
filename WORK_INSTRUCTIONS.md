# WORK_INSTRUCTIONS.md

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      Hamlet.sln
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwGate.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  src\CoreHMI

These four files are fixed. Do not substitute a different file for any of
them and do not report a check against a file this list does not name.

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## Why this unit exists

**The clock is wrong on every station Tim heard tonight, and the app is already
measuring how wrong.**

A dah is three dits. That is the definition. Five captures, 20 metres, the evening
of the 21st, all of them stations he could hear clearly:

| capture | `clockFit` dah in dits | `decoderWpm` | what the terminal showed |
|---|---|---|---|
| `195617` | **15.72** | not proved, rolling 50 | `T E E E E E TTON KT M 5O T E E E E E EE` |
| `195742` | **9.82** | withdrawn, rolling 31 | `E E E TEST GE N5O T E E E E   E E` |
| `200036` | **4.68** | 16 | `E E E E E E E EIE EH E EI I H EE E H E EE  WMTOE ET` |
| `200134` | **6.40** | 15 | unchanged from the above |

**Never three. Never twice the same. Two different senders, both slow and both
hand-sent.**

The text is the signature. `E` is one dit, `I` is two, `H` is four, `T` is one
dah. **A page of E, I, H and T is what a decoder emits when its unit is set several
times too short**: every real dah is chopped into a run of dits and every real dit
falls below the floor. Where whole words survive — `TEST GE N5O`, `QRL`, `TU`,
`TEST DE N 0O` — those are the moments the clock happened to be near enough.

**The signals were not weak.** 22.6 dB of envelope swing on `195617`, S3 to S4,
`Overflow: not overloading`, preamp off. Tim heard them plainly. **This is not a
sensitivity problem and not a front-end problem. It is one number.**

**Ruled by Tim: the senders are slow and manual.** That matters because it points
at a mechanism. A hand-sent fist at ten words a minute has long dahs and ragged
gaps; the key-up distribution measured on the 19th was smeared with no usable
3-unit or 7-unit structure. **Anything that takes part of the unit from key-up
gaps will be dragged short by exactly this kind of sender, and a short unit reads
as a fast fist.** Fitting 31 and 50 words a minute on a ten-word-a-minute manual
operator is that failure with the sign it would have.

An ask about precisely this closed when the old decoder came out — whether the
unit may still be averaged with key-up gaps. **Removing the old `Refine` put a
hand-read fist at 100.0 ms against 100.4 read by hand.** If anything equivalent
survived into the new decoder, this is it on live audio.

### The other three faults from the same evening

**The sidecar and the terminal disagree about the same instant.** `195617` records
`0 characters emitted` and `text: nothing read` for a moment when the screen showed
`T E E E E E TTON KT M 5O`. Two readouts, one instant, two answers. **The roster
Tim is building does not match what he saw**, which makes tonight's evidence
unusable for scoring. HM-DEC-091.

**The panel's pitch disagrees with the decoder's.** `200036` shows `500 Hz` on the
terminal while `toneHz` reads 400 and the keying meter reports `no keying at
400 Hz`. Three readings, two disagreeing.

**The copy-speed control is inert and comes out.** The new decoder reads no seed.
It has been sitting there asking Tim for a number, and **he has said plainly he
cannot judge sending speed by ear** — which is the whole reason the decoder was
supposed to work it out. Remove the control and its wording.

---

## Verify this instruction against the tree

- **Report mismatches; do not repair the instruction silently.**
- **Record the exact failing-test set before you start and after you finish, and
  name every difference.** The red count from the decoder replacement blinds this
  unit unless the sets are compared exactly.
- **The five recordings above are the evidence.** If they are not in the tree, say
  so and work from those that are — but say which figures you could not reproduce.
- `HM-OPEN-055`: rig tests that flake and pass on a rerun. **Not this unit.**

---

## Rulings in force

**HM-DEC-091 — one source, and it says which.** Tasks 4 and 5 are this ruling
applied twice.

**HM-DEC-120 — nothing is emitted on audio holding no signal.** Every change here
must leave it standing. **Report the sensitivity sweep after each task that
touches the decoder.**

**HM-DEC-048 — nothing raises a confidence score.**

**HM-DEC-093 — no radio on the development machine.**

---

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` per `CLAUDE.md` 13 —
the six fields 13 names, **`PHASE`**, `UPDATED` from the clock, `NOTE` saying what
is moving inside the task. Also every ten minutes while a task runs.

---

## Task 1 — Why the clock fits four to sixteen dits to a dah

**Report before changing anything.**

1. **What produces `clockFit`, and is it the same number the decoder decodes
   with?** If the reported fit and the working unit are different quantities, say
   so immediately — the whole table above would then be measuring the wrong thing.
2. **Does anything in the new decoder take the unit, or any part of it, from
   key-up gaps?** Name it if so. This is the leading hypothesis and it must be
   confirmed or killed first.
3. What speeds does the hypothesis grid cover, and **what is its slowest?** The
   reference decoder starts at 8 words a minute. **If Hamlet's grid does not reach
   below ten, a ten-word-a-minute sender cannot be fitted at all** and the nearest
   hypothesis wins by default.
4. Run all five captures through the decoder and report, for each, the fitted unit,
   the fitted ratio, and the hypothesis that won.
5. **Say what the true speed of each sender is**, measured from the audio
   independently of the decoder — `KeyingEnvelope` is the witness and shares no
   code with it.

**If the cause is not in the clock, say so and say where it is.**

---

## Task 2 — Fit a dah at three dits

Gated on task 1. Build what task 1 found, not what this instruction guessed.

- **The unit comes from key-down durations. Never from key-up gaps.** The evening's
  gap distribution has no usable structure and these senders are hand-sent.
- **Extend the hypothesis grid down to at least 8 words a minute**, and up far
  enough for a machine sender — a station running 35 or 40 is likely a program,
  sending clean perfect timing, and is the easiest thing on the band to read if a
  hypothesis exists for it. **Say what range you chose.**
- **A fit whose ratio is far from three is not to be trusted and must lower
  confidence rather than be corrected into shape** (HM-DEC-048).
- **Report the fitted ratio for all five captures after the change.** The target is
  near three. **If it is not, say so plainly rather than reporting the character
  count instead.**

---

## Task 3 — Prove it on the evening's audio

For each of the five captures: the fitted ratio, the speed found, and the text,
before and after.

Then **the two recordings holding no keying stay silent and the sensitivity sweep
invents nothing at any level.** If the fix costs HM-DEC-120, **stop and report** —
it is not worth having.

---

## Task 4 — The sidecar and the terminal must agree

Both must describe the same instant. A capture whose sidecar says `nothing read`
while the screen shows text makes the roster useless for scoring, and the roster is
the only instrument this project has for measuring whether Tim can read the band.

Report which one was wrong on `195617` and why.

---

## Task 5 — One pitch, or say which is which

The terminal shows `500 Hz`, `toneHz` reads 400, and the keying meter says `no
keying at 400 Hz`, all on `200036`. **Either they agree, or each says what it is a
measurement of** (HM-DEC-091).

**The keying meter's own wording may not change.** It is the independent witness
and its text was ruled.

---

## Task 6 — Remove the copy-speed control. **THIS IS THE DROP CANDIDATE.**

The control and the wording around it. The new decoder reads no seed, Tim cannot
judge speed by ear, and **a control that looks live and does nothing is its own
confident wrong answer.**

**Drop it whole if the session is running long and say so.**

---

## Parked — do not touch, do not raise

- **Mode-follow** not switching to USB in the voice portion. Its own unit.
- **The fifty dead tests** describing the removed decoder.
- **Word spacing** on the streaming path.
- **The likelihood gate at 15.0.**
- **HM-OPEN-055, HM-DEC-098, HM-DEC-130, HM-OPEN-033, HM-OPEN-007.**

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: 9.5.1 one branch
and it is `main`, **and every session commits and pushes to it**; no interactive
or destructive git; do not invent a ruling id; do not touch coverage thresholds.

Unit-specific:

- **Do not derive the unit from key-up gaps.** *Hand-sent gaps are smeared and
  these senders are hand-sent.*
- **Do not fix the ratio by clamping it to three.** *A fit forced into shape hides
  the fault instead of correcting it, and HM-DEC-048 says a doubtful fit lowers
  confidence.*
- **Do not break HM-DEC-120 to raise a character count.**
- **Do not silence a readout to make two disagree less.** *Tasks 4 and 5 are about
  agreement or provenance, not about removing a number.*
- **Do not touch the front-end panel or the layout.** *Both shipped and both are
  right.*

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. Four sections, no
other headings: **What Claude did**, **What Tim should expect**, **What we should
do next**, **What's blocking us** — the last carrying **Asks still outstanding**
per HM-DEC-139.

**Section 1 opens with the fitted ratio for all five captures, before and after.**

**Section 2 states in one sentence what a slow hand-sent station reads as now.**

**Report the failing-test set exactly, before and after.**

**Stop and report.**
