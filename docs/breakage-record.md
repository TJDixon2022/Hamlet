# The breakage record — what has actually broken in this project

**Written 2026-09-05 by work instruction 250, task 1, from the record only. No
build and no test was run to produce it.**

This file is the evidence for `docs/gate-set.md`. **Nothing goes in the gate set
without an entry here**, and every gate-set entry cites one of these by number.

Each entry says **what broke**, **which unit**, **how it was found**, and
**whether a test would have caught it** — because the ones a test would *not*
have caught are the more valuable half of this list and they are marked as such.

Sources: `PHASE_OUTCOME.md`, `docs/phase-sensitivity-run/PHASE_OUTCOME.md`,
`docs/phase-ft8/PHASE_OUTCOME.md`, `RUN_LEDGER.md`, `DECISIONS.md`,
`docs/test-baseline.md` and the commit messages of the fixes themselves, which
are the only place several of these are written down.

---

## A. Breakages a test caught, or would have

### B1 — Hamlet was wired to the overload that hands Deep an empty span

**Unit 249.** `Ft8Reader` called `Decode(Ft8Waterfall)`. That overload gives the
sibling's per-candidate loop **no samples**, so fine sync refused **42 of 42
candidates for want of samples**: a Hamlet wired the obvious way would have paid
**2.1 times the port's cost for none of the off-grid gain**, and every count on
the sheet would have looked normal while it happened. Through the samples entry
point fine sync re-synced 42 and accepted 14.

**Found by:** task 1 of unit 249, by measurement, before shipping.
**A test would have caught it.** Not a unit test on the sibling — the sibling was
correct — but a test *at Hamlet's level* asserting the reader returns at least
what the port returned. That test now exists and is gate entry 2.

### B2 — identity between the two decoders was trivially true, and then it was not

**Units 245 → 246.** At unit 245 `Ft8Sharp.Deep` **held an `Ft8SlotDecoder` and
delegated to it**, so whole-result identity was one decoder called twice. Unit
246 replaced that with the sibling running the port's per-candidate loop itself
through the port's public members, because ordered statistics had nowhere else to
sit. **From that commit the two scoreboard columns were two pieces of code**, and
any divergence in the reproduction would have made the OSD-off column something
other than the port — silently invalidating every decibel units 246 to 249
attributed to one named change.

**Found by:** unit 246, which foresaw it and made the identity test mandatory
(its ruling 4).
**A test caught it, and still does.** Gate entry 1.

### B3 — an OSD codeword cannot be turned into a result except by handing it back

**Unit 246.** `Ft8CodewordResult` **cannot be constructed outside `Ft8Sharp`**, so
a codeword recovered by ordered statistics has to be handed *back* to
`Ft8CodewordDecoder` as normalised ratios. That route works, and **a later
refactor could trivially shortcut it** — at which point a message would reach the
operator without the port's parity check or its CRC-14.

**Found by:** unit 245's census of the seam, recorded and not opened as an issue
because a public route existed.
**A test would have caught the shortcut**, and does: gate entry 2's seam tests
are watched refusing a wrong codeword.

### B4 — the licensing reference could have gone on the wrong project

**Unit 245.** A brand-new GPL-3.0 sibling had to be reached from a tree where the
MIT port already existed, which meant adding a `ProjectReference`. The one added
went on `tests/Ft8Sharp.Tests`, and the arbiter ruled that direction safe **on
the grounds that the mechanical guard already catches the breaching one**. The
natural mistake — putting it on `src/Ft8Sharp.csproj` — **would have made an MIT
library depend on a GPL-3.0 one, and it compiles silently.**

**Found by:** nothing had to find it; the guard made the ruling cheap.
**A test catches it.** Gate entry 3, and it is the one property in this phase
that cannot be fixed after a release.

### B5 — unbounded pairing would have put 366 messages nobody sent on the ladder

**Unit 247.** The combiner's pairing budget was written down *before* the code and
then **counted rather than estimated**: 516 combinations submitted across the
whole jittered -21 dB walk, naive false-accept expectation **0.031**, zero
returned. **Unbounded, the same arithmetic gives 366 across a 306-trial rung**
against 0.24 for the bounded rule that shipped.

