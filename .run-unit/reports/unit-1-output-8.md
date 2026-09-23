```
READ IN THIS ORDER.

A. The gate's bar: 1.0 log-likelihood per hop, CwProbabilisticDecoder.CharacterMargin,
   its value unchanged. What changed is that below it nothing is printed, where a
   placeholder used to be. On the 17:37 capture, replayed on the bench, it reads the
   same with it on as off: 46 named, 0 placeholders, the scored region 29 edits from
   an inferred key. The bench replay has nothing below the bar. The sidecar's 26 unsure
   were the CW tab's session count, and the sidecar is not in the tree.
B. Step 3's criteria: 3.9 met, the 37 rows re-measured with no named character lost;
   3.6 closed, #45 green, #15 #43 #44 parked; 3.7 met, placeholders on the 37 captures
   299 to 28, no named character lost anywhere; 3.8, the tracker, not started.
C. The 37-row table is in section 3 and docs/phase-cw/unit408-floors.md. Section 4
   raises 0 items; three findings are parked as 408 items 1 to 3.
```

```
UNIT:       408 - complete at task 4 of 4, tasks 0 to 4, M2 dropped as the named candidate - 2026-09-23 17:09
PHASE GOAL: bring the engine's CW decoder back to what it read on the air on 2026-08-25, hold it there with three floor tests, clear or park the inherited reds, then have Tim judge it at the radio
UNIT GOAL:  count the floors in named characters so suppressing a placeholder is not a regression, close 3.6 in one pass under that rule, and stop the decoder printing characters it cannot clear its own bar on
ADVANCED:   yes - 3.6, 3.7 and 3.9 ticked on measured numbers, #45 green, placeholders on the 37 captures 299 to 28 with no named character lost
NUMBER:     placeholders on the 17:37 capture: 26 in the app's session, 0 on the bench at entry -> 0; named 59 in the session, 46 on the bench at entry -> 46; across the 37 captures placeholders 299 -> 28, named 1295 -> 1297
DRIFT:      0 consecutive units without advance  (was 2)
```

## 1. What Claude did

**Complete at task 4 of 4.** All of tasks 0 to 4 ran. Inside task 2, **M2 was dropped**, which is
the named drop candidate. QUIVERFULL, `C:\Source\HamLet`, Hamlet confirmed by the gate, branch
`main`. Commits `c19ecf61`, `f5bbbb8e`, `0bcf04e6`, `c1640919` and `aded8dcd`, plus the report
commit, all pushed.

**Section 5 against the tree, reported and not repaired:**
- `src\Hamlet.RadioEngine\Cw\CwEmissionGate.cs` does not exist. The bar is
  `CwProbabilisticDecoder.CharacterMargin`, **1.0**, in a character's own span log-likelihood ratio
  **per hop**. It is applied in `Marked`, now `Judged`. `CwEmissionGateTests` is a test type.
- The 17:37 WAV and `key.md` are on disk. Both were already tracked in `65b4f9d0`, so there was
  nothing to commit. **`cw-2026-09-23-173723.txt`, the sidecar, is not on disk.**
- The floor table held 37 rows, counted from `CharactersEmitted`, placeholders included.
- `reds-3.6.md` held #15, #43, #44 and #45 open.
- `unit405-reds.md` describes B2, M2 and G1 in prose, and 405's diffs are not in the tree. B2 and
  G1 were rebuilt from those descriptions.

**Task 0.** HM-DEC-168 is in `DECISIONS.md`, and its row sits at the top of `CLAUDE.md` section 1.
Version is 1.13.95. `PHASE_STATUS.md` names 408 and step 3. `PHASE_OUTCOME.md` has the unit 408
entry. Entry round, one type per invocation after one build:
- engine line 178 of 178 in 371 s
- app line 276 of 278, both losses to the dispatcher loop before an assertion, then 278 of 278 on
  the one re-run
- captures 37 of 37, every row identical to unit 407's exit
- adjudicated 13 of 13
- clean synthetics 2 of 2

**Task 1, 3.9.** The harness now counts what settles through `CharacterSettled`, in three columns:
- **named**: the floor
- **named elements**: the element floor
- **placeholders**: printed, asserted on nothing

All 37 rows were measured at `c19ecf61` in one run. **1,295 named and 299 placeholders, out of
1,594 emitted.** On every row, named equals emitted less placeholders exactly, so **no real
character was lost**. At the same commit the anchors were 13 of 13 with every printed reading
identical to entry, and the captures were 37 of 37 on the new table.

**Task 2, 3.6's closing pass.**
- **B2 kept, `0bcf04e6`.** B2 is the flush no longer settling an unreadable character still inside
  the delay, built with B1, the easy tier reading `CharacterSettled`, exactly as 405 measured it.
  402 had measured B2 alone as no movement. Results:
  - **#45 went from 1 + 3 to 0 + 0, green.**
  - Captures 37 of 37. Six rows lost only placeholders. No named count or named element fell.
  - Anchors 13 of 13, and every named character identical.
