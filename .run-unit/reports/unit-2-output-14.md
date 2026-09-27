READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 4 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 2 of 5, 8 0 of 6, 9 3 of 8; steps 2 to 8 still barred by R86.
B. Step 9, criterion 9.4: HM-REQ-129 - fldigi's detection front end,
   (A1) half-dit integrator at the speed in force refused on 004507,
   coverage (and the real set's coverage 403 to 38 and MET-WBE 37 to
   89), (A2) AGC-normalized level refused on 173723, MET-INVENTED (and
   the real set's MET-CER-SURE 33 to 39, synthetic worse on all four);
   nothing committed: real (inferred) MET-CER-SURE 33 of 436 to 33 of
   436, MET-INVENTED 33 to 33, coverage 403 to 403, MET-WBE 37 to 37;
   synthetic (exact) 14, 14, 159, 44 to 14, 14, 159, 44; adjudicated 13
   of 13; V-11 0 of 35 worse at exit (A1 26, A2 11 of 35 while
   screened); gate margin on 014854 0.475 to 0.475 (A1 1.266, A2 0.462
   while screened); the port byte-identical yes; 9.4 open.
C. The findings weighed against A and B: section 4 raises 5 items. Item
   1 is in the way of 9.4. Every mechanism 9.3 named, (A) to (E), has
   now been refused, and what comes next is the next arbiter's to
   decide. Item 2 records this unit's R85 readings. Items 3 to 5 are
   findings about the likelihood model, the parity harness and the
   tree. None is in the way of 9.5, which R86 does not yet reach. None
   asks anything about keying, transmit, or what the product tells the
   operator.

UNIT:       463 - complete at task 4 of 4, none dropped - 2026-09-27 01:50
PHASE GOAL: Hamlet's CW decoder meets every requirement in CW_REQUIREMENTS.md at the condition each names, proved by a test that names it. For now the plan routes everything through step 9: one fldigi technique has to earn a place in ours before any other decoder work resumes.
UNIT GOAL:  Screen the last untried fldigi mechanism, its detection front end, in two fixed forms: a half-dit integrator at the read's own speed, and fldigi's AGC dividing our level. Keep whichever R78 says makes ours read better with no recording worse, and leave the port untouched.
ADVANCED:   no - 9.4 is not ticked. Both forms were screened, refused under R78 and reverted. A1 collapsed coverage on the real set (403 to 38). A2 added sure-wrong letters on both sets (real 33 to 39, synthetic 14 to 33).
NUMBER:     HM-REQ-129 not met; real MET-CER-SURE 33/436 to 33/436 (inferred); synthetic 14/173 to 14/173 (exact); MET-INVENTED real 33 to 33; coverage real 403 to 403; MET-WBE real 37 to 37; the port byte-identical yes
DRIFT:      step 2 1; step 3 0; step 4 1; step 5 1; step 6 0; step 7 0; step 9 3 (was 2)

## 1. What Claude did

**Complete, at task 4 of 4, none dropped.** QUIVERFULL, Hamlet confirmed by the gate (all six
checks held), branch `main`. The session found `SESSION.lock` already there (PID 38940, 00:32:05).
The lock is the launcher's; the session did not take it or release it.

**Tree against the instruction.** HEAD was `f40a97f9`. Both copies of `PHASE_PLAN.md` are
identical: 9.1 to 9.3 ticked, 9.4 to 9.8 open, and R86 at line 342. The port matched `19109b51`.
`IntegratorBandwidthHz` is 45.0, and both envelope paths take their window through
`IntegratorWindow`. 462's three patches, their numbers and the classed texts are present.
**Mismatch:** there is no `.run-unit\watched.rc`. Git status showed `watched.cpu` deleted, and the
runner later rewrote it. The runner's writes were committed as they were at task 0; `watched.cpu`
was rewritten again during the unit and is left as the runner left it. The `RULES_AT`
disagreement and `outcome-read`'s step titles for steps 2, 3 and 8 are logged here as the
instruction states them. They are not repaired, and `outcome-read` was not run.

**Task 0.** Record `## UNIT 463 - STEP 9` in both `PHASE_OUTCOME.md` copies; `PHASE_STATUS.md` names
463 at `CURRENT_STEP: 9` in both copies; version 1.13.149 to 1.13.150. Entry round: every figure as at
462's exit (section 3, item 4). The app line lost two `TheCqPressWritesTheLabelTheOperatorPressed`
cases to Avalonia's headless "dispatcher loop" on the first run, and passed 278 of 278 on the one
rerun under DECIDED (8). Saved: ours' texts (`unit463-text-before.txt`, 126 sorted lines, identical
to 462's), each character's class and verdict through 462's committed fact
(`unit463-text-before-classes.txt`, byte-identical to 462's), the port's texts
(`unit463-port-before.txt`, byte-identical to 462's), and the 35-row V-11 table
(`unit463-v11-before.txt`, identical to 462's).

**Task 1.** `WhatFldigisFrontEndWouldLiftFact` (HM-REQ-129) prints the two chains, the lift at
group (a)'s marks under three envelopes and 014854's gate room, then fixes both forms. It asserts
nothing and changed nothing under `src`. Its output is `.run-unit\unit463-frontend.txt`. The
instruction's reading of fldigi's time constants is confirmed from the source: `decode_stream`
runs once per decimated sample, at 500 a second (704-710), and `decayavg` is
`avg + (x - avg) / weight` (`misc.h` 59-63). So attack 200 is 0.4 s and decay 1000 is 2 s, which
is 80 and 400 of our 5 ms hops.

**Task 2.** Each form was built in `CwProbabilisticDecoder` and `CwProbabilisticStream` in the
form task 1 fixed, and judged in the working tree. The judging covered the build, the adjudicated
readings, the metrics, the V-11 table against task 0's, the silence types, the captures floor,
the gate room through the stream, and a parity run for decode time and the port. Each patch and
its numbers were saved, then the tree was reverted with `git checkout -- src`, and
`git diff -- src` printed nothing. **Both were refused** (section 3). A2 was built from the clean
tree after A1's revert. Neither was narrowed, re-edged or re-timed after its numbers were seen.

**Task 3.** Nothing was kept, so no test was written, nothing under `src` was committed and 9.4 is
not ticked. The two patches and their numbers are committed as evidence (task 2's commit), and
`.run-unit\unit463-refusals.txt` records each form's refusing recording and metric.

**Task 4.** The exit round matches entry on every figure (section 3, item 4).

**Decisions the session made for itself:**
1. Ours' per-character classes were taken at task 0 through 462's committed
   `WhatFldigisEdgesWouldMoveFact`, run by name, which is how 462 took them.
2. The A1 envelope is centred on each hop, aligned to where the 45 Hz integrator of that hop is
   centred (offline: centred; stream: the trailing Hann's centre). fldigi's own filter runs behind
   the audio. Centring keeps the marks on the hops that the speed measurement, gap measurements,
   relabel and `GapsFitTheUnit` still read from the 45 Hz window. Decimated samples beyond the
   audio held count as nought, which is how the offline envelope already treats the edges.
3. The audio is brought to 8 kHz for A1 by averaging the mixed samples that fall in each 8 kHz
   sample. fldigi's own input is resampled to 8 kHz upstream of `cw.cxx`.
4. The low-pass is normalised to unity gain at DC. fldigi normalises to its peak response, which
   is the same point for this low-pass. The likelihoods are scale-free, so the gain decides
   nothing.
5. Under A2 the AGC runs continuously in the stream, as fldigi's does. It starts at fldigi's
   constructor values and is reset only on `Restart`. Offline it runs from the file's first hop.
6. The gate room is measured two ways. Task 1 used a whole-file and windowed offline read, the way
   `TheIntegratorBandwidthTable` measures it. Task 2 added `TheEmptyBandThroughTheStream` to the
   fact, which drives the stream itself and takes the highest ratio any read scored. That added
   method is test code and went into task 2's commit. The "before" was measured on the committed
   tree after A1's revert and appended to `unit463-A1.txt` by `unit463-gatefix.sh`.

## 2. What the owner should expect

Nothing changes on the screen: every recording reads exactly as it did yesterday, letter for letter,
and the port reads exactly as it did. Neither of fldigi's two front-end ideas made our decoder read
better. Listening to each mark over half a dit at the sender's own speed made Hamlet print almost
nothing: 38 correct letters on the real recordings where it prints 403 today, with `VA3VRR` and
five other checked readings gone. Levelling the signal the way fldigi's AGC does came closest. It
kept all 13 checked readings, and printed one more letter right on each of `004507`, `032129` and
`004427`. But it printed six more wrong letters on the real set and nineteen more on the generated
practice set, `CQ CQ CQ DE N0CALL` at 5 dB among them. The rules do not trade a wrong letter for a
right one. What will look wrong but is not: `TheSpeedFollowsTheSendersMarkPairsTests` still fails
at 28 of 31, as unit 459 left it, and it is on neither carry-forward line.

## 3. What you should see

No visible change. This unit screened two forms and kept neither, so the decoder and every text
it prints are as at entry.

**The screen table.** Real is 23 keyed recordings with inferred keys; synthetic is 12 cases with
exact keys. Each cell reads MET-CER-SURE / MET-INVENTED / coverage / MET-WBE.

| form | real before | real with it | synthetic before | synthetic with it | adjudicated | V-11 worse | gate room on 014854 | decode, ours over 690 s | verdict |
|---|---|---|---|---|---|---|---|---|---|
| A1 half-dit integrator at the read's speed, `cw.cxx` 352-361, 396, 428-431, 696-708 | 33/436, 33, 403/473, 37 | 13/51, 13, 38/453, 89 | 14/173, 14, 159, 44 | 1/85, 1, 84, 60 | 7 of 13 | 26 of 35 (every recording that moved, except `cq-18wpm-15db-char5` and `003758`, which improved) | 0.475 to 1.266 | 51.47 s to 72.92 s | **refused** on real coverage and MET-WBE; synthetic coverage and MET-WBE |
| A2 AGC-normalized level, `cw.cxx` 599-632 | 33/436, 33, 403/473, 37 | 39/442, 39, 403/473, 37 | 14/173, 14, 159, 44 | 33/182, 33, 149, 46 | 13 of 13 | 11 of 35: `cq-12wpm-5db`, `cq-18wpm-15db-char5`, `cq-18wpm-5db`, `cq-18wpm-5db-char5`, `cq-25wpm-5db`, 031905, 032050, 032113, 173723, 004108, 004322 | 0.475 to 0.462 | 51.47 s to 51.64 s | **refused** on real MET-CER-SURE and MET-INVENTED; all four synthetic |

Under A1, 17:37 read nothing of the key's opening, so it has no scored stretch; that is why real
"sent" falls to 453. Silence held under both forms: `TheSilencePropertyIsLockedTests` 6 of 6,
`NothingIsReadFromAudioWithNoKeyingTests` 4 of 4, both empty-band capture rows green, and nothing
settled on 014854 or 014935. The captures floor, run for silence, fell to 6 of 51 under A1 and to
31 of 51 under A2. Every row and figure is in `.run-unit\unit463-A1.txt` and `unit463-A2.txt`.

**1. Task 1's two chains, and the lift.**

| stage | ours, at HEAD | fldigi, `cw.cxx` at `61b97f41` |
|---|---|---|
| mixer | quadrature mixdown at the audio's rate (`CwProbabilisticDecoder.cs` 930-940; `CwProbabilisticStream.cs` 256-270); no low-pass but the integrator | complex mixdown at 8 kHz (688-692), then a 1024-tap Blackman sinc low-pass (`fftfilt.cxx` 128-171): 150 Hz by default, 5 x WPM / 1.2 only under `CWmfilt`, which is off by default (352-356, 395-398) |
| integrator | Hann of 33.4 ms, 45 Hz ENBW, fixed by `IntegratorBandwidthHz` (416, 593-635, 916-974) | decimate by 16 to 500/s (704), magnitude, boxcar of symbollen / 32: half a dit, 29 Hz at 17.1 WPM, set by `CWspeed` (358-361, 428-431, 708) |
| level | none: sigma = the quarter point / 0.7585, keyed = the 97th percentile, over 2.5 s centred, re-taken every 62 hops (1004-1154) | `decayavg` trackers: signal at decay, floor and peak at attack 0.4 s / decay 2 s (599-623); value / peak (629-632) |
| speed vs envelope | chosen from the same 45 Hz envelope: the measured unit, else the grid, then the marks' overrule (`CwProbabilisticStream.cs` 442-449, 516-531; 768-783) | tracked after detection (524-535, 831-843); the front end follows the operator's `CWspeed`, not the tracked speed (395-396, 416, 428) |

Lift on group (a)'s marks, as median and peak in each envelope's own noise sigmas against that
envelope's keyed level. The strips are in `unit463-frontend.txt`.

| departure | ours | A1 | A2 |
|---|---|---|---|
| 17:37 R (read E, 29.195 s), 17.1 WPM | marks 3.2-3.5 of keyed 3.7; nothing above noise where the missing dah belongs | marks 1.9-2.2 of keyed 2.2; nothing there either | ours to one decimal (keyed 3.8) |
| 17:37 D (read I, 30.000 s) | as above, keyed 3.7 | keyed 2.2 | keyed 3.8 |
| 032012 O (read T, 1.765 s), 22.9 WPM | three dahs at 5.8 of keyed 5.8 in the whole-file envelope | 2.1 of keyed 2.1 | 5.0-5.9 of keyed 7.1 (the peak still falling from the burst) |
| cq-18wpm-15db-char5 L (read E), 17.8 WPM | 12.7-13.6 of keyed 14.3 | 7.1-7.7 of keyed 7.9 | 11.9-13.3 of keyed 13.8 |
| cq-18wpm-5db-char5 0 (read U), 18.5 WPM | 4.3-5.0 of keyed 5.5 | 2.1-2.6 of keyed 2.7 | 3.9-5.2 of keyed 5.6 |

**The prediction matched the screen.** Task 1 found that neither form lifts the marks 17:37 is
missing. It also found that A1 pulls every keyed level down toward the noise in the likelihood's
own sigma terms, and the screen found A1 printing almost nothing. A2 differed from ours only where
the peak was moving, and the screen found A2 moving a handful of letters each way.

**2. Every recording the kept change moved:** none. Nothing was kept. Each screened form's moved
rows are in its `.txt`.

**3. How the kept change's test was watched failing first:** no kept change, so no test was
written.

**4. The commit table.** Build, both carry-forward lines and the three floor tests at each commit.

| commit | what | build | engine line | app line | named | captures | adjudicated |
|---|---|---|---|---|---|---|---|
| `41f445ef` | task 0: record, bump, entry printouts | 0 errors | 178/178 | 278/278 (rerun, DECIDED (8)) | 13/13 | 51/51 | 13/13 |
| `06bcd261` | task 1: the fact and its printout | 0 errors | not re-run | not re-run | not re-run | not re-run | not re-run |
| `90713038` | task 2: A1 and A2 patches and numbers | 0 errors (revert build) | not re-run | not re-run | not re-run | not re-run | not re-run |
| `17e1ae57` | task 3: the refusal record | not re-run | not re-run | not re-run | not re-run | not re-run | not re-run |
| (task 4) | exit printouts, output.md | 0 errors | 178/178 | 278/278 | 13/13 | 51/51 | 13/13 |

`src` is identical at every one of these commits (`git diff f40a97f9 -- src` prints nothing).
The only test added is the fact, which is on no carry-forward line. The "not re-run" cells were not
measured at that commit; they rest on unchanged `src` and unchanged test lines between two green
measurements.

Exit figures beside entry: build 0 errors; engine 178 of 178; app 278 of 278 (first run, no
dispatcher loss); named 13 of 13; captures 51 of 51; adjudicated 13 of 13; metrics real 33/436,
33, 403, 37 (29 ins, 8 del) and synthetic 14/173, 14, 159, 44 (13 ins, 31 del), as at entry; both
decoders scored alike 5 of 5; the port's tests 8 of 8; `TheSpeedFollowsTheSendersMarkPairsTests` 28
of 31 sure wrong or added, red as expected; ours decodes 690 s in 52.45 s (51.47 at entry).
`git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/` prints nothing. The port's texts are
byte-identical to task 0's save, 74 lines. `git diff 7e209cb4` over the eleven transmit files
prints nothing. Our texts are identical to task 0's, 126 lines. `.run-unit\fldigi\` is still
untracked. Everything is in `.run-unit\unit463-tx-exit.txt`.

## 4. What's blocking us

Most-blocking first.

1. **Every mechanism 9.3 named has now been refused at 9.4, and R86 still bars steps 2 to 8.** This
   is a plain fact, and what happens next is the next arbiter's to decide. Each mechanism, with the
   recording and metric that refused it:
   - (A) detection: A1 on 004507, coverage (43 to 0; real coverage 403 to 38); A2 on 173723,
     MET-INVENTED (sure added 0 to 2; real MET-CER-SURE 33 to 39). Unit 463.
   - (B) the spike rule: every real metric worse, three recordings worse. Unit 462.
   - (C) the two-dot class edge: 031838, MET-WBE. Unit 462.
   - (D) pair speed tracking: 031948's adjudicated reading broken, three recordings worse. Unit 459.
   - (E) 880-888, inside W: 032113, coverage; 031838 and 004550, MET-WBE. Unit 462.

   No ruling is proposed here and no new technique.
2. **This unit's R85 readings, recorded and carried on with.**
   - The speed in force is the speed the read is decoded at, as 462 read it. fldigi's own front end
     follows the operator's configured `CWspeed`, not its tracked receive speed (395-396, 416, 428),
     and ours has no operator speed.
   - The 5 x WPM / 1.2 Hz low-pass is fldigi's matched-filter option. Its default is a fixed 150 Hz
     (`CWmfilt` off). A1 took the instruction's form, as DECIDED (2) fixes it.
   - fldigi's weights of 200 and 1000 decimated samples at 500 a second become 80 and 400 hops of
     5 ms.
   - The AGC division sits between the envelope and `LogLikelihoods`: the lattice, grid included,
     scores the divided window, and every timing measurement reads the raw one.
3. **Finding: our likelihood's noise scale assumes a Rayleigh envelope.** sigma is read from the
   quarter point by an identity that holds for one quadrature pair of noise. fldigi's half-dit
   boxcar averages many decimated magnitudes, and that narrows the noise's spread around its mean,
   so the same identity reads the noise as higher against the marks. Keyed levels fell from 3.7 to
   2.2 sigma on 17:37 and from 14.3 to 7.9 on a 15 dB case. That, and not the bandwidth itself, is
   what the task 1 print shows behind A1's collapse. This is recorded as a finding, and nothing is
   proposed from it.
4. **Finding: the parity harness's "port byte-identical" check can move without the port.** Under
   A1 one port line disappeared from the save: 17:37's `CQDW SRED`. The port was untouched. The
   harness prints the port's text over the stretch ours scored, and under A1 ours scored none on
   17:37. A later unit judging the port by this file should know that.
5. **The tree: no `.run-unit\watched.rc`, as unit 459 also found.** The `RULES_AT` disagreement and
   `outcome-read`'s step titles for steps 2, 3 and 8 are logged, not repaired, and not this unit's.
