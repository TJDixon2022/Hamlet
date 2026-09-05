# PHASE_PLAN.md

**Governed by `PHASE_CONTROL.md`. Approved by Tim, 2026-08-31.**

---

## The phase

**Hamlet hears FT8 off the radio and displays the decoded text on screen.**

## The description

Hamlet controls an IC-7300 and decodes CW. It cannot decode FT8 at all  -  there is
no decoder in the tree and no managed one worth using anywhere else. This phase
builds one, as a port of Karlis Goba's `ft8_lib`, and wires it to Hamlet's audio
and display.

**The phase ends at Tim's eyes, not at a test.** Every step is closed by its own
assertions passing and the whole suite green. The phase is closed by Tim sitting
at the radio, seeing FT8 text on Hamlet's screen, and saying it passed. No script
can evaluate that, because no script can hear his antenna.

**A session reading this cold should understand:** the work is a port, not an
invention; the reference implementation is on this machine and pinned; the
decoder is deterministic and needs no radio for six of the seven steps; and the
seventh ends by handing control back to Tim.

---

## The steps

```
STEP: 1 | the library exists and its tables are proven
STEP: 2 | messages round-trip through 77 bits
STEP: 3 | a valid FT8 signal can be produced
STEP: 4 | signals are found in noise
STEP: 5 | a found signal becomes a message
STEP: 6 | sensitivity meets the published threshold
STEP: 7 | Hamlet displays decoded FT8
```

**A step is an outcome, not a work instruction.** The arbiter authors as many
units against a step as it takes  -  one, or ten. Sizing is the arbiter's judgment
per unit, and **small units are the default**: verification is cheap, rollback is
cheaper, and a large unit can carry work that turns out to be mutually exclusive.

This changes what `CLAUDE_CODE.md` section 4.6's drop candidate is for. When another
unit can be authored against the same step, the answer to *this is too big* is a
second unit rather than a sacrifice. A drop candidate is expected only inside a
genuinely large single task.

### When a step is done - ruled 2026-09-01

**A step is done when its must-pass criteria are met. An unmet nice-to-pass
criterion does not hold a step open.**

This is what the two tiers were always for, and the plan did not say it plainly
enough. Unit 212 met all four must-pass criteria of step 3 with quoted
measurements - 51 of 51 symbol sequences identical to upstream, the synthesized
audio agreeing with upstream's WAV to one count maximum - and the step was judged
partial because criterion 3, marked nice-to-pass, could not run: the reference
decoder does not exist on this machine and a unit cannot build it.

**Every unmet nice-to-pass criterion is recorded in `OPEN_ISSUES.md` by name**,
with what it would have shown and what it would take to run it. Recorded is not
dropped. Steps 5 and 6 carry nice-to-pass criteria that may need the same missing
pieces, and a gap nobody wrote down is a gap that gets rediscovered.

**Currently carried:** the reference decoder `decode_ft8.exe` is not built on this
machine. It would confirm that a signal this library synthesizes is decodable by
upstream's own decoder, rather than merely identical to upstream's own encoder
output. Step 5 will want it.

### What a unit runs, and it is deliberately small

**Ruled 2026-09-01, after four units were eaten by the test suite.**

The tree declares **2718 tests**: Hamlet.RadioEngine.Tests 2157,
Hamlet.App.Tests 523, Ft8Sharp.Tests 38. The RadioEngine set runs real signal
processing over recorded audio and takes well over half an hour. **Running it on
every unit is waste, and it has already cost this phase four units** - one killed
by the watchdog mid-run, one blinded by contention, and two that reported a
criterion unmet because a truncated run looked like a finished one.

**A unit runs the smallest set that answers the question, and no more.**

**1. Ft8Sharp's own tests, every unit.** They are the work. 38 today, seconds to
run, and every one must pass.

**2. Attribution, not execution, for Hamlet.** The question is not *is Hamlet
green today* - it is *could this phase have reached Hamlet's code at all*. Answer
it with the diff from the phase boundary commit `2828ab6`:

```
git diff --name-only 2828ab6..HEAD
```

**No path under `src/Hamlet.*` or `tests/Hamlet.*` may appear.** If one does, the
unit says so and the reduction below does not apply. This is a stronger
instrument than a test run: a green suite proves nothing broke today, attribution
proves nothing *could* have.

**3. The channel tests.** Attribution alone misses an indirect break through a
shared artifact. Unit 205 identified the three channels this phase can reach
Hamlet through - rows added to `CLAUDE.md` section 1, the root version, and
`Hamlet.sln` membership - and named the tests that read them, `DecisionLogOrderTests`
above all, since it parses the very table this phase writes rulings into. **That
set runs by filter in about a minute and runs every unit.**

**If a unit adds a new shared artifact, it adds the channel and says so.** A
channel list nobody maintains is worse than none.

### The full suite is Tim's, once, at the end

**No unit runs the whole suite. No step requires it.** It is run by hand, by Tim,
uncontended, once, before he looks at the screen and closes the phase.

**Two measurement traps, both paid for.** No project in this tree prints a
summary line, so a truncated run is indistinguishable from a clean one by eye -
a run counted at 1049 on 2026-08-31 was two projects stopping partway and was
recorded in this plan as a baseline until `--list-tests` showed 2718 declared.
**Use a TRX logger and read `ResultSummary.Counters`**, never a console count.
And the console logs are UTF-16, so anything grepping them as UTF-8 reports zero.

**There are real inherited reds.** Measured 2026-09-01 in
`Hamlet.RadioEngine.Tests.Cw`: `WhereTheTrackerStartsDoesNotDecideThis` wanted
500 Hz and got 501, `AStationElsewhereIsStillFound` failed on a CW callsign.
Pre-existing, nothing to do with FT8, and **not this phase's to fix.**

**HM-DEC-151's *18 of 38 red* is withdrawn** - a `git grep` finds that phrase
only inside the ruling's own text and nothing records what the 38 counted. **The
84 named reds are withdrawn** - taken under concurrent runs. Neither was ever a
measurement.

### What *whole suite green* means here

**Ruled 2026-08-31.** The tree carries an inherited CW ratchet at 18 of 38 red.
This phase did not create it and does not own it. Read strictly, *whole Hamlet
suite green* would mean **no step in this phase could ever close**, however good
the FT8 work  -  which was a defect in this plan's first draft, not a real gate.

**The criterion is: no new red, and the inherited failing set unchanged.** The
report **names the failing set and counts it**, not the count alone  -  a count
alone lets a swap hide, where one inherited red goes green and one green goes
red and the total matches.

This cannot launder a regression and it lets the phase close on its own merits.
**It is not a licence to leave the CW reds alone forever**  -  it says only that
fixing them is not this phase's work.

### The library carries its own version

**Ruled 2026-08-31, amending HM-DEC-063 for `Ft8Sharp` alone.**

`Directory.Build.props` injects `AssemblyVersion`, `AssemblyInformationalVersion`
and `BuildStampUtc` into every project, so `Ft8Sharp.dll` was compiling as
version 1.12.8 of Hamlet, carrying Hamlet's git commit. **An extracted
`Ft8Sharp` would publish itself as a version of a program it has never heard
of**, and the boundary test cannot see it  -  that test walks assembly references
and an injected attribute is not one.

**`src/Ft8Sharp/` gets its own `Directory.Build.props` that does not inherit**,
with its own version. Recorded in `porting-notes.md` so it reads as deliberate.

HM-DEC-063 exists so the tree has one answer to *what version is this app*.
`Ft8Sharp` is not the app  -  it is a separate work product with its own licence,
its own boundary and an intended life outside this repository. **A second version
number there is the same reasoning that gave it its own `LICENSE`, not drift.**

