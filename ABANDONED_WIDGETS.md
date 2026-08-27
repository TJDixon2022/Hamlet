# Abandoned widgets

**Written 2026-08-27, as the widget canvas was removed** (Tim's ruling of that
date: *"I don't care when it destroys. We're abandoning all of that."*).

## What this file is

The canvas, the tray, the preset bar, the layout namer and every saved
arrangement were deleted in work instruction 028. Fifteen widgets went with
them.

**This is the list, so that rebuilding any of them later as a real panel starts
from a description rather than from git archaeology.** Each row is the widget's
own name, the family colour it carried, the sentence the tray showed about it,
and the size it opened at.

**Two of the fifteen did not go anywhere.** `Terminal` and `Send` are the CW
workspace now — permanent panels, not widgets, not in any catalogue, not
removable. They are listed here because they were in the catalogue and are not
any more.

**Nothing here is a promise.** A widget on this list may come back as a panel, or
may not come back at all. What the list guarantees is that the decision is made
with the description in hand.

## The fifteen

| widget | family | what it did | opened at |
|---|---|---|---|
| **Where to start** | Amber | One sentence saying where to point the radio right now, and why that is the answer. | 360x200 |
| **Happening now** | Green | Who is on the air this minute, ranked for what you could actually work rather than for how far away they are. | 420x520 |
| **CW terminal** **— now a permanent CW panel** | Green | Morse arriving as text, with the characters Hamlet is unsure of marked rather than guessed at. | 520x320 |
| **Send** **— now a permanent CW panel** | Amber | What you could say next, written out in full, with a button that sends it. | 520x400 |
| **Did anybody hear me** | Green | Whether your last call reached anyone, from the receivers that listen all day and report what they hear. | 420x260 |
| **Phrasebook** | Green | The handful of things people actually say on the air, with what each one means. | 380x340 |
| **Neighborhood map** | Blue | What lives where across the band, so you can see what you are tuning into before you get there. | 560x200 |
| **Dial tape** | Amber | Fine tuning by dragging, with the stations somebody has reported marked along the top. | 560x180 |
| **Scanner** | Blue | Hamlet works down the band for you, stopping where somebody is actually calling rather than wherever there is a tone. | 460x400 |
| **Call CQ on a cycle** | Green | Hamlet does the calling and listens between rounds, and it stops the moment somebody answers. Into a dummy load while this is being proved. | 460x520 |
| **Waterfall** | Blue | The radio's own picture of the band, with signals as bright marks moving down the screen. | 560x320 |
| **I can hear it and Hamlet can't** | Blue | One button for when the radio is clearly hearing something and the decoder is not. | 420x300 |
| **Field guide** | Blue | What each mode looks and sounds like, so an unfamiliar noise stops being a mystery. | 420x400 |
| **Field notes** | Amber | The story of whatever stretch of band you are sitting in. | 380x260 |
| **What a contact sounds like** | Amber | A whole contact from the first call to the sign-off, both sides, in your own callsign. | 420x420 |

## The arrangements that went with them

Four saved arrangements, each a set of placements on the canvas:

- **Just receive and send** — nothing out, added 2026-08-27 as the one press back
  to a clear tab. Superseded by there being no canvas at all.
- **Getting started** — the lead card, the happening-now feed, the field guide
  and a worked contact. What a first run used to land on.
- **Listening around** — the dial tape, the waterfall and the terminal sharing
  one frequency axis, with the scanner beneath them.
- **Making contacts** — the arrangement for working somebody rather than reading
  the band.

**And the machinery**: `LayoutStore` and its `layouts.json`, `LayoutPresets`,
`CanvasLayout`, `Widget`, `Widgets`, `CanvasViewModel`, `WidgetViewModel`,
`WidgetCanvas` and `WidgetFrame`.

**A saved `layouts.json` on somebody's machine no longer loads, and that is the
intended outcome** rather than a regression (the ruling above).

## Why the canvas went

Two photographs, a day apart. On 2026-08-26 the CW tab rendered `wh at the rad io
is he ari ng` one or two letters to a line, because Receive was squeezed beside
the whole arrangement. On 2026-08-27, with Receive fixed, the arrangement was
still underneath it — **a second neighborhood map and a second CW terminal on the
same screen**, restored from a saved layout.

The pattern behind both: a surface the operator arranges is a surface that can be
arranged wrongly, and a panel that is also a widget can appear twice. **The three
workspaces have permanent panels instead**, and HM-DEC-086's "nobody ever starts
on an empty canvas" is superseded for them — CW opens on two working panels, and
Digital and Voice are empty because they have nothing to do yet.
