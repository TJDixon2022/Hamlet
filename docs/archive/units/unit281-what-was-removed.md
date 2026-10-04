# What this unit moved, and where it went — work instruction 281, task 7

**Removing words can remove facts, and the only defence is a list somebody can
read.** This is that list, in the shape unit 280's is.

**Tim's ruling, 2026-09-08**: *"I want clean visual screens with text only where I,
the user, intentionally hover."* Not less text — **none, unless he asked for it**,
with one exception: **a fault speaks unasked.**

---

## The number

Permanently-visible characters across the **whole application**, walked off the
realized window by `tests/Hamlet.App.Tests/Views/HowMuchTheApplicationSaysTests.cs`.
Not the Digital tab alone — that was the author's error and this figure measures the
ruling.

| | Before | After |
|---|---:|---:|
| **The whole application** | **14,670** | **7,665** |
| MainWindow — CW tab | 1,450 | 649 |
| MainWindow — Digital tab | 1,441 | 1,347 |
| MainWindow — Voice tab | 647 | 531 |
| SettingsWindow | 6,845 | 1,628 |
| RigDiagnosticsWindow | 1,546 | 1,346 |
| LogContactWindow | 810 | 716 |
| AboutWindow | 726 | 726 |
| DecisionLogWindow | 400 | 196 |
| ContactLogWindow | 399 | 246 |
| FavoritesWindow | 377 | 280 |

**Just under half of what the application was saying without being asked has gone to
a hover.** The CW workspace on its own went from 897 characters to 172.

**The figure wobbles about 25 characters run to run**, because the Shakespeare byline
rotates through forty-five strings of different lengths (HM-DEC-039) and the
clock-offset line carries a live number. Both figures were taken with the same
instrument, which is what makes the difference meaningful.

**`AboutWindow` did not move and that is deliberate.** It is a page opened in order
to be read, not prose beside a control he is trying to use. Its mission statement is
its content.

---

## The mark

`src/Hamlet.App/Controls/HintMarkControl.cs`. A small ring holding one sentence,
shown only on hover, in three kinds:

| Kind | Glyph | Holds |
|---|:---:|---|
| Tip | `?` | teaching: how it works, what to do, what a word means |
| Measurement | `#` | a number and where it came from |
| Boundary | `⊣` | what Hamlet can and cannot see past (§0.0) |

**The tooltip leads with the kind in words**, so the glyph is never the only carrier
of what sort of thing it is (§0.6). **It draws nothing when it holds nothing**,
because an empty mark is a hover target for a sentence that does not exist.

**It replaced unit 280's `hm-tip` TextBlock idiom** as well, so there is one mark in
the application and not two (§0).

---

## Moved to hover, nothing lost

### SettingsWindow — 23 blocks, 6,845 characters to 1,628

Every `hm-note` paragraph and every `GlossaryTextControl` explanation, on 31 marks.
The twelve longest:

| Chars | About | Kind |
|---:|---|---|
| 451 | Morse copy speed, and what Hamlet will not claim about it | tip |
| 353 | the spot networks, and what identifying to them is owed | boundary |
| 337 | CW pitch and where the decoder starts | tip |
| 333 | the scanner's band file | boundary |
| 326 | mode-follow | tip |
| 316 | how long a spot means anything | boundary |
| 259 | license class, and that listening is never restricted | tip |
| 249 | reconnecting on launch | tip |
| 240 | which audio device carries FT8 | boundary |
| 223 | what a grid square is | tip |
| 217 | the transmit guard | boundary |
| 186 | who is at the key, and that it is never uploaded | boundary |

### MainWindow

| Was | Chars | Now on screen | Where the words went |
|---|---:|---|---|
| *CQ tells the band you are looking for a conversation… Send puts what is on the line on the air.* | 317 | the buttons | a Tip mark under the send row |
| *Press this whenever you can hear a station… the only place the ones Hamlet missed are counted at all.* | 226 | the button | a Boundary mark beside it |
| *a dimmed character is one Hamlet is not sure of, and a block is something it heard and could not read…* | 142 | the transcript | a Boundary mark under it |
| *hover a dot to see who it is · click a dot to tune there…* | 121 | the map | a Tip mark above it, on all three tabs |
| *what the radio is hearing, as it arrives* | 40 | the panel's title | a Tip mark |
| *click a reply* | 13 | the ring | the ring's own hover, as a whole sentence |
| *yours is next* | 13 | the ring | the ring's own hover |
| *nothing heard yet* | 17 | the ring | the ring's own hover |
| *· sent* under a bubble | 7 | the alignment | the caption's hover, `DirectionTip` |
| *contacts* beside the count | 8 | the number | the belt ring's hover |
| *6 to 10* beside the count | 7 | a drawn bar | the belt ring's hover |
| *The sound card was handed −6.0 dBFS — that is the peak the endpoint actually got, measured on the way out after clamping…* | 250 | `sound card got -6.0 dBFS · 4 samples clamped` | `TransmitLevelTip`, on a Boundary mark |
| *It was composed at −12.0 dBFS with nothing clipped — that is the level Hamlet built, before this machine's own volume…* | 473 | `composed at -12.0 dBFS · nothing clipped` | `ComposedLevelTip`, on the line's own hover |

