UNIT:       042 — complete at task 7 of 7 — 2026-08-28 18:41
PHASE GOAL: Digital modes: name the mode and decode it without the operator guessing. Taken from `CLAUDE.md` §2 phase 3, because `PROJECT_CARD.md` carries no phase-goal field — it is the five standing lines HM-DEC-132 rules, and none of them is a goal.
UNIT GOAL:  Hamlet sets whatever the radio needs for a mode, so the operator never has to touch the radio; and CW to Data and back works.
ADVANCED:   yes — the first three stages of an FT8 decoder now exist and are tested: the slot cutter, the Costas sync search, and the receiver setup that makes a block audible at all. Task 1 also removed the blocker.
NUMBER:     none — the order states no figure. Per `CLAUDE_CODE.md` §4.2 it should carry one, and that is a finding rather than an omission to work around: this unit is measured against "he had to press three buttons", which is a count of frustrations and not a scoreboard.
DRIFT:      not carried — the order has no §4.2 block, so there is no count to increment and none to reset. It cannot be recovered from here, because `OUTPUT.md` is overwritten and the previous report is gone.

## 1. What Claude did

**Complete. All seven tasks, including task 7, which the order named as the drop.**

Claude Code on the development computer, project `Hamlet` claimed and confirmed
against the tree, branch `main` throughout. Nine commits, all pushed, none
refused. Version 1.12.4 to 1.12.5.

**Nothing here is evidence about the radio.** No rig was connected. **Nothing
transmitted.**

**No decision was recorded under §12.1.** Section 4 carries what needs a ruling.

**Unit 041's tasks 2 through 7: task 2 and task 3 landed, tasks 4 to 7 did not.**
The order asked for this and it is measured from the tree, not from its report.
`MainWindowViewModel.cs:243` calls `ScheduleModeFollow()` from
`OnOperatingModeChanged`, so a tab press does reach the radio, and
`RigState.ModeWithVariant` renders `USB-D`, `USB` and `USB-?` with a test holding
the third apart from the first. `DigitalCaptureSheet` and the capture press
exist. There is no slot cutter, no sync search and no receiver setup in the tree
before this unit, which is tasks 4 through 7 of 041 not done.

**The order's other claims check out against the tree**, with one correction: it
says the sidecar showed values read up to 32 seconds before the press, which is
true and is the behaviour, not a fault. It has not been weakened.

**One instruction was followed late.** The order says *record the failing counts
from the tree before task 2*. The first full engine run was started during task 1
and returned during task 2; the count is in section 2 and it is a before-count in
substance, since the only change in the tree at that point was task 1's.

## 2. What the owner should expect

**Switching between CW and Digital now leaves the radio set for the mode in both
directions, and tuning into FT8 sets the scope span and the noise controls as
well, telling you in plain words what it changed and why.**

What that reads like on screen, composed from the block's own row:

> I turned the noise blanker off because it chops up the steady tones this mode
> sends, and turned the auto notch off because it hunts steady carriers and
> everything in this block is one. The AGC usually wants to be slow here, because
> dozens of stations transmit together here and the gain would ride up and down
> under the loudest of them, and that is not settled well enough for me to change
> it on your radio. Your scope span wants to be 3 kHz across, because a scope
> showing a couple of hundred kilohertz draws the whole block about seven pixels
> wide, and that is one I cannot set from here.

**Two things in that sentence are admissions and they are deliberate.**

**The scope span is the one you pressed SPAN for, and Hamlet still cannot set
it.** `CLAUDE.md` §4 carries no CI-V command for the span. The sub-command list
on p. 19-7 runs 00, 10, 11, 12, 13, 14, 15, 16, 17, 19, 1A and 1B, and this
project has read the pages for 10 and 11 only. No byte is written that is not
cited (HM-DEC-084), so the span is spoken and not written until one of the others
is read. **That is the one button of the three you may still have to press**, and
it is section 4's first ask.

**The AGC is stated and not written.** Slow is the usual advice and it is not
settled: the argument for fast is that one loud burst on slow holds the gain down
for the rest of the slot. Nobody here has measured it on your radio, so it says
what the mode usually wants and leaves the knob alone.

**What will look wrong and is not.**

The engine suite still shows a red count. **It is the same twenty-eight CW
failures as before this unit, by name**, and the one that changed is a test of
mine that had to change. Nothing new is red.

| | before | after |
|---|---|---|
| engine | 1811 passed, 29 failed | **1929 passed, 28 failed** |
| app | 509 of 509 | **509 of 509** |

The engine count grew by about 120 because this unit added tests, not because
anything was skipped. The one failure that cleared was
`ModeFollowTests.TheWriteIsCommand26WithTheDataFlagAndNoFilter`, which asserted a
single frame on the wire and now asserts three: the write, then the two reads
that ask the radio what it actually did.

**Nothing consumes the slots or the candidates yet.** The sync search is not
wired to a screen and puts nothing on the decoded-text panel. Marking candidates
on the waterfall was skipped: the search takes about two seconds a slot, which is
not cheap enough to run behind a live render, and the order said to skip and say
so.

