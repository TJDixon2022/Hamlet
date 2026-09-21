# Work instruction 377 - one RSID file, seven Olivia variants, and none of them on the air until Hamlet has read its own back

**Step 2 of the hardening phase, criterion 2.2 - the one criterion in this phase that was
held for the owner, and the owner has ruled.** Five tasks. **Task 1 builds nothing**: what
the two data files carry, which one the engine actually reads, which variants compose
today and what each variant costs to modulate and decode are all measured before one line
moves, because R41's gate is a measurement and not a preference.

**Status.** `tools/status.sh`, real clock, after every commit and every task.

---

## 0. The project gate

```
SHACK_FACTS.md                                          must exist
src\Hamlet.RadioEngine\Cw\CwProbabilisticDecoder.cs     must exist
CoreHMI.sln                                             must not exist
MURC.sln                                                must not exist
root                                                    C:\Source\HamLet
```

**If any of the four is wrong, stop and say so in `output.md` section 4. Write nothing
else.** The refusal text: *This is not Hamlet. Nothing was changed.*

*All four were checked against the tree at authoring time and all four hold: both files are
present, neither solution exists, and the root is `C:\Source\HamLet`.*

---

## 1. The rules that killed sessions

**HM-DEC-155.** No suite; only this unit's names and `docs\carry-forward-tests.txt` as its
top comment says - **two invocations, one build each**, status written immediately before
each. Never background and poll.

**This unit's own runs are filtered and foregrounded too.** Task 1's measurement is one
`[Fact]` trace in the engine project, run by name. Task 3's proof is two named engine types
and one app type. That is not a suite and it is not a poll.

**The engine invocation is the one that has never failed.** Seven runs for unit 375 and two
for unit 376, 146 of 146 every time, never once touched by the headless dispatcher loop -
and **this unit's work is almost all engine work.** Expect the app invocation to be the
flaky one and record it, do not chase it.

**A loop goes in a script file** (`;` is refused in a compound command).

## 2. The tool facts

Apostrophes in quoted heredocs break; doubled backslashes collapse; `;` is refused; `rm` is
refused; `-m` more than once for a multi-line commit. **Write this repository's files as
UTF-8; a PowerShell `>` redirect writes UTF-16 and the launcher cannot read it.**

- **`git worktree add` is refused.** No task here asks for one.
- **Shell output redirection (`>`) is refused to every path**, including the scratchpad.
  Write files with the editor. **A test run's output is read from the console**, or piped to
  `grep` in the same command.
- **A compound command with `;` or a second operation is refused.** **Python runs here.**
- **An apostrophe inside a quoted argument breaks the arbiter's own tooling** - this
  instruction's loop test had to be retyped without one. Watch it in commit messages.
- **A JSON data file in this repository is read two ways** - as an embedded resource by
  logical name, and from the tree by path, by tests that check the two agree. **Move both or
  neither.**

## 3. Asks still outstanding

Carried per HM-DEC-139, **verbatim in section 4**. Unit 376 raised five, and **two of them
have been answered by the owner since**, in `PHASE_PLAN.md` at HEAD `34e80728`:

- **Unit 376's item 1 - criterion 2.2 and the four RSID tone sequences - is ANSWERED by R41
  and it is this unit's whole subject.** The arbiter reopened it; Tim read the reopening and
  ruled the larger thing. **It comes off the queue and section 6 carries it in full.**
- **Unit 376's item 2 - 6.1 at 214 px against 180 - is ANSWERED by R42.** *Met at the
  measured floor; the criterion reads at or under 220 and is checked.* **Step 6 is done.**
  Task 0 records that and nothing else about it. **Do not re-measure the band. Do not open
  `MainWindow.axaml`.**
- **Unit 376's items 3, 4 and 5 stay** - the rig rearrangement that was not made and why,
  `TopRow`'s `MaxHeight` binding the row's reported height rather than the card's content,
  and section 5's count of four types being 33 where the tree says 29. **All three are
  findings, none wants a ruling, and this unit answers none of them.**

**With unit 376's items 1 and 2 closed, the carried queue is eighteen:** unit 376's items 3,
4 and 5; unit 375's items 3 and 4; unit 374's item 3; unit 373's item 2; unit 372's items 4
and 7; unit 371's five; unit 369's four. **This unit answers none of them.**

---

## 4. Why this unit exists

```
PHASE GOAL: Hamlet holds what it has - the hardening phase, screen, record and test
            work banked in the PSK31 and Olivia threads that needs neither the radio
            nor the owner, judged by tests that ran and, at the end, by Tim at his
            window.
UNIT GOAL:  One RSID data file, carrying every code fldigi knows with its tone
            sequence and fldigi's two tables, read by the detector and by the burst
            generator, every listed code proved to round-trip through Hamlet's own
            generator and detector - and R41's gate built: a variant goes on the air
            only once Hamlet has modulated it and read its own audio back identical,
            and a variant that has not stays off the air in a sentence that says so.
ADVANCES:   step 2, criterion 2.2 - the last criterion of step 2 that no unit has
            been permitted to attempt, now permitted by R41.
DRIFT:      none.
```

**The count today.** Step 0 `partial`, 2 units (0.1 met on unit 369's completed negative,
0.2 to 0.5 met). **Step 1 `done`**, 6 units, closed by unit 374. **Step 2 `partial`**, 2
units: 2.1 met by unit 374, 2.3 met by unit 375, **2.4 not met** - the best streak was one
completed green round against a target of five - and **2.2 never attempted, because until
this morning no unit was permitted to attempt it.** **Step 6 `done`** by R42, 2 units, and
the status files have not caught up (section 5). Steps 3, 4, 5, 7 and 8 `not started`, 0
units each.

