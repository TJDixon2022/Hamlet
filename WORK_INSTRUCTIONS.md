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

# Work instruction 045 — port the decoder that already works

**ISSUED: 2026-08-28. A fresh order, not an amendment.**

**Four tasks; task 4 is the drop. Nothing in this unit is invented. Every line of
it is a port of code that already reads the operator's own captures.**

## Why this unit exists

**Six invented statistics, all measured dead, while a working decoder sat in the
repository root.**

`cwdecoder.py` reads these captures. It has read them since before this phase
began. Across units 1.11.17 to 1.11.21 this project built and measured **six
families of admission statistic** — cluster separation, dah/dit ratio, level
spread, lift over the band floor, quantisation residual, agreement between fitted
units — and every one failed. **None of them appears in any published CW decoder,
because nobody needs them.** The reference in this tree does not ask *is this bin
a station*. It thresholds against a tracked floor, fits a clock, and refuses when
no clock fits.

```
PHASE GOAL:   Readable CW on the operator's screen — eighty percent of a
              strong signal read correctly, first time.
UNIT GOAL:    Port the reference decoder's acquisition and gating into the
              engine and measure it head to head against the shipped path.
ADVANCES:     task 2
```

**The operator's two priorities, and what in the reference answers each:**

**No letters on a dead frequency.** `fit_clock` (`cwdecoder.py:163`) returns
`None` when the marks do not form two lengths, and nothing downstream runs
without a clock. **A bin of noise produces no fitting clock, so there is no floor
to sweep and no threshold to choose.** That is the difference between this and
every refusal this project has built: the reference does not decide whether to
believe a decode, it declines to produce one.

**The right pitch often enough.** `acquire_tone` (`:62`) scores each of 300–900
Hz in 25 Hz steps by **P90 minus P30 of the bin's power in decibels, over active
audio only** — a keyed tone spreads, a steady carrier and an empty bin do not. It
is four lines and it does not estimate its noise scale from the signal it is
scoring, which is the fault that made the quietest bin win in unit 1.12.6.

**Two of the reference's own comments are the record of this project's mistakes
and must survive the port verbatim as comments:** that the 2.5–3.8 ratio band
**refused `cw-2026-08-17-134712`'s real 4.24-dit fist**, adjudicated as
HM-DEC-144; and that `well_separated` was tried as a *replacement* for the band
and measured at five decibels dropping fast-working from 58 % to nothing, so the
reference keeps **both**, band first and scatter as a widening that can only
accept.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and
report any mismatch.

**`cwdecoder.py` is in this zip at the repository root**, so the port has its
source beside it. **If the tree's copy differs from the one delivered, the tree's
is authoritative and the difference is reported** — unit 1.12.5 found that two
orders described this file wrongly.

**Known from unit 1.12.6, to be confirmed rather than assumed:** baseline 28
failing, 1930 passing, 1958 total. Candidate band 300–900 Hz at 25 Hz gives 25
candidates, which is the same grid `acquire_tone` sweeps — **the reference and the
tracker already agree on the search space.**

**The corpus is 44 captures** and unit 1.12.6's offline ranking got 34 of them.
**That is the number to beat.**

## Rulings in force

**Transcribed in full with what was rejected. Do not re-argue either.**

**Tim's ruling, 2026-08-28:**

> **Stop inventing. Port the reference.** `cwdecoder.py` reads these captures and
> this project has spent twenty units reinventing what it does. Its acquisition
> and its gating go into the engine as a port — the same algorithm, not the same
> idea reimplemented — behind a setting, and it is measured head to head against
> the shipped path on all forty-four captures.
>
> **Rejected: adopting its ideas and writing fresh code.** That is what produced
> six dead statistics. **Port the algorithm.**
> **Rejected: replacing the shipped path outright in this unit.** It ships behind
> a setting so the two can be compared on the operator's own audio, and so a
> regression is one toggle away from being undone.
> **Rejected: a seventh statistic, a swept floor, or any new discriminator.** The
> reference's refusal is structural — no clock, no decode — and needs no
> threshold chosen by anybody.

**Standing, and this unit is bound by them:**

- **HM-DEC-120** — nothing is emitted on audio holding no signal. Both silence
  controls stay silent under both paths.
- **HM-DEC-144** — `N4L` on `cw-2026-08-17-134712` is adjudicated. **The
  reference's own comment says the ratio band refuses that fist and
  `well_separated` is what admits it. The port must read it.**
- **§0.0 / HM-DEC-009** — never present a guess as a decode.
- **§0.2 / HM-DEC-008** — no transmit work of any kind.

## Status cadence

Named here as well as in the prompt. After each task, before the next, update
`PROJECT_STATUS.md` — `STATE`, `TASK: n of m`, `BALL`, `UPDATED` read from the
clock, `NOTE` saying what is moving inside the task. The same every ten minutes
while a task runs.

## The tasks

### Task 1 — port the reference's chain, function by function

Into the engine, as a port. **Same algorithm, same constants, same order.** Each
function keeps its name so the two can be read side by side:

