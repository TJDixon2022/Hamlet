# Work instruction 282 — the composed paragraph, and the log shows his contacts

```
UNIT:      282
TASKS:     8 of 8, none dropped
NUMBER:    the status bar on a CW tune-in, 884 characters -> 0
           (890 of it on the mark beside it; nothing deleted)
ADVANCED:  no
DRIFT:     5 consecutive units without advance, carried from unit 281
VERSION:   1.12.181 -> 1.12.189
BRANCH:    main, pushed
```

---

## 1. What Claude did

**Surface: Claude Code, on the development computer, on `main`.** The prompt claimed
`PROJECT: Hamlet` and the tree confirmed it — `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` present, `CoreHMI.sln` and
`MURC.sln` absent, `Hamlet.sln` at the root. **Nothing in this report is evidence
about the radio**: there is no rig and no antenna here, and everything below is
measured off headless windows and synthesized data.

**Nothing was recorded under §12.1.**

### Task 1 — the composed paragraph

**The three lines the operator located are all real, and one is off by a method.**
`OwnedSettings.cs:65` and `ReceiverConditions.cs:102` are exact — both are the class
declarations that build it. `MainWindowViewModel.cs:8179` is
`var conditions = ReceiverConditions.ForBlock(here);`, inside the right method; **the
assignment that puts it on the bar is 8207**, and there is a second at 8137 for
mode-follow narration. Reported, not repaired.

**It is 884 characters**, measured from the nine rows the shipped
`mode-receiver-conditions.json` states for CW and composed through
`ReceiverSetupVoice.Say`. FT8 composes 448 the same way.

**The line gains a kind, and it defaults to speaking.** Thirty-one sites write
`StatusText`; a classification that guessed wrong in the quiet direction would hide a
fault, so a site is a fault unless deliberately marked as narration — which also
means **a new call site added by a session that never read the file speaks.** Twelve
sites marked, all of them Hamlet reporting something that worked.

**And the paragraph folds three admissions in with the narration.** A setting the
radio would not confirm, one Hamlet could not read, and one it cannot reach at all
are all in the same string, so narrating the whole thing would have hidden them
behind a hover — the thing the instruction forbids in as many words.
`ReceiverSetupVoice.Admissions` filters the same clauses out of the same results,
**not written a second time**, and those go on the bar while the whole line stays on
the mark.

`ReceiverConditions.ForMode` is the lookup this class's own remarks describe. Until
now the only way to ask what a mode needs was through `ForBlock`, which wants a whole
`Neighborhood` — part of why nobody had measured what these rows compose to.

### Task 2 — a ceiling nothing can quietly exceed

Ten surfaces, in `HowMuchTheApplicationSaysTests` where the instruction puts it, each
capped at **its measured figure plus 100 characters rounded up to the next 50**. The
margin is asserted as arithmetic, so raising a ceiling means re-measuring and saying
so rather than nudging a number.

**The margin is chosen against the fault, not against the noise.** A paragraph is 448
to 884 characters on the shipped rows, so 100 cannot hide one — and the test that
matters says it directly: the CW tab measures **611** and would have measured
**1,495** with the paragraph on the bar, against a ceiling of **750**.

**Two things had to be fixed before a ceiling could mean anything.**

- **The measurement was racing a network lookup.** With the licence class unset the
  panel resolves it from the callsign as it is built, and whether that lands before
  the layout is pumped depends on how long the run has been going: **the Voice tab
  read 541 unresolved and 623 resolved, the same window in two states.** The fixture
  now carries a settled profile, which is also the screen he operates.
- **The byline is a die roll.** Forty-five lines of 20 to 104 characters, an
  84-character swing between launches — more than the whole margin. It is named in
  the markup and left out of the ceiling walk, and still measured and printed,
  because it is on the screen and a flourish is not prose about the radio.

**One draft is recorded rather than quietly replaced**: it narrated the paragraph and
then measured, and the rig heartbeat overwrote the status line in between, so it
asserted something true about a screen that no longer held it. A test that passes for
the wrong reason is worse than no test.

**Settings is capped at 1,750 where it stands.** It is not reduced and this unit
makes no claim that it is small.

### Task 3 — the log shows the station's grid

