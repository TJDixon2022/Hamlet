```
READ IN THIS ORDER.

A. PHASE GOAL - CW decodes again. Steps 1, 2 and 4 done; step 3 partial on
   3.6 alone; step 5 is Tim's.
B. THIS STEP - step 3, the inherited reds. 3.1 to 3.5 met; 3.6, 2 of eight
   with a verdict - none of #6 #15 #42 #43 #44 #45 went green at a kept
   commit; #45 and #6 went green only under changes put back for lowering
   capture rows; #6 #15 #43 #44 #45 stay open at attack 2 with their counts
   restarted, #42 stays open, not attacked.
C. THIS REPORT - the six-red table leads section 3;
   section 4 raises 5 items and items 1, 2 and 3 stand in the way of 3.6.
```

```
UNIT:       405 - complete at task 4 of 4, tasks 0 to 4, none dropped - 2026-09-23 13:27
PHASE GOAL: get the engine's CW decoder back to what it produced on 2026-08-25, held there by three floor tests, then let Tim judge it at the radio
UNIT GOAL:  make a second, better-aimed try at each of the six test recordings the decoder still gets wrong, starting from the causes unit 402 named, and keep only a change that clears one without costing any other recording
ADVANCED:   no - no red reached a verdict; the two changes that turned a red green each lowered real capture rows, and the rest moved a number without turning a test green
NUMBER:     of 3.6's eight 2 with a verdict; this unit 0 green, 5 moved, 1 not attacked
DRIFT:      1 consecutive units without advance  (was 0)
```

## 1. What Claude did

**Complete at task 4 of 4. All of tasks 0 to 4 were done and none was dropped.** Machine
QUIVERFULL, project Hamlet, branch main. The gate held: SHACK_FACTS.md and
CwProbabilisticDecoder.cs are present, there is no CoreHMI.sln or MURC.sln, and the root is
C:\Source\HamLet. Every item in section 5 matched the tree, with no mismatch. Commits `63c28f7a`,
`a0256e3e`, `d40d6653` and `ef34d601` were each pushed and each push succeeded. The report commit
follows them, and whether its push succeeded is in `PROJECT_STATUS.md`'s NOTE, written after the
push. The session ran from 12:28 to 13:27, well inside
task 2's clock rule, so #6 was attacked.

**Task 0.** Version 1.13.91 to 1.13.92. `PHASE_STATUS.md` was set to 405 and step 3, and
`PHASE_OUTCOME.md` got its entry. The `f74b6d51` diff over `src`, `tests` and the list was empty,
so the lines at entry are unit 404's exit: engine 178, app 278. Every type was run at entry and
matches unit 404's numbers (table in `docs/phase-cw/unit405-reds.md` section 1).

**Task 1, the trace.** A new printer, `TheSixRedsTraceTests`, asserts nothing and is on neither
line. Its output is in `.run-unit/unit405-trace.txt`. It named a line for five of the six reds
and a property for #42:

- **#15 and #43:** the doubled fit comes from the unit estimator, not the speed grid.
  `CwUnitEstimator.cs` 96, through the tenth-percentile seed at 507, takes a few 15 to 35 ms noise
  marks as the dit cluster. At the read where #15 leaves 12 for 22.9, the grid itself prefers 12.
- **#44:** held gaps of 15/1127/323 ms, from `MeasureGaps` 221 to 231, put the character gap past
  the word gap. Every element then reads as its own letter.
- **#45:** the flush settles a trailing unreadable character (`CwProbabilisticStream.cs` 501 to
  507). With B1 and B2 applied uncommitted, tightfist-easy settles `VVVTESTDETESTK`.
- **#6:** `C` and `Q` are whole in the envelope. The stream mixes them at the tracker's unmeasured
  600 Hz, 40 Hz off the sender, so they arrive 8 dB down and the lattice reads them as key-up
  (`CwDecoder.cs` 585 to 589).
- **#42:** the only property separating the fixture's guard spans from the real captures' is the
  generator's flat -82 dBFS residue. 46 of `013347`'s 59 floor characters and 28 of `013622`'s 55
  sit inside the guard's own-send spans.

**Task 2, the attacks.** Five changes were built and measured one type per invocation, and none
was kept. `src` at exit is identical to entry.

- **A1**, B1 and B2 together: #45 green, but 6 capture rows lower.
- **S1**, at line 96: #15 0.54 to 0.61 and #43 moved, but 14 capture rows lower.
- **G1**, gap order: #44 moved at no cost, but turned no red green.
- **M1 and M2**, re-mixing the held window at the tracker's new pitch: #6 0.75 to 0.86, green,
  but 11 and 10 capture rows lower.

Widening the printer's pitch line then showed that on every single-sender case the misses follow
`CwToneTracker.Switch` moving the mixdown 25 to 85 Hz off the sender, reported as measured: 575
for 615 on coverage-easy and exchange-easy, 700 for 640 on #15, 550 and 725 for 640 on #6's two
tails.

