READ IN THIS ORDER

A. THE PHASE GOAL — **Hamlet works stations on the air**: Tim answers a CQ on
14.074 or 7.074 from Hamlet and completes an exchange. Where every step stands
after tonight: **0, A, B and C were `done` before this unit began**, closed by
units 266, 267 and 268 — the record made honest, the row that knows where a
contact stands, right-click-and-it-goes, and the whole chain from one click at
the bench, where unit 268 decoded `"W1ABC KC3QIS RRR"` back off a real sound
card. **D is `blocked`** on Tim at his own radio; this unit did not touch it and
only he can move it. **E is `in progress`, which is the most it can honestly be.**
**All three of step E's exit criteria are Tim's evening and this unit closed
none of them** — it cleared a bench blocker sitting under one of them and claims
nothing more.

B. THIS STEP AND ITS EXIT CRITERIA — **step E, *Tim works a station*.** Three
criteria. **1: he answers a CQ on 14.074 or 7.074 and completes an exchange** —
only he can meet it, and nothing moved under it tonight. **2: the transmitted
slots appear in telemetry and the row reads complete** — only he can meet it, and
the criterion did not move, but this is the whole subject of the unit. **The
telemetry half was already proved**: unit 264's committed walk finds both
transmitted slots on disk by `slotStartUtc` through the application's own writer
and its own category predicate. **The "the row reads complete" half had the
hole**: it was proved for one ending only, the exchange that finishes with a
message *heard*, because that places a new row after both sends. Where the
operator's own message is what completes the exchange and the station then goes
quiet, no row is ever placed after it — so the newest row on the table still read
*your move* and nothing on the screen ever said the contact finished. That hole
is measured and closed. **3: what he saw and anything that surprised him,
recorded** — only he can meet it, nothing moved under it, and `SHACK_FACTS.md`
was not opened. **Criteria 1 and 3 were untouched.**

C. WHAT THIS REPORT ADDS, AND WHETHER IT BEARS ON A OR B — **the hole was there,
and it is closed.** Section 4 **raises 4 items**, **none of which is in the way of
a criterion in B and none of which asks the owner to decide anything**: two
instruction-versus-tree mismatches reported and not repaired, a shell refusal
already named in the plan with its alternative already taken, three reload
disagreements the instruction says to report rather than fix, and one committed
test this unit was not licensed to run. **Task 3 did not take its fallback** —
task 1's third question found `Ft8MessageSplit.Split` and `IsCallToAnyone` public
and already used from the app, so the line names the station off the `To` field
of what actually went out, by asking the same two methods the ledger asks. The
one thing that bears on B is B's own criterion 2, and it bears on it as a blocker
cleared and not as a criterion met.

UNIT:       270 — complete at task 5 of 5 — 2026-09-07 12:41
PHASE GOAL: Hamlet works stations on the air — Tim answers a CQ on 14.074 or 7.074 from Hamlet and completes an exchange, at his own licensed station on an antenna.
UNIT GOAL:  After the operator's own last transmission, the Send area under the waterfall says where that contact stands — the station named, the state in the same four words the rows use, the slot count, and the slot it was read at — computed from the one ledger the rows already read, with no row already on the table restating itself.
ADVANCED:   no — no criterion of step E closed, and none could be, because all three are Tim's at his own radio. What moved: the bench blocker under criterion 2, measured and then closed — the ledger reaching complete on his own last message with nothing on screen saying so.
NUMBER:     never -> 0 slots — how many slots pass, after his own last message completes a contact, before anything on Hamlet's screen says so. Before, with the station gone quiet, the honest answer is never: measured tonight as the ledger reading "complete, 0 slots" at 16:37:15 while both rows on the table read "your move, 0 slots" and neither Send area line carried any of the four state words. After: the same slot it happened in.
DRIFT:      2 consecutive units without advance  (was 1) — and both are bench halves of criteria that are Tim's: 269 built the bench half of step D criterion 1, 270 the bench half of step E criterion 2.

## 1. What Claude did

**Exit state: complete, at task 5 of 5.** All five tasks were done, each committed
and pushed before the next began. Nothing was left undone and the named drop
candidate — task 5, the page for his evening — was not dropped.

