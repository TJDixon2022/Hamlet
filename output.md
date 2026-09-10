# Work instruction 305 — the offset reaches the snapshot and not the app

**READ IN THIS ORDER.** The wire is found, mended and proved. **One thing in the
instruction did not survive contact with the tree and section 1 says so plainly** —
the wire explains the screen, and the record could not say whether it explains the
missing slots, because nothing in it said which mode the application was on. Task 4
closes that gap rather than guessing at it.

A. **The phase goal — FT4 works exactly the way FT8 does.** No step of it moved. But
   **the clock wire is what stood between a measured offset and a slot boundary**,
   and no FT4 or FT8 decode happens without one.

B. **Step 4 and its exit criteria** — pressing FT4 tunes and decodes; the panel, the
   conversation, the ring, the filters, the tooltips, the ledger and the right-click
   menu working unchanged; one click, one transmission; a whole exchange from one
   right click at the bench. **None was measured tonight.** Step 4 stays `partial`.

C. **The report last, and section 4 raises 2 items** on top of a queue of five you
   have not yet ruled on.

```
UNIT:       305 — complete at task 7 of 7, none dropped — 2026-09-10 15:31
PHASE GOAL: FT4 works exactly the way FT8 does.
UNIT GOAL:  The measured offset reaches every reader that needs it, and the record
            says an attempt was made before it says what became of it.
ADVANCED:   no — no phase step moved. Step 4 stays partial: nothing tuned, nothing
            transmitted, no exchange run at the bench.
NUMBER:     whether a slot is cut after the wire is mended
            BEFORE: 0 slots cut, and the cutter refuses in words.
            AFTER:  1 slot cut from the same audio and the same clock, and
            `CQ K1ABC FN42` decoded out of it.
DRIFT:      10 consecutive units without advance  (was 9, carried from unit 305a)
```

---

## 1. What Claude did

**Complete. Seven tasks of seven, none dropped.** Development machine, prompt claimed
`PROJECT: Hamlet`, and the tree confirmed it: `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` both present, `CoreHMI.sln` and
`MURC.sln` both absent. Branch `main`, seven commits, all pushed. Root version
1.12.268 to **1.12.269**, bumped once. **No file under `src/Ft8Sharp/` was touched.**

**Nothing in this report is evidence about the radio** (HM-DEC-093). No IC-7300 is on
this machine, and no port was opened and nothing was transmitted.

**Nothing was recorded to `DECISIONS.md`.** Five judgments were made inside the
order's licence and are named below so you can overturn any of them.

### Task 1 — the wire, found before it was mended

**Where it stopped: `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:913-920`**, the
`[NotifyPropertyChangedFor]` list on `_clockOffset`.

**How it was found: by comparing two readers rather than searching.** The instruction
is right that the snapshot has the offset and the screen does not, and the reason it
is findable that way is that **both readers are on the same object**. The settled
snapshot reads `ClockOffset` at line 6176; the sentence he read is
`DigitalCardsIdle` at line 1509, which asks `CardsNow`, which asks the same property.
The value was never lost.

