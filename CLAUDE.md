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
| 2026-08-15 | **The transmit path is hardened for a real antenna, and no transmit feature is added.** The break-in precondition gates the send buttons rather than sitting beside them, naming the setting, because a correct frame with a correct acknowledgement and no signal is the worst outcome available on a live day. The abort is proved against a send that is actually running. **The dummy load warning is retired**: the code has run and the test passed, and what replaces it does not pretend to know what is on the antenna socket. Power below a quarter of range is said as a consequence, as a percentage and never a wattage. | He would read a silent transmitter as nobody wanting to talk to him; and a warning nobody needs is one everybody learns to read past. | HM-DEC-074 |
| 2026-08-15 | **Hamlet reads callsigns off the air, and refuses to nearly read one.** Two conditions, both required: the ritual position (after `DE`, before `DE`, before a closing prosign) and every character solid. One dimmed or blocked character and nothing is claimed, with no partial claim and no confidence-marked callsign. A callsign-shaped string in loose text is never claimed. Provenance is inseparable from the name, so an identification Hamlet heard and one a feed reported cannot be blurred, and a name with no known source is not shown at all. Receive only. | A callsign with one uncertain character is also a plausible reading of somebody else's, and a wrong one is worse than none on the day he uses it to decide whether anybody answered. | HM-DEC-073 |
| 2026-08-14 | **Recent stations: ten places the operator has been, beside favorites and behaving like them.** Dwell rather than landing, twenty seconds, taken from the length of one relaxed CQ call and not a setting. Same place is within 200 Hz, read from the spot bucket, as a tolerance rather than a bucket. An entry names a station only where one was identified and is a place otherwise, and the newest visit wins including when it knows nothing. Persisted, starrable into a favorite from the dropdown and the manage window. **HM-DEC-060's Favorites submenu turned out never to have been built**, so the manage window was unreachable; both submenus exist now. | Favorites are places he chose and these are places he was, and most favorites will be born from realizing later that somewhere was worth keeping. | HM-DEC-072 |
| 2026-08-14 | **One edition of the truth: every §4 citation re-verified column-aware against the Full Manual, publication `A7292-4EX-6`**, the newest obtainable. Six page numbers moved and every value held; two rows cited a page 19-14 this edition does not have and merged into the scope rows. The filter-width row was wrong about where the scale lives, command `26`'s skipped bytes also force DATA OFF, and the frequency BCD encoding is verified rather than assumed. Affected rulings gain dated correction notes rather than edits, and a test pins the edition. | A table right in three different books is one nobody can check in a sitting, and that seam had already produced two defects. | HM-DEC-071 |
| 2026-08-14 | **The star moves inside the display and says `save` or `saved`, superseding HM-DEC-060 on placement and label only.** Tim weighed keeping Hamlet's chrome off a faithful LCD against being able to find the control, and chose finding it. A strip along the top of the warm panel carries the favorite's name at the left and the dropdown at the right, and the tuning hint gets its own line back. The hit test reads where the glyph was actually drawn, and the wheel does not tune over it. | A star against near-black is the brightest thing on the panel; and a favorite's name is as long as somebody made it, which collides with the mode badge or the clock at some width. | HM-DEC-070 |
| 2026-08-14 | **Hamlet does not read the radio's RTTY decoder, and the reason is the radio.** Verified against the Full Manual, publication A7292-4EX-5, p. 12-9: "USB Serial Function" is one setting with two options, CI-V or RTTY Decode, on one port, so taking the decoded text costs rig control entirely. The manual also never says what that output looks like on the wire (HM-OPEN-008). The field guide says the tradeoff plainly, the choice stays the operator's and is made at the radio, and a connect failure now names the setting beside the cable and the baud rate. | A terminal fed from that port would stop following the radio the moment it started working, and everything on screen would keep looking right. | HM-DEC-069 |
| 2026-08-14 | **A card's lines are composed together and no card may say the same thing twice.** A clause an earlier line carried is dropped from a later one, splitting on the separator and on commas, with the first line always surviving whole. Spot cards and the lead card compose through it; a new family joins by calling it rather than by being remembered. A sweep over every source, call type, mode and age fails on any repeat, and it was checked against the unfixed code to be sure it fires. | Both lines asked one function for one sentence and neither was wrong to; and a thing said twice reads as two pieces of evidence when it is one. | HM-DEC-068 |
| 2026-08-14 | **The waterfall says why it is empty and names the two menus that control it**, quoted as the radio names them, with the path. Shown only where no waveform data has arrived, including the case where both settings read as on. No fault language, held by a test. The collapsed summary stops saying "receiving" while nothing arrives. **Narrows HM-DEC-050**: consequences-never-instructions covers settings Hamlet reads and judges, and does not cover a feature that is inert until a switch only the operator can reach is thrown. | An empty waterfall that says nothing reads as a broken program while the answer is a pair of menu screens away, and neither switch is a command Hamlet can send. | HM-DEC-067 |
| 2026-08-14 | **Morse copy speed becomes a setting, defaulting to 13 words a minute**, which is the ranking's own relaxed threshold read off the existing scale rather than a second number beside it. The speed bands slide to wherever it is set, so a fresh install ranks exactly as before. Hamlet may say a station is far over the stated figure, because it was stated; it still may not say anybody can or cannot copy something. Nothing is filtered and nothing is hidden. | The app weighed sending speed with no way to hear what the operator wanted; and a speed box in a radio program reads like a test unless the copy says otherwise. | HM-DEC-066 |
| 2026-08-14 | **An unresolved license class warns and labels, and never blocks. Confirms HM-DEC-029 rather than amending it.** The guard is unchanged: it permits, says what it does not know, and gets out of the way. Where a send control sits, one label beside the buttons says Hamlet cannot check this frequency and leaves the license to the person who holds it. Nothing reads it but the label. | A brief claimed the guard refuses on an unknown class and it does not; and locking somebody out of their own transmitter over a failed lookup would teach a beginner something false about their own license. | HM-DEC-065 |
| 2026-08-14 | **The Explorer's panels are reordered: where to start, happening now, field notes, field guide, what a contact sounds like.** The rig display stays above them all and stays the one that does not collapse. Collapse state is keyed by panel and never by position, and a test writes a pre-move settings file by hand and reads it back. | The first two get somebody on the air and the last three explain what they are hearing; six years of understanding without a contact is the problem this application exists to solve. | HM-DEC-064 |
| 2026-08-14 | **The version is 1.2.0, and semantic versioning is the convention**, recorded here because none existed. Major breaks an existing setup or reconceives the application, minor adds a capability the operator can see and use, patch fixes and polishes. The number lives in `Directory.Build.props` alone and About reads it off the assembly. `CHANGELOG.md` is an index of which rulings shipped in which release and deliberately not a second copy of their reasons. | An application that can key a transmitter for the first time is not a patch; and a version written down twice is a version that drifts. | HM-DEC-063 |
| 2026-08-14 | **The radio's own spectrum reaches the waterfall, CI-V `27 00`. Reads only.** A sweep in eleven parts, span read off the wire or the row is not drawn, a sweep with a hole dropped rather than patched, and drops counted. Nothing turns the scope on: the two settings are read and what is missing is said, including the two CI-V menu settings that are not commands at all. The stream issues no commands, so the poll loop cannot be starved. | The radio computes the panadapter already, and a waterfall that sat empty without saying why would look broken while the answer was four menu screens away. | HM-DEC-062 |
| 2026-08-14 | **Three family chips at the head of the happening-now panel: Morse, Digital, Voice.** Multi-select, all on by default, in their family colors, persisted, and named in the collapsed summary. **Each carries a live count that shows even when the family is switched off**, and every chip off shows everything rather than an empty panel. They filter and never delete, and compose with the two lenses. | Somebody who filters to Morse and still sees forty-one voice stations learns the band is full of people they could talk to, which is the fact this app exists to reveal. | HM-DEC-061 |
| 2026-08-14 | **Favorites, which carry the reason.** Saving captures the frequency, mode, band and neighborhood with nothing typed, so one reads "14.074, FT8 city" rather than "MEM 07". A star that names where you are and un-saves on a second press, a dropdown beside it, and a Favorites submenu with a manage window under Radio. On the warm panel below the LCD and never inside it. | The radio's own memory channels are numbered slots whose meaning you have to remember, which is the problem rather than the answer; and Hamlet already knows what lives where. | HM-DEC-060 |
| 2026-08-14 | **Hamlet keys the radio and sends Morse**, by handing text to the radio's own keyer with CI-V `17`. One door to the transmitter that calls the guard first, a same-thread abort that awaits nothing, the break-in precondition checked before the send rather than reported after, and nothing that could key unattended. Contextual send buttons, staged sending on by default, a phrasebook with a column for admitting you are new, speed offered and never asserted, and a closing card that is not a logbook. **USB keying and Farnsworth are deferred to their own ruling**, and the absence is stated in the UI rather than hidden. | The app has been walking toward this, and it belongs to somebody who has held a license for six years and made one contact. | HM-DEC-059 |
| 2026-08-14 | **The happening-now list ranks for what a newcomer can work, not for distance.** Distance does not run in a straight line with workability on HF, and a distance-led sort would bury every RBN spot besides, so it stays on the card and earns a vote when FG-007 lands. Liveness comes from the lens machinery and runs from a penalty to a bonus, so a finished activation cannot outrank a live CQ. Weights in one named place with their reasons, no control for them, and speed describes the station and never matches the operator. | Sorting nearest-first would put the hardest contacts at the top and call them the best chance; and Hamlet has never asked what speed this person can copy. | HM-DEC-058 |
| 2026-08-14 | **The happening-now panel gets two named lenses, not a refresh button.** "Best chance" is the arrival question and ranks everything alive; "what's new" is the between-contacts question and shows the delta since the operator last looked. Both always visible, age fading the display across each source's ruled lifetime, inference may choose which opens and may never override afterward, and nothing is ever deleted. **Recorded, not built.** | Two different questions, and refresh answers neither; and a newcomer learning that people hunt again after a contact is worth the control on its own. | HM-DEC-057 |
| 2026-08-14 | **Hamlet writes to the radio for the first time, and the write is the mode: tuning into a neighborhood sets the mode it is worked in.** Command `26` and not `06`, because only 26 carries the data flag and USB is not USB-D. Visible setting on by default, the operator's own hand always wins and suspends it visibly until the next band change, one write per settled dial, narrated in the app's voice, and a write the radio did not confirm leaves the mode unknown rather than assumed. **This is the writes ruling HM-DEC-050 deferred.** | Having the mode wrong is the commonest reason a beginner hears nothing, and the app already knows what lives where. | HM-DEC-056 |
| 2026-08-14 | **One out-of-band fact in the engine, read by every surface that speaks.** The map draws past both band edges and labels what is out there in a cold gray belonging to no mode family, distinct from listen-only and from open; the card goes amber and never invites; the dial tape and the rig display speak from the same derivation. The dial stops at the end of the picture rather than the end of the band, and a frequency the radio reported is never clamped at all. | At 14.350 the card said "yours to use, call away", because the overlay read "past the end of my data" as "no restriction found" in the one place a confident error has legal consequences. | HM-DEC-055 |
| 2026-08-14 | **The neighborhood map becomes cited data under `data/bands/`, and the digital watering holes are on it.** PSK31, FT8, FT4, JS8 and RTTY across all seven bands, every row carrying the source it was read from, with what could not be sourced declared as an explicit unknown. Band edges are derived from the cited Part 97 file rather than transcribed again, unclaimed stretches are open ground rather than Morse, and the card stops saying "Call away" where the crowd cannot hear Morse. | The map said 14.000 to 14.150 was Morse and the card invited a Morse call at 14.074, which is true about the regulation and would have put a beginner on top of the busiest digital block on Earth. | HM-DEC-054 |
| 2026-08-14 | **Broadcast is a provenance, not an absence.** A field the radio volunteers is supported and populated; `Unsupported` is reserved for what the capabilities record says the rig genuinely lacks. Fixed in the taxonomy rather than as a special case, which caught the filter designator too, and the frequency gains a read of its own for the connect sweep. | The diagnostics screen said "not on this radio" against the frequency while the rig display an inch above it showed the live one, which is the app denying what it holds on the surface built to prove what it holds. | HM-DEC-053 |
| 2026-08-14 | **Hamlet reconnects to the last radio when it opens, as a setting that ships on**, beside the audio settings. It never blocks the window and never shows a dialog, it tries once and never in a loop, and every failure lands on the training radio with one sentence in the status line. A port that is no longer on the machine is named as itself rather than reported as a generic failure, and the fallback leaves the remembered port alone. | Connecting is the one thing done every single time; and Windows renumbers a USB radio often enough that "could not connect" sends somebody to check a cable that was never the problem. | HM-DEC-052 |
| 2026-08-14 | **Three rules from the first evening on a real radio:** teardown returns promptly whatever the port does, since Windows' serial read ignores its cancellation token and cancel-then-await hung disconnect forever; the window is one vertical scroller with a permanently visible bar and a pinned header, because the tape eats the wheel; and the terminal's Clear wipes the display and never the decoder's speed, tone or noise floor. | Disconnect did not work at all, the CW terminal was unreachable on a 1080p screen, and a clear that cost the decoder what it had worked out would land in the worst possible moment. | HM-DEC-051 |
| 2026-08-15 | **Hamlet keeps a model of the radio's whole state**, from twenty-five cited CI-V reads across twenty-eight fields and the broadcasts the radio already sends. Unknown is a state and never a number, distinct from unsupported and from undocumented. Polling is rationed: fast values only while the window is visible, settings on connect and rarely, nothing polled that the radio volunteers, one command in flight, a timeout marks unknown rather than retrying. **The hardcoded "CW" and "FIL2" badges are corrected**, the S-meter is fed, and a diagnostics screen under Tools shows every value with its age and provenance. **Corrects §4: the CW pitch is `14 09`, not `14 08`.** Reads only; writes get their own ruling. | The first live connection produced garbage and took half an hour to diagnose by asking somebody to walk to the radio and read menu settings out loud, and the filter was wide open the whole time. | HM-DEC-050 |
| 2026-08-14 | **The IC-7300 Full Manual is read and §4 is verified with page citations.** Command `17` takes 30 characters, `FF` stops, `^` runs characters together; `27 00` needs `27 10` and `27 11` on; CW pitch is 300 to 900 Hz. **Two corrections: USB CI-V baud defaults to Auto, and the manual states no default CW pitch.** The manual is cited and never committed, because Icom permits individual use and prohibits redistribution. Closes HM-OPEN-002. | Code must not rest on a recalled command byte; and the transmit precondition nobody had written down would have cost an evening. | HM-DEC-049 |
| 2026-08-14 | **Hamlet decodes received CW, and says how sure it is about every character.** Goertzel bank tracking the note anywhere in 300 to 900 Hz, adaptive gate that follows a fade down, speed re-derived from a rolling window, prosigns as prosigns. Confidence from two measurements with the worse one winning, plus a veto for a contested signal; low renders dimmed and unresolved renders as a placeholder, never a guessed letter. Nothing raises a score. Nothing is emitted at all when the timings do not look like Morse. Poor decodes get plain-language notes that describe measurements and are tested against diagnosing the band or anybody's equipment. Audio behind `IAudioSource`; seven synthesized WAV fixtures in `tests/fixtures/cw`. Receive only. | CW is the last part of this hobby still guarded by the claim that you need an ear for it, and a beginner reading clean-looking garbage concludes the fault is theirs. | HM-DEC-048 |
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

