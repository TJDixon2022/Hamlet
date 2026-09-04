READ IN THIS ORDER

A. THE PHASE GOAL. Hamlet hears FT8 off the radio and displays the decoded text
on screen. It heard it on 2026-09-04 at 21:41 UTC. This unit is about the second
half of that sentence: the panel it lands in was built for four hard-coded rows
and now takes about fourteen a slot.

B. THIS STEP AND ITS EXIT CRITERIA. Every item came off the operator's own
screen. The headers did not line up with the text under them, there was no
scrollbar, no way to clear it, no way to choose the order, and the columns read
as gibberish to somebody who has not been told what `snr`, `dt` and `hz` are or
that a message is three parts.

C. WHAT THIS REPORT ADDS, AND WHETHER IT BEARS ON A OR B. It bears on B and
finishes it. All six tasks are done. The misalignment had a mechanism rather
than a fudge - two sibling grids of `Auto` columns, which share no measure - and
it is now provable rather than eyeballed: the header and every row report the
same five column origins in a headless build of the real window. **No engine
work, no decoder work, no transmit work of any kind, and nothing in
`src/Ft8Sharp/` was touched.**

---

UNIT:       241 — complete at task 6 of 6 — 2026-09-04 19:40
PHASE GOAL: Hamlet hears FT8 off the radio and displays the decoded text on screen.
UNIT GOAL:  An operator who has never seen FT8 can read the panel without being told what it means, in the order he chose, and start it again when he wants to.
ADVANCED:   **yes** — every fault the operator named is fixed, and the panel that asserted something untrue is gone.
NUMBER:     header and every row share column origins **0, 76, 124, 172, 226**; row list bounded at **300 px** with 201 rows; **35** vocabulary tests, and a payload off the table produces **nothing**.
DRIFT:      0 consecutive units without advance.

## 1. What Claude did

**Complete. Six tasks of six.** Nothing was dropped.

Hamlet confirmed against all four gate checks before the instruction was read:
`SHACK_FACTS.md` and `src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs`
present, `CoreHMI.sln` and `MURC.sln` absent.

Development machine, branch **`main`**, six commits, **every push succeeded**.
Root version **1.12.44 → 1.12.45** per HM-DEC-150; `Ft8Sharp` did not move.

**Nothing in this report is evidence about the radio** (FACT-004).

### Task 1 — what the panel is made of

**Why the columns did not line up.** The header is one `Grid` with
`ColumnDefinitions="Auto,Auto,Auto,Auto,*"` at `MainWindow.axaml:2932`. Each data
row was a **separate** `Grid`, declared inside the `ItemsControl.ItemTemplate`,
with its own identical string.

**Sibling grids share no measure.** Every `Auto` column sizes to its own content
and nothing else: the header's first column to `utc` at FontSize 11, a row's to
`214135` at FontSize 12. Two rows carrying `231` and `2438` in the `hz` column
disagreed with each other as well. It was not padding and not the font.

**`DigitalDecodes`** is an `ObservableCollection<DigitalDecodeRow>` at line 817.
`AddDecodeRow` appends, keyed on slot, frequency and message so a repeat inside
the window is swallowed. `ClearDigitalDecodesOnRetune` empties it when the dial
moves far enough. Readers: `HasDigitalDecodes`, `DigitalDecodedSummary`, and the
markup's `ItemsSource`. Nothing else.

**The growth**, from the shack machine's 21:41 slot — fourteen messages, and
sixty-three shown. There is no telemetry for 2026-09-04 in this tree, so this is
arithmetic on that one reading and the slot geometry rather than a measurement
taken here.

| | |
|---|---|
| 15 s slots | 4 a minute, 240 an hour |
| Rows an hour | **3,360** |
| Rows over a five-hour evening | **16,800** |

Committed as `bdc03b1`.

### Task 2 — the columns line up

Both grids carry identical pixel widths and `hz` is right-aligned.

| | |
|---|---|
| Header column origins | 0, 76, 124, 172, 226 |
| Every row's origins | 0, 76, 124, 172, 226 |
| `231` and `2438` right edges | 212 and 212 |

**Two approaches failed first and are written into the code so nobody repeats
them.** A shared `StaticResource` does not work — `ColumnDefinitions` has no
conversion from a resource string, and one shared instance would be worse than
two literals because definitions carry per-grid layout state. And the first set
of widths was too narrow: **a pixel column narrower than its content is widened
to fit**, so the drift came straight back at header 58 against row 72. Every
column is now wider than anything that can land in it.

What stops the two literals drifting is the test, which builds the real window
headless and asserts the origins agree. That is stronger than one declaration
because it checks the result rather than the intent.

Committed as `4ac422e`.

### Task 3 — the panel scrolls

The rows sit in their own `ScrollViewer` at `MaxHeight="300"` — `MaxHeight` and
not `Height`, so three rows are three rows tall rather than a mostly empty box.

