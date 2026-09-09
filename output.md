# Work instruction 297 — the conversation becomes cards

**READ IN THIS ORDER.** The panel he could not follow is now cards, so section 3 is
what he will see.

A. **The phase goal — FT4 works exactly the way FT8 does.** This unit advanced no
   step of it. It rebuilt the For you panel, which is what step 4's second criterion
   is written about, and that criterion now describes a different panel.

B. **Step 4 and its exit criteria** — pressing FT4 tunes and decodes; the panel, the
   conversation, the ring, the filters, the tooltips, the ledger and the right-click
   menu all work unchanged; one click, one transmission; a whole exchange from one
   right click at the bench. **None was measured tonight**, so step 4 stays `partial`
   exactly where unit 296 left it.

C. **The report last, and section 4 raises 3 items.** None blocks the next unit from
   starting. Two are wordings Tim has not ruled on and one is a defect in a test seam
   that this unit worked around rather than widened.

```
UNIT:       297 — complete at task 6 of 6, none dropped — 2026-09-09 16:35
PHASE GOAL: FT4 works exactly the way FT8 does.
UNIT GOAL:  Each station he is in contact with becomes a card saying what happened,
            how long ago and what to do next, readable with no radio knowledge, with
            the technical detail one deliberate hover away.
ADVANCED:   no — no phase step moved. Step 4 stays partial: nothing was tuned,
            nothing transmitted, no exchange run at the bench. This unit re-cut the
            panel that step 4's second criterion is about.
NUMBER:     how many of the four states produce a sentence every clause of which
            traces to a ledger fact
            0 of 4 -> 4 of 4
            The panel had no sentences at all before tonight; it had messages.
DRIFT:      1 consecutive unit without advance  (was 0, carried from unit 296)
```

---

## 1. What Claude did

**Complete. Six tasks of six, none dropped, including the named drop candidate.**
Development machine, prompt claimed `PROJECT: Hamlet`, branch `main`, eight commits,
all pushed. Root version 1.12.259 to **1.12.260**, bumped once. `Ft8Sharp` and
`Ft8Sharp.Deep` do not move; **no file under `src/Ft8Sharp/` was touched.**

The gate held: `SHACK_FACTS.md` present, `CwProbabilisticDecoder.cs` present, no
`CoreHMI.sln`, no `MURC.sln`. This unit was numbered **297** from `PHASE_OUTCOME.md`
as the instruction directs; the last entry there was 296.

**No test suite was run.** Twenty tests were constructed in this instruction, across
three classes, and run filtered by exact name, foregrounded, with a 500-second
timeout: **20 of 20 green.** Every `dotnet build` was foregrounded. Nothing was
backgrounded and nothing was polled.

**Task 1 — reading only.** The whole finding is in
`docs/unit297-cards.md`. The ledger holds three never-overwritten lists per station,
each entry carrying the raw text, its split fields and its slot boundary, so almost
everything a card needs is already a function of what is there. **Seven things it
cannot support are named**, and the sharpest is in section 3.

**Task 2 — the panel is cards.** The interleaved list, the waiting strip and the
standalone turn ring all came out together. The seam is where §0.1 puts it:
`Ft8CardFacts` in the engine derives what passed and composes no words;
`Ft8ContactCard` in the shell composes words and reads no ledger.

**Task 3 — the `i` hover.** `HintKind` gains a fourth kind whose glyph is the `i` Tim
asked for. One mark, not a second control.

**Task 4 — clearing.** The X removes a card and touches nothing else. The one
hazardous case warns once and clears on the second press.

**Task 5 — the trace.** A ledger built in each state, each sentence quoted, each
clause traced. **The measurement corrected the test twice and the card neither time.**

**Task 6 — the outcome entry.** `outcome-append.bat` ran clean and exited 0.

**Four decisions made for themselves, each a sizing call and none a trade-off.**

1. **The waiting strip and the standalone ring were removed rather than left beside
   the cards.** The strip existed only because the panel drew one conversation and
   everybody else had to be visible somehow; every station has a card now, so a
   second list would be this one with less on it. The ring moved to beside the button
   that needs it, which is what the ruling asks for. Two lists of the same stations
   and two rings differ only in which one is stale.
2. **The card ring binds only the two station-independent classes** — unknown, and on
   air — and not *his slot* or *theirs*. Seconds to the next boundary is one fact for
   every card; the parity belongs to the conversation station, and drawing it on
   another station's card would be a claim about a station it was not measured from.
