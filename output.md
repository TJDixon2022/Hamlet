```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Step 0 partial, step 1 done, step 2
   partial on 2.4 alone, steps 6, 7 and 8 done by units 376, 378 and 379, step 4 and
   step 5 not started with step 4 waiting on step 3. Step 3 is partial: unit 380 met
   3.2, 3.4 and 3.5 and the judging session named 3.1 and 3.3 open. This is the
   second unit on step 3.
B. Step 3, criteria 3.1 and 3.3 - which row, and not how many. 3.1 met: of 1,740 rows
   counted in the record over the busy-band fixture, 1,740 are named by their own
   place, 0 counted only, 0 dropped and declared; and over the row-state fixtures
   every drawn, filtered, trimmed and folded row is named, with a row Hamlet cannot
   place counted, left out and declared in atDropped. 3.3 met: from the four-signal
   fixture the file names 700, 1,100, 1,600 and 2,200 Hz - all four carriers, with
   1,100 named again on his own side - and from the six-row gate case it names
   1,084, 1,802 and 2,205 Hz held by the CQ filter. Re-checked and not assumed: 3.2
   held, 3.4 37.1 kB an hour against 50 at 5.07 bytes a place measured on the file,
   3.5 CallsignPrivacyTests 4 of 4 at 82 writers, unmoved.
C. The report last. Section 4 raises 5 items on top of the carried twenty-seven, and
   none of them is in the way of a criterion in B. Unit 380's items 2, 3, 5, 6 and 7
   came off the queue - two were measurements this instruction now states as fact,
   two were disclosures acted on, and the fifth is built into ruling 1 - and its
   items 1 and 4 stay on, with item 1 answered tonight in the file this unit was
   already in.
```

```
UNIT:       381 - complete at task 4 of 5 - 2026-09-21 22:12
PHASE GOAL: Hold on to what Hamlet already has. Everything banked in the PSK31 and
            Olivia threads that is screen, record or test and needs neither the
            radio nor the owner, hardened while Tim is away and judged by tests
            that ran - and at the end by Tim at his own window.
UNIT GOAL:  The record already said how many rows were hidden and by what. It did
            not say WHICH. Put every row's and every card's own place - its offset -
            inside the line that already counts it, so a reader can walk the file
            and name each row that was held back rather than only count them, and
            keep it under 50 kB an hour.
ADVANCED:   step 3, criteria 3.1 and 3.3
NUMBER:     rows the record NAMES rather than counts, over the four-signal fixture and
            the six-row gate case: 3 of 10 -> 10 of 10; the cost,
            37.1 kB an hour against 50, at 5.07 bytes a place
DRIFT:      none
```

## 1. What Claude did

**Complete, at task 4 of 5 - every task done and no drop candidate taken.** Windows 11,
`C:\Source\HamLet`, project gate `PROJECT: Hamlet` verified against the tree, branch `main`.

**The project gate passed on all four checks**: `SHACK_FACTS.md` present,
`src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` present, no `CoreHMI.sln`, no `MURC.sln`,
root `C:\Source\HamLet`.

**The judging session's verdict was transcribed, not re-argued.** Unit 380's own task-0 claim of
all five criteria stands where it was written; beside it, in the step 3 lines of both
`PHASE_STATUS.md` and `PHASE_OUTCOME.md`, is the `STATE_AFTER: partial` the separate session
returned, verbatim, naming 3.1 and 3.3 as the open ones. That is transcription of a verdict
another session returned. It is not a judgment of this unit's, not one of its two rulings, and
not a criticism of unit 380, whose measurements this unit is built on.

**Task 1 measured before anything moved, and it changed no source file.** `Unit381Trace`, five
`[Fact]` items - none needed a window, so none is an `[AvaloniaFact]`.

- **The before, read out of the record rather than quoted from the instruction.** The four
  carriers are taken off `psk31_carrier_appeared` at 700, 1,100, 1,600 and 2,200 Hz. The file
  **named 2 of 4** - 700 on the drawn line of count 4, 1,100 on the `addressed_to_operator` line
  of count 1 - and **counted the other 2**: five row mentions in two lines carrying two offsets,
  one place per LINE and never one per ROW. The six-row gate case **named 1 of the 3** it held.
