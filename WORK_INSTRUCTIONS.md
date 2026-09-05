# Work instruction 246 - ordered statistics decoding, on the most reliable basis, when belief propagation gives up

```
STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project - nothing done."

If all four hold, say "Hamlet confirmed" and continue.
```

---

## THE TOOL RULE - read this before task 1

Carried forward from units 244 and 245 unchanged, because it is measured and it
still holds. **Two different faults behave oppositely and have been conflated
before.**

| | What it is | How it announces itself | The answer |
|---|---|---|---|
| **A - the allow-list** | a fixed set of permitted command prefixes, matched against the command **as typed** | `This command requires approval` | **there is usually a permitted spelling that works.** Find it before concluding anything |
| **B - the sandbox** | the shell may not create files or directories | `... was blocked. For security, Claude Code may only write to files in the allowed working directories for this session` | **there is no shell spelling that works.** Use `Write`/`Edit` |

`docs/shell-probe-243.md` has all eight probes verbatim. What runs: `dotnet build
<path>`, `dotnet test <path>`, `git` throughout, `ls`, `grep`, `sed`, `head`,
`tail`, `find`, `wc`, `date`. What never runs: `mkdir`, `cp`, `mv`, `git mv`,
`dotnet sln add`, `git clean`, any redirect write, and any compound line where one
part is refused. `dotnet --version` is refused and **is not evidence about the
toolchain.**

Three spellings unit 245 paid for, banked here so this unit does not pay again:
**`dotnet sln add` is refused** - edit `Hamlet.sln` with the file tools; **`git
clean` is refused** - a one-shot `.proj` deletes itself with an MSBuild `<Delete>`
task; and **`-p:EntryProps=` needs an ABSOLUTE path with FORWARD slashes**, e.g.
`C:/Source/HamLet/.run-unit/scratch/unit246-outcome.props`, because a relative
path resolves against `tools/arbiter/` and Git Bash eats backslashes.

**`tools\arbiter\*.bat` is a closed loop.** Go through `dotnet build
tools/arbiter/outcome-append.proj` and `dotnet build
tools/arbiter/validate-output.proj`. **No apostrophes in any `PHASE_OUTCOME.md`
field** - one breaks the tool's PowerShell parse.

**A refused shell call is a signal to reach for the other tool, not to stop.**
Nothing in this unit halts the loop.

---

## Why this unit exists

**This is unit 246. It is the fourth unit of this phase, and the first aimed at
step 2 - the step that holds the decibel.**

Steps 0 and 1 are closed. The ladder runs in the loop, the as-is baseline is
reproduced at **13 of 306 at a delivered -21.001 dB with 0 wrong**, a wrong decode
is counted separately from a missed one everywhere, the capture fixture format and
its four loud refusals and the shack command are committed, and `Ft8Sharp.Deep`
exists as a GPL-3.0 sibling that returns exactly what the port returns over 69
reference recordings, 801 messages, two ladder blocks and the committed capture.
**Four steps were gated on step 1 and on nothing else. All four are open, and this
unit takes the first of them.**

**Nothing in this tree has ever attempted step 2.** The loop test reads `NOT
FOUND` and no approach in `PHASE_OUTCOME.md` resembles this one: the three
recorded approaches are a shell probe and a ladder handle, a capture fixture
format and its reader, and a delegating sibling. **`units spent: 0` against step 2
is the truth here rather than a gap in the record.**

**Where the 1.5 dB is, and why it is this step.** `HM-OPEN-067` puts the 50 per
cent crossing near **-19.5 dB** against a published **-21**. Unit 222 took that
1.5 dB apart and could not find it in any one stage: oracle alignment, unquantised
magnitudes, physics-derived ratios and four times the iteration bound each landed
inside the as-is 95 per cent interval. Its finding is this phase's starting point
- **the information is not in the ratios.** At -21 dB the hard decisions carry
about **31 bit errors** against a code whose recovery reaches zero at **17**.
**Belief propagation gives up while the answer is still reachable, and `ft8_lib`
has no ordered statistics decoder at all.**

```
PHASE GOAL:   Hamlet reads FT8 as well as the best decoder there is, and then
              reads it further.
UNIT GOAL:    Ft8Sharp.Deep reaches codewords belief propagation refused, by
              re-ordering bits by reliability and searching low-weight patterns
              among the most reliable ones - and every codeword it recovers is
              accepted or refused by the port's own parity and CRC-14 gates,
              never by OSD's own say-so.
ADVANCES:     step 2. The decode rate at -21 dB on the 306-trial ladder that
              reads 4.2 per cent today; the order and search weight stated with
              the cost each buys, measured; the citation of Fossorier and Lin
              1995 at the point of use; and the worst-case time per slot with
              its margin against 15 seconds. Zero wrong decodes rides on every
              one of them.
```

