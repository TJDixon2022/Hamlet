# CLAUDE_CODE.md

**Version 1.8. This file is byte-identical in every project.**

It governs how a Claude Code prompt is built, delivered, executed and reported.
It does not govern the work itself.

---

## §0 Authority

**`CLAUDE.md` governs the work. This file governs the artifacts.**

`CLAUDE.md` is per-project and changes constantly. This file is the same in every
project and changes rarely. **Where the two disagree about the shape of a work
instruction, a prompt, a delivery or a report, this file wins** — a conflict is
far more likely to be a project file drifting than the standard being wrong.

**The session follows this file and names the conflict in its report.** A silent
override leaves the drift in `CLAUDE.md` forever.

`CLAUDE.md` still wins on everything about the project: the prime directive, the
decisions in force, what the code must do, whose files are whose, the trunk, the
test command, project-specific prohibitions.

### The specificity floor

**This file wins on shape. It does not win by being vaguer.**

Where a project file names a specific artifact, path, script or mechanism and
this file names only an outcome, **the project file wins**, and the session
reports the gap so this file can be fixed.

That clause exists because the rule above got it backwards on its first day. §1
originally said the session should ask the owner to run *the project's harvest
script* — an outcome with no artifact behind it. A project's own rules named the
artifact exactly and recorded why: a peer project lost four rounds to a rule that
named an outcome, and a session satisfied it by inventing a script. **The project
file was right and the standard was the vague one**, which the tiebreaker as
written would have overridden.

Specificity beats generality regardless of which document holds it. A standard
that overrides a precise rule with an imprecise one is not standardising
anything.

**Nothing in this file is tailored per project.** The gate's four checks are
project-specific *values*; the requirement to have four is universal. Four copies
of one document diverge and nothing here catches it — the version line above is
the only handle, and it depends on somebody comparing.

---

## §1 Starting a conversation

A phase and a conversation are the same span. A new phase means a new
conversation, with this file and the project's own documents loaded fresh.

The owner says: **read `CLAUDE_CODE.md` and begin.**

### Collecting the current state

The session cannot read the tree. It needs the root `.md` files, and it gets them
by giving the owner a script to run.

**The script is generated from `get-files.template.bat` and its README, both of
which are in project knowledge and are read at turn one.** They are named here
because naming an outcome instead of an artifact is what this fails at: a peer
project lost four rounds to a rule that said *collect the files*, and a session
satisfied it by writing its own script.

The template is **copied verbatim.** Three things change and nothing else:

- the **file-list block** marked `<<< REPLACE THIS BLOCK EVERY TIME >>>`;
- the **`Generated <date> for:` line** in the header;
- the **default repository root**, taken from `PROJECT_CARD.md`'s `REPO_PATH`,
  which is measured rather than remembered. A wrong default makes every
  double-click fail before doing anything.

Not the subroutines, not the staging paths, not the output name, not the zip
mechanism, not the `XD` exclusion list. **The README records why each of those
cost a round to get right**, including the exclusion parity with the listing
script — change one and the arrival check compares two different views of the
tree and reports differences that are not real.

### Where the templates live

**Both templates — `get-files.template.bat` and §5's
`extract-gate.template.bat` — are in project knowledge, and that is the copy the
session reads.** It is the only one available at turn one, before any harvest has
run, and it is what generates the script that fetches everything else.

