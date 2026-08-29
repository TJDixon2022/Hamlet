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

# Work instruction 044 — no clock, no letters

**ISSUED: 2026-08-29. A fresh order, not an amendment. Follows unit 043.**

**Seven tasks; task 7 is the drop. This is a long unit by instruction.**

## Why this unit exists

**Three captures, fifty seconds apart, one frequency, one station, one variable.
The best test material this project has ever had.**

At 03:08–03:10 UTC on 2026-08-29 the operator sat on 7.0473 MHz listening to a
W1AW propagation bulletin — **a text published verbatim by the ARRL, so ground
truth exists for it.** He pressed capture three times.

| capture | pitch used | `decoderWpm` | fit vs silence | result |
|---|---|---|---|---|
| `-030850` | **850 Hz** | **withdrawn** | 36.0 | **clean read** |
| `-030940` | 400 Hz | **withdrawn** | **8224.4** | **141 characters, 1 unsure, all `E I S` garbage** |
| `-031024` | 400 Hz | **24** | 11.1 | **clean read** |

**Measured outside Hamlet, on the audio in those files: the station sits at
398.4 Hz, 35–36 dB over the band floor, in every one of the three. At 850 Hz the
energy is 4.4 dB *below* the floor — there is nothing there at all.**

So the pitch was **correct** in both the disaster and the recovery. **Pitch is not
the variable. The speed clock is.**

### The fault

**When the speed clock is withdrawn, the decoder keeps emitting characters at full
confidence.** `-030940` produced 141 characters with **one** marked unsure. `E`,
`I` and `S` are one, two and three dits — a decoder chopping the envelope into
short elements because it does not know how long an element is.

**A decoder that does not know the sender's speed cannot tell a dit from a dah.**
`decoderWpm withdrawn` is Hamlet saying exactly that, in its own sidecar, while
letters reach the screen anyway. **This is HM-DEC-009 broken in the same shape
unit 036 fixed for pitch, on the other half of what a decoder must know.**

### The second fault, and it is worse

**The confidence figure is inverted.**

- The garbage scored **8224.4 better than silence per hop**, against a gate of 1.
- The clean bulletin read scored **11.1**.

**The number that is supposed to say how well the decoder is doing rose by three
orders of magnitude as the output became worthless.** Whatever it measures rewards
chopping the envelope into single dits. Its `spanLlr` entries run into the
hundreds of thousands.

**The prime directive holds only if the confidence numbers mean something. Here
they do not.**

### The third fault, already known and now seen again

`-030850` produced a **clean, correct read** — `AUGUST 27, 2026, BY F. K. JANDA,
OK1HH <BT> MORE OR LESS IN LINE WITH EXPECTATIONS` — **from 850 Hz, where the
audio holds nothing.**

**That is `N4L` again.** Unit 036 recorded the mechanism: a right answer obtained
the way the phantoms are obtained, certifying a mechanism that produces junk
everywhere else. **It has now happened twice, on different captures, months
apart.**

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches.
Trust the tree over this order everywhere they differ.

**This author has not seen unit 043's report and does not know whether it ran.**
**Task 1 must establish which refusals are present at the emit seam before task 2
adds another**, because 043's task 2 changes that same seam. **If 043's refusal is
absent, say so and build this one beside where it would go, not on top of it.**

**Record the failing counts for both suites before task 2.** Unit 041 last
reported engine 28 of 1916 byte-identical, app 509 of 509;
`AConfirmedModeWriteFoldsTheDataVariantTooAsync` is a known intermittent.

**Tonight's captures should be in the tree** — `cw-2026-08-29-030850`, `-030940`,
`-031024`, and the five from 02:05–02:10. **Confirm; if any are missing, say
which.**

## Rulings in force

**Transcribed with what was rejected. Do not re-argue either.**

**Tim's rulings:**

> **Ship the refusal** (2026-08-27, unit 036): Hamlet stops printing letters from
> a pitch the survey admitted no keying at, and `N4L` becomes blocks. **The
> phantoms are the priority.**
>
> **Rejected with it and not to be revisited:** the clock-withdrawn refusal **as
> unit 1.11.33 built it** — measured at 26, 38 and 25 characters off three good
> captures, which was the good case paying for the bad; and raising the gate,
> which fires correctly.