**Step 2's entry, checked:** *step 1 done.* Step 1 is done and closed by unit 374. **The
entry is open and has been since unit 374.**

**Why 2.2 and not step 7.** Step 7's entry - *step 6 done* - opened this morning too, and
it is a fair unit. **2.2 is the better one, for a reason that is about the phase and not
about the work:** it is the only place in nine steps where a unit stopped and handed the
owner a question, the owner has answered it on the same day, and an answered question left
unspent is the one kind of waste an unattended phase cannot recover. **R41 is also the
ruling with the most in it** - it overturns the fact R38 (a) rested on, it licenses four
variants Hamlet has never transmitted, and it attaches a gate that has to be built before
any of that is true. Step 7 will still be there tomorrow with its entry open.

**What this unit is worth, in the owner's terms.** Tim answers a station at the variant that
station is using (R27). Today, four of the seven variants Hamlet can hear are variants it
cannot answer in - the send throws where the codes carry no burst. **After this unit he can
answer in any of the seven Hamlet has proved to itself it can read back, and in none that it
has not.**

---

## 5. Verify this instruction against the tree

Every line below was read at authoring time. **Report every mismatch in section 4 and
section 1; repair nothing but this unit's.**

**The two RSID files, and this is the fact that changes the shape of 2.2.**

- **`data\rsid\rsid-codes.json` - 889 bytes, and it is the one the engine reads.** Eight
  codes in `codes` (BPSK31 1, OLIVIA_8_250 69, OLIVIA_16_500 70, OLIVIA_32_1000 71,
  OLIVIA_8_500 72, OLIVIA_16_1000 73, OLIVIA_4_500 74, OLIVIA_4_250 75) and **four tone
  sequences** - BPSK31, 8/250, 16/500, 32/1000. **No `squares`, no `indices`.**
- **`assets\data\rsid-codes.json` - 2,988 bytes, written 2026-09-20, and nothing reads it.**
  The same eight codes, **eight tone sequences** - the four missing ones carried with the
  note *tone_sequences for codes 72-75 added 2026-09-20 from the same ported encoder; the
  Squares and indices tables are fldigi rsid.cxx, GPL-3* - and **both tables**, `squares`
  (256 entries) and `indices` (12).
- **So the data 2.2 asks for is already in the tree and has been since before this phase
  began.** The four sequences are 15 tones each, which is the file's own `symbols`. **What
  2.2 is actually asking for is that the engine read that file instead of the short one**,
  and that is a three-line change with a long tail of readers.

**Who reads the short file, all of it measured by name:**

- `src\Hamlet.RadioEngine\Hamlet.RadioEngine.csproj` line **68**:
  `<EmbeddedResource Include="..\..\data\rsid\rsid-codes.json" LogicalName="Hamlet.RadioEngine.Data.Rsid.rsid-codes.json" />`
- `src\Hamlet.RadioEngine\Olivia\OliviaData.cs` line **24**: `RsidResourceName`, the logical
  name above. **The logical name does not have to change and should not.**
- `src\Hamlet.RadioEngine\Rsid\RsidCodes.cs` line **22**:
  `public const string FilePath = "data/rsid/rsid-codes.json";`, and line **7**'s comment.
- `tests\Hamlet.RadioEngine.Tests\Olivia\TheOliviaDataTests.cs` lines **59**, **136**, **236**
  and **252** - reads by path from the repository root.
- `tests\Hamlet.App.Tests\ViewModels\TheOliviaSeamTests.cs` line **164** - the same.

**`TheOliviaDataTests` line 57-63 is the name that exists to catch a half-move:** it reads
the file from the tree and asserts the codes match the embedded copy. **Moving one and not
the other turns it red, correctly.**

**What already round-trips every code in the file, by construction.**
`tests\Hamlet.RadioEngine.Tests\Rsid\TheRsidBurstTests.cs`:

- **`EveryCodeWithASequenceMakesTheFilesTones`** (line 39) walks **every** entry in
  `ToneSequences`, makes the burst and measures the strongest tone in each symbol back.
  **Its second loop** (line 58) walks every code **without** a sequence and asserts the
  burst is null and `Samples` throws. **With the long file that second loop iterates zero
  times** - see the drop of dead assertions in task 3.
- **`EachBurstReadsBackAtThreeCentersAcrossThePassband`** (line 69) is the generator ->
  detector loopback, every sequence at 500, 1500 and 2500 Hz, `Assert.Equal(tried, read)`.

**So 2.2's *every listed code round-trips through Hamlet's generator and detector* is
asserted by two names that iterate the file** - they go from four codes to eight the moment
the file moves, and **that is the measurement task 1 must print before and after.**

**The gate that keeps four variants off the air today.**
`src\Hamlet.RadioEngine\Olivia\OliviaModulator.cs`:

- Line **147**: `codes ?? throw new InvalidOperationException("the RSID codes could not be read, so the send cannot be announced: " ...)`.
- Line **151**: `if (codes.CodeOf(AnnouncedAs(variant)) is not { } code || Rsid.RsidBurst.TonesFor(codes, code) is null)` -> `throw new InvalidOperationException("the RSID codes carry no burst for Olivia " + variant + ", so the send cannot be announced")`.
- The doc comment above it, **NO BURST, NO SEND**, is work instruction 365's decision AS and
  R27. **It stays true. It is simply no longer the only gate.**

