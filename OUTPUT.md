# What Claude did

**Hamlet confirmed.** `Hamlet.sln` and `src\Hamlet.RadioEngine\Cw\CwGate.cs` exist;
`CoreHMI.sln` and `src\CoreHMI` do not. Development machine, **no radio**
(HM-DEC-093) — nothing in this report is evidence about the radio. Branch `main`,
**committed and not pushed**.

**Both tasks done. The defect was real.**

## Task 1 — the file is named for the evening

**What it did before, checked rather than assumed.** `CwCaseRoster.FileName` read
`$"cases-{atUtc:yyyy-MM-dd}.txt"` and `Append` handed it `one.AtUtc` — **the UTC
date, throughout.** This machine is on EDT, UTC−4, so an evening starting at eight
would have opened `cases-2026-08-19.txt` and silently begun `cases-2026-08-20.txt`
at eight o'clock local. The instruction's premise holds.

**What it does now.** The name takes the local date. Everything inside is
untouched: the `time` column is still UTC, still in the same position, still
formatted the same way.

**The line each file opens with**, above the column header:

```
# Evening of Wednesday 19 August 2026 at the rig, local time UTC-04:00. Every time below is UTC.
```

Four choices in it, and each is doing work. The **day name and long month** so no
reader has to decide whether `08-19` is August or the nineteenth. **"At the rig"**
so it is plain the date is about where he was sitting rather than about the air.
**The offset as it stood that night**, taken from the zone rather than a constant,
so somebody reading a January file is not left working out whether daylight saving
was in force. And a **leading `#`**, so a scorer can tell a note from a row without
having to understand the note.

**One clock names the file, the other stamps the rows, and the file says which is
which on its own first line** (HM-DEC-091). That was the part worth getting right:
two clocks on one sheet is exactly the fault that ruling exists for, and the
convention now lives in the file rather than in anybody's memory.

## Task 2 — the crossing is proved

**The seam is a `TimeZoneInfo` parameter on the roster and nowhere else.**
`FileName`, `Evening` and `Append` each take an optional zone that defaults to the
machine's. The application passes nothing, so it uses the shack's own clock; the
test passes a fixed UTC−4 zone, so it depends on neither the machine's timezone nor
the time of year. Nothing was added to the decoder, the tap or the view model.

`AnEveningThatCrossesUtcMidnightIsStillOneFile` presses at 23:30 UTC on the 19th
and 01:30 UTC on the 20th — half past seven and half past nine at the rig. It
**asserts the two stamps fall on different UTC dates before asking the roster
anything**, so the test cannot quietly stop crossing the boundary it is named for.
Then: one path, one file, named `cases-2026-08-19.txt`, four lines, the evening line
exact, and both rows still stamped in UTC and in the order they were pressed.

**Six tests in that file, all green. 2,008 tests, the same three red.**

**A mismatch to report, since it is the one place two of your instructions
collide.** Task 1 requires a line above the column header; task 2 says the existing
five tests stay green and are not rewritten. Adding a line necessarily moves every
line index below it, so **three assertions in two existing tests had to move** —
`lines.Length` from 3 to 4, the header from `lines[0]` to `lines[1]`, the two rows
from `lines[1]`/`lines[2]` to `lines[2]`/`lines[3]`, and the one-line-per-row sweep
now skips the note. **No test's substance changed and none was rewritten**; they
assert what they asserted before, one line further down, and they gained assertions
about the new line rather than losing any.

**A fragility I am naming and have not repaired** (§12.6).
`TheRosterIsOneFilePerEvening` uses 22:00 UTC and expects `cases-2026-08-19.txt`.
That is right on this machine and on any clock west of Greenwich, and it would fail
on a build agent east of about UTC+2. Repairing it means passing it the fixed zone,
which is rewriting a test the order says not to rewrite. **The new test is hermetic
and that one is not.**

**Nothing was recorded to `DECISIONS.md` this session.**

# What Tim should expect

