# ANNUNCIATOR.md — reporting status to the panel

**PROJECT: Hamlet**

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
PROJECT: Hamlet
ONE_LINE: what this project is and for whom
REPO_PATH: C:\Source\...
REMOTE: https://github.com/...
TRUNK: main
```

`ONE_LINE` is what tells two similarly-named projects apart on screen. `TRUNK` is
what makes the off-trunk check possible — without it the panel can see the branch
but cannot say whether it is the wrong one.

**`PROJECT_STATUS.md` — volatile state.** Overwritten whole. Six lines:

```
STATE: PREPARING_PROMPT | ANSWERING_QUESTIONS | EXECUTING | COMPLETED | BLOCKED
PHASE: n of m
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

`PHASE` is `n of m` — the phase you are on, and the total the prompt gave you.
`—` when no work order is running.

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

## When a Code session writes

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

> PROJECT: Hamlet
>
> Verify the gate against the tree before anything else. If the tree is a
> different project, say which and stop.
>
> `ANNUNCIATOR.md` is now at this repository root. Read it in full. Two rulings
> have been made that answer the questions the last session correctly stopped on:
>
> **`ANNUNCIATOR.md` replaces `STATUS_PROTOCOL.md` for this project.**
> `STATUS_PROTOCOL.md` is not coming and should not be waited for; it stays in the
> `ClaudeProjectStatus` repository as the specification of what the panel can
> read, while `ANNUNCIATOR.md` is what this project is asked for. Update the prose
> in `PROJECT_CARD.md` and `PROJECT_STATUS.md` that points at
> `STATUS_PROTOCOL.md` by name, in the same pass.
>
> **The eleven fields in `ANNUNCIATOR.md` are required; every other field either
> file already carries is optional and stays.** This is not a rewrite against a
> smaller schema. The card's 21 fields and the status file's 16 are kept as they
> are. Do not remove a field to match the new document.
>
> Then:
>
> 1. Add a numbered section to `CLAUDE.md` stating that this project maintains
>    `PROJECT_CARD.md` and `PROJECT_STATUS.md` per `ANNUNCIATOR.md`, quoting the
>    six status fields and the five write triggers **inline** — so a session that
>    reads only `CLAUDE.md` still knows the rule.
> 2. Add one row to `CLAUDE.md` §1 as `HM-DEC-132`, dated today, **superseding
>    `HM-DEC-131`.** The supersession is narrow and the row should say so: the
>    only substantive change is the write trigger. `HM-DEC-131` says *at each
>    state transition and at no other time*; the new rule adds **every ten minutes
>    while `STATE: EXECUTING`**, because a phase can run an hour and a long phase
>    and a dead session are otherwise indistinguishable. Everything else in
>    `HM-DEC-131` carries forward.
> 3. Place the row at the true head of §1, above `HM-DEC-131`, as the last session
>    did. `HM-OPEN-036` — that §1 is not newest-first at its head — stays open and
>    uncorrected. Say in your report where you placed it.
> 4. Confirm `PROJECT_CARD.md` carries `PROJECT`, `ONE_LINE`, `REPO_PATH`,
>    `REMOTE` and `TRUNK`, and add any that are missing, measured from git.
> 5. Rewrite `PROJECT_STATUS.md` from measurement, now, before you report.
>    `UPDATED` from the clock, never typed.
>
> Commit and push to `main`. Report in four sections: what you did, what the owner
> should expect, what to do next, what is blocking.

## The standing rule for the chat side

**Every Claude Code prompt ends with:**

> Update `PROJECT_STATUS.md` per `CLAUDE.md`'s status section at each transition
> and every ten minutes while executing.

And **state how many phases the prompt contains**, so `PHASE: n of m` has a real
`m` rather than a guess.

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
