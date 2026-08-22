# OUTPUT.md

## 1. What Claude did

**The trigger fired three times across the corpus, not nought.** Every recording
and both fixtures give nought. **The sensitivity sweep gives three**, out of a
hundred and twenty-four runs of it: once at fifteen decibels, where the tracker
went 650 to 575 hertz at 7.5 seconds on one seed of four, and once each at nine
and eight decibels, where it went 600 to 675 at 6.0 seconds on another seed.

Those are true instances of the ruled line — a move of at least the decoder's own
bandwidth, made while somebody was being read — and what the tracker moved to is
noise rather than a second sender. **One of the three costs something**: fifteen
decibels went from 0.94 right and 0.00 invented to **0.92 right and 0.08
invented**. The other two cost nothing measurable. Every other level is unchanged.

**So the order's premise, that it fires on nothing in the current corpus and
shipping it therefore cannot make things worse, is false at one level.** The
ruling is Tim's and the order rejected leaving the machinery dormant, so it is
switched on and this is the first thing in the report rather than a footnote. It
is one line to reverse.

Claude Code on the development computer, `C:\Source\HamLet`, on `main`. Gate
verified against the tree: `Hamlet.sln` and `CwProbabilisticStream.cs` present, no
`CoreHMI.sln`, no `src\CoreHMI`, `PROJECT_CARD.md` says Hamlet. **No radio was
connected and nothing here is evidence about the radio** (HM-DEC-093). Nothing was
recorded under §12.1.

### Task 1 — the trigger, on the ruled line

`CwDecoder.ShouldClearWindow(fromHz, toHz, reading)`, read once per tracker
reading against the pitch at the previous reading.

- **Where the bandwidth comes from.** `CwProbabilisticDecoder.BandwidthHz`, the
  constant the stream's own quadrature filter is built from — the same field the
  mixdown uses to size its boxcar. No literal appears in the comparison, so if
  that filter ever widens the line widens with it.
- **What "while something was being read" resolves to.** `_probabilistic.Last
  .Text.Length > 0`, the decoder's own current reading. That is the thing being
  protected rather than a proxy for it: a keying verdict takes three seconds to
  form, and a signal margin says a tone is present rather than that anybody is
  reading it.
- **A move is one step, not an accumulation.** The reference is the pitch at the
  previous reading, five milliseconds earlier, so a jump crosses the line and a
  walk does not. That is what keeps the two-step settle onto one station off it:
  600 to 650 on the sweep's fixture and 475 to 525 on `004507` are 50 hertz each.
- **`StationChanges` is left exactly as it was and nothing reads it.** Its meaning
  is unchanged, so nothing depends on a meaning that moved. It is not the trigger,
  because measured last session it fires twice on `004507` with nothing read and
  not once on the two-station fixture.

### Task 2 — what it changes in the corpus

**Every recording is character for character last session's string**, and the
trigger fired on none of them:

| recording | window clears | text |
|---|---|---|
| `004507` | 0 | `E AT ARRL DOT NET <BT> E ACH STATION HANDLING ET HIS M E S S A G E P E` |
| `003016` | 0 | `E ■I KPA1■IS<HH> ■NK <BT> STILLHVEMY ETO 91B E TT JETST VFB TUBE LIN` |
| `003126` | 0 | `E S 5 IWATTCH ATL E<AS>T 2 IOVI ES A DAY WID X■ WHY N■TT E E , WESTERNS , E` |
| `003758` | 0 | `E ■HES EHEHSE AA■IH/5■IS E E E EAN EANQNI<HH>SK  E E E E E E EIIE` |
| `014854`, `014935` | 0 | silent, offline and streamed |
| two-station fixture | 0 | unchanged |

**The sweep, against last session's, every level:**

| dB | 18 | 17 | 16 | **15** | 14 | 13 | 12 | 11 | 10 | 9 | 8 | 3 | 0 | −5 | −6 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| was, right/wrong | 1.00/0.00 | 1.00/0.00 | 1.00/0.00 | **0.94/0.00** | 1.00/0.00 | 1.00/0.00 | 1.00/0.00 | 0.92/0.06 | 0.94/0.03 | 0.86/0.08 | 0.83/0.11 | 0.72/0.19 | 0.56/0.33 | 0.03/0.14 | 0.00/0.00 |
| now | 1.00/0.00 | 1.00/0.00 | 1.00/0.00 | **0.92/0.08** | 1.00/0.00 | 1.00/0.00 | 1.00/0.00 | 0.92/0.06 | 0.94/0.03 | 0.86/0.08 | 0.83/0.11 | 0.72/0.19 | 0.56/0.33 | 0.03/0.14 | 0.00/0.00 |

**Fifteen decibels is the only level that moved**, and it is one of the three
firings. **It is not short-window guessing**: the refill guard was swept at 3, 4,
6, 8 and 12 seconds and every length gives the same 0.92 and 0.08 at that level,
so the invention is the decoder reading at 575 hertz after the tracker was dragged
there, rather than the window being briefly thin. Without the clear, the held good
audio was outvoting the noise; with it, what the tracker moved to is all there is.

### Task 3 — proving it fires when it should

