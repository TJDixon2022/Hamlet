# Every permanent string in the application — work instruction 281, task 1

**Reading and measuring only.** Nothing was removed in this task.

**Tim's ruling, 2026-09-08**: *"I want clean visual screens with text only where I,
the user, intentionally hover."* Unit 280 measured one tab because the order it was
given named one tab. **The ruling was always app-wide**, so this measurement is.

---

## How it was measured, and why the number does not match unit 280's

Walked off the **realized window** as unit 280 did, not counted in source: almost
none of this wording is a literal in the markup, so a source count measures the
wrong thing and then flatters whatever moved.

**It counts two text-bearing controls and unit 280 counted one.** A `TextBlock` and
a `GlossaryTextControl` both put words on the screen; the second is a bare `Control`
that draws its own runs, so a walk that looks only for `TextBlock` never sees it.
Two of them are on the Digital tab — `DigitalReadinessLine` and `DigitalMineIdle` —
and **`DigitalMineIdle` is the 124-character string unit 280's own list records as
deliberately kept**. So unit 280's *after* figure of 529 excluded a string that unit
was aware of keeping. The corrected figure for the same subtree is **869**.

That is reported rather than reconciled away. Task 7 re-measures with this same
harness, so this unit's before and after are the same instrument.

**The figure wobbles by about 25 characters between runs**, and the reason is
known: the Shakespeare byline rotates through forty-five strings of different
lengths (HM-DEC-039), and the clock-offset line carries a live number whose digits
change. Neither is a defect. **A total quoted to the character would be a precision
this measurement does not have**, so the run-to-run range is stated instead.

The harness is `tests/Hamlet.App.Tests/Views/HowMuchTheApplicationSaysTests.cs`.
It asserts no total — a threshold would be a session deciding how terse is terse
enough, which is not its call.

---

## The number

| Screen | Chars | Blocks |
|---|---:|---:|
| MainWindow — CW tab | 1,450 | 49 |
| MainWindow — Digital tab | 1,441 | 74 |
| MainWindow — Voice tab | 647 | 30 |
| **SettingsWindow** | **6,845** | **126** |
| RigDiagnosticsWindow | 1,546 | 141 |
| LogContactWindow | 810 | 40 |
| AboutWindow | 726 | 28 |
| DecisionLogWindow | 400 | 4 |
| ContactLogWindow | 399 | 5 |
| FavoritesWindow | 377 | 6 |
| **TOTAL** | **≈14,670** | **503** |

Subsets of the main window, not added into the total: `CWWorkspace` 897 in 19,
`DigitalWorkspace` 869 in 44, `VoiceWorkspace` 0 in 0. The rest of each tab's figure
is the shared chrome — the top strip, the menu, the neighborhood map and the status
bar — which is why the three tabs do not differ by as much as their workspaces do.

**Settings is 47 per cent of the whole application.** It is not the Digital tab and
it is not `ReceiveAdvice`.

---

## The two mismatches with the instruction

Both reported, neither repaired here.

### `ReceiveAdvice` is not on any screen

The instruction says its 18 KB "surfaces in a collapsible Receive-help widget on the
canvas". **There is no canvas any more, and the widget is not in the realized
window.** `MainWindow.axaml` declares fifteen `widget.*` data templates and
**thirteen of them are referenced by nothing**:

| Referenced | Unreferenced |
|---|---|
| `widget.map`, `widget.terminal` | `widget.tape`, `widget.waterfall`, `widget.autocall`, `widget.scan`, `widget.send`, `widget.phrasebook`, **`widget.receiveHelp`**, `widget.heard`, `widget.lead`, `widget.spots`, `widget.story`, `widget.guide`, `widget.contact` |

So `ReceiveAdvice`'s eight sentences reach no screen at all. What does reach a
screen is `ReceiveOffer`, one line inside `widget.terminal` on the CW tab, and the
**"Have a look" button beside it, which runs `OpenReceiveHelpCommand`, which sets
`ReceiveHelpExpanded = true` on a panel that no longer renders.** Pressing it does
nothing visible. That is §0.5.1's fault exactly — a control whose resting appearance
says it can be pressed, that cannot do anything — and it is **not this unit's to
repair**, because §12.6 says name it and leave it. It is raised in `OPEN_ISSUES.md`.

### The largest instance of the thing task 2 is aimed at is Settings

Not `ReceiveAdvice`. **Thirteen `hm-note` paragraphs and ten `GlossaryTextControl`
explanations** carry 6,845 characters of pure teaching prose beside the controls
they explain — 451 characters on Morse copy speed, 353 on the spot networks, 337 on
CW pitch, 333 on the scanner's band file, 326 on mode-follow. Task 2's rule lands
here.

---

## Classification, screen by screen

**A fact** is a measured value, a provenance mark, or a label naming a real control
or column. **A fault** is something wrong that Hamlet must say unasked. **Advice**
is teaching: how it works, what to do, what a word means.

### MainWindow — CW tab, 1,450

