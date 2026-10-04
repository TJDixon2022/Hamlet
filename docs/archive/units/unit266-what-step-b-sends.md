# What step B may send, in each of the four states

**Work instruction 266, task 5. Named, not built.** No menu is built here, no send
path is added, and nothing in `src/` was changed for this file. It is the list step
B works from.

Everything below is read off the tree as it stands, with a file and a line for each
claim: `src/Hamlet.RadioEngine/Contacts/Ft8SendOptions.cs` and
`src/Hamlet.RadioEngine/Contacts/Ft8ContactState.cs`.

---

## 1. The headline, and it is not what the question expects

**The contact state does not decide which messages are valid. It never has, and it
must not start.**

`Ft8SendOptions.For` at `Ft8SendOptions.cs:99` takes the station's record, the
operator's callsign, his grid and the measured report. **It does not call
`Ft8ContactStates.Read` and does not call `Ft8ContactStates.IsComplete`.** It walks
`InExchangeOrder` at `:83` - grid, report, roger and report, acknowledge, `73` -
and offers every one of the five whose text it can build.

So the answer to *which messages are valid in each of the four states* is, in all
four: **all five.**

**That is a ruling, not an accident**, and `Ft8SendOptions.cs:56-61` records the
day it was watched failing the other way. Built first to return only the message
that conventionally comes next, `K9RST` - complete at slot 5 of the band scene -
came back with an empty list. No `73`, and no way to send his grid again when the
first was lost. *Expected: 5, Actual: 0.* **A contact is never closed by the app,
and that version closed one.**

**What the state decides is which one is highlighted.** That is all it decides, and
section 3 is the table.

---

## 2. The five messages, and the two that can be absent

Toward a station `W9GAP`, from `KC3QIS` at `FN00` with a report of -11 measured,
built by `TextFor` at `Ft8SendOptions.cs:201`:

| Shape | Label | On the air |
|---|---|---|
| `Grid` | grid | `W9GAP KC3QIS FN00` |
| `Report` | report | `W9GAP KC3QIS -11` |
| `RogerAndReport` | roger and report | `W9GAP KC3QIS R-11` |
| `Acknowledge` | acknowledge | `W9GAP KC3QIS RRR` |
| `Seventy3` | 73 | `W9GAP KC3QIS 73` |

**Two of the five can be missing, and missing is not forbidden.** `TextFor` returns
null - so the option is not in the list - in exactly two cases, and
`Ft8SendOptions.cs:126-138` says so out loud in the menu's `Absent` list rather
than leaving a gap:

- **No grid in Settings** - the grid message is absent, because there is no grid to
  put in it. *the grid square is not set in Settings, so the messages that carry
  one are not offered.*
- **No report measured for this station** - the report and the roger-and-report are
  absent for the same reason. *no signal report has been measured for this station,
  so the messages that carry one are not offered.*

**There is no message to send, rather than a message withheld.** The distinction is
the whole of it: step B must render an absent option as a stated reason, never as a
greyed-out row, because a greyed-out row is the app telling the operator no.

**And a repeat is correct operating.** `Ft8SendOption.SentBefore` at
`Ft8SendOptions.cs:29` counts how many times that exact text has already gone to
that station (`AlreadySent`, `:144`). FT8 loses transmissions constantly, so
sending the grid a second time is what an operator does. **The count is shown
beside the option - *grid, 2nd time* - and is never a reason to remove it.**

---

## 3. The mapping step B builds the menu from

The highlight is `Ft8SendOption.IsExpected`, set at `Ft8SendOptions.cs:122` from
`Expect` at `:177`. **`Expect` reads one thing: the last message heard from that
station addressed to the operator** (`LastHeardToUs`). Not the state, not what the
operator has already replied, and - the remark at `:168-176` is explicit -
**never completeness**.

So the rule per state is a rule about what he last said, and the state is how that
same fact reads on the row:

| The row says | Because | Valid to send | Highlighted | The rule that decides it |
|---|---|---|---|---|
| **your move** | he spoke to us last and nothing has gone back | all five, less any absent | **whatever answers his last message to us** - see the four rows below | `Expect`, `:177`, on `LastHeardToUs` |
| **waiting on him** | we spoke last and he has not come back | all five, less any absent | **the same one as before, now carrying its count** | `Expect` does not consult what we sent, so the answer to his last message is still the answer - and it is a repeat, which is correct |
| **complete** | the exchange has what a QSO needs | all five, less any absent | usually **`73`**, because his last was an acknowledgement | `Expect`, unchanged. Completeness is not consulted anywhere in the menu |
| **gone quiet** | nothing heard from him for a stated count of slots | all five, less any absent | **whatever answered his last message**, however old | silence changes nothing about what he last said |

**The four highlights `Expect` can return**, all of them decided by the shape of his
payload field and by nothing else (`Ft8SendOptions.cs:186-198`):

| His last message to us | Highlighted next | Line |
|---|---|---|
| nothing to us, but he was heard calling anyone (`CQ W9GAP DM79`) | **grid** - which is how an operator takes a CQ | `:181-184` |
| a grid (`KC3QIS W9GAP DM79`) | **report** | `:198` |
| a report with no roger (`KC3QIS W9GAP -11`) | **roger and report** | `:193-196` |
| a report carrying its roger (`KC3QIS W9GAP R-11`) | **acknowledge** | `:193-196` |
| a courtesy (`RRR`, `RR73`, `R73`, `73`) | **`73`** | `:188-191` |
| never heard from at all | **none** - nothing conventionally comes next, and nothing is marked | `:181-183` |

**Nothing is interpreted** (§12.1). A grid is four Maidenhead characters, a report
carries its sign, a courtesy is one of four words. That is arithmetic about the
format's field shapes; it never says what a station meant.

---

## 4. The one message that is not in this list

**`CQ`.** It is not a message toward a station, so it is not in `For`. It is built
by `Ft8SendOptions.CallToAnyone` at `:152` from the operator's own callsign and
grid, and step B's exit criterion gives it a button of its own rather than a row in
a menu. With no grid set it is `CQ KC3QIS`, not a refusal.

---

## 5. What step B must not take from this

- **Do not gate the menu on the state.** The four states are what the row *says*;
  they are not a permission. Every option is offered in every state.
- **Do not remove a repeat.** Show its count.
- **Do not grey out an absent option.** Say why there is nothing to send.
- **Do not require `73`, and do not stop offering it once it has gone.**
- **One click, one transmission**, and the abort is not to be weakened, made
  conditional or routed around.
