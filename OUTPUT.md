# 1. What Claude did

**STATE.** Development computer, `C:\Source\HamLet`, Claude Code surface (§9.5).
**Branch: `main`, and nowhere else** (§9.5.1). The prompt claimed `PROJECT: Hamlet`
and the tree confirms it: `CLAUDE.md`'s header reads `Project: Hamlet`, the
solution is `Hamlet.sln`, the namespaces are `Hamlet.*`. Gate passed. **Nothing in
this report is evidence about the radio** (HM-DEC-093): every number comes from a
fixture, a generated signal, or one of the off-air recordings decoded here. Phases
4 and 5 are verified at the screen on COM3 and not here.

**Nothing was recorded under §12.1.** One question came up and it is in section 4.

**All six phases completed. Nothing was dropped.** No transmit work of any kind,
and nothing was built toward auto-CQ.

## Phase 1 — a confirmed station is not abandoned for a candidate far below it (HM-DEC-127)

**The criterion was measured across the corpus and it is the filter's own
rejection, already in the file.** Every switch in every recording was traced with
the candidate's level beside the level of the station being read:

| what it was | measured separation |
|---|---|
| the survey settling one bin over on `013347`, `004507`, `two-station` | **0.3 dB above to 1.5 dB below** |
| the caller at 615 handing to the answerer at 730 | from cold, nothing being abandoned |
| **the 400 Hz station's own image at 575** | **34.6 dB below** |

There is nothing in between. The floor is `FilterRejectionDb`, which this file
already carries with the right meaning: past 125 hertz of separation the window
takes at least that much off a rival, so anything that far below the station being
read is inside what that station's own leakage could produce, and calling it a
different station is a claim the measurement does not support.

**HM-DEC-095 is not amended and is not in tension.** It governs which of several
signals to read on an empty-handed survey, where there is nothing to abandon.

**The whole corpus was re-decoded either side of the change** — twenty-four
recordings, the four off-air captures included — and is **identical character for
character except the one the ruling is about**:

```
gen-400-from-600   before  3 moves   '■■ ■■■ ■ K DE W1AW K'
                   after   1 move    '■■ ■■■ CQ DE W1AW K'
```

**`ASignalAtTheWrongPitchIsStillFound(400)` passes on phase 1 alone**, which the
work order asked to be told.

**One thing found while writing the control**: a station at 350 hertz is not read,
and was not read before this change either — two moves and a row of placeholders,
identical either side. Fifty hertz off the bottom of the survey's range. Recorded
as HM-OPEN-034 at severity none rather than asserted in a test, because it is a
pre-existing hole and nobody has decided to fix it.

## Phase 2 — the fixture gets a band (HM-DEC-127, second half)

`ASignalAtTheWrongPitchIsStillFound` becomes
**`ASignalAtTheWrongPitchIsStillFoundInABand`**, and the name carries it so nobody
reads the change as a bar moving. Fifteen decibels, which is
`CwFixtureCatalogue.EasyDb` — the easy tier's own number — so it now asserts what
the rest of the suite asserts at the same strength. **The assertion is untouched**:
the whole message, and the pitch within half a filter width.

Done after phase 1 so the two are separable, and they are: **phase 1 alone turned
it green**, so what the band buys is that the test stops resting on digital silence
no receiver produces, which is the fault HM-OPEN-018 was opened for and which every
fixture under `tests/fixtures/cw/receiver` was rebuilt to remove.

## Phase 3 — HM-DEC-128 recorded, and what it closes

**Confirmed by sweep: no `Adopt`, no `ForgetAdopted` and no adoption flag remains
anywhere in the engine**, as was done for HM-DEC-122.

`HM-OPEN-027` and `HM-OPEN-032` closed against it with their reasons. The coupling
HM-OPEN-027 traced is gone — with the refining distinction in place, adoption
produces three moves and one follow on the real capture, identical to adoption off
— and HM-OPEN-032's own finding is what superseded the ruling, by handing the
streaming estimator the shared fitter and removing the choice HM-DEC-116 was
making.

## Phase 4 — scan results are click-and-go

**A list of stations is a report; the operator now gets a destination.** Each
result carries a control that says where it will send the dial — `listen at 7.029
MHz` — and pressing it tunes there and stops the scan.

