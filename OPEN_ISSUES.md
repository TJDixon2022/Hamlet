# Open issues

Questions with owner and severity. `owner` is who must act next. Format in
`CLAUDE.md` §3.

---
id: HM-OPEN-049
status: open
owner: tim
raised: 2026-08-19
severity: slows
blocks: nothing; the test has been red since the day it was written
refs: HM-DEC-115, HM-DEC-114, tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/CwFarnsworthTests.cs
---

The ARRL bulletin capture has **not** degraded. It reads today exactly as it read
the day the test was written, and the claim it is measured against comes from
somewhere else.

**Measured rather than bisected, because the history answers it directly.**

| When | Recorded | Reading |
|---|---|---|
| 2026-08-17, `2ec922f` — the test is written | "36 characters against 47" | red from birth |
| 2026-08-17, `95de0a3` | "unmoved: 36 characters against 45" | T read as A twice, dropped letters in EACH, MESSAGE, HANDLING |
| 2026-08-18, `d033e7c` | "30 of 44 correct" | a different metric: aligned against the key rather than counted |
| 2026-08-19, today | **36 characters against 47** | `NL DOT NET ■I ECH STAAION HAND■ AHIS MESAGE P` |

**There is no regression to find.** The count today is the count on the day the
test was introduced, and the intervening numbers moved in both directions because
one of them was measured a different way. A `git bisect` would have spent an
evening and landed nowhere.

**What actually disagrees is HM-DEC-115 and the test.** That ruling's text says the
same audio read `AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAGE P`, every
character correct after acquisition. The test written the next day already showed
36 of 47. One of the two is wrong about the same recording, and a day apart is not
rot — it is a measurement that was never reproduced.

**The errors are characterised and their mechanism is not spacing.** `STAAION` for
`STATION` and `AHIS` for `THIS` are both T read as A: a lone dah gaining a leading
dit. That is an element-level fault — a mark split in two, or a character boundary
missed so a preceding dit joins the dah — and it is untouched by HM-DEC-142, which
is about where the words are rather than what the letters are. The dropped letters
in `EACH` and `MESSAGE` and the unresolved `■` for the `BT` prosign sit with it.

**Naming the line needs the element-level investigation this order did not have
room for**, and this project has twice been burned by a diagnosis that named a
suspect without naming the mechanism. What is wanted is a ruling on which of the
two readings of that capture is believed, and then one order aimed at the T-to-A
substitution with the audio in front of it.

---
id: HM-OPEN-048
status: open
owner: claude
raised: 2026-08-19
severity: slows
blocks: the transcript half of the CW terminal; the leading edge is unaffected
refs: HM-DEC-102, HM-DEC-114, DECODER_AND_SCANNER_BRIEF.md phase 4, tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/CwSettledSilenceTests.cs
---

The settled pass is not reading worse than the leading edge. On two of three
proved fixtures it is barely reading at all, and **the two fail differently**.

Measured 2026-08-19, on fixtures the reference reads at 96 to 100 percent:

| Fixture | Leading edge | Settled | Last refusal | What it fitted |
|---|---|---|---|---|
| `exchange-easy` | 28 characters, `VV CQ CQ DE N0CALL N0CALL K` | **3** | `Clock`, keying verdict false | no clock, dit 0 |
| `coverage-easy` | 28 characters, `V 1234567890 QRZ? DE/N0CALL` | **0** | **None** | dit 100 ms at 23.6 dB, keying true |
| `tightfist-easy` | 11, correct | 10, garbled | — | — |

`DECODER_AND_SCANNER_BRIEF.md` phase 4 records 15 characters with 73 percent
unresolved on `exchange-easy`. It is three now. **Whatever moved between those two
measurements moved it a long way**, and nothing in the suite could see it, because
every existing measure was a share of what was emitted and a pass that emits
nothing has no share.

**The `coverage-easy` line is the one to start from.** The pass refused nothing,
fitted a hundred millisecond dit — twelve words a minute, which is the fixture's
own speed — measured 23.6 dB of contrast and passed the keying gate, **and handed
back an empty window.** That is not a sensitivity problem. A pass that reports it
read and produces no characters is contradicting itself, and the repair is in
extracting runs from a window it says it read rather than in making it hear
better.

`exchange-easy` fails earlier and differently: the clock is refused outright and
`KeyingRecently` is false at the end of the recording, so it never reaches the
reading at all. Two faults, not one.

**The lead the brief names is still the lead**: the reference de-glitches at 20 ms,
extracts runs, fits the clock, then de-glitches again at 0.4 of a dit and re-reads
every run. Hamlet's settled pass reads once.

**`CwSettledSilenceTests.APassThatReadSomethingEmitsSomething` is red on purpose**
and is the third standing failure. HM-DEC-114 is the precedent: a fixture the
reference reads at 100 percent, read by Hamlet as nothing, is a defect and a red
test is the correct state of the world rather than a ratchet to be tuned.

**The operator's evening is not blocked by this.** The leading edge reads both
fixtures perfectly and is what the terminal shows live; what is degraded is the
transcript the settled pass keeps.

**MECHANISM FOUND 2026-08-19, END TO END, AND THE REPAIR NEEDS A RULING.**

Tallied across the whole of `coverage-easy`, window by window: **258 windows
returned `None`** — read successfully — 63 said `NotYet` and 16 refused the clock.
Not one character was emitted.

The loss is in `Emit`, at its first line. It asks for the sender's gap classes and
returns without producing anything when there are none, which is HM-DEC-115 doing
exactly what it says: *no cuts means no transcript, not a guessed one.* There are
**80 gaps** to cluster, far past the ten `CwGapFit` needs. **The fit refuses
anyway, because it requires three non-empty heaps** — element, character and word
— and this message leaves almost no word gaps, so the top class comes back empty
and `Fit` returns null.

So the settled pass is silent on any transmission that does not contain several
word gaps. **That is not an exotic case**: a callsign, a contest exchange, a `V`
test string, anything sent without spaces. The leading edge reads all of it
perfectly.

**And the brief's lead was already built.** It says Hamlet reads once and the
reference de-glitches a second time at 0.4 of a dit and re-reads. `CwSettledPass`
has done that second de-glitch and refit since HM-DEC-096. That is not the gap.

**What a session may not decide.** Two honest answers exist and they assert
different things about where the words are:

- **Cluster two heaps when there is no third.** Element and character gaps are
  still the sender's own, so this is not a return to dit multiples; what it costs
  is that a genuine word gap, of which this fixture has two or three, is folded
  into the character class and the spaces disappear.
- **Say the transcript has no word boundaries here**, and render it without
  spaces, which is true to what was measured and reads badly.

Both change what a transcript asserts about where the words are, which §12.1 puts
outside a session's authority without exception. HM-OPEN-017's labelled
approximation is the third option and is likewise taken by ruling.

`CwSettledGapTests` and `CwSettledSilenceTests` now record the gap count and
whether classes were fitted, so whichever way it is ruled, the next session starts
from the mechanism rather than from a percentage.

**MEASURED 2026-08-19: IT IS A DEFECT AND NOT A RULING, AND THE NUMBERS SAY SO.**

The last session handed back the `coverage-easy` question as a ruling, on the
reasoning that the classes are named by position and the content might be
element and character-or-longer. **That reasoning was wrong and one measurement
settles it.** The eighty gaps are:

| Duration | How many | What it is |
|---|---|---|
| 110 ms | 61 | element gaps, one dit at twelve words a minute |
| 310 ms | 17 | **character gaps**, three dits |
| 710 ms | 2 | **word gaps**, seven dits |

**All three heaps are present, textbook spaced.** There was never a question about
what the transcript should assert. The fit refuses because the three seeds land on
110, 110 and 310: the percentiles are the quarter, three-quarter and
nineteen-twentieth marks, and with element gaps three quarters of everything a
sender produces, **the middle seed starts inside the element heap**. Its cluster
empties and the whole fit is refused.

**Ordinary sending is what does this.** The commoner the element gaps, the further
up the sorted list the three-quarter mark sits inside them. A fixture with short
words hides it; a callsign does not.

**Two repairs were tried and each traded one green test for another.** Moving a
collapsed seed to the first value standing clear of the one below fixed
`coverage-easy` — settled went from 0 characters to 14, with all three classes
fitted and the word gaps found — **and cost the clean recordings their word
space**: `CQDE W1AW K` for `CQ DE W1AW K`, on two fixtures, plus the training
radio and sample-rate tests. Re-seeding the middle at the gap standing furthest
from both neighbours fixed those and broke the two-class gate instead, because with
genuinely two heaps it manufactures a third.

**Both are reverted.** Nothing in the tree changed from this attempt. The
requirement is one rule that rescues a collapsed middle seed **only where a third
heap exists**, and telling those apart is the fit's own job rather than something
to patch at the seeds. That is its own work, with every fixture adjudicated, and
it is not a ruling ask.

---
id: HM-OPEN-047
status: open
owner: tim
raised: 2026-08-19
severity: slows
blocks: nothing; this is the size of the re-reading, not the re-reading
refs: HM-OPEN-042, HM-DEC-092, HM-DEC-084, HM-DEC-062, CLAUDE.md §1
---

The write-outcome sweep Tim ruled on 2026-08-18: which rulings reasoned from a
measurement that was wrong, and which of them survive it.

**The fault, in one sentence.** From HM-DEC-084 on 2026-08-15 until the repair on
2026-08-19, `SetSettingAsync` wrote, took the radio's `FB`, then read the setting
back to confirm it — and issued that read with no expected response command, so it
completed only on another `FB` or an `FA`. A read is answered with a value frame.
**Every readback timed out, so every setting write the radio took was reported as
`NoAnswer`.** Reads were never affected: `ReadAsync` passes its own command and
sub-command as the expected reply.

**Nothing here is re-ruled.** This is a documents pass, which is what makes it
cheap: it says how big the re-reading is so Tim can decide what deserves an
evening.

## Worst first

**HM-DEC-092, 2026-08-17 — materially affected, and part of it is already in the
queue.** It concluded two things. First, that the link should report commands
sent, answered and unanswered, reasoned from *five settings written one evening,
all five reported as unanswered, at least two actually in effect*, read as radio
frequency energy knocking the link about. **The counters survive and are worth
having** — they are what later proved the radio never broadcasts — but the
diagnosis they were built on was the readback fault wearing the link's clothes,
and the ruling's own text still attributes it to the link. Second, that `27 11`
may be written because attempting it and reporting the answer replaces a guess
with a measurement. **That reasoning does not survive**: the answer could not be
read, so Hamlet reported the write refused without knowing. The automatic write
was removed on 2026-08-18 restoring HM-DEC-062, and whether Hamlet may ever ask is
the unruled question already in the outstanding-asks queue.

**HM-DEC-084, 2026-08-15 — the rule survives; the evidence from its own window
does not.** "An acknowledgement says the radio understood the frame, not that the
setting moved… a write that cannot be confirmed by a read-back is reported as
unconfirmed and never as done." **That is exactly right and the fault proves it**,
since the mechanism it demanded was the one that broke. What does not survive is
anything in the record between 2026-08-15 and 2026-08-19 about which settings
took: every tier-one write in that window reported unconfirmed whatever the radio
did. The exclusion of `16 65` for being unreadable stands on the manual rather
than on any measurement, so it is untouched.

**HM-OPEN-041, mode follow — checked and clear.** Its eighteen writes were
reported as they happened: `SetModeAsync` does not read back at all, it folds the
new mode into the model on the acknowledgement, so this fault never reached it.
The repeats had their own cause and the last session named it.

**HM-DEC-056, mode writes — unaffected**, for the same reason.

**HM-DEC-107, the scanner — unaffected.** It aborts on an unanswered *read*, and
reads always carried their expected command.

**HM-DEC-093, the scope stage counts — unaffected.** It rests on frames received
rather than on any write's outcome, and it is the ruling that made the frame count
the thing nobody may report the waterfall working without.

## Where the sweep is limited, and it is not a small limit

**`CLAUDE.md` §1 rows 096 to 133 have no entries in `DECISIONS.md`**
(HM-OPEN-045), so for thirty-eight rulings the one-line summary is the whole of
what could be swept. I read every one of those rows for the fault's fingerprints —
an unanswered count, a write reported failed, a setting believed not to have taken
— and none carries them. **That is weaker than it sounds.** A summary is written to
say what was ruled, not what the reasoning leaned on, so a ruling in that range
could rest on a wrong write outcome without the row showing it. If the chat-side
recovery of those entries succeeds, this sweep is worth ten minutes again with the
full text.

---
id: HM-OPEN-046
status: closed
owner: tim
raised: 2026-08-19
closed: 2026-08-19
severity: slows
blocks: nothing; both rulings are in force and both are indexed
refs: CLAUDE.md §1, §2.1, HM-DEC-088, HM-OPEN-036, HM-OPEN-045
---

**Two different rulings both carry the id HM-DEC-088**, which §2.1 forbids
absolutely: never reuse an id, never renumber.

Both are dated 2026-08-16 and both are in `CLAUDE.md` §1, at what are now lines
399 and 400:

- **"The decoder measures the noise beside the tone, integrates over the element,
  keeps what it heard, and says what it can see when it decodes nothing."** This
  one has a full entry in `DECISIONS.md`.
- **"The top strip becomes one row."** Bands beside the readout, the privilege
  line and the way to your places on one line under them. This one exists only as
  its index row, since it falls inside the 096-to-133 window's neighbourhood of
  the same fault (HM-OPEN-045).

**How it was found.** Not by reading. The ordering sweep written for HM-OPEN-036
rebuilt the row block from a dictionary keyed by ruling id, and a dictionary keeps
the last of a duplicate key — so the decoder ruling's row was written out of the
file and six rows shifted to fill the space. The test written in the same phase
caught it within a minute, the change was reverted, and the reorder was redone on
positions rather than ids. **The duplicate had been sitting there since
2026-08-16 and nothing had looked.**

**Why no session should renumber it.** Every reference to HM-DEC-088 in the tree —
in code comments, in other rulings, in open items — points at one of the two, and
which one is not always readable from the citation. A renumber that guesses wrong
breaks a citation silently, which is worse than a duplicate that is visible.
Choosing the new id and re-pointing the references is Tim's.

**What is in place meanwhile.** `DecisionLogOrderTests` names 88 as the one known
reused id and fails on any other, so the defect stays visible and cannot spread.

**CLOSED 2026-08-19. Tim ruled A: the later ruling takes the next free id, and
the tiebreak comes from the history rather than from judgment.**

Both index rows arrived in the same commit, `49b844c`. Within it the decoder's
noise-measurement row is written first, and `DECISIONS.md`'s only HM-DEC-088 entry
is that same ruling. **So the decoder keeps 088** and **the top strip becoming one
row is now HM-DEC-141**, the next free id — 105 and 136 are not free, one being a
ruling whose entry is missing and the other deliberately absent.

Eleven citations were re-pointed, each classified by reading the comment it sits
in: the wheel hint retiring, the bands sitting beside the readout, the strip
costing one row rather than a third of the window, and the settings flag behind
the hint. Everything else citing 088 is about measuring noise beside the tone and
is untouched.

**This was a clerical correction and not a supersession.** An id that was never
valid is not a ruling being overturned, so nothing here needs a further ruling and
neither ruling's text changed.

`DecisionLogOrderTests` no longer allows a repeated id at all — the allowance went
with the thing it allowed for, rather than staying as a door somebody could walk
back through. What it names instead is that HM-DEC-141 is a 2026-08-16 ruling
carrying a later id, so the same-date id ordering cannot speak about it while the
date ordering still does.

---
id: HM-OPEN-045
status: open
owner: tim
raised: 2026-08-19
severity: slows
blocks: nothing today; every affected ruling is in force and indexed
refs: CLAUDE.md §1, §3, §5, §12.1, DECISIONS.md, HM-DEC-105, HM-DEC-112
---

Thirty-eight rulings between HM-DEC-096 and HM-DEC-133 have no entry in
`DECISIONS.md`, and the history says they were **never written** rather than
written and lost.

**The evidence, and it is binary.** `git log -S "id: HM-DEC-096"` across all paths
and all history returns nothing, and the same is true of 100, 113, 120, 129 and
133: those strings have never existed in this repository. `DECISIONS.md` has only
ever grown — 5,056 lines, then 5,174, then 5,331 at HM-DEC-095 on 2026-08-17, then
5,369 at HM-DEC-134 on 2026-08-18 — and no commit has shortened it. Between those
two the file was not touched at all.

