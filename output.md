```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Step 0 partial, step 1 done, step 2
   partial on 2.4 alone, steps 6, 7 and 8 done by units 376, 378 and 379, step 4 and
   step 5 not started with step 4 waiting on this one. This is the first unit ever
   spent on step 3.
B. Step 3 - the record says what was on screen. 3.1 met: 5 of the five row states
   written, at 5 sites. 3.2 met: 4 of the four card states, the named drop candidate
   not taken. 3.3 met: from the four-signal fixture with the CQ filter on, the record
   alone says all four carriers were drawn on the left list and one was also on the
   operator's own side, and the gate that hid a row was none - no row was filtered,
   because unit 337's own repair took the CQ toggle off a text row. 3.4 met: 12.5 kB
   an hour measured against 50, at a head budget of one line per state-and-gate group
   per 120-second window from a measured 3,360 rows an hour and a measured 267-byte
   line. 3.5 met: CallsignPrivacyTests 4 of 4 with the walk at 82 writers.
C. The report last. Section 4 raises 9 items on top of the carried twenty-five, and
   none of them is in the way of a criterion in B. Unit 379's items 4, 5 and 6 came
   off the queue - two were disclosures already acted on and the third is the count,
   stated here - and its items 1, 3 and 7 stay on.
```

```
UNIT:       380 - complete at task 4 of 5 - 2026-09-21 17:02
PHASE GOAL: Hamlet holds what it has. Bank the screen, record and test work already
            won in the PSK31 and Olivia threads, unattended, needing neither the
            radio nor the owner, and judge it by tests that ran and at the end by
            Tim at his own window.
UNIT GOAL:  Make a night that looked empty readable out of the file. For every
            decoded row and every card, the record says what became of it on the
            screen - drawn, held off the list by a gate it names, scrolled past,
            behind a folded panel, or aged off the table - so the question that cost
            a screenshot in September is answered from the record alone, at a size
            that does not swamp a busy FT8 evening.
ADVANCED:   step 3, criteria 3.1, 3.2, 3.3, 3.4 and 3.5 - the whole step
NUMBER:     of the five visibility states a decoded row can be in, those the record
            named before tonight: 0 of 5 -> 5 of 5; and for a card: 0 of 4 -> 4 of 4;
            the cost, 12.5 kB an hour against 50
DRIFT:      none
```

## 1. What Claude did

**Complete, at task 4 of 5** (tasks 0 to 4). Development machine, project gate checked
against the tree and passed - `SHACK_FACTS.md` and
`src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs` present, no `CoreHMI.sln`, no
`MURC.sln`, root `C:\Source\HamLet` - branch `main`.

**Nothing was dropped.** The instruction named one drop candidate, the scroll-settle
reporter on the For You (cards) panel, and it was taken rather than dropped, so all
four of 3.2's card states are written and 3.2 is met rather than partial.

### Task 0 - the record and the entry run

`PHASE_STATUS.md` `CURRENT_STEP` 0 to 3 and `WORK_INSTRUCTION` 379 to 380, both stale.
Version 1.13.66 to 1.13.67 with its line in the version log. The `UNIT 380 - STEP 3`
entry appended to `PHASE_OUTCOME.md`. **Step 8 already read `done` in both files**, so
there was nothing to transcribe; the step 6 and step 7 headers were checked and agree.

**The entry carry-forward, and the entry round is the one that broke.** App attempt 1,
2 m 15 s: 215 of 216, and the red was **not an assertion** -
`TheOliviaRowsTests.WithRowsPresentNothingIsComposedUntilAPressAndEachPressCarriesItsRowsVariant`
threw `System.IO.IOException` at `TheOliviaRowsTests.cs:675`, *the process cannot access
the file ... refuse\2026-09-21.jsonl because it is being used by another process*.
**That is a shape of environmental fault no prior unit has recorded** and it is section
4's item 4. App attempt 2, 2 m 20 s: 215 of 216 again, the file-lock red **not
reproduced**, and a different name lost at 1 ms to Avalonia's headless
`InvalidProgramException: You've caused dispatcher loop` in
`HeadlessUnitTestSession.EnsureApplication` -
`ThePsk31ConversationCardTests.NoSlotClockUnderPsk31AndFt8AndFt4StillShowIt`, **a ninth
name** that fault has landed on. That is unit 375's item 3, re-run once as directed,
recorded and not chased. **The union of the two attempts is 216 of 216 and not one name
failed an assertion.** Engine 150 of 150 green first time.

