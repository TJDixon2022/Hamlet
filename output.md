READ IN THIS ORDER.

A. Hamlet meets the CW requirements: step 5 at 0 of 6, step 2 at 3 of 5 and step 3 at 3 of 6 by
   the plan's checkboxes, steps 0 and 1 done, 4, 6, 7 and 8 not started.
B. Step 5, criterion 5.1, HM-REQ-030 with HM-REQ-010, 011 and 012 as guards: the search ran
   8 to 40 and still runs 8 to 40. 5 to 45 was built and not kept. Under it, 5 WPM reads (shown 5,
   was 8) and 45 WPM reads (shown 44, was 36; its text was already right at HEAD). R78 refused it on
   the real set: MET-CER-SURE 45 -> 50, MET-INVENTED 45 -> 50, coverage 374 -> 369. At exit all
   three are unchanged: 45, 45 and 374. Decode time on one recording was 5721 -> 4785 ms/min under
   the change, and is 5721 at exit. 5.1 is not ticked, because no change was kept. 5.6 is red on
   17:37.
C. Section 4 raises 2 items; the first (whether 5.1 may be kept one end at a time) is in the way of 5.1, and the second (`135641` absent) is in the way of 5.2, not of a criterion in B.
   This report adds three things. First, which bounds stop each end, by file and line. Second, the
   refused change's numbers. Third, a finding: a synthetic 45 WPM station at the right pitch is
   already read letter for letter at HEAD. That makes the speed ceiling an unlikely sole cause of
   `135641` reading nothing. That recording is not in the tree.

UNIT:       446 - complete at task 3 of 3, none dropped (the 135641 print was not possible, not dropped: the file is not in the tree) - 2026-09-26 02:13
PHASE GOAL: Make Hamlet's CW decoder meet CW_REQUIREMENTS.md, measured on that document's own metrics, one requirement group at a time.
UNIT GOAL:  Make the decoder look for, and read, a station sending anywhere from 5 to 45 WPM without being told the speed, and make nothing it reads today worse.
ADVANCED:   no - the one change built reached both ends on the synthetic cases, but R78 refused it on the real set, so no criterion flipped
NUMBER:     range 8-40 -> 8-40 (5-45 built, not kept); MET-CER-SURE 0.1074 -> 0.1074 (0.1193 under the change); MET-INVENTED 45 -> 45 (50 under the change); decode 5721 -> 5721 ms/min (4785 under the change)
DRIFT:      step 5 1; step 2 2; step 3 0

## 1. What Claude did

**Exit state: complete, 3 of 3 tasks.** The one change was built, judged, refused and kept out of
`src`. Run on QUIVERFULL, project Hamlet, branch `main`. Commits: `5b58d04d` (task 0), `7e9f3161`
(task 1), `c63ded1a` (task 2), and this closing commit. All are pushed.

**Task 0, entry.** HEAD was `83d3897c`, version 1.13.132 -> 1.13.133. PHASE_STATUS names 446 and
CURRENT_STEP 5 in both copies. The runner's `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md`
and `WORK_INSTRUCTIONS.md` were committed as the runner wrote them. The runner's copy had
CURRENT_STEP 2, and the session set it to 5 as instructed. Entry round results:
- build: 0 errors.
- engine carry-forward: 178 of 178 in 376 s.
- app carry-forward: 277 of 278 in 166 s. The one loss, `TheRstIsYoursToCorrectTests`, is 4 of 4
  alone.
- captures: 51 of 51 in 131 s.
- adjudicated: 13 of 13 in 32 s.
- named: 12 of 13 in 70 s, with 17:37 red (banked 46, reads 38).
- real, inferred keys: MET-INVENTED 45 over 473, 0.0951. MET-CER-SURE 45 of 419, 0.1074.
  Sure-and-right coverage 374 over 473, 0.7907. MET-WBE 52 over 113, 0.4602.
- synthetic, exact keys: MET-INVENTED 14 over 252. MET-CER-SURE 14 of 173, 0.0809. Coverage 159
  over 252, 0.6310. MET-WBE 48 over 84.

These match the instruction's table.

