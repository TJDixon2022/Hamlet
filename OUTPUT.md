# 1. What Claude did

**STATE.** Development computer, `C:\Source\HamLet`, Claude Code surface (§9.5).
**Branch: `main`, and nowhere else** (§9.5.1). The prompt claimed `PROJECT: Hamlet`
and the tree confirms it: `CLAUDE.md`'s header reads `Project: Hamlet`, the
solution is `Hamlet.sln`, the remote is `TJDixon2022/Hamlet`. Gate passed.
**Nothing in this report is evidence about the radio** (HM-DEC-093) — nothing was
connected, and where a phase needed a radio to finish, it says so rather than
reasoning past it.

All six phases were worked. Six commits, all pushed to `main`.

## Phase 1 — the cycle is in the build now, and three things had hidden it

**HM-DEC-113 was not the cause.** The code is on `main` and always was. Three
separate faults produced one symptom:

- the widget was on **no preset at all**, so it was reachable only from the tray,
  by somebody who already knew it existed — which is HM-DEC-072's own shape,
  ruled and built and never invoked;
- `Directory.Build.props` still read `1.9.0`, last touched the day before the
  cycle landed, so the running build's version could not tell him whether he had
  it;
- the tray name and the ruling's name did not match.

The cycle is on the making-contacts preset, appended so nothing already there
moves under somebody who has learned where it sits, and the version is `1.10.0`.
Recorded as HM-OPEN-040, closed.

## Phase 2 — the cycle and the recent list have a record

Nine events, all built in `AppEvents`, which is still the only site that builds a
payload (HM-DEC-019). The privacy walk grew with them and passes.

- **The cycle**: armed with its rounds and interval, started, each round by
  number with the frequency, and stopped with the reason as a **stable token**
  rather than a display string (HM-DEC-077). The message goes in **by its length
  and never its text**, because it carries the operator's own callsign every time
  (HM-DEC-018).
- **The recent list** (HM-OPEN-039, written and closed): the entry written with
  its frequency and whether it was named; the fold with both frequencies and the
  gap; the dwell not met and by how much; the drop off the end of ten; and the
  removal.

The dwell tracker now hands back the place just abandoned and how far short it
fell, because **a list that stays empty while somebody sits still looks identical
to a broken one** (§0.0.1).

## Phase 3 — HM-DEC-134, written to `DECISIONS.md` at the head and built

The ruling went in verbatim as delivered. What was built:

- **A return counts on the entry that is there.** The six qualifying dwells from
  session `9f9d23eb` are the test fixture, and they leave three places: 7.047
  once, 7.030 twice, 7.059 three times, six visits across three entries.
  HM-DEC-072's two hundred hertz is untouched, and the newest visit's
  identification still wins including when it is empty.
- **The return is said, not counted** (§0.7): "you have been back here", then
  "you keep coming back to this one". A "3" beside a frequency is a number the
  operator has to interpret.
- **Removal, per entry and whole**, persisted with the count. A profile written
  before the ruling has no count, absent reads as one, and nothing is migrated
  because nothing is lost.
- **Removal is not a correction to the record**: a place visited again afterward
  comes back counting from one.

Twelve engine tests, all passing.

## Phase 4 — the favorites controls are reachable, and the answer arrived through a failure

**Radio → Manage favorites… exists, opens, and every binding in it resolves**,
which is now measured headlessly rather than assumed. So the favorites half is
not the fault, and HM-DEC-134's removal was new work rather than a regression
hunt.

**The first build of the forget button was wrong and the test is what said so.**
It went inside the recent dropdown's row template, where the containers live in a
popup with its own visual root: the test could not reach the button at all, and a
control a test cannot reach is a control whose deadness nothing can report
(HM-DEC-087). So it moved out — the strip offers **"forget this place"** beside
the box, absent rather than grey where there is nothing to forget (§0.5.1); the
manage window carries a **forget** beside each star, which is the remove
favorites have had since HM-DEC-060 and recent never inherited; and the whole
list goes from the Radio menu. The strip test presses the button and checks the
list.

## Phase 5 — mode follow wrote at a dial nobody was touching

**The decision asked the radio and never asked itself.** `Decide` refused only
when the rig already reported the target mode, and an unread field reads as
not-in-that-mode **on purpose**, because HM-DEC-056 wants an unknown data setting
to be a reason to write. So every tick that found the mode unread looked like a
fresh arrival at a neighborhood, and eighteen writes went out.

The plan now also remembers the last write **the radio confirmed**, with the
frequency it was made at, and will not repeat it. Nothing writes where the old
test would have refused, so the memory can only take writes away; a band change
or the operator's own hand clears it. A fixture where nothing changes now
produces exactly one follow.

**The `send_buttons_enabled` contradiction was a catch-all.** Four states fell
through to `already_transmitting`, so a refusal on the operator's license
recorded the radio as busy while the state field one column over said
`OutsidePrivileges`. Every state names its own branch now, and a test proves no
two share a token.

**What triggers the recomputation is still unseen** and needs a connected radio.
HM-OPEN-041 records it, and names `recent_dwell_short` as the instrument: a dial
that is not moving files no near misses.

## Phase 6 — not dropped, and rung one of the ladder had never actually been answered

**The readback that confirms every setting write was waiting for an
acknowledgement the radio does not send.** After the write and its `FB`, Hamlet
reads the setting back, because an acknowledgement says the radio understood the
frame and not that the setting moved (HM-DEC-084). That read was issued with **no
expected response command**, and with none the dispatcher completes only on `FB`
or `FA` — while a read is answered with the value frame. Every readback timed
out. Every write the radio took was reported as unanswered.