**Where it started, and what the commits were doing.** `d6b4ce2`, 2026-08-17 at
16:26, added the §1 row for HM-DEC-096 along with `BATCH_BRIEF_AMENDMENT.md`,
`CLAUDE.md` and `SESSION_PROTOCOL.md`, and **did not touch `DECISIONS.md`**.
`83d58d1` and `c1a76f8` the same evening did the same for 097 and for the rulings
around 113. All three are commits of chat deliveries, which write `CLAUDE.md` and
brief files; the entry the row points at was simply not in the zip.

**The rule was already there and nothing enforced it.** §1 has said "Detailed
records live in `DECISIONS.md`; this table is the index" since before the first
missing row, in that wording, unchanged. §3 requires the file to hold rulings with
`id` and `date`, and §5's definition of done requires the records updated in the
same delivery. Nothing compares the two, which is the same shape as HM-OPEN-044:
a correct rule with no mechanism behind it.

**One ruling is worse off than the other thirty-seven.** **HM-DEC-105 has neither
an entry nor an index row.** It is cited by HM-DEC-112's row as having put the
half-amplitude correction in the settled pass, twice by
`DECODER_AND_SCANNER_BRIEF.md`, and by two entries in this file. It was ruled,
acted on, and recorded nowhere. Everything anybody knows about it is a sentence
inside another ruling's summary.

**What a repair may not be.** A ruling reconstructed from its own one-line index
row is a session writing Tim's reasoning for him and attributing it to him, which
§2.1 and §12.1's attribution rule forbid absolutely. That is worse than the hole,
because the hole is visible and a plausible forgery is not.

**Three shapes, none of them ruled here.**

- **Leave it and mark it.** `DECISIONS.md` gains a note at the 095/134 seam saying
  what is missing and why, so nobody reads the gap as rulings that were withdrawn.
  Cheapest and honest; the reasoning stays lost.
- **Restore from the chat transcripts**, if Tim still has the conversations those
  rulings were made in. That is recovery rather than reconstruction, and it is the
  only route to the actual text.
- **Promote the index rows as they stand**, each entry saying on its face that it
  is the index row and that the full reasoning was never recorded. Honest about
  what it is, and it makes the file complete without inventing a word.

HM-DEC-105 needs an answer under any of the three, since there is no row to
promote and nothing to point at.

---
id: HM-OPEN-044
status: closed
owner: tim
raised: 2026-08-19
closed: 2026-08-19
severity: slows
blocks: nothing today; the two known cases are now ruled
refs: HM-DEC-138, HM-DEC-113, CLAUDE.md §9.5, §12.2
---

A change ships carrying a ruling request, the request goes unanswered, and the
next session inherits the change as settled.

**The case that names it.** `099de5a` moved the frequency to the live poll. The
report that shipped it put the ruling ask in section 4, as §12.2 requires, and no
ruling came back. The next work order then withdrew a draft of that same ruling on
the grounds that its premise was disproved, while the change itself was already in
the tree and running. Two sessions later it was still unruled and still live, and
it took a third to notice. HM-DEC-138 has now ruled it, in favour of the code.

**It is HM-DEC-113's shape.** There, a session invented a branch and every session
after it inherited the invention as settled, four reports naming it without one
treating it as a question. Here a session built a thing it correctly asked about,
and the asking is what makes it invisible: section 4 was written properly, so
nothing looked wrong from inside any single session.

**What §9.5 already says, and what it lacks.** "A decision that is not in
`DECISIONS.md` is not made." The rule is right and it has no mechanism: nothing
compares the tree against the record, and a session reading `CLAUDE.md` §1 sees
only rulings that were made, never behavior that is waiting on one.

**Ways it could be closed, none of them ruled here.** A standing section in the
report for asks still outstanding from earlier sessions, so an unanswered question
is re-raised rather than aged out. A marker in the source at the site itself, in
the manner of §12.4's marked assumptions, that a test could sweep for and fail on
after some number of days. Or a rule that a change needing a ruling is not shipped
until it has one, which is the strictest and would have cost the operator a
working display for two evenings.

**Not a rule invented by a session.** It is Tim's to rule, which is why this is an
open item rather than an entry in `DECISIONS.md`.

**CLOSED 2026-08-19 by HM-DEC-139, which ruled the standing heading and ruled that
it starts now.** Every report's section four ends with `Asks still outstanding`,
present even when empty, carried verbatim until Tim rules and dropped by the report
that records the ruling; every work order carries the same list inbound. The marker
at the site was deferred rather than rejected, and refusing to ship without a ruling
was rejected on a measured cost.

---
id: HM-OPEN-043
status: open
owner: tim
raised: 2026-08-18
severity: slows
blocks: nothing; the read works or reports unknown, and either way it is marked
refs: CLAUDE.md §4, §12.4, HM-DEC-071, src/Hamlet.RadioEngine/Civ/CivReads.cs
---

`1A 05 0071` is read as the radio's transceive setting on the strength of a work
order rather than a page in `A7292-4EX-6`.

**Why it was built anyway.** Whether the radio announces its own changes decides
whether the frequency on screen follows the dial in a tenth of a second or by
Hamlet asking four times a second, and Hamlet had never asked. An app tracking at
poll speed and an app with a dead broadcast path look identical from the inside,
and §0.0 wants the condition stated rather than inferred.

**Why it is marked rather than cited.** §4's table carries `1A 05` rows for the
ACC/USB settings (19-4, 19-5) and for the CI-V USB port (19-5), and no row for
this one. The sub-command came from the work order of 2026-08-18. Writing a page
number nobody had read would be the fault HM-DEC-071 exists to prevent, on a table
whose whole worth is that a figure arrives with the page it was read from. So the
row's page reads `uncited (HM-OPEN-043)`, `CitationTests` accepts that shape and
**proves it names a live open item**, and nothing else in the file can quietly
join it.

**What a wrong sub-command would do.** Read a neighboring setting and report a
confident number about the wrong thing, which is why the read is the only thing
built on it: nothing is written, and the value only ever makes Hamlet say whether
the radio is announcing. If it is wrong, the sentence is wrong and no byte on the
radio moved.

**To close it:** one column-aware read of `A7292-4EX-6` around p. 19-4 and 19-5,
confirming the sub-command and its page, and the row cites it like every other.

---
id: HM-OPEN-042
status: open
owner: tim
raised: 2026-08-18
severity: slows
blocks: FACT-003's ladder beyond rung two, which needs a connected radio
refs: FACT-001, FACT-003, HM-DEC-092, HM-DEC-084, src/Hamlet.RadioEngine/Rig/Ic7300Rig.cs
---

The readback that confirms every setting write was waiting for an
acknowledgement the radio does not send, so a write the radio took reported as
unanswered.

**What the code did.** `SetSettingAsync` writes, gets `FB`, then reads the
setting back, because an acknowledgement says the radio understood the frame and
not that the setting moved (HM-DEC-084). That readback was issued with no
expected response command, and with none the dispatcher satisfies the request
only on `FB` or `FA` — while a read is answered with the value frame. **So every
readback timed out.** The write was reported as `NoAnswer`, and the setting had
moved anyway.

**HM-DEC-092 saw this from the other end and attributed it elsewhere.** Five
settings written one evening, all five reported unanswered, at least two actually
in effect: that was read as the link dropping commands, and the unanswered
counters were built. The counters are right and worth having; they were not the
fault.

**What it means for `27 11`.** `scope_output_requested` failed `noanswer` on all
six connects of session `9f9d23eb`. Under this code that is what a **successful**
write looked like as well as a silent one, so **rung one of FACT-003's ladder
was never actually answered** and rung two could not be reached. The readback now
waits for the value frame, and the two outcomes are separate:
`no_answer` is silence, `read_back_disagreed` is the radio saying yes and then
reporting the setting still off.

**What is still unknown, and needs the radio.** Whether `27 11` now confirms.
Nothing in this session is evidence about that (HM-DEC-093), and no session may
report the waterfall working without a nonzero received-frame count from a
connected radio (HM-DEC-093, FACT-003). The next connect answers it in one line
of the record.

---
id: HM-OPEN-041
status: closed
owner: claude
raised: 2026-08-18
closed: 2026-08-19
severity: slows
blocks: nothing; the writes have stopped, but what triggers them has not been seen
refs: HM-DEC-056, HM-DEC-077, src/Hamlet.RadioEngine/Explore/ModeFollowPlan.cs, src/Hamlet.App/ViewModels/MainWindowViewModel.cs
---

Mode follow wrote to the radio eighteen times in one evening, ten of them with
nothing the operator did anywhere near them, and the record cannot say what
recomputed it.

**What was measured.** Session `9f9d23eb`, app 1.9.0, 2026-08-18: eighteen
`mode_followed` events, ten with no `tune_requested` inside three seconds,
including an unbroken run at 20:30:39, :50, :51, :53, :56, :57, :59 and 20:31:02.

**What is understood, and is fixed.** `ModeFollowPlan.Decide` asked one question
before writing: is the radio already in this mode. That question is answered from
`RigState`, where an unread field reads as not-in-that-mode by design
(HM-DEC-056 wants an unknown data setting to be a reason to write). So any tick
that found the mode unread looked like a fresh arrival at a neighborhood, and
every trigger produced another write. The plan now also remembers the last write
the radio **confirmed**, with the frequency it was made at, and refuses to repeat
it. Nothing writes where the old test would have refused, so the memory can only
reduce what goes out, and a band change or the operator's own hand clears it.

**What is not understood.** `ScheduleModeFollow` has two callers, a band change
and the frequency changing, and the frequency handler runs for values the radio
reports as well as for the operator's own tuning. Something moved that value with
the dial standing still — a poll disagreeing with a broadcast, or a value
round-tripping through the clamp — and **that has not been seen**, because seeing
it needs a connected radio and this session had none (HM-DEC-093). The suppression
above means the symptom no longer reaches the transmitter, which also means the
next evening will not show it. `recent_dwell_short` is the instrument to watch:
a dial that is not moving files no near misses, so a run of them with nobody
touching anything names the fault directly.

**A refusal token that named the wrong branch, found beside it and fixed.** At
00:29:23.700 `send_buttons_enabled` carried `reason: "already_transmitting"` with
`readinessState: "OutsidePrivileges"` in the same event. Four states fell through
a catch-all to that token, so a refusal on the operator's license recorded the
radio as busy. Every state names its own branch now and a test proves no two
share a token, which is the whole worth of a stable token under HM-DEC-077: a
session months from now counting refusals by cause was going to be counting the
wrong thing and would have had no way to tell.

**CLOSED 2026-08-19. What recomputed it was the frequency changing, and what was
changing the frequency was the snap-back.**

`ScheduleModeFollow` has exactly two callers: a band change, and `FrequencyHz`
changing by any route, including a reading from the radio. In the build of that
evening a reading older than the operator's own tune dragged the display back and
the next poll moved it forward again, so the number changed twice per tune with
nobody touching anything, and each change restarted the six hundred millisecond
settle. **The one-to-eleven second gaps in the run are what a settle timer does
when it is restarted by a value that will not sit still.**

The instrument this issue named is the confirmation: `recent_dwell_short` fires
from the same handler, so four of them in a session with two tunes is four
frequency changes the operator did not make.

**The repair was already shipped** as `DialGuard` on 2026-08-19: a reading taken
before Hamlet's own tune cannot move the display, so the alternation stops at
source. What was missing was anything asserting the quiet case, and
`ModeFollowRecomputeTests` is that: forty polls reporting the frequency the
display already holds, and nothing recomputes; a stale reading after a tune,
nothing recomputes; a genuine move, exactly one. `ModeFollowReschedules` counts
the asks so the assertion is on the loop rather than on its symptom.

---
id: HM-OPEN-039
status: closed
owner: claude
raised: 2026-08-18
severity: slows
closed: 2026-08-18
blocks: nothing, but HM-DEC-072 and HM-DEC-134 cannot be shown to work
refs: HM-DEC-072, HM-DEC-134, CLAUDE.md §0.0.1, src/Hamlet.RadioEngine/Explore/RecentStation.cs
---

Nothing the recent list does appears in the app's own record, so its rulings
cannot be checked against a live session.

`favorite_saved` is emitted; its sibling emits nothing. There is no event for an
entry being added, for a visit folding into an existing entry under the two
hundred hertz tolerance, for the dwell threshold being met or missed, or for an
entry falling off the end of ten.

**What that cost today.** Session `9f9d23eb`, app 1.9.0, 2026-08-18. Six visits
met HM-DEC-072's twenty-second dwell:

| Arrived (UTC) | Frequency | Dwell |
|---|---|---|
| 20:30:22 | 7.047.00 | 43.5 s |
| 20:31:09 | 7.059.60 | 24.8 s |
| 20:31:34 | 7.030.10 | 35.6 s |
| 20:32:12 | 7.059.50 | 23.9 s |
| 20:32:36 | 7.030.10 | 20.2 s |
| 20:32:56 | 7.059.60 | 218.9 s |

Under HM-DEC-072 those are three places. The operator reported seeing near
duplicates. **Which of the two happened is not in the record, and the whole of
this issue is that both readings survive the evidence.** Dwell times are derived
from `tune_requested` timestamps, which is the arrival of a request and not a
statement that the entry was written.

Wanted, and each is one line: the entry written, with the frequency and whether
it was named; the fold, with both frequencies and the gap, since that is the
one that proves the tolerance; the dwell not met, with how long it fell short,
because a list that stays empty while somebody sits still looks identical to a
list that is broken; and the drop off the end of ten.

§0.0.1 is the standard being applied. A ruling whose behavior leaves no trace
cannot be told apart from its own absence, which is HM-DEC-072 and now
HM-DEC-134 both resting on a claim nobody can check.

**CLOSED 2026-08-18, and closed by measurement rather than by argument.** All
four events are in `AppEvents` and emitted from `MainWindowViewModel.NoteDwell`:
`recent_remembered` with the frequency and whether a station was named,
`recent_folded` with both frequencies and the gap between them,
`recent_dwell_short` with how far short the dial fell before it moved on, and
`recent_dropped` naming the place that fell off the end of ten. `recent_removed`
joins them for HM-DEC-134's own half. **The six dwells above are the test
fixture**, and they now produce three entries and three folds in the record
rather than a silence that reads the same either way.

---
id: HM-OPEN-001
status: closed
owner: tim
raised: 2026-08-12
severity: hard
blocks: solution scaffold — the App project cannot be created without it
closed: 2026-08-12
refs: HM-DEC-011
---

WPF or Avalonia for the UI shell?

| | WPF | Avalonia |
|---|---|---|
| Platform | Windows only | Windows/Linux/macOS |
| Maturity / tooling | Deepest, designer support | Good and improving |
| Tim's familiarity | High (old-school C#) | New |
| Open-source audience | Windows hams only | All — and Linux is common in ham shacks |
| WriteableBitmap waterfall | Native | Equivalent (WriteableBitmap exists; API differs slightly) |

Industry-standard answer for a public open-source ham tool: Avalonia, for the
Linux audience. Fastest-start answer: WPF. Per §0, "faster to start" is not a
reason, but "Tim ships phase 1" has weight too. Tim rules.

---
id: HM-OPEN-002
status: closed
owner: tim
raised: 2026-08-12
severity: slows
blocks: nothing yet; becomes hard when CI-V code is written
closed: 2026-08-14
refs: HM-DEC-049, HM-DEC-005, CLAUDE.md §4
---

Obtain the IC-7300 CI-V reference (the "Full Manual" / CI-V command tables
from Icom) so the command facts in CLAUDE.md §4 can be verified and the cited
pages vendored into data/vendor/.

Everything Claude currently holds about 0x17 (CW send), 0x27 (scope data),
frame format and BCD encoding is general knowledge, marked unverified. Code
must not depend on an unverified command byte. Tim downloads the PDF from
Icom and uploads it to the session; Claude extracts and vendors the cited
sections only.

**CLOSED 2026-08-14 (HM-DEC-049).** Tim supplied the Full Manual and section 19
CONTROL COMMAND was read directly. §4 now carries the verified facts with page
citations, two corrections and one precondition nobody had written down.

**The vendoring half was NOT done, and that is the ruling rather than an
omission.** Icom's terms permit individual use and prohibit redistribution, so
the repository cites pages and carries none of the PDF. §4's "vendor the cited
pages" rule stands for sources that allow it; this one does not, and §2.1 wins.

---
id: HM-OPEN-003
status: open
owner: tim
raised: 2026-08-12
severity: none
blocks: nothing; the app reads most of these itself now
refs: HM-DEC-050, HM-DEC-048
---

