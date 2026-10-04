# What this unit removed, and where it went — work instruction 280, task 6

**Removing words can remove facts, and the only defence is a list somebody can
read.** This is that list. Written after tasks 7 and 8 rather than before them,
because a list of removals written before the last two removals would have been
wrong by construction; the departure from the numbering is reported in section 1 of
`output.md`.

**The measure this unit is judged by**: permanently-visible characters on the
Digital tab, walked off the realized window rather than counted in source.

| | |
|---|---|
| Before | **1,367** across 43 text blocks |
| After | **529** across 42 |

---

## Moved to hover, nothing lost

| Was | Chars | Now on screen | Where the words went |
|---|---|---|---|
| *How hard Hamlet drives the radio's input — -12.0 dBFS at this setting. This is a starting point, not a specification. Set it against your own radio's ALC meter: turn it up until the ALC just begins to move and then back off. Full scale is a wide, distorted signal over other people's band, which is why Hamlet starts at 25 %.* | 325 | `-12.0 dBFS` | `TransmitDrive.Tip`, behind the Radio tips mark beside the control |
| *Nothing has been sent, so there is no contact to report on. After a transmission this line says where that contact stands — the station, the state in the same words the table's Contact column uses, the slot count, and the slot it was read at.* | 242 | `no contact yet` | `MainWindowViewModel.ContactStandsIdleTip`, on the line's own hover |
| *Nothing has been transmitted yet, so there is no measured level. After a send this line says what the sound card was actually handed.* | 133 | `no level measured yet` | `TransmitLevelIdleTip`. **The §0.0 boundary is kept verbatim**: *what the sound card was actually handed* is a statement about what Hamlet can and cannot observe |
| *Nothing has been sent. Right-click a decoded row to choose a message, or press CQ.* | 82 | `nothing sent yet` | `SendIdleTip`. For somebody who has never worked a station the how-to is the difference between a quiet screen and a usable one, so it is a hover and not a deletion |
| *clock is 0.09 s slow, checked just now* | 38 | `◷` | the same string, on the glyph's hover. **A clock that is not fine still gets its words**, because that is a fault |

## Replaced by a shape, nothing lost

| Was | Now |
|---|---|
| *You are transmitting, with 2 seconds of it left. This slot is yours and you are using it, so there is nothing to send into until it finishes.* — and the three other turn sentences | A ring with the count inside it and a caption of at most three words. The four states separate in grayscale by dash pattern, stroke thickness and caption |
| `x2` inside the message text | *heard twice* under the message. **This one is a correction as much as a shortening**: `x2` sat inside the message string, where it read as part of what the station transmitted, and the message is the one thing on that row that must be exactly what went out |
| The For you column header, `utc | message` | Nothing. That side is a conversation of bubbles, not a table, so the header labelled columns that no longer exist |

## Removed outright, and why each was not a fact

| Was | Why it is not a fact lost |
|---|---|
| *your move, 0 slots*, under a received message in the conversation | The turn ring above the conversation says whose move it is, and says it better. This was a leftover from when the contact state was a column of a table. **Two places saying whose move it is disagree the first time one of them is wrong.** |
| *…was decoded and the search found no place in it that looked like the start of an FT8 transmission, so nothing reached the decoder at all* on a slot he transmitted in | **It was never true.** Hamlet suspends decoding while transmitting (HM-DEC-147), so that slot was not searched. The sentence asserted a search result nobody measured, which is §0.0 broken. It now says the slot was his. |
| *, read by Ft8Sharp.Deep with fine sync and ordered statistics* on every census line | The decoder and its stage list belong in the sidecar and on hover, not repeated four times a minute. **Nothing about what is decoded changed.** The sidecar still records it. |
| The census's stage sentences | **No number was lost.** Candidates, codewords and checksums are each still on the line, asserted one by one in `ASlotHeTransmittedInSaysSoTests`; only the prose around them went. |

## A fact removed, and it is named here rather than let go quietly

**The band came out of the worked-station hover.**

- **Was**: *You worked IK4LZH before, on 2026-09-07, on 20m.*
- **Now**: *You logged a contact with IK4LZH on 09/07/26*

**That is a fact removed, not a sentence shortened**, and it was Tim's own wording
and his own ruling of 2026-09-08. It is listed here because task 6 exists to make
exactly this visible rather than let it pass as tidying. **The band is still in the
log** and the log window shows it; what is gone is the band on the hover.

## Kept deliberately, with the reason

| Kept | Chars | Why |
|---|---|---|
| *nothing on this frequency yet. FT8 runs in fifteen second slots, so give it a slot or two before deciding the band is empty.* | 124 | **An empty panel that says nothing reads as a broken program**, which is the §0.0 fault this whole rule is subordinate to. It explains an absence, which is what the rule allows a sentence for. |
| The For you empty state naming a missing callsign | — | Named by the instruction as a keeper. The difference between a blank box and a fixable problem. |
| *what the sound card was actually handed* | — | A §0.0 boundary statement about what Hamlet can and cannot see past, not advice. Named by the instruction as a keeper. |
| The log window's empty state | — | Named by the instruction as a keeper. |
| A clock that is **not** fine | — | A fault gets a sentence. Shortening the one that matters would remove a fact rather than a sentence. |
| Every refusal and licence wording | — | Explains why something did not happen. Untouched. |

## What was not swept

- **`ReceiveAdvice`**, the 18 KB of receive-side prose. The instruction places the
  AGC paragraph in the status bar; it is not there, and that mismatch is reported.
  It surfaces in a collapsible Receive-help widget on the canvas, which is not the
  screen he operates from in the sense the ruling means, and restructuring an engine
  file feeding a different panel is outside what this unit was asked for.
- **The CW tab and the Explore panels.** The ruling and every task name the Digital
  tab.
- **Tooltips generally.** Hover is where words were moved *to*.
