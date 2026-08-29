UNIT:       043 (2026-08-29) — stopped at task 7 of 7 — 2026-08-28 23:05
PHASE GOAL: Readable CW on the operator's screen — eighty percent of a strong signal read correctly, first time.
UNIT GOAL:  Fix four faults tonight's captures caught — letters from an unchosen pitch, admission refusing a real station, the sweep's 25 Hz grid, and state carrying across a dial move.
ADVANCED:   no — the goal task is task 2 and its audio is not in the tree; tasks 5 and 7 landed and neither is the goal.
NUMBER:     none — task 3's number is the unit's scoreboard and it cannot be measured without `-020938`.
DRIFT:      3 consecutive units without advance  (was 2)

## 1. What Claude did

**Stopped at task 7 of 7, with four of the seven tasks blocked on audio that is
not in the repository.**

**Tasks 2, 3, 4 and 6 were not done, and faults 1 to 3 of task 1 could not be
reproduced.** Every acceptance line in them is defined on files that do not
exist here. **This is not the drop the order named** — task 7 was the drop
candidate and it is one of the two that did land.

Development computer, prompt claimed `PROJECT: Hamlet`, branch `main`, version
`1.12.6` unchanged. **Nothing here is evidence about the radio**: no radio was
connected.

### The blocker, first, because it decides most of the unit

**None of the five 2026-08-29 captures is in the tree.** The order says they
should be and asks which are missing. All of them:

| | |
|---|---|
| `cw-2026-08-29-020541` | missing |
| `cw-2026-08-29-020616` | missing |
| `cw-2026-08-29-020707` | missing |
| `cw-2026-08-29-020809` | missing |
| `cw-2026-08-29-020938` | missing |
| `cases-2026-08-28.txt` | missing |

The newest audio in `tests/fixtures/cw/captured/unadjudicated/` is
`cw-2026-08-28-005243`, and the roster files present are `cases-2026-08-24.txt`,
`-26` and `-27`. Nothing named `2026-08-29` exists anywhere in the tree.

**What that costs**: task 1's faults 1, 2 and 3; task 2 entirely, including its
acceptance that `-020809` and `-020938` emit no letters; task 3 entirely, which
is the measurement the report was told to lead with; task 4's acceptance, which
is stated only on `-020938`; and task 6 entirely.

### Task 1 — what could be established

**Both suites, run whole and recorded before anything changed:**

| | before | after this unit |
|---|---|---|
| engine | **28 failing, 1944 passing, 1972 total** | one regression, found and fixed; see section 2 |
| app | **511 passing, 0 failing** | 519 passing, 0 failing |

The order cites unit 041 at "engine 28 of 1916, app 509 of 509". The failing
count is the same 28 and the totals have grown — the app's extra two were added
by unit 045 last night, and this unit added eight more.
`AConfirmedModeWriteFoldsTheDataVariantTooAsync` did not fire as an intermittent
in any run here.

**The engine suite excludes `TheGateHasItsOwnWindowNowTests`**, which crashes the
test host rather than failing (HM-OPEN-061, raised and reproduced on an older
tree). That is five tests, and it is why the totals read 1972 rather than 1977.

**Fault 4 does not reproduce as described, and the real defect is its opposite.**

`CwDecoder.Retuned()` has existed since HM-DEC-111 (`CwDecoder.cs:358`) and is
already called on a dial move (`MainWindowViewModel.cs:5512`). It already cleared
the measured pitch, the operator's lock and **`tonePeak`** — so the order's claim
that a peak measured on the first station was still decaying into later readings
**is not true of this tree**. The comment at the call site records the 2026-08-26
QSY that put it there.

**What did survive a move**, each named with its file and line as the task asks:

