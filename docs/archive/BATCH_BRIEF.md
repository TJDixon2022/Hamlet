**PROJECT: Hamlet**

# Batch work order: honest CW detection, then band scanning, then auto-CQ

Three sessions, in this order. Each depends on the one before it, and the
order is not negotiable: a scanner built on a broken tone detector tours the
band and stops on nothing, and an auto-CQ cycle that decides "someone
answered" from a broken decoder transmits over the top of the person
answering.

`CLAUDE.md`, `SHACK_FACTS.md`, `DECISIONS.md`, `OPEN_ISSUES.md` first; §9.5
governs. Pull current versions of every file before editing. Session 3 is the
first work in this project's history that puts RF in the air; its rulings are
drafted below and want the operator's acceptance before code.

## The two machines

| | Development computer | Ham computer |
|---|---|---|
| Serial ports | **COM1 only — a simulated radio** | **COM3 — the real IC-7300** |
| What it establishes | code facts, logic, fixtures | **whether any of it works** |

Detect which machine you are on by enumerating serial ports; state it in your
first report line. COM1 is a simulator, not evidence about the radio. COM3 is
exclusive — Hamlet and `tools/Hamlet.ScopeCheck` cannot both hold it.
HM-DEC-093 stands: no session may report a streaming or transmitting feature
working without a measurement from the real radio.

**Before session 1 can start, HM-OPEN-015 must be closed** — these files must
be present on the development machine: `CW_RECEIVE_BRIEF.md`, `cwdecoder.py`,
the 13:47 interference capture, the 22:58 pair, one of the 23:26 group. The
prior session correctly refused to guess at the validated chain's constants
from a prose summary. If they are still absent, say so and stop.

---

# SESSION 1 — the decoder, and honest tone detection

This is the blocking work. Everything else in the batch is built on it.

## 1.1 The tone detector reports the wrong frequency

| Capture | Hamlet reports | Actual strongest tone |
|---|---|---|
| 23:26 group | 575 Hz | 595 Hz |
| 01:33 | 600 Hz | 612 Hz |
| 01:36 | 575 Hz | 612 Hz |
| 13:47 | 550 Hz | 501 Hz |

Never right, off by 11 to 49 Hz, and in the last case reporting a figure that
is neither the real tone nor the configured 600 Hz pitch — it looks like a
pull toward the pitch rather than a measurement of the audio. Find out
whether the configured pitch is contaminating the measurement.

## 1.2 Loudest is not keyed

In the 13:47 capture the 501 Hz tone is 37 dB above the noise and
**continuously on** — a carrier or birdie, not Morse. Any detector picking
the strongest bin locks onto it and stays there.

Score candidates on **keying structure, not amplitude**: on/off contrast,
duty cycle in a plausible range, and element durations that cluster around a
dit and a dah in roughly 1:3 ratio. A continuously-on bin is disqualified as
a CW candidate however strong it is, and is reported as interference (1.4).

**Do not gate the character path on the detector as currently built.** In the
23:34 capture the detector said `toneHz none` while the decoder resolved
`IIGNAL HI`, legible English. Gating on a broken detector trades phantom
output for deafness. Fix the detector first, then gate.

## 1.3 The validated receive chain

Independent analysis of the 01:33 capture decoded a real answering station
that Hamlet returned nothing on: **615 Hz, dit 106 ms, dah 283 ms (ratio 2.8,
≈11.4 WPM)**, inter-element gaps **60–70 ms — shorter than its own dits**,
character gaps 112–155 ms, word gaps 220–320 ms, envelope SNR 12–20 dB in a
20 Hz detection bandwidth but only **0–5 dB in the radio's 500 Hz passband**,
QSB ~0.7 Hz at up to 25 dB depth. `cwdecoder.py` is the validated semantic
reference; port its behavior into the existing Goertzel chain, not its
structure.

- **Two-stage Goertzel.** Acquire across 300–900 Hz scoring by envelope
  spread; then a fine bank of bins 5 Hz apart, 50 ms window, 10 ms hop
  (~20 Hz ENBW) centered on the winner, re-centered when the peak walks.
  Worth ~14 dB over the passband — the difference between nothing and copy.
  Widen to ~40 Hz ENBW before clock lock or above ~18 WPM.
- **Threshold from clustering, referenced to the fade**: two-means fit per
  ~3 s window, 6 dB hysteresis, de-glitch 20 ms before clock lock and
  0.4 × dit after. Refuse to gate when cluster separation is under 6 dB.
- **Clock proven, not assumed**: two-means on mark durations, accept only if
  the dah/dit ratio lands in 2.5–3.8; classify the three gap lengths by
  clustering the gaps themselves, never by fixed dit multiples — that is what
  makes a fist with 65 ms gaps and 105 ms dits readable.
