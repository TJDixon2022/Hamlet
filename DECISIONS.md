# Decisions

Rulings, newest first. A ruling is never edited — a later decision supersedes
it by id. Index in `CLAUDE.md` §1.

---
id: HM-DEC-021
date: 2026-08-13
refs: CLAUDE.md §0.5, src/Hamlet.App/Controls/CollapsiblePanel.cs, HM-DEC-012
---

Every panel in Hamlet collapses, its state persists in settings.json, and a
collapsed panel still carries its summary on the header.

Screen real estate belongs to the operator, not to the designer's idea of
what matters today: a CW operator with no antenna for 20 m does not need the
waterfall open, and an operator reading the field guide does not need the
dial tape. Collapsing hides detail, never information — the shut header still
reads "Happening now · 7 spots · updated 30s ago", "Field guide · 6 modes",
"CW main street · 7.000–7.125". A collapse that silences a panel would be a
prime-directive violation by omission: the operator would be looking at a
screen that had quietly stopped telling them something.

Header treatment: chevron and title on the left in the panel's family colour
as TEXT only, summary right-aligned, subtle hover, and the whole bar
clickable. The family colour is not painted across the bar — seven filled
colour bars stacked down a window read as a stripe pattern rather than as
structure — so panel bodies stay white on warm paper, which is what HM-DEC-012
said in the first place. Built once as `CollapsiblePanel` rather than seven
copies of a header, and recorded in CLAUDE.md §0.5 as a standing design
principle so future panels inherit it without re-litigation.

The rig display is the single exception. It is the IC-7300's own face and the
app's anchor; a Hamlet window with the frequency hidden is not Hamlet.

---
id: HM-DEC-020
date: 2026-08-13
refs: src/Hamlet.App/ViewModels/SpotFreshness.cs, HM-DEC-016, HM-DEC-009, FG-001
---

The happening-now feed refreshes on a timer the operator sets (off, 1, 2, 5,
10 or 15 minutes; five by default), shows its own age at all times, marks
arrivals, and pauses while the window is not on screen.

The feed is the product's star and it must never be silently stale. The panel
header reads "7 spots · updated 30s ago" and ticks; the age goes amber past
twice the refresh interval and reads "stale" past four times it. That is
HM-DEC-009 turned on the Explorer itself — a confident count of spots that
stopped being true twenty minutes ago is a guess presented as a decode.
Switching auto-refresh off does not switch off aging: the operator turned off
the refresh, not the passage of time, so the panel keeps measuring against the
shipped five minutes. The rule lives in `SpotFreshness` as pure functions of
elapsed time and interval, so every threshold is testable without a clock.

Arrivals get a small "new" tag that fades after thirty seconds or on the next
refresh. Surviving spots keep their position in the list and departures drop
out; the list is not re-sorted on every tick, because moving a card out from
under a reading operator's cursor is a worse cost than a perfectly ranked
order. Manual refresh from the Explore menu always works whatever the interval
says, and resets the timer.

Pausing when the window is minimised or hidden costs nothing today against a
fixture. It is recorded now because the seam it protects is HM-DEC-016's
`IActivitySource`: when RBN, POTA and PSK Reporter land behind it, an app that
polls them while nobody is watching is rude to services that are free, and
the polite version has to be built before the first live call, not after.
`FakeActivitySource` now varies its output between calls so the new-arrival
path is exercisable at all.

---
id: HM-DEC-019
date: 2026-08-13
refs: src/Hamlet.App/Settings/OperatorProfile.cs, src/Hamlet.App/Telemetry/AppEvents.cs, HM-DEC-018, FG-001, FG-004
---

Hamlet stores an operator profile — callsign, name, location, grid square —
in the existing settings.json, and shows an About window carrying version,
runtime, dependency versions, session id and a copy-diagnostics button.

The profile is one shaped object rather than three loose strings because
these fields already have futures: location and grid feed propagation and
distance-to-spot work (FG-001), and the callsign feeds logging (FG-004). It
goes in the one settings file, not a second one — §0's "one place" applied
literally.

