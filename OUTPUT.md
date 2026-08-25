# Work instruction 012 — ready for tonight

## 1. What Claude did

Claude Code, on the development computer, in `C:\Source\HamLet`. The prompt
claimed `PROJECT: Hamlet`; the tree confirmed it — `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` exist, neither
`CoreHMI.sln` nor `MURC.sln` does, `CLAUDE.md`'s header says Hamlet and the
solution is `Hamlet.sln`. Branch `main` throughout, six commits, all pushed,
none refused. Version 1.11.8 to 1.11.9 per HM-DEC-150.

**Nothing here is evidence about the radio.** No rig was connected; every number
comes from recordings already in the tree.

**No decision was recorded under §12.1.** Everything needing a ruling is in
section 4.

### Where the instruction and the tree disagree

- **The thirteen captures of 2026-08-25 are still absent** — nothing matching
  that date exists under `tests/`. Fourth consecutive unit. Task 6 is void.
- **The engine baseline was 30 failing of 1674, not 29 of 1661.** The extra
  failure is `RigDisconnectTests.TheStateMonitorDoesNotHoldUpADisconnect`, which
  passes alone: **a second timing-flaky rig test**, alongside the known one.
- **The app suite was 483 of 483 green**, as stated.
- `CLAUDE_CODE.md` §8 says four sections; its version line still reads 1.3.
- `DECISIONS.md` still has no record for HM-DEC-096–133, 136, 141 or 150.

### Task 1 — the ground truth, hardened

The W1AW truth file's header now reads ADJUDICATED, citing this instruction and
the date. **The filename and the folder were left alone**: renaming would break
the references in unit 1.11.8's committed report and in the commit messages that
carry it, which is a worse record than a stale name. The file says so on its face.

**Twelve success tests, the first in this repository that fail when a repair
breaks a success.** Every other ratchet here guards a failure getting less bad,
so nothing in the suite could tell a repair from a coincidence — HM-OPEN-026
named that gap and had no candidate to fill it.

**What they assert is a run, not a line, because not one of the seven bulletin
lines is read whole.** Each carries the longest unbroken run of its own
adjudicated text the decoder gives back, and the shortfall is printed rather
than papered: **153 of 384 adjudicated characters, 40 %**.

**The starting pitch matters more than anything else measured, and the anchors
are pinned to what the operator's radio actually does.** Every one of these
captures records `CwPitch 600 Hz`, and `MainWindowViewModel` hands the decoder
`_settings.CwPitchHz`. Started instead at each station's own recorded note,
`032113` reads 22 characters of its line rather than 4, `032012` 43 rather than
22, `032050` 24 rather than 17 — while `031905` falls from 12 to 7 and `032129`
from 10 to 7. **`ANALYSIS-cw-emit-decision-2026-08-24.md` is written at the
station's note and therefore reads better than the operator does**, which is
worth knowing before quoting it.

### Task 2 — the band display

The band row has a row of its own beneath the strip, so it has the whole window
width and nothing to collide with. It is still outside the canvas and still
cannot be closed or moved; the wavelength-proportioned widths are untouched.

**Measured at four widths rather than the one the test was pinned to.** Before:
every card reachable at 1400 and 1200, `15 m` and `10 m` unreachable at 1000,
three cards unreachable at 820. After: all seven at all four. **HM-OPEN-060 is
closed**, and the closure was verified by reverting the layout and watching the
same test name the same cards again.

**The hit test had to be rebuilt before it could say anything trustworthy.** The
headless renderer draws these cards about thirteen pixels above where every
geometry API reports them and about two thirds as tall — `TranslatePoint`,
`TransformedBounds` and a hand-summed layout chain all agree with each other and
all disagree with the hit test. A probe at a computed centre therefore lands past
the bottom of the card and reports an occlusion that is not there. It now walks
down the card's own rectangle and takes the first point that reaches anything,
which is immune to the offset and still answers the question exactly.

### Task 3 — the witness's verdict, under Tim's condition

The verdict moves onto the element median **and the swing keeps it honest**. The
requirement was already in the tree.

| | meter right, of 23 | empty six-second windows claiming Keying |
|---|---|---|
| the old all-runs median | 10 | 0 |
| element median alone | 17 | **11** |
| **element median + swing ≥ 20** | **16** | **0** |

