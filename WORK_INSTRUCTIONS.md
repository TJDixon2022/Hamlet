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

# Work instruction 043 — decode every pitch, keep the best

**ISSUED: 2026-08-28. A fresh order, not an amendment. Follows unit 042.**

**Five tasks; task 5 is the drop.**

## Why this unit exists

**One in eight.** That is how often the operator says Hamlet lands on the pitch a
station is actually sending at. When it lands, the decode measures **84.2 %** on
`cw-2026-08-24-012403` and 85–90 % on the settled stretch of
`cw-2026-08-28-004844`. **The whole gap between this project and its phase goal
is pitch acquisition, and nothing else.**

```
PHASE GOAL:   Readable CW on the operator's screen — eighty percent of a
              strong signal read correctly, first time.
UNIT GOAL:    Choose the mixdown pitch by which candidate decodes best,
              instead of by statistics on an intermediate signal.
ADVANCES:     task 3
```

**Where the number came from and what it replaces.** Six families of admission
statistic have been built and measured dead across units 1.11.17 to 1.11.21 —
cluster separation, dah/dit ratio, level spread, lift over the band floor,
quantisation residual, and agreement between fitted units. Every one asked *is
this bin a station*. **That question has no good answer and six measurements say
so.** This unit asks a different one: *which of these candidates decodes best*.

**The evidence that ranking works is already in the operator's own sheets, and
it is not subtle:**

| capture | best score, better than silence per hop |
|---|---|
| `cw-2026-08-28-004844` — reads `TUES AUG 25`, `W7GB`, `BRUCE` | **36.3** |
| `cw-2026-08-28-004902` — reads the same net | **28.3** |
| `cw-2026-08-28-005051` — phantom | 1.48 |
| `cw-2026-08-28-005158` — phantom | 1.49 |
| `cw-2026-08-28-005243` — phantom | 3.34 |

**A factor of six to twenty-four, on the one quantity that measures the thing the
operator wants.** Unit 1.11.33 proved it cannot work as a *gate* — no fixed
threshold separates those columns from the rest of the corpus. **As a ranking
between candidates on the same audio it does not need to.**

**This also answers the phantoms, which is the operator's first goal tonight.**
Last night's four junk captures were decoded at 750–775 Hz. Tested outside
Hamlet at every candidate pitch including 599.3 — where two independent
instruments pointed — **there is no readable Morse in those files at any pitch.**
`#T#E2T#H1 N E TE KTE SAITINT` at 599, `##I##E#E TE #ME SA#INT` at 775. Hamlet
was not misreading a station; it picked an empty bin and printed what it found.
**A pitch chosen by decode score does not rank an empty bin first, and a winner
whose own score is poor produces nothing.**

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and
report any mismatch.

**This author's picture has been stale by several units and unit 1.11.33 caught
it.** Trust the tree over this order everywhere they differ, and list the
differences. The engine total in particular has moved twice this week.

**Known from unit 1.11.34:** 28 failing in the engine as the stable set; 509 in
the app; `CLAUDE_CODE.md` at 1.7. **Read the file's own section count for the
report shape.** Seven captures of 2026-08-28 are in the tree.

**The harness that runs a fixed pitch through the decoder exists** — unit 002
built it and it has been the diagnostic instrument since. **Task 1 establishes
whether it can be called per candidate, and if it already can, this unit is
mostly plumbing.** Say so; that is tokens back.

## Rulings in force

**Transcribed in full with what was rejected. Do not re-argue either.**

**Tim's ruling, 2026-08-28:**

> **The mixdown pitch is chosen by which candidate decodes best, not by whether a
> bin passes a test.** Decode at each candidate across the filter's width, rank by
> the decoder's own score, take the winner. **Where even the winner's score is
> poor, nothing is emitted.**
>
> **Rejected: a seventh admission statistic.** Six families measured dead across
> five units; the fault is the question, not the choice of measure.
> **Rejected: using the score as a fixed gate.** Unit 1.11.33 measured that no
> threshold separates the corpus, and this unit does not need one — a ranking
> compares candidates on the same audio.
> **Rejected: thinning the band or lengthening the cadence to make the cost fit,
> without reporting it.** If the compute does not fit, that is a measurement to
> report, not a silent trade.

**Standing, and this unit is bound by them:**

- **§0.0 / HM-DEC-009** — never present a guess as a decode. A pitch nobody
  measured must not produce letters that imply it was measured.
- **HM-DEC-120** — nothing is emitted on audio holding no signal. **Both silence
  controls stay silent; this unit may only tighten that, never loosen it.**
- **HM-DEC-095** — a note is chosen by how it is keyed, never by how loud it is.
  **A decode score is a keying measurement, not a loudness one**, which is why
  ranking by it is inside that ruling rather than against it.
- **§0.2 / HM-DEC-008** — no transmit work of any kind.

## Status cadence

Named here as well as in the prompt. After each task, before the next, update
`PROJECT_STATUS.md` — `STATE`, `TASK: n of m`, `BALL`, `UPDATED` read from the
clock, `NOTE` saying what is moving inside the task. The same every ten minutes
while a task runs.

## The tasks

### Task 1 — trace, and say what you find rather than confirming this list

Answer from the code, with file and line, before writing anything:

1. **What it costs to decode one window at one pitch** — measured, not estimated.
   The candidate count across a 500 Hz filter at 25 Hz steps is about
   thirty-three; **thirty-three times that cost is the number this unit lives or
   dies on.**