**Found by:** unit 247, in advance.
**A test would have caught a relaxed budget**, and does: gate entry 4.

### B6 — the new extractor is worse than the port at the position the port is good at

**Unit 248.** The baseband fine-sync extractor is a rectangular one-symbol matched
filter; the port's is a tapered two-symbol frame. At the **same coarse position**
the new one is measurably worse — **median hard-decision distance 56 against
48 at -21 dB**. Its whole value is at the *wrong* place. **Wiring it in front of
the port instead of behind the port's refusals would have cost decodes on every
on-grid signal while appearing to help off-grid**, and the ladder's default
placement is the one place that damage would not have shown.

**Found by:** unit 248, by measuring the extractor against the port before
believing the column.
**A test would have caught it:** every message the ordinary path returned is
still there. Gate entry 5.

### B7 — three surfaces described the decode and none could say the audio was starved

**2026-09-03, recorded in `AudioArrival`'s own remarks and as HM-DEC-093.** The
tap filled at **13 per cent of real time for an entire evening** and **not one of
the three surfaces said so**: telemetry, sidecar and census line all described
the decode, so **a starved sound card and an empty band wrote identical output.**

**Found by:** eventually, by hand, after an evening was lost.
**A test would have caught it** only in the weak sense that the census must reach
all three surfaces and a slot with nothing in it must still be counted rather
than omitted — which is gate entry 6, and is why an empty slot writing its census
is a gate at all.

### B8 — the sheet printed two answers to the same question

**Unit 238.** On capture `ft8-2026-09-03-210644` the sidecar wrote
`wholeSlots 1 ... whole transmission inside the audio` and **the line directly
under it** read `refusal no whole slot fits inside the recording`. The sheet
measured the 12.64 s a transmission occupies; the cutter required a full 15 s
slot. **Both were defensible and they cannot both be printed.**
`Ft8Slots.TransmissionFits` is now the only answer and both call it.

**Found by:** reading a capture sidecar.
**A test would have caught it** — and the fix carries one. Not in the gate set:
it guards the cutter, which this phase does not touch.

### B9 — the operator's own settings folder was being written by the test suite

**Unit 235.** Nine tests of one `Hamlet.App.Tests` class, run alone with a SHA-256
snapshot either side, **changed two files in `%AppData%\Hamlet`** — the
`MainWindowViewModel` constructor opens his real spots database, saves his
settings and runs a live callsign lookup whose answer it saves. After that unit's
seam the same nine tests change nothing.

**Found by:** unit 235, by hashing the folder before and after rather than by
reasoning about it.
**A test would have caught it**, and unit 235 shipped five gates that do. Not in
the gate set: it guards the app's construction seam, not a phase property.

### B10 — a report shipped a placeholder where a measured number belonged

**Unit 248.** Its `NUMBER:` line shipped `Ft8Sharp.Tests SUITE_TOTAL_PENDING` — **a
reported number naming a total nobody read back.** A report with all four
sections and an unfilled number passed every shape rule then in force.

**Found by:** unit 249, reading 248's report.
**A check would have caught it**, and now does: rule 7 of
`tools/arbiter/validate-output.bat`. It is not a unit test and is not in the gate
set; it runs from `run-unit.bat` on every report.

### B11 — the check written to catch B10 could not catch B10

**Unit 249.** The first cut of rule 7 read **the first 60 lines** — the window rule
6 uses — and **248's token sits at line 71**, because that report's header runs
long where `NUMBER:` and `TESTS:` wrap. **The rule was written for one case and
could not see it.** The boundary is now everything before the `## 1.` heading,
which is what *the header block* actually means rather than a guess at how long a
header gets.

**Found by:** watching it fail first, against the real file, before believing it.
**This is the entry that argues for watched-failing-first**, and it is why a
green check is not evidence until it has been seen red for the right reason.

### B12 — the port's gates were assumed to be in the path rather than re-checked

