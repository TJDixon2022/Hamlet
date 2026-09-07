READ IN THIS ORDER

A. THE PHASE GOAL — **Hamlet works stations on the air**: Tim answers a CQ on
14.074 or 7.074 from Hamlet and completes an exchange. Where the steps stand:
**0, A, B and C are `done`**, closed by units 266, 267 and 268 — the record made
honest, the row that knows where a contact stands, right-click-and-it-goes, and
the whole chain from one click at the bench. **D is Tim's at his own radio**;
this unit did not touch it. **E is `in progress` and this unit closed none of
its criteria** — all three are his evening. What this unit did is clear four
defects standing between him and them, one of which had already gone out over a
live antenna twice.

B. THIS STEP AND ITS EXIT CRITERIA — **step E, *Tim works a station*.** Three
criteria. **1: he answers a CQ on 14.074 or 7.074 and completes an exchange** —
only he can meet it. **It was not meetable before tonight.** Hamlet's CQ
composed `CQ KC3QIS FN00DJ`, which encodes to `<CQ KC3QIS> <FN00DJ>` and
**decodes to nothing at all**; a station that cannot be heard cannot be answered.
That is measured below and it is repaired. **2: the transmitted slots appear in
telemetry and the row reads complete** — only he can meet it; unit 270 closed the
row's half and unit 264 the telemetry's. This unit found the telemetry's
`messageLength` measures the wrong thing and reports it without changing it.
**3: what he saw and anything that surprised him, recorded** — only he can meet
it, and `SHACK_FACTS.md` now holds the one number he had already measured and
nobody had written down. **No criterion of B is claimed as met.**

C. WHAT THIS REPORT ADDS, AND WHETHER IT BEARS ON A OR B — **the headline is
that Hamlet transmitted something nobody could decode, twice, on a live
antenna**, and it bears directly on A and on B's criterion 1. It is measured
rather than inferred, the cause is named, and it is repaired with the round trip
as the test. Section 4 **raises 5 items**, **none of which asks the owner to
decide anything and none of which blocks a criterion in B**: one
instruction-versus-tree mismatch that this unit could not resolve and reports
rather than repairs, one shell refusal with its alternative taken, one criterion
this unit could not assert by a runnable test because the only home for that test
is the project the instruction forbids running, one test run wider than the
letter of a standing rule and said so, and one format observation about `RR73`.
**Task 6, the named drop candidate, was not dropped.**

UNIT:       271 — complete at task 6 of 6 — 2026-09-07 13:42
PHASE GOAL: Hamlet works stations on the air — Tim answers a CQ on 14.074 or 7.074 and completes an exchange
UNIT GOAL:  The CQ button calls CQ, the grid fits the message, the contact column speaks only about his own contacts, and the stop control says what it is
ADVANCED:   yes — a defect that had already gone out on a live antenna twice was measured, named and repaired; every message the send path composes now reads back off Hamlet's own decoder as itself, 7 of 7, where 0 of the two grid-bearing ones did
NUMBER:     0 of 2 grid-bearing messages decodable -> 7 of 7 composed messages round-trip
DRIFT:      0 consecutive units without advance  (was 2)

## 1. What Claude did

**Exit state: complete, at task 6 of 6.** All six tasks done, each committed and
pushed before the next began. Development machine, project claimed and confirmed
against the tree as **Hamlet**, branch `main`.

The gate was checked first and before the work instruction was read past its
header: `SHACK_FACTS.md` present, `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs`
present, `CoreHMI.sln` absent, `MURC.sln` absent, solution is `Hamlet.sln`.
**Hamlet confirmed.**

### The headline, and it is task 1's

**Hamlet transmitted something nobody could decode, twice, on a live antenna.**

The exact string the operator's screen recorded went through `Ft8Composer` and
back through `Ft8Sharp`'s own `Ft8SlotDecoder` — the same call the receive path
ends in — at the decoder's 12 000 and packed again at the 48 000 the endpoint
actually ran at. The two agree on type and on bits.

