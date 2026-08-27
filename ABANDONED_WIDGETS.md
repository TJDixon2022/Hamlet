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

## The recent-places row

**Removed from between the header and the tabs on 2026-08-27**, by Tim's ruling
of that date. It is the one entry here that was **not deleted**: the control's
view model, its list and its commands are all still in the tree and still tested.
What it lost was its slot.

**What it did.** A dropdown of the last ten places the operator had been, beside
his favourites and behaving like them. A place was recorded by **dwell rather
than by landing** — twenty seconds, taken from the length of one relaxed CQ call
and deliberately not a setting — so tuning past somewhere never put it on the
list. Two visits within 200 Hz counted as the same place, read from the spot
bucket so that two numbers meaning *near enough* could not drift apart. An entry
named a station where one had been identified and was a bare frequency
otherwise, and the newest visit won even when it knew nothing. A second visit was
noted on the entry already there rather than making a new one, and the operator
could take any entry out by hand, or all of them.

**Why it existed.** Favourites are places he chose; recents are places he was,
and most favourites are born from realising later that somewhere was worth
keeping. The `forget this place` button belonged to wherever the dial was, so it
came and went with the dial.

**Where it might go.** It is not a CW thing, a Digital thing or a Voice thing, so
the three workspaces are the wrong home for it. The header above the divider is
the right kind of place and is already full. **That is the open question**, and it
is why the control was left in the tree rather than destroyed.

Refs: HM-DEC-072, HM-DEC-134.

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

## Which of these are capabilities and which are pictures

**Read on 2026-08-27, after the send button came back.** The button was removed
by three orders that each said not to wire Send to the transmitter, and the
transmit path had been finished, ruled and used for months. **Removing a surface
is not removing a capability, and the two had stopped being told apart.** So the
other fourteen were read the same way, against the tree rather than against the
catalogue.

**Report only. Nothing here was restored, and nothing here proposes a schedule.**

### Two are working capabilities with a radio attached and no control anywhere

**These are the same shape as Send was.** Both view models are constructed in
`MainWindowViewModel`, both are handed the live rig on connect at
`MainWindowViewModel.cs:5041-5042` — the two lines directly below the transmit
attach — and both are detached on disconnect. **What is missing is only the
markup.**

| widget | view model | engine | tests |
|---|---|---|---|
| **Scanner** | `ScanViewModel`, 598 lines | `BandScanner` 621 lines, plus `ScanDwell`, `ScanSegments`, `ScanStop`, `ScopeBinSurvey` | `BandScannerSafetyTests`, `ScannerEndToEndTests`, `ScanDwellTests`, `ScanSegmentsTests`, `ScanStopClassifierTests`, `ScannerFaceTests` |
| **Call CQ on a cycle** | `AutoCallViewModel`, 461 lines | `AutoCall` 759 lines, plus `AutoCallAnswers` | `AutoCallSafetyTests`, `AutoCallAnswerTests`, `AutoCallFaceTests`, `CallsignPrivacyTests` |

**Both carry the interlocks their rulings demand, and both interlocks are live
right now with nothing on screen to trip them.** The scanner refuses to start
before rig state is populated and restores the starting frequency by any exit
route (§0.2.1, HM-DEC-107); the calling cycle is dummy load only until §0.2's
first sentence is amended (HM-DEC-098). **They also ask each other**: the scanner
will not tune while the cycle is transmitting and the cycle will not key while
the scanner is moving, wired as two predicates at
`MainWindowViewModel.cs:2054-2065` so that neither holds a stale copy of the
other's state.

**The calling cycle is the one to be careful about.** It is the only thing in
this application that transmits without a hand on it, and it is the one feature
whose ruling says the interlocks are watched firing into a dummy load before it
reaches an antenna. **A surface for it is a separate decision from a surface for
the scanner**, and neither follows from the send button coming back.

### Three do real work whose only output was the deleted picture

| widget | what still runs | what is gone |
|---|---|---|
| **Waterfall** | `RigSpectrumSource` is attached with the rig and asks the radio for CI-V `27 00` | Every pixel. The frame counters of HM-DEC-093 have nowhere to appear, so "nothing has ever arrived" and "the band is quiet" are once again the same sight |
| **Did anybody hear me** | `HeardWatch` filters the spot feed for the operator's own callsign and `MainWindowViewModel` keeps the reports | The answer. It is computed and nothing displays it |
| **Dial tape** | `DialTapeControl` exists and tunes by dragging | The control's only placement |

**The waterfall and the heard watch are the two that cost something while they
sit like this**, because both call out — one to the radio four times a second,
one to somebody else's spot network — and neither can be seen. **A feed running
with no surface is the shape HM-DEC-024's politeness rule exists to prevent**,
and it is worth a measurement before it is worth an opinion.

### Nine were pictures over data, and the data is all still there

Where to start, Happening now, Phrasebook, Neighborhood map, I can hear it and
Hamlet can't, Field guide, Field notes, What a contact sounds like, and the
receive help. **Each was a rendering of something the engine still computes**,
and rebuilding one is markup and layout rather than machinery. The neighborhood
map is not really on this list at all: its template is one of the two still
instantiated, and it is on the CW screen now.

### What the count actually is

**Fifteen templates are still defined in `MainWindow.axaml` and two are still
used** — `widget.map` and `widget.terminal`, at lines 2308 and 2708.
`widget.send` joined the orphans this unit, because the send panel was rebuilt
permanently rather than restored from its template. **Thirteen `DataTemplate`
blocks are dead markup**, roughly 1,700 lines of it, and no test can see them:
`BindingHealthTests` and `EveryResourceKeyResolvesTests` both walk the live
window, and a template nothing instantiates is never built. **Deleting them is
not this unit's to do**, and it is named here so that the next reader knows the
markup is dead rather than dormant.
