**PROJECT: Hamlet**

# Standing shack facts — read before advising anything at the radio

Recorded 2026-08-17 at the operator's instruction, after three separate
sessions advised changing radio settings that were already correct.

**FACT-004 added 2026-09-03**, after two units spent a night deriving a
fault from an empty capture folder on a machine that has never had a radio
attached to it.

---
id: FACT-001
status: standing
source: operator statement, 2026-08-17; standing for days prior
---

The IC-7300's CI-V USB configuration is, and remains:

- **CI-V USB Port: Unlink from [REMOTE]**
- **CI-V USB Baud Rate: 115200**

These are not going to change. No session may advise setting, re-checking,
or walking to the radio to confirm them. They are ground truth.

**Verification, when a session needs it, is silent and by wire only:**
read `1A 05 0074` (Full Manual p. 19-5; 00=Link, 01=Unlink, read-only) and
expect 01; confirm the host port is open at 115200 from ISerialPort's own
report. If either check fails, that is a *finding about the link or the
code* to be reported as a measurement — not a reason to ask the operator
to visit a menu.

---
id: FACT-002
status: standing
refs: Full Manual p. 19-7/19-8 (command table, footnote *4)
---

Citations resolved for HM-OPEN-013, so no session rediscovers them:

- `1A 05 0074` — "Send/read the CI-V USB port setting (00=Link to
  [REMOTE], 01=Unlink to [REMOTE]) (Read only)". Full Manual p. 19-5.
- `27 11` (Scope wave data output) carries footnote *4: settable only
  when CI-V USB Port = Unlink from [REMOTE] **and** CI-V USB Baud Rate =
  115200. Per FACT-001 both conditions are already satisfied on this
  radio.
- Footnote *2, same table: in CW mode with break-in ON, text sent from
  the PC is transmitted as CW. Standing caution for any future write
  path.

---
id: FACT-003
status: standing
---

Consequence for the dark waterfall: **the radio-settings hypothesis is
eliminated.** The fault lies in Hamlet or on the wire, and diagnosis
proceeds down-chain in this order, each step observable in telemetry:

1. Was `27 11 = 01` actually sent on the last connected run, and what
   came back — OK, NG, or nothing? (scope_output_requested event,
   CivLinkHealth unanswered count)
2. Readback of `27 11` after the write — does the radio now report
   output on?
3. Frame counter — how many `27 00` waveform frames have ever been
   received? Zero frames with output confirmed on is a link/parse
   question; nonzero frames with a black display is a render question.
4. Parse — first real frames against CivScope's 11-part handling
   (real frames have never been seen; the parser is tested only
   against constructed ones).
5. Render — frames parsed but nothing drawn.

A session reporting "the waterfall works" without a nonzero received-
frame count from a connected radio is making the claim §0.0 now
forbids a display to make.

---
id: FACT-004
status: standing
source: operator statement, 2026-09-03; standing since the project began
---

**There are two computers, and only one of them has a radio on it.**

- The **development machine** holds the repository, the toolchain and the test
  suite. **No radio has ever been attached to it.** Hamlet has never captured
  audio off the air on this machine and never will while that remains true.
- The **shack machine** is where the IC-7300 is connected over USB, where the
  bench check at 14.074 is performed, and where every capture, sidecar and
  telemetry line about real air has ever been written.

**Consequences, and no session may reason past them:**

- **An absent capture folder, an empty telemetry file, a missing sidecar or a
  capture count of zero on the development machine is the expected state.** It
  is not a finding, not evidence of a defect in the capture path, and not a
  reason to author a unit. Reading it as one is FACT-003's mistake in a new
  place: eliminating a hypothesis that was never live.
- **No session may derive a fault from the development machine's filesystem**,
  nor propose a measurement whose result is the presence or absence of captures
  on it.
- **No measurement of the development machine's audio endpoints says anything
  about the radio.** The IC-7300's USB codec is not present on this machine, so
  what format tag, channel count, sample rate or encoding it declares is
  **unknown from this side and may not be inferred**. A unit that clears or
  implicates an audio-path defect by enumerating this machine's capture devices
  has measured the wrong hardware and its conclusion does not stand.
- **Which machine a piece of evidence came from is part of the evidence.** A
  report that does not say is incomplete, and a claim about the bench check
  drawn from the development machine is a claim about nothing.

**Verification, when a session needs it:** whether a capture, a log line or a
device exists at the radio is a question about the shack machine, and only the
operator can answer it. Ask him, or say the answer is unknown. Do not answer it
from the tree.
