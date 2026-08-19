PROJECT: Hamlet
ISSUED: 2026-08-19

## Asks still outstanding (inbound, per HM-DEC-139)

Carried verbatim. Five. **HM-OPEN-045 is being repaired on the chat side, not
here** — the reasoning survives in conversations only that surface can read.

| Ask | First made | Waiting on |
|---|---|---|
| **Whether an attended automatic cycle may reach an antenna** (§0.2, HM-DEC-098) | 2026-08-17 | Every interlock watched into the dummy load. **Phase 5 below exists to make that evening possible** |
| **A callsign too long for one keyer send** (HM-DEC-130) | 2026-08-18 | Five minutes at the bench measuring the seam between two sends |
| **Whether the star asks for a name at the moment of saving** (HM-DEC-060, HM-DEC-134) | 2026-08-18 | Nothing but the ruling |
| **Whether Hamlet may ever ask the radio to send its spectrum, and if so when** (HM-DEC-062, HM-DEC-092, HM-OPEN-042) | 2026-08-18 | The ruling. **This bounds phase 4** |
| **What repair the 096-to-133 hole gets, and what happens to HM-DEC-105** (HM-OPEN-045) | 2026-08-19 | The ruling. Recovery from the chat transcripts is now proven possible |

---

# Work order — clear the standing backlog

**Six phases, each independently committable. Phase 6 is the one to drop.**

Gate first (HM-DEC-099): `PROJECT: Hamlet` against `PROJECT_CARD.md` and against
the pasted prompt. Any disagreement, stop.

**Write `PROJECT_STATUS.md` now, at every phase boundary, and at the finish**
(§13.2, §13.3.1, HM-DEC-137).

Every one of these has been named-and-left across four orders. They are worked
now.

---

## Phase 1 — `CLAUDE.md` §1's head ordering (HM-OPEN-036)

The head reads 140, 139, 135, 138, 137, 134, with a 2026-08-18 row sitting above
two dated 2026-08-19. Every previous order told you to leave it because tidying in
passing is what HM-OPEN-036 exists to prevent. **This is the deliberate moment it
was waiting for.**

- Find why insertions have been landing at a fixed anchor rather than at the true
  head, and fix the cause, not just the symptom. If the anchor is a literal string
  in a delivery script or a habit in the instructions, name it.
- Reorder the head so the table is genuinely newest-first.
- **Reconcile every row against `git log`.** The last session found HM-DEC-113's row
  dated 2026-08-18 and added by `c1a76f8` on 2026-08-17 at 21:22, and reported it
  without correcting. Sweep the rest. Where a date conflicts, **report it and change
  nothing** — a row's date may be the ruling's date rather than the commit's, and
  guessing which is which is how a record gets quietly falsified.
- Leave a test if one is possible: §1's dates in descending order is checkable.

Close HM-OPEN-036 if the cause is fixed; narrow it with the date and reason if not.

## Phase 2 — The write-outcome record sweep (Tim ruled B on 2026-08-18)

Every setting write in the application reported its outcome wrongly until
HM-OPEN-042's readback fault was found: a read issued with no expected response
command completed only on `FB` or `FA`, so a successful write and a silent one were
indistinguishable. **Rulings were then reasoned from those counts.**

HM-DEC-092 is the one already named — five writes reported unanswered, at least two
in effect, read as the link dropping commands. **It is named because a session
tripped over it, not because it is the only one.**

- Sweep `DECISIONS.md`, `OPEN_ISSUES.md` and `CLAUDE.md` §1 for every ruling or item
  whose reasoning rests on an unanswered-write count, a `NoAnswer`, a write reported
  as failed, or a settings write believed not to have taken.
- For each, state what it concluded and **whether the conclusion survives** now that
  the measurement is known to have been wrong.
- **Re-rule nothing.** This is a documents pass. It tells Tim how big the re-reading
  is, which is the whole point of him ruling B over doing it at the radio.
- Note that `CLAUDE.md` §1 rows 096–133 have no entries, so for that range the
  summary is all there is. Say where that limits the sweep.

Output is a single open item listing the candidates, worst first.

