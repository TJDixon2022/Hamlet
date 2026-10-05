## 1. What Claude did

- **Session:** development computer, Claude Code. The prompt claimed `PROJECT: Hamlet`.
- **Gate:** checked in `C:\Source\HamLet`.
  - Present: `SHACK_FACTS.md`, `docs\cw-scoreboard.md`, `tests\fixtures\cw\captured\scans\catch-153810-7033367.wav`.
  - Absent: `CoreHMI.sln`, `MURC.sln`.
  - An earlier paste of this order was refused at the gate, before the catches were in the tree.
- **Nothing in this report is evidence about the radio.**
- **Work instruction 544, run by hand on `main`:**
  - SESSION.lock was taken and released.
  - Nothing was written to RUN_LEDGER.md, nothing under `tools\arbiter\` was touched, and nothing in PHASE_PLAN.md was ticked.
  - Nothing keys or transmits; listen-only is unchanged. The only radio write is the frequency.
- **R88:** lifted for the owner's twelve recordings and the scan catches the instruction names.
  - Three catches are in the tree.
  - The six that are not (`144059`, `041036`, `035414`, `040053`, `143718`, `144637`) are **waiting**, not failed; synthetic stand-ins were used where the order allows.
- **Version:** 1.13.228 → 1.13.229. **Ruling:** HM-DEC-248, *What three real scans found*.
- **The scoreboard read 191 after every task** (208 right, 15 wrong, 2 invented).
  - Hard limits are as at HEAD: the carrier at seed 5195 and one letter in a silence stay red; noise prints nothing; the first recording reads.
  - Spaces added went from 2 to 3 with task 1. Spaces are not in the score.

**Task 1: why the over falls apart** (commit `e13ac5a9`).
- **What was traced:**
  - the main catch read through the app's chain at the scan's passband (pitch 600, filter 500);
  - beside it, my offline read: mixed at the station's 860 Hz, a 40 Hz low-pass run forward and back, one level cut, and letters and words cut from its own three gap clusters;
  - in ten-second windows: marks missed or added, dit/dah flips, letter cuts, and the gate's dit, split, character and word lines and level reference.
- **What holds steady:** the chain hears the marks (394 against 411), cuts letters where the offline read does, and keeps its level reference within about a decibel.
- **What goes wrong: dot against dash, two ways, one root.**
  - **Broken dahs.** Nine dahs the offline read hears whole (about 150 to 165 ms) come out of the detector as two short marks about 20 ms apart. Each is read as two dits.
  - **The line collapses.** At 13.9, 34.7, 36.5, 43.8, 45.5, 50.7, 65.5 to 67.3 and 80.6 s the gate showed "dit none, dah none": among its last forty marks no two neighbouring lengths differed by twice, because the broken pieces (85 and 107 ms) filled the jump. The gate then took the dit from the gaps inside letters, 36 ms on this heavy fist where its dits are 65. The line fell to 65–69 ms and fourteen letters had dits read as dahs.
- **Fixed:** the sender keeps the line it last showed while a few marks hide the jump, redrawn from the marks now (rule switch `a sender keeps its line`).
  - The plain version kept a stale line, and the straight key's second `N0CALL` read `N0FALLK`. Redrawing the line from the marks now restored it.
  - The seven CW reds that remain are the same seven red at HEAD, checked in a worktree.
- **Not shipped:** treating a piece of a mark as part of its neighbour.
  - With the join it measured 188; without the join, 190.
  - It turned `NETAGIT` into `NETAGTT` on the owner's fists. The broken dahs remain.
- **The `scans` table:** both station catches, scored against my offline reads, are in `docs\cw-scoreboard.md`. They are **pending**: reported, never totalled.

**Task 2: the tone sits at the pitch before the stay** (commit `507dde3d`).
- **Found in the catch JSON:**
  - At the scope's peak neither station gave a tone. The peak was 380 and 315 Hz from the station, outside the filter.
  - Half a filter to one side the tone was heard, and the retune went the wrong way: +130 Hz took 730 Hz to 860, and −65 Hz took 535 to 470.
  - On the owner's radio in CW the tone rises with the dial; landing assumed it falls.
- **Fixed:** after landing the scan moves the dial, measures the tone again, and repeats, up to three moves, until it is within 10 Hz of the pitch.
  - Where a move takes the tone further away or out of the filter, it turns round and remembers the direction for the scan.
  - It starts from CW rising (CW-R the other way).
  - The catch JSON gains `toneAfterHz` and `toneFollowsDial`.

**Task 3: a carrier is a carrier** (commit `c9e3b6a8`).
- **The rule:** after 8 s of a stay, the share of 20 ms frames at the tone sitting 6 dB or more under the median of their own second.
  - A keyed signal is key-up a quarter of its time or more. Under **0.15** the catch is `carrier`, left at once.
  - Its true frequency is remembered (dial, less the tone's offset from the pitch, the way the tone moves), and any peak within **400 Hz** of it is passed by, from either side.
- **Measured:** the two stations read 0.23 to 0.39 over every 8 s; the carrier in the tree 0.085 to 0.105.
- **The 400 Hz:** the scope's peak sat 315 and 380 Hz from the stations it led to.

**Task 4: fewer empty stops, no lost stations** (commit `ee84f0b0`).
- **The survey's repeat count:**
  - A place is listed only where, at some height, its tops outnumber what noise that high gives by chance: under one false station in **a hundred** surveys, and **three** sweeps at least.
  - That replaces the quarter of the sweeps. At 5.26 Hz bins with dense noise, a quarter is what noise does, and a station pausing between overs fell short of it.
- **The tone check:** judged over all its pieces and over their strongest quarter, the plainer taken. A steady carrier stays plain, and a station keying half its check is not missed.
- **The longer listen:** a peak the scope draws at **20** or higher (three times the noise blips' 6 and 7) gets a **6 s** listen before it is called empty. That is longer than the 4 s an operator pauses between overs.

**Records:** DECISIONS.md HM-DEC-248, the CLAUDE.md §1 row, PHASE_OUTCOME and PHASE_STATUS in both copies, the scoreboard rows, and the version.

## 2. What the owner should expect

- **Rebuild.**
- **Why the strong station's over fell apart:** the station sent heavy, and now and then one of its dahs came through as two short pieces. A run of those hid the difference between its dots and dashes. Hamlet then guessed its dot length from the spaces, which on this heavy fist are about half a dot, and from then on read ordinary dots as dashes.
- **Whether it reads now:** better, not whole.
  - It now keeps the dot/dash line it had already learned. The main catch went from 103 to 109 letters right of 152, and `THURSDAY` reads `TIEURSDAY` where it read `TIEUGODAY`.
  - The dahs that arrive in two pieces still read as two dots.
- **The two stations, before and after** (through the app, as the scan hears them):
  - 7.0334, 860 Hz, before: `… SACK PAIN SOTHIW TIEUGODAY HAVE RM OTINE6 … RMYT MSEE WHATITA …`
  - 7.0334, 860 Hz, after: `… SACK PAIN SOTHIS TIEURSDAY HAVE RM UTINE6 … RAYT MSEE WHATITA …`
  - 7.0511, 470 Hz: unchanged, `N E ANEI BTW■ I E GG IRIATE YOUR NICE KEYI N`.
- **Landing:** the scan now puts each station's tone at your CW pitch, and checks again after each move. Your radio raises the tone as the dial goes up, and Hamlet had it backwards, which is why the strong station sat at 860 Hz on the edge of your filter.
- **Carriers:** a tone that never keys is now called a carrier after 8 s, the scan moves on, and it does not stop on that carrier again from either side.
- **Empties:** far fewer stops on noise. A station that pauses between overs is listened to for longer before the scan gives up on it, if the waterfall shows it strongly.
- **For you to confirm by ear.** These are my offline reads of the two catches. They are pending, and neither counts toward the score until you say so.
  - `catch-153810-7033367` (860 Hz): my read was `T HE MATRESS IS SOFT ES CATT FINDA SAFE SPOT WITH OUT PAINHEEQ IKE NEUER HTD BACK PATN SOTHIS TRURADAY HAKE ROUTTNE X EEE KK UW ESGG TOASK UOR X AAY TMSEE WUAT I TWINTED HEE<BT> XIREMETBER AT AGE`. The reference uses the shack read's words where it gives them whole: `THE MATRESS IS SOFT ES CANT FIND A SAFE SPOT WITHOUT PAIN HEE IVE NEVER HAD BACK PAIN SO THIS THURSDAY HAVE ROUTINE X EEE KK UW ESGG TO ASK FOR XRAY TO SEE WHAT I TWINTED HEE<BT> I REMEMBER AT AGE`. The middle run (`X EEE KK UW ESGG`) neither read makes sense of.
  - `catch-154819-7050903` (470 Hz): `KS AND BTW, I AGGREIRIATE YOUR NICE KEYIE R`, probably *thanks and by the way, I appreciate your nice keying*.

## 3. What you should see

**Task 1, the drift trace of `catch-153810-7033367` at HEAD** (the chain against the offline read, ten-second windows):

| window | offline marks | chain marks | missed | dot/dash flipped | letter cuts missed / added | dit ms | split ms | char line ms | word line ms | level ref dB |
|---|---|---|---|---|---|---|---|---|---|---|
| 0-10 s | 43 | 31 | 14 | 3 | 0 / 0 | 60 | 104 | 85 | 284 | -18.9 |
| 10-20 s | 51 | 51 | 1 | 1 | 0 / 0 | 64 | 108 | 78 | 253 | -19.1 |
| 20-30 s | 48 | 42 | 6 | 0 | 0 / 0 | 60 | 108 | 86 | 293 | -18.6 |
| 30-40 s | 52 | 52 | 3 | 6 | 0 / 0 | 60 | 112 | 74 | 209 | -19.3 |
| 40-50 s | 53 | 56 | 0 | 9 | 0 / 1 | 59 | 104 | 70 | 205 | -18.9 |
| 50-60 s | 45 | 45 | 2 | 7 | 0 / 1 | 59 | 109 | 83 | 304 | -19.0 |
| 60-70 s | 47 | 43 | 7 | 5 | 0 / 0 | 64 | 106 | 80 | 292 | -19.2 |
| 70-80 s | 49 | 50 | 2 | 3 | 0 / 0 | 65 | 111 | 74 | 243 | -18.9 |
| 80-90 s | 23 | 24 | 1 | 1 | 0 / 0 | 49 | 88 | 71 | 194 | -19.9 |

- **Every flip, by kind:**
  - Nine pairs are one dah read as two short marks, at 3.54, 31.62, 32.65, 40.69, 49.17, 52.14, 53.63, 69.54 and 72.11 s. For example, at 31.62 s the chain has 67 and 50 ms, 23 ms apart, where the offline read has one 150 ms dah.
  - Nine are dits read as dahs at a split of 65 to 67 ms, at 36.66, 43.84, 43.94, 45.54, 46.03, 51.33, 51.44, 65.45 and 81.08 s.
- **At those times the gate's clusters read** `dit none, dah none, split 67 ms; gaps element 36 ms ±0.26, letter 142 ms, word 286 ms`.
- **After the fix:** the "dit none" collapses are gone; flips per window are 3, 1, 0, 5, 7, 5, 4, 3, 1. The broken dahs remain.

**The two station catches through the app's chain:**

| catch | when | marks at the tone | shape | green | right / wrong against the pending reference |
|---|---|---|---|---|---|
| `catch-153810-7033367`, 860 Hz | before | 394 | 0.136 | yes | 103 of 152 / 39 |
| | after | 394 | 0.136 | yes | 109 of 152 / 33 |
| `catch-154819-7050903`, 470 Hz | before and after | 82 | 0.532 | yes | 25 of 35 / 6 |

**Landing** (`TheScanLandsByEar`, scope peak off the call, both directions of tone):

| scope off | tone with the dial | tone heard | tone after | dial from the call |
|---|---|---|---|---|
| 260 Hz | rising | 610.4 Hz (half a filter aside) | 600.3 Hz | +2 Hz |
| 260 Hz | falling | 589.6 Hz | 598.9 Hz | +2 Hz |
| 150 Hz | rising | 749.6 Hz | 599.7 Hz | −2 Hz |
| 150 Hz | falling | 450.4 Hz | 601.1 Hz | −2 Hz; the scan learned the direction |
| 200 Hz | rising | 805.0 Hz | 600.0 Hz | 0 Hz |

**Carriers** (`ACarrierIsACarrierTests`, as the scan listens, judged at 8 s):
- `catch-154614-7047190` (in the tree, called negative at shape 0.45 before): tone 670.4 Hz, key-up **0.103**. It is a carrier, ended at 8.0 s, printed nothing, no shape seen.
- **A synthetic carrier held 78 s** (standing in for the missing `143718`), steady: key-up **0.000**, a carrier at 8.0 s, printed nothing.
- **The same carrier fading 6 dB:** key-up **0.000**, a carrier at 8.0 s, printed nothing.
  - Fed the whole 78 s, the decoder itself prints `ITEI D I TIEI` from this fading carrier. The scan leaves at 8 s, before it does.
- **A keyed station at the same level:** key-up **0.298**, not a carrier.
- **In the scan** (`ThreePeaksAreVisitedAndCaught`): the steady carrier is `carrier`, left after 8 s, remembered within 30 Hz, and never stopped at again.

**Empties** (`FewerEmptyStopsTests`): 3 minutes at 5.26 Hz bins, forty noise blips a sweep, and a station keying SOS with 4 s pauses.

| | surveys | listed | stops | empty | the pausing station |
|---|---|---|---|---|---|
| before (task 3 commit) | 4 | 27 | 23 | 23 (100 %) | called empty |
| after | 43 | 1 | 1 | 0 | visited, negative, not empty |

**The scoreboard rows** (`docs\cw-scoreboard.md`):

| unit | right | wrong | invented | score | printed in silence | spaces |
|---|---|---|---|---|---|---|
| 544 task 1 | 208 of 244 | 15 | 2 | **191** | 1, as at HEAD | 55 of 77, 3 added |
| 544 task 2 | 208 of 244 | 15 | 2 | **191** | 1 | 55 of 77, 3 added |
| 544 task 3 | 208 of 244 | 15 | 2 | **191** | 1 | 55 of 77, 3 added |
| 544 task 4 | 208 of 244 | 15 | 2 | **191** | 1 | 55 of 77, 3 added |

**Build:** no warnings. **Scan tests:** all 103 pass. **App carry-forward:** 277 of 278. The one loss, `AtEachOf354sNineSizesStopIsInTheStatusBarAndOnTheWindow`, is the dispatcher loop, and it passes alone.

## 4. What's blocking us

- **The two `scans` references wait on your ear.** Until you confirm them they are reported and not totalled.
- **Six named catches are not in the tree,** and their tests wait for them: `144059`, `041036`, `035414`, `040053`, `143718` and `144637`.
- **Broken dahs remain.** The detector still breaks some dahs at shallow dips, and the gate reads them as two dots. The one rule tried, joining a piece to its neighbour, lowered the score and did not ship.
- **The scan starts from the tone rising with the dial in CW,** which is what your two catches show. A radio set the other way corrects itself after one wrong move.
- **A station within 400 Hz of a known carrier is passed by** too, for the rest of the scan.
- **The 15 %, 400 Hz, 20 and 6 s figures are mine,** from what CW does and the catches here. None is measured on more of your radio's audio.
- **The silence limit stays red at one letter,** and **the carrier limit at seed 5195,** as at HEAD.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
