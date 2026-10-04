**PROJECT: ClaudeProjectStatus**

# PHASE_UPLIFT.md

**How a project brings its copy of this layer up to date, one capability at a
time.**

This document is the other half of `tools\arbiter\layer-check.bat`. **The check
names the gap; this names the fix.** Between them a project fixes itself without
asking anyone — which is the whole of the owner's ruling of 2026-09-01.

**Written by unit 060, 2026-09-01.** A copy of this document in another project
is only as current as the day it was copied; see §1.

---

## §0 Run the check first, and any time a card looks plausible and wrong

```
tools\arbiter\layer-check.bat
```

**Exit `0`** every capability present. **Exit `1`** something is absent, and
each `ABSENT` line names the section below that fixes it. **Exit `2`** the
directory is not a project of this layer.

**Run it now, run it after any delivery, and run it the moment a card looks
plausible and wrong.** That last one is what this exists for. A peer project ran
**eight work instructions** on a launcher missing one capability; its card read
`step 2 of 7` while the phase was on step 3 or 4. Nothing was broken visibly.
**Nothing told anyone**, and each of the three attempts to fix it was a support
round trip to another repository.

`reload.bat` prints the same summary, so an arbiter authoring the next unit sees
it without being told to look.

**`UNCHECKABLE` is not a gap.** It means the file that capability lives in is
not in this project — a monitored project has no `app\PROJECT_ANNUNCIATOR.html`
and is not missing anything by not having one.

---

## §1 What this document cannot tell you

**It cannot tell you it is out of date.** If the layer gained a capability after
the date at the top, neither this document nor your copy of `layer-check.bat`
knows about it, and the check will report green while you are behind.

`layer-check.bat` prints one local signal: if your `CLAUDE.md` §1 carries a
ruling **newer** than its capability list, it says `LOOK`. That is a hint and
not a verdict — a ruling can be recorded without adding a capability, and a
capability can be added with no ruling at all. **Nothing here reaches another
repository to find out, by design**, because a project must be able to run this
alone.

---

## §2 What is canonical and what is yours

`tools\arbiter\*` and `app\PROJECT_ANNUNCIATOR.html` are **canonical** — take
them whole, do not edit them locally, and if you need a change, ask for it
upstream rather than diverging. `CLAUDE.md`, `PROJECT_CARD.md`,
`PROJECT_STATUS.md`, `PHASE_PLAN.md` and your rulings are **yours**.

**A locally edited canonical file is the case this layer cannot help you with**:
`layer-check.bat` will report `PRESENT` on a capability whose call site you have
since broken, because it reads and never runs.

---

## §3 `reload.bat`

**What it is.** One command that measures what an arbiter must read before
authoring, and **leads with what disagrees** — `RULES_AT` against `CLAUDE.md`,
the working tree against `HEAD`, `PHASE_OUTCOME.md`'s header against its
entries, and the presence of every file `PHASE_CONTROL.md` §3 requires.

**The fix.** Copy `tools\arbiter\reload.bat` whole.

---

## §4 The launcher

**What it is.** `tools\arbiter\run-phase.bat` — the loop that authors a unit,
runs it, judges it and records it.

**The fix.** Copy it whole, with its siblings: `run-unit.bat`,
`run-unit-watched.bat`, `arbiter-tools.txt`, `run-unit-tools.txt`.

**Do not edit it while it is running.** `cmd.exe` reads a batch file by byte
offset; a file edited mid-run resumes at an offset that no longer falls on a
line boundary and executes the middle of a line. A unit that needs to change it
**prepares the change and reports it**; the owner applies it in a standalone
session.

---

## §5 The step-state writer — `:phasesteps`

**What it is.** After each successful `outcome-append`, the launcher copies the
step states from `PHASE_OUTCOME.md`'s header into `PHASE_STATUS.md`'s `STEP:`
lines, and sets `CURRENT_STEP` to the lowest step that is not `done`.