**The order is the whole of it.** It stops the scan first and **waits for the
scanner to put the dial back where the operator left it**, and only then does the
tune go out, through `TuneTo` — the same path a spot card or a map dot uses. So
what moves the dial to a result is the operator tuning rather than the scanner
writing, every §0.2.1 exit route still restores, and the crash-safe note on disk is
cleared by the scanner's own restore rather than by anything reaching around it.
Without that wait the tune lands first and the restore then drags the dial off the
station he just chose, which is §0.2.1 failing by way of the feature meant to
serve it.

- **The row carries the frequency the dwell listened at**, never the bin a
  candidate was ranked in. A candidate is a place the waterfall saw something; a
  dwell is a place the dial sat and the decoder listened.
- **The sureness pill's colour now follows its words.** It was green whatever it
  said. Amber is the default and green is earned, because this is a row the
  operator acts on and a confident-looking maybe costs him an evening (§0.0,
  §0.6 — the words still carry it too).
- **A callsign-shaped stop still names no callsign** (HM-DEC-073), unchanged and
  still tested.

Four tests: the frequency is the one heard on, tapping tunes and stops, a row with
nowhere to go moves nothing, and the dim-versus-solid check now covers the colour.

## Phase 5 — a favourite carries a note

**The name says where and Hamlet derives that from the map. Why is a thing only
the operator knows**, so this is the one part of a favourite that nothing derives,
suggests or fills in (§0.0).

- **In the strip beside the name** (HM-DEC-070), inside the flexible column, so
  however long it is **it cannot push the dropdowns out of shape**. One line, 80
  characters, no returns: a favourite is a signpost rather than a logbook entry.
- **There from the moment the star lights**, because a box somebody has to go and
  find in a management window never gets written, **and editable afterwards** both
  there and in the manage window, because a box that only appears at save time
  gets left blank by somebody in a hurry.
- **An empty note is the ordinary state and renders as nothing at all.** Clearing
  one is allowed, unlike clearing a name — a favourite with no name is one nobody
  can pick out of a list, and a favourite with no note is most of them.
- A favourite written before notes existed still loads and has none, so there is
  nothing to migrate (§6.1), and there is a test that says so.

## Phase 6 — `tightfist-easy`, fixed

**The boundary was right and the centre was not.** Traced with the fit's own
numbers printed at the moment the character was judged. The fixture's element gaps
are 80 milliseconds and its character gaps 162; the gate measured the two gaps
inside the first `S` at 85 and 75, which is right; the boundary was 89, which
classifies every one of them correctly; and the element class **centre was 49**.

```
Toward(85, cut 89, centre 49) = 4 / 40  = 0.10
Toward(75, cut 94, centre 55) = 19 / 39 = 0.49
```

Confidence is measured from the boundary toward the centre, so a character whose
pattern was `...` and whose elements were clean came back as a placeholder at 28.6
decibels over the noise — and four seconds later, with the window full of this
fist's own gaps, the same pattern read as `S` at 0.98.

**What dragged the centre down was the detector rather than the sender**: the
rolling window still held gaps of 15, 20, 30 and 35 milliseconds from before the
signal was acquired. Twenty-five milliseconds is the shortest dit this radio can
send — `CwToneSurvey.ShortestDitMs`, forty-eight words a minute, the fastest its
own keyer goes — so nothing below it is a silence anybody left. They are dropped
before the fit rather than trimmed after it, because they spoil the class centres
and not merely their edges.

**The corpus was re-decoded around it.** Six of twenty-four recordings move: the
easy-tier bar goes green, `fast-working` gets an opening character back, two edge
tiers shuffle their move counts while decoding nothing either way, and two
working-tier transcripts shuffle inside text unreadable either way. HM-DEC-114
makes the easy tier pass-or-fail and the working tiers a statement about
degradation, so nothing any test asserts moved except the one that was meant to.

# 2. What Tim should expect

- **Build succeeds, no warnings.**
- **1845 tests, 3 failing.** 1423 of 1425 in the engine, 419 of 420 in the app.
  Thirteen tests are new.
- **Five failures become three.** `ASignalAtTheWrongPitchIsStillFound(400)` and
  `TheEasyTierIsReadWhole(tightfist-easy)` are both green.