- **Tonight is one file: `cases-2026-08-19.txt`**, and it stays that file when the
  clock passes eight o'clock and UTC rolls over. Nothing new appears at 20:00.
- **The file opens with a `#` line naming the evening**, then the column header,
  then your rows. **A scorer skips two lines now, not one.**
- **Every time in the rows is still UTC** — unchanged, deliberately. A row stamped
  `01:30:00` in a file named for the 19th was sent at half past nine your time, and
  the first line of the file says so.
- **WAV and sidecar filenames are unchanged** and still stamped UTC, so a row still
  matches its recording by name and yesterday's captures still line up.
- **No columns moved and `read` is still last and still empty.**
- **2,008 tests, 3 failing** — the same three, by name:
  `CwSettledSilenceTests.APassThatReadSomethingEmitsSomething`,
  `CwFarnsworthTests.TheBulletinDecodesToItsAnswerKey`,
  `CwTerminalTests.ClearingTheTranscriptLeavesTheDecoderAlone`. One test more than
  last session and the same three red.
- **No decoder behaviour changed.** `CwGate`, `CwSettledPass`, `CwToneSurvey` and
  `CwDecoder` are untouched, so tonight measures the decoder you have been running.
- **Committed, not pushed**, on `main`.

# What you should see

The first three lines of tonight's file, as they will appear — the note, the header,
and a row, printed by the test run rather than composed here:

```
# Evening of Wednesday 19 August 2026 at the rig, local time UTC-04:00. Every time below is UTC.
time      frequency  band   wav                       toneHz  snrDb  wpm  chars                 text                                               read
23:14:05  7.030      40 m   cw-2026-08-19-231405.wav  505     42.7   22   19 emitted, 6 unsure  N L D O T NET ■E ECH STATION HANDNG AHIS MESAGE P
```

**And the crossing, which is the whole of this unit**, from the new test:

```
# Evening of Wednesday 19 August 2026 at the rig, local time UTC-04:00. Every time below is UTC.
time      frequency  band   wav                       toneHz  snrDb  wpm  chars                 text            read
23:30:00  7.030      40 m   cw-2026-08-19-233000.wav  505     42.7   22   19 emitted, 6 unsure  CQ CQ DE W1AW
01:30:00  7.030      40 m   cw-2026-08-20-013000.wav  505     38.1   22   4 emitted, 0 unsure   K3XYZ
```

Half past seven and half past nine, one evening, **one file** — and before this
change the second of those two rows was in a file named for tomorrow, with nothing
anywhere saying it existed.

Tomorrow morning you open one file, and the count you take from it is the count for
the whole evening.

# What's blocking us

Nothing blocks tonight.

## Asks still outstanding

Seven, per HM-DEC-139 and scoped by HM-DEC-140.

| Ask | First made | Waiting on |
|---|---|---|
| **Whether an attended automatic cycle may reach an antenna** (§0.2, HM-DEC-098) | 2026-08-17 | The bench evening |
| **A callsign too long for one keyer send** (HM-DEC-130) | 2026-08-18 | The seam measured at the bench |
| **Whether the star asks for a name at the moment of saving** (HM-DEC-060, HM-DEC-134) | 2026-08-18 | Nothing but the ruling |
| **Whether Hamlet may ever ask the radio to send its spectrum** (HM-DEC-062, HM-OPEN-042) | 2026-08-18 | The ruling |
| **Whether HM-DEC-097 is satisfied by existing silence** (HM-OPEN-052) | 2026-08-19 | The ruling; a floor would be 19.8 |
| **Whether `ShortestVote` goes 5 to 7** (HM-OPEN-053) | 2026-08-19 | The ruling. **Carried forward unchanged**: 13 → 27 of 43 on the bulletin's leading edge against five synthesized tests, two on acquisition. Still held out so the instrument and the subject do not move together |
| **How the settled pass tells keying from a carrier** (HM-OPEN-054) | 2026-08-19 | The ruling; HM-DEC-143 is recorded and unbuilt until it has one |

Nothing was dropped from the queue this session.
