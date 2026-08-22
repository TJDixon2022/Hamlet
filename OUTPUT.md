# OUTPUT.md

## 1. What Claude did

**The strip was empty because the commit that removed the old decoder cut the
capture press and the keying meter out of the window, and the press is back.**

That commit meant to remove one thing below the transcript, the revisions row,
and it cut one contiguous block from there down to the offer row. The keying
meter and the press that marks a case were in between. **Nothing failed**: every
property and command behind them is still on the view model and every binding
still resolves, so `BindingHealthTests` had nothing to complain about. **A binding
that resolves and an element that is not there look the same to a test that only
reads the log.**

**The layout unit is not the fault.** It consolidated the conditional notes below
the transcript into the fixed-height advisory region on purpose (HM-DEC-080), and
those notes still reach it: `AdvisoryNote` takes the first non-empty of the
suspended note, the overflow advice, the capture note and the decoder story. An
empty grey box means none of them had anything to say at that moment, which is the
region working. **The blank gap under it was the two deleted blocks.**

Nothing else was missing. Comparing what the region is meant to hold against what
it holds, the only other binding that commit removed was `TipIsUnstable`, which
went deliberately with the settled pass that fed it.

**Where the line falls between a refinement and a station change: the tracker
draws one, and it is in the wrong place in both directions.** Section 4 carries
the ask; the evidence is under task 3 below.

Claude Code on the development computer, `C:\Source\HamLet`, on `main`. Gate
verified against the tree: `Hamlet.sln` and `CwProbabilisticStream.cs` present, no
`CoreHMI.sln`, no `src\CoreHMI`, `PROJECT_CARD.md` says Hamlet. **No radio was
connected and nothing here is evidence about the radio** (HM-DEC-093). Nothing was
recorded under §12.1.

### Task 1 — why the strip was empty

1. **What renders it.** A fixed-height `Border` below the transcript bound to
   `MainWindowViewModel.AdvisoryNote`, which walks `Advisories()` and shows the
   first message with anything in it.
2. **Why it was empty.** Nothing is placed into it when every advisory is silent,
   which is the design. The **large blank gap beneath** it was not the region at
   all: it was the space the two deleted blocks used to occupy.
3. **Where the press went.** `CaptureAudioCommand` is still on the view model and
   still does exactly what it did. The `Button` bound to it was deleted from the
   window in `4bc3bce`, along with the keying meter's whole `Border`. Absent, not
   disabled, exactly as reported.
4. **Anything else.** No. `TipIsUnstable` also went in that commit and was meant
   to.

### Task 2 — the press is back, and committed on its own

Both blocks are restored byte for byte from the commit that removed them, with a
note above them saying what happened. **Nothing about the press changed**: same
command, same tooltip, same enabling on `IsDecoding`. The keying meter came back
with it, because it is the independent witness and the whole point of it is that
it can contradict the decoder while the operator is at the radio.

**The transcript does not move.** Both blocks sit below it, and the fixed-height
advisory region is untouched.

**`TheCapturePressIsOnTheScreenTests`** builds the real window headless, puts the
terminal on the canvas the way the operator does, and fails unless the button is
there, has a command, and the meter's own explanation is on the screen beside it.
It fails on the tree as it was this morning.

### Task 3 — what counts as a move

The tracker changes where it listens in four ways.

| What | Where | Was the held audio mixed at a different pitch afterwards? |
|---|---|---|
| **Refinement inside the fine bank** | `ReadSurvey`, "inside reach" | Yes, by a few hertz. The bank does not move and nothing is counted; the reported pitch shifts within the same station |
| **Acquiring jump from cold** | `ReadSurvey`, while no keying has ever been confirmed | Yes, by any distance. Counts a retune and a follow |
| **`Switch` within `ConfirmWithinHz` of the bank centre** | `Switch`, `refining` true | Yes, by up to the coarse spacing. Counts a retune only |
| **`Switch` beyond it** | `Switch`, `refining` false | Yes, by any distance. Counts a retune and a follow, and now a **station change** |

