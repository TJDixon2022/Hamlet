READ IN THIS ORDER.

A. Phase goal: Hamlet meets the CW requirements. Steps by the plan -
   0 done, 1 done, 2 4 of 5, 3 3 of 6, 4 5 of 7, 5 1 of 6, 6 3 of 6,
   7 2 of 5, 8 0 of 6, 9 5 of 8; steps 2 to 8 still barred by R86,
   and 9.4 open with every mechanism 9.3 named refused.
B. Step 9, criterion 9.6: HM-REQ-120 met - the port reads the
   same chunks live at 8000 Hz (FldigiRateAdapter's own filter,
   streamed), pitch followed by re-construction at our pitch in force
   whenever it moves 30 Hz or more from the port's; 121 met - one
   transcript, no decoder name, the sheet records per character the
   decoder emitted, the case, ours' character and p and the port's
   character and p; 125 / 126 / 127 met each, watched failing first
   against a stub arbiter that emitted ours unchanged (8 of 8 red, then
   8 of 8 green); span rule: an overlap of at least half the shorter
   span, one to one, largest overlap first; margin 0.05, provisional;
   live condition real HF, all, so ours votes yes and the port votes
   no; corpus through the arbiter identical to ours yes (35 of 35, text,
   class and p); decode time 52.22 s to 65.09 s; the port byte-identical
   yes; 9.6 ticked.
C. The findings weighed against A and B: section 4 raises 9 items.
   Item 5 bears on 9.7. The port reads less well live than in
   parity.md, because live it follows our tracker's pitch and is rebuilt
   each time that pitch moves. So 9.7 should measure the live path, not
   the parity harness. That is a choice for how 9.7 is measured, not a
   block on it. Item 2 is the standing reading: R86, with 9.4's
   refusals, holds steps 2 to 8, and only the owner can change how they
   read. It is not a stop here. Nothing touches transmit, and nothing
   the operator sees changes under today's calibration.

UNIT:       465 - complete at task 4 of 4, none dropped - 2026-09-27 06:15
PHASE GOAL: Hamlet's CW decoding meets every requirement in CW_REQUIREMENTS.md at the condition each names, each proved by a test that names it. Until a technique from fldigi is kept in ours, R86 routes every unit through step 9.
UNIT GOAL:  Have fldigi's decoder listen to the same audio as ours, live, and have one arbiter turn the two readings into the single transcript the CW tab shows. Agreement takes the surer decoder's class; disagreement goes to the higher calibrated p; a near tie prints dim, never sure; a decoder not calibrated on the condition only advises. The capture sheet keeps both readings of every character, and every rule is watched failing first on a hand-built case.
ADVANCED:   yes - 9.6 is ticked in both copies. All six of the instruction's conditions hold, each with a test naming its requirement, green, and watched failing first (BothDecodersReadTheSameSamplesTests, TheHigherCalibratedReadingWinsTests, TheOperatorSeesOneTranscriptTests).
NUMBER:     HM-REQ-120 121 125 126 127 5 of 5; spans per condition agree 290 disagree 113 one-sided 486 (438 ours, 48 port) tie 16 on real HF, all (inferred keys), agree 120 disagree 16 one-sided 69 (37 ours, 32 port) tie 2 on synthetic, all (exact keys); arbitrated text identical to ours on 35 of 35; decode time 52.22 s to 65.09 s over 690 s
DRIFT:      step 2 1; step 3 0; step 4 1; step 5 1; step 6 0; step 7 0; step 9 0 (was 0)

## 1. What Claude did

**Complete, at task 4 of 4, none dropped.** QUIVERFULL, Hamlet confirmed by the gate (all six
checks held), branch `main`. The session found `SESSION.lock` already there (PID 40680, 04:05:14).
The lock is the launcher's; the session did not take it or release it.

