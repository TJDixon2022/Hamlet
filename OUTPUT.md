UNIT:       047 — complete at task 6 of 6 — 2026-08-29 11:43
PHASE GOAL: Readable CW on the operator's screen — reading one mode at ninety-nine percent, measured against what was actually sent.
UNIT GOAL:  One owned-settings contract so two conversations cannot overwrite each other's radio, with CW's row filled in.
ADVANCED:   no — this unit is the receive side and the contract, not the decoder; the corpus score is unchanged by design.
NUMBER:     yield 0.763, precision 0.761 — unchanged, as the order requires.
DRIFT:      2 consecutive units without advance  (was 1)

## 1. What Claude did

**Complete: all six tasks.** Task 6 was the drop candidate and it ran.

Development computer, prompt claimed `PROJECT: Hamlet`, branch `main`, version
`1.12.6` unchanged. **Nothing here is evidence about the radio**: no radio was
connected, and every write below was exercised against a scripted one.

**The eight captures of 2026-08-29 are still not in the tree** — a fifth
consecutive unit. Nothing in this unit needed them.

### Task 1 — every write Hamlet makes on its own initiative

**Two automatic write paths, and that is itself the finding.**

| path | trigger | what it writes |
|---|---|---|
| `MainWindowViewModel`, `:5779` | the dial settles in a block and mode-follow decides | **mode, data flag and filter slot**, one frame, command `26` (§4 p. 19-11) |
| `ReceiverSetup.ApplyAsync`, via `EstablishReceiveConditionsAsync` `:5834` | the same settle, once per block (`_conditionsSetForBlockHz`) | **whatever the block's row states**, one write per setting |

**Twenty-seven writes are defined** in `CivWrites` and **only those two paths fire
automatically.** Everything else is operator-triggered through
`ReceiveHelpViewModel`. `16 65` IP+ remains excluded because it cannot be read
back (HM-DEC-084) and **no second such write was found**.

**Nothing can reach the transmitter from either path.** `CivWrites.BreakIn`,
`BreakInDelay` and `AntennaTuner` exist and are not on the owned list, are not in
any neighborhood row, and are reachable only by an explicit operator action. The
new test `WhatIsNotOwnedStaysOffTheList` asserts break-in is off the list rather
than trusting it.

**Unit 042 landed the mechanism and unit 043 gave CW its first four rows.**
`ReceiverSetup.ApplyAsync` already did read-before-write, `AlreadyRight`,
`LeftToTheOperator` via a hand-memory, and `SpokenOnly` for rows with no cited
byte.

**And it did not read back, which is task 4's gap.** It took the CI-V
acknowledgement as confirmation and then recorded `condition.WantedText` as the
value the setting now holds. **`FB` is the radio saying it accepted the frame, not
that the setting holds what was asked** — so the write was asserting its own
success, on the surface built to prove what Hamlet did. Fixed in task 4.

### Task 2 — the owned list, and the coverage table

`OwnedSettings.All` is twelve entries, each with its rig field and its §4
citation, **as data rather than scattered code**. The scope span is named
separately as spoken-only, because §4 carries no CI-V command for it and a row may
state it without a byte going out.

**Three answers, and absent is one of them.** A row states a value, defers to the
operator, or is silent — and silence leaves the setting alone and is **reported,
not failed**, because the digital rows belong to another conversation.

**The coverage table, which is this unit's handover:**

| block | stated | deferred | absent |
|---|---|---|---|
| **CW** | **9** | 0 | 3 |
| FT8 | 3 | 1 | 8 |
| FT4 | 3 | 1 | 8 |

Absent on CW: **mode and data flag, filter slot, filter width.**
Absent on FT8 and FT4: those three plus **manual notch, preamp, attenuator, RF
gain, squelch.**

**The three absent everywhere are a real gap in the contract, not in the
behaviour.** Mode, filter slot and filter width *are* written automatically — by
the other path, `SetModeAsync`. **So two mechanisms answer for the twelve, which
is the fragmentation the contract exists to end**, one level up. Reported rather
than merged: folding the mode write into `ReceiverSetup` touches mode-follow, and
unit 048 owns everything near the decoder.

### Task 3 — CW's row