Station configuration facts from Tim's PC: the COM port the IC-7300
enumerates as, the exact audio device names ("USB Audio CODEC" variants),
the radio's CI-V baud and CI-V address menu settings, and CW sidetone pitch
setting.

These are config values, not constants. Needed before the first
connect-and-read-frequency test. Device Manager and the radio's SET menu
answer all of them in five minutes at the desk.

**NARROWED 2026-08-15 (HM-DEC-050), severity dropped from `slows` to `none`.**
The first live connection has happened, so this no longer blocks anything, and
the app now answers most of it itself:

- **CW sidetone pitch** is read from the radio (`14 09`) and shown on the
  diagnostics screen. Nobody has to walk over and look.
- **Audio device names** are enumerated and chosen automatically, preferring one
  whose name matches the radio's USB codec, with the operator's own choice
  remembered (HM-DEC-048). A machine with none says so and carries on.
- **CI-V address** is proved by the connection succeeding at all: the probe read
  only answers when the radio, the address and the baud agree.

What is genuinely left is the pair a person still has to supply, because nothing
can be read until they are right:

- **The COM port.** Hamlet lists the ports it can see and the operator picks;
  there is no way to know which one is the radio until something answers on it.
- **CI-V USB baud when it is not Auto.** The radio defaults to Auto (p. 12-11)
  and Hamlet has no way to discover a fixed setting except by failing to
  connect.

Both are answered by connecting once and writing down what worked, which is
configuration rather than a question anybody has to research.

---
id: HM-OPEN-004
status: open
owner: unassigned
raised: 2026-08-12
severity: none
blocks: phase 3 only
---

FT8 decode integration approach: P/Invoke wrap of ft8_lib, or shell out to a
WSJT-X jt9 subprocess?

ft8_lib is small, clean C, designed for embedding; jt9 is the reference
decoder with better weak-signal performance but a process boundary and
version coupling. Both are GPL (HM-DEC-004 already accounts for that).
Decide during phase 3 planning; nothing before then depends on it.

---
id: HM-OPEN-005
status: closed
owner: tim
raised: 2026-08-12
severity: slows
closed: 2026-08-18
refs: src/Hamlet.RadioEngine/Bands/HfBands.cs, data/privileges/us-part97-privileges.json, data/bands/us-neighborhoods.json, CLAUDE.md 0, 0.2.1, HM-DEC-107, HM-DEC-110
---

Move the band plan out of code into a source-marked data file in /data, with
citations (ARRL band plan, FCC Part 97) and per-license-class privileges.

The current BandPlan.cs carries US allocations marked [extrapolated] from
general knowledge. Fine for phase 1 tuning; not fine as the basis for FG-006
band-plan coaching or transmit-privilege warnings, which need cited,
class-aware data. Generate-don't-transcribe applies (§0).

**NARROWED 2026-08-13 (HM-DEC-029).** The privileges half is done:
`data/privileges/us-part97-privileges.json` carries 47 CFR 97.301, 97.305 and
97.307 transcribed from eCFR, cited per row, with its gaps declared as explicit
unknowns. Transmit-privilege warnings now rest on cited data.

What remains is `BandPlan.cs` itself, which still holds three kinds of number
in code:

- **Band edges** (`LowHz`, `HighHz`). Now redundant — the same edges are in the
  privileges file under the Extra class, which by definition reaches every band
  edge. These should be derived from it rather than kept in parallel.
- **CW segment boundaries** (`CwLowHz`, `CwHighHz`). Still [extrapolated].
  These are convention, not regulation, and they do NOT align with the
  privilege boundaries — both encodings are needed and neither derives from the
  other (HM-DEC-029).
- **Jump spots** (`JumpHz`). Editorial: QRP watering holes and activity
  conventions. A citation would be an ARRL band plan or a club convention, not
  a regulation.

So this stays open at severity `none`, now meaning: derive the band edges from
the cited data, and give the conventions a source mark of their own kind.

**NARROWED AGAIN 2026-08-14 (HM-DEC-054).** The neighborhood conventions are
now cited data in `data/bands/us-neighborhoods.json`, with the ARRL Considerate
Operator's Frequency Guide, WSJT-X's shipped frequency table, the JS8Call user
guide, the 070 Club's PSK31 list and QRP ARCI's centers of activity on the rows
that use them. The map derives its data-against-phone boundary from the
privileges file rather than carrying a copy.

What remains in `BandPlan.cs` and is still `[extrapolated]`:

- **Band edges** (`LowHz`, `HighHz`). Unchanged from above: derivable from the
  privileges file under the Extra class and still kept in parallel.
- **CW segment boundaries** (`CwLowHz`, `CwHighHz`). Now used by less than they
  were, since the map no longer builds itself from them, but they still drive
  the dial tape's "inside the CW segment" line.
- **Jump spots** (`JumpHz`). Now demonstrably wrong in at least one place: 20 m
  jumps to 14.030 and QRP ARCI puts the 20 m center of activity at 14.060. The
  neighborhood file has cited jump spots per block, so a band button could take
  its landing place from there instead of carrying its own number.

**MEASURED 2026-08-17, AND THE SEVERITY GOES UP.** Raised from `none` to
`slows` and the owner from `unassigned` to `tim`, because this is now load
bearing for a feature that moves the operator's dial: §0.2.1 forbids
frequencies asserted from a model's memory, so the scanner was built around
`BandPlan` rather than on it and its segments come from
`data/bands/us-neighborhoods.json` instead. Two band plans in one tree, one
cited and one not, is the state §0 exists to prevent, and the uncited one has
the friendlier name.

**Two of the three kinds of number are provably derivable and the third is
not.** Measured against the cited files rather than argued about:

- **Band edges: all seven match exactly.** The union of the Extra class ranges
  in `data/privileges/us-part97-privileges.json`, cited to `97.301(b)`, gives
  `LowHz` and `HighHz` for every band. 80 m is the CFR's 80 m and 75 m rows
  together, which is the only join needed.
- **CW segments: all seven match exactly.** The union of the ranges carrying
  `Data` in the same file, cited to `97.305(c)`, gives `CwLowHz` and
  `CwHighHz` for every band, down to the hertz. 40 m needs two rows joined,
  `(c)(3)(iv)` and `(c)(3)(vi)`, because the phone segment overlaps the first.
  This corrects the note above: they were thought not to derive from the
  privileges data, and they do.
- **Jump spots do not derive, and the reason is that a rule has to be chosen.**
  Five of the seven are exactly a "CW main street" block's `jumpHz` in the
  neighborhood file; 40 m is the QRP watering hole's rather than main street's;
  and **30 m matches nothing cited at all** — it lands on 10.110 where the
  blocks are 10.103, 10.106 and 10.120.

The neighborhood file on its own does **not** cover the CW segments and cannot
be made to. Its Morse rows fall short at the top of every band, by 10 kHz on
17 m up to 230 kHz on 10 m, and 40 m has a hole in the middle between 7.040
and 7.050. That is not a defect in it: those rows are places somebody
published a convention for, and the space between belongs to nobody
(HM-DEC-054). The CW segment is a regulatory boundary and its source is the
privileges file.

**What is needed is one ruling, on the jump spots.** At least three rules are
defensible and they land in different places: the first "CW main street"
block, the QRP watering hole (which is what the note above argues for on 20 m,
against QRP ARCI's 14.060), or keeping the current numbers and citing them as
editorial. Whichever is taken changes where a band button lands on between
three and seven bands, which is a trade-off between cited data and the
operator's muscle memory, and §12.1 puts that with Tim.

**CLOSED 2026-08-18 (HM-DEC-110).** `BandPlan` is deleted and `HfBands`
replaces it, deriving every number from a citation. The migration ran exactly
as the measurement above said it would.

**AND THE CORRECTION ABOVE BELONGS IN THE RECORD RATHER THAN BEING QUIETLY
DROPPED.** The 2026-08-14 entry said the CW segment boundaries are convention
rather than regulation and do not align with the privilege boundaries. That was
wrong. They are the union of the ranges carrying data in 47 CFR 97.305(c) and
they align to the hertz on all seven bands. Two of those rows say "Entire band"
rather than a range, 80 m and 30 m, and expanding them from 97.301's own edges
is what makes them look like conventions when they are not.

**The cited data was verified against the regulation itself before anything
re-pointed to it** (CLAUDE.md 4), from the eCFR versioner API for title 47 as of
2026-08-01 rather than from the file that quotes it. §97.301(b) gave every band
edge and §97.305(c) every data range, and all fourteen numbers matched. The
column-awareness that section insists on earned its keep twice: 97.301's tables
carry ITU Regions 1, 2 and 3 side by side and the United States is Region 2, so
reading Region 1 would have given 40 m as 7.000 to 7.200 and 75 m as 3.600 to
3.800; and a naive search for paragraph (b) lands first on a footnote reference
inside a table cell.

What moved: 40 m's landing spot from 7.030 to 7.028, and 30 m's from 10.110 —
which matched no cited source at all — to 10.103. The other five were already
the "CW main street" block they now derive from.

---
id: HM-OPEN-006
status: open
owner: tim
raised: 2026-08-14
severity: none
refs: FG-002, ONBOARDING.md ONB-C04, HM-DEC-058, src/Hamlet.RadioEngine/Explore/SpotRankWeights.cs
---

Hamlet has never asked the operator what Morse speed they can copy, so the spot
ranking may describe a station's sending speed and may not claim any speed suits
this person.

The ranking weighs sending speed where the source reports it, which RBN does.
What it cannot do is match that figure against the operator, because there is no
figure to match it against. A card reading "15 WPM, slow enough for you" would
be a confident match against a number nobody has ever measured, which is exactly
what §0.0 forbids, and it would be wrong in the direction that costs most: it
would send somebody to a contact they cannot make and let them conclude the
fault is theirs.

So the copy is descriptive and the preference in `SpotRankWeights` is a fact
about Morse rather than about this person: slower sending is easier for anybody
still learning, which is why the slow-speed clubs exist. `SpotRankingTests`
sweeps the reason lines for the phrasings that would cross back over.

What closes it is ONB-C04, which is the onboarding step that finds out, and its
own note says the honest form is probably a listening exercise rather than a
question: somebody who has never made a contact does not know what speed they
can copy either, and asking them to type a number invites a guess. FG-002 is the
other half, since a copy speed Hamlet knows is what turns the Elmer mode's
practice into something aimed at where this person actually is.

**Update 2026-08-14 (HM-DEC-066): the setting now exists and this stays open.**
The operator states a Morse speed in Settings, defaulting to 13, and the ranking
reads it. That is the weaker half of the answer. A stated preference is not a
measured ability, so what the app gained is permission to compare two stated
numbers and say a station is far over the one in the settings. It gained no
permission to say anybody can or cannot copy something, and `CopySpeedTests`
sweeps the composed card text for every phrasing that would.

What still closes this is ONB-C04, for the reason its own note gives: somebody
who has never made a contact does not know what speed they can copy either, and
asking them to type a number invites a guess. The setting takes an answer; the
listening exercise finds one out.

Severity `none`: the ranking works without it and says nothing untrue. It stays
open because the day somebody adds a speed filter without noticing this is the
day the app starts making a claim it cannot support. Nothing is filtered today
and a test holds that too.

---
id: HM-OPEN-007
status: open
owner: tim
raised: 2026-08-14
severity: none
refs: HM-DEC-060, HM-DEC-054, src/Hamlet.RadioEngine/Explore/Favorite.cs
---

Two questions about favorites that HM-DEC-060 deliberately did not answer.

**Do favorites ever sync to the radio's own memory channels?** The IC-7300 holds
ninety-nine of them and they survive being unplugged from the computer, which is
the one thing Hamlet's list cannot do. Writing to them would make a favorite
reachable from the radio's own front panel on a day the PC is switched off, which
is a real benefit to somebody who operates both ways.

Against it: memory channels are somebody's own, they may already hold things that
matter, and a program that quietly rewrote ninety-nine of them would be
unforgivable. If this is ever built it is one-way, explicit, per favorite, and it
says which channel it is about to overwrite before it does. It also needs its own
CI-V verification pass, since nothing about the memory commands has been read
from the manual yet.

**What happens to a favorite whose neighborhood changed underneath it?** A
favorite records what the map said when it was saved. The map is cited data now
and it will be corrected as sources are re-read (HM-DEC-054), so a favorite saved
as "14.070, PSK31 ribbons" could later sit in a block the file calls something
else. Three options, none obviously right: leave the saved text alone as a record
of what was true then, re-derive it every time it is shown, or show both and let
the operator notice. Leaving it alone is what the code does today, because it is
the only one of the three that cannot surprise anybody, and that is a default
rather than a decision.

Severity `none`: favorites work, nothing is wrong, and neither question has to be
answered before somebody uses them.

---
id: HM-OPEN-008
status: open
owner: unassigned
raised: 2026-08-14
severity: none
refs: HM-DEC-069, IC-7300 Full Manual p. 12-9 (publication A7292-4EX-5)
---

The IC-7300 will send its decoded RTTY out the USB port, and the manual never
says what those bytes look like.

It states that the setting exists, that "an RTTY decoded signal is output," and
that the rate is 4800, 9600, 19200 or 38400 bps with 9600 the default. It does
not say whether the characters are ASCII, what marks the end of a line, whether
anything frames or brackets the text, or how the decode screen's own display maps
onto what leaves the port. A read column-aware over the whole manual found no
fourth statement about it.

So a decoder written against it today would be guessing, and dressing a guess as
decoded text is what §0.0 exists to forbid.

What would close it is an observation rather than a document: setting USB Serial
Function to RTTY Decode, tuning an RTTY signal, and capturing the port. That
costs rig control for as long as it runs (HM-DEC-069), so it is an experiment
somebody chooses to do rather than something the app can find out on its own.

**Update 2026-08-14: open, and dormant.** Tim has ruled RTTY off the list
altogether and the thinking moved to FG-012. Nothing is waiting on this: if the
mode ever returns, the route recorded there is Hamlet demodulating the audio the
way it already does Morse, and that route never reads this port at all.

Severity `none`: nothing is blocked. HM-DEC-069 already rules that the mode is
not built, and for a reason this answer would not change on its own.

---
id: HM-OPEN-009
status: open
owner: tim
raised: 2026-08-15
severity: none
refs: HM-DEC-074, HM-DEC-049, src/Hamlet.RadioEngine/Cw/TransmitReadiness.cs
---

Holding TRANSMIT down is one of the three ways a command `17` message reaches the
air, and Hamlet refuses to send while the radio reports it is transmitting.

Footnote 2 on p. 19-7 says a CW message sent with `17` is transmitted when
TRANSMIT is on, **or** an external TX switch is on, **or** break-in is on.
`TransmitReadiness` returns `AlreadyTransmitting` and refuses whenever the radio
reports transmit status on, which closes off the first of those three.

The refusal is in the conservative direction: nothing goes out unexpectedly, and
break-in is the ordinary path for keyer sends and the one the panel now names.
So this costs an operator who prefers to hold the transmitter on himself, and it
costs nobody a signal they did not ask for.

Left alone deliberately on 2026-08-15 rather than fixed, because loosening a
transmit precondition hours before a live contact is not a change worth making
against a benefit this small (§0.2). What it needs is a decision about whether
Hamlet should distinguish "transmitting because the operator is holding it on",
which is permission, from "transmitting because a send is already in flight",
which is a reason to wait. The rig state model reads one flag for both.

**Update 2026-08-15 (HM-DEC-077): the refusal is now visible, which changes what
this costs.** Transmit status is checked before mode and before break-in, so it
refuses ahead of both, and until now nothing recorded that it had. Every readiness
evaluation now carries the transmit-status reading with its provenance and age, so
a session where this fired can be told from one where it did not, and it is one of
the things the next file will settle about the greyed-out buttons.

The ruling is unchanged and the gate is not loosened. What was missing was never
the strictness, it was the silence.

Severity `none`: the ordinary path works and the refusal explains itself.

---
id: HM-OPEN-010
status: open
owner: unassigned
raised: 2026-08-15
severity: slows
refs: HM-DEC-075, HM-DEC-038, FG-008
---

"Did anybody hear me" cannot say how far his signal went, because Hamlet has no
skimmer locations.

The reports carry the receiver's callsign, the signal-to-noise it measured and
the speed it read, which is what the RBN line format states. What would make the
panel land is the distance: "19 dB" means nothing to a newcomer and "your signal
reached Nevada, 2,050 miles" is the thing he would remember for the rest of his
life.

The obstacle is a ruling rather than an oversight. HM-DEC-038 says no grid means
no distance anywhere, and names this exact case: a callsign says where a license
was issued and not where its owner is standing, and stacking that guess under a
figure in miles would dress it as a measurement. So the prefix cannot be turned
into a location, and there is nothing else in the feed to use.

What closes it is a **cited file of skimmer locations under `data/`**, with a
source mark on every row in the shape `data/bands/` and `data/privileges/`
already use (§4). RBN's own node list is the obvious candidate and it was not
verified in this session. Skimmers that are not in the file get no distance,
exactly as a spot with no grid gets none today.

Severity `slows`: the panel works and says only true things, and it is doing half
of what it exists for.

---
id: HM-OPEN-011
status: open
owner: tim
raised: 2026-08-17
severity: slows
blocks: the real-signal regression corpus, and confirmation that HM-DEC-090 fixed the reported fault
refs: HM-DEC-090, HM-DEC-088
---

The three real captures HM-DEC-090 was written from are not in the repository.

The brief of 2026-08-17 described `cw-2026-08-16-225822`, `-225835` and
`-233446`, with hashes, tone frequencies and measured levels, and asked for all
of them to be committed as permanent regression fixtures. **They were never on
the machine the session ran on.** `%AppData%\Hamlet\captures` did not exist and a
search of the user profile and the repository found nothing.

Everything in HM-DEC-090 was therefore measured against synthesized audio built
to reproduce the one property those captures demonstrate: a strong narrow tone
present for a small fraction of the recording. That is a faithful stand-in and it
is not the evidence.

What is needed: the three WAV files and their sidecars, copied into
`tests/fixtures/cw/`. Once they are there, the decoder can be run against the
real thing and the claim that it now finds a tone near 627 Hz and 595 Hz can be
stated as a measurement rather than as a reasonable expectation. §2.1 makes an
off-air recording Tim's to review before it ships in a public repository, which
is the other reason this cannot be done without him.

---
id: HM-OPEN-012
status: open
owner: claude
raised: 2026-08-17
severity: slows
blocks: reading a real station, which is what the application is for
refs: HM-DEC-091, HM-DEC-090, HM-DEC-048
---

The keying gate's peak tracker cannot survive a station that keys five percent of
the time, and the fix that works breaks the one guarantee that cannot be traded.

**The mechanism, measured on `tests/fixtures/cw/captured`.** `CwGate` places its
threshold below a tracked peak that follows a signal down over a couple of
seconds, so a fade cannot strand it above the signal (HM-DEC-048). A station
answering a call sends short bursts seconds apart. Between them the peak decays
the whole way to the noise, `PeakDb - NoiseFloorDb` collapses to about eight
decibels against a `MinimumSpreadDb` of ten, and the gate stops deciding on a
signal that the narrowband measurement puts twenty-eight decibels above the band.
With the threshold that low the key also stays down through the gaps: eleven
seconds of key-down were measured in a recording containing roughly one and a
half.

It is the same duty-cycle fault HM-DEC-090 fixed in the reported ratio and the
located pitch, one layer further down.

**The fix that works.** Build the threshold from the held narrowband figure
rather than from the tracked peak: `NoiseFloorDb + heldSpread - drop`, with
`HasSignal` reading the same held figure. Measured:

- `cw-2026-08-17-013347`: one unreadable character becomes `I■E■N`
- `cw-2026-08-17-013622`: nothing becomes `■EI`
- key-down falls from 11.2 s to 7.1 s, marks from 138 to 72
- synthetic sensitivity improves from −4.0 dB to −5.0 dB

**Why it is not shipped.** It makes the decoder confidently wrong on
`fading-18wpm`, failing `NothingTheDecoderWasSureOfIsWrong`. A held peak is the
right answer for deciding whether a tone exists and the wrong one for deciding
where the threshold goes, because after a fade it strands the threshold above the
signal, which is exactly what the tracked peak was designed to prevent.

**Three narrower variants were tried and none is both safe and useful.** Falling
only while the key is down: two fade tests still fail. Falling normally unless
the held figure says a tone is still present: the held figure stays high through
a five-second fade, so it does not discriminate. Holding only once the power is
within six decibels of the noise: one fade test still fails. Holding only once
the tracked spread has already collapsed past `MinimumSpreadDb`: everything
passes and the real captures are unchanged, so it rescues nothing.

**What is probably needed.** The gate wants a threshold whose memory of the
signal is separate from its memory of the silence, which is the same shape of
answer as HM-DEC-090's held peak but applied to marks rather than to
measurements. Something like a peak over the last N *marks* rather than over
time. Whatever is tried, `fading-18wpm` and the two captures now fail in opposite
directions, so the corpus can tell a real fix from a trade.

---
id: HM-OPEN-013
status: closed
owner: tim
raised: 2026-08-17
closed: 2026-08-17
severity: slows
blocks: naming the CI-V USB Port setting as a reading rather than as a candidate
refs: HM-DEC-092, HM-DEC-093, HM-DEC-071, §4
---

**CLOSED 2026-08-17.** Tim supplied the citation: `1A 05 0074`, Full Manual
p. 19-5, "Send/read the CI-V USB port setting (00=Link to [REMOTE], 01=Unlink to
[REMOTE]) (Read only)". Recorded as FACT-002 in `SHACK_FACTS.md` and added to the
rig state model (HM-DEC-093). It is read so the precondition is a measurement and
never so that anybody is asked to go and look at it.