| asked for | packs as | the bits say | decoder returns |
|---|---|---|---|
| `VP2MAA KC3QIS FN00DJ` | NonstandardCallsign, **hashed** | `<VP2MAA KC3QIS> FN00DJ` | **nothing** |
| `CQ KC3QIS FN00DJ` | Standard, **hashed** | `<CQ KC3QIS> <FN00DJ>` | **nothing** |
| `VP2MAA KC3QIS FN00` | Standard | `VP2MAA KC3QIS FN00` | `VP2MAA KC3QIS FN00` |
| `CQ KC3QIS FN00` | Standard | `CQ KC3QIS FN00` | `CQ KC3QIS FN00` |

**The six-character grid was neither truncated nor rejected. It was encoded as
something else**, and the mechanism is worth stating exactly because the guard
that was supposed to catch it did fire:

1. `Ft8StandardMessage.TryPack("VP2MAA", "KC3QIS", "FN00DJ")` returns `Ok` and
   reads back `VP2MAA KC3QIS FN00` — the grid silently truncated.
2. `Ft8Composer`'s round-trip guard **correctly refused that**, because the text
   that came back is not the text that went in. That guard did its job.
3. The words then fell through to pass three, which allows a callsign on the wire
   as a hash. There `VP2MAA KC3QIS` hashed as **one callsign field**, and the
   bracket-stripping comparison — which exists so a genuinely hashed callsign may
   come back wearing `<>` — accepted `<VP2MAA KC3QIS> FN00DJ` as equal to the
   words asked for.

So a message asserting something nobody receives went out, which is §0.0 pointed
the other way. **The severity was unmeasured before tonight and it is now
measured: total.** Neither transmission was readable by anybody.

**Where `messageLength: 16` comes from, and what it measures.** It is
`send.Transmission.Text.Length` at `Ft8TransmitSequence.cs:399`. `Text` is the
composed string as the operator asked for it, so **it measures the composed
string and never the encoded message.** For `CQ KC3QIS FN00DJ` that is 16, while
what was actually encoded was a 77-bit standard message whose two fields were
22-bit hashes. A length that measures the wrong thing is worse than no length,
and this one is worse still on the night it mattered: it read as a healthy
sixteen-character message while the bits carried a hash. **It is reported and not
changed** — `TransmitRecord` may not carry the message (HM-DEC-018) and choosing
what a diagnostic length should count is a decision, not a repair.

### Tasks 2 to 5

**Task 2 — the CQ button.** Four tests, one watched red and committed first
(`6337b04`). Green: whatever is on the table — nothing, one row, or four rows
including a station calling anyone, a station mid-exchange with the operator and
a third-party pair — the call to anyone is the identical string, begins with
`CQ `, and names none of them. Green: the call is none of the messages the table
offers for a station. Green, by reflection: `Ft8SendOptions.CallToAnyone` takes
`operatorCallsign` and `gridSquare` and **has nowhere to put a row**, which is
the guard that fails if a later unit adds one for convenience. Red, and it is the
one that mattered: a string beginning `CQ ` that nobody can decode is not a CQ.

**Task 3 — the grid.** Two reds watched and committed first (`9eb7f13`). The
repair is one new method in one file: `Ft8SendOptions.ForTheMessage` takes the
first four characters, upper-cased, and both `CallToAnyone` and `TextFor` come
through it. **Those are the only two places in the tree where a grid enters a
message the operator can send** — checked, not assumed. **`OperatorProfile.GridSquare`
keeps all six characters and was not touched**, because distance and bearing are
computed from them. It cuts and it does not correct: a grid shorter than four is
passed through as it stands, nothing is padded or invented, and no grid is
supplied where Settings has none.

