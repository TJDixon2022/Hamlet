**PROJECT: Hamlet**

# Work order: auto-CQ, into a dummy load

Six phases. Reported per §12.2: four sections, **written to `OUTPUT.md` at the
repository root, overwriting it**, and printed to the session as well. **Name
the branch in section 1** (§9.5.1 — `main`, and nowhere else).

**Read first:** `CLAUDE.md` (§0.0, §0.2, §0.2.1, §12), `SESSION_PROTOCOL.md`,
`SHACK_FACTS.md`, the previous `OUTPUT.md`, `OPEN_ISSUES.md`, `DECISIONS.md`.
Then read `CLAUDE.md` §0.2 again, in full, before writing anything.

**This is the first work in this project's history that keys a transmitter.**
Everything before it was read-only or a receive-side write. Read §0.2 and
HM-DEC-098 as governing text, not as background.

## The standing limit on this whole order

**HM-DEC-098: an automated transmit cycle is built and exercised into a dummy
load only.** It does not reach an antenna on the strength of this order. Whether
§0.2's first sentence — *no unattended transmission* — is ever amended to permit
an attended automatic cycle on the air is **a separate ruling Tim takes after
watching every interlock fire into the load**, including the USB link pulled
mid-cycle. HM-DEC-008 stands beside it unchanged.

Nothing in this order transmits during a test run. Every test drives the keying
path through a fake that records what would have gone out.

## Standing instruction

A phase needing a ruling records the question in `OUTPUT.md` section 4 and
continues. §12.1 unchanged, and **anything touching §0.0, §0.2, transmit, or
what the display asserts is Tim's without exception** — which on this order is
most of it. When in doubt, ask rather than record.

The suite stands at 1845 tests, three failing. Two of them —
`ClearingTheTranscriptLeavesTheDecoderAlone` and
`TheBulletinDecodesToItsAnswerKey` — are settled positions, not work. The third
is handled in phase 6.

---

## Phase 1 — the keyer, and nothing else

**Keying is CI-V command `17`**, the radio's own keyer generating the CW at its
configured speed (`14 0C`). Full Manual p. 19-13: **up to 30 characters**;
permitted are 0–9, A–Z, a–z and `/ ? . - , : ' ( ) = + " @` and space; `^` joins
characters with no inter-character space, which is how a prosign is sent
properly; **`FF` stops a message in progress**.

Host-timed keying on DTR or RTS (`00 79`) is **rejected and stays rejected**: it
makes the host responsible for continuous control of a transmitter it cannot
guarantee it will be alive to release, and this project has already seen RF knock
USB devices off the bus. A stuck carrier on a shared band under the operator's
callsign is the failure §0.0 exists to prevent. With `17` the radio owns all
timing and malformed elements are physically impossible; the worst case is one
truncated message already in flight.

- **`0xFF` is the abort, and §0.2 already requires it**: every path that keys the
  transmitter has a same-thread, no-await abort available. Build the abort first
  and test it before building anything that sends.
- **Validate the message at edit time**, not on air: over 30 characters, or a
  character outside the permitted set, fails where the operator can see it.
  `CQ CQ DE KC3QIS KC3QIS K` is 25 — inside, but not by much. **A longer call
  needs two messages, which is a design question to raise rather than decide.**
- **Break-in is the arming interlock, not a caveat.** Footnote \*2 (p. 19-8): in
  CW mode a `17` message transmits only when TRANSMIT is on, an external switch
  is on, or break-in is on. Read `16 47` before every send; **if break-in is off,
  say so plainly and do not transmit.** Silent non-transmission must never look
  like success.

## Phase 2 — the cycle

Repeating CQ at a configurable interval, **default 30 seconds**, stopping when
someone answers.

- **The message text is the operator's**, stored in config. **No session may ever
  invent the content of a transmission that goes out under his callsign.**
- **The cycle stops after N unanswered rounds**, configurable, **default 10**, so
  an unattended app never calls into an empty band for an hour.
- **The listen window does not run during transmit.** The operator's own
  full-break-in transmission arrives as 50–84 dB audio mutes with about 24 ms of
  T/R hang, and the transmit-mute guard reads that as a muted receiver — which is
  exactly the truncated-evidence garbage that would false-trigger a response.
  **Listening starts after T/R recovery.**
- **Every transmission is logged**: timestamp, frequency, message, round number.
  An audit trail of what the operator's callsign put on the air.

## Phase 3 — response detection, tiered

**Missing a real answer is worse than stopping on noise**: the other operator
hears CQ over the top of his own reply. Bias toward stopping.