The eight types this unit can move: **71 of 71 green**, so any red met later was this
unit's. `ViewTestsActThroughControlsTests` 1 of 2, the inherited red named so it could
not be read as this unit's.

### Task 1 - the trace, which built nothing and decided the budget

One `[Fact]` and one `[AvaloniaFact]`, run by name. Nothing was built, no source file
changed, nothing repaired and nothing asserted about the product.

- **The four-signal fixture.** With the CQ filter off and with it **on**, the three
  lists read identically: table 4, visible 4, mine 0, shown 4, mineCount 0, hidden 0.
  Each run wrote **52 telemetry lines** - `psk31_line_parsed` 10, `psk31_squelch` 7,
  `psk31_reading` 8, four carriers appearing and four retiring, and seven lines not
  about the path at all. **Not one of the 52 names a list, a gate or a fate.**
- **A gate that does fire.** Six FT8-shaped rows with the CQ filter on: table 6,
  visible 2, mine 1, hidden 3 - **three rows on no list at all**, and nothing in the
  file naming them or saying why.
- **The row rate, counted.** 112 rows placed over 8 slots through the real path at the
  shack's own 14-a-slot reading of 2026-09-04, 240 slots an hour: **3,360 rows an
  hour**.
- **The byte length, weighed on the file.** Envelope alone 142 bytes, a full slotted
  candidate **267**, a lean text candidate 223. **One event per row is 876.1 kB an
  hour, 17.5 times criterion 3.4's 50 kB**, so the literal reading of 3.1 is not
  available on a busy band. 50 kB buys 191 lines an hour, one every 18.8 s, **0.80 of
  a line per slot - less than one, so the window cannot be the slot.** A 120-second
  window gives 6.4 lines. **That is where the sampling number came from.**
- **The cards, the fold and the scroller.** One card appeared and one was dismissed
  and the record said nothing about either. `panel_toggled` carried the key and
  open/shut and not the 5 rows and 1 card behind each fold. The scroller read extent
  720, viewport 141, first index 0 and last 7 at rest and first 11 and last 18 at
  offset 200, with all 40 containers realized - **the index range is readable without
  new view code**, so ruling 1 item 3 stands.
- **The privacy walk before:** 81 writers against a guard reading 81.

### Task 2 - the writer, the sites, and the privacy walk

`AppEvents.OnScreen`: **one** method, **one** event name, `on_screen`, carrying `kind`,
`state`, `by`, `where`, `count` and the sub-mode. The category is chosen **inside the
writer** from the item's own `IsTextOnly` - `Psk31` for a text row, `Decode` for a
slotted one - and **no new telemetry category** was added. `OnScreenKind`,
`OnScreenState`, `OnScreenBy` and `OnScreenViewport` are closed types rather than
strings typed at call sites. A fact Hamlet does not have is **absent** and not zero.

**The writes go where the decision is.** `OnDigitalDecodesChanged` and
`ApplyDecodedFilter` are the two places a row is put on a list or kept off one, and
both arrival paths - `PlaceRow` for a slotted row, the text-row builder for a PSK31 or
Olivia one - run through them. `TrimDigitalDecodes` writes `removed`, `Reconcile`
writes a card appearing and a card dismissed, `PersistPanel` writes what went behind a
fold **beside `panel_toggled` and never instead of it**.

**The last state is remembered off the row**, in maps the view model owns and nothing
draws from - **two maps and not one**, because a text row has no object identity to
keep: the builder replaces a PSK31 row with a new record four times a second, so kept
by object the record would write `drawn` for every carrier on every tick, which is the
second copy of `psk31_line_parsed` this step exists not to write.

**Criterion 3.5 in the same commit as the writer.** `ExpectedEventMethodCount` 81 to
82 and the walk grown with it over all five states, both kinds, both categories and the
scroll-settle shape, so **no commit tip in this unit leaves a carry-forward name red**.

**3.1 and 3.2 asserted off the file the run wrote**, never off a view-model property:
`TheRecordSaysWhatWasOnScreenTests` 6 of 6.

### Task 3 - the diagnosis, the size of an hour, and the drop candidate taken

`TheRecordDiagnosesTheEveningTests` 6 of 6. The four-signal fixture with the CQ filter
on, read out of the file; the case where a gate does fire asserted beside it; the cost
measured end to end at **12.5 kB an hour**; the sampling shape asserted (20 rows in 3
lines against 600 rows in 4); **both** panels' scroll settles driven through the real
window, the real `ScrollChanged` and the real quarter-second settle; and unit 354's
nine sizes re-measured.