- **G1 on B2** took #44 from 0 + 7 to 0 + 3, with the second call whole. It turned no red green and
  cost nothing, so it went out and was never committed.
- **M2 dropped.** Its target, #6, has been green since unit 406's `775907b6`. Unit 405 measured #15
  unmoved under M1 and M2. So M2 could not meet the keep rule.
- **#15 at 0.54, #43 at 4 + 7 and #44 at 0 + 7 are parked** in `PARKED.md`, each with the units that
  attacked it.
- The set's closing line now reads 31 green, 17 repaired, 0 retired, 3 parked. 3.6 ticked.

**Task 3, 3.7.** A sweep printer went over every settled character on the 37 captures and 17:37:
- All **261 of 261** placeholders marked by margin sit below 1.0 per hop, and **no named character
  does**.
- Every higher bar costs named characters: **1.047 per hop costs 3**, and **a span total of 5 costs
  9**. The weakest named characters are lone `E`s at totals of 3.3 to 8.0.
- The other 28 placeholders are patterns the alphabet does not know, at span totals of 90 to 29.8
  million. They stay printed as `■`.

**So the bar stays at 1.0, and the gate now leaves out what is below it instead of marking it.**
- `NothingBelowTheBarIsPrinted` was watched red on the three anchors' recordings: 57, 47 and 5
  printed below the bar, leading edge included. It was green on 17:37, where the bench settles
  nothing below the bar. After the change it is 4 of 4.
- Captures: named 1,295 to **1,297**, with no row lower. Placeholders 289 to **28**.
- The three anchors are identical on named characters. `031838` gains one `T`.
- `AWeakCharacterIsMarkedRatherThanRemoved` asserted the marking that 3.7 replaces. It was
  rewritten under R12 as `AWeakCharacterIsNotPrinted`.

**Task 4, exit round.**
- engine line 178 of 178 in 369 s
- app line 276, then 277, of 278. Each loss was a different name to the dispatcher loop before an
  assertion and green in the other run, so it counts neither way: **278 of 278**.
- captures 37 of 37 and adjudicated 13 of 13, both identical to task 3
- clean synthetics 2 of 2
- `CwReceiverFixtureTests` 25 of 27, up one with #45
- `CwAcquisitionWindowTests` 11 of 12, as entry
- the 17:37 type 5 of 5
- the exit sweep finds **nothing printed below the bar on any of the 38 recordings**

The eleven transmit files print nothing against `7e209cb4`. `src/Hamlet.App` prints nothing.
`src` changed in two files only: `CwProbabilisticDecoder.cs` and `CwProbabilisticStream.cs`.

**Decisions made for myself, each the author's and overrulable:**
1. **The element floor is re-measured over named characters.** Otherwise a suppressed placeholder
   would lower the element floor by its own elements, which is the same miscount R57 removes.
2. **B2 was rebuilt together with B1.** That is the form in which 405's green was measured; B2
   alone is 402's measured no-movement.
3. **The bar keeps its value.** The instruction asked for it to be raised to the single digits. On
   this tree, every raise loses named characters, so it sits at the highest value that loses none.
4. **The two rows that rose, `031838` and `001952`, were not written in as floors.** The table stays
   as task 1 set it.
5. **`NothingBelowTheBarIsPrinted` covers 17:37 plus the three anchors' recordings**, because on
   17:37 alone the fact could never have been watched red.
6. **Every invocation in tasks 3 and 4 ran one type.** In task 2, B2's check ran
   `CwEmissionGateTests` and `CwAcquisitionWindowTests` together in one invocation. That bent
   section 1's one-type rule once, and I'm saying so here.

## 2. What the owner should expect

On the CW tab, the `■` blocks that used to sit around the text are mostly gone. When the decoder
hears something it cannot clear its own evidence bar on, it now prints nothing, where it used to
print a block. Across the 37 saved recordings, blocks drop from 289 to 28, and every letter the
decoder named before is still there. The few blocks left mark places where the keying was strong
but did not spell a Morse character, so something really was sent there. At 17:37 you saw
`CQ CQ CQ DEW B 6 RE D W B`. The gate removes only characters below its bar, and on no saved
recording is a named letter below it, so the named letters stay. What goes away is the blocks
between them, and the leftover-noise block that used to appear when a transmission ended.
**The spacing fault in that call is not touched.** Word boundaries and the callsign split are the
tone tracker's and the timing's business, and 3.8 is next. None of this says the text is correct.

