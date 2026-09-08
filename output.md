# Work instruction 284 — split the admissions from the advice

```
UNIT:      284
TASKS:     4 of 4, none dropped
NUMBER:    the bar in the reproduced state, 590 -> 184 characters
           (596 on the hover, every word, nothing deleted)
ADVANCED:  no
DRIFT:     7 consecutive units without advance, carried from unit 283
VERSION:   1.12.195 -> 1.12.199
BRANCH:    main, pushed
```

---

## 1. What Claude did

**Surface: Claude Code, on the development computer, on `main`.** The prompt claimed
`PROJECT: Hamlet` and the tree confirmed it — `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` present, `CoreHMI.sln` and
`MURC.sln` absent, `Hamlet.sln` at the root. **Nothing here is evidence about the
radio**: this is the simulated one.

**Nothing was recorded under §12.1.** No shell refusals.

### Task 1 — the reproduction, and the path followed back

**It reproduces.** Simulated radio, Digital tab, 20 m, 14.074 MHz, driving connect
and the tune-in through the view model's own commands. The bar rendered **590
characters**, his screenshot word for word.

**Five clauses, and the order's reading of them is right:**

| Chars | Kind | Outcome |
|---:|---|---|
| 180 (three clauses) | **admission** — *I could not read the …* | `NotRead` |
| 410 (two clauses) | **advice** — *…usually wants to be…*, *…I cannot set from here* | `SpokenOnly` |

**Followed the running value, not a grep.** `StatusOnScreen` reads the spoken half,
set by `Narrate` at `MainWindowViewModel.cs:8433` from `ReceiverSetupVoice.Admissions`
— which at `ReceiverSetupVoice.cs:111` gathered `NotConfirmed`, `NotRead` **and, at
line 135, `SpokenOnly`.** Both advice clauses come from there
(`ReceiverSetup.cs:153` and `:216`). **That is the join.**

**Two fixture faults found on the way, recorded rather than quietly fixed**, because
each made the bar say something else and each would have read as a clean result:

- **The connect command is a toggle** and the panel starts connected, so the first
  run disconnected it. The bar read `Disconnected`.
- **`OnFrequencyHzChanged` clamps the operator's own tuning to the band map on
  screen**, so setting 14.074 while the panel sat on 40 m landed on 40 m and composed
  nothing.

`FollowTheMapForTests` is the seam, and it is a weaker one than `NarrateForTests`: it
hands over nothing, so what the bar shows is composed by the application from the rig
it is talking to. The tune-in otherwise fires from a settle timer that does not run
headless.

### Task 2 — the split

**One line.** `Admissions` stops gathering `SpokenOnly`.

**The line is drawn on whether Hamlet tried.** `NotConfirmed` and `NotRead` are
attempts that came back with nothing, so the operator is left not knowing where a
control is and has to be told. `SpokenOnly` leaves him knowing exactly where
everything is: it names a change Hamlet **could** make and has **decided not to**, and
says why. **Hamlet did not try and fail; it chose not to try.** That is advice.

**Split at the source.** `Say` still composes all five kinds and `Admissions` filters
the same results, so what he hovers and what he is shown come from one place and
cannot disagree. **One rule for which clause is which**, and nothing downstream cuts a
finished string up.

### Task 3 — the test, and an invariant that outlives the wording

The reproduction test was watched failing first in task 1. It gains the structural
form of the rule: **every advice clause joins its reason with *because*, and neither
admission shape has one**, so the bar may not contain the word at all and every
sentence on it must open with one of the two admission shapes. **A sixth kind of
clause arriving with a reason attached fails here even if nobody adds a phrase for
it.**

That it bites is not asserted from theory: **the 590-character bar recorded before the
split carries *because* twice.**

### Task 4 — what moved, listed and measured

`docs/unit284-what-was-removed.md`. **No fact removed**; nothing deleted.

---

## 2. What Tim should expect

**The bar says what Hamlet could not do, and nothing else.**

On the simulated radio, tuned into the FT8 block, it now reads three sentences —
*I could not read the noise blanker, so I have not touched it*, and the same for the
noise reduction and the auto notch. **That is all.**

**The advice is on the hover**: the AGC wanting to be slow and why, and the scope span
wanting to be 3 kHz across and why. Hover the small ring at the left of the bar and
all 596 characters are there, admissions included.

### What will look wrong and is not

- **Three sentences on the bar is the new normal on the simulated radio**, because
  the simulator answers no setting read. **On your own radio those clauses only
  appear when a read genuinely fails.**
- **Nothing else changed.** No ceiling, no other surface, no sweep. This unit did one
  thing.

### The build and the tests

Build clean, no warnings. **This unit ran no suite** (HM-DEC-155): only the test it
wrote, plus the classes its one-line change could have disturbed.

| Class | Result |
|---|---|
| `TheBarSaysWhatHamletCouldNotDoTests` | 3 of 3 — new |
| `TheStatusBarStopsLecturingTests` | 4 of 4 |
| `WhichPathsPutNarrationOnTheBarTests` | 5 of 5 |
| `TheTopStripStopsLecturingTests` | 4 of 4 |
| `HowMuchTheApplicationSaysTests` | 6 of 6 |
| `HamletSaysWhatItChangedTests` (engine) | 5 of 5 |
| **Total** | **27 of 27** |