**EVERY ROW BELOW WAS RE-VERIFIED 2026-08-14 AGAINST ONE EDITION**, and the
edition is part of the citation rather than a detail:

> **IC-7300 Full Manual, publication `A7292-4EX-6`, © 2016–2018.** Chapter 19
> runs 19-1 to 19-13, chapter 12 runs 12-1 to 12-12. Read with a column-aware
> extraction (`pdftotext -table`), which is not optional: the command table is
> two columns, and a flattened read is what put the CW pitch on the wrong
> sub-command and cost weeks (HM-DEC-050, HM-DEC-071).

This table used to span three printings, with each block naming its own, and
that seam produced two defects. It now points at one document. **A page number
here is a page number in `A7292-4EX-6` and nowhere else**; a different printing
paginates differently, so a figure checked against another edition is checked
against a different book. The earlier page numbers are not deleted from the
rulings that carry them, because a ruling is never edited (§1). They are
corrected in place with a dated note, which is what the entries below the table
are.

The PDF is **cited and never committed**: Icom's terms permit individual use and
prohibit redistribution, and §2.1 forbids third-party proprietary material in
this repository. Anybody checking a figure downloads the manual free from Icom
and turns to the page named.

**TWO NOTATIONS, AND MISREADING ONE FOR THE OTHER IS A REAL HAZARD.** Where a
row below writes a value as byte pairs like `01 28`, that is the BCD on the wire.
The manual writes the same value in its own column as the decimal `0128`. They
are the same number in two alphabets, and reading `02 55` as hexadecimal gives
597 rather than 255. Where both appear below, the manual's decimal is in
parentheses.

