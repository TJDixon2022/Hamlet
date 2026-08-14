# Hamlet — Project Instructions and Decision Record

**Project:** Hamlet. A C# MVVM desktop application that controls an Icom
IC-7300 over its single USB connection: CW (Morse) send and receive first,
then frequency control and signal scanning, then digital mode auto-detection
and decoding, then a polished waterfall UI and open-source release. Tim's own
project, own time, own money — intended for eventual public release under
GPL-3.0 (HM-DEC-004).

**What this file is.** The complete record of decisions governing this
repository — technical, procedural, and structural. Anything not written down
becomes an assumption, and assumptions are what this file exists to eliminate.

Read automatically by every Claude Code session in this repository. In a
Claude **web** project it is not read from disk — it must be present in
project knowledge and **re-uploaded whenever it changes**, or the session runs
on stale rules and will do so confidently.

**Amending:** Tim rules; the entry is written here. Add to the decision log in
§1 with a date. Never silently edit a ruling — supersede it and say so.

**Companion documents:**

| Document | Holds |
|---|---|
| `OPEN_ISSUES.md` | Questions, with owner and severity |
| `DECISIONS.md` | Rulings, newest first, never edited |
| `FUTURE_GOALS.md` | Aspirations (`FG-###`) — direction, not scope; graduate only by ruling |
| `ONBOARDING.md` | The first-run experience: seeded steps (`ONB-###`), standing principles, and what still needs a first-time moment. Direction, not scope, until ruled |
| `data/vendor/` | Pinned snapshots of documents cited from outside this repository (see §4) |
| `tools/` | The repo-listing and get-files scripts — the chat-session workflow |

---

## 0.0 Prime directive — the reason everything below exists

> **Never present a guess as a decode.** What Hamlet shows on screen is
> what was actually on the air. Uncertainty is displayed as uncertainty —
> a low-confidence character is marked, a failed decode is silence, and a
> mode identification below threshold says "unknown", not its best guess
> dressed as an answer.

Practical test: could the operator, acting on what the screen says, be wrong
because the app was more confident than its input justified? If yes, the
display is wrong regardless of how much cleaner it looks.

This binds hardest exactly where it is most tempting to break: a Morse
decoder in noise, a mode classifier at low SNR, an S-meter reading
interpolated between polls. Show the confidence, or show nothing.

Proposed by Claude 2026-08-12; ratified by Tim's commit of this file.
HM-DEC-009.

## 0.0.1 Second principle — the app must be diagnosable

> **When a decode is wrong or missing, the app's own record must be enough to
> tell whether the fault is in the signal, the radio, or Hamlet itself.**

What that requires, at minimum: raw audio can be captured to WAV on demand;
every CI-V frame sent and received is loggable verbatim with a millisecond
timestamp; every decoder records the parameters it ran with (center pitch,
WPM estimate, threshold) alongside its output. A wrong decode that cannot be
replayed is an argument; a wrong decode with its input WAV attached is a
regression test (HM-DEC-007).

---

## 0. Governing principle — overrides everything below

> Default to: best usage by industry standard, cleanest to test and deliver,
> best result for the end user. **Tim's convenience does not figure into it.**

Consequences that follow directly, and are not to be re-litigated as
"overhead":

- Where a stricter setting and a permissive one both work, take the stricter.
- Where something can be **generated** from a source of truth, generate it.
  Hand-copied duplicates drift, and they drift silently. This applies to the
  CI-V command table, the band plan, and mode frequency conventions: data
  files with source marks, not constants sprinkled through code.
- Where a check can run in CI, run it in CI.
- Where a compiler can catch a class of error, let it. New projects enable
  `Nullable` and treat warnings as errors.
- "Faster to start" is not a reason. "Harder to maintain" and "harder to hand
  over" are reasons — this project is headed for public contributors.

When Claude presents options, it states the industry-standard answer and the
reason, rather than offering a convenience trade-off as an equal branch.

### 0.1 The engine owns radio knowledge — ABSOLUTE

Everything about the radio lives in `RadioEngine`. The UI knows only what the
engine exposes through its interfaces.

- `RadioEngine` references no UI assembly, no WPF/Avalonia type, no
  `Dispatcher`. Enforced at compile time by project references.
- CI-V framing, audio capture, DSP, decoders, band plan, mode conventions:
  engine. Views and ViewModels hold no radio facts.
- The one permitted exception is the waterfall pixel path (HM-DEC-006), which
  bypasses data binding for throughput. It is an exception in *mechanism*,
  not in *ownership* — the spectrum data still comes from the engine.

Practical test: could the engine be wrapped in a console app or a web service
without touching it? If not, something leaked.

### 0.2 Transmit safety — ABSOLUTE

- Development transmit testing is into a **dummy load**, not an antenna,
  until the feature is proven (HM-DEC-008).
- Every code path that keys the transmitter has a same-thread, no-await abort
  available (CI-V `0x17` with `0xFF`, and PTT off as the fallback).
- No unattended transmission. A scan never transmits.

### 0.3 Terse, and how questions are asked

Claude answers short. Point first, no preamble. **Walls of text are the
enemy.**

Questions follow a fixed protocol (HM-DEC-010):

- **One question at a time.** Probe as deeply as needed — follow-ups on the
  same question are fine — before moving to a second question.
- Every question is a **clear decision ask**: option A, option B, option C,
  each with pros and cons, as a table.
- Claude states the industry-standard answer and why (§0), then Tim rules.

### 0.4 Tim rules, Claude executes

Tim is the architect and owns the outcome. Claude makes no assumptions, no
decisions, and no forward progress without his say.