**Why it matters, and it is the reason this document exists.** Without it
nothing writes those fields by machine. The executor writes them by hand
mid-unit, `ARBITER.md` §5 forbids the arbiter, and the state judge produces the
verdict and writes nowhere — **so the card is one judgment stale at every step,
and at the end of a phase permanently stale**, because the last unit's judgment
happens after that unit has exited. **This is what made a peer's card read
`step 2 of 7` for eight units.**

**The fix.** Take `:phasesteps` from `run-phase.bat` whole, and its call site at
stage 5a — after `outcome-append`, guarded on that append having succeeded,
because the outcome header is the authority and a failed append means the
authority did not move.

**Where every step is done**, `CURRENT_STEP` is set to the **highest** step
number. A finished phase still has a position, and a field naming no step costs
the face its position line.

---

## §6 The heartbeat — `:heartbeat` and `:heartbeatclear`

**What it is.** The launcher writes `HEARTBEAT: <clock>` into `PHASE_STATUS.md`
each time it acts, and **removes it when it halts**.

**Why both halves.** The card reads the beat to say `loop turning` or `loop
stopped`. Leaving the last beat behind would have the card claim the loop is
turning for up to an hour after it stopped; **absent renders as stopped and
never as turning**, so removing it is the honest act.

**The fix.** Take `:heartbeat` and `:heartbeatclear` whole, with their call
sites: after the reload, either side of the arbiter, either side of the watched
run, at the record, and `:heartbeatclear` at `:stopped`.

**The insert rule matters and is not optional.** Replace an existing
`HEARTBEAT:` line in the header in place; otherwise insert it immediately above
the **first `^STEP:` line**; **never append**. A key found below the `---`
terminator is collected as stranded and the whole file reads unreadable, which
takes the entire phase region off the card. **The anchor is `^STEP:` and not the
substring** — `CURRENT_STEP:` contains `STEP:` and comes first.

---

## §7 The scratch path — `:scratch`

