# 1. What Claude did

**STATE.** Development computer, `C:\Source\HamLet`, Claude Code surface (§9.5).
**Branch: `main`.** Prompt, `WORK_INSTRUCTIONS.md` and `PROJECT_CARD.md` all say
`PROJECT: Hamlet`; gate passed on all three (§9.6). **No radio was connected**
(HM-DEC-093). Status written at the start and at every phase boundary.

**Phases 1 to 4 worked, phase 5 dropped. Two commits, and no decoder behaviour
changed** — both experiments were measured and reverted, and the one ruling did not
meet its own gate.

## Phases 1 to 3 — the streaming path, and the constant that doubles it

**The mark table, published before anything was touched.** ARRL bulletin capture,
ten-millisecond buckets, dit 60 ms and dah 160:

| ms | 0 | **10** | 20 | 30 | 40 | 50 | **60** | 70–90 | 120–130 | **160** | 170–270 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| marks | 2 | **21** | 13 | 6 | 1 | 3 | **60** | 5 | 3 | **35** | 8 |

**43 of 157 marks are under fifty milliseconds — twenty-seven per cent**, against
seven per cent in the settled pass. The callsign capture is 20 of 72 with a dit of
100.

**The floor is what binds, not the fraction.** `CwGate.FollowSpeed` wants
`round(ditHops / 3)`, which is two hops at twenty words a minute, so it clamps up to
`ShortestVote` every time — **the constant is the setting and the fraction never
decides anything at ordinary speeds.**

**Swept:**

| `ShortestVote` | bulletin, leading edge | bulletin, settled |
|---|---|---|
| **5 — today** | **13 of 43** | 33 |
| 6 | 27 | 32 |
| **7** | **27** | **34** |
| 9 | 27 | 32 |

Seven is the best of both and removes runs under forty milliseconds, two thirds of
a dit at that speed. I added a cap at the dit itself so a fast fist keeps its narrow
window.

**And it breaks five green tests, all synthesized**, two of them about finding the
station at all: `AStationElsewhereIsStillFound` at 400 Hz,
`TheTrackerDoesNotLeaveAStationForItsOwnImage`, `prosigns-easy` tone finding,
`coverage-easy` read whole, and the settled pass's callsign ratchet.

**Reverted.** HM-DEC-091 says a real capture outranks a synthetic one; §12.5 says a
fixture the reference reads well is evidence. Both apply and they point opposite
ways, so it is in section four.

**Phase 3's table now exists**, both passes on both captures, which it never had:

| capture | pass | in order | emitted |
|---|---|---|---|
| bulletin | leading edge | 13 of 43 | 19 |
| bulletin | settled | 33 of 43 | 37 |
| `cw-2026-08-17-013347` | leading edge | callsign present | 8 |
| `cw-2026-08-17-013347` | settled | callsign present | 10 |

The second capture has no answer key — HM-DEC-091 forbids inventing one — so it is
scored on the callsign, the one thing independently confirmed about it.

## Phase 2 — the class of error, swept

Both faults found this week are the same mistake in different clothes: **a threshold
that does not mean what its name says.**

| Threshold | Meant to remove | Actually removes |
|---|---|---|
| `CwSettledPass.Deglitch(0.020)` | 20 ms | **10 ms** — a median filter removes half its window. Fixed this morning |
| `CwSettledPass.Deglitch(0.4·dit)` | 24 ms at 20 wpm | **10 ms**, same cause, same fix |
| `CwGate.ShortestVote = 5` | a floor under the vote window | **the whole setting** — the fraction of a dit is always smaller, so the clamp decides |
| `CwGate.VoteShareOfDit = 1/3` | a third of a dit | **nothing at ordinary speeds**, for the same reason |
| `KeyingRecently` — six surveys | "keying was found recently" | **keying was found within 3 s of the survey's last sighting**, which expires mid-message on a sparse fist |

## Phase 4 — HM-DEC-143 is recorded and unbuilt, by its own condition

Written verbatim to `DECISIONS.md`, index row at the true head of §1.

**It makes the carrier recording the gate**, and the gate failed:

| | `cw-2026-08-17-134712`, the carrier |
|---|---|
| today, with the survey's verdict | **0 characters** |
| verdict removed, leaving the pass's own structure tests | **33 characters** |
| plus a tightness test on the mark clusters | **33 characters** |

**The pass's existing tests are not the judgement the ruling assumed it already
had.** `FitClock` requires eight marks, two populated clusters, a legal ratio and a
dit in range — and the carrier passes all four, exactly as HM-DEC-095 predicted it
would. The strengthening that ruling's own reasoning implies, two clusters against
one smear, does not separate them either: a carrier chopped by noise sits near two
centres.

Raising a constant until the carrier goes quiet would be tuning against one
recording with every fixture unadjudicated, which is how this week's other two
trades were made and reverted. **So it does not ship**, exactly as the ruling
instructs, and HM-OPEN-054 carries the numbers and three candidate distinguishers.

