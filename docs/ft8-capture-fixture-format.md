# The FT8 capture fixture format

**What this is for.** There is no WSJT-X on the development machine and there
never will be — the only machine that can run it is the one with the radio on it.
So WSJT-X enters this project as a **committed file**, not as a program. Tim runs
one command at the shack over a capture, commits the `.wav` and the fixture beside
it, and from then on any session on any machine can score Hamlet against what
WSJT-X actually returned for that exact audio, message by message.

**And the reason the format is this careful.** A file like this is trusted
completely by every unit that comes after it. Six units from now nobody will
remember which capture was real and which was a worked example, and nobody will
re-check by hand. So the file has to say, in itself, **which audio it is about**
and **what produced its rows**, and the reader has to refuse rather than guess when
either answer is wrong. A stale fixture does not fail — it quietly measures the
wrong thing and reports a number that looks exactly like a good one.

Written by unit 244 against `PHASE_PLAN.md` step 0.

---

## Where the files live, and why there

```
tests/fixtures/ft8/
  captured/                 real captures and their fixtures
    <stem>.wav
    <stem>.fixture.txt
  example/                  worked examples, never scored against
    <stem>.wav
    <stem>.fixture.txt
```

**The sidecar sits beside the audio, same stem.** That is CW's precedent —
`tests/fixtures/cw/captured/` has held a `.wav` and a `.txt` of the same stem side
by side for forty units — and it is the arrangement that makes a capture and its
truth impossible to separate by accident. A rename that moves one and not the other
is caught by the hash rather than going unnoticed.

**Three deliberate divergences from CW's sidecar.**

1. **A dedicated `ft8/` tree, not CW's.** The FT8 decode path is built on
   `Ft8WaterfallGeometry.DefaultSampleRate`; every committed CW capture is 48 kHz.
   Mixing them in one folder invites a scorer to pick up a file the decoder cannot
   read and report the result as a decoder fault.
2. **The extension is `.fixture.txt`, not `.txt`.** CW's `.txt` sidecar is a
   *state snapshot* — `key value` lines carrying the rig's filter setting and
   S-meter. This file is a *truth list*. They are different kinds of document and a
   distinct extension stops any tool, or any person, reading one as the other.
3. **`captured/` and `example/` are different folders.** Not a flag inside one
   folder — a folder. The provenance field is the check that matters, but a reader
   pointed at `captured/` should never have an example in its hands in the first
   place, and one directory listing should answer *how many real fixtures do we
   have.*

**Today `captured/` is empty**, and that is correct: `SHACK_FACTS.md` FACT-004
records that the radio lives on a different computer. Zero real fixtures is the
expected state on the development machine and is not a defect. It becomes one the
moment a fixture is committed that names a capture which is not there.

---

## The file

Plain text, UTF-8, one logical item per line. Two kinds of line carry meaning:
**header lines** (`key  value`) and **row lines** (`ROW  …`). Blank lines are
ignored, and so is any line whose first non-space character is `#`.

Header keys are single tokens; the value is everything after the first run of
whitespace, trimmed. **All seven header keys below are required.** A fixture
missing one is refused, because the alternative is a file that exists, reads
cleanly and is missing the field that made it trustworthy.

| key | value |
|---|---|
| `format` | `1`. The format version. A reader that does not know a version refuses rather than reading it optimistically. |
| `capture` | The audio file's name — **a bare file name, no path**, resolved beside the fixture. A path would let a fixture point outside its own folder. |
| `utc` | When the capture was taken, ISO 8601 with a `Z`: `2026-09-04T21:30:15Z`. UTC always; the shack is UTC−04:00 and the CW manifest already records one evening's captures reading as two dates. |
| `sha256` | The SHA-256 of the capture file's **bytes**, lower-case hex, 64 characters. Not of the samples, not truncated — of the file, so anybody holding the `.wav` can recompute it with any tool on any machine. |
| `sampleRate` | Samples per second of the capture, as an integer. Stated so the reader can refuse a capture the decoder cannot read; `WavFile` reads the rate but checks nothing against it. |
| `provenance` | One of exactly two tokens. See below. |
| `generator` | Free text naming **what actually produced the rows** — the program, its version, and the command if there was one. Read by a person, not parsed. |

Then **one `ROW` line per message**, in the order the producing decoder emitted
them:

```
ROW  <snrDb>  <dt>  <freqHz>  <message>
```

- **`snrDb`** — decibels, signed, decimal point allowed. Invariant culture.
- **`dt`** — seconds, signed, decimal point allowed. The time offset of the
  transmission within the slot.
