READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 3 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 0 of 5, 8 0 of 6, 9 3 of 8.
B. Step 9, criterion 9.2: HM-REQ-123 - both decoders on 23 keyed
   recordings and 12 synthetic cases through the same CwMetrics calls,
   sameness watched failing first; per condition, ours against the port:
   real (inferred keys) MET-CER-SURE 33 of 436 / 62 of 239, MET-INVENTED
   33 / 62 over 473, coverage 403 / 177 over 473, MET-WBE 37 / 86 over
   113 words; synthetic (exact keys) MET-CER-SURE 14 of 173 / 34 of 168,
   MET-INVENTED 14 / 34 over 252, coverage 159 / 134 over 252, MET-WBE
   44 / 56 over 84 words; the port's unclassed output mapped as every
   non-space character sure and as itself, its no-match `*` a
   placeholder, its spaces word boundaries; 9.2 ticked; 9.3 ticked (13 of
   13 covered); 9.4 to 9.8 open as they stand, 9.8 held red by the three
   named floors (443 DECIDED (3)).
C. The findings weighed against A and B: section 4 raises 5 items. None
   is in the way of 9.3, 9.4 or 9.5. Item 1, the mapping, decides the
   port's MET-CER-SURE and is the starting point for 9.5. Item 2 is the
   reading on which 9.3 was ticked; if it is overruled, 9.3 goes back to
   open with its 13 printouts intact, and 9.4 would wait on it.

UNIT:       458 - complete at task 4 of 4, none dropped - 2026-09-26 17:28
PHASE GOAL: Hamlet's CW decoder meets every requirement in CW_REQUIREMENTS.md, each shown by a test that names it; section M adds fldigi's receiver as a second decoder that reads the same audio, is scored beside ours, is calibrated, and only then votes.
UNIT GOAL:  Run Hamlet's decoder and the fldigi port over the same audio of every keyed recording and every synthetic case, score both texts with the same scorer, and table the four requirement metrics side by side, per recording and per condition, in parity.md, stating how fldigi's unclassed output is mapped.
ADVANCED:   yes - 9.2 ticked: BothDecodersAreScoredAlikeTests is green 5 of 5 (sameness watched red at 01b6a651), parity.md is committed with all five parts, and all 35 recordings have a row for both decoders; 9.3 ticked with 13 of 13 wins printed and named
NUMBER:     HM-REQ-123 met; recordings scored by both 23 of 23 keyed, 12 of 12 synthetic; real ours/port MET-CER-SURE 33 of 436/62 of 239 coverage 403/177 of 473; synthetic ours/port MET-CER-SURE 14 of 173/34 of 168 coverage 159/134 of 252; our decoder's text byte-identical yes
DRIFT:      step 2 0; step 3 0; step 4 1; step 5 1; step 6 0; step 9 0 (was 0)

## 1. What Claude did

**Complete: tasks 0 to 4 are done, none dropped.** 9.2 and 9.3 are ticked in both copies of
`PHASE_PLAN.md`. Development machine, Claude Code; the prompt claimed `PROJECT: Hamlet`, and the
tree confirmed it (`SHACK_FACTS.md`, `CwProbabilisticDecoder.cs`, `CW_REQUIREMENTS.md` and
`CW_SPEC.md` present; no `CoreHMI.sln` or `MURC.sln`). Branch `main`, pushed after every commit.
Nothing in this report is evidence about the radio.

**Commits:** `bf9a3f59` (task 0), `8e0ecafa` (task 1), `01b6a651` (task 2, watched red),
`af7f92d6` (task 2, green, 9.2), `3ad6333a` (task 3, 9.3), and the exit commit that carries this
report.

**Where the tree differed from the instruction.** `.run-unit/watched.rc` did not exist, tracked or
not. `.run-unit/watched.cpu` showed as deleted at launch and was rewritten by the runner during the
session; it was committed as the runner left it at task 0. Every other section 3 item held.

**Task 0, entry.** Version 1.13.144 to 1.13.145. `PHASE_STATUS.md` names 458 with
`CURRENT_STEP: 9` in both copies (the runner had written 2). The outcome block went into both
copies of `PHASE_OUTCOME.md`, and the runner's writes were committed as they were.
`.run-unit/fldigi/` was never staged. Results:
- build 0 errors;
- engine line 178 of 178;
- app line 275 of 278, then 277 of 278 on the one rerun, every loss Avalonia's headless
  "dispatcher loop". Under DECIDED (6) each lost type was run alone and passed:
  `Unit376TheTopBandTests` 5 of 5, `TheFavoritesAreUnderTheGreenZoneTests` 3 of 3,
  `TheRstIsYoursToCorrectTests` 4 of 4, `TheRecordNamesTheSubModePressedTests` 12 of 12;
