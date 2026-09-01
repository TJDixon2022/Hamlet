READ IN THIS ORDER — A, then B, then C.

A. PHASE — Hamlet hears FT8 off the radio and displays the decoded text on screen.
Seven steps. **Step 1 is closed** (the library exists and its tables are proven);
**step 2 is in progress and this is its second unit**; **steps 3, 4, 5, 6 and 7 are all
not started and remain unreachable until step 2 closes**, every step depending on the one
before it by the plan's own named deviation. Task 1 measured the ground under step 2 and
it is still standing: `Ft8Sharp.Tests` at 74 total, 73 passed, 0 failed, 1 skipped in
5.3 s cold; the library building at 0 warnings and 0 errors with no `PackageReference` and
no `ProjectReference`; attribution at 44 paths from `2828ab6` with not one under
`src/Hamlet.App/`, `src/Hamlet.RadioEngine/`, `tests/Hamlet.App.Tests/` or
`tests/Hamlet.RadioEngine.Tests/`; and all three channels green at 55 and 13.
B. STEP 2 — messages round-trip through 77 bits. SIX exit criteria: (1) CRC matches known
values, must-pass — **closed by unit 206, untouched tonight**; (2) standard, free-text,
telemetry and non-standard-callsign messages round-trip across a large generated corpus,
must-pass — **three of its four categories round-trip tonight** (standard at 200 000
messages, free text at 100 000 strings, telemetry at 100 000 bodies) **and the criterion
stays open, because non-standard callsigns pack against a rolling hash cache which is
parked to unit 208**; (3) any random 77-bit pattern either decodes or fails cleanly and
never throws, must-pass — **claimed CLOSED**: 1 000 000 seeded random patterns, seed
20260901, **0 exceptions, 0 decodes returned for a type not built, 0 decodes returned for
an unresolvable callsign**, and the type cover is complete at 15 of 15 combinations;
(4) contest and DXpedition types round-trip, nice-to-pass — **not built, so the
nice-to-pass half is not met; its must-pass clause IS met**, with 15 combinations
enumerated, 4 built and round-tripping and 11 refused as unsupported by name, none
throwing and none returning a decode; (5) Ft8Sharp tests green, must-pass — **108 total,
107 passed, 0 failed, 1 skipped**; (6) attribution clean from `2828ab6` and the channel
tests green, must-pass — **58 paths, 0 under any Hamlet project**, and all three channels
green with `VersionTests` re-run at 3 of 3 **after** the version bump, `PrivilegeTests`
and `AudioSeamTests` at 55, `DecisionLogOrderTests`, `DecisionEmissionTests` and
`VoiceTests` at 13.
C. THIS REPORT — **No usable message-level known value exists in the pinned clone**: its
live test source drives its own encoder into its own decoder, which is the same
self-consistency this port measures, and the one real vector is three message strings with
stated symbol sequences inside the disabled block of `test/test.c`, against the superseded
72-bit packing rather than the 77-bit layer. **Every corpus here proves self-consistency
and not agreement with upstream** — a field packed in the wrong order round-trips
perfectly — and correctness is standing on 5 machine-corroborated scalars and on step 3's
bit-identical symbol comparison, which is named as where it gets settled. **The
grid-and-report field was exhausted at all 32 768 values: 32 533 round-tripped and 235
were refused, and the two sum to 32 768.** **Task 6 was not dropped** — free text and
telemetry are built and the type cover moved from 2 built to 4. **The Ft8Sharp project
returns in 2.3 s warm and 3.5 s with a rebuild, and no corpus was cut for the clock.**
Section 4 raises 3 items, and **none of them stands in the way of a criterion named in B**.

UNIT:       207 — complete at task 7 of 7 — 2026-09-01 11:55
PHASE GOAL: Hamlet listens to the radio, finds FT8 signals in the audio, and puts the
            words that were actually on the air on Tim's screen.
