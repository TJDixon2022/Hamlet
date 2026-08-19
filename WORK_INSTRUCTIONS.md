PROJECT: Hamlet
ISSUED: 2026-08-18

# Work order — outstanding asks get a standing heading

## Asks still outstanding

Carried inbound per HM-DEC-139. Four, checked against `OPEN_ISSUES.md` and the
recent reports rather than copied forward.

| Ask | First made | Waiting on | Where it already sits in the tree |
|---|---|---|---|
| **Whether an attended automatic cycle may reach an antenna** (§0.2, HM-DEC-098) | 2026-08-17 | Every interlock watched to fire into the dummy load, per `BENCH_CARD.md`, including the link pulled mid-cycle | Built and armed: `AutoCaller`, `AutoCallAnswers`, the widget on the making-contacts preset. Dummy load only until this is ruled |
| **A callsign too long for one keyer send** (HM-DEC-130) | 2026-08-18 | Five minutes at the bench measuring the gap between two sends into the load | Refused, not split. `CwMessage.Split` exists and is unused for this |
| **Whether the star asks for a name at the moment of saving** (HM-DEC-060, HM-DEC-134) | 2026-08-18 | Nothing but the ruling; handed back under §12.1 clause 3 as a trade-off | Favorites are born unnamed from places the operator was. The manage window renames them afterwards |
| **Whether Hamlet may ever ask the radio to send its spectrum, and if so when** (HM-DEC-062, HM-DEC-092, HM-OPEN-042) | 2026-08-18 | The ruling. Three ways were put: leave the switch to the operator, ask once on a button, or ask automatically once the counters show the stream is not eating the link | **Not asked at all.** The automatic `27 11` was removed on 2026-08-18 and HM-DEC-062 restored; the reads of `27 10` and `27 11` stay |

**Dropped as ruled since they were asked**, and named here once so the queue is
not silently shorter than the record: the frequency's cadence (HM-DEC-138), the
record sweep for rulings resting on a write outcome (ruled B), and HM-OPEN-044
itself (HM-DEC-139).

---

**One phase. Nothing else is in this order.** Finish it, report, stop.

Gate first (HM-DEC-099): verify `PROJECT: Hamlet` against `PROJECT_CARD.md` in
this tree and against the prompt you were pasted. Any disagreement, stop.

**On the date, before you check it.** The last `OUTPUT.md` carries 2026-08-19,
which came from a work order's text rather than from this machine's clock — the
last session reported that discrepancy and it was mine. **This order is dated from
your clock.** For HM-DEC-135's staleness check, compare `ISSUED` against the
machine clock, not against that report's date, and do not stop on it. Say in your
report if the two still disagree.

**Write `PROJECT_STATUS.md` now, before reading further** (§13.2, §13.3.1,
HM-DEC-137), then at the finish.

---

## What Tim ruled

HM-OPEN-044 asked how a shipped change waiting on a ruling gets back in front of
him. Three options were put; **he ruled the standing heading, and ruled that it
happens now.**

The marker-at-the-site option is not rejected on the merits and is wanted when the
record is healthier. It is not this order.

## The one phase

**1. Write this to `DECISIONS.md` at the head**, verbatim. Next free id is 139;
136 is deliberately absent and is not to be filled.