**The tracker did not distinguish leaving a station from finding one, so that was
built**: `CwToneTracker.StationChanges` counts only the fourth row, the moves made
after keying has been confirmed somewhere. `Follows` is untouched, because the
speed guard and a test both depend on its present meaning.

**And it is still the wrong line, measured in both directions.**

- **The two-station fixture, the one thing in this repository built to contain a
  station change, produces none.** Six retunes, four follows, zero station
  changes: the answering station is reached through the acquiring branch rather
  than through `Switch`.
- **Fixtures holding one sender produce them.** `004507` declares two in its first
  three seconds, 600 to 475 and 475 to 525, both while nothing has been read yet.
  A twelve words a minute fixture declares one mid-message.

So a trigger hung on it fires where there is nobody to leave and stays silent
where somebody answers.

### Task 4 — the window is not cleared, and why not

**The machinery is built and it is not switched on.** `CwProbabilisticStream
.Restart()` drops the held envelope and the leading edge with it, keeps the audio
clock and the settled mark so **nothing already settled is retracted**, and sets a
refill guard so a short window says nothing rather than guessing. `ListeningAfresh`
and the terminal sentence for task 5 are built on top of it.

**What clearing costs, measured on both, which is why it is not on:**

| triggered on | the sweep at 18 dB | `004507` |
|---|---|---|
| any follow | 0.67 right, 0.22 wrong | `T SM G JL D O T N E T <BT> E ACH STAT ION…` |
| a station change | 0.67 right, 0.22 wrong | same |
| a station change of at least the decoder's own 60 Hz bandwidth | 1.00 right, 0.00 wrong | `E A T SM G JL D O T N E T…` |
| that, and only while something was being read | 1.00 right, 0.00 wrong | `E AT ARRL DOT NET…`, unchanged |

The last row is the honest line and it never fires on anything in the corpus, so
the clear is not exercised by a single fixture. **And with the first three it cost
a real decode**: a twelve words a minute message fell to 0.63 of itself at
eighteen decibels, on a fixture holding one sender.

**What the likelihood ratio does while the window is short**, which the order
asked for: the refill length makes no difference to it. Swept at 0.5, 2, 3, 4, 6
and 8 seconds, the worst invented share across the sweep is 0.22 at every one of
them, and the eighteen decibel reading is 0.67 right and 0.22 wrong at every one.
**The invention after a clear is not the short window; it is the audio that was
thrown away.**

### Task 5 — what the terminal says

The advisory is written and sits in `Advisories()` above the capture note:

> somebody else has started sending and Hamlet has moved across to them, so it has
> let go of what it was holding, because those twelve seconds were listened to at
> the other station's pitch and reading them now would put one operator's letters
> in the other's mouth. Give it a few seconds to fill up again and the text picks
> up where the new station is.

It clears the moment text resumes, and it cannot show while the clear is off.

### Task 6 — the sweep and all six recordings, together

**Every recording is character for character what it was last session**, because
nothing now fires on any of them:

| recording | last session | now |
|---|---|---|
| `004507` | `E AT ARRL DOT NET <BT> E ACH STATION HANDLING ET HIS M E S S A G E P E` | identical |
| `003016` | `E ■I KPA1■IS<HH> ■NK <BT> STILLHVEMY ETO 91B E TT JETST VFB TUBE LIN` | identical |
| `003126` | `E S 5 IWATTCH ATL E<AS>T 2 IOVI ES A DAY WID X■ WHY N■TT E E , WESTERNS , E` | identical |
| `003758` | `E ■HES EHEHSE AA■IH/5■IS E E E EAN EANQNI<HH>SK  E E E E E E EIIE` | identical |
| `014854`, `014935` | silent | silent, offline and streamed |

**`003758` and `003016` have not come back** to their pre-removal strings, and
nothing in this unit moved them either way.

The sweep is unchanged at every level: 1.00 right and 0.00 invented from eighteen
decibels down to twelve, 0.06 wrong at eleven, 0.19 at three, 0.33 at zero, and
silence below minus five.

### Task 7 — the version

**`Directory.Build.props` moved 1.10.3 to 1.10.4.**

