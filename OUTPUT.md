# 1. What Claude did

**STATE.** Development computer, `C:\Source\HamLet`, Claude Code surface (§9.5).
The prompt claimed `PROJECT: Hamlet` and the tree confirms it: `CLAUDE.md`'s
header reads `Project: Hamlet`, the solution is `Hamlet.sln`, the namespaces are
`Hamlet.*`. Gate passed. **Nothing in this report is evidence about the radio**
(`SHACK_FACTS.md`, HM-DEC-093). This machine has COM1 only and COM1 is a
simulator, so everything below is a fact about a fixture, a generated signal or
a headless window, and none of it has been near an antenna. **Almost all of this
order is verified by your eyes at the screen**, which is why it waited for you;
what could be proved by test was proved by test anyway.

**Nothing was recorded under §12.1.** Two phases reached a question and both
turned out to touch a recorded ruling or a trade-off, which §12.1 puts outside a
session's reach. They are in section 4.

**All seven phases were worked. Phase 7 was not dropped**, and it did not
migrate anything either: it measured, found two thirds of the migration provable
and one third needing your ruling, and stopped there because the brief says a
half-migrated band plan is worse than two whole ones.

## Phase 1, the scanner's face and the stop §0.2.1 requires

`ScanViewModel`, a Scanner widget, and the stop control.

**The stop lives in the pinned strip and that placement is the whole point.**
The strip is outside the canvas, so the control cannot be scrolled away from,
closed, or dragged off the edge. A stop inside a widget is a stop the operator
can lose while the radio goes on being tuned. It appears only while a scan runs,
because a control that stops something that is not happening teaches people to
stop reading that part of the window.

Beside it, in the same strip, Hamlet says plainly that it is moving the dial and
which frequency it is listening at.

The panel carries what the waterfall proposed and everywhere the scan listened,
**including the places it found nobody**. How sure Hamlet is comes from the
engine's own verdict rather than being re-derived, so a call made of dim letters
reads "not at all sure" and cannot be drawn like a solid one. A callsign-shaped
stop still prints no callsign. Six tests.

The scanner reads sweeps the waterfall is already receiving and issues no
command of its own, so having it attached costs the bus nothing.

## Phase 2, the file you are supposed to edit

Written on first run and never touched again, opened from a button in Settings,
which also shows its path. It is a file rather than a row of boxes because boxes
would be Hamlet's list with your numbers in it, and every stretch in the file
carries the source its numbers came from, which a box cannot.

**One finding changed the code.** The refusal for an unreadable file was
unreachable: the radio was checked before the file, so an operator with a broken
file and no radio got nothing, and with a radio he would only find out after
pressing. Whether your scan file can be read is a fact about your configuration
and not about what is plugged in, so it is checked first now. Proved against a
real unreadable file rather than argued about.

`RestoreHomeAsync` is called on connect. The note was being written before the
first tune for exactly that case and nothing had ever read it.

## Phase 3, the counter row, and one word that was wrong

The row is silent while parts arrive, are read, and become sweeps. It appears
when any of that stops being true, in the order the data travels, so the first
zero is still the address of the fault. The numbers moved to the waterfall's
tooltip rather than being deleted with the row: they are what proved the path
was discarding 2,740 parts.

HM-DEC-093's property survives, and is now a test: **a quiet band and a cable
that never spoke still paint different pictures**, because the second is not
healthy and therefore not silent.

**The word `receiving` was wrong, and not in the way the brief expected.** It
was not driven by connection state. It was driven by the cumulative sweep count,
so the first sweep of an evening bought the word for the rest of it: the cable
could come out and the summary would go on saying "receiving" until the app was
restarted. It now measures how long ago the last part arrived and says "nothing
arriving now" when that is what is true. Eleven tests, in the engine, because a
stage count is a radio fact and this fault was invisible for weeks precisely
because nothing measured it.

## Phase 4, the clipping, and the favorites strip

**The clipping is widget-level, not the canvas**, which the brief asked to be
established first. Each widget's body sits in a scroll viewer that constrains
its content to the widget's width, inside a border with a corner radius that
clips to it, so anything that cannot give way at that width is cut at the edge.
The canvas scrolls both ways and clips nothing.

The level meter's fix is structural. Its sentence sat in a third column sized to
its own content, so "audio has stopped arriving" made the row wider than the
widget. That is the line that says the decoder is hearing nothing, which is the
line somebody stares at when the app looks broken. It is under the bar now and
wraps to whatever width there is.

Nine other sentences gained wrapping, the widget body says out loud that it
never scrolls sideways, and a widget refuses to shrink below the same floor the
resize grip already used. A sweep over the window fails on any sentence that can
be silently cut, and **it was checked against an unwrapped line to be sure it
fires**.

**On the favorites strip, neither hypothesis in the brief holds.** Both
dropdowns are populated correctly and no label has detached. What you saw was
the recent dropdown showing a station and the favorites dropdown beside it
showing its placeholder. The two boxes said what they were only through
placeholder text, so the moment you landed somewhere the recent one replaced
"recent" with the station and the pair read as one control with a stray word
next to it. A placeholder is not a label, because a label survives a selection.
Both carry one now.