| field | where | what it is |
|---|---|---|
| `_probabilistic` | `CwDecoder.cs:28` | the twelve-second envelope window, the speed hypothesis fitted to it, the settled mark and the leading edge |
| `_charactersEmitted` | `:81` | characters the sidecar reports |
| `_charactersUnsure` | `:82` | of those, how many were marked |
| `_elementsResolved` | `:83` | elements the sidecar reports |
| `_toneLatched` | `:63` | whether there is a tone worth calling one |
| `_snrHistory` / `_snrWrite` / `_snrFilled` | `:89–91` | the rolling signal-to-noise figures |
| `_hasFollowed` / `_lastFollows` | `:59–60` | whether the tracker has ever moved station |
| `_lastPitchHz` | `:45` | where the tracker was at the previous reading |
| `_reReadAt` / `_lastMeasuredForReRead` | `:854`, `:856` | the re-read bookkeeping |

**And the defect the order did not look for: `Retuned()` fired on every change to
the dial, including a ten-hertz one.** `OnFrequencyHzChanged` calls it
unconditionally, so nudging a station a couple of hundred hertz to centre it threw
away the pitch the survey had just measured on that station, the held peak, and
the window being read. **The station a nudge is aimed at is the station already
being read**, which is precisely what task 5 warns must not happen — and it was
already happening.

### Task 5 — a frequency change clears and resets

Both halves built and tested.

**The reset is now complete.** `Retuned()` additionally restarts the probabilistic
stream, and zeroes the counters, the tone latch, the follow state and the
signal-to-noise history. The test for it is the ruling's own words: a retuned
decoder is compared field by field against one that has never listened, so nothing
can be added later and quietly missed.

**A nudge is no longer a move.** `MainWindowViewModel.NudgeHz` is 500 Hz — the CW
filter's own width, so inside it the receiver is passing the same signal.
**Provisional and marked as such** (§12.4); the number is raised in section 4 with
three candidates costed.

**The decoder still reads after a move**, which is tested rather than assumed: a
reset that leaves it unable to read is not a reset.

**One committed test asserted the opposite of the ruling and was re-expressed
rather than deleted**, per the order's own instruction not to delete an anchor.
`TheReleaseDoesNotThrowAwayWhatIsNotAboutTheFrequency` held that *the speed the
tracker has learned is a fact about the operator's ear and his habits rather than
about a frequency*. The ruling names the speed hypothesis explicitly as something
to reset. The test is now `TheReleaseStartsTheReadingFresh` and carries the old
reasoning in full, with what makes it survivable: **the old behaviour was
protecting against a reset that fired on every dial click**, and now that a nudge
is not a move, the speed is kept exactly where the old argument wanted it kept.

**The transcript break was not built** — the order says the wording is Tim's and
must be proposed rather than settled. Proposed text is in section 4.

### Task 7 — CW's receive conditions

**Unit 042 did land a per-neighborhood mechanism and this inherits it.**
`ReceiverConditions.ForBlock(Neighborhood?)` reads
`data/bands/mode-receiver-conditions.json` and returns `ReceiverCondition` records
carrying `Control`, `Field`, `Wanted`, `WantedText`, `Says`, `Because`,
`Confirmed` and `Confirm`. It covered **FT8 and FT4 only** — CW had no rows at
all. **No second mechanism was built.**

Four CW rows added as data, each with its reason:

| control | wanted | why |
|---|---|---|
| attenuator | off | twenty decibels thrown away on a signal that did not have them to spare |
| preamp | preamp 1 | on a quiet night the receiver's own noise is bigger than the band's |
| noise blanker | off | a dit's leading edge looks enough like a crack that the blanker bites into the elements |
| AGC | slow | fast winds the gain up in every gap between elements and lifts noise into the spaces the decoder measures |

**The AGC row caught an error on the way in.** It was first written as 2 for
"slow"; `ReceiveAdvice.AgcFast` is 1 and the cited encoding is 00–03 with 00 off,
so slow is **3**, which is what FT8's existing row uses. Corrected before commit.

**Two of the four ship `confirmed: false`**, so they are spoken and never
written (§12.4). The AGC, because which setting reads better on a deep fade is a
question about this receiver and this operator and nobody has measured it here.
And **the preamp, which was demoted after it was written**: the order names the
attenuator and not the preamp, so its value was this session's inference from
"it sat at 20 dB with the preamp off", and an inference is not something to write
to somebody's radio. Only the attenuator, which the order names directly, and the
noise blanker, whose reasoning matches FT8's own confirmed row, may be written.