Raise a thing once, then stop. Committing, publishing and rulings are his.
Never ask him to re-confirm a rule he has already given. Executing inside an
approved plan is not deciding — building the files the plan names needs no
per-file permission.

### 0.5 Panels collapse — standing design principle

Every panel in the app is collapsible, and stays collapsed across restarts
(HM-DEC-021). New panels inherit this; it is not re-decided per panel.

- Built once, as `src/Hamlet.App/Controls/CollapsiblePanel.cs`. Seven copies
  of a header bar is seven places for it to drift.
- Header: chevron + title on the left in the panel's family color — amber
  tuning, blue spectrum, green decode — as **text color only**. The bar is
  never filled with the family color; a column of filled bars reads as
  stripes, not as structure. Panel bodies stay white on warm paper
  (HM-DEC-012). Summary right-aligned, subtle hover, whole bar clickable.
- **A collapsed panel still carries its summary.** Collapsing hides detail,
  never information: `▸ Happening now · 7 spots · updated 30s ago`,
  `▸ Field guide · 6 modes`, `▸ Waterfall · not yet receiving`. A shut panel
  that goes silent is the prime directive broken by omission.
- Expand/collapse state persists per panel in `settings.json`; an unknown
  panel is open.
- The rig display — the IC-7300's own face — is the one exception. It is the
  app's anchor.

Practical test: could the operator shut this panel and still know what it
would have told them at a glance? If not, the summary is wrong.

### 0.6 Modes have one color language — standing design principle

Every surface that shows a mode family uses the same four colors, defined once
in `src/Hamlet.App/Controls/ModePalette.cs` (HM-DEC-032). New surfaces read
from it; they do not carry color literals.

| Family | Fill | Ink | Covers |
|---|---|---|---|
| Morse | `#EDC375` | `#5E3800` | CW |
| Digital | `#BFB6E4` | `#2B2360` | FT8, RTTY, PSK31, JS8 |
| Voice | `#A3CBE8` | `#0B3B5C` | SSB, AM, FM |
| Open / mixed | `#E4E0D5` | `#5F5C53` | Unclaimed space, or several at once |

- **Color carries meaning, so it may never be the ONLY carrier of meaning.**
  Roughly one man in twelve has a color vision deficiency, and this hobby's
  demographics make that a real slice of the people who will use this. Every
  colored thing also says what it is: map segments are labeled, the legend
  names each family in words, the listen-only veil hatches as well as tints,
  the band cards carry an icon and a width beside their hue.
- **A map that uses color needs a legend.** A wash nobody can decode is
  decoration, and decoration that looks like information is worse than none.
- **Every ink clears WCAG AA — 4.5:1 — against its own fill**, with no
  exceptions (HM-DEC-036). This hobby's median age makes contrast a mainstream
  requirement here, not an accessibility footnote.
- The family is declared on the data — `Neighborhood.Family`, `ModeInfo.Family`
  — never on the control. A per-control literal is a second copy of the
  language, and a second copy drifts silently.

Practical test: print the screen in grayscale. Can the operator still tell what
each region is? If not, color is doing work nothing else is doing.

### 0.7 The voice — standing rule

Hamlet's explanatory prose is written as **connected speech**: a patient friend
with forty years on the air explaining it while you both look at the radio
(HM-DEC-034). This governs tooltips, blurbs, status lines, panel summaries and
anything else the operator reads.

- Thoughts run into one another. A stack of short declarative sentences reads
  as machine-written, and the person this is for has had enough of being told
  things by machines.
- The reason is attached to the fact, not left implied. "80 m is a night band"
  is a fact; "daylight thickens a layer that soaks up low frequencies, and
  after dark it thins" is the same fact with its reason, and only the second
  one teaches.
- Ordinary words beat correct ones where they differ.
- Numbers are spoken, not counted: "the sun went down about an hour and a half
  ago", never "sunset was 94 minutes ago".
- **Warmth never buys a claim.** It is a matter of how a thing is said and
  never of what is asserted (§0.0). A friendly sentence that overstates what
  Hamlet knows is worse than a cold one, because it is more readily believed.
- **If Hamlet says it, Hamlet explains it** (HM-DEC-041). Any term a
  six-year licensee might not know earns a glossary entry, and the marking
  finds it automatically wherever it already appears.

**Em dashes are used sparingly: at most one in a paragraph, and usually none**
(HM-DEC-040). A dash is usually a sentence that has not decided where it ends.
Prefer a comma, or a full stop and a fresh sentence. Warm writing breathes with
periods, short sentences are allowed to land on their own, and a pause where the
reader should reflect is worth more than a clause bolted on with a dash.

The rule is enforced rather than remembered: `VoiceTests` sweeps every string
the operator can read and fails on the second dash in one passage. Records,
comments and code are outside it, and this file is deliberately full of them.

Copy written before a voice ruling is corrected where it is touched rather than
in one sweep, so the change arrives with the work that gives it context.

Practical test: read it aloud. Does it sound like a person, or like a manual?

---

## 1. Decision log

Every ruling, most recent first. Detailed records live in `DECISIONS.md`;
this table is the index.