**So `27 11` failing `noanswer` on six connects is not the measurement it looked
like**: under that code, a successful write looked exactly the same. And
`NoAnswer` covered two different facts, which now separate — `no_answer` is
silence, `read_back_disagreed` is the radio saying yes and then reporting the
setting still off, which is the ladder's second rung and was invisible.

**HM-DEC-092 saw this from the other end and attributed it elsewhere**: five
settings written one evening, all five reported unanswered, at least two actually
in effect, read as the link dropping commands. The unanswered counters are worth
having and were not the fault.

Recorded as HM-OPEN-042. Whether `27 11` now confirms is a question only the
radio answers.

## Recorded under §12.1

**Nothing.** HM-DEC-134 is Tim's ruling, transcribed verbatim. HM-OPEN-039,
HM-OPEN-040, HM-OPEN-041 and HM-OPEN-042 are open items rather than rulings.

## Why the status file did not fire

Asked directly, so answered directly: **§13 was read, and not applied.** It was
read and acted on in the session that wrote it — that session wrote `EXECUTING`
and then `COMPLETED`, which is why the file said `COMPLETED` — and this work order
began without a write and crossed two phase boundaries without one. Nothing in
§13 says anything other than what you think it says: it names the start of work
and each phase boundary explicitly. The rule did not fail; it was not carried into
the next work order's loop. It has fired at every boundary since, and at the
finish.

# 2. What Tim should expect

- **Build succeeds, no warnings.**
- **1,932 tests, 2 failing, and both are the standing baseline** —
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone` and
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`. Neither is touched by this
  work; both are the decode-side failures HM-DEC-114 deliberately left red.
- **The version reads 1.10.0.** If the app still says 1.9.0, the build did not
  come from this tree.
- **The calling cycle is on the making-contacts preset**, at the bottom right. A
  layout he has already saved is his and will not gain it — that is HM-DEC-086
  working, not a fault. Loading the preset afresh brings it.
- **The recent dropdown now says when he has been back**, and a **forget this
  place** button appears beside it only when where he is is in the list.
- **Radio → Forget where I have been** clears the list. Manage favorites carries
  a forget on each recent row.
- **Mode follow will be much quieter.** If it goes silent where he expects a
  change, the memory is per frequency and per target, so tuning elsewhere or
  changing band re-asks. That is the intended shape; a silence that survives a
  band change is a defect and worth reporting.
- **Setting writes may start reporting success where they used to report
  unanswered.** That is the phase 6 fix. It is not the radio behaving differently.
- **Do not read anything here as evidence that the waterfall works.** No radio was
  connected, and the frame count is the only thing that settles it (HM-DEC-093).
- **Six commits, all on `main`, all pushed. Nothing local, no branches.**
- Four new open items: HM-OPEN-039 (closed), 040 (closed), 041, 042. `OPEN_ISSUES.md`
  now runs to 042.

# 3. What we should do next

- **Connect the radio and watch two lines of the record.** `scope_output_requested`
  now says which rung of FACT-003 it died on, and a run of `recent_dwell_short`
  with nobody touching the dial names phase 5's unseen trigger. Both are one
  evening and neither needs a change first.
- The dummy-load evening for the calling cycle, per `BENCH_CARD.md`, now that the
  cycle is reachable and every stop reason is in the record.
- HM-OPEN-042's remaining half: whether `27 11` confirms, then rungs three to five
  of the ladder — frames received, parsed, drawn.
- HM-OPEN-036, §1's head ordering, as one deliberate move.
- **`DECISIONS.md` holds HM-DEC-001 to 095 and then HM-DEC-134.** Everything from
  096 to 133 is indexed in `CLAUDE.md` §1 and has no entry in the decision file.
  That is a gap in the record rather than a gap in the rulings, and it is stated
  here rather than filled in on the way past (§12.6).

# 4. What's blocking us

Nothing this session produced is blocked. Two questions, and the standing pair
unchanged.

---
date: 2026-08-18
refs: HM-OPEN-042, HM-DEC-092, HM-DEC-084, FACT-003
---

**Every setting write in the application has been reporting its outcome wrongly,
and the rulings written on top of those reports are worth re-reading.**

HM-DEC-092 was ruled on an evening where five writes reported unanswered and two
had taken effect. The cause was taken to be the link dropping commands, and the
unanswered counters were built on that reading. The actual cause was the readback
waiting for a frame the radio does not send, which made a successful write
indistinguishable from a silent one.

Nothing in HM-DEC-092 is withdrawn by this: the counters are right, the panel
saying what it has and has not had is right, and the waterfall is still dark. But
**the number of unanswered commands that ruling reasoned from was measuring
something else**, and any conclusion drawn from a write that reported
`NoAnswer` before today should be re-taken rather than inherited.

What is wanted is a ruling on whether that re-reading happens now or on the next
evening at the radio, since one connect settles what months of argument cannot.

---
date: 2026-08-18
refs: HM-DEC-134, HM-DEC-072, §0.7
---

**Whether the star should ask for a name at save time is still unruled**, and
phase 4 was told to hand it back rather than choose (§12.1 clause 3).

Nothing about it blocks anything. It is here so it is not lost: favorites are born
unnamed from places he was, the name is what makes one findable a week later, and
asking at save time costs a keystroke at exactly the moment somebody is busy
listening to a station.

---

The two standing questions are unchanged and still yours: **whether an attended
automatic cycle may reach an antenna** (§0.2, HM-DEC-098), awaiting the interlocks
watched into the load; and **a callsign too long for one keyer send** (HM-DEC-130),
refused until the seam between two sends is measured into the load.