**Where the air is, and it is one site.** `src\Hamlet.App\ViewModels\MainWindowViewModel.cs`
line **16071** is the only call to `OliviaModulator.Compose` in the whole app. The `try`
opens at **16068** and the `catch` at **16084** writes
`Psk31Events.SendRefused(_telemetry, "cannot_compose", macro, "compose", tag)` and
`DigitalSendLine = "Hamlet did not send it: " + error.Message + "."`. **That is the refusal
mechanism R41's sentence rides; no new event and no new stage is needed** (R13).

**Where the send's variant comes from** - unit 376 measured this and it is why R38 (a) was
reopened: `_oliviaSendVariant` is read at line **16009** and set at **6027**, **16533**,
**16789** and **16930**, **from the variant of a decoded row or a card**, and
`OliviaListener` line **280** starts a channel from an RSID detection's own variant. **The
operator does not pick the variant from a list. The air does.** That is exactly why the gate
has to be a gate and not a menu.

**The seven variants.** `data\olivia\format.json` lines **61-67**: 4/250, 4/500, 8/250,
8/500, 16/500, 16/1000, 32/1000, each with its tones, bandwidth, bits per symbol, spacing,
symbol seconds and first-tone offset. `OliviaModulator` is driven from that table.

**What proves a variant today, and it is already on the carry-forward line.**
`tests\Hamlet.RadioEngine.Tests\Olivia\TheOliviaModulatorTests.cs` line **70**,
`EachMacroAndATypedLineComeBackIdentical`, `[InlineData]` for **8/250, 16/500 and 32/1000**:
it calls `OliviaModulator.Compose`, **reads the variant and the center off the burst with
the detector rather than from the test** (decision AT), builds an `OliviaDemodulator` from
what the detector said, decodes, and asserts the text comes back identical for every macro
and a typed line at two centers. **That is R41's loopback, already written, for three of the
seven.** It is on the engine carry-forward line by name.

**The counts to expect.** Unit 376's exit run: **app 211 of 211, engine 146 of 146**. The
app invocation takes about 2 m 18 s; the engine about 4 m 39 s. **Four clean invocations out
of four in unit 376 - no dispatcher loop at either end.**

**The version.** `Directory.Build.props` line **993**: `<Version>1.13.63</Version>`.

**The record's own state, and the first two are yours to repair in task 0.**

- **Yours.** `PHASE_STATUS.md` and `PHASE_OUTCOME.md` both carry `STEP: 6 | partial`, while
  `PHASE_PLAN.md` at HEAD ticks **6.1, 6.2, 6.3 and 6.4 all `[x]`** and R42 states why: *met
  at the measured floor; 180 px was the author's number, the sun map's height makes 197 the
  arithmetic minimum and 214 is what was reached with nothing lost; the criterion reads at or
  under 220 and is checked.* **Set both files to `STEP: 6 | done`, citing R42 in one line.
  This is transcription of the owner's ruling, not a judgment of yours, and it is not one of
  your two rulings.**
- **Yours.** `PHASE_STATUS.md` `CURRENT_STEP: 0` and `WORK_INSTRUCTION: 376 - ...`. Set them
  to `2` and to this instruction.
- **Not yours.** `PROJECT_STATUS.md` `RULES_AT` reads `HM-DEC-165 (2026-09-19)` while
  `CLAUDE.md` section 1 holds `CPS-DEC-0165` - the id-scheme split, carried in
  `PHASE_PLAN.md` section 7, and reported unrepaired by unit 376 as it was told to. **Report
  it again, do not repair it.**
