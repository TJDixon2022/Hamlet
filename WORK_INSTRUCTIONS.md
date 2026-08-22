# WORK_INSTRUCTIONS.md

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      Hamlet.sln
  MUST EXIST:      src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs
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

# The W1AW captures — a decoder measured against published truth

**Read first:** `CLAUDE.md` (§0.0, §0.0.1, §12), `SESSION_PROTOCOL.md`,
`OPEN_ISSUES.md` (HM-OPEN-012, HM-OPEN-016, HM-OPEN-022), `DECISIONS.md`
(HM-DEC-090, HM-DEC-091, HM-DEC-095, HM-DEC-103, HM-DEC-105),
`ANALYSIS-cw-2026-08-22-014113.md`, `ANALYSIS-w1aw-arlp034-2026-08-22.md`.

---

## Why this work order exists

On 2026-08-22, 03:18–03:22 UTC, seven 30-second captures were taken of **W1AW
sending ARRL Propagation Forecast Bulletin ARLP034 at its standard 18 WPM on
7.0475 MHz.** The transmitted text is published by the ARRL.

**For the first time in this project, real off-air recordings have a word-perfect,
externally published transcript** — ground truth no fixture built in this
repository could provide. §12.6 says a fixture built from the same
misunderstanding as the code proves nothing; **this one was built by the ARRL.**

Independent analysis of those files found the front end healthy — element counts
within 10 % of measurement in four of seven — and everything above it wrong:

- **Clock.** The source is a constant ~18 WPM. The decoder reported 22, withdrawn,
  22, then 28 four times, each "won out of 8 to 32". **18 never won.**
- **Grouping.** At a 28 WPM clock the character-gap boundary (~82 ms) sits inside
  the true inter-element gap cluster (40–80 ms), so elements are promoted to
  characters. `105EE EE E E E E EE` is a correctly heard dit stream cut at every
  gap.
- **Element counter instability.** `inThis elements` read 95 then 202 against a
  measured flat ~120, in consecutive files of an unchanged signal.
- **Pitch record.** The carrier measures 499.9 Hz ±0.1 in every file; sidecar
  `toneHz` reads 495/500/300/500/475×4, and the log asserts `snrDb 69.5` at
  300 Hz — below the filter passband, where nothing exists. **§0.0.1 breach: the
  sidecar does not record what the decoder ran with.**
- **`tonePeak`/`snrDb` inflation.** Sidecars report 62–78 dB; honest narrowband
  measurement is ~26 dB. **Third independent sighting tonight**; the 01:43 analysis
  showed the arithmetic — a noise reference sampled in the filter skirt, ~30 dB
  low, rather than in the passband.

---

## Standing instruction for this run

A session that needs a ruling normally stops and asks (§9.5). **For this work
order only:** record the question in `OUTPUT.md` under **NEEDS A RULING**, in
HM-DEC-010's options-table form, and **continue to the next phase.** A phase that
cannot proceed without a ruling is **skipped and named as skipped**.

**§12.1's four-part test is unchanged.** Anything touching §0.0, §0.0.1 or what the
display asserts is Tim's without exception.

**No transmit work of any kind. No scanner work** — `BATCH_BRIEF.md` session 2 is
not this work order.

**Do not loosen HM-DEC-095's separation limit, confirmation rule, or plausibility
bounds** to make anything below pass. Those were set from measurements with margin
on both sides.

**The captures are permanent read-only fixtures (HM-DEC-091). Nothing edits a WAV
or a sidecar.**

---

## Before the session starts — Tim's checklist, not the session's

1. Copy the seven pairs `cw-2026-08-22-031838` through `cw-2026-08-22-032129`
   (`.wav` + `.txt`) into `tests/fixtures/cw/captured/`.
2. Save the ARLP034 bulletin text from arrl.org into
   `data/vendor/arrl/arlp034-2026-08-21.txt`, with the URL and retrieval date at
   the top per §4. **Do it before the page rotates.**
