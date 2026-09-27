READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 4 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 2 of 5, 8 0 of 6, 9 3 of 8.
B. Step 7, criterion 7.1: CH-* profiles produced 10 of 10 with values,
   CH-MDV refused by name; proved on their own audio - spread 18 of 18
   paths (9 of 9 profiles), power 9 of 9, delay 9 of 9, SNR in the 2500 Hz
   reference 3 of 3 within 0.5 dB, V-06 held; recipe in channels.md
   written; 7.1 ticked; 7.5 ticked.
   Task 3 ran: HM-REQ-013 on CH-AWGN +15 dB not met;
   HM-REQ-040 at -4 dB on CH-LM not met, CH-MM not met;
   HM-REQ-041 at -1 dB on CH-HM not met; one point each, no floor
   claimed.
C. The findings weighed against A and B: section 4 raises 7 items. None is
   in the way of 7.1 or 7.5, which are both ticked. Two bear on HM-REQ-040
   and 041 being measured as sweeps. Item 1: on every CH-AWGN case both
   decoders lose the first character at the 1.0 s lead-in, on our audio and
   on the untouched generator's, so any sweep's reading of HM-REQ-013 or 020
   carries it until that is ruled on. Item 3: on the fading profiles our
   decoder is mostly sure and wrong even at +15 dB, so a sweep today would
   find no floor for 040 or 041 at any SNR. Neither stops the sweep being
   run. None of the 7 asks the owner anything about keying, transmit, or
   what the product tells the operator.

UNIT:       461 - complete at task 4 of 4, none dropped - 2026-09-26 22:27
PHASE GOAL: Hamlet's CW decoder meets every requirement in CW_REQUIREMENTS.md, each proved by a test that names it, at the condition the requirement names.
UNIT GOAL:  Give the test fixtures every fading channel CW_SPEC.md 9.1 gives numbers for, plus CH-AWGN, over the project's noise band and at an SNR stated in the 2500 Hz reference. Prove each one on its own audio against the published model, write the recipe down, and take one first reading of our decoder on the must profiles. Nothing under src changes.
ADVANCED:   yes - PHASE_PLAN.md 7.1 is ticked in both copies. All ten profiles with values are produced and proved on spread, power, delay, SNR and V-06, CH-MDV is refused by name, and channels.md is written. 7.5 is ticked too: all four commits exited green on the five, and the exit round agrees.
NUMBER:     CH-* profiles produced and proved 0 -> 10 of 10; in-passband to 2500 Hz reference offset -8.93 dB at 600 Hz (-8.66 dB at 615 Hz); HM-REQ-040/041 at their SNR not met on CH-LM, CH-MM and CH-HM; real MET-CER-SURE 33/436 to 33/436 (inferred, unchanged)
DRIFT:      step 2 1 (460 said 0; the ledger judged 460 ADVANCED: no); step 3 0; step 4 1; step 5 1; step 6 0; step 7 0 (advanced); step 9 1

## 1. What Claude did

**Complete at task 4 of 4, none dropped.** Task 3, the drop candidate, ran.

- **Where.** Development machine, Claude Code. `PROJECT: Hamlet` confirmed by the gate: the four
  files are present, there is no `CoreHMI.sln` or `MURC.sln`, and the root is `C:\Source\HamLet`.
- **Branch.** `main`, pushed after every commit.
- **The lock.** `SESSION.lock` was already there (PID 34860, 20:43:54). It is the launcher's; the
  session neither took it nor released it.

**The tree against the instruction.** Mismatches are reported, not repaired.
- **Matched.** HEAD was `e6140891`. Both copies of the plan had 2.1, 2.2, 2.3 and 2.5 ticked, and
  2.4 and 7.1 to 7.5 open. `ShapedNoise` shapes to 350 to 870 Hz at 8000 Hz, and
  `SignalToNoiseDb` is set in the band's own bandwidth. Nothing produced a CH-* profile.
- **No `watched.rc`.** There was no `.run-unit/watched.rc`. The runner had deleted
  `.run-unit/watched.cpu`; task 0 committed that deletion. The runner then wrote the file again
  during the session. The exit commit carries it as it stands.
- **The carry-forward line.** `tools/run-carry-forward.sh` carries an older engine list of 133
  tests. The line is the list in `docs/carry-forward-tests.txt`: 178 engine tests and 278 app
  tests. Every figure here uses the document's list, read by `.run-unit/unit461-cf.sh`.
