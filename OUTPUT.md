# 1. What Claude did

**STATE.** Development computer, `C:\Source\HamLet`, Claude Code surface (§9.5).
**Branch: `main`, and nowhere else** (§9.5.1). The prompt claimed `PROJECT: Hamlet`,
`WORK_INSTRUCTIONS.md` carries the same gate, and both agree with
`PROJECT_CARD.md`. Gate passed on all three (§9.6).

**On the date: they agree now.** `ISSUED: 2026-08-18` matches this machine's clock,
which reads 2026-08-18. The last `OUTPUT.md` says 2026-08-19 because that order's
own text carried that date, so an `ISSUED` newer than the last report would have
looked stale under a naive comparison. Compared against the clock as instructed,
nothing is stale and nothing stopped.

**The status file was written before the order was read past its opening
instruction**, and again at the finish. One phase, worked as numbered. **No file
under `src/` or `tests/` was changed** — `git status` on both was empty at the
commit.

## HM-DEC-139, written verbatim to `DECISIONS.md` at the head

> **Every session report carries a heading for asks still outstanding, and every
> work order carries the same list inbound. A report or an order without it is
> defective and is redone.** Closes HM-OPEN-044. Supersedes nothing; HM-DEC-106's
> four sections are unchanged and this is a standing heading within the fourth.
>
> AN ASK NOBODY ANSWERED LOOKS EXACTLY LIKE AN ASK NOBODY MADE. `099de5a` changed
> the frequency's cadence and asked for the ruling in its own section four. The ask
> was correct, complete and properly placed. Three sessions then inherited the
> change as settled, and one of them — mine — withdrew a draft of that same ruling
> while the code was already in the tree. §9.5 says a decision not in the record is
> not made, and nothing in the project compared the tree against the record. What
> made it invisible was not carelessness. It was that a section four is read once,
> by one person, on one evening, and then the conversation moves.
>
> SO THE QUEUE CARRIES ITSELF FORWARD RATHER THAN BEING REMEMBERED. The heading
> lists every ask still outstanding, each with the date it was first made, what it
> is waiting on, and where the change it concerns already sits in the tree. It is
> carried forward verbatim by every report until Tim rules, and dropped in the
> report that records the ruling. **The heading appears even when the queue is
> empty and says so**, because an absent heading and an empty queue are the same
> sight, and this project has now twice been caught by a silence that looked like
> a state.
>
> AND THE WORK ORDER CARRIES IT INBOUND TOO, for HM-DEC-137's reason and no other.
> A rule that lives in one channel fails when that channel is written in a hurry,
> and both of this project's channels have now failed in the field. One of the two
> will catch.
>
> IT IS DEFECTIVE RATHER THAN AN OVERSIGHT, which is HM-DEC-099's shape and
> HM-DEC-137's. The failure is one a session cannot detect from inside: a report
> that omits the heading looks complete, and the ask simply stops existing. Holding
> the artifact to it is the only thing that makes the requirement real rather than
> advisory.
>
> THE MARKER AT THE SITE WAS NOT REJECTED, only deferred. A marked assumption that
> a sweep test ages out is the stronger answer, because it survives a session
> forgetting and this one does not. It needs a marker convention and a test, and
> the queue needed to be visible tonight. When the record is healthier it is worth
> taking up.
>
> REFUSING TO SHIP WITHOUT THE RULING WAS REJECTED AND THE COST IS MEASURED, NOT
> FEARED: `099de5a` fixed the display the operator uses more than any other, and
> holding it at the door would have cost him two more evenings of a radio that did
> not track. A queue that is visible is worth more than a gate that is closed.

**Its index row is at the true head of §1**, above the 2026-08-18 HM-DEC-135 row.
The head now reads **139, 135, 138, 137, 134**, so the out-of-order pair the order
told me to leave is still there and now has a correctly-dated row above it.
HM-OPEN-036 stays open and untouched.

**`CLAUDE.md` §12.2** now carries the heading for a session that reads only that
file: what it lists, that it is verbatim until ruled, that it is dropped by the
report recording the ruling, that an empty queue says so in words, and that a
report without it is defective. **§9.6** carries the inbound half and says what a
session does when an order omits it: reconstruct the queue from the record rather
than read the omission as an empty queue.

**HM-OPEN-044 is closed** by that ruling, with the closing note naming what was
deferred and what was rejected.

## The queue, checked rather than copied

Your three, plus one you missed, minus nothing. I read `OPEN_ISSUES.md` and section
four of the last three reports.