| Fact | Value | Page |
|---|---|---|
| CI-V frame, controller to radio | `FE FE 94 E0 Cn Sc <data> FD` | 19-2 |
| CI-V frame, radio to controller | `FE FE E0 94 Cn Sc <data> FD` | 19-2 |
| Acknowledged / not acknowledged | `FE FE E0 94 FB FD` / `... FA FD` | 19-2 |
| Radio address | `94h` default; range `02h ~ 94h ~ DFh` | 12-8 |
| CW message send | Command `17`, **up to 30 characters** | 19-11 |
| Stop sending | `FF` as the message | 19-11 |
| Run characters together | `^` sends a string with no inter-character space | 19-11 |
| CW message character set | 0-9, A-Z, a-z, and `/ ? . - , : ' ( ) = + " @` and space, as ASCII codes | 19-11 |
| CW pitch range | 300–900 Hz | 4-14 |
| CW pitch over CI-V | `14 09`; `00 00`=300 Hz (0000), `01 28`=600 Hz (0128), `02 55`=900 Hz (0255), 5 Hz steps | 19-3 |
| S-meter | `15 02`; `00 00`=S0 (0000), `01 20`=S9 (0120), `02 41`=S9+60 dB (0241) | 19-3 |
| IF filter width | `1A 03`, an index `00` to `49`. The table gives the **endpoints** (`00`=50 Hz, `31`/`40`=2700/3600 Hz, AM `00`=200 Hz to `49`=10 kHz); the **step scale** is on p. 4-6 | 19-4, 4-6 |
| IF filter scale | SSB/SSB-D/CW 50–500 Hz in 50 Hz steps then 600 Hz–3.6 kHz in 100 Hz; RTTY the same to 2.7 kHz; AM 200 Hz–10 kHz in 200 Hz; **FM fixed**, FIL1 15 kHz / FIL2 10 kHz / FIL3 7 kHz | 4-6 |
| Read operating mode and filter | `04`; command table on 19-3, data content on 19-8. Filter byte `01`=FIL1, `02`=FIL2, `03`=FIL3 | 19-3, 19-8 |
| CI-V USB baud | **Default Auto**; 4800/9600/19200/38400/57600/115200 | 12-9 |
| CI-V USB port | **Default "Link to [REMOTE]"**; the other option is "Unlink from [REMOTE]" | 12-8 |
| CW keyer menu | `MENU > KEYER > EDIT/SET > CW-KEY SET`: side tone level, side tone limit, keyer repeat time, dot/dash ratio, rise time, paddle polarity, key type, MIC up/down keyer. **No character spacing, so no Farnsworth**, and the word appears nowhere in the manual | 4-21 |
| USB keying (CW) | Connectors set screen; OFF / DTR / RTS. The second keying path, not built | 12-9 |
| Scope waveform data | `27 00`. First part is header only; parts 2 and later carry waveform. Division current `01`–`11`, division maximum `11` over USB, center-or-fixed flag, waveform information, out-of-range flag, then the data. **Data range 0–160, data length 475** | 19-12 |
| Scope preconditions | `27 10` scope ON and `27 11` wave data output ON. **`27 11` needs CI-V USB Port = "Unlink from [REMOTE]" and CI-V USB Baud Rate 115200** (footnote 4) | 19-7 |
| Command `17` precondition | In CW mode a `17` message transmits **only** when TRANSMIT or an external TX switch is on, or Break-in is on (footnote 2) | 19-7 |
| Set operating mode | `06`, mode byte then optional filter byte; **no data variant**. Omitting the filter selects that mode's default | 19-8 |
| Set mode, data variant and filter | `26`, VFO selector then mode then `00`/`01` data mode then optional filter. **Omitting them selects DATA OFF and that mode's default filter**, so the data byte must be sent to get a data mode | 19-11 |
| Read mode, data variant and filter | `26` with the VFO selector alone; the only read that tells USB from USB-D | 19-11 |
| Mode bytes | `00`=LSB, `01`=USB, `02`=AM, `03`=CW, `04`=RTTY, `05`=FM, `07`=CW-R, `08`=RTTY-R | 19-8 |
| Frequency encoding | Commands `00`, `03`, `05`, `1C 03`. **Five bytes, least significant pair first**, two BCD digits per byte with the more significant in the high nibble: `[10 Hz][1 Hz] [1 kHz][100 Hz] [100 kHz][10 kHz] [10 MHz][1 MHz] [1000 MHz][100 MHz]`, the last pair fixed at zero. **Verified 2026-08-14, no longer carried from general knowledge** | 19-8 |
| USB serial function | **One setting, two options, one port: `CI-V` or `RTTY Decode`.** Choosing the decoded text means CI-V stops (HM-DEC-069) | 12-9 |
| RTTY decode baud rate | Default 9600; 4800/9600/19200/38400 | 12-9 |
| RTTY mark frequency / shift | `1A 05 0036` (`00`=1275, `01`=1615, `02`=2125 Hz) and `1A 05 0037` (`00`=170, `01`=200, `02`=425 Hz) | 19-4 |
| RTTY decode output format | **NOT STATED ANYWHERE IN THE MANUAL.** Explicit known-unknown: HM-OPEN-008 | — |