**Task 3.** Six rows marked *attack 2* were added to `reds-3.6.md`, and the attack table to
section 3 of `unit405-reds.md`. No red went green at a kept commit, so the failing set's closing
line and 3.1's tick are unchanged. 3.6 is not ticked.

**Task 4.** The exit round is identical to entry on every floor and every type. The app line lost
three different names to the dispatcher loop before any assertion across two runs, and counts
278. 3.5 is re-confirmed.

**Decisions the session made for itself**, reproduced in full:

1. **#42 was not attacked, although task 1 found a separating property.** The instruction says
   the change must be conditioned on the property found. The property found is that the
   fixture's mutes sit at -82.8 to -81.6 dBFS in every span, which is the generator's residue
   constant (`CwFixtureGenerator.cs` 672 to 688), with the fixture at 8 kHz and 15 dB quieter.
   A skip conditioned on that would turn #42 green on synthetic audio and never fire on the air,
   which would present a test result as a repair (CLAUDE.md 0.0, §12.5, HM-DEC-091). I read
   decision 4 as covering a property that does not separate the operator's sending from anyone
   else's. **Rejected:** building the residue-keyed skip to show the number move. It would have
   been a measurement of the fixture, not of the decoder.
2. **M2 replaced M1 on the same cause**, as unit 402's A2 replaced A1. M1 re-mixed whenever the
   pitch moved while the text was empty, which is broader than the trace. M2 re-mixes only before
   the window's first read, which is the stretch the trace named. Both are recorded, and both are
   out.
3. **S1 and G1 were gated beyond the keep rule's first answer**, for the record. S1's cost on the
   captures (14 rows) is now known. G1 was run on every floor and every red-holding type, so the
   next unit inherits a change that is measured and costs nothing.
4. **The tracker switch found in task 2 was not attacked.** Task 1 did not name it. The tracker's
   switching is governed by HM-DEC-095 and HM-DEC-127, and the cold-start bin choice is
   HM-OPEN-033, scheduled as its own work order. It is written up as the common cause for the
   next unit (section 4, item 3).
5. **B2 was rebuilt from unit 402's description**, because 402's diff is not in the tree: at the
   flush, a character the alphabet does not know, still inside the delay, is not settled. It
   reproduced 402's own-type result, #45 1 + 3 on `CharacterDecoded`.
6. **The status file was written as `STATE: EXECUTING` and `BALL: code`.** Unit 404's run script
   wrote `ACTIVE` and `claude`, which are not among CLAUDE.md 13.1's values.

## 2. What the owner should expect

Nothing the decoder does has changed. `src` is byte-for-byte what it was at the start. Five of
the six reds moved under a change built for them, and none of those changes was kept:

- #45's trailing placeholder went away, but six recordings' floors count their own trailing
  placeholders and fell by one to three characters.
- #6's first word came back at 0.86 against a 0.79 bar, but ten recordings' counts moved, some up
  and some down by as many as ten characters.
- #15 went from 0.54 to 0.61, and coverage-easy lost all seven strangers on the settled
  transcript, but fourteen recordings fell.
- #44's second call came out whole on the settled transcript, and nothing else moved, but no test
  turned green, so the rule sends it back.

#42 was not touched, because the only thing that tells its fixture apart from the two real
recordings is a constant in the fixture generator.

**What will look wrong but is not:** `reds-3.6.md` shows every attacked red at *sequence 0 of 3*
after unit 402 recorded *attempt 1 of 3*. That follows the record head's own rule that movement
restarts the count, as decision 3 asked; the disagreement is item 4 below. `TheSixRedsTraceTests`
appears in the test list and passes, because it asserts nothing.

## 3. What you should see

| red | cause as traced | change | before | after | moved | kept | sequence |
|---|---|---|---|---|---|---|---|
| #6 | start of a bare call mixed at the unmeasured 600 Hz for a 640 Hz sender, read as key-up; the tails follow a tracker switch to 550 and 725 Hz | M1, M2: re-mix the held window at the new pitch | 0.75 | 0.86, green | yes | no: captures 26 and 27 of 37 | 0 of 3, restarts |
| #15 | dit cluster of noise marks, `CwUnitEstimator.cs` 96; upstream, mix at a measured 700 Hz for 640 | S1: marks under 0.45 of the element gap left out | 0.54 | 0.61 | yes | no: captures 23 of 37 | 0 of 3, restarts |
| #42 | the only separating property is the generator's residue; the captures' floors count own-send slivers | none | 70 | 70 | - | not attacked | 0 of 3, broken |
| #43 | as #15; mix at 575 Hz for 615 from 22 to 28.5 s | A1; S1 | 5 + 37 | A1 4 + 7; S1 6 + 25 | yes | no | 0 of 3, restarts |
| #44 | held gaps 15/1127/323 ms, `MeasureGaps` 221 to 231; mix at 575 for 615 | A1; G1: an out-of-order gap reading is not separated | 3 + 21 | A1 0 + 7; G1 4 + 16 | yes | no: G1 cost nothing, no red green | 0 of 3, restarts |
| #45 | the flush settles a trailing unreadable character | A1: B1 and B2 together | 1 + 3 | 0 + 0, green | yes | no: captures 31 of 37 | 0 of 3, restarts |