**Task 1, the trace.** `TheSpeedSearchReachesBothEndsTests.EveryBoundOnTheSpeedSearch` asserts
nothing. It prints 22 bounds from the source text, with file, line and value, what 5 and 45 WPM
need of each, and which end each one stops.

What the ends need:
- The unit is 240 ms at 5 WPM (48 hops) and 26.7 ms at 45 WPM (5.3 hops).
- The longest dah at 5 WPM is 720 ms (144 hops), and the word gap is 1680 ms (336 hops).

Bounds that stop 5 WPM:
- `SlowestWpm = 8`, `CwProbabilisticDecoder.cs:466`.
- The loop start, `:763`.
- The stream's measured-speed range, `CwProbabilisticStream.cs:432`. A measured 240 ms unit is
  refused, so the grid decides.
- The marks' re-read range, `:510`.
- `CwDecoder.SlowestPlausibleWpm = 6`, `CwDecoder.cs:388`. The operator is never shown a speed of 5.

Bounds that stop 45 WPM:
- `FastestWpm = 40`, `:487`.
- The loop end, `:764`.
- The stream's range, `:433`.

`WpmStep = 2` (`:509`) puts neither end on a grid point.

Bounds that stop neither end:
- The lattice's segment spans. At 5 WPM a word gap's longest span is 739 hops, inside the
  2400-hop window.
- The 12 s window (50 units at 5 WPM).
- The 1 s decision delay (4.2 units).
- The structure count (6 s).
- The refill (3 s).
- The noise span (2.5 s).
- `ShortestRunHops` (2 hops, under the 5.3-hop dit at 45 WPM).
- `FastestPlausibleWpm = 48`.

The Hann integrator spans 33.4 ms, which is longer than a 45 WPM dit. It is a bandwidth, not a
speed bound, and was not moved. `CwToneTracker.FastFistWpm` was left alone. `AutoCall.cs:688`
clamps the transmit keyer to 5 to 60, shares nothing with the search, and was not touched (§0.2).

The generator is `CwFixtureGenerator.Generate` via `SyntheticCq.Recipe`, which makes both 5 and 45.
At HEAD the two HM-REQ-030 tests failed and the 8 and 40 controls passed. The failures were:
- 5 WPM: shown 8.
- 45 WPM: shown 36.

**Task 2, the change** (`.run-unit/unit446-speed-notkept.diff`):
- `SlowestWpm` 5.
- `FastestWpm` 45.
- The search tries `SpeedGrid`: the even speeds 6 to 44 plus the two ends.
- `SlowestPlausibleWpm` 5.

The stream's two ranges and the marks' re-read range move with the constants, because they read
them. Under the change all six speed tests passed. R78 refused it:
- Real MET-CER-SURE 45 -> 50 of 419, and MET-INVENTED 45 -> 50 over 473.
- Coverage 374 -> 369.
- MET-WBE 52 -> 53.
- V-11 found 2 of 35 recordings worse.

Other results under the change:
- adjudicated: 13 of 13.
- captures: 48 of 51.
- named: 11 of 13.
- engine line: 177 of 178 in 373 s.
- app line: 276 of 278 in 166 s. The two losses, `TheChipSaysTheChosenModeTests`, are 6 of 6
  alone.

The change was taken out of `src` and recorded in `docs/phase-requirements/metrics.md`. No second
change was built.

**Task 3, the exit round**, at `c63ded1a` with `src` identical to entry:
- build: 0 errors.
- engine line: 178 of 178 in 380 s.
- app line: 278 of 278 in 166 s.
- captures: 51 of 51 in 130 s.
- adjudicated: 13 of 13 in 32 s.
- named: 12 of 13 in 68 s, with 17:37 red as at entry.
- metrics: exactly the entry figures, and V-11 finds 0 of 35 worse.
- `TheSpeedSearchReachesBothEndsTests`: 4 of 6 in 34 s. The two HM-REQ-030 tests are red, as at
  HEAD, and are on no line.

**What changed in `src`: nothing.** Neither `CwProbabilisticDecoder.cs` nor `CwDecoder.cs` carries
the change. Nothing that keys or transmits was touched. Nothing was ticked:
- 5.1: no change was kept.
- 5.2: `135641` is not in the tree.
- 5.3 to 5.6: not ticked, as instructed.

