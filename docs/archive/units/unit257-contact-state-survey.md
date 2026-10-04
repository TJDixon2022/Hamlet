# Unit 257, task 1 — what the tree already knows about a station

**Measuring and reading only. Nothing under `src/` changed in this task.**

Everything below was read off the tree at `HEAD 49053db` on 2026-09-06, with the
build green: `dotnet build Hamlet.sln` returned **0 warnings, 0 errors** in
12.79 s before a word of this was written.

The instruction's own table was checked line by line. **It is accurate.** The one
correction it carried forward from unit 256 — that `Ft8Composer.cs:585` calls
`Ft8MessageDecoder.Decode` and not `Ft8SlotDecoder` — is still true and is not
re-derived here.

---

## 1. What a decoded slot actually hands over

**Confirmed. The relation between two stations lives only inside `Message`, and
nothing else on the path carries it.**

| Member | Where | What it is |
|---|---|---|
| `SlotStartUtc` | `src/Hamlet.RadioEngine/Audio/Ft8Reception.cs:25` | the quarter minute, corrected |
| `OffsetSeconds` | `:26` | the `dt` column |
| `FrequencyHz` | `:27` | the lowest of the eight tones |
| `SyncScore` | `:28` | a Costas match count, **not** a ratio |
| `Message` | `:29` | the text, exactly as sent |
| `SignalToNoiseDb` | `:62` | decibels in 2500 Hz, or null for not measured |

Six members. Five of them are about *when, where and how strongly*; the sixth is
a string. **There is no To, no From, no addressee, no sender and no station
anywhere on this record**, and the record's own remarks at `:19-23` say why:
*the text is not interpreted here and is not interpreted anywhere yet*
(`CLAUDE.md` §12.1).

Nothing beside it carries the relation either. `Ft8SlotCensus`
(`Ft8Reception.cs:100`) is eight counts, a level, a decoder identity, a port
comparison and an SNR spread — and its remarks at `:159-166` record that this is
deliberate: `AppEvents` is handed the census and never the reception, **so that
HM-DEC-018 is enforced by a signature** rather than by a caller remembering. A
per-message telemetry line would have carried callsigns into the one file that
ruling keeps them out of.

`Ft8Reception` (`:342`) is a list of decodes, three counts and a refusal.
`Ft8Reception.Slots` (`:358`) is the census list. `Ft8Reception.Offset` (`:369`)
is the clock offset. **None of them holds a callsign.**

**So the ledger has one input and it is a string plus a slot boundary.** There is
nothing already carrying the fields that the ledger could use instead, which
makes question 2 the load-bearing one.

**One thing the port already knew and threw away.** `Ft8Vocabulary`'s own remarks
at `src/Hamlet.App/ViewModels/Ft8Vocabulary.cs:41-46` record it:
`Ft8Sharp.Ft8StandardMessage.TryUnpack` produces the three fields **separately**,
and `Ft8Decode` carries only the joined string. That remark ends *"it is done
this way because unit 241 may not change the engine. It is reported rather than
worked around quietly."* — and this unit is not changing `Ft8Sharp` either
(`PHASE_PLAN.md`'s ruling, and this instruction's *what not to do* item 2). The
join stands; the split is re-derived from the text.

---

## 2. The splitter, and where it lives

### What `Ft8Vocabulary.Split` accepts and refuses

`src/Hamlet.App/ViewModels/Ft8Vocabulary.cs:48`, thirteen lines of code:

| Input | Result |
|---|---|
| null, empty, whitespace | **null** (`:50-53`) |
| `CQ DX W1ABC FN42` — four words, first is `CQ` | `("CQ DX", "W1ABC", "FN42")` (`:60-65`) |
| `CQ EU W1ABC FN42`, `CQ POTA W5LST EM33`, `CQ 123 K1ABC FN42` | same rule — the call and its direction are **one addressee in two words** |
| `K1ABC W9XYZ RR73` — plainly three fields | `("K1ABC", "W9XYZ", "RR73")` (`:67-69`) |
| `TNX BOB 73 GL` — four words, first is not `CQ` | **null** |
| `HW CPY OM` — three words | **accepted**, wrongly-shaped but three fields |
| `K1ABC W9XYZ` — two fields | **null** |
| `ABCDEFGHIJKLM` — one field | **null** |

It splits on spaces with `RemoveEmptyEntries` after a `Trim()`. **It does not
validate a callsign, a grid or a report** — it counts fields and applies one
special case. That is exactly right for the ledger: the ledger needs to know who
addressed whom, not whether the callsign was real.

