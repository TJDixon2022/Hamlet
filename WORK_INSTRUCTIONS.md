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

# Work instruction 044 — put the ranking on the screen

**ISSUED: 2026-08-28. A fresh order, not an amendment. Follows unit 043.**

**Four tasks; task 4 is the drop. The operator is at the radio within the hour
and task 2 is what he will see.**

## Why this unit exists

**34 of 44, measured, and not one line of it reaches the operator.**

Unit 1.12.6 found that ranking candidate pitches by the decoder's own score picks
the station on **34 of 44 captures**, against **1 of 44** for the score as it
stands — same window, same decoder, one change. Its own report says why that
number is not on the screen: *"no decoder path was changed, so it is not on the
operator's screen yet."*

```
PHASE GOAL:   Readable CW on the operator's screen — eighty percent of a
              strong signal read correctly, first time.
UNIT GOAL:    Make the ranked pitch drive the live decode, and stay silent
              when even the best candidate is poor.
ADVANCES:     task 2
```

**Why the score as it stands ranks backwards, because task 2 depends on
understanding it.** `LikelihoodRatio` estimates both the noise scale and the
keyed level from the very envelope it is scoring
(`CwProbabilisticDecoder.LogLikelihoods`, `:973`). A bin the receiver's filter has
already emptied has almost no noise left in it, so the residual wobble is scored
against a tiny sigma and looks like the cleanest keying in the band. **The
quietest bin wins.** On `cw-2026-08-28-004844` the winner sat at 875 Hz scoring
**312.62** and read `E E EE E EEEE E EEE`, against **29.84** at the pitch that
reads the net.

**The fix unit 1.12.6 measured: stand every candidate on one noise floor measured
across the whole band** — each envelope combined in power with a single pedestal,
the loudest per-bin floor in the band, which is what each bin would look like if
the receiver's floor were flat. A bin holding nothing goes flat against it and
scores near nothing. A bin holding a station keeps its marks above it.

**And the winners read, which is what makes 34 evidence rather than circular** —
`VA3VRR`, `N4L`, the ARRL bulletin, `KD0UN`, `W7GB`, `BRUCE`, `D NOT SURE - BUT
ANY WAY VY NICE`.

**The phantoms behave as hoped.** On `005158`, `005218` and `005243` the winner
picks 600 Hz rather than the 750–775 Hz Hamlet used, reads junk there, and scores
**5.28, 3.07 and 5.13** against **15.71, 12.32 and 10.15** for the three good
captures of the same night. **That gap is what task 3 puts a floor in.**

**A correction the previous order needs, carried so it is not repeated.** Unit
043's evidence table quoted the phantoms at 1.48, 1.49 and 3.34 against 36.3 and
28.3. **The sheets in the tree say 7.6, −68562.4 and 158.4.** A phantom scores
158.4 where a real net scores 36.3, so the separation that order was commissioned
on was never in the sheets. **The pedestal supplies it.** Right conclusion, wrong
evidence; do not carry that table forward.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and
report any mismatch. Unit 1.12.6 disproved part of its own order's premise and was
right to.

**Known from unit 1.12.6, and to be confirmed rather than assumed:** baseline 28
failing, 1930 passing, 1958 total. Candidate count is **25**, not thirty-three —
`CwToneTracker.MinimumToneHz` 300, `MaximumToneHz` 900, `CoarseSpacingHz` 25
(`CwToneTracker.cs:125,128,138`). The harness is already callable per candidate:
`CwProbabilisticDecoder.Decode(MonoAudio, double toneHz)` at `:639`,
`Decode(IReadOnlyList<double>, double)` at `:652`, `CwDecoder.AssertAt(double)` at
`CwDecoder.cs:515`. `CwProbabilisticStream.ReadAgain(audio, toneHz)` at `:475`
already re-mixes held audio at a new pitch, and `AudioTap` already keeps thirty
seconds.

