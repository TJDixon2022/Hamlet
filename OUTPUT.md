# OUTPUT.md

## 1. What Claude did

**Turning the clear off restored the sweep exactly.** Fifteen decibels is back to
**0.94 right and 0.00 invented**, which is the figure the order predicted, and
nine and eight decibels came back with it, to 0.94/0.03 and 0.92/0.06.
Every level from eighteen down to twelve is 1.00 right and 0.00 invented. **All
six recordings are character for character what they were**, and the trigger fired
nought times on every one of them.

**Then task 2 measured the three fires and two of them are not what anybody
thought.** They are one station being reported at two bins seventy-five hertz
apart, and the third is a genuine noise bin. **So this stops at the end of task 2,
under task 2's own clause**, and task 3 is not built.

Claude Code on the development computer, `C:\Source\HamLet`, on `main`. Gate
verified against the tree: `Hamlet.sln` and `CwProbabilisticStream.cs` present, no
`CoreHMI.sln`, no `src\CoreHMI`, `PROJECT_CARD.md` says Hamlet. **No radio was
connected and nothing here is evidence about the radio** (HM-DEC-093). Nothing was
recorded under §12.1.

### Two mismatches in the order, reported and not repaired

- **`ANALYSIS-cw-2026-08-22-014113.md` is not in the tree.** Nothing by that name
  exists anywhere in the repository.
- **`cw-2026-08-22-014113.wav` is not in the tree either.** The captured folder
  ends at `cw-2026-08-20-014935`. So the second half of task 2, questions 4 and 5
  about that file, could not be measured. **What could be answered from the code
  is answered below.**

### Task 1 — the clear is off

One line: `CwDecoder.ClearOnAStationChange`, a constant, false. The line, the
emptying, what survives the emptying and the sentence are all still in the tree
and all still tested.

| dB | 18 | 15 | 12 | 11 | 10 | 9 | 8 | 3 | 0 | −5 | −6 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| with the clear on | 1.00/0.00 | **0.92/0.08** | 1.00/0.00 | 0.92/0.06 | 0.94/0.03 | 0.86/0.08 | 0.83/0.11 | 0.72/0.19 | 0.56/0.33 | 0.03/0.14 | 0.00/0.00 |
| off, now | 1.00/0.00 | **0.94/0.00** | 1.00/0.00 | 0.92/0.06 | 0.94/0.03 | 0.94/0.03 | 0.92/0.06 | 0.72/0.19 | 0.56/0.33 | 0.03/0.14 | 0.00/0.00 |

Every recording, clears nought on each: `004507` `E AT ARRL DOT NET <BT> E ACH
STATION HANDLING ET HIS M E S S A G E P E`; `003016` `E ■I KPA1■IS<HH> ■NK <BT>
STILLHVEMY ETO 91B E TT JETST VFB TUBE LIN`; `003126` `E S 5 IWATTCH ATL E<AS>T 2
IOVI ES A DAY WID X■ WHY N■TT E E , WESTERNS , E`; `003758` `E ■HES EHEHSE
AA■IH/5■IS E E E EAN EANQNI<HH>SK  E E E E E E EIIE`; `014854` and `014935`
silent.

**`RefillSeconds` also stopped being a settable static** in the same commit. It
was mutable so a sweep could measure what each length was worth, the answer was
nothing at any length from half a second to twelve, and **a mutable static the
whole suite shares is a way for one test to change another test's numbers without
either of them saying so.** That is the likeliest explanation for nine and eight
decibels reading 0.86 and 0.83 in the last two reports and 0.94 and 0.92 today.

### Task 2 — why a noise bin beat a station

**1. What the tracker scores a bin by.** Admitted bins are ranked by `LiftDb`,
which is how far that bin's key-down level stands above the band beside it. Not
clustering. Clustering is an admission test rather than a ranking: a bin is
admitted only if it shows at least eight marks, a dit between 25 and 200 ms, a
dah-to-dit ratio between 2.5 and 3.8, and a separation between its two mark
clusters of at least 4.0 measured in the marks' own scatter.

**The three fires, with every bin the survey admitted at that instant.** Nothing
here is the tracker's summary; it is the same examination the survey runs, handed
back through a diagnostic added for this (`CwToneSurvey.Candidates`,
`CwToneTracker.CoarseCandidates`). The fixture sends at **640 Hz**.

**Nine and eight decibels, the move to 675:**