## Phase 5, the two-stage decode, which nothing rendered

The terminal is one line: settled text, which is what a transcript keeps, with
the leading edge as an italic tail that is replaced as the second pass overtakes
it.

**Where the settled pass has refused, nothing is coming behind the leading edge
and waiting would be waiting forever**, so that reading is committed at once
carrying the mark saying nothing confirmed it. Losing the text would be worse
than showing it marked, and the moment somebody answers a call is the worst
possible moment for the live feed to go dark. The tip cannot grow without limit
either.

The speed field says why it is blank. A speed change and a lost clock are
annotated, because both mean somebody else started transmitting. The window
ceiling announces itself when it binds. The revision log has a way out: it stays
in memory, is never written unasked, and a button beside the count writes every
character the second pass changed its mind about. Six tests.

## Phase 6, the stale rig-state block

**Half of this was already done.** The capture header takes its frequency and
its band from the rig model and says so when the radio was not read; a previous
session fixed it. The sidecars showing 7.030 MHz over a rig block reading 14.055
predate that fix.

The other half stands and its root is one line of the poll plan. **The frequency
is the only field marked never polled**, because the radio broadcasts changes.
Two things follow. Its age means something different from every other field's,
so with nobody touching the dial it recedes without limit and reads as a link
going quiet when it is a link with nothing to report. And a broadcast missed
while the app is starting leaves the model holding a frequency the radio is not
on, with nothing to correct it until the dial is next turned. Mode and filter
are swept anyway for exactly that reason; the frequency was the one left out.

**The chain the brief asked about is real.** The band on screen is derived from
this reading, and the band scopes what RBN is filtered to and what the skimmer
watch listens for, so a wrong one would make "nobody heard you" a defect wearing
the clothes of an answer.

**And a test found the boundary.** Sweeping the frequency would act against
HM-DEC-050's own words, which say nothing the radio volunteers is polled for, so
it is not a session's to overturn. What is done instead is the on-demand read
that ruling explicitly provides for, taken at the two moments the value is about
to be reasoned from: before a capture sidecar is written, and before the
band-scoped spot sources are rebuilt. One command each, and neither is a poll.
An operator's tune still in flight wins over both.

## Phase 7, the band plan, measured and stopped

Measuring turned up a better answer than expected on two thirds of it.

- **Band edges derive exactly.** Every one is the Extra class's own range in the
  cited privileges file, `97.301(b)`, with 80 m being the regulation's 80 m and
  75 m rows joined.
- **CW segments derive exactly, which corrects the record.** HM-OPEN-005 said
  they are convention rather than regulation and do not align with the privilege
  boundaries. They do: each is the union of the ranges carrying Data in the same
  file, `97.305(c)`, to the hertz, with 40 m needing two rows joined because the
  phone segment overlaps the first.
- **The neighborhood file does not cover them**, which was the assumed source.
  Its Morse rows fall short at the top of every band, by 10 kHz on 17 m up to
  230 kHz on 10 m, and 40 m has a hole between 7.040 and 7.050. That is not a
  defect in it: those rows are conventions somebody published and the space
  between belongs to nobody. A CW segment is a regulatory boundary.
- **Jump spots do not derive.** Five of seven are exactly a "CW main street"
  block; 40 m is the QRP watering hole's instead; and 30 m matches nothing cited
  at all, landing on 10.110 where the blocks are 10.103, 10.106 and 10.120.

Choosing the rule changes where a band button lands on three to seven bands,
which weighs cited data against your muscle memory. That is a trade-off, so it
is yours. Nothing was migrated, the measurement is kept as nine tests so the
answer cannot rot, and HM-OPEN-005 goes from `none` to `slows` and from
unassigned to you.

The source verification §4 asks for before re-pointing was not done, and did not
need to be: nothing re-points.

**No transmit work of any kind was done and nothing was built toward auto-CQ.**

# 2. What Tim should expect

- **Build succeeds, no warnings**, engine and app.
- **1753 tests, 9 failing.** 1330 of 1338 pass in the engine, 414 of 415 in the
  app. 45 tests were added this session.
- **All 9 failures were already failing before this session's work** and are the
  same 9 the last report named: `ASignalAtTheWrongPitchIsStillFound` at 400,
  500, 750 and 875 Hz; `ACleanSignalDecodesExactly` at 25 words a minute;
  `AFadingSignalComesBackRatherThanStayingDead`;
  `ItGoesQuietRatherThanInventingLettersInTheNoise`;
  `TheSpeedEstimateFollowsAChangeWithinAFewCharacters`; and, in the app,
  `ClearingTheTranscriptLeavesTheDecoderAlone`. **Nothing regressed.**
- **`BindingHealthTests` passes with every new panel**, so nothing added this
  session has an unresolved binding. That is the check that catches a control
  which renders and does nothing.
- **What you will see that is new.** A Scanner panel on the canvas and on the
  "Listening around" preset. A stop control and a line about the dial in the
  pinned strip, both only while a scan runs. A "Where the scanner may take your
  dial" row in Settings. An italic tail on the CW terminal's text.