UNIT GOAL:  Turn a callsign and a grid into the 77 bits FT8 carries, turn 77 bits back
            into words, and put a dispatcher in front of it that gives every one of the
            protocol's type codes a defined answer — so a pattern this library cannot read
            is refused rather than guessed at.
ADVANCED:   yes — criterion 3 closed with three zero counts over a million patterns and a
            complete type cover, and criterion 4's must-pass clause closed with it.
NUMBER:     step 2 must-pass criteria demonstrated: 3 -> 5 of 6
DRIFT:      0 consecutive units without advance  (was 0 — unit 206 advanced)

## 1. What Claude did

**Complete, at task 7 of 7. Nothing was dropped, including the named drop candidate.**

Machine `C--Source-HamLet`, project claimed and verified as Hamlet, branch `main`, HEAD
now `4e4bd72`. Seven commits, one per task, each pushed to `origin/main` before the next
began. **Every push succeeded; none was refused.**

The gate passed against the tree: `SHACK_FACTS.md` and
`src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs` are both present, `Hamlet.sln` is
the only solution file, and neither `CoreHMI.sln` nor `MURC.sln` exists.

**What was traced.** `HEAD` was `c2ab4bf` on `main` as stated. `git status --short`
printed **34** lines where the instruction said 33 — one more, reported below.
`Ft8Sharp.Tests` measured 74 / 73 / 0 / 1. The library built at 0 and 0 with `net8.0`,
nullable enabled, warnings as errors, and no package or project reference. Attribution was
44 paths with no Hamlet path. All three channels green. **The clone-gated tests passed
rather than skipped, so `C:\Source\ft8_lib` was reachable and the port had its route.**

**What was built.** Six new files in `src/Ft8Sharp/Message/` — the character primitives,
the callsign field, the grid-and-report field, the type selectors, the standard message,
free text and telemetry, and the dispatcher. Five new test files and one new inventory
file in `tests/Ft8Sharp.Tests/`. The test count went from 74 to 108.

**What was measured.** Section 3 carries it.

**Decisions this session made for itself, reproduced in full:**

**1. Four values are refused where upstream returns text, and one where upstream returns
nothing.** The governing rule of this unit is HM-DEC-009 — never present a guess as a
decode — and each of these is a place where following upstream exactly would put a
confident wrong answer on the operator's screen rather than no answer. Each is recorded in
`porting-notes.md` with its reasoning, each affects only patterns no conforming
transmitter produces, and none of them changes what this library *sends*, because nothing
in this phase transmits. **(a)** The grid value at the sub-range boundary, which both
sub-ranges reach — upstream's unpacker reads it as the last grid square while upstream's
packer arrives at it from a report of thirty-five below zero, so the bits are ambiguous
and upstream presents one reading as certain. **(b)** One grid square whose four
characters spell a sign-off token; the packer tests for tokens first so it can never
produce that value, while the unpacker can, and the text an operator would read is not the
place those bits name. **(c)** A report code whose number will not fit the two digits
upstream's formatter writes it into — the text it emits contains a character that is not a
digit, so it is not a report and is not anything. **(d)** A free-text body larger than
thirteen characters of a 42-symbol alphabet can be; rather more than half of all 71-bit
bodies are such numbers, and upstream shows the low part of the number as though it were
the message. **(e)** The character lookup refuses a negative index where C would index off
the front of a range — no caller here can produce one, and what it buys is a total
function, which is what lets the dispatcher promise never to throw.

**2. The telemetry packer sets the type selectors, where upstream's sets none.** Upstream's
own comment asks whether it should; without them a message it produces declares itself
free text and cannot be read back as telemetry. The bit the secondary selector needs is
exactly the one upstream's left shift vacates, which is what the shift is for.

**3. Two test corpora were shaped to stop them generating messages the protocol cannot
express.** The standard-message corpus gives both callsigns in a message the same suffix
kind, because one message carries one suffix meaning and a mixed pair is a correct
refusal rather than a failure. The same corpus does not generate the one grid square named
in (b) or the two prefix-collision spellings, because a corpus that generated them would
be measuring those named refusals rather than the message layer. **No assertion was
loosened and no corpus size was cut**; both restrictions are stated in the tests
themselves, and both classes are measured directly elsewhere — the collisions in
`Ft8CallsignFieldTests` at 4971 of a million, the grid square in `Ft8GridFieldTests`.