| Chars | String | Class |
|---:|---|---|
| 317 | *CQ tells the band you are looking for a conversation and puts your callsign on it. RST is the signal report… Send puts what is on the line on the air.* | **advice** |
| 226 | *Press this whenever you can hear a station, whether or not anything appeared here. It keeps the last half minute…* | **advice** |
| 142 | *a dimmed character is one Hamlet is not sure of, and a block is something it heard and could not read…* | **advice** |
| 121 | *hover a dot to see who it is · click a dot to tune there · click the background for a neighborhood's story · drag to tune* | **advice** (shared chrome) |
| 83 | *Hamlet is keeping up with your radio, asking it where it is several times a second.* | fact — link state |
| 58 | *Training radio does not transmit, so this is receive only.* | **fault** — a refusal, keep |
| 50 | *Your General license covers Morse here. Call away.* | fact — licence, HM-DEC-086 keeps it |
| 42 | *scroll wheel over a digit tunes that digit* | **advice** — HM-DEC-141 already rules this retires once used |
| 40 | *what the radio is hearing, as it arrives* | **advice** — a caption on a panel that has a title |
| 33 | the Shakespeare byline | flourish, HM-DEC-039, out of scope |
| 30 | *General, from FCC data, today.* | fact — provenance |
| 28 | *Training radio (no hardware)* | fact |
| 28 | *CW main street · 7.000–7.125* | fact |
| 28 | *preamp unknown · att unknown* | fact — §0.0 unknown is a state |
| 24 | *7.030 MHz · yours to use* | fact |
| 19 | *what you would send* | **advice** — caption |
| ≤16 | 33 labels: menu items, band buttons, `Send`, `Clear`, `RST`, `CQ`, `73`, `▾` | facts |

**Advice on the CW tab: 767 characters in 6 blocks.**

### MainWindow — Digital tab, 1,441

| Chars | String | Class |
|---:|---|---|
| 219 | *this is the training radio rather than the receiver, so everything in the audio was made by Hamlet…* | **fault** — a boundary on what can arrive, keep |
| 124 | *nothing on this frequency yet. FT8 runs in fifteen second slots…* | **advice** — unit 280 kept it deliberately, explaining an absence |
| 121 | *hover a dot…* | **advice** (shared chrome) |
| 121 | *Hamlet does not know your license class, so it is not checking this transmission. Set your class in Settings and it will.* | **fault** — a setting missing, keep |
| 83 | link state | fact |
| 58 | *Your General license covers digital modes here. Call away.* | fact |
| 42 | *scroll wheel…* | **advice** |
| 38 | *clock is 1.15 s slow, checked just now* | **fault** — keep, unit 280 already ruled it |
| 36 | *200–3000 Hz · 15 s slots · simulated* | fact |
| 35 | *214130 UTC · 1 shown · newest first* | fact |
| 24 | *keep the last 30 seconds* | **advice** — a caption under a button that already says what it does |
| 21 | *no level measured yet* | fact — §0.0, unit 280 shortened it here |
| 18 | *clock not measured* | fact |
| 17 | *on this frequency* | fact — caption |
| ≤16 | 60 labels: modes, columns, counts, callsigns, `utc snr message` | facts |

**Advice on the Digital tab: 311 characters in 4 blocks.**

### MainWindow — Voice tab, 647

Shared chrome only; `VoiceWorkspace` is empty. Carries the 121-character map hint,
the 42-character scroll hint, *Receiving is never restricted. Any license may listen
anywhere.* (63, a fact about the law), *License class unknown. Set it in Settings to
see your privileges.* (65, a **fault**), and *On the training radio, with synthesised
signals and nothing on the air* (70, a **fault**/boundary).

**Advice on the Voice tab: 163 characters in 2 blocks**, both shared chrome.

### SettingsWindow, 6,845 — the largest by a factor of four

Thirteen `hm-note` paragraphs and ten `GlossaryTextControl` explanations.
**Essentially all of it is advice.** The twelve longest:

| Chars | About |
|---:|---|
| 451 | Morse copy speed, and what Hamlet will and will not claim about it |
| 353 | the spot networks, and what identifying to them is owed |
| 337 | CW pitch and where the decoder starts |
| 333 | the scanner's band file |
| 326 | mode-follow |
| 316 | how long a spot means anything |
| 259 | license class and that listening is never restricted |
| 249 | reconnecting on launch |
| 240 | which audio device carries FT8 |
| 227 | why SOTA ships off |
| 223 | what a grid square is |
| 217 | the transmit guard |

The remainder is field labels — `Callsign`, `Name`, `Location`, `Grid square` — and
provenance marks, which are facts.

**Advice in Settings: about 6,300 characters in 23 blocks.**

### RigDiagnosticsWindow, 1,546 in 141 blocks

**One advice block of 200 characters** at the head. The other 140 are field names
and readings — `Noise reduction level`, `no radio connected`, `0 of 45 read from
the radio` — which are facts and the entire purpose of the screen.

### LogContactWindow, 810

Three advice blocks totalling 336 characters, and **nine repetitions of *Hamlet did
not hear this* at 24 characters each — 216 characters saying one thing nine times.**
Each is a fact about its own field, so it is not a repeat in the HM-DEC-068 sense,
but it is the largest single block of text on that screen and it is a candidate.

### AboutWindow, 726

A 257-character mission statement and a 77-character note about what the diagnostics
carry. **About is a page opened to be read**, which is nearer to hovering than to
occupying the screen while he operates.

### ContactLogWindow 399, FavoritesWindow 377, DecisionLogWindow 400

Each is a header sentence plus an empty-state sentence. **The empty states explain
an absence**, which is the one thing §0.0 requires a sentence for, and unit 280's
list already records them as keepers.

---

## Where the advice actually is

| Screen | Advice chars | Share of that screen |
|---|---:|---:|
| SettingsWindow | ≈6,300 | 92% |
| MainWindow — CW tab | 767 | 53% |
| MainWindow — Digital tab | 311 | 22% |
| AboutWindow | 334 | 46% |
| LogContactWindow | 336 | 41% |
| ContactLogWindow | 351 | 88% |
| FavoritesWindow | 345 | 91% |
| DecisionLogWindow | 374 | 94% |
| RigDiagnosticsWindow | 200 | 13% |
| MainWindow — Voice tab | 163 | 25% |

The three small windows read as almost all advice because they were measured empty:
their text is a header and an empty state, and the rows that would fill them are
facts nobody has generated yet on this machine.
