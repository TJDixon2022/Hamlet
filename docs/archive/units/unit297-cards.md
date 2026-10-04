# Unit 297 - the conversation becomes cards

Work instruction 297. The panel stops being a list of messages and becomes one card
per station.

**The rule underneath everything below:** the face of a card needs no radio
knowledge, and every sentence on it must be derivable from the ledger.

---

## Task 1 - what the ledger can say without inventing anything

**Reading only.** Nothing was built in this task.

### What the ledger actually holds

`Ft8ContactLedger` (`src/Hamlet.RadioEngine/Contacts/Ft8ContactLedger.cs`) keeps one
`Ft8StationRecord` per station, and each record holds **three lists, never
overwritten**:

| List | What is in it | Line |
|---|---|---|
| `Heard` | every message from that station, **whoever it was addressed to** | `:39` |
| `HeardToUs` | the subset addressed to the operator | `:52` |
| `Sent` | every message the operator sent **to that station** | `:55` |

Each entry is an `Ft8LedgerMessage` (`:9`): the **raw text exactly as it passed**, its
three split fields (`To`, `From`, `Payload`) or null where the splitter refused it,
and the **UTC boundary of the slot it occupied**. Nothing is summarized and nothing is
discarded, so anything below that is a function of those three lists is derivable.

Both doors are live. `RecordHeard` is called at `MainWindowViewModel.cs:9398` and
`RecordSent` at `:10820`, on the send path, from what actually went out.

> **Mismatch, reported and not repaired.** `Ft8ContactLedger.cs:158` still says in
> bold, in its own remarks, **"Nothing calls `RecordSent` today"**. It has a call site
> and `Ft8ContactLogEntry.cs:66-68` says so in as many words. This is HM-DEC-159's
> shape: a file stating something its own tree contradicts, which the next session
> reads and believes.

### The four states

They are `Ft8ContactState` (`Ft8ContactState.cs:11-24`) and `Ft8ContactStates.Read`
(`:138`) decides between them in a fixed order: **Complete, then Gone quiet, then
whose turn**. The order is the load-bearing part - a complete exchange stays complete
however long the silence afterwards runs.

### That he answered your call - **yes, in one precise sense, and no in another**

**Derivable.** `HeardToUs` is exactly the messages from him addressed to the operator,
and `LastHeardToUs` is the newest. A CQ is not in that list, by construction
(`Ft8ContactLedger.cs:48-50`), so a station calling anyone never reads as an answer.
`Ft8MessageSplit.IsAddressedTo` is the one predicate that decides it and there is no
second copy (`Ft8MessageSplit.cs:196`), and compound and portable forms of the
operator's own call count as his (`IsSameStation`, `:117`).

**So the card may honestly say: *he came back to you*.** That is bookkeeping over
addressees.

**Not derivable: that a given message is a reply to a given earlier one.** The ledger
has no threading - only ordering by slot boundary. Two messages from him with one of
ours in between is all the structure there is, and *he answered your call* in the
stronger sense of *this message responds to that one* is a reading of intent, which
§12.1 forbids.

**And one gap that is easy to miss:** `Sent` is only what **Hamlet** transmitted. A
station the operator answered on another program has an empty `Sent`, so *your call*
has no record at all and the card must not assert one.

### That reports were exchanged both ways - **yes, with the numbers**

`Ft8MessageSplit.IsReport` (`:229`) reads a payload's shape and hands back the sign,
the decibels and whether it carried a leading `R`. `Ft8ContactLogEntry.For` already
takes **the last report each way** - `ReportReceived` off `HeardToUs`, `ReportSent`
off `Sent` (`:105-106`) - and its remarks record why it must never read `Heard`: a
report inside a message to a third station is what he heard **that** station at.

**So the card may say the reports were swapped, and the hover may carry both
numbers.** The face may not: no `dB` on the face.

### That he signed off - **`73` and `RR73` are distinguishable, and this is where the danger is**

**Distinguishable, yes.** The raw payload is kept verbatim and `IsCourtesy`
(`Ft8ContactState.cs:340`) tests the exact strings `RRR`, `RR73`, `R73` and `73`. So a
card can tell `RRR` from `RR73` from `73` by reading the payload it already holds. The
shape test is a fact about the format, not a reading.

**And `RRR` is not a sign-off.** It is a roger. `RR73`, `R73` and `73` end with `73`
and are; `RRR` does not and is not.