**Two things the order asked for that are not in the data file, and both are
recorded in its `unknowns` block**: the attenuator's condition on the live
overflow reading, which is a change to 042's mechanism rather than a row; and
whether `CwPitch` should follow an admitted station's measured tone, which the
order itself says to raise rather than decide. Both are in section 4.

**One limitation of the inherited mechanism, named and not fixed** (§12.6): the
lookup is by the block's short name, so `CW` matches and `CW DX` and `QRP` do not.
FT8 and FT4 have the same property, so this is 042's shape rather than something
this unit introduced.

No decision was recorded under §12.1.

## 2. What the owner should expect

**On a frequency where nothing is happening the terminal still fills with
letters** — task 2 is the change that would stop it and its audio is not here.
**Moving the dial does now start the decoder fresh** rather than carrying the last
station's speed, window and counters along with it, and fine-tuning a station no
longer throws away what has just been measured about it.

What is now true of the tree:

- A dial move of 500 Hz or more resets the decoder completely; anything smaller
  resets nothing.
- A capture sheet written after a move describes that frequency in every field.
  It previously carried the elements and characters counted somewhere else.
- CW blocks now state four receive conditions with their reasons, through the
  same mechanism FT8 uses.

**What will look wrong but is not:**

- **The engine baseline is still 28 failing.** Same 28 as before this unit.
- **Two committed tests were reversed by this unit's rulings, and both are
  re-expressed rather than deleted.** `TheReleaseDoesNotThrowAwayWhatIsNotAbout​TheFrequency`
  held that the learned speed must survive a QSY and is now
  `TheReleaseStartsTheReadingFresh`. `TheDigitalBlocksStateWhatTheirModeNeeds`
  walked every block that stated anything — the same set as the digital blocks
  until CW started stating something — and now walks the digital blocks, with
  `TheMorseBlocksStateWhatMorseNeeds` asserting CW's four just as hard. **If
  either ruling did not intend to reverse a decision, these are the two lines to
  look at.**
- **The one test that regressed and was fixed** is
  `Explore.TheBlockStatesWhatTheModeNeedsTests.TheDigitalBlocksStateWhatTheirModeNeeds`.
  It went red because CW's new rows brought CW blocks into a loop that asserted
  the digital four fields. It passes now.
- **The transcript break is not built.** Its wording is yours and is proposed
  below rather than assumed.
- **The full engine suite has no single clean run in this report**, and the
  crash turned out to be wider than HM-OPEN-061 recorded. Excluding
  `TheGateHasItsOwnWindowNowTests` got a complete run before this unit's changes
  — **28 failing, 1944 passing, 1972 total** — and a second run afterwards that
  found the one regression named below. A later targeted run over everything this
  unit touched passed **544 of 544 with none failing** and then crashed anyway, on
  a filter that did not include that class at all. **So excluding it is not a
  workaround, it is a way of getting further.** The issue is updated with that.
- **The evidence for "no regression" is therefore assembled rather than single.**
  The one test that did go red is named below and now passes; the 544-test run
  covers `Explore`, the held-pitch tests, the retune tests and the new ones; the
  app suite is 519 of 519.
- **This order is numbered 043 and says it follows unit 042.** Work instructions
  numbered 043, 044 and 045 were executed and pushed on 2026-08-28, in commits
  `0fc1496` through `df33092`. The author says they have not seen 042's report;
  they appear not to have seen those three either.

## 3. What you should see

**Task 3's number, which this section was told to lead with, could not be
measured: `cw-2026-08-29-020938` is not in the tree, and neither are the other
four.** How often a strong keyed carrier is refused admission across the corpus is
the size of what is still wrong, and it remains unmeasured.

