# ANNUNCIATOR.md — reporting status to the panel

**PROJECT: [FILL IN — this project's gate name, exactly as `CLAUDE.md` uses it]**

Drop this file into the project, and paste it into the project's chat
**instructions** field as well — knowledge is searched, so a short message never
retrieves it; instructions are read every turn.

---

## Why

The owner runs several projects at once, in separate Claude Code sessions and
separate chat conversations. A panel reads two small files from each repository
and shows every project on one screen, so he can see which window a clipboard
belongs in before he pastes.

**The panel only knows what a session last wrote.** A project whose sessions
never write reads as dead on the panel while it is working. That has already
happened: a session ran for an hour with tests going green while the panel showed
it blocked, because nothing in that tree told it to write.

A chat session cannot write to disk. Only a Code session can. So the chat side's
job is to make sure every prompt it hands the owner carries the instruction.

---

## Two files, at the repository root

**`PROJECT_CARD.md` — standing facts.** Changed only by ruling. Five lines:

```
PROJECT: [gate name]
ONE_LINE: what this project is and for whom
REPO_PATH: C:\Source\...
REMOTE: https://github.com/...
TRUNK: main
```

`ONE_LINE` is what tells two similarly-named projects apart on screen. `TRUNK` is
what makes the off-trunk check possible — without it the panel can see the branch
but cannot say whether it is the wrong one.

**`PROJECT_STATUS.md` — volatile state.** Overwritten whole. Eight lines:

```
STATE: PREPARING_PROMPT | ANSWERING_QUESTIONS | EXECUTING | COMPLETED | BLOCKED
TASK: n of m
WORK_INSTRUCTION: what you are running, or none
PROMPT: how many prompts this phase has taken, counting this one
BALL: code | web | tim | unassigned
NEXT_PASTE: none, or what -> where
UPDATED: 2026-08-18T14:32:05-04:00
NOTE: one line
```

**Both files: the reader takes the leading run of `KEY: value` lines and stops at
the first blank line, `---`, or `#`.** So every value is one line, and no value
may begin a line with `#`. Put prose below a `---` if you want it; nothing reads
it.

### The fields that get misused

`STATE` is one of those five words and **nothing else**. A sentence there makes
the whole window unreadable — that has already happened.

`TASK` is `n of m` — the task you are on inside this prompt, and the total the
prompt gave you. `—` when nothing is running.

**It used to be called `PHASE`, and `PHASE` still reads.** The word `phase`
now means something larger — the day's goal, set with the web session and
holding for a whole conversation — and that lives on `PROJECT_CARD.md`. Two
meanings for one word is the drift this whole arrangement exists to catch, so
the smaller meaning got the new name. Write `TASK`; a file still writing
`PHASE` keeps working.

`WORK_INSTRUCTION` is **what you are running**, in whatever this project calls it:
`007`, `WU-J`, `CLEANUP_BRIEF.md`. It is not a filename — the file is
`WORK_INSTRUCTIONS.md` in every project — because the point is telling the owner
*what is running*, not naming the file it is written in. **`none` when nothing is running, never absent**: a project between
work units and a project that forgot the field are different facts, and the panel
shows them differently. `step 3 of 6` says very little without it, and the owner
is looking at several projects that each ran something different today.
`WORK_ORDER` still reads as an alias, so a file already in the field keeps
working.

`BALL` is who must act next. `unassigned` means nobody has taken it, and is not a
polite way of saying it is the owner's.

`NEXT_PASTE` is `none`, or **what → where**: `OUTPUT.md -> Claude Web`. Name the
destination even when it seems obvious; telling identical-looking windows apart is
the entire point.

`UPDATED` is ISO 8601 **with a UTC offset**, read from the clock, never typed. A
typed timestamp has already claimed to be five minutes in the future.

`NOTE` is one line, and it is a caption rather than a record. Anything that must
survive the session goes in the decision log and the report as usual.

**Do not report branch, commit, or working-tree state.** The panel reads those
from `.git` itself. A measured fact stays true when a session goes quiet; a typed
one does not, and a dirty working tree cannot be determined from `.git` at all.

---

## Which document wins

This file is **authoritative on what a project must supply** — the fields above,
their names, and the write triggers below. `STATUS_PROTOCOL.md`, the long form
kept in the annunciator repository, is authoritative on **what the panel does
with what you supply**: how the header is parsed, what each check compares, what
it reads from `.git`.

**Where the two disagree about what a project must supply, this file wins.**

That is settled by ruling, not convention, because this is the file that is
actually read — it is handed to the project and pasted into the chat
instructions, and a requirement nobody reads is not a requirement.

**Nothing enforces that the two stay in step except care.** If you change what is
asked for here, the long form has to change in the same delivery, and there is no
lamp that will tell you if it did not.

---

## When a Code session writes

- **when it has verified the gate**, before any task — `TASK: 0 of m`, `NOTE`
  saying preflight. Phases are numbered from 1, so nought means *gate checked,
  nothing started*. Without this the window shows the previous run while a
  session is already working in the folder.
- **when it refuses the gate** — the prompt named another project, or carried no
  gate line. `STATE: BLOCKED`, `BALL: tim`,
  `NEXT_PASTE: corrected prompt -> Claude Code`, `NOTE` naming what it refused
  and which project the prompt claimed. There is no separate state for this:
  a refused gate is work stopped that only you can restart. A session that
  refuses silently leaves the panel showing the last run, so the refusal looks
  exactly like nothing having been pasted at all.
- when it starts work
- at each phase boundary
- when it stops for a ruling
- when it finishes
- **every ten minutes while `STATE: EXECUTING`**

The ten-minute rule is the one that matters. Phases can run an hour, and **a long
phase and a dead session look identical** without it. On that write, update `NOTE`
to say what is happening *inside* the phase — `Phase 3 — rebuilding the AR/IR
fixtures, 4 of 11 rebuilt` — so there is something moving to look at.

Do not otherwise rewrite the file to say the same thing again. Outside a run, a
rewrite with an unchanged state is a heartbeat, and a heartbeat makes staleness
meaningless.

---

## The reset — run once

> PROJECT: [gate name]
>
> Verify the gate against the tree before anything else. If the tree is a
> different project, say which and stop.
>
> Read `ANNUNCIATOR.md` at the repository root. Then:
>
> 1. Add a numbered section to `CLAUDE.md` stating that this project maintains
>    `PROJECT_CARD.md` and `PROJECT_STATUS.md` per `ANNUNCIATOR.md`, and giving
>    the six status fields and the write triggers inline — so a session that reads
>    only `CLAUDE.md` still knows the rule.
> 2. Add one row to `CLAUDE.md` §1 recording it, dated today, with this project's
>    next free ruling id. Do not edit any existing row. If §1 is not newest-first
>    at its head, say so in your report and state where you placed the row.
> 3. Write `PROJECT_CARD.md` — measure `REPO_PATH`, `REMOTE` and `TRUNK` from git;
>    take `ONE_LINE` from this project's own `CLAUDE.md` header.
> 4. Write `PROJECT_STATUS.md` from measurement, now, before you report.
>
> Commit and push to trunk. Report in four sections: what you did, what the owner
> should expect, what to do next, what is blocking.

**Step 1 matters more than it looks.** `CLAUDE.md` is read automatically by every
Code session; a companion file is read only if something points at it. Putting the
rule in `CLAUDE.md` is the whole reason this works.

---

## The standing rule for the chat side

### Putting the rule in `CLAUDE.md` is necessary and not sufficient

**This is the part that has already failed in practice.** On 2026-08-18, three
projects had the rule written into their own `CLAUDE.md`, and all three read that
file at the start of a run — and none of them wrote the status file during the
run.

The likely reason is placement, not comprehension. A session deep in phase 3 of a
work order is executing the work order. A section of a file it read half an hour
ago is no longer in front of it, and nothing in the work order reminds it. The
rule was present, correct, and out of view.

So the rule lives in two places, and both are load-bearing:

| Where | Why it is there |
|---|---|
| `CLAUDE.md`, a numbered section | Read automatically at the start of every Code session, so a session that gets no other instruction still knows |
| **The prompt itself, every time** | What the session is actually looking at while it works |

Neither replaces the other. The first is what makes the rule exist; **the second
is what makes it happen.**

### The exact line to append

**Every Claude Code prompt ends with this block**, in the prompt's own text —
not as a reference to a file, because a reference is the thing that did not
work. It is `CLAUDE_CODE.md` §6 verbatim:

> ```
> PROJECT: <name>
> Execute WORK_INSTRUCTIONS.md.
>
> Status cadence, for this and every session:
>
> After each task, before starting the next, update PROJECT_STATUS.md per
> CLAUDE.md — STATE, TASK: n of m, BALL, UPDATED from the clock, and
> NOTE saying what is moving inside the task, not restating the task name.
>
> Do the same every ten minutes while a task is running.
> 
> Commit and push each task before starting the next.
> ```

And **state how many tasks the prompt contains**, so `TASK: n of m` has a real
`m` rather than a guess.

Spelling the fields out matters as much as the instruction. *Update the status
file* is a sentence a session can satisfy once and consider discharged; naming
`UPDATED` *from the clock* and `NOTE` *saying what is moving inside the task* is
a thing it has to do again at each boundary, and it is the `NOTE` that makes the
difference between a heartbeat and something worth looking at. **Not restating
the task name** is the sharpest of the four: a note that repeats the heading tells
the owner nothing he did not already have.

**This is a diagnosis, not a proven fix.** Three projects, one afternoon. If
prompts start carrying the line and the status files still do not move, placement
was the wrong explanation and the next thing to examine is what the session is
being asked to do instead.

---

## What not to do

- **Do not claim to have updated the files from a chat session.** You cannot write
  to disk. Say what the Code session should write.
- **Do not invent a state or a `BALL` value.** Put it in `NOTE`, or raise it as a
  decision ask.
- **Do not write a count or a timestamp you did not measure.** `none`, `—` and
  `not run` are real answers; a plausible number is not.
- **Do not edit the card during a work order.** It holds standing facts; if one is
  wrong, that is a ruling, not an edit.