- **The row rate re-measured off the file**: 112 rows over 8 slots, 14.0 a slot, 240 slots an
  hour - **3,360 rows an hour, agreeing with unit 380 exactly**.
- **What a place costs, each figure a difference between two lines in a file and never `.Length`
  on a string this test built**: baseline 268 bytes; a bare number array in whole Hz **5.07 bytes
  a place**; at one decimal **6.93**; an array of `slot+offset` strings **14.07**.
- **The arithmetic**: the busy band writes 16 lines and 4,283 bytes over 1,200 s - 12.5 kB an
  hour, unit 380's figure to the tenth - and those lines count 1,740 items, **5,220 places an
  hour**. Whole Hz 38.4 kB an hour, fits; one decimal 47.9, fits; strings 84.3, does not. **The
  window does not move** and did not need to. **The cap is 128**, arithmetic: 37.5 kB left for
  places over 48 lines an hour at 5.07 B is 157.5 places a line, and 128 is the largest power of
  two at or under it - while the busiest measured group holds 112, so nothing truncates.
- **Where the identities live**, key shapes printed and never a key: the row map by object, the
  text map by channel, **the card map by callsign** with a window identity of `"c|" + who`, which
  would leak verbatim.

**Task 2 was three commits, one per site group.** One new parameter on `AppEvents.OnScreen` -
`OnScreenPlaces`, a record, for the same reason `OnScreenViewport` is one - carrying `At`,
`Dropped` and `SlotLast`. The payload gains `at`, with `atDropped` and `slotLast` where ruling 1
items 4 and 2 call for them. **No new writer, no new event name, no new telemetry category, no
second line.** `count` is unchanged and still counts items; `offsetHz`, `slot` and `dialHz` are
unchanged and still the head. `OnScreenGroup.Items` stays a set and a place is added only where
the group has not seen that identity, so a place list is not a licence to count changes again.
`count == at.length + atDropped` always holds and is asserted on every line of every guard.
**The privacy walk grew in the same commit as the parameter** with a populated `at` through every
branch, and `ExpectedEventMethodCount` did not move.

**Task 3 was two R12 rewrites, each its own commit after the change it is about**, plus the
re-checks. Both grown guards had to have their fixtures moved first, and that is the finding as
much as the growth: **unit 380's six rows were all heard at 1,240 Hz**, so a record naming one row
and a record naming all six read identically and 3.1 could not be tested at all.

**One decision this session made for itself, reproduced in full.** Ruling 1 item 1 says the writer
gains *one parameter and the payload gains one key, `at`*; items 2 and 4 of the same ruling then
require `slotLast` and `atDropped` on the line. Those cannot both be literally true. **The reading
taken is that `at` is the one list-key of item 1 and `slotLast` and `atDropped` are items 2's and
4's own fields**, and that the one new parameter is one *parameter* - a record carrying all three,
in the idiom `OnScreenViewport` already set in this file - rather than three positional
parameters on a writer that already had eleven. It is a finding and not a ruling request; section
4 item 3 states it.

**Unit 380's item 1 was answered in the file this unit was already in**, as section 3 directs.
`AddDecodeRowForTests` said in its own remarks that it is *the same door the decoder uses*, which
is untrue of the tree. Four lines of prose, no behaviour, no signature, no call site, its own
commit.