- At authoring time HEAD was **34e80728** on `main`. **Two uncommitted at the root, neither
  under `src\`:** `PHASE_STATUS.md` carrying one added `HEARTBEAT:` line, and
  `.run-unit\reload.txt`.

**What failures are expected, and what they mean.**

- **Task 2 moves a data file's path.** Expect the five reader sites in section 5 to be the
  whole of it: the csproj, `RsidCodes.FilePath`, and the five test reads across
  `TheOliviaDataTests` and `TheOliviaSeamTests`. **Those test reads are a path this unit
  moved, so moving them with it is not an R12 rewrite and asserts exactly what it did
  before.** Say so in the report rather than claiming a rewrite.
- **Task 2 adds a field to a variant row in `format.json`.** **Measure what that turns red
  before you write it** - a strict parser or a name asserting a row's exact shape is
  possible and was not read at authoring time. If one goes red it is this unit's under R12.
- **What is not expected and is not a rewrite:** any name in `TheRsidDetectorTests`,
  `TheRsidBurstTests`, `TheOliviaDemodulatorTests`, `TheOliviaBlindSearchTests`,
  `TheOliviaListenerTests` or `TheOliviaBelowTheNoiseTests` going red. **Those read the air.
  A red there means the longer file changed detection**, which is a regression under
  HM-DEC-165 and is reported as one, not rewritten.
- **`TheOliviaSendTests.AnOliviaCqReachesTheAir` going red is the loudest possible signal**:
  it is Olivia's send guard on the carry-forward list. **If the gate you build refuses the
  send that reaches the air today, the gate is wrong, not the guard.**

---

## 6. Rulings in force

Transcribed in full. **Do not re-argue any of these.**

### The owner's ruling that licenses this unit, in full

**R41 - Tim, 2026-09-21: Hamlet may transmit at all seven Olivia variants, each proved by
loopback.** *R38 (a) rested on a wrong fact - the modulator is table-driven and makes every
variant in `data/olivia/format.json`. The four missing RSID sequences go in; a variant goes
on the air only once Hamlet's own modulator and demodulator round-trip it; a variant that
fails loopback stays off the air with a sentence saying so. Answering a station at his
variant is R27's whole point.*

**This answers unit 375's parking of 2.2 and unit 376's reopening of it. The block is
lifted by the owner's own words and the later ruling wins** (`PHASE_PLAN.md` section 6).
**It is not lifted further than it reads:** R41 permits a proved variant and forbids an
unproved one. **A variant that this unit cannot round-trip does not go on the air, is not
argued with, and is not made to pass by loosening what round-trip means.**

**And the stop is still there, one inch away.** `PHASE_PLAN.md` section 6: *anything that
would change what goes on the air, or what keys - `MOVE: stop`.* R41 licenses one specific
change to what goes on the air, gated one specific way. **Anything you find that would put a
variant on the air that the loopback has not proved - a caller that bypasses the gate, a
default that means permitted, a path where the variant is chosen after the check - is
outside R41. Stop, record it, hand it back.**

### The first is mine: what replaces what, and what a loopback proof is

1. **The tree keeps one RSID data file and it is `assets\data\rsid-codes.json`.** The csproj
   `EmbeddedResource Include` points at it, **with its `LogicalName` unchanged** so nothing
   downstream of the logical name moves; `RsidCodes.FilePath` becomes
   `assets/data/rsid-codes.json`; **and every test that reads the old path moves in the same
   commit**, so the file in the tree and the file in the assembly can never be two files.
2. **`data\rsid\rsid-codes.json` is not deleted** (`PHASE_PLAN.md` section 6: *empty it,
   comment it, list it*). It is left carrying a single key naming the file that replaced it
   and the unit that did it, **a test asserts nothing in `src\` or `tests\` reads that path
   any more**, and the report lists it.
3. **A loopback proof, for R41, is this and only this:** Hamlet composes the text at the
   variant through the same `OliviaModulator.Compose` the send uses; **Hamlet's own detector
   reads the variant and the center off that audio** and they are what the demodulator is
   built from (decision AT - never the test's own literals); Hamlet's demodulator decodes;
   **the text comes back identical, character for character**. Clean audio, no added noise -
   the noise cases are `TheOliviaBelowTheNoiseTests`' and are not in scope.
4. **The proof is the existing name, extended.** `EachMacroAndATypedLineComeBackIdentical`
   gets the four new `[InlineData]` variants. **Do not write a second loopback rig.** It is
   already on the carry-forward line, so extending it banks R41's proof permanently and by
   name - which is what makes the gate true tomorrow and not only tonight.
5. **In-process only.** No port is opened, no device enumerated, no sound card touched, and
   nothing is keyed. Every sample lives in an array (FACT-004). **Nothing in this unit is
   evidence about the radio, and the report says so.**

*Author's, overrulable.* It settles a path, a mechanism and what a test means, which
`PHASE_PLAN.md` section 6 makes the arbiter's and never a stop.

### The second is mine: where "proved" lives, where the gate stands, and the order it lands in

1. **"Proved" is data, in the table the modulator is already driven from.** Each row of
   `data\olivia\format.json`'s `variants` gets **`proved_by_loopback`**, read into the
   variant the engine already carries. **Absent or false means not proved**, because the safe
   direction is off the air, and a file Hamlet cannot read must not become a file that
   permits everything.
2. **The gate stands at the one site where audio becomes a transmission** - the app's Olivia
   send path, **before** `Compose` is called at `MainWindowViewModel` line 16071. An unproved
   variant writes `Psk31Events.SendRefused` with the reason **`variant_not_proved`** and a
   sentence on `DigitalSendLine`, and returns. **Nothing is composed and nothing is keyed.**
   **No new event type and no new stage** (R13) - it is the existing refusal with a truer
   reason on it.
3. **The engine keeps composing.** `OliviaModulator.Compose` is a library call and the
   loopback proof itself goes through it; gating it would make the proof impossible to write
   without a bypass, **and a bypass is the hole**. The *air* is the app. **Assert this rather
   than assuming it: a test names 16071 as the only `Compose` call in `src\Hamlet.App\` and
   fails if a second appears.**
4. **The sentence is operator-facing and says the true reason** (section 0.0, R19 American).
   Its shape: *Hamlet did not send it: it has not proved to itself that it can read back
   Olivia 4/500, so it will not put that variant on the air.* **The exact words are the
   unit's; whatever you write, a test asserts that string exists in an operator-facing
   string**, and **it never tells him to touch the radio** (R11).
5. **The order the work lands in is part of the ruling, because the order is the safety.**
   Task 2 moves the file **and** builds the gate **in the same task**, with
   `proved_by_loopback` true for **8/250, 16/500 and 32/1000 only** - the three the existing
   green loopback name already proves. **After task 2 the set of variants Hamlet will
   transmit is exactly what it is today**, and the report says so with a diff. **A variant's
   flag is flipped to true only in a commit that also carries the run that proved it**
   (task 3). **There is no commit in this unit's history where Hamlet would transmit a
   variant it has not read back.**

*Author's, overrulable.* A mechanism, a default, a refusal's wording and the order of
commits are the arbiter's by `PHASE_PLAN.md` section 6.

**That is two rulings, which is R31's limit.** The eighteen carried asks get none. Recording
step 6 as done is R42's, not a ruling of this unit's.

### The phase's standing rulings

**R42 - Tim, 2026-09-21, on 6.1:** *met at the measured floor. 180 px was the author's
number; the sun map's height makes 197 the arithmetic minimum and 214 is what was reached
with nothing lost. The criterion reads at or under 220 and is checked.* **Step 6 is done.
Task 0 records it. Nothing else in this unit touches the screen.**

**R38 (b) - Tim, 2026-09-21.** The transmit sequence's teardown abort pair is accepted,
logged as tidy-up, not a stop. **Closed. Nothing here touches it, and nothing here touches
the abort path, `StopNow` or any unkey.**

**R27 - every Olivia send begins with its variant's RSID burst**, and answering a station at
his variant is the point of the mode. **The burst is not optional and this unit does not make
it optional** - it makes four more of them possible.

**R35 - the record's small lies.** 2.2's *every RSID code fldigi knows is in the data file
with its tone sequence* is this unit's half of it.

**R11 - nothing at the radio.** No sentence this unit writes asks the operator to set a
level, read a meter, or know what ALC is.

**R12 - a session fixes its own tests and never asks the owner to approve it.** A test this
unit turns red is this unit's to rewrite **in its own commit**, asserting more than it
replaced and never less. **The arbiter never puts the wording of a test to the owner.**

**R13 - telemetry is a must-pass on every remaining step.** **This unit adds no stage and no
event**; it adds one reason value to a refusal that already exists. If you find yourself
writing a new event, you have left the criterion.

**R14 - eyes on the prize.** *"We don't focus too much on pointless testing."* The tests this
criterion needs and no others.

**R19 - American spelling** in every operator-facing string and every instruction.

**R31 - this phase runs unattended.** Criteria by id. A done step is closed. The owner's step
ends the run. **Two rulings per unit at most.**

**HM-DEC-018 section 2.1** nothing personal in an event - the refusal carries the variant and
the reason, **never the text and never a callsign**. **Section 0.0** a sentence on the screen
is a claim and a refusal says the true reason. **Section 0.5** hiding detail is allowed,
hiding information is not. **HM-DEC-155**, **HM-DEC-139** (the carried queue verbatim),
**HM-DEC-165** (no name green before a unit is red after it), **FACT-004** (nothing here is
evidence about the radio).

## 7. Status cadence

`tools/status.sh`, real clock, **after every task and every commit**, and **immediately
before each carry-forward invocation** - task 0's two and task 4's two. **Task 1's
measurement is one task**: status before it and after it, not between runs. **Task 3's
loopback runs are one task** likewise. Never compose a timestamp.

---

## 8. The tasks

### Task 0 - the record, step 6's close, and the entry run

Append `UNIT 377 - STEP 2` to `PHASE_OUTCOME.md` with `ADVANCED: step 2`. Patch-bump
**1.13.63 -> 1.13.64** in `Directory.Build.props` with its line in the version log.

**Set `STEP: 6 | done` in `PHASE_STATUS.md` and `PHASE_OUTCOME.md`, citing R42 in one
line** - the plan at HEAD ticks all four of step 6's criteria and both status files still
say partial (section 5). **Set `CURRENT_STEP: 2` and `WORK_INSTRUCTION: 377 - ...`.** One
line in the report saying you did and why. **Do not re-measure the band and do not open
`MainWindow.axaml`.**

**Run the carry-forward list, both invocations, before anything changes**, status written
immediately before each, and put the two counts in the outcome entry's `ENTRY:` line. Unit
376 left it at **app 211 of 211, engine 146 of 146**. A red here is not yours; name it and go
on. **If either invocation dies of `InvalidProgramException: You have caused dispatcher loop`
before any assertion, that is unit 375's item 3 - re-run it once and record both attempts.**

**Drop candidate:** none.

### Task 1 - the trace: what the tree holds, what composes, and what each variant costs

**This task builds nothing, changes no source file and repairs nothing.** One `[Fact]` trace
named for this unit in `tests\Hamlet.RadioEngine.Tests\Olivia\` or `\Rsid\`, run by name.
Print:

1. **Both RSID files side by side** - every code, whether each has a tone sequence in each
   file, whether each file carries `squares` and `indices`, and the length of every sequence
   against the file's own `symbols`. **This is 2.2's before.**
2. **Which file the engine is reading right now**, proved rather than assumed: the embedded
   copy's sequence count beside the tree copy's at each path.
3. **Every one of the seven variants through `OliviaModulator.Compose` today**, on one short
   macro, and **what happens** - the samples, the announced code and center for the ones that
   compose, and **the exception message verbatim** for the ones that do not. **Expect four to
   throw line 151's sentence. Print it; do not repair it in this task.**
4. **The cost of a loopback at each variant, and this is the number the sweep's breadth turns
   on**: for each of the seven, using the three that compose today and the format table's
   arithmetic for the four that do not, **the seconds of audio for one macro and for a typed
   line, and the decode CPU seconds** measured where it can be measured. **4/250 is the
   slowest variant in the table and 4/500 is next; if the full sweep is minutes, task 3's
   drop candidate is the answer and this task is what licenses it.**
5. **`TheRsidBurstTests`, `TheRsidDetectorTests`, `TheOliviaDataTests`,
   `TheOliviaModulatorTests` and `TheOliviaDemodulatorTests`, run once, filtered, one
   build**, counts recorded. **That is 2.2's before for the round-trip claim.**
6. **What a new field in a `variants` row would turn red** - read `OliviaFormat`'s parse and
   say whether an unknown or added key is tolerated, and name any test asserting a row's
   exact shape. **Read it, do not change it.**

**What this task must answer in the report, by id.** For **2.2**: which codes have sequences
in which file, which tables are where, which of the seven variants compose today and which
throw with what sentence, and the five types' counts before anything moves.

**Drop candidate: none, and if the unit runs long everything else goes before this does.**
A measured before handed to the next unit beats an unmeasured change.

### Task 2 - one file, every reader, and the gate - in one task, in its own commits

**The file, first commit.** `assets\data\rsid-codes.json` becomes the one RSID data file:
csproj line 68's `Include` with **`LogicalName` unchanged**, `RsidCodes.FilePath` and its
comment, and the five test reads in `TheOliviaDataTests` and `TheOliviaSeamTests`, **all in
the one commit** so the tree copy and the embedded copy are never two files.
**`data\rsid\rsid-codes.json` is emptied to a single key naming its replacement and this
unit - never deleted** - and a test asserts nothing under `src\` or `tests\` reads that path.

**The gate, second commit.** `proved_by_loopback` on each row of `data\olivia\format.json`'s
`variants`, read into the engine's variant; **true for 8/250, 16/500 and 32/1000 and false
for the other four**; the refusal at the app's send path before line 16071, with reason
`variant_not_proved` and the sentence; and the test that line 16071 is the only
`OliviaModulator.Compose` call under `src\Hamlet.App\`.

**Say in the report, with a diff, that after this task the set of variants Hamlet will
transmit is identical to the set it would transmit at task 0.** The file got longer, the
detector got four more bursts to recognize, **and nothing new can be sent.** That is the
point of the order.

**Drop candidate: none. This is the gate, and the gate is what R41 licensed the rest on.**

### Task 3 - the loopback, and a flag flipped only beside its evidence

**Extend `EachMacroAndATypedLineComeBackIdentical` to all seven variants** - four new
`[InlineData]` rows, nothing else about the name's shape changed, **the variant and the
center still read off the burst by the detector and never from the test**.

**Run it. Then, for each of the four new variants, in the same commit as the run that proved
it:** round-tripped identical on every case -> `proved_by_loopback` true; anything less ->
**it stays false, it stays off the air, and the report says which variant, which case, how
many characters came back and what the sentence the operator would see reads.** **Do not
retune the demodulator to rescue a variant** - that is a decoder change and it is outside
this unit and outside R41.

**Also in this task:**

- **The round-trip claim of 2.2.** Record `EveryCodeWithASequenceMakesTheFilesTones` and
  `EachBurstReadsBackAtThreeCentersAcrossThePassband` going from four sequences to eight,
  **with the count printed**. **Their dead second loop** (`TheRsidBurstTests` line 58, now
  iterating zero times) is this unit's under R12: **replace it with the assertion 2.2
  actually makes - every code the file lists has a sequence of the file's own length** - so
  the name asserts more than it did and not less.
- **The refusal.** A test that an unproved variant is refused at the send path with the
  reason `variant_not_proved` and that the sentence exists in an operator-facing string, and
  that **nothing was composed and nothing keyed** on that path (no port, no `Arm`, no
  `PttOn`).
- **`docs\carry-forward-tests.txt`**: the extended loopback name is already on the engine
  line by type and method, **so check its paragraph still describes what it now covers and
  say what the four extra theory cases add to the invocation's time**. Add the refusal name
  by type and method with its paragraph, **and change the human-readable list underneath to
  match** - a mismatch there has been a finding in three of the last four units. **Add
  nothing that is knowingly red.**

**Drop candidate: the breadth of the sweep for the four new variants, and only that.** If
task 1 measured the full set of texts at two centers as minutes rather than seconds for
4/250 and 4/500, prove those variants on **one macro and one typed line at one center** -
the detector still choosing variant and center - **leave the three existing variants' sweep
exactly as it is**, and say in the report which cases were not run and what the measured cost
was. **A whole variant is never the drop: a variant nobody proved stays false and stays off
the air, which is the gate working and not a shortfall.**

### Task 4 - the exit run, the record and the report

- **Run the carry-forward list, both invocations, after the last change.** Compare name by
  name against task 0. **A red after that was green before is a regression** and sections 1
  and 4 both name it as one (HM-DEC-165). **Olivia's send guard,
  `TheOliviaSendTests.AnOliviaCqReachesTheAir`, and its read guards are the ones to read
  first.**
- **`PHASE_STATUS.md` and `PHASE_OUTCOME.md`:** step 2 criterion by criterion by id (R31).
  **2.2 is `met` only if the file moved, the detector reads all eight, every listed code
  round-trips, and every variant marked proved was proved by a run in this unit's history.**
  Any one short and it is `partial` with what was reached. **Do not round up.** **Step 2 is
  `done` only if 2.4 is met too, and 2.4 is not this unit's** - so expect to write step 2
  `partial` with 2.1, 2.2 and 2.3 met and 2.4 outstanding, **and say plainly that it is 2.4
  and nothing else that holds the step open.**
- Write `output.md` per section 12.

**Drop candidate:** none.

---

## 9. Parked - do not touch, do not raise

- **Criterion 2.4 - the five green rounds.** Not this unit's. **Run the carry-forward list
  twice, at task 0 and task 4, and no more.** A change to the transmit data path has to
  settle before five consecutive runs of anything mean what they say, and unit 375 spent a
  whole unit to reach a streak of one against the headless dispatcher loop. **The next unit's
  night, not yours.**
- **Step 6 and the screen.** R42 closed it. **Record `done` at task 0 and open no `.axaml`
  file in this unit.**
- **Steps 7 and 8** - the canned list, the hover on a PSK31 row, the achievements. **Step
  7's entry opened with R42 and it is the next unit's most likely subject. Do not build
  toward it, and do not touch `data\psk31\canned.json`.**
- **Steps 3, 4 and 5.** The visibility events, the radio sheet, Tim's verdict.
- **The decoder and the demodulator's tuning.** `OliviaDemodulator`'s thresholds, passes,
  tracking and sync are **not** this unit's, not even to rescue a variant that fails
  loopback. A variant that fails stays false.
- **The abort path, `StopNow`, the teardown pair, `Arm`, `PttOn`, the drive level and every
  sample of composed audio for PSK31, FT8 and FT4.** R38 (b) closed the one live question
  there and nothing here goes near it.
- **`PHASE_PLAN.md`'s criterion checkboxes** (unit 372's item 7). Do not tick them; do not
  raise it again. The record this phase maintains is `PHASE_STATUS.md` and `PHASE_OUTCOME.md`.
- **`tools\run-carry-forward.sh`** (unit 374's item 3). It still does not match the list.
  **Run the two command lines from `docs\carry-forward-tests.txt` itself.**
- **The headless dispatcher loop** (unit 375's item 3) and
  `TheStopIsAlwaysOnScreenTests.WithNothingKeyedItSaysStopAndIsStillPressable` (item 4).
  **Record if they show; do not chase.**
- **`ApplyBestBet`'s stale `BestBetLabel`** (unit 373's item 2), **`LearnedAlcReference.Ago()`**
  (unit 369's item 3), **the archived Olivia phase** at `docs\phase-olivia-run\` - a phase is
  never reopened - and **the `RULES_AT` id-scheme split.** Carry them; do not repair them.
- **Any package.** A package is `MOVE: stop`.

## 10. What not to do

- **No unfiltered `dotnet test`** (HM-DEC-155). Every invocation filtered, foregrounded, one
  build. **Never background and poll. Never compose a timestamp.**
- **Do not put a variant on the air that this unit did not read back.** Not by defaulting the
  flag to true, not by marking a variant proved on the arithmetic of the format table, not by
  proving 8/500 and inferring 4/500. **R41 says round-trip, and round-trip means Hamlet read
  Hamlet.**
- **Do not loosen what identical means.** The existing name compares the decoded text to the
  sent text after `Unify`; **do not add a tolerance, a character count threshold or a
  "close enough"** to make a variant pass. A variant that fails is a variant that fails.
- **Do not retune the demodulator, the detector or the modulator** to rescue a variant
  (section 9). That is a decoder change, it is not in 2.2, and it is not what R41 licensed.
- **Do not delete `data\rsid\rsid-codes.json` or any other file.** Empty it, comment it, list
  it (`PHASE_PLAN.md` section 6).
- **Do not change the embedded resource's `LogicalName`.** The path moves; the name the
  assembly knows it by does not.
- **Do not add an event or a stage** (R13). One reason value on an existing refusal is the
  whole of the record change.
- **Do not put the text, a callsign or a grid in the refusal** (HM-DEC-018 section 2.1). The
  variant and the reason, and nothing else.
- **Do not open a port, enumerate a device, or key anything.** Every sample in this unit
  lives in an array (FACT-004), and the report says so.
- **Do not loosen a test to make a criterion pass** (`PHASE_PLAN.md` section 6). Rewrite it
  under R12 to assert the new rule, asserting more and never less, or leave it red and report.
- **Report mismatches; repair nothing but this unit's. Write American. Write files as UTF-8.**

## 11. Committing and pushing

**One commit per task, and within task 2 two commits** - the file and its readers first, the
gate second - **and within task 3 one commit per variant proved, or one commit carrying both
the run and the flags it justifies**. The rule is section 6's fifth item: **no commit in this
unit's history may leave Hamlet able to transmit a variant it has not read back.** An R12
rewrite of an existing test goes in its own commit. A carry-forward line edit goes in the
same commit as the test it names. Push once, at the end, after task 4's runs are green or
their reds are named.

---

## 12. Reporting

`output.md` at the root. **Canonical headings:** `## 1. What Claude did`, `## 2. What the
owner should expect`, `## 3. What you should see`, `## 4. What's blocking us`.