## 2. What the owner should expect

**Ft8Sharp can now read an FT8 message.** Give it 77 bits and it gives back the text a
person would read, or it tells you it cannot and why. That is the first time anything in
this repository has done that.

**What will look wrong but is not:**

- **The library decodes fewer patterns than `ft8_lib` would.** That is the point. Five
  classes of bits that upstream turns into text are refused here, all listed above and all
  in `porting-notes.md`. In every one of them upstream's answer is a plausible-looking
  callsign, grid or message that the transmitter did not send.
- **Non-standard callsigns do not decode at all.** `EA8/G5LSI` and everything shaped like
  it packs against a 22-bit hash resolved from a rolling cache, and that cache is unit
  208's. Where a message carries one, **the whole message is refused** rather than returned
  with a placeholder in it.
- **Criterion 2 is still open even though three of its four categories round-trip.** The
  fourth category is the non-standard callsigns above.
- **`Ft8CallsignFieldTests` reports 4971 callsigns of a million that did not come back as
  they went in, and the test passes.** They are upstream's own prefix work-arounds
  colliding with calls spelled the same way; the test asserts that *every* mismatch is of
  exactly that shape and that there are no others.
- **A `TheFullTwentyEightBitSweep` test exists and does nothing on an ordinary run.** It is
  268 million round-trips, gated behind `FT8_CALLSIGN_FULL_SWEEP=1` in the
  `FT8_TABLEGEN_WRITE` idiom. It was run once here, in 67 s, and the result is in
  section 3.
- **`tests/Ft8Sharp.Tests/TempEncoderProbe.cs` is still on disk, untracked and emptied to a
  comment.** Not touched; four sessions' sandboxes have refused to delete it and this one
  did not try a fifth time.
- **One `Ft8Sharp` test still skips** — `Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`
  — which is the write gate, not a failure.

## 3. What you should see

### The type cover — 15 combinations, every one with a defined behaviour

| i3 | n3 | type | behaviour | corpus that round-tripped it |
|---|---|---|---|---|
| 0 | 0 | FreeText | **built** | 100 000 seeded strings |
| 0 | 1 | DxPedition | refused as unsupported | — |
| 0 | 2 | EuVhfContest | refused as unsupported | — |
| 0 | 3 | ArrlFieldDay | refused as unsupported | — |
| 0 | 4 | ArrlFieldDay | refused as unsupported | — |
| 0 | 5 | Telemetry | **built** | 100 000 seeded bodies |
| 0 | 6 | Unknown | refused as unsupported | — |
| 0 | 7 | Unknown | refused as unsupported | — |
| 1 | — | Standard | **built** | 171 133 of the 200 000 standard corpus |
| 2 | — | Standard | **built** | 28 867 of the 200 000 standard corpus |
| 3 | — | ArrlRttyRoundup | refused as unsupported | — |
| 4 | — | NonstandardCallsign | refused as unsupported | — |
| 5 | — | WwrofContest | refused as unsupported | — |
| 6 | — | Unknown | refused as unsupported | — |
| 7 | — | Unknown | refused as unsupported | — |

**15 enumerated. 4 built and round-tripping. 11 refused as unsupported. None throws and
none returns a decode for a type that is not built.**

### The fuzz — corpus 1 000 000, seed 20260901

```
exceptions                            : 0
decodes returned for a type not built : 0
decodes returned for an unresolvable callsign : 0
```

237 820 of the million decoded as standard messages before task 6, 261 800 after it added
free text and telemetry. The rest were refused by name: 718 866 unsupported type, 11 678
malformed field, 7 656 unresolvable callsign. **Criterion 3 is claimed closed on these
three zeros together with the complete type cover above.**

### The grid-and-report field, exhausted