Two consequences the ledger has to live with, and both are correct behaviour:

- **`HW CPY OM` splits.** Three words is three fields, so the ledger will book a
  station called `CPY` addressing a station called `HW`. That is what the format
  says and reading further would be interpreting the message (§12.1). The corpus
  in task 2 therefore uses a **four-word** free-text message for its refusal
  case, which is the one the splitter actually declines.
- **`CQ` is an addressee and not a station.** `Ft8Vocabulary.IsCallToAnyone` at
  `:246` is the existing test for it, and the ledger uses the same rule rather
  than a second one.

### Where the split lives — the decision

**The splitter moves into the engine, and the app's `Split` delegates to it.
There will be one splitter in this repository and not two.**

Why the engine and not a second engine-side copy:

1. **The alternative is two copies of the parsing rules, and the instruction
   forbids it.** The engine cannot reach `Hamlet.App` — §0.1 is absolute and
   enforced at compile time by project references — so leaving the splitter where
   it is means either writing it again in the engine or handing the ledger a
   delegate from the app. A delegate would put the engine's bookkeeping at the
   mercy of a caller in the UI layer, which is the same coupling by another door.
2. **The rule is a fact about the FT8 message format, not about a tab.** Which
   field is the addressee is fixed by the standard. §0.1's practical test —
   *could the engine be wrapped in a console app without touching it?* — says a
   fact about the format belongs in the engine, and the file's own remark at
   `:41-46` already says the split is the view doing work the engine could hand
   over.
3. **Every existing test keeps passing without being edited.** Seventeen call
   sites were counted (`Ft8Vocabulary.Split` and `Ft8MessageFields` across
   `src/` and `tests/`); every one either uses `var` or names
   `Ft8MessageFields`, so moving the record and the method into the engine and
   leaving `Ft8Vocabulary.Split` as a one-line forward keeps them all compiling
   and all asserting the same behaviour.

**The move, precisely:** the body of `Split` and the `Ft8MessageFields` record go
to `src/Hamlet.RadioEngine/Contacts/Ft8MessageSplit.cs`, in namespace
`Hamlet.RadioEngine.Contacts`. `Ft8Vocabulary.Split` becomes
`=> Ft8MessageSplit.Split(message)` and keeps its remarks. `Ft8Vocabulary.Explain`
and its closed table **do not move** — that table is meaning, Tim closed it on
2026-09-04, and it stays exactly where it is in the app.

---

## 3. Slot arithmetic — reuse, do not write a second one

`src/Hamlet.RadioEngine/Audio/Ft8Slots.cs` already holds all of it:

| Member | Line | What |
|---|---|---|
| `SlotSeconds = 15` | `:126` | the slot period |
| `TransmissionSeconds = 12.64` | `:135` | the tones inside it |
| `SlotStart(DateTime)` | `:182` | the quarter-minute boundary at or before a moment |
| `IntoSlot(DateTime)` | `:195` | seconds since the boundary |
| `BoundariesBetween(from, to)` | `:209` | every boundary in a stretch, oldest first |

**The call the ledger uses for *how many slots ago* is
`Ft8Slots.BoundariesBetween(then, now).Count`**, over the two slot starts. It
returns the boundaries strictly after `from` and up to and including `to`, so for
two consecutive slots it returns 1 and for the same slot it returns 0 — which is
exactly *how many slots ago*, counted in boundaries crossed rather than in
subtracted seconds.

**Why boundaries and not `(now - then).TotalSeconds / 15`:** the division is a
second copy of the arithmetic, it drifts on the rounding `Ft8Slots`'s own epsilon
remark at `:157-160` was written about, and it would answer 0 for two slot starts
14.9 s apart. `BoundariesBetween` is the function this repository already trusts
to draw the waterfall's rules, and it is the one that will still be right if
anybody ever changes what a slot is.

**No new slot arithmetic is written in this unit.**

---

## 4. What already tracks a station across time