**The ordering block first. `validate-output.bat` refuses a report without it.** Fill every
line from what you measured - a line that is the same every unit is furniture.

```
READ IN THIS ORDER.

A. The phase goal - Hamlet holds what it has. Step 0 partial, step 1 done, step 2
   partial with 2.1 and 2.3 banked and 2.4 short at one round of five, step 6 done
   by R42 and recorded so at task 0, steps 3, 4, 5, 7 and 8 not started. This unit
   is the first ever spent on 2.2, which was the owner's until R41 this morning.
B. Step 2 - the record says what was true. 2.2 <met|partial|not>: the one RSID file
   is <path>, carrying <n> codes with <n> tone sequences and both tables, against
   <n> and <n> at task 0; every listed code round-trips generator to detector,
   <n> of <n> at <n> centers; and of the seven Olivia variants <n> are proved by
   loopback and <n> are refused, named. 2.1 and 2.3 carried met, unaltered. 2.4
   not attempted and it is the only thing holding step 2 open.
C. The report last. Section 4 raises <N> items on top of the carried eighteen, and
   <none of them is | item <k> is> in the way of a criterion in B. Unit 376's items
   1 and 2 came off the queue answered - R41 licensed this unit, R42 closed step 6.
```

```
UNIT:       377 - <complete|stopped> at task N of 5 - <date time>
PHASE GOAL: <restated in your own words>
UNIT GOAL:  <restated in your own words>
ADVANCED:   step 2, criterion <the ids you actually moved>
NUMBER:     Olivia variants Hamlet will transmit: <before> of 7 -> <after> of 7,
            each of the <after> proved by a loopback run named in this report
DRIFT:      none
```