**All 32 768 values, no seed and no sampling argument.**

```
round-tripped : 32 533     of which 32 399 grid squares, 130 reports,
                            3 fixed tokens, 1 "no third field"
refused       :    235
sum           : 32 768
```

The 235 refusals are: 1 at the sub-range boundary both ranges claim, 1 grid square whose
name a token has taken, and 233 report codes whose number overruns two digits. **The
second of those was found by the sweep and not predicted** — which is the argument for
exhausting a field rather than sampling it, and it is worth its own line.

Under the `R` flag, where the property is weaker and upstream's own comment says why: 130
decoded and re-packed, 32 403 decoded without re-packing the flag, 235 refused, summing to
32 768 with no exception anywhere.

### The callsign field

**Corpus 1 000 000, seed 20260901**, generated systematically across all eight shapes the
pin's branching admits — four base shapes, each with and without a suffix, and both suffix
meanings. **0 refused at pack. 995 029 round-tripped. 4971 did not, and every one of them
is one of upstream's two prefix work-arounds colliding with a call spelled its compressed
way; there were 0 unexplained mismatches.**

**Special tokens, all by name:** `DE`, `QRZ` and `CQ` each round-trip as tokens; the
numeric CQ family round-trips for all 1000 of its values; the lettered CQ family was
walked over all 531 441 of its values, of which 475 255 round-trip and 56 186 carry a
space in the modifier and do not — upstream's own asymmetry, asserted to be exactly that
shape and nothing else.

**Sub-range boundaries, each with the value on either side:** the first token, the last
bare token, the first and last numeric CQ, the first and last lettered CQ, the last defined
token, the start of the hash range, the end of the hash range, and the top of the field.
Every one produced a defined answer.

**The hashed region refuses rather than guesses.** 400 010 values across the whole 22-bit
region, at both suffix settings, **every one refused as unresolved with no text written**.
On the packing side, `EA8/G5LSI`, `YL/LB2JK`, `PJ4/KA1ABC` and `K1ABC/QRP` are each refused
as requiring the hash cache rather than written as a value nothing could read back. A whole
message carrying one is refused whole — no placeholder, no partial message, no numeric
field dressed as a call.

**The optional full 2^28 sweep was run once, and it is not in the default run.** Gated
behind `FT8_CALLSIGN_FULL_SWEEP=1`. **All 268 435 456 values in 67.0 s:**

```
decoded                        : 262 709 644
    of which not re-packing    :      56 186
unresolved (the hash region)   :   4 194 304
malformed                      :   1 531 508
sum                            : 268 435 456
```

Two things fall out of it that the million-callsign sample could not show. **The
unresolved count is exactly the size of the hashed sub-range**, so the seam covers that
range precisely and nothing on either side of it. And **the only values in the entire
field that decode without re-packing are the 56 186 lettered-CQ modifiers carrying a
space** — every one of the 262 million standard basecall values re-packs to the integer it
came from.

### The standard message round-trip corpus

**200 000 seeded messages, seed 20260901. All 200 000 came back. None did not.** 171 133
packed under the first type code and 28 867 under the second, and 50 351 carried a suffix
on the transmitting station's call. **No packed message ever set a bit past the
seventy-seventh**, asserted directly on every one rather than discovered through the
container's refusal.

### Free text and telemetry

**100 000 seeded free-text strings**, including the empty string, a full-length string, and
strings with leading, trailing and both-end spaces. **95 800 came back exactly and 4200
came back trimmed** — the encoder pads to the full width with spaces and the decoder trims
them, so the two are not distinguishable. That is upstream's shape and it is reported here
rather than narrowed out of the corpus. **0 refused at pack, 0 unexplained.** The whole
42-character alphabet was packed in chunks so that no code point went untried; characters
outside the alphabet are refused rather than substituted.

**100 000 seeded telemetry bodies**, including all zeros and all ones. **All 100 000
round-tripped** and each decoded to exactly 18 hexadecimal digits.

### The character primitives

