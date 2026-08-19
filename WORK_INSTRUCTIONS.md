PROJECT: Hamlet
ISSUED: 2026-08-18

## Asks still outstanding (inbound, per HM-DEC-139)

Carried verbatim from the last report. Four, unchanged, none ruled since.

| Ask | First made | Waiting on |
|---|---|---|
| **Whether an attended automatic cycle may reach an antenna** (§0.2, HM-DEC-098) | 2026-08-17 | Every interlock watched to fire into the dummy load, per `BENCH_CARD.md` |
| **A callsign too long for one keyer send** (HM-DEC-130) | 2026-08-18 | Five minutes at the bench measuring the gap between two sends into the load |
| **Whether the star asks for a name at the moment of saving** (HM-DEC-060, HM-DEC-134) | 2026-08-18 | Nothing but the ruling |
| **Whether Hamlet may ever ask the radio to send its spectrum, and if so when** (HM-DEC-062, HM-DEC-092, HM-OPEN-042) | 2026-08-18 | The ruling. Three ways were put |

---

# Work order — the queue's boundary, and the hole at HM-DEC-096 to 133

**Two phases**, fewer than §12.3's five or six on purpose: phase 1 is a ruling to
transcribe, and phase 2 is an investigation whose repair depends on what the
history holds. Pre-scoping a repair nobody has seen yet is how the last week went
wrong more than once.

Gate first (HM-DEC-099): verify `PROJECT: Hamlet` against `PROJECT_CARD.md` and
against the prompt you were pasted. Any disagreement, stop.

**On the date.** `ISSUED` is set from this machine's clock as it last reported,
2026-08-18. If the clock has since rolled past midnight, this order is current and
not stale — compare against the clock, not against the last report's text
(HM-DEC-135).

**Write `PROJECT_STATUS.md` now, before reading further** (§13.2, §13.3.1,
HM-DEC-137), then at the phase boundary, then at the finish.

---

## Phase 1 — Record the queue's boundary

The last session drew a boundary and handed it back: the queue lists questions
handed back for a ruling in a session report, and not every unruled question Tim
owns. **He ruled the boundary as drawn.** Nothing changes in the tree; it is
recorded so that the next session does not redraw it.

**Write this to `DECISIONS.md` at the head**, verbatim. Next free id is 140.

```
---
id: HM-DEC-140
date: 2026-08-19
refs: CLAUDE.md §12.2, §9.6, HM-DEC-139, HM-OPEN-007
---

**The outstanding-asks queue lists questions handed back for a ruling in a session
report, and nothing else.** Amends nothing; it settles the boundary HM-DEC-139 left
open on its first use.

AN ASK IN A REPORT HAS NO OTHER HOME. That is the whole of it. `OUTPUT.md` is
overwritten by the next session, so a question raised there and not answered that
evening ceases to exist — which is the failure HM-DEC-139 was written for, and it
is specific to that channel. An entry in `OPEN_ISSUES.md` already has an id, an
owner, a status and a date, and is swept every time the file is opened. It is not
invisible and does not need a second list to keep it alive.

AND THE QUEUE HAS TO STAY SHORT ENOUGH TO BE READ. `OPEN_ISSUES.md` holds twenty-odd
items owned by Tim, most of them wanting a capture file, a manual page or a station
fact rather than a judgment. Folding those in makes a list of ten in which the four
real questions are harder to find than they were before. A queue nobody reads is
the same failure by a longer route.

EVERY UNRULED QUESTION TIM OWNS WAS REJECTED for that reason. Splitting the queue by
what unblocks each item was also rejected, and it is the better shape if the queue
ever grows: two of today's four wait on an evening at the dummy load rather than on
Tim, and reading them as four things he is behind on is wrong. At four items a
second heading is machinery for its own sake. **If the queue reaches a length where
the distinction stops being obvious at a glance, this is the first thing to
revisit.**

WHAT WOULD REOPEN IT. This rests on `OPEN_ISSUES.md` being genuinely swept rather
than nominally so. HM-OPEN-007's two favorites questions have sat unruled since
2026-08-14, and one of them reached Tim only because a session handed it back in a
report five days later. If that turns out to be the rule rather than the exception,
the premise here is false and the boundary moves.
```

Index row at the **true head** of `CLAUDE.md` §1. The head currently reads 139,
135, 138, 137, 134; **leave the out-of-order pair alone**, it is HM-OPEN-036's own
specimen.

Amend `CLAUDE.md` §12.2 with one sentence stating the boundary, so a session
reading only that file does not have to infer it.

Commit: `docs(docs): record HM-DEC-140, the queue's boundary`

## Phase 2 — Find out what happened to HM-DEC-096 to 133

**`DECISIONS.md` holds 001 to 095, then 134, 135, 137, 138, 139 and now 140.**
Thirty-eight rulings between them exist only as one-line index rows in `CLAUDE.md`
§1. Two of the four asks in the queue above cite rulings in that range. Every
session for days has been reasoning from summaries, including the ones that got
this bug wrong.

**Establish the fact before proposing anything.**

`git log -p -- DECISIONS.md`. The question is binary and the history answers it:

- **Were the entries written and later lost?** Then they are recoverable verbatim
  and the repair is mechanical. Find the commit that lost them, say what it was
  doing, and say whether anything else went with them.
- **Were they never written?** Then thirty-eight sessions added an index row
  without the entry it points at, and that is a process failure rather than a file
  failure. Say when it started and whether anything in `CLAUDE.md` or
  `SESSION_PROTOCOL.md` ever required the entry as well as the row.

**Report before repairing.** If they are recoverable, restore them in the same
commit only if the restoration is genuinely mechanical — same text, right order,
nothing reconstructed. **The moment you would be writing a ruling's reasoning
rather than recovering it, stop.** A ruling reconstructed from its own one-line
summary is a session claiming Tim's authority for words he never said, which §2.1
forbids absolutely and which is worse than the hole.

If they were never written, propose the repair and hand it back. Do not begin
drafting thirty-eight entries.

**Check the index rows against the history while you are there.** If a row's date
or id conflicts with what the commits show, say so; do not correct it in passing.

Commit as the finding warrants.

## Named and left (§12.6)

- HM-OPEN-036, §1's head ordering, whenever it is opened deliberately.
- The record sweep for rulings resting on a write outcome (Tim ruled B).
- HM-OPEN-042's remaining rungs.
- Mode follow, favorites, the recent list.

## Reporting

`OUTPUT.md`, four sections (HM-DEC-106), section four carrying the standing
`Asks still outstanding` heading (HM-DEC-139) — the four above, plus anything
phase 2 raises, minus anything ruled.

**Section one leads with phase 2's finding**: written and lost, or never written,
and the evidence either way.

**Stop and report. Do not start anything else.**