- **What will look wrong and is not.** The waterfall's counter row is now
  **absent** while everything works. That is the ruling, not a regression: hover
  the waterfall for the numbers. If it reappears, a stage has genuinely stopped
  and the sentence says which.
- **The scanner has not been run end to end by anything.** Every piece is
  tested, and the phases have never been exercised together against a radio,
  training or real. First run should be into the training radio.
- `%AppData%\Hamlet\scan-segments.json` is created the first time you start the
  app after this. Twenty stretches across seven bands, every one cited. It is
  yours from then on and Hamlet will not touch it again.
- **Nothing was pushed.** Seven commits on `main`, local.

# 3. What we should do next

- Run the scanner end to end against the training radio, which is the first
  thing that exercises the survey, the dwell and the envelope together.
- Rule on the settled pass's character boundaries, still the last §0.0 gap in
  the decoder and still the thing blocking the transcript being trustworthy.
- Rule on the jump spots so `BandPlan` can be retired and the tree stops holding
  two band plans.
- Take the eight pre-existing decoder failures in one pass. Four are the same
  test at four pitches, so it is probably fewer faults than tests.
- Watch what the on-demand frequency read does to the bus over an evening. It is
  one command per spot refresh and one per capture, which should be invisible,
  and "should be" is not a measurement.

# 4. What's blocking us

---
date: 2026-08-17
refs: CLAUDE.md §0.0, §12.1; HM-DEC-048; HM-DEC-107 phase 4
---

**The settled pass may show a character at full strength when the elements were
clean and the boundary decision that produced it was marginal.**

Unchanged from the last report and still the item blocking the most. It is the
last thing standing between the settled pass and being a transcript, and phase 5
of this order has now put that transcript on screen, so what it asserts matters
more than it did a session ago.

Two answers, and both are yours because both decide what the display asserts.
The first is a third measurement, how far the gap that ended a character sat
from the boundary it was judged against, worst-of-three winning as the existing
two do; that changes HM-DEC-048's ruled design. The second is blunter: bar the
settled pass from full strength until the stranger count reaches zero, which
costs it the thing it was built for.

Rejected, and stated so it is not proposed again: leaving it because the numbers
are small. Two wrong letters in eight at full strength is one in four, and on a
callsign he is about to answer the operator could act on it.

---
date: 2026-08-17
refs: CLAUDE.md §0, §0.2.1, §12.1; HM-OPEN-005; HM-DEC-054
---

**Where a band button lands is chosen by one of three rules, and the choice is
not derivable from the data.**

Retiring `BandPlan` needs this and nothing else. Band edges and CW segments were
measured this session and both derive exactly from the cited privileges file, so
two thirds of the migration is provable. Jump spots are the third.

Three rules are defensible and they land in different places. **Take the first
"CW main street" block**, which is what five of the seven current spots already
are, and which moves 40 m from 7.030 to 7.028 and 30 m from 10.110 to 10.103.
**Take the QRP watering hole**, which is what HM-OPEN-005 itself argues for on
20 m against QRP ARCI's 14.060, and which moves all seven. **Keep the current
numbers and mark them editorial**, citing them as Hamlet's own choice rather
than anybody's published convention, which moves nothing and leaves one number,
30 m's 10.110, matching no source at all.

The trade-off is cited data against the operator's muscle memory, which is what
makes it yours rather than a session's. Rejected: migrating the edges and the
segments now and leaving the jump spots in code. That is the half-migrated band
plan the brief names, and it would leave a file and a class each holding part of
one answer.

---
date: 2026-08-17
refs: CLAUDE.md §0.0.1, §12.1; HM-DEC-050; HM-DEC-024; HM-DEC-075
---

**The frequency is swept on the session poll like the mode and the filter, for
the reason already recorded beside them.**

HM-DEC-050 says nothing the radio volunteers is polled for, and the frequency is
the one field that rule still covers. A test pins it, so this session did not
take it.

It has two costs and both were seen. A broadcast missed while the app is
starting leaves the model holding a frequency the radio is not on, with nothing
to correct it until the dial is next turned; the comment beside Mode and
FilterSelection says exactly that and those two are swept anyway because of it.
And the frequency's age means something different from every other field's, so
a sidecar showed it stale at sixty seconds beside neighbours at twenty-seven,
which reads as a link going quiet when it is a link with nothing to report.

It is worth more than a tidier sidecar. The band on screen is derived from this
reading, and the band scopes what RBN is filtered to and what the skimmer watch
listens for, so a wrong one makes "nobody heard you" a defect wearing the
clothes of an answer.

Rejected: leaving it as it is. This session put on-demand reads at the two
moments the value is reasoned from, a capture and a spot refresh, which closes
the consequences that were seen and not the cause. Anything else that reads the
frequency in future gets the old behaviour back, silently.

Also rejected: leaving the staleness display alone if the sweep is taken. Once
it is swept its age means the same thing as everything else's, and no separate
rule about it is needed, which is part of why the sweep is the clean answer.