**Both rulings still want recording in `CLAUDE.md` section 1** as HM-DEC-151 and
HM-DEC-152, with `RULES_AT` advanced to match. Mechanical, and a task for the
next unit.

### How a unit runs its tests

**Ruled 2026-08-31, after a healthy session was killed by the watchdog.**

Hamlet's full suite is 2682 tests and takes over 25 minutes. The stall threshold
is 12. On 2026-08-31 a session building step 1 sat honestly reporting *baseline
suite executing 24 min* and was killed 12 minutes after its last status write,
with the project, licence, notes and boundary test already built and unreported.

**A unit runs only the `Ft8Sharp` tests.** `Bash(dotnet test:*)` already permits a
filtered invocation. The inner loop is seconds, and the threshold stays tight
enough to mean something.

**The whole Hamlet suite runs before a step is called complete**, not on every
unit. It is a real asset and is not demoted  -  it is moved to where it gates
something. A unit working inside `Ft8Sharp` does not need 2682 CW tests to know
whether it broke itself.

**`run-phase.bat` is invoked with `--minutes 25`**, loose enough for the one long
run at a step boundary and still short enough to catch a stall.

### Dependencies, and a named deviation

**Every step depends on the one before it. There is no independent step.**

`PHASE_CONTROL.md` section 2 requires at least one step depending on nothing, so the
arbiter has somewhere to go when an early step blocks. **This plan does not
satisfy that, and the deviation is deliberate.** The rule is for a phase carrying
independent tracks  -  CW steps beside FT8 steps, where a stall in one leaves the
other reachable. This phase is a single pipeline: without a working library there
is no candidate search, and without a decoder there is nothing to display.
Manufacturing an independent step here would mean splitting the audio plumbing
out of step 7 to satisfy a counter, which is contrivance rather than
independence.

Per section 0, the canonical file wins and the disagreement is reported rather than
forked. **Tim is amending the rule at source to read as advisory.** Until it
does, this plan names the conflict here so the arbiter sees the reasoning rather
than finding a plan that quietly violates its standard.

**The consequence, stated so the arbiter is not surprised by it:** when a step
blocks, there is nowhere to route. Work the blockage or halt.

---

## Branching

**`PHASE_CONTROL.md` section 3 governs.** Its three moves  -  work around, cut down,
declare unachievable  -  and its rule that repetition rather than a count ends a
step, are the whole of the branching for this phase. Two things Tim added:

**The arbiter reasons from all three goals together**  -  the phase goal, the step
goal, and the unit goal  -  and decides the best path. It may add units to a step,
rewrite a step, insert a step, or delete one. **The step list above is the
current best plan, not a contract.**

**Halting is valid and must be earned.** A phase that ends because nothing more
can move is as real an ending as one that ends satisfied. But it is never the
first answer, and **a halt whose report does not say what was tried is a halt
that was not earned.** This is section 3's *declaring victory too early is the likelier
failure*, stated as Tim wants it weighted.

### The three things the arbiter may not reason past

These are facts about the destination, not fences. Reaching the goal by crossing
one of them is not reaching the goal.

1. **Transmit.** `CLAUDE.md` section 0.2 is untouched. Nothing in this phase keys the
   radio. The encoder built in step 3 produces audio as a test oracle and is
   never routed to a transmitter.
2. **The licensing boundary.** See the rulings below. No route to a table or an
   algorithm goes through `ft4_ft8_public/` or WSJT-X, however much easier it
   would be.
3. **What Hamlet asserts to Tim.** `CLAUDE.md` section 12.1 puts anything touching what
   the display claims with Tim. The arbiter may not rule on it.

---

## Rulings in force for this phase

Taken with Tim on 2026-08-29 and 2026-08-31. **Not to be re-argued by any unit.**