- captures 51 of 51, adjudicated 13 of 13;
- named 10 of 13, with 17:37 at 38 of 46, 032113 at 43 of 45 and 032129 at 42 of 64;
- metrics as 457's exit;
- `TheSecondDecoderIsAFaithfulPortTests` 8 of 8.

126 text lines were saved to `.run-unit/unit458-text-before.txt`, identical to 457's exit.

**Task 1, the trace** (`.run-unit/unit458-scoring-path.txt`).
- **How our text is scored.** Our decoder hands `TheRequirementsAreMeasuredTests.Measure` a list of
  `CwCharacter`. `CwSymbol.Of` (`CwMetrics.cs:48-51`) classes each one as a word gap, a
  placeholder or sure. **Every scored stretch is located in the text, never by time.** That is
  `FromFirst` for 17:37, `Within` for the other 22 real recordings, and `Whole` for the synthetic
  set. `CwCharacter.At` is never read.
- **What the port emits.** It prints through `Print`, where `cw.cxx:665-674` calls `put_rx_char`:
  - `rx_lookup`'s `prt`, which gives `<BT>` for a prosign with the shipped `CW_prosign_display`
    off;
  - `CW_noise`, `*` by default (`configuration.h:249-251`), where the table has no match
    (`cw.cxx:892-898`, `morse.cxx:254`);
  - a space after more than 4 dot lengths of silence (`cw.cxx:909-914`).
- **The join and the stamp.** 456's recorder already times each string (`FldigiCwEmission.InputSample`,
  not fldigi's). So the port's text joins at the one `Measure` call with no stamp added, and `src`
  is unchanged.
- **Measured.** The port puts a right character out 0.30 s after its last mark (median of 18), and
  ours 1.27 s after (median of 21), on cq-18wpm-15db. The pitch of both decoders is tabled for all
  35 recordings.

**Task 2, the table.** `BothDecodersAreScoredAlikeTests` names HM-REQ-123 and drives both
decoders from sample 0 of each file:
- ours hop by hop from 600 Hz, as the metrics do;
- the port through `FldigiRateAdapter`, given the pitch instrument's median (DECIDED (3)).

Both texts go through `Measure` and the same `CwMetrics` calls. Its assertions are about the harness
only:
- **sameness,** our row equal to `TheRequirementsAreMeasuredTests`' own per recording and in total;
- **coverage,** a row for both decoders on every recording.

Sameness was watched red at `01b6a651`, with the expected sure-wrong total one too many. It then
went green at `af7f92d6`. The test writes `docs/phase-requirements/parity.md` with its five parts.

**Task 3, where the port wins** (`.run-unit/unit458-where-it-wins.txt`). A printer,
`WhereThePortScoresBetter`, selects 13 of 35 recordings by one comparison on the four metrics. For
each one it prints both aligned texts with each character's class, and the decoder's own evidence
beside every departure. For each recording, the file names the `cw.cxx` lines behind the
difference. `cw-2026-09-24-135641` is not in the tree. The port was run on the five unkeyed
captures ours reads as nothing, and each is named with the port's text.

**Task 4, exit.** Every figure is as it was at entry, apart from the app line:
- build 0 errors;
- engine line 178 of 178;
- app line 278 of 278 on the first run;
- captures 51 of 51, adjudicated 13 of 13;
- named 10 of 13, with 17:37 at 38, 032113 at 43 and 032129 at 42, the same three held red;
- metrics real 33 of 436, 33 / 473, 403 / 473, 37 (29, 8) / 113, and synthetic 14 of 173, 14 / 252,
  159 / 252, 44 (13, 31) / 84, all equal to entry;
- `TheSecondDecoderIsAFaithfulPortTests` 8 of 8, `BothDecodersAreScoredAlikeTests` 5 of 5.

These printed nothing:
- `git diff 7e209cb4` over the eleven transmit files, all eleven present;
- `git diff 845fd70f -- src/Hamlet.RadioEngine/Cw/Second/`, because no line changed, so there is
  nothing to mark;
- `git diff --stat 845fd70f HEAD -- src`, because no `src` file changed this unit;
- our decoder's text against task 0's save, 126 lines identical.

`git status` at exit shows `.run-unit/fldigi/` untracked (`.run-unit/unit458-tx-exit.txt`).

**Decisions recorded by this session under §12.1:** none. The mapping is the instruction's DECIDED
(2), stated in `parity.md` part 1.

## 2. What the owner should expect

Nothing the operator sees changes: no line of either decoder moved, the port is still wired to
nothing on screen, and every floor, metric and carry-forward line reads as it did at entry. What the
owner gains is the comparison itself. Recording by recording, and in the requirements' own numbers,
it shows whether fldigi's receiver reads the same air better or worse than Hamlet's, and where.

