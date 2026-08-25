STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.

---

# Work instruction 010 — bank the evening, then fix the six things that break it

**This is the last work unit of 2026-08-24 and it is a batch. Seven tasks.**
Task 7 is the drop candidate and is dropped whole if the unit runs long. **No
other task may be shortened to reach it.**

## Why this unit exists

**The unit's number: fifty-nine characters, one unsure, at 32 WPM.**

On 7.0259 MHz between 0115 and 0135 UTC on 2026-08-25, Hamlet read a rag chew
between two FOC operators end to end — `…UR RST 599 5NN QRN QRN HIGH TONITE …
BUT UR SIG DOOING FINE <BT> WL TNX VY MUCH FER THE … CONGRATS ES YES HAVE TO
WAIT 5 MONTHS OR SO I GUESS BUT ALL GUD ES CAN … KEEP MYSELF OCCUP[IED]` — and
on `cw-2026-08-25-013303` it **beat the independent analysis chain outright**,
reading `CONGRATS ON CHECKING ALL BOXES FOR FOC` clean where the independent
chain dropped the opening. That has not happened before.

**Why it started working, measured independently on all nine captures of the
evening:**

| capture | tone error | speed error | outcome |
|---|---|---|---|
| `011552` | −5.3 Hz | +1.9 WPM | callsign read, some soup |
| `012748` | −6.2 Hz | +0.2 WPM | **2 characters** — task 4 |
| `012823` | **−49.8 Hz** | **+9.5 WPM** | pure E/I/S soup |
| `012922` | −2.3 Hz | +0.5 WPM | turning good |
| `013010` | −6.1 Hz | +0.2 WPM | full QSO readable |
| `013150` | −1.4 Hz | −0.6 WPM | good |
| `013303` | −1.4 Hz | −1.0 WPM | excellent |
| `013402` | −11.6 Hz | +1.1 WPM | excellent, 0 unsure |
| `013520` | +3.2 Hz | −1.2 WPM | excellent, 1 unsure of 59 |

**Every readable capture has the tone inside about 12 Hz and the speed inside
about 2 WPM. The single catastrophic capture is the one where both went wrong
together.** The band did not change, the operators did not change fists, and the
input level was −13.3 dBFS in every file. A clock fitted to the wrong bin is
fitting noise, so the tone work pays twice.

**What shipped that caused it, for the record:** unit 1.11.6's task 4 — the
mixdown falling back to the last pitch the survey actually measured rather than
to a bank centre. That unit's headline refinement was measured worse and
withdrawn; the part that shipped almost as a footnote is what did the work.

**Nothing else in this unit matters if this evening can be lost. Task 1 banks it
and runs alone before anything is touched.**

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim and report mismatches,
including where the work succeeded anyway. Unit 1.11.6 corrected two of its
instruction's premises as out of date and was right to; do the same.

**Known red: 33 failing of 1607 in the engine, 481 of 481 in the app**, the
failing set byte-identical to what unit 1.11.5 left. Two are the accepted cost
on `clean-12wpm` and `clean-18wpm`, which contain exact digital silence that
HM-OPEN-018 records as physically impossible. One,
`ABroadcastDoesNotAnswerTheCommandInFlight`, is flaky in both directions.
**Do not fix any of these.**

**`ARecordingWithNoStationInItSaysNothing(014854)` is green and must stay green.**