> **This is the sharpest §0.0 exposure in the unit, and it is not the one the
> instruction expected.** `IsComplete` (`:290`) **does not require a sign-off at all**,
> and its own remarks say so: *"`73` is not on the list and its absence never withholds
> complete."* Complete means both callsigns, a grid or report each way, and an
> acknowledgement each way - where `IsAcknowledgement` (`:334`) counts **`RRR` as an
> acknowledgement**.
>
> So **a card in the Complete state that says *he signed off* is wrong every time the
> exchange ended `RRR`**, which is an ordinary way for an FT8 contact to end. The
> honest Complete sentence is about **what was exchanged**, not about a farewell -
> and if the card wants to mention a sign-off it must test the payload for one
> separately rather than infer it from the state.

### How long ago - **both, and both need the clock**

- **In slots:** `Ft8StationRecord.SlotsAgo(then, now, grid)` (`:113`), which is
  `SlotGrid.BoundariesBetween` filtered to boundaries strictly after `then`. **One
  implementation, and the grid is a parameter with no default** - work instruction 294
  removed the second copy that had been dividing by FT8's fifteen seconds while the
  tab ran FT4.
- **In seconds and as a wall time:** directly, because `SlotStartUtc` is a real UTC
  moment. `14:06:15 UTC` is that value formatted; *30 seconds ago* is
  `now - SlotStartUtc`.

**The condition on all of it:** the relative half needs a measured clock offset.
`Ft8Slots.TrueUtc(DateTime.UtcNow, ClockOffset)` returns null where none has been
taken, and `MainWindowViewModel.Quiet` (`:2529`) already returns `""` in that case
rather than counting from a reading nobody took. **A card with no clock offset can
still print the slot's UTC** - that came off the decode - **and must not print a
relative age.**

### That nothing has come back, and for how long - **yes, and the clock is the right one**

`SlotsSinceHeard(now, grid)` (`:71`) runs off `LastHeard`, which is **every**
transmission from that station whoever it was addressed to. `Ft8ContactState.cs:69-77`
argues why, and the argument is worth keeping in view when wording the card: **a
station working three others at once is transmitting constantly while answering us
rarely.** *Gone quiet* means he has stopped transmitting at all, not that he has
stopped answering.

The threshold is `GoneQuietAfterSlots = 4` (`:104`), and its remark says plainly that
**this is a choice and not a specification** - no document in the repository fixes it.
Four slots is two of the station's own transmission opportunities missed: sixty
seconds on FT8, thirty on FT4.

### Said plainly: what the ledger cannot support

Seven things. A card must not say any of them.

1. **A US state, a province or a town.** `DxccPrefixes.EntityOf` names a **DXCC
   entity**, which is a country, and `EntityQualifier.Describe` (`:64`) adds only
   `northern` or `southern` where the entity is tall enough for the word to mean
   something. **There is nothing finer anywhere in the tree**, and the callook
   owner-and-town lookup is parked to its own instruction.

   > **Mismatch, reported and not repaired.** The ruling's own example is
   > **`Arizona, 2,100 mi`**. Hamlet cannot say Arizona. What it can say is
   > `United States - 2,100 miles`, or `northern Italy - 4,100 miles`. The distance
   > half of the example is exactly right and available.

2. **A distance where either grid is missing.** The station's grid is read off a
   message he actually sent (`Ft8ContactLogEntry.LastGrid`, over `Heard`), so a
   station that sent only reports and courtesies has none; the operator's comes from
   Settings. `Ft8Vocabulary.WhereTheyAre` (`:252-255`) already has the branch that
   says so instead of guessing.

3. **Which message answered which.** No threading, as above.

4. **That the contact is over.** Complete is a count of field shapes and nothing
   closes a contact in this application - `Ft8ContactStates`' own header says it
   reports and does not rule.

5. **That *this* contact has been logged.** `_workedBefore`
   (`MainWindowViewModel.cs:10285`) is the ADIF log read back **keyed by callsign**,
   so it answers *you have logged a contact with this station* and never *you have
   logged this one*. A second contact with the same station on another day reads as
   already logged. **Task 4's hazard has to be worded against that**, because the
   thing it must not do is tell him a contact is safely written down when the entry
   it found was last week's.

6. **Why a station went quiet.** Nothing about propagation, his equipment or his
   attention is in the ledger, and `GoneQuiet` is deliberately a count and never a
   verdict.

7. **The operator's own report where he answered elsewhere.** `ReportSent` stays null
   by design (`Ft8ContactLogEntry.cs:64-67`): an offer in a menu is not a
   transmission.

### What this means for tasks 2, 3 and 5

Every sentence a card needs is available except a sign-off, which needs its own
payload test rather than the Complete state, and a place name finer than a country,
which does not exist. The face gets: **who, where in the world, how far, the state
word, what happened in plain words, the UTC and the relative age.** The `i` hover
gets: **the reports both ways with their decibels, the grid, the bearing, the
frequency, the slot times, and which payload closed the exchange.**
