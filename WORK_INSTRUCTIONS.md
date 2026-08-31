STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.

---

# Work instruction 056 — the split, the bound, and why letters instead of words

**ISSUED: 2026-08-31. A fresh order, not an amendment. Follows unit 055, and
tasks 3, 4, 6 and 7 of that order return here as Tim directed.**

**Six tasks; task 6 is the drop.**

## Two rulings arrived with this order. Record both.

> **HM-DEC (for Tim to enter): the corpus precision floor is redefined.** The
> average floor may move only when **every individual lock holds** — the
> clean-read locks, the adjudicated anchors, and both silence locks. An average
> can never again be traded against a collapsed easy read, because the easy reads
> are individually locked. **Unit 055's 0.889 against the prior 0.894 is accepted
> under this rule and is the worked example**: yield rose 0.750 → 0.872 while
> every individual lock stayed green.
>
> Rejected: reverting — it returns `003229` to a wall of blocks and `002443` to
> 48 `E`s from noise. Rejected: treating the average floor as inviolable on its
> own — its purpose was always carried by the per-capture locks.

> **HM-DEC (for Tim to enter): `CwProbabilisticDecoder.FastestWpm` is lowered
> from 40 to 30, provisionally.** The ceiling **rises again the day a capture
> shows something faster worth reading** — record that condition with the value.
> The hold-over's safety bound rises from 30 ms to 40 ms with it, reaching the
> lower half of the measured 32–53 ms dropouts.
>
> Rejected: keeping 40 — nothing in the corpus, the bulletins, or any capture the
> operator has sent runs above about 28 WPM, and the bound was sitting just under
> the fault it was built for.

**The corpus floor for this unit is therefore precision 0.889, plus every
individual lock.** A change that drops any single lock is reverted regardless of
what it does to the average.

## Where the tree is

From unit 055's report, to be verified against the tree, which governs:

- **Precision 0.889, yield 0.872, substitutions 20.** Baseline for every task.
- Swing admission shipped: threshold **15 dB**, bounded by measurement — silence
  0.0, band noise 11.9, weakest station 17.2, the CQ 21.5. **The peak keeps the
  pitch; swing keeps the verdict** — that division is written into the code after
  a catastrophic regression (0.894 → 0.470) was caught inside the unit.
- **`003229` shows 57 named characters at 587.5 Hz where it showed 43 blocks —
  and it is not `CQ`.** The admission half is fixed; the reading half is not.
- `002443` emits nothing. The silence set is silent. `013347` recovered 9 → 37
  named, 84% → 36% blocks.
- The negative counters are fixed; `Over` refuses a window whose counters went
  backwards, and the trail drops on retune.
- The nine captures of 2026-08-31 are in the tree as read-only fixtures.
- Locks: `TheSilencePropertyIsLockedTests` 6, `TheCleanReadsStayCleanTests` 7,
  `TheAdjudicatedReadingsKeepReadingTests` 13, `AStationIsABinThatSwingsTests`
  10, `TheSheetDoesNotLieAboutArithmeticTests` 6 — all green.

**Record both suites and the corpus score before task 2.**

## The reference reading, for task 4

An independent decoder sharing no code with Hamlet, run on `003229` at 583.5 Hz
with a 25 ms integrator and a threshold at the envelope's 98th percentile minus
6 dB, read:

```
CQ SITKTZDZ TIQTITK#TTE K
```

with dah CV **0.05** — and at minus 8 dB, `COSITKTZDD DQIITK#TN KA`. **`CQ` and
the closing `K` are stable across settings.** Hamlet, at the same station, now
reads `E`-heavy fragments. **Same audio, same pitch, one reads the call and one
does not — the difference is in the element extraction, and it is findable.**

## Rulings in force

**Transcribed with what was rejected. Do not re-argue either.**

**Tim's rulings**, beyond the two above:

> **Two streams may be a conversation or two people who cannot hear each other.
> Do not assume.** Separate and label by pitch only.

> **Do not break the silence behaviour.**

> **The phase goal is 85% correct CW on a capture where the pitch is right,
> precision before yield.**

> **The only measurement is against real data from the real radio.**

> **FT8, FT4 and every other digital mode are outside this conversation's scope.**

**Standing rulings this unit is bound by:**

- **§0.0 / HM-DEC-009** — never present a guess as a decode.
- **HM-DEC-120** — nothing emitted on audio holding no signal. **Tightened only.**
- **§0.4** — reproduce, then change, then measure.
- **HM-DEC-007 / HM-DEC-091** — WAV fixtures, read-only.
- **§12.5** — a floor is not lowered to fit a change; **the redefinition above is
  Tim's, not a session's.**
- **§0.2 / HM-DEC-008** — **no transmit work of any kind.**

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` — `STATE`,
`TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is moving
inside the task. Same every ten minutes while a task runs.

## The rule that governs every change

- **Precision at or above 0.889, and every individual lock green.** Either
  failing reverts the change.
- **Every task reports precision, yield, substitutions — and what `003229` now
  shows.**

## The tasks

### Task 1 — the two rulings take effect

- **Record both rulings** in the report's decision section in the form above.
  **Do not mint ids.**
- **Lower `FastestWpm` to 30**, with the provisional condition in a comment beside
  the value, and **raise the hold-over bound to one dit at the new ceiling.**
- **Re-sweep the hold-over across the newly legal range** — 12, 16, 20, 24, 28,
  32, 36, 40 ms — against the corpus and every lock. Unit 054 found 16 ms broke
  the `, AND` anchor on `031838`; **that anchor still governs.** Adopt the best
  point at which **every lock holds**; report the curve either way.
- **Report what the wider bound does to the shredded pair** (`003408`, `003419`)
  even though nothing is promised for them — the dropouts there are the far end
  of the range this bound was raised for.

### Task 2 — every element carries its own pitch

For each element the decoder resolves, **measure the element's frequency over the
element's own samples** — peak with parabolic interpolation. A 190 ms dah
resolves to ~5 Hz; a 55 ms dit to ~18 Hz with interpolation.

- **Store it on the element**; it reaches the capture sheet's element records.
- **No behaviour changes.** The corpus score must be **identical** — this task
  adds a measurement and its acceptance is that nothing moves.

### Task 3 — near-tied senders separate into streams

**Cluster an admitted station's elements by their measured pitch.**

- **One cluster: nothing changes.** The common case must cost nothing.
- **Two clusters separated by more than the per-element measurement error but
  within the detector's bandwidth** — the `002829` case, ~13 Hz — **the elements
  divide into two streams, each decoded separately**, each with its own speed and
  gap structure.
- **Conservative**: when in doubt, one sender. **State the separation criterion
  and its margin over the measurement error** — a split that fragments one wobbly
  sender into two is worse than the collision it fixes.
- **Labelled by pitch only.** No claim about their relationship (Tim's ruling).

**Acceptance:** on `002829`, two streams near 602 and 615 Hz, each stream's dit CV
reported beside the combined 0.47, and the decode of each stream shown. On every
single-sender fixture, exactly one stream and an unchanged decode. **Floor and
locks hold.**

### Task 4 — why letters instead of words

**Same audio, same pitch: the reference reading finds `CQ … K` and Hamlet finds
`E`-fragments. Find the difference. This is the unit's centre.**

Work the comparison concretely on `003229`:

- **Extract Hamlet's element stream** — every mark and space with durations — **and
  the element stream implied by the reference parameters** (25 ms integrator,
  threshold at the 98th percentile minus 6 dB, minimum run 15 ms with
  drop-and-merge). **Print both, aligned in time, over the seconds where the
  reference reads `CQ`.**
- **Name where they diverge**: marks split that the reference keeps whole, gaps
  missed, marks invented. **The `E`-storm means Hamlet's stream has many short
  isolated marks; establish whether they are real dits the reference merges, or
  fragments the reference never sees.**
- **Candidate causes to check against the divergence, in this order**: the
  integrator width (45 Hz here, ~40 Hz in the reference — unlikely); **the
  threshold's placement on this capture** (Otsu on a mostly-quiet window was unit
  051's finding — `003229`'s station is present for only part of the file, and the
  admission window fix of unit 052 was measured and refused, so the *decoder's*
  threshold may still be computed over the silent majority); the hold-over's
  reach after task 1; **and the minimum-run handling** — unit 054 proved
  `Runs` drops a short run **without merging the two it separated** and that at
  one hop the hysteresis makes it unreachable; **establish whether at this
  capture's SNR longer noise notches reach it**, because drop-without-merge
  corrupts every duration after it and is the exact bug that made the reference
  bench unreadable until fixed.