| Date | Decision | Why | Ref |
|---|---|---|---|
| 2026-08-14 | **The dial tape carries the map's spots on a rail along its top edge.** One `FrequencyAxis` now serves the map, the tape and the waterfall, so a station is at the same frequency on all three; `SpotMarkerStrip` takes an axis and a rectangle and is the phase 2 waterfall's for the asking. Same tooltip, same click-to-tune, out of the frequency scale's way, and an empty rail is not drawn. | The tape showed nothing while the map showed dots for the same stations, which teaches a newcomer that the tape is decoration; and three copies of one mapping is three mappings. | HM-DEC-047 |
| 2026-08-14 | **The best-bet badge is ranked from observation, not the clock.** One `BandOpportunities.Rank` feeds the badge, the pips and the lead card, so they cannot disagree about which band is best. The clock heuristic drops to a tiebreaker for when nothing has been heard anywhere, and the badge then says it is going on the hour rather than wearing the same words as an observation. | Three surfaces answered one question and the loudest answered from a first-week lookup table that cannot hear anything: badge on 80 m with no pips while the lead card pointed at 40 m. | HM-DEC-046 |
| 2026-08-14 | **Spots persist to a local SQLite store and the display is a view over history.** Each source gets a lifetime (activation an hour, skimmer twenty minutes, contest longest), age is spoken with likelihood claims only the source can support, and feed freshness ("checked") is kept apart from opportunity freshness. An empty band reaches for a busier one before declaring nothing. **SQLite is chosen here, not inherited: no prior ruling covered local storage.** | The app discarded good invitations at ten minutes and said "nothing here" while holding them, which is exactly when a newcomer gives up. | HM-DEC-045 |
| 2026-08-14 | **Settings joins the color language, and provenance becomes visible.** Each section tints to its existing family from a shared `PanelPalette`; a field a lookup confirmed carries a "verified" pill that clears live as you type, and a hand-set value that disagrees carries an amber one instead. Driven only by stored provenance, never inferred. | Every other surface uses color to say what a thing belongs to and this one did not; and the app knew where it learned each fact but only whispered it in gray. | HM-DEC-044 |
| 2026-08-13 | **A panel showing what a contact actually sounds like** — a worked example, both sides, first CQ to sign-off, Morse and voice on one toggle, in the operator's own callsign. Tone is enforced: a test fails any copy that says "you must" or "be careful". | The real terror is not the radio, it is not knowing what to say. The shape is a ritual everybody knows except the person who has never made one. | HM-DEC-043 |
| 2026-08-13 | **Signal reports made legible where they appear.** A spot's SNR shows as "24 dB over the noise, which is strong", and the RST convention gets one honest paragraph including the part about it being a polite fiction. A measured figure is never converted into a reported one. | "You're five by nine" is in every contact ever made and nobody explains it; and a computer's measurement dressed as somebody's opinion would be inventing a courtesy. | HM-DEC-042 |
| 2026-08-13 | **A hover glossary over a data file, marked automatically** in the app's own copy: quiet dotted underline, first occurrence per block, whole words only, never inside a callsign or frequency. Definitions do emotional work, not just semantic. **If Hamlet says it, Hamlet explains it.** | The vocabulary is the gate this hobby is kept behind, and handing out the dictionary is the most direct thing software can do about it. | HM-DEC-041 |
| 2026-08-13 | **Em dashes sparingly: at most one per paragraph, usually none.** Part of the voice standard in §0.7, applied by recasting the existing copy rather than swapping punctuation, and enforced by `VoiceTests` sweeping the source. | A dash is usually a sentence that has not decided where it ends; and a rule that lives only in a governance file is one the next session rediscovers by breaking it. | HM-DEC-040 |
| 2026-08-13 | **A rotating Shakespeare byline under the wordmark** — one of forty-five bent toward radio, never the same twice running, the play on hover, the index stored in settings.json. A missing or malformed file means no byline at all, never a placeholder. | Ham radio is intimidating and a small daily chuckle costs nothing. Shakespeare died in 1616, so the source is public domain and the alterations are ours. | HM-DEC-039 |
| 2026-08-13 | **Distance and rough bearing on spot cards and map tooltips**, in miles by default. Shown ONLY where the source stated where the *station* is — POTA does, RBN states where a *receiver* is and so carries none. No grid means no distance anywhere; never estimated from a location string or a callsign prefix. | It is the only way a newcomer acquires the sense of what distances are plausible on which bands; and a distance to whoever heard a signal is a lie about the transmitter. | HM-DEC-038 |
| 2026-08-13 | **The grid square is derived from the lookup's coordinates**, resolved lazily and automatically like the class, with its own provenance, explained in one plain sentence, and never overwriting a hand-entered value. Coordinates are the stored fact; the locator is rendered from them. | "Maidenhead locator" is jargon with nothing behind it — the coordinates are already in the response. This is also what makes the band cards visible at all (HM-DEC-033). | HM-DEC-037 |
| 2026-08-13 | **Two corrections Tim ruled:** the open/mixed ink darkens to `#5F5C53` so every family clears WCAG AA with no carve-out, and the canonical tool scripts under `tools/` are exempt from the spelling standard entirely and restored byte-identical. | Contrast is mainstream in a hobby this age; and a rule with a "but it was only a comment" exception is not a rule. | HM-DEC-036 |
| 2026-08-13 | **American spelling is the project standard** — code, comments, prose, records and UI alike. Two exceptions: a quoted external source keeps its spelling verbatim, and a rename that changes a stored settings key ships with a migration and a test proving an existing profile survives. `LicenceClass` → `LicenseClass` migrated and proved against Tim's own file. Recorded in §6. | US operators, US regulations, US contributors; and mixed spelling splits identifiers and searches. A silent profile reset would look exactly like the app forgetting who he is. | HM-DEC-035 |
| 2026-08-13 | **The voice is connected speech, not a stack of facts** — a patient friend with forty years on the air, explaining while you both look at the radio. Reasons attached to facts, numbers spoken not counted, warmth never buying a claim. Standing rule in §0.7. | The product is an argument that this hobby can be explained; clipped fragments sound like the manuals that already failed him. | HM-DEC-034 |
| 2026-08-13 | **Band buttons become character cards:** width follows wavelength, a drawn sun or moon marks the band's element, the card dims out of it, and hover gives plain prose about the sun and the season. Sunrise/sunset computed from the operator's coordinates, checked against vendored USNO data. No location means nothing dims and nothing is claimed. | A row of identical rectangles teaches nothing, and "80 meters is a long wave" is the fact that makes every other band fact stop being arbitrary. | HM-DEC-033 |
| 2026-08-13 | **One mode color language across the app**, defined once in `ModePalette`: Morse amber, digital lavender, voice blue, open neutral. The map fills from `Neighborhood.Family` rather than a color literal, and gains a legend. Color is never the only carrier of meaning. Standing rule in §0.6. | Two copies of a language are two languages; and a wash nobody can decode is decoration that looks like information. | HM-DEC-032 |
| 2026-08-13 | **Band buttons carry a per-band activity indicator** from live spot counts, with hover detail supplying the evidence. Counts are a proxy for activity, never for propagation — the app reports what was heard and does not assert what the ionosphere is doing. No data and no spots are visually and textually distinct, and a band-scoped source (RBN) no longer vouches for bands it cannot see. | The first control anybody touches said nothing, while the data to answer them was already flowing; and a count taken by a source that was not listening is a claim, not an observation (HM-DEC-025). | HM-DEC-031 |
| 2026-08-13 | **`IRig` gains a capabilities record** — scope, keyer, USB audio, transmit, supported bands — reported by the implementation and never configured. The UI degrades honestly on a radio lacking a feature. | HM-DEC-003's revisit condition, taken early while there are only two implementations to change. | HM-DEC-030 |
| 2026-08-13 | **Part 97 privileges are cited data under `/data`**, transcribed from eCFR with the paragraph on every row; the two CFR tables stay separate and the join is code with tests. The band map veils listen-only segments over the culture map, tuning is never restricted, the status line explains in amber rather than scolding in red, and an unresolved class draws NO overlay. Guard rail is transmit-only and ships on. | Listening is never restricted and the app must say so; and this is the one place where a confident error has legal consequences (HM-DEC-009). | HM-DEC-029 |
| 2026-08-13 | **License class in the profile with provenance**, resolved lazily and automatically whenever a callsign has no class, narrated in the status bar. A lookup NEVER overwrites a hand-set class — a mismatch shows both values and the operator decides. The callsign goes to callook.info and still never to telemetry. | People skip wizards, so resolution attaches to the fact rather than a screen; and it is their license. | HM-DEC-028 |
| 2026-08-13 | **The waterfall renderer is built now**, against synthesised frames of the same shape CI-V 0x27 will deliver, per HM-DEC-006: a custom control owning a WriteableBitmap, subscribing to the engine's event directly, click-to-tune sharing the dial tape's and map's frequency axis. **Field-guide audio is synthesised, not recorded** — license-free, deterministic, testable, and parameterised by speed. | Phase 2 then swaps the data source and not the UI; and a renderer that exists is a renderer being exercised. | HM-DEC-027 |
| 2026-08-13 | **The simulated radio is a training feature, not a test double.** FakeRig becomes TrainingRig, the port list says "Training radio", and the waterfall states its signals are simulated whenever the connected rig is — derived from connection state, with no setter at any level, so no practice mode or toggle can put synthetic signals on screen unlabeled. Signals are placed by reading NeighborhoodPlan, so practice teaches the real band. | Someone who cannot yet tell one signal from another needs to practice without owning an antenna; and a rule the type system enforces is a rule nobody has to remember (HM-DEC-009). | HM-DEC-026 |
| 2026-08-13 | **Ranked happening-now list with a stated reason on every card**, a written lead card with its rationale, and a band-conditions line that shows its evidence, softens on a thin sample and says when Hamlet cannot see the bands. Amends HM-DEC-020 on re-sorting. | The operator needs one sentence telling him where to point the radio and what he will hear, not another list; and a confident count taken while the feeds were down is a guess presented as a decode. | HM-DEC-025 |
| 2026-08-13 | **Live activity sources: POTA, SOTA and RBN behind IActivitySource**, polite identification on every request, self-imposed rate floors, RBN filtered to band and continent. SOTA ships off pending SOTA's own registration and AI-code approval. The callsign goes to these services and still never to telemetry. | Endpoints and field names read off the live services, not recalled; and a license Hamlet does not hold is not a permission it may assume. | HM-DEC-024 |
| 2026-08-13 | **Map dots are first-class**: per-dot hit testing, hover tooltip carrying story, frequency, mode, source and age, click-to-tune, rank-scaled prominence, layout precomputed on data change. | The dots always drew the eye and never earned it; the prime directive does not weaken because the surface got smaller. | HM-DEC-023 |
| 2026-08-13 | **Several sources behind one aggregate**: per-source switches, a failed source keeps its spots on screen ageing visibly, exponential backoff, and per-source status published every refresh. | Losing a network is not a reason to blank a panel somebody was reading, and the conditions line cannot be honest without knowing who answered. | HM-DEC-022 |
| 2026-08-13 | **Every panel collapses**, state persisted per panel in settings.json, and a collapsed header still carries its summary line. Chevron + title in the family color as text only — never a filled color bar. Rig display exempt. Standing design principle in §0.5. | Screen real estate belongs to the operator; collapsing hides detail, never information. | HM-DEC-021 |
| 2026-08-13 | **Happening-now auto-refresh:** operator-set interval (off/1/2/5/10/15 min, default 5), always-visible age that goes amber at 2× and reads "stale" at 4×, "new" tags on arrivals, pause while the window is hidden, manual refresh always available. | The feed is the product's star and must never be silently stale; pausing when unwatched is politeness to the live spot networks it will call later. | HM-DEC-020 |
| 2026-08-13 | **Operator profile** (callsign, name, location, grid) in the existing settings.json; every telemetry payload built in one place (`AppEvents`) that cannot see the profile, proved by test. **About window** with version, .NET and Avalonia versions read at run time, session id and a copy-diagnostics button. | The callsign now shares a file with the telemetry switches, so HM-DEC-018's rule needs one site to hold; the About box is §0.0.1 meeting the user. | HM-DEC-019 |
| 2026-08-13 | **Local settings and telemetry:** `%AppData%\Hamlet\settings.json` (window state, last port/band, switches) and `telemetry\YYYY-MM-DD.jsonl`, size-capped. Six categories, all on, switchable. No machine id, no callsigns, no message content, no upload. Roadmap-shaped menu with phase-labeled disabled items. | HM-DEC-013-style honesty applied to the app's own record; the community is right to distrust phone-home software. | HM-DEC-018 |
| 2026-08-13 | **Renamed Ham Manager → Hamlet: repo `C:\Source\Hamlet`, GitHub `TJDixon2022/Hamlet`, solution and namespaces `Hamlet.*`.** Hamlib/"Hamlet UI" collision found and accepted. Pre-rename records keep the old name verbatim. | "Let me ham" is the mission in one word; Tim ruled the collision immaterial for this audience. | HM-DEC-017 |
| 2026-08-12 | **The Explorer is the product's center, built UI-first on fixture data behind an IActivitySource seam: neighborhood map, mode field guide, happening-now feed.** Phase 1.5. | The app demystifies; automation apps already exist. Partially graduates FG-001. | HM-DEC-016 |
| 2026-08-12 | **Tuning HMI: band buttons with best-bet badge, band ribbon map, dial-tape fine control with momentum; per-digit wheel tuning; no step buttons.** | Tim's approved design; the ribbon and tape become the waterfall's axis in phase 2. | HM-DEC-015 |
| 2026-08-12 | **Graphify is adopted as a navigation aid with its blind-spot list carried into §10; Tim supplies fresh `repo_listing.txt` and graphify output at the start of each conversation.** The graph raises questions; the listing and file reads answer them. | The parent project lost rounds acting on graph noise; freshness at conversation start prevents confident work against a stale tree. | HM-DEC-014 |
| 2026-08-12 | **Every delivery ends with a ready-to-paste check-in block: exact git commands, §7-format message covering the zip's contents.** Amends §9.2. | Tim commits every file drop; composing the message for Claude's work is Claude's job, and an uncommitted drop with no message invites an unrecorded one. | HM-DEC-013 |
| 2026-08-12 | **UI is light theme with color — warm paper, white panels, deep amber, decode green. Not dark mode.** | Tim's ruling; recorded because dark is the SDR convention a future session would revert to. | HM-DEC-012 |
| 2026-08-12 | **The UI framework is Avalonia 11 on .NET 8.** Closes HM-OPEN-001. | Cross-platform reach for the open-source release; WPF-shaped enough that Tim's MVVM fluency transfers; the WriteableBitmap difference is confined to the waterfall control. | HM-DEC-011 |
| 2026-08-12 | **Question protocol: one question at a time, probed to depth before the next; every question a decision ask — options A/B/C with pros and cons in a table.** Amends §0.3. | Walls of text are the enemy; an unstructured question invites an unstructured answer. | HM-DEC-010 |
| 2026-08-12 | **Prime directive: never present a guess as a decode.** | The app exists to tell the operator what is on the air; a confident wrong answer is worse than none. | HM-DEC-009 |
| 2026-08-12 | **Development transmit goes into a dummy load until a feature is proven.** | Buggy keying code on an antenna is an on-air incident, not a bug. | HM-DEC-008 |
| 2026-08-12 | **Decoders are built and tested against recorded WAV fixtures before live audio.** Every decoder bug becomes a replayable case. | Live signals are unrepeatable; a decoder that only ever ran live cannot be regression-tested. | HM-DEC-007 |
| 2026-08-12 | **Waterfall rendering bypasses data binding**: a custom control owns a `WriteableBitmap` and subscribes to the engine's spectrum event directly. ViewModel carries settings only. | 20–30 frames/s of spectrum bins through `INotifyPropertyChanged` is allocation churn and stutter. Standard SDR practice. | HM-DEC-006 |
| 2026-08-12 | **Spectrum scope data streams from the radio (CI-V `0x27`) from phase 1.** The app does not compute a wideband FFT the radio already computes. | The 7300's panadapter is free, band-wide, and becomes the phase 2 scanner's input. Verify command details per §4. | HM-DEC-005 |
| 2026-08-12 | **License is GPL-3.0.** | Phase 3 links ft8_lib (GPL); anything permissive now is a promise that dependency breaks. Norm for ham software (WSJT-X, fldigi, Hamlib). | HM-DEC-004 |
| 2026-08-12 | **CI-V is hand-rolled for v1 behind an `IRig` interface; Hamlib is not a dependency.** | One radio, a simple framed protocol, and the learning is the point. The interface keeps Hamlib substitutable if multi-rig support is ever wanted. | HM-DEC-003 |
| 2026-08-12 | **C# MVVM desktop app. `RadioEngine` class library strictly separated from the UI shell** (§0.1). WPF vs Avalonia is HM-OPEN-001. | Real-time serial + audio + DSP fights the browser sandbox; a web frontend can wrap the same engine later without rework. | HM-DEC-002 |
| 2026-08-12 | **Governance established** — this file, `OPEN_ISSUES.md`, `DECISIONS.md`, `tools/`, ids `HM-OPEN-###` / `HM-DEC-###`, repo `C:\Source\HamManager` (renamed by HM-DEC-017), GitHub `TJDixon2022/HamManager`. Carried from Tim's simulator project. | The rules carried are the ones learned by failing there: scaffolded delivery, the canonical collection script, the repo listing, never editing a file not pulled this session. | HM-DEC-001 |