### The order, checked against the rulings it cites

Every ruling this order cites says what the order says it says: HM-DEC-120 the
emission property, HM-DEC-009 the prime directive, HM-DEC-096's phase 3 the
mid-character interlock, HM-DEC-091 one source, HM-DEC-150 the version scheme,
HM-DEC-093 with `SHACK_FACTS.md` the no-radio rule. **No mismatch.**

### The inbound asks queue

Every id it names is `status: open` in `OPEN_ISSUES.md`. Nothing on it is closed
and nothing open and relevant is missing.

## 2. What Tim should expect

**He can mark a case again: the press is on the screen, wired to the same command,
with the keying meter back above it.**

**And when Hamlet follows somebody mid-contact he sees exactly what he saw
yesterday, because the window is not being cleared** — the sentence explaining it
is written and cannot appear until there is a move worth firing it on.

Build clean, no warnings, version 1.10.4. **28 failing, the same 28 by name as
when this unit started.** The app suite is 477 green, one more than yesterday,
which is the new capture-press test.

**What will look wrong and is not:** `CwProbabilisticStream.Restart`,
`RefillSeconds`, `CwToneTracker.StationChanges`, `CwDecoder.ListeningAfresh` and
`FollowedNote` are all built and none of them runs. That is deliberate and it is
the ask below.

## 3. What we should do next

- **Rule on what a station change is**, in section 4. Everything else here waits
  on it.
- **The two-station fixture may not contain what it says it does.** The tracker
  reaches the answering station through the acquiring branch, which is what it
  does when it has not found anybody, and that is worth looking at on its own.
- **`003758` and `003016` are still short of their pre-removal strings**, and
  neither the interlock nor this unit moved them.
- **`FollowSpeed` still has no supplier.**

## 4. What's blocking us

Nothing blocks the next unit. One ask.

> **A station change is declared by evidence that somebody else is sending, not by
> how far the filter moved.**
>
> The window-clearing Tim ruled for is built and cannot be switched on, because
> the only signal available for triggering it is wrong in both directions. The
> tracker declares a station change **twice on `004507` in its first three
> seconds while nothing has been read**, and **not once on the two-station fixture
> built to contain one**, where the answering station is reached through the
> acquiring branch instead.
>
> **Two candidate answers, and both need a fixture that actually exercises them.**
> The first is distance measured against the decoder's own filter: a move of less
> than `CwProbabilisticDecoder.BandwidthHz`, which is 60 Hz, lands inside the
> passband the held audio was taken through, so it cannot have made that audio
> incoherent, and everything measured here settles onto a station in 40 to 50 Hz
> steps. That plus "only while something was being read" leaves every recording
> untouched and the sweep clean, and fires on nothing at all. The second is to
> stop asking the tracker and ask the decoder: a new sender is a speed or a fist
> that does not fit what has been read, which is what the operator notices, and it
> is a measurement nothing here makes yet.
>
> **Rejected: shipping the clear on the tracker's own classification.** It cost a
> twelve words a minute message 0.63 of itself at eighteen decibels and `004507`
> its opening, on fixtures holding one sender, which is the disease rather than
> the cure.
>
> **Rejected: leaving it on and calling those fixtures unrepresentative.** They
> are the corpus; there is nothing else to be right about.

### Asks still outstanding

Carried per HM-DEC-139, verbatim until ruled.

- Whether the sidecar's `text` should include the leading edge.
- The captures from the evenings of the 20th and 21st are not in the tree.
- Thirty seconds since the last character, for mode-follow's guard.
- Whether `RfGain`'s hundred per cent is a defect or the right answer.
- The likelihood gate at 15.0.
- The keying meter's provisional thresholds.
- `FollowSpeed` has no supplier.
- The mark-and-gap witness behind HM-DEC-144 and HM-DEC-145.
- HM-OPEN-052, HM-OPEN-053, HM-OPEN-054, HM-DEC-130, HM-DEC-098, HM-OPEN-033,
  HM-OPEN-007.
- **What a station change is**, first made today, above.