3. Copy `2026-08-22.jsonl` to `tests/fixtures/logs/` (phase 5 reads it).
4. Commit.

**If any of these are missing, the session says so and stops rather than reasoning
around the gap (§12.4).** A decoder scored against a transcript nobody vendored is
a guess with a citation.

---

## Overfitting guard — applies to every phase

The seven files are **one station, one speed, one pitch.** Every change must also
keep the synthetic corpus and the earlier captures green.

**A parameter chosen because it makes ARLP034 score well is tuning to the answer
key. A parameter chosen because a mechanism was found and fixed will score well on
ARLP034 and everywhere else.** The report states which kind each change was.

---

## Phase 0 — Ship the ratio penalty first

**Ruled by Tim and diagnosed by the previous session. It goes in before the
harness, so the floors are set on the decoder being kept rather than on one about
to change.**

In `CwProbabilisticDecoder.DecodeAt` the length penalty is

    off = (span - want) / max(want * 0.35, 1)    score = evidence - 0.5 * off²

**The scatter allowed is a share of each kind's own expected length**, so the gap
between characters gets three times the gap inside a character. **The two costs
cross at 1.5 units, not 2**: at a gap of exactly two units the element reading
costs **4.08** and the character reading **0.45**, and the evidence term is
identical for both, so nothing argues back. **That is the promotion described in
the grouping fault above, seen from inside the code.**

**It becomes** `off = ln(span / want) / 0.35`, which puts both crossovers at the
geometric mean, **1.73 units**, and rests on a property of hands rather than
textbooks: **timing error is multiplicative.** Guard the logarithm against a zero
span.

Measured by the previous session, **not to be re-derived**: `2 MOVIES A DAY` where
it read `2 IOVI ES`, `EACH` as one word, **`N4LQ K` on the capture HM-DEC-144
adjudicated as `N4L`**, **`VRR VA` on the one HM-DEC-145 adjudicated as
`VA3VRR`** — and **elements per character unmoved in aggregate.** Both halves are
true and both go in the record.

**`tools\reference-decoder\reference_decoder.py` arrives with this order already
carrying the same change**, so `ItReadsWhatTheReferenceReads` keeps its meaning
and **must still pass.** Confirm the file against the tree; do not re-derive it.
**If the test fails, the port and the reference disagree about something other
than the penalty — say what, and continue to phase 1 with the penalty reverted.**

**Leave the two rejected models in the comments** where the previous session put
them, with what each cost, and add this one's arithmetic beside them.

---

## Phase 1 — The scoring harness: align decoder output to published truth

**Build the instrument before touching the decoder**, so every later phase is a
number and not an impression.

- A test utility that runs the full decoder over a captured WAV and aligns its
  output against the relevant span of the vendored ARLP034 text **by edit
  distance**, reporting per file: **character accuracy, insertions, deletions,
  substitutions, and the accuracy over sure characters only.**
- **`■` and dimmed characters are never counted as wrong against truth** — they are
  the decoder saying "unknown", which §0.0 ranks above a wrong letter. **A sure
  character that disagrees with truth is the §0.0 failure and gets its own
  column.**
- **Print the aligned pairs, not only the totals**, so a failure names its letters.
- Commit the harness **with the numbers the current decoder scores, written into
  the test as floors** (assert `>=` what is measured today, minus nothing). **The
  suite must go green on today's behaviour: this phase measures, it does not fix.**
  The floors exist so every later phase either raises them or is caught lowering
  them.

---

## Phase 2 — The clock: why 28 beat 18 on an 18 WPM machine

**The decisive defect. Fix priority one.**

**Half the answer is already measured and must not be rediscovered.** The previous
session imposed the speed from 11 to 32 words a minute on `cw-2026-08-18-004507`
and found the likelihood **flat at 32.3 to 32.4 across the entire range**, with
elements per character between 2.33 and 2.50. **Nothing meaningfully preferred any
speed.** So "why did 28 score higher than 18" may have the answer "almost nothing
scored higher, and the winner is near-arbitrary."