**The one place the mixdown pitch enters is `CwDecoder.Step`, `CwDecoder.cs:753`.**

**`CwDecodeReport.ToneHz` comes from `_tracker.ToneHz` (`CwDecoder.cs:279`), not
from the mixdown.** A ranked pitch driving the mixdown does **not** by itself reach
the capture sheet, the duty line or the panel — task 2 must feed those
deliberately or the sheet will report one pitch while the decode uses another.

## Rulings in force

**Transcribed in full with what was rejected. Do not re-argue either.**

**Tim's ruling, 2026-08-28, on the two questions unit 1.12.6 asked:**

> **The ranking pass runs on a four-second window, and the full path runs on the
> winner alone.** Twenty-five candidates at the shipped twelve seconds costs 248 %
> of one core and does not fit; four seconds costs 78 % and holds several
> characters at twenty words a minute. **Cost is linear in window length, so this
> is a trade and it is being made deliberately.**
>
> **Ranking runs once on tune-in, and again if the winner's score collapses.** It
> does not run continuously. That matches how the lock already behaves, costs
> nothing while the operator sits on a station, and still recovers when it lands
> wrong.
>
> **Rejected: three seconds** — cheaper, but thin on evidence the first time this
> runs live. **Rejected: continuous re-ranking** — the cost is unmeasured at the
> shipped cadence and nothing shows it is needed. **Rejected: shipping the ranked
> pitch to the mixdown without feeding the sheet from it** — a sheet reporting one
> pitch while the decode uses another is the `tonePeak` fault a third time.

**Standing, and this unit is bound by them:**

- **HM-DEC-120** — nothing is emitted on audio holding no signal. **Both silence
  controls stay silent. This unit may only tighten that.**
- **§0.0 / HM-DEC-009** — never present a guess as a decode; a value that was not
  measured says so.
- **HM-DEC-095** — a note is chosen by how it is keyed, never by how loud it is.
  **A decode score is a keying measurement**, which is why ranking by it sits
  inside that ruling rather than against it.
- **§0.2 / HM-DEC-008** — no transmit work of any kind.

## Status cadence

Named here as well as in the prompt. After each task, before the next, update
`PROJECT_STATUS.md` — `STATE`, `TASK: n of m`, `BALL`, `UPDATED` read from the
clock, `NOTE` saying what is moving inside the task. The same every ten minutes
while a task runs.

## The tasks

### Task 1 — the pedestal, committed and tested

Unit 1.12.6 built the pedestal inside `tools/Hamlet.PitchRank`. **Move it into the
engine where the shipped path can call it**, with its own tests:

- a bin holding nothing scores near nothing against the pedestal;
- a bin holding a keyed station keeps its structure;
- **the 1-of-44 against 34-of-44 result reproduces** from the engine's copy, not
  the tool's.

**If the tool's implementation and the engine's disagree on any capture, stop and
report it** — that is the measurement this unit rests on.

### Task 2 — the ranked pitch drives the decode *(the goal task)*

Per the ruling: **ranking on a four-second window, once on tune-in and again when
the winner's score collapses; the full path on the winner alone.**

- **The mixdown takes the ranked pitch** at `CwDecoder.Step` (`:753`), in the
  existing precedence — the operator's lock still wins.
- **`CwDecodeReport.ToneHz` is fed from the ranking**, not from `_tracker.ToneHz`,
  so the capture sheet, the duty line and the panel all report the pitch that was
  actually used.
- **The sheet says the pitch was ranked**, and carries **the winner's score and
  the runner-up's**, so a wrong pick is visible afterwards instead of being a
  mystery.
- **Existing admission is not removed.** It keeps running and its verdict keeps
  being recorded. A statistic measured dead is not deleted in the unit that
  replaces it.

**Acceptance:**

- **`004844`, `004902`, `004915` unchanged or better, character for character** —
  they read a real net and must not pay for this;
- **all twelve adjudicated anchors green**; every floor held;
- **both silence controls silent**; chunk invariance intact;
- **the measured cost at the shipped cadence is reported**, not estimated.