Across the whole corpus the port reads worse on every metric: less than half our coverage on the
real recordings, and about three times our sure-character error. Where it scores better, the
printout says why. Mostly it printed less, or it kept short elements and a sane speed on a few
stretches where ours lost them.

What will look wrong but is not:
- the three named floors red at 38, 43 and 42, held by 443 DECIDED (3);
- `parity.md`'s decode-time rows changing whenever the harness runs, because they are wall clock
  and nothing asserts them;
- the port's MET-CER-SURE on 239 sure characters against our 436, because the port prints far fewer
  characters.

## 3. What you should see

**No visible change.** This unit builds the measurement that every later step-9 line reads from.
The answer it was commissioned for:

| condition | key | recordings | CER-SURE ours | CER-SURE port | INVENTED ours | INVENTED port | coverage ours | coverage port | WBE ours | WBE port | decode s ours | decode s port (+ resample) |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| real HF, all (no CH-*) | inferred | 23 | 33 of 436 (0.076) | 62 of 239 (0.259) | 33 / 473 | 62 / 473 | 403 / 473 (0.852) | 177 / 473 (0.374) | 37 (29 ins, 8 del) / 113 | 86 (25 ins, 61 del) / 113 | 52.6 | 3.7 (+8.9) |
| synthetic, all (not CH-AWGN) | exact | 12 | 14 of 173 (0.081) | 34 of 168 (0.202) | 14 / 252 | 34 / 252 | 159 / 252 (0.631) | 134 / 252 (0.532) | 44 (13 ins, 31 del) / 84 | 56 (24 ins, 32 del) / 84 | 8.4 | 1.5 |

The audio is 690 s real and 280 s synthetic. The finer rows, by named sender on the real
recordings and by level and character gap on the synthetic set, are in `parity.md` part 3. Neither
condition is a `CH-*` condition, and no real capture is counted toward one (7.4).

**1. Per recording, the first rows** (all 35 are in `parity.md` part 2):

| recording | key | CER-SURE ours / port | coverage ours / port | WBE ours / port |
|---|---|---|---|---|
| 17:37 (173723) | inferred | 3 of 20 / 1 of 8 | 17 / 7 of 20 | 7 / 6 of 6 words |
| 013347 | inferred | 0 of 6 / 2 of 3 | 6 / 1 of 6 | 0 / 0 of 1 |
| 134712 | inferred | 0 of 3 / 0 of 2 | 3 / 2 of 3 | 0 / 0 of 1 |
| 003758 | inferred | 3 of 11 / 2 of 10 | 8 / 8 of 11 | 0 / 2 of 2 |
| 012403 (KD0UN) | inferred | 0 of 13 / 1 of 6 | 13 / 5 of 13 | 0 / 3 of 4 |
| 004507 (traffic net) | inferred | 0 of 43 / 3 of 33 | 43 / 30 of 44 | 5 / 4 of 11 |
| cq-18wpm-15db | exact | 0 of 21 / 1 of 19 | 21 / 18 of 21 | 0 / 1 of 7 |
| cq-18wpm-0db | exact | no sure / 5 of 7 | 0 / 2 of 21 | 6 / 5 of 7 |

**2. The mapping** (`parity.md` part 1, 458 DECIDED (2)):
- Every non-space string the port prints is scored sure and as itself.
- Its no-match `*` (`cw.cxx:892-898`, default `configuration.h:249-251`) is a placeholder.
- Its spaces are word boundaries.

Over all 35 files the port printed 613 sure, 24 `*` and 181 spaces. Inside the scored stretches
that was 407 sure, 13 placeholders and 114 boundaries.

**3. Sameness.**

| run | commit | result | what it says |
|---|---|---|---|
| red | `01b6a651` | 3 of 4 in 173 s | only the two totals failed: real here 33 of 436 against expected 34, synthetic here 14 of 173 against 15; all 35 per-recording rows equal |
| green | `af7f92d6` | 4 of 4 in 174 s | |
| exit | exit commit | 5 of 5 in 178 s | |

**4. Latency and pitch.**
- **Latency** on cq-18wpm-15db is port 0.30 s against ours 1.27 s. No span is located by time, so
  this cannot move a stretch.
- **Pitch on the real recordings.** The instrument and our tracker agree within about 2 Hz on 14 of
  23. On 032050 ours sat at 325 Hz against 499.9, on 032113 at 600 against 499.9, and on 031838 at
  525 against 499.9. Ours never measured a pitch on 134712 or 032129.
- **Pitch on the synthetic set.** Construction is 615 Hz ± 3. The instrument gave 614.4 to 617.3,
  and ours read 615 to 625, with none at 0 dB on three cases.