**Confirm that on these seven files before looking for a different mechanism.**

- Instrument the 8–32 sweep on these files: **per candidate speed, the per-hop
  score** that made 28 "21.0 better than silence" while 18 lost. **The answer to
  why the wrong speed scores higher is the diagnosis; do not patch past it.**
- Candidate mechanisms **to check against the evidence, not to assume**: the
  scorer rewarding more character boundaries (28 WPM cuts more, and more cuts may
  score as more decoded); the dah/dit boundary at 28 folding true dahs into
  acceptable patterns; the sweep scoring against its own segmentation rather than
  against element-length fit.
- **Done means:** the sweep selects **18±1 on all seven files**, `decoderWpm`
  reports it, `TheSpeedEstimateFollowsAChangeWithinAFewCharacters` still passes,
  HM-OPEN-022's withhold-while-reacquiring behaviour is untouched, and **the
  phase-1 floors rise. Record the new floors.**

---

## Phase 3 — Grouping at the true clock

With the clock right, **most of the E/T soup should already be gone.** This phase
measures what remains and fixes only what has a diagnosed mechanism.

- Re-run the harness; update floors.
- The known error classes from the analysis: **`WITH`→`WINH` (inserted dit),
  `OF`→`OOT`, `OTHER`→`OTHYE`.** Align, locate them in the audio, and state per
  class whether it is **segmentation** (an edge decision Hamlet controls) or **air**
  (QSB/interference the truth cannot fix). **Fix the former only.**
- **The element counter's 95 and 202** (files `032050`, `032113`) against a measured
  flat ~120: reproduce, diagnose, fix or file with numbers. **An element counter
  that invents 65 % is upstream of everything and may be the same mechanism as the
  clock fault.**

---

## Phase 4 — The noise reference: `tonePeak`/`snrDb` confined to the passband

- The "noise beside it" reference **must come from inside the filter passband.**
  `FilterBandwidth` and `CwPitch` are already in the sidecar/rig state; derive the
  audio passband from them, and **when they are unknown, say unknown rather than
  measuring in the skirt** (§0.0: a marked unknown beats a wrong number).
- Test against the captures: reported figure **within a stated tolerance of the
  independently measured ~26 dB**, and a regression test that **a reference taken
  ~300 Hz outside the passband can never again produce a figure** — the
  `snrDb 69.5` at 300 Hz log line is the fixture for this.
- **The held-and-decaying semantics of `tonePeak` are display behaviour and stay as
  ruled.** What changes is the measurement underneath it.

---

## Phase 5 — The pitch record is the running pitch (§0.0.1)

**This is the phase named in advance as the one to drop if room runs out**, because
phases 2–4 carry the accuracy.

- The sidecar's `toneHz` must be **the pitch the decoder was actually demodulating
  at the moment of capture** — not a survey candidate, not a stale hold. **If those
  are different quantities, the sidecar names both explicitly.**
- Diagnose the walk to 300 Hz on a ±0.1 Hz carrier using the jsonl (three
  sightings, timestamped) and the captures.
- **If the fix risks the tracker stability rules set by HM-DEC-095, write the
  options table and skip.**

---

## Phase 6 — Ratchet and record

- Final harness run; write the per-file accuracy table into `OUTPUT.md` **and into
  `tests/fixtures/cw/captured/README.md` beside the fixtures**: what each file is,
  what W1AW sent, what the decoder scores as of this commit, and the two analysis
  documents as provenance.
- **Raise every floor to the measured result. Floors only rise.**
- **Bump the version.** Read the current from `Directory.Build.props`, bump the
  patch, report what it moved from and to (HM-DEC-150).

---

## A note on scope

**Six phases is larger than anything that has run cleanly this week.** The
successful units have been five or six tasks. **Phase 5 is named as the drop and
phase 4 is the next most droppable.** Dropping a phase whole and saying so is
correct; half-building one is not.