---

## 2. What this project is

Four phases, each delivering something usable on the air:

| Phase | Delivers | Exit criterion |
|---|---|---|
| 1 | CW terminal: connect, key CW from typed text via the radio's keyer, decode received CW to text. Foundation: solution, `RadioEngine`, CI-V serial, audio capture, crude scope display | Call CQ; read the reply on screen |
| 1.5 | The Explorer (HM-DEC-016): neighborhood map with activity dots, mode field guide with waterfall fingerprints, happening-now feed with one-click tune — fixture data behind IActivitySource | A newcomer clicks a story and the rig goes there |
| 2 | Frequency control and scanning: tune, band presets, peak detection on scope data, CW-keying discrimination, click/auto-hop to a found signal | The app finds a CW signal Tim did not tune to |
| 3 | Digital modes: `IModeDecoder` plugins (FT8 via ft8_lib wrap, PSK31, RTTY), auto-detection by convention + spectral signature + parallel probe decode | The app names the mode and decodes it without Tim guessing |
| 4 | Polish and release: waterfall palettes/zoom, decoded-text overlays, settings persistence, installer, docs, public GitHub | A stranger installs it and works a QSO |

Work stays scoped to the current phase. Adjacent work that looks obviously
worth doing is outside the plan: name it and stop (§0.4).