**Nothing on a send path was touched**: no composer, no `Arm`, no `PttOn`, no cap, no RSID burst,
no Olivia variant gate. **No file under `src\Hamlet.RadioEngine\` was opened except to read it**,
and `git diff` over the whole unit shows no file under `Views`, none under `Controls` and none
under `Hamlet.RadioEngine` changed at all.

**No task was dropped.** The three named drop candidates - the trace's item 2, `slotLast`, and the
card place list - were all kept, because the measured hour left 12.9 kB of margin and none of them
had to be spent.

## 2. What the owner should expect

When a station you were working disappears off the list, the file now tells you **which station**.
Before tonight it could say *three rows were held back by the CQ filter* and show you one
frequency that was probably not yours; from tonight it says *the rows at 1,084, 1,802 and 2,205 Hz
were held back by the CQ filter*, and you can find yours by the offset you were tuned to instead
of trusting a count. The same is true of everything else that can take a row off your screen - the
trim aging it out of the table, a panel you had folded shut, a row that never reached the left list
- each one now names every row it took. It costs **37.1 kB an hour** on a busy band against a
ceiling of 50, measured on the file rather than estimated, with every one of the 1,740 rows named
and none left merely counted. Nothing on your screen changed: no control, no label, no layout, no
colour, and unit 354's nine window sizes read the same numbers to the pixel. Nothing personal went
into the file - an offset, a slot, a kind, a count and a reason, never a callsign, never a grid,
never a word of what was decoded - and where Hamlet genuinely has no offset for something it says
so rather than writing a zero that would read as a station at the bottom of the band. Every figure
here was computed on the development machine (FACT-004): **no port was opened, no device was
enumerated and nothing was keyed.**

**What will look wrong but is not.** The `on_screen` lines in the file are longer than they were,
and on a busy band one of them can carry a list of a hundred and twelve numbers. That is the
whole point of the unit and it is inside the budget with 26% to spare.

## 3. What you should see

**The unit 337 diagnosis, read row by row out of the file.**

| Carrier | What the file said BEFORE tonight | What it says AFTER |
|---|---|---|
| 700 Hz | **named** - the head of the drawn line | **named** - `at[0]` of `[700,1100,1600,2200]` |
| 1,100 Hz | **named** - the head of the `addressed_to_operator` line | **named** on both sides, the same carrier twice |
| 1,600 Hz | **counted only** - inside a count of 4 | **named** |
| 2,200 Hz | **counted only** - inside a count of 4 | **named** |

Before: `{"kind":"row","state":"drawn","by":"","count":4,"subMode":"PSK31","offsetHz":700}`
After: `{"kind":"row","state":"drawn","by":"","count":4,"subMode":"PSK31","offsetHz":700,"at":[700,1100,1600,2200]}`

And the answer itself is unchanged and still the one nobody expected: **all four stations were
drawn and not one row was filtered.** If the list looks empty on PSK31 tonight the band is empty,
because since unit 337's own repair the CQ button does not touch a PSK31 or an Olivia row at all.
The difference is that the file can now be asked about each carrier by name rather than asked to
be believed about four.

**The gate that does fire, beside it.** Six FT8-shaped rows with the CQ toggle on:

| Offset | What it is | Before | After |
|---|---|---|---|
| 617 Hz | a CQ | counted | **drawn, named** |
| 884 Hz | a CQ | counted | **drawn, named** |
| 1,084 Hz | between two other stations | named, as the head of a count of 3 | **held by `cq_filter`, named** |
| 1,410 Hz | addressed to the operator | counted | **drawn on his own side, named** |
| 1,802 Hz | between two other stations | counted | **held by `cq_filter`, named** |
| 2,205 Hz | between two other stations | counted | **held by `cq_filter`, named** |

**3 of 10 named before, 10 of 10 after**, across the two fixtures.

**The arithmetic, measured on the file.**

| Encoding | Bytes a place | Places an hour | Places | Lines | Total an hour | |
|---|---|---|---|---|---|---|
| bare number array, whole Hz | **5.07** | 5,220 | 25.9 kB | 12.5 kB | **38.4 kB** | fits |
| bare number array, one decimal | 6.93 | 5,220 | 35.3 kB | 12.5 kB | 47.9 kB | fits |
| array of `slot+offset` strings | 14.07 | 5,220 | 71.7 kB | 12.5 kB | 84.3 kB | **does not fit** |

**Whole Hz was taken.** The cap that follows is **128** - 37.5 kB of place budget over 48 lines an
hour at 5.07 bytes is 157.5 places a line, and 128 is the largest power of two under it. **The
window did not move**: widening it buys the 12.5 kB of envelope only, because places scale with
rows and not with windows, which makes it the smallest lever and not the first.

**The hour, re-earned end to end and not in a spreadsheet**: 80 slots x 14 rows, 1,120 decodes, 16
lines, **12,667 bytes over 1,200 s of band = 37.1 kB an hour against 50**, a margin of 12.9 kB and
**26% of the ceiling**. It was 12.5 kB before the places, so the places cost 24.6 kB an hour.

**Nothing was capped, and no group came near it.** 1,740 items counted, 1,740 places carried, 0
declared absent. The busiest group held **112** against a cap of 128. `drawn` did not have to take
the cap first and the gated states did not have to be protected, because ruling 2 item 3's
fallback was never reached.

**The privacy scan over `at`.** `CallsignPrivacyTests` 4 of 4 with `ExpectedEventMethodCount`
**unmoved at 82**, the walk grown in the same commit as the parameter with a populated `at` through
every branch, plus the truncated shape, the spanned-slot shape and a card Hamlet has no
measurement for. This unit's own scan is extended over `at` and `atDropped` and asserts **every
element is a number** - a stronger claim than a word list, because nothing that could hold a name
can be in the list at all, whatever a future call site is tempted to hand in. The card map is keyed
by callsign and that key never reached the file: a card is named by the offset of the station it
stands for, or counted and not named.

**Unit 354's nine sizes**, re-measured: `483, 71, 92, 163, 171, 267, 483, 460, 860` - **identical
to units 376's and 380's, to the pixel** - with `TheStopIsAlwaysOnScreenTests`, `TheTopRowTests`,
`TheWorkingPanelsTests` and `BindingHealthTests` 29 of 29. And stronger than a measurement this
time: **no file under `Views`, none under `Controls` and none under `Hamlet.RadioEngine` changed at
all.**

**The carry-forward counts, before and after, with the fate of every invocation named.**

| | App | Engine |
|---|---|---|
| Task 0 attempt 1 | **completed RED, 218 of 219** - dispatcher loop, not an assertion | — |
| Task 0 attempt 2 | **completed GREEN, 219 of 219** | **completed GREEN, 150 of 150**, first attempt |
| Task 4 attempt 1 | **completed RED, 219 of 220** - dispatcher loop, not an assertion | — |
| Task 4 attempt 2 | **completed GREEN, 220 of 220** | **completed GREEN, 150 of 150**, first attempt |

**Name for name, the only difference between task 0 and task 4 is this unit's one new
carry-forward guard**, `TheRecordSaysWhatWasOnScreenTests.ARowHamletCannotPlaceIsCountedAnd
DeclaredRatherThanInvented`. **No regression; nothing red that was green before** (HM-DEC-165).
**Not one name failed an assertion in any of the four invocations.** The dispatcher loop took an
app attempt in both rounds - a **tenth** name at task 0
(`ThePowerIsOfferedTests.TheOfferRendersAtHalfAndNothingMirrorsTheUsbModLevel`) and an **eleventh**
at task 4 (`TheTestsStayOffTheNetworkTests.ThePlainFixtureTakesGeneralFromTheFixedAnswer`), both at
1 ms in `HeadlessUnitTestSession.EnsureApplication` before any assertion, both re-run clean. Unit
380's file-lock `IOException` did not reproduce. The engine invocation has still never failed since
unit 375.

**The named types, task 0 against task 4**: the nine types this unit can move went **64 of 64 to 65
of 65**, the one new name the only difference. `Unit381Trace` 5 of 5, `Unit380TraceTests` green.
**Both inherited reds re-run and both unchanged**: `ViewTestsActThroughControlsTests` 1 of 2 with
its message character for character, and `TheOliviaMoveUpTests.ItIsNotOfferedOnAGuessedYourTurn`
red as section 9 says. No task of this unit edited either file.

## 4. What's blocking us

**Nothing is blocking. Every item below is a finding or a disclosure, and none of them asks the
owner to decide anything.** No item wants a ruling; no criterion in B depends on any of them.

**1. A third shape of environmental test fault, and this one is a marginal guard rather than a
platform fault. A finding.** `TheRecordDiagnosesTheEveningTests.AScrollerThatSettlesSaysWhatIs
InsideItOnBothPanels` - unit 380's, not on the carry-forward list - is intermittent under parallel
load. It waits 600 ms of wall clock (twenty sleeps of 30 ms, each followed by a forced render tick
and `RunJobs`) for a 250 ms `DispatcherTimer`, and when enough CPU-heavy `[Fact]`s run alongside it
in one invocation the timer's signal is not delivered inside the budget, so **no `scrolled_out`
line is written at all** and the failure reads `Assert.NotEmpty() Failure: Collection was empty` at
line 453. **It is environment and not this unit's payload, and that was proved rather than
assumed**: it fails the same way in an invocation crowded with **nine types this unit never
touched** - `TheTopRowTests`, `TheWorkingPanelsTests`, `ThePsk31PanelHearsTests`,
`ThePsk31ExchangeTests`, `TheCqReceiptTests`, `ThePressingOfCqTests`, `TheDecodedTableIsRealTests`,
`TheOliviaRowsTests` - none of which writes an `on_screen` line or carries a place. It is green
every time in isolation, and the privacy walk this unit grew costs 40 ms, so the bytes added are
not the load. **What this unit could NOT establish, and says so plainly: the rate before tonight.**
The A/B needed `git stash` or `git checkout <sha> -- <paths>`, and **both were refused by the
permission mode** in this non-interactive session, so no pre-change rate was measured and none is
claimed. What is known is that the nine named types were 64 of 64 green at task 0 and 65 of 65
green at task 4, and that the name failed in between on roughly half of the crowded runs. The
honest repair - lengthening the guard's wait or driving the settle deterministically - is unit
380's test and not this unit's to touch, and it is offered to whichever unit next works in that
file.

**2. The record names two carriers before tonight, not one. A finding, and a measurement that
refines this instruction.** Work instruction 381 section 4 says *four carriers at 700, 1100, 1600
and 2200 Hz, and the file names one of them*, and the judging session's verdict says *four carriers
reported under offset 700*. Both are right about the line they quote and the whole file names
**two**: the four-signal fixture writes two lines, one headed 700 for the left list and one headed
1,100 for the operator's own side, so 1,100 is named as well. The before is therefore **2 of 4
named and 2 counted**, not 1 and 3. It does not change the verdict or the work - two of four is
still not *which rows* - and the numbers in section B and section 3 are the measured ones.
Nothing was repaired on the strength of it.

**3. The one new key is three keys, and the one new parameter is one record. A finding, and the
one decision this session made for itself.** Ruling 1 item 1 says the payload gains **one** key,
`at`; items 2 and 4 of the same ruling then require `slotLast` and `atDropped` on the line. The
reading taken is that `at` is item 1's one list-key and the other two are items 2's and 4's own
fields, all three conditional and absent where the fact is absent. The parameter really is one:
`OnScreenPlaces`, a record carrying all three, in the idiom `OnScreenViewport` already set in that
file - because a twelfth, thirteenth and fourteenth positional parameter on a writer that had
eleven is one a call site gets wrong in silence. **It is disclosed rather than done quietly.**

**4. `OnScreenBy` has no squelch token, so there is nothing to report there. A finding, and it
closes the question section 5 asks.** Section 5 says *`OnScreenBy.Squelch`, if it is still in the
token list, names a gate that cannot fire on a row*. It is not in the list:
`src\Hamlet.App\Telemetry\OnScreen.cs:53` holds exactly five tokens - `""`, `cq_filter`,
`addressed_to_operator`, `trim` and `dismissed`. Unit 380 measured that the squelch is not a
visibility gate and never added one. Nothing to repair.

**5. `validate-output.bat` refused for the third unit running, and the six rules are hand-checked
below. A finding about the harness, not about the report.** The command was run in the exact shape
section 2 names:

```
./tools/arbiter/validate-output.bat output.md
```

and the exact refusal was:

```
This command requires approval
```

That is the permission mode and not the syntax - **a non-interactive session cannot answer it** -
and it is the same refusal from the same shape that units 379 and 380 both reported. **What
follows is a HAND-CHECK against the script's own source at
`tools\arbiter\validate-output.bat`, not a run of it, and it is worth exactly what a hand-check is
worth: the script was not executed and nothing independent agreed with me.**

| Rule, as the script states it | Hand-check | Verdict |
|---|---|---|
| 1 - a `UNIT:` line above section 1, parseable, within the first 60 lines | `UNIT:` is line 28; section 1 is line 45; the file is UTF-8 with **no BOM** (first bytes are `` ` ` ` \n ``), so the BOM fault the script's own comment describes does not arise | **ok** |
| 2 - the four top-level sections, in order, exact names | the only `^## ` lines are 45, 123, 145, 241: `1. What Claude did`, `2. What the owner should expect`, `3. What you should see`, `4. What's blocking us` - matching the script's `WANT` string word for word, including the apostrophe | **ok** |
| 3 - no fifth top-level section | four `^## ` lines and no more; the one `### ` heading is the carried queue below, which the script says in its own comment it ignores | **ok** |
| 4 - section 4 present even when empty | `## 4. What's blocking us` at line 241, and it is not empty | **ok** |
| 5 - section 3 non-empty | 77 non-blank lines between `## 3.` and `## 4.` | **ok** |
| 6 - the ordering block above the `UNIT:` line, with A, B, C, and C naming a count | `READ IN THIS ORDER.` line 2, `A.` line 4, `B.` line 9, `C.` line 19 - all inside the 60-line window the script reads - and C says *Section 4 raises 5 items*, which matches the script's `raises \d+ item` | **ok** |