That puts the operator's identity in the same file as the telemetry switches,
which makes HM-DEC-018's rule — no callsigns in telemetry, ever — easy to
break by accident at a call site. So there are no call sites: every telemetry
payload the shell emits is now built in `AppEvents`, the ViewModels call those
methods, and no method on that class is handed an `AppSettings` or an
`OperatorProfile` to reach the profile through. One test walks every method on
it with a full profile loaded and asserts no written line contains the
callsign, name, location or grid; a second test fails if a new event is added
without joining the walk.

The About box is §0.0.1 meeting the user. "The app must be diagnosable" is
only half true if the diagnosis needs Tim at the keyboard — a stranger filing
a bug needs the build, the runtime, the Avalonia version, the session id and
the telemetry state in one click, and the copied block deliberately carries no
identity because it is going into a public issue tracker. Runtime and library
versions are read at run time; nothing is hardcoded, and a build date that
cannot be read says "unknown" rather than a plausible number.

---
id: HM-DEC-018
date: 2026-08-13
refs: src/Hamlet.App/Settings/AppSettings.cs, src/Hamlet.RadioEngine/Telemetry/
---

Hamlet remembers state and records telemetry locally, per Tim's interview
rulings: one settings.json in %AppData%\Hamlet (window bounds, last port and
band, telemetry switches), a corrupt file yielding defaults rather than a
crash; telemetry in %AppData%\Hamlet\telemetry as daily YYYY-MM-DD.jsonl
files, size-capped with oldest-first eviction, cap editable in Settings.

Six switchable categories — Diagnostics, Rig, Tuning, Explore, Decode,
Performance — all ON by default, each independently switchable in Settings.
Line schema is timestamp, sessionId, level, appVersion, category, event,
data. Deliberately absent: any machine identifier, callsigns, and decoded
message content. Decode telemetry records that a decode happened and its
confidence, never what was said — amateur transmissions are public, but a
file quietly accumulating who you talked to is a different thing.

Nothing uploads. Any future upload is an explicit, separate act with its own
ruling. The menu is the roadmap-shaped B option: File, Radio, Explore,
Tools, Help, with unbuilt items disabled and labelled with the phase that
brings them, so the menu says "not yet" rather than implying "broken".

---
id: HM-DEC-017
date: 2026-08-13
refs: CLAUDE.md throughout, Hamlet.sln
---

The product is renamed Ham Manager -> Hamlet: repo C:\Source\Hamlet, GitHub
TJDixon2022/Hamlet, solution Hamlet.sln, namespaces Hamlet.RadioEngine and
Hamlet.App, tool-script default roots updated.

Name diligence found a collision — "Hamlet UI", an existing Hamlib
front-end — and one-letter adjacency to Hamlib itself. Tim ruled with eyes
open: this app's audience is newcomers who have never heard of either, and
the pun ("let me ham") is the mission in one word. Records dated before
this ruling keep HamManager verbatim, because rulings are never edited;
anything that says HamManager is history, not error.

---
id: HM-DEC-016
date: 2026-08-12
refs: FUTURE_GOALS.md FG-001/FG-002/FG-006, CLAUDE.md §2, src/HamManager.RadioEngine/Explore/
---

The Explorer is the product's center, and it is built UI-first: the app
explores in the interface until the UI tells the story, then implements
behind it. Phase 1.5 "Explorer" enters the plan between the CW terminal and
scanning: the neighborhood map (the band drawn as named places with live
activity), the mode field guide (sound, waterfall fingerprint, why it's
cool), and the happening-now feed (spots as plain-language invitations with
one-click tune). All three run on fixture data behind an IActivitySource
seam today; live feeds (RBN, POTA, PSK Reporter, contest calendars) slide
in behind the same seam later, exactly as Ic7300Rig slid in behind FakeRig.