**5. Where the port wins** (13 of 35, all covered, in `.run-unit/unit458-where-it-wins.txt`):
- **Six are wins only because the port printed less.** These are 17:37, 032129 and 004108, plus
  the three plain 0 dB cases. The 0 dB cases gain 2 right characters each at a cost of 11 of 13,
  5 of 7 and 5 of 7 wrong. fldigi has no refusal and keys whatever crosses `CWupper`
  (`cw.cxx:649`).
- **Real reading differences.**
  - Ours lost elements that the port kept. On 031838 ours read `2,` as `T T` (patterns `-`, `-`)
    where the port read `2, 2,`. On 004234 ours read `OU` as `TT`. On 17:37 ours read R and D as
    `E` and `I`, losing the dahs. The port's detector, spike rule and classing are at
    `cw.cxx:610-656`, 515, 818-822 and 847-855.
  - On 003758 ours ran at 40 WPM and cut `AA4` to `EET`. The port held about 24 WPM by pair
    tracking (`cw.cxx:524-535`, 832-843) and read `A4`.
- **Word boundaries.** The port wins only where ours splits words: 004507 `D O T N E T` and 004133.
  On cq-18wpm-5db-char5 its rule of more than 4 dot lengths for a space (`cw.cxx:910`) gives right
  letters and 10 inserted spaces (456's finding, scored).

On 135641: not in the tree, so not run. On the five unkeyed captures ours reads as nothing, the
port printed the following, unscored:

| capture | port |
|---|---|
| 014854 | `G S CNDE*IIAEEEIE M EOP ` |
| 014935 | `SE EEEEEEUIBRUTAU6EN ST4` |
| 014113 | `E IIWPA *TEZ K E ` |
| 014308 | `ETJ* IE` |
| 125941 | `I6MTI I IEU*IE ` |

## 4. What's blocking us

Nothing blocks the next step. Five items are recorded, most-consequential first.

1. **The mapping of the port's output (458 DECIDED (2), the author's, overrulable). Applied, not
   blocking.**
   - **Ruling as applied:**
     - every non-space character the port prints is sure;
     - `*` is a placeholder;
     - spaces are word boundaries.
   - **Reasoning:** fldigi shows every character alike, and an operator reads each as asserted.
     Mapping to dim would hide its errors from MET-CER-SURE.
   - **Rejected:** mapping the port's characters to dim, or to anything but themselves.
   - **If overruled:** the port's MET-CER-SURE, MET-INVENTED and coverage columns change. Ours do
     not. It bears on 9.5, where the port first gets a confidence.
2. **The reading on which 9.3 was ticked. Recorded, not blocking.**
   - **Ruling as applied:** "names from its source what it does differently on that stretch" is
     met by naming the `cw.cxx` lines behind each difference. That naming comes from the aligned
     texts and the decoders' own evidence printed beside each departure: our pattern and speed,
     and the port's representation, speed and `two_dots`.
   - **Reasoning:** the evidence identifies the class of difference on each stretch, such as an
     element lost, a speed off or a gap rule. The criterion asks for the source, not a trace.
   - **Rejected:** a sample-by-sample trace of all 13 in this unit, which would have been 13 units
     of 457's task 2.
   - **If overruled:** 9.3 returns to open with 13 of 13 printed, and the traces become the next
     unit's work before 9.4.
3. **A scorer property the port's text exposes. Recorded in `parity.md` part 5.**
   - **What happened:** `CwScorer.Within` locates each stretch independently with free ends. On a
     text far from its keys, two stretches can land on overlapping characters, as with the port's
     three stretches on 004322.
   - **Ruling as applied:** left as it is, because it is the rule ours is scored by and the scorer
     is not this unit's to change. The table is as tabled.
   - **Effect:** it can only flatter or penalise the port on the ten's multi-stretch rows. Ours has
     no overlap.
4. **The app line at entry lost tests on both runs. Housekeeping.**
   - **What happened:** 3, then 1, each to the headless dispatcher loop. DECIDED (6) was applied:
     each lost type was run alone, and all four passed. At exit the line passed 278 of 278 on the
     first run.
5. **`parity.md` is rewritten by every run of the harness. Housekeeping.**
   - **What happened:** its decode-time rows are wall clock, so the tracked file shows a diff after
     any run. The exit commit carries the exit run's figures.
   - **Proposal:** if that noise is unwanted, a later unit could move decode time to the test
     output only.

**Asks still outstanding**
- **457's section 4 item 1, first made 2026-09-26.** This is the reading on which 9.1 was ticked.
  The state reader judged it not honestly supported. The ask waits on the owner's word. The tick
  sits in `PHASE_PLAN.md` line 302 from `20a13aaf`. This unit logged it and did not re-work it, per
  the instruction's section 7. This unit's table scores every file from sample 0 with nothing
  added, so the port's first-element loss shows in the synthetic rows.

No other ask from an earlier session is waiting on a ruling. Parked items were not touched.