- **A `STOP` file.** An empty `STOP` appeared at the root at 21:01, during task 0. By the
  runner's 089 design this is the owner ending the night: it is read between units, never
  mid-unit. The unit finished. The file was neither staged nor deleted.

**Task 0 - the record and the entry figures** (`f50b2397`).
- **The record.** `## UNIT 461 - STEP 7` is in both copies of `PHASE_OUTCOME.md`. Both copies of
  `PHASE_STATUS.md` name 461 at `CURRENT_STEP: 7`. The version went from 1.13.147 to 1.13.148.
  The runner's writes were committed as they were.
- **The entry figures, every one as at 460's exit.**
  - build 0 errors;
  - engine line 178 of 178; app line 278 of 278;
  - named 13 of 13; captures 51 of 51; adjudicated 13 of 13;
  - the real set and the synthetic set, every metric as at 460's exit;
  - the generator's guards: 13 of 13 and 2 of 2.
- **The texts.** All 126 lines were saved. They are identical to 460's exit.

**Task 1 - the trace** (`6af44dd7`).
- **The trace file.** `.run-unit/unit461-channel-trace.txt` walks `CwFixtureGenerator` stage by
  stage, with line numbers.
- **The band's equivalent noise bandwidth.** Measured from `ShapedNoise`'s own output: two
  410 s seeded runs through the public `Generate` with the tone at -400 dB.
  - 290.2 Hz referred to the peak, 320.1 Hz at 600 Hz, and 340.1 Hz at 615 Hz.
  - The three biquads predict 293.5, 319.8 and 342.1 Hz.
  - **So the in-passband SNR is 8.93 dB above the 2500 Hz reference at 600 Hz.**