**WHAT CHANGED WHEN THE TABLE WAS BROUGHT ONTO ONE EDITION (HM-DEC-071).** Six
page numbers moved, and every value they pointed at was confirmed unchanged. The
radio address is on 12-8 rather than 12-10; the CI-V USB baud rate on 12-9 rather
than 12-11; the three command `17` rows on 19-11 rather than 19-13; command `04`'s
data content on 19-8 rather than 19-9; footnote 2 on 19-7 rather than 19-8; and
`1A 03` on 19-4 rather than 19-3. **Two rows cited a page 19-14 that does not
exist in this edition**, whose chapter 19 ends at 19-13, and both were duplicates
of the scope rows already read from `A7292-4EX-6`, so they merged. The old `00`–`A0`
and the current `0–160` are the same range in hexadecimal and decimal; the manual
writes decimal, so the table now does.

**THE ONE PLACE THE TABLE WAS SIMPLY WRONG** was the IF filter width. It said the
scale is on p. 4-6 "and not in the command table", and the command table does
carry the endpoints: `00`=50 Hz, `31`/`40`=2700/3600 Hz, and AM `00`=200 Hz to
`49`=10 kHz. Only the step scale needs p. 4-6. The code was already right about
this and its comment said so, which is how the row was caught.

**AND ONE ROW GAINED A CLAUSE THAT MATTERS.** Command `26`'s skippable bytes were
recorded as "skipping the filter selects that mode's default". The manual says
both settings can be skipped, and that "DATA OFF" **and** the default filter are
then selected. So a `26` sent without the data byte turns the data variant off
rather than leaving it alone, which is the opposite of harmless for HM-DEC-056's
mode writes.