**This unit is not required to reach 40 per cent.** `PHASE_PLAN.md` step 2 says
the step stays open while the number is still moving and closes *unachievable*,
with the figure reached and what was tried, when it is not. **A measured 12 per
cent with its cost and its wrong count is a result this phase can carry. A claimed
40 per cent with one wrong decode in it is not.**

---

## Verify this instruction against the tree

**Nothing here describes the tree.** Every line below was read from it while this
instruction was written. **Check each one and report mismatches in section 1; do
not repair them and do not repair this instruction.**

The algebra, all `public` on `Ft8Sharp`'s surface:

- `src/Ft8Sharp/Tables/Ft8Tables.g.cs`: `LdpcM` 83 at `:46`, `LdpcN` 174 at `:49`,
  `LdpcKBytes` 12 at `:52`, and `LdpcGenerator`, `LdpcNm`, `LdpcMn`,
  `LdpcNumRows` as `ReadOnlySpan<byte>` properties at `:76`, `:165`, `:254`,
  `:434`.
- `src/Ft8Sharp/Ldpc/LdpcEncoder.cs`: `Encode(ReadOnlySpan<byte> payload,
  Span<byte> codeword)` at `:73` and the two-argument generator overload at `:91`.
- `src/Ft8Sharp/Ldpc/LdpcDecoder.cs`: `Decode(ReadOnlySpan<float> ratios,
  Span<byte> codewordBits, int maxIterations = 25)` at `:136`. **Positive ratio
  means the bit is more likely 1.** `LdpcDecodeResult` carries `UnsatisfiedChecks`,
  `Iterations` and `ParitySatisfied`.
- `src/Ft8Sharp/Ldpc/Ft8CodewordDecoder.cs`: `Decode(ratios, cache,
  maxIterations)` at `:70`, with **GATE 1 parity at `:80` and GATE 2 the
  checksum at `:96`**, both commented as such in the file.
- `src/Ft8Sharp/Message/Ft8Payload.cs`: `MessageBits` 77 at `:57`, `PayloadBits`
  91 at `:66`, `Create` at `:98`, `TryRead` at `:165`.
- `src/Ft8Sharp/Dsp/Ft8SoftSymbols.cs`: `Extract` `:117`, `Normalise` `:287`,
  `Variance` `:323`, `HardDecision` `:351`.
- `src/Ft8Sharp/Dsp/Ft8SyncSearch.cs`: `DefaultMinimumScore` 10 at `:82`,
  `DefaultCandidateLimit` **140** at `:88`.
- `src/Ft8Sharp/Dsp/Ft8SlotDecoder.cs`: `DefaultMessageLimit` 50 at `:63`, the
  per-candidate loop at about `:161`, and its dedup key recovered by re-running
  `LdpcDecoder.Decode` at about `:194`.

The seam and the seat:

- `src/Ft8Sharp.Deep/Ft8DeepSlotDecoder.cs`: one public type, both `Decode`
  overloads delegating whole to `_port` at `:97` and `:105`, and `Port` exposed at
  `:74`.
- `tests/Ft8Sharp.Deep.Tests/Ft8DeepSlotDecoderTests.cs:156` asserts the sibling
  assembly holds **exactly one type**. **That is a tripwire unit 245 left for this
  unit on purpose. Changing it deliberately is this unit's job; discovering it
  afterwards is not.**
- `tests/Ft8Sharp.Tests/Dsp/Ft8LadderHarness.cs`: `Decoder` record at `:73`,
  `Run(rung, trials, seed, decoders?, frequencyHz?, offsetSamples?, log?)` at
  `:244` - **`decoders` is an optional list, so a three-way comparison is a local
  list and costs no other test a second** - and `Available()` at about `:190`
  returning two entries.
- `tests/Ft8Sharp.Tests/Encode/EncodeCorpus.cs:57`: `Entry(Label, Kind, byte[]
  Message, CarriesHashedCallsign, Text?)`, where `Message` is **the 77 bits that
  went on the wire**. `Ft8Step6Ladder.Population()` returns these.
- `docs/unit245-deep-seam.md` is the census. **Read §3, §4 and §5 before task 1**
  and do not re-measure what it already measured; say in the report where it was
  found right and where the tree has moved under it.

Versions and bookkeeping:

- Root version `1.12.48` at `Directory.Build.props:145`. `Ft8Sharp` **`0.10.7`**
  at `src/Ft8Sharp/Directory.Build.props:396`. `Ft8Sharp.Deep` `0.1.0` at
  `src/Ft8Sharp.Deep/Directory.Build.props:41`.
