# Work instruction 281 — text only where he hovers, and a belt for the count

```
UNIT:      281
TASKS:     8 of 8, none dropped
NUMBER:    14,670 -> 7,665 characters of permanently-visible text
           across the whole application
ADVANCED:  no
DRIFT:     4 consecutive units without advance, carried from unit 280
VERSION:   1.12.173 -> 1.12.181
BRANCH:    main, pushed
```

---

## 1. What Claude did

**Surface: Claude Code, on the development computer, on `main`.** The prompt claimed
`PROJECT: Hamlet` and the tree confirmed it — `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` both present, `CoreHMI.sln`
and `MURC.sln` both absent, `Hamlet.sln` at the root. **Nothing in this report is
evidence about the radio**: there is no rig and no antenna here, and everything below
is measured off headless windows and synthesized data.

**Nothing was recorded under §12.1.** Every judgement below is reported for his
ruling, not taken as one.

### Task 1 — every permanent string in the application

`tests/Hamlet.App.Tests/Views/HowMuchTheApplicationSaysTests.cs` walks the realized
window of all ten screens and sums what is on them without hovering or opening
anything. **14,670 characters in 503 blocks.**

| Screen | Chars | Blocks |
|---|---:|---:|
| **SettingsWindow** | **6,845** | 126 |
| RigDiagnosticsWindow | 1,546 | 141 |
| MainWindow — CW tab | 1,450 | 49 |
| MainWindow — Digital tab | 1,441 | 74 |
| LogContactWindow | 810 | 40 |
| AboutWindow | 726 | 28 |
| MainWindow — Voice tab | 647 | 30 |
| DecisionLogWindow | 400 | 4 |
| ContactLogWindow | 399 | 5 |
| FavoritesWindow | 377 | 6 |

Classified per screen in `docs/unit281-what-the-screens-say.md`. **Settings is 47 per
cent of the application and 92 per cent of that screen is teaching prose.**

**Two mismatches with the instruction, reported and not repaired.**

**`ReceiveAdvice` is on no screen at all.** The order places it in a Receive-help
widget on the canvas; there is no canvas, and `MainWindow.axaml` declares fifteen
`widget.*` templates of which **thirteen are referenced by nothing** —
`widget.receiveHelp` among them. Its eight sentences reach nobody. What does reach a
screen is the **"Have a look" button on the CW tab, which runs
`OpenReceiveHelpCommand`, which expands a panel that is not in the tree**: pressing
it does nothing and it looks exactly like a button that works, which is §0.5.1 and
HM-DEC-087 and precisely the fault `BindingHealthTests` cannot see, because the
binding resolves perfectly onto a property nothing renders. Raised as
**HM-OPEN-087** and left alone under §12.6 — whether to rehome the widget, delete
thirteen dead templates, or fold the advice into a menu decides whether a ratified
feature is in the application or out of it, and that is a ruling.

**The measurement corrects unit 280's own number.** That harness counted `TextBlock`
alone, and `GlossaryTextControl` is a bare `Control` that draws its own runs. Two are
on the Digital tab, and one of them is the 124-character empty state unit 280's list
records as **deliberately kept** — so its *after* figure of 529 excluded a string it
knew it was keeping. The corrected figure for the same subtree is **869**. This
unit's before and after are the same instrument throughout.

### Task 2 — advice behind a mark, in two passes

`HintMarkControl`: a small ring holding one sentence, shown only on hover, in three
kinds the mark itself names — a tip (`?`), a measurement (`#`), a boundary (`⊣`).
The tooltip leads with the kind in words, so the glyph is never the only carrier of
it (§0.6). It draws nothing when it holds nothing.

The first pass took Settings — 23 blocks onto 31 marks, **6,845 to 1,628**. The
second, in task 7, took the rest task 1 had found: the CW tab, the map hint on all
three tabs, and the header sentence on five other windows.

**A fault is not advice, and two properties were carrying both.**
`AudioDeviceNote` and `TransmitEndpointNote` each returned either *Hamlet cannot see
a recording device on this computer just now* or *pick the input the radio's audio
arrives on*, from one string. The first is something wrong he needs told; the second
is a tip. Each is split so only the tip half goes behind the mark.

**It replaced unit 280's `hm-tip` TextBlock idiom**, so there is one mark in the
application and not two (§0).

### Task 3 — text only where he hovers

**The turn ring's captions go.** *click a reply*, *yours is next* and *nothing heard
yet* are on the ring's own hover as whole sentences. **Two captions stay and both are
faults** — *clock not measured* and *stopped partway* — and the on-air line is not a
caption at all but the message going out.