**`CLAUDE_CODE.md` in the working tree has gone backwards, from version 1.8 to
1.7**, and it is not mine. It is uncommitted and I have left it that way rather
than either committing a governance downgrade or reverting a file the owner may
have placed on purpose. **The next `git add -A` will pick it up**, so it wants a
decision before then. Section 4 carries it.

## 3. What you should see

### Task 1's cause, with file and line, and it is not staleness

**`src/Hamlet.RadioEngine/Explore/ModeFollowPlan.cs`, the "already done" guard.**
The automation remembers the last write it made and declines to repeat it.
**Nothing ever cleared that memory.** Once USB-D had been confirmed at 14.074,
the automation would never establish it there again, however far the radio
wandered afterwards.

The guard's own comment claimed it "can only ever reduce what goes out", and that
was true exactly while the ledger and the memory agreed. **They disagree in one
situation and it is yours**: the radio has left the mode Hamlet set it to. Your
forced re-read corrected the **display**, which is why it read as staleness. **A
re-read does not clear that memory, so the write still would not have fired.**

The order's four candidates, resolved:

| candidate | verdict |
|---|---|
| the write does not fire on the return | no — `OnOperatingModeChanged` reaches the follow path since 041 |
| it fires without the data flag | no — command `26` carries the flag and the frame is asserted byte for byte |
| it fires and is not read back | **partly true, and a second real defect** |
| it fires, is read back, and the ledger serves a cached answer | no — this was the hypothesis and the memory is what refused |

**The second defect is real and was found on the way.** A confirmed mode write
folded its own request into the ledger rather than asking the radio, and said
nothing at all about the filter that the same frame had just changed. So a
widening write left the ledger reporting the old width **for up to thirty
seconds**, which is the session poll interval. That is a stale value shown as
current, and every field on the row looked measured. `Ic7300Rig` now reads
`26 00` and `1A 03` after every confirmed mode write, and the values arrive
stamped with the time the radio answered.

**And a third, smaller one.** `RigState.IsDataMode` was a bare bool answering
false for both "off" and "nobody has said". Against a target wanting the variant
**off**, an unread flag compared equal to it and the automation concluded the
radio was already right without anybody having looked. It is three-valued now,
like every other reading.

### The app suite caught a regression from that fix, and the repair is the better rule

`ASnapBackDoesNotWriteAgain` went red, correctly. **The snap-back and your hand on
the mode knob are the same picture by value**: the ledger says CW, the target says
USB-D, and Hamlet remembers writing USB-D here.

**What separates them is when the reading was taken.** Older than the write means
the radio has not been asked since, which is HM-OPEN-041's snap-back and the
evening that carried eighteen mode writes with the dial standing still. Newer
means the radio was asked and answered CW, so somebody turned the knob. The
memory now carries when the radio confirmed the write, and a reading may only
contradict it if it is newer. A caller that cannot say when it read gets the
cautious answer.

**The readback is what makes that rule bite**, which is why the two fixes belong
together: the ledger is stamped from the radio's own answer immediately after a
write, so a genuine snap-back is now the only thing that can be older than it.

Measured, three ways:

```
read 4 s before the write: write=False
read 4 s after  the write: write=True
no reading time at all:    write=False
```

### The acceptance, against a radio that holds its own state

`FakeSerialPort` answers what a test enqueues, which cannot fail an acceptance
about ten round trips, because the script is the answer. So `ScriptedRadio` holds
mode, data flag, filter slot and four receive-side switches, and answers for
itself.

- **CW at 14.074, press Digital**: USB-D, FIL1, and the ledger's `3000 Hz` comes
  from `CI-V 1A 03` rather than from the request. Nothing acknowledged a number
  of hertz, so a ledger holding one has been told it.
- **Ten round trips**: ten arrivals, ten writes, **and not an eleventh**. Both
  failure modes are asserted, because they are opposite ones.
- **Three of the four new tests fail without the fix.** The fourth asserts the
  write loop the narrowed guard still prevents, and passes either way.

### What the tune-in does, and what it refuses to do

Every condition is read before it is written. Already correct is not sent. A
control you moved yourself is left where you put it, **per control rather than
wholesale**: switching the blanker back on to get through an electric fence does
not hand you the auto notch as well.

**A control the radio will not report is not written.** Without a reading Hamlet
cannot tell an operator who set something deliberately from a radio nobody has
touched, so silence is a stop, as it is for the scanner.

Once per tune-in, keyed by the **block** and not by the dial. Nudging the VFO a
hundred hertz inside an FT8 block is not arriving somewhere new.

### The sync search works, and the floor under it is measured

Three synthetic transmissions in one fifteen-second slot at three times the noise
amplitude:

```
  frequency | starts at | sync score
  ----------|-----------|-----------
    2375.0 Hz |    0.40 s |   7.97
    1112.5 Hz |    0.40 s |   7.86
    1800.0 Hz |    0.40 s |   7.47
```

Eight is the ceiling. **The reporting floor was measured rather than reasoned**:
ten seeds of pure noise reach 1.85 to 2.02, because the sweep tries tens of
thousands of positions and takes the luckiest, so the floor is 4.0 and a test
holds it above what an empty band actually gives. Noise produces nothing.