3. **A narrow `RecordSentForTests` was added rather than widening
   `AddSentRowForTests`**, which four test files call and two of which are sweeps that
   count how much text the application shows — tests HM-DEC-155 forbids this unit to
   run. Reported in section 4.
4. **`DigitalCardsIdle` was split three ways**, so an empty panel with no measured
   clock stops saying *nothing addressed to you yet* while stations sit in its own
   ledger.

**Four mismatches against the instruction, reported and not repaired.**

1. **The ruling's own example, `Arizona, 2,100 mi`, is unreachable.**
   `DxccPrefixes.EntityOf` names a DXCC entity — a country — and
   `EntityQualifier.Describe` adds only *northern* or *southern*. There is no state,
   province or town anywhere in this tree. The face says `the United States · 320
   miles` instead. The distance half of his example is exactly right and is built.
2. **`Ft8ContactLedger.cs:158` says in bold that nothing calls `RecordSent`.** It has
   a call site at `MainWindowViewModel.cs:10820`, and `Ft8ContactLogEntry.cs:66` says
   so in as many words. That is HM-DEC-159's shape: a file stating something its own
   tree contradicts.
3. **`WORK_INSTRUCTIONS.md`'s heading carries no unit number.** It reads `# Work
   instruction - the conversation becomes cards`, and tells the session to take the
   next number from `PHASE_OUTCOME.md`. But `outcome-append.bat` resolves the number
   from that heading — unit 266's repair, and it is right — so it fell back to 296 and
   filed this entry as unit 296. **The instruction was not repaired.** The entry's own
   body already carried `UNIT_AS_CALLED: 297`; its heading is corrected in place to
   `## UNIT 297 - STEP 4`, with the correction stated on its face, rather than
   appending a second entry that would describe one night twice.
4. **The instruction says unit 274's Log dialog is populated from the ledger and that
   the card's Log button is that dialog.** It is, and it reaches it through
   `LogContactAsync` unchanged — but that method takes a `DigitalDecodeRow`, so the
   card finds the newest row that station addressed to the operator and hands it over.
   A station the operator called that has never come back has no such row and so no
   Log button, which is correct: there is no contact to log.

**And one thing this unit swept in that is not its own.** `git add -A tests/` in task
4 committed `tests/Ft8Sharp.Tests/Unit289SourceProbe.cs`, which had been sitting
untracked since unit 289. It builds and it is harmless, and it is named here rather
than deleted, because removing another unit's file is worse than committing it.

## 2. What the owner should expect

**The For you panel is not a list of messages any more.** Each station he is working
is a card that says, in a sentence with no radio words in it, who came back and when
and what to do next — with one button on it that does that thing, a countdown ring
beside the button telling him how long he has to press it, an `i` to hover if he wants
the numbers, and an X to take the card away.

**Nothing is hidden.** Every message is still there behind *show the N messages*, and
every decibel, hertz, time offset, bearing and courtesy token the face gave up is on
the hover. A sweep asserts both halves: nothing nerdy on any face, and all of it in
the hover, so deleting the facts cannot pass.

**What will look wrong and is not.**

- **The waiting strip is gone and the ring has moved.** Both were replaced rather than
  removed: every station that was on the strip has a card of its own, and the ring
  now sits beside a send button so its seconds mean *how long to press this*.
- **A station calling CQ gets no card.** That is correct and it is asserted — a card
  is a station he is in **contact** with, and a CQ is an invitation. Callers are on
  the decoded list to the left, where answering one starts from.
- **A card in `Gone quiet` is drawn faded.** It has not been removed and never will
  be on its own; the word says the same thing, so it survives grayscale.
- **The first press of the X on a `Finished` card that is not in the log does not
  clear it.** It says what is about to be lost. The second press always clears.
- **`PHASE_STATUS.md` still names unit 296 and step 1.** The launcher owns that file
  and task 6 did not name it; `PHASE_OUTCOME.md`'s header is the one the script keeps
  current, and it reads step 4 `partial`.
- **The suite was not run and its state is unknown to this unit.** The four inherited
  reds named in the instruction were not chased and not looked at.

## 3. What you should see

### 1. A card in each of the four states, quoted whole

These are printed by `Unit297CardSentenceTests` itself, so what follows is the test's
own output rather than a transcription of it.