Provenance: the development machine, `C:\Source\HamLet`, branch `main`. The gate
was checked against the tree before the instruction was read: `SHACK_FACTS.md`
and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` both present,
`CoreHMI.sln` and `MURC.sln` both absent. **Hamlet confirmed.**

### Task 1 — the trace, in its own commit (`7fdc88b`)

`docs/unit270-the-last-message-trace.md`, six questions, each answered with a
file, a line and a quotation.

1. **Nothing recomputes the `Contact` cell of a row already on the table.**
   `PlaceRow` at `MainWindowViewModel.cs:7826`, the assignment at `:7840`, and
   exactly two callers — `:7787` the decoder's door and `:7872`
   `AddDecodeRowForTests`. **No third.**
2. **`RecordSent` at `:8474` reaches nothing on screen.** It returns `void` into a
   private ledger. Five surfaces can see the ledger; three do not move at all, and
   the two that do need either another decode to arrive or the operator to
   right-click. **If the station has gone quiet, neither happens.**
3. **The addressee is reachable.** `Ft8MessageSplit.Split` at `:53` and
   `IsCallToAnyone` at `:86` are public in the namespace the view model already
   uses, and the app forwards to both from `Ft8Vocabulary.cs:57` and
   `DecodedFilter.cs:86`. `Ft8MessageFields` is `(To, From, Payload)` — `To`
   first. `Split("W1ABC KC3QIS RRR")` gives `To=W1ABC`;
   `Split("CQ KC3QIS FN00")` gives `To=CQ` with `IsCallToAnyone` true.
4. **The moment is `result.Send!.SlotStartUtc`** — the slot the transmission went
   out in. It is the value `:8474` books the send at, the convention
   `ContactTextFor` uses at `:7931`, and a real slot boundary, which the wall
   clock at the post is not. The count means how many fifteen-second slots lie
   between the last message that carried the state and the moment read at.
5. **The telemetry half is proved twice; the row half for one ending only.**
6. **The fourth line goes last in the Send area's `StackPanel`**, after
   `DigitalTransmitLevelText` at `:3257-3261` and before the close at `:3262`.
   `DigitalStopButton` is child one at `:3119` and cannot be moved by a child
   appended after it.

Root version read, not assumed: `Directory.Build.props:205` was `1.12.123`, bumped
to `1.12.124`. `Ft8Sharp` did not move.

### Task 2 — the measurement, before anything was built (`3bbf475`)

**Added to the committed walk file, not copied.** `Panel()`, `Heard`,
`ClickAsync` and `WaitForSlotAsync` are reused exactly as they stand and **no
existing method in that file changed a line**; the one other edit is an added
`using`. **Why that and not a copy:** a second copy of a harness is a second thing
to drift. It is an `[AvaloniaFact]` where the walk is a `[Fact]` because it quotes
the two Send-area lines, which are set inside a `Dispatcher.UIThread.Post` — with
no dispatcher running, they would be quoted as something the operator never sees.

**The hole reproduced.** Figures in section 3.

**This is not a red to be fixed**, and the file says so: the row is a record of
its own slot and stays one. The breakage written into it is **a future unit
"fixing" the stale cell by rewriting rows already on the table**, which makes a
table of moments lie about its own moments.

### Task 3 — the line, watched red first (`2a67260` red, `cf0630c` green)

**The red was committed before it was made green.** Four tests, each run alone by
exact name and foregrounded, all four failing with the same sentence: *there is no
TextBlock called "DigitalContactStandsText" on the realized window.*

Then the product: `DigitalContactStandsLine`, an `[ObservableProperty]` following
the shape of `_digitalSendLine`; `ContactStandsLine(message, slotUtc)`, which asks
`Ft8MessageSplit` who the message was addressed to and `Ft8ContactStates.Read` for
the state and the count, and works nothing out itself; set in the **same**
`Dispatcher.UIThread.Post` unit 269's readout is set in; and a `TextBlock` bound to
it as the **last** child of the Send area's stack.

**The fallback was not taken.** No second splitter, no second copy of the
completeness rule or of the four state words, no invented callsign.

**Nothing in the keying path was touched.** `Ft8TransmitSequence.RunAsync`, the
key, the sink call, the `finally`, the abort and the stop are byte for byte what
they were. `Ft8ContactLedger` and `Ft8ContactState` are untouched — this unit is a
reader of them.

### Task 4 — the phase's bookkeeping (`c530236`)

`outcome-append.bat` was refused in both spellings. The unit 270 entry was
appended by hand with the file-editing tools, twelve fields plus an `APPENDED_BY:`
line that says on its face that a script did not write it — matching
`outcome-entry.py`'s own `FIELDS` list at `:115-118`, which holds twelve. **The
header's `STEP: E` line moved from `not started` to `in progress` and to nothing
else.** `STEP: D` was not touched and still reads `blocked`. In `PHASE_STATUS.md`
only `WORK_INSTRUCTION:` was written.

### Task 5 — the page for his evening (`cdf5d60`)

`docs/unit269-what-step-e-asks.md` **corrected in place, not rewritten**. Its
advice to *"read the row that arrives after his last transmission rather than the
one he clicked"* is replaced by where he now reads it, what the line says when the
station has gone quiet, and a new table saying which of step E's three criteria is
still entirely his: **all three.** `SHACK_FACTS.md` was not touched.

### Decisions this session made for itself, reproduced in full

1. **The present state goes into a line about the present and never into the cells
   of rows already placed.** `ContactTextFor`'s own contract at `:7885-7888` is
   that a row shows where the contact stood in its own slot and never restates
   itself. A table of moments that edits its own moments is a worse instrument
   than an incomplete one. The row-rewrite route was named in the instruction,
   parked, and the measurement proving the cells do not move is committed as a
   permanent guard.
2. **The line is required to say which slot it was read at**, because a state with
   no moment is the same class of fault as unit 269's readout showing the operator
   his own setting back and calling it a measurement. That is why a later decode
   leaving the line where it stands is honest rather than stale, and why should-pass
   6 is answered *it does not move, and here is the wording that covers it* rather
   than by adding a second writer on the decode path.
3. **Task 2's measurement joined the committed walk file; task 3's four tests are
   a new file.** The first because the harness it needs is already there and
   nothing had to be copied. The second because they read the line off the realized
   `TextBlock` where the operator would read it and one of them asserts where that
   control sits, and the walk's file builds no window at all.

## 2. What the owner should expect

**One new line in the Send area under the waterfall**, at the bottom, below the
Transmit drive control and below the measured-level readout. It appears after a
transmission has gone out and says where the contact that message was addressed to
now stands.

**What it says when a contact completes on your own message and the other station
has gone.** After your `RRR` finishes the exchange, it reads — this is the exact
sentence off a run at the bench tonight:

> Where the contact with W1ABC stands: complete, 0 slots, read at the 16:35:15
> UTC slot. That is what passed between you, counted in slots; it is not advice
> about what to send next, and nothing is closed or withheld by it.

If the station never comes back, that sentence stands. **It does not decay into
something else and it does not pretend to be live** — it names the slot it was read
at, so it is a record of a moment rather than a claim about now. Before tonight,
with the station gone quiet, nothing in Hamlet ever said the contact had finished.

**What will look wrong but is not.** **The row you right-clicked still says what
it said.** After your last transmission the top of the decode table can still read
*your move* while the new line says *complete*. That is deliberate and it is not a
lag: each row records where the contact stood **in its own slot**, which is what
lets you watch a contact progress down the table. The two are answering different
questions, and both are right. **The new line also does not move when a later
decode arrives** — it stays at the slot it was read at, and the row that decode
places carries the newer state in its own cell.

**Nothing was taken away, nothing was closed and no message was suggested to
you.** A completed contact still shows, is still clickable, and its menu still
offers exactly the five messages it offered before. Nothing is hidden, greyed,
filtered or forbidden because a contact reached a state, nothing transmits because
of one, and the line never tells you what to send next. A CQ books nobody, so
after the CQ button the line names no station and invents no contact.

**`SHACK_FACTS.md` was not touched, and step D is still yours.** No figure printed
tonight is advice about your drive level, and nothing here says anything about the
IC-7300 — no radio has ever been attached to this machine, no render endpoint was
opened, no serial port was opened and no sound was made.

## 3. What you should see

### 1. The exchange of task 2, slot by slot, as the application ran it

Driven through the application on `FakePort` and a substituted sink factory. One
run, `AfterHisOwnLastMessageTheLedgerIsCompleteAndTheNewestRowIsNot`:

| Slot | What happened | The row's `Contact` cell as placed |
|---|---|---|
| 16:36:30 | heard `CQ W1ABC EM12` | `your move, 0 slots` |
| 16:36:45 | **clicked** `W1ABC KC3QIS FN00`, off the menu | — no row; sends place none |
| 16:37:00 | heard `KC3QIS W1ABC R-09` — a report and a roger in one field | `your move, 0 slots` |
| 16:37:15 | **clicked** `W1ABC KC3QIS RRR` — **his own message completes it** | — |
| 16:37:30 | **nothing.** The station has gone. | — |

**What the ledger the application kept said**, at his own send slot:

```
IsComplete                       : True
Read at 16:37:15, his send slot  : "complete, 0 slots"
sent   : W1ABC KC3QIS FN00 | W1ABC KC3QIS RRR
heard  : CQ W1ABC EM12 | KC3QIS W1ABC R-09
```

**What the newest row's cell said:** `your move, 0 slots`, at 16:37:00, on
`KC3QIS W1ABC R-09`. Two rows for `W1ABC` and no more, because nothing was heard
after his last transmission.

**What the Send area said:** `DigitalSendLine` reported what went out and at what
composed level; `DigitalTransmitLevelLine` reported what the card was handed.
**Neither carried any of the four state words** — `waiting on him | your move |
complete | gone quiet`, read off the enum in the test rather than written out, so
a fifth or a renamed state could not slip past. **Nothing on the screen said the
contact had finished.**

### 2. The line after his last transmission, quoted exactly as it appears

From `AfterHisOwnLastMessageTheLineNamesTheStationAndReadsComplete`, read off the
realized `TextBlock` on a realized window:

> Where the contact with W1ABC stands: complete, 0 slots, read at the 16:35:15
> UTC slot. That is what passed between you, counted in slots; it is not advice
> about what to send next, and nothing is closed or withheld by it.

- **The station it names:** `W1ABC`, off the `To` field of what actually went out.
- **The state:** `complete`.
- **The count:** `0 slots`.
- **The moment it was read at:** the `16:35:15` UTC slot — the slot the
  transmission went out in.
- **Which route:** **the ledger route, not the fallback.** The expected text in the
  test is `Ft8ContactStates.Read(ContactRecordForTests("W1ABC"), slot).Text` read
  off the application's own ledger, so a second copy of the completeness rule or
  of the four words inside the view model could not have passed it.

**Where the control is:** inside `DigitalSendReserved`, asserted by visual
ancestry on a realized window, and in the same test `DigitalStopButton` is
visible, effectively enabled, and at y 766 on a 1400-high window — inside the
bounds. It is the **last** child of the area's stack, below
`DigitalTransmitLevelText`, so nothing before the Stop button moved.

### 3. What did not move

**The contact cells of the rows already on the table**, captured before his last
send and compared after it:

```
"KC3QIS W1ABC R-09"   before: "your move, 0 slots"   after: "your move, 0 slots"
"CQ W1ABC EM12"       before: "your move, 0 slots"   after: "your move, 0 slots"
```

**The sink and the port across a second boundary with nothing armed:**

```
outcome     : NothingArmed
sink calls  : 2 -> 2
port frames : 4 -> 4
the line    : unchanged
```

**One click, one message**, and the new reader of the ledger is not a second way
to reach the sink.

**The seven committed tests from the instruction's named exception**, each run
alone by exact name, foregrounded, with a five-minute timeout stated. **None had
to change:**

| Test | Result |
|---|---|
| `TheWholeContactWalksThroughTheApplicationTests.AWholeContactWalksThroughAndTheRowReadsComplete` | **Passed**, 20.5 s |
| `TheWholeContactWalksThroughTheApplicationTests.OneSendLeavesOneLineOnDiskAndTheLineNamesNobody` | **Passed**, 224 ms |
| `TheRowSaysWhereTheContactStandsTests.TheContactCellCarriesTheStateAndItsSlotCount` | **Passed**, 179 ms |
| `TheRowSaysWhereTheContactStandsTests.AStationNotHeardForFourSlotsReadsGoneQuietWithItsCount` | **Passed**, 156 ms |
| `TheRowSaysWhereTheContactStandsTests.WithNoOperatorCallsignTheRowSaysNothing` | **Passed**, 161 ms |
| `TheDriveIsSetWhereHeIsLookingTests.TheControlUnderTheWaterfallOpensShowingTheLevelInForce` | **Passed**, 772 ms |
| `TheReadoutSaysWhatTheCardWasHandedTests.AfterOneClickedSendTheReadoutIsWhatTheSinkReported` | **Passed**, 640 ms |

The slow one ran green first time and did not need a re-run.

### The five tests this unit built, red then green

| Test | Red | Green |
|---|---|---|
| `TheWholeContactWalksThroughTheApplicationTests.AfterHisOwnLastMessageTheLedgerIsCompleteAndTheNewestRowIsNot` | n/a — a measurement, green on the tree as it stood | **Passed**, 20 s, and **re-run after the green and still passing unchanged** |
| `TheContactStandsAfterHisLastTransmissionTests.AfterHisOwnLastMessageTheLineNamesTheStationAndReadsComplete` | **Failed** 18 s | **Passed** 24 s |
| `TheContactStandsAfterHisLastTransmissionTests.TheLineIsInTheSendAreaAndTheStopButtonIsStillUsable` | **Failed** 967 ms | **Passed** 900 ms |
| `TheContactStandsAfterHisLastTransmissionTests.ACallToAnyoneBooksNobodyAndTheLineNamesNoStation` | **Failed** 678 ms | **Passed** 711 ms |
| `TheContactStandsAfterHisLastTransmissionTests.ALaterDecodeLeavesTheLineWhereItWasAndTheNewRowCarriesTheState` | **Failed** 25 s | **Passed** 25 s |

**Both should-pass items got an answer.**

- **5 — a CQ books nobody: yes.** With `W1ABC` already in the ledger, the line
  after the CQ button reads *"That was a call to anyone, so it is addressed to no
  station and there is no one contact to report on yet."* — it names no station and
  claims no state.
- **6 — a later decode does not move the line, and the wording covers it.** After
  the exchange, `KC3QIS W1ABC 73` heard one slot later left the line byte for byte
  where it was, still naming the `16:35:45` slot it was read at, while the row that
  decode placed read `complete, 0 slots` in its own cell. The news is on the table;
  the line carries the moment his own transmission left.

**No suite was run, nothing was run unfiltered, and nothing was backgrounded.**
Every test invocation named one test by its fully qualified name, in the
foreground.

## 4. What's blocking us

**No ruling is wanted. Four items, none blocking a criterion in B, none asking the
owner to decide anything.**

### 1. Two places where work instruction 270 does not match the tree — reported, not repaired

- The instruction places the *"a row never restates itself"* remark on `PlaceRow`
  at `MainWindowViewModel.cs:7826` and says *"Its own remarks are the design this
  unit does not overturn"*. **The remark is on `ContactTextFor`'s doc comment at
  `:7885-7888`**, which is what `:7840` calls. The words and the design are exactly
  as quoted; only the owning member differs, and the design was followed.
- The instruction's list of `_contacts` sites names `:1129`, `:7901`/`:7927` and
  `:8474`. It does not name **`SendMenuFor`'s `_contacts.For(row.Sender)` at
  `:8296`**, a fourth reader of the ledger. It writes nothing and changes nothing,
  but the trace's question 2 had to account for it.

Every other line number the instruction gives was read off the tree tonight and is
correct. Nothing was repaired in the instruction.

### 2. `outcome-append.bat` refused again — the fourteenth consecutive unit

Both spellings were refused by this session's shell, verbatim:

```
This command requires approval        tools\arbiter\outcome-append.bat --help
This command requires approval        cmd //c "tools\arbiter\outcome-append.bat" --help
```

**The plan's named alternative was taken and nothing halted.** The entry was
appended with the file-editing tools in the format the existing entries use,
twelve fields matching `outcome-entry.py`'s own `FIELDS` list at `:115-118`, plus
an `APPENDED_BY:` line saying on its face that a script did not write it. **The
file-editing tools were unaffected throughout**, as thirteen units before this one
recorded. No ruling is wanted: the alternative is already in the plan.

### 3. The reload's disagreements — reported, not repaired, as the instruction directs

- **`PROJECT_STATUS.md` `RULES_AT` reads `HM-DEC-158 (2026-09-07)` while
  `CLAUDE.md` §1's newest table row is `HM-DEC-152` of 2026-08-31.** Six rulings
  apart, and **seven days as measured tonight**; unit 269 recorded the same gap as
  nine days, which does not match the two dates in the files. Not this unit's to
  fix and nothing tonight depended on the six.
- **`.commit-msg.txt` and `.oa-267.bat` are untracked at the root**, so a fresh
  clone does not have them. Left exactly as found. **`git add -A` was not used
  anywhere in this unit** — every path was added by name, which is how unit 269
  tracked these two by accident.
- **`.run-unit/watched.rc`, the third file the instruction names, is not on disk
  at all tonight.** Reported as measured rather than assumed present.

### 4. One committed test this unit was not licensed to run

**`BindingHealthTests` was not run.** It builds the real window headless and fails
on any unresolved binding, and this unit added a binding — but it is not one of the
seven the instruction's named exception licensed, and the instruction forbids
running anything outside them. **The new binding was proved another way instead**:
two realized-window tests read `DigitalContactStandsText`'s own `Text` back, once
showing the default sentence before anything was sent and once showing the
composed sentence after a send, which a null binding could not do. Naming it here
so that the next unit knows it was not run rather than assuming it was.
