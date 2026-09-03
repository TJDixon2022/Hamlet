READ IN THIS ORDER

A. THE PHASE GOAL. Hamlet hears FT8 off the radio and displays the decoded text
on screen. Steps 1 to 5 are done. Step 6 is blocked at 14 of 306 against a 40 per
cent band, on an owner ruling that is already in front of him and that nobody
else may give. Step 7's five must-pass criteria are all evidenced by units 224 to
228, and its closing line - the owner at 14.074 - was performed on 2026-09-03 and
nothing appeared on screen.

B. THIS STEP AND ITS EXIT CRITERIA. Step 7's five criteria are: slot alignment
asserted against synthesized audio and a controllable clock (unit 224); audio at
a sound card's own rate decoding to the message that went in (unit 224); a decode
reaching the table as itself (unit 224); an unmeasured clock refusing in words
rather than showing an empty table (unit 226); and the tab decoding unattended
rather than on a press (unit 225, hardened by 228). All five are met and none is
reopened. THIS UNIT CLAIMS NONE OF THEM. What it clears instead is whether
Hamlet's two record-keepers - the telemetry sink and the digital capture - write
anything at all, which decides whether the next bench check can be read.

C. THIS REPORT. Section 4 raises 5 items. NONE of them is in the way of a
criterion in B, because all five criteria in B are already met. THE SINK WRITES:
driven with `App.axaml.cs`'s own four arguments it put today's dated file on disk
in 12 milliseconds with no `Dispose` called, so the stop rule in task 2 did not
fire and all seven tasks ran. The capture write path works too, watched from a
test for the first time. What bears on A is item 1: the newest build that has ever
written a line on this machine is 1.12.0, thirty-seven patches behind this tree,
which is the cheapest available explanation of the whole silent morning.

UNIT:       234 - complete - 7 of 7 tasks, nothing dropped - 2026-09-03 13:37
PHASE GOAL: Hamlet hears FT8 off the radio and shows the decoded text on screen.
UNIT GOAL:  Watch Hamlet's two record-keepers actually write, by driving them the
            way the application drives them, and make a refused capture press
            leave a line instead of a status bar that scrolls away.
ADVANCED:   no - this unit clears a blocker on step 7 and claims none of its five
            criteria. A unit cannot advance a step whose only remaining work is
            the owner's, at a radio, with an antenna on it.
NUMBER:     sessions of Hamlet whose record can be found afterwards: 0 -> 0. Task
            1 found no file anybody had missed. What moved is that a missing
            record is now a failing test rather than a mystery.
DRIFT:      3. Reported honestly and it is structural: step 7's five criteria are
            all met and its remaining work is the owner's; step 6 is out of
            unit-reachable moves and waits on a ruling. There is no step left for
            a unit to advance, so a unit that does real work still reads as drift.

---

## 1. What Claude did

Seven tasks, all seven completed. **Nothing was dropped**, including task 5, the
named drop candidate - it was droppable because tasks 2, 3 and 4 all landed green,
but task 1 measured something the owner needs on that sheet, so it was written.

### Task 1 - the trace, and the finding that reframes the morning

A scratch xunit harness (overwriting unit 233's spent file, left untracked, never
staged) read `%AppData%\Hamlet` and wrote what it found into the repository.
**Nothing was written into the operator's folder** - not a probe file, not a
folder. It is a listing and a set of reads.

Eight files and one subfolder. `captures` **still does not exist**. The newest
telemetry file is **still `2026-08-28.jsonl`** - no file for 2026-09-03, none
after 2026-08-28 at all - while `settings.json` was rewritten that same day at
16:35:55 UTC, so a Hamlet did run. Then the census nobody had asked for.

### Task 2 - the sink, driven the way `App.axaml.cs` drives it

A committed test in `tests/Hamlet.App.Tests/Telemetry/`, constructing
`JsonlTelemetry` with the same four arguments the shell passes - a folder, a
version string, `settings.IsTelemetryEnabled` off a default `AppSettings`, and
`TelemetryMaxMegabytes * 1024 * 1024` - pointed at a temporary folder, then
`AppEvents.AppStart` on it. Four facts, each asserted as a file on disk and not as
a `Write` call returning.

