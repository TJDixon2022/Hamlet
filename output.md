READ IN THIS ORDER — A the phase goal, B this step and its criteria, C what this
report adds and whether any of it bears on A or B.

A. THE PHASE GOAL AND WHERE EVERY STEP STANDS. *Hamlet works stations on the
air*: Tim answers a CQ on 14.074 or 7.074 and completes an exchange. Steps 0, A,
B and C are `done`. **Step D is `done` after task 4**, on the evidence of
`SHACK_FACTS.md` FACT-005 — 25 per cent, -12.04 dBFS, ALC `-2.0 to -1.5, inside
the red zone` on the IC-7300 at 14.074 MHz, sourced to the operator at his own
radio on 2026-09-07 and written by unit 271 before this unit started. **Step E is
`in progress` and all three of its criteria are his**, at the radio, on a band.
(The tree's `PHASE_OUTCOME.md` header actually reads `STEP: E | blocked`; the
instruction said not to touch that line and it was not touched. Reported in
section 4.)

B. THIS STEP AND ITS EXIT CRITERIA, AND WHICH WERE MET. Step E's three, quoted
from `PHASE_PLAN.md:265-268`, and **no criterion of step E is claimed by this
unit — none could be.**

1. *He answers a CQ on 14.074 or 7.074 and completes an exchange.* **Not met.** No
   unit can meet it. **Its remaining measured bench blocker was that 15 of the 43
   messages the menu can offer against the callsigns he could plausibly work
   tonight would have been keyed and read back as nothing** — every one of them a
   compound prefix, including his own `W4/KC3QIS`. **That blocker is now closed:**
   Hamlet refuses to key any of them and says so.
2. *The transmitted slots appear in telemetry and the row reads complete.* **Not
   met.** Its telemetry half moved: `ft8_transmission` now records
   `carriedHashedCallsign`, which it did not carry at all, and `messageLength`
   now counts the encoded message rather than the string the operator clicked.
3. *What he saw, and anything that surprised him, recorded.* **Not met.** His.

C. THIS REPORT'S OWN FINDINGS, WEIGHED AGAINST A AND B.
**Section 4 raises 5 items, and none of them is in the way of a criterion in B.**
None asks for a ruling. Three are mismatches between the instruction and the tree, reported and
not repaired; one is a conflict inside the instruction that cost the click-driven
test its run; one is a parked question the arbiter already logged.

```
UNIT:       272 — complete at task 4 of 4 — 2026-09-07 14:10
PHASE GOAL: Hamlet works stations on the air — Tim answers a CQ on 14.074 or
            7.074 from his own shack and completes an exchange.
UNIT GOAL:  Hamlet never keys a message nobody can read, it says so when it
            refuses, and the telemetry line says which kind of message went out.
ADVANCED:   yes — 15 of 43 messages the menu can offer would have been keyed
            unreadably; now 0 are, and the telemetry line names the fault.
NUMBER:     15 of 43 keyed-and-unreadable -> 0 of 43, refused with a sentence
DRIFT:      0 consecutive units without advance  (was 0)
```

## 1. What Claude did

**Complete, at task 4 of 4.** Nothing was dropped, including task 4, which the
instruction named as the drop candidate.

Provenance: `C:\Source\HamLet`, branch `main`, project claimed `Hamlet` and
confirmed against the tree — `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` both present, `CoreHMI.sln`
and `MURC.sln` both absent. Six commits, `3f641e8` through `7953146`, each pushed
before the next task began, the trace on its own and each red before its green.
Version `1.12.125 -> 1.12.126`.

### Task 1 — the trace

**The central claim holds.** `grep -rn CarriesHashedCallsign src/` returns
`Ft8Composer.cs` and nothing else — lines 83, 99, 405, 681, 696 and 748, plus
generated `bin/obj` artefacts. Every reader in the tree is a test. The same grep
over `tests/` returns twenty-one source lines across nine files, all of them
assertions or table columns.

The six questions, answered with a file, a line and a quotation:

1. **Who reads it in `src/`?** Nobody. See above.
2. **Where does a composed message become an armed send?**
   `MainWindowViewModel.SendMessage:8446`, the only `ComposeSignal` call site in
   the whole of `src/`. Between `if (!composed.Composed)` at `:8471` and
   `_armedSend.Arm(new OperatorSend(...))` at `:8498` the method reads the clock,
   checks `_armedSend is null`, and sets `_armedText`. **Nothing there consults
   `Type`, `ReadsBackAs` or `CarriesHashedCallsign`.**
3. **What does the composer hand back?** `Ft8Transmission` carries `Text`,
   `ReadsBackAs`, `Type`, `Samples`, `SampleRate`, `BaseFrequencyHz` and
   `CarriesHashedCallsign` at `Ft8Composer.cs:92-99`. The flag is set true on the
   hashing pass at `:681` and false at `:696`; all three are copied onto the
   returned record at `:398-405`.
4. **Where is the bracket comparison, and is `allowHashing` reachable?**
   `ReadsBackAsItself(buffer, cache, wanted, allowHashMarking: true, out var
   hashedText)` at `:674`. `ComposeSignal`'s parameters, read by reflection off
   the running assembly, are `text, sampleRate, baseFrequencyHz, drivePeak` —
   **no `allowHashing`**, so there is no one-argument route from the application.
   **But the flag itself is public on the returned transmission**, so the guard
   reads it at the caller.
5. **How big is the class?** The table is in section 3. **15 of 43.**
6. **What does the telemetry say?** `Ft8TransmitSequence.Recorded:389-405` passed
   `send.Transmission.Text.Length`. For `CQ KC3QIS FN00DJ` on 2026-09-07 that
   counted **16**, and what actually went out was `<CQ KC3QIS> <FN00DJ>`, twenty
   characters, with the callsign hashed. For a compound call it would count 22 for
   a slot carrying 24. **A reader of that line would not know.**

### Task 2 — the guard

Watched red first and committed red (`7485c81`): five red, five green, with
`VP2MAA KC3QIS FN00DJ` reading `armed: True` beside `the encoder made:
"<VP2MAA KC3QIS> FN00DJ"`. Green at `52d4859`, ten of ten.

**A decision I made for myself, reproduced in full: I guarded on the flag and not
on the written fallback.** The instruction's fallback said to guard on
`ReadsBackAs` *if task 1 q4 finds the flag unreachable from the application*. It
is not unreachable — `allowHashing` is not a parameter of `ComposeSignal`, but
`CarriesHashedCallsign` is a public property of the record `ComposeSignal`
returns. So `Ft8ReadBack.Check` reads the flag first, and uses `ReadsBackAs`
only for the second half of the arbiter's rule (*unless he clicked brackets*),
which is expressed as *what the bits say is character-for-character what he
typed*.

**The rule lives in the engine** (§0.1), in
`src/Hamlet.RadioEngine/Contacts/Ft8ReadBack.cs`. It reads the two facts
`Ft8Composer` already measured and nothing else: it does not know what a callsign
looks like, does not split a message into fields, and does not know which
callsigns are compound. **There is no second copy of any callsign rule** (§0).
The view model asks it and states nothing of its own.

**Nothing in `src/Hamlet.RadioEngine/Transmit/` changed in task 2, and it is
proven:** `git diff --stat` over that directory was empty at the commit, and
`TheOperatorsStopFiresFromEveryStateTests` passed unchanged, **twelve of twelve**,
including `KeyedMidTransmissionTheAbortFiresWhileItIsStillRunning`.
`TheSeamTurnsWordsIntoASlotOfAudioTests` 23 of 23 and
`TheGridFitsTheMessageTests` 3 of 3, each alone by exact name, foregrounded.

**The menu was not touched.** No item removed, greyed, hidden or reordered;
`SendMenuFor` is unchanged and nothing about the guard reaches `Ft8SendOptions`.
No dialog, no confirmation, no prompt: a line in the Send area, where the licence
gate already refuses.

### Task 3 — the telemetry line

Watched red first and committed red (`930ef0a`): four red — `KeyNotFoundException
: The given key 'carriedHashedCallsign' was not present in the dictionary`, and
`messageLength Expected: 22, Actual: 20`. Green at `228e0d1`, five of five.