**The one I added: whether Hamlet may ever ask the radio to send its spectrum, and
if so when** (HM-DEC-062, HM-DEC-092, HM-OPEN-042). It was asked on 2026-08-18 in
the report that removed the automatic `27 11`, three ways were put, and nothing has
been ruled. It also appears on this order's own named-and-left list as "the
automatic `27 11` at connect", which is the same subject seen from the code side.

**Three I checked and left out, because they have been ruled since they were
asked**, named here once so the queue is not silently shorter than the record:

- the frequency's cadence, ruled last night as HM-DEC-138;
- the record sweep for rulings resting on a write outcome, which you ruled B;
- HM-OPEN-044 itself, ruled by this order.

**And a boundary I drew, which you may want differently.** `OPEN_ISSUES.md` holds
twenty-odd open items owned by you, and most are not asks in this sense: they want
a capture file, a manual page, a station fact. The queue lists questions handed
back for a ruling in a session report. HM-OPEN-007's two favorites questions sit
right on that line, raised on 2026-08-14 and never ruled, and I left them out
because they were raised as record entries rather than handed back in a report. If
you want the queue to mean every unruled question you own, it is a longer list and
I will carry it.

## Where the list now lives

Both channels, as the ruling requires. It is at the top of
`WORK_INSTRUCTIONS.md`, which is committed (HM-DEC-135), so the inbound channel
starts with this order rather than the next one, and it is at the end of section
four below.

# 2. What Tim should expect

- **One ruling, one closed open item, three `CLAUDE.md` edits, no code.**
- **The suite is untouched** and was not run in anger: 1,969 tests, 2 failing, the
  standing decode baseline. Nothing in this delivery could move it.
- **Every report from here carries `Asks still outstanding`**, including when it is
  empty. If one arrives without it, that report is defective and should be sent
  back rather than read around.
- **This order's own copy of the queue is in the tree**, so the next session finds
  it whether or not the next order carries it.
- **The queue is four items and you listed three.** The fourth is the spectrum
  request, and it is the one with code already removed on its account.
- **One commit, pushed to `main`. Nothing local, no branches.**

# 3. What we should do next

- `DECISIONS.md` missing 096 to 133, which your own list calls the largest hole and
  next. The queue makes that hole more visible rather than less: an ask made in a
  report between those numbers has no entry to point at.
- The spectrum question, which is one ruling and would take an item off the queue
  and a widget out of limbo.
- HM-OPEN-036 whenever §1 is opened deliberately.

# 4. What's blocking us

Nothing is blocked. No new question this session; the standing queue follows.

## Asks still outstanding

Four, per HM-DEC-139. Carried verbatim until ruled.

| Ask | First made | Waiting on | Where it already sits in the tree |
|---|---|---|---|
| **Whether an attended automatic cycle may reach an antenna** (§0.2, HM-DEC-098) | 2026-08-17 | Every interlock watched to fire into the dummy load, per `BENCH_CARD.md`, including the link pulled mid-cycle | Built and armed: `AutoCaller`, `AutoCallAnswers`, the widget on the making-contacts preset. Dummy load only until this is ruled |
| **A callsign too long for one keyer send** (HM-DEC-130) | 2026-08-18 | Five minutes at the bench measuring the gap between two sends into the load | Refused, not split. `CwMessage.Split` exists and is unused for this |
| **Whether the star asks for a name at the moment of saving** (HM-DEC-060, HM-DEC-134) | 2026-08-18 | Nothing but the ruling; handed back under §12.1 clause 3 as a trade-off | Favorites are born unnamed from places the operator was. The manage window renames them afterwards |
| **Whether Hamlet may ever ask the radio to send its spectrum, and if so when** (HM-DEC-062, HM-DEC-092, HM-OPEN-042) | 2026-08-18 | The ruling. Three ways were put: leave the switch to the operator, ask once on a button, or ask automatically once the counters show the stream is not eating the link | **Not asked at all.** The automatic `27 11` was removed on 2026-08-18 and HM-DEC-062 restored; the reads of `27 10` and `27 11` stay |

Dropped as ruled since they were asked: the frequency's cadence (HM-DEC-138), the
record sweep for rulings resting on a write outcome (ruled B), and HM-OPEN-044
itself (HM-DEC-139).

---

## Named and left, as the order directs

Not started: `DECISIONS.md` missing 096 to 133; HM-OPEN-036; the record sweep;
the automatic `27 11` at connect, which is already out of the tree and is on the
queue above as a ruling rather than as work; HM-OPEN-042's remaining rungs; mode
follow, favorites and the recent list.