**On `TelemetryTests.cs`, which section 2 told me to read first.** It does not
assert what task 2 asserts and was not extended. It proves the daily-file naming,
the eviction order, the never-throw discipline and `ClearAll`, all against a
hand-written `_ => true` predicate inside a `using`. Nothing in it builds the sink
from an `AppSettings`, goes through `AppEvents`, reads a line back as JSON, or
measures anything without `Dispose` - and a process that never disposes is exactly
the bench case. The new class is a different question in a different project, and
it says so in its own remarks.

### Task 3 - the capture, driven the way the button drives it

A committed test that points `MainWindowViewModel.CaptureFolder` at a temporary
folder, restores it in a `finally`, and asserts it is back. **The seam:**
`CaptureDigital` reads `_decoder?.Tap`, and `_decoder` is a private field set only
when a sound card opens. Rather than open one, the field is set by reflection to a
real `CwDecoder` whose real `AudioTap` has been fed real samples; everything
downstream - the decode, the WAV, the sheet - is production code running
unmodified. Nothing opens a window and nothing reaches a transmitter.

### Task 4 - the four silent paths

One new event, `AppEvents.DigitalCaptureRefused`, written on every one of the four
ways `CaptureDigital` produces nothing. Its parameter is a new enum,
`DigitalCaptureRefusal`, with four members - and **the two exception members are
named for their exception types**, which is how the type name reaches the file
without a string ever being passed. HM-DEC-018 is enforced by the signature, in
the manner unit 233 used for `Ft8SlotCensus`: there is no parameter that can hold
a character, so the call site has nothing to remember. Level `warn`.
`CallsignPrivacyTests`'s walk went 64 to 65.

**No sentence was added to any screen.** The status bar keeps the four sentences
it already had, in Tim's words, unchanged.

### Task 5 - `BENCH_CHECK.md`, two additions and nothing else touched

A **step 0** before "plug the radio in", telling the owner to open About and read
the version against 1.12.38, with the measured reason why that step now exists and
an explicit statement that what to do about an older number is his call and not an
instruction to reinstall anything. And a **step 10**, the two files to look at
afterwards, each with what its *absence* means now that both writers have been
measured. Every added line says whether it was measured tonight or is predicted.
Unit 226's existing text is untouched.

### Tasks 6 and 7

Five gates, run one after another, never overlapping, every count read from
`ResultSummary.Counters` in a TRX and never off a console. Root version
`1.12.37` -> `1.12.38` under HM-DEC-150, taken before the App channel gate because
`VersionTests` reads it. **`Ft8Sharp` did not move** - no file under
`src/Ft8Sharp/` changed. Six commits, named paths only, no `git add -A` and no
`git add .` at any point.

### What was left alone

Step 6 in its entirety, everything under `src/Ft8Sharp/`, step 5's criterion 3,
the `snr` column, the plain-English panel, the untracked debris at the repository
root, `tools/`, and the full Hamlet suite.

## 2. What the owner should expect

**Open About before you sit down at the radio.** That is the whole of what changed
for you tonight, and it is now step 0 of `BENCH_CHECK.md`. If the version on
screen is not 1.12.38, the Hamlet in front of you does not contain this phase's
work and nothing on the Digital tab will happen however good the band is. Every
version of Hamlet that has ever written a line on this machine is 1.12.0 or
older - the whole phase has been built in a tree whose output, so far as the
machine's own record goes, has never been run.