| from `cwdecoder.py` | what it does |
|---|---|
| `mute_mask` `:46` | marks the operator's own transmissions, 150 ms holdoff |
| `acquire_tone` `:62` | P90−P30 in dB over active audio, 300–900 Hz by 25 |
| `fine_envelope` `:77` | 25 ms window envelope at the acquired pitch |
| `two_means` `:105` | the two-centre split used by the gate and the clock |
| `gate` `:113` | 3 s windows, **6 dB minimum contrast**, 6 dB hysteresis |
| `deglitch` `:135` | merges runs shorter than a fraction of the fitted dit |
| `runs` `:145` | marks and spaces, with 60 ms border truncation |
| `fit_clock` `:163` | **returns nothing when no clock fits** |
| `well_separated` `:193` | the widening that admits a heavy fist |
| `classify_gaps` `:212` | three gap classes fitted from the sender |
| `decode` `:245` | elements to characters |

**Where the reference and the engine already have the same thing, use the
engine's and say so** — that is tokens back. **Where they differ, the
reference's wins in this unit**, because the point is to measure the reference
rather than a hybrid.

**Its comments come across verbatim** where they record a measurement — the two
named above especially. A rule whose reason is deleted is one the next session
talks itself out of.

**Tests: the reference's own behaviour, not new judgement.** `fit_clock` returns
nothing on noise. `acquire_tone` finds 429 Hz on `cw-2026-08-28-004844`. The
`gate` refuses a window under 6 dB of contrast.

### Task 2 — head to head on all forty-four *(the goal task)*

Behind `AppSettings.UseReferenceDecoder`, **default off**, run both paths over
every capture and report a table with one row per capture:

- the pitch each path chose, and the pitch measured from the audio where known;
- the characters each emitted;
- **for the four phantoms — `005051`, `005158`, `005218`, `005243` — whether the
  reference emits anything at all**;
- **for the twelve adjudicated anchors, whether each reading survives**;
- **for both silence controls, that both paths emit nothing.**

**Then answer in one sentence: on how many of the forty-four does the reference
pick the pitch that reads, against the shipped path's one in eight live and unit
1.12.6's 34 of 44 offline?**

**If the reference loses, say so plainly and ship it off.** That is a real result
and it retires an argument this author has made three times.

### Task 3 — the setting defaults on, if it earned it

**Default `UseReferenceDecoder` on only if all of these hold**, each stated in the
report:

- **the four phantoms emit no letters**;
- **all twelve adjudicated anchors still read**, including `N4L`;
- **both silence controls silent**;
- **the reference picks the reading pitch on more captures than the shipped path
  does.**

**If any fails, ship it off with the table** and name which line failed. **Do not
tune the reference's constants to pass a line** — it is a port, and a tuned port
is a seventh invention.

### Task 4 — cost and cadence *(the drop candidate)*

Measure what the reference's chain costs per second of audio at the shipped
cadence, against the current path. **Measure only; change nothing on its
account.** `acquire_tone` sweeps 25 bins with a 25 ms Goertzel at 10 ms hop, which
is far cheaper than 25 full decodes — **say how much.**

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

- **The pedestal ranking of unit 1.12.6.** Real, measured at 34 of 44, and **its
  own unit if the reference loses.** Not built into this one.
- **The no-keying refusal, the clock-withdrawn refusal, the swept floor** — all
  superseded by `fit_clock`'s structural refusal if the reference wins.
- **The six dead admission families, the keying meter, `competing`, the
  independent sweep.**
- **The joint decoder, the constrained margin, the integrator width, the
  whole-file second pass, `001520`'s quadrillions, `013347`'s 17.2 million.**
- **The whole FT8 and layout stream**, the favourites list, the redesign
  inventory, the scanner and calling cycle.
- **`CHANGELOG.md`, the missing `DECISIONS.md` records, the intermittents.**

**Both halves are required: do not touch them, and do not raise them.** A parked
item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are cited, not retyped.
Unit-specific:

- **Do not improve the reference while porting it.** A port that is better than
  its source is not a port and cannot be measured against it.
- **Do not tune its constants to pass an acceptance line.**
- **Do not build a seventh discriminator, a floor, or a threshold.**
- **Do not delete the shipped path.** Both stay; the setting chooses.
- **Do not default the setting on unless every line in task 3 holds.**
- **Do not trade the silence property.**
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 — **read the file's own section count** — to
`output.md` at the repository root, overwritten and printed. **Writing it is the
only way out**: complete, blocked, failed or stopped.

**Section 3 leads with task 2's sentence and its table.**

**The section on what the owner should expect leads with this: whether, with the
setting on, a dead frequency shows nothing and a station he can hear gets picked
— in those words, because those are the two things he asked for.**

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140.

1. **The pedestal ranking is measured at 34 of 44 and unbuilt** — its own unit if
   the reference loses.
2. **The ranking's ten misses are unexamined.**
3. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
4. **The `reading` line's span wording needs approval.**
5. **Two stations closer than 125 Hz are not named.**
6. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
7. **Nothing checks that deleting a surface is not deleting a capability** — the
   favourites list is gone and the operator found it by hand.

**If you finish every task, stop and report. Do not start the next unit.**
