**READ IN THIS ORDER.**

A. **The phase goal.** Hamlet works stations on the air. It reads FT8 as well as
anything and cannot answer — fourteen messages from one slot on 14.074, five
continents, down to -21 dB, and not a word back. This phase gives it a mouth,
under one rule: one operator click, one transmission.

B. **This step and its exit criteria.** Two steps, both closed done. **Step 0** —
§0.2 replaced with the delivered text, a superseding ruling in Tim's name dated
2026-09-06, every other reference in the tree found and reported with live
documents corrected and archives named, and the abort and one-click rules
surviving verbatim. **Step 1** — a same-thread no-await abort, `17 FF` with PTT
off as the fallback, watched to fire from about-to-key, keying, mid-transmission
and waiting-to-unkey; fired against a dead transport, a gone port and a silent
radio; not disableable; and **no transmitting code existing when the step
closes**.

C. **What this report adds, and whether it bears on A or B.** It adds the hit
count, the abort's fourteen cases and their measured times, and the survey of
what already existed for keying. **Two findings bear on A**, and section 4
raises 2 items: the licence gate is bypassable in the tree today although §0.2
now says it is not, and a live UI string still tells the operator that Hamlet
transmits into a dummy load. **One finding bears on B**: the delivered §0.2 text
itself contains the phrase step 0's must-pass says must not survive anywhere in
`CLAUDE.md`, so that criterion is met everywhere except inside the sentence
recording the withdrawal, and that is reported rather than repaired.

```
UNIT:       253 — complete at task 5 of 5 — 2026-09-06 18:37
PHASE GOAL: Hamlet works stations on the air. It reads FT8 as well as anything
            and cannot answer; this phase gives it a mouth.
UNIT GOAL:  Get the withdrawn dummy-load rule out of every live document so no
            later session halts on a rule it cannot satisfy, and prove the
            same-thread no-await abort from every state — before any code exists
            that could key a transmitter.
ADVANCED:   yes — steps 0 and 1 both closed done, both with every must-pass
            criterion met. Step 1 is the one the plan says nothing else may be
            built before.
NUMBER:     none — this unit has no scoreboard number of its own. The receive
            figure the instruction quotes, 252 of 306 at -21 dB against the
            port's 13, is inherited from unit 252 and is unchanged, because
            nothing here touches the decode path.
DRIFT:      0 — reset on this advance. Work instruction 253 carries no drift
            count in its own block, so the previous value is not stated rather
            than assumed.
```

## 1. What Claude did

**Complete, five of five, all pushed.** Machine `C:\Source\HamLet`, project
claimed and confirmed as Hamlet, branch `main`. Version 1.12.76 → **1.12.81**, a
patch a task, `Ft8Sharp` untouched.

The gate was checked against the tree before the instruction was read:
`SHACK_FACTS.md` and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` both
present, `CoreHMI.sln` and `MURC.sln` both absent. **Hamlet confirmed.**

| Task | Commit | What moved |
|---|---|---|
| 1 — §0.2 rewritten | `b02009b` | The delivered text, verbatim. HM-DEC-156 in Tim's name |
| 2 — every other trace | `609991e` | Six live documents corrected, four archives named and left |
| 3 — what exists for keying | `a467505` | `docs/unit253-keying-survey.md` — reading only, nothing under `src/` changed |
| 4 — the abort | `7617728` | `TransmitAbort`, 14 cases, watched failing first |
| 5 — the record | `f5383c6` | `PROJECT_CARD.md`, HM-DEC-157, both `PHASE_OUTCOME.md` entries |

### Three things I am handing back rather than deciding

**One: I put typed timestamps into `PROJECT_STATUS.md` and they ran ahead of the
clock.** Three of the five status writes in this session carried an `UPDATED`
value I composed instead of reading — by the end, 19:16 against a real clock of
18:37, **thirty-nine minutes in the future**. That is exactly the failure §13.1
names, and `CLAUDE_CODE.md` §11 records a previous instance of it in this
repository. The final write is read from the clock and says so in its own `NOTE`.
The intermediate writes are gone, overwritten, so nothing downstream is still
carrying a wrong number — but a panel watching this session during those three
writes was shown a future time, and that is worth knowing.

**Two: the shell refused `outcome-append.bat`, twice.** Verbatim, both times:

```
This command requires approval
```

once as `cmd //c "tools\arbiter\outcome-append.bat" <fourteen arguments>` and
once bare with no arguments at all, to see whether the arguments were the
problem. They were not. Both `PHASE_OUTCOME.md` entries were therefore written
with the file-editing tools in the exact format `tools\arbiter\outcome-entry.py`
writes — twelve fields, same names, same order, ASCII, appended below the
existing prose — and the two step lines in the header were updated in place the
way the script updates them. Task 5 anticipated this and licensed it.