**Units 245 to 248 asserted the gates inside the sibling; unit 249 asserted them
in Hamlet's path.** Every unit of the sensitivity phase could say that
`Ft8Sharp.Deep` submits codewords to the port's parity check and CRC-14 — none of
them could say that *what Hamlet displays* passed them, because Hamlet was not
calling the sibling at all until unit 249. The engine test now **re-checks every
returned message by packing it back into its 77 bits** rather than assuming.

**Found by:** unit 249, writing the test the phase plan asked for.
**A test would have caught a bypass**, and it is gate entry 2's first line. **This
is the §0.0 hazard of the whole phase**: a wrong decode lands in the operator's
table looking exactly like the others.

### B13 — a capture taken before unit 249 cannot say what decoded it

**Unit 249.** The tree holds captures from **both sides of the switch from the
port to `Ft8Sharp.Deep`**, and on the sheet they are indistinguishable — same
fields, same five counts, different decoder. Every capture taken before that unit
is **unattributable**, and a capture read six months from now cannot be compared
against anything unless it says what read it.

**Found by:** unit 249, as a consequence of making the switch.
**A test would have caught the gap** and does — including the one that matters
most and costs nothing: **an unstamped census says *unrecorded* rather than
naming the port by default**, because a plausible default is worse than a hole.
Gate entry 7.

### B14 — a column headed `snr` was committed on the assumption that a decoder produces one

**Work instruction 037, and it stood for two hundred units.** The Digital tab's
decoded table was committed with five columns, one of them `snr`, **48 pixels
wide and reserved**. `Ft8Sharp` does not produce a signal-to-noise ratio: it
produces a **Costas sync score**, a count of how far the expected tone stood
above the average of the eight, in no units and calibrated against nothing. That
count is carried on `Ft8Decode.SyncScore` and has sat **one formatting call away
from the cell** ever since.

**Found by:** review, not by a test. `DigitalDecodeRow` and `Ft8Reception` each
grew a paragraph saying in prose that the column carries a dash and why, and the
column carried the dash from 037 until unit 251. **The prose worked. It is the
only thing that did.**

**A test would have caught the thing the prose cannot stop**, which is the *next*
edit rather than the original one: a plausible number appearing under that
heading. Unit 251's measurement is that test — it takes the estimate at the place
the decoder actually reports and compares it against the ratio the ladder
delivered, so **anything that is not a signal-to-noise ratio in the 2500 Hz
reference bandwidth reddens it by tens of decibels**. A sync score substituted
for the estimate does not agree with a commanded ratio at all.

**And it caught something on its first run, which is why the entry is here rather
than in part B.** Watched failing first, the estimate taken at the decoder's own
reported place without alignment was **3.50 dB out over 510 messages** — 3.78 dB
at −18 and 10.57 dB at −6, all of it at the cell centre, worsening as the signal
got *stronger*. **That is a number 10 dB out under a heading an operator reads
before the message**, and it was one commit from shipping. `CLAUDE.md` §0.0's
arithmetic — a message shown that nobody sent is worse than a message missed —
governs a decibel figure nobody measured in exactly the same way.

**The two smaller regressions it also guards** are named because each is a single
edit: averaging `TonePowerGrid`'s decibels instead of inverting its `1e-12` floor
first is **2.51 dB** low on the noise floor and therefore 2.51 dB high on the
answer, and dropping `Ft8DeepSlotDecoder.CandidateTimeBiasSeconds` is one whole
symbol and **2.5 dB**. Both would leave a plausible column.

---

## B. Breakages no test would have caught

**These are worth more than the coverage above.** Every one of them got through,
and none of them is the kind of thing a unit test can see.

### G1 — every figure this phase quoted was taken where the grid had nothing to lose

**Found at unit 248, after units 243 to 247 had all quoted numbers.** The ladder's
`DefaultFrequencyHz` is **320 transform bins exactly** and `DefaultOffsetSamples`
is **six sub-blocks exactly** — both axes sit on the analysis grid. At -20 dB over
306 trials the port reads **73 on the grid and 0 at the centre of the same cell**;
at -19 dB, **248 and 6**. Averaged uniformly over the cell the port reads **8.1
per cent against the 23.5 it had been quoting.**