`WhenTheWindowIsEmptiedTests`, four of them, and no audio fixture was synthesized
for any of them.

- **A move of at least the filter width while reading empties it** — at the width
  exactly, in both directions, and far beyond it.
- **A smaller move does not**, including the two real settles this corpus makes,
  600 to 650 and 475 to 525, and one hertz under the line.
- **A long move with nobody being read does not**, including `004507`'s 600 to 475
  in its first two seconds, and the first reading of all, where there is no
  previous pitch to have moved from.
- **Emptying drops the held audio and the leading edge and keeps what was
  settled.** Real audio goes through the stream until characters settle, then
  `Restart` is called directly: the envelope is empty afterwards, the last reading
  is empty, the leading edge was raised empty, and the settled count and the
  settled text are exactly what they were. Nothing already said is taken back or
  said twice.

### Task 4 — the sentence, proved on the real window

`TheFollowedSentenceReachesTheScreenTests` builds the actual window headless, puts
the terminal on the canvas the way the operator does, and reads the text out of
the visual tree. It asserts the sentence is drawn while the window is refilling
and gone once it is not. **No property is consulted for the proof** — a property
returning the right string and an element that is not on the screen look identical
to a test that reads a view model, which is exactly how the capture press
disappeared.

To make that possible the state is carried as `ListeningAfresh` on the view model,
the way `DecodingIsSuspended` already is, and set from the decoder on the poll.
The sentence itself sits in `Advisories()` above the capture note, so it lands in
the fixed-height region and **the transcript does not move** (HM-DEC-080).

### Task 5 — the version

**`Directory.Build.props` moved 1.10.4 to 1.10.5.**

### The order, checked against the rulings it cites

Every ruling cited says what the order says it says: HM-DEC-120 the emission
property, HM-DEC-009 no confident wrong answer, HM-DEC-096 phase 3 the interlock,
HM-DEC-091 one source, HM-DEC-080 the fixed-height region, HM-DEC-150 the version
scheme, HM-DEC-093 with `SHACK_FACTS.md` the no-radio rule. **No mismatch.**

**One premise in the order is contradicted by measurement**, and it is the safety
argument rather than a ruling: "It fires on nothing in the current corpus." It
fires three times on the sensitivity sweep. Reported rather than repaired.

### The inbound asks queue

Every id it names is `status: open` in `OPEN_ISSUES.md`. Nothing on it is closed,
and nothing open and relevant is missing.

## 2. What Tim should expect

**The first time somebody answers his call on a different pitch, the terminal will
stop, say that Hamlet has moved across to them and let go of what it was holding,
and pick up again a few seconds later with the new station's text.**

Build clean, no warnings, version 1.10.5. **28 failing, the same 28 by name as
when this unit started.** The engine suite gained four tests and the app suite one,
all green.

**What will look wrong and is not:** nothing in the app looks different until a
move that big happens, and on everything in this repository except three runs of
one synthetic fixture, it never does.

**What is genuinely worse:** the sensitivity sweep at fifteen decibels now returns
0.08 of the message as wrong characters where it returned none. That is the clear
firing on a tracker move onto noise. It is one level of one fixture, the four real
recordings are untouched, and the line above tells him what it buys.

## 3. What we should do next

- **Ask the decoder whether a new sender is speaking**, which the order parks and
  which is the answer that does not depend on the tracker being right about a
  pitch. All three firings this unit measured were the tracker moving onto noise
  while a station was being read, and a speed-and-fist test would not have been
  fooled by any of them.
- **Look at why the tracker leaves a station it is reading for a bin 75 hertz
  away**, which is what those three firings are. HM-DEC-127 already forbids
  abandoning a confirmed station for a candidate far below it, and this looks like
  the same fault surviving in a different form.
- **`003758` and `003016` are still short of their pre-removal strings.**
- **`FollowSpeed` still has no supplier.**

## 4. What's blocking us

Nothing blocks the next unit. One ask.

> **The window clear stays on, or it comes off until the decoder can say who is
> sending.**
>
> It is built exactly as ruled and it is switched on. What the ruling assumed, and
> the order stated, is that it fires on nothing here; it fires three times on the
> sensitivity sweep, and one of those three costs fifteen decibels 0.08 of the
> message as invented characters where it invented none.
>
> **All three firings are the tracker leaving a station it is reading for a bin
> seventy-five hertz away that holds noise.** The ruled line is doing what it says
> and the thing feeding it is wrong, which is the same shape as the previous
> unit's finding about `StationChanges` in a different place.
>
> **The choice is between two costs**: leave it on and accept invented characters
> where the tracker is dragged onto noise, in exchange for the protection when
> somebody really does answer; or take it off until the decoder itself can say a
> different sender is speaking, and accept that a real handover keeps being
> decoded as one station until then.
>
> **Rejected: tuning the bandwidth line so the three firings fall below it.** The
> line is 60 hertz because that is the decoder's filter and the argument is
> physical; a number chosen to make a test green is a different ruling wearing the
> same clothes.
>
> **Rejected: a longer refill guard.** Swept at 3, 4, 6, 8 and 12 seconds and it
> changes nothing at the level that moved, so the invention is not a thin window.

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
- **Whether the window clear stays on**, first made today, above.
