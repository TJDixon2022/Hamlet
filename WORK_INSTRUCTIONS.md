PROJECT: Hamlet
ISSUED: 2026-08-19

# Work order — three rulings the record is missing

**PHASES: 2.** That is your `m` for `PHASE: n of m`.

**Write `PROJECT_STATUS.md` now, before you read further** — `STATE: EXECUTING`,
`PHASE: 1 of 2`, `BALL: code`, `UPDATED` from the clock, `NOTE` saying you are
starting. Then again every ten minutes while a phase runs, with `NOTE` saying what
you are doing *inside* it, and again at the phase boundary. §13.2 has carried this
rule since HM-DEC-132; three consecutive sessions did not apply it, which is what
phase 1 is partly about.

Gate first (HM-DEC-099): verify `PROJECT: Hamlet` against `PROJECT_CARD.md` in
this tree and against the prompt you were pasted. Any disagreement, stop.

**This is a records order, not a work unit.** §12.3's five-or-six phases governs
work units; this is three rulings and two `CLAUDE.md` sections, and padding it to
five phases would be inventing work. **No source file is changed by this order.**
If you find yourself editing anything under `src/`, stop — something has gone
wrong.

Every ruling below is Tim's, reproduced verbatim. **None is recorded under §12.1
and none is a session's to reword.** If a text below disagrees with something you
find in the tree, say so and stop rather than reconciling it yourself (§12.4).

---

## Phase 1 — HM-DEC-138 and HM-DEC-137

### 1. HM-DEC-138 — the frequency's cadence, ruled 2026-08-19

The last session found that the frequency is already on the live poll in the tree,
shipped as `099de5a` in the session before it, with the ruling asked for and never
given. **The code has been running ahead of the record.** Tim ruled to leave it
live and record it.

Write to `DECISIONS.md` at the head:

```
---
id: HM-DEC-138
date: 2026-08-19
refs: src/Hamlet.RadioEngine/Rig/RigPollPlan.cs, src/Hamlet.RadioEngine/Rig/RigStateMonitor.cs, HM-DEC-109, HM-DEC-050, HM-DEC-062
---

**The frequency is read on the live poll and stays there. Supersedes HM-DEC-109 on
this field's cadence and sets aside HM-DEC-050's exemption for it.** The rest of
HM-DEC-050 stands: rationing a slow shared line is right, and what is set aside is
one exemption granted in favour of something that is not happening.

THE PREMISE WAS FALSE AND NOBODY HAD MEASURED IT. HM-DEC-050 exempted the
frequency from polling because the radio broadcasts it, so asking could only ever
be more stale. Measured on the operator's own radio on 2026-08-19, session
`6630ee0f`: 5,499 inbound frames in sixty-one seconds, `inboundTransceive` zero,
`inboundBroadcast` zero, `radioIsBroadcasting` false. **CI-V Transceive is off on
this radio and Hamlet does not write the operator's settings.** Asking is not the
more stale option. It is the only one.

WHAT IT COSTS, MEASURED RATHER THAN FEARED. A frequency read is six bytes out and
eleven back. The link already carried 1,380 commands in that minute and answered
1,379. Four reads a second is under seventy bytes on a cable moving eleven
thousand, for the field the operator looks at more than any other.

REVERTING TO THE SESSION SWEEP WAS REJECTED, and it is the option this ruling
exists to close. The sweep is what turned the snap-back defect into thirty seconds
of wrong display instead of one poll: once something put a stale value on screen,
only the next reading could move it forward. The guard built on 2026-08-19 stops
that particular write, but a cadence chosen so that the *next* such fault is
thirty seconds long rather than a quarter of a second is choosing badly on
purpose.

A CONDITIONAL CADENCE WAS ALSO REJECTED, though `SkipLiveRead` already implements
it. On a radio that never announces it is the live poll with extra steps, and on
one that does the broadcast wins the race anyway and costs nothing. What it adds
is a second mechanism and a decision about which applies — and **push is the thing
that proved unreliable here.** A display that always asks finds out immediately
when the radio goes quiet; one that waits to be told finds out two builds later,
which is what happened.

THE CODE SHIPPED BEFORE THE RULING AND THAT IS ITS OWN FAULT. `099de5a` changed
the cadence with the ruling requested in that session's report and not given, and
the next order withdrew a draft of the same ruling while the change was already in
the tree. §9.5 says a decision not in the record is not made; this ruling makes it,
and the gap between the two is worth an open item rather than a shrug.
```