**Run over a real recording from your radio it finds nothing, which is correct.**
That capture is Morse from 40 m and holds no FT8.

### The window's ambiguity is closed

**The press is the end of the window.** The button keeps audio that had already
arrived, so the window runs backwards from it, and the sidecar now says both ends
in UTC on their own labelled lines. The old `captured` line is gone rather than
kept beside them.

That is not cosmetic. A thirty-second window read from the wrong end is out by
two whole slots, which is invisible in a fifteen-second cycle and fatal to the
alignment — it is why the 2.4-second offset in your 20:47:20 capture could not be
resolved.

## 4. What's blocking us

**What are CI-V sub-commands `27 12` through `27 1B`, and is one of them the scope
span?**

The order's premise is that you were sent to press SPAN and should not have been.
Hamlet now knows the span a block needs, derived from the block's own width, and
says so. It cannot set it, because `CLAUDE.md` §4 records only `27 00`, `27 10`
and `27 11`, and no byte is written that is not cited.

The page is Full Manual `A7292-4EX-6` p. 19-7, which lists the sub-commands and
which this machine does not hold. **A page read and a row added to §4 closes the
last of the three buttons.** Rejected: guessing the sub-command from the pattern
of the others, which is exactly the uncited number §4 exists to prevent, and it
would move your scope on a guess.

---

**Is AGC slow right for FT8 on this radio, or is fast?**

Stated in the data with `confirmed: false` and an owner, so it is spoken and not
written. Slow is the usual advice; the argument for fast is that one loud burst
on slow holds the gain down for the rest of the slot. **Nobody here has measured
it**, and it is a setting on your radio.

An evening with the block audible and both settings tried would settle it, and it
is the kind of thing that decides itself in five minutes at the radio. Rejected:
writing it anyway on the strength of the usual advice, because a setting changed
on a guess is the prime directive broken with a byte instead of a sentence.

---

**`CLAUDE_CODE.md` in the working tree is version 1.7 and the committed copy is
1.8.**

It changed on disk during this session and not by me; the likely cause is a
delivery zip carrying an older copy. The difference is a whole section, *Where the
templates live*, which 1.8 has and 1.7 does not.

It is uncommitted and I have left it so, per §12.6. **Rejected: reverting it
myself**, because a governance file is yours and it may have been placed
deliberately; **rejected: committing it**, because that would silently take the
project's standards backwards. **The next `git add -A` in this repository will
sweep it into a commit**, so it wants deciding before the next unit rather than
after.

---

**The order carries no §4.2 number block and no drift count.**

`CLAUDE_CODE.md` §4.2 requires `PHASE GOAL`, `UNIT GOAL` and `ADVANCES` before
the tasks, and §8 requires the report to carry the drift count forward from it.
Neither is in work instruction 042, so this report's `DRIFT` line says it cannot
be carried, and the count is lost rather than incremented — `OUTPUT.md` is
overwritten, so the previous report is gone and a session cannot recover it.

Not a blocker for the work. It is a blocker for knowing whether the phase is
advancing, which is what those lines exist for.

### Asks still outstanding

**Carried forward per HM-DEC-139 and HM-DEC-140.** The order parks the CW decoder
stream, the CW capture path, the scanner, the calling cycle, `CHANGELOG.md`, the
missing `DECISIONS.md` records, the phrasebook, the recent-places row, the prefix
table, the plain-English parser, the decoded-text panel's placeholders, the mode
strip's static status, the waterfall's rendering and the Twin PBT. **The
thirty-one asks from unit 1.11.34's list stand unchanged.**

**Carried and still open:**

1. **`ft8_lib` cannot be built here** — no C toolchain; the decoder is C#. That is
   now a decision with code behind it rather than a constraint: the FFT, the slot
   cutter and the sync search are all C# and all tested.
2. **The inner PBT's readability** — needs the manual. Both of this project's
   written sources were checked in unit 041 and neither answers it.
3. **The manual is not obtainable from this machine**, so any task needing a fresh
   page read has to be met another way. **This is now blocking real work**, not
   just documentation: it is the scope span, above.

**New this unit:**

4. **The scope span sub-command**, above.
5. **AGC slow against fast**, above.
6. **`CLAUDE_CODE.md` has gone backwards in the working tree**, above.
7. **The order carries no number block or drift count**, above.
8. **There is no digital capture in this repository.** The order says the
   operator's captures in `captures\digital\` hold FT8 keying on a 15.0-second
   cycle and to use one as a fixture. **That folder does not exist in the tree and
   no `ft8-*` file has ever been committed**, so the slot cutter and the sync
   search were tested against a Morse capture, which proves they handle a real
   file and proves nothing about FT8. **One of those three captures committed to
   `tests/fixtures/` is what turns the sync search from arithmetic into
   evidence.**

**Closed this unit:** **why entering Digital did not establish USB-D** — the
automation remembered its own earlier write and nothing ever cleared that memory.
**Whether a confirmed write is a reading** — it is not, and the filter it changed
was going unreported for up to thirty seconds. **Which end of the capture window
the press is** — the end.