**One decision this session made for itself, reproduced in full.** *The `count` field
counts rows and not changes.* Section 6 ruling 1 says `count` is *1 for an item event
and n where one line stands for n items*, and the first implementation tallied changes.
Measured, that is wrong by a factor of a hundred: a live PSK31 row is rebuilt four times
a second and its reading re-parsed each time, which moves it between the operator's own
side and the left list and back - **521 state changes across four carriers over 38
seconds of the four-signal fixture**. A tally would have told a reader that 521 rows were
on the screen when four were, which is exactly the §0.0 fault this step exists to
prevent. The window therefore keeps a **set of identities** and writes its size. The
identity is named to the channel at the text-row builder, because the obvious key - the
first-heard `HHmmss` cell - **collides**: the four-signal fixture's four carriers all
appear in the same second.

### Task 4 - the exit run

Three guard names added to `docs\carry-forward-tests.txt` by type and method with their
paragraphs, and the human-readable list changed to match. **Exit carry-forward: app 219
of 219 in 2 m 24 s and engine 150 of 150 in 4 m 55 s, both green on the first attempt.**
The eight types re-run in the same filtered shape: 71 of 71, identical.
`ViewTestsActThroughControlsTests` 1 of 2, the same inherited red with the same message
character for character.

**Nothing on a send path was touched** - no composer, no `Arm`, no `PttOn`, no cap, no
RSID burst, no variant gate - and **no file under `src\Hamlet.RadioEngine\` was opened
except to read it**.

## 2. What the owner should expect

When a night looks empty, you can send back the file and it will say why. For every line
Hamlet decoded and every card it raised, the record now says what became of it on your
screen - whether it was drawn, whether a filter you had left on held it off the list and
which filter, whether you had scrolled past it, whether the panel holding it was folded
shut, or whether it aged off the table when the list filled. **That means the empty-list
question that needed a screenshot on 12 September is now answerable from the file alone**
- and the answer it gives is worth knowing: on the four-signal test, with the CQ button
**on**, the record says every one of the four stations was drawn and nothing at all was
hidden. The CQ button has not touched a PSK31 or Olivia row since that fault was
repaired, so if your keyboard-mode list looks empty tonight, the band is empty, and the
file now says so instead of saying nothing. **It costs 12.5 kB an hour to know**, against
a ceiling of 50, measured on a band as busy as the busiest evening this project has ever
recorded.

**What will look wrong but is not.** Nothing on your screen changed - no control, no
label, no layout, no colour; the panels are the same size to the pixel at all nine window
sizes. The file will have a new event in it called `on_screen`, and there will be **far
fewer of them than there are rows**: one line stands for a whole two-minute window of
rows in the same state, carrying how many there were and where the first of them sat.
That is deliberate, and it is the only way both *every change is accounted for* and *it
fits in 50 kB an hour* can be true at once. Nothing personal is in any of those lines -
an offset, a slot, a dial, a kind, a count and a reason, and never a callsign, a grid or a
word of what anybody typed.

Every claim above was **computed on the development machine, not seen** (FACT-004): **no
port was opened, no device was enumerated and nothing was keyed.**

## 3. What you should see

### The unit 337 diagnosis, read out of the file

The four-signal PSK31 fixture, carriers at 700, 1100, 1600 and 2200 Hz, driven through
the real tap and tick **with the CQ filter on**.

**Before tonight**, the whole record of that run was 52 lines, of which the ones about
the path were `psk31_line_parsed` 10, `psk31_squelch` 7, `psk31_reading` 8, four carriers
appearing and four retiring. **Not one of them named a list, a gate or a fate.** Per row,
what the record said became of it: *nothing*, four times over. That is the gap that cost a
screenshot in September.

**After tonight**, the same run writes two more lines and they answer it:

```
{"kind":"row","state":"drawn","by":"","count":4,"subMode":"PSK31","offsetHz":700}
{"kind":"row","state":"drawn","by":"addressed_to_operator","count":1,"subMode":"PSK31","offsetHz":1100}
```

Per row: **700 Hz drawn, 1600 Hz drawn, 2200 Hz drawn, 1100 Hz drawn and then moved to
the operator's own side when it answered him. No row filtered, by any gate.** In his own
words: *every station was drawn and nothing was hidden - if the list looks empty tonight
the band is empty, because the CQ button does not touch a PSK31 or an Olivia row.*

**And a gate that does fire is named, because a diagnosis that can only say *nothing was
hidden* has not been tested.** Six FT8-shaped rows with the CQ toggle on: the record says
`filtered` `by: cq_filter` `count: 3`, `drawn` `by: ""` `count: 2`, and `drawn`
`by: addressed_to_operator` `count: 1`.

### The nine states, and the site each is written at

| Kind | State | Written at | By |
|---|---|---|---|
| row | `drawn` | `OnDigitalDecodesChanged`, `ApplyDecodedFilter` | `""` or `addressed_to_operator` |
| row | `filtered` | `OnDigitalDecodesChanged`, `ApplyDecodedFilter` | `cq_filter` |
| row | `scrolled_out` | the decoded panel's `ScrollChanged` settle | `digital.decoded` |
| row | `folded` | `PersistPanel`, beside `panel_toggled` | `digital.decoded`, `digital.mine` |
| row | `removed` | `TrimDigitalDecodes` | `trim` |
| card | `drawn` | `Reconcile` | `""` |
| card | `scrolled_out` | the For You panel's `ScrollChanged` settle | `digital.mine` |
| card | `folded` | `PersistPanel`, beside `panel_toggled` | `digital.mine` |
| card | `removed` | `Reconcile` | `dismissed` |

**Five of five row states and four of four card states, from none of either.**

### The volume arithmetic

| | Measured |
|---|---|
| Rows an hour | **3,360** - 112 rows placed over 8 slots, 240 slots an hour |
| Bytes a line | **267** full, 223 lean, of which **142** is the schema-B envelope |
| One event per row | **876.1 kB an hour - 17.5 times the budget** |
| What 50 kB buys | 191 lines an hour, one every 18.8 s, **0.80 a slot** |
| Window chosen | **120 s**, one line per state-and-gate group, count carrying the rest |
| **Measured cost of a busy hour** | **12.5 kB** - 16 lines and 4,283 bytes over 1,200 s of band |
| Rows accounted for in that run | **1,120 of 1,120, exactly** |

Thirty times the traffic writes one more line, and that line is a group the thin run
never reaches.

### The privacy walk

**81 writers before, 82 after**, with `CallsignPrivacyTests` green at 4 of 4 and its walk
grown in the same commit as the writer, over all five states, both kinds, both categories
and the scroll-settle shape. Beside it, this unit's own scan proves what the walk cannot:
the walk calls every writer by hand and shows it has nowhere to put a callsign; the scan
drives **real rows and real cards through the real collections** and shows the **call
sites** did not hand it one, checking 13 forbidden strings - the operator's callsign, his
grid, every station in the fixture and several words of the decoded text - over the
serialised JSON of every `on_screen` line.

### Unit 354's nine sizes - the screen did not move

| Size | Panel row | Unit 376 left it at |
|---|---|---|
| 1920 x 1040 | 483 | 483 |
| 900 x 620 | 71 | 71 |
| 1100 x 780 | 92 | 92 |
| 1280 x 720 | 163 | 163 |
| 1366 x 728 | 171 | 171 |
| 1536 x 824 | 267 | 267 |
| 1400 x 1040 | 483 | 483 |
| 1920 x 1017 | 460 | 460 |
| 2560 x 1400 | 860 | 860 |

Identical at every one, with `TheStopIsAlwaysOnScreenTests`, `TheTopRowTests`,
`TheWorkingPanelsTests` and `BindingHealthTests` green. The whole view-side change is two
names on `ScrollViewer`s that already existed and two handlers that draw nothing.

### The counts, and the fate of every invocation

| Run | Task 0 | Task 4 |
|---|---|---|
| App carry-forward | **216 of 216** (union of two attempts) | **219 of 219** |
| Engine carry-forward | **150 of 150** | **150 of 150** |
| The eight movable types | 71 of 71 | 71 of 71 |
| `ViewTestsActThroughControlsTests` | 1 of 2 | 1 of 2 |

The only differences are this unit's own three new guard names. **No regression.**

**The fate of all four carry-forward invocations**, which is the only 2.4 evidence
tonight produces:

| Invocation | Fate |
|---|---|
| Task 0 app, attempt 1 | **completed red** - 215 of 216, a file-lock `IOException`, not an assertion |
| Task 0 app, attempt 2 | **completed red** - 215 of 216, the headless dispatcher loop at 1 ms, a ninth name |
| Task 0 engine | **completed green** - 150 of 150, first attempt |
| Task 4 app | **completed green** - 219 of 219, first attempt |
| Task 4 engine | **completed green** - 150 of 150, first attempt |

**The exit round is one completed green round of both invocations.** The engine
invocation has still never failed since unit 375.

## 4. What's blocking us

**Nothing is blocking. Nine items, most-blocking first; every one is a finding or a
disclosure and none asks for a ruling.** No item here is in the way of a criterion this
unit met.

**1. `AddDecodeRowForTests` is half a door, and no test in this repository could reach
the row cap through it. A finding.** Its own remarks say it is *the same door the decoder
uses*; it calls `PlaceRow`, and the decoder's door is `AddDecodeRow`, which **also** keys
the duplicate set and runs `TrimDigitalDecodes`. This unit's trim guard wrote nothing
until that was found. It was **not repaired in place** - four or more test files call that
hook and making it key the duplicate set would change what a repeated message does in
every one of them, which is §12.6's rule and unit 297's own precedent for exactly this
shape. A second hook, `AddDecodeRowThroughTheCapForTests`, was added beside it and named
for what it adds. **The queue item is whether the original hook's remarks should be
corrected, which is the next unit that touches that file's to do.**

**2. Section 5 says *the squelch is the only gate on a text row*, and the measurement
disagrees. A finding, and the instruction asked for it.** Over
`psk31-snr-10db-1000hz.wav` the squelch wrote 27 lines on one carrier - 13 shut and 14
open - and the row stayed on a list throughout, carrying 221 characters. **The squelch is
not a gate on a text row's visibility at all**: it gates what the row *says*, and a
carrier Hamlet cannot read still gets a row, carrying the heard-not-readable words instead
of text. The consequence is that **a text row has no visibility gate whatever** except
which of the two sides it is on, which is what makes 3.3's answer *nothing was hidden*.

**3. Section 5's list of what the record says today omits
`AppEvents.DecodesReachedTheScreen`, at `MainWindowViewModel.cs:13583`. A finding.** That
writer has carried a per-slot count of rows added, rows shown, rows for the operator and
cards since unit 305. **R36's gap is real but narrower than the instruction states**:
`decodes_drawn` can say four rows were drawn and three were not; it can never say which,
where, or by what, and it is per slot and FT8 only. Nothing was repaired on account of
it - the writer is untouched and `on_screen` sits beside it - but a reader of section 5
would have thought the file said nothing at all about the screen, and it did.

**4. A new shape of environmental test fault, and it is 2.4 evidence. A finding.** At
task 0, app attempt 1,
`TheOliviaRowsTests.WithRowsPresentNothingIsComposedUntilAPressAndEachPressCarriesItsRowsVariant`
failed with `System.IO.IOException` at `TheOliviaRowsTests.cs:675`: *The process cannot
access the file 'C:\Users\TimDi\AppData\Local\Temp\hamlet-olivia-rows-...\refuse\2026-09-21.jsonl'
because it is being used by another process.* It is **not an assertion** and it is **not
the dispatcher loop** - it is the test's own reader racing the telemetry writer's
background thread. It did not reproduce on the re-run. **Every environmental break this
phase has recorded until tonight was the dispatcher loop; this is a second, different
one**, and 2.4's soak will keep meeting it. Queue.

**5. This unit's carry-forward line edit landed one commit late. A disclosure, already
acted on.** Section 11 says a carry-forward line edit goes in the same commit as the test
it names; this unit's three guards landed in tasks 2 and 3 and the line edit in task 4.
**That is unit 379's item 4 repeated**, which section 2 of this instruction carried as a
lesson rather than a queue item, so it is disclosed here plainly rather than quietly. No
commit tip left a carry-forward name red at any point.

**6. Task 1's item 6 ran as `[AvaloniaFact]` and not `[Fact]`. A disclosure.** The
instruction says *one `[Fact]` named `Unit380Trace`*. Item 6 is about a `ScrollViewer`,
which only exists inside a built window, and a plain `[Fact]` cannot build one. The other
six items are the `[Fact]` the instruction asks for, and item 6 is a second method in the
same file run in the same filtered invocation. Splitting it also kept the six items that
need no window clear of the headless dispatcher fault.

**7. A slotted row's `where` carries its tone offset beside its slot and dial. A
disclosure, and it widens the author's own ruling rather than narrowing it.** Section 6
ruling 1 says *the row's offset in Hz for a text row and its slot and dial for a slotted
row*. A slot and a dial name **fourteen rows at once** on a band as busy as the one this
unit measured, and criterion 3.3 asks which row - so the tone offset goes on too. It adds
a field and removes none, at 14 bytes a line inside a budget measured at a quarter of its
ceiling. Overrulable in one line.

**8. `validate-output.bat` refused again, in the exact shape section 2 predicted. Unit
379's item 7, carried, and still a finding about the harness.** The command run,
verbatim and in the shape the instruction specifies - forward slashes, a leading `./`,
one command, no `cd` in front and no `cmd /c` around it:

```
./tools/arbiter/validate-output.bat output.md
```

The refusal, verbatim: `This command requires approval`. That is the permission mode and
not the syntax, and a non-interactive session cannot answer it. **So the six rules were
hand-checked against the script's own source, and this is a hand-check and not a
validator run:**

| Rule | Held by the script | Hand-checked |
|---|---|---|
| 1 | a `UNIT:` line above section 1, parseable | **ok** - line 24, above section 1 at line 42, inside the 60 lines the script reads, no BOM |
| 2 | the four top-level sections, in order, exact names | **ok** - lines 42, 173, 202, 317, matching the script's `WANT` string exactly |
| 3 | no fifth top-level section | **ok** - four `## ` headings and no more; `###` is ignored by the script's own reading |
| 4 | section 4 present even when empty | **ok** - `## 4. What's blocking us` at line 317 |
| 5 | section 3 non-empty | **ok** - 114 lines between the section 3 and section 4 headings |
| 6 | the ordering block above `UNIT:`, A, B, C, and C naming a count | **ok** - `READ IN THIS ORDER.` at line 2, `A.` at 4, `B.` at 8, `C.` at 17, all inside the first 60 lines, and line 17 reads *Section 4 raises 9 items*, which is the `raises \d+ item` the script matches |