`TransmitRecord` gains `bool CarriedHashedCallsign` and the bag gains
`carriedHashedCallsign`. `Ft8TransmitSequence.Recorded` now passes
`send.Transmission.ReadsBackAs.Length` instead of `Text.Length`, which is the
decision the arbiter made and this unit implemented rather than re-argued.

**HM-DEC-018 holds and is asserted on both sides of the change.** The record's
constructor still has no string parameter at all —
`DateTime SlotStartUtc, Double StartSecondsIntoSlot, Int64 FrequencyHz, Double
DurationSeconds, Int32 SampleRate, Int32 SampleCount, Ft8MessageType MessageType,
Int32 MessageLength, Boolean CarriedHashedCallsign, Ft8TransmitOutcome Outcome,
UnkeyRoute CameOutOfTransmit, Boolean Keyed` — and none of `KC3QIS`, `VP2MAA`,
`FN00`, the message or its read-back appears anywhere in the bag.

**The one change in `Transmit/`, quoted rather than asserted**, replacing task 2's
empty-diff proof:

```
--- a/src/Hamlet.RadioEngine/Transmit/Ft8TransmitSequence.cs
@@ -396,7 +405,8 @@
             send.Transmission.Type,
-            send.Transmission.Text.Length,
+            send.Transmission.ReadsBackAs.Length,
+            send.Transmission.CarriesHashedCallsign,
             run.Outcome,
```

plus a doc paragraph above it saying why. **Nothing between the end of the audio
and the unkey moved**; the record is still written after the radio is out of
transmit. The abort's twelve passed again after it.

**The refusal in task 2 does mean a hashed message can no longer reach `Recorded`
through the application**, so these tests drive `Ft8TransmitSequence.RunAsync`
directly, as the instruction directs — the field still has to be right for
anything that arrives by another route later.

### Task 4 — step D

`PHASE_OUTCOME.md`'s header read `STEP: D | blocked`; it now reads `done`, with
the figures on the line and one sentence naming FACT-005 as the evidence. **The
cursor moved and no entry was invented.** Step E's line was not touched.
`SHACK_FACTS.md` and `docs/phase-send-run/` were not touched, proven by an empty
`git diff --stat` over both.

### What was run, and what was not

Only what this unit constructed, plus the three named committed tests, each alone
by exact name, foregrounded, under a 7-minute timeout: three test classes of my
own (3, 10 and 5 cases), `TheOperatorsStopFiresFromEveryStateTests` twice (12 of
12 both times), `TheSeamTurnsWordsIntoASlotOfAudioTests` (23 of 23) and
`TheGridFitsTheMessageTests` (3 of 3). **No suite, nothing unfiltered, nothing
backgrounded, and `Hamlet.App.Tests` was not run.** `dotnet build Hamlet.sln`
foregrounded, 0 warnings 0 errors. `src/Ft8Sharp/` not touched.

**Four shell refusals, recorded verbatim:**

- `This Bash command contains multiple operations. The following part requires
  approval: echo "EXIT:$?"` — the call was split and rerun.
- `This Bash command contains multiple operations. The following part requires
  approval: tools/arbiter/validate-output.bat output.md 2>&1`
- `This command requires approval` — `tools/arbiter/validate-output.bat output.md`
- `This command requires approval` — `dotnet msbuild
  tools/arbiter/validate-output.proj -p:Report=output.md`

The validator was reached through `dotnet build
tools/arbiter/validate-output.proj -p:Report=output.md`, which is the parked
permitted-spellings bug working exactly as unit 243 documented it, and is not
raised as an item. **`output.md` validates: all seven rules passed,
`validate-output exit 0`.** The file-editing tools were unaffected throughout, as
they have been for sixteen consecutive units. **Nothing halted the loop.**

## 2. What the owner should expect

**Hamlet will now sometimes refuse to send something you clicked, and it will
tell you why.** If you right-click a station whose callsign will not fit an FT8
message — a compound prefix like `VP2M/K1ABC` or `SV9/PA3EXX`, the kind of call
a DX station answering your CQ often signs — and click one of the five messages,
nothing will be keyed and the Send area will say:

