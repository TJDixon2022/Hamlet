# 1. What Claude did

**STATE.** Development computer, `C:\Source\HamLet`, Claude Code surface (§9.5).
**Branch: `main`** (§9.5.1). Prompt, `WORK_INSTRUCTIONS.md` and `PROJECT_CARD.md`
all say `PROJECT: Hamlet`; gate passed on all three (§9.6). Status written at the
start, at every phase boundary, and at the finish.

**All six phases worked. Phase 6 was not dropped.** Six commits.

## Phase 1 — §1's head ordering (HM-OPEN-036, closed)

**The cause was two habits, not one, and both are in the history.** `5d00bd4` and
`303c4f4` inserted a row immediately *below* the top row rather than above it —
the fixed anchor that open item names. `d263f95` pasted a block of four rulings in
the order they were ruled, which is oldest-first inside a newest-first table.
Neither is a script in this repository; both are how a delivery gets composed. So
**§1's own header now states the insertion point and the batch order**, where the
next delivery will be written, and `DecisionLogOrderTests` fails on any inversion.

Six rows moved, none edited: HM-DEC-135 down to its own day, and the same-date
runs 130/129/128, 104/103/102/101 and 83/82 into descending order. The row
multiset before and after is byte-identical.

**Two findings, neither corrected, both as instructed:**

- **HM-DEC-051's row is dated 2026-08-14 and HM-DEC-050's 2026-08-15**, so dates
  and ids disagree about which is newer. A row's date may be the ruling's day
  rather than the writing's, and guessing which is wrong would falsify the record.
- **Two different rulings carry HM-DEC-088** — the decoder's noise measurement and
  the top strip becoming one row — which §2.1 forbids absolutely. **HM-OPEN-046.**
  Renumbering would silently break whichever citations point at the other one.

**And I have to report how the duplicate was found, because it was my defect.** My
first reorder rebuilt the row block from a dictionary keyed by ruling id. A
dictionary keeps the last of a duplicate key, so the decoder ruling's row was
written out of the file and six rows shifted to fill the gap. The test I was
writing in the same phase caught it within a minute, I reverted, and redid the
reorder on positions. Nothing reached a commit — but the safe version and the
destructive one looked identical, and only the test told them apart.

## Phase 2 — the write-outcome sweep (HM-OPEN-047, nothing re-ruled)

Candidates worst first, each with what it concluded and whether it survives:

- **HM-DEC-092 — materially affected, both halves.** The link counters survive and
  are worth having; the diagnosis they were reasoned from (five writes unanswered,
  two in effect, read as the link dropping commands) was the readback fault wearing
  the link's clothes, and the ruling's text still says otherwise. Its second half,
  that `27 11` may be written because attempting it and reporting the answer beats
  guessing, **does not survive**: the answer could not be read. That is the
  spectrum question already in the queue.
- **HM-DEC-084 — the rule survives and the fault vindicates it.** "An
  acknowledgement says the radio understood the frame, not that the setting moved."
  What does not survive is any record between 2026-08-15 and 2026-08-19 of which
  settings took.
- **HM-OPEN-041, HM-DEC-056, HM-DEC-107, HM-DEC-093 — checked and clear**, each
  with the reason. Mode writes never read back; the scanner aborts on an unanswered
  *read* and reads always carried their expected command.

**The limit is stated rather than glossed:** rows 096–133 have no entries, so for
thirty-eight rulings only the summary could be swept, and a summary says what was
ruled rather than what the reasoning leaned on.

## Phase 3 — what recomputes mode-follow (HM-OPEN-041, closed)

**It is the frequency changing, and the snap-back was changing it.**
`ScheduleModeFollow` has two callers: a band change, and `FrequencyHz` changing by
any route including a reading from the radio. In that evening's build a reading
older than the operator's tune dragged the display back and the next poll moved it
forward, so **the number changed twice per tune with nobody touching anything**,
and each change restarted the 600 ms settle. The one-to-eleven second gaps in the
run are what a settle timer does when restarted by a value that will not sit still.

The instrument HM-OPEN-041 named confirms it: `recent_dwell_short` fires from the
same handler, so four near misses in a session with two tunes is four frequency
changes the operator did not make. **The repair shipped yesterday as `DialGuard`**;
what was missing was anything asserting the quiet case. `ModeFollowReschedules`
counts the asks, and the fixture is forty polls of an unchanging frequency with
nothing recomputing, a stale reading with nothing recomputing, and a genuine move
with exactly one.

## Phase 4 — the scope path, bounded by the unruled ask

**Nothing asks the radio for anything.** HM-DEC-062 stands, the automatic `27 11`
is out of the tree, and the spectrum question stays in the queue.

- **The reporting lie is proved fixed, not assumed.** The sweep walks every write
  outcome and refuses to let `outcome` and `reason` disagree. **That contradiction
  was mine**: I moved the caller to the stable token two sessions ago and left the
  comparison on the enum's name.
- **Rung five had no instrument at all.** Received and parsed are counted
  (HM-DEC-093); whether a parsed sweep becomes pixels was covered by nothing, and a
  waterfall drawing none of them looks exactly like a quiet band. Four tests now:
  a sweep reaches the pixels, an empty bin stays the floor, a sweep with no bins
  draws nothing, the newest sweep is on top.