Tim's ruling on seeing the concept: ham radio is hidden behind the wizard's
mask, and the app exists to take something hard and make it intuitive —
rig-automation apps already exist and are not the goal. This partially
graduates FG-001 (discovery UI now, live feeds still future), seeds FG-002
(spots carry WPM), and previews FG-006 (the map is band coaching). The
prime directive extends to spots: source and age always shown; sample data
is labeled sample.

---
id: HM-DEC-015
date: 2026-08-12
refs: src/HamManager.App/Controls/, HM-DEC-005, FG-001
---

The tuning HMI is the approved three-tier design: band buttons that jump to
each band's CW watering hole and carry a time-of-day best-bet badge; a band
ribbon (the map) with the CW segment shaded and click/drag tuning; and a
dial tape (the fine control) — a fixed hairline with the frequency scale
dragged underneath it, flick momentum, 10 Hz snap. Per-digit mouse-wheel
tuning on the frequency face; arrow keys are plus/minus 10 Hz. There are no
step buttons.

Tim rejected the plus/minus step buttons on sight. The tape and ribbon share
one frequency axis: in phase 2 the waterfall paints behind the tape and the
ribbon, so click-a-signal-to-tune falls out of controls that already exist.
The best-bet badge is the seed FG-001 replaces with live spot data. The mode
line goes red outside the CW segment — honest state per the prime directive.

---
id: HM-DEC-014
date: 2026-08-12
refs: CLAUDE.md §10, §11
---

Graphify is adopted as a navigation aid, its known blind spots recorded in
§10.1, and Tim supplies a fresh repo_listing.txt plus graphify output
(GRAPH_REPORT.md, graph.json, manifest.json) at the start of each
conversation.

The graph raises questions; the listing and file reads answer them. The
blind-spot list is carried because the parent project acted on graph noise
— isolated static classes read as dead code, low cohesion on prose read as
a refactoring signal — and lost rounds to it. Conversation-start freshness
exists because a session working from last week's listing makes confident
requests for paths that no longer exist, and the failure looks like a
tooling bug instead of a stale input.

---
id: HM-DEC-013
date: 2026-08-12
refs: CLAUDE.md §9.2, §7
---

Every delivery ends with a check-in block: the exact git add and git commit
commands, ready to paste, message in §7 format covering precisely what the
zip contains.

Tim commits every file drop. Composing the commit message for Claude's work
is Claude's job — Claude knows what changed and why; making Tim reconstruct
it invites messages that drift from the diff, and an uncommitted drop with
no prepared message invites an unrecorded one. If a delivery amends a prior
uncommitted drop, the block says so and amends.

---
id: HM-DEC-012
date: 2026-08-12
refs: src/HamManager.App/App.axaml
---

The UI is a light theme with color: warm paper ground, white panels, deep
amber frequency face, decode green. Not dark mode.

Tim's ruling on seeing the first shell. Dark is the SDR-software convention,
which is exactly why this is recorded — a future session would otherwise
"correct" back to it. A dark variant may return later as a user option;
the default is light.

---
id: HM-DEC-011
date: 2026-08-12
closes: HM-OPEN-001
refs: CLAUDE.md §6
---

The UI framework is Avalonia 11 on .NET 8.

Cross-platform reach matters for the phase 4 public release — Linux is
common in ham shacks — and Avalonia is deliberately WPF-shaped, so Tim's
MVVM fluency transfers whole. The learning cost lands on Claude, who writes
the code. The one API divergence that matters, WriteableBitmap's lock/write
surface, is confined to the waterfall control by HM-DEC-006. Rejected: WPF
(Windows-only forever), WPF-then-port (every control written twice,
including the hardest one).

---
id: HM-DEC-010
date: 2026-08-12
refs: CLAUDE.md §0.3
---

Questions follow a fixed protocol: one question at a time, probed as deeply
as needed before the next; every question is a clear decision ask — option
A, option B, option C — with pros and cons in a table. Walls of text are
the enemy.

Amends §0.3. An unstructured question invites an unstructured answer, and a
question buried in prose is a question Tim has to excavate before he can
rule on it.

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