**A SECOND TYPO IN THE MANUAL.** Page 19-12 says "See page 19-14 for Scope Fixed
edge frequency settings" and this edition's chapter 19 ends at 19-13. The
settings themselves are on 19-13. Recorded for the same reason as the one below:
the next session to read that page will see it too.

**A TYPO IN THE MANUAL, FOUND AND RECORDED.** The `27 00` row's own description
says the waveform data is output only when `27 10` and **`27 20`** are on. There
is no `27 20`: the sub-command list on the same page runs 00, 10, 11, 12, 13, 14,
15, 16, 17, 19, 1A, 1B, and `11` is "Send/read the Scope wave data output". So
`27 10` and `27 11` is right, which is what HM-DEC-049 already recorded, and the
cross-reference beside it is wrong. Worth writing down because the next session
to read that page will see the same contradiction.

**WHY THE `26` ROWS ARE THE ONES THAT MATTER (HM-DEC-056).** Command `06` sets a
mode and a filter and has no way at all to say whether the data variant is
wanted, so a radio told "USB" by `06` lands in voice USB with the microphone live
rather than in USB-D routing the computer's audio, which is the difference
between hearing FT8 and hearing nothing useful.

**CORRECTION 2026-08-15 (HM-DEC-050), CONFIRMED AGAIN 2026-08-14 against
`A7292-4EX-6` p. 19-3: the CW pitch was recorded here as `14 08` and it is
`14 09`.** Sub-command 08 is the outer Twin PBT position. The command
table is two columns and the extraction that verified it had been flattened into
one, so the description landed against the wrong row. Issuing 08 with a payload
would move the passband while trying to read a pitch. **A citation is only as
good as the extraction it came from: use a column-aware read on a two-column
table, and check a value against a second mention of it where one exists.**