- **QSO-shaped text stops the cycle outright**: the operator's own callsign, `DE`
  plus a callsign-shaped token, `K`, `R`, `73`, or a pattern repeated across the
  window. **Report why it stopped** — "heard KC3QIS" beats "heard something".
- **Confident-but-unrecognized text suspends** the next transmission and shows
  what was heard, awaiting resume or stop.
- **These are different claims and must look different on screen** (§0.0).
- A callsign-shaped stop **names no callsign** (HM-DEC-073), as the scanner's
  already does not.

**Two signals worth using that the decoder already produces.** A tracker switch
to a new pitch and a clock loss are both annotated already, and operationally
both mean somebody else started transmitting. **That is a stronger indication
that a CQ was answered than any text classifier alone**, and it arrives sooner.
Use it as evidence, not as a verdict.

## Phase 4 — arming, stopping, and the dead man

- **Arm is a distinct step from start**, and displays what will be sent, on what
  frequency, at what power, how many rounds, plus break-in state and rig-state
  readiness. **Consent is an explicit act against displayed facts**, not a click
  on a button whose state the operator is inferring.
- **Stop**: one large always-visible control, plus Escape. Not in a widget he can
  scroll away from — the scanner's stop already lives in the pinned strip for
  this reason and this belongs beside it.
- **Automatic stop** on any of: break-in reading off; transmit status stuck on
  longer than one message; rig state unknown or stale; the operator touching the
  dial or PTT; the round limit reached; a response detected.
- **Dead-man reads between rounds**: re-read break-in (`16 47`) and transmit
  status (`1C 00`). **If either read fails to answer, stop.** Silence is a stop,
  never a licence to continue on stale state. **Spurious stops are the correct
  failure direction.**
- **Refuse to start unless `RigStateMonitor.Populated`.** A write fired 0.8
  seconds after connect against forty fields of `provenance: unknown` is this
  project's own history, and it is the same race with a transmitter attached.
- **Mutually exclusive with the scanner** (HM-DEC-098). Arming one disables the
  other with a message saying why. The scanner tunes the VFO and this transmits
  on it; concurrency means transmitting mid-tune on a frequency neither component
  believes it is on.

## Phase 5 — prove every interlock by breaking it

**Test each abort by simulating it, not by reasoning about it.** The scanner's
safety tests were verified by sabotaging the code — each guard broken in turn,
each producing exactly its own failure — and that is the standard here.

At minimum, each with its own named test: break-in off at arm; break-in going off
mid-cycle; transmit status stuck; rig state going stale; the dial moved; PTT
pressed; the round limit; a response detected; **an unanswered dead-man read**;
and **the serial link failing outright mid-cycle**.

**The last one is the one that matters most** and is the one HM-DEC-098 names
specifically: the operator will pull the USB cable mid-cycle and watch what
happens. Make that path exist and be tested before he does.

## Phase 6 — DROP THIS ONE IF SHORT OF ROOM

Record **HM-DEC-129**: HM-DEC-114's bar does not apply to `prosigns-easy`. Its
first character arrives at 7.44 seconds on a message running about four and a
half, so its opening is gone before the detector has found the signal — a
different claim from *a loud clean signal read wrongly*, and one **no real
station makes**, because a CQ repeats.

Mark the test as not asserting the bar, with the reason on its face so nobody
reads it as a bar being lowered. **The fixture is not edited** — that is the move
§12.5 exists to stop, and it would leave the survey defect untouched.

**HM-OPEN-033 — the cold-start bin choice — is scheduled, not closed.** Three
sightings now: the 400 Hz image, `prosigns-easy` at 675 on a 615 signal, and the
two-station recording from cold. HM-DEC-127's floor protects a station already
confirmed; **nothing protects the first choice.** It is its own work order and
this order does not begin it. Note HM-OPEN-034 beside it — a station at 350 hertz
is not read, fifty hertz off the bottom of the survey's range, pre-existing and
unfixed.

If dropped, say so.

---

**If every phase completes, stop and report. Do not begin HM-OPEN-033's work
order.**

## Definition of done

The cycle can be armed, sends into a dummy load, stops on every condition in
phase 4, and **every one of those conditions has been proved by breaking
something rather than by argument**. Nothing reaches an antenna. The message is
the operator's own text and the log says what went out.

**Everything here is provable on the development computer against the simulator,
and none of it is evidence about the radio** (HM-DEC-093). **Tim verifies on
COM3, into a dummy load, and the antenna question is a separate ruling he takes
afterwards.**