---

## Parked — do not touch, do not raise

- The window clear, off by ruling. How a sender change is decided — pitch distance
  measured dead.
- A finer speed grid — measured: invents 0.22 at eighteen decibels.
- Dit-scaled scatter — measured: costs five of seven recordings their text.
- The survey ranking admitted bins by loudness, against HM-DEC-095.
- The advice line asserting a cause the app can disprove.
- The sidecar asserting `13 emitted` beside `text nothing read`.
- `FollowSpeed` has no supplier; the reacquiring guard; `HM-OPEN-051`.
- The twenty-eight failing tests, except any a phase moves.
- **HM-DEC-098, HM-DEC-130, HM-OPEN-033, HM-OPEN-007, HM-OPEN-052, HM-OPEN-053,
  HM-OPEN-054.**

**Do not repair unrelated things on the way past — name them in `OPEN_ISSUES.md`
and leave them (§12.6).**

---

## Asks still outstanding

Carried inbound per HM-DEC-139, verbatim until ruled. **Verify against
`OPEN_ISSUES.md` and report anything here that is closed, or open and missing.**

- The likelihood is flat in speed above eleven words a minute.
- Whether a sender change can be decided by pitch distance at all — measured dead.
- Whether the window clear comes back on.
- The advice line asserting a cause the app can disprove.
- The sidecar asserting two incompatible things about one span.
- Whether the sidecar's `text` should include the leading edge.
- The captures from the evenings of the 20th and 21st are not in the tree.
- Thirty seconds since the last character, for mode-follow's guard.
- Whether `RfGain`'s hundred per cent is a defect or the right answer.
- The likelihood gate at 15.0.
- The keying meter's provisional thresholds.
- `FollowSpeed` has no supplier.
- The mark-and-gap witness behind HM-DEC-144 and HM-DEC-145.
- HM-OPEN-052, HM-OPEN-053, HM-OPEN-054, HM-DEC-130, HM-DEC-098, HM-OPEN-033,
  HM-OPEN-007.

**Whether the length penalty becomes a ratio leaves this queue** with phase 0.

---

## What not to do

Standing prohibitions are in `CLAUDE.md`. Cited, not restated: 9.5.1 one branch and
it is `main`, **and every session commits and pushes to it**; no interactive or
destructive git; do not invent a ruling id; do not touch coverage thresholds.

- **Do not tune to the answer key.** *State which kind each change was.*
- **Do not lower a floor.** *Floors only rise.*
- **Do not count `■` as wrong against truth.** *It is the decoder saying unknown,
  which §0.0 ranks above a wrong letter.*
- **Do not tune the 0.35 in phase 0.** *It is the scatter that was already there.*
- **Do not trade HM-DEC-120.**
- **Do not delete `ItReadsWhatTheReferenceReads`.**
- **Do not edit a WAV or a sidecar.**
- **Do not assert a transcript for any capture without published truth**, and **do
  not build a validity scorer** — one reached thirty valid Morse characters out of
  thirty and returned `ETTT TOGATMETTEMTTEEEATEEEMN`.

---

## Reporting

`OUTPUT.md` at the repository root, overwritten and printed. **§12.2 names the four
headings** — **What Claude did**, **What Tim should expect**, **What we should do
next**, **What's blocking us** — with **RECORDED / NEEDS A RULING / STATE** per
§12.2, and **Asks still outstanding** per HM-DEC-139.

**Section 1 opens with the per-file accuracy table against ARLP034**, before and
after, because that is now a number and no longer an impression.

**Section 2 quotes what one capture reads against what W1AW actually sent**, and
says in one sentence whether the operator will see more CW.

**A session on the development computer states in STATE that nothing in its report
is evidence about the radio** (`SHACK_FACTS.md`, HM-DEC-093).

**If you finish every phase, stop and report; do not start the next work unit.**
