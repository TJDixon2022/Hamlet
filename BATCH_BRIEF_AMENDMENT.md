**PROJECT: Hamlet**

# Amendment to BATCH_BRIEF.md — session 1's remaining work, phased

Answers HM-OPEN-017 and HM-OPEN-018, which the last session correctly escalated
rather than deciding. Nine rulings taken by Tim on 2026-08-17, recorded here as
requirements. Written in phases per `CLAUDE.md` §12.3, and reported per §12.2.

**Read `SESSION_PROTOCOL.md` before starting.** It is in force as HM-DEC-096
and summarized in `CLAUDE.md` §12; its three report headings govern this
session's report.

**The stated goal, in Tim's words:** *decode almost anything I can hear, within
limits.* A trained ear copies down to roughly 0 dB SNR in a 500 Hz passband. The
20 Hz detection bandwidth buys about 14 dB over that passband, so on paper
Hamlet should beat the ear. Phase 5's 0 dB tier is where that claim gets tested
instead of asserted.

---

## HM-OPEN-017 — RULED: two-stage decode

The reference (`cwdecoder.py`) is a batch decoder; Hamlet is a streaming one.
The last session was right that the clustering gate's behavior is not separable
from its structure, and right to revert it rather than force the graft. The
resolution is not to pick one:

**Hamlet emits one line of text with a live provisional tip that firms into
settled text behind it.**

- The **provisional tip** is produced by the streaming gate as elements
  complete, at the leading edge, and is visually distinct from settled text.
- The **settled text** is produced by the clustering gate a few seconds behind,
  and consumes the provisional tip as it catches up.
- **Fallback:** if the settled pass cannot be landed in this session, ship the
  streaming approximation **labelled as such on screen** and record the
  two-stage design as the target. A labelled approximation satisfies §0.0; an
  unlabelled one does not.

### Revision log
Consumed provisional readings are retained in an **in-memory** log, exportable
on demand, not persisted across runs. It is diagnostic (§0.0.1), not a record
of the air, and a growing on-disk log needs a retention policy nobody has
designed.

### Confidence when the two passes disagree
`CLAUDE.md`'s rule that nothing raises confidence stands, with one narrow
addition:

- The passes **agree** on the character → use the **settled** score. Two
  independent passes confirming a reading is corroboration, not
  self-persuasion.
- The passes **disagree** → use the **lower** of the two. Disagreement is
  itself evidence of marginality and must cost confidence.
- Both scores are retained in the revision log either way.

---

## Phase 1 — the settled pass and its window

**Per-character trailing window. No blocks, no caching.** When a character
completes, fit the threshold over the window of audio ending at that character.
Blocks create seams; an element straddling a seam gets two thresholds applied to
its start and its end. A trailing window has no seams to reason about.

Cost is not an objection: the envelope is decimated to ~100 Hz, so a 3-second
window is ~300 points and a two-means fit on that is trivial beside the Goertzel
bank already running per hop. **Caching is explicitly rejected** — "the audio
has not changed materially" needs a definition, and a wrong one silently
reintroduces stale thresholds, which is the failure mode that produced four
frequency faults and a write-before-read race in this repository.

### Window length and lag
The window is **the longer of ~2.5 seconds and ~30 elements**, evaluated at the
current speed. Both constraints are real and they bind at opposite ends of the
speed range: 2.5 seconds spans a fade cycle, 30 elements makes the two-means
fit stable. At 25 WPM the element count binds; at 8 WPM the time does.

**Hard ceiling of ~4 seconds**, regardless of speed. At slow speeds this means
accepting a weaker fit rather than an unusable delay — the settled line exists
to catch a callsign in a live QSO, and a 6-second lag makes it useless for the
one thing it is for.

**When the ceiling binds, the display says its fit is short.** A degraded
measurement announced is fine; concealed, it is §0.0 broken. Same instinct as
the gate refusing below 6 dB contrast.

## Phase 2 — clock loss

**When incoming marks stop fitting the current clock, declare clock loss, emit
nothing from the settled pass, and re-acquire.** A two-means fit over a mixture
of 11 WPM and 22 WPM marks can land inside the 2.5–3.8 ratio band while
describing neither station. That is a confident wrong answer, which is the
output this project fears most.

**Keep the previous clock as a candidate.** On re-acquisition, test incoming
marks against both the freshly fitted clock and the previous one; if the
previous fits better, the discontinuity was a fade or a burst of QRM and
nothing is lost. Without this, the refusal fires on every fade.

**A genuine speed change is annotated** on the settled line and in the log. It
is a fact about the air, and in a QSO it usually means a different station
started transmitting — which is the earliest evidence session 3 will have that
someone answered.

## Phase 3 — tracker switching

Drift is already handled by re-centering the fine bank. A **different station**
is usually a different pitch entirely, and both failure directions are real:
chase eagerly and the tracker abandons a fading station mid-word; chase
reluctantly and the answer arrives 200 Hz away unheard.

- A **coarse acquisition scan runs continuously in the background**, so a
  stronger keyed candidate elsewhere is known about while tracking continues.
- **Switch only when a background candidate scores clearly better on keying
  structure — not on amplitude — for a sustained period.** Amplitude scoring is
  how a carrier steals the tracker; keying-structure scoring is already ruled in
  by 1.2.