**Stood up and measured** (unit 284's method), before the mend:

```
BEFORE THE OFFSET ARRIVES
  ClockOffset.IsKnown : False
  DigitalCardsIdle    : Hamlet has not been able to check the clock against a...

AFTER THE OFFSET ARRIVES
  ClockOffset.IsKnown : True
  DigitalCardsIdle    : Nothing addressed to you yet. Anything a station sends...

WHAT THE OBJECT TOLD THE SCREEN
  raised: ClockIsConcerning
  raised: ClockOffset
  raised: ClockOffsetLine
  raised: DigitalReadinessLine
  raised: DigitalWaterfallSummary
  raised: HasDigitalReadiness

WHAT IT DID NOT TELL THE SCREEN
  never raised: DigitalCardsIdle
```

**Five surfaces were told and the sixth was not**, so its binding kept the sentence
composed at startup, when the clock really was unknown. `DigitalCardsIdle` computes
the right answer the moment anything asks it. Nothing ever asked it again.

**MISMATCH AGAINST THE TREE, AND IT IS THE ONE THAT MATTERS.** The instruction's
table says the missing offset explains the absent `ft8_slot` events. **On this tree it
does not.** `OnSlotTick` reads `ClockOffset` live off the same property four times a
second (`MainWindowViewModel.cs:10023`), so from 19:01:05 the watch could cut. Two
explanations are consistent with the file as quoted, **and the record cannot choose
between them**: he was on CW, where no slot is cut and none should be; or the extract
spans about five seconds and an FT8 slot closes every fifteen, so it could not hold one
either way. **That the record cannot say is the fault, and task 4 fixes it** — the mode
is now in the snapshot and on every state change. **I did not repair the
instruction** (§12.6, and the order's own rule).

**Then mended, and a decode proved it:**

```
WITH NO MEASURED OFFSET
  slots cut : 0
  refusal   : the clock offset has not been measured, so where the slot
              boundaries fall is not known and nothing was cut
  decoded   : (nothing)

WITH THE MEASURED OFFSET
  slots cut : 1
  decoded   : CQ K1ABC FN42
```

**Judgment 1: the mend is a notification *and* a rebuild.** Cards are built only where
a corrected now exists, so every message that arrived before the clock answered built
no card at all. A notification alone would have left an empty panel truthfully
explaining itself.

**Judgment 2: the rebuild fires on the crossing, not on every measurement.** The clock
is asked again on a timer, and rebuilding the card list on each answer replaces the
control under his mouse (HM-DEC-078).

### Task 2 — an operator action is recorded

**Five presses now write one line each, before anything they trigger**: the CQ button,
the one send door, the stop, a card action, a card cleared. What was done and on which
mode; **never to whom and never what it said** (§2.1).

**Watched failing.** With the CQ write removed the test fails; restored, three pass.

### Task 3 — a pipeline stage records entry

**Eight stages write on entry.** Application side: composed, read back, armed,
boundary reached. Engine side, in `Ft8TransmitSequence`: gate asked, keyed, handed to
the sound card, unkeyed. **The existing transmission record is unchanged and gains
`stagesEntered`.** `Played` asserts exactly what it asserted before — that is ask 1
and yours.

### Task 4 — the FT8 decoder says it exists

`digital_decoder_started` names the mode, the slot length, the sample rate and the
device — **once per shape, not once per tick**, which would be 14,400 lines an hour.
**A slot the watch refused now writes its own `ft8_slot` line** with the reason
verbatim; before this the only route to that event was a slot already decoded, so a
session that cut none wrote nothing. The snapshot carries `appOperatingMode` and
`appDigitalMode`, and `state_changed` carries the mode. **Audio health reaches the
digital side**: the tap's own peak, floor and `nearlySilent` ride every slot line.

**Judgment 3: once per mode-and-rate for the announcement.**
**Judgment 4: one `decodes_drawn` line per slot**, which the order allows in words.

### Task 5 — what the screen drew

One event per slot: how many messages came out, how many became rows, how many the
filters left on the table he is reading, how many are addressed to him, how many cards
stand. **A slot that decoded and drew nothing is a warning**, because it is the case
worth finding by scanning (§8.1).

### Task 6 — the honest test

**Five for five, each constructed and run, with one limit stated.** Section 3 carries
them.

**Judgment 5: I reported the task 1 mismatch rather than repairing it**, which is the
order's own instruction.

**No test suite was run** (HM-DEC-155). Fourteen tests were constructed across five
classes and every run was filtered by exact name, foregrounded, with a 600 s timeout.
Every `dotnet build` was foregrounded. Nothing was backgrounded and polled.

---

## 2. What the owner should expect

**The clock line goes.** Once a time server answers, the cards panel stops saying the
clock has never been checked — it was saying that five seconds after it stopped being
true, and would have said it all evening.

**Messages arrive again, and his own CQ appears where he sent it** — *provided the
Digital tab is what he is on*. That last clause is not hedging: it is the thing the
old record could not tell you and the new one can. If the next file shows the mode as
Digital and still no slots, the refusal lines will say why in his own words.

**What will look wrong and is not.** The telemetry file is **noticeably larger**. A
press writes a line, each send stage writes a line, and every refusal to cut a slot
writes one. That is the whole point of the unit, and the cadence is bounded: the
decoder announces itself once per shape, a refusal is written when the refusal
*changes* rather than on every tick, and there is one drawn line per slot rather than
one per row.

**What I could not check, and it is a real risk.** This unit added `stagesEntered` to
the existing `ft8_transmission` record and two fields to the startup snapshot.
**Other tests read both of those events, and HM-DEC-155 forbids running their
suites**, so I cannot tell you whether one of them now fails on a field count or an
exact-shape assertion. Every test this unit constructed passes and every project
builds clean with no new warnings.

**Three inherited reds are unchanged and unnamed by any order** —
`TheAchievementsScreenTests`' two and
`TheFitGuardAsksAboutTheGridTheSendIsOnTests`' one.

---

## 3. What you should see

### 1. Where the offset stopped, with file and line, and how it was found

**`src/Hamlet.App/ViewModels/MainWindowViewModel.cs:913-920`** — the notify list on
`_clockOffset` named five properties and not `DigitalCardsIdle`.

**Found by comparing two readers on one object**, not by searching. The settled
snapshot reads the property at `:6176`; the sentence he read composes at `:1509` from
`CardsNow` at `:2686`, which reads the same property. So the only thing that can
differ between them is *when each is asked* — and the printout above shows the object
telling five surfaces and not the sixth. That is a missing notification and nothing
else.

**The arithmetic was never in question.** `Ft8Slots.TrueUtc` returns null only when
`OffsetSeconds` is null; with `0.033` it returns a corrected moment, measured.

### 2. A slot cut from a measured offset, and a decode from it

Same audio, same clock, the watch driven twice:

```
WITH NO MEASURED OFFSET
  slots cut : 0
  refusal   : the clock offset has not been measured, so where the slot
              boundaries fall is not known and nothing was cut
  decoded   : (nothing)

WITH THE MEASURED OFFSET
  slots cut : 1
  decoded   : CQ K1ABC FN42
```

And the arrival of the offset is now a line of its own:

```json
{"ts":"2026-09-10T19:15:47.366Z","level":"info","appVersion":"1.12.269","category":"diagnostics","event":"state_changed","data":{"what":"clock_offset","from":"unknown","to":"known","why":"a time server answered, so slots can be cut and cards counted","mode":"Digital"}}
```

### 3. Task 6's five cases

**1. A measured offset that reaches the snapshot and no other reader — today's own
fault. YES.** The crossing from unknown to known is its own line, with the mode beside
it. A file carrying `clock_query_finished` and **no** `state_changed` for
`clock_offset` says the measurement was taken and never delivered.

**2. A CQ press that produces no transmission. YES.** Quoted whole from a run on a
machine with no radio and no sound card:

```json
{"event":"operator_action","data":{"action":"cq_pressed","mode":"Digital","detail":"Ft8"}}
{"event":"operator_action","data":{"action":"send_requested","mode":"Digital","detail":"9 characters"}}
{"event":"send_stage","data":{"stage":"composed","entered":true,"detail":"Ft8"}}
{"event":"send_stage","data":{"stage":"read_back","entered":true,"detail":"Standard"}}
```

The last stage is `read_back` and nothing follows it, which reads as **composed and
never armed**. Through a fake port and sink the same path writes `gate_asked`,
`keyed`, `handed_to_the_sound_card`, `unkeyed`, in that order, and comes out `Played`.

**3. A slot that is never cut. YES.**

```json
{"event":"digital_decoder_started","data":{"mode":"Ft8","slotSeconds":15,"sampleRate":12000,"device":"unknown"}}
{"level":"warn","event":"ft8_slot","data":{"outcome":"refused","refusal":"the clock offset has not been measured, so where the slot boundaries fall is not known and nothing was cut","audioPeakDb":-90,"audioFloorDb":-90,"nearlySilent":true}}
```

A decoder start with no slot at all now means the tab was never looked at; refusals
mean it was looking and could not cut, and say why.

**4. A decode that reaches no panel. YES.** The same slot offered twice:

```json
{"level":"info","event":"decodes_drawn","data":{"outcome":"proceeded","reason":"drawn","decodes":2,"rowsAdded":2,"rowsOnTheTable":2,"rowsAddressedToTheOperator":0,"cards":0}}
{"level":"warn","event":"decodes_drawn","data":{"outcome":"degraded","reason":"decoded_but_no_row_added","decodes":2,"rowsAdded":0,"rowsOnTheTable":2,"rowsAddressedToTheOperator":0,"cards":0}}
```

And with the CQ filter on, one line separates three numbers that used to be one:
`decodes 2, rowsAdded 2, rowsOnTheTable 1`.

**5. Receive audio collapsing while FT8 is running. YES, with one honest limit.** The
tap's peak, floor and `nearlySilent` ride every FT8 slot line, so the collapse from
-12.9 dB to -67.9 dB would be caught on the digital side rather than only where the CW
decoder happened to be sampling. **The limit: a slot line is written when a slot is
cut or refused.** A tab nobody is on writes none, and audio health while the operator
is on Voice is still nowhere.

**Five for five, and the fifth is qualified rather than claimed clean.**

### 4. What the record still cannot answer

**Whether anything actually keyed the transmitter.** `unkeyed` says Hamlet wrote the
frame; it does not say the radio transmitted. That is ask 1, it is yours, and this
unit did not touch what `Played` asserts.

**Whether a send that stopped mid-path stopped for the reason the last stage
suggests.** The stages say where it got to. Why it stopped there is still the outcome
event's job, and a stage with no outcome after it is a question rather than an answer.

**Audio health anywhere but the digital slot path**, as case 5 says.

**And whether this unit broke a test it is forbidden to run.** Named in section 2.

---

## 4. What's blocking us

**Nothing blocks the next unit.** Two items want a ruling.

**1. Whether the next unit should run the tests this one could not.**

> A unit that changes the shape of an existing telemetry event runs the tests that
> read that event, filtered by exact name, before it reports.
>
> Why: HM-DEC-155 exists because three sessions died polling a suite, and it is right.
> But it leaves a real hole — this unit added a field to `ft8_transmission` and two to
> `startup_snapshot`, both of which other tests read, and **the rule that keeps the
> session alive is also the rule that stops it checking.** A named, filtered,
> foregrounded run of the handful of tests that read a changed event is not a suite
> and is not a poll.
>
> Rejected: running the whole project, which is the thing that killed three sessions;
> and leaving it as it is, which means every unit that touches a shared event ships a
> risk it names and cannot measure.

**2. Whether a fault should speak on the screen as well as in the file.**

> Where the application knows a stage of the send path was entered and none after it,
> it says so on the screen rather than only in the telemetry.
>
> Why: your standing rule is that a fault speaks unasked, on screen and in the file.
> This unit did the file half everywhere and the screen half nowhere, deliberately —
> the order's tasks are about the record. **But the file is the second line of defence
> and the screen is the first** (§8.1), and a send that stopped at `read_back` is
> something he could be told in a sentence at the moment it happens.
>
> Rejected: doing it inside this unit, which would have been scope nobody asked for on
> the one screen a wrong sentence is most expensive.

### Asks still outstanding

Carried verbatim per HM-DEC-139.

1. **Whether the transmission record asks the radio whether it keyed.** *First made
   2026-09-10, unit 303.* **Waiting on:** your ruling. **Where it sits:** `Played` is
   honest and unchanged; `unkeyed` now says Hamlet wrote the frame and claims nothing
   about the radio.

2. **Nothing in this repository can look at a picture.** *First made 2026-09-09, unit
   300.* Nine units running. Real pixels want `Avalonia.Headless.Skia`, and **a
   package is a dependency decision** (§0.4). **Waiting on:** your ruling. **Where it
   sits:** nowhere.

3. **The map image.** *First made 2026-09-09, unit 301.* The projection is built and
   proved; the picture and its three numbers are missing. **Waiting on:** a file.
   **Where it sits:** `assets/azimuthal-map.md`.

4. **Three inherited reds** — `TheAchievementsScreenTests`' two and
   `TheFitGuardAsksAboutTheGridTheSendIsOnTests`' one. *First made 2026-09-10, units
   302 and 303.* All proved red with the previous unit's changes stashed, and named by
   no order. **Waiting on:** an order that takes them.

5. **Two ±2 wobbles** on `SettingsWindow` and `AboutWindow`, far under their ceilings.
   *First made 2026-09-10, unit 302.* **Waiting on:** an order that takes them.
   **Note:** unit 304 made the About window's text depend on the machine's device
   list, so that wobble may now move.