**What will look wrong but is not:**
- Where a whole "word" used to be a single `■`, you will now see two spaces.
- The bench replay of the 17:37 recording reads differently from what the tab showed at the radio.
  The recording is the last 30 s, read by a decoder starting cold. The tab had been listening
  longer.

## 3. What you should see

**3.7 in one line:** bar 1.0 per hop. Placeholders on the 37 captures fall **289 to 28**. Named
characters go **1,295 to 1,297**. The three anchors are unchanged on every named character. The
17:37 scored region is 29 edits against an inferred key, before and after.

| capture | old total | named, 3.9 | placeholders, 3.9 | named after gate | placeholders after gate |
|---|---|---|---|---|---|
| cw-2026-08-17-013347 | 59 | 57 | 2 | 57 | 0 |
| cw-2026-08-17-013622 | 55 | 51 | 4 | 51 | 0 |
| cw-2026-08-17-134712 | 63 | 21 | 42 | 21 | 1 |
| cw-2026-08-18-003016 | 57 | 54 | 3 | 54 | 0 |
| cw-2026-08-18-003126 | 54 | 48 | 6 | 48 | 1 |
| cw-2026-08-18-003758 | 63 | 44 | 19 | 44 | 1 |
| cw-2026-08-18-004507 | 50 | 49 | 1 | 49 | 0 |
| cw-2026-08-20-014854 | 0 | 0 | 0 | 0 | 0 |
| cw-2026-08-20-014935 | 0 | 0 | 0 | 0 | 0 |
| cw-2026-08-22-014113 | 0 | 0 | 0 | 0 | 0 |
| cw-2026-08-22-014308 | 0 | 0 | 0 | 0 | 0 |
| cw-2026-08-22-031838 | 57 | 42 | 15 | 43 | 1 |
| cw-2026-08-22-031905 | 42 | 36 | 6 | 36 | 1 |
| cw-2026-08-22-031948 | 34 | 31 | 3 | 31 | 0 |
| cw-2026-08-22-032012 | 44 | 43 | 1 | 43 | 0 |
| cw-2026-08-22-032050 | 53 | 44 | 9 | 44 | 2 |
| cw-2026-08-22-032113 | 55 | 47 | 8 | 47 | 2 |
| cw-2026-08-22-032129 | 66 | 65 | 1 | 65 | 1 |
| cw-2026-08-23-001520 | 5 | 1 | 4 | 1 | 1 |
| cw-2026-08-23-001831 | 55 | 44 | 11 | 44 | 1 |
| cw-2026-08-23-001952 | 75 | 56 | 19 | 57 | 2 |
| cw-2026-08-23-002016 | 75 | 44 | 31 | 44 | 4 |
| cw-2026-08-24-012403 | 22 | 21 | 1 | 21 | 0 |
| cw-2026-08-25-011552 | 30 | 22 | 8 | 22 | 1 |
| cw-2026-08-25-012748 | 2 | 2 | 2 | 2 | 2 |
| cw-2026-08-25-012823 | 41 | 26 | 15 | 26 | 0 |
| cw-2026-08-25-012922 | 50 | 45 | 5 | 45 | 0 |
| cw-2026-08-25-013010 | 54 | 48 | 6 | 48 | 0 |
| cw-2026-08-25-013150 | 58 | 51 | 7 | 51 | 2 |
| cw-2026-08-25-013303 | 54 | 44 | 10 | 44 | 1 |
| cw-2026-08-25-013402 | 61 | 56 | 5 | 56 | 1 |
| cw-2026-08-25-013520 | 60 | 55 | 5 | 55 | 0 |
| cw-2026-08-25-013637 | 63 | 60 | 3 | 60 | 1 |
| cw-2026-08-25-021410 | 47 | 36 | 11 | 36 | 0 |
| cw-2026-08-25-021629 | 47 | 27 | 20 | 27 | 2 |
| cw-2026-08-25-021825 | 41 | 25 | 16 | 25 | 0 |
| cw-2026-08-26-125941 | 0 | 0 | 0 | 0 | 0 |
| **total** | **1,592** | **1,295** | **299** | **1,297** | **28** |

`cw-2026-08-25-012748`'s old total, 2, is its floor. It emitted 4 at entry. The after-B2 column
and the 17:37 row are in `docs/phase-cw/unit408-floors.md` section 4.

**Step 3:** 3.1 to 3.7 and 3.9 ticked. 3.8, the tracker, is not started. That is R56's grant, and
it belongs to the next unit.

## 4. What's blocking us

Nothing blocks a criterion of step 3. Three findings go to `docs/phase-cw/PARKED.md` under R54:
- **408 item 1**: the 17:37 sidecar is absent, and its session counts cannot be replayed from the
  WAV.
- **408 item 2**: `CwEmissionGate.cs` does not exist, and the bar's value was not raised.
- **408 item 3**: two spaces now stand where a placeholder-only word stood.