**This unit revisits that rejection and the order says so plainly.** What Tim
rejected was a refusal that cost 89 characters across three good captures. **The
evidence now available did not exist then**: a capture where the withdrawn clock
costs **141 characters of pure garbage carrying one unsure mark**, beside a
capture fifty seconds later where the clock is locked and the same station reads
cleanly. **Task 2 measures the cost again against tonight's corpus. If it is still
the good case paying for the bad, the task reports that and ships nothing.**

> **When the frequency changes, clear and reset.**

> **Hamlet sets whatever the radio needs for the mode. The operator does not touch
> the radio.**

**Standing rulings this unit is bound by:**

- **§0.0 / HM-DEC-009** — never present a guess as a decode.
- **HM-DEC-120** — nothing emitted on audio holding no signal, and no letters from
  a pitch nobody judged to be a station. **Tightened only, never loosened.**
- **§0.0.1** — the app's record must distinguish a fault in the signal, the radio,
  or Hamlet. **Every fault in this unit was found in a sidecar. Do not weaken
  it.**
- **HM-DEC-007** — decoders tested against WAV fixtures.
- **§0.2 / HM-DEC-008** — **no transmit work of any kind.**

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` — `STATE`,
`TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is moving
inside the task. Same every ten minutes while a task runs.

## The tasks

### Task 1 — the emit seam as it stands, and the three faults reproduced

**Run both suites whole and record the numbers first.**

**Establish what refusals exist at the emit seam right now** — unit 036's, unit
043's if it ran, and any other. **Name them with file and line.** Task 2 adds to
that list and must not duplicate or contradict it.

Then reproduce, **on tonight's three captures, in tests**, and report before task
2 changes a line (§0.4):

- **`-030940` emits 141 characters with 1 unsure while `decoderWpm` is withdrawn.**
  Confirm the counts from the tree.
- **`-030940` scores 8224.4 and `-031024` scores 11.1.** Confirm both.
- **`-030850` reads correctly from 850 Hz.** Confirm, and **confirm from the audio
  that 850 Hz holds nothing** — this order measures the station at 398.4 Hz at
  +35 dB and 850 Hz at −4.4 dB relative to the band floor. **Correct those figures
  from your own measurement.**
- **Why was the clock withdrawn** on `-030850` and `-030940` and not on
  `-031024`? Name the condition, with file and line. **Measure only.**

### Task 2 — no clock, no letters

**A decoder that does not know the sender's speed cannot resolve a dit from a dah.
While the speed clock is withdrawn, characters are blocks.**

- **Blocks rather than deletions**, as unit 036 ruled, so no character position is
  lost and only the assertion goes.
- **Wire it to the decoder's own existing withdrawal condition** — the one that
  already prints `decoderWpm withdrawn` in the sidecar. **Do not invent a second
  test for the same state.**
- **This is not unit 1.11.33's refusal.** That one was measured at 26, 38 and 25
  characters off three good captures and Tim rejected it on that measurement.
  **Measure this one the same way and report the same table** before declaring the
  task done.

**Report the cost per capture before declaring the task done, not after.** Name
every test that goes red and every capture that loses text.

**Acceptance:**
- `-030940` emits **no letters at all**.
- **`-031024` loses nothing** — it reads `BEEN OBSERVING MODERATE DASH SIZED
  FLARES SINCE AUGUOT I24` with the clock locked at 24 WPM, and that is the whole
  point of the refusal being conditional.
- **The three ragchew captures of 02:05–02:07 lose no more than unit 036 measured**
  — 2, 2 and 7 blocks on the good captures it named.
- **If the cost across the corpus is materially larger than unit 036's, stop and
  report rather than shipping.** That is the same bar Tim applied last time and it
  applies here.

### Task 3 — the confidence figure is inverted

**Measure and report with file and line. Change nothing.**

The garbage scored **8224.4 better than silence per hop**; the clean read scored
**11.1**. `spanLlr` entries in `-030940` reach the hundreds of thousands, and
`-030850`'s tail carries `T:504898.5/7518.5` and `■:-299310.6/99805.6`.

- **What is this figure actually computing**, expression by expression, and **which
  term grows when the envelope is chopped into single dits?**
- **Across every capture in the corpus: report the fit figure beside the fraction
  of emitted characters that are `E`, `I`, `T` or `S`.** If the correlation is
  positive, the metric rewards fragmentation and **that is the size of the
  problem.**
- Is this the same root as the open asks about `013347` returning 17.2 million and
  `001520`'s quadrillions? **Say yes or no with the reason.**