2. **Whether the fixed-pitch harness can be called per candidate**, or whether
   the decode path assumes one tracked pitch throughout. **If it is already
   callable, say so — the unit shrinks.**
3. **Where the mixdown pitch enters the decode**, and what else consumes it.
4. **What the decoder's score is per window** — the quantity printed as *better
   than silence per hop* — and whether it is comparable between two runs over the
   same audio at different pitches. **If it is not comparable, this unit's
   premise fails and task 2 must find what is.**

**Then build and run, and record the baseline by diffing which tests fail rather
than by a total.**

### Task 2 — rank the candidates, measured, changing nothing

For every capture in the corpus: decode a window at **each candidate pitch across
the filter**, and report per capture — the winning pitch, its score, the score at
the pitch Hamlet chose today, and the score at the pitch measured from the audio
where that is known.

**Then answer in one sentence: how often does the winner match the pitch that
reads best, against the one-in-eight the operator has now?**

That sentence is this unit's number. **If ranking is no better than what is
shipped, stop, report it, and build nothing further** — that is an honest result
and it retires the approach.

### Task 3 — the winner drives the decode *(the goal task)*

Where task 2 shows ranking works, make it the source of the mixdown pitch.

- **The ranking runs on short windows**; the full path runs only on the winner,
  if task 1's cost requires it. **Report which was necessary.**
- **The sidecar says the pitch was chosen by ranking**, and carries the winner's
  score and the runner-up's, so a bad choice is visible afterwards.
- **Existing admission is not removed.** It keeps running and its verdict keeps
  being recorded; what changes is what drives the mixdown. **A statistic measured
  dead is not deleted in the same unit that replaces it.**

**Acceptance:**

- **`004844`, `004902`, `004915` unchanged or better, character for character** —
  they read a real net and must not pay for this;
- **the four phantom captures emit no letters, or the winner's own score is
  reported as poor and nothing is emitted**;
- **all twelve adjudicated anchors green**; every floor held; both silence
  controls silent; chunk invariance intact.

**If an anchor goes red, report which and what it loses before shipping.**

### Task 4 — what the operator sees when the winner is poor

Where no candidate decodes well, **the terminal says so in the plain language it
already uses**, in the existing prose area. One line. **No new panel, no new
control, nothing else on the screen moves.**

The complaint is not an empty screen. It is a screen that lies.

### Task 5 — the tune-in case *(the drop candidate)*

Unit 1.11.34 measured that a blanket refusal costs
`AFastFistIsReadWithoutARunUp` — **tuning onto a station already sending returns
nothing**, which is most of how an operator finds one.

**Measure only**: with ranking in, what does that test do? Ranking should help it,
because a station mid-transmission decodes well immediately and needs no
admission history. **Report the number; change nothing on its account.**

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

Built from unit 1.11.34's sections 3 and 4, and the operator's standing holds.

- **The no-keying refusal.** Measured twice, handed back twice; ranking may make
  it unnecessary. **Its own unit if it is still wanted after this one.**
- **The clock-withdrawn refusal** — measured dead, 26/38/25 characters off the
  good captures.
- **The keying meter, `competing`, and the independent sweep** — task 2 supersedes
  them as pitch sources; **do not fix, remove or rebuild them here.**
- **The joint decoder, the constrained margin, the integrator width, the
  whole-file second pass, `001520`'s quadrillions, `013347`'s 17.2 million, the
  short-character bias, `cwdecoder.py`'s divergence.**
- **The whole FT8 and layout stream of units 037–042**, the favourites list, the
  redesign inventory, the recent-places row, the scanner and calling cycle.
- **`CHANGELOG.md`, the missing `DECISIONS.md` records, the intermittents.**

**Both halves are required: do not touch them, and do not raise them.** A parked
item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are cited, not retyped.
Unit-specific:

- **Do not build a seventh admission statistic.** Ruled out above with six
  measurements.
- **Do not use the score as a fixed threshold.** It is a ranking.
- **Do not thin the band or slow the cadence silently.** Report the cost.
- **Do not let the three good captures lose a character.**
- **Do not remove existing admission in this unit.**
- **Do not trade the silence property.**
- **Do not touch the panel beyond task 4's one line.**
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — **read the file's own section count** — to
`output.md` at the repository root, overwritten and printed. **Writing it is the
only way out**: complete, blocked, failed or stopped.

**Section 3 leads with task 2's sentence — how often ranking picks the pitch that
reads best, against one in eight.**

**The section on what the owner should expect leads with this: on a frequency
where nothing is happening the terminal stops filling with letters, and on a
frequency where a station is sending Hamlet lands on it more often than it did.**

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140, from unit 1.11.34's list, trimmed
to what this unit does not park.

1. **The refusal costs reading a station you tune onto** — task 5 measures what
   ranking does to it.
2. **Admission admits a pitch 150 Hz off the station and holds it for
   forty-five seconds without a refresh** — the held peak decayed at exactly
   1 dB per second across both gaps, so it was never refreshed at all.
3. **The `reading` line's new span wording needs approval.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
5. **Two stations closer than 125 Hz are not named** — the operator's item five.
6. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
7. **Nothing checks that deleting a surface is not deleting a capability** — the
   operator has since found the favourites list gone. **Parked here, and it is
   the next unit after this one unless he says otherwise.**

**If you finish every task, stop and report. Do not start the next unit.**