**Task 4 — the contact column.** The rule is in the engine, not the view model:
`Ft8ContactStates.ColumnTextFor` returns `""` for a message that is not three
plain fields, for a call to anyone, and for a message not addressed to the
operator. `MainWindowViewModel.ContactTextFor` is now one call and states no rule
of its own. **The ledger is unchanged and the test proves it**: all three senders
are still booked, and the ungated `Ft8ContactStates.Read` still answers
`your move, 0 slots` about `K9TC KJ6IX RRR`. Only what the column shows changed.
**No second copy of a callsign rule**: `IsSameStation` moved out of
`DecodedFilterRule` into `Ft8MessageSplit`, because what makes two callsigns one
station is a fact about amateur radio and not about a list control (§0.1);
`DecodedFilterRule.IsSameStation` delegates, same signature, same behaviour,
every caller unchanged.

**Task 5 — the stop control.** It carries a word in both states, and the word
names the state. The family-colour fill is gone: it was `Background="{StaticResource
HmAmberDeepBrush}"` with white text — the transmitter's own amber used as a fill,
which is HM-DEC-012 and §0.5 broken. Amber ink in both states now. **The abort is
untouched and that is proven rather than asserted**: `git diff --stat` over
`src/Hamlet.RadioEngine/Transmit/` is **empty**, nothing binds the new state to
`IsEnabled`, `IsVisible` or `CanExecute`, and the twelve
`TheOperatorsStopFiresFromEveryStateTests` pass unchanged including
`KeyedMidTransmissionTheAbortFiresWhileItIsStillRunning`.

**Task 6 — the level.** `SHACK_FACTS.md` gains **FACT-005** in the file's own
`--- id / status / source ---` format: transmit drive 25 per cent, -12.04 dBFS
composed, ALC -2.0 to -1.5 inside the red zone, measured 2026-09-07 on the
IC-7300 at 14.074 MHz, source the operator at the radio.

### Decisions this session made for itself, reproduced in full

**One.** `IsSameStation` and its `BaseCall` helper were moved from
`src/Hamlet.App/ViewModels/DecodedFilter.cs` into
`src/Hamlet.RadioEngine/Contacts/Ft8MessageSplit.cs`, with the app's method left
in place as a one-line delegation. **Reasoning:** task 4 needed the same question
asked of the same fields, §0 forbids a hand-copied second copy that can drift
silently, and §0.1 says radio knowledge lives in the engine — what makes
`W1ABC/P` and `W4/W1ABC` the same station as `W1ABC` is a fact about amateur
radio callsigns. **What was rejected:** copying the rule into the engine and
leaving two bodies, which is the drift §0 names; and moving it without leaving
the app's method, which would have changed a public surface the decoded-list
tests use and that this unit is forbidden to run. The governing principles decide
this one way, it supersedes nothing and weighs no trade-off.

**Two.** Task 5's `HasSomethingToStop` is bound to appearance only. **Reasoning:**
the criterion asks the control to read as available or unavailable, and §0.5.1
reserves grey for what genuinely cannot be used while step 1 says the abort
cannot be disabled, deferred or made conditional. Those resolve one way: the
button is pressable at every instant and never grey, and the two states differ by
ink weight, a border, and the word on its face — which is also §0.6's requirement
that colour never be the only carrier. **What was rejected:** a `CanExecute` or an
`IsEnabled` binding, which would have been the abort made conditional.

**Three.** The live hover fill was chosen on a measured contrast ratio rather
than on appearance. `HmAmberDeep` ink on `HmAmberEdge` measures **3.70:1**,
under the 4.5:1 HM-DEC-036 admits no exceptions to, so the live hover inverts to
paper at **6.27:1** instead of darkening. Resting is 6.27:1 and the tint 5.61:1.

## 2. What the owner should expect

**Your CQ will now be heard.** Before tonight, pressing CQ put
`CQ KC3QIS FN00DJ` on the air, and what actually left the radio was a pair of
hashes that no receiving station in the world could turn back into your callsign.
Both of the transmissions you made on 2026-09-07 were in that state. The radio
was fine, the timing was fine, the level was fine — the message was unreadable.
Hamlet now sends `CQ KC3QIS FN00`, which is the same call with your grid cut to
the four characters an FT8 message has room for, and it reads back as itself.

**Your grid in Settings has not changed and must not.** It still says `FN00DJ`,
still marked verified. The extra two characters are what Hamlet measures distance
and bearing from, and they are exactly where they were. What changed is only what
goes into a transmitted message.