**A copy of each also lives at `tools\templates\` in every repository**, pushed
by the standards distribution script and verified byte-identical across all
roots. That copy is not what the session reads. It exists because project
knowledge is five separate stores that nothing on the owner's machine can write
to or compare, so **a template corrected in one project is corrected in one
project and nowhere else, silently.** The repository copy is the one a machine
can check.

**Where the harvested copy and project knowledge disagree, the session says so**
and uses project knowledge, because that is the copy it was handed. Drift then
surfaces on the first turn of the next conversation instead of never. **A
template is not distributed until both are updated** — the file-list block for a
conversation start includes `tools\templates\` for this reason.

For a conversation start the block is the root `.md` files. The project's own
`CLAUDE.md` may name more; where it does, it wins under §0's specificity floor.

The session then:

1. Delivers the generated `get-files.bat`, scaffolded per §5, and asks the owner
   to run it and upload the zip.
2. Reads everything in it.
3. Replies with **one line and one question**:

> The last conversation completed [phase]. What is the overall goal for this phase?

**Not a status dump.** No four sections, no summary of what was read. The harvest
is held, not recited, and surfaces only when something in it contradicts what the
owner is about to do.

**The exception:** if what arrived shows the previous phase did not complete — a
session left `BLOCKED`, an unanswered ruling in the last report's section 4 — say
so first, in one or two lines, then ask the question. This should be rare; seeing
it is itself a signal.

**A `MISSING` line in the script's output is loud on purpose. Do not build past
it.** A file that did not arrive is not a file that does not exist, and the two
have already been confused — a listing is a view of the working tree, not of the
repository, and a file absent from one may be committed on another branch.

---

## §2 The hierarchy

| | What it is | Who sets it | Span |
|---|---|---|---|
| **Phase** | The day's goal | The owner, with the web session | Many sessions |
| **Work instruction** | One Code session, one token spend | The owner drives; the web session writes it | 45–60 min |
| **Task** | A step inside one work instruction | The web session | Minutes to an hour |

A phase might be *connect CoreHMI to the simulator and see live data in the HMI*
and take ten work instructions to reach.

**`TASK: n of m` in the status file is the task, not the phase.** The phase lives
in `PROJECT_CARD.md` because it is a standing fact for the length of a
conversation, and a session rewriting the status file every ten minutes should not
be retyping the day's goal.

**Target 45 to 60 minutes of Code time per work instruction.** That is the sizing
constraint, not a task count — one task can be an hour and four can be twenty
minutes. The drop candidate exists to keep a session inside that window when a
task turns out larger than expected.

---

## §3 Who drives

**The owner drives.** He states what the next set of tasks is. The web session
writes them into the correct format and delivers them.

The web session does not propose work instructions, offer candidate units, or
decide what comes next.

**Every work instruction advances the application.** Documentation, records and
housekeeping ride along inside a task; they are never a work instruction of their
own. A session that is only tidying is a token spend for nothing.

**Defects advance the application.** A defect is behaviour that is wrong — from a
failing test, or from the owner running the application and getting something
other than what was expected. The second is the majority case and the owner calls
them out. Neither is housekeeping.

**Nothing interrupts a running session.** Defects found while a session runs are
held. The cycle is: session completes → `output.md` arrives → then everything that
accumulated is considered and folded into the next work instruction.

---

## §4 The work instruction file

`WORK_INSTRUCTIONS.md` at the repository root. **Detailed.** The prompt is short
because the file is not.

Sections in this order.

### §4.1 The gate — a hard preflight

First, before anything else. The `PROJECT:` line is for the owner deciding which
window to paste into. The four checks are for the session.

```
STOP. Verify the project before reading any further.

PROJECT: <name>

Check the repository root:
  MUST EXIST:      <file>
  MUST EXIST:      <file>
  MUST NOT EXIST:  <file>
  MUST NOT EXIST:  <file>

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project — nothing done."