| Type | Where | Reuse or leave | Why |
|---|---|---|---|
| `HeardWatch` / `HeardSummary` | `src/Hamlet.RadioEngine/Explore/HeardWatch.cs:65`, `:62` | **Leave** | It answers *did anybody hear me* from RBN skimmer reports (HM-DEC-075) — reports **about the operator, from automated receivers, off a spot feed**. Its four states (`Idle`, `Waiting`, `Heard`, `Nothing`, `:42`) are about one outgoing call and a window, not about a two-way exchange with a named station. Nothing here holds two callsigns together either. |
| `RecentStation` | `src/Hamlet.RadioEngine/Explore/RecentStation.cs:45` | **Leave** | It is *somewhere the operator stopped, and who was there* (HM-DEC-072) — a frequency, a band, a neighbourhood, a visit count. One callsign, no messages, no direction, no slots. `StationSource` (`:14`) is a good precedent for keeping *how we know* separate from *what we know*, and that precedent is followed in the ledger by keeping heard and sent apart; the type itself is not the shape. |
| `ContactStage` / `SendOption` / the CW send path | `src/Hamlet.RadioEngine/Cw/ContactStage.cs:10`, `:36` | **Leave — and this one is the trap** | See below. |
| `AutoCaller` | `src/Hamlet.RadioEngine/Cw/AutoCall.cs` | **Leave. Do not touch.** | It keys repeatedly from one operator start (`:272`), it is on the parked CW path, and it is on this unit's parked list. It is named here only so the list is complete. |
| `ContactShape` | `src/Hamlet.RadioEngine/Explore/ContactShape.cs` | **Leave** | Teaching material about what a contact looks like. It describes the ritual; it records nothing that happened. |

### `ContactStage` is the trap, and this is the sentence for the next reader

**`ContactStage` is the right shape and the wrong ruling, and it must not be
reached for.**

It is five stages — `Calling`, `Answering`, `Exchanging`, `Confirming`,
`SigningOff` — and it looks like exactly what a contact-state ledger wants. It is
not, and the reason is in its own remarks at `ContactStage.cs:4-9`: it exists so
that *"Hamlet offers the one thing anybody would say next rather than the whole
ritual at once"* (HM-DEC-059), and `SendOptions`'s remarks at `:47-53` double
down — *"CONTEXTUAL, NOT A MENU OF EVERYTHING ... The operator is never presented
with the whole ritual at once and asked to pick."*

**This phase rules the opposite way.** `PHASE_PLAN.md`: *nothing is forbidden in
the menu*; the expected next message is highlighted and **everything valid stays
clickable**; a repeat is correct behaviour and shows its count. A type whose
entire purpose is to narrow five choices to one is the previous ruling wearing
the right clothes, and building step 4 on it would carry that ruling into step
5's menu, where it would grey things out.

There is a second reason and it is enough on its own: **`ContactStage` is on the
CW send path**, its `SendOption.Pieces` calls `CwMessage.PieceCount` (`:40`), and
CW send is on this unit's parked list. Touching it reaches parked surfaces.

**So the ledger counts what passed and derives four states from the counts. It
does not have a stage, it does not advance through one, and it never offers
anything.**

---

## 5. What our own sent messages are, to the ledger

**`TransmitRecord` cannot tell the ledger anything, and that is by design rather
than by omission.**

`src/Hamlet.RadioEngine/Telemetry/TransmitRecord.cs:45` — eleven parameters: a
moment, five numbers, three enumerations and a flag. **Not one of them is a
string.** Its own remarks at `:16-23` say why in as many words: *"THE SHAPE
REFUSES RATHER THAN THE CALL SITE REMEMBERING ... so `Ft8Transmission.Text` and
`ReadsBackAs` have nowhere to be put, not by accident, not in a hurry, and not
behind a flag"* — and `ATransmitRecordCannotCarryTheMessage` asserts it by
reflection. That is HM-DEC-018 and it is not this unit's to weaken.

**So the ledger does not learn what we sent from telemetry. It is told.**

The ledger takes an explicit call, and it is one line:

```csharp
ledger.RecordSent("K1ABC KC3QIS -13", slotStartUtc);
```

Same shape as the receive side, which is `RecordHeard(message, slotStartUtc)` —
or `Record(Ft8Decode)` where a decode is in hand. Two methods, both taking a
message and a slot, differing only in which direction the message went.

**Nothing in this repository calls `RecordSent` today**, and that is correct:
nothing in `src/` transmits, this unit builds nothing that does, and the caller
arrives with step 5, where a right-click that sends a message is exactly the
place that knows what it sent and when. It is designed so that step 5's send
path adds one line beside the line that hands the samples to the sink.

**What the ledger does not do:** it does not read `OperatorProfile`, it does not
read settings, it does not subscribe to telemetry, and it does not watch a
transmit sequence. The operator's own callsign arrives as a **constructor
parameter** — `src/Hamlet.App/Settings/OperatorProfile.cs:44` holds the value and
the app passes it in, so the engine still does not know that settings or tabs
exist (§0.1).