`GRIDSQUARE` has been written since unit 274 and never shown. **`their grid`, then
`my grid`**, labelled apart rather than one column called `grid`. A record carrying
none still says `not recorded`, and the test asserts it is not quietly filled in from
the column beside it. Both directions go through `AdifLog` rather than a hand-built
record, because the question is whether the field survives the file.

### Task 4 — why `sent` is empty

**It is the first of the two: not recorded.** The path carries both reports when both
were observed, so a record missing one is a record where one was never observed.

Proved by driving the whole path. A ledger fed a real exchange — his grid, their
`-09`, his `R-12`, their `RR73`, his `73` — produces an entry carrying **sent −12 and
rcvd −09**. `Ft8ContactLogEntry.cs:88` reads them off `record.Sent` and
`record.HeardToUs`, and `Ft8ContactLedger.RecordSent` has one call site,
`MainWindowViewModel.cs:10234`, gated on `run.Sent`.

**What is not recorded, and where it would have to be.** A sent report exists only
where Hamlet transmitted the message: a station worked on another program leaves that
side empty and fills the received side normally, **which is the shape of four of his
five records**. And a contact whose messages carried no number — a grid, a roger, a
sign-off — has no report either way, because not one of those three contains one.
**That one of the five does carry a sent report is itself evidence the mechanism
works.** Left alone as instructed; no record back-filled or edited.

**And the *Have a look* button.** It expanded a panel not in the tree, so pressing it
did nothing while looking exactly like a button that works. The command now refuses,
the button is drawn as unusable, and the reason is on its hover. **Disabled rather
than removed**: HM-OPEN-087 is Tim's, and taking the button away would tidy the
evidence out of sight before he has ruled.

**One correction inside that task, recorded rather than quietly fixed**: the first
draft of that tooltip sent him to the Radio menu for the receive help. That menu
carries Connect, Favorites and Recent and never carried it. **A tooltip naming a
screen that does not exist is the same fault as the button, one level along.** The
test now asserts it does not.

### Task 5 — the log stops truncating

`not recorc` was a 64-pixel column carrying a twelve-character word. Measured
unclamped: that word wants **120 pixels**, nearly twice what it was given.

**Pixels were the wrong unit.** How wide twelve characters of Consolas are depends on
the machine, so a number chosen here clips on his screen or wastes space. The columns
are `Auto` now — the Grid is asked instead of told — under **one shared size scope**,
so the header cannot drift off its own cells.

| | call | when | band | mode | sent | rcvd | their grid | my grid | notes |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| **Before** | 96 | 150 | 64 | 64 | 96 | 96 | 88 | — | * → **10** |
| **After** | 70 | 200 | 130 | 130 | 130 | 130 | 130 | 130 | 160 |

Header and rows agree to the pixel on all nine, asserted.

**What still does not fit, said rather than hidden.** Eight content columns plus notes
come to **1,210 px** under this machine's font and the window is 1,000, so **the table
scrolls sideways** — header inside the same scroller as the rows — rather than
squeezing a column to nothing, and notes has a 160-pixel floor. On his screen Consolas
is narrower than the fallback measured here, so it will often fit without scrolling;
the window went 900 → 1,000 to make that likelier.

**One false red of the test's own making, recorded**: the first run compared
`DesiredSize` against `Bounds` and reported nine clipped cells. `DesiredSize` includes
the cell margin and `Bounds` does not.

### Task 6 — carried from unit 281

**Both landed and nothing was built.** All seven of unit 281's tests are green: a sent
message carries no sender tooltip and no payload explanation, a received one does, the
entity reads *the United States*, and a received row's tooltip reads *W3YNI is calling
anyone. He is in the United States, in grid FN20, 250 miles away on a bearing of 87
degrees* — no mention of Settings.

**What was added is the guard that should have existed when the fault was fixed.**
Three sites build a row and all three go through `WithOperatorGrid` — but counting
call sites is exactly the check that passed for six units while `KeepSentRow` sat
outside the door. The new sweep asks the bound lists instead, and fails if it sweeps
nothing.

### Task 7 — what moved, listed and measured

`docs/unit282-what-was-removed.md`. **No fact was removed**; nothing was deleted in
this unit at all.

### Task 8 — the outcome entry

Appended by `tools\arbiter\outcome-append.bat`, exit 0, as `UNIT 282 - STEP E`. Units
273, 274 and 275 were not back-filled.

### No shell refusals

Every heredoc carrying an apostrophe went through a script file from the start.

---