- **Never switch mid-character.** Costs at most one character, and prevents a
  hybrid character assembled from two stations — the same class of confident
  wrong reading the truncated-evidence rule exists to prevent.
- **A switch is a clock-loss event**: fresh clock, annotated exactly as a speed
  change is. Operationally a pitch change and a speed change are the same event
  — someone else started transmitting.

## Phase 4 — the provisional line during refusals

When the settled pass refuses (clock loss, tracker switch), **the provisional
tip keeps running and is visibly marked unstable.** It is not silenced: the
moment someone answers is the worst possible moment for the live feed to go
dark, and the provisional line's entire purpose is catching a callsign fast.
Marking it unstable preserves §0.0 while keeping the feature useful.

## Phase 5 — interference naming

Section 1.4 of `BATCH_BRIEF.md`, unchanged, including the verified citations:
manual notch `16 48`, notch position `14 0D`, Twin PBT inner `14 07` / outer
`14 08`, auto notch `16 41`, CW pitch `14 09`. **`14 08` is the outer PBT** —
the sub-command this project once mistook for CW pitch.

## Phase 6 — DROP THIS ONE IF SHORT OF ROOM

The stale rig-state block (`BATCH_BRIEF.md` item 6): capture sidecar headers
must come from the rig model, not configuration. Three captures show the header
at 7.030 MHz / 40 m while the rig block says 14.055 MHz.

Worth doing before those captures are committed as permanent fixtures, so they
do not carry a known-wrong field forever. **If dropped, say it was dropped** and
do not half-build it — and until it is fixed, treat the band and frequency
header lines in any sidecar as unreliable and take band from the rig block.

**If every phase completes, stop and report. Do not start session 2.**

---

# HM-OPEN-018 — the fixture rebuild is a separate session

**Do not rebuild the CW fixtures in this session.** The last session was right
that making failing tests pass by rewriting their fixtures deserves suspicion
and its own scrutiny. The finding stands: the noiseless fixtures use exact
digital silence between elements, the transmit-mute guard correctly reads
digital silence as a muted receiver, and **a real receiver never hands over
digital silence.** Those fixtures encode a physical impossibility, and Hamlet
passes them partly because it lacks the guard.

This is the same defect class as the scope parser and its fixtures, which were
green for months while the instrument discarded every frame. Recorded as `CLAUDE.md` §12.5.

The rebuild session's terms, ruled in advance:

1. **Rebuilt fixtures land under new names.** The existing ten failures stay in
   place, and the session must state, for each of the ten individually, whether
   it fails against realistic audio too. Old fixtures are retired one at a time
   with a recorded reason each — never wholesale.
2. **The reference decoder must score well on a rebuilt fixture before that
   fixture is allowed to judge Hamlet.** A fixture the reference cannot decode
   is a bad fixture, not a Hamlet failure. This is what makes the fixtures
   themselves falsifiable, which is the property the old ones lacked.
3. **Three SNR tiers per message**: ~15 dB (easy), ~5 dB (the VA3VRR operating
   point), ~0 dB (the edge). Noise shaped to the 500 Hz passband; mutes at
   −90 dBFS rather than zero, per `CW_RECEIVE_BRIEF.md` §4.
4. **QSB on the 5 dB tier only** — 0.7 Hz fade at up to 25 dB depth, as measured
   off-air. Not a full matrix.
5. **The 0 dB tier asserts copy-or-refuse, never copy.** A test that the
   refusals fire is as load-bearing as a test that the decode succeeds.
6. **Purpose-built messages**, using `N0CALL` and no real callsign: a callsign
   exchange for session 2's classifier, a prosign set, a coverage string, and
   one message with the tight-fist timing measured off VA3VRR — 65 ms gaps
   against 105 ms dits, the shape that breaks fixed 1:3:7 gap assumptions.
7. **One QSK-preamble fixture**: ~12 seconds of element-patterned mutes before
   the message, exercising the transmit-mute guard and the truncated-evidence
   rule against a committed fixture rather than only against the two real
   captures.

---

# Standing conflict — session 3 cannot proceed without a ruling

`CLAUDE.md` §0.2, marked ABSOLUTE: **"No unattended transmission. A scan never
transmits."** Auto-CQ as scoped in `BATCH_BRIEF.md` session 3 is automated
repeating transmission. The mutual-exclusion ruling satisfies the second
sentence; the first sentence it does not satisfy.

HM-DEC-008 also stands: **development transmit testing goes into a dummy load,
not an antenna, until the feature is proven.**

**Session 3 does not start until Tim rules on both.** Raise it once, at the top
of the next report, and do not build toward it in the meantime.

Two supporting facts, both confirming choices already made:

- §0.2 already requires that every path keying the transmitter has a
  same-thread, no-await abort available, and **names CI-V `0x17` with `0xFF`**.
  That is exactly the keying method ruled for auto-CQ, and its documented stop
  code (Full Manual p. 19-13). Governance anticipated this.
- Command `17` sends **up to 30 characters**; permitted are 0–9, A–Z, a–z and
  `/ ? . - , : ' ( ) = + " @` and space; `^` joins characters with no
  inter-character space, which is how a prosign is sent properly.
  `CQ CQ DE KC3QIS KC3QIS K` is 25 characters — inside the limit, but not by
  much. A longer call needs two messages, which is a design question for the
  session to raise rather than decide.