**A hand-check is one check, not two**, and CPS-DEC-066's whole point is that the script
and the standard are two independent copies that must agree. Tonight only one of them
was read. Queue.

**9. The `RULES_AT` id-scheme split, carried and not repaired.** `PROJECT_STATUS.md` reads
`HM-DEC-165 (2026-09-19)` and `CLAUDE.md` §1 holds `CPS-DEC-0165`. `tools/status.sh`
writes that field as a literal and `tools\` is not this unit's to edit. Reported, not
repaired, as instructed.

### The carried queue, verbatim per HM-DEC-139 - twenty-five, and this unit answers none

- **Unit 379's item 1** - the CQ list cannot tell an Olivia channel with no readable
  variant from a PSK31 row and labels it `PSK31`. Strictly narrower than what it replaced
  and inside step 8, which is `done`; closing it means putting a mode fact on
  `DigitalDecodeRow` at the text-row builder, which is the file this unit wrote in, and a
  visibility unit that quietly reopened a closed step's criterion would be drift. Reported,
  not repaired.
- **Unit 379's item 3** - the unearned Olivia card spells the band `80m` where PSK31's
  spells `80 m`. A finding about a data file inside a closed step.
- **Unit 379's item 7** - `validate-output.bat` refuses with `This command requires
  approval`.
- **Unit 378's items 1, 3 and 4.**
- **Unit 377's item 4.**
- **Unit 376's items 3, 4 and 5.**
- **Unit 375's items 3 and 4** - item 3 is the headless dispatcher loop, which this unit
  met once more on a ninth name.
- **Unit 374's item 3.**
- **Unit 373's item 2.**
- **Unit 372's items 4 and 7.**
- **Unit 371's five.**
- **Unit 369's four.**

**Unit 379's items 4 and 5 came off the queue** - both were disclosures already acted on -
**and its item 6, the queue count, is answered by this count: twenty-five.**

### The two inherited reds, reported and repaired neither

- `ViewTestsActThroughControlsTests.NoViewTestWritesAPropertyAControlOwns`, red for the
  sixth unit running, naming `TheStopIsAlwaysOnScreenTests.cs:102 writes OperatingMode`.
  Unit 378 ruled it belongs to whichever unit next touches that file. **No task of this
  unit edited it**, so it is reported and not repaired. Its message is character for
  character what it was at task 0.
- `TheOliviaMoveUpTests.ItIsNotOfferedOnAGuessedYourTurn`. Reported, not repaired.
