# Decisions

Rulings, newest first. A ruling is never edited — a later decision supersedes
it by id. Index in `CLAUDE.md` §1.

---
id: HM-DEC-009
date: 2026-08-12
refs: CLAUDE.md §0.0
---

The prime directive is: never present a guess as a decode.

The app exists to tell the operator what is on the air. A confident wrong
answer costs more than an honest blank: the operator acts on it. Uncertainty
is rendered as uncertainty — marked low-confidence characters, "unknown"
mode, silence on failed decode. Rejected: best-effort display with no
confidence marking, on the grounds that every decoder is best-effort in noise
and the display would be indistinguishable from a clean decode.

Proposed by Claude; ratified by Tim committing this file.

---
id: HM-DEC-008
date: 2026-08-12
refs: CLAUDE.md §0.2
---

Development transmit testing goes into a dummy load until the feature is
proven.

Buggy keying code on an antenna is an on-air incident. Every transmit path
keeps a synchronous abort available. No unattended transmission; scanning
never transmits.

---
id: HM-DEC-007
date: 2026-08-12
refs: CLAUDE.md §5, §8
---

Decoders are built and tested against recorded WAV fixtures before live
audio, and every decoder bug becomes a replayable fixture.

Live signals are unrepeatable. A decoder validated only against live audio
cannot be regression-tested, and a reported wrong decode without its input
audio is an argument rather than a bug report. Fixtures destined for the
public repository are reviewed by Tim first (CLAUDE.md §2.1).

---
id: HM-DEC-006
date: 2026-08-12
refs: CLAUDE.md §0.1
---

Waterfall rendering bypasses data binding: a custom control owns a
WriteableBitmap and subscribes directly to the engine's spectrum event. The
waterfall ViewModel carries settings only (span, gain, palette).

Spectrum frames arrive at 20–30/s with thousands of bins; pushing them
through INotifyPropertyChanged is allocation churn and UI stutter. This is
the single sanctioned exception to strict MVVM data flow, standard practice
in SDR applications. Ownership is unchanged — the data is still the
engine's.

---
id: HM-DEC-005
date: 2026-08-12
refs: CLAUDE.md §4
---

Spectrum scope data streams from the radio via CI-V command 0x27 from
phase 1. Ham Manager does not compute a wideband FFT the radio already
computes.

The IC-7300's internal panadapter is band-wide and free; the app's own FFT
sees only the receiver passband. The scope stream is also the phase 2
scanner's input — peak detection over data already in hand instead of
stepping the VFO. Command framing details are unverified and must be
confirmed against the CI-V reference before code depends on them
(HM-OPEN-002).

---
id: HM-DEC-004
date: 2026-08-12
refs: LICENSE
---

The license is GPL-3.0.

Phase 3 links ft8_lib, which is GPL; any permissive license chosen now is a
promise that dependency breaks. GPL is also the norm in amateur radio
software (WSJT-X, fldigi, Hamlib), so contributors expect it. Rejected:
MIT-with-isolated-GPL-decoder-process, as plumbing the project does not need
when GPL costs it nothing.

---
id: HM-DEC-003
date: 2026-08-12
refs: CLAUDE.md §6
---

CI-V is hand-rolled for v1 behind an IRig interface; Hamlib is not a
dependency.

One radio, a simple framed byte protocol, and learning the protocol is part
of the project's purpose. The IRig seam keeps a Hamlib-backed implementation
substitutable if multi-rig support is ever wanted. Rejected for v1: Hamlib,
on native-dependency and learning-value grounds — not on merit for the
multi-rig case, which is exactly when this ruling should be revisited.

---
id: HM-DEC-002
date: 2026-08-12
refs: CLAUDE.md §0.1, §6
---

Ham Manager is a C# MVVM desktop application. RadioEngine is a class library
strictly separated from the UI shell: the engine references no UI type, and
a web frontend could later wrap the same engine unchanged.

Real-time serial and audio device access fights the browser sandbox; a
web-first build means writing a native backend anyway with the browser as a
second deliverable. Rejected: web app, Electron. WPF vs Avalonia is
deliberately left open as HM-OPEN-001.

---
id: HM-DEC-001
date: 2026-08-12
refs: CLAUDE.md throughout
---

Governance is established: CLAUDE.md, OPEN_ISSUES.md, DECISIONS.md at the
repository root; tools/repo-listing and tools/get-files carried from Tim's
simulator project with the repo root corrected to C:\Source\HamManager; id
sequences HM-OPEN-### and HM-DEC-###; GitHub TJDixon2022/HamManager,
private for now, public at phase 4.

The carried rules are the ones learned by failing in the prior project:
scaffolded zip delivery, the canonical verbatim collection script, the repo
listing as bootstrap, and never editing a file whose current version was not
pulled this session.