**After the session, look at two files** - `%AppData%\Hamlet\telemetry\<today>.jsonl`
and `%AppData%\Hamlet\captures\digital\`. `BENCH_CHECK.md` now says what each
one's absence means. In short: a Hamlet that runs writes the first one within
milliseconds, so its absence means the thing you ran was not this code; and the
second one has never existed on this machine, so if you press *keep the last 30
seconds* and it is still absent, the press refused.

**A refused press is no longer silent.** From 1.12.38 a press that produces
nothing writes a `warn` line into the log saying which of the four ways it
refused. Search a morning's file for `digital_capture_refused`. This is why unit
233's question to you - whether you pressed the capture button on 2026-09-03 - is
not being waited on: both answers now lead to the same place next time.

**Nothing on any screen changed.** No new sentence, no new column, no new panel.

**Two things you should not read into this.** Sensitivity is still short of the
published figure and step 6 still waits on your ruling; nothing tonight touched
the decoder. And nothing tonight has heard a radio - the writers were watched
working on a bench, which is a different claim from the phase goal.

## 3. What you should see

**THE VERDICT ON THE SINK: driven the way `App.axaml.cs` drives it, Hamlet's
telemetry writer DOES put a line on disk for today.** File `2026-09-03.jsonl`, one
line, verbatim:

```
{"ts":"2026-09-03T16:52:47.652Z","sessionId":"32f617cd","level":"info","appVersion":"1.12.38","category":"diagnostics","event":"app_start","data":{}}
```

`DroppedEventCount` was **0**. **Elapsed before the line appeared with no
`Dispose` ever called: 12 ms.** That is the number that matters, because an
application killed rather than closed never disposes - so the bench case is
covered, and a missing file cannot be explained by a process that exited badly.
With Diagnostics switched off through the same `AppSettings`, there was no line
and **no file at all**, and dropped stayed 0: the guard was watched refusing. A
default `AppSettings` was asserted directly to enable Diagnostics, which is the
branch `settings.json`'s empty `{}` takes.

**So the sink is not the fault, and the silence has to be explained somewhere
else.** The next table is where.

### Which builds of Hamlet have ever run on this machine (task 1)

Every distinct `appVersion` across all eight jsonl files, with the newest `ts`
carrying it. 2 897 lines, 0 unparseable.

| appVersion | lines | newest ts | in file |
|---|---|---|---|
| 1.0.0 | 948 | 2026-08-14T22:02:26.013Z | 2026-08-14.jsonl |
| 1.2.0 | 171 | 2026-08-15T17:51:39.229Z | 2026-08-15.jsonl |
| 1.2.7 | 50 | 2026-08-15T19:43:21.211Z | 2026-08-15.jsonl |
| 1.2.9 | 10 | 2026-08-15T20:12:30.428Z | 2026-08-15.jsonl |
| 1.3.0 | 22 | 2026-08-15T22:27:56.924Z | 2026-08-15.jsonl |
| 1.4.0 | 34 | 2026-08-15T23:59:22.328Z | 2026-08-15.jsonl |
| 1.4.1 | 42 | 2026-08-16T00:41:20.672Z | 2026-08-16.jsonl |
| 1.5.0 | 99 | 2026-08-16T01:09:36.490Z | 2026-08-16.jsonl |
| 1.5.1 | 96 | 2026-08-16T02:04:42.865Z | 2026-08-16.jsonl |
| 1.6.0 | 199 | 2026-08-16T13:53:56.432Z | 2026-08-16.jsonl |
| 1.8.1 | 18 | 2026-08-17T13:35:14.068Z | 2026-08-17.jsonl |
| 1.10.10 | 15 | 2026-08-22T21:28:19.610Z | 2026-08-22.jsonl |
| 1.11.23 | 25 | 2026-08-27T15:33:29.858Z | 2026-08-27.jsonl |
| 1.11.24 | 393 | 2026-08-27T17:08:01.835Z | 2026-08-27.jsonl |
| 1.11.25 | 77 | 2026-08-27T17:51:06.407Z | 2026-08-27.jsonl |
| 1.11.34 | 17 | 2026-08-28T14:55:48.168Z | 2026-08-28.jsonl |
| **1.12.0** | **73** | **2026-08-28T15:33:38.978Z** | **2026-08-28.jsonl** |

**THE NEWEST BUILD IN THE RECORD IS 1.12.0, WHICH IS OLDER THAN THIS TREE'S
1.12.37.** Thirty-seven patch versions - every unit from 225 onward, the
continuous slot watch, the per-slot census, the capture sheet's audio-path and
geometry blocks - have never been seen running on this machine.

I am not asserting that the owner ran an old build on 2026-09-03; the record for
that day does not exist, so it cannot say. What the record does say is that **no
build carrying this phase's work has ever written a line here**, and that is the
cheapest explanation available for a morning that produced nothing on screen,
nothing in telemetry and nothing in captures all at once. It is item 1 of section
4 and the reason step 0 was added to `BENCH_CHECK.md`.

### The rest of the trace (task 1)

Every file in `%AppData%\Hamlet`, sizes and last-write times in UTC:

| entry | bytes | last written UTC |
|---|---|---|
| `layouts.json` | 819 | 2026-08-27T17:08:01Z |
| `scan-segments.json` | 4 601 | 2026-08-17T23:40:52Z |
| `settings.json` | 1 353 | **2026-09-03T16:35:55Z** |
| `spots.db` | 716 800 | 2026-08-28T15:33:39Z |
| `spots.db-shm` | 32 768 | **2026-09-03T16:35:53Z** |
| `spots.db-wal` | 370 832 | 2026-09-03T12:29:17Z |
| `telemetry\2026-08-13.jsonl` | 84 605 | 2026-08-13T23:19:03Z |
| `telemetry\2026-08-14.jsonl` | 109 634 | 2026-08-14T23:55:13Z |
| `telemetry\2026-08-15.jsonl` | 69 122 | 2026-08-15T23:59:22Z |
| `telemetry\2026-08-16.jsonl` | 214 802 | 2026-08-16T13:53:56Z |
| `telemetry\2026-08-17.jsonl` | 8 848 | 2026-08-17T13:35:14Z |
| `telemetry\2026-08-22.jsonl` | 7 507 | 2026-08-22T21:28:19Z |
| `telemetry\2026-08-27.jsonl` | 144 592 | 2026-08-27T17:51:06Z |
| `telemetry\2026-08-28.jsonl` | 36 910 | 2026-08-28T15:33:38Z |

- **`captures` does not exist. `captures\digital` does not exist.** Unit 233's
  measurement is confirmed against the tree tonight.
- **File for 2026-09-03: no. Any file after 2026-08-28: no.**
- Line counts, oldest to newest: 473, 601, 161, 436, 18, 15, 495, 90.
- **`settings.json` and `spots.db-shm` were written at 16:35:55Z and 16:35:53Z on
  2026-09-03** - two seconds apart, and about fourteen minutes before the trace
  ran. Something opened the spots database and saved settings that day. Whatever
  it was left no telemetry line, and after task 2 that is a statement about the
  process, not about the sink.
- Last five lines of the newest file (`2026-08-28.jsonl`), verbatim, **no callsign
  in any of them** - the last is `app_stop`, the four before it are
  `decode_quality` and `rig_heartbeat` at `appVersion 1.12.0`, carrying counts and
  decibels only.
- `settings.json`: `TelemetryCategories: {}` and `TelemetryMaxMegabytes: 50`,
  verbatim. The empty object is the branch that reads as *every category on*.
- **`ft8` events in any jsonl, of any kind: 0.** Every decode event in the whole
  folder is CW.

### The capture verdict (task 3)

**The digital capture write path works, and tonight is the first time it has been
watched working anywhere.** Pointed at a temporary folder and driven through
`CaptureDigitalCommand`:

- `captures\digital` was **created by the press** - asserted absent before it and
  present after.
- `ft8-2026-09-03-165722.wav`, **720 044 bytes**, and `ft8-2026-09-03-165722.txt`,
  **2 061 bytes**, both present, names paired by stamp.
- The WAV read back through `WavAudio.Read` as **360 000 samples at 12 000 Hz**,
  which is what the tap held.
- The sheet is **46 lines** and carries unit 226's blocks and all three of unit
  233's - the audio path (`device`, `audioIsReal`, `windowsMuted`), the slot
  geometry, and the census. Measured from the sheet itself:

```
  slot     2026-09-03 16:57:00 UTC  whole transmission inside the audio
  slot     2026-09-03 16:57:15 UTC  CUT SHORT: the audio ends before the transmission does