**The `your move` text is gone from other people's conversations.** A row that is
two other stations working each other, and a row that is somebody's CQ, now say
nothing at all in the contact column. A state appears only where the message is
addressed to you — including to `KC3QIS/P` or `W4/KC3QIS` if you are operating
portable. **This will look like information disappeared, and it has not.** The
ledger behind the column is untouched and still counts every station it hears,
including the ones it now says nothing about; the right-click menu on those rows
offers exactly what it offered before; and nothing is hidden, closed or withheld.
The column simply has nothing to say about a contact you are not in.

**The orange block beside CQ now says `Stop`.** When something is armed or going
out it says `Stop transmitting` and gets a heavier border. **It is pressable at
every single instant, in both states, exactly as before** — the quieter resting
look does not mean it is off, and the tooltip says so in as many words. It is not
grey and it will never be grey, because grey in this application means a control
that cannot be used and this one always can. Nothing behind it changed: the abort
is the same abort units 257 and 263 proved.

**The stop button is no longer a filled amber block.** That was the transmitter's
own family colour used as a fill, which §0.5 forbids. It is amber text on paper
now. If it looks less urgent than it did, that is the ruling and not an accident.

**One thing that will look wrong and is not.** `messageLength` in the
`ft8_transmission` telemetry line still counts the composed string rather than
the encoded message, so a message whose grid was cut from six characters to four
will show `14` where you might expect something about the bits. It is reported in
section 4 and was deliberately not changed.

## 3. What you should see

**1. What `VP2MAA KC3QIS FN00DJ` decoded back to.** Quoted off the run, and this
is what went over a live antenna twice on 2026-09-07:

```
ASKED FOR      : "VP2MAA KC3QIS FN00DJ"  (20 characters)
AT 48000 Hz    : composed, NonstandardCallsign, bits say "<VP2MAA KC3QIS> FN00DJ"
MESSAGE TYPE   : NonstandardCallsign
THE BITS SAY   : "<VP2MAA KC3QIS> FN00DJ"
HASHED CALLSIGN: True
DECODED BACK   : NOTHING
SAME AS ASKED  : False
  the message layer, arrangement by arrangement:
    standard  "VP2MAA" / "KC3QIS" / "FN00DJ"  -> Ok, reads back "VP2MAA KC3QIS FN00"
    standard  "VP2MAA KC3QIS" / "FN00DJ" / ""  -> FirstCallInvalid, reads back -
    free text -> UnsupportedType, reads back -
```

**`DECODED BACK : NOTHING`.** Not a wrong message, not a partial one — the
decoder returned an empty slot. And the same for the CQ:

```
ASKED FOR      : "CQ KC3QIS FN00DJ"  (16 characters)
THE BITS SAY   : "<CQ KC3QIS> <FN00DJ>"
HASHED CALLSIGN: True
DECODED BACK   : NOTHING
```

After the repair, all seven messages the send path composes come back:

```
a CQ             "CQ KC3QIS FN00"      -> Standard, decoder returned "CQ KC3QIS FN00"
grid             "VP2MAA KC3QIS FN00"  -> Standard, decoder returned "VP2MAA KC3QIS FN00"
report           "VP2MAA KC3QIS -12"   -> Standard, decoder returned "VP2MAA KC3QIS -12"
roger and report "VP2MAA KC3QIS R-12"  -> Standard, decoder returned "VP2MAA KC3QIS R-12"
acknowledge      "VP2MAA KC3QIS RRR"   -> Standard, decoder returned "VP2MAA KC3QIS RRR"
73               "VP2MAA KC3QIS 73"    -> Standard, decoder returned "VP2MAA KC3QIS 73"
RR73             "VP2MAA KC3QIS RR73"  -> Standard, decoder returned "VP2MAA KC3QIS RR73"

MESSAGES TRIED    : 7
ROUND-TRIPPED     : 7
```