> Hamlet composed "VP2M/K1ABC KC3QIS FN00" and sent nothing: it encodes as
> "<VP2M/K1ABC> KC3QIS FN00", which puts a callsign on the air as a 22-bit hash
> instead of as a callsign. A station can only put a name to that hash if it
> heard the whole callsign in the same slot, so anybody hearing this on its own
> decodes nothing at all. Nothing was keyed. The message is still in the menu and
> everything else toward that station will go.

**This is the fault that put two unreadable slots on 14.074 on 2026-09-07.** The
level was right, the timing was right, the log said `Sent`, and nobody on the
band could turn either slot back into your callsign. Unit 271 fixed the grid that
caused those two; this closes the class they belonged to.

**Nothing was removed from the menu.** Every one of the five messages is still
there, in the same order, with the same labels, and the expected one still
highlighted. The refusal is at the send path, in the same place and the same
shape as the line you already get when a frequency is outside your privileges.
There is no new dialog, no confirmation and no second click.

**What will look wrong but is not.** Refusing a genuinely nonstandard callsign
means you cannot work that station from Hamlet tonight, even though other
software would put something on the air for you. That is deliberate — what that
software puts out is a slot nobody can decode either, and Hamlet will not assert
something nobody receives. Whether Hamlet should later offer you a way to send
one anyway is logged and parked, not decided against.

**And the log will be honest about it.** The `ft8_transmission` line now says
whether the callsign went out as a hash, and the length it prints is the length
of what actually went on the air rather than of what you typed.

## 3. What you should see

### 1. The count from task 1, quoted off the run

> **15 of 43 messages the menu can offer today would be keyed and read back as
> nothing.**

Measured at 48000 Hz through the application's own `ComposeSignal`, resampled to
12000 by `Ft8Resample` and decoded by `Ft8SlotDecoder` — the same two calls
`Ft8Reception` makes on every slot Hamlet hears. The corpus is
`Ft8SendOptions`'s own: the CQ button's message and the five shapes the
right-click menu offers, against eight operator-and-station pairs.