- **Two tools,** both asserting nothing: `CwSpectrum` (the tests' own FFT and Welch average) and
  `TheNoiseBandIsMeasuredFact`.

**Task 2 - the layer, the proof and the recipe** (`2914d1f2`).
- **The layer.** `CwChannel` in the test fixtures. `Generate(profile, snr_2500, message, wpm,
  pitch, seed)` returns the audio, the exact key, and a sidecar that carries the profile's
  status.
- **The proof.** `TheChannelProfilesAreWhatTheySayTests`: 71 of 71. Section 3 has its table.
- **The recipe.** `docs/phase-requirements/channels.md`.
- **7.1 is ticked in both copies.**

**Task 3 - one reading** (`7af558b3`).
- **The fact.** `TheChannelConditionsAreReadFact` decoded 30 cases.
- **The trace.** A V-04 trace of the CH-AWGN +15 dB opening misread, with the fldigi port's
  reading beside ours.
- **The printout.** `.run-unit/unit461-conditions.txt`.

**Task 4 - the exit round.** Every figure is as at entry. Section 3 gives them. 7.5 is ticked in
both copies.

**Decisions I made for myself:**
1. **Three generator methods widened.** `CwFixtureGenerator.KeyEdges`, `ShapedNoise` and `Gate`
   went from `private` to `internal`, so that the layer keys and fills its audio exactly as the
   generator does rather than through a copy. That is the only change to the file.
   `TheSyntheticCqRebuildsTests` compares every committed synthetic WAV byte for byte, and it
   stayed 13 of 13.
2. **The band at half level.** The layer uses the band at 0.5 of its level (RMS 0.01,
   -40 dBFS). At +15 dB reference a faded peak would otherwise clip. The proof shows 0 samples
   clipped on every output.
3. **SNR referred to the tone.** The SNR is referred to the band's density at the tone, not at
   the band's peak, because 8.2 says "beside the tone". At 600 Hz that is -8.93 dB; referred to
   the peak it would be -9.35 dB.
4. **Each path at half power.** Each path carries half the unfaded power (`g/sqrt(2)`). The
   instruction's formula, with unit-power paths, would put the faded signal 3 dB above its
   label, against the instruction's own power check.
5. **A gap in the power check, closed.** Round B of the watched-red builds showed that the power
   assertion measured the tap processes, not the paths rendered into the audio: a render that
   dropped path 2 passed it. I added a check that each rendered path carries its tap process's
   power over the same samples, at the 0.3 dB already stated, and re-ran round B. Power then went
   red on 9 of 9.
6. **A looser identity check.** `TheOutputIsThePiecesSummed` is the proof's own check that the
   output is the pieces summed. Its first run went red on float rounding: the phase was summed in
   another order, and the two differed by 5e-13. It now compares to 1e-9. This is not one of the
   eight measured tolerances.
7. **The judgement for task 3, fixed before its first run.** A requirement is met at its point
   only if each of the three seeds meets every clause.
8. **The seed rule for task 3.** 461400 + 10 x the profile's row + the seed index, written in
   channels.md before the run.

## 2. What the owner should expect

The screen reads exactly as before, because nothing under `src` changed. Every recording decodes
to the same text it did at the start of the unit. What is new is in the test fixtures. The decoder
can now be put through the fading the published HF channel models describe, at a signal level
stated the way the requirements state it:
- quiet, moderate and disturbed conditions;
- at low, mid and high latitude;
- over the same noise band the other fixtures use.

So the owner can ask how our decoder copes with a signal fading the way a real path fades, and get
a number for each named condition. The first answer is not good. On the moderate fading profiles,
at a signal a listener would call loud, it prints mostly confident wrong letters. That was always
true; it was just unmeasured. **What will look wrong but is not:** a new printer,
`TheChannelConditionsAreReadFact`, shows HM-REQ-013, 040 and 041 as "not met". It is a reading and
asserts nothing, and it is on neither carry-forward line, so nothing goes red.

## 3. What you should see

**The profile table.** Measured at exit (deterministic, the same at every run). Delay and spread
are 9.1's figures. Spread is two sigma per path, measured from 2^18 taps. Power ratio is path 1
against path 2; the sum is against the unfaded power. Delay is every mark on the tap-applied
signals. V-06 is the lowest 10 ms block against the band's RMS, at -7 and +15 dB reference; the
limit was -30 dB, and nothing clipped. SNR was measured on CH-AWGN, the one profile the instruction
asks it of. Every other profile sets its level through the same `A` and does not measure it
separately.

| id | delay | spread | status | spread measured, path 1 / path 2 | power ratio (sum) | delay measured | SNR at -7 / -4 / +15 ref | V-06 lowest block, -7 / +15 |
|---|---|---|---|---|---|---|---|---|
| CH-LQ | 0.5 ms | 0.5 Hz | [verify] | 0.5008 / 0.4975 Hz | -0.13 dB (+0.05) | 0.500 ms, 4 samples every mark | - | -8.46 / -8.21 dB, held |
| CH-LM | 2 ms | 1.5 Hz | confirmed | 1.4998 / 1.5066 Hz | 0.00 dB (-0.02) | 2.000 ms, 16 | - | -8.28 / -7.41 dB, held |
| CH-LD | 6 ms | 10 Hz | [verify] | 9.9898 / 10.0360 Hz | 0.00 dB (-0.01) | 6.000 ms, 48 | - | -8.14 / -7.85 dB, held |
| CH-MQ | 0.5 ms | 0.1 Hz | [verify] | 0.1003 / 0.1007 Hz | +0.10 dB (-0.01) | 0.500 ms, 4 | - | -7.02 / -9.67 dB, held |
| CH-MM | 1 ms | 0.5 Hz | [verify] | 0.5004 / 0.5000 Hz | -0.10 dB (-0.01) | 1.000 ms, 8 | - | -8.42 / -8.87 dB, held |
| CH-MD | 2 ms | 1 Hz | [verify] | 1.0063 / 1.0048 Hz | +0.08 dB (-0.03) | 2.000 ms, 16 | - | -7.90 / -8.54 dB, held |
| CH-HQ | 1 ms | 0.5 Hz | [verify] | 0.4994 / 0.4972 Hz | -0.04 dB (-0.04) | 1.000 ms, 8 | - | -8.08 / -8.44 dB, held |
| CH-HM | 3 ms | 10 Hz | [verify] | 10.0576 / 9.9261 Hz | -0.15 dB (-0.02) | 3.000 ms, 24 | - | -8.36 / -8.53 dB, held |
| CH-HD | 7 ms | 30 Hz | [verify] | 29.9442 / 29.9661 Hz | +0.01 dB (-0.02) | 7.000 ms, 56 | - | -8.28 / -8.56 dB, held |
| CH-AWGN | - | - | project baseline | - | - | - | -6.94 / -4.00 / +14.97 dB | -8.88 / -7.07 dB, held |
| CH-MDV | - | - | [read from Annex 3] | **refused by name** | | | | |

Other results of the proof:
- **Doppler shift.** Every path's mean shift was within 0.7 % of its spread (tolerance 5 %). The
  largest was CH-LM path 1, at 0.64 %.
- **Rendered paths.** Each rendered path carried its tap process's power to 0.000 dB.
- **The key.** 10 of 10.
- **Determinism.** 10 of 10.
- **The refusal.** The CH-MDV refusal message names 9.1's silence and 12 item 2.

**1. Task 1's offset and the three R85 readings.**
- **The offset.** The band's ENBW beside a 600 Hz tone is 320.1 Hz measured (319.8 predicted),
  so the offset is **-8.93 dB**. It is -8.66 dB at 615 Hz, and -9.35 dB referred to the peak.
- **Spread** is 2 sigma of each path's Gaussian Doppler power spectrum, with no shift. This is
  the author's reading, taken. The sources are Watterson, Juroshek and Bensema, IEEE
  Trans. Commun. Technol. COM-18(6), 1970, and CCIR Rec. 520 / ITU-R F.1487. None of them is
  vendored.
- **Noise enters after the fading** and is never faded. The author's reading is taken. In the
  Watterson model the paths fade and the noise is the receiver's, and CW_SPEC 8.2 measures it
  in the receiver passband. One addition: "equal power" is read as two halves summing to the
  unfaded power.
- **TX-ITU in the tree.** The recipe defaults are not TX-ITU: 105/283/65/130/280 ms is 013347's
  fist. `SyntheticCq.Recipe` is exactly 1:3:1:3:7 at the key-change instants, and is named TX-ITU.
  Two things ride on it: its 5 ms raised-cosine edges and the recipe's ±3 Hz drift. The layer
  holds the pitch fixed.

**2. How each assertion was watched failing first.** The good layer was saved to
`.run-unit/unit461-CwChannel-good.tmp` and broken on purpose. The whole type was run, and the
layer was restored and checked by md5. No broken state was committed.

**Round A** had seven faults at once and turned 48 red. What each fault did:

| fault | what went red | the measured figure |
|---|---|---|
| spread read as sigma | spread, 9 of 9 | +100 %: 3.0028 Hz for 1.5 |
| noise before the fading | V-06, 6 of 9 | lowest block -44.34 dB on CH-LM (limit -30) |
| SNR in the band's own bandwidth | SNR, 3 of 3 | -9.0 dB off |
| each path at full power | power, 9 of 9 | sum +3.0 dB |
| the key run together | key, 10 of 10 | |
| the noise seed not alone | determinism, 10 of 10 | |
| CH-MDV given invented values | refusal | no exception |

V-06 caught the noise-before-fading fault on 6 of the 9 fading profiles only. The three fast
profiles (CH-LD, CH-HM, CH-HD) fade for less than a 10 ms block.

**Round B** had one fault: path 2 dropped from the render.
- The delay went red on 9 of 9: the path had no onsets.
- **Power stayed green.** Decision 5 in section 1 closed the gap. Re-run, power went red on 9 of 9:
  path 2 rendered at -Infinity dB against its taps.

Printouts: `.run-unit/unit461-channel-watchA.txt`, `-watchB.txt`, `-watchB2.txt`.

**3. Task 3's table.** TX-ITU at 20 wpm, `SyntheticCq.Text`, 600 Hz, three seeds, an exact key.
The figures are the three seeds pooled. In full, per seed, in `.run-unit/unit461-conditions.txt`.

| profile | SNR (ref) | key | MET-CER-SURE | MET-INVENTED | MET-COVERAGE | MET-WBE | texts, the three seeds |
|---|---|---|---|---|---|---|---|
| CH-AWGN | +15 | exact | 3 of 63, 0.048 | 3/63 | 60/63, 0.952 | 0/21 | `<AR>Q CQ CQ DE N0CALL N0CALL K` on all three |
| CH-AWGN | -4 | exact | 7 of 62, 0.113 | 7/63 | 55/63, 0.873 | 1/21 | clean; `<AR>Q CQ CQ DE I ■IL N0CALL K`; `CQ NNQ CQ DE NMOCALL N0CALL K` |
| CH-AWGN | -7 | exact | 0 of 0 | 0/63 | 0/63 | 18/21 | nothing emitted, all three |
| CH-AWGN | -10 | exact | 0 of 0 | 0/63 | 0/63 | 18/21 | nothing emitted, all three |
| CH-LM | +15 | exact | 52 of 65, 0.800 | 52/63 | 13/63, 0.206 | 35/21 | `R E ■ K K Q EMMT T AA L E M A E K`; ... |
| CH-LM | -4 | exact | 58 of 63, 0.921 | 58/63 | 5/63, 0.079 | 32/21 | `TE ART K I KQ EUME IAR ■ M AA K`; ... |
| CH-MM | +15 | exact | 30 of 57, 0.526 | 30/63 | 27/63, 0.429 | 22/21 | `■ ■ CQ CZ DE NG TC SL N O CA D K`; ... |
| CH-MM | -4 | exact | 31 of 49, 0.633 | 31/63 | 18/63, 0.286 | 21/21 | `E ■ CG SN EE N E EL`; ... |
| CH-HM | +15 | exact | 117 of 121, 0.967 | 117/63 | 4/63, 0.063 | 44/21 | `■IEEE IEE EU HSS T T E4IE ...`; ... |
| CH-HM | -1 | exact | 111 of 115, 0.965 | 111/63 | 4/63, 0.063 | 44/21 | `■ ETEE IEEEU HSS I T E4ETT ...`; ... |

The verdicts, each on one point with no floor claimed:
- **HM-REQ-013 is not met.** On every seed the first `C` is read as `<AR>`; everything after it
  is right.
- **HM-REQ-040 is not met** on CH-LM or on CH-MM.
- **HM-REQ-041 is not met** on CH-HM.

**The V-04 trace of the +15 dB opening** (seed 461490). I changed one thing at a time between the
channel's case and the existing generator:
- **Ours** reads `<AR>Q` on every variant but one. That includes the untouched
  `CwFixtureGenerator.Generate` at the set's own 15 dB tier and 20 wpm. The exception is the old
  generator at 615 Hz with drift, at the same in-band SNR.
- **The fldigi port** misreads the opening on every variant: `RQ`, `NQ`, `T`, `EK`, or it drops a
  `CQ`.
- So the channel layer does not cause it. It is the first character at the shared 1.0 s lead-in,
  at a speed (20 wpm) the synthetic set never covered.
- Nothing was lowered. It is item 1 below.

**4. Every commit of the unit, with the five at its exit.**

| commit | task | build | engine line | app line | named | captures | adjudicated |
|---|---|---|---|---|---|---|---|
| f50b2397 | 0 | 0 errors | 178 of 178 | 278 of 278 | 13 of 13 | 51 of 51 | 13 of 13 |
| 6af44dd7 | 1 | 0 errors | 178 of 178 | 278 of 278 | 13 of 13 | 51 of 51 | 13 of 13 |
| 2914d1f2 | 2 | 0 errors | 178 of 178 | 277, one dispatcher-loop loss (`ThePsk31ConversationCardTests.NoSlotClockUnderPsk31AndFt8AndFt4StillShowIt`); rerun 278 of 278 | 13 of 13 | 51 of 51 | 13 of 13 |
| 7af558b3 | 3 | 0 errors | 178 of 178 | 278 of 278 | 13 of 13 | 51 of 51 | 13 of 13 |
| task 4 (this) | 4 | 0 errors | 178 of 178 | 278 of 278 | 13 of 13 | 51 of 51 | 13 of 13 |

The generator's guards were green at entry, at task 2 and at exit: 13 of 13 and 2 of 2.

**The exit round against entry.**
- **The five.** Build 0 errors; engine 178 of 178; app 278 of 278; named 13 of 13; captures 51 of
  51; adjudicated 13 of 13.
- **The real set, inferred key.** Every figure the same at exit as at entry:
  - MET-CER-SURE 33 of 436;
  - MET-INVENTED 33 over 473;
  - coverage 403 over 473;
  - MET-WBE 37 (29 inserted, 8 deleted) over 113.
- **The synthetic set, exact key.** Every figure the same at exit as at entry:
  - MET-CER-SURE 14 of 173;
  - MET-INVENTED 14 over 252;
  - coverage 159 over 252;
  - MET-WBE 44 (13 inserted, 31 deleted) over 84.
- **No row moved.**
- **The guards and the new proof.** Generator guards 13 of 13 and 2 of 2;
  `TheChannelProfilesAreWhatTheySayTests` 71 of 71.
- **Nothing else changed.** `git diff e6140891 -- src` and the diff over `Cw/Second` are both
  empty. `git diff 7e209cb4` over the eleven transmit files is empty, and all eleven are present.
  Our texts are identical to task 0's save (126 lines). `git status` shows `.run-unit/fldigi/`
  still untracked. Printout: `.run-unit/unit461-tx-exit.txt`.

**No visible change in the application.** This unit only gives the tests conditions to measure
against.

## 4. What's blocking us

Most-blocking first. None blocks 7.1 or 7.5.

1. **Both decoders lose the first character of a 20 wpm CH-AWGN case at the 1.0 s lead-in.** This
   happens on the channel's audio and on the untouched generator's alike. HM-REQ-013 at +15 dB
   therefore cannot be met by any fixture built this way, whatever the decoder does after the
   first character.
   - *Proposed ruling:* read V-04's "a fixture the reference decoder cannot read is a generator
     defect" as not reaching this case. The reference decoder misreads the opening on every
     variant, including the existing generator's own output. So this is acquisition
     (MET-TACQ), scored as the decoder's. The channel layer keeps the generator's 1.0 s
     lead-in.
   - *Reasoning:* the misread moves with the seed and the pitch, not with the channel. It
     reproduces on `CwFixtureGenerator.Generate` unchanged, at the set's own 15 dB tier. The
     port's first-element loss at the lead-in is already on record (457's verdict (b)).
   - *Rejected:* lengthening the lead-in or adding a run-up for the channel cases. That would
     hide an acquisition fault from HM-REQ-013 and make the channel cases differ in shape from
     the synthetic set.