**No test would have caught this.** Every test passed; the instrument was
answering a narrower question than anybody was asking. It was found by a unit
choosing to measure the instrument instead of the decoder. **Nothing on 14.074
arranges itself on an analysis grid.**

### G2 — a suite nobody can finish, and four reports with no total in them

`Hamlet.RadioEngine.Tests` had **never once completed a whole-project run** —
started alone at 08:15 on 2026-09-01 and cut off at 09:16 — and **four
consecutive reports carried no total for it.** The mechanism of the silence is
mechanical and worth writing down: **the console logs in this tree are UTF-16**,
so a filter reading them as UTF-8 finds nothing and reports **zero**, which reads
exactly like a suite that ran and had nothing to say.

**No test would have caught this.** It is the reason `docs/gate-set.md` exists.

### G3 — contention turned one standing failure into five

**2026-08-31.** Test projects run concurrently produced a failing set that was not
reproducible; the measurement it poisoned is still labelled unreliable in
`docs/test-baseline.md`. **One project at a time, never concurrently**, is a rule
paid for once.

**No test would have caught this** — the tests were fine and the harness was not.

### G4 — a working toolchain was written off on one refused probe

**Unit 243.** A previous unit declared `dotnet` dead after a single refused call —
`dotnet --version`, **which is simply not one of the spellings `.run-unit`'s
allowed list names**. `dotnet build` and `dotnet test` both ran and always had. A
whole night was spent blocked on a false negative.

**No test would have caught this.** The remedy is a convention: probe before
believing, and record every refusal verbatim.

### G5 — three sessions killed by the watchdog in one day

**2026-09-05**, from `RUN_LEDGER.md`: `01:32→02:48`, `12:02→12:35`,
`13:09→13:47`, each *killed by the watchdog: no status write within 12 min of the
launch clock*. Every one of them was sitting in a poll loop over a backgrounded
command with a 900,000 ms timeout. **The suite was incidental; the poll was
fatal.**

**No test would have caught this**, and it is the direct cause of the standing
rules now recorded at the top of `docs/gate-set.md`.

### G6 — a scoring artefact that read as a decoder losing something

**Unit 247.** Scoring the combined column on the **last slot alone** threw away a
message the first slot had given up on, and made one trial of 51 read as **lost
by combining**. Combining had lost nothing; the scoreboard had. It is scored on
the union over a trial's slots now.

**No test would have caught this.** A measurement can be wrong in a way that
looks exactly like a result.

### G7 — the near-miss that says how a test can be worse than none

**Unit 240.** The spectrum ring was rewritten — 78 million floats a buffer, 99.5
per cent of everything the device callback did. Its guard,
`tests/fixtures/spectrum/waterfall-frames.txt`, was captured **from the old
implementation, before a line of it was touched**, and committed in that state.
**A test that captured its own expectation after the change would have passed
whatever the change did** (`CLAUDE.md` §12.5). The fixture is pushed in
4,800-sample buffers on purpose, because 4,800 does not divide the 4,096-sample
hop and a ring that only worked when it did would pass a tidier fixture and fail
on the radio.

**This one did not break.** It is here because it names the failure mode
precisely: **the expectation must predate the change, or the test is a
photograph of the bug.**

---

## What this list is used for

| Gate-set entry | Breakages it cites |
|---|---|
| 1. Deep is a superset of the port | B2 |
| 2. The port's gates are in Hamlet's path | B1, B3, B12 |
| 3. `Ft8Sharp` references nothing outside itself | B4 |
| 4. The ladder returns nothing that was not sent | B5 |
| 5. Deep adds and never removes, with the stages on | B6 |
| 6. The census reaches all three surfaces | B7 |
| 7. A decoder's identity is recorded | B13 |
| 8. One slot decodes inside the budget | B1 |
| 9. The `snr` column carries a ratio and not something else | B14 |

**B8, B9, B10 and B11 are real and are deliberately not in the gate set**: two are
guarded by their own units' tests over code this phase does not touch, and two are
guarded by `validate-output.bat`, which runs on every report already. **G1 to G7
are not guarded by anything and cannot be**, which is why they are written down
here instead.