**Decisions the session made for itself** (author's, overrulable):
1. **The grid keeps its old even points and adds the two ends.** Stepping from 5 would have moved
   every hypothesis onto the odd speeds, which is a different grid, not a wider one.
2. **`CwDecoder.SlowestPlausibleWpm` counts as a bound that stops the 5 end.** It withholds the
   speed the operator is shown.
3. **The speed test judges the operator's shown speed**, as a median over one-second samples taken
   after the first sure letter. It passes within 10 %.
4. **The seeds are 20260926 plus the speed.**
5. **The fixed recording for decode time is `cw-2026-08-18-004507`.**
6. **Both ends were built as one change**, because neither needed more than moving bounds. So the
   refusal cannot say which end, or which moved bound, reddened the real recordings.

**Mismatches, each reported once and none repaired:**
- `cw-2026-09-24-135641` is **not in the tree**. It is in no git history and nowhere under the
  repository. So it was not printed, and 5.2 is unmeasured.
- HM-REQ-031 reads *"After acquisition **on TX-ITU**, the decoder shall report speed within 10 % of
  true (MET-WPM-ERR ≤ 10 %)."* The instruction drops "on TX-ITU". The PARIS cases are TX-ITU
  timing, so nothing changes.
- The instruction says to leave the marks' speed alone. Its range check (`CwProbabilisticStream.cs`
  510) reads `SlowestWpm`, so moving the floor moves it. The trace names it as stopping 5.
- `TheFiveToEightDecibelPlateauHolds` is in neither line, confirmed in
  `docs/carry-forward-tests.txt`. It was not run.
- Known and not the session's:
  - `PHASE_OUTCOME.md`'s header titles are old.
  - `CW_SPEC.md` §11 defines MET-COVERAGE as sure over sent.
  - `PROJECT_STATUS.md` RULES_AT says HM-DEC-165. The instruction says `CLAUDE.md` §1 holds
    CPS-DEC-0183, but a search of `CLAUDE.md` finds no CPS-DEC-0183.

HM-REQ-030 as the document states it: *"The decoder shall meet every must-tier requirement at any
sending speed from 5 to 45 WPM inclusive."*

HM-REQ-033: *"The decoder shall acquire speed at any speed in range without a prior speed to
iterate from."*

HM-REQ-010, 011 and 012 are as the instruction lists them.

## 2. What the owner should expect

No. The operator will not yet see text from a station sending at 5 WPM, and will not see the right
speed for one sending at 45. Nothing read today got worse, because nothing in `src` changed.

The evidence, all synthetic with exact keys (V-13):
- **At HEAD, 5 WPM** is fitted at 8 and printed as `TTC Q CTQ …`, with 35 wrong sure letters for
  21 sent.
- **At HEAD, 45 WPM** is read letter for letter but shown as 36.
- **Under the refused change**, 5 WPM was shown as 5 with 10 wrong for 21, and 45 WPM was shown as
  44.

The change was refused because it made 13 real recordings read differently, most of them worse.
`134712` lost `N4LQ`.

What will look wrong but is not: `TheSpeedSearchReachesBothEndsTests` has two red tests in the tree.
They are HM-REQ-030 unmet, on no carry-forward line.

What the synthetic cases do not prove (§12.5):
- The sends are PARIS-perfect machine keying at 15 dB, with no real sender and no hand-key scatter.
- There is no real channel, no fading and no second station.
- There is no Farnsworth spacing.
- The generator and the decoder share the one-three-seven model.

## 3. What you should see

**The four speed cases.** Each uses the exact key `CQ CQ CQ DE N0CALL N0CALL K`, is started cold at
600 Hz, and is 15 dB in the passband. The speed column is the median the operator was shown after
the first sure letter.

| case | HEAD text | HEAD speed | under the change (not kept) text | speed |
|---|---|---|---|---|
| 5 WPM | `TTC Q CTQ TTCTTTQ DE TT MMET■EATTTT TA TTTT TTTL TNTTEIZI■■CALL K` | 8 | `TTCQ CQ CQ DE NNN■ACALL NNIBIL■CALL K` | 5 |
| 8 WPM | `C Q CQ CQ DE NGWCALL NGGWCALL K` | 8 | `CQ CQ CQ DE NGWCALL NGGWCALL K` | 8 |
| 40 WPM | `CQ CQ CQ DE N0CALL N0CALL K` | 40 | the same | 40 |
| 45 WPM | `CQ CQ CQ DE N0CALL N0CALL K` | 36 | the same | 44 |