Is `1A 05 0074` the CI-V USB Port setting, and on which page of `A7292-4EX-6`?

The brief of 2026-08-17 states that `1A 05 0074` reads it, `00=Link to [REMOTE],
01=Unlink from [REMOTE]`, and asked for it to be added to the rig state model
with its citation. **It has not been added, because §4 requires a page number
from a column-aware read of the settled edition and this session had no access to
the manual.**

That discipline is not ceremony here. The command table is two columns, a
flattened read is what put the CW pitch on `14 08` instead of `14 09`, and that
error survived for weeks and would have moved somebody's passband while trying to
read a pitch (HM-DEC-050, HM-DEC-071). A sub-command taken on trust is exactly
the same shape of mistake.

What is needed: the row confirmed against `A7292-4EX-6` with a column-aware
extraction, and its page. It is almost certainly in the `1A 05` settings block
around pp. 19-4 to 19-6.

**What it unlocks.** The scope's data output has two documented preconditions
(p. 19-7, footnote 4). Hamlet knows the baud rate because it opened the port
itself. With this row it would know the other, and the refusal message could name
which condition failed as a reading rather than offering the remaining candidate
as something left to check (HM-DEC-092).

---
id: HM-OPEN-014
status: open
owner: claude
raised: 2026-08-17
severity: none
refs: HM-DEC-093, §8
---

`TheDecoderAggregationDoesNotAllocatePerCharacter` fails under concurrent load.

Seen once on 2026-08-17 while two `dotnet test` processes were running at the
same time, and passing on every isolated run and on three consecutive clean full
runs afterwards. It measures allocation with `GC.GetAllocatedBytesForCurrentThread`
and asserts a ceiling, which is a real and worthwhile property (§8: the decoder's
own record may not allocate per character) measured in a way that another busy
process can disturb.

Not urgent and not ignorable: it will flake in CI on a shared runner, and a
guard that cries wolf is one somebody eventually reruns without reading. Worth
either widening the ceiling with a stated margin or forcing the test onto its own
xUnit collection so nothing runs beside it.

---
id: HM-OPEN-015
status: open
owner: tim
raised: 2026-08-17
severity: slows
blocks: items 3, 3b and 4 of the 2026-08-17 work order
refs: HM-DEC-094, HM-DEC-090
---

The decoder work in the 2026-08-17 brief cannot start: three of its five
captures and both reference documents are absent from this machine.

The brief cites `CW_RECEIVE_BRIEF.md` and `cwdecoder.py` for the validated
receive chain, and names five captures. Present: `cw-2026-08-17-013347` and
`cw-2026-08-17-013622`. **Absent: the 22:58 pair, the 13:47 interference
capture, the 23:26 group, and both documents.**

What is blocked, and why each needs what is missing:

- **Item 3, the tone detector's frequency.** The comparison table covers four
  captures and two of them are not here. A detector tuned against the two
  present ones would be tuned against a quarter of the evidence.
- **Item 3b, the two-stage Goertzel chain.** The reference implementation and
  its measured parameters are in `cwdecoder.py` and `CW_RECEIVE_BRIEF.md`.
  Reimplementing from the brief's summary would be guessing at the numbers that
  matter, which is what the 20 Hz ENBW figure exists to prevent.
- **Item 4, interference.** The 501 Hz carrier lives in the 13:47 capture. There
  is nothing here to detect it in, and a detector for a thing nobody can
  reproduce is untestable by construction.

The brief's own warning applies and is worth repeating: the operator heard CW in
the 13:47 capture that independent analysis could not find. Human copy at low
signal-to-noise beats automatic detection, and an analysis finding nothing is not
evidence that nothing is there.

---
id: HM-OPEN-016
status: open
owner: claude
raised: 2026-08-17
severity: hard
blocks: merging feature/honest-cw-detection, and sessions 2 and 3 of the batch brief
refs: HM-DEC-095
---

The keying-structure detector regresses eleven tests against synthesized
fixtures, and must not merge until they pass or are shown to be wrong.

Nothing here is a real-signal failure. Every one is a synthetic fixture, and
the common cause is architectural: the old tracker retuned to the loudest bin
five times a second, which is perfect for one clean strong tone and wrong on
every real recording. The new survey wants three seconds of keying evidence
and two agreeing readings before it moves, which is right on the air and slow
on a fixture that lasts eight seconds.

| Test | What it does now |
|---|---|
| `ASignalAtTheWrongPitchIsStillFound` (400, 875 Hz) | `■ DE W1AW K` — loses the opening character to acquisition |
| `ASignalAtTheWrongPitchIsStillFound` (500, 750 Hz) | `■ B ■AW K` — worse, and `B` is a wrong character rather than a placeholder |
| `ACleanSignalDecodesExactly` (25 wpm) | fails; 12 and 18 pass |
| `TheCleanRecordingsDecodeExactly` / `EveryRecordingGivesBackTheShareItShould` (clean-25wpm) | as above |
| `TheSpeedEstimateFollowsAChangeWithinAFewCharacters` | speed adaptation across a change |
| `AFadingSignalComesBackRatherThanStayingDead` | fade recovery |
| `TheDecoderReadsAsFarDownAsItDidBefore` | reads to −2 dB and below, but the 17 and 18 dB rows are worse than the 10 dB row |
| `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone` (app) | transcript content after a clear |

**The sensitivity one is the most interesting and should be read before the
others.** The decoder still returns better than half the characters correct at
minus two decibels. What broke is the top of the range: eighteen decibels out of
the noise returns a third right and a third wrong, which is worse than the same
decoder manages at ten. A strong signal failing where a weak one succeeds is not
a sensitivity problem, it is something firing on strong signals that does not
fire on weak ones, and the two candidates already found and fixed in that family
were the window switching without hysteresis at eighteen words a minute and the
speed being discarded whenever the tracker moved.

**The 500 and 750 Hz cases deserve their own look.** Those two produce a wrong
letter where 400 and 875 produce a placeholder, and they are the two nearest the
600 Hz starting pitch, which suggests the fine bank is being left straddling the
signal rather than moved onto it.

What must not be done to close this: loosening the separation limit, the
confirmation rule, or the plausibility bounds. Those are what stop a carrier
being announced as a station, and every one of them was set from a measurement
with margin on both sides (HM-DEC-095).

---

**ATTRIBUTED 2026-08-18, and the count is nine rather than eleven.** The two
`clean-25wpm` rows retired with that fixture. Each remaining failure was tested
rather than reasoned about, by giving the decoder a run-up of Morse before the
message under test and seeing whether the message then decoded. The theory in
the paragraph above says it should, and for six of the nine it does.

**SIX ARE THE FIXTURE, and all six are the same fixture fault: the signal is
too short for a detector that wants three seconds of keying before it moves.**

| Test | Bare | With a run-up |
|---|---|---|
| wrong pitch, 500 Hz | `■■EIW K` | `CQ DE W1AW K`, exactly |
| wrong pitch, 750 Hz | `■ ■ ■ ■ AW K` | `CQ DE W1AW K`, exactly |
| wrong pitch, 875 Hz | `■ DE W1AW K` | `CQ DE W1AW K`, exactly |
| clean, 25 wpm | `CQ D■ W1AW K` | `CQ DE W1AW K`, exactly |
| fade recovery | 0 letters in the last third | 7, against the 3 it asks for |
| speed after a change | 10 characters, final 24 wpm | 10 characters, final 24 wpm |

**The pitch is found correctly in every one of those**, to the hertz: asked to
start at 600 and given 400, 500, 750 or 875, the tracker lands on exactly the
right number. The test that fails is the text, not the pitch it is named for,
and what is lost is the characters that arrive while the detector is still
gathering its evidence.

**The speed one is not even that.** Its final estimate is 24 words a minute,
inside the 23 to 27 the test demands, and exactly ten characters arrive after
the change. The test then takes `Skip(10)` and asserts the remainder is not
empty, so it fails on an off-by-one in its own margin rather than on anything
the decoder did.

**THREE ARE HAMLET, and they are two distinct faults.**

- **`ASignalAtTheWrongPitchIsStillFound` at 400 Hz** is the only pitch that a
  longer signal does not fix. With twelve groups of run-up it still returns
  `■V VVV VVE ■ ■V VVV VVV VVV VVE ■ ■V ■ KB■ K`, breaking down and
  re-acquiring over and over. The tracker reports 400 Hz correctly throughout,
  so it finds the pitch and will not hold it.
- **`ClearingTheTranscriptLeavesTheDecoderAlone`** is the same shape from the
  other end. It runs at 12 words a minute at the expected pitch, where the bare
  fixture decodes perfectly, and it feeds fourteen seconds of a repeating
  message. Given a run-up, 12 wpm degrades from exact to `CQ D■ W1AW K`. **A
  longer signal decoding worse than a short one** is the observation
  HM-OPEN-016 already flagged as the interesting one, and this is a second
  instance of it away from the sensitivity sweep.
- **`ItGoesQuietRatherThanInventingLettersInTheNoise` is a ruling that was never
  built**, and it needs one more ruling before it can be. See below.

---

**THE SENSITIVITY ONE IS A SEPARATE PROBLEM AND IS NOT A REGRESSION AT ALL.**

The sweep reproduces HM-DEC-097's own published figures: perfect from 18 dB
down to 1 dB, and at minus two decibels a full message of which 0.44 is
invented. That ruling says the decoder **refuses below 0 dB** rather than
copying into the band where it is half wrong. Nothing in the decoder does that.
There is no SNR floor: the streaming pass gates on coherence and a plausible
speed, and the settled pass on six decibels of contrast, and neither is the
floor the ruling describes.

**And it cannot simply be added, because the ruling is stated in a unit the
decoder cannot measure.** HM-DEC-097's decibels are the broadband ratio the
fixture was generated at. The decoder measures inside a narrow tone filter and
reads about seventeen decibels higher for the same audio:

| Generated | What the decoder calls its own margin |
|---|---|
| 12.0 dB | 28.8 to 31.0 |
| 0.0 dB | 17.2 to 19.0 |
| −2.0 dB | 15.3 to 17.1 |
| −5.0 dB | 7.6 to 14.4 |

So implementing the floor means choosing what the decoder's own margin
corresponds to nought decibels broadband. That number decides what the display
asserts and is not a session's to pick (§12.1). The proposal is in the
2026-08-18 `OUTPUT.md`.

**Nothing was loosened to make any of this pass**, and no bound was moved. The
six fixture cases stay red until their fixtures are rebuilt long enough for the
detector, which is generator work rather than decoder work.


---
id: HM-OPEN-017
status: open
owner: tim
raised: 2026-08-17
severity: hard
blocks: finishing session 1 of the batch brief
refs: HM-OPEN-016, HM-DEC-095, HM-DEC-048, CW_RECEIVE_BRIEF.md, cwdecoder.py
---

The validated reference decodes in two passes over a whole recording and
Hamlet decodes in one pass as the audio arrives. Which of those Hamlet is
supposed to be is a ruling nobody has made.

`CW_RECEIVE_BRIEF.md` says to port the reference's behavior and not its
structure. Three attempts at that this session each made the real recording
worse, and the reason is that the behavior is not separable from the
structure:

**The reference fits the element clock and then goes back and re-reads the
whole recording with it.** `run()` de-glitches at twenty milliseconds, extracts
the runs, fits the clock from them, then de-glitches again at four tenths of a
dit and extracts every run in the recording a second time before a single
character is decoded. Its gate does the same thing at a coarser grain: it walks
the entire envelope in overlapping three-second blocks and fits a threshold to
each before any of them is used.