**A count of elements was the obvious guard and it is measured backwards.** In
six seconds an empty band gives 26 to 40 element-length runs and a real station
gives 11 to 38, median 26 — the empty windows sit at the *top* of the range, so
any count that silences them silences all nineteen real captures with it.

**The swing separates with room**: those eleven empty windows run 14.7 to 17.7 dB
while the real windows reach 218 with a tenth percentile of 18.9.
`ConfidentSwingDb` is already 20, already calibrated against this same question
on two independent sets of evidence, and until now decided only which speed
estimate the decoder started from. Eighteen would keep one more capture and
eighteen is the empty windows' own maximum rounded up, which is fitting a
constant to a fixture.

**What it costs, named**: `cw-2026-08-23-001831`, a pileup with nothing
adjudicated in it, swinging 19.3 against a bar of 20. **What it holds**: all four
recordings that emit nothing give **nought** Keying windows out of twenty-five
each, and a test drives the meter a window at a time to keep that at nought
rather than at few.

### Task 4 — one decoder, not two

**The divergence is not two code paths. `Listen` calls `Process`; the only
difference is chunk size.** `Process` set the mixer's pitch once per chunk from
the tracker's state *after* the tracker had consumed that whole chunk, then mixed
the whole chunk at that one pitch — so with a chunk four hops long the first
three hops were mixed at a pitch the tracker only reached at the end of the
fourth.

`CwDecoder.cs`, the line that read `_tracker.Process(chunk.Samples, …)` followed
by `_probabilistic.Process(chunk.Samples)`. The application feeds 960 samples and
the floors harness feeds 240, so **the suite and the operator were reading two
different decoders**. Measured on `cw-2026-08-22-032113` before the repair: fed
240 it tracks 650 Hz, fed 960 it tracks 500, and the text moves with it.

The fix was contained — the decoder walks the audio a hop at a time whatever size
it arrives in. **Acceptance met in full**: every capture in the tree now reads
identically at 240, 480, 960, 1920 and 4800 samples a chunk, and identically
through `Listen` and `Process`, in both text and tracked pitch. The 240-sample
answer is unchanged, so nothing measured through the harness moved. Floors
intact, success tests green.

### Task 5 — the joint cutter: built, measured, not shipped

Option A built as specified: character validity folded into the path score, as a
bonus when a candidate cut completes a letter the alphabet knows, in the length
penalty's own dimensionless units — the only scale here that means the same thing
on two recordings. It biases segmentation only; a character the alphabet does not
know still prints as the placeholder.

Swept against the success tests and the floors:

| weight | success tests + floors | adjudicated characters read | better | worse |
|---|---|---|---|---|
| 0 | 36 of 36 | 158 of 384 | — | — |
| **0.5** | **36 of 36** | **158** | **none** | none |
| 1.0 | 34 of 36 | 158 | none | none |
| 2.0 | 32 of 36 | 156 | the ARRL bulletin | **`VA3VRR`** |
| 4.0 | 25 of 36 | — | — | — |

**The largest safe weight is 0.5 and at 0.5 it buys nothing.** It changes seven
of twelve transcripts and improves not one adjudicated reading. At 2.0, the only
weight where it measurably helps anything, the single gain is paid for with an
adjudicated callsign — which is the threat the instruction named as an acceptance
test rather than an afterthought.

So: nothing shipped, and nothing left behind. The bit-coded alphabet lookup the
term needed was reverted with it rather than left as machinery nothing uses.

### Task 6 — void

The thirteen captures of 2026-08-25 are not in the tree, so there was nothing to
floor. Stated rather than silently skipped.

### The suite

| | baseline | end |
|---|---|---|
| engine | 30 failing of 1674 | **29 failing of 1724** |
| app | 483 passing, 0 failing | **487 passing, 0 failing** |

**Not one test broke across the five tasks that shipped.** The single difference
against the baseline is the flaky rig test, which passed this time.

## 2. What Tim should expect at the radio tonight

**The band row is where he can reach it.** It sits below the readout across the
full width, and every card takes a click at every window size — including the
narrow one where `15 m` and `10 m` were unreachable last night.

**The keying meter tells the truth more often than not, for the first time.** It
was right about ten of the twenty-three recordings in the tree and is now right
about sixteen. On a band with a station on it, it says keying; on an empty one it
still says nothing, and that half was checked window by window rather than over
whole files, because the whole-file reading is not the one he gets.