| | |
|---|---|
| 1 row | list height 12 px |
| 201 rows | list height **300 px**, extent exceeds viewport |
| Parked at 216, a row arrives | still 216 |
| Scrolled to the end, a row arrives | 0 px from the end |

`FollowingScroll` **works out which end is the live end rather than being told**,
by watching where rows are inserted. So task 4's toggle could not get out of step
with the scrolling, and needed no change here.

Committed as `f94e7cf`.

### Task 4 — clear, and the sort toggle

Both controls sit in the panel header, where the waterfall panel above already
puts its capture button, and both hide while the table is empty.

**Within one slot, order is not the sort's to invent.** The direction reverses
**slots**; inside a slot the decoder's order is preserved exactly. That is why
the display is derived from a separate arrival list rather than reversed in
place — reversing would turn each slot's rows back to front too, and flipping
twice would not return what was there. Both are asserted.

**Three latent faults the toggle would have exposed**, each fixed here: inserting
now goes after any same-slot rows already at the top rather than at index nought;
the trim drops the oldest **arrival** rather than the first row on screen, which
under newest-first would have thrown away the row that just arrived; and
`DigitalDecodedSummary` read `DigitalDecodes[^1]` and would have named the
**oldest** row.

Committed as `9f84be0`.

### Task 5 — the message reads as three parts

The message draws as addressee, sender and payload — addressee muted, sender in
the decode green, payload in the primary text colour. **Colour is not the only
carrier** (§0.6): the order is fixed and each field says what it is on hover, so
the structure survives being printed in grey.

`Ft8Vocabulary` is the closed table, in one place. The eight ruled payloads get
their sentence. **Everything else gets nothing** — `599`, `QRZ`, `TNX`, `5NN` and
a compound callsign are each asserted to produce null. A grid square is "grid
square: where he is" and never a place: the test sweeps five grids against twelve
place words, and the sentence does not vary with the square.

`RR73` is matched before the grid test on purpose — it is letters then digits and
would otherwise read as a Maidenhead field.

**A message that is not plainly three fields is left whole**, with no colouring
and no field tooltips, because labelling the wrong half is worse than labelling
none of it.

Committed as `727cd05`.

### Task 6 — the trim says so, and the panel that lied is gone

**The cap was never the fault; the silence was.** `MaxDigitalDecodes` and its
trim were already here. 500 rows is about nine minutes of the band measured, and
the trim then runs all evening — 33.6 times over. It is not raised, and the
reason is rows rather than bytes: the list does not virtualise, and task 5 made a
row more expensive rather than less. The summary now says `oldest 7 dropped` once
it starts.

**"What people are saying" is removed.** It never had a feed and said "nobody
heard yet" while sixty-three real messages sat in the panel directly above it.
Checked before deleting: the panel was Digital-only, CW has no equivalent, and
`DigitalIdleText.Waterfall`, `.Decoded` and `.ModeStrip` are all still in use, so
only `.Saying` went with it. An orphaned `digital.saying` key in an existing
`settings.json` is inert, so no migration is needed.

Committed as `3d823a7`.

### Where the instruction did not match the tree

**Every claim in its verification list held**, with two additions it did not know
about:

- **A cap already existed.** The instruction's task 6 reads as though the panel
  had none. `MaxDigitalDecodes = 500` and its trim were both in place; what was
  missing was telling the operator.
- **That cap's own remark described a `ScrollViewer` that was not there** — "a
  plain `ItemsControl` inside a `ScrollViewer`". The items control was in a
  `StackPanel`. That comment was the missing-scrollbar fault, written down and
  not noticed.
- **The panel's family is `Lavender` in the markup**, where the rulings section
  says the decode family is green. Left alone; the header bar is text colour only
  either way, which is what the ruling governs.

### A value the engine does not expose

`Ft8Sharp.Ft8StandardMessage.TryUnpack` already produces the three message fields
separately, and `Ft8Decode` carries only the joined string. Task 5's split is the
view re-deriving what the decoder knew and did not pass on. **Reported rather
than reached for**, per the instruction: the engine was not touched, the reason
is written in `Ft8Vocabulary` where somebody will find it, and a later unit could
pass the fields through instead.

### Recorded under §12.1

**Nothing.**

## 2. What Tim should expect

Every item below is something he pointed at on his own screen.

- **The headers sit over their columns**, at every value, and `hz` is
  right-aligned so 231 and 2438 line up on their units.
- **The panel has its own scrollbar** and stops growing at about eighteen rows.
  The waterfall above it no longer gets pushed off the screen, and a slot's worth
  is visible without touching anything.
- **It follows new rows when he is at the live end and leaves him alone when he
  has scrolled away** to read a callsign he missed.
- **Two buttons in the panel header**: the order, which reads `newest first` or
  `oldest first` and says which state it is in, and `clear`. Both disappear while
  the table is empty.
- **It opens newest-first** and remembers that between evenings.
- **The message is three coloured parts.** Hovering the addressee or the sender
  says which is which; hovering a payload Hamlet knows says what it means, in
  ordinary words; hovering anything else says **nothing at all**.