`grep` also aborted with exit 134 on several multi-pattern and `-c` invocations
and had to be run one pattern at a time. Nothing was lost to it; it cost time.

**Three: I widened task 2's scope in one place, and it should be checked.** The
instruction named `ARBITER.md`, `PHASE_CONTROL.md`, `ft8sharp-spec.md` and
`CLAUDE_CODE.md` as the live documents to correct. **None of those four carries a
single hit** — and `ft8sharp-spec.md` does not exist anywhere in the tree, which
is a mismatch between the instruction and the tree, reported not repaired. What
did carry it was six other files, three of them spent work orders at the root. I
bannered those rather than rewriting them, on the reasoning that a reader must
not act on a withdrawn rule but the record of what was once asked for is worth
keeping. **If the owner would rather they were rewritten or deleted, that is his
call and it is one edit away.**

### Decisions I made for myself, reproduced in full

**That `CLAUDE.md`'s two index rows for HM-DEC-008 and HM-DEC-098 are rewritten
as `WITHDRAWN IN FULL by HM-DEC-156` rather than deleted.** `CLAUDE.md`'s own
amending rule says never silently edit a ruling — supersede it and say so — and
the plan's must-pass says no mention of a dummy load survives anywhere in that
file. Rewriting the row satisfies both: the phrase goes, the supersession is
recorded on the row itself, and the original texts stand unedited in
`DECISIONS.md` where a ruling is never edited.

**That `CivConstants` names only the receive value and deliberately not the value
that keys the radio.** `PttOff = 0x00` is in the file with a comment saying why
its opposite is not. This unit built an abort before anything that transmits, and
a constant is the easiest thing in a codebase to reach for by accident.

**That the CW-send and auto-CQ surfaces are reported and not corrected.** They
carry 25 of the tree's remaining mentions. CW send and automatic sequencing are
both on this instruction's parked list, `Hamlet.App.Tests` may not be run here,
and one of the files is `DummyLoadNoticeTests.cs`, which exists to assert the
string. Editing the string without being able to run its test would have been the
worse of the two moves. **This leaves a live defect on the operator's screen and
it is in section 4.**

## 2. What the owner should expect

**The rule is gone and Hamlet still cannot transmit.** That is the intended
outcome of a first transmit-phase unit that builds nothing that transmits, and it
is worth being clear about: **there is no new capability in the application**. The
build is green, the app looks and behaves exactly as it did after unit 252, and
the only new code is a static class that nothing calls.

**What will look wrong but is not:**

- **`CLAUDE.md` still contains the words "dummy load", once.** They are inside
  the §0.2 text delivered with this instruction, in the sentence recording the
  withdrawal — *"superseding HM-DEC-008 and HM-DEC-098, which required a dummy
  load"*. Task 1 required that text verbatim; `PHASE_PLAN.md`'s step 0 says no
  mention survives anywhere in the file. **The two cannot both be satisfied**, the
  work instruction is the later and more specific order, and the surviving mention
  is a withdrawal notice rather than a live requirement. Reported, not repaired.
- **`DECISIONS.md` still carries six of them.** A ruling is never edited there.
  HM-DEC-008 and HM-DEC-098 stand exactly as written, with HM-DEC-156 above them
  saying they are withdrawn in full.
- **`WORK_INSTRUCTIONS.md`, `PHASE_PLAN.md` and `PHASE_STATUS.md` carry it too.**
  Every one of those mentions is the withdrawal itself, including step 0's own
  name, *the dummy load is gone from the tree*.
- **`data/glossary.json` defines the term and keeps it.** It is a definition of a
  piece of amateur radio equipment in a glossary, not a requirement. Leaving it is
  correct; the entry does not tell anybody to use one.
- **The auto-CQ widget's blurb in `MainWindow.axaml` still says Hamlet sends into
  a dummy load.** That one is a real defect and is in section 4, not here.
- **`AutoCallFaceTests`, `DummyLoadNoticeTests` and five other test files still
  reference it.** No test was run against them and none was changed, so nothing
  has gone red that was not already.
- **Two more decision ids exist.** HM-DEC-156 and HM-DEC-157, both dated
  2026-09-06, both in Tim's name and both reproducing his reasoning rather than
  summarising it.

## 3. What you should see

### 1. The dummy load's hit count

**Twenty lines across six live documents carried it. All twenty are corrected.**