## Phase 5 — dropped

# 2. What Tim should expect

**13 of 43 before, 13 of 43 after.** The leading edge on the ARRL bulletin is
exactly where it was this morning, because the change that doubles it is not in the
build — it is waiting on your ruling.

**What that means tonight:**

- **Nothing about the decoder changed today.** The transcript and the live text
  read exactly as they did this morning. Two experiments were measured and both
  were reverted.
- **The one change that matters is one line and it is measured**: `ShortestVote`
  from 5 to 7 takes the text you watch from 13 of 43 to 27 of 43 on real off-air
  audio. It costs five synthesized tests. That is the decision in section four and
  it is the largest single improvement measured this week.
- **The transcript still stops early on a slow sender**, and HM-DEC-143 was meant
  to fix it. It is recorded and unbuilt because the carrier recording still speaks
  when the guard is removed, which is the condition you set.
- **Keep the audio.** Everything above came from two captures. The carrier
  recording is now doing real work as a gate, which is exactly what a kept file is
  for.

**The suite: 2,002 tests, 3 failing**, the same three as this morning.

# 3. What we should do next

- **Rule the `ShortestVote` trade.** It is one line either way.
- If it ships, the five synthesized failures want adjudicating one at a time — a
  day's work, and the safer reading of §12.5.
- HM-OPEN-054's three candidate distinguishers, whichever you prefer, each gated on
  the carrier recording.

# 4. What's blocking us

Two rulings, and neither blocks tonight.

---
date: 2026-08-19
refs: HM-OPEN-053, HM-DEC-091, §12.5
---

**Whether `CwGate.ShortestVote` goes from 5 to 7, doubling the leading edge on real
audio and breaking five synthesized tests.**

Measured: the text he watches arrive goes from **13 of 43 to 27 of 43** on the ARRL
bulletin, and the settled pass gains one. The callsign capture keeps its callsign in
both passes.

The cost is five green tests, all synthesized: two about finding a station at all,
one tone-finding fixture, one read-whole fixture, and the settled pass's callsign
ratchet.

For it: HM-DEC-091 says a real capture outranks a synthetic one, and the gain is on
the two recordings the operator actually made.

Against it: §12.5 says a fixture the reference reads well is evidence, and two of
the five guard acquisition, which is upstream of everything.

Rejected as a session's choice: both directions. This is a trade between two kinds
of evidence the project ranks differently, on the number the whole week has been
aimed at.

---
date: 2026-08-19
refs: HM-OPEN-054, HM-DEC-143, HM-DEC-095
---

**How the settled pass should tell keying from a carrier, given that its own marks
cannot.**

HM-DEC-143 is ruled and unbuilt. Removing the survey's verdict lets the carrier
recording produce 33 characters; a tightness test on the mark clusters leaves it at
33. The pass's structure tests pass a carrier because a carrier chopped by noise
fits two centres with a legal ratio.

Three candidates, none measured:

- **The gaps rather than the marks.** Keying alternates mark and gap at related
  lengths; a chopped carrier does not.
- **Runs per second against the fitted speed.** A carrier produces far more or far
  fewer transitions than the clock implies.
- **Requiring the mark ratio near three** rather than merely inside a legal band.

Rejected as a session's choice: picking one and tuning it against the single
carrier recording, which is how two changes this week were made and reverted.

## Asks still outstanding

Seven, per HM-DEC-139 and scoped by HM-DEC-140.

| Ask | First made | Waiting on |
|---|---|---|
| **Whether an attended automatic cycle may reach an antenna** (§0.2, HM-DEC-098) | 2026-08-17 | The bench evening |
| **A callsign too long for one keyer send** (HM-DEC-130) | 2026-08-18 | The seam measured at the bench |
| **Whether the star asks for a name at the moment of saving** (HM-DEC-060, HM-DEC-134) | 2026-08-18 | Nothing but the ruling |
| **Whether Hamlet may ever ask the radio to send its spectrum** (HM-DEC-062, HM-OPEN-042) | 2026-08-18 | The ruling |
| **Whether HM-DEC-097 is satisfied by existing silence** (HM-OPEN-052) | 2026-08-19 | The ruling; a floor would be 19.8 |
| **Whether `ShortestVote` goes 5 to 7** (HM-OPEN-053) | 2026-08-19 | The ruling; 13 → 27 against five synthesized tests |
| **How the settled pass tells keying from a carrier** (HM-OPEN-054) | 2026-08-19 | The ruling; HM-DEC-143 is unbuilt until it has one |

**Dropped since it was asked**: whether the settled pass may emit with the keying
verdict expired — ruled C as HM-DEC-143, which is recorded and did not pass its own
gate.

---

## Named and left, as the order directs

The settled pass in every other respect including the gap-fit seeding; HM-OPEN-052;
the four older asks. No transmit work toward auto-CQ. **No records work** beyond
HM-DEC-143 and the two findings these phases produced. **Phase 5 dropped.**