The same cases' metrics, exact key, HEAD -> under the change:

| case | MET-CER-SURE | coverage | MET-INVENTED |
|---|---|---|---|
| 5 WPM | 35 of 51 -> 10 of 29 | 16 -> 19 over 21 | 35 -> 10 over 21 |
| 8 WPM | 5 of 24 -> 5 of 24 | 19 over 21, unchanged | 5 over 21, unchanged |
| 40 WPM | 0 | 21 over 21 | 0 |
| 45 WPM | 0 | 21 over 21 | 0 |

At exit, `src` is HEAD's, so the HEAD columns stand.

**`cw-2026-09-24-135641`: unmeasured.** The file is not in the tree. It cannot be printed beside
`nothing read`. At HEAD a synthetic 44-to-45 WPM send at the right pitch reads whole, and is only
shown at the wrong speed. So the recording's silence is more likely the 75 Hz pitch offset (step 4,
R76) than the speed ceiling. This is an inference, not a measurement.

**Real recordings whose text changed under the refused change**, inferred keys. At exit every one
reads as it did before.

| recording | before | under the change |
|---|---|---|
| `134712` | `E ■ NT N4LQ K`, callsign N4LQ | `E ■ NT K ■ LQ K`, no callsign |
| `031838` | `A 3, AT3 , 2TT 2, AND ■ W IAH A MEAN OF 2 TT` | `… W IA■ A MEAM OF TTTTT TTTO` |
| `003126` | `U <BT> I WATCH AT L EAST 2 MOVIESA DAY …` | `U ■T I OETTCH AT L EAST 2 MOVIESA DAY …` |
| `013303` | `G ALL BOXE S F OR FOC …` | `G ALLQ Q OX E S F OR FOC …` |
| `012823` | `E S SE EE TTN T K E M TE E O IN U T E` | `E S SE E TTN T D E M TT T O TTTT U T E` |

Eight more changed, and all are listed in `.run-unit/unit446-text-change.sorted.txt`: `004507`
(spacing only), `032113` (spacing), `032129`, `001952`, `002016`, `012748`, `012922`, `003919`.

**Decode time:**

| measure | before (entry) | under the change | exit |
|---|---|---|---|
| engine carry-forward line | 376 s | 373 s | 380 s |
| app carry-forward line | 166 s | 166 s | 166 s |
| captures type | 131 s | 130 s | 130 s |
| `004507`, 0.50 min, three runs | 5721, 5618, 5831 ms/min | 4849, 4785, 4777 ms/min | src unchanged |
| its median | **5721 ms/min** | **4785 ms/min** | 5721 |

No carry-forward type approached the 300 s cap.

## 4. What's blocking us

1. **Ruling asked: may 5.1 be met by one kept change per end, or must one change carry both?**
   - Reasoning: the change here moved both ends and four dependent bounds together. R78 refused it
     on 13 real recordings while the synthetic set did not move. The damage came either from
     letting a real recording's measured or marks' speed below 8, or above 40, be taken, or from 5,
     6, 42, 44 or 45 winning the grid. Which one it was was not measured.
   - Recommended: the next unit builds the 45 end alone (FastestWpm and the stream's upper range),
     then the 5 end alone, and judges each under R78. The plan's wording, "met at both ends", would
     then be ticked after the second.
   - Rejected: building a variant in this unit to find out, because §7 forbids a second change to
     rescue the first.
   - This is in the way of 5.1.
2. **Ask: put `cw-2026-09-24-135641` in the tree**, with its sidecar, under
   `tests/fixtures/cw/captured/unadjudicated/`.
   - Reasoning: 5.2 names it and cannot be measured without it. It is also the pitch evidence for
     step 4.
   - Rejected: ticking or measuring 5.2 on a synthetic stand-in, which §12.5 forbids as proof.
   - This is in the way of 5.2 only, not of a criterion in B.