**All six alphabets walked end to end — 190 code points.** Every one round-trips index to
character to index, and every one is distinct, so **each alphabet is a bijection between
its indices and a set of ASCII characters**. **All 128 ASCII inputs have a defined answer
in every alphabet** — accepted counts of 42, 38, 37, 27, 36 and 10, with the rest cleanly
rejected, and **no exception for any input in any alphabet**. Indices from −600 to 599
answer with the sentinel rather than throwing.

**These prove the mapping is a bijection. They do not prove it is upstream's** — a mapping
in the wrong order round-trips perfectly and is wholly wrong on the air. That is settled by
the corroborated scalars below and, definitively, by step 3.

### Scalars corroborated against the pin by machine

Read out of the pin at run time. **5 corroborated, 3 not.**

| Scalar | Corroborated | How |
|---|---|---|
| Hashed-callsign sub-range size | yes | integer macro in `message.c` |
| Start of the hashed sub-range | yes | integer macro in `message.c` |
| Grid and report boundary | yes | integer macro in `message.c` |
| The six alphabet lengths | yes, **weakly** | the comment beside each enumerator in `text.h`, not a macro |
| The two type selector widths | yes | the mask and shift shapes in `message.c` |
| Type code to message type mapping | **no** | it is a `switch`, not a table |
| Token sub-range boundaries | **no** | literals inside a function body |
| Basecall positional alphabet sizes | **no** | literals in the multiply chain |

