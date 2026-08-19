PROJECT: Hamlet
ISSUED: 2026-08-19

## Asks still outstanding (inbound, per HM-DEC-139)

| Ask | First made | Waiting on |
|---|---|---|
| **Whether an attended automatic cycle may reach an antenna** (§0.2, HM-DEC-098) | 2026-08-17 | The bench evening |
| **A callsign too long for one keyer send** (HM-DEC-130) | 2026-08-18 | The seam measured at the bench |
| **Whether the star asks for a name at the moment of saving** (HM-DEC-060, HM-DEC-134) | 2026-08-18 | Nothing but the ruling |
| **Whether Hamlet may ever ask the radio to send its spectrum** (HM-DEC-062, HM-OPEN-042) | 2026-08-18 | The ruling |
| **Whether HM-DEC-097 is satisfied by existing silence** (HM-OPEN-052) | 2026-08-19 | The ruling. Nothing invented at any level; a floor would be 19.8 |

**HM-OPEN-051 is ruled C and is phase 4.** Dropped from the queue.

---

# Work order — the leading edge, which is the weak half, and the verdict that expires mid-message

**Five phases. Phase 5 is the one to drop.**

Gate first (HM-DEC-099). Write `PROJECT_STATUS.md` now, at every phase boundary,
and at the finish.

## Read this before scoping anything

**The leading edge reads 13 of 43 on the ARRL bulletin. The settled pass reads
33.** The text the operator watches arrive at the radio is a third as accurate as
the record kept afterwards, and every report this week asserted the opposite on the
strength of synthesized fixtures.

Phases 1 to 3 are aimed at the number 13. **Phase 4 is the one exception where the
settled pass may be touched**, and only for what Tim ruled.

---

## Phase 1 — The streaming path's own surviving fragments

The settled pass halved its de-glitch window and 25-to-50 ms fragments became dits.
**`CwGate.FollowSpeed` does not have that bug and does have the same exposure**:
`ShortestVote` is 5, sized in hops rather than milliseconds, so it removes about
twenty-five milliseconds while the split fragments on that recording run to fifty.

- **Dump the streaming path's mark widths in ten-millisecond buckets** on the ARRL
  bulletin capture, exactly as the settled pass was dumped — which showed 1,075
  marks between 20 and 50 ms against a dit of 60 and a dah of 160. **Publish the
  streaming path's table before changing anything.**
- **Then size the vote window against the measured dit rather than a constant.**
  What fraction is right is a decoder question with a measurable answer, not a
  ruling — the settled pass uses 0.4·dit and the reference uses 20 ms then 0.4·dit.
  Try it, measure it, report the number chosen and why.
- **Report leading-edge accuracy before and after**, aligned by longest common
  subsequence. The greedy in-order walk is wrong on a decode that starts
  mid-acquisition and must not come back.

## Phase 2 — Sweep the rest of the streaming path for the same class of error

Both faults found so far are the same mistake in different clothes: **a threshold
that does not mean what its name says.** One halved its window; one is measured in
the wrong unit.

Find every threshold, window, vote count or run-length test between the audio and
the emitted character in the streaming path. For each, state in one line what it is
*meant* to remove and what it *does* remove, with the arithmetic. **Anything where
those differ is a finding whether or not it is repaired here.**

## Phase 3 — One recording is not a measurement

Every number so far comes from the ARRL bulletin capture. **Measure the leading
edge on `cw-2026-08-17-013347` as well**, before and after phases 1 and 2, aligned
the same way.

If the two captures disagree about whether the change helps, **say so and do not
average them.** Two real recordings pointing opposite ways is a finding and the
point at which this stops being a session's call.

Report both passes on both captures in one table. It has never existed and every
future decode order will want it.

## Phase 4 — HM-DEC-143, which Tim ruled on 2026-08-19

Write this to `DECISIONS.md` at the head, verbatim. Next free id is **143**.

```
---
id: HM-DEC-143
date: 2026-08-19
refs: src/Hamlet.RadioEngine/Cw/CwDecoder.cs, src/Hamlet.RadioEngine/Cw/CwToneTracker.cs, HM-OPEN-051, HM-DEC-095, §0.0
---

**The settled pass judges for itself whether somebody is keying, from the marks it
has already extracted, rather than asking the tone survey.** Closes HM-OPEN-051.
HM-DEC-095's guard is not weakened and its carrier case is the condition on this
shipping at all.

THE VERDICT WAS ANSWERING A DIFFERENT QUESTION AT THE WRONG CADENCE.
`KeyingRecently` is a six-survey counter over half-second surveys, so it goes false
three seconds after the survey last saw keying, and the survey needs enough marks
inside three seconds to see two clusters. `exchange-easy` is twenty-seven
characters across thirty-two seconds. **The protection expired while the station
was still sending**, and everything the pass read afterwards was discarded — with
0.7 s of trailing silence, identical to a fixture that stayed protected, so the
ending is not the cause. **A slow sender leaving big gaps is exactly who a newcomer
works, and the end of a message is where the callsign is.**

THE PASS THAT READS THE MARKS IS THE ONE THAT KNOWS. The survey infers structure
from a window of raw energy, which is why it looks for two clusters and why a
sparse sender defeats it. The settled pass has already extracted the runs: it holds
the structure directly rather than guessing at it from outside. A carrier has
energy and no structure, and the pass can see that more clearly than the stage
currently being asked.

LENGTHENING THE PROTECTION WAS REJECTED. It trades against HM-DEC-095's guard by
exactly the amount added, and that guard exists because a carrier produced two
hundred characters of confident nonsense. Buying a slow sender's callsign by
re-opening that door is paying in the same currency §0.0 is trying to protect.

EXEMPTING THE FINAL DRAIN WAS REJECTED because it repairs the end of a recording
and leaves a live contact stopping mid-exchange, which is the case that matters at
the radio.

AND IT DOES NOT SHIP UNPROVED. This is the option nobody had measured. **The
carrier case must still produce silence**, demonstrated on the audio that produced
the two hundred characters, before any of it lands. If it cannot, none of this
ships and the finding comes back.
```

Then build it, and **the carrier case is a gate rather than a test**: if a carrier
produces characters, revert and report. Index row at the true head of `CLAUDE.md`
§1.

## Phase 5 — What the operator sees when the two passes disagree (DROP IF SHORT)

The leading edge reads `O T ■T  T ■T ■ O   ■  ISE SSRG E ■` where the settled pass
reads `N L D O T NET ■E ECH STATION HANDNG AHIS MESAGE P`. **He is watching the
first and keeping the second**, and nothing tells him the second is three times
better.

Establish what the terminal currently shows and whether he can tell them apart at
all. If the answer is a design decision about what the display asserts, **hand it
back rather than choosing.**

**Drop this whole if short and say you dropped it.**

## Named and left (§12.6)

- **The settled pass in every other respect**, including the gap-fit seeding.
- HM-OPEN-052, with Tim.
- The four older asks. **No transmit work toward auto-CQ.**
- **No records work** beyond HM-DEC-143 and entries these phases produce.

## Reporting

`OUTPUT.md`, four sections (HM-DEC-106), section four carrying the asks queue.

**Section two opens with one number: the leading edge on the ARRL bulletin, before
and after.** It was 13 of 43. Everything else is context for whether that moved.

**If you finish every phase, stop and report.**
