# Standing shack facts — read before advising anything at the radio

Recorded 2026-08-17 at the operator's instruction, after three separate
sessions advised changing radio settings that were already correct.

**Every session reads this before writing a word about the radio's own
menus.** A fact here is ground truth and outranks any inference a session
draws from its own reading.

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
source: this session, 2026-08-17
refs: HM-DEC-093
---

**The tool that answers FACT-003 exists**: `tools/Hamlet.ScopeCheck`.

```
dotnet run --project tools/Hamlet.ScopeCheck -- COM3
```

It reads `1A 05 0074`, reports the host's own rate, asks for `27 11 = 01`,
reads it back, listens for waveform parts, and prints the six numbers with
the address of the first zero. It puts `27 11` back as it found it and
keys nothing.

**It advises nothing about the radio's menus**, per FACT-001. A CI-V USB
port reading that contradicts FACT-001 is reported as a finding about the
reading — either the sub-command is wrong or the link is answering for
something else — and never as an errand.