**Nothing under `tools\` was edited and neither `CSourceParser` nor `ExpressionEvaluator`
was changed.** `Ft8TableGenerationTests` was re-run and is green at 2 passed, 1 skipped.
No scalar value is printed anywhere — only whether it matched.

### End-to-end: text through the proven encoder

**2000 messages** went text → pack → `Ft8Payload.Create` → `LdpcEncoder` → **all 83 parity
checks** → `Ft8Payload.TryRead` → unpack → **the same text**. Every one cleared every
parity check and every one came back as what went in. **This is the first time in this
phase that words have made the round trip through the encoder step 1 proved.**

### The Ft8Sharp test totals

| | total | passed | failed | skipped | wall clock |
|---|---|---|---|---|---|
| Task 1, before | 74 | 73 | 0 | 1 | 5.3 s cold |
| Task 5 | 104 | 103 | 0 | 1 | 2.6 s |
| Task 7, final | **108** | **107** | **0** | **1** | **2.3 s warm, 3.5 s with rebuild** |

**34 new tests.** The one skip is the table-rewrite write gate. **The project still returns
in well under a minute and no corpus was cut for the clock.** Clone-gated tests now number
**17** by `grep` — up from the 11 measured at task 1, the six new ones being the message
inventory and the message provenance.

### Attribution and the three channels

`git diff --name-only 2828ab6..HEAD` is **58 paths**, up from 44. **Not one is under
`src/Hamlet.App/`, `src/Hamlet.RadioEngine/`, `tests/Hamlet.App.Tests/` or
`tests/Hamlet.RadioEngine.Tests/`.**

| Channel | Verdict |
|---|---|
| `AudioSeamTests` + `PrivilegeTests` | **green, 55 of 55** — re-run after the version bump |
| `DecisionLogOrderTests`, `DecisionEmissionTests`, `VoiceTests` | **green, 10 of 10** |
| `VersionTests` | **green, 3 of 3 — measured after the bump, not before** |

**One shared artifact changed: the root `Directory.Build.props`.** That is the version
channel, which is already named in the plan's channel set and is already read by
`VersionTests`; no new channel was opened.

### What task 2's inventory found in the clone — names and shapes only

**Known item 1 confirmed on this session's own reading, not inherited.** `ft8/pack.c` and
`ft8/unpack.c` are both **absent**, and the inventory test asserts they stay absent.
Packing lives in `ft8/message.c` at 37 805 bytes and 1156 lines, which declares **7**
functions whose names mention packing. `ft8/message.h` is 8497 bytes and 161 lines,
`ft8/text.c` 6421 bytes and 304 lines, `ft8/text.h` 3041 bytes and 83 lines.

**16 candidate C sources were swept for a message-level known value**, in three shapes: a
message-shaped string literal, a braced byte array of packed-payload shape, and an array
named like a tone or symbol sequence. **10 sources carried at least one of those shapes.**
The only test source, `test/test.c`, holds 22 message-shaped literals, 2 packed arrays and
5 comparison calls, and its functions `test_std_msg`, `test_msg` and `main` are live code
that **encodes with upstream's encoder and decodes with upstream's decoder and compares the
text** — self-consistency, not a known value. The one true message-level vector is in the
same file's commented-out block and is against the superseded 72-bit packing.

A gated emitter keyed on `FT8_MESSAGE_SOURCE_DUMP=1`, with an optional
`FT8_MESSAGE_SOURCE_FILE` so one file can be read at a time, is how the port read what it
ported. **Not one line of upstream source, and no value of any kind, is in `output.md`, in
any commit message, in `porting-notes.md`, or in any committed file.**

### Task 6 — done, not dropped

Free text and telemetry were both built, both corpora were run, the type cover moved from
2 built and 13 refused to **4 built and 11 refused**, and the fuzz was re-run against the
wider cover with all three counts still zero.

### The two versions as they now stand

**`src/Ft8Sharp/Directory.Build.props`: 0.2.0 → 0.3.0**, with the reason written into both
the props file and `porting-notes.md` — the library can now produce and consume a *message*
rather than the envelope one travels in. **Root `Directory.Build.props`: 1.12.13 →
1.12.14.**

## 4. What's blocking us

**Nothing is blocking. Three items, all observations, none of them a ruling request, and
none of them standing in the way of any criterion named in B.** The pinned clone was
reachable throughout, so section 4 is not the blocker case the instruction describes.

**1. Five known items were confirmed and one measurement disagreed by one — reported, not
repaired.** `git status --short` prints **34** lines where the instruction states 33; the
extra entry appears to be the loop's own `SESSION.lock`, which is another author's and was
not touched or committed. Confirmed as stated: known item 4, `PHASE_STATUS.md` says
`STEP: 1 | done` with `CURRENT_STEP: 2` while `PHASE_OUTCOME.md`'s header says step 1
`partial` and step 2 `partial` — and `PHASE_STATUS.md` additionally says step 2 is `not
started`, which is a **third** disagreement with the arbiter's judgement that step 2 is in
progress; neither file was hand-edited. Known item 8, `TempEncoderProbe.cs` is still on
disk, untracked, 673 bytes; not touched. Known item 14's prediction held: **shell output
redirection into the tree was refused** when appending to `porting-notes.md`, and the file
tool was used instead rather than a workaround. **This is an observation. It asks for
nothing.**

**2. Four refusals were added where upstream returns text, and a fifth where upstream
returns nothing.** They are reproduced in full in section 1 and recorded in
`porting-notes.md`. Each was taken under `CLAUDE.md` §0.0 / HM-DEC-009, which the
instruction names as the governing rule of tonight's unpacker, and each affects only
patterns that no conforming transmitter produces. **The consequence Tim should know about
is that this library will be silent where `ft8_lib` would print something** — which is the
trade this project has already ruled on, but it is now real rather than theoretical.
**This is a decision already acted on, written here as one. It asks for nothing.**

**3. Correctness against upstream is not settled and cannot be settled before step 3.**
There is no usable message-level known value in the pin, so every measurement in this unit
is self-consistency. The provenance is honest but partial: **5 of 8 scalars corroborated by
machine, and one of those 5 only through a comment rather than a macro; the mapping from
type code to message type, the token sub-range boundaries and the basecall alphabet sizes
are not corroborated at all.** A systematically wrong alphabet or a swapped field would
survive everything measured tonight and would be caught for certain by step 3's
bit-identical symbol comparison. **This is a note for whoever writes step 3's unit: that
comparison is now load-bearing for the whole message layer, not just for the encoder. It
asks for nothing tonight.**