- The highest issue id in `OPEN_ISSUES.md` is `HM-OPEN-073`.
- `PHASE_STATUS.md` and `PHASE_OUTCOME.md` head their `STEP: 0` line **`partial`**
  while the last `## UNIT 2 - STEP 0` entry reads `done`. **Do not re-audit step 0
  and do not reconcile the header.** The `partial` verdict's whole stated reason
  was that two step-0 exits name `Ft8Sharp.Deep`, which did not then exist; it
  exists now and the harness scores it literally. Ruled below.
- `PROJECT_STATUS.md` `RULES_AT` reads `HM-DEC-153 (2026-09-04)` while `CLAUDE.md`
  §1 tops out at `CPS-DEC-0152`. **Report once if still present. Do not
  reconcile it** - `CLAUDE.md` is the owner's file.
- `.run-unit/*` and `PHASE_OUTCOME.md`, `PHASE_STATUS.md`, `RUN_LEDGER.md` were
  modified and uncommitted at the root when this was authored. **The three root
  files are the loop's own bookkeeping - commit them with your first task's commit
  and say you did.** `.run-unit/` is the launcher's; leave it.

**Expected to fail, and not this unit's:**
`CwAdjudicationTests.ASpeedChangeInRealisticAudio` and the 51 inherited CW reds in
`docs/unit239-failing-set.txt`. **None of those is in `Ft8Sharp.Tests`.** The one
expected skip is `Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`. If
`Ft8Sharp.Tests` shows red outside that set, or a second skip appears, that is a
finding and it goes in section 1.

---

## Rulings in force for this phase

Transcribed from `PHASE_PLAN.md`. **Not to be re-argued by this unit.**

**The seam is split.** `Ft8Sharp` stays a faithful MIT port of `ft8_lib`,
byte-identical in behaviour, and **nothing in this phase changes a line of it.**
Improvements live in `Ft8Sharp.Deep`. The port's value now is that it cannot
drift: every measurement in this phase is taken against something known-identical
to upstream, so a regression in the sibling is always visible.

**`Ft8Sharp.Deep` is GPL-3.0**, carrying its own `LICENSE` and a `NOTICE` citing
the published sources it implements. Both exist and the `NOTICE` already cites
Fossorier and Lin 1995 and the QEX paper, **before a line of either was written**,
which was step 1's fourth exit. Ruled by Tim, 2026-09-04. **No unit raises the
licence and no step is held by it.**