**The composed line is the one task 1's measurement never saw**, because it only
appears after something has gone out. 473 characters on the screen after every send.

### The other windows

| Window | Was | Kind |
|---|---|---|
| ContactLogWindow | *Every contact you have logged, newest first. Hamlet reads this file and never changes it…* (153) | Boundary |
| RigDiagnosticsWindow | *Everything Hamlet has asked the radio, with what each answer was and when it arrived…* (200) | Boundary |
| DecisionLogWindow | *Every time Hamlet worked something out and it came out differently from last time…* (204) | Tip |
| FavoritesWindow | *Each one remembers what it was for, which is the thing a numbered memory channel cannot tell you.* (97) | Tip |
| LogContactWindow | *Yours, and nothing above is changed by what you write here. It goes in the ADIF COMMENT field.* (94) | Boundary |

---

## Replaced by a shape, nothing lost

| Was | Now |
|---|---|
| `6 to 10` beside the count | `BadgeProgressControl`, a drawn bar at the real fraction between two ranks, in a neutral ink. Six of the ten to the yellow belt draws six tenths; at the gold it is not drawn at all |
| The turn ring's captions for his slot and theirs | The ring itself. **His and theirs used to separate by hue and by caption**, so taking the caption away would have left hue alone — theirs is now drawn at a thinner stroke. The four states read solid 3px, solid 1.5px, solid 6px, dashed 3px, and the test compares shape with the words stripped out |
| The count in green | The count in the primary ink, inside a ring in the belt's colour. **Two coloured things beside each other was one too many** once the ring carried a rank |

---

## Removed outright, and why each was not a fact lost

| Was | Why |
|---|---|
| The sender tooltip on a message he sent | **He knows who sent it.** Every sentence the vocabulary table produces is about the sender, and on his own transmission the sender is him. `DirectionTip` still says when it went out |
| The payload explanation on a message he sent | Same reason. Hamlet was telling him his own callsign is from the United States and offering to work out how far away he is |

---

## A name that changed rather than moved

**`United States of America` is now said as `the United States`**, along with
`North Korea`, `Laos` and `Tanzania`.

**The cited file is untouched.** `data/callsigns/dxcc-prefixes.json` quotes the ARRL
DXCC List and a quotation that has been tidied is no longer a quotation (§6.1), so
`EntitySpoken` is a spoken form laid over it and used only where a name goes into
prose. A label or a citation still reads exactly as the ARRL wrote it, and a test
asserts every key is still a real name in that file.

**Four of the thirteen names over twenty characters, not thirteen.** Most of the rest
are simply their names. **The two Congos are deliberately left long**: every short
form that fits one of them reads as the other to somebody, and a name that names the
wrong country is §0.0 broken where a long name is only untidy.

---

## What stayed in words, and why

A fault speaks unasked, and so does anything that explains an absence or a refusal.

| Kept | Where |
|---|---|
| *clock not measured* | the turn ring — the ruling's own example of a fault |
| *stopped partway* | the turn ring — a transmission that did not finish |
| The message going out, on air | the turn ring — a fact, and the most useful thing on the screen while his carrier is up |
| *this transmit device reports no level* | the Send block — a device that will not answer |
| *Hamlet cannot see a recording device on this computer just now…* | Settings — split out of `AudioDeviceNote`, which was carrying a fault and a tip in one property |
| *Hamlet cannot see a playback device…* | Settings — split out of `TransmitEndpointNote` the same way |
| The SOTA refusal, and the RBN callsign note | Settings — a refusal explaining itself |
| *License class not set…* and *Not set yet* for the grid | Settings — a setting that is genuinely missing |
| The scanner's file path | Settings — where the file he may edit actually is |
| Every empty state | the log, favorites, decisions, the For you column, *nothing on this frequency yet* |
| Every licence and privilege wording | the top strip, HM-DEC-086 |
| `utc snr message` | the decoded list — column headers naming real columns |
| The nine *Hamlet did not hear this* cells | the log dialog — each is a fact about its own field |

---

## No fact was lost, and here is what says so

`AdviceWaitsToBeAskedTests` names nineteen sentences that left the Settings screen
and eleven that left the main window, and fails if any one of them is not behind
something on the realized window. It sweeps every tooltip and not only the marks,
because some of what moved went onto a control's own hover — the turn ring's, the
send line's, the bubble caption's.

`TheTurnIsARingTests.HoveringTheRingAnswersInEveryState` covers the six turn states
one at a time, which is where a state machine belongs; a realized window cannot be
in six states at once.

**Unlike unit 280, this unit has no heading for a fact that was lost**, because it
did not lose one. The one thing genuinely deleted is the sender explanation on his
own messages, and it is listed above with its reason.

---

## A correction to unit 280's own number

**Unit 280 measured `TextBlock` and nothing else.** `GlossaryTextControl` is a bare
`Control` that draws its own runs, so a walk looking only for `TextBlock` never sees
it — and two of them are on the Digital tab, one of which is the 124-character empty
state unit 280's own list records as **deliberately kept**.

So unit 280's *after* figure of 529 excluded a string that unit knew it was keeping.
The corrected figure for the same subtree is **869**. Reported rather than reconciled
away, and this unit's before and after are the same instrument throughout.
