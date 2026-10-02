```
UNIT: 525 - partial (three tasks landed; the 5-dit row and +6 dB stay short; unit 519's edge case turns red) - 2026-10-02
UNIT GOAL: the word gap is five dits, and a mark survives AGC and a fade
NUMBER: AGC +2 to +4 dB and the 6 dB fade read the bulletin whole (AGC after its first letter), and the 7-dit call reads whole
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`, and the order's gate held: `SHACK_FACTS.md`, `CwRunReader.cs` and `CW_REQUIREMENTS.md` exist, there is no `CoreHMI.sln` or `MURC.sln`, and the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio. Unit 525 and HM-DEC-229 were free.

**How the session ran:**
- It took SESSION.lock and released it at the end.
- It wrote nothing to `RUN_LEDGER.md`, touched nothing under `tools\arbiter\`, and ticked no box.
- It read no recording, fixture or telemetry; every case is synthetic audio written in its test.
- Nothing keys, transmits or writes to the radio.
- **Unit 517's word-gap ask is answered** by this order and the owner's ruling below, so it leaves the outstanding queue.
- **One question was put to the owner** mid-run. The order's five-dit rule and its 5-dit green contradict each other on the test's own audio, and the owner chose the literal five dits.

**Task 1: the word gap is five dits, decided in the gate.** Commit `8fe70d70`.
- **`CwPatternGate.cs`:**
  - `GapLines` draws a sender's element, letter and word lines from its dit, its gaps inside letters and its gaps between runs.
  - `CwGapLines.KindOf` labels a gap element, letter or word, and `DitFromGaps` reads a one-length sender's dit from its gaps.
  - New constants: `WordGapDits`, plus `MeasuredRunGaps` and `SlowestFarnsworthWordGapSeconds`, both moved from the reader.
- **`CwRunReader.cs`:**
  - The arithmetic of units 500, 501, 504, 510 and 513 moved into the gate unchanged.
  - A new letter, a new word and the re-split of unprinted runs all ask `KindOf`.
  - The cluster helpers stay shared, because the mark kinds use them too.
  - Not moved: the waits built on the lines (twice the word gap to release or confirm, the calling lag) and the mark-length √3. These are times and mark kinds, not gap kinds.
- **The new line.** Where only the letter cluster shows and it sits under five dits, a word gap is five dits or more, counted on the true dit.
  - A mark reads short and a gap long by the same smear, so the true dit is the marks' dit plus half the smear.
  - Otherwise √(7/3) of the letter gap, as before: for Farnsworth senders, and wherever three clusters show.
- **The owner's ruling, 2026-10-02:** the literal five dits, over the nearer of three and five (3.87) and over five less the sender's scatter.
- **Test:** `TheSpacesComeFromTheShapeTests` now asserts no space inside either callsign at 4, 5 and 7 dits, and the 7-dit row whole. The 5-dit row is printed.

**Task 2: a mark's top is judged from where it settles.** Commit `3b476768`, `CwEnvelopeDetector.cs`.
- **`Run`.** A run that began with a key-down may step down, for its first `SettleHops`, by no more than the flatness tolerance per hop. Those hops count toward length, not level. A larger step is the fall, and a rise is judged as before.
- **`SettleHops` = 7.** That is the shortest dit, 25 ms, plus the 10 ms window. The IC-7300's AGC attack time is not in `A7292-4EX-6`, so this is the author's figure from what a keyed tone must do: an AGC not settled by the end of the shortest dit leaves no dit a level.
- **A key-down** is a rise, measured from before the window began to rise, of at least half the bin's keying contrast, or of its height over the loudest keying gap.
  - Without the key-down guard, noise settled too, and the noise-bar narrowness test turned nothing away.
  - Without measuring from before the window, a half-risen first hop kept the guard from firing.
- **Test:** the AGC +2, +3 and +4 rows, plain and through the filter, are now asserted. +6 is printed.

**Task 3: every level reference is local to the mark.** Commit `b4842321`.
- **Found first:**
  - Under the 6 dB fade every mark stood at its right length; I compared all 154 against the plain bulletin, and only their levels differed.
  - The fault was the reader's level reference: the mean of the sender's last eight marks, about two seconds at 18 WPM, while the fade moves the station up to 4.7 dB a second.
  - A second 625 Hz sender began at 15.7 s, the J of JUMPS, and the letters were dealt between the two.
  - The gate's agreement uses the same eight-mark mean, and it refused a few fade-sized marks.
  - The other references were checked and are not the fault. The bars' level pairing reaches back a second, but neighbouring bars sit a quarter-second apart. The gap wander decides the keying light, not delivery. The contrast fades with the signal.
- **Fixed:** the reader (`Sender.Reference`) and the gate (`Agrees`, `TakesQuieter`) judge level against the sender's last three marks, `CwPatternGate.LevelMarks`, a letter's worth.
  - The last mark alone fixed the fade but split `N0CALL` at 600 Hz, 12 dB, on its own noise.
  - Three marks are half a second at 18 WPM, across which the fade moves the mean about a decibel.
  - Pitch is still read over the eight, since a fade does not move it.
- **Test:** the fading row now asserts the bulletin whole.

**Records:**
- HM-DEC-229 in `DECISIONS.md`.
- The `CLAUDE.md` index row.
- `PHASE_OUTCOME` (both copies) has `## UNIT 525 - STEP 12`.
- `PHASE_STATUS` (both copies) names 525.
- Version 1.13.209 to 1.13.210.

**Build and app line:** build 0 warnings, 0 errors. App carry-forward 278 of 278.