census     1 slots, counts below
  slot     2026-09-03 16:57:00 UTC  candidates 5  parity 0  checksum 0  text 0  duplicate 0  at 12000 Hz  top Costas match counts 20, 16, 15
```

- `CaptureFolder` was restored in a `finally` and **asserted back** afterwards.

### A refused press leaves a line (task 4)

All four paths were forced for real - not mocked - and each was read back off
disk. The two exception paths were provoked by putting a file where the `digital`
folder has to go, and a directory where the WAV has to go.

```
{"ts":"...","level":"warn","appVersion":"1.12.38","category":"decode","event":"digital_capture_refused","data":{"reason":"NothingIsListening"}}
{"ts":"...","level":"warn","appVersion":"1.12.38","category":"decode","event":"digital_capture_refused","data":{"reason":"NoAudioYet"}}
{"ts":"...","level":"warn","appVersion":"1.12.38","category":"decode","event":"digital_capture_refused","data":{"reason":"IOException"}}
{"ts":"...","level":"warn","appVersion":"1.12.38","category":"decode","event":"digital_capture_refused","data":{"reason":"UnauthorizedAccessException"}}
```

**Exactly one line per press** on every path, asserted. The `data` object was
asserted to hold **exactly one key**, and the whole line asserted to contain no
backslash - so no path, no exception message, no free text of any kind.

### The gates

Every count read from `ResultSummary.Counters` in a TRX. Run one after another,
never overlapping, never blocking the shell.

| Gate | total | executed | passed | failed | skipped | failing set |
|---|---|---|---|---|---|---|
| `Ft8Sharp.Tests`, whole | 524 | 523 | 523 | 0 | 1 | **EMPTY** |
| Channel, `Hamlet.RadioEngine.Tests` | 38 | 38 | 38 | 0 | 0 | **EMPTY** |
| Channel, `Hamlet.App.Tests` (after the bump) | 9 | 9 | 9 | 0 | 0 | **EMPTY** |
| Changed code, `Hamlet.App.Tests` | 20 | 20 | 20 | 0 | 0 | **EMPTY** |

The one skip in `Ft8Sharp.Tests` is `Ft8TableGenerationTests.RewriteTheCheckedInTablesFile`,
the table-writing gate that is skipped by design. Nothing under `src/Ft8Sharp/`
changed tonight, so that gate is regression insurance and it held.

The changed-code gate names five classes: `TheSinkWritesWhenDrivenLikeTheAppTests`
(4, new), `ThePressActuallyWritesItsCaptureTests` (2, new),
`ARefusedPressLeavesALineTests` (4, new), `CallsignPrivacyTests` (5, extended) and
`EverySlotLeavesALineTests` (5, unchanged, run because it walks the same surface).

**No new red, and the inherited failing set is unchanged: it is empty in every
gate this unit runs.** The two standing CW failures recorded in `BENCH_CHECK.md`
are outside all four filters and were not run, which is the plan's ruling on what
a unit runs and not a claim about them.

### Attribution, and the reduction that is not claimed

`git diff --name-only 2828ab6..HEAD` gives **244 paths, of which 31 are under
`src/Hamlet.*` or `tests/Hamlet.*`** - unit 233 measured 240 with 27, so this unit
added four. **The plan's attribution reduction does not apply to step 7 and is not
claimed.** Step 7 is by construction the step that reaches Hamlet's code. The
honest substitute is used instead: every Hamlet path this unit added or touched,
named, with the tests run over the changed code.

Added: `src/Hamlet.App/Telemetry/DigitalCaptureRefusal.cs`,
`tests/Hamlet.App.Tests/Telemetry/TheSinkWritesWhenDrivenLikeTheAppTests.cs`,
`tests/Hamlet.App.Tests/Telemetry/ThePressActuallyWritesItsCaptureTests.cs`,
`tests/Hamlet.App.Tests/Telemetry/ARefusedPressLeavesALineTests.cs`.
Touched: `src/Hamlet.App/Telemetry/AppEvents.cs` (one method added),
`src/Hamlet.App/ViewModels/MainWindowViewModel.cs` (four call sites in
`CaptureDigital`), `tests/Hamlet.App.Tests/Telemetry/CallsignPrivacyTests.cs` (the
walk, 64 to 65). Outside Hamlet: `Directory.Build.props`, `BENCH_CHECK.md`,
`PROJECT_STATUS.md`, `PHASE_STATUS.md`.

### The push

**`c3f10a1..443aaf7` on `main`, pushed and confirmed.** Six commits, one per task,
named paths only:

| commit | task |
|---|---|
| `83bc502` | 1 - the trace and the appVersion census |
| `0533cc8` | 2 - the sink watched writing |
| `5b7118c` | 3 - the capture watched writing |
| `914048f` | 4 - a refused press leaves a line |
| `30c900b` | 5 - `BENCH_CHECK.md` step 0 and step 10 |
| `fca61c8` | 7 - root patch to 1.12.38 |
| `443aaf7` | 6 - the gates |

The validator could not be executed from this session; what was done instead is
item 5 of section 4.

## 4. What's blocking us

Five items. **One is a prompt I am asking the owner to act on; four are records.**

**1. The build the owner runs has never been one this phase produced. THIS IS THE
ONE I AM ASKING YOU TO ACT ON, and it is not a ruling request - it is a
prompt to check.** The newest Hamlet that has ever written a line on this machine
is 1.12.0. This tree is 1.12.38. I cannot see how you build or install Hamlet and
no unit has touched it, so I cannot tell you whether the icon you click points at
this tree's output. What I can tell you is that if it does not, every artefact of
units 225 to 234 is invisible to you and the bench check cannot succeed. Step 0 of
`BENCH_CHECK.md` now asks you to read the version off the About box before you
start. **This does not block a step 7 criterion** - all five are met against the
code - but it is squarely in the way of the phase goal being *observed*.

**2. A capture press decodes nothing until the SNTP clock query returns.
Recorded, not a ruling request.** Driving the press for task 3 turned this up:
`MainWindowViewModel.ClockOffset` starts Unknown and is set by a background time
query, and `Ft8SlotCutter` refuses to cut any slot against an unmeasured clock.
The first run of my own test refused and the second decoded, purely on whether the
query had landed; I fixed the offset in the test so the branch is a decision
rather than a coin toss. **This is correct behaviour, not a defect** - HM-DEC-009
says an unmeasured clock refuses rather than guessing, and `BENCH_CHECK.md` part 3
already tells the operator to wait a minute for the first query. I record it
because *a press in the first seconds after start-up produces a capture with no
census in it*, and nobody had written that down. No task of mine covered it and I
changed nothing about it.

**3. `%AppData%\Hamlet` was read, never written. Recorded.** Task 1 is a listing
and a set of file reads. No probe file, no folder, nothing created. Whether that
folder is writable is therefore still an inference - from the eight files already
in it and from task 2 writing freely to a temporary folder - and not a
measurement. That was the arbiter's instruction and I followed it; I note it so
nobody later reads "the sink writes" as "the sink writes *there*."

**4. The scratch trace file is untracked and I could not delete it. Recorded.**
`tests/Hamlet.RadioEngine.Tests/Audio/Unit233ScratchTraceTests.cs` was overwritten
rather than orphaned, as instructed, and is left untracked and never staged - but
`rm` and `git clean` are outside this unit's permission scope, so it is still
there, now holding unit 234's harness instead of unit 233's comment. It can be
deleted by hand. The same is true of `.unit234\task1-trace.txt` and
`TestResults234\`, which are this unit's own scratch output.

**5. `validate-output.bat` could not be run and exit 0 is NOT claimed.
Recorded.** Every form of invoking it from this session was refused by the
sandbox - `cmd /c`, `cmd //c`, the bare path, and running it in the background -
and so was `powershell -File`. Unit 233 hit the same wall from the other side
(cmd could not find unit 228's shim) and it is now two units running. **I am not
reporting a validator result I did not get.**

What I did instead is below. It is a hand-run of the six rules, using the file's
actual content and the same expressions the script's own body uses, and it is
weaker than running the script for exactly the reason the script's header
argues - a second copy of the rules read by the same reader is one check wearing
two coats. Treat it as a self-check, not as the gate.

**Nothing else is raised.** Step 6 was not measured, not argued and not touched.
Nothing under `src/Ft8Sharp/` changed, so `Ft8Sharp`'s version did not move under
HM-DEC-152.

### The six rules, hand-checked against this file

| rule | what the script measures | measured here |
|---|---|---|
| 1 | a `UNIT:` line above section 1, parseable | present, line 29; section 1 opens at line 47 |
| 2 | the four top-level sections, in order, exact names | the only `## ` lines are 1, 2, 3, 4 at lines 47, 132, 162, 365, spelled as the script's `WANT` string |
| 3 | no fifth top-level section | four `## ` lines and no more |
| 4 | section 4 present even when empty | `## 4. What's blocking us` present, with a plain ASCII apostrophe, which is what the script's `findstr /b /c:` needs |
| 5 | section 3 non-empty | 167 non-blank lines between the section 3 and section 4 headings |
| 6 | ordering block above `UNIT:`, A B C, and C naming a count | `READ IN THIS ORDER` ×1, `^A.` ×1, `^B.` ×1, `^C.` ×1, and `raises 5 items` ×1, all inside the first 60 lines |

All six read as passing. **The script itself was not run, so the unit does not
claim its exit code**, and if that is disqualifying under the standing rule then
it is disqualifying - saying so is the point of this item.