## 2. What Tim should expect

**The paragraph is off the bottom of the window.** Tune into a CW block and the status
bar stays empty. Hover the small ring at its left-hand end and the whole thing is
there, word for word — what Hamlet turned off, and why, all nine of them.

**Except when something is wrong.** If the radio would not confirm a setting, or
Hamlet could not read one, or there is one it cannot reach at all, **that part is on
the bar without being hovered**, and only that part. Same for a port that will not
answer, a mode that would not set, a capture that could not be written.

**His log says who he worked and where they were.** A `their grid` column beside `my
grid`, and no more `not recorc` — the columns size themselves to what is in them now.
If the table is wider than the window it slides sideways rather than crushing the
notes column.

**The *Have a look* button on the CW tab is grey.** That is deliberate and it is
correct: the panel it opened is not on any screen, so it never did anything. Hover it
and it says so.

### What will look wrong and is not

- **An empty status bar is the normal state now.** The ring at its left is the
  hover, not a broken control.
- **`sent` still reads `not recorded` on most of his records**, and that is true
  rather than a bug — see task 4. Nothing was back-filled.
- **The log window is 1,000 px wide** rather than 900.
- **`AboutWindow`, Settings and the rig diagnostics are unchanged.** Settings is
  capped, not swept.

### The build and the tests

Build clean, no warnings. **This unit ran no suite** (HM-DEC-155): only the tests it
wrote or rewrote, filtered by name, foregrounded.

| Class | Result |
|---|---|
| `TheStatusBarStopsLecturingTests` | 4 of 4 — new |
| `HowMuchTheApplicationSaysTests` | 5 of 5 — 4 new, ceilings added |
| `TheLogShowsBothGridsTests` | 3 of 3 — new |
| `TheLogDoesNotClipItsColumnsTests` | 4 of 4 — new |
| `WhyTheReportsAreEmptyTests` | 4 of 4 — new |
| `TheOfferButtonCannotBePressedTests` | 2 of 2 — new |
| `TheGridInSettingsReachesEveryRowTests` | 5 of 5 — 1 new |
| `TheSenderTooltipIsNotAboutHimTests` | 3 of 3 |
| `AdviceWaitsToBeAskedTests` | 5 of 5 |
| `TheCountWearsARankTests` | 5 of 5 |
| `TheTurnIsARingTests` | 7 of 7 |
| **Total** | **47 of 47** |

**The ten inherited reds in `TheMessageReadsAsThreePartsTests` were not touched**
(`HM-OPEN-088`, raised by unit 281), nor the CW set, nor the `Ft8Sharp.Deep` tripwire.

**Eight commits, all on `main`, all pushed**, 1.12.181 → 1.12.189. Nothing
uncommitted.

---

## 3. What we should do next

### The status bar as it now renders, and the paragraph quoted

The bar draws **nothing** on a clean tune-in. The count, the belt ring and the glyphs
are all that is left on it. Hovering the ring gives, verbatim:

> tip — I turned the auto notch off because it hunts steady tones and Morse is a
> steady tone, turned the manual notch off because the same trap, under your hand
> rather than automatic, turned the noise blanker off because it chops holes in keying
> instead of in the noise, turned the noise reduction off because it was built for
> speech and it mangles a keyed envelope, set the AGC to fast because slow gain rides
> over the gaps and hides the keying, set the RF gain to 100% because anything less
> throws away signal the decoder needs, set the squelch to open because a gate that
> shuts between elements is fatal to a decoder, set the attenuator to off unless the
> front end is overloading because twenty decibels thrown away on a signal that had
> none to spare, and set the preamp to preamp 1 above 40 m, off at 40 m and below
> because below 40 m the noise arrives with the signal and gain adds both.

And where a setting would not confirm, this stays **on** the bar:

> I asked for the noise reduction to be off and the radio did not confirm it, so I do
> not know where it is now.

### The ceilings, and the red one gives

| Surface | Holds | Ceiling |
|---|---:|---:|
| SettingsWindow | 1,628 | 1,750 |
| RigDiagnosticsWindow | 1,346 | 1,450 |
| MainWindow — Digital tab | 1,201 | 1,350 |
| AboutWindow | 726 | 850 |
| LogContactWindow | 716 | 850 |
| MainWindow — CW tab | 611 | 750 |
| MainWindow — Voice tab | 601 | 750 |
| FavoritesWindow | 280 | 400 |
| ContactLogWindow | 246 | 350 |
| DecisionLogWindow | 196 | 300 |