**All six rules pass on the hand-check. Nothing verified them but me.** The ask stays on the queue
as unit 379's item 7, carried through unit 380's item 8; this unit adds only that it is now three
units in a row.

**The `RULES_AT` split, reported and not repaired.** It is unit 380's item 9:
`PROJECT_STATUS.md` reads `HM-DEC-165 (2026-09-19)` because `tools/status.sh` writes that field as
a literal, while `CLAUDE.md` §1 holds `CPS-DEC-0165`. **`tools\` is not this unit's to edit.**
Reported, not repaired.

**No item of the kind section 12 singles out arose.** The measured hour held every place with 12.9
kB to spare, so 3.1 is met rather than partial and the ceiling was never re-read. A place was
unavailable for exactly one deliberately constructed state - a row whose `Hz` will not parse - and
that is named, sited and declared in `atDropped` rather than guessed. **Nothing needed a change on
a send path.**

### The carried queue, verbatim per HM-DEC-139 - twenty-seven, and this unit answers none of them

- **Unit 380 item 1** - `AddDecodeRowForTests` is half a door: it reaches `PlaceRow` while the
  decoder's own door `AddDecodeRow` also keys the duplicate set and runs the trim, so no test could
  reach the row cap through it. **Answered tonight in the remarks, in the file this unit was
  already in; the hook itself is untouched and the ask stays on the queue.**
- **Unit 380 item 4** - a second shape of environmental test fault, the file-lock `IOException` at
  `TheOliviaRowsTests.cs:675`: *the process cannot access the file ... `refuse\2026-09-21.jsonl`
  because it is being used by another process*, the test's own reader racing the telemetry writer's
  background thread. It did not reproduce tonight.
- **Unit 379 item 1** - the CQ list's mode label.
- **Unit 379 item 3** - the `80m` spelling.
- **Unit 379 item 7** - `validate-output.bat` returns `This command requires approval` from the
  exact documented shape, which is the permission mode and not the syntax, and a non-interactive
  session cannot answer it.
- **Unit 378 items 1, 3 and 4.**
- **Unit 377 item 4.**
- **Unit 376 items 3, 4 and 5.**
- **Unit 375 item 3** - Avalonia's headless `InvalidProgramException: You have caused dispatcher
  loop`, which kills an app invocation at about 1 ms in `HeadlessUnitTestSession.EnsureApplication`
  before any assertion and moves between names. **Eleven names now**, two of them this unit's two
  entry and exit attempts.
- **Unit 375 item 4.**
- **Unit 374 item 3.**
- **Unit 373 item 2.**
- **Unit 372 items 4 and 7.**
- **Unit 371's five.**
- **Unit 369's four.**

**Which of unit 380's nine came off, and why.** Items 2 and 3 - the squelch is not a visibility
gate, and instruction 380 omitted `AppEvents.DecodesReachedTheScreen` - were measurements that
contradicted their instruction and won, and work instruction 381 section 5 now states both as facts
of the tree. Items 5 and 6 - the carry-forward line one commit late, and the trace running as
`[AvaloniaFact]` - were disclosures, and section 11 carries the first as a rule and task 1 the
second as permission. Item 7 - a slotted row's `where` carries its tone offset beside its slot and
dial - was accepted and made load-bearing: ruling 1 is built on that offset, and without it a
slotted row could not have contributed a place at all. Item 8 stays on as unit 379's item 7 and
item 9 is reported in item 5 above. **Items 1 and 4 remain, and the queue is twenty-seven.**