- **Transmit-mute guard.** The operator's own full-break-in transmission
  arrives as 50–84 dB audio mutes with ~24 ms of T/R hang, and destroyed the
  gate's trackers before the answer arrived (1,211 elements of chatter).
  Detect broadband RMS collapse below −60 dBFS, freeze floor and peak, hold
  150 ms after recovery, clamp the floor at −75 dBFS so it can never chase
  digital silence.
- **Truncated evidence is not evidence.** Any mark bordering a frozen span is
  excluded from the clock fit and rendered as a placeholder, never a letter.
  Without this rule the slivers audible between the operator's own elements
  decode as a confident string of E and T — the most seductive wrong output
  this feature can produce.

## 1.4 Name the interference

A steady carrier inside the 500 Hz filter is an operational problem, not a
curiosity: AGC rides the loudest thing in the passband, so it sets the
receiver's gain and suppresses everything quieter — with AGC on FAST making
it worse. Report it as measured, at the frequency measured, and offer the
fixes (all tier 1 receive-side writes):

- Move the dial a couple of hundred hertz so the carrier falls outside the
  filter.
- **Manual** notch on the carrier: manual notch function `16 48` (00=OFF,
  01=ON), `[NOTCH]` position `14 0D` (00 00 = max CCW, 01 28 = center,
  02 55 = max CW). Manual, not automatic — auto notch (`16 41`) hunts and
  would eat the Morse too.
- Twin PBT to move the carrier out while keeping the signal in: inner
  `14 07`, outer `14 08`, same 0–255 encoding. **`14 08` is the outer PBT** —
  the exact sub-command this project once mistook for CW pitch, which is
  `14 09`. All verified in the Full Manual command table.

§0.0 applies: do not claim to know whose carrier it is or that removing it
will fix the copy.

## 1.5 Fixtures and done

Commit the 22:58 pair and 01:33 as decoder fixtures; 13:47 as the
**interference** fixture (strong steady carrier, CW audible by ear that no
automatic analysis has yet located); keep one of the three 23:26 files, which
are one recording written three times.

Done: reported tone frequency matches independent measurement on all five
captures; the 501 Hz carrier is reported as interference, not a CW candidate;
the 01:33 station decodes; and **01:36 produces low-confidence or placeholder
output rather than a clean sentence — a test that the refusals fire is as
load-bearing as a test that the decode succeeds.**

**Be careful here.** The operator heard CW in the 13:47 capture that
independent analysis could not find. Human copy at low signal-to-noise beats
automatic detection, and an analysis finding nothing is not evidence that
nothing is there.

---

# SESSION 2 — the band scanner

One button: sweep the CW segment, stop on conversations. Built only after
session 1, because the stopping decision is the decoder's.

## 2.1 What the waterfall can and cannot tell you

The scope delivers ~4.5 sweeps/second. A dit at 20 WPM is 60 ms; seeing
elements would need ~30 sweeps/second. **The waterfall cannot identify
Morse** — it aliases the keying completely. What it can do, over a 10–30 s
window per bin, is measure occupancy and variability: a steady carrier is
high amplitude with low variance; an operator sending is intermittent
presence at roughly 40–70% duty; empty spectrum is flat. The scope span is
~500 kHz against a 500 Hz receive passband, so the waterfall surveys about a
thousand times more spectrum than the operator can hear. It hands the scanner
a ranked work queue instead of a linear crawl.

## 2.2 Architecture: the waterfall proposes, the audio decoder confirms

Accumulate per-bin statistics from the sweeps already being parsed. Rank bins
by intermittency, explicitly demoting steady carriers (which 1.4 reports as
interference anyway). Dwell on each candidate long enough for the real
decoder to run — 10–20 s is roughly two CQ cycles — and score what came back.
Stop on something that decodes; log and move on if not. Bin-to-frequency
mapping comes from the sweep header parsed in the scope work.

## 2.3 The stopping classifier

"A conversation" in decoder terms is a callsign-shaped token, `DE`, `CQ`,
`K`, `73`, or a pattern repeated across the window. A scanner that stops on
`CQ` is worth ten times one that stops on "there's a tone here." Carry
confidence through: stopping on a 0.3-confidence maybe-CQ must look different
on screen from stopping on a clean one.

## 2.4 Scanning tunes the radio — draft ruling for acceptance

Changing the VFO is a new category: it moves the operator's dial out from
under him. Rules:

- Never while transmitting.
- Remember the starting frequency and restore it on stop.
- Stay inside a band-plan segment **the operator configures in a data file he
  edits** — never frequencies asserted from a model's memory.
- Abort instantly on: the operator touching the dial or PTT, rig state going
  unknown or stale, or the link failing to answer.
- Refuse to start unless `RigStateMonitor.Populated` is satisfied — the same
  gate added after the third instance of the connect race.
- One obvious, always-visible stop control.

---

# SESSION 3 — auto-CQ

