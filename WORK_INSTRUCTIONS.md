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

# Work instruction 050 — the pitch, the envelope, and the bench

**ISSUED: 2026-08-29. A fresh order, not an amendment. Follows unit 049.**

**Six tasks; task 6 is the drop.**

## The number this unit is judged by

**Precision is 0.766. The phase goal is 0.85. The gap is 8.4 points.**

**Every task reports precision, yield, substitutions, and the distance remaining.**

## Why this unit exists

**Every unit since 044 has been built on an argument. This one is built on two
measurements taken outside Hamlet, on the operator's own captures.**

A standalone Python bench was written this evening — 150 lines, no dependency on
Hamlet — implementing Guenther's 1973 classification from a plain FFT tone
estimate. **It ships in this zip as `cwbench.py`.** What it showed:

### Finding 1 — a plain FFT peak beats the tone tracker

| capture | Hamlet's tracker said | the bench measured | the truth |
|---|---|---|---|
| `cw-2026-08-29-030850` | **850 Hz** | **400.4 Hz** | 399.9 Hz, +53 dB. **850 Hz is 4.4 dB below the band floor — nothing is there.** |
| `cw-2026-08-29-020938` | **800 Hz, NOT MEASURED — "nothing has judged it to be a station"** | **801.3 Hz** | a keyed carrier at 802.7 Hz, 21 dB over the floor |

**Twelve lines — a magnitude peak with parabolic interpolation over an averaged
spectrum — got both right where the tracker did not.** The 850 Hz excursion is the
phantom that produced unit 044's whole premise; the 800 Hz refusal is the station
unit 043 was written about.

### Finding 2 — a run-merging bug that corrupts every duration

The bench's first version dropped runs shorter than a minimum **without merging the
neighbours those runs had been separating.** Dropping a 10 ms blip between two
gaps leaves two adjacent gaps that are then counted separately, **so every duration
after it is wrong.** The symptom was consecutive same-state runs — `s100 s28`,
`P171 P194` — and the decode was unreadable until it was fixed.

**Hamlet does the same kind of short-run filtering. Whether it has the same bug is
unknown and task 2 answers it.**

### Finding 3 — the classical algorithm alone is not better

The bench reads `S#ELEN ODSERKING MODERATE DAAN SIMED FWTRES` where Hamlet reads
`BEEN OBSERVING MODERATE DASH SIZED FLARES SINCE AUGUOT I24`. **Hamlet is better.**

**So this unit does not rip anything out.** The classical classifier is a source of
specific, testable ideas — not a replacement.

### Finding 4 — and the thing Hamlet has that the classics do not

On `cw-2026-08-29-020809`, where Hamlet correctly shows blocks, **the bench emitted
text from noise.** Guenther has no refusal and neither does any classical decoder
in the literature. **The silence property is Hamlet's own and it is not tradeable.**

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches. Trust
the tree over this order everywhere they differ.

From unit 049's report:

- Corpus **yield 0.768, precision 0.766, substitutions 58, deletions 31** over 384
  adjudicated characters. App **519 passing, 0 failing.**
- **The engine has had no completed run in three units** — the host crash has ended
  four. Unit 048 saw 28 failing of 1963 before an abort.
- `CwDecoder.PosteriorTemperature` exists and **ships at 1.0.** Nothing earned a
  different value; **leave it alone.**
- The four `TheSpeedIsFoundAndNotTold` pins were relabelled and are green.
- **`TheSilencePropertyIsLockedTests` is green and unmodified.**
- The evidence term outweighs the duration prior by roughly 2000:1 on sane
  captures and 2 × 10⁹ on `cw-2026-08-17-013347`.

**Record both suites and the corpus score before task 2.**

## Rulings in force

**Transcribed with what was rejected. Do not re-argue either.**

**Tim's rulings, 2026-08-29:**

> **The phase goal is 85% correct CW, precision before yield.**

> **The decoder is not ripped out.** The classical classifier reads worse than what
> ships. Ideas from it are tested in the bench and adopted only if they score.
>
> Rejected: porting AG1LE's Bayesian decoder — **its own author abandoned it**,
> reporting it was never accurate enough for real-world signals and that he had
> never found the cause of its base error rate.

> **Do not break the silence behaviour.** Not tradeable at any price.

> **The only measurement is against real data from the real radio.**

> **FT8, FT4 and every other digital mode are outside this conversation's scope.**

**Standing rulings this unit is bound by:**

- **§0.0 / HM-DEC-009** — never present a guess as a decode.
- **HM-DEC-120** — nothing emitted on audio holding no signal, and no letters from
  a pitch nobody judged to be a station. **Tightened only.**
- **§0.4** — reproduce, then change, then measure.
- **HM-DEC-007** — tested against WAV fixtures.
- **§0.2 / HM-DEC-008** — **no transmit work of any kind.**

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` — `STATE`,
`TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is moving
inside the task. Same every ten minutes while a task runs.

## The measurement rule that governs every task

**Every change is measured with `CwAccuracy` over the whole scored corpus, before
and after.** Every task reports **precision, yield, substitutions, and points
remaining to 0.85.**

- **Precision must not fall. Ever.** A change that lowers it is reverted and
  reported.
- **`TheSilencePropertyIsLockedTests` runs after every task and may not be
  modified.** A task that turns it red is reverted.

## The tasks

### Task 1 — the bench enters the tree, and reproduces the two findings

**`cwbench.py` ships in this zip.** Put it under `tools/` with a note saying what
it is: a standalone reference bench, not part of the application, written to test
an idea before a unit is spent on it.

- **Run it over every capture in the corpus** and record its tone estimate and its
  text beside Hamlet's, in a table.