**Hamlet's decoder cannot do that and still be Hamlet.** It measures one hop at
a time, commits each element as it ends, and emits characters while the operator
watches. There is no second pass available, because the second pass would have
to run over audio that has already been shown to somebody. Everything Hamlet
holds back is latency on a live screen.

Measured this session, grafting the reference's pieces onto the streaming chain
one at a time:

| Change | Capture 1 (`013347`) |
|---|---|
| Start of session | `■ ■` — two characters |
| Fine bank read whole, loudest bin per hop | `■   ■W■RR ■` — seven characters |
| Plus the reference's clustering gate | `W■■` — three characters |
| Reference decoder itself, batch | `▯ ▯ ▯ ▯ ▯ ▯ MVRRVA3VRR`, confidence 0.74 to 1.00 |

The middle row is kept. The clustering gate is reverted, and it is not that the
gate is wrong: it is the right gate for a decoder that can fit a threshold to a
block and then apply it to that same block, and the wrong one for a decoder that
has to answer before it has heard the block.

**Three ways out, and the choice is not Claude's:**

- **Decode twice.** Keep the streaming pass for what is on screen now, and run a
  second, batch pass over the last half minute whenever the tap has it, revising
  the transcript behind the cursor. Honest about what it is doing and the most
  work; it also means characters change after they are displayed, which is its
  own §0.0 question.
- **Delay the display.** Hold everything for three seconds and decode the block
  that has just closed. Simple, matches the reference exactly, and puts a
  three-second lag on the one screen where the operator is trying to keep up with
  a live contact.
- **Accept the streaming approximation.** Take what a single pass can do, which
  today is about seven characters out of eleven with the rest as placeholders,
  and say so on screen. Nothing here breaks §0.0 — every character it will not
  stand behind is already a placeholder — but it does not meet the brief's own
  definition of done.

**What is not in doubt** is that the reference is right about this audio and
Hamlet is not yet. Its answer, `MVRRVA3VRR` at high confidence, matches the
independent hand analysis, and it is what the operator needs on the evening he
uses this.

---
id: HM-OPEN-018
status: open
owner: claude
raised: 2026-08-17
severity: slows
refs: HM-OPEN-016, HM-DEC-095, HM-DEC-048
---

The synthesized fixtures have no noise floor, which makes them unrepresentative
of every real receiver, and the reference decoder scores zero on all of them.

Run `cwdecoder.py` against this repository's own fixtures and it decodes nothing
at all:

| Fixture | What the reference does |
|---|---|
| `clean-12wpm` | active 20%, no clock, emits nothing |
| `clean-18wpm` | active 11%, no clock, emits nothing |
| `clean-25wpm` | **active 0%**, no tone found at all |
| `prosigns-18wpm` | active 10%, no clock, emits nothing |

The cause is the same one that cost this session an afternoon. Those fixtures
are tone-or-silence: between elements the samples are exact digital zero, which
measures about minus two hundred and forty decibels. Any transmit-mute guard
reading a level that low as "the receiver is muted" blocks the gaps between
every element, and there is nothing left to decode. The reference has no lower
bound and blocks all of them. Hamlet now has one at minus ninety, measured from
the real captures where the mutes bottom out around minus eighty-two, and passes
the ones the reference fails.

**A real receiver never hands over digital silence.** There is always band noise,
which is why `noisy-18wpm` and `fading-18wpm` are unaffected and why every
failure in HM-OPEN-016 is against a noiseless fixture. The fixtures encode an
assumption about the audio path that the audio path does not have.

`CW_RECEIVE_BRIEF.md` §4 anticipates this and specifies a replacement recipe:
noise shaped to a 500 Hz passband, 3 dB in-passband SNR, two-path QSB, an
interfering carrier, and a preamble of QSK-style mutes **at minus ninety
dBFS** rather than at zero. Building it is a session's work on its own and it
would replace, not join, the noiseless fixtures.

Recorded rather than acted on. Making failing tests pass by rewriting their
fixtures is exactly the move that deserves suspicion, and the case for it here
rests on a measurement anybody can repeat: run the reference against them.

---
id: HM-OPEN-019
status: open
owner: tim
raised: 2026-08-17
severity: slows
refs: HM-OPEN-018, HM-DEC-097, FIXTURE_BRIEF.md
---

Phase 6's retirement assessment ran and **nothing qualified**, so both fixture
sets stay in place.

`FIXTURE_BRIEF.md` phase 6 asks for superseded fixtures to be retired one at a
time with a reason each, and names leaving both sets in place as the safe
state. That is where this landed, and the reasoning is worth keeping so the
next session does not redo it:

| Old fixture | Tests | Retired? | Reason |
|---|---|---|---|
| `clean-12wpm` | all pass | no | Passing. Retiring it removes coverage and gains nothing |
| `clean-18wpm` | all pass | no | Passing |
| `prosigns-18wpm` | all pass | no | Passing, and the new `prosigns-*` set does not yet read well enough to replace it |
| `noisy-18wpm` | all pass | no | Passing |
| `fading-18wpm` | all pass | no | Passing |
| `interference-18wpm` | all pass | no | Passing |
| `clean-25wpm` | 2 fail | **no** | The only twenty-five words a minute coverage in the repository. The rebuilt suite is at twelve, so retiring this deletes the fast-fist case rather than replacing it |

**The one that fails is the one with no replacement**, which is the shape that
makes retirement destroy evidence rather than tidy up. A twenty-five word a
minute fixture on realistic audio would close it, and the measurement to do
that first is already in `CwAdjudicationTests`: at that speed Hamlet reads
`■ALL N0CALL K E` off a realistic signal against nothing at all off the
noiseless one, so the scenario is real and the fixture is what was wrong.

---
id: HM-OPEN-020
status: open
owner: tim
raised: 2026-08-17
severity: slows
refs: HM-OPEN-018, cwdecoder.py, CW_RECEIVE_BRIEF.md
---

The reference chain measures every mark about twenty-five milliseconds long,
and at high contrast that pushes a real fist below its own ratio floor.

Measured across the rebuilt fixtures, on the fist recorded off the air — dit
105 ms, dah 283 ms, a true ratio of **2.70**:

| Gate contrast | Measured dit | Measured dah | Ratio | Read? |
|---|---|---|---|---|
| 10 dB | 109 | 295 | 2.70 | yes |
| 13 dB | 112 | 294 | 2.63 | partly |
| 22 dB | 128 | 305 | **2.39** | **refused** |

The cause is arithmetic rather than mysterious. A twenty hertz detection
bandwidth needs a fifty millisecond window, and a window that long smears each
keyed edge over about the same. The gate crosses its threshold early on the
rise and late on the fall, so every mark measures long by roughly a constant,
and adding a constant to both lengths compresses their ratio. The stronger the
signal, the lower the threshold sits relative to the peak, and the earlier it
crosses.

`cwdecoder.py` refuses any clock outside 2.5 to 3.8, so **it refuses at fifteen
decibels a fist it reads perfectly at zero**. Three rebuilt fixtures are held
out of the phase 4 gate for this and none of them was edited to get round it:
`tightfist-easy`, `tightfist-working`, `qsk-preamble`.

This matters beyond the fixtures. `FIXTURE_BRIEF.md` phase 4 says a fixture the
reference cannot decode is a bad fixture rather than a Hamlet failure, and here
**the fixture is a measurement taken off the air and the floor is what fails**.
The rule is right about the common case and this is the case it does not cover.

Two things worth settling, neither of them Claude's:

- whether the 2.5 floor should be widened, or the bias corrected before the
  ratio is taken, in Hamlet's own clock fit — which uses the same floor
  (`CwToneSurvey.MinimumRatio`) and will meet the same wall;
- whether a held-out list is the right shape for phase 4's exception, or
  whether the gate should record a reason per fixture instead.

A separate finding from the same work, recorded so it is not rediscovered:
**five dahs in a row do not survive a tight fist.** `N0CALL` contains `0`, and
at sixty-five millisecond gaps read through a fifty millisecond window those
five dahs merge into a single mark of about one and two thirds of a second,
which dominates the clock fit and collapses it. The reference read
`K W BG EN` out of `N0CALL N0CALL`. The tight-fist message avoids digits for
that reason and the case deserves a fixture of its own.

---
id: HM-OPEN-021
status: open
owner: tim
raised: 2026-08-17
severity: slows
refs: HM-OPEN-019, HM-OPEN-020, HM-DEC-101, HM-DEC-103, HM-DEC-105, GENERATOR_BRIEF.md
---

The eleven re-adjudicated against the corrected fixtures. **Every entry decided
last session against defective audio has been re-tested and only one verdict
moved.**

Phase 6 of the generator brief. The table in HM-OPEN-019 predates the generator
fix and this supersedes it.

| Test | Fails on realistic audio? | Fault |
|---|---|---|
| `ASignalAtTheWrongPitchIsStillFound` (400) | no — found at 400, reads `N0CALL K` | fixture |
| `ASignalAtTheWrongPitchIsStillFound` (500) | no — found at 525, reads `■0CALL N0CALL K` | fixture |
| `ASignalAtTheWrongPitchIsStillFound` (750) | no — found at 750, reads `CALL N0CALL K` | fixture |
| `ASignalAtTheWrongPitchIsStillFound` (875) | no — found at 875, reads `DE N0CALL N0CALL K` | fixture |
| `ACleanSignalDecodesExactly(25)` | partly — reads `■ALL N0CALL K E` against nothing on the noiseless one | mostly fixture |
| `TheCleanRecordingsDecodeExactly(clean-25wpm)` | as above; **not retired**, its replacement `fast-easy` does not pass the gate | mostly fixture |
| `EveryRecordingGivesBackTheShareItShould(clean-25wpm)` | as above | mostly fixture |
| `AFadingSignalComesBackRatherThanStayingDead` | **yes** — on a twelve decibel fade it reads `■ ■ ■ S■ ■ F■ R D E` where the reference reads 53% | **Hamlet** |
| `ItGoesQuietRatherThanInventingLettersInTheNoise` | no — nothing emitted at −3, −6 or −10 dB | fixture; bound predates HM-DEC-097 |
| `TheSpeedEstimateFollowsAChangeWithinAFewCharacters` | **now adjudicated** — see below | **Hamlet**, and needs a ruling |
| `ClearingTheTranscriptLeavesTheDecoderAlone` (app) | no — 12 wpm at 625 Hz before and after the clear, 8 characters then 23 | fixture |

**Only the fade verdict was at risk and it survived.** It was attributed to
Hamlet last session on audio carrying a twenty-five decibel fade that deleted
most of the message rather than fading it. The fade is now twelve decibels, the
reference reads 53 percent of the same file, and Hamlet still returns mostly
placeholders. The attribution stands on better evidence than it was made on.

**The two that had never been adjudicated now have been**, which was the point
of HM-DEC-104. Clearing the transcript is a fixture fault. The speed estimate is
not, and it found something nobody had looked for.

---
id: HM-OPEN-022
status: open
owner: tim
raised: 2026-08-17
severity: slows
refs: HM-OPEN-021, HM-DEC-090, HM-DEC-104
---

Across a change of station the decoder names sending speeds belonging to
neither of them.

Measured on the two-station recording, a caller at eleven words a minute
handing over to an answerer at twenty-two:

- it names **16 and 18**, which is the average of the two and describes nobody;
- it names **24, 26, 27, 28, 29, 30, 31, 34, 36, 37, 41, 42 and 44**, all faster
  than either station;
- it comes to rest correctly, naming no speed at all rather than a wrong one.

Where it settles is already covered and asserted. What is not settled is whether
a streaming decoder may put a transitional speed on screen at all while its
clock re-acquires, or must withhold one until the new clock is proved.

HM-DEC-090 already ruled **one guarded answer, read by every surface**, on the
grounds that a speed is a fact about somebody's keying. A number that belongs to
no station on the band is not that, and it appears on the one screen a beginner
uses to judge whether they could have copied the exchange.

This is a question about what the display asserts, so §12.1 reserves it. Two
shapes it could take, neither of them Claude's to choose:

- **withhold** — name no speed between clock loss and the next confirmed clock,
  which costs nothing but a gap and matches the refusal machinery already in
  place;
- **mark it** — show the transitional reading as unsettled, the way the
  provisional tip already distinguishes itself from settled text.

---
id: HM-OPEN-023
status: open
owner: tim
raised: 2026-08-17
severity: slows
refs: HM-DEC-105, HM-OPEN-020
---

The half-amplitude correction is applied in the settled pass and deliberately
not in the tone survey, and the second half is unresolved.

HM-DEC-105 ruled that the dah-to-dit floor stays at 2.50 and what the ratio is
computed over gets fixed. In the settled pass that lands cleanly: deciding six
decibels below the keyed level rather than midway between the two clusters
measures a mark at the length it was, and unresolved characters fell from 73 to
50 percent on `exchange-easy` and from 75 to 33 on `coverage-easy`.

Applied in `CwToneSurvey` as well it costs:

- five noiseless fixtures, which is the class HM-OPEN-018 established encode a
  physical impossibility and would be weak evidence on its own; and
- **the real 13:47 off-air capture**, where the tone stops being found at all.
  That is not a fixture argument.

The reason is that the two are answering different questions. The settled pass
measures how long a mark is, and half amplitude is where an element's true edge
sits. The survey decides whether a bin holds anybody keying, judged on the
separation between two clusters of mark durations; moving the decision up the
leading edge shortens every mark and tightens that separation, which is exactly
the measurement the survey exists to make.

A correction that improves one measurement and breaks another is not one
correction. Whether the survey should be corrected differently, or left alone,
is Tim's.

---
id: HM-OPEN-024
status: closed
owner: claude
raised: 2026-08-18
closed: 2026-08-19
severity: none
refs: tests/Hamlet.App.Tests/ViewModels/BandCardStyleTests.cs, src/Hamlet.RadioEngine/Bands/HfBands.cs, HM-DEC-110
---

Three band-card style tests failed once in a combined run and have not failed
since: `DimmingIsVisible`, `StylesAreDeterministic` and
`WithoutALocation_NothingDimsAndNothingClaimsTheSun`.

Seen once, on 2026-08-18, in a `dotnet test` across both projects. Not
reproduced in the two combined runs and three app-only runs that followed, so
it is recorded rather than chased.

**The leading suspicion is the band plan going lazy** (HM-DEC-110). Those tests
share a static `AllBands` initialized from `HfBands.Bands`, which is now a
`Lazy` over two more lazies, `PrivilegeData.Current` and
`NeighborhoodData.Current`, each of which reads an embedded resource. Before
the retirement `BandPlan.Bands` was a plain array literal and could not be
empty or late. An empty `AllBands` would make all three of those assertions
fail together and leave every other test untouched, which is the shape that was
seen.

What would settle it: make the three tests state their own band list rather
than deriving one, or prove the lazy is safe under xunit's parallel
collections. It is `none` because the app builds the same list once on the UI
thread and no failure has been seen outside a parallel test run.

**Not chased further on purpose** (§12.6). It surfaced while running the
scanner end to end and is unrelated to that work.

**A second one, seen once, 2026-08-18.** `TheStopFrameIsCommand17CarryingFf`
failed in one full run and passed alone and in the two full runs either side of
it. It shares nothing with the band list, so what these two have in common is
only that both are intermittent and both appear under xunit's parallel
collections. Named and left (§12.6): one occurrence is not a diagnosis, and a
transmit test that fails intermittently is worth knowing about before it fails on
an evening that matters.

**A FOURTH SIGHTING, 2026-08-18, AND ITS NAMES WERE NOT CAPTURED.** One combined
run of both assemblies reported five app failures where every run either side of
it reported one; four consecutive app-only runs and three further combined runs all
gave the steady figure. The grep in use at that moment printed summary lines only,
so **which four they were is not known**, which is worth less than a named sighting
and is recorded rather than tidied away or guessed at. The pattern is the same as
the other three: intermittent, under xunit's parallel collections, in no one
subsystem.

**A THIRD, AND THE FIRST HAS NOT RECURRED, 2026-08-18.**
`RigReadTests.EachSettingParsesToTheManualsOwnWords` failed once in a full run and
passed alone and in every run either side; `TheStopFrameIsCommand17CarryingFf` did
not fail once across six full runs this session. So all three are still
intermittent rather than any of them becoming reliable, and they share nothing but
running under xunit's parallel collections. Still named and left (§12.6): three
single occurrences in three different subsystems is a property of the harness, and
chasing it from inside any one of them would be chasing the wrong thing.

