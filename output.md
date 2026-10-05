## 1. What Claude did

- **Session:** development computer, Claude Code. The prompt claimed `PROJECT: Hamlet`.
- **Gate:** checked in `C:\Source\HamLet`.
  - Present: `SHACK_FACTS.md`, `docs\cw-scoreboard.md`, `docs\cw-keying-map.md`.
  - Absent: `CoreHMI.sln`, `MURC.sln`.
- **Nothing in this report is evidence about the radio.**
- **Work instruction 541, run by hand on `main`:**
  - SESSION.lock was taken and released.
  - Nothing was written to RUN_LEDGER.md, nothing under `tools\arbiter\` was touched, and nothing in PHASE_PLAN.md was ticked.
  - Nothing was keyed, transmitted or written to a radio.
- **R88:** lifted for the owner's twelve recordings only.
- **Version:** 1.13.225 → 1.13.226. **Ruling:** HM-DEC-245.

**Task 1: set A ships** (commit `c8b19f44`).
- **What changed:** narrowness, three lone letters dropped, and the neighbour judgement of gaps are on by default.
- **Re-measured at HEAD before shipping:** 191, as work instruction 539 measured.
- **Every hard limit as at HEAD:**
  - the first recording reads whole with its spaces;
  - loud noise prints nothing;
  - the carrier prints at 1 of 20 seeds;
  - one letter prints in a silence.

**Task 2: ten rules leave the tree** (commit `5e46385a`).
- **How:** the six decoder files were reset to their state before work instruction 539 restored the thirteen. Only the three that earned their place were re-added, with:
  - the crowds-narrowing switch;
  - the hook the parallel scoreboard uses.
- **Removed, with their switches:**
  - the 0.2 standing line
  - quieter marks
  - the pause
  - a hand's two kinds
  - key-up
  - the five-dit floor
  - the shape's inside-letter tightness
  - the shape's letter-gap tightness
  - the rectangle fit
  - the neighbour split of marks
- **Where they live now:**
  - The tag `before-scoreboard` holds each as it stood before work instruction 534 removed it.
  - The tag `before-false-characters` holds the tree before they were restored behind switches.
- **Lines removed:** 901 lines out and 57 in, **844 lines of decoder source net**, across:
  - `CwEnvelopeDetector`, `CwPatternGate`, `CwSenderGate`, `CwSequenceShape`, `CwMark` and `CwRules`.
- **Tests:** none exercised only these rules; the rectangle-fit tests already left with work instruction 534. The scoreboard's search test now steps the kept rules only.
- **The scoreboard reads exactly as after task 1:** every stretch identical, 191.

**Task 3: the scan remembers its settings** (commit `92a188fa`).
- **Where they are kept:** the scan's length and two stays are in Hamlet's settings file: `ScanMinutes`, `ScanPositiveStaySeconds`, `ScanNegativeStaySeconds`.
- **When they are saved and read:** saved the moment they change, as the operating mode is, and read back when the window opens.
- **Out-of-range values:** a value outside the popover's range reads as the default.
- **Tests:** two new tests, plus the settings-upgrade tests, pass.

**Recorded, at the owner's order:** HM-DEC-245, *Three rules earned their place; ten left the tree*, in `DECISIONS.md` and the `CLAUDE.md` §1 index. It names the score before and after, and every rule removed with its tags.

## 2. What the owner should expect

- **Rebuild.**
- **The score goes from 186 to 191:** two more letters right (206 to 208), three fewer wrong (18 to 15), and the same two invented. Nothing reads worse on any hard limit.
- **The three rules now on:**
  - **Narrowness:** a mark only counts if its energy sits in its own pitch rather than across the whole band, the way noise and clicks spread.
  - **Three lone letters dropped:** three or more one-mark letters in a row (E, T, E…) are thrown away together, because a run of nothing but single dits and dahs is noise, not sending.
  - **A gap judged against its neighbours:** a gap is called inside a letter or between letters by comparing it with the sender's gaps on either side, so a hand that speeds up or slows down is followed letter by letter.
- **Code removed:** the ten rules that never raised the score are gone from the decoder, **844 lines**. They are kept at two tags if they're ever wanted again.
- **The scan now remembers** its length and stays across restarts.
- **What you'll see on the air:**
  - The 14:40:45 station reads `N TEMP` where it read `N E MP`.
  - The 22:15:30 station's opening reads `6 CHA MPION` where it read `H CHA MPIMTN`.
  - Some noisy stretches print a little less.
  - One low-confidence stretch, 22:18:51, prints more junk than before (section 4).
- **Build:** clean.
- **App carry-forward:** 276 of 278. The two losses are the known dispatcher-loop flake and both pass when run alone.
- **Red tests, all red at HEAD too:**
  - the scoreboard: the carrier at seed 5195, and the one letter in a silence;
  - the 20-seed carrier test at seed 5195;
  - two reply tests.
- **One red at HEAD now passes:** `TheNextOverReadsItsLetters`.
- **Pushed:** to `main`.

## 3. What you should see

**The scoreboard rows:**

| unit | right | wrong | invented | score | printed in silence | spaces right | what changed |
|---|---|---|---|---|---|---|---|
| HEAD | 206 of 244 | 18 | 2 | **186** | 1 | 58 of 77, 4 added | |
| 541 task 1 | 208 of 244 | 15 | 2 | **191** | 1 (`221502`, an `S` at 21.46 s) | 55 of 77, 2 added | set A on |
| 541 task 2 | 208 of 244 | 15 | 2 | **191** | 1 | 55 of 77, 2 added | ten rules out, 844 lines; every stretch identical |
| 541 task 3 | 208 of 244 | 15 | 2 | **191** | 1 | 55 of 77, 2 added | the scan remembers its settings |

Hard limits at every row:
- the first recording reads `FER C HAT<BT> BEST 7V 73 <SK> KC4ZGP DEWA`;
- loud noise prints nothing;
- the carrier prints at seed 5195 alone, 1 of 20.

**Each stretch that changed with set A**, before → after:

| stretch | confidence | before | after | right | wrong | invented |
|---|---|---|---|---|---|---|
| `143906` @514 | none | `IEE II E NIEEE IET TAEEKEEIEEII IMESE ITT` | `S II E NI ST TAEEKEEIEEII IMESE` | – | – | 0 → 0 |
| `143951` @500 | low | `EWAE IIEWRK I AGN ES` | `O WAE IIEWR K T AG E ES` | 12 → 12 of 16 | 3 → 4 | 1 → 0 |
| `144045` @600 | high | `N E MP 57 57<BT> BTU BOB DE KG8V K` | `N TEMP 57 57<BT> BTU BOB DE KG8V K` | **22 → 23** of 23 | 0 → 0 | 0 → 0 |
| `221502` @492 | low | `ED OF ITS OWN HEE BK BK WHATBUGAEI EUI E■ ■` | `ED OF ITS OWN HEE BK BK WHATBUGAE EUI EINIEEI S` | 29 → 33 of 42 | 1 → 3 | 1 → 2 |
| `221530` @492 | medium | `H CHA MPIMTN` | `6 CHA MPION` | **6 → 9** of 11 | 1 → 0 | 0 → 0 |
| `221548` @598 | low | `I M IEE IEE E USINGA V BPLX Z EPS` | `I M I I USINGA V BPLX Z EPS` | 18 → 16 of 26 | 5 → 3 | 0 → 0 |
| `221548` @498 | medium | `E WRHEE MYSCOET T MASTEN` | `E WRHEE MYSCO MASTEN` | 14 → 14 of 18 | 3 → 1 | 0 → 0 |
| `221745` @502 | medium | `E IAND TMN 4MEMM TONITE . EUROWEE H IRD TOO` | `IAND TMN 4MEMM TONITE . EUROWEE H IRD TOO` | 19 → 18 of 29 | 9 → 9 | 0 → 0 |
| `221805` @601 | medium | `E 40T U THESE DAYS. TNX FERANOTHERF` | `40T U THESE DAYS. TNX FERANOTHERF` | 28 → 27 of 33 | 1 → 1 | 0 → 0 |
| `221851` @601 | low | `SES E IE E IEA E I GE EI E NAERE I BK` | `SES E IE E IEA I GE EI E NAFE I BTK ES5 I NEA EIR TU STRAY OTE` | 4 → 4 of 20 | 6 → 6 | 0 → 0 |

Every other stretch reads as at HEAD.

**The two invented letters** moved:
- **Before:** an `E` in `143951` and a `■` in `221502`.
- **After:** an `E` and an `S`, both in `221502`.
- **The silence print:** still one letter, now an `S` instead of the `■`, at the same 21.46 s, 60 ms after the station stops.

## 4. What's blocking us

- **The low-confidence 22:18:51 stretch prints more junk with set A:** `... BTK ES5 I NEA EIR TU STRAY OTE`.
  - The score cannot see it. Its stretch is low confidence, so its wrong letters are not counted, and its letters sit over real keying, so none are invented.
  - It is worth your ear on that recording.
- **The silence limit stays red at one letter,** as at HEAD. A fragment prints 60 ms into the pause after `221502`'s station stops.
- **The carrier limit stays red at seed 5195,** as at HEAD.

### Asks still outstanding

- **Unit 520, 2026-10-01:** how a mark finds its own tone beside a louder one. Partly answered by unit 524; still open before a sender stands.
- **Unit 440's item 1:** MET-COVERAGE counts wrong sure characters. Raised 2026-09-25 and waiting on the owner. No change for it sits in the tree.
- **Unit 440's item 2:** R72 is cited as HM-DEC-175. Raised 2026-09-25 and scheduled as step 8 record work under R80.
- **Unit 487, 2026-09-28:** whether the terminal shows only settled text, at the cost of seconds of lag.
  - The run path, the only path to the screen, already shows only settled text.
  - The ask stands only for the timing-only path.
  - No change for it sits in the tree.
- **Unit 540, 2026-10-04:** does amber count as a positive? Waiting on the owner. The scan counts green; the change is one line in `CwCatchEar`.

Unit 539's ask, set A or set B, is answered by the owner's *ship it* (HM-DEC-245) and is dropped.
