**PROJECT: Hamlet**

# Work order: finish the clock, then measure the floor

Five phases. Reported per §12.2: four sections, **written to `OUTPUT.md` at the
repository root, overwriting it**, and printed to the session as well. **Name
the branch in section 1** (§9.5.1 — it is `main`, and nowhere else).

**Read first:** `CLAUDE.md` (§0.0, §12), `SESSION_PROTOCOL.md`, the previous
`OUTPUT.md`, `OPEN_ISSUES.md`, `DECISIONS.md`.

**New rulings this order carries: HM-DEC-116, 117, 118.** The previous session's
work — gap classes from the gaps, the floor at 17, the strong-signal bar, the
answer key, the amber placeholder, the revision denominator — is done and is not
repeated here.

## Standing instruction

A phase needing a ruling records the question in `OUTPUT.md` section 4 and
continues. §12.1 unchanged. **No transmit work of any kind.**

---

## Phase 1 — the clock fit, all three parts in one change

**HM-DEC-112 is right and half of it cannot ship. Tim ruled: ship all three.**

The last session measured every partial variant and each one is worse than
doing nothing:

| Variant | `ACleanSignalDecodesExactly(25)` | Speed at 25 | Suite |
|---|---|---|---|
| As committed | `CQ D■ W1AW K` | 24 wpm | 9 failing |
| Mark at half amplitude only | **exact** | **29 wpm** | 10 |
| Mark and gap both corrected | **exact** | 25 | **23** |

Three parts, together:

1. **The mark is taken at half amplitude**, 6 dB below the local mark level.
2. **The gap is given back exactly what the mark sheds.** A mark and the silence
   after it are complementary; the detector's fall time was taken out of one
   without being returned to the other, which is where 29 wpm came from.
3. **`ClassifyMark` fits the dit/dah boundary between the two measured mark
   clusters** rather than splitting at two dits. This is the reason correcting
   both takes the suite to twenty-three: the dit moved under a multiple that
   never moved with it.

**Part 3 is not scope creep. It is HM-DEC-115's own argument applied one level
up** — a boundary taken from a multiple instead of from the data — and phase 1
of the last session already proved the technique on gaps, including the two
findings that made it work: fit **per signal, not per window**, and seed on
**percentiles, not on the extremes**. Marks want the same treatment.

**Shipping the mark correction alone was rejected**: a speed readout 16 percent
high is its own §0.0 problem, because the speed is on screen and a beginner uses
it to decide whether he could have copied something. Widening the bandwidth was
rejected by HM-DEC-112 itself.

Acceptance: `ACleanSignalDecodesExactly(25)` exact, 25 reads 25, 18 reads
17–18, `fast-easy` unchanged in text with its speed correct, and the suite back
to or below where it started.

## Phase 2 — the bulletin's answer key

`cw-2026-08-18-004507` is the first fixture in this project with an answer key
somebody knows rather than measured. It stands at **36 characters against 47**
and reads

```
JJ AOT NET ■I ECH STAAION HAND■ AHIS MESAGE P
```

against `AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAGE P`.

**The spaces are already right** — `NET`, `ECH`, `STAAION`, `AHIS`, `MESAGE`,
`P` are all correctly divided, which is phase 1 of the last session working. What
is left is character-level and belongs to the clock, so **re-measure it
immediately after phase 1 and report the number before doing anything else to
it.**

If phase 1 does not close it, report how far it moved and what is left, with the
misread characters named. **Do not tune anything to this one recording** — a
decoder fitted to a single capture is a decoder that has learned one station.

## Phase 3 — measure the refusal floor, then report (HM-DEC-117)

**The floor stays in force. The number is measured, not translated again.**

Seventeen was expected to bite at HM-DEC-097's nought decibel line. It bites at
about five: untouched to 6 dB, then 0.94 correct at 5, 0.61 at 4, 0.11 at 3,
silence at 2 and below. **Four decibels of reach were given up and the
arithmetic that gave them up was Claude's, not this project's.**

The property the ruling wanted does hold — the worst invented share across the
whole sweep is zero.

Sweep candidate floors — 17, 16, 15, 14, 13, 12, 11, 10 — and for each report
**where the invented share first rises above zero** and what correct share
survives at 3, 4 and 5 dB. One table.

**Report it and stop. Do not choose the floor.** §12.1 puts a number that
decides what the display asserts with Tim without exception, and this one has
already been guessed wrong once.

## Phase 4 — the streaming tip adopts the settled classes (HM-DEC-116)

HM-DEC-115 is **amended for the streaming path, not withdrawn.** Applying it
there directly was measured and broke `NothingIsInventedAtTheHandover` and
tone-finding on a two-station recording, because fitting three classes needs a
history long enough to hold word gaps, and a rolling window cannot tell one
sender from two.

The last session's own diagnosis is the fix: **the classes have to belong to a
sender before they are useful.**

- The settled pass already fits gap classes per signal. **Hand those classes
  forward to the streaming pass** for the current sender.
- **Reset on a tracker switch or a lost clock** — both already mean somebody
  else started transmitting (HM-DEC-116, and the annotations already exist).
- **Before the first fit, the tip uses dit multiples**, then adopts the fitted
  classes when they arrive. Text firming up under the reader is what a
  provisional tip is for, and it is already marked as provisional.
- Do not lengthen the estimator's twenty-gap window. That was measured, fixes
  three tests and breaks four, two of them prime-directive.

Acceptance: `NothingIsInventedAtTheHandover` and the two-station tone test both
still pass, and the two passes stop disagreeing about where the words are on a
Farnsworth signal.

## Phase 5 — DROP THIS ONE IF SHORT OF ROOM

**The startup spot refresh waits for the radio** (HM-DEC-118). `ReloadSpotsAsync("startup")`
runs from the view model's constructor and the radio is not connected until the
window's `Opened` event, so RBN is filtered and the skimmer watch scoped to the
remembered band. An empty panel asserts nothing; a wrong-band panel asserts
something false. Asking the radio from the constructor stays rejected — it would
put a serial read on the path that builds the window.

Then, if room remains: **rebuild the six short fixtures with enough run-up for
the detector.** The session before last proved all six decode `CQ DE W1AW K`
exactly when given three seconds of lead-in. It is generator work and it clears
most of the pre-existing eight failures along with three of the four new bar
failures, which lose only their opening characters to acquisition.

If dropped, say which.

---

**If every phase completes, stop and report. Do not start any other work unit,
and build nothing toward auto-CQ.**

## Definition of done

`ACleanSignalDecodesExactly(25)` passes with a correct speed readout. The
bulletin's distance from its answer key is a reported number. The floor sweep is
a table awaiting Tim's ruling. The two passes agree about where the words are.

Still outstanding and not in this order: `cw-2026-08-18-003758` is not on the
machine (HM-OPEN-026) and would be the suite's only regression test for a
success; `prosigns-easy` reads `IR` where `AR` was sent, the only strangers case
the bar catches for the right reason; and the 400 Hz tracker will not hold a
pitch it finds.

**Everything here is provable on the development computer against fixtures, and
none of it is evidence about the radio** (HM-DEC-093).