**Section 3 must lead with task 1's two-file table and the seven-variant compose table** -
every code, which file carries its sequence, which tables are where, and what each of the
seven variants did when composed today, with the refusal sentence verbatim for the four that
threw - **because that table is what makes 2.2 a measurement and not a claim.** Then the same
tables after task 2. Then **the loopback table: each of the seven, characters in, characters
out, identical or not, seconds of audio, decode CPU, and the flag it earned.** Then the
generator-to-detector round-trip counts, before and after. Then the five types' counts. Then
the carry-forward counts before and after.

**Section 2 tells Tim in one paragraph what is different**, in his terms: not that an
embedded resource moved, but that **Hamlet can now answer a station in the variant that
station is using, in every one of the seven it has proved to itself it can read back - and
that where it has not proved one, it says so plainly instead of sending something it cannot
read.** Every claim computed, not seen (FACT-004), and say in one line that no port was
opened and nothing was keyed.

**Section 4:** your own items first, most-blocking first, each saying plainly whether it
wants a ruling or is a finding - a note is not a ruling request.

**If any variant failed its loopback, that is your first item and it is a finding, not a
ruling request** - R41 already ruled what happens to it. Name the variant, the case, what
came back, and what would take it off the bench.

**If you found anything that could put a variant on the air without passing the gate, that is
your first item and it wants a ruling** - it is outside R41 and inside `PHASE_PLAN.md`
section 6's first stop.