### 2.1 Boundaries

1. **No credentials in the repository.** Ever, including test fixtures.
2. **No third-party proprietary material.** Icom's manuals are cited and the
   cited pages vendored (§4); they are not committed wholesale.
3. **Recorded off-air audio may contain callsigns and message content.**
   That is public by nature (amateur transmissions are public), but fixtures
   committed to the public repo are reviewed by Tim first.
4. **Id sequences are this project's own**: `HM-OPEN-###`, `HM-DEC-###`.
   Never reuse an id, never renumber.

---

## 3. Records

Two files at the repository root.

**`OPEN_ISSUES.md`** — questions. Required fields: `id`, `status`, `owner`,
`raised`, `severity`.

```
---
id: HM-OPEN-001
status: open
owner: tim
raised: 2026-08-12
severity: hard
blocks: what is actually stuck, or omit
closed: YYYY-MM-DD, when it becomes a decision
refs: where it is written up
---

The question, one paragraph.

Context, reasoning, what was rejected.
```

| Field | Values |
|---|---|
| `status` | `open` · `answered` · `closed` |
| `owner` | `tim` · `claude` · `unassigned` |
| `severity` | `hard` (work stops) · `slows` (continues, degraded) · `none` |
| dates | `YYYY-MM-DD`, no other format |