**CLOSED 2026-08-19. The cause was the runner rather than the band plan, and it is
fixed for the whole assembly.**

The suspicion recorded here was a lazily-initialized static shared between those
three tests. What it actually was is simpler and covers the other sighting too:
**xUnit runs test classes in parallel, and an Avalonia headless test runs on one
process-wide dispatcher.** Several tests take turns on a thing there is only one
of, and under load one of them loses. Two classes in this assembly also set
`LayoutStore.Path`, a mutable static, for the same reason.

`tests/Hamlet.App.Tests/TestParallelism.cs` disables parallelization for the
assembly. It costs about two seconds — the app suite runs in four rather than two
— and three consecutive full runs afterwards reported the same two standing
failures each time, which is the point: **a suite that invents four failures under
load is a suite whose red count nobody reads.**

**The engine assembly is untouched and HM-OPEN-014 is not covered by this.** That
one measures allocation against a ceiling and is disturbed by another busy
process, which is a different fault with a different repair.

---
id: HM-OPEN-025
status: open
owner: tim
raised: 2026-08-18
severity: none
refs: HM-DEC-113, CLAUDE.md 9.5.1
---

Something on the development machine commits as `"save"` while a session is
running.

It caught the 2026-08-17 session's phase 1 work at `20c8ae5` and discarded the
commit message that session had written. Harmless to the content, which was
committed whole, and corrosive to the history: a one-word message on a change
that carried a measurement and its reasoning.

Recorded rather than chased, per the work order. Whatever it is — an editor
plugin, a file watcher, a scheduled task — it is Tim's machine and Tim's to
identify. What matters here is that a session cannot rely on its own commit
boundaries being the ones that end up in the log, and a report saying "committed
as X" may not describe what is on disk afterwards.

**Confirmed still cosmetic and still a single occurrence, 2026-08-18.** `20c8ae5`
is the only one-word `save` in the whole log; its diff is five files and 430
insertions, all of it the work that session did, so nothing was lost. This
session made four commits and all four kept the messages they were written with.
Whatever it is has not fired again, and nothing was found that would show it had.

---
id: HM-OPEN-026
status: closed
owner: tim
raised: 2026-08-18
closed: 2026-08-18
severity: slows
blocks: three of the four off-air fixtures the cleanup order asked for
refs: CLAUDE.md 2.1, HM-DEC-091, HM-DEC-126, tests/fixtures/cw/captured
---

**CLOSED 2026-08-18 BY HM-DEC-126: unobtainable, and this entry reopens if the
file appears.** Asked across four sessions without it arriving, and a sweep of the
tree confirmed nothing in the fixture set names it, so no fixture rests on absent
evidence.

**AND THE GAP IT LEAVES IS RECORDED RATHER THAN CLOSED WITH IT.** This suite has
**no regression test for a success at all.** Every ratchet in it is a ratchet on a
failure getting less bad: the settled pass reaching further into a callsign, the
bulletin's distance from its key shrinking, a tier coming back with fewer
strangers. Nothing in it asserts that something Hamlet reads correctly today is
still read correctly tomorrow, so **nothing in it can tell a repair from a
coincidence**. `cw-2026-08-18-003758` would have been the first, because Hamlet
read `DE AA4MP/4 QNIK` off it and somebody confirmed that independently. That is
worth naming without a candidate to fill it.

Three of the four 2026-08-18 off-air captures are not on the machine.

The work order names four and expects them "on `main` already or supplied
alongside". Only `cw-2026-08-18-004507` is present, which is the ARRL bulletin
that produced HM-DEC-115. Missing:

| Capture | What it would prove |
|---|---|
| `cw-2026-08-18-003758` | **A regression test for a success**, which the suite has none of. Hamlet read `DE AA4MP/4 QNIK` off this on screen and it was independently confirmed correct |
| `cw-2026-08-18-003126` | The half-amplitude evidence behind HM-DEC-112, and a key containing `<BT>`, `<AR>`, `VFB`, `MY`, `IT` |
| `cw-2026-08-18-003016` | Tone tracking only; no full key |

Searched for under `tests/fixtures/cw/captured`, the whole repository,
`%AppData%\Hamlet\captures` and Downloads. Nothing.

**The rule permits them and this is not a permission question.** §2.1 says
recorded off-air audio is public by nature and asks only that fixtures
committed to the public repository are reviewed by Tim first, which he did when
he committed `004507`. They are simply not here.

`003758` is the one worth chasing first. Two of these are the only evidence the
project has that Hamlet has ever read a real station correctly, and a suite with
no regression test for a success cannot tell a repair from a coincidence.

**Re-checked 2026-08-18, and the fixture set does not name it.** The work order
asks whether the reference should be removed, because the fixture records must
not name evidence that does not exist. Swept across the whole tree: `003758`
appears in this entry, in `OUTPUT.md`, and in the work order that asked the
question. **No test, no sidecar, no catalogue entry and no assertion refers to
it**, so nothing in the fixture set rests on a file that is absent and the
property the work order wanted is already true.

What remains is only this question. **The recommendation is to close it**: it has
been asked across four sessions, the file has not appeared, and an open issue
nothing depends on is a question with no work behind it. Closing it costs the
project the regression test for a success it has never had, which is a real loss
and is why the recommendation is not a decision. If the file turns up it is
committed and the entry is reopened.

---
id: HM-OPEN-027
status: closed
owner: tim
raised: 2026-08-18
closed: 2026-08-18
severity: slows
blocks: HM-DEC-116, which stays blocked until this is closed
refs: HM-DEC-116, HM-DEC-121, HM-DEC-123, HM-DEC-128, HM-OPEN-028, HM-OPEN-032, src/Hamlet.RadioEngine/Cw/CwDecoder.cs, src/Hamlet.RadioEngine/Cw/CwToneTracker.cs
---

**CLOSED 2026-08-18 BY HM-DEC-128, WHICH SUPERSEDES THE RULING THIS ENTRY WAS
BLOCKING.** Both halves are answered and neither is a question any more.

The coupling this entry traced is gone: HM-DEC-123 built the refining-versus-
following distinction, and with adoption applied on top of it
`cw-2026-08-17-013347` produces **three moves and one follow, identical to
adoption off**, where the whole diagnosis was that adoption turned one retune into
three. `MidCharacter` costs nothing because a refinement resets nothing.

And the ruling it was blocking is superseded rather than unblocked: HM-DEC-116's
premise dissolved when the streaming estimator began reading `CwGapFit`, so the
choice it was making no longer exists. **Confirmed by sweep 2026-08-18: no
`Adopt`, no `ForgetAdopted` and no adoption flag remains anywhere in the engine.**

**RULED, AND THE WORK IS ITS OWN ORDER: HM-DEC-123.** A retune that refines the
pitch of the station being read no longer resets the settled window; one that
follows a different station does. No session begins that in passing. This entry
stays open because the code is unchanged, and it is no longer a question — it is
queued work. The same ruling answers HM-OPEN-028. Seen again 2026-08-18 from a
third direction, HM-OPEN-032: any change to where the streaming pass divides
characters moves `MidCharacter` and pays this cost.

---

**BUILT 2026-08-18 AND THIS HALF IS ANSWERED.** `CwToneTracker.Follows` counts the
moves that go to a different station, the decoder acts only on those, and
`TheSettledPassNoLongerStopsShortOfTheCallsign` is green: the settled pass reads
`■■■ ■■VA3VRR` where it read `■■■ ■`.

**The criterion was measured and it is the survey's own grid.** Every move within
one station across every recording here is exactly one coarse bin — the capture's
two moves are 625 to 600 and 600 back to 625, the bulletin's is 525 to 500 — and
the one genuine station change, the caller at 615 handing to the answerer at 730,
is a hundred. Nothing lies between them. `ConfirmWithinHz` already carried that
number for the neighbouring question, whether two consecutive surveys are the same
signal, and its own note already called it "a station drifting or the survey
preferring its neighbor, rather than a different signal". A tracker that has not
yet reported a pitch has nothing to refine, so its first move is a follow.

The measurement is against the bank the tracker listens through rather than the
pitch it last reported: the fine bank answers a few hertz outside its own centre —
730 through a bank centred at 725 — and measuring from the report would make one
bin read as one and a bit.

---

**AND HM-DEC-116 WAS RE-ATTEMPTED ON TOP OF IT AND IS STILL NOT SHIPPED, FOR A
DIFFERENT REASON.** The chain this entry traced is genuinely broken: with the
streaming pass adopting the settled classes, `cw-2026-08-17-013347` shows **three
moves and one follow, identical to adoption off**, where the whole of HM-DEC-121's
diagnosis was that adoption turned one retune into three. `MidCharacter` no longer
costs anything, because a refinement no longer resets anything.

**The new path is direct and it is about the classes themselves.** Adoption now
changes only where the streaming pass divides characters, and on the two
recordings where it fires the settled pass's classes are the worse of the two
fits:

| | adoption off | adoption on |
|---|---|---|
| `013347` settled | `■■■ ■■VA3VRR` | `■■■ ■■VA3VRR` |
| **`013347` streamed** | **`■    ■VA3VRR`** | `■    ■■■■R` |
| `004507` settled | `NL DOT NET ■I ECH STAAION HAND■ AHIS MESAGE P` | unchanged |
| **`two-station` settled** | **`L DE W1XYZ K`** | `ATD■VTXYZ` |
| `ClearingTheTranscript…` | fails at `■ DE W1AW K` | **passes** |

Everything else in the corpus is unchanged, character for character. So the trade
is one synthetic looping training signal against the streaming pass losing the
callsign on the only real capture that carries one, and **a real capture outranks
a synthetic one** (HM-DEC-091). The work order said not to ship it if it still
costs a real capture. It does, so it is not shipped.

**And the ruling's premise has dissolved underneath it.** HM-DEC-116 says the
streaming pass "uses dit multiples only until those classes exist", which was true
when it was ruled and is not true now: the streaming estimator reads `CwGapFit`
like the settled pass does (HM-OPEN-032). Read literally against today's code —
adopt only where the estimator has no fit of its own — it was measured and **it is
a no-op on every recording here**, because wherever the settled pass has classes
the streaming pass already has its own. The full form overrides a working local fit
with a worse global one; the narrow form never fires. Whether that makes
HM-DEC-116 superseded rather than blocked is in `OUTPUT.md` section 4.

The path behind HM-DEC-121 is found, and it is not the dit hint.

**Traced 2026-08-18 on `cw-2026-08-17-013347`**, with the adoption applied and
then reverted:

| | without adoption | with adoption |
|---|---|---|
| tracker retunes | **1** | **3** |
| final tone | 610 Hz | 625 Hz |
| speed | 15 wpm | 14 wpm |
| settled text | `■■■ ■■VA3VRR` | `■■■ ■` |

The standing hypothesis was that the settled pass takes exactly one thing from
the estimator, the dit hint, and that the classes could be handed forward
without touching what the dit derives from. **That is wrong.** `Recompute`
computes the dit from the mark clusters and the shortest gap and reads none of
the gap cuts, so adoption cannot move it directly.

**The path runs through the tone tracker.** In `CwDecoder`:

```
_tracker.MidCharacter = _pattern.Length > 0 || _pending.Count > 0;
```

`MidCharacter` is the streaming pass's own segmentation state, and the tracker
uses it to decide when a held retune may be released:

```
if (!double.IsNaN(_heldSwitchHz) && !MidCharacter) { Switch(_heldSwitchHz); }
```

So: adopted gap classes change where the streaming pass divides characters,
which changes when `_pattern` and `_pending` are empty, which changes when the
tracker is allowed to retune. It retunes twice more and lands on a different
pitch.

**And every tracker switch calls `_settled.Reset()`**, which throws the settled
window away, because a switch means somebody else started transmitting
(HM-DEC-096 phase 3). Two extra resets on a thirty-second capture is enough to
lose the callsign.

The settled pass is also fed `reading.PowerDb` — the envelope measured at
whatever pitch the tracker is currently on — so a different retune history is
literally different audio arriving at the second pass.

**What this means for HM-DEC-116.** It cannot be made safe by keeping the
classes away from the dit, because the dit was never the coupling. Two
directions are open and both are Tim's:

- Stop the streaming segmentation steering the tracker. `MidCharacter` exists so
  a retune does not land in the middle of a character, which is a real
  protection; something that does not depend on the gap classification would
  have to replace it.
- Delay adoption until the tracker has settled, so the classes never move
  `MidCharacter` while a retune is pending. Cheaper, and it leaves the loop in
  place for whatever changes segmentation next.

Nothing was shipped. HM-DEC-121 keeps HM-DEC-116 blocked and this is the trace
it asked for.

---
id: HM-OPEN-028
status: answered
owner: tim
raised: 2026-08-18
severity: slows
blocks: ASignalAtTheWrongPitchIsStillFound at 400 Hz
refs: HM-OPEN-027, HM-DEC-096, HM-DEC-123, src/Hamlet.RadioEngine/Cw/CwToneTracker.cs
---

**RULED, AND THE WORK IS ITS OWN ORDER: HM-DEC-123**, the same ruling that
answers HM-OPEN-027. Recorded here so the next session reads it rather than
re-deriving it: the 400 Hz failure is not about 400 Hz, the cause is already in
this entry, and the fix belongs to that work order and to no other.

---

**BUILT 2026-08-18, AND THIS ENTRY'S OWN DIAGNOSIS WAS WRONG.** HM-DEC-123's
distinction is in the tree and it does not touch this failure, because **the
retunes never cost this case anything through the settled window at all.**
Measured by disabling every reset outright: the decode is unchanged, still
`■■ ■■■ ■ K DE W1AW K` with the `CQ` missing. This entry said "why the retunes
cost so much is HM-OPEN-027's finding"; they do not.

**What they cost is where the tracker went.** Traced with every survey verdict
printed, the three moves on a 400 hertz signal started from 600 are: from cold to
400, then **to 575, then back to 400**. The tracker spends about half a second
listening at 575 while the station sends `CQ`, and the characters are lost because
the filter was pointed away from the signal, not because anything was thrown away.

**And 575 hertz is the station's own image, thirty-five decibels down.** The
survey's verdicts either side of the move:

```
keyed 400  dit 77  dah 213  sep  8.7  lift 64.0  keyedDb -21.9
keyed 575  dit 83  dah 220  sep 30.8  lift 26.3  keyedDb -56.5
keyed 400  dit 80  dah 228  sep  5.6  lift 62.8  keyedDb -21.2
```

Same dit, same dah, same keying, thirty-five decibels quieter, and **clustering
three times more cleanly than the station itself** — separation 30.8 against 8.7.
On the reads where the 400 bin fails to score at all, that image is the only
candidate left and it wins twice running, which is all the confirmation rule asks
for.

**It is an artifact of a fixture with no band in it** (§12.5, HM-OPEN-018). The
same signal with noise in it:

| noise amplitude | moves | decode |
|---|---|---|
| 0 | 3 | `■■ ■■■ ■ K DE W1AW K` |
| 0.002 | 3 | `TT■ ■■■ ■Q DE W1AW K` |
| 0.01 | **1** | `T■ ■■■ ■Q DE W1AW K` |
| 0.03 | **1** | **`V VVV VVV CQ DE W1AW K`** |
| 0.06 | **1** | **`V VVV VVV CQ DE W1AW K`** |

`ASignalAtTheWrongPitchIsStillFound` generates its audio with no noise at all, so
between the elements there is digital silence and the sidelobe is a hard-limited
replica of the station with nothing to bury it. A receiver never hands that over,
which is the finding HM-OPEN-018 was opened for and the reason every fixture under
`tests/fixtures/cw/receiver` was rebuilt with a shaped band in it.

**Not fixed here.** Giving the test a band would turn it green, and changing a
fixture to turn a test green is the one move §12.5 exists to stop a session making
on its own authority. It is in `OUTPUT.md` section 4.

The 400 Hz pitch failure is not about 400 Hz, and it is the same root cause as
HM-OPEN-027.

**The tracker always finds the pitch.** Told to start at 600 and given a signal
at 400, it reports 400 at the end of every run. What differs is how many steps
it takes to get there, and each step costs the settled window.

Measured 2026-08-18, one signal at 400 Hz, varying only where the tracker was
told to start:

| told | retunes | decode |
|---|---|---|
| 300 Hz | 3 | broken |
| 350 Hz | **1** | good |
| 400 Hz | **1** | perfect |
| 500 Hz | 3 | broken |
| 550 Hz | 3 | broken |
| 600 Hz | 3 | broken |
| 700 Hz | **1** | good |
| 900 Hz | **1** | good |