```
K9XP   the United States   [Your turn]
  K9XP came back to you, and it is your turn to answer him.
  02:11:15 UTC · 45 seconds ago
  [Confirm, and tell him how he is coming through]  (ring)   show the 2 messages

K9XP   the United States   [Waiting on him]
  You answered K9XP and he has not come back yet.
  02:11:30 UTC · 15 seconds ago
  [Send it again]  (ring)   show the 3 messages

K9XP   the United States · 320 miles   [Finished]
  You and K9XP got through to each other and you both confirmed it.
  02:11:45 UTC · 15 seconds ago
  [Log this contact]   show the 4 messages

K9XP   the United States   [Gone quiet]
  Nothing more has been heard from K9XP. He may have moved on.
  02:11:15 UTC · a minute ago
  [Send it again]  (ring)   show the 2 messages
```

The same finished exchange ending `RR73` instead of `RRR` reads **You and K9XP got
through to each other and you both confirmed it. He said goodbye.**

### 2. The trace: every clause against the fact it came from

| Clause | Fact | Where |
|---|---|---|
| `K9XP` | `Ft8CardFacts.Callsign` | the ledger's own key |
| *came back to you* | `HeCameBack` = `LastHeardToUs is not null` | cannot be satisfied by a CQ, by construction |
| *it is your turn* | `State == YourMove` | `Ft8ContactStates.Read` |
| *You answered … and he has not come back* | `YouCalledHim` and `State == WaitingOnHim` | `Sent.Count > 0`, and `Read` |
| *each told the other how you were coming through* | `ReportsBothWays` | last report each way over `HeardToUs` and `Sent` |
| *got through to each other … you both confirmed it* | `State == Complete` | `IsComplete`: both calls, a grid or report each way, an acknowledgement each way |
| *He said goodbye* | `HeSignedOff` | **`Ft8MessageSplit.IsSignOff` over what he actually sent** |
| *Nothing more has been heard* | `State == GoneQuiet` | `SlotsSinceHeard` against four slots, counted over **everything** he transmitted |
| `the United States` | `DxccPrefixes.EntityOf` | the callsign, never the grid |
| `320 miles` | `GridPath.MilesBetween` | his grid and the operator's |
| `02:11:15 UTC` | `Ft8CardFacts.LastAtUtc` | the slot boundary the decode carried |
| `45 seconds ago` | that boundary against corrected now | absent with no measured clock |
| *show the 2 messages* | `HisMessages + YourMessages` | this conversation, never everything heard from him |
| `Confirm, and tell him…` | `Ft8SendOption.IsExpected` | the engine's own answer since 2026-09-06 |

**Not one clause failed to trace, and the one that could have been confidently wrong
is the goodbye.** `Ft8ContactStates.IsComplete` counts `RRR` as an acknowledgement and
says in its own remarks that `73`'s absence never withholds completeness — correctly,
because a contact that ends `RRR` is a contact. So reading the farewell off the
`Complete` state would have had this card announcing a goodbye nobody sent, on an
ordinary way for an FT8 contact to end. It is its own shape test now, and
`TheFinishedSentenceNeverInventsAGoodbye` is that case.

**Two clauses were corrected by measurement, and the card was right both times.** The
first draft of the test expected the Finished sentence to name both reports; over a
grid-then-report exchange only one report passed, and the card said *got through to
each other* rather than claiming a swap. The second expected a card for a bare CQ;
there is none, and there should not be.

### 3. The `i` hover on a your-turn card, quoted whole

> He hears you at -9 dB, and you have not told him how he is coming through yet. Those
> are decibels against the noise, so a minus number is the ordinary case here: this
> decoder reads down to about -21, and anything well above that is a comfortable
> signal rather than a marginal one. He has not put a grid square on the air, so
> Hamlet has no way to say how far away he is. His tone sat 1240 Hz up inside the
> receiver's passband while the dial was on 14.074000 MHz. Everybody on the band
> shares one dial setting and takes a different slice of the audio, which is how
> dozens of stations fit where one voice would go. His transmission began 0.2 seconds
> into the slot. Both clocks have to agree within about a second for this to decode at
> all, so a small number here is the two of you keeping the same time. This ran from
> 02:11:00 to 02:11:15 UTC. That is 3 slots ago, counted in the transmit-and-listen
> turns the band runs on rather than in seconds. The last thing he sent you was -09,
> which is how well he is hearing you, in decibels against the noise.

**Every clause carries its reason and no clause is a bare number.** Where a fact is
missing the clause says so — *he has not put a grid square on the air* — rather than
being dropped silently or filled in.