Nine settings, each with its reason as text in the file (unit 040's pattern):

| setting | CW | |
|---|---|---|
| auto notch | **off** | it hunts steady carriers and a keyed Morse signal is one |
| manual notch | off | the same trap, sitting wherever it was last put |
| noise blanker | off | a dit's leading edge looks enough like a crack that it bites |
| noise reduction | off | built for speech; it smears the edges the decoder measures |
| AGC | **fast** | it tracks the keying rather than pumping across it |
| RF gain | 100 % | anything less throws away signal the decoder needs |
| squelch | open | a gate that shuts between elements hands over a chopped envelope |
| **attenuator** | **rule: `overflow`** | off unless the front end says it is overloading |
| **preamp** | **rule: `band`** | off at 40 m and below, on above |

**The two rules are rules and not constants**, which the file could not express
before. `ReceiverCondition` gained a `Condition`, and `ReceiverSetup` resolves it
against a live reading — the overflow flag for the attenuator, the frequency for
the preamp, with the boundary at 10 MHz. **A rule whose reading is unknown is
spoken and no byte goes out**, because a rule applied without its input is a
constant wearing a rule's clothes.

**That is the fault of 2026-08-29 addressed at its cause.** The attenuator sat at
20 dB while a station faded S4 to S1 to nothing, and later sat off while the front
end read `overloading` at S9+10. **Both wrong, in opposite directions, and Hamlet
held the reading that decides it on both evenings.**

**One test from unit 043 was re-expressed rather than deleted.** It asserted CW's
preamp and AGC were unconfirmed — spoken and never written — on the grounds that
nobody had measured what CW needs of them here. That caution was right while the
values were unruled; Tim's table settles both, and **it reverses my own AGC value
from slow to fast.** The old reasoning is kept in the test's remarks.

### Task 4 — the write, and the read-back

`ApplyAsync` now re-reads the field after a successful write and reports what the
radio actually holds:

- a value that cannot be read afterwards is **`NotConfirmed`**, not assumed;
- a radio that took the frame and set something else is **`NotConfirmed` carrying
  the value it actually holds**, which is a different fact from a refused write
  and gets its own line;
- only a confirmed match records `Changed`, and only then is it remembered as
  Hamlet's own so the operator's hand can be told apart from it.

Read-before-write, already-right, the operator's hand and once-per-tune-in were
all unit 042's and are untouched.

### Task 5 — the round trip, asserted

Four tests, all green:

- **Ten CW → FT8 → CW round trips with no drift**, compared field by field against
  the state after the first tune-in. Ten rather than one because the failure this
  guards against is drift, and a setting that moves a little on each crossing
  looks fine once.
- **Coming back to Morse restores what Morse needs** — and the AGC is the setting
  the two modes disagree about, so it is the one a partial delta would have left
  wherever the other mode put it. It ends on CW's fast.
- **A setting no row states is untouched in both directions.**
- **The auto notch left on is corrected on entering Morse**, with the write named.

`TheSilencePropertyIsLockedTests` is green and unmodified.

### Task 6 — the search range against the filter

**Measured, and the order's arithmetic does not hold.**

**The decoder does not search 400 to 1200 Hz.** `KeyingEnvelope.LowestToneHz` and
`HighestToneHz` are taken from `CwToneTracker`'s own 300 and 900, and the comment
records why: it *used* to run 400 to 1200 while the tracker ran 300 to 900, and
that mismatch cost the meter its only job. **It was already corrected.**

The filter's passband, from the sheets rather than from arithmetic: **39 of 44
captures read `FilterBandwidth 500 Hz` and `CwPitch 600 Hz`**, so the passband is
about **350 to 850 Hz**. Against a search of 300 to 900, the search overhangs by
fifty hertz at each end — **not "more than half outside".**

**And in practice the tracker essentially never leaves it:**

| | |
|---|---|
| captures with a sheet pitch | 44 |
| tracked pitch outside 350–850 Hz | **1** |
| that one | `cw-2026-08-26-125941` at **exactly 300.0 Hz** |
| lowest tracked | 300.0 | 
| highest tracked | **800.0** |

**The single exception is an estimator pinned at the floor of its own search**,
which is a failure to settle rather than a pitch outside the filter — the same
shape unit 044 reported for the speed grid at 40 WPM.

**So neither excursion the order attributes to this is explained by it.** 850 Hz
is inside the passband, and 400 Hz is well inside it. Changed nothing, as
instructed.

No decision was recorded under §12.1.

## 2. What the owner should expect

**Tuning into CW now sets nine things on the receive side for CW, says what it
changed and why, and switching to a digital block and back lands in the same place
every time.** The auto notch comes off — it hunts steady carriers and Morse is a
steady carrier, so it was eating what you were reading.

**And the attenuator finally follows the reading Hamlet already had.** Off unless
the front end says it is overloading, rather than a constant that was wrong in
both directions on one evening.

What is now true of the tree:

- `OwnedSettings` names the twelve with their citations, and the coverage table
  prints in one test.
- CW's row states nine of them; three are answered by the mode write instead.
- A write is confirmed by reading it back, not by the acknowledgement.
- Ten round trips are asserted not to drift.

**What will look wrong but is not:**

- **The corpus score is unchanged at 0.763 / 0.761.** The order requires it —
  nothing here touches the decoder.
- **CW shows three settings absent in the coverage table.** Mode, filter slot and
  filter width are written by the other path. That is the contract gap named in
  task 2, not a behaviour gap.
- **The AGC for CW moved from slow to fast**, reversing the value I put in during
  unit 043. Tim's table settles it and the old reasoning is kept in the test.
- **The engine suite's result is not in this report yet.** It was still running
  when the report was written, and it is amended in below rather than waited for,
  so nothing here depends on a number that does not exist. The app suite is 519
  passing, 0 failing, and every targeted batch this unit ran was green.

### Amendment — the engine regression

**Pending.** This line is replaced by the result and the comparison against unit
046's failing set as soon as the run lands. **If it is not replaced, the run did
not finish** — the host crash of HM-OPEN-061 has ended three of them, and an
unreplaced line is the honest record of that rather than an omission.

## 3. What you should see

**Every write Hamlet makes on its own initiative, which is the inventory task 1
asked for: two paths and no more.** The mode, data flag and filter slot go out in
one frame when the dial settles; the block's stated receive conditions go out one
setting at a time, once per block. Twenty-seven writes are defined and the other
twenty-four are reachable only when the operator asks. **Nothing automatic can
reach the transmitter**, and there is now a test that says so rather than a belief.

**Then the coverage table, which is what the other conversation needs:**

| block | stated | deferred | absent |
|---|---|---|---|
| CW | 9 | 0 | 3 |
| FT8 | 3 | 1 | 8 |
| FT4 | 3 | 1 | 8 |

FT8 and FT4 are silent on the manual notch, preamp, attenuator, RF gain and
squelch. **Those are the five a CW row now writes and a digital row does not**, so
until the digital side states them, crossing from CW to FT8 leaves them at CW's
values. **That is the contract working as designed** — silence leaves a setting
alone — and it is the list the FT8 conversation needs to fill in.

**One thing found on the way that is worth more than it cost.** A write was being
confirmed by the radio's acknowledgement rather than by reading the setting back.
`FB` means the frame was accepted. It does not mean the value is what was asked
for, and HM-DEC-084 says read back and let unknown stay unknown. It now does.

## 4. What's blocking us

Two rulings.

> **The mode, data flag and filter join the owned-settings contract, or they are
> declared a second contract on purpose.**
>
> Twelve settings are owned and nine of them go out through `ReceiverSetup`. The
> other three — mode, data flag, filter slot and width — go out through
> `SetModeAsync` on the same trigger. **So two mechanisms answer for one list**,
> which is the fragmentation this contract exists to end, one level up: the
> coverage table reports them absent on every row, correctly, and they are being
> written anyway.
>
> **Rejected: merging them in this unit.** The mode write is entangled with
> mode-follow and its snap-back memory, and unit 048 owns everything near the
> decoder.
> **Rejected: leaving them off the owned list.** They are consequences of the
> operator's intent in exactly the sense the ruling defines, and dropping them
> would make the coverage table lie by omission.

> **What the digital rows state for the five they are silent on.**
>
> FT8 and FT4 say nothing about the manual notch, preamp, attenuator, RF gain or
> squelch. **Crossing from CW to a digital block therefore leaves all five at CW's
> values** — squelch open, RF gain full, attenuator following the overflow flag.
> For four of those that is probably right for FT8 too; **the AGC shows the two
> modes genuinely disagree**, so it cannot be assumed.
>
> **This unit did not write them and must not** — another conversation owns those
> rows and §12.4 forbids filling a gap with a plausible value. It is named here
> because the coverage table is the handover and this is what it says.

### Asks still outstanding

Carried forward per HM-DEC-139 and HM-DEC-140.

1. **The eight 2026-08-29 captures are not in the tree**, a fifth consecutive unit.
2. **The lattice restructure** — unit 046's finding, and it gates the confidence
   work.
3. **The fit figure and four other quantities do not track correctness** — five
   measured, five negative.
4. **The answer key's licensing** — §2.1 and HM-DEC-049 against vendoring an ARRL
   bulletin.
5. **The mode and filter's place in the owned contract** — raised above.
6. **What the digital rows state for the five** — raised above.
7. **The pedestal ranking is measured at 34 of 44 and unbuilt.**
8. **A dial move's threshold is provisional at 500 Hz.**
9. **The transcript break's wording.**
10. **Whether `CwPitch` should follow an admitted station** — deliberately not
    owned by this unit's contract, and still unruled as a question.
11. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150.**
12. **The `reading` line's span wording needs approval.**
13. **Two stations closer than 125 Hz are not named.**
14. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).
15. **Nothing checks that deleting a surface is not deleting a capability.**
16. **The engine test host crashes**, wider than the class HM-OPEN-061 names.
    Owned by Claude, not waiting on a ruling.