**One retune decodes and three does not**, and it is not distance: starting 300
hertz above works while starting 100 above does not.

The shape suggests three regimes. Within a bin or two the tracker locks at once.
Far enough away that the starting filter sees nothing of the signal, the coarse
survey drives one decisive switch. **In between — roughly 100 to 200 hertz off —
enough of the signal leaks through the starting filter to look like evidence
where the tracker already is, and it converges in steps instead of in one jump.**

The cliff the original test found at 400 Hz is an artifact of its 600 Hz start,
not a property of 400 Hz. The same test at 425 passes because 425 is 175 hertz
from 600 rather than 200.

**Why the retunes cost so much is HM-OPEN-027's finding.** Every tracker switch
calls `_settled.Reset()`, because a switch means somebody else started
transmitting (HM-DEC-096 phase 3). On a signal that is one station throughout,
two extra switches throw the settled window away twice for nothing.

**Two investigations converged on one cause**, which is worth saying plainly:
phase 4 traced a decode failure to extra retunes, and phase 5 traced a different
decode failure to extra retunes. Whatever is done about one is likely to settle
the other.

Not fixed, because the fix is not unambiguous. Making the tracker converge in
one jump changes acquisition behaviour on real signals, and making a switch stop
resetting the settled window acts against HM-DEC-096 phase 3, which exists
because a switch usually does mean a different station. Both are Tim's.

---
id: HM-OPEN-029
status: closed
owner: claude
raised: 2026-08-18
closed: 2026-08-18
severity: slows
blocks: TheEasyTierIsReadWhole(prosigns-easy), TheEasyTierIsReadWhole(exchange-easy)
refs: CLAUDE.md §12.5; HM-DEC-101; HM-DEC-124; HM-OPEN-031; tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/CwFixtureGenerator.cs
---

**Closed 2026-08-18 by HM-DEC-124.** The caret's separate branch is gone: the
caret now changes one thing and nothing else, which gap separates the letters,
so there is no opening edge to break the parity. The three prosign fixtures were
regenerated and HM-DEC-101's gate re-run over the whole set — **`prosigns-easy`
goes from 75% to 100% and the reference now reads `<BT> N0CALL <AR> <SK>` where
it read `EV N0CALL IR <SK>`**, which is the confirmation rather than a
coincidence. `prosigns-working` goes from 75% to 83%. Nothing else moved and no
fixture is held out.

**`exchange-easy` was re-checked after the fix and it is not the same defect.**
That fixture contains no caret, the generator was measured to render it exactly,
and the reference reads it at 100%. Hamlet reads `DE` as `B`, which is a
character gap being read as an element gap: HM-OPEN-031.


**Hamlet reads `IR` where `AR` was sent because the fixture generator sent `IR`.**
The caret that runs two letters together emits an unpaired key edge when the
joined pair begins a word, which swaps every mark and gap after it.

Chased on the instruction to chase it, and it is §12.5's exact pattern: the
fixture was built from a misunderstanding and the decoder was being blamed.

**The measurement.** Mark and gap lengths taken off `prosigns-easy.wav` itself by
complex demodulation at its stated tone, against the intended `^BT N0CALL ^AR ^SK`:

| word | intended | rendered as | reads |
|---|---|---|---|
| `^BT` | `-...-` | dit, **character gap**, dit dit dit dah | `EV` |
| `^AR` | `.-.-.` | dit dit, **character gap**, dit dah dit | `IR` |
| `^SK` | `...-.-` | correct | `SK` |

`EV` and `IR` are exactly what the reference implementation reads, which is the
confirmation: **two independent decoders agree, and they are both right.** The
audio genuinely says `EV` and `IR`. Nothing was wrong with either decoder.

**The path, and it is arithmetic rather than a hypothesis.** `KeyEdges` starts
with `edges = { messageStart }`, an opening edge with no closing partner yet. The
join branch's first act is `at += ElementGap; edges.Add(at);`, which assumes there
is a mark in progress to separate from. At the head of a word there is not, so
that edge closes a mark that never opened: **a phantom hundred-millisecond dit**,
and every edge after it lands on the opposite parity, so the dah that should have
opened `BT` becomes a three-hundred-millisecond gap and the element gaps become
marks. Predicted against the measurement for `^BT`, the model is exact at every
one of the nine edges.

`^SK` survives because its two letters carry six elements between them rather than
five, and the trailing-gap removal at the end of the branch restores the parity
that the opening edge broke. **An even-length prosign renders correctly and an
odd-length one does not**, which is why this has looked like a decoder fault on
some prosigns and not others.

**Not fixed, on the instruction to report it.** The fix is small, but it changes
what three prosign fixtures assert, so it needs HM-DEC-101's gate re-run and each
affected hold-out adjudicated individually with its reason recorded (§12.5), which
is the same discipline phase 3 was held to and is not a tail on another work unit.

**`exchange-easy` is likely the same defect**, since its text ends in a caret-joined
prosign, and it should be re-checked once this is fixed rather than investigated
separately.

---
id: HM-OPEN-030
status: closed
owner: tim
raised: 2026-08-18
closed: 2026-08-18
severity: slows
blocks: HM-DEC-122, which is built and measured and not live
refs: HM-DEC-122, HM-DEC-091, HM-DEC-095, HM-DEC-097, HM-DEC-120, src/Hamlet.RadioEngine/Cw/CwToneTracker.cs, tests/Hamlet.RadioEngine.Tests/Cw/CwAcquisitionWindowTests.cs
---

HM-DEC-122 was built exactly as ruled and does not survive its own measurement.
It is not live.

**What was built.** The tracker runs two extra coarse surveys during acquisition,
one over a twenty millisecond window and one over fifty, fed from the same ring
buffer on the same ten millisecond survey grid. Each is asked the question the
ruling names — two mark clusters inside the 2.5 to 3.8 ratio band, separated well
enough to be two lengths rather than a smear, which is `CwToneSurvey.Analyze`
unchanged. The shorter is preferred where both answer. The window it chooses
becomes the reading window, and an unproved speed estimate is no longer allowed to
lengthen the window, which is the death spiral the ruling names: runs merge, the
merged runs read as a slow fist, the slow fist asks for the long window, and the
long window merges the runs.

**The first finding is that the candidates name flukes for several seconds.**
Measured on a clean signal at 640 Hz, eighteen decibels over the noise, the short
candidate's answer across the first ten survey reads runs 325, 325, 325, 550, 550,
725, 725, 725, 650, 650 Hz. This is the fault the tracker's own two-agreeing-surveys
rule exists to prevent (HM-DEC-095), and the candidates were not subject to it.

**Settling on the first answer meets the ruling's acceptance and breaks §0.0.**
Tuned onto mid-transmission with no run-up, the fast end goes from about two thirds
of the message to about nine tenths:

| wpm, bare, 18 dB | before | after |
|---|---|---|
| 25 | 0.67 | 0.79 |
| 28 | 0.63 | 0.79 |
| 30 | 0.70 | 0.95 |
| 35 | 0.63 | 0.89 |

And `NothingIsEmittedAnywhereBelowTheFloor` fails: 2.8% of what comes back below
the refusal floor was never sent, where HM-DEC-120 measured zero at every level.
`ASignalAtTheWrongPitchIsStillFound(875)` also fails outright. **So a short window
taken early does not only cost sensitivity**, which is the premise the ruling's
tie-break rests on. It costs correctness, which §0.0 does not trade.

**Requiring the clock to belong to the confirmed station fixes both regressions
and leaves nothing behind.** Gating the settle on the tracker having confirmed
where the keying is — `_lastKeyedHz` known, and the candidate's answer within one
coarse bin of it — restores the refusal floor and the 875 Hz signal exactly. It
also arrives too late: on synthetic audio the confirmation takes about five and a
half seconds, by which time the message is nearly over, and **every cell of the
measurement matrix is then identical to the unmodified decoder**, at all nine
speeds and all five ratios, with and without a run-up. The window cap on its own
is likewise a no-op: it never binds.

**And where the gate does fire in time, it costs the only real recording seven
characters.** On `cw-2026-08-18-004507`, the ARRL bulletin at S4:

| | window | settled text | correct |
|---|---|---|---|
| unmodified | 40 ms | `JJ AOT NET ■I ECH STAAION HAND■ AHIS MESAGE P` | 36 of 47 |
| HM-DEC-122 | 20 ms | `T■E ECH STAAION HAND■ AHIS MESAGE P` | 29 of 47 |

Isolated by disabling the settle alone, which returns the bulletin to 36 character
for character. **Only the short candidate yields a clock there**, so the tie-break
is not even in play: the fifty millisecond window smears a 57 ms dit badly enough
to fail the cluster test while being the better window to read through, and the
forty millisecond window the ruling removes from consideration is better than
either. That is HM-DEC-095's measured table restated — 20 ms loses half a callsign
the same recording gives up whole at 40.

**What that leaves.** The ruling's diagnosis is sound and its remedy does not
follow from it. The window that yields the cleanest clock is not the window that
reads the signal best, and on the one real capture this repository holds those two
are different windows. Three directions, all Tim's:

- Keep the parallel run and change the selection to something that measures
  reading rather than clustering — the two candidates' own speed estimators at the
  tracked pitch, rather than the survey's per-bin scan.
- Keep the forty millisecond window as a third candidate, so the choice includes
  the one the evidence prefers.
- Take the acceptance figures above as the target and leave the mechanism alone.

Nothing was shipped. `CwAcquisitionWindowTests` pins the four fast-end figures and
the slow end so the next attempt is judged against a number.

**CLOSED 2026-08-18 BY HM-DEC-125**, which supersedes HM-DEC-122 and leaves it
unbuilt. Confirmed by sweep the same day: no candidate survey, no candidate window
constant, no clock-proved flag and no window-change counter remains anywhere in
`src`. `CwAcquisitionWindowTests` survives, because it is measurement rather than
mechanism, and still pins all three figures — the bare fast end, the same fist
with a run-up, and the slow end.

---
id: HM-OPEN-031
status: answered
owner: claude
raised: 2026-08-18
severity: slows
blocks: TheEasyTierIsReadWhole(prosigns-easy)
refs: HM-DEC-114, HM-DEC-115, HM-DEC-124, HM-OPEN-029, HM-OPEN-032, tests/fixtures/cw/receiver
---

**The `DE` half is fixed, 2026-08-18** (HM-OPEN-032). The streaming estimator was
carrying a second gap classifier and it was the one getting three heaps wrong.
`exchange-easy` and `coverage-easy` now read whole. The `prosigns-easy` half is
untouched and is still the four opening characters.

Two easy-tier fixtures the reference reads whole, and Hamlet does not. Both are
decoder faults with the fixture proved sound, which is what HM-DEC-114 exists to
surface.

**`exchange-easy`: Hamlet reads `DE` as `B`.** `D` is `-..` and `E` is `.`, so
`-...` is the two of them with the character gap between them read as an element
gap. The fixture is textbook spacing at twelve words a minute — element gap 100
ms, character gap 300 — and the reference reads the whole message. Measured
across a sweep of speeds and ratios it is not an edge case: **the same
substitution appears at 10, 12, 15, 18, 20, 22, 25 and 30 words a minute and from
eighteen decibels down to three**, which is the whole readable range. It is the
one error standing between the easy tier and HM-DEC-114's bar.

Two candidates, neither confirmed. `E` is a single dit, so `DE` is the shortest
character in the alphabet arriving immediately after a four-element one, and a
gap classifier fitted over a rolling window has very few character gaps to learn
from that early in a message. Or the boundary itself is misplaced: HM-DEC-115
fits the cuts by clustering the gaps, and where a message's gaps really are two
clean heaps at 100 and 300 the cut should be trivial, so a cut that still lands
above 300 says the fit is being pulled by something else.

**`prosigns-easy`: the opening `BT N0` is lost to acquisition.** With the caret
fixed the prosigns themselves read correctly — `CALLARSK` against `BTN0CALLARSK`
— so what is left is the four characters before the detector has found the
signal. Every other easy-tier fixture carries the ruled run-up for exactly this
(HM-DEC-103) and this one may not.

**And the run-up was re-tested after the caret fix rather than taken on trust.**
The recorded reason for excluding it was that `VVV` in front of a prosign gives
the mark-length clustering one smear rather than two groups, and that reason no
longer holds: a correctly rendered `^BT` is `-...-`, whose marks are the same two
lengths as `VVV`. Measured anyway, with the run-up in place: **the reference
reads the fixture at 100% and Hamlet emits a single placeholder.** So the
exclusion stands, and what stands behind it has changed from a fixture property
to a decoder one — a loud clean signal the reference reads whole and Hamlet
collapses on entirely.

Not chased further, per the work order. Both are named here rather than repaired
on the way past (§12.6).

---
id: HM-OPEN-032
status: closed
owner: tim
raised: 2026-08-18
closed: 2026-08-18
severity: slows
blocks: TheSettledPassNoLongerStopsShortOfTheCallsign, TheEasyTierIsReadWhole(tightfist-easy)
refs: HM-DEC-115, HM-DEC-123, HM-DEC-128, HM-OPEN-027, HM-OPEN-028, HM-OPEN-031, HM-OPEN-033, src/Hamlet.RadioEngine/Cw/CwTiming.cs, src/Hamlet.RadioEngine/Cw/CwGapClasses.cs
---

**CLOSED 2026-08-18 BY HM-DEC-128.** Both tests this blocked are settled.
`TheSettledPassNoLongerStopsShortOfTheCallsign` went green under HM-DEC-123 and
has stayed green. `tightfist-easy`'s placeholder is not this entry's to answer: it
was traced to one timing measurement inside one character and belongs to
HM-OPEN-033.

**And this entry's own finding is what superseded HM-DEC-116.** Handing the
streaming estimator the shared fitter is what removed the choice that ruling was
making — fitted classes against dit multiples — and left the live question as the
settled pass's global fit against the streaming pass's local one. On today's
evidence the local one wins, and if the two ever diverge that is the question to
ask rather than this one.

The streaming estimator now reads the one gap classifier, and two tests went red
in exchange for a substitution that was on every CQ call on the band.

**What was wrong.** `CwGapFit` carries the note "one implementation, read by both
passes, because two copies of a classifier is two classifiers", and there were
two: the settled pass used it and the streaming estimator had its own, which
split the gaps in two and then split the long half again. **A two-way split of
three heaps lands wherever the window's mixture puts it.** Traced on
`exchange-easy`, which is textbook spacing at twelve words a minute and about as
easy as this gets — element gaps 100 ms, character 295, word 695 — the first cut
wandered from 189 to 414 across one message, and wherever a couple of word gaps
crowded into the twenty-gap window it converged on the split between *character
and word* rather than between *element and character*. Every character gap then
read as an element gap and `DE` came back as `B`: one letter made out of two, at
every speed from ten to thirty words a minute and at every ratio from eighteen
decibels down to three.

**Two guards were needed and both were measured rather than reasoned.**

- **The element class has to be the crowded one**, tested at the streaming call
  site and not inside the fit. The gate flaps at the onset of the very first mark
  and leaves gaps of 25, 35 and 65 milliseconds behind; while those sit in the
  window the fit gives them a class of their own, puts the element boundary at 53
  milliseconds, and reads every real 100 millisecond element gap as a character
  gap — twelve elements came back as twelve letters. Applying the same test
  inside `CwGapFit`, where the settled pass would see it, costs the callsign on
  `cw-2026-08-17-013347`.
- **A lone gap far above everything else is a pause and not a class**, tested
  inside the fit because it is about the data. A looping training signal pauses
  two seconds between repeats, and that one silence took the whole top class:
  word gaps of 680 then had to share a class with character gaps of 290, the
  boundary went from 444 to 903 milliseconds, and every space between words
  disappeared.

**What it bought, measured:**

| | before | after |
|---|---|---|
| `exchange-easy` | `VVCQCQBN0CALL…` | reads whole |
| `coverage-easy` | reads whole | reads whole |
| bare fist at 25, 28, 30, 35 wpm | 0.67, 0.63, 0.70, 0.63 | 0.89, 0.89, 0.89, 0.88 |
| slow end with a run-up | 0.89 | 0.95 to 1.00 |
| `ClearingTheTranscript…` | `■ B■AW K` | `■ DE W1AW K` |
| bulletin, settled | `JJ AOT NET…` | `OT NET…` — same correct count, three fewer invented |
| `013347`, streaming | `■   ■<SK>3VRR` | `■    ■VA3VRR` |
| `013347`, settled | `■■■ ■■VA3VRR` | `■■■ ■` |