Repeating CQ at a configurable interval (default 30 s), stopping when someone
answers. **The first work in this project that transmits.** Decisions already
taken by the operator, recorded here as requirements:

## 3.1 Keying method: command `17`, the radio's own keyer — RULED

Hamlet sends one framed CI-V command carrying message text; the radio
generates the CW at its configured keyer speed (`14 0C`). **Host-timed keying
on DTR/RTS (`00 79`) is rejected**: it makes the host responsible for
continuous control of a transmitter it cannot guarantee it will be alive to
release, and this project has already seen RF knock USB devices off the bus.
A stuck carrier on a shared band under the operator's callsign is the failure
mode §0.0 exists to prevent. With `17`, the radio owns all timing and
malformed elements are physically impossible; the worst case is one truncated
message already in flight.

Verified against the Full Manual, p. 19-13: command `17` sends **up to 30
characters**; permitted characters are 0–9, A–Z, a–z, and `/ ? . - , : ' ( )
= + " @` and space; **`FF` stops sending**; `^` transmits a string with no
inter-character space. Footnote \*2 (p. 19-8): in CW mode the message
transmits as CW code when TRANSMIT is on, an external TX switch is on, or
break-in is on.

**That footnote is an interlock, not a caveat.** The operator's break-in
setting is a physical arming switch. Read `16 47` before every send; if
break-in is off, say so plainly and do not transmit — silent non-transmission
must never look like success.

## 3.2 Response detection: tiered — RULED

- **QSO-shaped text stops the cycle outright**: the operator's own callsign,
  `DE` plus a callsign-shaped token, `K`, `R`, `73`, or a repeated pattern.
  Report *why* it stopped — "heard KC3QIS" beats "heard something."
- **Confident-but-unrecognized text suspends** the next transmission and
  shows what was heard, awaiting resume or stop.
- These are different claims and must look different on screen.

Missing a real answer is worse than stopping on noise: the other operator
hears CQ over the top of their reply. Bias toward stopping.

Supporting rules: **the listen window must not run during transmit** — the
mute guard means own-QSK produces exactly the truncated-evidence garbage that
would false-trigger this, so listening starts after T/R recovery. And the
cycle **stops after N unanswered rounds**, configurable, default 10.

## 3.3 Arming and stopping: two-stage plus dead-man reads — RULED

- **Arm** is a distinct step from **start**, and displays what will be sent,
  on what frequency, at what power, how many rounds, plus break-in state and
  rig-state readiness. Consent is an explicit act against displayed facts.
- **Stop**: one large always-visible control, plus Escape.
- **Automatic stop** on any of: break-in reading off; transmit status stuck on
  longer than one message; rig state unknown or stale; the operator touching
  the dial or PTT; round limit reached; a response detected.
- **Dead-man reads between rounds**: re-read break-in (`16 47`) and transmit
  status (`1C 00`). **If either read fails to answer, stop.** Silence is a
  stop, never a licence to continue on stale state. Spurious stops are the
  correct failure direction.
- Refuse to start unless `RigStateMonitor.Populated` — a write fired 0.8 s
  after connect against forty fields of `provenance: unknown` is the same
  race with a transmitter attached.

## 3.4 Two rules regardless

- **Every transmission is logged**: timestamp, frequency, message, round
  number. An audit trail of what the operator's callsign put on the air.
- **The message text is the operator's**, stored in config. No session may
  ever invent the content of a transmission that goes out under his
  callsign.

## 3.5 Scanner and auto-CQ are mutually exclusive — RULED

Arming one disables the other, with a message saying why. The scanner tunes
the VFO and auto-CQ transmits on it; concurrency means transmitting mid-tune
on a frequency neither component believes it is on, and during transmit the
scope survey is meaningless because the operator's own signal is the
strongest candidate in it.

**Record as an open issue, designed but not built**: the sequential handoff —
auto-CQ exhausts its rounds, stops transmitting, the scanner picks the next
quiet candidate from waterfall statistics, tunes, and auto-CQ re-arms there
(with or without per-hop consent). That is the real goal, and it wants both
components proven separately first, plus real band data to check whether the
candidate ranking matches the operator's own judgment.

---

## Definition of done for the batch

Session 1: the five-capture criteria in 1.5, including the refusal test.
Session 2: on COM3, the scanner tours a configured segment, stops on
something that decodes, restores the starting frequency, and the operator
agrees the candidate ranking is sensible against what he sees on the
waterfall.
Session 3: on COM3, the operator arms, watches one cycle transmit and one
response stop it, and confirms every stop condition in 3.3 by exercising it —
including pulling the USB cable mid-cycle and seeing the dead-man fire.

Rulings to record: the tone detector scoring keying rather than amplitude;
interference detection as first-class; the transmit-mute guard and
truncated-evidence rule; VFO writes for scanning; transmit writes for
auto-CQ, naming command `17` and the rejection of host-timed keying. Commit
in §7 format and push.