**`DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141 or 150.**
Unit 1.11.6 transcribed HM-DEC-095 and HM-DEC-127 into its report; **read that
report's section 1 for their text** rather than re-deriving them.

**`CLAUDE_CODE.md` changed from five report sections to four without moving its
version line.** Read the file's own section count.

## Rulings in force

**HM-DEC-120.** The property is that nothing is emitted on audio holding no
signal. **`Gate` stands at 1.40** in a gap running 0.840 to 1.684 — **narrow, and
this unit must not consume it.** Both empty captures emit nothing, checked and
stated at every task that touches the signal path.

**Tim's ruling on the character margin, standing since unit 1.11.3: the margin is
nought**, because nought is the point where silence explains the span exactly as
well as the letter does. **Task 3 exists because characters are reaching the
screen at `■:-93.4`, which that ruling already forbids.**

**HM-DEC-095 and HM-DEC-127, per unit 1.11.6's transcription:** a note is chosen
by how it is keyed and never by how loud it is, and a confirmed station is not
abandoned for a candidate far below it. **A transform peak is a loudness
measurement and may never choose which note to read.** Nothing in this unit
changes which candidate the survey admits.

**Rejected already, do not revisit:** locking to the radio's `CwPitch`
(measured 10.36 against 13.94); widening the guard above 1.684; reverting the
corrected scale; quantising pitch to a bin; tuning a threshold to make a red test
green; regenerating a fixture to justify a code change in the same session.

**PROPOSAL, not ruled — §4.4.** Tasks 5 and 6 change the panel, which is Tim's
without exception under `CLAUDE.md` §0.0. **These two are drafted for his ruling
and are stated as what the field report asks for, not as decisions this session
may make.** Build them; **if any judgement beyond what is written here is
required, record it in section 4 and implement the narrowest reading.**

> **Task 5 — the operator can tune.** The band row's `20 m` button sits several
> pixels below `80/40/30 m` and the `best bet now` badge occupies the space
> above it. `40 m` could not be clicked at all and the only way to tune was via
> favourites. **The badge is to be rendered over the row rather than inside its
> layout flow, and must not swallow clicks on neighbouring buttons.**
>
> **Task 6 — one voice on screen.** The prose panel and the old advice block
> currently state simultaneously that a clear tone is present and that nothing is
> there, disagreeing about the pitch by 50 Hz, and send the operator to the radio
> to fix a decoder problem. **Where the prose panel has something to say, the
> advice block is suppressed rather than sitting beneath it.**

## Status cadence

Named here as well as in the prompt, per §4.5. After each task, before starting
the next, update `PROJECT_STATUS.md` per `CLAUDE.md` — `STATE`, `TASK: n of m`,
`BALL`, `UPDATED` read from the clock, and `NOTE` saying what is moving inside
the task. The same every ten minutes while a task runs. **Seven tasks; if one
overruns, say so in the note.**

## The tasks

### Task 1 — bank the evening, alone, before anything is touched

**The nine captures are NOT in this zip.** `cw-2026-08-25-011552` through
`-013520` with their sidecars are on the ham computer and were never delivered to
the session that wrote this instruction. **Locate them in the tree first.**

- **If they are already under `tests/fixtures/cw/captured/`**, use them.
- **If they are not, stop and report it as the first line of section 4.** Tasks
  2 through 7 may still run — none of them needs the fixtures to be committed —
  but **task 1's floors cannot be written without the audio, and the whole point
  of task 1 is that this evening must not be lost.** Say plainly that the
  evening is unbanked and what is needed to bank it.

Extend the harness to score each and write today's **character counts, unsure
counts and element counts in as floors** — `>=` what is measured now, never
equality. The suite goes green on current behaviour. **Floors only ever rise.**

- **`013520` is the reference case:** 59 characters, 1 unsure, 157 elements, 32 WPM.
- **`013303` is the case where Hamlet beat the independent chain.**
- **`012823` is the negative control:** same station, same evening, same input
  level, 39 characters of soup. **A change that improves `013520` while regressing
  `012823` has traded one failure for another and the harness must say so.**

Build and run the suite; record counts as the green baseline.

### Task 2 — the operator can tune *(see the ruling above)*

Check the badge first: a wrapper positioned over the row with a transparent hit
area and no `pointer-events: none` will swallow clicks across neighbouring
buttons while appearing to sit on one. Second candidate: a handler that no-ops
when the target band equals the current band, with the current band computed from
stale state — the dial was already on 7.028, which is 40 m, when the clicks
failed.

**Report which it was.** This is second because an operator who cannot tune
cannot test anything else in this unit.

### Task 3 — the confidence score, which is computed and not used

**Establish first whether the ruled margin is wired to the emit decision at
all.** Characters are reaching the screen at `■:-93.4`, `■:-44.4`, `■:-39.6`.
Tim ruled the margin at nought in unit 1.11.3, and a character scoring −93 should
never have passed it. **If the gate simply is not running, say so — that is a
smaller fix than a redesign, and the redesign below may not be needed in the
form written.**

Then: **a flat threshold is the wrong shape.** Median span LLR by element count,
measured across the evening:

```
1 element (E, T)     40
2 elements          225
3 elements          254
4 elements          446
5 elements (digits) 812
```

An E is one dit and can never accumulate the evidence a 7 does, so a flat gate
structurally punishes the shortest characters — at LLR ≥ 60, `NICE` becomes `NIC`
and `MEET` becomes `MT`. **Normalise, then gate.** At a per-element threshold of
25 on `013010`, 24 of 146 characters drop, the leading E-run disappears, and
`NICE`, `MEET` and `CALL ES` all survive.

**Measure two normalisations and report both**, because element count is not
obviously the right divisor: **per element**, and **per unit of keying time** —
a dah spans three dits and carries three times the evidence, so E and T are not
comparable at one element each, and the 1-element median of 40 may be hiding
that. **Ship whichever measures better on the evening's nine captures and say
which, with the numbers.**

**A suppressed character becomes `■` or nothing, never a letter.** §0.0 ranks a
marked unknown above a wrong letter.

Re-run the corpus and both empty captures. **Task 1's floors must hold**, and
`012823` must not get worse.

### Task 4 — the capture that gates off with everything right

`cw-2026-08-25-012748`. Tone within 6 Hz. Speed within 0.2 WPM. `tonePeak` 67.5,
**the highest of the entire evening.** It emitted **2 characters from 6 elements**
in thirty seconds, while independent measurement finds **113 marks at a clean 2.85
dit:dah ratio in the same file.** The capture immediately before reported
`sinceLast 531 characters, 1046 elements`, so the decoder was working a moment
earlier and a moment later.

**Everything upstream is right and something downstream refuses it. Diagnose it;
do not tune past it.** Report where the elements are lost, with file and line.

**If the cause is found and the fix is larger than this task, report the cause and
stop** — a named mechanism with a known-good neighbour either side is worth more
than a rushed change.

### Task 5 — one voice on screen *(see the ruling above)*

### Task 6 — the speed grid is about to run out

`013402` measures **30.9 WPM** and `013520` measures **30.8**. Hamlet reported 32
for both — **the top of its 8-to-32 search.** These are First Class CW Operators'
Club members and they run fast; one more notch and the grid cannot follow, and the
failure will look like a decoder fault rather than a range limit.

Raise the ceiling. **`CwProbabilisticDecoder`'s own remarks argue for forty while
the constant reads 32** — this is HM-OPEN-058, logged on 2026-08-23 and parked in
every unit since. **Make the sidecar say when the winning speed sits at either end
of the range**, so a range limit is never again mistaken for a measurement.

Re-run the corpus and the evening's floors.

### Task 7 — the keying sweep *(the drop candidate)*

Tally across every capture analysed 2026-08-22 to 2026-08-25: **agreed with an
independent measurement 6, contradicted it 11.**

- `013150`: `no keying at 500 Hz, 5 ms key down, 106 key-downs` — on a capture
  containing `CQ CQ CQ DE ND4K ND4K ND4K K`.
- `013010`: `no keying at 500 Hz, 7 ms key down, 80 key-downs` — on a capture
  containing a complete readable QSO sign-off.
- `012823`: `no keying at 500 Hz` — **and 500 Hz was correct**, while the
  decoder's own `toneHz` said 450. **The sweep found the right frequency and
  reported no keying; the decoder took the wrong frequency and produced soup.**

Two structural faults: **the `N ms key down` verdicts are arithmetically noise**
— 106 × 5 ms is 0.53 s of key-down in a six-second window against a real duty
near 40 %, and a 6 ms median cannot coexist with 30 WPM Morse whose shortest
element is 40 ms — and **the 25 Hz grid straddles tones**, the sweep repeatedly
choosing the worse of two adjacent bins.

Fix it against the nine fixtures banked in task 1. **The sweep's independence is
the right design and is not to be abandoned** — `012823` shows it finding the
right frequency when the decoder did not, which is the case for cross-checking
the two rather than retiring either.

**This is the drop candidate. Dropped whole, and the report says it was dropped.**
Task 5 removes the on-screen harm in the meantime by suppressing the block when
the prose panel speaks.

## Parked — do not touch, do not raise

- **The tracker's rules for choosing between admitted candidates** (HM-DEC-095,
  HM-DEC-127).
- **Task 2 of unit 1.11.6's withdrawn refinement**, left in the tree with its
  measurement — two pitch measurements disagree by six hertz depending on the
  window. Real, unruled, not this unit's.
- **Refusing to decode at an unmeasured pitch**, which costs `N4L` because
  `134712`'s bank centre of 500.0 lands on a station at 500.09. Needs a ruling.
- **`014113` and `014308`** — a second mechanism, envelope smear rather than
  pitch. The integrator width bears on it and is unruled.
- **Adjudicating the seven W1AW captures** as ARLP034.
- **`Gate` at 1.40**, the guard's two-to-one gap, `001520`'s quadrillions, the
  reference decoder's boxcar, `ElementsSeen`/`ElementsResolved` being one field,
  `CwUnitEstimator.Runs`, HM-OPEN-057, HM-OPEN-059.

A parked item that blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **Do not shorten tasks 1 through 6 to reach task 7.** Task 7 is the drop.
- **Do not lower a floor written in task 1.** Floors only rise.
- **Do not let `012823` get worse** to make `013520` better.
- **Do not change the panel beyond tasks 2 and 5**, and not beyond the narrowest
  reading of the rulings above.
- **Do not consume the guard's gap** — 0.840 to 1.684, `Gate` at 1.40. If any task
  moves those window ratios, report the new edges rather than adjusting the guard.
- **Do not trade the silence property**, and do not let
  `ARecordingWithNoStationInItSaysNothing(014854)` go red.

## Committing, pushing, reporting

Commit and push each task before starting the next. The report names the branch
and states whether each push succeeded; a refused push is reported as refused,
with the reason.

Report per `CLAUDE_CODE.md` §8 — **read the file's own section count rather than
trusting its version line** — to `output.md` at the repository root, overwritten
and printed.

**Section 3 leads with the evening's nine captures scored against their banked
floors, before and after this unit**, so that a batch of seven tasks cannot hide
a regression inside an average. **Section 2 says plainly whether the operator can
click the 40 m button.**

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Eleven inbound, none
ruled, the oldest open since 2026-08-14. Eleven consecutive units have now worked
beside rulings they cannot read.**

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
5. **The tone tracker** — *narrowed by unit 1.11.6's hold, and the evening's table
   shows the hold is what made the decoder work. Not closed.*
6. **Whether the integrator ships at 45 Hz or 30 Hz** — *bears on `014113` and
   `014308`.*
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named and the operator is not told
   they are not named.**
10. **The keying witness is wrong more often than right** — *6 agreed, 11
    contradicted. Task 7 if it survives; task 5 removes the on-screen harm either
    way.*
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

Still open from earlier units: **the lock helping sometimes and hurting sometimes
with nothing telling the operator which**; **the "Hold this pitch" button added
against instruction**; **two clean fixtures dropped from 9 of 9 for containing
exact digital silence**; **`001520` scoring in the quadrillions**; **the port and
its reference differing by an integrator**; **`CLAUDE_CODE.md` changing its report
contract without moving its version line**; **refusing an unmeasured pitch costs
`N4L`**; **a second mechanism silences `014113` and `014308`**; **two pitch
measurements disagree by six hertz depending on the window**; **seven adjudicable
W1AW fixtures are unadjudicated.**

**If you finish every task, stop and report. Do not start the next unit.**