`owner` is **who must act next**, not who raised it. Do not assign an item to
Tim to avoid an empty owner.

**`DECISIONS.md`** — rulings, newest at the top. Required: `id`, `date`.

```
---
id: HM-DEC-001
date: 2026-08-12
closes: HM-OPEN-001
supersedes: HM-DEC-000
refs: CLAUDE.md §0.1
---

The ruling, one sentence, present tense.

Why. What was rejected and on what grounds. What follows from it that should
not be re-argued later.
```

**A ruling is never edited.** If a later decision overrides an earlier one,
write a new record with `supersedes:` pointing at it.

Never invent ids, dates or owners to fill a field. Ask. Do not soften a
severity to make the picture look better.

---

## 4. Machine facts that constrain everything

**Icom IC-7300** — HF/50 MHz transceiver. One USB cable to the PC carrying
two functions: a virtual COM port for CI-V CAT control, and a USB audio
codec for RX/TX audio. No external interface hardware is needed or used.

Facts Claude carries from general knowledge, **all to be verified against the
IC-7300 Full Manual and CI-V reference before code depends on them**, with
the cited pages vendored into `data/vendor/` (the simulator project's
SIM-DEC-034 rule, carried here):

- CI-V frame: `FE FE <to> <from> <cmd> [subcmd] [data] FD`. Radio default
  address `0x94`; controller conventionally `0xE0`. Frequencies are BCD,
  little-endian by byte pair.
- CW transmit: command `0x17` sends a text string that the radio keys with
  its internal keyer at the set WPM; limited length per message (believed 30
  characters — verify); `0xFF` as the message aborts sending.
- Spectrum scope: command `0x27` family reads/streams scope waveform data;
  "scope ON" and "output to controller" flags must be set. Exact framing,
  span encoding and update rate: verify.
- USB serial baud: configurable on the radio; CI-V USB commonly run at
  115200. The radio menu setting and the app config must agree.
- Audio: the codec appears as a standard Windows audio device
  ("USB Audio CODEC"). Sample rate for DSP: decided when the audio pipeline
  is scaffolded; 48 kHz is the working assumption.

**A figure the source does not state is an explicit known-unknown** — marked
in the data file as `value: null, confirmed: false, reason`, loaded loud, and
never silently filled with a plausible number.