**If an anchor goes red, report which and what it loses before shipping.**

### Task 3 — nothing on the screen when the best candidate is poor

Sweep a floor on the winner's score, using the gap unit 1.12.6 measured: **5.28,
3.07, 5.13 for the phantoms against 15.71, 12.32, 10.15 for the good captures of
the same night.**

- **Sweep it across the whole corpus and report the table**, not a chosen value
  with the working hidden.
- **Where the winner is below the floor, no letters** — blocks or nothing.
- **Unit 1.11.33 found no fixed threshold separates the corpus in the old units.
  These are new units and that finding does not carry**, but it is the reason the
  sweep is reported in full rather than a number being picked.

**If no floor both silences the phantoms and keeps every anchor, ship no floor,
report the sweep, and say which anchor each candidate value costs.** Task 2 still
ships.

### Task 4 — the terminal says why it is quiet *(the drop candidate)*

Where task 3's floor is holding, **one line in the existing prose area**, in the
language the app already uses — that Hamlet can hear something and cannot make
letters of it. **No new panel, no new control, nothing else on the screen moves.**

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

Built from unit 1.12.6's sections 3 and 4 and the operator's standing holds.

- **The no-keying refusal and the clock-withdrawn refusal** — measured, handed
  back twice; ranking may make the first unnecessary. **Its own unit.**
- **The six dead admission families**, the keying meter, `competing`, the
  independent sweep. **Superseded as pitch sources; not fixed, removed or rebuilt
  here.**
- **The joint decoder, the constrained margin, the integrator width, the
  whole-file second pass, `001520`'s quadrillions, `013347`'s 17.2 million, the
  short-character bias, `cwdecoder.py`'s divergence.**
- **The whole FT8 and layout stream**, the favourites list, the redesign
  inventory, the recent-places row, the scanner and calling cycle.
- **`CHANGELOG.md`, the missing `DECISIONS.md` records, the intermittents.**

**Both halves are required: do not touch them, and do not raise them.** A parked
item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are cited, not retyped.
Unit-specific:

- **Do not rank continuously.** Once on tune-in, and on collapse.
- **Do not rank on a window other than four seconds** without reporting it as a
  trade and saying why.
- **Do not let the mixdown and the sheet disagree about the pitch.**
- **Do not let the three good captures lose a character.**
- **Do not remove existing admission in this unit.**
- **Do not pick a floor without publishing the sweep.**
- **Do not trade the silence property.**
- **Do not touch the panel beyond task 4's one line.**
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — **read the file's own section count** — to
`output.md` at the repository root, overwritten and printed. **Writing it is the
only way out**: complete, blocked, failed or stopped.

**Section 3 leads with the answer to what this unit was commissioned to ask: with
the ranked pitch driving the live decode, what do the three good captures and the
four phantoms now read, and at what measured cost per second.**

**The section on what the owner should expect leads with this: on a frequency
where a station is sending, Hamlet lands on it far more often than it did; on one
where nothing is, the screen stops filling with letters.**

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140, from unit 1.12.6's list.

1. **`DRIFT` had no count to carry** — unit 1.12.6's block carried none, so the
   chain starts here. **This unit's block carries none either; the count begins
   at whatever 1.12.6's report recorded.**
2. **The ranking's ten misses of forty-four are unexamined** — which captures,
   and whether they share a shape.
3. **Admission admits a pitch 150 Hz off the station and holds it for
   forty-five seconds without a refresh**, the held peak decaying at exactly
   1 dB per second because nothing refreshed it.
4. **The `reading` line's span wording needs approval.**
5. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
6. **Two stations closer than 125 Hz are not named.**
7. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
8. **Nothing checks that deleting a surface is not deleting a capability** — the
   operator has since found the favourites list gone. **The next unit unless he
   says otherwise.**

**If you finish every task, stop and report. Do not start the next unit.**