**No algorithm comes from WSJT-X's source or `ft4_ft8_public/`.** Published
description only - **Fossorier and Lin 1995** for ordered statistics, and the QEX
paper (Franke K9AN, Somerville G4WJS, Taylor K1JT, "The FT4 and FT8 Communication
Protocols," QEX, July/August 2020) - cited at the point of use. **WSJT-X is a
measuring instrument in this phase and never a source.** This is the second of the
three things the arbiter may not reason past.

**What Hamlet asserts to Tim.** §12.1 and §0.0. **A decode this phase produces
that nobody sent is worse than a decode it misses**, and step 2's zero-wrong
criterion is where that is enforced. If an approach produces a wrong decode, **that
approach is rejected and another is taken** - the step does not close and does not
stop.

**There is no WSJT-X on the development machine and no unit may assume one.** A
unit that cannot close without a real-air comparison says so; it does not
substitute `decode_ft8.exe`, which is `ft8_lib` and therefore the thing being
improved on. **Step 2's seventh exit, decodes-per-slot on real captures, is
`nice-to-pass` and needs a fixture Tim has not generated. It gates nothing
tonight.**

**Nothing is claimed without the scoreboard.** No unit in steps 1 to 6 may report
an improvement except as a number on step 0's instrument. A decode rate quoted
without it is not evidence.

**A wrong decode is counted separately from a missed one, everywhere, in every
report.**

**The steps are a hypothesis, not a contract.** `PHASE_PLAN.md` grants leave to
reorder, replace, retire and add steps and to split or re-scope criteria, with the
record in `PHASE_OUTCOME.md` as the only constraint. **A step whose number does not
move is evidence, not a halt.**

### Four things the arbiter decided, so this unit does not spend a night on them

1. **Step 2's entry criterion is satisfied and step 0 is not re-audited.**
   `PHASE_OUTCOME.md` and `PHASE_STATUS.md` still head step 0 `partial`; the last
   entry against it reads `done`, and that file's own rule is that **the entries
   win over the header**. The `partial` verdict's single stated reason was that two
   step-0 exits name `Ft8Sharp.Deep` as the thing scored, and unit 245 built it and
   put it in `Available()`, so the harness now scores it literally rather than
   through a re-scoping. **Proceed. Do not re-audit step 0, do not edit the
   headers, and do not report the disagreement a fourth time.**

2. **The citation goes in `src/Ft8Sharp.Deep/porting-notes.md`, a new file, not in
   the port's.** `PHASE_PLAN.md` step 2 says *cited at the point of use in
   `porting-notes.md`*, and the only `porting-notes.md` in this tree is
   `src/Ft8Sharp/porting-notes.md`, **inside the project this phase may not
   touch.** The sibling gets its own, in the port's file's shape, and the code
   carries the citation in XML remarks at the point of use as well. Licensed by the
   plan's leave to re-scope a criterion with the record as the constraint, and by
   the harder ruling that nothing in this phase changes a line of the port.

3. **The sibling reproduces the port's per-candidate loop through public members
   rather than wrapping it.** Unit 245's census measured every stage of
   `Ft8SlotDecoder.Decode(Ft8Waterfall)` public, and OSD has to sit **inside** that
   loop at the point where a candidate fails parity. There is nowhere else to put
   it. **This is route A of `docs/unit245-deep-seam.md` §4 and it was measured
   working.** Route C - making the port's factories public - is forbidden this
   phase and is not to be re-argued.

4. **The reproduction is guarded by an OSD-off identity test, and that test is not
   optional.** With OSD disabled the sibling must return **the whole
   `Ft8SlotResult`** - all five counts and every message in order - identical to
   `Ft8SlotDecoder`'s, on the ladder and on the committed capture. **That test is
   what keeps the instrument an instrument.** Without it, a difference between the
   two columns is no longer attributable to OSD, and the entire seam stops paying
   for itself.

---

## Status cadence

After each task, before starting the next, update `PROJECT_STATUS.md` per
`CLAUDE.md` - `STATE`, `TASK: n of m`, `BALL`, `UPDATED` from the clock, and
`NOTE` saying what is moving inside the task. The same every ten minutes while a
task is running. **Use the file-editing tools if the shell refuses.**

**A long measurement is a status update, not a silence.** Task 7 runs three rungs
over three columns; say so in `NOTE` before you start it.

---

## Tasks

Eight tasks. **Task 1 is a trace and comes first**, because this unit must measure
what OSD can possibly reach before it writes a line of OSD. **Task 8 is the named
drop candidate.**

**Start the `Ft8Sharp.Tests` baseline run early** - it was 5 m 14 s for unit 245
and it can run while task 1 is being written.

### Task 1 - the trace: measure the ceiling before building the ladder to it

**Reading and measuring only. No OSD is written in this task.** Report each with
file and line, and **say what you find, not what this instruction expects.**

1. **The generator, taken from the port's own encoder rather than from the
   table's packing.** The code is systematic in its first 91 bits -
   `Ft8CodewordDecoder` packs `codewordBits[..91]` as the payload. So the 91 by
   174 generator can be read off directly: **encode the 91 unit payloads (one bit
   set, all others clear) with `LdpcEncoder.Encode` and each returned codeword is
   one row of G.** Build it that way, then **verify it**: encode a few hundred
   random payloads with your G by GF(2) arithmetic and with `LdpcEncoder.Encode`,
   and assert every bit of every codeword agrees. **Report whether it did.** A
   table-packing mistake here would poison every number in this unit and would look
   like an algorithm that does not work.

2. **The population OSD gets to work on.** At **-21 dB, one whole block of 51
   trials**, seed and frequency and offset as `Ft8LadderHarness` defaults them,
   report per rung totals: candidates returned by the search, how many reached
   parity, how many passed the checksum, and how many became text. **This is what
   `Ft8SlotResult`'s five counts already carry** - use them rather than
   instrumenting anything.

3. **The ceiling, and this is the finding this unit owes the phase.** The ladder
   knows what it transmitted: `EncodeCorpus.Entry.Message` is the 77 bits, and
   `Ft8Payload.Create` then `LdpcEncoder.Encode` gives **the true 174-bit
   codeword**. For each of those 51 trials, over the candidates the search
   returned, measure and report:
   - **the smallest hard-decision Hamming distance** between any candidate's
     `Ft8SoftSymbols.HardDecision` bits and the true codeword - the distribution
     across the 51 trials, not just a mean;
   - **for that closest candidate, how many of its errors fall inside the 91 most
     reliable positions** by `|ratio|`. **This is the number that says which OSD
     order could possibly reach it**: order λ recovers a candidate only when its
     most-reliable-basis carries at most λ errors;
   - **how many trials have no candidate anywhere near the truth at all.** If the
     sync search never returned a place close to the signal, no amount of OSD helps
     and **the decibel is in step 4's baseband re-sync instead.** Say so plainly if
     that is what the numbers say - **that finding is worth more to this phase than
     a working order-1 search**, and the arbiter will re-order the plan on it.

4. **Where the sibling's loop reproduction has to be exact.** List, with line
   numbers, every public member `Ft8SlotDecoder.Decode(Ft8Waterfall)` calls and
   every piece of its bookkeeping that is not a call: the one `Ft8CallsignCache`
   per slot, the five counters and exactly which statuses increment them, the
   `MessageLimit` stop, and **the de-duplication key**. Note in particular that the
   port recovers its dedup key by **re-running `LdpcDecoder.Decode` over the same
   ratios** (about `:194`), which is a thing an OSD decode must not do - task 4.

### Task 2 - the reproduced loop, with OSD off, identical to the port

**Nothing new decodes in this task.** The sibling stops delegating and runs the
loop itself, through public members only.

- A public type in `Ft8Sharp.Deep` that reproduces
  `Ft8SlotDecoder.Decode(Ft8Waterfall)`: search, one cache for the slot, extract,
  normalise, `Ft8CodewordDecoder.Decode`, the five counts, the dedup key, the
  message limit. **`Ft8DeepSlotDecoder`'s existing overloads keep working** - name
  in the report whether you extended that type or added one beside it, and why.
- **An `Osd` setting, off by default for this task**, whose off state means *do
  exactly what the port does*.
- **The identity test of ruling 4**: OSD off, whole `Ft8SlotResult`, over one whole
  51-trial ladder block at -19 dB, one at -21 dB, and the committed capture
  `tests/fixtures/ft8/example/ft8-example-244.wav`. **Never `Texts` alone.** If the
  reproduction is not identical, **that is this task's finding and it is fixed
  before task 3 starts** - an OSD measured on top of a loop that already differs
  from the port measures nothing.
- **Change `Ft8DeepSlotDecoderTests.cs:156`'s one-type assertion deliberately**, to
  whatever the sibling now holds, and say in the report that you changed a
  tripwire unit 245 left rather than that a test broke.

### Task 3 - the OSD core, on its own, with tests that inject known errors

`src/Ft8Sharp.Deep/`, a new type. **From Fossorier and Lin 1995 and nothing else,
cited in `src/Ft8Sharp.Deep/porting-notes.md` and in XML remarks at the point of
use.** Not wired into any loop yet.

Given 174 ratios in the port's convention - **positive means more likely 1** - and
an order λ:

1. **Order the positions by reliability**, `|ratio|` descending.
2. **Find the most reliable basis.** Gaussian elimination over GF(2) on G with its
   columns visited in that order, taking the first 91 that are independent. Produce
   a generator systematic on those 91 positions.
3. **Order 0**: hard-decide the 91 basis bits from the ratio signs and re-encode.
4. **Order λ**: flip every subset of the basis of size 1 to λ, re-encode each.
5. **Rank by soft distance** - the sum of `|ratio|` over the positions where the
   re-encoded codeword disagrees with the hard decision. **Lowest wins.** Keep the
   best, and report how many re-encodings that took.

Tests, in `tests/Ft8Sharp.Deep.Tests`, on synthesized ratios rather than on audio:

- a clean codeword recovers at order 0;
- **errors planted at known positions**: λ errors inside the basis recover at order
  λ and λ+1 errors inside it do not. **That is the algorithm's contract and it is
  the one thing a unit test can pin.**
- the elimination returns 91 independent columns for every input tried, including
  ratios that are all equal and ratios that are all zero;
- **it never throws** on degenerate input, because it will be called 140 times a
  slot on noise.

### Task 4 - the gate: the port accepts or refuses, never OSD

**This is §0.0 and it is the criterion this step cannot trade.**

- **A codeword OSD recovers is submitted to `Ft8CodewordDecoder.Decode` as
  saturated ratios** - the route unit 245 measured, converging in one iteration -
  and **the port's parity gate and CRC-14 gate are the only acceptance.** Nothing
  in `Ft8Sharp.Deep` decides that a message is real.
- **Submit the single best codeword per candidate, and no more.** If you submit
  more than one, **state the number and state the arithmetic**: every codeword put
  to the CRC-14 is an independent chance of a false accept at about one in 16,384,
  and 140 candidates times a search of thousands would put tens of wrong decodes a
  slot in front of Tim. **This is the specific way this step fails, and it fails
  quietly.**
- **The dedup key for an OSD decode is the codeword OSD already has** - its first
  77 bits - **not a re-run of `LdpcDecoder.Decode` over the original ratios**,
  which is what the port does at about `:194` and which would fail for exactly the
  candidates OSD rescued, returning the same message twice.
- A test that hands the gate a **deliberately wrong** OSD codeword and watches the
  port refuse it, in the port's own words, quoted in the report.

### Task 5 - OSD in the loop, where belief propagation gave up

- Run OSD **only** where the port's per-candidate result is
  `Ft8CodewordStatus.ParityNeverSatisfied`. Where belief propagation converged, the
  port's answer stands unchanged.
- **State the stopping rule**: what makes the stage stop trying a candidate, and
  what makes it stop trying a slot.
- Count OSD's own outcomes separately - candidates offered to OSD, codewords it
  produced, codewords the port then accepted - and **print them beside the five the
  port already returns.** A rate that moved with no visible OSD activity behind it
  is not evidence.

### Task 6 - order and search weight, with the cost each buys

**Step 2's fourth must-pass exit, and it says measured, not tuned to a target.**

At **-21 dB over one whole 51-trial block**, with the same seed and noise draw for
every row, report a table of **order 0, order 1 and order 2**: decoded, missed,
**wrong**, and milliseconds per trial for each. **Report what each order bought and
what it cost, including the orders that bought nothing.** Then say which order the
default is set to and why - **from this table, not from a paper and not from a
target.**

### Task 7 - the scoreboard, whole, and the time budget

**Nothing is claimed without this.**

- **The whole ladder** - **-19, -20 and -21 dB, 306 trials each** - through
  `Ft8LadderHarness.Run` with a **local three-entry decoder list**: `Ft8Sharp`,
  `Ft8Sharp.Deep OSD off`, `Ft8Sharp.Deep OSD on`. `Run` takes `decoders` as an
  optional parameter, so this costs no other test in the suite anything. **Three
  counts on every row, never two.** Quote the table whole.
- **Zero wrong decodes across all three rungs and all three columns**, or the
  approach is rejected and the report says which rung produced it, with the message
  sent beside the message returned - `WrongReturn` already prints that line.
- **Worst-case time per slot with the margin stated against 15 seconds.** Take the
  worst single slot observed, not the mean, and say the candidate count it carried.
  The port sits at about 64 ms a slot and the search returns up to 140 candidates.
- **The one number the phase reads: the decode rate at -21 dB over 306 trials,
  against 4.2 per cent (13 of 306) as-is.** Quote it with its Wilson interval and
  its wrong count. **If it did not move, say so in the same sentence you say what
  it cost.**

### Task 8 - the write-up and the record. THIS IS THE DROP CANDIDATE

**If the night runs short, this is what is shed, and the report says it was.**

- `docs/unit246-osd.md`: task 1.3's ceiling distribution and task 6's order table
  written up, so the next unit on step 2 does not re-measure them.
- If task 1.3 found that the errors sit outside the most reliable basis, or that
  the sync search never returned a candidate near the signal, **open an
  `OPEN_ISSUES.md` entry at the next free id** (`HM-OPEN-074` unless something took
  it) naming what it means for step 4. **If the numbers said nothing of the kind,
  open nothing and say why** - an empty issue is worse than none.

**Dropping this costs the phase two documents, not a criterion. Tasks 3 to 7 are
step 2's must-pass exits, and task 1.3's numbers still go in section 3 even if this
task is dropped** - what is shed is the write-up, not the measurement.

### Both suites, every unit

`PHASE_PLAN.md`: `dotnet test tests/Ft8Sharp.Tests` and `dotnet test
tests/Ft8Sharp.Deep.Tests`, **whole, one project at a time and never
concurrently.** Baseline before your first code change and totals after. Unit 245
left them at **582 passed / 0 failed / 1 skipped / 5 m 14 s** and **18 passed / 0
failed / 0 skipped / 1.3 s**; a different baseline is itself a finding. **Do not
run `Hamlet.App.Tests` or `Hamlet.RadioEngine.Tests`** - nothing here touches
either.

---

## Parked - do not touch, do not raise

- **Subtraction, baseband re-sync, per-message SNR, cross-slot combining.** Steps
  3, 4, 5 and 6. Not one line tonight. If task 1.3 says the decibel is in step 4,
  **that is a measurement to report, not a step to start.**
- **`Ft8Sharp.Deep`'s licence.** Ruled GPL-3.0. Do not raise it.
- **Step 0's `partial` header** and `PHASE_PLAN.md:53`'s stale step-0 wording.
  Decided three times already. Do not report either.
- **The `RULES_AT` mismatch.** Report once under "verify against the tree"; go no
  further. `CLAUDE.md` is the owner's file.
- **The shell permission fault and `allowed.txt`.** Banked and not blocking; the
  working spellings are at the top of this file. Do not probe it.
- **`HM-OPEN-071`'s missing per-message SNR**, owed by step 5. **`HM-OPEN-073`, the
  real capture fixture**, Tim's and deferred. Both gate nothing here.
- **`tests/fixtures/ft8/captured/` holding a `README.md`.** Reported by 245.
  Harmless.
- **The CW decoder**, the 419 dropped chunks, the 51 inherited failing cases, the
  engine project's missing total, the waterfall's late first row.

---

## What not to do

- **Do not touch `src/Ft8Sharp/`** - not a line of code and not
  `porting-notes.md`. The port is the instrument. **If `Ft8Sharp`'s version moves
  off `0.10.7`, something changed and that is a finding, not a bump.**
- **Do not read WSJT-X source or `ft4_ft8_public/`**, and do not go looking for
  anyone's OSD implementation. Fossorier and Lin 1995 and the QEX paper, cited at
  the point of use. The second of the three things the arbiter may not reason past.
- **Do not let `Ft8Sharp.Deep` decide a message is real.** The port's two gates,
  always. A checksum re-implemented in the sibling is the worst line this unit
  could write.
- **Do not trade a wrong decode for a rate.** Whatever the rate. If an approach
  produces one, reject that approach and take another, and report both.
- **Do not tune the order to hit 40 per cent.** Exit 4 says the cost each order
  buys, measured. A number reached by trying settings until one passed is not a
  measurement, and this phase would carry it forward as though it were.
- **Do not claim an improvement that is not on the scoreboard.** No decode rate
  without its trial count, its Wilson interval and its wrong count.
- **Do not report a rate that moved without saying what it cost in milliseconds.**
- **Do not skip the OSD-off identity test to save time.** It is ruling 4 and it is
  the whole value of the seam.
- **Do not stop because the shell refused something.** Record it, switch tools,
  continue.
- **Do not stop because the rate fell short.** The step stays open while the
  number moves; falling short with a figure is a result this phase carries.

---

## Committing and pushing

Commit and push each task before starting the next, on `main`, which is trunk.
**Commit `PHASE_OUTCOME.md`, `PHASE_STATUS.md` and `RUN_LEDGER.md` with your first
commit.** Root version `1.12.48` to **`1.12.49`** if anything was committed;
`Ft8Sharp.Deep` `0.1.0` to **`0.2.0`**, because it grew an algorithm.
**`Ft8Sharp` stays `0.10.7`.** If nothing could be committed, do not bump and say
why.

Append this unit's entry to `PHASE_OUTCOME.md` through `dotnet build
tools/arbiter/outcome-append.proj`, with `-p:EntryProps=` an **absolute path with
forward slashes**. **Use the tool rather than writing the entry by hand** - it
updates the header's step state in the same call. **No apostrophes in any field.**
If it refuses, write the entry in exactly the format the existing entries use,
update the `STEP: 2` header line yourself, and say in the report that you did.

Validate `output.md` through `dotnet build tools/arbiter/validate-output.proj`
before you finish, and report the rule count and the exit code.

---

## Reporting

`output.md` at the repository root, overwritten, four sections per
`CLAUDE_CODE.md` §8.

**First, above everything, the ordering block. `validate-output.bat` refuses a
report without it.** Three parts, every line specific to this unit:

- **A - THE PHASE GOAL**, and the state of all seven steps as this unit leaves
  them. The phase is the 1.5 dB between -19.5 and -21, and **step 2 is the step
  that holds it**. Say where steps 0 and 1 stand, that steps 3, 4, 5 and 6 are open
  and untouched, and **what this unit did to the number** - the -21 dB rate against
  4.2 per cent, with its wrong count in the same line.
- **B - THIS STEP AND ITS EXIT CRITERIA.** Step 2's six must-pass exits and its one
  nice-to-pass, **one by one, with met or not met against each**: 40 per cent at
  -21 dB on 306 trials; the step stays open while the number moves; zero wrong
  decodes across the whole ladder; order and search weight with the cost each buys;
  implementation from Fossorier and Lin with no WSJT-X source read; worst-case time
  inside 15 seconds with margin; and decodes-per-slot on real captures, which needs
  a fixture nobody has and is nice-to-pass. **If an exit is not met, say which and
  what is needed - not a summary of effort.** **If the rate fell short, say the
  figure reached and what was tried**, which is what the second exit asks for by
  name.
- **C - THIS REPORT**, weighed against A and B: what it found that bears on the
  goal and the criteria - **task 1.3's ceiling distribution is the thing here**,
  because it says whether the remaining decibel is reachable by any OSD order at
  all or is sitting in step 4's synchronisation - **how many items section 4
  raises**, and **whether any of them stands in the way of an exit criterion in B.**
  An item that asks for no ruling is logged there as logged.

Then the six-line header: `UNIT`, `PHASE GOAL`, `UNIT GOAL`, `ADVANCED`, `NUMBER`,
`DRIFT`. **`NUMBER` for this unit is the decode rate at -21 dB over 306 trials with
its wrong count** - `4.2 per cent (13 of 306), 0 wrong` going in, whatever it is
coming out - plus both suites' totals. **A rate without its wrong count is not a
number this project prints.**

**Section 3 leads with four things, in this order:**

1. **The three-column ladder table, whole** - `Ft8Sharp`, `Ft8Sharp.Deep OSD off`,
   `Ft8Sharp.Deep OSD on`, at -19, -20 and -21 dB over 306 trials, three counts
   each. **The OSD-off column equalling the port is what makes the third column
   mean anything**; say so in one sentence.
2. **The ceiling** from task 1.3 - the distribution of the closest candidate's
   hard-decision error count, how many of those errors fell inside the 91 most
   reliable positions, and how many trials had no candidate near the signal at all.
   **State plainly what order of OSD that distribution admits.**
3. **The order table** from task 6 - order 0, 1 and 2, what each bought, what each
   cost in milliseconds, and the worst-case slot time with its margin against 15
   seconds.
4. **Both suites' totals**, and whether any red is outside the expected set.

**Section 4 says, in one line, whether step 2 is closed, and if it is not, whether
the number is still moving** - which is what decides whether the next unit takes
another approach at step 2 or the arbiter moves to another step.

Write `output.md`, then stop. Do not start the next unit.

---

```
ARBITER-DECISION
STEP: 2
APPROACH: ordered statistics decoding on the most reliable basis in Ft8Sharp.Deep when belief propagation fails parity, with the sibling reproducing the port per-candidate loop through public members and every recovered codeword gated back through Ft8CodewordDecoder
MOVE: continue
WHY: steps 0 and 1 are closed and step 2 is the step the phase 1.5 dB sits in - unit 222 measured 31 hard-decision errors against a code that recovers to zero at 17 and proved the extra information is not in the ratios, so the loss is belief propagation giving up while the answer is still reachable. The loop test returns NOT FOUND and no approach on record resembles this one.
STATE: not started
DECIDED: four. First, that step 2 entry is satisfied and step 0 is not re-audited: PHASE_OUTCOME.md heads step 0 partial while its last entry reads done, that file rule is that the entries win, and the partial verdict whole stated reason was that two step-0 exits name Ft8Sharp.Deep, which unit 245 built and put in the harness Available seat, so the scoring is now literal. Second, that the Fossorier and Lin citation goes in a new src/Ft8Sharp.Deep/porting-notes.md rather than the port own file, because the only porting-notes.md in the tree sits inside the project this phase may not touch - a re-scoping under the plan leave to split criteria with the record as the constraint. Third, that the sibling reproduces the port per-candidate loop through public members rather than delegating, because OSD has to sit inside that loop where a candidate fails parity and there is nowhere else to put it; this is route A of the unit 245 census and it was measured working. Fourth, that the reproduction is guarded by a mandatory OSD-off whole-result identity test against the port, because without it a difference between the two scoreboard columns is no longer attributable to OSD and the seam stops paying for itself.
LICENCE: PHASE_PLAN.md step 2 and its six must-pass exits - 40 per cent at -21 dB on the 306-trial ladder, the step staying open while the number moves and closing unachievable with a figure when it does not, zero wrong decodes across the whole ladder, order and search weight stated with the cost each buys measured rather than tuned, implementation from Fossorier and Lin 1995 and the QEX paper cited at the point of use with no WSJT-X source read, and worst-case time per slot inside 15 seconds with margin stated. The phase ruling that improvements live in Ft8Sharp.Deep and nothing changes a line of the port. The three things the arbiter may not reason past, of which the second is the licensing boundary and the third is what Hamlet asserts to Tim. The plan section that the steps are a hypothesis and not a contract, for the porting-notes re-scoping.
ACCOMPLISHED: Hamlet will reach codewords the best decoder in the world cannot, from the same ratios it already has - or it will have measured, on its own instrument and against the message it knows it transmitted, exactly why not and where the missing decibel is instead. Either way the phase stops guessing: the ceiling measurement says whether the remaining 1.5 dB is reachable by any amount of code searching or is sitting in the synchronisation step 4 has not taken yet, and the answer is a distribution over 51 trials rather than an opinion. The port stays the instrument, the sibling carries the divergence, and every codeword either of them returns has passed the port own parity and CRC-14 gates, so nothing this unit can do puts a message in front of Tim that nobody sent.
ADVANCES: step 2, and the criteria it moves are the decode rate at -21 dB on the 306-trial ladder that reads 4.2 per cent today, the order and search weight stated with the cost each buys, the citation of Fossorier and Lin 1995 at the point of use, and the worst-case time per slot with its margin against 15 seconds. Zero wrong decodes is carried on every one of them. Step 2 opens no other step - steps 3, 4, 5 and 6 were already open behind step 1 - so what this unit adds beyond its own criteria is the ceiling measurement that tells the arbiter whether step 4 should be taken next.
END-ARBITER-DECISION
```