- **Reproduce finding 1**: on `cw-2026-08-29-030850` and `cw-2026-08-29-020938`,
  report the bench's tone estimate against what Hamlet's tracker committed to and
  against the strongest keyed bin measured from the audio.
- **The eight captures of 2026-08-29 have blocked tasks in seven consecutive
  units. If they are still absent, say so once and run over what exists.**

**The bench is a reference, not a dependency. Nothing in the application may
import it.**

### Task 2 — the run-merging bug, found or ruled out

**Read Hamlet's envelope-to-elements path** and answer, with file and line:

- **Is a run shorter than some minimum dropped?** What is the minimum and where is
  it set?
- **When one is dropped, are the neighbours it separated merged?** If not, **that
  is the bug**, and every duration after a dropped run is wrong.
- **Assert it either way with a test** over a synthetic run sequence containing a
  sub-minimum blip between two gaps — the exact case that broke the bench.

**If the bug is present, fix it, and report the corpus score before and after.**
This is the highest-value line in the unit if it is there, because it corrupts the
durations that every later stage classifies.

**If it is not present, say so plainly and move on.** A clean answer is worth the
task.

### Task 3 — the tone estimate

**Add an FFT-peak tone estimator** with parabolic interpolation over a
time-averaged magnitude spectrum, as the bench does — and **measure it against the
tracker on every capture in the corpus.**

- **Report both estimates per capture**, beside the strongest keyed bin measured
  from the audio.
- **Then measure the decode both ways**: corpus score with the tracker, and corpus
  score with the FFT estimate feeding the decoder.
- **Adopt the FFT estimate only if precision rises or holds.** If it falls, report
  and revert — the tracker has hysteresis the peak does not, and that may be doing
  work on fading signals.

**Admission is not changed in this task.** Whatever decides that a station is
present stays as it is; only the pitch handed to the decoder is in question.

### Task 4 — Guenther's boundaries, tested in the bench first

**Three specific ideas from Guenther 1973, each tested in `cwbench.py` before any
of them touches Hamlet.** The bench is the cheap place to find out.

1. **The dit/dah boundary is not the midpoint** — it sits closer to the dot average
   because the two clusters have different variances.
2. **Space boundaries are conditioned on the preceding element** — character and
   word spaces following a dash are systematically shorter than those following a
   dot, so the boundary is a sloping line in two dimensions, not a threshold on
   duration alone.
3. **The character/word average is fed only by non-symbol spaces that follow a
   dot.** Guenther documents the instability that follows from doing otherwise:
   character spaces outnumber word spaces about four to one, ones slightly over the
   threshold drag the word average down, which lowers the threshold, which
   misclassifies more of them — until the threshold collapses onto the character
   average.

- **Measure each of the three in the bench, separately, over the corpus.** Report
  which improve the bench's own reading and by how much.
- **Then, and only for those that improved it**, state where the equivalent would
  go in Hamlet's duration model and what it would cost. **Do not implement them in
  Hamlet in this task** — the bench measurement is the deliverable.

**Idea 2 is the one this author expects to matter most**, because gap
misclassification breaks characters and words apart and Hamlet's boundary is
one-dimensional. **Measure it rather than assuming it.**

### Task 5 — the engine suite gets a completed run

**The engine has had no completed run in three units. The host crash has ended
four.**

- **Get one completed run**, by whatever splitting, batching or exclusion it takes,
  and **report the number and whether the failing set is byte-identical to unit
  048's twenty-eight.**
- **If the crash cannot be worked around, report exactly what it takes to
  reproduce it** and how far the run gets. HM-OPEN-061 names a narrower class than
  what is happening.

**This is not glamorous and it is overdue.** Three units have reported a number
they could not measure.

### Task 6 — the strongest bench idea, implemented *(the drop candidate)*

**Only for whichever of task 4's three measured best, and only if it improved the
bench's reading.**

- Implement it in Hamlet's duration model.
- **Measure with `CwAccuracy` before and after. Precision must not fall.**
- **Report the corpus score and the distance to 0.85.**

**Dropped whole if time runs out, and the report says so.** Task 4's measurements
stand on their own and the next unit can implement from them.

## Parked — do not touch, do not raise

**FT8, FT4 and every other digital mode**, the digital tab, the digital capture
press, the waterfall.

**The confidence work.** Seven quantities have been measured against correctness
and none discriminates. **Do not add an eighth, do not tune the temperature, do not
touch the emission gate or the character floor.**

Also: admission itself; the lattice's structure; the evidence term's magnitude;
the settings contract and `OwnedSettings`; the scanner and the calling cycle;
`CHANGELOG.md`; the missing `DECISIONS.md` records; the phrasebook and the
recent-places row; the Twin PBT; the answer key's licensing; the dial-move
threshold; the transcript break's wording.

**Both halves are required: do not touch them, and do not raise them.**

A parked item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **No transmit. Nothing keys the radio.**
- **Do not break the silence property**, and **do not modify its lock.** The bench
  emits text from noise; Hamlet must not.
- **Do not let precision fall on any task.** Revert and report.
- **Do not let the application import or depend on the bench.**
- **Do not implement Guenther's ideas in Hamlet before task 4 measures them.**
- **Do not change admission.**
- **Do not add an eighth confidence quantity.**
- **Do not rip out the decoder.**
- **Do not report a score without saying whether it is yield or precision.**
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused push
is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 to `output.md` at the repository root, overwritten
and printed. **Read the file's own section count and follow it.**

**Write `output.md` before you stop, for any reason at all. Do not hold it behind a
regression run.**

**The section that reports measurements leads with task 2's answer — whether the
run-merging bug is present — and then task 3's tone table, tracker against FFT peak
against the measured strongest keyed bin, per capture.**

**The section that says what the owner should expect leads with the precision
number and its distance from 0.85.**

**If you finish every task, stop and report. Do not start the next unit.**