**Removing them moved a grayscale carrier rather than dropping it.** His slot and
theirs separated by hue *and* by caption; with the caption gone the hue would have
been alone, so theirs is drawn at a thinner stroke. Measured on the realized window
with the words stripped out: **solid 3px, solid 1.5px, solid 6px, dashed 3px.**

**`sent` goes from under the bubble.** Unit 280 kept it citing §0.6 and **the
citation was wrong**: that rule is about colour and its own practical test is whether
the screen reads in grayscale, which alignment does. The fact is on the caption's
hover.

**The Send block keeps the facts.** The measured line is `sound card got -6.0 dBFS ·
4 samples clamped`; the clamp sentence and the boundary past the sound card are on
the mark beside it — the two the author told unit 280 to keep visible.

**And one the measurement never saw**: after every send the composed line ran to
**473 characters**, invisible to task 1 because it only appears once something has
gone out. It now reads `composed at -12.0 dBFS · nothing clipped`, with unit 269's
boundary statement on the line's own hover.

### Task 4 — the belt

Ten ranks in one table: **white 0, yellow 10, orange 25, green 50, blue 100, purple
500, brown 1000, red 2000, black 5000, gold 10000.**
`ContactMilestones.Thresholds` now derives from that table rather than carrying the
same nine numbers a second time. White is the rank that is not a threshold, because
*that is 0 contacts logged* would congratulate him for having done nothing.

**A border and never a fill.** The test asserts the background's alpha is zero rather
than trusting the markup to stay that way.

**The rank is the decoration and nothing else is.** The number stays 20 point and
becomes the primary ink rather than green — the ring carries the hue now, and two
coloured things side by side was one too many. `6 to 10` becomes a drawn bar in a
**neutral** ink, so in grayscale he still knows how many contacts he has and how far
the next rank is. Six of the ten to yellow draws six tenths; at the gold it is not
drawn at all.

### Task 5 — the sender tooltip is not about him

A message he sent gets **no sender tooltip and no payload explanation**. Every
sentence the vocabulary table produces is about the sender, and on his own
transmission that is him. The caption hover still says when it went out.

**The entity is said the way a person says it.** `EntitySpoken` lays a spoken form
over the ARRL's names for four of them — *the United States*, *North Korea*, *Laos*,
*Tanzania*. **The cited file is untouched** (§6.1), and this is used only where a
name goes into prose. Four of the thirteen names over twenty characters, not
thirteen: most of the rest are simply their names, and **the two Congos are
deliberately left long**, because every short form that fits one reads as the other
to somebody.

`DxccPrefixes` gains `Entities` so the shortening's keys can be checked against the
cited list; a key that stopped matching would otherwise silently do nothing.

### Task 6 — the grid that is there and was not found

**The cause, with file and line.** `MainWindowViewModel.KeepSentRow` builds its row
through `DigitalDecodeRow.Sent`, which passes `ObserverGrid: ""`, **and it does not
call `PlaceRow`** — which is where the operator's grid was read. So a message he
transmitted carried a blank grid while Settings held `FN00DJ`.

**It is the same shape as the other two.** The menu on the wrong list, the Log item
gated where it could never fire, and now this: a second construction site added later
that does not go through the door the first one uses, with the value plainly present
throughout. `PlaceRow`'s own remarks have claimed to be *the one place the operator's
own grid reaches a row* since unit 252, and that stopped being true the moment
`KeepSentRow` was added. **A remark claiming there is one place is not one place**,
so it is a method now — `WithOperatorGrid` — that both builders call.

**And the instruction's own criterion was already met, which is the honest half.**
Watched at HEAD with this unit's work stashed: **three of the four new tests were
already green**, including the one the order asks for. The received path went through
`PlaceRow` the whole time. **Exactly one was red, and it is the sent row** — which is
the row he was hovering when he saw it.

### Task 7 — what was removed, listed and measured

`docs/unit281-what-was-removed.md`. **14,670 → 7,665**, same instrument both times.
**Just under half of what the application said unasked is now one hover away.**

**This unit has no heading for a fact that was lost, because it did not lose one.**
The only thing deleted outright is the sender explanation on his own messages, listed
with its reason. `AboutWindow` did not move, deliberately: it is a page opened in
order to be read, not prose beside a control.

### Task 8 — the outcome entry

Appended to `PHASE_OUTCOME.md` by `tools\arbiter\outcome-append.bat`, exit 0, as
`UNIT 281 - STEP E`. Units 273, 274 and 275 were not back-filled.

