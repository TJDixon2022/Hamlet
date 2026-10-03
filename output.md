```
UNIT: 533 - all three tasks done - 2026-10-03
UNIT GOAL: a reply is read from its first letter
NUMBER: the QSO of 14:40:20 printed 5 words of 2 stations; it prints both, the reply from its W, with 1 letter of 18 wrong
```

## 1. What Claude did

Claude Code on the development machine, branch `main`. The prompt claimed `PROJECT: Hamlet`, and the gate held: `SHACK_FACTS.md`, the recordings the order names and `CwSenderGate.cs` exist, there is no `CoreHMI.sln` or `MURC.sln`, and the root is `C:\Source\HamLet`. Hamlet confirmed. Nothing in this report is evidence about the radio beyond the owner's five recordings. HM-DEC-237 was free.

**How the session ran:**
- It took SESSION.lock and released it at the end.
- It wrote nothing to `RUN_LEDGER.md`, touched nothing under `tools\arbiter\`, and ticked no box.
- **R88 was lifted for the owner's five recordings and no other.** Seven more recordings (`cw-2026-10-03-221502` to `-221851`) and two `cases-*.txt` sheets appeared in `tests\fixtures\cw\captured`; they were not read or committed.
- Nothing keys, transmits or writes to the radio.

**Task 1: the new sender's letters are kept.** Commit `eeb27676`. It also carries the four recordings of 2026-10-03 and their sheets, beside the tests that read them.

**What the fault was.** Two causes in the gate's sender stage (`CwSenderGate.Print`):
- **The released first station was picked again.** At 14:40:20 the 500 Hz station ends its over at 11.3 s and is let go. It still had the better shape (0.49, against the reply's 0.15 to 0.39), so the next sweep picked it again, then let it go again, and the reply never held the terminal.
  - **Change:** a sender silent past its release is no longer a candidate.
- **The reply's letters sent while another held the terminal were skipped** as "printed over".
  - **Change:** they are printed, in order, after what is already printed, then its live letters.

**What did not change:**
- Nothing printed is revised.
- The lone-letter rule and word ends apply to the backlog as they do to live letters.
- A sender that never stands has nothing to print.
- The handover itself.

**The scroll needed no change.** It places each letter by its own time (`CwTrainingGraph.Settle`), so backlog letters land over their blocks while those are still on screen.

**The three cases:**
1. **`cw-2026-10-03-144020`, read from the reply's first mark.**
   - **When the reply starts.** On the 600 Hz lane the reply's first mark is the dit of its W, at 13.57 s (dit 90 ms, then two dahs of 240 ms). Nothing of it is above the noise before that, so the opening is not cut by the overlap.
   - **The true text, measured from the marks at each pitch:** `ES OK ON PA <BT> WX I N N E T A G I T I U N TEMP`. It agrees with the web session's element list.
   - **One letter Hamlet cannot read.** The U at 24.09 s is a dit, a 40 ms gap, then 270 ms of tone. The only break where its second gap belongs is a 2 dB notch at 24.29 s. The dit stands. The two pieces of tone are turned away by the single-mark shape test, because a 2 dB notch is not a key edge. So the letter prints as E.
   - **Result:** the test asserts the true text and is **red on that one letter**.
2. **Synthetic QSO** (`CQ CQ DE W1AW W1AW K`, 15 WPM at 550 Hz; then `W1AW DE K3ZZ K3ZZ K` at 650 Hz, starting 2 s before the first ends; through the filter, 1 dB AGC):
   - **Before:** `… K AW DE K3ZZ K3ZZ K`.
   - **After:** `… K WE MAW DE K3ZZ K3ZZ K`.
   - **Why it is still red:** the reply now prints from its W. Its `1` loses two dahs keyed under the first sender's dah, 100 Hz away. In those dahs the beat between the two tones breaks the top into pieces. The key-up test ("a mark ends where its tone ends") turns each piece away because the tone carries on after it, so they never become candidates. That is the beat HM-DEC-225 left open, not the handover, so the detector was not touched.
3. **A sender beside the printed one that stops first** (`TEST TEST` at 650 Hz, 6 to 13 s, under a 27 s call at 550 Hz) prints nothing. Green.
   - **Right for the owner's screen?** Yes for a station that never takes over, since it would interleave a second conversation into the first.
   - **But:** when it is the other half of a QSO, its letters are lost. Not changed, as ordered.

**One existing test changed its yardstick.** `TheShapePicksTheSenderTests.ABetterShapeTakesTheTerminalAtTheNextSilence` required the clean sender's first letter to end, in audio time, after the fist's last. Its backlog now prints those earlier letters after the fist's whole call. The check is now **print order**, which is what "nothing of the clean one before the fist falls silent" means on the screen. Green.

**Task 2: the other recordings, reported.**
- The `-144045` letters test (`TheNextOverReadsItsLetters`) is in the same test file, so it went in with task 1's commit; there is no separate task 2 commit.
- **`-144045`** reads `N TEMP 57 57<BT>BTUBOB D`, the same as at HEAD. The leading N is the end of `IUN` from the recording before.
  - **The 940 ms gap (`TU BOB`).** This sender's word line is drawn at 1,121 to 1,194 ms, the boundary between its own letter and word clusters. The hand's gaps overlap: its letter gaps run up to 709 ms and its word gaps start at 639 ms. So the word gaps of 925, 945 and 1,015 ms fall under the line and count as letter gaps, and `57 <BT> B TU BOB` runs together. Only gaps over about 1.1 s print a space.
  - **The missing `E KG8V K`.** The same overlap stretches the letter-gap cluster from 195 ms to about 1 s, and letter-gap tightness falls to 0.17 at 18.65 s and 0.10 at 21.80 s.
    - The shape drops to 0.089, then 0.052, both under the 0.1 release line.
    - The sender is let go twice. The first time it is picked again at 20.32 s and its backlog `B` prints. The second time it never comes back.
  - **Result:** the letters test is red: `NTEMP5757<BT>BTUBOBD` against `NTEMP5757<BT>BTUBOBDEKG8VK`.
- **`-143951`** prints nothing. The 500 Hz station stands 34 marks from 15.76 s, but its shape never reaches the 0.2 needed to qualify; it peaks at 0.169, held down by letter gaps (0.24 to 0.48) and dit-to-dah separation (0.69). The weaker signal at 700 Hz stands nothing.
- **`-143906`** prints nothing. The 515 Hz station reaches 0.21 to 0.26 between 17.3 and 19.0 s. Its letter-gap tightness then falls to nought before the first pick's word-gap wait ends, and it never qualifies again.

**Task 3.** `TheOwnersRecordingReads` passes as at HEAD: `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`.

**Records:**
- HM-DEC-237 in `DECISIONS.md`, and the `CLAUDE.md` row.
- `PHASE_OUTCOME` (both copies) has `## UNIT 533 - STEP 12`.
- `PHASE_STATUS` (both copies) names 533.
- Version 1.13.217 to 1.13.218.