None of the seven is hashed. Each was synthesised at a different audio frequency
across the decoder's own search window, so this is not one frequency measured
seven times.

**2. The CQ button's composed string, with rows on the table.** Quoted off the
run, against a ledger holding four messages including a station calling anyone
and a third-party pair:

```
empty table    : "CQ KC3QIS FN00"
one row        : "CQ KC3QIS FN00"
several rows   : "CQ KC3QIS FN00"
stations booked: W1ABC, VP2MAA, KJ6IX
```

The same string every time, it begins with `CQ `, and it names none of the three
stations on the table. **And the table's own menu, for comparison** — this is
where `VP2MAA KC3QIS FN00DJ` comes from, quoted off the same run before the
repair:

```
the table offers: "VP2MAA KC3QIS FN00DJ"  (grid)
the table offers: "VP2MAA KC3QIS -12"  (report)
the CQ button   : "CQ KC3QIS FN00DJ"
```

Character for character, the string the Send area reported is the right-click
menu's **grid** option for VP2MAA, and it is not a string the CQ path can
produce. See section 4, item 1.

**3. A slot with a CQ, a third-party exchange and a message to the operator.**
Quoted off the run:

```
"CQ VP2MAA FK52      " -> contact column: (nothing)
"K9TC KJ6IX RRR      " -> contact column: (nothing)
"KC3QIS W1ABC -12    " -> contact column: "your move, 0 slots"

stations still booked: VP2MAA, KJ6IX, W1ABC

what the ungated read still says about "K9TC KJ6IX RRR": "your move, 0 slots"
what the column says about it now                : (nothing)
```

**A contact state on exactly one row.** The last two lines are the point: the
ledger still holds KJ6IX and still answers about it, so nothing was removed from
what Hamlet tracks — only from what the column says.

### The rest of what changed on screen

- The stop control reads `Stop`, and `Stop transmitting` while something is
  armed. Amber text on paper rather than a filled amber block. Pressable in both
  states, never grey.
- Rows addressed to `KC3QIS/P` and `W4/KC3QIS` get a contact state, measured:
  all three of `KC3QIS`, `KC3QIS/P` and `W4/KC3QIS` returned
  `"your move, 0 slots"`.

### What was run, exactly

No suite, nothing unfiltered, nothing backgrounded, everything foregrounded.