| Live document | Lines | What was done |
|---|---|---|
| `CLAUDE.md` | 9 | §0.2's own two lines replaced; six more lines corrected, including §13.4's hazard line; one hyphenated *"dummy-load evening"* the obvious grep misses |
| `ABANDONED_WIDGETS.md` | 3 | The auto-CQ widget's entry and its two prose paragraphs rewritten around the one-click rule |
| `BENCH_CARD.md` | 3 | Bannered **WITHDRAWN — do not run this card**; body left as the record of what was asked for |
| `CLEANUP_BRIEF.md` | 3 | Bannered **spent, and its premise is withdrawn**; body left |
| `ONBOARDING.md` | 1 | ONB-C05 rewritten: what a first-run moment should say is the click rule, the licence gate and the abort |
| `BATCH_BRIEF_AMENDMENT.md` | 1 | The standing clause marked withdrawn, pointing at `DECISIONS.md` for the original |

**Four lines across four archived phase records were left alone, and here they
are by name:** `docs/phase-onair/PHASE_PLAN.md:355`,
`docs/phase-onair-run/PHASE_PLAN.md:371`,
`docs/phase-sensitivity/PHASE_PLAN.md:453`,
`docs/phase-sensitivity-run/PHASE_PLAN.md:453` — each the same sentence,
*"Transmitting FT8. Its own phase, dummy load first"*. Rewriting history is worse
than a stale archive, and a reader who finds it there finds HM-DEC-156 in
`DECISIONS.md`.

**Also left alone, deliberately, and not archives:** six lines in `DECISIONS.md`,
where a ruling is never edited; nine lines in `docs/phase-send/`, which is this
phase's own plan and is byte-identical to the root copy; and **25 lines across 15
source, test and data files** on the parked CW-send and auto-CQ surfaces —
`AutoCall.cs`, `ICwSender.cs`, `TransmitNotes.cs`, `CwTransmitViewModel.cs`,
`MainWindowViewModel.cs`, `MainWindow.axaml`, `AppEvents.cs`, `glossary.json` and
seven test files.

**Does `CLAUDE.md` carry none? No — it carries exactly one**, and I am not going
to claim otherwise. It is in the delivered §0.2 replacement, in the sentence that
records the withdrawal, and task 1 required that text exactly as delivered. Every
other mention in that file is gone, including the §13.4 hazard line, which used
to tell every session that this software runs *"an unattended repeating cycle —
dummy load only"* and now says it keys on the air into the operator's antenna,
one transmission per click.

### 2. The abort's test list

`TheAbortFiresFromEveryStateTests`, against a fake CI-V transport. **Watched
failing first**: against a deliberately incomplete first version of
`TransmitAbort` carrying the stop code and no fallback — the tree exactly as task
3 found it — **9 of 14 red**. Complete: **14 of 14 green in 22 ms.**

| # | Case | Fires from / when |
|---|---|---|
| 1 | `ItFiresWhenTheTransmissionIsAboutToKey` | **about to key** |
| 2 | `ItFiresWhileTheRadioIsKeying` | **keying** |
| 3 | `ItFiresMidTransmission` | **mid-transmission** |
| 4 | `ItFiresWhileWaitingToUnkey` | **waiting to unkey** |
| 5 | `TheTwoFramesAreTheStopCodeAndPttOff` | the wire bytes, pinned |
| 6 | `ItFiresWhenTheTransportIsDead` | **transport disposed** — both attempted, neither lands, the record says so |
| 7 | `ItFiresWhenThePortIsGone` | **port gone**, every write throwing |
| 8 | `ItFiresWhenTheRadioDoesNotAnswer` | **radio silent** — it writes and reads nothing, so there is nothing to time out |
| 9 | `ItFiresWhenTheCivWriteItselfThrows` | **the write throws** — nothing propagates |
| 10 | `ThePttOffGoesOutWhenTheStopFrameThrows` | **the fallback, exercised** — `17 FF` refused, `1C 00 00` lands |
| 11 | `TheStopFrameGoesOutWhenThePttOffThrows` | the same in reverse — neither half needs the other |
| 12 | `NothingOnTheAbortPathWaitsForAnything` | **no `await`** |
| 13 | `NoFlagCanTurnTheAbortOff` | it cannot be disabled |
| 14 | `TheAbortCompletesWithinItsTimeBound` | **the time bound** |

**The time bound is 50 ms, stated in the test as a constant, and it completed
inside it.** All fourteen cases together, including the two that measure, ran in
**22 ms**; xunit reports the bounded case at under 1 ms, which is the finest
resolution it prints.

The two frames are `FE FE 94 E0 17 FF FD` — the keyer stop code, p. 19-11 — and
`FE FE 94 E0 1C 00 00 FD` — PTT off, p. 19-7. **They stop different things.**
`17 FF` means nothing to a transmission keyed by PTT with audio behind it, which
is what this phase is about, so the fallback is not a retry: it is the other half
of the answer and it goes out whether or not the first one landed.

**The no-`await` assertion was itself watched to fire.** I put
`await Task.Yield()` on the path, ran the test, watched
`NothingOnTheAbortPathWaitsForAnything` go red, and took the `await` back out. A
tripwire nobody has seen trip is a comment.