**And what it cost.** Two tests are red that were not:

- **`TheSettledPassNoLongerStopsShortOfTheCallsign`.** Changing where the
  streaming pass divides characters changes `MidCharacter`, which changes when
  the tracker may release a held retune: one retune becomes three,
  `_settled.Reset()` runs twice more, and the settled window is thrown away
  before it reaches the callsign. **This is HM-OPEN-027's coupling exactly, and
  HM-DEC-123 is the ratified fix for it**, in its own work order. The callsign
  did not disappear from the screen; it moved to the other pass, and the reading
  it moved from was `<SK>3VRR`, which is a confidently wrong prosign where `VA`
  was sent. §0.0 is better served on that recording than it was.
- **`tightfist-easy`** gains one placeholder, `TE■TDETESTK` against
  `TESTDETESTK`. Not the outlier trim, which was tested with the trim disabled
  and is unchanged; it is the fit itself on a fist whose element gaps are shorter
  than its dits. A placeholder is honest and this is the tier HM-DEC-114 says
  must be read whole, so it is a defect rather than a degradation.

**Why it shipped with those two red.** `DE` read as `B` is a wrong character
presented at full confidence, in the two commonest letters on the band, at every
speed a beginner will meet. What replaces it is a placeholder in one pass on one
recording, and a placeholder asserts nothing. §0.0 decides that one way.


---
id: HM-OPEN-033
status: open
owner: tim
raised: 2026-08-18
severity: slows
blocks: nothing red any more, and it is still the largest thing left
refs: HM-DEC-114, HM-DEC-095, HM-OPEN-028, HM-OPEN-031, src/Hamlet.RadioEngine/Cw/CwToneSurvey.cs
---

The last two bar failures are two different faults, and one of them is the same
fault as the 400 hertz test. Checked before treating them separately, as the work
order asked.

**`tightfist-easy` is a timing veto on one gap and nothing else.** The character
is the first `S` of the first `TEST`, its pattern is `...` — which is correct, and
the same pattern reads as `S` four seconds later — and it is suppressed because
its confidence comes back Unreadable:

```
'■' Unreadable score 0.11 snr 28.6 dB pat '...' at 5.31s
clarities [0.97, 0.11, 0.92, 0.49, 0.89, 0.97]
dit 93 ms, mark boundary 137 ms, element gap boundary 96 ms, 20 marks
```

**Twenty-eight decibels over the noise, so the signal is not the question**: one
timing measurement inside the character scores 0.11 against an element-gap
boundary of 96 milliseconds, on a fist whose element gaps are 80. Every other
measurement in the same character is between 0.49 and 0.97. This is the tight
fist's own shape — gaps shorter than its dits (HM-DEC-095) — meeting a boundary
fitted while the estimator is still filling its window. **A placeholder here is
honest and the character is genuinely marginal by the measurement**, which is why
this is a question about the boundary rather than about the veto.

**`prosigns-easy` loses its opening to acquisition, and the ruled remedy sends the
survey to the wrong bin.** The first character it emits is at 7.44 seconds on a
fixture whose message runs about four and a half, so `<BT> N0` is gone before the
detector has found the signal. Every other easy-tier fixture is given a `VVV`
run-up for exactly this (HM-DEC-103) and this one cannot take it: measured again
after the caret fix and after HM-DEC-123, with the run-up in front the tracker
makes two moves, **settles at 675 hertz on a fixture sitting at 615**, and emits
nothing at all.

**AND THAT IS THE THIRD SIGHTING OF ONE MECHANISM.** The coarse survey choosing a
bin that holds no station is now behind three separate failures:

| where | signal | survey chose | result |
|---|---|---|---|
| `ASignalAtTheWrongPitchIsStillFound(400)` | 400 Hz | 575 Hz, 35 dB down | `CQ` lost (HM-OPEN-028) |
| `prosigns-easy` with a run-up | 615 Hz | 675 Hz | nothing decoded |
| `two-station`, from cold | 615 Hz | 625, 600, 625 | three moves before it settles |

In the first two the chosen bin carries the same keying as the station, far
weaker, and clusters more cleanly than the station itself. HM-DEC-095 settled that
a note is chosen by how it is keyed and never by how loud it is, and that ruling
was about which of several signals to read on an empty-handed survey. **What is
not settled is whether a candidate may displace a station already being read when
it is thirty-five decibels quieter**, and that is the question all three of these
share. It is in `OUTPUT.md` section 4.

Neither bar failure was fixed here. Both are attributed, which is what the work
order asked for where a fix is not clear, and the shared mechanism is named rather
than repaired on the way past (§12.6).

---

**SCHEDULED AS ITS OWN WORK ORDER, 2026-08-18, AND NOTHING IS RED FOR IT ANY
MORE** (HM-DEC-129). `tightfist-easy` was fixed by HM-OPEN-035 — the fault there
was the confidence scale rather than the survey — and `prosigns-easy` no longer
asserts HM-DEC-114's bar, because a message whose opening is gone before the
detector has found it is a different claim from a loud clean signal read wrongly,
and **no real station produces it**, because a CQ repeats. The 400 hertz case was
closed by HM-DEC-127's floor.

**That leaves this entry with no red test behind it and it is still the largest
single defect in the decoder.** Three sightings stand: the 400 hertz image, the
prosigns fixture settling at 675 on a 615 signal, and the two-station recording
taking three moves from cold. HM-DEC-127's floor protects a station already
confirmed; **nothing protects the first choice.** A defect with nothing red
pointing at it is the kind that stays unfixed for a year, which is the reason this
paragraph exists.


---
id: HM-OPEN-034
status: open
owner: claude
raised: 2026-08-18
severity: none
refs: HM-DEC-127, src/Hamlet.RadioEngine/Cw/CwToneSurvey.cs, tests/Hamlet.RadioEngine.Tests/Cw/CwDisplacementFloorTests.cs
---

A station at 350 hertz is not read, and was not read before HM-DEC-127 either.

Found while writing the control for that ruling — the test that proves the new
floor has not made the tracker deaf to a real move. Every pitch
`ASignalAtTheWrongPitchIsStillFound` covers is found and read; 350 comes back as
`■■ ■■■■ ■` after two moves. **Measured either side of the change and identical
character for character**, so it is a pre-existing hole rather than one this
ruling made, and it is recorded here rather than asserted in a test that would
then be red for something nobody has decided to fix.

Fifty hertz above `MinimumToneHz`, so the likeliest cause is the bank's own edge:
the survey needs bins far enough from a candidate to sample the band beside it,
and near the bottom of the range there are fewer of them on one side. That is a
hypothesis and not a measurement.

**Severity none.** The IC-7300's CW pitch range is 300 to 900 hertz (Full Manual
p. 4-14) and the decoder's own default is 600, so a station at 350 is somebody
who has tuned a long way from where anybody normally sits. Named and left
(§12.6).

---
id: HM-OPEN-035
status: closed
owner: claude
raised: 2026-08-18
closed: 2026-08-18
severity: slows
blocks: TheEasyTierIsReadWhole(tightfist-easy)
refs: HM-DEC-114, HM-DEC-115, HM-OPEN-033, src/Hamlet.RadioEngine/Cw/CwGapClasses.cs
---

`tightfist-easy`'s placeholder was the confidence scale rather than the boundary,
and it is fixed.

**The boundary was right and the centre was not.** Traced with the fit's own
numbers printed at the moment the character was judged. The fixture's element gaps
are 80 milliseconds and its character gaps 162; the gate measured the two gaps
inside the first `S` at 85 and 75, which is right; the fit's boundary was 89,
which classifies all of them correctly, and its element class **centre was 49**.
Confidence is measured from the boundary toward the centre, so:

```
Toward(85, cut 89, centre 49) = 4 / 40  = 0.10
Toward(75, cut 94, centre 55) = 19 / 39 = 0.49
```

A character whose pattern was `...` and whose elements were clean came back as a
placeholder at 28.6 decibels over the noise. Four seconds later, with the window
full of this fist's own gaps, the same pattern read as `S` at 0.98.

**What dragged the centre down was the detector, not the sender.** The rolling
window still held gaps of 15, 20, 30 and 35 milliseconds from before the signal
was acquired. **Twenty-five is the shortest dit this radio can send** —
`CwToneSurvey.ShortestDitMs`, forty-eight words a minute, the fastest its own
keyer goes — so nothing below that is a silence anybody left. They are dropped
before the fit rather than trimmed after it, because they spoil the class centres
and not merely their edges.

**The whole corpus was re-decoded either side of it.** Twenty-four recordings,
six of which move:

| recording | before | after |
|---|---|---|
| `tightfist-easy` | `TE■T DE TEST K` | **`TEST DE TEST K`** |
| `fast-working` | `■ D ■E ■AEL T I■ ALEK` | `V D ■E ■AEL T I■ ALEK` |
| `coverage-edge` | 7 moves | 5 moves, nothing decoded either way |
| `fast-edge` | 1 move | 3 moves, nothing decoded either way |
| `exchange-working` | `SE<AS> F EA R` | `ESE<AS> FEEA R` |
| `two-station-second` | `LL DE W1XYZ K` | `D DE W1XYZ K` |

One easy-tier bar goes green, one opening character comes back, two edge tiers
move their move counts around while decoding nothing, and two working-tier
transcripts shuffle within text that is unreadable either way. HM-DEC-114 makes
the easy tier pass-or-fail and says the working tiers assert how the decoder
degrades rather than that it reads everything, so nothing any test asserts moved
except the one that was meant to.

**Nothing here raises a confidence by adding a term to it** (HM-DEC-048). The
worst measurement still wins and no term may lift a score; what changed is the
population the scale is fitted to, and a scale fitted partly to the detector's own
flapping describes the detector rather than the sender.

---
id: HM-OPEN-036
status: closed
owner: tim
raised: 2026-08-18
closed: 2026-08-19
severity: none
refs: CLAUDE.md §1, HM-DEC-131
---

`CLAUDE.md` §1 is not strictly newest-first at its head.

Read out in file order, the first five rows are HM-DEC-128, **HM-DEC-130**,
HM-DEC-129, HM-DEC-127, HM-DEC-126. The table's own instruction is "every ruling,
most recent first", and 130 sitting second means the newest ruling is not the one
a reader's eye lands on.

**The cause is known and is Tim's own**: insertions went in at a fixed anchor
rather than at the true head, so each new row landed under whatever was already
there rather than above it.

**Reported and deliberately not corrected.** A ruling is never edited (§1), and
while re-ordering rows is not editing their content, doing it in passing during a
session that came to write two status files is exactly the kind of unrelated
repair §12.6 exists to stop. It is one move, it should be one deliberate move, and
whoever makes it should decide at the same time whether the anchor that caused it
gets fixed or whether the table simply gets read from the top each time.

Severity **none**: nothing is blocked and no ruling is lost. What it costs is that
`RULES_AT` cannot be taken from row one, and this session took it from the highest
id present instead.

**CLOSED 2026-08-19. The table is newest-first, the cause is named, and a test
holds it.**

Six rows moved and none was edited: HM-DEC-135 down to its own day, and the
same-date runs 130/129/128, 104/103/102/101 and 83/82 put in descending order.
**The cause was two habits rather than one**, both visible in the history and
neither a script in this repository: `5d00bd4` and `303c4f4` inserted a row
immediately below the top row rather than above it, and `d263f95` pasted a block
of four rulings in the order they were ruled, which is oldest-first inside a
newest-first table. §1 now states the insertion point and the batch order in its
own header, where the next delivery will be composed, and
`DecisionLogOrderTests` fails on any inversion.

**Two things were found doing it and neither is corrected here.** HM-DEC-051's row
is dated 2026-08-14 and HM-DEC-050's is dated 2026-08-15, so their dates and their
ids disagree about which is newer; a row's date may be the day the ruling was made
rather than the day it was written down, and guessing which of the two is wrong
would falsify the record. And **two different rulings carry the id HM-DEC-088**,
which is HM-OPEN-046. Both are named in the test as known exceptions rather than
sorted away.

---
id: HM-OPEN-037
status: open
owner: tim
raised: 2026-08-18
severity: none
refs: CLAUDE.md §3, HM-OPEN-016, HM-OPEN-017
---

The two items marked `severity: hard` block work that no longer exists.

`HM-OPEN-016` blocks "merging `feature/honest-cw-detection`", a branch HM-DEC-113
brought back to `main` and whose whole practice that ruling ended.
`HM-OPEN-017` blocks "finishing session 1 of the batch brief", which finished
several work orders ago.

So `TOP_SEVERITY` reads **hard** out of the record while nothing is actually
stopped, and any status file counting severities out of `OPEN_ISSUES.md` will keep
saying so.

**Not softened, and that is the point.** §3 is explicit that a severity is never
lowered to make the picture look better, and a session deciding on its own that
somebody else's `hard` has gone stale is doing exactly that with extra steps. What
is wrong here is not the severity but the `blocks:` line underneath it, which
names work that has been done — and whether either item still has a live blocker
under a different name is a reading of two long entries that deserves its own
sitting rather than a paragraph at the end of an unrelated session.

---
id: HM-OPEN-038
status: open
owner: tim
raised: 2026-08-18
severity: none
refs: CLAUDE.md §4, §13.4, §12.4; HM-DEC-074; HM-DEC-082; HM-DEC-133
---

The transmitter's power output is not stated anywhere in `CLAUDE.md`, and §13.4's
hazard line does not supply it.

Found while writing that line. §4's header calls the radio an "HF/50 MHz
transceiver" and names no wattage; no row in its table cites a page for one; and
`100 W` appears nowhere in the file. **The figure is genuinely absent rather than
merely uncited.**

**There is a good reason it never arrived.** HM-DEC-074 and HM-DEC-082 both rule
that Hamlet reports power as a percentage of the radio's own range and **never as
a wattage** — because it cannot know what a percentage means in watts at this
frequency into this load. So no part of the application has ever needed the
number, and §4 records what the application needs.

**Why it is worth having now, and only now.** §0.2 is about what this software can
do to the physical world, and "keys a transmitter" and "keys a hundred-watt
transmitter" are different warnings to somebody who has just arrived. The hazard
line currently says the first.

What would close it: the page in Full Manual `A7292-4EX-6` that states the RF
output, added to §4 as a cited row, after which §13.4 can name it. **Not filled in
from general knowledge** — §4's whole discipline is that a figure comes with the
page it was read from, and a hazard line is the worst place in this file to break
that (§12.4).

Severity **none**: nothing is blocked, no ruling depends on it, and the hazard is
correctly stated without it.

---
id: HM-OPEN-040
status: closed
owner: claude
raised: 2026-08-18
closed: 2026-08-18
severity: slows
blocks: the operator finding the calling cycle at all
refs: HM-DEC-098, HM-DEC-063, HM-DEC-072, HM-DEC-086, HM-DEC-113
---

"Repeat send every 30 seconds" cannot be done because **no control is called
that**, the widget that does it is on no preset, and the version number cannot
tell the operator whether his build has it.

**It is not HM-DEC-113 and that was checked first.** The cycle is on `main`:
`5d00bd4` built the engine at 13:34 and `d8f6ce9` the face at 13:44 on
2026-08-18, with `29df4b2` after them. No branch is involved.

Three separate faults, each on its own enough to produce the report:

**The name.** Nothing in the application contains the string "repeat". The
widget is `Call CQ on a cycle`, and its interval is a field inside it rather
than part of its name. An operator hunting for the words he was told are the
feature finds nothing.

**No preset carries it.** `Widgets.AutoCall` was registered in the tray and
placed on none of the three layouts — not "Getting started", not "Listening
around", and not "Making contacts", which is the one with the terminal, the send
controls and "did anybody hear me" on it. So it was reachable only by somebody
who already knew to go to the tray and look, **which is HM-DEC-072's own shape:
ruled, built, and never invoked.** Fixed by appending it to "Making contacts",
appended rather than fitted in so nothing already there moves.

**The version could not have told him.** `Directory.Build.props` last changed on
2026-08-17 and still read `1.9.0` after a whole feature landed on the 18th, so
today's telemetry reads `appVersion 1.9.0` on builds that both do and do not
contain the cycle. HM-DEC-063 rules that a minor version "adds a capability the
operator can see and use", and three landed since 1.9.0: the calling cycle, scan
results that tune, and favourite notes. **The session that shipped them did not
bump it, and that session was mine.** Now 1.10.0.

Closed by the three fixes above. What it cost is recorded rather than tidied
away: an operator was told a feature existed, could not find it, and nothing on
his screen or in his telemetry could have told him whether he had it.