2. **The tree's synthetic tiers are not the SNR their labels suggest.** The generator's
   in-passband SNR is in a 340 Hz equivalent bandwidth at 615 Hz. So the set's 15, 5 and 0 dB are
   +6.3, -3.7 and -8.7 dB reference. They are not -7 dB for "0 dB", as a 500 Hz passband would
   make it. `TheRequirementsAreMeasuredTests` already labels them "not restated in the 2500 Hz
   reference".
   - *Proposed ruling:* a later unit restates each synthetic condition label with the measured
     offset. No fixture's bytes change.
   - *Rejected:* restating it here, which is traceability work outside this unit (R80).
3. **On the fading profiles our decoder is sure and wrong, not dim.** At +15 dB reference:
   - CH-LM: MET-CER-SURE 0.80, coverage 0.21;
   - CH-MM: MET-CER-SURE 0.53, coverage 0.43;
   - CH-HM: MET-CER-SURE 0.97, with MET-INVENTED at 1.86 of the characters sent.

   That is the 0.0 failure, and HM-REQ-011's, on every must fading profile. A sweep for HM-REQ-040
   and 041 can now be run, but it will find no floor until the decoder copes with fading.
   - *Proposed ruling:* the fading profiles enter the per-condition metrics as a new condition
     set, beside the real and synthetic sets, when a decoder unit next works HM-REQ-010 or 011.
     They are never counted into those sets' totals.
   - *Rejected:* adding them to the floors now. A number nobody chose does not become a floor
     in the unit that first measures it.