**Tim's station facts** (COM port number, audio device names as enumerated on
his PC, radio menu settings) are configuration, not constants: HM-OPEN-003.

---

## 5. Definition of done

A change is done when all of these hold. Not most.

1. **It builds**, with the project's own toolchain, no new warnings.
2. **Decoder and protocol work carries tests** — against WAV fixtures and
   canned CI-V byte sequences respectively (HM-DEC-007). UI shell work is
   validated by running it; no coverage theatre.
3. **A test names what it proves**, referencing a requirement or decision id.
4. **Determinism holds** for everything below the UI: same fixture in, same
   text out. Wall-clock dependence in a decoder is a defect.
5. **It is observable** (§0.0.1): state changes and decodes appear in the
   app's own record.
6. **The records are updated** in the same delivery if a question was
   answered or a ruling made.

---

## 6. Standards

This is a fresh repository; these rows are **decided**, not discovered.
Rows marked `<<<FILL IN>>>` await the named ruling.

| Item | Value |
|---|---|
| Language / runtime | C# on .NET 8 LTS |
| UI framework | Avalonia 11, Fluent theme, dark variant (HM-DEC-011). Compiled bindings on by default |
| MVVM toolkit | CommunityToolkit.Mvvm (source-generated `[ObservableProperty]`, `[RelayCommand]`) |
| Solution layout | `Hamlet.sln` at root; `src/` and `tests/` solution folders. Engine: `src/Hamlet.RadioEngine`. Shell: `src/Hamlet.App`. Live activity sources: `src/Hamlet.RadioEngine/Explore`. Signal synthesis and mode audio: `src/Hamlet.RadioEngine/Training`. Tests: `tests/Hamlet.RadioEngine.Tests`, `tests/Hamlet.App.Tests` (settings, telemetry payloads, freshness rule — app-layer facts with public promises attached) |
| Project settings | `Nullable` enabled, `ImplicitUsings` enabled, `TreatWarningsAsErrors` true — all new projects |
| Test framework | xUnit, no mocking framework; seams are hand-rolled interfaces (`IRig`, `IAudioSource`, `FakeRig`) |
| Audio | NAudio (WASAPI) |
| FFT | `<<<FILL IN — FftSharp vs Math.NET, decide when the audio pipeline is scaffolded>>>` |
| CI | GitHub Actions, build + engine tests, added once the solution exists |
| Data files | `/data` at repo root; vendored citations in `data/vendor/`; cited Part 97 privileges in `data/privileges/` (HM-DEC-029) |
| Spelling | **American English throughout** — identifiers, comments, prose, records, UI text (HM-DEC-035) |

### 6.1 Spelling — the three exceptions

American spelling is the standard everywhere, with exactly three exceptions:

1. **A quoted external source keeps its own spelling, verbatim.** SOTA's terms
   of service, the CFR, a vendored manual page: a quotation that has been
   tidied is no longer a quotation (§4). This covers `data/vendor/` entirely.
2. **A rename that changes a stored settings key ships with a migration**, and
   a test that proves an existing profile survives it. `LicenceClass` →
   `LicenseClass` renamed the key in `settings.json`; without
   `SettingsMigrations`, the first launch after the upgrade would silently take
   the default and forget the operator's class and its provenance.
3. **The canonical tool scripts under `tools/` are exempt entirely**
   (HM-DEC-036). §9.4 makes `get-files.template.bat` verbatim, and their whole
   value is being known-good and untouched — a rule with a "but it was only a
   comment" exception is not a rule. Their spelling is frozen at whatever it
   is, including `rem --- normalise`.

The second is the one that bites. A spelling change to a *public* identifier is
not cosmetic — check whether it is persisted, sent over a wire, or written into
the telemetry record before making it.

---

## 7. Source control

- Branches: `feature/…`, `fix/…`, `docs/…`.
- **One logical change per commit.**

**Commit format:**

```
<type>(<scope>): <summary>

<body — why, not what>

Refs: <HM-OPEN-### ids>
```

Summary imperative, under 72 characters, no trailing period. `HM-DEC-###`
ids go in the body, not the `Refs:` line.

| Type | Use |
|---|---|
| `feat` | New behavior |
| `fix` | Corrected behavior |
| `test` | Tests only |
| `docs` | Documentation, governance, records |
| `refactor` | No behavioral change |
| `chore` | Tooling, dependencies |

| Scope | Covers |
|---|---|
| `engine` | `src/Hamlet.RadioEngine` |
| `app` | `src/Hamlet.App` |
| `tests` | `tests/` |
| `tools` | `tools/` |
| `docs` | Governance and record files at the root |
| `data` | `/data` — machine facts and vendored citations |

---

## 8. Evidence

The app's own record exists to prove what it heard and what it did (§0.0.1):

1. **Replay** — any decode can be reproduced from a captured WAV plus the
   recorded decoder parameters.
2. **Debugging** — the CI-V log says what was sent and received, verbatim,
   with millisecond timestamps.
3. **Test authoring** — a captured failure becomes a fixture.

**Never-throw discipline**: logging that can crash the app is worse than no
logging. A failed write is dropped and counted, never propagated.

---

## 9. Delivery format for Claude

**§9 has two halves and the wrong one is dangerous.** §9.0 decides which.

### 9.0 Which surface is this session? — READ FIRST

| | Chat (claude.ai) | Claude Code |
|---|---|---|
| Sees the tree | No | **Yes, directly** |
| Gets files | §9.3 listing, §9.4 `get-files.bat` | Reads them |
| Delivers | §9.1 scaffolded zip | Edits files in place |
| Builds and runs | No | **Yes** |
| Governs | §9.1–§9.4 | **§9.5** |