| bin | lift | keyed | separation | marks | dit | dah |
|---|---|---|---|---|---|---|
| what it was reading, 650 | 27.6 | −20.8 | 5.3 | 11 | 70 | 214 |
| 600 | 13.1 | −35.3 | 4.9 | 11 | 72 | 225 |
| **675, taken** | 17.7 | −30.6 | **37.1** | 10 | 80 | 218 |

**The station's own bin was not admitted at all on that survey.** The only two
candidates were 600 and 675, and **both of them are the station**: 72/225 and
80/218 against the 70/214 it had been reading. They are the skirts of one signal
either side of a bin that dropped out, and the tracker took the louder skirt. **It
was right both times.** What crossed sixty hertz was 600 to 675, one move between
two bins holding one station.

**Fifteen decibels, the move to 575:**

| bin | lift | keyed | separation | marks | dit | dah |
|---|---|---|---|---|---|---|
| 575, what it went to | 11.7 | −42.8 | 4.3 | **21** | **31** | 98 |
| 725, the only bin admitted at the moment of the move | 7.4 | −47.4 | 6.4 | **20** | **37** | 100 |
| 650, one survey later | 34.1 | −20.6 | 4.7 | 9 | 60 | 217 |

**This one is noise.** A dit of 31 ms where the fixture sends 67, twenty-one marks
in three seconds where the station gives nine, and a separation of 4.3 against a
floor of 4.0. **It is HM-DEC-095's own case**: noise routinely produces
twenty-five-millisecond marks, and here it also produced a cluster separation over
the floor. One survey later the station was back at 34.1 dB of lift and the
tracker returned to it.

**2. What is actually in each bin**, measured by the other instrument rather than
by the tracker's opinion. `KeyingEnvelope`, the independent witness, scores every
bin across the whole range at 0.46 to 0.48 element share with purity 1.00:

| 575 | 600 | 640 | 650 | 675 |
|---|---|---|---|---|
| 0.480 | 0.468 | 0.462 | 0.463 | 0.467 |

**It ranks 575 highest and the true 640 lowest.** Its hundred-hertz boxcar is
wider than the spacing being judged, so on this fixture it cannot separate the
station from its neighbours at all, and where it does have an opinion the opinion
is wrong.

**3. Why the noise bin won.** Not because loudness beat clustering. Two different
mechanisms:

- **Twice, the station's centre bin failed admission and its skirts did not.** A
  real fist gives nine to eleven marks in a three-second window, which is close to
  the floor of eight, so the centre bin drops in and out between surveys while the
  skirts stay in. **The choice was then between two bins that both held the
  station** and loudness picked correctly between them.
- **Once, a noise bin passed admission.** Noise gives twenty-one short marks in
  the same three seconds, so it clears the mark count easily, its dit of 31 ms is
  legal at forty-eight words a minute, and its cluster separation landed at 4.3
  against a floor of 4.0. **Nothing separated it from the station except loudness,
  and at that moment the station's bin was not a candidate.**

**4. What the keying sweep ranks bins by.** `KeyingEnvelope.Best` walks 400 to
1200 hertz in 25 hertz steps and keeps the highest `Score`, which is
`ElementShare × ElementPurity` — how much of the window sits inside plausible
element lengths, times how cleanly those lengths cluster. **Loudness is not in
it.** Whether that explains 625 winning on `cw-2026-08-22-014113.wav` cannot be
said, because **the file is not in the tree**.

**5. Are they the same metric?** No. The survey chooses on `LiftDb`, loudness over
the band beside the bin, having admitted on clustering. The sweep chooses on
`ElementShare × ElementPurity`, clustering only. **They disagree on the fixture
measured here**: the survey's lift varies by twenty-two decibels across 575 to 675
and picks 650, while the sweep's score varies by 0.018 across the same bins and
picks 575.

**And task 2's stop clause applies.** Two of the three fires are the tracker
moving between two bins that both hold the station it is reading, seventy-five
hertz apart, because a station on a twenty-five hertz grid is present in several
bins at once. **The tracker was right, and what crossed the clear's line was a
legitimate move inside one station.** So the clear's premise — that a move wider
than the decoder's filter means a different sender — is false for a reason no
threshold fixes, which is the finding rather than a defect to repair.

### Task 3 — not built, and why