**Also raise the gap as an open issue** if nothing in `OPEN_ISSUES.md` already
holds it: a shipped change carrying an unanswered ruling request, inherited by the
next session as settled. It is the same shape as HM-DEC-113's invented branch.

### 2. HM-DEC-137 — the status instruction, ruled 2026-08-19

Write to `DECISIONS.md` at the head:

```
---
id: HM-DEC-137
date: 2026-08-19
refs: CLAUDE.md §13, ANNUNCIATOR.md, HM-DEC-132, HM-DEC-131, HM-DEC-099, HM-DEC-135
---

**The status-write instruction lives in `CLAUDE.md` and in every Claude Code work
order, and an order delivered without it is defective and is redone.** A session
writes the status whether or not the order it was handed says so. Supersedes
nothing; HM-DEC-132's triggers and fields are unchanged.

THE RULE WAS NEVER THE PROBLEM. §13.2 has carried five triggers since HM-DEC-132,
including every ten minutes while executing, and consecutive sessions did not
apply them. One said so directly in its own report: §13 was read, and not applied;
the order began without a write and crossed two phase boundaries without one. A
correct rule that nothing carries is indistinguishable from no rule, and the panel
it feeds showed a working project as dead, which is the exact failure HM-DEC-131
was written to prevent.

TWO CHANNELS BECAUSE NEITHER HAS HELD ALONE. A rule only in `CLAUDE.md` is read
once at the start and forgotten across a phase that runs an hour, which is
precisely the phase the ten-minute write exists for. A rule only in the prompt is
lost whenever a prompt is written in a hurry, and every order delivered to this
project had been missing the closing line `ANNUNCIATOR.md` already required of it.
Both channels have now failed in the field. One of the two will catch.

AND A MISSING LINE IS A DEFECT, NOT AN OVERSIGHT. HM-DEC-099 already takes this
shape: a prompt without its gate is defective and redone, because the failure it
prevents is one the session cannot detect from inside. The chat side cannot write
to disk (`ANNUNCIATOR.md`), so the only thing it can be held to is the instruction
it hands over — and holding it to that is what makes the requirement real rather
than advisory.
```

Add a short numbered subsection to `CLAUDE.md` §13 stating both channels and that
**a session writes the status regardless of what its order says**, so a session
reading only `CLAUDE.md` is still covered when the order is the thing that failed.

Add both index rows at the **true head** of `CLAUDE.md` §1 — HM-OPEN-036 records
insertions landing at a fixed anchor instead, and it stays open. Say in your report
where you placed them.

Then commit and push to `main` (HM-DEC-113), and write `PROJECT_STATUS.md` with
`PHASE: 2 of 2`.

## Phase 2 — HM-DEC-135, dropped from three orders now

The convention you are reading this under — that a work order is
`WORK_INSTRUCTIONS.md` at the repository root and the pasted prompt is the gate
line plus "read that file and execute it" — is unrecorded. It was delivered on
2026-08-18 and on 2026-08-19 and lost both times. **Its text is reproduced here so
that nothing has to be found in git history.**

Write to `DECISIONS.md` at the head:

```
---
id: HM-DEC-135
date: 2026-08-18
refs: CLAUDE.md §9.6, WORK_INSTRUCTIONS.md, HM-DEC-100, HM-DEC-106, HM-DEC-099
---

**A Claude Code work order is delivered as `WORK_INSTRUCTIONS.md` at the
repository root, and the prompt Tim pastes says only which project it is and to
read that file and execute it.** Amends HM-DEC-100 on what the pasteable prompt
contains and supersedes nothing.

THIS IS HM-DEC-106 POINTED THE OTHER WAY. That ruling moved the session's report
out of the terminal and into `OUTPUT.md`, because reports were being read off
photographs of a scrollback buffer and a report Tim has to photograph is a report
he reads less carefully. The inbound half had the same defect and nobody had named
it: a work order pasted into a prompt box is retyped, is truncated by whatever the
buffer holds, cannot be diffed, cannot be committed, and is gone the moment the
window closes. The two files are a pair. Work comes in through one and goes back
out through the other, both at the root, both in the tree the session is about to
change.

WHAT THE PASTED PROMPT CONTAINS IS NOW TWO LINES: the gate, and the instruction to
read and execute. HM-DEC-100 stands otherwise. A delivery is still a single
scaffolded zip extracted over the root, still never a snippet, still never a file
Tim places or patches by hand, and `WORK_INSTRUCTIONS.md` rides in that zip like
everything else.

THE GATE IS IN BOTH PLACES AND THAT IS NOT BELT AND BRACES. HM-DEC-099 requires
`PROJECT: Hamlet` on every prompt and every work order, and a one-line prompt makes
the failure it guards against worse rather than better: pasted into the wrong
repository, "read `WORK_INSTRUCTIONS.md` and execute it" finds that project's file
and executes somebody else's work order, with a gate that agrees with itself the
whole way down. So the prompt carries the gate, the file carries the gate, and the
session checks both against `PROJECT_CARD.md`. Any of the three disagreeing stops
the session.

AND IT CARRIES THE DATE IT WAS ISSUED, because a file at a fixed path is a file
that can be read twice. `WORK_INSTRUCTIONS.md` is overwritten whole per work
order, in the manner of `PROJECT_STATUS.md`, so a session opening one older than
the last `OUTPUT.md` is looking at work already done and stops. A pasted prompt
could not be stale; a file can.

IT IS COMMITTED. The work order that produced a commit is worth having beside it,
and a session that wants to know why the last one did something has the
instruction it was given rather than an inference from the diff.
```

Add `§9.6` to `CLAUDE.md`, after §9.5:

```
### 9.6 The work order — ABSOLUTE, Claude Code sessions

A work order is a file, never pasted text (HM-DEC-135). It is
`WORK_INSTRUCTIONS.md` at the repository root, it arrives in the delivery zip like
any other file (§9.1), and it is committed.

The prompt Tim pastes is two lines and nothing else:

    PROJECT: Hamlet

    Read WORK_INSTRUCTIONS.md at the repository root and execute it.

**Both carry the gate and the session verifies both against `PROJECT_CARD.md`**
(§13, HM-DEC-099). A one-line prompt in the wrong repository would read that
project's work order and find a gate that agrees with itself, which is the one
failure §9's gate exists to prevent.

The file opens with `PROJECT:` and `ISSUED:`. It is overwritten whole per work
order, so a session opening one dated earlier than `OUTPUT.md` is holding work
already done: it says so and stops.

**Its phases are worked as numbered.** A phase is skipped only where the order
names it droppable, and an item the order names and leaves (§12.6) is not worked
instead. A session that departs from the numbering says so in section one.

**Every order carries the status instruction of §13 and states its phase count**
(HM-DEC-137). An order missing either is defective and is redone; the session
writes the status regardless.

`WORK_INSTRUCTIONS.md` in, `OUTPUT.md` out (HM-DEC-106). Both at the root, both
committed, both in the tree the session is changing.
```

Add its index row at the true head of §1, dated 2026-08-18.

Then commit and push to `main`, write `PROJECT_STATUS.md` with `STATE: COMPLETED`
or `BLOCKED`, `BALL: web`, `NEXT_PASTE: OUTPUT.md -> Claude Web`, and report.

---

## Named and left (§12.6)

- HM-OPEN-042's remaining rungs.
- The record sweep for rulings resting on a write outcome (Tim ruled option B).
- `DECISIONS.md` missing entries for 096 to 133.
- HM-OPEN-036, §1's head ordering.
- Mode follow, favorites, the recent list.
- **HM-DEC-136 does not exist and is not to be written.** It was drafted on
  2026-08-18, withdrawn before delivery when the operator's manual tuning
  disproved its premise, and named as withdrawn in that order. The gap in the
  numbering is deliberate. HM-DEC-138 is what the question was eventually ruled.

## Reporting

`OUTPUT.md`, four sections (HM-DEC-106). Section one carries each entry you wrote
in full and says where in §1 you placed its row.

**Stop and report. Do not start anything else.**

---

Update `PROJECT_STATUS.md` per `CLAUDE.md`'s status section at each transition and
every ten minutes while executing. **This order contains 2 phases.**