- **Fix what the divergence names, smallest change first, measured after each**
  against the corpus and every lock.

**Acceptance:** `003229` reads **`CQ` and the closing `K`**, with the middle as
blocks or letters as the audio allows. **That was unit 055's acceptance and it is
still the acceptance.** If after the named causes are exhausted it still does not,
**report exactly which divergence remains and stop** — a named residue is worth
more than a forced pass.

### Task 5 — the sheet speaks for streams

Where task 3 split streams, the capture sheet carries **per-stream lines** —
pitch, element count, characters, speed — in the sheet's own voice.

- **Propose the wording in the report for Tim** (§12.1). Do not invent idioms
  beyond the proposal.
- The per-element pitch from task 2 appears in the element records.
- **The arithmetic locks** (`TheSheetDoesNotLieAboutArithmeticTests`) **must stay
  green** through the additions.

### Task 6 — the shredded pair, characterised *(the drop candidate)*

**Measure only. Change nothing. Promise nothing.**

On `003408` and `003419`, now with per-element pitch available: element-length
distribution, swing, per-element pitch spread, and **whether the fragments cluster
at one pitch or many** — which is the difference between one station being torn
apart and several stations colliding. **If the honest answer is "nothing reads
this," say so.**

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

**FT8, FT4 and every other digital mode.**

**Hysteresis on the peak** — costed in 053, still awaiting ruling. **The
peak-referenced threshold** — refused in 054, kept with its numbers. **The
admission window** — refused in 052. **The confidence quantities** — seven
measured, no eighth.

Also: the joint decoder; the lattice; the evidence term; the settings contract;
the scanner; `CHANGELOG.md`; the missing `DECISIONS.md` records; the phrasebook;
the Twin PBT; the answer key's licensing; the dial-move threshold; the transcript
break's wording; **the version bump — still unruled, do not guess.**

**Both halves are required: do not touch them, and do not raise them.**

A parked item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **No transmit. Nothing keys the radio.**
- **Do not break the silence property**, and **do not modify any lock.**
- **Do not let any individual lock fall**, whatever it does to the average.
- **Do not let precision fall below 0.889.**
- **Do not assert a relationship between two streams.**
- **Do not split one sender into two.** Conservative, margin stated.
- **Do not raise the hold-over past one dit at `FastestWpm`.**
- **Do not force `003229` to pass.** A named residue beats a forced pass.
- **Do not adopt off a non-monotonic sweep.**
- **Do not build peak hysteresis.**
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 to `output.md` at the repository root, overwritten
and printed. **Read the file's own section count and follow it.**

**Write `output.md` before you stop, for any reason at all.**

**The section that says what the owner should expect leads with one line: what
`cw-2026-08-31-003229` now shows.** Then `002829`'s two streams, side by side.

**The section that reports measurements leads with task 4's aligned element
streams — Hamlet's against the reference's — over the seconds where the reference
reads `CQ`.**

**If you finish every task, stop and report. Do not start the next unit.**