**THE PRECONDITION ON COMMAND `17`, which is not optional and cost nothing only
because it was found before the transmit work started:** in CW mode a message
sent with `17` is transmitted **only when TRANSMIT or an external TX switch is
on, or Break-in is on** (p. 19-7, footnote 2, corrected from 19-8 by HM-DEC-071).
Without it the app sends a correct frame, gets a correct acknowledgement, and the
radio stays silent.

Still unverified, and marked so rather than filled in:

- **What Windows calls the audio codec.** The manual describes the USB
  connection and never names the device as an operating system enumerates it.
  This is configuration, not a constant: HM-OPEN-003. `LooksLikeRadioCodec`
  matches "USB Audio CODEC" to **preselect** a device and never to claim one is
  the radio.
- **The factory default CW pitch.** The manual states the range and the CI-V
  encoding and no default. The decoder starts at 600 Hz because that is the
  midpoint of what this radio produces, which is a citation and not a
  recollection.

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
| Solution layout | `Hamlet.sln` at root; `src/` and `tests/` solution folders. Engine: `src/Hamlet.RadioEngine`. Shell: `src/Hamlet.App`. Live activity sources: `src/Hamlet.RadioEngine/Explore`. Signal synthesis and mode audio: `src/Hamlet.RadioEngine/Training`. Audio input and the WAV round trip: `src/Hamlet.RadioEngine/Audio`. CW decoding: `src/Hamlet.RadioEngine/Cw`. Rig state, polling and the CI-V read table: `src/Hamlet.RadioEngine/Rig` and `Civ` (HM-DEC-050). Tests: `tests/Hamlet.RadioEngine.Tests`, `tests/Hamlet.App.Tests` (settings, telemetry payloads, freshness rule — app-layer facts with public promises attached) |
| Project settings | `Nullable` enabled, `ImplicitUsings` enabled, `TreatWarningsAsErrors` true — all new projects |
| Test framework | xUnit, no mocking framework; seams are hand-rolled interfaces (`IRig`, `IAudioSource`, `FakeRig`) |
| Audio | NAudio (WASAPI), in the engine behind `IAudioSource` (HM-DEC-048) |
| Decoder fixtures | `tests/fixtures/cw` — synthesized WAV, 8 kHz 16-bit mono, regenerated byte for byte from the request beside each one (HM-DEC-007, HM-DEC-048) |
| FFT | **None, and that is the answer rather than a deferral** (HM-DEC-048). The audio pipeline is scaffolded and the CW decoder wants a couple of dozen known frequencies rather than a whole spectrum, which is a Goertzel filter bank: a handful of multiplies per bin per sample, nothing to allocate, no dependency. The question reopens if phase 3 needs a wideband transform in software, and the waterfall does not count because the radio computes that itself (HM-DEC-005) |
| CI | GitHub Actions, build + engine tests, added once the solution exists |
| Data files | `/data` at repo root; vendored citations in `data/vendor/`; cited Part 97 privileges in `data/privileges/` (HM-DEC-029). A source whose terms forbid redistribution is cited by page and never vendored (HM-DEC-049) |
| Spelling | **American English throughout** — identifiers, comments, prose, records, UI text (HM-DEC-035) |
| Versioning | Semantic, set once in `Directory.Build.props` (HM-DEC-063). Major breaks an existing setup or reconceives the app; minor adds a capability the operator can see and use; patch fixes and polishes. `CHANGELOG.md` indexes which rulings shipped in which release and carries none of their reasoning |

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
