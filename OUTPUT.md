# 1. What Claude did

**STATE.** Development computer, `C:\Source\HamLet`, Claude Code surface (§9.5).
**Branch: `main`, and nowhere else** (§9.5.1). The prompt claimed `PROJECT: Hamlet`,
`WORK_INSTRUCTIONS.md` carries the same gate, and both agree with
`PROJECT_CARD.md`; the solution is `Hamlet.sln` and the remote is
`TJDixon2022/Hamlet`. Gate passed on all three, per the ruling written below.

**Both phases worked, in order. No file under `src/` or `tests/` was changed** —
`git status` on both is empty, and the suite stands exactly where the last session
left it. **Nothing was recorded under §12.1**: every entry below is yours,
reproduced verbatim.

**The status file was written first, before the order was read past its opening
instruction**, then at the phase boundary and at the finish.

## Where each row went in §1

All at the **true head**, above the previous head row (HM-DEC-134), nothing edited.
Reading down from the top of the table now:

| Row | Date | Note |
|---|---|---|
| HM-DEC-135 | 2026-08-18 | Placed at the true head as the order directs, which leaves it **above** two rows dated 2026-08-19 |
| HM-DEC-138 | 2026-08-19 | |
| HM-DEC-137 | 2026-08-19 | |
| HM-DEC-134 | 2026-08-18 | The previous head, untouched |

**That out-of-order pair is HM-OPEN-036's own shape and I left it.** The order said
true head and said 2026-08-18, and both were followed; tidying it would be the
in-passing correction that open item exists to prevent. `DECISIONS.md` has the same
sequence for the same reason: 135, then 138, then 137.

## HM-DEC-138 — the frequency's cadence

Written to `DECISIONS.md` at the head, verbatim:

> **The frequency is read on the live poll and stays there. Supersedes HM-DEC-109 on
> this field's cadence and sets aside HM-DEC-050's exemption for it.** The rest of
> HM-DEC-050 stands: rationing a slow shared line is right, and what is set aside is
> one exemption granted in favour of something that is not happening.
>
> THE PREMISE WAS FALSE AND NOBODY HAD MEASURED IT. HM-DEC-050 exempted the
> frequency from polling because the radio broadcasts it, so asking could only ever
> be more stale. Measured on the operator's own radio on 2026-08-19, session
> `6630ee0f`: 5,499 inbound frames in sixty-one seconds, `inboundTransceive` zero,
> `inboundBroadcast` zero, `radioIsBroadcasting` false. **CI-V Transceive is off on
> this radio and Hamlet does not write the operator's settings.** Asking is not the
> more stale option. It is the only one.
>
> WHAT IT COSTS, MEASURED RATHER THAN FEARED. A frequency read is six bytes out and
> eleven back. The link already carried 1,380 commands in that minute and answered
> 1,379. Four reads a second is under seventy bytes on a cable moving eleven
> thousand, for the field the operator looks at more than any other.
>
> REVERTING TO THE SESSION SWEEP WAS REJECTED, and it is the option this ruling
> exists to close. The sweep is what turned the snap-back defect into thirty seconds
> of wrong display instead of one poll: once something put a stale value on screen,
> only the next reading could move it forward. The guard built on 2026-08-19 stops
> that particular write, but a cadence chosen so that the *next* such fault is
> thirty seconds long rather than a quarter of a second is choosing badly on
> purpose.
>
> A CONDITIONAL CADENCE WAS ALSO REJECTED, though `SkipLiveRead` already implements
> it. On a radio that never announces it is the live poll with extra steps, and on
> one that does the broadcast wins the race anyway and costs nothing. What it adds
> is a second mechanism and a decision about which applies — and **push is the thing
> that proved unreliable here.** A display that always asks finds out immediately
> when the radio goes quiet; one that waits to be told finds out two builds later,
> which is what happened.
>
> THE CODE SHIPPED BEFORE THE RULING AND THAT IS ITS OWN FAULT. `099de5a` changed
> the cadence with the ruling requested in that session's report and not given, and
> the next order withdrew a draft of the same ruling while the change was already in
> the tree. §9.5 says a decision not in the record is not made; this ruling makes it,
> and the gap between the two is worth an open item rather than a shrug.

**The code already matches it**, which I checked rather than assumed:
`RigPollPlan.RateFor(RigField.Frequency)` returns `Live`, and
`FrequencyFollowsTheDialTests` pins it. Nothing was changed to comply.