What did come out of the evening is that **fault 4 was diagnosed backwards, and
finding that was worth more than the fix.** The held peak was already being
released on a QSY — that repair went in on 2026-08-26. What nobody had noticed is
that the same release fired when the dial moved ten hertz, so every time the
operator centred a station he threw away the pitch that had just been measured on
it. The wandering pitch the order attributes to state carrying **forward** is at
least partly state being destroyed **too often**.

Both now behave: a real move clears everything, a nudge clears nothing.

And CW finally states what it needs from the receive side, in the same place FT8
does, with the attenuator first — which is the setting that sat at 20 dB all
evening while the station faded to nothing.

## 4. What's blocking us

**The audio, first. Everything else here is a question.**

> **Tasks 2, 3, 4 and 6 need the five captures of 2026-08-29 in
> `tests/fixtures/cw/captured/unadjudicated/`**, with their sidecars. Nothing in
> those tasks can be attempted without them, and task 3's measurement is the one
> the next unit is meant to be built from.

Then three rulings, most-blocking first.

> **A dial change of `N` hertz or more is a move; anything smaller is a nudge.**
>
> Three candidates, costed against what is knowable here — the operator's own
> moves on 2026-08-29 were 8.8 kHz and 13.0 kHz, and the CW filter is 500 Hz wide.
>
> | candidate | what it treats as a move | what it costs |
> |---|---|---|
> | **500 Hz**, the filter's width | both his moves; nothing inside the passband | a station tuned across more than half the filter resets — rare, and arguably right |
> | **2 kHz** | both his moves | a genuine hop to a station two kilohertz away keeps the old speed and window for a refill |
> | **8 kHz** | both his moves, barely | a move within a band segment reads as a nudge, which is the fault this ruling exists to fix |
>
> **Shipped at 500 Hz and marked provisional**, because it is the only one of the
> three with a physical meaning rather than a round number: inside the filter the
> receiver is passing the same signal, so there is nothing new to hear.
> **Rejected: deriving it from the tracker's search band**, 300–900 Hz, which is
> about where a tone can sit in the audio rather than about how far the dial
> moved.

> **What the transcript break says when the dial moves.** The wording is yours
> (§12.1) and this is a proposal, not a decision.
>
> Proposed, on one line, in the terminal's existing prose:
>
>     — 7.050.2 MHz — anything below this was heard on 7.037.2 —
>
> **The transcript is not erased**, per the order; the break exists so text from
> the old station stays readable and nothing new can be confused with it.
> **Rejected: erasing on a move**, which the order forbids and which would take
> away something the operator may still be reading. **Rejected: a bare rule with
> no frequency on it**, which says a break happened and not what changed.

> **Whether the attenuator's wanted value may depend on a live reading.**
>
> The order says the attenuator comes off unless the front end is actually
> overloading, and Hamlet reads that flag four times a second. Unit 042's file
> states one wanted value per control, so what is stated today is the quiet
> band's answer with the overflow condition written into its reason.
> **Making it conditional is a change to 042's mechanism**, and this unit was told
> not to build a second one, so it is raised rather than taken.
>
> **And separately: whether `CwPitch` should follow an admitted station's measured
> tone.** The order raises this itself. It read 600 Hz while the stations measured
> at 542 and 802.7, so the filter was centred on neither — and it changes what the
> operator hears, which is a different kind of write from the others.

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140.

1. **The five captures of 2026-08-29 are not in the tree.** Raised here first.
   Four tasks wait on them.
2. **The pedestal ranking is measured at 34 of 44 and unbuilt** — unit 045's order
   said it becomes its own unit if the reference lost, and it lost.
3. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
4. **The `reading` line's span wording needs approval.**
5. **Two stations closer than 125 Hz are not named.**
6. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
7. **Nothing checks that deleting a surface is not deleting a capability** — the
   favourites list is gone and the operator found it by hand.
8. **A capture sheet carries a score of −68562.4** (`cw-2026-08-28-005158`).
9. **The engine test host crashes, and not only on
   `TheGateHasItsOwnWindowNowTests`** (**HM-OPEN-061**, widened today: a run that
   excluded that class crashed anyway after 544 passing tests). Owned by Claude,
   not waiting on a ruling.