**The decoder reads the same way the tests read it.** Until now the application
fed it audio in a size that made it track a different note from the one the suite
measured — on one capture, 500 Hz against 650. That is gone, so a number in a
report is now a number about his radio.

**Nothing about the transcript itself changed.** The adjudicated readings come
back exactly as they did: `VA3VRR`, `AA4MP/4 QNIK`, the ARRL bulletin and
`DE KD0UN KD0UN K`.

**What will look wrong and is not:**

- **The bulletin still reads badly.** 40 % of the adjudicated text, and the
  worst line gives back 5 characters of 35. That number is now on the record
  instead of being absent, which is the change — the reading itself is no better.
- **`N4L` is guarded at `N4`, two characters of three.** The decoder puts a word
  gap inside the callsign. That is the joint cutter's fault and the joint cutter
  did not ship.
- **The keying meter is still wrong about six recordings.** They fail on the
  keying score, not on the element length, which is a separate question.
- **Nothing from task 5 is in the tree**, and task 6 produced nothing.

## 3. What you should see

**Twelve adjudicated readings are now guarded, and every one survived tasks 3, 4
and 5 untouched.**

| reading | ruling | guarded run | of |
|---|---|---|---|
| `VA3VRR` | HM-DEC-145 | **6** | 6 |
| `DE KD0UN KD0UN K` | work instruction 011 | **16** | 16 |
| `AA4MP/4 QNIK` | HM-DEC-126 | 9 | 12 |
| `N4L` | HM-DEC-144 | 2 | 3 |
| the ARRL bulletin | HM-DEC-115 | 22 | 57 |
| `110, 110, AND 110 WITH A MEAN OF 117` | Tim 2026-08-25 | **31** | 36 |
| `N OF 117. LINKS TO ARTICLES OR OTHER WEBSITES MENTI` | Tim 2026-08-25 | 22 | 51 |
| `THIS BULLETIN CAN BE FOUND IN TELEPRINTER, PACKET, AND INTE` | Tim 2026-08-25 | 17 | 59 |
| `DICTED 10.7 CENTIMETER FLUX IS 125, 125` | Tim 2026-08-25 | 11 | 39 |
| `2026 PROPAGATION FORECAST BULLETIN ARLP034` | Tim 2026-08-25 | 9 | 42 |
| `2, 2, AND 2 WITH A MEAN OF 2.9. PRE` | Tim 2026-08-25 | 5 | 35 |
| `ACKET, AND INTERNET VERSIONS` | Tim 2026-08-25 | 3 | 28 |

**153 of 384 characters, 40 %.** Two readings are whole. The two weakest anchors,
three and five characters, are weak guards and are marked as such in the test
rather than dressed up.

They were checked after every shipped change, and they are what stopped task 5:
the validity term went red on two of them at weight 1.0 and took `VA3VRR` at 2.0.
**That is the mechanism working on its first use** — before this unit, that term
would have shipped on a whole-corpus character count that improved.

## 4. What's blocking us

**Three rulings from today have no ids, and the adjudication is one of them.**

Today's adjudication of the seven W1AW captures, today's ruling on the band
display, and today's conditioned ruling on the keying witness are all in force,
all implemented, and none is in `DECISIONS.md`. The session does not mint ids
(§12.1). The truth file and this report are currently the whole record of the
adjudication, and twelve tests now rest on it.

This sits on top of the standing gap: `DECISIONS.md` has no record for
HM-DEC-096–133, 136, 141 or 150. **Fourteen consecutive units have worked beside
rulings they cannot read**, and this is the first one where a *test* depends on a
ruling that exists only in prose.

*Rejected: minting ids in the session.* §12.1 is explicit and this is exactly the
class it protects — an adjudication of what a station sent is the strongest form
of "what the display asserts".

---

**The joint cutter cannot be safely weighted, and the reason is sharper than
"not yet".**

Option A is built and measured and the numbers are in section 1. The shape of the
result matters more than the values: **the term's effect on adjudicated text is
flat until it is strong enough to break something.** From weight 0 to 1.0 it
rewrites seven of twelve transcripts and moves the adjudicated character count not
at all — 158 either way. The first weight that moves it, 2.0, moves it *down*,
buying four characters of the ARRL bulletin with six of `VA3VRR`.