```
---
id: HM-DEC-139
date: 2026-08-19
refs: CLAUDE.md §12.2, HM-OPEN-044, HM-DEC-106, HM-DEC-137, HM-DEC-099, HM-DEC-138
---

**Every session report carries a heading for asks still outstanding, and every
work order carries the same list inbound. A report or an order without it is
defective and is redone.** Closes HM-OPEN-044. Supersedes nothing; HM-DEC-106's
four sections are unchanged and this is a standing heading within the fourth.

AN ASK NOBODY ANSWERED LOOKS EXACTLY LIKE AN ASK NOBODY MADE. `099de5a` changed
the frequency's cadence and asked for the ruling in its own section four. The ask
was correct, complete and properly placed. Three sessions then inherited the
change as settled, and one of them — mine — withdrew a draft of that same ruling
while the code was already in the tree. §9.5 says a decision not in the record is
not made, and nothing in the project compared the tree against the record. What
made it invisible was not carelessness. It was that a section four is read once,
by one person, on one evening, and then the conversation moves.

SO THE QUEUE CARRIES ITSELF FORWARD RATHER THAN BEING REMEMBERED. The heading
lists every ask still outstanding, each with the date it was first made, what it
is waiting on, and where the change it concerns already sits in the tree. It is
carried forward verbatim by every report until Tim rules, and dropped in the
report that records the ruling. **The heading appears even when the queue is
empty and says so**, because an absent heading and an empty queue are the same
sight, and this project has now twice been caught by a silence that looked like
a state.

AND THE WORK ORDER CARRIES IT INBOUND TOO, for HM-DEC-137's reason and no other.
A rule that lives in one channel fails when that channel is written in a hurry,
and both of this project's channels have now failed in the field. One of the two
will catch.

IT IS DEFECTIVE RATHER THAN AN OVERSIGHT, which is HM-DEC-099's shape and
HM-DEC-137's. The failure is one a session cannot detect from inside: a report
that omits the heading looks complete, and the ask simply stops existing. Holding
the artifact to it is the only thing that makes the requirement real rather than
advisory.

THE MARKER AT THE SITE WAS NOT REJECTED, only deferred. A marked assumption that
a sweep test ages out is the stronger answer, because it survives a session
forgetting and this one does not. It needs a marker convention and a test, and
the queue needed to be visible tonight. When the record is healthier it is worth
taking up.

REFUSING TO SHIP WITHOUT THE RULING WAS REJECTED AND THE COST IS MEASURED, NOT
FEARED: `099de5a` fixed the display the operator uses more than any other, and
holding it at the door would have cost him two more evenings of a radio that did
not track. A queue that is visible is worth more than a gate that is closed.
```

Add the index row at the **true head** of `CLAUDE.md` §1. The head currently reads
135, 138, 137, 134, with a 2026-08-18 row above two dated 2026-08-19; **leave that
alone.** It is HM-OPEN-036's own specimen and tidying it in passing is what that
item exists to prevent.

**2. Amend `CLAUDE.md` §12.2** so a session that reads only `CLAUDE.md` finds it:
the fourth section of `OUTPUT.md` carries a standing heading for asks still
outstanding, present even when empty, carried forward verbatim until ruled. A
report without it is defective. Cross-reference §9.6, since the work order carries
the same list inbound.

**3. Seed the queue** in this session's own report, since it is the first one that
must carry it. From the record as it stands, outstanding asks are:

- **Whether an attended automatic cycle may reach an antenna** (§0.2, HM-DEC-098),
  awaiting the interlocks watched into the dummy load per `BENCH_CARD.md`.
- **A callsign too long for one keyer send** (HM-DEC-130), refused until the seam
  between two sends is measured into the load.
- **Whether the star asks for a name at the moment of saving** (HM-DEC-060,
  HM-DEC-134), handed back by an earlier session as §12.1 clause 3 and unruled.

Check that list against `OPEN_ISSUES.md` and the last three `OUTPUT.md` files
rather than taking it from me, and add anything I have missed. **If an ask I have
listed was in fact ruled, say so and leave it out** — a queue that carries settled
questions forward is worse than none.

**4. Add the same list to the top of this file** — `WORK_INSTRUCTIONS.md` is
committed (HM-DEC-135), so the inbound channel starts carrying it from here rather
than from the next order.

Then write `PROJECT_STATUS.md`, commit and push to `main` (HM-DEC-113), and
report.

## Asks still outstanding, inbound

Per HM-DEC-139 as written above, and the reason step 3 exists: the three items
listed there are the queue as I hold it. This heading is the channel from now on.

## Named and left (§12.6)

- `DECISIONS.md` missing entries for 096 to 133 — the largest hole in the record,
  and next.
- HM-OPEN-036, §1's head ordering, whenever it is opened deliberately.
- The record sweep for rulings resting on a write outcome (Tim ruled B).
- The automatic `27 11` at connect, half the cable, against HM-DEC-062.
- HM-OPEN-042's remaining rungs.
- Mode follow, favorites, the recent list.

## Reporting

`OUTPUT.md`, four sections (HM-DEC-106), with section four now carrying the
standing heading this order creates.

**Stop and report. Do not start anything else.**