## HM-DEC-137 — the status instruction

Written to `DECISIONS.md` at the head, verbatim:

> **The status-write instruction lives in `CLAUDE.md` and in every Claude Code work
> order, and an order delivered without it is defective and is redone.** A session
> writes the status whether or not the order it was handed says so. Supersedes
> nothing; HM-DEC-132's triggers and fields are unchanged.
>
> THE RULE WAS NEVER THE PROBLEM. §13.2 has carried five triggers since HM-DEC-132,
> including every ten minutes while executing, and consecutive sessions did not
> apply them. One said so directly in its own report: §13 was read, and not applied;
> the order began without a write and crossed two phase boundaries without one. A
> correct rule that nothing carries is indistinguishable from no rule, and the panel
> it feeds showed a working project as dead, which is the exact failure HM-DEC-131
> was written to prevent.
>
> TWO CHANNELS BECAUSE NEITHER HAS HELD ALONE. A rule only in `CLAUDE.md` is read
> once at the start and forgotten across a phase that runs an hour, which is
> precisely the phase the ten-minute write exists for. A rule only in the prompt is
> lost whenever a prompt is written in a hurry, and every order delivered to this
> project had been missing the closing line `ANNUNCIATOR.md` already required of it.
> Both channels have now failed in the field. One of the two will catch.
>
> AND A MISSING LINE IS A DEFECT, NOT AN OVERSIGHT. HM-DEC-099 already takes this
> shape: a prompt without its gate is defective and redone, because the failure it
> prevents is one the session cannot detect from inside. The chat side cannot write
> to disk (`ANNUNCIATOR.md`), so the only thing it can be held to is the instruction
> it hands over — and holding it to that is what makes the requirement real rather
> than advisory.

**`CLAUDE.md` §13.3.1** carries the two channels for a session that reads only this
file, and states that a defective order does not excuse the session: it writes the
status on §13.2's triggers regardless, and reports the missing line.

## HM-DEC-135 — how a work order arrives

Written to `DECISIONS.md` at the head, verbatim as supplied, and **`CLAUDE.md` §9.6**
added after §9.5 with the text the order gives, word for word. The entry runs:

> **A Claude Code work order is delivered as `WORK_INSTRUCTIONS.md` at the
> repository root, and the prompt Tim pastes says only which project it is and to
> read that file and execute it.** Amends HM-DEC-100 on what the pasteable prompt
> contains and supersedes nothing.
>
> THIS IS HM-DEC-106 POINTED THE OTHER WAY. That ruling moved the session's report
> out of the terminal and into `OUTPUT.md`, because reports were being read off
> photographs of a scrollback buffer and a report Tim has to photograph is a report
> he reads less carefully. The inbound half had the same defect and nobody had named
> it: a work order pasted into a prompt box is retyped, is truncated by whatever the
> buffer holds, cannot be diffed, cannot be committed, and is gone the moment the
> window closes. The two files are a pair. Work comes in through one and goes back
> out through the other, both at the root, both in the tree the session is about to
> change.
>
> WHAT THE PASTED PROMPT CONTAINS IS NOW TWO LINES: the gate, and the instruction to
> read and execute. HM-DEC-100 stands otherwise. A delivery is still a single
> scaffolded zip extracted over the root, still never a snippet, still never a file
> Tim places or patches by hand, and `WORK_INSTRUCTIONS.md` rides in that zip like
> everything else.
>
> THE GATE IS IN BOTH PLACES AND THAT IS NOT BELT AND BRACES. HM-DEC-099 requires
> `PROJECT: Hamlet` on every prompt and every work order, and a one-line prompt makes
> the failure it guards against worse rather than better: pasted into the wrong
> repository, "read `WORK_INSTRUCTIONS.md` and execute it" finds that project's file
> and executes somebody else's work order, with a gate that agrees with itself the
> whole way down. So the prompt carries the gate, the file carries the gate, and the
> session checks both against `PROJECT_CARD.md`. Any of the three disagreeing stops
> the session.
>
> AND IT CARRIES THE DATE IT WAS ISSUED, because a file at a fixed path is a file
> that can be read twice. `WORK_INSTRUCTIONS.md` is overwritten whole per work
> order, in the manner of `PROJECT_STATUS.md`, so a session opening one older than
> the last `OUTPUT.md` is looking at work already done and stops. A pasted prompt
> could not be stale; a file can.
>
> IT IS COMMITTED. The work order that produced a commit is worth having beside it,
> and a session that wants to know why the last one did something has the
> instruction it was given rather than an inference from the diff.