### Departures from the numbering

**Task 2's sweep finished inside task 7.** The first pass went where the bulk was and
the second took what task 1 had found on the other screens. Reported here rather than
left to be noticed.

### One shell refusal, recorded verbatim

A bash heredoc carrying an apostrophe failed with:

```
/usr/bin/bash: -c: line 102: unexpected EOF while looking for matching `''
```

Exactly the fact the instruction names. Every subsequent edit went through script
files written with the file-editing tools, and the outcome entry through a `.bat`.

---

## 2. What Tim should expect

**Clean screens, and words only where you ask for them.**

Open Settings and it is a list of controls with a small ring beside each one instead
of a paragraph under it. Hover the ring and the paragraph is there, word for word,
with the kind of thing it is in front of it — *tip*, *measurement*, or *what Hamlet
can see*. Nothing was deleted. Thirty-one rings on that screen, and a test names
nineteen of the sentences behind them and fails if any one has gone.

**The count is now a number in a coloured ring.** White until ten contacts, then
yellow, and up through orange, green, blue, purple, brown, red and black to **gold at
ten thousand**. Hover it: *You are on the white belt, at 4 contacts. 6 more contacts
and the ring turns yellow at 10.* The bar beside it is grey on purpose — the ring is
the only thing carrying the rank, so if you printed the screen in black and white you
would still know the count and how far the next one is, and lose only the colour.

**The turn ring says nothing until you hover it**, unless something is wrong. A clock
that has not been measured still says so in words, and so does a transmission stopped
partway, and while your carrier is up the line beside the ring is the message going
out.

**Your own messages no longer explain themselves to you.** The CQ that told you your
callsign is from the United States and offered to work out how far away you are has
no tooltip at all now.

### What will look wrong and is not

- **The grey ring beside the count is the progress bar**, not a broken control. It is
  neutral on purpose.
- **A lone ring with nothing beside it** on the Settings screen, the map, the CW
  transcript and the log windows is the mark. It is a hover target, not a leftover.
- **The white belt is drawn as a light warm grey.** A white ring on a white panel is
  no ring at all. The rank is still called white and the hover says so.
- **`AboutWindow` is unchanged** and still carries its paragraph.

### The build and the tests

Build clean, no warnings. **This unit ran no suite** (HM-DEC-155): only the tests it
wrote or rewrote, filtered by name, foregrounded.

| Class | Result |
|---|---|
| `HowMuchTheApplicationSaysTests` | 1 of 1 — new |
| `AdviceWaitsToBeAskedTests` | 5 of 5 — new |
| `TheCountWearsARankTests` | 5 of 5 — new |
| `TheSenderTooltipIsNotAboutHimTests` | 3 of 3 — new |
| `TheGridInSettingsReachesEveryRowTests` | 4 of 4 — new, one watched failing first |
| `TheTurnIsARingTests` | 7 of 7 — rewritten |
| `TheReadoutSaysWhatTheCardWasHandedTests` | 5 of 5 — rewritten |
| `TheCountIsTheRecordsTests` | 5 of 5 — two assertions updated |
| `WhatTheLogCostsAtTenThousandTests` | 3 of 3 — one assertion updated |
| **Total** | **38 of 38** |

**Ten reds found in `TheMessageReadsAsThreePartsTests`, and they are inherited.**
Measured with this unit's work stashed: 91 tests across the five tooltip classes, 81
green, **10 red before anything here was applied**. They are not in
`docs/unit239-failing-set.txt` and the instruction does not name them. Raised as
**HM-OPEN-088** and left alone under §12.6 — and three of them assert that
`SenderHelp` is exactly *Who sent it.* while `TheSenderTooltipNamesTheEntityTests`
asserts it names the entity, so **the two classes contradict each other** and that
wants a decision rather than a string edit.

Two assertions in that set were mine and are updated: both expected `United States of
America` where the spoken form is now used.

**Nine commits, all on `main`, all pushed**, version 1.12.173 → 1.12.181. Nothing
uncommitted.

---

## 3. What we should do next

1. **Rule on HM-OPEN-087** — thirteen dead widget templates and a live button that
   does nothing. It decides whether HM-DEC-084's receive help is in the application.
2. **Look at the Settings screen and say whether the marks are findable enough.** A
   14-pixel ring is quiet by design and quiet is one step from invisible; if it wants
   a lighter touch it is one control to change.
3. **Rule on HM-OPEN-088** — the two test classes that disagree about `SenderHelp`.
4. **Then back to the radio.** Every bench step is closed and this unit added nothing
   Hamlet can do; steps D and E are you at your own station.

---

## 4. What's blocking us

Nothing blocks the next unit. Four questions want a ruling.

### The dead widgets, and whether receive help is a feature

**Ruling asked for:** thirteen of the fifteen `widget.*` templates in
`MainWindow.axaml` are referenced by nothing, including `widget.receiveHelp`, and the
CW tab's *Have a look* button expands a panel that is not in the tree. Either the
widget is rehomed onto a tab, or the templates are deleted and the button with them.

**Why:** `ReceiveAdvice` is 18 KB of ratified work (HM-DEC-084) that reaches no
screen, and a button that looks live and does nothing is §0.5.1 exactly — the fault
`BindingHealthTests` cannot catch, because the binding resolves onto a property
nothing renders.

**Rejected:** doing it inside this unit. Restoring a feature nobody has ruled is
missing, on a session's own reading, is what §12.6 forbids; and deleting a ratified
feature's markup is not a session's call either.

### Whether a mark is findable enough

**Ruling asked for:** whether a 14-pixel outlined ring is the right weight for a
hover target, or whether it wants to be larger, filled on hover, or given a word.

**Why:** the whole ruling rests on him knowing there is something to hover. **Nobody
has looked at this on a screen** — it is measured, tested and unseen. A mark too
quiet to find turns a moved sentence into a deleted one in practice while the test
still passes.

**Rejected:** guessing at a bolder mark. §0.0's test is about what the operator can
act on, and only he can say whether he would notice it.

### Whether `AboutWindow` should have been swept

**Ruling asked for:** whether About's 726 characters are in scope.

**Why:** it was left alone on the judgement that About is a page opened in order to
be read rather than prose beside a control. **That is a judgement, and the ruling is
app-wide**, so it is reported rather than assumed.

### The two test classes that contradict each other

**Ruling asked for:** whether `SenderHelp` names the entity or is exactly *Who sent
it.* `TheSenderTooltipNamesTheEntityTests` asserts the first and three cases in
`TheMessageReadsAsThreePartsTests` assert the second.

**Why:** they cannot both be right, and it is not a typo — unit 271 added the entity
deliberately and the other class was not brought along. Ten reds in that file were
inherited by this unit and are recorded as **HM-OPEN-088**.

**Rejected:** making the strings match. Whichever way it goes it decides what that
tooltip is for, and picking one to clear a count is how the contradiction comes back.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139, with this unit's four added.

1. **Two issues of one work-instruction number.** Unit 271, and 252 before it.
   **The author's error.** No unit action.
2. **`PM95` reads *southern Japan***, unit 271. **Not a defect.**
3. **`HM-OPEN-083` and `HM-OPEN-084`.** In `OPEN_ISSUES.md` by HM-DEC-140.
4. **Three pixels.** **Waiting on Tim.**
5. **`dt` and `hz` were never on the mine list.** **Waiting on Tim.**
6. **Where an outcome entry goes when the unit it corrects has none.** Unit 276, in
   the tree. **The ruling wants Tim's eye.**
7. **`PHASE_OUTCOME.md` is written by a tool no unit is told to run.** Named in unit
   280's task 8; **the general question is the author's.** Unit 281 was told to run
   it and did.
8. **Where the repeat fold stops**, unit 277, at `0c359cc`. **Still open.**
9. **Whether a faded row needs a second carrier of its meaning**, unit 279. **The
   hover carries it in words**; a glyph as well is Tim's.
10. **Whether counting subjects is a new assertion**, unit 279, at `4d91bfe`. **The
    ruling wants Tim's eye.**
11. **Whether the fade is still obvious now the row is a bubble**, unit 280. **Not
    measured.** Interacts with 9.
12. **Whether `ReceiveAdvice` is in scope for the same ruling.** **Answered by Tim,
    2026-09-08: yes, app-wide.** Unit 281 found it is on no screen at all — see 13.
    **Dropped as an ask; it is now HM-OPEN-087.**
13. **The dead widget templates and the button that does nothing**, unit 281.
    `HM-OPEN-087`, in the tree. **Blocks the most work of the four.**
14. **Whether a 14-pixel mark is findable**, unit 281. **Not seen on a screen by
    anybody.**
15. **Whether `AboutWindow` is in scope**, unit 281. **A judgement this unit made and
    is reporting rather than assuming.**
16. **What `SenderHelp` is for**, unit 281. `HM-OPEN-088`, in the tree. **Ten
    inherited reds hang on it.**