Inherited reds untouched: `HM-OPEN-088`'s ten, the CW set, the `Ft8Sharp.Deep`
tripwire.

**One environment fault, recorded**: a build was interrupted and left the XAML
precompilation broken, which failed two tests with *No precompiled XAML found for
Hamlet.App.App*. Cleared by deleting `obj/Debug` and `bin/Debug` for the app and the
test project and rebuilding. Nothing in the tree caused it and nothing in the tree
was changed for it.

**Four commits, all on `main`, all pushed**, 1.12.195 → 1.12.199. Nothing uncommitted.

---

## 3. What we should do next

### The bar as it renders now, verbatim

> I could not read the noise blanker, so I have not touched it. I could not read the
> noise reduction, so I have not touched it. I could not read the auto notch, so I
> have not touched it.

**184 characters.** Before: 590.

### The hover, verbatim

> tip — I could not read the noise blanker, so I have not touched it. I could not read
> the noise reduction, so I have not touched it. I could not read the auto notch, so I
> have not touched it. The AGC usually wants to be slow here, because dozens of
> stations transmit together here and the gain would ride up and down under the
> loudest of them, and that is not settled well enough for me to change it on your
> radio. Your scope span wants to be 3 kHz across, because a scope showing a couple of
> hundred kilohertz draws the whole block about seven pixels wide, and that is one I
> cannot set from here.

**596 characters. Both advice clauses arrived**, asserted by phrase.

### The test's red, before the split

```
Assert.DoesNotContain() Failure: Sub-string found
                             ↓ (pos 193)
String: ···"touched it. The AGC usually wants to be s"···
Found:  "usually wants to be"
```

The red quotes the paragraph on his screen, from a fixture that stood the application
up rather than handing a string to a seam.

### Then

1. **Look at it on the real radio.** On the IC-7300 those three admissions should not
   appear at all — a read that succeeds produces no clause. **If they do appear, the
   settings genuinely are not being read**, and that is a different fault worth its
   own unit.
2. **The parked queue is untouched** — `HM-OPEN-087`, `HM-OPEN-088`, the hover ring's
   findability, the ceilings, Settings, `AboutWindow`.
3. **Then back to the radio.** Every bench step is closed.

---

## 4. What's blocking us

Nothing blocks the next unit.

### Whether three admissions on the simulated radio is right

**Ruling asked for:** on the simulator, every owned setting comes back unread, so the
bar carries three sentences whenever you tune into a block. That is honest — Hamlet
genuinely could not read them — but the simulator is a training tool and *could not
read* is arguably not a fault there so much as a fact about a simulator.

**Why it is worth a ruling rather than a session's judgement:** it touches what the
display asserts, which §12.1 keeps yours without exception. Silencing them on the
training radio would also mean the one screen a newcomer learns on shows a state the
real radio does not, and that trade is not mine.

**Rejected:** doing it here. This order says one thing, and widening past the subject
is what the last five units were criticised for.

### Asks still outstanding

Carried forward per HM-DEC-139. **All of it is parked by this order and none of it is
this unit's**, listed so the queue survives.

1. **Two issues of one work-instruction number.** Unit 271, and 252 before it. **The
   author's error.** No unit action.
2. **`PM95` reads *southern Japan***, unit 271. **Not a defect.**
3. **`HM-OPEN-083` and `HM-OPEN-084`.** In `OPEN_ISSUES.md` by HM-DEC-140.
4. **Three pixels.** **Waiting on Tim.**
5. **`dt` and `hz` were never on the mine list.** **Waiting on Tim.**
6. **Where an outcome entry goes when the unit it corrects has none.** **Tim's.**
7. **`PHASE_OUTCOME.md` is written by a tool no unit is told to run.** **The
   author's.** **Unit 284 was not told to run it and did not** — the order has no
   outcome task.
8. **Where the repeat fold stops**, unit 277. **Still open.**
9. **Whether a faded row needs a second carrier of its meaning**, unit 279. **Tim's.**
10. **Whether counting subjects is a new assertion**, unit 279. **Tim's.**
11. **Whether the fade is still obvious now the row is a bubble**, unit 280.
12. **`HM-OPEN-087`** — thirteen unreferenced `widget.*` templates. **Still Tim's.**
13. **Whether a 14-pixel hover ring is findable**, units 281, 282, 283. **Not seen by
    anybody on a real screen**, and it now carries the receive narration, the link
    check **and this unit's advice**.
14. **Whether `AboutWindow` is in scope for the terseness ruling**, unit 281.
15. **What `SenderHelp` is for**, unit 281. `HM-OPEN-088`. **Ten inherited reds.**
16. **Whether an order should name a behaviour rather than a file and line**, unit
    283. **Answered by this order in practice**: it named a reproduction instead, and
    the paragraph was found and split in one unit. Worth keeping as the pattern.
17. **Whether three admissions on the simulated radio is right**, unit 284. **New.**