**This is the measurement the next unit is built from. Change nothing.**

### Task 4 — the second `N4L`

`-030850` read a bulletin correctly from a pitch holding nothing.

- **Confirm from the audio** and **report why the survey admitted keying at 850
  Hz** when the station is at 398.4 Hz — 450 Hz away, and outside the 500 Hz
  filter's likely passband centred on a 600 Hz pitch.
- **Record it as an anchor obtained the way the phantoms are obtained**, in the
  form unit 036 used for `N4L`: re-expressed with its reason in the test itself,
  returning as a reading anchor when the station is found honestly.
- **Do not delete the read.** Its text is correct and it is ground truth.

### Task 5 — the bulletin as ground truth

**The W1AW propagation bulletin of 2026-08-27 by F. K. Janda, OK1HH, is published
verbatim.** These captures are the first in this corpus with an external answer
key.

- **Record what the three captures read**, and mark which fragments are confirmed
  against the bulletin's published wording and which are not. **If the bulletin
  text is not in the tree and cannot be obtained from it, say so and record the
  fragments as read rather than as verified** (§0.0).
- Known reads to preserve: `AUGUST 27, 2026, BY F. K. JANDA, OK1HH`,
  `MORE OR LESS IN LINE WITH EXPECTATIONS`, `BEEN OBSERVING MODERATE DASH SIZED
  FLARES SINCE AUGUOT I24`.
- **`DASH` is the sender's `M` read as the word.** Record it as a known
  substitution, not as a correct read.

### Task 6 — regression fixtures from tonight

The three captures become fixtures with their measured truth:

- **`-030850`** — clean text from a pitch holding nothing. **Anchor retired per
  task 4.**
- **`-030940`** — **floor is zero letters.** Carries the fit figure 8224.4 as the
  recorded evidence of task 3's fault.
- **`-031024`** — **floor is what it reads tonight.** This is the capture that
  proves the task 2 refusal is conditional rather than blanket, and **it must never
  regress.**

### Task 7 — the withdrawal itself *(the drop candidate)*

**Measure only. Change nothing.**

Task 1 names the condition that withdraws the clock. This task asks whether it is
the right condition:

- **Across the corpus, how often does the clock withdraw, and on which captures?**
- On `-030850` and `-030940` the best hypotheses were 22 and 28 WPM; `-031024`
  settled at 24. **Was the true speed ever outside the search range**, or did the
  search simply fail to settle?
- The previous session's `-020809` pinned at **40 WPM, the top of the search**, and
  `-030850`'s sweep pinned at **400 Hz, the bottom of its range.** **Report every
  place in the corpus where an estimator lands on the edge of its own search
  space**, since an estimator at its boundary is reporting failure, not a value.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

**The whole digital stream** — the FT8 decoder, the slot cutter, the sync search,
the digital waterfall, the digital capture press.

Also: the joint decoder; the constrained margin; the meter's rebuild; the
integrator width; the whole-file second pass; the scanner and the calling cycle;
`CHANGELOG.md`; the missing `DECISIONS.md` records; the phrasebook and the
recent-places row; the Twin PBT; **the receive-conditions work** — the attenuator
and `CwPitch` belong to unit 043's task 7 and are not this unit's.

**Both halves are required: do not touch them, and do not raise them.**

A parked item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **No transmit. Nothing keys the radio.**
- **Do not change the fit figure in task 3.** Measure only.
- **Do not change admission or the tracker.** This unit is the emit seam and the
  measurements.
- **Do not loosen the silence property.** Only tighten.
- **Do not let `-031024` lose a character.** A blanket refusal that also blocks the
  clean read has failed, and shipping it would repeat exactly the trade Tim
  rejected in unit 1.11.33.
- **Do not delete an anchor.** Re-express it with its reason.
- **Do not assert the bulletin's wording from memory.** If the published text is
  not obtainable from the tree, the fragments are recorded as read, not verified.
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 to `output.md` at the repository root, overwritten
and printed. **Read the file's own section count and follow it.**

**The section that says what the owner should expect leads with this: when Hamlet
does not know how fast the sender is going, it now shows blocks instead of
letters — and the capture where it knew the speed still reads the bulletin.**

**The section that reports measurements leads with task 3's finding** — what the
confidence figure is actually computing, and whether it rewards chopping a signal
into single dits. **That is the fault that makes every other number in the sheet
untrustworthy.**

**If you finish every task, stop and report. Do not start the next unit.**