Gated on task 2, and task 2 found that the tracker was substantially right. The
one genuine noise admission is HM-DEC-095's standing case, and the only knobs that
would exclude it are the mark-count floor, the dit floor and the separation floor:
**the noise bin sat at 4.3 against a floor of 4.0 while the station sat at 4.7 to
5.3, so there is no daylight to cut in** and any cut is the threshold-tuning this
order forbids. With the clear off, that admission costs nothing measurable.

### Task 4 — the corpus

Unchanged, and quoted under task 1. **28 failing, the same 28 by name as when this
unit started.** Both recordings holding no keying are silent offline and streamed.

### Task 5 — the version

**`Directory.Build.props` moved 1.10.5 to 1.10.6.**

### The rulings, checked

Every ruling this order cites says what the order says it says. **HM-DEC-095 in
particular**: it rules that a note is chosen by how it is keyed and never by how
loud it is, and that a sender's gaps are classified by clustering that sender's own
gaps. **The survey honours it in admission and not in ranking**, where loudness
decides between admitted bins. That is worth knowing and it is not what caused any
of the three fires, because in two of them both candidates were the same station
and in the third the station was not a candidate at all.

### The inbound asks queue

Every id it names is `status: open` in `OPEN_ISSUES.md`. Nothing on it is closed
and nothing open and relevant is missing.

## 2. What Tim should expect

**The app invents nothing from eighteen decibels down to twelve, and what it
invents below that is exactly what it invented two days ago.**

Build clean, no warnings, version 1.10.6. **28 failing, the same 28 by name.** The
suite is otherwise unchanged: nothing was added this session except a diagnostic
that hands back every bin the survey admitted, which changes nothing the survey
decides.

**What will look wrong and is not:** the window clear is in the tree, fully
tested, and does nothing. That is Tim's ruling and the constant that switches it is
one line.

## 3. What we should do next

- **A station is in several bins at once, and nothing downstream knows it.** That
  is the finding under this unit, and it is what makes a distance test unusable for
  deciding whether the sender changed. It also means the tracker's reported pitch
  can jump seventy-five hertz while reading one station.
- **The station's centre bin drops out of admission between surveys** because a
  real fist gives nine to eleven marks in three seconds against a floor of eight.
  That is worth measuring across the recordings before anybody touches it.
- **`003758` and `003016` are still short of their pre-removal strings.**
- **`FollowSpeed` still has no supplier.**

## 4. What's blocking us

Nothing blocks the next unit. One ask, and one thing needed before the rest of
this order can be executed.

> **The evidence for the second half of task 2 is not in the repository.**
>
> `cw-2026-08-22-014113.wav` and `ANALYSIS-cw-2026-08-22-014113.md` are both
> absent. Questions 4 and 5 of that task, about why the keying sweep ranked 625
> above 600 and 608 on that capture, cannot be answered from the code alone: what
> the sweep ranks by is answered above, and whether that explains the choice needs
> the audio.

> **Deciding that a different person is sending cannot be done by how far the
> pitch moved.**
>
> Measured this session: on a fixture sending at 640 hertz, the survey admitted 600
> and 675 as candidates while the station's own bin dropped out, and both of those
> bins carry the station's own fist — 72 and 225 milliseconds, 80 and 218, against
> the 70 and 214 it had been reading. **A station on a twenty-five hertz grid is
> present in several bins at once**, so the tracker's reported pitch can move
> seventy-five hertz without anybody else transmitting.
>
> **That is why the clear fired three times where the order expected nought**, and
> it is not something a threshold repairs: the moves are real, the bins are real,
> and only the meaning attached to them was wrong.
>
> **The remaining candidate is the one already parked**: ask the decoder whether
> the speed and the fist changed, which is what an operator notices and what does
> not depend on the tracker being right about a pitch. **That is a measurement
> nothing here makes yet.**
>
> **Rejected: raising the separation floor to keep noise out.** The noise bin sat
> at 4.3 and the station at 4.7 to 5.3; there is no daylight between them, and a
> number chosen to make three fires stop is the tuning this order forbids.

### Asks still outstanding

Carried per HM-DEC-139, verbatim until ruled.

- Whether the window clear comes back on once the tracker is right.
- Elements per character, 1.54 against 3, and gap promotion.
- The advice line asserting a cause the app can disprove.
- The sidecar asserting two incompatible things about one span.
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
- **`cw-2026-08-22-014113.wav` and its analysis are not in the tree**, first made
  today, above.
- **Whether a sender change can be decided by pitch distance at all**, first made
  today, above.