If all four hold, say "<name> confirmed" and continue.
```

**Four checks, two of each kind.** Must-exist alone passes in a repository that
holds the right file among many others. **The must-not-exist half is what
discriminates**, and the files are chosen to separate this project from the one
most likely to receive its prompt by mistake.

A name gate is a string a session can interpret. Filesystem checks are not.

### §4.2 Why this unit exists

A short block before the tasks, **leading with the number the unit is aimed at.**

> The leading edge reads 13 of 43 on the ARRL bulletin. The settled pass reads 33.

Not *decode accuracy is poor*. The number is what the report is measured against.

**Where the number is new or contested, say where it came from** — and where
previous reports asserted otherwise, say that too. That clause is what stops the
next session trusting the old figure.

**Where there is no number**, one sentence naming the specific consequence: *after
an Emergency Stop, an operator at the panel cannot bring the machine back.*

### §4.3 Verify this instruction against the tree

Standing, in every unit:

> **Nothing here describes the tree.** Check every claim against the files and
> report any mismatch.

The web session writes these blind. Every path, method name, threshold and count
comes from a report, and reports go stale.

- **Report the mismatch; do not repair the instruction.** A session that corrects
  it silently teaches nobody and leaves the next unit carrying the same wrong fact.
- **Mismatches go in the report even when the work succeeded anyway**, or only
  fatal ones surface and the slow drift never comes back.
- **Name the checks expected to fail**, with their ids, so a known red is not
  rediscovered every unit.

### §4.4 Rulings in force

**Transcribed in full, including what was rejected and why.** Not cited by id.

The rejections are what make it work. A ruling stated as a conclusion invites a
session to find the better answer — and the better answer is usually the one
already rejected for a reason the session cannot see.

Followed by: **do not re-argue either.**

Where the unit's job includes recording the ruling, give the exact text to write
to the decision log, verbatim. **Then the transcription and the record are the
same text** and it cannot be paraphrased into something weaker on the way in.

**A ruling in a work instruction is the owner's, or it is a proposal marked as
such.** A session's own reasoning never appears in the ruling block.

### §4.5 Status cadence

Named in the file as well as in the prompt. See §7.

### §4.6 The tasks

**Task 1 traces before anything is built.** What exists today, answered from the
code, findings reported before a line is written.

> Say what you find rather than confirming this list.

The list in the instruction was written without seeing the tree. A task that lists
five things to check invites a session to tick them off.

Three returns, all evidenced:

- It catches a unit commissioned against a premise that is not true.
- It catches the fix being smaller than the build — *if the path already exists
  and is merely unwired, say so.* That is tokens back.
- It produces the before-number that the report is measured against.

**The exception:** where the unit *is* the measurement, task 1 is the measurement.

Remaining tasks: independently committable, ordered so each is buildable when
reached, every non-obvious instruction carrying its reason inline. A rule whose
reason is forgotten is one the next session talks itself out of.

**Name the drop candidate explicitly.** Not by position — it has not always been
last, and a session assuming the pattern drops the wrong thing. It is dropped
**whole**, and the session says it dropped it. Never half-built.

**A task that weighs two costs against each other is not a task. It is a decision
ask.**

### §4.7 Parked — do not touch, do not raise

Real things that are not this unit's business.

**Both halves are required.** Not touching stops scope creep. Not raising stops
the report filling with things the owner already knows about.

- **Each item says why it is parked** where the reason is not obvious — *both
  real, both panel changes, both their own unit.* That is what stops it being
  raised anyway.
- **Built from the last report's sections 3 and 4**, plus anything the owner has
  said to hold. That gives the list a source so it does not become a permanent
  graveyard.
- **A parked item that turns out to block the task is raised anyway, once, and
  says it was parked.** Otherwise it is silently worked around.

### §4.8 What not to do

Split in two.

**Standing prohibitions** live in `CLAUDE.md` and are cited, not retyped. Do not
run interactive or destructive git. Do not edit another person's files. Do not
invent a ruling id. Do not touch the coverage thresholds.

**Not "do not push."** See §4.9.

**Unit-specific prohibitions** live here, because they only apply now — *do not
re-vendor, `3c1ac48` stands until Tim says otherwise.*

Keeping the unit's own list to the three or four that bite is what makes those
three or four land. Fifteen prohibitions of equal weight are read as none.

**Each says what it protects**, briefly. *Do not let a fan command imply airflow —
`-KBP` reads what the air is doing and is the machine's answer.* A rule with its
reason can be applied to a case nobody anticipated.

### §4.9 Committing and pushing

**A session commits and pushes its work before it reports.** Every task, every
unit, without being told.

Work that is committed and not pushed exists on one machine. It is invisible to
the panel's push check, invisible to a peer project, and lost with the disk. The
owner has had to run `git push` by hand for another session's finished work while
the panel showed `UNPUSHED` — that is the owner doing a session's job.

An earlier version of this file carried *do not push* in the standing
prohibitions above. It was copied from two projects that each had a specific
reason — one where `main` sits behind a pull request and an approval, one where a
single run's commits were deliberately held — and generalised into a rule that
fitted neither. **That was wrong and is struck.**

Where a project genuinely cannot push to trunk, `CLAUDE.md` says so and says what
to do instead, and it wins under §0's specificity floor. **Silence in `CLAUDE.md`
means push.**

The report names the branch and states whether the push succeeded. **A push that
was refused is reported as refused**, with the reason, and is not left to be
discovered by the panel.

### §4.10 Reporting

Names the four sections, and **what leads section 3** — the answer to the question
this unit was commissioned to ask.

Ends: **write `output.md`, then stop.** Do not start the next unit.

**Every exit writes it.** §8's report is written on the way out of every session,
not only a successful one — finished, blocked, failed, or **stopped early by the
session's own judgment**. A unit that blocks, refuses, fails or stops says so in
`output.md` — everything done, everything blocking, every question wanting an
answer — because that file is the only thing that reaches the owner and the next
unit.

**Stopping early is the exit this clause was rewritten for.** A session that
finishes ten of fourteen tasks and judges the rest a natural break has not
finished, and it is not blocked or failed either. It read *if you finish every
task* and *if you cannot finish* and found itself in neither, so it wrote nothing.
There is no third case: the report is written before the session stops, whatever
made it stop.

---

## §5 Delivery

**A scaffolded zip.** Paths relative to the repository root, so the owner extracts
over the root and everything lands where it belongs. This includes documents and
data, not only the work instruction.

**The project name is the first element of the filename.**
`ClaudeProjectStatus-work-instructions-007-2026-08-19.zip`.

Not the contents, not the date. First element, because when several zips sit in
Downloads and several roots are open, the first token is what gets read. The gate
protects the prompt; **the filename is the only thing protecting the extraction.**

**The sequence number is mandatory and monotonic**, between the contents and the
date. Never a word, never a description, never a date alone. **The number is what
tells the owner which order to run them in**, and one named `-tonight-` beside one
named `-456-` tells him nothing. It was in the example above and in none of this
prose until a session shipped both in an evening — §11's *name the artifact*, one
level down.

**A delivery is exactly three things: one zip, one prompt in its own code block,
and the extraction gate.** Nothing else is delivered outside the zip. **Missing
any of the three is a failed delivery**, and it is not complete until all three
are in the same message. A prompt referred to but not reproduced has not been
delivered.

**The zip carries a `MANIFEST.txt`.** See below: a delivery without one cannot be
verified on arrival and the receiver refuses it.

### The manifest

**Every delivery zip carries a `MANIFEST.txt` at its root**, listing every path in
the zip relative to the repository root, one per line, backslashes, **including
itself**. Blank lines and `rem` lines are ignored by the reader.

**A delivery without one cannot be verified on arrival, and the receiver refuses
it.** That is the point of it: the sender declares what it sent, so a zip that is
short a file is *detected* by the receiver rather than assumed away. Nothing else
in the round trip can tell the difference between a file that was never packed and
a file that was never asked for.

**It is written by the sending script, not by hand.** `get-files.bat` enumerates
its own staging folder into the manifest immediately before zipping, so the
declaration cannot drift from the contents — a hand-written list is a second
opinion about what is in the zip, and a second opinion is what this exists to
remove.

### The extraction gate

**Generated from `extract-gate.template.bat`, which is in project knowledge and
is copied verbatim.** Only the four checks, the repository root and the
`Generated` line change — the same three edits, and the same reason, as §1's
harvest script.

It ships **outside the zip** because it runs before there is anything extracted.
The owner puts it in Downloads beside the zip and double-clicks it; it resolves
§4.1's four checks against the root by filesystem, refuses without touching the
zip if they do not all hold, and extracts only if they do.

**It then stamps every extracted file with the receiving machine's clock.** Zip
entries carry the timestamps of the machine that built them, and a delivery built
on a machine running ahead lands with a future mtime. A `WORK_INSTRUCTIONS.md`
dated tomorrow is permanently newer than any report beside it — which left a
panel showing a delivered card whose review control never came alive, on two
projects at once. **The clock is read on the machine that owns the tree**, never
composed and never taken from the zip. The stamping step is part of the canonical
script and is not one of the three edits above.

The four checks are the work instruction's, unchanged. **The same gate runs
twice** — once against Tim's hands before extraction, once against the session's
attention after — and the second is worth nothing without the first, because
§4.1's gate lives inside the file that landed in the wrong place.

This clause replaces the sentence above it. **The filename was the only thing
protecting the extraction**, and §4.1 says in as many words that a name is a
string a reader interprets while filesystem checks are not. The same argument
applies to an owner with eight repositories and two machines open.

**A delivery that cannot be scaffolded says so and names the deviation** — one
spanning several repositories has no single root and must not pretend otherwise.
A delivery with no single root has no gate either, and says that too.

**One writer at a time.** No zip is extracted while a Code session is running. A
Code session verifies `HEAD` is where it was when it read the tree before its
first commit, and stops if it is not. A web session that has delivered treats the
tree as unread until told the commit landed.

**One unit in flight.** A web session delivers one work instruction and **writes no
further unit until the report for it arrives.** That is the paragraph above seen
from the other end, said as an artifact rather than as a state of mind: a unit
written before its predecessor's report is aimed at a tree its author is supposed
to be treating as unread, and its §4.2 number and §4.3 claims rest on measurements
that do not exist yet. Two zips in Downloads and no way to tell which is current is
the owner's problem within the hour.

**A superseded instruction is named in the delivery message**, not only inside the
new file, with one line saying which zip to delete. **A withdrawal buried in a file
the owner has not extracted is not a withdrawal.**

---

## §6 The prompt

In its own code block, containing nothing but the prompt.

```
PROJECT: <name>
Execute WORK_INSTRUCTIONS.md.