## 2. What the owner should expect

- **Rebuild.**
- **Hand senders who pause five dits between words get their spaces.** Four-dit pausers still run together, and that is the limit of what timing can tell. A hand that wanders under five dits loses that space; the test hand at "five dits" wanders to 4.2, so half its spaces still go.
- **A strong station through AGC FAST no longer loses its dahs** at the start of each mark, for overshoots up to 4 dB. Only the very first letter of a transmission still can, because it comes before Hamlet knows anybody is keying. A 6 dB overshoot is still too much without the fit.
- **A station fading slowly reads whole.**
- **One thing reads differently.** Beside a louder hand sender at the filter's centre, a clean station at the edge no longer prints first. The hand sender's first word (`TEST`, correctly read) prints, then the clean station takes over, two letters later than before.
- **Nothing else changed.** The strong bulletin still reads identically with the fit on and off, and every other existing case reads as before. Only the shipped path (shape-first off) was measured.

## 3. What you should see

**The word-gap rows** (18 WPM, a hand's scatter of a sixth):

| word gaps | true gaps, dits | reads |
|---|---|---|
| 4 dits | 3.38 to 4.63 | `KI1MMDEVE2JDNAMEISJEANQTHQUEBECHW` (as at HEAD) |
| 5 dits | 4.22 to 5.76 | `KI1MM DEVE2JD NAME ISJEANQTHQUEBECHW` (as at HEAD; line 352 ms, was 362) |
| 7 dits | 5.87 to 7.98 | `KI1MM DE VE2JD NAME IS JEAN QTH QUEBEC HW`, whole |

- No space inside `KI1MM` or `VE2JD` at any of them.
- Letter gaps reach 3.49 dits.
- The fist scattered by a third now reads `CQ CQ DE N0CALL N0CALL K` (it read `N0CALLK`).
- The 5 WPM Farnsworth call reads `CK CQ DE N0CALL N0CALL K` at 10 dB, as at HEAD.

**The AGC rows** (the bulletin at 24 dB, 18 WPM):

| overshoot | HEAD, fit off | now, fit off and on |
|---|---|---|
| +2, plain and filter | whole | whole |
| +3 | `HE E I INEE SE E I E U E …` | `HE QUICK BROWN FOX JUMPS OVER THE LAZY DOG 0123456789` |
| +3 filter, +4, +4 filter | dits | the same, all four ways |
| +6 | nothing | nothing (fit off); `IE G UICK BROWN FOX …` with the fit on (was `IEN I I MW …`) |
| +6 filter | nothing | nothing (fit off); `EIE UE CK BROWN FOX JUMPS OVER THE LAZY DOG 0123456789` with the fit on (was `EIE I SI SE IE ES E S`) |

**The fade rows:**

| case | HEAD | now |
|---|---|---|
| bulletin, 6 dB over 4 s | `THE G NK D ERO WN U EOD T JU MP …` | `THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG 0123456789`, fit on and off |
| station 24 to 10 dB and back | whole | whole |

**Existing cases, shipped path:**
- **Reds as at HEAD:** `TheCallReadsAtEveryStrength(8)`, `FarnsworthAndFastReadAtTenDecibels`, `ALetterReadFromNoiseDoesNotReachTheScreen(blocks: True)`, and the carrier tests at 725 and 775, which set shape-first themselves.
- **New red:** `ACleanSenderAtTheEdgeOutranksALouderFistAtTheCentre`. It prints at 600 `TEST`, at 825 `ACKEEN■CALAEN■KAEILK`; it printed nothing at 600 and `RTACKEN■CALAEN■KAEILK` at 825 before.
- **Better:**
  - the burst with blocks asked reads `CQ CQSHDE N0CALL K` (was `NCAL`);
  - the 8 dB "before" row reads `CQ NIQ DE N0CALL N0NIALL K`.
- **Report-only rows** that moved between junk readings:
  - the 10 and 12 dB fit-off rows (12 dB now `CQ CQ DE N0CALL E0CALL K`, was `CT A CQ DE N0CALL N0CALL K`);
  - the 8 dB fit-on row (`CQ IIQ` for `CQ NIQ`);
  - the carrier junk.
- **Unchanged:** the strength table (24, 16, 12 and 10 dB whole), the speed change, the fists, the two stations, the five pitches, the quiet dit and dah, `TEST DE W1AW K`, `DE DE`, and the lone marks.
- **Noise:** 30 s and 3 min stand nothing. `DecisionLogOrderTests` gaps check is red as at HEAD; the order check passes.

## 4. What's blocking us

1. **Unit 519's edge case is red on the shipped path.** Settling let the louder fist's marks through the filter stand whole, so it qualifies before the clean sender and prints its first word. The fist is a real station and the word is right, but the case asked that it print nothing. The next unit should decide whether the first pick should wait for a second sender's evidence.
2. **The 5-dit row reads as at HEAD** under your literal five-dit ruling. Its own audio sends half its word gaps under five dits.
3. **A 6 dB AGC overshoot** still breaks the per-hop path with the fit off. Seven settle hops are not enough, and the radio's attack time is not published.
4. **The first mark of a transmission** gets no settling, since nobody is keying yet.
5. **Shape-first (off) was not re-measured** against these changes. Task 2's settling and task 3's references sit on paths it shares.

### Asks still outstanding

- **Unit 522, 2026-10-01:** whether to switch shape-first on before its failures are fixed. Still off. Two cases remained at unit 524.
- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by following a standing sender on its own bin (unit 524). Before it stands, it is still open.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