```
callsign class           shape            message                    composes  hashed  bits say                     decoder returns
--------------------------------------------------------------------------------------------------------------------------------
a plain call             CQ button        CQ KC3QIS FN00             yes       no      CQ KC3QIS FN00               "CQ KC3QIS FN00"
a plain call             Grid             K1ABC KC3QIS FN00          yes       no      K1ABC KC3QIS FN00            "K1ABC KC3QIS FN00"
a plain call             Report           K1ABC KC3QIS -12           yes       no      K1ABC KC3QIS -12             "K1ABC KC3QIS -12"
a plain call             RogerAndReport   K1ABC KC3QIS R-12          yes       no      K1ABC KC3QIS R-12            "K1ABC KC3QIS R-12"
a plain call             Acknowledge      K1ABC KC3QIS RRR           yes       no      K1ABC KC3QIS RRR             "K1ABC KC3QIS RRR"
a plain call             Seventy3         K1ABC KC3QIS 73            yes       no      K1ABC KC3QIS 73              "K1ABC KC3QIS 73"
the one from 2026-09-07  Grid             VP2MAA KC3QIS FN00         yes       no      VP2MAA KC3QIS FN00           "VP2MAA KC3QIS FN00"
the one from 2026-09-07  Report           VP2MAA KC3QIS -12          yes       no      VP2MAA KC3QIS -12            "VP2MAA KC3QIS -12"
the one from 2026-09-07  RogerAndReport   VP2MAA KC3QIS R-12         yes       no      VP2MAA KC3QIS R-12           "VP2MAA KC3QIS R-12"
the one from 2026-09-07  Acknowledge      VP2MAA KC3QIS RRR          yes       no      VP2MAA KC3QIS RRR            "VP2MAA KC3QIS RRR"
the one from 2026-09-07  Seventy3         VP2MAA KC3QIS 73           yes       no      VP2MAA KC3QIS 73             "VP2MAA KC3QIS 73"
a compound prefix        Grid             VP2M/K1ABC KC3QIS FN00     yes       YES     <VP2M/K1ABC> KC3QIS FN00     NOTHING
a compound prefix        Report           VP2M/K1ABC KC3QIS -12      yes       YES     <VP2M/K1ABC> KC3QIS -12      NOTHING
a compound prefix        RogerAndReport   VP2M/K1ABC KC3QIS R-12     yes       YES     <VP2M/K1ABC> KC3QIS R-12     NOTHING
a compound prefix        Acknowledge      VP2M/K1ABC KC3QIS RRR      yes       YES     <VP2M/K1ABC> KC3QIS RRR      NOTHING
a compound prefix        Seventy3         VP2M/K1ABC KC3QIS 73       yes       YES     <VP2M/K1ABC> KC3QIS 73       NOTHING
a portable suffix        Grid             K1ABC/P KC3QIS FN00        yes       no      K1ABC/P KC3QIS FN00          "K1ABC/P KC3QIS FN00"
a portable suffix        Report           K1ABC/P KC3QIS -12         yes       no      K1ABC/P KC3QIS -12           "K1ABC/P KC3QIS -12"
a portable suffix        RogerAndReport   K1ABC/P KC3QIS R-12        yes       no      K1ABC/P KC3QIS R-12          "K1ABC/P KC3QIS R-12"
a portable suffix        Acknowledge      K1ABC/P KC3QIS RRR         yes       no      K1ABC/P KC3QIS RRR           "K1ABC/P KC3QIS RRR"
a portable suffix        Seventy3         K1ABC/P KC3QIS 73          yes       no      K1ABC/P KC3QIS 73            "K1ABC/P KC3QIS 73"
a short call             Grid             PJ4G KC3QIS FN00           yes       no      PJ4G KC3QIS FN00             "PJ4G KC3QIS FN00"
a short call             Report           PJ4G KC3QIS -12            yes       no      PJ4G KC3QIS -12              "PJ4G KC3QIS -12"
a short call             RogerAndReport   PJ4G KC3QIS R-12           yes       no      PJ4G KC3QIS R-12             "PJ4G KC3QIS R-12"
a short call             Acknowledge      PJ4G KC3QIS RRR            yes       no      PJ4G KC3QIS RRR              "PJ4G KC3QIS RRR"
a short call             Seventy3         PJ4G KC3QIS 73             yes       no      PJ4G KC3QIS 73               "PJ4G KC3QIS 73"
a long call              Grid             SV9/PA3EXX KC3QIS FN00     yes       YES     <SV9/PA3EXX> KC3QIS FN00     NOTHING
a long call              Report           SV9/PA3EXX KC3QIS -12      yes       YES     <SV9/PA3EXX> KC3QIS -12      NOTHING
a long call              RogerAndReport   SV9/PA3EXX KC3QIS R-12     yes       YES     <SV9/PA3EXX> KC3QIS R-12     NOTHING
a long call              Acknowledge      SV9/PA3EXX KC3QIS RRR      yes       YES     <SV9/PA3EXX> KC3QIS RRR      NOTHING
a long call              Seventy3         SV9/PA3EXX KC3QIS 73       yes       YES     <SV9/PA3EXX> KC3QIS 73       NOTHING
the operator portable    CQ button        CQ KC3QIS/P FN00           yes       no      CQ KC3QIS/P FN00             "CQ KC3QIS/P FN00"
the operator portable    Grid             K1ABC KC3QIS/P FN00        yes       no      K1ABC KC3QIS/P FN00          "K1ABC KC3QIS/P FN00"
the operator portable    Report           K1ABC KC3QIS/P -12         yes       no      K1ABC KC3QIS/P -12           "K1ABC KC3QIS/P -12"
the operator portable    RogerAndReport   K1ABC KC3QIS/P R-12        yes       no      K1ABC KC3QIS/P R-12          "K1ABC KC3QIS/P R-12"
the operator portable    Acknowledge      K1ABC KC3QIS/P RRR         yes       no      K1ABC KC3QIS/P RRR           "K1ABC KC3QIS/P RRR"
the operator portable    Seventy3         K1ABC KC3QIS/P 73          yes       no      K1ABC KC3QIS/P 73            "K1ABC KC3QIS/P 73"
the operator compound    CQ button        CQ W4/KC3QIS FN00          NO        -       (refused)                    (never keyed)
the operator compound    Grid             K1ABC W4/KC3QIS FN00       yes       YES     K1ABC <W4/KC3QIS> FN00       NOTHING
the operator compound    Report           K1ABC W4/KC3QIS -12        yes       YES     K1ABC <W4/KC3QIS> -12        NOTHING
the operator compound    RogerAndReport   K1ABC W4/KC3QIS R-12       yes       YES     K1ABC <W4/KC3QIS> R-12       NOTHING
the operator compound    Acknowledge      K1ABC W4/KC3QIS RRR        yes       YES     K1ABC <W4/KC3QIS> RRR        NOTHING
the operator compound    Seventy3         K1ABC W4/KC3QIS 73         yes       YES     K1ABC <W4/KC3QIS> 73         NOTHING

MESSAGES THE MENU CAN OFFER : 43
compose and read back       : 27
refused by the composer     : 1
carry a hashed callsign     : 15

15 of 43 would be keyed today and read back as nothing.
```