**One repository.** `Ft8Sharp` is a project inside Hamlet at `src/Ft8Sharp/`,
carrying **its own MIT `LICENSE`** naming Tim, and a `NOTICE` crediting Goba,
citing the QEX paper (Franke K9AN, Somerville G4WJS, Taylor K1JT, "The FT4 and
FT8 Communication Protocols," QEX, July/August 2020), and stating that this is an
independent port.

**The boundary is mechanical.** A test asserts `Ft8Sharp` references nothing
outside itself. This is what makes *built as if it will be published* true rather
than aspirational; extraction stays cheap only while it holds.

**Tables come from `ft8/constants.c`, machine-converted by a checked-in tool.**
Not transcribed, and **not from `ft4_ft8_public/`**  -  that folder is Fortran of
uncertain provenance inside an MIT repo whose LICENSE names Goba's copyright, and
routing the most licence-sensitive artifact in the project through it is the
wrong risk for a library intended for publication. **Do not read, port, or
reference it.**

**Inheriting Goba's bugs is accepted**, recorded in `porting-notes.md`. A wrong
table bit cannot hide  -  parity fails immediately. An algorithmic weakness can,
and **step 6 is what would reveal it**, measured against the published threshold
rather than against `ft8_lib`.

**Reference WAVs are never committed.** ~21 MB of someone else's off-air
recordings do not enter a repository headed for publication. Tests read them from
`C:\Source\ft8_lib` and **report skipped when absent**, so a fresh clone stays
green.

**Upstream is pinned at `9fec6ca39886edbf96f4f5e71edc76da5074e871`**, cloned at
`C:\Source\ft8_lib`, outside the tree and never committed. Recorded in
`porting-notes.md` as the provenance of everything ported.

**Unit numbering starts at 200** under the phase layer. A Claude-side labelling
convention; Hamlet's versioning is untouched, patch per work unit, minor when
this phase closes.

---

## Step 1  -  the library exists and its tables are proven

**Delivers:** an `Ft8Sharp` project that compiles, is licensed, cannot reach into
Hamlet, and carries the FT8 protocol tables  -  **proven correct, not assumed.**

**Entry:** `src/Ft8Sharp/` does not exist. `C:\Source\ft8_lib` is present at the
pinned commit.

**Exit:**
- The project builds; .NET 8, nullable enabled, warnings as errors, no
  third-party runtime dependencies. *must-pass*
- `LICENSE`, `NOTICE`, `porting-notes.md` present and correct. *must-pass*
- The boundary test passes and **has been shown to fail** when a Hamlet reference
  is added. A guard that has never refused is not a guard. *must-pass*
- Tables converted by a checked-in tool that reads `ft8/constants.c`, reproducible
  against a future upstream. *must-pass*
- **Tables verified by LDPC encode against reference parity**, without a decoder.
  *must-pass*
- `Ft8Sharp` tests green. *must-pass, every unit*
- Attribution clean from `2828ab6`, and the channel tests green.
  *must-pass, every unit* - the full suite is not run by any unit

**Note.** `ft8sharp-spec.md` section 10 leaves tables unverified until belief
propagation runs several stages later  -  four stages of work on an unchecked
parity matrix, and a wrong bit there produces a decoder that fails in ways nearly
impossible to diagnose. Verification is pulled forward deliberately. **If the
tables cannot be verified this way, the step is not complete**, and the arbiter
reasons about what would verify them rather than proceeding.

**Depends on:** nothing built. This is the first step.

## Step 2  -  messages round-trip through 77 bits

**Delivers:** the FT8 message layer. CRC, packing, unpacking, callsign hashing.
**No signal processing anywhere.**

**Entry:** step 1 complete  -  verified afresh, not inherited from its report.

**Exit:**
- CRC matches known values. *must-pass*
- Standard, free-text, telemetry and non-standard-callsign messages round-trip
  across a **large generated corpus** of callsign and grid combinations, not a
  handful of examples. *must-pass*
- Any random 77-bit pattern either decodes or fails cleanly and never throws.
  *must-pass*
- Contest and DXpedition types (`EU_VHF`, `ARRL_FD`, `ARRL_RTTY`, `WWROF`,
  `DXPEDITION`, `CONTESTING`) round-trip. *nice-to-pass*  -  rare outside contest
  weekends, and the phase reaches its goal without them. **An unsupported type
  must fail as unsupported and never as a wrong decode**, and that assertion is
  *must-pass* whether or not the types are built.
- `Ft8Sharp` tests green. *must-pass, every unit*
- Attribution clean from `2828ab6`, and the channel tests green.
  *must-pass, every unit* - the full suite is not run by any unit

**Why the boundary matters:** with the bit layer proven and no DSP in the tree,
every later failure is unambiguously in the signal processing.

**Callsign hashing is the subtle part.** Non-standard calls pack against 22, 12
and 10-bit hashes resolved from a rolling cache. **Port it faithfully**  -  a C#
design that reads better but hashes differently produces callsigns that are
silently wrong.

**Depends on:** step 1.

## Step 3  -  a valid FT8 signal can be produced

**Delivers:** LDPC encode and the symbol sequence, and audio synthesis from it.

**Entry:** step 2 complete, verified.

**Exit:**
- LDPC parity matches the reference for known payloads. *must-pass*
- **The symbol sequence is bit-identical to `ft8_lib`'s** for the same message.
  *must-pass*
- Audio synthesis produces a signal the reference decoder decodes. *nice-to-pass*
   -  strong evidence, but it needs `ft8_lib` built on this machine, which may not
  be available.
- `Ft8Sharp` tests green. *must-pass, every unit*
- Attribution clean from `2828ab6`, and the channel tests green.
  *must-pass, every unit* - the full suite is not run by any unit

**This step is the hinge of the phase.** Once encode exists, every later step
generates its own fixtures at any SNR, with no radio, no recordings, and no
licensing question. Step 6 is impossible without it.

**Transmit boundary:** the encoder is a test oracle. It produces audio. **Nothing
routes it to a transmitter**, and no unit under this phase may.

**Depends on:** step 2.

## Step 4  -  signals are found in noise

**Delivers:** FFT, the waterfall representation, Costas correlation, and
candidate search across the passband.

**Entry:** step 3 complete, verified.

**Exit:**
- A synthesized signal at a known offset and time is found. *must-pass*
- **Twenty simultaneous synthesized signals across the passband are found**, which
  is the real case  -  a 3 kHz slice of 20 m carries dozens at once. *must-pass*
- Candidate ranking is stable across runs. *must-pass*
- `Ft8Sharp` tests green. *must-pass, every unit*
- Attribution clean from `2828ab6`, and the channel tests green.
  *must-pass, every unit* - the full suite is not run by any unit

**Depends on:** step 3, for the fixtures.

## Step 5  -  a found signal becomes a message

**Delivers:** soft symbol extraction, belief-propagation decoding, CRC
validation, and the full path from samples to text.

**Entry:** step 4 complete, verified.

**Exit:**
- A corrupted codeword within the code's correcting power is recovered; one
  beyond it **fails honestly rather than returning a wrong message**. *must-pass*
- **A candidate failing CRC is never returned as a decode**, however tempting the
  partial. *must-pass*
- **No decode that is present and recoverable is lost.** Measured over
  `ft8_lib`'s reference WAVs where `C:\Source\ft8_lib` is present; **skipped,
  not failed, when absent.** *must-pass*

  **Ruled 2026-09-02, replacing "matching its expected decode lists".** Units 217
  and 219 both measured that count and it did not move: 760 of 1298, against a
  representable ceiling of 1157. Unit 219 then took the 78 strong-signal misses
  apart - **5 present and recoverable, 35 present and beyond this code's
  correcting power, 38 not present in the audio at all** - and every one of the
  five is an expected line the list carries twice, which the decoder found and
  de-duplicated by upstream's own payload rule. Across all 169 matchable missed
  lines at -5 dB or better: **0 are recoverable and thrown away.**

  So the old criterion measured the list, not the decoder, and could not be met
  by any decoder that de-duplicates the way upstream does. This one measures the
  property that matters and the instrument for it already exists. **A real
  sensitivity shortfall is step 6's to catch**, against the published figure and
  against physics, not against somebody's expected list.

  The unit reports the raw count alongside it - 760 of 1298 today - so a
  regression in the count is still visible even though it is not the gate.
- `Ft8Sharp` tests green. *must-pass, every unit*
- Attribution clean from `2828ab6`, and the channel tests green.
  *must-pass, every unit* - the full suite is not run by any unit

**This is the subtlest step and sensitivity depends on it almost entirely.** If
it proves to be two problems wearing one coat  -  extraction and integration fail
for different reasons  -  the arbiter should split it and say so.

**Depends on:** step 4.

## Step 6  -  sensitivity meets the published threshold

**Delivers:** a measured sensitivity curve against synthesized signals at known
SNR.

**Entry:** step 5 complete, verified.

**Exit:**
- A curve exists, generated from synthesized signals across a range of SNR, and
  is reproducible. *must-pass*
- **Decode rate at -21 dB is comparable to the published figure.** *must-pass*
- Behaviour below the threshold degrades rather than producing wrong decodes.
  *must-pass*
- `Ft8Sharp` tests green. *must-pass, every unit*
- Attribution clean from `2828ab6`, and the channel tests green.
  *must-pass, every unit* - the full suite is not run by any unit

**This step is the verdict on steps 1 through 5.** Everything before it can pass
its own tests and still be a deaf decoder, because `ft8_lib`'s reference WAVs
carry strong signals and a busy band always does. **A port that decodes at -15 dB
and calls itself finished is broken.** The threshold is published and independent
of `ft8_lib`, which is what makes this measurement worth anything.

**If the number falls short, the step has done its job.** Failing here is the
step working. The arbiter reasons about where the loss is  -  soft symbols first  - 
rather than treating the number as the step's failure.

**Depends on:** step 5.

## Step 7  -  Hamlet displays decoded FT8

**Delivers:** audio from Hamlet into the decoder at 12 kHz, aligned to UTC
quarter-minute slots, and decoded text on screen.

**Entry:** step 6 complete, verified.

**Exit:**
- Audio arrives in 15-second slots aligned to the quarter minute, asserted
  against synthesized audio and a controllable clock. *must-pass*
- **The clock offset is measured and shown.** FT8 needs the PC within about a
  second of UTC or nothing decodes, and it fails silently  -  a blank screen,
  indistinguishable from a dead band. It is the commonest newcomer failure in
  this mode. **Hamlet says so plainly rather than showing an empty window.**
  *must-pass*
- Decodes render on screen. *must-pass*
- `Ft8Sharp` tests green. *must-pass, every unit*
- Attribution clean from `2828ab6`, and the channel tests green.
  *must-pass, every unit* - the full suite is not run by any unit
- **Then the arbiter halts and hands Tim a bench check.**

**This step cannot close itself.** Its assertions are deterministic and the radio
is not involved in any of them. **The phase closes when Tim tunes to 14.074,
looks at the screen, and says it passed.**

**Depends on:** step 6.

---

## What is not in this phase

Named so the arbiter logs rather than chases them (section 3, *a report raising
something outside the phase is logged, not chased*):

- **FT4.** Reuses nearly everything and belongs after this phase.
- **The legibility surface**  -  sync score, SNR, which stage a candidate died at.
  `ft8sharp-spec.md` section 6 makes it the reason this library is worth building, and
  it is a phase of its own once text is on screen.
- **Transmitting FT8.** Not this phase and not this project's section 0.2.
- **The CW decoder.** Nothing in this phase is evidence about it.
- **Whether WSJT-X runs alongside as a comparison oracle.** The spec permits it
  as testing rather than derivation. Unruled, and needed no earlier than step 5;
  the arbiter raises it to Tim if it becomes the cheapest route.