## Phase 3 — What recomputes mode-follow (HM-OPEN-041)

Session `9f9d23eb`, 2026-08-18: 18 `mode_followed` events, 10 with no
`tune_requested` within three seconds, an unbroken run at 20:30:39, :50, :51, :53,
:56, :57, :59 and 20:31:02 with nothing driving it. The last session fixed the
decision — the plan now remembers the last write the radio confirmed and will not
repeat it — but recorded that **what triggers the recomputation is still unseen.**

- Find it. The plan not repeating a write is a guard; something is still calling
  `Decide` at a cadence nothing explains, and a guard in front of an unexplained
  loop is a symptom treated.
- HM-OPEN-041 names `recent_dwell_short` as the instrument: a dial that is not
  moving files no near misses. That reasoning is available at the desk now — the
  2026-08-19 telemetry has four of them in a session with two app-initiated tunes.
- Fixture: nothing changes, exactly one follow, and **nothing recomputes.**

## Phase 4 — The scope path, as far as it goes without asking (HM-OPEN-042)

**Bounded by an unruled ask.** Whether Hamlet may request the spectrum at all is in
the queue above. **Do not add any request for scope output, automatic or
otherwise.** HM-DEC-062 stands and the automatic `27 11` is correctly out of the
tree.

What is reachable regardless:

- **The reporting lie.** `scope_output_requested` logged `outcome: failed` with
  `reason: confirmed` and `unansweredCommands: 0` while 2,748 scope frames arrived.
  Two fields contradicting each other, on a write that plainly succeeded. If the
  readback repair already fixed this, prove it with a test rather than assuming.
- **Rungs three to five, built against captured frames rather than a live radio.**
  Frames received, parsed, drawn. The 2026-08-19 session carries 2,748 real scope
  frames at `scopeShare` 0.50; if any capture of them exists, it is a fixture. If
  none does, say so and build the parse and render tests against synthesized frames
  matching p. 19-14's format, and mark them as synthesized (§12.4).
- **`IsRadioBroadcasting` and the link counters belong where the operator can see
  them**, not only in a file he has to upload. That was step 4 of the order that
  fixed the tracking bug and I do not know whether it landed. Check; build it if
  not.

## Phase 5 — Make the bench evening possible (unblocks the oldest ask)

**Whether an attended automatic cycle may reach an antenna has been outstanding
since 2026-08-17**, waiting on every interlock being watched to fire into the dummy
load. That evening has not happened, and the longer it waits the more the cycle
ages untested.

- Walk `BENCH_CARD.md` against the code. **Every interlock it names must have a
  path that can be provoked deliberately at the bench**, including the link pulled
  mid-cycle. An interlock that only fires on a condition Tim cannot create is an
  interlock he cannot watch.
- Every stop reason emits its stable token and its operator sentence (HM-DEC-077).
  Cross-check the card's list against what the code can actually emit; report any
  the card names that the code cannot produce, or vice versa.
- If anything on the card requires a step Tim would have to improvise on the night,
  fix the card. It is the artifact the ask is waiting on.
- **Do not key an antenna and do not change the dummy-load-only constraint.**

## Phase 6 — The headless test flake (DROP THIS ONE IF SHORT)

An earlier report: running both suites at once, one pass reported five app failures;
two runs immediately after reported the one standing failure. The extra four were
headless window tests, which build a real window and lose races under load.

A suite that reports four false failures under load is a suite whose red count
nobody trusts, which is how the standing baseline of two became something people
read past. Stabilize them or isolate them, and make the standing baseline
unambiguous.

**Drop this whole if short, and say you dropped it.**

## Named and left (§12.6)

- HM-OPEN-045's repair. Chat side, not here.
- The star-naming question, the spectrum question, the antenna question and the
  callsign seam — all in the queue above, all unruled, none to be built around.

## Reporting

`OUTPUT.md`, four sections (HM-DEC-106), section four carrying the standing
`Asks still outstanding` heading (HM-DEC-139), scoped by HM-DEC-140.

**Section one is ordered by phase.** Section two leads with anything Tim would see
or notice on his own machine.

**If you finish every phase, stop and report. Do not start the next work unit.**
