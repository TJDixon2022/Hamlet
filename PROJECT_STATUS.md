PROTOCOL: 2
PROJECT: Hamlet
STATE: COMPLETED
PHASE: none — the auto-CQ work order finished 2026-08-18 and no work order is open
BALL: tim
NEXT_PASTE: none prepared
WORK_ORDER: CLEANUP_BRIEF.md — auto-CQ, into a dummy load. All six phases completed 2026-08-18.
DROP_CANDIDATE: none — phase 6 was that order's drop candidate and it was not dropped
OPEN_QUESTIONS: 2 — whether an attended automatic cycle may reach an antenna (§0.2, HM-DEC-098), and what to do about a callsign too long for one keyer send (HM-DEC-130 refuses it until the seam is measured into the dummy load)
OPEN_ITEMS: 25 open in OPEN_ISSUES.md
TOP_SEVERITY: hard
RULES_AT: HM-DEC-131, 2026-08-18
TESTS: 1902 total, 1900 passing, 2 failing — both the recorded baseline. Measured 2026-08-18 on this machine.
ASSUMPTIONS: STATUS_PROTOCOL.md is absent, so neither this file nor PROJECT_CARD.md has been checked against the protocol its header names. §1's decision table is not strictly newest-first at its head — HM-DEC-128 sits above HM-DEC-130 and HM-DEC-129 — which is HM-OPEN-036 and is reported rather than corrected. The two items marked severity hard, HM-OPEN-016 and HM-OPEN-017, block work that no longer exists, so TOP_SEVERITY reads hard from the record while nothing is actually stopped: HM-OPEN-037.
SESSION_SURFACE: Claude Code (§9.0, §9.5) — reads the tree directly, edits in place, builds, and commits
UPDATED: 2026-08-18T18:45:18+00:00
NOTE: Nothing has reached an antenna and nothing has transmitted; the interlocks are proved by test and not yet watched into the load.

---

## What this file is

Volatile state: what is true about Hamlet right now, and only right now. Where
`PROJECT_CARD.md` holds the facts that survive between sessions, this one holds
the ones that change within them — who must act next, what is measured, and what
is being assumed while nobody has ruled.

**Every session writes this file at each state transition and at no other time**
(HM-DEC-131). Writing it more often makes it a log, which is what the telemetry
record is for; writing it less often makes it a fiction, which is worse than
absent because it looks current.

**`BALL: tim`** means the next act is his and nobody else is waiting on anything.
It is not a synonym for blocked: the two questions in `OPEN_QUESTIONS` are in
`OUTPUT.md` section 4 in the decision log's own format, ordered with the one that
blocks the most work first.

**Every count here is measured rather than estimated.** `TESTS` says what a real
run produced on this machine and would say *not run* if none had happened.
`OPEN_ITEMS` and `TOP_SEVERITY` are counted out of `OPEN_ISSUES.md` rather than
recalled. `RULES_AT` is the ruling id and date actually in `CLAUDE.md` §1.

**`ASSUMPTIONS` is the field that earns its place.** An unmarked wrong value is
indistinguishable from a right one; a marked one is a question with an owner
(§12.4). The three recorded there are all cases where the record and the world
have drifted apart slightly, and each is named rather than quietly reconciled —
including one, the ordering of §1's head, that a session could have tidied in
passing and deliberately did not, because a ruling row is never edited (§1).

## The protocol it names has not arrived

`PROTOCOL: 2` refers to `STATUS_PROTOCOL.md`, which **is not in this repository
yet.** It is being revised elsewhere and will be delivered here later.

The header therefore says which protocol this file is written against and **makes
no claim of conformance that anybody here can currently check.** When the protocol
arrives, this file and `PROJECT_CARD.md` are checked against it rather than
assumed to already agree, and any disagreement is an open issue rather than a
quiet edit.