- **`freqHz`** — hertz, the audio frequency of the transmission in the passband.
- **`message`** — everything remaining on the line. It contains spaces; it is
  always last for that reason, and it is never quoted.

**Why that column order.** It is WSJT-X's own display order, so a row in this file
lines up with a row on the screen Tim is looking at when he generates it. Getting
`dt` and `freq` the wrong way round is the kind of error that produces a
plausible-looking file, and matching the source's own layout is the cheapest guard
against it.

**At least one row is required.** A fixture with no rows is not a measurement that
found nothing — it is a fixture that will silently score every decoder as having
missed nothing, forever. If WSJT-X returned nothing for a capture, that capture is
not a scoreboard and no fixture is written for it.

### `provenance` — the field that is not in the plan

The plan names the capture, the UTC, the SHA-256 and the rows. **This field is unit
244's addition**, and it matters more than the rest of the format, because it is
the difference between a measurement and a fabrication.

| token | means | may be scored against? |
|---|---|---|
| `wsjtx` | The rows are the output of a real WSJT-X run over this exact audio. | **Yes.** |
| `example` | The rows came from something else and **make no claim about WSJT-X.** | **No.** Reading is fine; scoring is refused, loudly. |

**Any other value is refused at parse time.** Not defaulted to `example`, not
defaulted to `wsjtx` — refused. An unrecognised provenance means the file was
written by something this reader does not understand, and neither default is safe:
one silently discards a real measurement, the other silently promotes a fake one.

**Only the shack generator writes `wsjtx`**, and it writes it only after a real
decoder run returned real rows. Nothing else in this repository may write that
token, and a fixture row that claims to be WSJT-X's and is not would be the single
most damaging artefact this project could carry — indistinguishable from a real one
to every session that comes after, and trusted by all of them.

---

## The worked example, whole

`tests/fixtures/ft8/example/ft8-example-244.fixture.txt`:

```
# Hamlet FT8 capture fixture - format 1
#
# THIS IS AN EXAMPLE AND ITS ROWS ARE NOT WSJT-X'S.
#
# The audio beside it was synthesised by this repository's own ladder, which knows
# exactly what it transmitted, where it put it and at what ratio. So every row below
# is ground truth about a signal we built, not a decode of a signal we heard.
# provenance is "example" and Ft8CaptureFixture.RequireScorable REFUSES to score a
# claim against it.
#
# It exists so the reader has something to be tested against, and so Tim has something
# to hold his first real one beside. See docs/ft8-capture-fixture-format.md.
#
# On the snrDb column: it is the ratio actually DELIVERED onto the samples, each
# transmission's own power against the noise power the fixture itself mixed in. That is
# a figure this repository computed about audio it made. WSJT-X reports a MEASURED
# per-message SNR, which is not the same measurement - and is exactly why this file's
# provenance can never be "wsjtx".

format      1
capture     ft8-example-244.wav
utc         2026-09-04T00:00:00Z
sha256      9a4b71aa6820f18047c04018fc7a7d7112a3045716f3acd15bb5116247b2688d
sampleRate  12000
provenance  example
generator   Hamlet unit 244, Ft8ExampleFixture, three synthesised transmissions at a commanded 5.0 dB with noise seed 244001

ROW     5.0    0.48    1000  CQ K1ABC FN42
ROW     5.0    0.64    1500  CQ W9XYZ
ROW     5.0    0.80    2000  K1ABC W9XYZ -11
```

That is the committed file, whole, copied out of it rather than composed here.

**It is regenerated and checked on every test run.** `Ft8ExampleFixtureTests`
rebuilds it from `Ft8ExampleFixture.Build` and compares digest and text against
what is committed, so the example cannot drift away from the code that made it. To
rewrite it after a deliberate change:

```
dotnet test tests/Ft8Sharp.Tests -e HAMLET_WRITE_FT8_EXAMPLE=1 --filter Ft8ExampleFixtureTests
```

**Where the example's numbers come from, one column at a time**, because "we made
it up" and "we measured it" must never be ambiguous in a file like this:

- **message** — the ladder was handed these messages and encoded them. Ground truth.
- **freqHz** — the frequency the synthesiser was told to put the lowest tone at.
  Ground truth.
- **dt** — the sample offset the signal was written at, divided by the sample rate.
  Ground truth.