**Build and app line:** build 0 warnings, 0 errors. App carry-forward 275 of 278. The three losses, each at 1 ms ("You've caused dispatcher loop"), pass alone:
- `HisCardIsDrawnInTheSendingGreenWithTheWord`
- `ThreeChipsCostTheTopBandNothing`
- `AClickTunesTheStarFillsAndTheCrossForgetsAndThatPersists`

## 2. What the owner should expect

- **Rebuild.**
- **When a QSO changes hands, the reply now prints from its first letter** instead of losing its opening. On your recording from 14:40:20 the terminal used to stop at `ES OK ON PA <BT>`. It now carries on with `WX IN N E TA GIT IEN TEMP`, where the true text is `… IUN TEMP`.
  - **The one wrong letter.** The `U` was keyed with almost no second gap, a 2 dB dip, so Hamlet hears a dit and then one long tone it won't accept as a dah.
- **The reply's letters print after the first station's**, not interleaved, and nothing already printed changes.
- **A station that talks over another and stops first still prints nothing.**
- **14:40:45 still reads `N TEMP 57 57<BT>BTUBOB D`.** Two problems, both from how unevenly this operator spaces:
  - **The spaces are missing.** His letter and word gaps overlap, so Hamlet's word line sits near 1.1 s.
  - **The end, `E KG8V K`, never prints.** That same unevenness makes his spacing look ragged enough that Hamlet lets him go.