**Tree against the instruction.** HEAD was `71ac86be`. Both copies of `PHASE_PLAN.md` were
identical, with 9.1, 9.2, 9.3 and 9.5 ticked, 9.4 and 9.6 to 9.8 open, and R86 at line 342.
`git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/` printed nothing. Unit 464's work was all
present: `CwCharacter.Probability`, `FldigiConfidence.cs`, `CwCalibration` and `calibration.md`,
with the held-out verdicts as the instruction states them. There are two mismatches:
- **No `.run-unit\watched.rc`.** The instruction lists it as untracked. It is not in the tree.
- **`watched.cpu`.** Git status showed it deleted at the start. The runner rewrote it before task
  0's commit, which committed it as the runner left it. That commit's message says "deleted",
  which was out of date by then. The runner has rewritten it again since, and it is left
  uncommitted.

Every entry figure was as 464's exit left it (section 3). The only change was decode time, which
moves from run to run: ours took 51.64 s at entry.

**Task 0.** Recorded unit 465 in both copies of `PHASE_OUTCOME.md` and `PHASE_STATUS.md` and bumped
the version from 1.13.151 to 1.13.152. Committed the runner's writes as they were and ran the entry
round. Saved both decoders' characters (text, class and p) through a new printer,
`WhereTheTwoReadingsMeetFact.EachDecodersCharactersWithClassAndP`: 1,467 lines for ours and 818 for
the port. `d3915f01`.

**Task 1.** `WhereTheTwoReadingsMeetFact.BothReadingsOnOneClock` puts both decoders on one clock.
Ours ends at `CwCharacter.At` and starts `SpanHops` × 5 ms before it. The port's span comes from
its up events, placed on the decision rows they were raised in, less the filter's 512-sample lag.
The fact pairs the characters, counts them per condition and names the seams. On 410 agreements
the two clocks differ by a median of −0.008 s at the start and +0.005 s at the end. The span rule,
the vote table and the margin were fixed in the fact's header and the commit message before any
arbitrated text existed. `089c4bb8`.