4. **CH-AWGN at -7 and -10 dB reference emits nothing.** It emits nothing at all, not even
   placeholders, on all three seeds. It matches the set's own 0 dB cases (-8.7 dB reference),
   which also read empty. At HM-REQ-020's -7 dB reference, HM-REQ-011 holds only because nothing
   is emitted (MET-INVENTED 0), and HM-REQ-010 has no number (0 sure characters). Logged; it is
   step 2's, and not chased.
5. **Unvendored sources I believe differ from, or add to, 9.1.** From memory, and not used:
   - F.1487 states its test SNRs in a 3 kHz noise bandwidth; CW_SPEC 8.1's 2500 Hz wins here,
     and the two differ by 0.8 dB.
   - F.1487's mid-latitude disturbed NVIS row is 7 ms and 1 Hz.
   - *Proposed ruling:* both are checked when 12 item 2 is vendored, and CH-MDV is built then
     from the vendored row.
6. **V-06's 10 ms test is not sensitive to faded noise on the fast profiles.** "Noise before
   the fading" put blocks 44 dB under the band on the slow profiles, but not on CH-LD, CH-HM or
   CH-HD, whose fades are shorter than a block. The type still went red on that build.
   - *Proposed ruling:* no change. V-06 asks about silence, which it catches everywhere. Where
     the noise enters is proved by construction and by the slow profiles.
7. **`tools/run-carry-forward.sh` is stale.** It runs 133 engine tests, not the 178 of
   `docs/carry-forward-tests.txt`. This unit ran the document's list.
   - *Proposed ruling:* the tool reads its filters from the document, as
     `.run-unit/unit461-cf.sh` does. Logged; not repaired here (CLAUDE.md 12.6).
