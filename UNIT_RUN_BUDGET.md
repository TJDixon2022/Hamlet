# Unit run budget - standing rule, owner's order 2026-11-04 (HM-DEC-301)

The owner: units were running three to four hours because the work instructions asked for the
full survey every round, the engine line more than once, and a rule-off survey at the end.
That stops. This rule is cited by every work instruction and is recorded in the tree as a
ruling so that Claude Code refuses a unit that breaks it.

## The budget

- **One full survey per unit**, at the end, after the last round. Never mid-round.
- **One engine line and one app line per unit**, at the end.
- **No rule-off survey.** Code behind an off switch is not re-measured to prove it is off.
- **Per round: the quick measure only** - the boards (twelve-plus-three, W1AW) and the wild
  bank, about five minutes - plus the round's trace. A guard test is run alone, filtered, only
  when the round's change could touch it.
- **At most three rounds**, as before; a round is a build, a trace, a fix, a quick measure.
- **Target: a unit finishes in 60 to 90 minutes.** If the trace shows it will not, the unit
  stops after the current round, measures once, and reports.

## For the writer of work instructions (Claude, in chat)

Every work instruction header carries the line: *"Run budget: UNIT_RUN_BUDGET.md applies - one
survey, one engine line, one app line, at the end; quick measures per round."* A work
instruction that asks for more is wrong and Claude Code says so in `output.md` section 4
instead of running it.

## For Claude Code

If a work instruction asks for a survey mid-round, a second engine line, or a rule-off survey,
do not run it: note the departure in section 4 and follow this budget.

The lock is taken with the root named: `tools\arbiter\lock.bat take C:\Source\HamLet`.