- **The frames are synthesized and say so** (§12.4). **No capture of the 2,748 real
  scope frames exists here** — `tests/fixtures` holds `cw` and nothing else — so
  these prove the path and not the radio.
- **The link counters had already reached the operator** on 2026-08-19: the check
  line under the readout, the counts on the diagnostics screen. Checked, not
  rebuilt.

## Phase 5 — making the bench evening possible

Every stop reason the card names **is** produced by the code; I checked each. The
cross-check the other way found what was missing:

- **`RoundLimit` was on neither list.** It is the only stop nothing external
  causes, and it is the one the antenna question most needs watched. The card now
  runs a short cycle to its own end.
- **Three cheap refusals added**: empty message, over-thirty-character message,
  arming before the rig facts fill in.
- **Three stops this bench cannot provoke are named as such** — heard an answer,
  heard something else, radio stuck in transmit — so their absence is not read as
  an interlock that failed.
- **HM-DEC-130's measurement could not have been taken as written.** The cycle
  refuses a long message at edit time; `KeyerCwSender` splits at the spaces on the
  manual path. The card said the split "may not be wired into the cycle" and left
  him to find out at the bench. It now sends him to the send panel with a message
  that actually splits.

## Phase 6 — the flake (HM-OPEN-024, closed)

**An Avalonia headless test runs on one process-wide dispatcher and xUnit runs
test classes in parallel.** Several tests take turns on a thing there is only one
of; under load one loses. Two classes also set `LayoutStore.Path`, a mutable
static. Parallelization is disabled for the app assembly: it costs two seconds,
and three consecutive full runs afterwards each reported the same two failures.
HM-OPEN-024's recorded suspicion (a lazy band plan) was wrong and is corrected in
its closing note. **HM-OPEN-014 is engine-side and is not covered by this.**

# 2. What Tim should expect

- **The red count is trustworthy now.** 1,981 tests, **2 failing, always the same
  two**: `ClearingTheTranscriptLeavesTheDecoderAlone` and
  `TheBulletinDecodesToItsAnswerKey`, both the standing decode baseline. Anything
  above two is real.
- **The app suite takes four seconds instead of two.** That is the price of the
  above and it is deliberate.
- **Nothing on screen changed.** No behavior changed in phases 1, 2, 4 or 6; phase
  3 added a counter; phase 5 changed a checklist.
- **`BENCH_CARD.md` is longer and the measurement step now points at the send
  panel rather than the calling cycle.** If you go to the bench with the old card
  printed, that step will not work.
- **`CLAUDE.md` §1 reads newest-first**, and one row deep in the table is still out
  of order on purpose: HM-DEC-051/050 disagree with themselves about dates.
- **Six commits, pushed to `main`.** Nothing local, no branches.
- **No radio was connected** (HM-DEC-093).

# 3. What we should do next

- The bench evening. The card can now be followed end to end, and it is the only
  thing two of the five queued asks are waiting on.
- HM-OPEN-046: choose HM-DEC-088's new number and re-point its citations. It is
  small and it gets smaller the sooner it is done.
- HM-OPEN-047 tells you the size of the write-outcome re-reading; whether any of it
  is worth an evening is yours.

# 4. What's blocking us

Nothing is blocked. Two new questions, both in the queue.

## Asks still outstanding

Six, per HM-DEC-139 and scoped by HM-DEC-140. Carried verbatim until ruled.

| Ask | First made | Waiting on | Where it already sits in the tree |
|---|---|---|---|
| **Whether an attended automatic cycle may reach an antenna** (§0.2, HM-DEC-098) | 2026-08-17 | Every interlock watched into the dummy load. **The card can now be followed end to end** | Built and armed. Dummy load only until this is ruled |
| **A callsign too long for one keyer send** (HM-DEC-130) | 2026-08-18 | The seam measured at the bench. **The card now says where**: the send panel, since the cycle refuses and the single send splits | Refused in the cycle, split by `KeyerCwSender` on the manual path |
| **Whether the star asks for a name at the moment of saving** (HM-DEC-060, HM-DEC-134) | 2026-08-18 | Nothing but the ruling | Favorites are born unnamed; the manage window renames them |
| **Whether Hamlet may ever ask the radio to send its spectrum, and if so when** (HM-DEC-062, HM-DEC-092, HM-OPEN-042) | 2026-08-18 | The ruling. Three ways were put | **Not asked at all.** Rungs one to five now have tests; the request does not exist |
| **What repair the 096-to-133 hole gets, and what happens to HM-DEC-105** (HM-OPEN-045) | 2026-08-19 | The ruling; recovery is proceeding on the chat side | `DECISIONS.md` runs 001–095 then 134 onwards |
| **What HM-DEC-088's duplicate becomes** (HM-OPEN-046) | 2026-08-19 | The new id, and who re-points the citations | Two different 2026-08-16 rulings share the id. `DecisionLogOrderTests` names 88 as the one known reuse so it cannot spread |

Nothing was dropped: no ask in the queue was ruled since the last report.

---

## Named and left, as the order directs

Not started: HM-OPEN-045's repair, which is chat-side; and the four queued
questions, none of which was built around. **HM-OPEN-014**, the engine-side
allocation ceiling that flakes under a second busy process, is named here because
phase 6 did not cover it.