**The decode floor is FT8's and FT4 gets none.** The sensitivity phase measured FT8's
whole curve against a published -21 dB; unit 288 found upstream states no FT4
equivalent and nothing in this tree does either. On FT4 the paragraph teaches the sign
and the direction and invents no number.

### 4. Clearing a finished, unlogged card

The first press does not clear it. The card says:

> You finished this contact with K9XP and it is not in your log. Clearing the card
> lets it go, and Hamlet keeps no record of it anywhere else. Log it first if you want
> it, or press the X again to clear it anyway.

The second press clears it. **It warns and never refuses**: the X is never greyed and
the second press always works.

**Whether *this* contact is logged is decided against the log entry's own start and
never by the callsign.** `_workedBefore` holds the newest entry per callsign, so a
station worked last month would have read as logged and tonight's finished contact
would have gone without a word. That case is its own test,
`AnOlderContactWithTheSameStationDoesNotCountAsThisOne`. Where it cannot tell — no
start recorded — it warns, because a warning nobody needed costs one press and a
silence that was wrong costs the contact.

**And clearing touches nothing else**, asserted: no write to the log, every message
still in the ledger, and the station's card back the moment it transmits again.

## 4. What's blocking us

Nothing blocks the next unit. Three items want a ruling or a decision, in the order
they matter.

---

**Hamlet cannot name a place finer than a country, and the card says `the United
States · 320 miles` where the ruling's example says `Arizona, 2,100 mi` — either the
example stands as a country plus a distance, or naming a state becomes work with a
citation behind it.**

`DxccPrefixes.EntityOf` answers with a DXCC entity and `EntityQualifier.Describe` adds
only *northern* or *southern* where the entity is tall enough for the word to mean
something. **Nothing in this tree holds a state, a province or a town**, and the
callook owner-and-town lookup is parked to its own instruction by this order.

What was rejected. **Deriving a US state from the callsign prefix**, which is wrong:
US call areas are historical and an operator keeps his callsign when he moves, so
`W6` in Ohio is ordinary. **Deriving one from the four-character grid**, which is a box
about seventy miles across that straddles borders, and would need a cited
grid-to-state table this repository does not have. **Wording round it** — *somewhere in
the United States* — which is longer and says less.

What would settle it. The parked callook instruction may bring a town with a source
behind it, which is the honest route to `Arizona`. Until then the face carries the
country and the distance, which is what the tree can support.

---

**A wording exists that no card can reach today: `K9XP is calling and nobody has
answered him yet`.**

`Ft8ContactStates.Read` documents a `YourMove` case for a station heard calling anyone
and not yet answered, and `Ft8ContactCard` words it separately from *he came back to
you* — they are different news and one wording for both would tell him a stranger had
answered a call he never made. **But a CQ is not addressed to the operator, so it
never enters the mine side and never becomes a card**, which
`ABareCallToAnyoneIsNotACard` asserts.

The branch is kept rather than deleted, because `Read` documents that state and a card
built from `Ft8CardFacts` elsewhere would reach it. **Whether an unanswered caller
should get a card is Tim's**: it would make the panel a list of everybody calling him
rather than of contacts he is in, which is a different panel from the one he ruled on.

What was rejected. **Deleting the branch**, which would leave `Read`'s documented state
with no wording and the next unit to reach it inventing one. **Putting callers on
cards on this unit's own authority**, which is a scope decision his ruling did not
make.

---

**`AddSentRowForTests` says in its own remarks that it is *the same door the send path
uses* and it is half of one, so no test in this repository can reach a finished
contact state through it.**

The send path does two things on adjacent lines: it keeps the panel row **and** it
tells the ledger. The hook does only the first, so `Ft8StationRecord.Sent` is empty in
every test that uses it and `Ft8ContactStates.IsComplete` can never be satisfied. **This
unit found it by measurement**: its first fixture read *Your turn* over an exchange
that had everything a QSO needs.

A narrow `RecordSentForTests` was added beside it and this unit's fixtures call both.

What was rejected, and it is the item that wants a decision. **Widening the existing
hook**, which is the right repair and was not done here: four test files call it, two
of them sweeps that count how much text the application puts on screen, and booking
the ledger inside it would put cards on a panel those sweeps count. Under HM-DEC-155
this unit may run only the tests it wrote, so it cannot see what that would break.
**A unit that may run those four files should widen it and delete the narrow hook.**