---

## 6. What FT8 material exists on this machine

**Verified, and the instruction's reading is correct. There is no real capture in
this tree.**

Every file under `tests/fixtures/ft8/`:

```
tests/fixtures/ft8/captured/README.md
tests/fixtures/ft8/example/ft8-example-244.fixture.txt
tests/fixtures/ft8/example/ft8-example-244.wav
tests/fixtures/ft8/parser-inputs/README.md
tests/fixtures/ft8/parser-inputs/bad-number.decode.txt
tests/fixtures/ft8/parser-inputs/good.decode.txt
tests/fixtures/ft8/parser-inputs/no-tilde.decode.txt
tests/fixtures/ft8/parser-inputs/nothing.decode.txt
tests/fixtures/ft8/parser-inputs/short-line.decode.txt
```

Nine files. **One `.wav`, and it is the worked example.**

- `tests/fixtures/ft8/captured/README.md` holds no captures beside it and says
  so: *"This folder is empty of captures today, and that is the correct state on
  the development machine"*, `SHACK_FACTS.md` FACT-004, and *"A fixture that
  names a capture which is not here ... is a hard failure — see
  `Ft8CaptureFixture.RequireCapture`."*
- The same README: *"Only fixtures whose `provenance` reads `wsjtx` may be scored
  against; the worked example in `../example/` is refused for scoring."*
- `tests/fixtures/ft8/parser-inputs/` is text fed to a fixture-file parser. No
  audio, no slots, no scene.

**I did not find a real capture the instruction missed.** There is none to
prefer, so the scene in task 2 is synthesized, goes in a **new**
`tests/fixtures/ft8/scenes/` folder, is never given the provenance `wsjtx`, and
its README says in its own words that it may never be scored against the
decoder's accuracy.

**What Tim would run at the shack to make a real one:** capture a run of
consecutive slots off 14.074 MHz through the IC-7300's USB audio with WSJT-X
running beside Hamlet, then, per capture,
`dotnet run --project tools/Ft8FixtureMaker -- <capture.wav>` — which is the one
command `tests/fixtures/ft8/captured/README.md` names, with no editing step
afterwards.

---

## 7. What one slot's decode costs

**Neither `WhatOneSlotCostsTests` nor `HowFastTheDecoderEatsAudioTests` states a
figure in its source** — the first prints its timings and asserts only that the
budget is met (`WhatOneSlotCostsTests.cs:111-116`); the second pushes sixty
seconds of audio and asserts the harness pushed it (`:113`). So it was measured
once, filtered by exact name, in the foreground.

```
dotnet test tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj
  --filter "FullyQualifiedName~WhatOneSlotCostsTests.OneSlotThroughThePortAndThroughDeep"
```

Passed in 2 s. Total run 5.16 s. On
`tests/fixtures/ft8/example/ft8-example-244.wav`, one slot, decode only:

| Route | Cost | Messages |
|---|---|---|
| the port, `Ft8SlotDecoder` | **50.9 ms** | 3 |
| Deep via the waterfall overload | 88.1 ms | 3 — fine sync refused 42 for want of samples |
| **Deep via samples, `Ft8DeepSlotDecoder`** | **582.5 ms** | 3 — the entry point `Ft8Reader` calls |

Budget 15,000 ms. Deep uses **3.88 %** of it; 14,418 ms of margin.

**What that makes the corpus cost.** Twelve slots is the cap:

```
12 slots x 582.5 ms  =  6.99 s of decoding through Deep
12 slots x  50.9 ms  =  0.61 s through the port
```

Composition is on top of that and is cheap — `Ft8Composer.Compose` is a pack and
a synthesis, and unit 256's round-trip class composed and decoded 117 messages
inside one test. **Seven seconds is not a budget problem**, so the audio route is
affordable at the cap with two orders of magnitude to spare, and there is no
figure here that forces the scripted route.

**Which decoder the corpus uses: `Ft8DeepSlotDecoder` with both stages on**, the
same construction `Ft8Reception.cs:460-462` makes. It is Hamlet's own decoder
since unit 249 by Tim's ruling of 2026-09-05, and a corpus read back by the
decoder that is *not* the one the application runs would be evidence about a
decoder nobody uses.

---

## Go / no-go

Seven seconds of decoding against a cap of twelve slots, on a machine that opens
no audio device to do it. The measurement does not say I cannot.

```
CORPUS ROUTE: audio
```