- **14:39:51 and 14:39:06 print nothing:** neither station's spacing holds together long enough to qualify.

## 3. What you should see

**The QSO recording, `cw-2026-10-03-144020`:**

| | reads |
|---|---|
| true text, from the marks | `ES OK ON PA <BT> WX I N N E T A G I T I U N TEMP` |
| before (HEAD) | `ES OK ON PA <BT>` |
| now | `ES OK ON PA <BT> WX IN N E TA GIT IEN TEMP` |

**The other recordings:**

| recording | measured | before (HEAD) | now |
|---|---|---|---|
| `-144045` | `N TEMP 57 57 <BT> B TU BOB DE KG8V K` | `N TEMP 57 57<BT>BTUBOB D` | `N TEMP 57 57<BT>BTUBOB D` |
| `-143951` | report only | empty | empty |
| `-143906` | report only | empty | empty |
| `-200157` | `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA` | same | same, green |

**The synthetic cases:**

| case | before | now |
|---|---|---|
| QSO, reply 2 s into the first's end | `CQ CQ DE W1AW W1AW K AW DE K3ZZ K3ZZ K` | `CQ CQ DE W1AW W1AW K WE MAW DE K3ZZ K3ZZ K` (red: two dahs lost under the first's dah) |
| a sender beside that stops first | `CQ CQ CQ DE W1AW W1AW W1AW K` | the same, green |

**The existing cases**, every printed reading line against unit 532:

| set | lines identical |
|---|---|
| a (53 tests) | 55 of 55 |
| b1 (27) | 19 of 19 |
| b3 (29) | 43 of 43 |
| b4, first half (31) | 25 of 27 |
| b4, second half (8) | 13 of 13 |
| b5 with the recording (40) | 36 of 36 |

**The two lines that changed** are both "before" diagnostics, with shape picking turned off. That old rule now prints a backlog too:
- `… K 1AW K TEST DE W1AW K` became `… K W ■AW K 1AW K TEST DE W1AW K`;
- `TEST DE W1AW TEST DE W1AW K IL K` became `… K RT A C K N ■CALAE N ■K AE IL K`.

**Every reading case with shape picking on reads as at HEAD.**

**Reds:**
- Known before this unit:
  - the 8 dB call;
  - Farnsworth and fast at 10 dB;
  - a letter from noise (blocks);
  - the random carrier at 825 Hz;
  - the clean sender at the edge;
  - the radio strength table at 8 and 10 dB;
  - `AFistThatTightensIsFollowed`.
- New, each the order's own case: 14:40:20 (one letter), the synthetic QSO (two dahs), and 14:40:45 (the release).

## 4. What's blocking us

1. **Two senders keying at once, 100 to 200 Hz apart, lose marks** to the beat, and the key-up test refuses the pieces. It costs the synthetic QSO two dahs, and puts holes in a clean sender's backlog beside a fist (HM-DEC-225).
2. **A hand whose letter and word gaps overlap (14:40:45):**
   - its word line sits above its word gaps, so it prints no spaces;
   - its letter-gap tightness collapses and it is let go mid-over.
3. **A U keyed with a 2 dB notch for a gap** prints as E, since a notch is not a key edge.
4. **14:39:51 and 14:39:06 never qualify:** their shapes stay under 0.2.
5. **A second station that stops before the printed one prints nothing.** That suits a station talking over another; it costs the other half of a QSO. Not changed, as ordered.
6. **Pre-existing app reds** outside the line are untouched.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