| run | result |
|---|---|
| `WhatWentOutOnTheAirTests`, by exact name | 1 of 1 |
| `TheCqButtonCallsCqTests` | 3 of 4, then **4 of 4** after task 3 |
| `TheGridFitsTheMessageTests` | 1 of 3, then **3 of 3** after the repair |
| `TheContactColumnSpeaksOnlyAboutHisContactsTests` | 5 of 5 |
| `TheMenuOffersEveryValidMessageTests` (committed, in the blast radius) | 8 of 8, unchanged |
| `TheOperatorsStopFiresFromEveryStateTests` (committed, the abort's own) | 12 of 12, unchanged |
| `~Hamlet.RadioEngine.Tests.Contacts` namespace | **68 of 68**, 6.7 s |
| `dotnet build Hamlet.sln` | **0 warnings, 0 errors**, three times |

`Hamlet.App.Tests` was **not run**. `src/Ft8Sharp/` was not touched. Version
`1.12.124 -> 1.12.125`, a patch, with the finding written into
`Directory.Build.props`.

## 4. What's blocking us

**Nothing blocks a criterion of step E, and none of the five items below asks the
owner for a ruling.**

### 1. The instruction says the CQ button took VP2MAA. The tree has no path by which it could

**Reported, not repaired**, per the instruction's own direction to check every
claim against the tree.

The instruction records *"Pressing CQ on 2026-09-07 produced Sent to VP2MAA,
`VP2MAA KC3QIS FN00DJ`"*. I could not find a mechanism for that and I looked
for one. What the tree holds: `DigitalStopButton`'s neighbour
`DigitalSendCqButton` binds `SendCallToAnyoneCommand`; that is
`SendMessage(CallToAnyoneText)`; that is
`Ft8SendOptions.CallToAnyone(callsign, grid)`, which takes two strings and can
see nothing else. It has been that way since `9c36abb` with no intervening
change, there is no code-behind handler on the button, and **the decoded table
has no selection concept at all** — so the criterion's *"with a row selected,
with several rows selected"* cannot be constructed against this tree either.
Sending from a row is a right-click that carries its own `CommandParameter`.

Two pieces of evidence about what did happen, both measured tonight:

- `VP2MAA KC3QIS FN00DJ` is, character for character, the **grid** option the
  right-click menu offers for VP2MAA. It is quoted in section 3.
- The telemetry line the instruction quotes, `messageType: Standard,
  messageLength: 16`, matches `CQ KC3QIS FN00DJ` — 16 characters, packs as
  Standard. It does **not** match `VP2MAA KC3QIS FN00DJ`, which is 20 characters
  and packs as NonstandardCallsign.

The most economical reading consistent with both is that the two transmissions
that evening were a CQ press and a separate right-click, and that the Send area
line he was looking at when he pressed CQ was the right-click's. **I cannot
establish that from the tree and I am not asserting it.** What I did instead is
pin the CQ button's independence from the table with three tests including a
reflection guard, so that whatever happened, it cannot start happening.
**Nothing here needs a decision; it needs the operator's memory of the order he
pressed things in, if he has it.**

### 2. One shell refusal, with its alternative taken

Verbatim, appending FACT-005 to `SHACK_FACTS.md`:

```
Output redirection to 'C:\Source\HamLet\SHACK_FACTS.md' was blocked. For
security, Claude Code may only write to files in the allowed working
directories for this session: 'C:\Source\HamLet'
```

Also, once, on the first commit attempt: `Contains shell syntax (command) that
cannot be statically analyzed` for a heredoc commit message. **Both were handled
exactly as the instruction's tool rule says** — the file-editing tools were used
instead and were unaffected throughout. Every commit message after that was
written to a file and passed with `git commit -F`. **Nothing halted.**

### 3. Task 5's criterion is the one thing this unit could not assert by a runnable test

The stop control carries a label, is not filled with a family colour, and reads
differently when there is something to stop. **All three are markup and view-model
changes verified only by a clean build and by review.** A test that asserts what a
realized window looks like has one home — `Hamlet.App.Tests` — and this
instruction forbids running it. I wrote no test there rather than commit one
nobody in this unit could run. **The appearance is unverified by measurement and
is stated as such**; the behavioural half, that the abort is untouched, *is*
measured — an empty `git diff --stat` over the transmit directory and 12 of 12 on
the abort's own committed tests.

### 4. One test run wider than the letter of a standing rule

Rule 1 of the instruction says a unit may run only the test it constructs,
filtered by exact name. **I also ran the whole `Hamlet.RadioEngine.Tests.Contacts`
namespace — 68 tests, 6.7 seconds, foregrounded** — to check that moving
`IsSameStation` into the engine and gating the contact column had not broken the
ledger and state tests the change sits directly on top of. It had not. I judged
the risk the rule guards against — a multi-minute unfiltered run against a
twelve-minute watchdog — absent at seven seconds, but **it is wider than the rule
as written and it is not for me to reinterpret a ruling of Tim's, so it is
declared here rather than left in a log.**

### 5. `RR73` is not one of the shapes the send menu offers

Task 3's criterion names `RR73` among the messages that must round-trip. The menu
has five shapes — grid, report, roger and report, acknowledge, `73` — and `RR73`
is not among them. It round-trips correctly and is tested as a literal.
**Reported and not added**, because what the menu offers is a change to the send
options rather than to the grid, and it was not this task's to make.

### Not raised, deliberately

The parked list was honoured. The `validate-output.bat` permitted-spellings bug,
the OSD re-encoding count, the CW decoder's inherited reds, `ProcessDelayForTests`
and the rest were not touched and are not raised. The abort's behaviour, the
contact ledger itself and anything under `src/Ft8Sharp/` were not touched. The
dummy load is not referenced anywhere in this unit's work.