## HM-OPEN-044 — the gap HM-DEC-138 names

Nothing in `OPEN_ISSUES.md` held it, so it is raised: **a change ships carrying a
ruling request, the request goes unanswered, and the next session inherits the
change as settled.** `099de5a` is the case that names it, and the asking was done
properly — section 4 carried the ask, which is exactly what makes it invisible from
inside any single session. It is HM-DEC-113's shape with nobody at fault.

The entry sets out three ways it could be closed and rules none of them: a standing
report section for asks still outstanding; a marked assumption at the site that a
test can age out (§12.4's shape); or refusing to ship a change that needs a ruling,
which is strictest and would have cost the operator a working display for two
evenings. **Yours to rule.**

## One thing I did not reconcile

**`WORK_INSTRUCTIONS.md` says `ISSUED: 2026-08-19` and this machine's clock says
2026-08-18, late evening.** Every date I wrote is the one your text gives, verbatim,
and every `UPDATED` in `PROJECT_STATUS.md` is read from the clock as §13.1 requires,
so the two disagree by design rather than by my hand. Said rather than reconciled
(§12.4).

# 2. What Tim should expect

- **Three rulings in `DECISIONS.md`, one open item, two `CLAUDE.md` sections, and
  no code.** `git status` on `src/` and `tests/` was empty at both commits.
- **The suite is untouched**: 1,969 tests, 2 failing, the standing decode baseline
  (`ClearingTheTranscriptLeavesTheDecoderAlone`,
  `TheBulletinDecodesToItsAnswerKey`). Run to confirm nothing moved, not because
  anything here could have moved it.
- **`DECISIONS.md` now holds 134, 135, 137 and 138 and still has no entries for 096
  to 133.** That gap is on your named-and-left list and I did not touch it, so the
  file reads as four modern entries on top of a run ending at 095.
- **§1's head is 135, 138, 137, 134**, with 135 dated a day earlier than the two
  below it. That is the order's instruction followed literally and it is HM-OPEN-036
  in miniature.
- **Two commits, pushed to `main`. Nothing local, no branches.**
- **HM-DEC-136 does not exist** and nothing in this delivery refers to it as
  anything other than deliberately absent.

# 3. What we should do next

- HM-OPEN-044 wants a ruling, and the cheapest of the three is a standing section in
  the report for asks still outstanding. It costs one heading and would have caught
  `099de5a` on the next session rather than the third.
- HM-OPEN-036, whenever §1 is opened deliberately: the head now carries a visible
  example of what it describes.
- `DECISIONS.md` missing 096 to 133, which is the largest remaining hole in the
  record and is still on your list rather than mine.

# 4. What's blocking us

Nothing. One ruling wanted, and the standing pair unchanged.

---
date: 2026-08-19
refs: HM-OPEN-044, HM-DEC-138, HM-DEC-113, CLAUDE.md §9.5, §12.2
---

**How a shipped change that is waiting on a ruling gets back in front of you.**

HM-DEC-138 has now ruled the one case, in favour of the code, and the mechanism
that let it sit for three sessions is untouched. §9.5 says a decision not in the
record is not made, and nothing compares the tree against the record.

Three ways, none of them a session's to choose:

- **A standing section four heading for asks still outstanding**, carried forward
  until ruled. Cheapest, and it makes the queue visible without changing what
  anybody may build.
- **A marker at the site**, in §12.4's manner, that a test sweeps and fails on after
  some number of days. Strongest, and it puts the record's health in CI.
- **No shipping without the ruling.** Strictest, and it would have left the operator
  with a display that did not follow his dial for two more evenings.

Rejected: leaving it to sessions to remember, which is what has been happening.

---

The two standing questions are unchanged and still yours: **whether an attended
automatic cycle may reach an antenna** (§0.2, HM-DEC-098), awaiting the interlocks
watched into the load; and **a callsign too long for one keyer send** (HM-DEC-130),
refused until the seam between two sends is measured into the load.

---

## Named and left, as the order directs

Not started: HM-OPEN-042's remaining rungs; the record sweep for rulings resting on
a write outcome; `DECISIONS.md` missing 096 to 133; HM-OPEN-036; mode follow,
favorites and the recent list. **HM-DEC-136 was not written.**