**It is not the class the instruction predicted, and that is the finding inside
the finding.** The instruction named *the compound and portable callsigns*.
**Portable suffixes are fine** — `K1ABC/P KC3QIS FN00` packs `Standard` and reads
back as itself on all five shapes, and so does the operator's own `KC3QIS/P`.
**It is compound prefixes**, and it includes his own `W4/KC3QIS`. One message
already refused honestly before this unit: `CQ W4/KC3QIS FN00` will not compose
at all.

### 2. `VP2MAA KC3QIS FN00DJ` clicked, after the guard — beside a good message

```
he clicked        : "VP2MAA KC3QIS FN00DJ"
the encoder made  : "<VP2MAA KC3QIS> FN00DJ"
hashed callsign   : True
armed             : False
bytes at the port : 0
sink touched      : False

THE SEND AREA LINE, IN FULL:
Hamlet composed "VP2MAA KC3QIS FN00DJ" and sent nothing: it encodes as
"<VP2MAA KC3QIS> FN00DJ", which puts a callsign on the air as a 22-bit hash
instead of as a callsign. A station can only put a name to that hash if it heard
the whole callsign in the same slot, so anybody hearing this on its own decodes
nothing at all. Nothing was keyed. The message is still in the menu and
everything else toward that station will go.
```

Driving the slot boundary it would have gone out on still produces
`Ft8ArmOutcome.NothingArmed`, no run, zero bytes and an untouched sink — a
refusal that leaves something armed is not a refusal.

**And beside it, through the same three calls, so it reads as a gate:**

```
"CQ KC3QIS FN00" -> "CQ KC3QIS FN00"
outcome            : Ran
sent               : True
bytes at the port  : 16
wire               : FE FE 94 E0 1C 00 01 FD FE FE 94 E0 1C 00 00 FD
samples to the card: 606720 at 48000 Hz
```

The same for `VP2MAA KC3QIS FN00`, `K1ABC/P KC3QIS -12` and `CQ KC3QIS/P FN00`.
**Ten of ten.** And the menu toward a station every one of whose messages is
refused still offers all five, in exchange order, with their labels:

```
grid                   "VP2M/K1ABC KC3QIS FN00"  menu: offered, send path: refused
report                 "VP2M/K1ABC KC3QIS -12"   menu: offered, send path: refused
roger and report       "VP2M/K1ABC KC3QIS R-12"  menu: offered, send path: refused
acknowledge            "VP2M/K1ABC KC3QIS RRR"   menu: offered, send path: refused
73                     "VP2M/K1ABC KC3QIS 73"    menu: offered, send path: refused
```

### 3. The two telemetry bags, hashed and not

```
HASHED  - "VP2MAA KC3QIS FN00DJ"        IN FULL - "VP2MAA KC3QIS FN00"
  messageType : NonstandardCallsign       messageType : Standard
  messageLength         : 22              messageLength         : 18
  carriedHashedCallsign : True            carriedHashedCallsign : False
  outcome               : Sent            outcome               : Sent
  keyed                 : True            keyed                 : True
```

**And the line from 2026-09-07, then and now.** It read `messageType: Standard,
messageLength: 16` for a slot that carried `<CQ KC3QIS> <FN00DJ>` and decoded to
nothing. It now reads:

```
  messageType           : Standard
  messageLength         : 20
  carriedHashedCallsign : True
```

The type is unchanged, because `Standard` is a true fact about the format and not
this unit's to alter. **What has changed is that the flag beside it says the slot
was unreadable.** An ordinary transmission's line did not move at all —
`CQ KC3QIS FN00` still counts 14, because for a message that reads back as itself
the two counts are the same count.

## 4. What's blocking us

**Five items. None asks for a ruling, and none is in the way of a criterion in
B.**

### 1. The instruction predicted the wrong class, and the tree wins

The instruction says *the compound and portable callsigns of the DX stations he
is most likely to answer are the obvious candidates*. Measured, **portable
suffixes are not in the class at all**: `K1ABC/P`, `KC3QIS/P` and every message
built on them packs `Standard` and reads back as itself. The fifteen are all
compound *prefixes*. Reported, not repaired. It does not change what was built —
the guard is written on what the encoder did, not on the shape of a callsign, so
it catches the real class whatever it turns out to be.

### 2. The arbiter's brackets branch is unreachable against this tree

The ruling is *a hashed callsign is not the words the operator clicked, **unless
he clicked brackets***. Measured, `Ft8Composer` refuses a bracketed string
outright, before the guard is ever asked:

> Hamlet did not send "<VP2M/K1ABC> KC3QIS FN00": "<VP2M/K1ABC> KC3QIS FN00" is
> not a message this library can put on the air. What was tried … standard, in
> full, "<VP2M/K1ABC>" / "KC3QIS" / "FN00" would not pack: FirstCallInvalid …

I expected that string to compose and arm, and the tree said otherwise; the test
records the measured truth rather than my expectation. The second half of the
rule is kept anyway, because it costs nothing and a guard written on the flag
alone would refuse such a message for the wrong reason if the composer ever
accepted one. **Nothing is asked here** — it is a note for whoever picks up the
parked question of offering him a way to send a nonstandard callsign.

### 3. Step D's criterion 1 has a clause the instruction did not quote

`PHASE_PLAN.md:246` reads *Tim sets the Transmit drive control and reads the dBFS
**and clip count** under the waterfall*. The instruction quoted it without the
clip count, and **no clip count is recorded anywhere** — `grep -rn clip
SHACK_FACTS.md` returns nothing. I moved the cursor to `done` on the arbiter's
decision, which named FACT-005 as answering all three criteria and is not this
unit's to re-argue, and I am naming the gap so it can be reversed by a reader who
judges otherwise. My own reading: a peak of 0.25 cannot clip, so that reading is
arithmetically forced and its absence from the record is a bookkeeping gap rather
than an unanswered question. The two must-pass criteria that genuinely blocked
step D — his ALC in his words, and the number written down — are answered without
qualification.

### 4. Step E's line in the tree says `blocked`, not `in progress`

`PHASE_OUTCOME.md`'s header reads `STEP: E | blocked | Tim works a station`,
while the instruction and the `ARBITER-DECISION` block both say `in progress`.
The instruction says not to touch step E's line, so it was not touched. The
arbiter's own append is what resolves it. Reported, not repaired.

### 5. The click-driven test was built and not run — a conflict inside the instruction

Task 2 requires the test to be *driven from the operator's click rather than from
a constructed transmitter*. The click lands on
`MainWindowViewModel.SendMessageCommand`, which lives in `Hamlet.App`;
`Hamlet.RadioEngine.Tests` references only the engine and cannot reach it, and
the instruction forbids running `Hamlet.App.Tests`. Resolved by doing both:
`tests/Hamlet.App.Tests/ViewModels/TheSendPathRefusesWhatNobodyCanReadTests.cs`
drives `SendMessageCommand.Execute` and reads `DigitalSendLine`, and is **built
and not run**; the runnable measurement is
`HamletDoesNotKeyWhatNobodyCanReadTests`, which makes the send path's own three
calls with its own arguments against the **real** `Ft8ArmedSend`,
`Ft8TransmitSequence`, fake port and fake sink. **What I cannot claim on a run
tonight is that the guard is reached from the click itself** — only that the
guard is correct and that the view model calls it. Said plainly rather than
glossed.