**The answer to the question this unit was commissioned to ask:** a second attack from traced
causes turned two of the six green, #45 and #6. Neither could be kept, because each lowers real
capture rows. 3.6 stays at 2 of eight with a verdict.

**Gate numbers for each kept change:** none; no change was kept. The measured costs are in
`docs/phase-cw/unit405-reds.md` section 3.

**Exit round, at `ef34d601`:**

| line or type | exit |
|---|---|
| engine line | 178 of 178 in 369 s |
| app line | 277 of 278, then 276 of 278; three different names lost to the dispatcher loop before any assertion, each green in the other run: 278 of 278 |
| captures, adjudicated, synthetics | 37 of 37 with every row identical to entry, 13 of 13, 2 of 2 |
| the ten red-holding and speed-reader types | each identical to entry |
| transmit files against `7e209cb4`; `src` and `src/Hamlet.App` from `f74b6d51` | nothing printed |

**Visible change: none.** This unit changed no decoder code. What it leaves is a traced cause
common to four of the reds, and one measured, zero-cost change (G1) for the next unit to pair with
a green.

## 4. What's blocking us

1. **#42 cannot turn green by a decoder repair without lowering two capture floors.** Stands in
   the way of 3.6.
   - **Ruling asked:** may the character floors of `cw-2026-08-17-013347` (59) and
     `cw-2026-08-17-013622` (55) be re-expressed so they do not count characters read inside
     the transmit guard's own-send spans? Or is #42 to be judged some other way?
   - **Reasoning:** 46 of `013347`'s 59 floor characters and 28 of `013622`'s 55 sit inside
     spans the guard itself marks as the operator transmitting, and they are the `E I H S`
     slivers #42 exists to forbid. Any skip that fires on real own-send audio takes them off the
     floors, which section 6 forbids. The only property that separates the fixture is the
     generator's residue. So #42 can be neither greened nor honestly attacked, and it can never
     reach section 6's three-attack parking.
   - **Rejected:** a skip keyed to the flat residue, which is a fixture artifact; and lowering
     the floors in this unit, which section 6 forbids and is the owner's call.
2. **#45 and #6 each have a green that the capture floors refuse.**
   - **Ruling asked:** does a change that turns a red green while moving capture rows count as
     *a floor lowered*, when the rows lost are trailing placeholders (#45)? Or, for #6, when rows
     move both ways, `002016` up 75 to 93 and `021410` down 47 to 37?
   - **Reasoning:** the keep rule counts any lower row. B2 removes only placeholders the flush
     settles after the audio ends. M2 re-takes the envelope at the pitch the tracker moved to
     before the first read, which changes real decodes in both directions. HM-DEC-091 says a
     change that costs one recording is not a fix, and I did not keep either.
   - **Rejected:** narrowing either change until the floors hold. That would be fitted to the
     floors rather than traced.
3. **The shared upstream cause is `CwToneTracker.Switch` (1092, 1154 to 1169), and a 3.6 unit
   needs leave to change it.** Stands in the way of #15, #43 and #44 in practice.
   - **Ruling asked:** may the next 3.6 unit change the tracker's switch, or does HM-OPEN-033's
     own work order come first?
   - **Reasoning:** on every single-sender case, every miss follows the tracker moving the mix 25
     to 85 Hz off the one station, reported as measured. The estimator and gap lines this unit
     attacked are where that wrong pitch becomes a wrong reading. `CwDecoder.cs` 658 to 665
     already says the fault is upstream.
   - **Rejected:** attacking it here. Task 1 did not name it, and HM-DEC-095 and HM-DEC-127 rule
     that code.
4. **`reds-3.6.md`'s head and unit 402's rows disagree about the count, as section 5 expected.**
   - **Ruling asked:** which counts, the head's *movement restarts the count* or the rows'
     *attempt 1 of 3*?
   - **Reasoning:** this unit's rows follow the head (decision 3), so every attacked red is at 0
     of 3. Under the head's rule, a red whose attacks keep moving it never reaches parking, so
     section 6's three-attack exit may not arrive for any of the six. That bears on stop 10's
     two-unit count, which the arbiter flagged as the owner's to weigh.
   - **Rejected:** rewriting either, as the instruction says.
5. **G1 is measured and costs nothing, but the keep rule sends it back.** Does not block.
   - **Ruling asked:** may a change that moves a red on its own event and on the settled
     transcript, with every floor and type identical or up, be kept without a red turning green?
   - **Reasoning:** G1 took #44 from 3 + 21 to 4 + 16, and settled from 0 + 7 to 0 + 3 with the
     second call whole. It kept captures 37 of 37 (`021825` up by one), adjudicated 13, synthetics
     2, and every type identical. It was put back under unit 402's keep rule, which the
     instruction says not to re-argue.
   - **Rejected:** keeping it. That is the rule's owner's change to make, not the session's.