- **The failing three, named:**
  - `ClearingTheTranscriptLeavesTheDecoderAlone` — reads `■ DE W1AW K` against
    `CQ DE W1AW K`. It would pass with HM-DEC-116 shipped, which HM-DEC-128 has
    now settled it will not be.
  - `TheBulletinDecodesToItsAnswerKey` — the long-standing bar on a real
    recording, unmoved this session at `NL DOT NET ■I ECH STAAION HAND■ AHIS
    MESAGE P`, 36 characters against 47.
  - `TheEasyTierIsReadWhole(prosigns-easy)` — reads `CALLARSK` against
    `BTN0CALLARSK`. The prosigns themselves are right and the opening four
    characters are lost to acquisition on the one easy-tier fixture that cannot
    carry a run-up.
- **What will look wrong and is not.** `ASignalAtTheWrongPitchIsStillFound` has a
  new name and a band in it; that is HM-DEC-127's second half and the assertion is
  unchanged. The scan panel's sureness pill is amber more often than it was green
  before — that is the confidence being drawn honestly rather than a new fault.
- **What to look at on COM3.** Phases 4 and 5 are the two you verify at the
  screen. Run a scan, let it list a few dwells, and press `listen at …` on one:
  the dial should end on that frequency with the scan stopped and the status line
  saying so. Then star a frequency and type a line in the box that appears under
  the favourite's name; it should still be there next time you tune back, and in
  Radio → Manage favourites.
- **Nothing is tuned to any recording.** No decoder parameter was moved to suit
  `cw-2026-08-17-013347` or `cw-2026-08-18-004507`.
- **Seven commits, pushed to `main`.** Nothing local, no branches. The first
  carries the uncommitted `CLAUDE.md` and `CLEANUP_BRIEF.md` that were in the
  working tree when the session opened.

# 3. What we should do next

- The auto-CQ work order, which HM-DEC-098 has been waiting on and which this
  order deliberately built nothing toward.
- `prosigns-easy`, the last bar failure. It is acquisition on a fixture that
  cannot take the ruled run-up, and section 4 has the shape of the question.
- Re-measure the bulletin after either. It has moved on three of the last four
  sessions and did not move on this one.
- HM-OPEN-034, the 350 hertz hole, whenever the survey's edges are next opened.

# 4. What's blocking us

---
date: 2026-08-18
refs: CLAUDE.md §0.0, §12.1, §12.5; HM-DEC-103; HM-DEC-114; HM-OPEN-031; HM-OPEN-033
---

**`prosigns-easy` gets something to acquire on, or HM-DEC-114's bar stops applying
to a fixture that cannot have one.**

It is the last easy-tier failure and it is not a decoder fault in the ordinary
sense. Its first character arrives at 7.44 seconds on a message that runs about
four and a half, so `<BT> N0` is gone before the detector has found the signal.
The prosigns themselves read correctly — `CALLARSK` against `BTN0CALLARSK` — since
HM-DEC-124 fixed the generator's caret.

**Every other easy-tier fixture is given a `VVV` run-up for exactly this**
(HM-DEC-103), and this one cannot take it: measured again this session, with the
run-up in front the tracker makes two moves, **settles at 675 hertz on a fixture
sitting at 615**, and emits nothing at all. That is HM-OPEN-033's third sighting of
the survey choosing a bin that holds no station, and phase 1's floor does not touch
it because nothing has been confirmed yet when it happens.

Three directions, and the choice is yours:

- **Give it a different run-up.** Something whose mark lengths do not smear
  against a prosign's — a callsign, or `TEST`, rather than `VVV`. It is the
  smallest change and it is still a fixture edit, which §12.5 says a session may
  not make alone to turn a test green.
- **Fix the cold-start bin choice**, which would close HM-OPEN-033 entirely and is
  the largest of the three. The floor built this session protects a station
  already being read; nothing protects the first choice.
- **Say the bar does not apply here.** HM-DEC-114 made the easy tier pass-or-fail
  on the reasoning that a loud clean signal read wrongly is a defect. A message
  too short for the detector to acquire on is a different claim, and this is the
  one fixture where the ruled remedy is unavailable.

Rejected: editing the fixture on this session's authority, for the reason above.
Rejected: leaving it unattributed, which is what "the last bar failure" has meant
for two sessions.