That is not a weight waiting to be tuned. It says validity is pulling cuts toward
letters that are not the ones sent, which is §0.0's own failure mode wearing a
better score, and the success tests caught it on their first outing.

*Rejected: shipping at 0.5 because it is safe.* A term that changes seven
transcripts and improves none of them is churn nobody can review.

*Not attempted: validity scored against the fitted clock as well as the
alphabet* — the instruction's phrase "against the fitted clock" was implemented
as the existing length penalty rather than as a second term. If the cutter is
worth another attempt, that is the untried half, and it should be measured
against `N4L`, whose failure mode is exactly a cut inside a character.

---

**The starting pitch is worth more than anything the cutter could buy, and it is
unruled.**

Task 1 measured it by accident. Starting the decoder at each station's own
recorded note rather than at the operator's 600 Hz changes the adjudicated
reading of five captures, three of them substantially: `032113` from 4 characters
to 22, `032012` from 22 to 43, `032050` from 17 to 24. It costs two: `031905`
from 12 to 7, `032129` from 10 to 7.

Hamlet cannot know the station's note before it finds it, so this is not a
setting. What it suggests is that **the decoder should re-read a window once the
tracker has settled**, rather than living with whatever it started from — which
is a real feature and a large one, and outside anything ruled.

*Rejected: acting on it in this unit.* It is not in the instruction, and §12.6.

---

**The band card's own geometry is not what any API says it is.**

Every geometry API in Avalonia agrees with every other and all of them disagree
with the hit test by thirteen pixels vertically and a third of the height. The
test now works around it and says so. Whether this is the headless renderer only,
or something the operator's window does too, is unmeasured — and it bears on every
future test that asks where something is drawn.

*Rejected: chasing it in this unit.* The workaround is honest and the band row is
reachable; the question is a day's work with no defect behind it yet.

### Asks still outstanding

Carried forward verbatim per HM-DEC-139 and HM-DEC-140. **Thirteen inbound. The
adjudication and the witness verdict were acted on this unit under Tim's rulings;
both now need ids, which is item 4.** The oldest of the rest is open since
2026-08-14.

1. **The sweep's `invented` column counts substitutions, not invented
   characters.**
2. **Whether the refill guard should apply to the first fill at all.**
3. **`ANNUNCIATOR.md` renamed `PHASE` to `TASK` while HM-DEC-150 makes `PHASE`
   match the version's minor.**
4. **`DECISIONS.md` has no record for HM-DEC-096–133, 136, 141 or 150** — and now
   also none for today's three rulings, one of which twelve tests rest on.
5. **The tone tracker** — narrowed by the hold, not closed.
6. **The integrator width** — bears on `014113`/`014308`.
7. **The guard's gap is two to one**, calibrated on two empty captures.
8. **A boxcar's nulls made two of five swept offsets pathological best cases.**
9. **Two stations closer than 125 Hz are not named** — and `competing`'s
   diagnosis shows the 125 Hz floor is not what blocks it.
10. **The keying witness** — acted on; it is now right about sixteen of
    twenty-three and the remaining six fail on the score.
11. **HM-OPEN-057** (2026-08-22) and **HM-OPEN-007** (2026-08-14).

New this unit: **the joint cutter cannot be safely weighted**, above; **the
starting pitch is worth more than the cutter**, above; **the band card's geometry
disagrees with its own hit test**, above.

Still open: **the lock's mixed help with nothing telling the operator which**;
**the "Hold this pitch" button**; **three fixtures at accepted cost**;
**`001520`'s quadrillions**; **the reference/port integrator difference**;
**`CLAUDE_CODE.md`'s version line**; **an unmeasured pitch costs `N4L`**;
**`014113`/`014308`'s second mechanism**; **the six-hertz window disagreement**;
**the short-character bias needs a per-character expectation**; **the thirteen
captures of 2026-08-25, absent for a fourth unit**.

Closed this unit: **HM-OPEN-060**, by task 2. **The two drive paths**, by task 4.
**HM-DEC-126's reopening**, by task 1 building the success test its entry called
for.

Also noted and not acted on (§12.6): **`CHANGELOG.md` stops at 1.9.0** while the
version is now 1.11.9, and its convention paragraph still describes HM-DEC-063's
meaning of minor and patch, which HM-DEC-150 superseded. **A second rig test
flakes**, `RigDisconnectTests.TheStateMonitorDoesNotHoldUpADisconnect`.