- **snrDb** — the ratio actually delivered onto the samples, from
  `SignalToNoise.DecibelsFor`, which is computed from the signal power and the
  noise power the fixture itself mixed. It is a property of the **whole slot**, so
  every row carries the same figure. On a real capture WSJT-X reports a **per
  message** SNR, and these are not the same measurement — which is exactly why this
  file's provenance is `example`.

---

## Where the row format came from

**Stated explicitly, because a parser that guesses is how a wrong number reaches a
report.**

The understanding of WSJT-X's decode line shape in this repository comes from **one
place**: `tests/Ft8Sharp.Tests/Dsp/ReferenceRecordings.cs`, whose header documents
upstream `ft8_lib`'s own print format verbatim as

```
"%02d%02d%02d %+05.1f %+4.2f %4.0f ~  %s\n"
```

— that is `HHMMSS`, SNR, dt, frequency, a `~`, then the message. `ft8_lib` prints in
that shape because it mimics WSJT-X's own display; that is a published, observable
output format, and reading it is the *testing rather than derivation* the project's
rules permit.

**No WSJT-X source was read**, and none may be. Nothing under `ft4_ft8_public/` was
read.

**What follows from admitting that.** This is knowledge of one program's output
format transcribed from a second program that imitates it — it is good enough to
write a parser against and **not** good enough to write a lenient one against. So
the shack generator's parser (`WsjtxDecodeLines`) is **strict and loud**: it takes
lines in exactly that shape, refuses anything else by name, and **never skips a
line it does not understand.** If Tim's first real run produces lines this parser
refuses, the refusal message carries the line verbatim and the parser is corrected
against a real sample — which is the right way round. A parser that silently
dropped the lines it did not recognise would produce a fixture that is short by an
unknown number of rows, and nothing downstream could tell.

---

## How the fixture is read, and the four ways it refuses

`Ft8CaptureFixture` in `tests/Ft8Sharp.Tests/Fixtures/`. Every refusal throws
`Ft8FixtureException`, naming the fixture, the capture and what was wrong. **None
of them is a skip, a warning, or an empty list** — quiet is the failure mode this
whole exit exists to prevent.

1. **The named capture is absent.** The `.wav` the `capture` key names is not beside
   the fixture.
2. **The hash does not match.** The capture is there and its SHA-256 is not the one
   recorded. *This is the one the plan calls out by name: a stale fixture silently
   measures the wrong thing.*
3. **A row is malformed.** Too few fields, an unparseable number, an empty message —
   or a header key missing, unknown, repeated, or carrying an unrecognised
   `provenance`.
4. **The provenance is not `wsjtx` and the caller asked to score.** Reading an
   example fixture is fine. Scoring a claim against one is not.

**Zero fixtures and a missing capture are different things.** An empty
`tests/fixtures/ft8/captured/` is a clean pass — it is FACT-004's expected state.
A fixture that *names* a capture which is not there is a hard failure. Those two
cases are tested separately.

**The message comparison is not this file's to invent.** It is
`ReferenceRecording.Normalise`, called and not re-implemented: trim, then cut at the
first run of two or more spaces, and **nothing else** — no case folding, no bracket
stripping, and `RR73` and `RRR` stay different messages.

---

## How a fixture is scored

**Two entry points, and the split is the point.** `Ft8LadderHarness.ScoreFixture`
is the call that makes a claim about WSJT-X, and it calls `RequireScorable` before
it does anything else — so that sentence can never be produced from a worked
example. `Ft8LadderHarness.Compare` does the same arithmetic without the claim, and
**prints the fixture's provenance at the top of every report**, so counts cut out
and pasted elsewhere carry the qualification with them.

Either way it decodes the named capture with **every decoder `Available()`
returns** and reports, per decoder, **three counts, never two**:

- **matched** — a message in the fixture that this decoder also returned;
- **missed** — a message in the fixture that it did not;
- **returned wrong** — a message it returned that is not in the fixture, each one
  printed on its own line.

**The third count means something different here than it does on the ladder, and
the two must never be added together.** On the ladder, the harness knows what it
transmitted, so a message returned that was not sent is an error and the phase's
zero-wrong criterion bites. On a real capture the fixture is *WSJT-X's* list, not
the air's: a message WSJT-X missed and Hamlet found is a decode this phase is
actively trying to produce. It is counted, printed and looked at — never scored as
a fault.

**The SNR column is carried and compared by nothing.** Hamlet has no per-message
signal-to-noise ratio today — see `docs/unit244-trace.md` §1 — and `PHASE_PLAN.md`
step 5 is the step that owes it. The column exists so that the day it arrives, the
fixtures already hold the figure to compare against.