**Task 2.** Added `CwArbiter`, `CwVoteTable` and `CwArbiter.TieMargin = 0.05`, plus
`CwSecondHarvester` and `CwSecondReading`, which read the port's characters with span and p from
its public record. `CwCharacter.Arbitration` carries the record. Nothing under `Second\` changed.
`TheHigherCalibratedReadingWinsTests` has 8 cases. All 8 were red against a stub `Arbitrate` that
emitted ours unchanged, and all 8 are green on the arbiter. The corpus through the arbiter is
byte-identical to task 0's save of ours on all 35 recordings, and the arbiter's own span counts
equal task 1's exactly. `4c9a8433`.

**Task 3.** Added `CwSecondReader`, which feeds the port the same hops `CwDecoder.Step` reads,
before ours reads them, at 8000 Hz and at our pitch in force. `CwDecoder` has a new `secondReader`
argument. With it on, `CharacterSettled`, `LeadingEdge` and `CharacterDecoded` all pass through the
arbiter. The app builds its decoder with it on, and the capture sheet gains an `arbiter` line.
`BothDecodersReadTheSameSamplesTests` (engine, HM-REQ-120) and `TheOperatorSeesOneTranscriptTests`
(app, HM-REQ-121) were both watched red first (section 3). The corpus through the live path is
byte-identical to ours. `7aed2ea6`.

One HM-REQ-121 case needed a second watch. The tab case's first red was its own defect: it read
`MainWindow.axaml`'s comments, which cite "the arbiter's ruling" (the project's rulings, never
drawn). With that corrected, it was watched red against a temporary stub `Arbitrate` that put both
readings of the span on the transcript. The committed arbiter was then restored, `src` and `tests`
were checked equal to `7aed2ea6`, and the tests went green again. That account is in the test's
remarks. After that, 9.6 was ticked in both copies of `PHASE_PLAN.md` and nothing else was.
`cf38e1c4`.

**Task 4.** Ran the exit round (section 3), then committed it with this report.

**Readings the tree forced, each in section 4:** a tie is a disagreement's case (item 3). The
leading edge is arbitrated against what the port has printed so far (item 4). The port follows a
pitch change only by being rebuilt (item 5). A rate that is not a multiple of 8000 leaves the port
silent (item 6). The port's decision rows are bounded by rebuilding it (item 7).

## 2. What the owner should expect

Nothing changes on the screen: the CW tab shows exactly the letters, in exactly the brightness, it
showed before, on every one of the 35 recordings. What changes is underneath. fldigi's decoder now
listens to the same audio as Hamlet's own, live, and every letter on the tab has passed through
one referee that has both readings in hand. The capture sheet gains an `arbiter` line. For each
letter it says which decoder it came from, whether the two agreed, disagreed or tied, and what each
read and how sure it was. The tab never names either decoder. Under today's calibration, fldigi is
only advising: its confidence has not proved honest on any condition, so it never displaces a
letter or brightens one. It gets a vote on a condition only once its confidence proves out there,
and 9.7 decides whether that vote helps. The cost is about a quarter more decode time: 52 s becomes
65 s over 11½ minutes of audio.

## 3. What you should see

**The meeting table.** These counts come from the harness, where the port is given the pitch
instrument's median, as `parity.md` gives it. Task 2's arbiter records reproduce every count.
Under today's vote table, the arbitrated text equals ours on every row. Short names:
- sender not stated = real HF, no CH-* profile, SNR_2500 not measured, sender not stated in
  CW_SPEC.md;
- TX-ITU 15 dB = synthetic, no fading, shaped noise band, TX-ITU (1:3:1:3:7), 15 dB in the
  passband;
- gap 5 = the synthetic character gap of 5 units, inside TX-FARNS's 3 to 7.

| condition | key | recordings | agree | disagree | one-sided ours | one-sided port | tie within 0.05 | ours votes | port votes | arbitrated = ours |
|---|---|---|---|---|---|---|---|---|---|---|
| real HF, all | inferred | 23 | 290 | 113 | 438 | 48 | 16 | yes | no | yes |
| real, TX-FARNS | inferred | 1 | 30 | 2 | 17 | 2 | 0 | no (not calibrated) | no | yes |
| real, TX-ITU | inferred | 1 | 8 | 2 | 9 | 3 | 0 | no (not measurable) | no | yes |
| real, TX-TIGHT | inferred | 1 | 9 | 19 | 27 | 4 | 2 | no (not measurable) | no | yes |
| real, sender not stated | inferred | 20 | 243 | 90 | 385 | 39 | 14 | yes | no | yes |
| synthetic, all | exact | 12 | 120 | 16 | 37 | 32 | 2 | yes | no | yes |
| synthetic, TX-ITU 0 dB | exact | 3 | 0 | 0 | 0 | 27 | 0 | no (not measurable) | no | yes |
| synthetic, TX-ITU 5 dB | exact | 3 | 42 | 6 | 16 | 0 | 0 | no (not calibrated) | no | yes |
| synthetic, TX-ITU 15 dB | exact | 3 | 54 | 2 | 8 | 0 | 0 | yes | no | yes |
| synthetic, gap 5, 0 dB | exact | 1 | 0 | 0 | 0 | 4 | 0 | no (not measurable) | no | yes |
| synthetic, gap 5, 5 dB | exact | 1 | 7 | 7 | 11 | 1 | 1 | no (not measurable) | no | yes |
| synthetic, gap 5, 15 dB | exact | 1 | 17 | 1 | 2 | 0 | 1 | no (not measurable) | no | yes |

Agreements whose two p's are also within 0.05: 28 real, 11 synthetic. They count as agreements,
not ties (section 4 item 3). On the disagreements and ties the port's p is the higher on 17 of 129
real spans and 0 of 18 synthetic. The mean p is 0.910 for ours against 0.748 for the port on real
spans, and 0.898 against 0.582 on synthetic. Through the **live path**, where the port follows our
pitch:
- **real:** agree 228, disagree 94, tie 13, one-sided ours 522, one-sided port 40;
- **synthetic:** 112, 16, 2, 45, 26.

The live transcript equals ours on all 35 (`.run-unit/unit465-live-t3.txt`).

**1. The span rule, the vote table and the margin, as task 1 fixed them.**
- **Span rule:** two characters, one of each decoder, are on the same span when their spans
  overlap by at least half the shorter span. Pairs are taken one to one, largest overlap first.
  Word boundaries are not paired.
- **Vote table:** `calibration.md` section 2's held-out verdicts (unit 464, `efdd5d11`). A decoder
  votes only where it is calibrated: ours on real HF, all; real, sender not stated; synthetic, all;
  and synthetic TX-ITU 15 dB. The port votes on no condition. The harness runs each recording
  under its own condition row; live runs under real HF, all.
- **Margin:** 0.05 on |p ours − p port|.
- **Emission:** where both vote, agreement takes the more confident decoder's class, a
  disagreement goes to the higher p at its own class, and a tie goes to the higher p, dim. Where
  one votes, its reading stands and the other's goes on the record. Where neither votes, ours is
  emitted as it prints today. A span only ours read is emitted as ours; a span only the port read
  is emitted only where the port votes.

**2. The seams task 1 named, and what each reads now.**
- **The CW tab.** `MainWindowViewModel.StartDecoding` (`MainWindowViewModel.cs:11244-11254`) wires
  `LeadingEdge` to `Transcript.OfferEdge` and `CharacterSettled` to `Transcript.Settle`.
  `CharacterDecoded` carries no text; it only stamps `_lastDecodeUtc` and `_lastCharacterUtc`.
  All three are now raised from the arbiter's output inside `CwDecoder` whenever `secondReader`
  is on, and the app turns it on.
- **Other readers.** `AutoCallViewModel.cs:426` and `ScanViewModel.cs:515` subscribe to
  `CharacterSettled` too, so they read the arbiter's output.
- **The sheet.** `CaptureAudioAsync` still writes `text` (12012) and `spanLlr` (12033), and now
  `arbiter`, from `MainWindowViewModel.ArbitrationLine`.
- **The chunks.** `CwDecoder.Process` walks each chunk a tracker hop at a time at the source's own
  rate. `Step` now hands the same hop to `CwSecondReader.Read` at `_probabilistic.ToneHz` before
  ours reads it.
- **The port's pitch.** `FldigiCwDecoder.frequency` is readonly and set by the constructor
  (`FldigiCwDecoder.cs:281`), and `Frequency` has only a getter (342). So the port is rebuilt at
  the new pitch; it has no setter, and none was added.

**3. How each of the five tests was watched failing first.**
- **HM-REQ-125, 126 and 127** (`TheHigherCalibratedReadingWinsTests`, all 8 cases): red against a
  stub `CwArbiter.Arbitrate` that returned ours unchanged with no records
  (`.run-unit/unit465-wins-red.txt`). The 125, 126 and 127 cases failed on the character or class
  emitted. The mirror, advisory and neither-votes cases, whose character the stub already emitted,
  failed on the missing record.
- **HM-REQ-120** (`BothDecodersReadTheSameSamplesTests`): the every-character case was red at the
  stub stage of the live wiring, where the constructor argument existed and nothing was routed:
  the port was handed 0 samples (`.run-unit/unit465-same-red.txt`). Its two supporting cases were
  already green at that stage and were not watched red:
  - the streamed port prints what the whole-file port prints;
  - under today's table the transcript is ours.
- **HM-REQ-121** (`TheOperatorSeesOneTranscriptTests`): both sheet cases were red at a stub
  `ArbitrationLine` that wrote nothing (`.run-unit/unit465-one-red.txt`). The tab case was red
  against a temporary stub `Arbitrate` that put both readings of the span on the transcript: two
  characters on one span (`.run-unit/unit465-one-red-tab.txt`).

**4. Three real disagreements, as the sheet records them** (inferred keys, R61):
- **`cw-2026-08-17-134712`, 21.72-22.33 s:** key `4` (in `N4L`).
  `4:ours/disagree/ours 4 0.974/port ■ 0.819`. The port printed its no-match `*`.
- **`cw-2026-08-18-004507`, 20.50-20.96 s:** key `L` (in `HANDLING`).
  `L:ours/disagree/ours L 0.943/port É 0.806`.
- **`cw-2026-08-18-004507`, 29.21-29.77 s:** key `P` (the stretch's last).
  `P:ours/disagree/ours P 0.946/port Ö 0.822`.

In all three, ours had the higher p and was right against the key, and it would have won even
with the port voting.

**5. Decode time** over the real set (690 s of audio), through the live `CwDecoder` path hop by
hop:
- ours alone 52.22 s; ours with the port and the arbiter 65.09 s (×1.25);
- synthetic (279.5 s of audio): 8.38 s to 9.98 s.

`parity.md`'s own harness put ours at 51.64 s at entry and 51.57 s at exit. That run rewrote the
decode-time rows of `parity.md`; a copy of each run's file is kept under `.run-unit/`, and the
committed `parity.md` was restored.

On the real set the port was built 58 times on 23 recordings: 35 of those were retunes, none were
after a skip and none were for the memory bound. On the synthetic set it was built 14 times, 2 of
them retunes.

**6. The commits.** The five checks at each commit are: build (0 errors unless noted); engine
carry-forward line; app carry-forward line; named, captures and adjudicated floors.

| commit | what | engine line | app line | named / captures / adjudicated floors |
|---|---|---|---|---|
| `d3915f01` | task 0: record, 1.13.152, entry, saves | 178 of 178 | 278 of 278 | 13/13, 51/51, 13/13 |
| `089c4bb8` | task 1: the meeting fact | 178 of 178 | 276 of 278, rerun 275 of 278 (all lost to the dispatcher loop); each lost type alone: BindingHealthTests 1/1, TheFavoritesAreChipsTests 4/4, TheFavoritesAreUnderTheGreenZoneTests 3/3, ThePsk31ConversationCardTests 8/8 | 13/13, 51/51, 13/13 |
| `4c9a8433` | task 2: arbiter, vote table, margin, test | 178 of 178 | 276 of 278 (dispatcher loop), rerun 278 of 278 | 13/13, 51/51, 13/13 |
| `7aed2ea6` | task 3: live wiring, sheet, two tests | 178 of 178 | 274 of 278 (dispatcher loop), rerun 278 of 278 | 13/13, 51/51, 13/13 |
| `cf38e1c4` | the 9.6 tick (and the test's remarks) | 178 of 178 | 278 of 278 | 13/13, 51/51, 13/13 |
| task 4 commit | exit round, output.md | as `cf38e1c4`: no source or test changed | | |

At `d3915f01` the five checks are the entry round's. The save printer was added after them and
built with 0 errors. `cf38e1c4`'s five checks are the exit round's, run at that tree.

**The exit round, each figure beside its entry figure:**
- **Build:** 0 errors (0).
- **Carry-forward lines:** engine 178 of 178 (178); app 278 of 278 (278).
- **Floors:** named 13 of 13 (13); captures 51 of 51 (51); adjudicated 13 of 13 (13).
- **Real set** (inferred keys), unchanged from entry, no per-recording row moved:
  - MET-CER-SURE 33 of 436;
  - MET-INVENTED 33 over 473;
  - coverage 403 over 473;
  - MET-WBE 37 over 113.
- **Synthetic set** (exact keys), unchanged: 14 of 173, 14 over 252, 159 over 252, 44 over 84.
- **The port, per `parity.md`:** real 62 of 239, coverage 177; synthetic 34 of 168, coverage 134.
  Unchanged.
- **Test types:**
  - `BothDecodersAreScoredAlikeTests` 5 of 5 (5);
  - `EveryCharacterCarriesAConfidenceTests` 2 of 2 (2);
  - the port's own tests 8 of 8 (8);
  - `TheHigherCalibratedReadingWinsTests` 8 of 8;
  - `BothDecodersReadTheSameSamplesTests` 3 of 3;
  - `TheOperatorSeesOneTranscriptTests` 3 of 3;
  - `TheSpeedFollowsTheSendersMarkPairsTests` red at 28 of 31 sure characters wrong or added, as
    at entry. It is not required green.
- **Byte-identical against task 0's saves:** ours alone, the port alone and the arbitrated
  transcript.
- **Diffs:** `git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/` prints nothing, and
  `git diff 7e209cb4` over the eleven transmit files prints nothing.
- **`git status`:** `.run-unit/fldigi/` and `SESSION.lock` untracked and not staged.
  Printout: `.run-unit/unit465-exit-print.txt`.

## 4. What's blocking us

Nothing blocks. These are readings, recorded as the instruction asks. None is a ruling request.

1. **HM-REQ-127's margin is 0.05, the requirement's recommended value, held provisionally.** It is
   `CwArbiter.TieMargin`. The threshold is the owner's under `PHASE_PLAN.md` section 6, and its
   absence never halts a unit.
2. **9.4's words and 9.3's five refused mechanisms leave 9.4 no authorable route, and R86 holds
   steps 2 to 8 behind 9.4.** This is logged for the owner, not a stop: it touches neither transmit
   nor what the product promises the operator.
3. **A tie is a disagreement's case.** HM-REQ-127 says "the winning character is emitted in the
   dim class", which presumes two characters in contention. An agreement has one character, and
   HM-REQ-125 gives it the more confident decoder's class. So two agreeing readings within 0.05 are
   an agreement: 28 real and 11 synthetic spans. If the owner reads 127 as also dimming close
   agreements, that is a one-line change in `CwArbiter.Decide`. It moves no letter today, because
   the port votes nowhere.
4. **The leading edge cannot wait for the port.** The seam is `CwDecoder.LeadingEdge` and
   `CharacterDecoded`, to `Transcript.OfferEdge`. The port prints a character only after 2 to 4 of
   its own dots of silence, plus its filter's 1,024-sample block (128 ms). The edge is ours' live
   reading, which HM-REQ-083 ties to the settled pass's word boundaries. Holding it until the port
   has printed would put a gap in the live line. So the edge is routed through the arbiter against
   what the port has printed so far: an edge character the port has not printed yet shows as
   one-sided, and it is re-decided at settle.

   At settle, ours runs about 1 s (`DecisionDelaySeconds`) behind the audio. That is longer than
   the port's lag at every speed in the corpus, but at 6 WPM and below the port's lag (4 dots,
   0.8 s, plus 0.19 s) comes close to it. A slow sender's character could then settle as
   one-sided when the port had simply not printed yet. This is not measured: the corpus has no
   sender that slow.
5. **The port reads less well live than in `parity.md`.** This bears on 9.7. Live, the port is
   given our pitch in force from the first hop, and our tracker hunts from 600 Hz, so the port is
   rebuilt at each move of 30 Hz or more: 35 retunes on the 23 real recordings. Each rebuild loses
   the port's filter, level and speed state. The harness gives it the instrument's median pitch
   once. Agreements on the real set fall from 290 (harness) to 228 (live), and spans only ours read
   rise from 438 to 522.

   9.7's "the arbitrated output measured against each decoder alone" should be measured on the live
   path, because that is what the operator's screen would carry. A retune rule that holds the port
   still until our pitch is measured (`Tracker.HasMeasuredPitch`) would likely recover much of the
   difference. That would be a later unit's choice; V-14 fixed the rule here.
6. **A sound card rate that is not a whole multiple of 8000 leaves the port silent.**
   `FldigiRateAdapter` refuses such a rate, and `CwSecondReader` follows it and says why in
   `Unavailable`. Every character is then one-sided ours, and the tab is unchanged. The rate the
   owner's device actually runs at is not recorded in the tree; the captures are 48,000 Hz.
7. **The port's decision rows grow at 500 a second live.** `TraceDecisions` must be on, because
   only the decision rows place the port's key events. `CwSecondReader` therefore rebuilds the port
   at the same pitch after 150,000 rows (5 minutes), at the first 2 s with no key event. None of
   the recordings is long enough to reach that bound, so its effect on the port's reading at the
   radio is unmeasured.
8. **Decode time rises by a quarter**, 52.22 s to 65.09 s over 690 s of real audio. It is not a
   doubling, and it is not a keep rule. The port and the streamed filter take the extra 13 s.
9. **Live pairing takes each settled character as it arrives.** The harness takes the largest
   overlap first over the whole stream. Live, a settled character takes its largest-overlap port
   reading unless a character still on our leading edge overlaps that reading more. Where the
   competing character is not yet on the edge, the two can pair differently. That changes a record
   and never a letter today, because the port votes nowhere. The effect is not measured apart from
   item 5's pitch difference.