**What it is.** `.run-unit\scratch\` is the one permitted scratch location, and
the launcher clears it at the start of every iteration.

**Why.** A unit can rely on finding it empty, and no unit has to decide whether
to delete a predecessor's litter. It is gitignored.

**The fix.** Take `:scratch` and its call at the top of `:iterate`, and add
`.run-unit/scratch/` to `.gitignore`.

---

## §8 The two judges — `:judgestate` and `:judges4`

**What they are.** After a unit runs, a session judges **what state the step is
now in** against the plan's exit criteria, and a second judges **whether the
report's section 4 actually wants a ruling**.

**Why.** A unit's own report is a session's account of itself, and sessions have
been wrong about themselves in both directions. The state a step is left in is a
judgment and is made by something other than the session that did the work.

**The fix.** Take `:judgestate`, `:judges4` and their prompt writers whole.
`:judgestate` must be handed **the step's exit criteria** — its machine-readable
line and its prose section — or it is judging on the report alone.

---

## §9 `ADVANCES` — `:noadvances`

**What it is.** An authored decision block must carry an `ADVANCES` field naming
the step and the exit criterion the unit moves, or say `none` and what it
clears. A block without one **does not run**.

**The fix.** Take `:noadvances` and the check that reaches it.

---

## §10 The step-list form

**What it is.** `PHASE_PLAN.md`, `PHASE_OUTCOME.md` and `PHASE_STATUS.md` all
use one step-line form — `STEP: <n> | ...`, anchored at the line start, upper
case, **case-sensitive** — and the launcher's counter matches exactly that.

**Why.** Two patterns for one concept in one line of code produced **three
halted nights**: a loose matcher counted the sentence *the second step defines
where each lands* as the plan's entire step list and reported the phase
satisfied.

**The fix.** Convert your `PHASE_PLAN.md` block to `STEP: <n> | <what it
delivers>`, and take the counter and the plan-text extractor from
`run-phase.bat`. **Both, in one commit** — the counter alone takes planned to
zero and the loop halts immediately as satisfied.

---

## §11 `status-check.bat`, and calling it

**What it is.** One script that validates a status file **where it is written**:
`UPDATED` present, parseable and not ahead of the clock; every required field
present; `STATE` one of the five terms; no duplicate keys; no empty required
value; no mojibake; transport named.

**Why.** A bad field caught at the panel is caught an hour later by someone who
has to go looking. Caught in the unit that wrote it, it is attached to that
unit's report.

**The fix.** Copy `tools\arbiter\status-check.bat`, **and wire it** — stage 4d
of `run-phase.bat`, before `outcome-append`, with its verdict carried into the
outcome entry on `FATE`. **A failing check does not halt the phase.**

**Both halves or neither.** A project can have the script and never call it, and
`layer-check.bat` reports those separately for exactly that reason.

---

## §12 `readkey.bat` and the tolerant readers

**What it is.** One reader that pulls a `KEY:` value out of a `.md` **whatever
transport the file arrived in** — CRLF, LF, a bare CR, or a byte-order mark —
and says on stdout when it had to normalize.

**Why.** `findstr /b` fails in two complementary ways that between them cover
every line of a file: in a **CR-only** file it finds only the first field,
because the whole file is one line to it; in a **BOM'd** file it fails on the
first field only, because three bytes sit in front of the key. A reader that
reports a missing field about a file that has one sends someone to fix what is
not broken.

**The fix.** Copy `tools\arbiter\readkey.bat` and convert every `findstr` read
of a `.md` to it: `rules-at.bat`, `watchdog.bat`, `return-package.bat`,
`run-unit.bat`, `outcome-read.bat`, `outcome-render.bat`.

**`%~dp0` must be captured before any `shift`.** `shift` moves `%0` too, so
afterwards `%~dp0` resolves to the *caller's* directory and the sibling goes
missing — and the script then reports a missing field about a file that has one,
which is the fault it was being changed to fix.

---

## §13 CRLF for `.bat`

**What it is.** `.gitattributes` pinning `*.bat text eol=crlf`.

**Why.** `cmd.exe` resumes a running batch file by byte offset, and with LF-only
endings that arithmetic drifts: a large file **jumps to the wrong label**.
Measured — a launcher left the reload check and arrived at `:noadvances`,
refusing a unit for a missing field before any arbiter had been asked. It cost
three units of wrong diagnoses.

**The fix.** Add `*.bat text eol=crlf` to `.gitattributes` and normalize the
working copies.

---

## §14 The panel reads per field, and tolerates transport

**What it is.** `app\PROJECT_ANNUNCIATOR.html`: every field parses
independently, one bad field costs **that field's signal and nothing else**, and
the card refuses entirely only when nothing at all can be extracted. Line
endings and a byte-order mark are normalized on read, and the fact is reported
on the card.

**Why.** Four faults in three days each produced the same outcome — the card
refused the whole header and the owner was left with nothing, while the state,
the task, the ball and the instruction number sat readable in the same file.

**The fix.** Take `app\PROJECT_ANNUNCIATOR.html` whole. **Only the project that
holds the panel has this**; for everyone else `layer-check.bat` reports
`UNCHECKABLE`, which is not a gap.

---

## §15 The loop reading, and the blink

**What it is.** The card reads the launcher's `HEARTBEAT:` and says `loop
turning` or `loop stopped`, and the current step's segment blinks — **except
when every step is done**, because a finished phase is not working on anything.

**The fix.** Part of the panel; take it whole. See §14.

---

## §16 What this layer does not do

It does not run your suite, judge whether your project is right, or repair
anything. `layer-check.bat` **reads and reports**. `reload.bat` writes only its
own output file. The panel opens project folders read-only.

**And none of it can tell you a capability was added after your copy was made.**
That is §1, and it is the limit you should assume is biting whenever a card
looks plausible and wrong.