Margin: **measured + 100, rounded up to the next 50**, asserted as arithmetic. Adding
a 400-character paragraph to the Voice tab takes it from 601 to 1,001 against a
ceiling of 750, and the test says:

> `a 400-character paragraph did not take this surface over its ceiling, so the
> ceiling is too loose to catch the fault it is for` — the message it would print if
> the margin were ever widened past usefulness.

### A log row with both grids

> `IK4LZH  2026-09-07 21:41:30  20m  FT8  not recorded  not recorded  JN54  FN00DJ`

`their grid` JN54, `my grid` FN00DJ, and `not recorded` whole in both report columns
rather than `not recorc`.

### Why `sent` was empty

**Not recorded.** The path carries a report whenever one was observed; four of his
five records are contacts Hamlet did not transmit for, or exchanges with no number in
them either way. Nothing to fix, nothing back-filled.

### Then

1. **Rule on HM-OPEN-087** — thirteen dead widget templates and now a deliberately
   disabled button pointing at them.
2. **Look at the status mark on a real screen.** Same ask as unit 281's: a 14-pixel
   ring is quiet by design, and it is now carrying the whole receive narration.
3. **Rule on HM-OPEN-088** — the two test classes that contradict each other about
   `SenderHelp`.
4. **Then back to the radio.** Every bench step is closed.

---

## 4. What's blocking us

Nothing blocks the next unit.

### Whether the status mark is findable

**Ruling asked for:** whether a 14-pixel ring at the left of the status bar is enough
to tell him there is something to hover.

**Why:** it now carries the entire receive-setup narration, which is the most useful
thing Hamlet says about his radio. **Nobody has looked at it on a screen** — it is
measured, tested and unseen. Unit 281 asked the same question about the Settings marks
and it has not been answered; this makes it larger.

**Rejected:** guessing at a bolder mark. Only he can say whether he would notice it.

### The dead widgets, and the button now pointing at them

**Ruling asked for:** `HM-OPEN-087`, carried. Thirteen unreferenced `widget.*`
templates, `widget.receiveHelp` among them.

**Why:** this unit disabled the button rather than removing it, precisely so the
evidence is in front of him. It now reads as a dead control on the CW tab, which is
honest and is not a resting state anybody wants to keep.

**Rejected:** deleting the templates or rehoming the widget. Both are the ruling.

### What `SenderHelp` is for

**Ruling asked for:** `HM-OPEN-088`, carried. Two test classes assert opposite things
about the same property, and ten inherited reds hang on it.

### Asks still outstanding

Carried forward per HM-DEC-139.

1. **Two issues of one work-instruction number.** Unit 271, and 252 before it.
   **The author's error.** No unit action.
2. **`PM95` reads *southern Japan***, unit 271. **Not a defect.**
3. **`HM-OPEN-083` and `HM-OPEN-084`.** In `OPEN_ISSUES.md` by HM-DEC-140.
4. **Three pixels.** **Waiting on Tim.**
5. **`dt` and `hz` were never on the mine list.** **Waiting on Tim.**
6. **Where an outcome entry goes when the unit it corrects has none.** **Tim's.**
7. **`PHASE_OUTCOME.md` is written by a tool no unit is told to run.** **The
   author's.** Units 281 and 282 were both told to run it and did.
8. **Where the repeat fold stops**, unit 277. **Still open.**
9. **Whether a faded row needs a second carrier of its meaning**, unit 279. **Tim's.**
10. **Whether counting subjects is a new assertion**, unit 279. **Tim's.**
11. **Whether the fade is still obvious now the row is a bubble**, unit 280. **Not
    measured.**
12. **`HM-OPEN-087`** — the dead widget templates. **Unit 282 disabled the button
    that pointed at them and touched no template.** **Still Tim's.**
13. **Whether a hover mark is findable on a real screen**, units 281 and 282.
    **Still not seen by anybody**, and it now carries the receive narration.
14. **Whether `AboutWindow` is in scope for the terseness ruling**, unit 281. **A
    judgement reported rather than assumed.** Unit 282 capped it and did not sweep
    it.
15. **What `SenderHelp` is for**, unit 281. `HM-OPEN-088`. **Ten inherited reds hang
    on it.**