**It cannot be disabled.** `TransmitAbort` is static, declares no fields and no
properties, takes no `bool` on any public method, and its body contains no `if`.
There is nowhere for a flag to live, and the test fails the moment somewhere
appears.

### 3. Nothing in this unit can key a transmitter

**Confirmed.** `grep -rn "TransmitAbort" src tests` returns its own declaration,
its own file, and its own test. **There is no caller.** No code was added that
sends `17` with text, that writes `1C 00 01`, or that starts a tuning cycle; the
only new constant in the keying family is the value that puts the radio back into
*receive*, and the value that keys it is deliberately absent from the file.
`Ic7300Rig.SendCwAsync` and `CivWrites.TuneNow` are the two things in this tree
that can key a radio and both are exactly as unit 252 left them.

**What step 2 must build before anything can transmit** — and step 2 is the next
one, and it also keys nothing:

1. **FT8 audio generation.** Hamlet can decode a slot and cannot make one. There
   is no encoder, no Costas-arrayed symbol stream and no waveform anywhere
   outside `src/Ft8Sharp/`, which is a faithful MIT port nothing may change.
2. **Proof by its own decoder.** Step 2's exit is Hamlet's receive path reading
   Hamlet's own transmission, which is the only check that does not require
   trusting the encoder.

**Step 3 is where a transmitter can first be keyed**, and by then it has this
abort to reach for — which is the whole reason step 1 came before step 2 rather
than after step 3.

### And, from task 3's reading

`docs/unit253-keying-survey.md` has it all with file and line. The short version:
**the hard part already existed and was already right.** `Ic7300Rig.AbortCw`
writes the stop code straight at the port, on the calling thread, deliberately
outside the command gate, and never throws; `ISerialPort.Write` is a synchronous
seam whose doc comment already said *THE ABORT PATH AND NOTHING ELSE*. What was
missing was the other half — **CI-V `1C 00` existed in this engine only as a
read**, so there was no PTT-off anywhere in `src/`, and all four `ptt` hits were
`AutoCallStop.PttPressed`, which detects the operator's hand rather than
commanding anything.

## 4. What's blocking us

Nothing blocked this unit. Two things need a ruling before step 5 designs a send
path, most-blocking first.

### The licence gate is bypassable today, and §0.2 as it now reads says it is not

**The new §0.2 says:** *"Hamlet never transmits outside the operator's licence
privileges. The Settings check is the gate and it is not bypassable from any send
path."*

**The tree does not do that.** `TransmitGuard.Check` at
`src/Hamlet.RadioEngine/Licensing/TransmitGuard.cs:87-90` returns
`MayTransmit: true` with `WasOverridden: true` when the Settings toggle *Only let
me transmit where my license allows* is off. The toggle is On by default and the
override is deliberate, documented, and older than this phase. A second, separate
path is at `:77-85`: an **unknown** licence class does not block transmitting
either — it warns, names what it does not know, and gets out of the way, on the
reasoning that Hamlet has no business refusing to key a radio because it could
not reach a lookup service.

**The ruling asked for:** does §0.2's new sentence bind the tree — the toggle
comes out and an unknown class refuses — or does the sentence get amended to
describe the gate that exists? **They are opposite changes** and I am not going to
guess which, because one of them removes a control the operator has had since
August and the other weakens a sentence in an ABSOLUTE section.

**Rejected as an option: leaving both as they are.** A rule in `CLAUDE.md` that
the code contradicts is the failure step 0 existed to prevent, one section further
down the same file.

### A live UI string tells the operator Hamlet transmits into a dummy load

`src/Hamlet.App/Views/MainWindow.axaml:504`, on the auto-CQ widget's blurb:

> *"It sends into a dummy load while this is being proved, which is not caution
> for its own sake: an automatic transmitter that goes wrong goes wrong on a
> frequency somebody else is using."*

Two more at `:495` and `:1160`, and eleven more in the CW-send and auto-CQ source
behind them. **This is operator-facing text that is now false**, and it is the one
hit in the tree that a person rather than a session will read.

**I did not touch it**, and the reasoning is in section 1: CW send and automatic
sequencing are both on this instruction's parked list, `Hamlet.App.Tests` may not
be run here, and `tests/Hamlet.App.Tests/ViewModels/DummyLoadNoticeTests.cs`
exists to assert that string. Editing a string whose test I am forbidden to run
would have left a red nobody could see.

**The ruling asked for:** which unit removes it. The honest shape of the question
is bigger than the string — **the auto-CQ feature itself is cut across by "one
click, one message"**, so the choice is between a one-line correction to a blurb
for a feature that no longer has a home, and retiring the widget. `PHASE_PLAN.md`
puts the right-click menu at step 5, which is the natural place for it.