Status cadence, for this and every session:

After each task, before starting the next, update PROJECT_STATUS.md per
CLAUDE.md — STATE, TASK: n of m, BALL, UPDATED from the clock, and
NOTE saying what is moving inside the task, not restating the task name.

Do the same every ten minutes while a task is running.

Commit and push each task before starting the next.

Before you stop, for any reason at all, write output.md per
CLAUDE_CODE.md §8. Complete, blocked, failed or stopped early are
all reported the same way and there is no exit that leaves the file
unwritten. If you are stopping with tasks remaining, name them and
say why in section 1.
```

Everything else is in the file. The prompt names the project, names the file, and
carries the two instructions that must stay in front of a running session.

**The report is in the prompt for the same reason the cadence is.** See §7. It was
in §8 and in §4.10 only — a standards file read at minute zero and a work
instruction a session stops consulting around task 6 — and sessions reached the
end of a run with neither in front of them. The one rule whose whole purpose is to
survive to the exit was the one furthest from it.

---

## §7 The status cadence, and why it appears three times

It is in `CLAUDE.md`, in the work instruction, and in the prompt. **The
duplication is deliberate.**

Three projects carried this rule in their own `CLAUDE.md`, read it at the start of
a run, and did not act on it. A session working task 3 is looking at the work
instruction, not at a file it read half an hour ago. **The prompt is the only copy
still in front of a session an hour in.**

- **The fields are named, not referenced.** *Update the status file* is a sentence
  a session satisfies once and considers discharged.
- **`NOTE` says what is moving inside the task, not the task name.** *Task 3* says
  nothing; *rebuilding the AR/IR fixtures, 4 of 11* says it is alive.
- **`UPDATED` is read from the clock, never composed.** A timestamp written into
  the future defeats the one signal that catches a stopped session.

**This is a diagnosis, not a proven fix.** If prompts carry the line and the files
still do not move, the cause is elsewhere.

---

## §8 The report

`output.md` at the repository root, overwritten, and printed to the session.
**Four sections, in this order, no other headings.**

**Writing it is the only way out.** Complete, blocked, failed or stopped, a
session writes `output.md` before it stops. **There is no exit that leaves the
file unwritten** — a session that blocks on task 1 writes it, a session that
cannot do the work at all writes it, a session that decides to stop with tasks
remaining writes it, and a session that is about to ask the owner a question
writes the question into section 4 rather than into the terminal. What is in the
terminal is gone when the window closes, and what is not in the file did not
happen as far as every reader downstream is concerned. **The panel reads the
file, the next unit is written from the file, and the owner is holding neither.**

**One line before section 1**, from the clock per §11, never composed:

```
UNIT: <n> — <exit state> at task <n> of <m> — <YYYY-MM-DD HH:MM>
```

That line is not a fifth heading and does not breach *no other headings*. It is
there because `output.md` is overwritten in place: without it, *this session did
not write the report* and *this is last week's report* are the same file on disk,
and the second is worse — the owner reads a finished unit's findings as the
current one's. `PROJECT_STATUS.md` has `UPDATED` for exactly this and the report
had nothing.

1. **What Claude did.** **Leading with the exit state** — complete, blocked,
   failed or **stopped**, and at which task of how many. That is the fact every
   reader needs first and the one currently inferred from a told `STATE`. Then
   surface, machine, project claimed, what in the tree confirmed it, branch. Then
   what was traced, built and measured, with the numbers. Any decision the
   session made for itself, reproduced in full.

   **Stopped is a real exit state and the one most often unreported.** A session
   that finishes ten of fourteen tasks and judges the rest a separate unit has
   stopped; it has not completed. Stopping may well be the right call — the
   45-to-60-minute window in §2 is a constraint, not a target to overrun — and it
   is reported like any other exit: **which tasks were not done, why stopping
   there was judged better than continuing, and what the next unit inherits.**

   **Where the tasks left undone are not the drop candidate §4.6 named, say so.**
   A session that drops something other than the named candidate has made a
   sizing decision the owner did not make, and that is a decision made for itself
   under the clause above. Report it as one.
2. **What the owner should expect.** What is now true, and **what will look wrong
   but is not.**
3. **What you should see.** See §9.
4. **What's blocking us.** Every question needing a ruling, in the decision log's
   format — ruling, reasoning, what was rejected and why, no id. Most-blocking
   first. **Empty is a real answer.**

Plus, when a unit affects a peer project, a named cross-project section —
`FOR <PROJECT>` — leading with whatever changes a shared interface.

**Section 3 leads with the answer to the question this unit was commissioned to
ask**, with its evidence. Not a summary. The number, or the yes/no.

That closes the loop with §4.2: the unit opens with the number it is aimed at, and
the report opens with what that number became.

---

## §9 What you should see

**User-visible change, in the application, in the owner's terms.**

> The leading edge on the ARRL bulletin goes from 13 of 43 to about 30 — text
> arriving at the radio becomes roughly as readable as the record kept afterwards.

> You can press Emergency Stop and bring the machine back from the panel without
> touching the wire.

Not *the vote window is sized against the measured dit*. Not *fan commands route
through `IMachineLink`*. Not *the render suite passes 26 cases*.

**If a work instruction produces nothing the owner would see, section 3 says so
plainly:** *no visible change — this unit only makes the tests catch a regression
later.* That is then his call on whether it was worth the tokens.

---

## §10 The delivery message

What the web session writes around the zip and the prompt. **A purchase decision,
not a report** — the owner is deciding whether to spend 45 to 60 minutes of tokens
before he spends them.

Four sections, same names as §8.

1. **What Claude did** — the analysis behind this work instruction. What was
   examined, what was found, what it means. This justifies the spend.
2. **What Tim needs to do** — **numbered, physical, in order**, including
   preconditions. *Confirm the simulator is up. Extract over `C:\Source\...`.
   Paste the block. Upload `output.md`.* Every element of the zip that needs
   placing gets named. **The prompt block sits here.**
3. **What you should see** — §9, forward-looking. What will be different when the
   session finishes, and how to check.
4. **What's blocking us** — rulings wanted, drift noticed, housekeeping. This is
   explicitly the low-priority pile, and it is written so the owner can read it
   and know exactly what to say to make it stop being raised.

**This format is for a work instruction delivery only.** Not for questions, not
for conversation, not for a short answer. Applying it there is ceremony.

---

## §11 Recurring failures this standard exists to prevent

Each has happened. Each is cheap to avoid and expensive to find.

- **Composing file content inside nested shell quoting.** Four corruptions in
  three runs, silent at the moment of writing, three landing in files treated as
  the record — including a `TEST_CMD` that could not run. Write the script to a
  file and execute it, or use a file-writing tool.
- **Typed timestamps.** Twice, one thirty-nine seconds in the future.
- **Editing a file whose current version was not pulled this session.** Ruling ids
  assigned from a stale copy, twice.
- **Asserting a tree state from inference rather than measurement.** Three times
  in one afternoon.
- **A listing is a view of the working tree, not of the repository.** A file
  absent from a listing may be committed on another branch.
- **Work committed and never pushed.** A session that stops at the commit leaves
  its work on one machine, invisible to every check and to every peer. The owner
  has pushed a finished session's commits by hand. This file caused it, by
  generalising two projects' specific reasons into a standing prohibition.
- **A rule that names an outcome instead of an artifact.** A peer project lost
  four rounds to *collect the files*; a session satisfied it by inventing a
  script. This document did the same thing in its own §1 on its first day. Name
  the artifact.
- **A gate naming more than one project.** An unsubstituted template went to four
  windows; three sessions reasoned past it and one refused. **The refusal was
  correct.** A gate naming several projects names none.
- **Two units in flight.** A web session wrote a second work instruction before the
  first had reported, against a tree it was supposed to be treating as unread. Its
  numbers and its paths rested on measurements that did not exist. The owner was
  left holding two zips and no way to tell which was current, and the session then
  invented withdrawal language *inside* the file he had not extracted.
- **A session that stopped without writing the report.** A unit that blocks, refuses
  or fails leaves its findings, its questions and its partial work in a terminal
  window that closes. The owner is then holding a card, a commit and nothing to
  read; the next unit is written blind against a tree the last session measured and
  did not record. **The report is the only exit**, and §4.10 said *if you finish*
  for long enough that not finishing read as exempt.
- **A session that stopped by choice and reported nothing.** Repeatedly, across
  projects: ten or fourteen tasks done, tests green, commits pushed, and a session
  deciding the rest is a separate unit. Not a failure and not a block, so neither
  half of §4.10's conditional caught it, and the session that had just made a
  sizing judgment for the owner was the one that recorded nothing. **Stopping is an
  exit state and is named as one**, and the requirement now sits in §6's prompt
  rather than only in a file read at minute zero — §7's argument, applied to the
  one rule that has to survive to the end of a run.
- **A zip extracted over the wrong repository root.** The gate in §4.1 does not
  catch it: the gate lives inside the file that just landed in the wrong place, and
  by the time a session reads it the write has happened. Eight repositories across
  two machines, and §5's answer was a naming convention — the thing §4.1 rejects in
  its own last line. **The gate now runs before the extraction, from outside the
  zip.**
- **A delivery landing with a future timestamp.** Zip entries carry the clock of
  the machine that built them, not the one receiving them. A `WORK_INSTRUCTIONS.md`
  dated tomorrow is permanently newer than any report beside it, and a panel
  comparing the two showed a delivered card whose review control never came alive —
  on two projects at once, for a day, with no error anywhere. **The gate stamps
  what it extracts.**
- **A delivery that could not be verified on arrival.** Four `MISSING` lines and a
  zip built anyway, on 2026-08-27, by the canonical harvest script — whose own
  standard, §1 above, has always said a `MISSING` line is loud on purpose and must
  not be built past. The script printed the rule and broke it, and no zip carried a
  manifest, so the receiving half could not have caught it either: **the count of
  deliveries ever verified on arrival was zero.** A short delivery is invisible to
  everyone downstream, because a zip that is missing a file looks exactly like a zip
  that was never asked for it. **The sender declares what it packed and refuses to
  build a zip over a `MISSING` line.**
- **A template corrected in one project and nowhere else.** Project knowledge is
  one store per project, unreachable and uncomparable from the owner's machine, so
  a fix to a canonical script is a fix in one of five. Nothing detects the other
  four. **The repository copy exists to be checked**, and a session that reads a
  harvested template different from the one it was handed says so.
- **A standard edited from a stale copy.** A session read `CLAUDE_CODE.md` at the
  start of a conversation, the owner advanced it four versions during that
  conversation, and the session then generated two new versions from the copy it
  had first read — each of which would have overwritten the newer file in all five
  roots on distribution. **The version line is the handle and it only works if it
  is re-read**, not remembered from the top of a conversation.
- **A filename without its sequence number.** `-tonight-` and `-456-` shipped in one
  evening. The number was in §5's example and in none of its prose — the same
  failure as naming an outcome instead of an artifact, one level down, and in the
  one element §5 calls the only thing protecting the extraction.