Then the carried queue verbatim per HM-DEC-139: **unit 376's items 3, 4 and 5; unit 375's
items 3 and 4; unit 374's item 3; unit 373's item 2; unit 372's items 4 and 7; unit 371's
five; unit 369's four - eighteen**, with one line saying that unit 376's items 1 and 2 came
off it by R41 and R42.

---

```
ARBITER-DECISION
STEP: 2
APPROACH: give codes 72-75 their RSID tone sequences in assets/data/rsid-codes.json and let an Olivia variant on the air only once the modulator and demodulator round-trip it in loopback, with an unproved variant refused in a sentence
MOVE: continue
WHY: 2.2 is the only criterion in nine steps where a unit stopped and handed the owner a question, and R41 of 2026-09-21 answers it in the larger direction - all seven variants may transmit, each proved by loopback - so the block is lifted by the owner's own words and the later ruling wins; the loop test finds nothing resembling this approach in any entry, because no unit has ever been permitted to attempt 2.2. Step 7's entry opened this morning too and is the fair alternative, but an answered question left unspent is the one waste an unattended phase cannot recover, and step 7 will still be there tomorrow.
STATE: partial
DECIDED: author's, overrulable, two, both transcribed in work instruction 377 section 6. (1) What replaces what and what a loopback proof is: the tree keeps one RSID data file and it is assets/data/rsid-codes.json, with the csproj Include moved and its LogicalName unchanged, RsidCodes.FilePath moved, and all five test reads of the old path moved in the same commit so the tree copy and the embedded copy can never be two files; data/rsid/rsid-codes.json is emptied to a key naming its replacement and never deleted, per PHASE_PLAN.md section 6, with a test asserting nothing reads that path; and a loopback proof is Hamlet composing through the same OliviaModulator.Compose the send uses, Hamlet's own detector reading the variant and center off that audio, Hamlet's demodulator decoding it, and the text coming back identical on clean audio in process, proved by extending the existing carry-forward name EachMacroAndATypedLineComeBackIdentical rather than writing a second rig. (2) Where proved lives, where the gate stands and the order it lands in: proved_by_loopback is a field on each row of data/olivia/format.json's variants table, absent or false meaning not proved because the safe direction is off the air; the gate stands at the app's Olivia send path before MainWindowViewModel line 16071, refusing with reason variant_not_proved and an operator-facing sentence, no new event and no new stage, with a test asserting 16071 is the only Compose call under src/Hamlet.App; the engine keeps composing because the proof itself goes through Compose and a bypass would be the hole; and the order is part of the ruling - task 2 moves the file and builds the gate with only the three variants the existing green loopback already proves marked true, so no commit in this unit's history leaves Hamlet able to transmit a variant it has not read back, and a flag is flipped only in the commit that carries the run that proved it. Recording step 6 as done is R42's and is not counted as a ruling of this unit's.
LICENCE: PHASE_PLAN.md R41 - the owner's ruling of 2026-09-21 that Hamlet may transmit at all seven Olivia variants, each proved by loopback, which lifts unit 375's parking of 2.2 and answers unit 376's reopening of it; R42 closing step 6; R38 (b), R35, R31 and section 6 - a path, a mechanism, a default, a refusal's wording and the order of commits are the arbiter's and never a stop, and the later ruling wins; PSK31 plan R11, R12, R13, R14, R19, R27; CLAUDE.md 0.0, 0.2, 0.5; HM-DEC-018 section 2.1, HM-DEC-139, HM-DEC-155, HM-DEC-165; FACT-004
ACCOMPLISHED: When a station calls Tim in an Olivia variant Hamlet has never been able to speak, Hamlet answers him in it - in any of the seven the mode has, and in none that Hamlet has not first proved to itself it can read back. The proof is Hamlet sending to Hamlet with no radio involved: it makes the audio the way it would make it on the air, listens to its own audio, and only counts the variant if the words come back exactly. Where a variant does not come back, Hamlet will not transmit it and says so in a sentence instead of sending something it cannot read. And the file that names every one of these announcements stops being two files that could disagree.
ADVANCES: step 2, criterion 2.2 - the file, the tone sequences for codes 72-75, the two tables, the detector reading all of them, every listed code round-tripping through Hamlet's generator and detector, and R41's loopback gate on all seven variants. It does not advance 2.4, which is deliberately left to a unit whose night is its own and which is then the only thing holding step 2 open.
END-ARBITER-DECISION
```