A Claude Code session that follows §9.3/§9.4 is asking Tim to run a batch
script to hand it files it can already read — introducing a stale copy where
none existed. **A rule applied on the wrong surface can create the problem it
exists to stop.**

What holds on **both** surfaces, without exception:

- Complete, working files. Never snippets or "add this after line N".
- Never edit a file whose current content has not been read in this session.
- Make no assumptions. Take no step outside the approved plan. **Ask.**
- The four lists of §9.2.
- Everything in §0 through §8.

### 9.1 Scaffolded delivery — ABSOLUTE, chat sessions

**Every delivery ships fully scaffolded in repo path structure**, as a zip
whose internal paths are relative to the repository root — even for a single
file. Extract over `C:\Source\Hamlet` and files land where they belong.
No loose files, no path instructions in prose.

Files compile without modification. Too large for one response → split into
multiple complete files. Ask for files by repo path, not by name.

### 9.2 Every delivery is presented as four lists and a check-in — ABSOLUTE

No preamble between them:

| List | Contains | Never contains |
|---|---|---|
| **Delivering** | What is in the package, one line each | Reasoning, or how it works |
| **You do** | Numbered actions for Tim, in order | Anything Claude could have done |
| **Expect** | What will happen, and what will go wrong first | Reassurance |
| **Need from you** | Only what genuinely blocks Claude | Questions Claude could answer by reading |

Bullets, not paragraphs. "Need from you" may be empty, and an empty list is a
real answer. Reasoning goes in the delivered file, where it survives the
conversation.

**Every delivery ends with a Check-in block** (HM-DEC-013): the exact
`git add` and `git commit` commands, ready to paste, message in §7 format
covering precisely what the zip contains. Tim commits every file drop; he
never composes the message for Claude's work. If a delivery amends an
uncommitted prior drop, the block says so and amends instead.

### 9.3 The repo listing — RUN THIS FIRST, chat sessions

`tools/repo-listing/get-listing.bat` produces `repo_listing.txt`: every file,
its size, its modified date, the commit, and whether the tree is dirty.
Default repo root: `C:\Source\Hamlet`.

Claude cannot request a file it does not know exists, and cannot deliver into
a structure it has not read. Re-run whenever files are added, removed or
moved. **A stale listing is worse than none.**

### 9.4 Requesting files — ABSOLUTE, chat sessions

**Claude asks for files with `tools/get-files/get-files.template.bat`, copied
verbatim.** Claude changes the marked file-list block and the `Generated`
header line, and nothing else — not the subroutines, not the staging paths,
not the zip mechanism. The subroutines took several rounds to get right on
`cmd.exe`, Claude cannot execute Windows batch to test a replacement, and
`.ps1` does not run on Tim's machine (execution policy).

The script runs from Downloads, double-clicked. It writes `for_claude.zip`
to Downloads. **A prose list of paths is never acceptable.** Pull whole
folders when the work touches a subsystem. A `MISSING` line is loud on
purpose — do not build past it. The zip is verified on arrival: file count
and sizes against `repo_listing.txt`.

**Never edit a governance or record file without pulling the current version
in this session** — even when Claude believes it produced the current version
itself.

### 9.5 Claude Code sessions

Reads the tree directly, edits in place, builds, commits (§7 format) without
per-file confirmation. **A decision that is not in `DECISIONS.md` is not
made** — if code needs a ruling no record contains, the session stops and
asks, exactly as it would for a missing file. Within an approved plan, act;
report once at the end of a work unit.

---

## 10. The dependency graph — graphify

Tim runs `graphify` and supplies its output (`GRAPH_REPORT.md`,
`graph.json`, `manifest.json`) alongside a fresh `repo_listing.txt` at the
start of each conversation (HM-DEC-014). The graph is a navigation aid.

The report header carries the commit it was built from — compare it to the
listing's commit; a mismatch means one of them is stale.

### 10.1 Known blind spots — READ BEFORE TRUSTING IT

Recorded from real use in the parent project, where they produced wrong
conclusions that survived several sessions:

- **Static-class member calls produce no edges.** Graph isolation of a
  static class is NOT evidence of dead code; acting as though it were has
  already proposed deleting live code.
- **Service-locator and reflection calls produce no edges**, likewise
  XAML→ViewModel wiring (`DataContext` set in code, compiled bindings) —
  expect the App's views to look less connected than they are.
- **The file scan has missed files that exist.** Absence from the graph is
  not absence from the repository — the listing is authoritative for what
  exists.
- **Thin communities are omitted from the report.** A node can be in
  `graph.json` and not in `GRAPH_REPORT.md`. Query the JSON before
  concluding something is missing.
- **The graph goes stale silently**, and low cohesion scores on governance
  prose (this file) are noise, not a refactoring signal.

**Rule: the graph raises questions, it does not answer them.** Anything it
suggests about dead code or orphaned classes is verified against the real
tree — the listing or a file read — before it is acted on.

---

## 11. What Claude does at the start of a session

1. Read this file, `OPEN_ISSUES.md` and `DECISIONS.md`.
2. Establish the surface (§9.0).
3. Take the fresh `repo_listing.txt` and graphify outputs Tim supplies with
   the conversation (HM-DEC-014); verify their commits agree. If a session
   will touch code and no listing was supplied, ask for one.
4. Request the files the work needs, whole folders, via the template script.
5. Do not begin work on the strength of a summary. If it is not in a file,
   it was not decided.