- **The column headings explain themselves on hover**, including `snr`, which
  says the column shows a dash because nothing in this path measures one yet.
- **The summary carries the direction and the trim**, so a collapsed panel still
  says which end is live and whether rows have been dropped.
- **"What people are saying" is gone.**

**One thing that will look like a loss and is not:** the panel below the decoded
table has disappeared entirely. It never had a feed, and after task 5 the row
itself carries the vocabulary it was waiting for.

**Build:** clean, 0 errors, 0 warnings, both projects.

**Tests:**

| | |
|---|---|
| `Hamlet.App.Tests`, not-Views leg | **572 of 572** |
| `Hamlet.App.Tests`, Views leg | **66 of 66** |
| Engine, audio and FT8 channels | **242 of 243** |

**What will look wrong and is not:**

- **`CwAdjudicationTests.ASpeedChangeInRealisticAudio` is red**, named
  pre-existing by this instruction and by units 238 to 240. Not touched.
- **The 51 inherited CW reds are untouched** and remain on the parked list.
- **The engine project has no total**, for the third report running. This unit
  had no reason to run it — nothing outside `Hamlet.App` was changed — and the
  audio and FT8 channels were run as a sanity check.
- **Ten existing tests changed.** None is a repair; every one is a ruled change,
  and section 3 names the one that is a narrowed promise rather than a moved
  expectation.

**Pushed to `main`,** six commits.

## 3. What we should do next

**The three things this section was asked to lead with.**

**The mechanism that was misaligning the columns, named:** the header and each
data row were **separate `Grid`s whose `Auto` columns measure independently**.
Not padding, not the font, not a proportional typeface — there was no shared
measure of any kind, so the header sized to its labels and every row sized to its
own values.

**The measured row rate and the cap it argues for:** fourteen messages a slot on
the band measured, which is **3,360 rows an hour and 16,800 over a five-hour
evening**. That argues for keeping 500 rather than raising it — it is nine
minutes of the busiest band recorded here, the list does not virtualise, and task
5 made each row more expensive. **What it argues for far more strongly is saying
so**, which is what changed: the cap and the trim both predate this unit and the
operator was never told either existed.

**A payload off the table produces no tooltip rather than a fallback:**
confirmed, and asserted for `599`, `QRZ`, `TNX`, `5NN`, a compound callsign and
an empty payload. `Ft8Vocabulary.Explain` returns null and the row's
`PayloadHelp` is the empty string, which Avalonia renders as no tooltip at all.
Not "unrecognised", not a partial reading, nothing.

Then, in order:

1. **Measuring SNR**, which the instruction says is ruled next. The column and
   its full width are reserved for it and the header tooltip already says the
   dash means nothing measured one, so the measurement drops in without moving a
   pixel.
2. **Pass the three message fields through the engine** rather than re-splitting
   the string in the view, so `Ft8Decode` carries what `Ft8Sharp` already knows.
3. **One uncontended engine run to a summary line**, still outstanding from unit
   239.

## 4. What's blocking us

**Nothing is blocking the next step.**

One question is handed back:

> **The decoded panel is `Family="Lavender"` in the markup, and this
> instruction's rulings section describes the decode family as green.**
>
> Task 5 then coloured the sender field in the decode green, so the panel now
> carries a lavender chevron and a green field inside it. Both readings are
> defensible — lavender is the digital family under §0.6 and this is the digital
> tab, green is the decode family and this is decoded text — and the two are
> currently mixed on one panel.
>
> I did not change the panel's family, because §0.5 and HM-DEC-012 govern it and
> a family is not a session's to pick. The alternatives are: leave it, and accept
> a lavender header over a green field; make the panel green, which matches the
> ruling's own words and makes the tab two families; or colour the sender
> lavender, which loses the distinction from the payload.
>
> **Rejected already:** filling the header bar with either colour. Text colour
> only.

### Asks still outstanding

1. **The decoded panel's family colour** — raised above, 2026-09-04. **New.** The
   markup says `Lavender`; the sender field is green.
2. **The waterfall's dropped window and its late first row** — raised by unit
   240, 2026-09-03. The code is in `AudioSpectrumSource.Idle()`.
3. **`ReusableWindow`'s borrowed buffer** — raised by unit 239, 2026-09-03.
4. **`ProcessDelayForTests` as a hook or a seam** — raised by unit 238,
   2026-09-03.
5. **The tap's owner** — parked since work instruction 238, 2026-09-03.
6. **The divergence ruling on `Ft8Sharp` sensitivity** — owner's, open. Nothing
   in `src/Ft8Sharp/` was touched.
7. **Unit 237's Extensible-format conclusion** — the fix stands, the exoneration
   does not (FACT-004).
8. **Work instruction 231's four tree items** — the `PHASE_OUTCOME.md` header,
   the `RULES_AT` mismatch, uncommitted root paths, the Views stall.
9. **`validate-output.bat`'s permitted-spellings bug** — it has refused for
   eleven units. Not exercised this unit.
