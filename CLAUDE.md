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
| `ANNUNCIATOR.md` | **How status is reported to the panel** — the two files, the six status fields, and when a session writes them. Summarized inline in §13, because a companion file is read only if something points at it (HM-DEC-132) |
| `PROJECT_CARD.md` | **Standing facts, measured and quoted rather than composed**: where the repository is, what it can do to the physical world, the ratings it may never infer, the commands, and the prime directive verbatim from §0.0 (HM-DEC-131) |
| `PROJECT_STATUS.md` | **Volatile state, written at each state transition and at no other time**: who must act next, what is measured, and what is being assumed while nobody has ruled (HM-DEC-131) |
| `SHACK_FACTS.md` | **Standing ground truth about the operator's own station.** Read before writing a word about the radio's menus; a fact here outranks any inference a session draws from its own reading (HM-DEC-093) |
| `SESSION_PROTOCOL.md` | **How a work unit is scoped and how a session reports.** The four-part test for what a session may record itself, the three mandatory report headings, and the phase discipline (§12, HM-DEC-096) |
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

**This binds pictures as hard as sentences** (HM-DEC-092). A waterfall, a meter,
a bar or a chart asserts that a signal is at a frequency, that a band is busy,
that one thing is louder than another. It is more persuasive than a sentence and
harder to catch, because nobody reads a picture sceptically and there is no
wording to object to. A display draws what was measured; an empty band renders as
an empty band; an axis is a claim and carries real values from one source of
truth; and "no data" and "data that is all noise" are different pictures.

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

**An automated transmit cycle may be built and exercised into a dummy load
only** (HM-DEC-098). It does not reach an antenna on the strength of this
clause. Whether §0.2's first sentence is amended to permit an attended
automatic cycle on the air is a **separate ruling, taken after every interlock
has been watched to fire into the dummy load** — including the link being
pulled mid-cycle. Reasoning about an interlock is not the same as seeing it
work, and this is the one feature where the difference is somebody else's
band.

### 0.2.1 Tuning writes — the scanner moves the operator's dial

Reading the radio is unrestricted. **Changing the VFO is a different category**:
it moves the dial out from under the person sitting at it, and a scanner does it
repeatedly and unasked. Proposed by Claude 2026-08-17; ratified by Tim's commit
of this file. HM-DEC-107.

- **Never while transmitting**, and never while a transmit path is armed.
- **The starting frequency is remembered and restored** when scanning stops, by
  any route including a crash-safe path on next connect.
- **Scanning stays inside a band-plan segment the operator configures in a data
  file he edits.** Frequencies are never asserted from a model's memory, per §0
  on generated-from-a-source-of-truth data.
- **Abort instantly** on: the operator touching the dial or PTT, rig state going
  unknown or stale, or the link failing to answer a read. Silence is a stop.
- **Refuse to start** unless `RigStateMonitor.Populated` is satisfied — the gate
  added after three separate faults raced the same poll sweep.
- **One obvious, always-visible stop control**, and the scan states plainly that
  it is moving the dial.
- A scan **never transmits** (§0.2), and never runs while an automatic transmit
  cycle is armed (HM-DEC-098).

Practical test: could the operator walk away mid-scan, come back, and be unable
to tell where his radio had been left or why? If yes, the scan is wrong.

### 0.3 Terse, and how questions are asked

Claude answers short. Point first, no preamble. **Walls of text are the
enemy.**

Questions follow a fixed protocol (HM-DEC-010):

- **One question at a time.** Probe as deeply as needed — follow-ups on the
  same question are fine — before moving to a second question.
- Every question is a **clear decision ask**: option A, option B, option C,
  each with pros and cons, as a table.
- Claude states the industry-standard answer and why (§0), then Tim rules.

### 0.3.1 Every prompt is gated by project name — ABSOLUTE

Tim runs several projects and can put the wrong prompt into the wrong session.
The cost of that is a session confidently editing a repository it was never
written for.

**Every work order and every pasted prompt opens with `PROJECT: Hamlet`.**

- A session whose **first action** is not verifying that gate has already
  failed. Verify against the tree, not against the prompt's own claim: this
  file's header says `Project: Hamlet`, the solution is `Hamlet.sln`, the
  namespaces are `Hamlet.*`.
- **A prompt with no gate line is not executed.** Say so and stop.
- **A prompt gated to another project is not executed, not adapted, and not
  partially applied.** Say which project it names and stop. Do not reason
  about whether it might still be relevant here.
- The gate is checked once, before reading the work order, and the report's
  `STATE` block records that it passed.

Claude's own deliveries carry the gate too. A work order Claude writes without
`PROJECT: Hamlet` on its first line is a defective delivery and is redone.

### 0.4 Tim rules, Claude executes

Tim is the architect and owns the outcome. Claude makes no assumptions, no
decisions, and no forward progress without his say.

Raise a thing once, then stop. Committing, publishing and rulings are his.
Never ask him to re-confirm a rule he has already given. Executing inside an
approved plan is not deciding — building the files the plan names needs no
per-file permission.

**Tim does not edit files.** He downloads, extracts, runs and commits. A
delivery that requires him to hand-edit a line, paste a block into a file, or
patch anything by hand has failed §9.1 and is redone. Asking him to make an
edit that Claude could have made is the same defect as asking him to run a
script Claude could have avoided needing.

**One narrow class of conclusion is now the session's to record, not Tim's**
(§12.1, HM-DEC-096): an entry the governing principles decide one way, that
supersedes nothing, that weighs no trade-off, and that the report reproduces in
full. Everything else still comes back to him, and the attribution rule is
untouched — no entry ever claims his authority for a ruling he did not make.

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
  app's anchor, and **HM-DEC-086 widens that exemption to the whole top strip**
  it sits in: band, frequency, mode, where you are and whether you may transmit
  are not on the canvas, cannot be closed and cannot be moved.
- **A widget that is not on the canvas still carries its news** (HM-DEC-086).
  The same rule one level up: taking a panel off removes a display and never a
  subscription, so a quiet line says what is happening with one press to bring
  it back, and nothing accumulated while it was away is lost.

Practical test: could the operator shut this panel and still know what it
would have told them at a glance? If not, the summary is wrong.

### 0.5.1 What a control looks like — standing rule

**A control's resting appearance says it can be pressed. Grey is reserved for
what genuinely cannot be used** (HM-DEC-087). Fixed once, in the application's
own styles, and never again per screen: the same fault was fixed narrowly three
times before it was fixed here.

**A binding that does not resolve is a defect, not a diagnostic.** Avalonia
yields null on a failed cast rather than throwing, and a button whose command is
null renders and behaves exactly like a disabled one, so the failure is silent
and its symptom is indistinguishable from a design decision. `BindingHealthTests`
builds the real window headless and fails on any unresolved binding. There is no
acceptable number of them.

**The one exception is the law** (HM-DEC-089). Where the operator's license
does not cover transmitting at this frequency, the send controls are disabled and
that is the answer rather than a fault. It still says why, and it still says what
would change it.

Practical test: can the operator tell a live control from a dead one without
pressing it? If not, the style is wrong, whatever the state machine says.

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

**Where a new row goes: immediately below the `|---|` separator, above every
existing row.** A batch of several goes in **newest first**, which is the reverse
of the order they were ruled in. Both halves of that were got wrong in the field —
`5d00bd4` and `303c4f4` inserted below the top row rather than above it, and
`d263f95` pasted four rulings in the order they were made — and neither was
noticed for days, because the table is a hundred and thirty-nine rows long and
nothing checked it. `DecisionLogOrderTests` checks it now.

| Date | Decision | Why | Ref |
|---|---|---|---|
| 2026-08-21 | **Hamlet shows the receive path's own settings and says what they mean, and does not write them; where it can name the control standing between the operator and a readable signal, it does.** On 20 metres in daylight at S9 with nothing readable, the sidecar carried `Overflow: overloading`, `Preamp: preamp 1` and `RfGain: 100%` — **Hamlet was reading the overflow flag four times a second and showing him none of it**, and he found the answer in a text file the next day. The front end was compressing the whole passband: 16 to 17 dB of swing at every pitch from 450 to 700 Hz where a real station gives 22 to 24 at one. **The ear takes rhythm out of a compressed mess and the decoder, which measures amplitude, cannot** — so the decoder was right to read nothing and a switch on the front of the radio was the fault. **A diagnosis is not help**: the message names **P.AMP/ATT**, and mentions the attenuator only once the preamp is already off. **Read only**, because a write is still the app changing his radio underneath him (HM-DEC-056); a later unit may offer a button he presses. **A value never read says so**, and **`RfGain` is not on the panel at all** until its 100-per-cent reading is explained — the leading explanation being that the RF/SQL knob is configured as squelch only (`1A 05 0025`), which Hamlet can write and cannot read. | Something the app knows and does not say is the same defect as a decode with no signal behind it. | HM-DEC-148 |
| 2026-08-21 | **Hamlet suspends decoding while the radio is transmitting, takes that state from the radio and never from the audio, and never lets sent text enter the received stream.** The operator keyed by hand and the terminal filled with his own sending, with nothing on screen to say so: break-in is `full`, so the receiver opens between elements and the sidetone arrives chopped by transmit-receive switching and decodes as a page of isolated letters. **The decoder was behaving correctly on input it should never have been given.** The subtler half is what makes it a ruling: with CW transmit built, Hamlet would decode its own sent text back and an operator could read his own callsign returning and believe somebody answered. **The state is CI-V `1C 00`**, read four times a second for months, displayed correctly on the diagnostics screen for months, and consumed by nothing. **Suspension is immediate and resumption waits half a second**, asymmetric on purpose, and the figure is measured against two poll intervals and `CwTransmitGuard`'s twenty-four milliseconds of transmit-receive hang rather than against break-in, which at tens of milliseconds the poll cannot see at all. **Not knowing is not transmitting**, because a decoder silenced by a quiet link is a band that reads as empty. **Nothing is held and released later**; the audio is dropped before a decoder sees it. | A confident wrong answer with the operator's own hand on the key is the one this application is least able to survive. | HM-DEC-147 |
| 2026-08-20 | **HM-DEC-119's mark-length figures hold at a hundred milliseconds and do not hold below it.** Supersedes it on that point and on nothing else. Measured on generated audio with no noise and a dit known to the millisecond: a true 100 reads 102.8, 101.4 and 102.6 across three fixtures, **a true 48 reads 45.3 and a true 56 reads 48.9**. **The dahs are long by nought to two per cent at every length**, so it is short marks specifically, and the loss is not a fixed number of milliseconds either. **The de-glitch is cleared**: bypassed by clamping the vote window to one measurement the error gets *worse*, `farnsworth-heavy` going -13% to -19% and `fast-easy` -6% to -12%, so `ShortestVote` stays at 5 on evidence rather than on a park. **Why the correction matters more than the figure**: 119 was measured at one speed and generalised to every speed, and that generalisation was the whole argument for `Refine`'s averaging — a mark long and the next gap short by the same amount cancel. At 56 ms the mark reads short and the gap reads true, so there is nothing to cancel. **What does not follow**: the mechanism is unnamed, and the analysis window is narrower where the error is worse, so a rounded top does not explain it. | A ruling right about the audio it was taken from and wrong about the rest is the most expensive kind, because everything downstream cites it rather than re-measuring. | HM-DEC-146 |
| 2026-08-20 | **`cw-2026-08-17-013347` holds a station and its callsign is `VA3VRR`.** The second adjudicated ground truth here, and the first that is not `N4L`. **The evidence is the gate's own elements, cut by their own means**: forty-one elements from 22.55 s, marks split at 187 ms and gaps at 112 ms, giving `...-` `.-` `...--` `...-` `.-.` `.-.` — V, A, 3, V, R, R. Dit 100.4 ms, dah 274.3, ratio 2.73, element gap 73.3, character gap 150.0: about twelve words a minute, Farnsworth in HM-DEC-115's manner. **It is not taken from the decoder's reading**, which emits `VA3VRR` here with one character at low confidence, and that is precisely why an unchecked decode is not ground truth. **The leading silence is excluded and it matters**: left in, the 325 ms quiet before the station drags the long-gap centre to 209 ms and nothing divides into characters at all. **Why a second one is worth a session**: `N4L` has been the only one for five sessions, and this is a different fist — 2.73 dits to the dah where `N4L` sends 4.24, 100 ms to the dit where `N4L` sends 56. | A rule fitted to one recording has nowhere to be wrong, and a rule with nowhere to be wrong is not falsifiable. | HM-DEC-145 |
| 2026-08-20 | **`cw-2026-08-17-134712` holds a station, not a carrier, and its callsign is `N4L`.** Supersedes HM-DEC-095 on that one finding and on nothing else. **The evidence is the decoder's own elements, read by hand**: between 21.45 s and 23.01 s the gate produces `225 30 55 | 180 | 55 40 55 40 60 40 55 30 245 | 150 | 60 25 245 40 55 40 55`, which cut at the midpoint of its own two means is `-.` `....-` `.-..` — N, 4, L. Dit 56.3 ms, dah 238.3, ratio 4.24, element gap 35.6, character gap 165.0: about twenty-two words a minute with a heavy fist and the Farnsworth spacing HM-DEC-115 measured elsewhere. **A carrier cannot produce that**, and all three instruments now agree — the keying meter scored it 0.37 at 500 Hz with a 54 ms element, the gate reads 55 and 235 in the same stretch, and the elements spell a callsign. **What it cost to have this wrong**: three sessions chased a real decode failure while a ruling said the audio held no station, and one was blocked outright by `ACarrierNeverConvincesTheTrackerItIsAStation`, which reads this file by name and made any change that noticed the station fail the suite. **What does not follow**: how the survey tells keying from a carrier is HM-OPEN-054 and stays parked, and the rest of the transcript is still Tim's ear. | A ruling that is wrong about a fixture is worse than no ruling, because the tests built on it turn the error into a wall. | HM-DEC-144 |
| 2026-08-19 | **The settled pass judges for itself whether somebody is keying, from the marks it has already extracted, rather than asking the tone survey.** Closes HM-OPEN-051; HM-DEC-095's guard is not weakened and its carrier case is the condition on this shipping at all. **The verdict was answering a different question at the wrong cadence**: `KeyingRecently` is a six-survey counter over half-second surveys, so it goes false three seconds after the survey last saw keying, and the survey needs enough marks in three seconds to see two clusters. On a fixture of twenty-seven characters across thirty-two seconds the protection expired **while the station was still sending**, with 0.7 s of trailing silence identical to a fixture that stayed protected. **The pass that reads the marks is the one that knows** — it holds the structure directly where the survey infers it from raw energy, and a carrier has energy and no structure. **Lengthening the protection was rejected** as trading against the carrier guard by exactly the amount added; **exempting the final drain was rejected** as repairing the end of a recording while a live contact still stops mid-exchange. | A slow sender leaving big gaps is exactly who a newcomer works, and the end of a message is where the callsign is. | HM-DEC-143 |
| 2026-08-19 | **When the sender leaves too few word gaps to form a third class, the settled pass emits the characters it read, unspaced, and says on the transcript that word spacing was not measured.** Closes HM-OPEN-048 and **narrows HM-DEC-115 to the case it was ruled for**, overturning none of it. **What is there is not a guess**: eighty gaps clustered from the sender's own keying, a clock fitted at a hundred milliseconds, two of three classes populated, and the word class missing because he sent a callsign without spaces — a fact about his sending rather than a failure to measure it. **The current behavior is the one that fails §0.0**: 258 windows read successfully on a fixture the reference reads at 100% and the transcript is empty, and an empty box says nothing was sent. A ham reads `CQCQDEW4AWHK`; nobody reads a blank. **Clustering two heaps and calling the wider class a word gap was rejected** as placing spaces nobody measured, which is the guess 115 forbids, and the sentence on screen is load-bearing rather than a caveat. **Measured before it ships**: two classes must still separate element from character gaps, or a callsign runs together and reads as confident nonsense, and then none of it ships. | An empty transcript is a belief formed from the screen that is not true, and it is today's behavior rather than a risk of changing it. | HM-DEC-142 |
| 2026-08-19 | **The outstanding-asks queue lists questions handed back for a ruling in a session report, and nothing else.** It settles the boundary HM-DEC-139 left open on its first use and amends nothing. **An ask in a report has no other home**: `OUTPUT.md` is overwritten by the next session, so a question raised there and unanswered that evening ceases to exist, which is the failure 139 was written for and is specific to that channel. An entry in `OPEN_ISSUES.md` already has an id, an owner, a status and a date and is swept whenever the file is opened. **And the queue has to stay short enough to be read** — folding in twenty-odd items that want a capture file or a manual page rather than a judgment makes the four real questions harder to find than before, which is the same failure by a longer route. **Splitting it by what unblocks each item was also rejected** and is the better shape if it grows, since two of today's four wait on a dummy-load evening rather than on Tim; at four items that is machinery for its own sake. **What would reopen it**: the premise is that `OPEN_ISSUES.md` is genuinely swept, and HM-OPEN-007 has sat unruled since 2026-08-14. | A queue nobody reads is the same failure as a queue that does not exist. | HM-DEC-140 |
| 2026-08-19 | **Every session report carries a heading for asks still outstanding, and every work order carries the same list inbound; a report or an order without it is defective and is redone.** Closes HM-OPEN-044 and supersedes nothing — HM-DEC-106's four sections stand and this is a standing heading within the fourth. **An ask nobody answered looks exactly like an ask nobody made**: `099de5a` changed the frequency's cadence and asked for the ruling in its own section four, correctly and completely, and three sessions inherited the change as settled while one withdrew a draft of that same ruling with the code already in the tree. What made it invisible was not carelessness but that a section four is read once, on one evening, and then the conversation moves. **So the queue carries itself forward rather than being remembered**, each ask with the date it was first made, what it waits on and where its change already sits, carried verbatim until ruled and dropped by the report that records the ruling. **The heading appears even when the queue is empty and says so**, because an absent heading and an empty queue are the same sight. **The marker at the site was deferred rather than rejected** as the stronger answer that needs a convention and a test, and **refusing to ship without the ruling was rejected on a measured cost**: it would have held the display fix for two more evenings. | A queue that is visible is worth more than a gate that is closed. | HM-DEC-139 |
| 2026-08-19 | **The frequency is read on the live poll and stays there, superseding HM-DEC-109 on this field's cadence and setting aside HM-DEC-050's exemption for it.** The rest of that ruling stands; what is set aside is one exemption granted in favour of something that is not happening. **The premise was false and nobody had measured it**: the frequency was exempt because the radio broadcasts it, and on the operator's own radio 5,499 frames arrived in sixty-one seconds with `inboundTransceive` zero, `radioIsBroadcasting` false. Asking is not the more stale option, it is the only one. **The cost was measured rather than feared** — six bytes out and eleven back, under seventy bytes a second on a cable moving eleven thousand. **Reverting to the session sweep was rejected**, because that sweep is what turned the snap-back into thirty seconds of wrong display instead of one poll, and a cadence chosen so the next such fault lasts thirty seconds is choosing badly on purpose; **a conditional cadence was rejected** as a second mechanism resting on the push that proved unreliable here. | The code shipped in `099de5a` with the ruling asked for and never given, and a decision not in the record is not made. | HM-DEC-138 |
| 2026-08-19 | **The status-write instruction lives in `CLAUDE.md` and in every Claude Code work order, and an order delivered without it is defective and is redone. A session writes the status whether or not its order says so.** HM-DEC-132's triggers and fields are unchanged. **The rule was never the problem**: §13.2 has carried five triggers since that ruling and consecutive sessions did not apply them, one saying so in its own report — read, and not applied, with two phase boundaries crossed without a write. A correct rule that nothing carries is indistinguishable from no rule, and the panel showed a working project as dead. **Two channels because neither has held alone**: this file is read once and forgotten across the hour-long phase the ten-minute write exists for, and a prompt written in a hurry loses the line. **A missing line is a defect rather than an oversight**, on HM-DEC-099's precedent, because the chat side cannot write to disk and the instruction it hands over is the only thing it can be held to. | A rule nothing carries into the session is the same as no rule, and it had already failed three times running. | HM-DEC-137 |
| 2026-08-18 | **A Claude Code work order is delivered as `WORK_INSTRUCTIONS.md` at the repository root, and the pasted prompt says only which project it is and to read that file and execute it.** Amends HM-DEC-100 on what the prompt contains and supersedes nothing. **This is HM-DEC-106 pointed the other way**: that ruling moved the report into `OUTPUT.md` because a report read off a photograph is read less carefully, and the inbound half had the same defect unnamed — a pasted order is retyped, truncated by the buffer, cannot be diffed or committed, and is gone when the window closes. **The gate is in both places and that is not belt and braces**: a one-line prompt in the wrong repository would read that project's order and find a gate agreeing with itself all the way down, so prompt, file and `PROJECT_CARD.md` are all checked and any disagreement stops the session. **It carries its issue date** because a file at a fixed path can be read twice, and one older than `OUTPUT.md` is work already done. It is committed, so the order that produced a commit sits beside it. | Work comes in through one file and goes back out through the other, both at the root, both in the tree the session is changing. | HM-DEC-135 |
| 2026-08-18 | **A return to a place already in the recent list is noted on the entry that is there, and the operator can remove any entry by hand. HM-DEC-072's two hundred hertz stands unamended.** Two visits a hundred hertz apart read as two entries, and a hundred hertz is well inside the two hundred that ruling already calls one place, so **widening the figure was rejected**: five hundred would fold the same pair and would break the deliberate link to `SpotIdentity.FrequencyBucketHz` that keeps two numbers meaning near enough from drifting apart. A second visit is a fact about the entry, which is the same shape as 072's rule that the newest visit's identification wins including when it is empty. **And a list the operator did not curate is one he must be able to take things out of** — ten places kept automatically on a dwell nobody set and nobody can see — per entry and whole. Removal is not a correction: a place visited again afterward returns as a new entry counting from one. | The near duplicates were never the tolerance, and a number changed to hide a behavior nobody has measured is the wrong repair. | HM-DEC-134 |
| 2026-08-18 | **§13 gains a HAZARD line, and the ratings stay in §4 rather than being copied beside it.** The annunciator's card is five standing lines with no room for either (HM-DEC-132), and **a session that learned where the repository is without learning that it can put a hundred watts on somebody else's frequency has learned the wrong half** — so the hazard is restated in the file every session reads automatically, ruling nothing that §0.2 and §0.2.1 do not already rule. **The ratings are deliberately not restated**: §4 carries the thirty-character keyer message, the 300–900 Hz pitch range, the `94h` address and the filter index, each page-cited to Full Manual `A7292-4EX-6`, and a second copy is a second thing to drift (§0) — this table already spanned three printings once and that seam produced two defects. **And the power output is the one rating this file does not carry, so it is marked rather than supplied** (HM-OPEN-038): §4 names no wattage and cites no page for one, because HM-DEC-074 and HM-DEC-082 have Hamlet report power as a percentage of range and never as a wattage, so the application never needed the figure. | A hazard line is the worst possible place to introduce this project's first uncited number. | HM-DEC-133 |
| 2026-08-18 | **The project reports status to the panel per `ANNUNCIATOR.md`, and §13 carries the six status fields and the write triggers inline. Supersedes HM-DEC-131 on the triggers and on the shape of both files.** The panel reads `PROJECT_CARD.md` and `PROJECT_STATUS.md` from every repository and shows them on one screen, and **it only knows what a session last wrote** — a project whose sessions never write reads as dead while it is working. The card drops to five standing lines and the status file to six volatile ones, because a reader that takes the leading run of `KEY: value` lines cannot use fields it was not built for, and sixteen fields written for a protocol that never arrived are sixteen chances to disagree with a panel about what `STATE` means. **The ten-minute write while executing is the substantive change**: HM-DEC-131 said at each state transition and at no other time, which is right for a record and wrong for a panel, because a phase here can run an hour and **a long phase and a dead session look identical** without a heartbeat inside it. §13 sits in this file rather than only in the companion because this file is read automatically and a companion is read only if something points at it. | A session that ran an hour with tests going green behind a panel showing blocked, because nothing in that tree told it to write. | HM-DEC-132 |
| 2026-08-18 | **The project maintains `PROJECT_CARD.md` and `PROJECT_STATUS.md` at the repository root, and every session writes the status file at each state transition and at no other time.** The card holds what is true between sessions — the repository path and remote, the trunk, the branch policy, the build, test and run commands, the ratings that may never be inferred, and what this software can do to the physical world; the status file holds what is true only right now — the state, whose ball it is, the measured counts, and what is being assumed while nobody has ruled. **Everything in both is measured from the tree or quoted from this file**, never composed: the prime directive and its practical test are taken verbatim from §0.0, because a quotation that has been tidied is no longer a quotation. **Writing the status file more often makes it a log**, which is what the telemetry record is for, **and writing it less often makes it a fiction**, which is worse than absent because it looks current. Both name `PROTOCOL: 2`, and that protocol — `STATUS_PROTOCOL.md` — **is not in this repository**: each file says so in its own prose, so the header is read as which protocol it is written against rather than as conformance nobody here can check. | A session's first minutes go on rediscovering what the last one already knew, and a header claiming conformance to an absent document is the same fault as a decode with no signal behind it. | HM-DEC-131 |
| 2026-08-18 | **A message too long for one keyer send stays refused until the seam between two sends has been measured into the dummy load.** The keyer takes thirty characters in one command 17 message; `CQ CQ DE KC3QIS KC3QIS K` is twenty-four, so this operator's own call fits with six to spare and nothing is blocked meanwhile. `CwMessage.Split` already splits at the spaces, so the machinery exists — **what does not exist is a number.** The second send only goes out when the radio has taken the first, and Hamlet does not know how long the radio holds it, so **the gap in the middle of every round is of unknown length** and a CQ with a ragged pause in it is a worse transmission than a refusal. **Timing the second send from `CwDuration` was rejected for now** as making Hamlet responsible for a gap inside a transmission, which is a smaller version of why host-timed keying is rejected outright; **refusing permanently was rejected** as a ceiling some callsigns cannot get under. Measured at the bench with the load already connected, then ruled. | The choice between refusing and splitting should be made on a number, and the number costs five minutes of an evening that is happening anyway. | HM-DEC-130 |
| 2026-08-18 | **HM-DEC-114's bar does not apply to `prosigns-easy`, and the cold-start bin choice becomes its own work order.** The bar was ruled for *a loud clean signal read wrongly*; this fixture's first character arrives at 7.44 seconds on a message running about four and a half, so its opening is gone before the detector has found the signal — **a different claim, and one no real station makes**, because a CQ repeats. Every other easy-tier fixture is given a `VVV` run-up for exactly this (HM-DEC-103) and **this one cannot take it**: with the run-up in front the tracker settles at 675 hertz on a fixture sitting at 615 and emits nothing at all — the third sighting of the survey choosing a bin that holds no station, which HM-DEC-127's floor does not reach because nothing is confirmed yet when it happens. **Editing the fixture was rejected** as the move §12.5 exists to stop a session making alone, and it would leave the survey defect untouched; **fixing the cold start inside this order was rejected** because squeezing a real on-air defect in to turn a test green is how it gets done badly. HM-OPEN-033 is scheduled rather than closed. | Two questions have been conflated for two sessions: what a fixture asserts, and what the survey does from cold. | HM-DEC-129 |
| 2026-08-18 | **HM-DEC-116 is superseded: its premise dissolved when the streaming estimator began reading the shared gap fitter, and on the evidence the answer is no.** The ruling said the streaming pass adopts the settled pass's classes *and uses dit multiples only until those classes exist*; the second clause was true when ruled and stopped being true the session the estimator gave up its own two-way classifier for `CwGapFit`. **The choice it was making no longer exists** — not fitted classes against dit multiples, but the settled pass's global fit against the streaming pass's own local one, which nobody has ruled on. Measured on top of HM-DEC-123: **the full form costs a real capture**, streaming on `cw-2026-08-17-013347` falling from `VA3VRR` to placeholders and the two-station settled text from `L DE W1XYZ K` to `ATD■VTXYZ`, buying one synthetic looping training signal (HM-DEC-091 decides that); **the narrow form never fires**, leaving every recording unchanged character for character because wherever the settled pass has classes the streaming pass already has its own. HM-DEC-121's blocking condition was met and answered — adoption now produces three moves and one follow, identical to adoption off. **If the two fitters ever diverge, the live question is which fit is better, and today it is the streaming pass's own.** | A ratified ruling absent from the tree is the state the record likes least, and one waiting on a condition already met is how an unexamined convention survives. | HM-DEC-128 |
| 2026-08-18 | **A confirmed station is not abandoned for a candidate far below it, and `ASignalAtTheWrongPitchIsStillFound` gets a band in it.** The survey's bin choice is behind three failures: the tracker leaves a 400 Hz station for 575 and loses the `CQ`; `prosigns-easy` with a run-up settles at 675 on a fixture sitting at 615 and emits nothing; the two-station recording takes three moves from cold. In the first the chosen bin carries **the same dit and the same dah as the station being read, thirty-five decibels quieter, clustering three times more cleanly** — separation 30.8 against 8.7. It is the station's own image. HM-DEC-095 settled that a note is chosen by how it is keyed and never by how loud it is, which was about **which signal to read on an empty-handed survey**; it never settled **whether a candidate may take the tracker away from a station already confirmed**. That is a different question and the answer is not "prefer the louder" but "do not abandon what you have for something far below it". **The fixture is separately wrong**: it generates audio with no noise at all, so the image is a hard-limited replica with nothing to bury it, and every other fixture under `tests/fixtures/cw/receiver` was rebuilt with a shaped band for exactly this reason (HM-OPEN-018) while this one was missed. Giving it a band **changes what it asserts** — from found at 400 Hz in silence to found at 400 Hz in a band — **and the second is the better test**. | A survey that can be fooled by a station's own image is not measuring the band, it is measuring the fixture's silence. | HM-DEC-127 |
| 2026-08-18 | **HM-OPEN-026 is closed: `cw-2026-08-18-003758` is unobtainable and the entry reopens if the file appears.** Asked across four sessions without the file arriving, and a sweep of the tree confirms **nothing in the fixture set names it** — no test, no sidecar, no catalogue entry, no assertion — so no fixture rests on absent evidence and the question had no work behind it. **The cost is real and is why it was not a session's to close**: it is the recording on which Hamlet read `DE AA4MP/4 QNIK` correctly with independent confirmation, and **this suite has no regression test for a success at all** — every ratchet it holds is a ratchet on a failure getting less bad, so nothing in it can tell a repair from a coincidence. Leaving it open a fifth time was rejected as producing a paragraph a session and no decision. | A question nobody can act on is not an open issue, it is furniture. | HM-DEC-126 |
| 2026-08-18 | **HM-DEC-122 is superseded and unbuilt: the window that yields the cleanest clock is not the window that reads the signal best, and on real audio those are different windows.** Two parallel acquisition candidates were built exactly as ruled and did not survive their own measurement. **Taking the first answer meets the acceptance and invents characters** — the candidates name 325, 550 and 725 hertz for a signal at 640 across their first several reads, having never been subject to the tracker's own two-agreeing-surveys rule (HM-DEC-095), and 2.8% of what came back below the refusal floor was never sent where HM-DEC-120 measured zero at every level. **Gating the settle on a confirmed station removes both regressions and every gain with them**, leaving all nine speeds and five ratios identical to the unmodified decoder. **And where it fires in time it costs the bulletin seven characters**: only the twenty millisecond candidate yields a clock there, so the tie-break never runs, while fifty smears a 57 ms dit past the cluster test despite reading better and forty — which the ruling removed from consideration — beats both. **The mechanism is not rebuilt now**, because the fast end it was ruled to buy was delivered by the gap classifier repair instead, 0.63 to 0.88 without touching acquisition. Scoring candidates by their own speed estimator at the tracked pitch is the direction if a measurement later shows a gap; it is a measurement of reading rather than of clustering, and a fluke at 725 hertz cannot fool it. | A ratified ruling absent from the tree is the one state worse than either having it or not, and the premise it rested on — that a short window only costs sensitivity — was measured false. | HM-DEC-125 |
| 2026-08-18 | **The fixture generator's caret is fixed and the prosign fixtures are regenerated through HM-DEC-101's gate, each hold-out adjudicated individually, inside the next work order rather than a session of its own.** Hamlet reads `IR` where `AR` was sent **because the fixture sends `IR`**: `KeyEdges` opens with one unpaired edge at the message start, and the caret's join branch begins by adding a gap edge on the assumption that a mark is in progress to separate from — at the head of a word there is not, so that edge closes a mark that never opened. A phantom hundred-millisecond dit, and **every edge after it on the opposite parity**. `^SK` survives because six elements restore the parity that five break, so an even-length prosign renders and an odd-length one does not, which is why it has looked intermittent. The reference implementation reads `EV` and `IR` too: two independent decoders agree and both are right. §12.5's own pattern, with the decoder blamed for months. `exchange-easy` is re-checked after the fix rather than investigated separately. | The machinery §12.5 protects was exercised as a phase last session and held; the fix is arithmetic whose model predicts all nine edges of `^BT` exactly. | HM-DEC-124 |
| 2026-08-18 | **A retune that refines the pitch of the station being read does not reset the settled window; one that follows a different station does. Its own work order.** Two unrelated investigations converged on one sentence — **one retune decodes and three does not** — a lost callsign when adopted gap classes moved `MidCharacter`, and an unreadable 400 Hz signal found from two hundred hertz away. Neither is about distance: starting three hundred hertz above the signal decodes and one hundred above does not. The cost is paid in one line, `_settled.Reset()` on every switch, which HM-DEC-096 put there because a switch usually does mean somebody else started transmitting. **A fifteen hertz refinement within the current bank and a jump to another operator are not the same event**, and the tracker already knows how far it moved. Stopping the streaming pass's segmentation from gating the tracker was **held rather than shipped**: it is right in principle — a provisional judgement governing a measurement is backwards — but it treats one symptom of a mechanism not yet proven, and if this ruling is the whole story that change would be made for a reason that dissolved. | Two failures, two causes, one line. | HM-DEC-123 |
| 2026-08-18 | **Acquisition runs two analysis windows in parallel, twenty milliseconds and fifty, and keeps whichever yields a valid clock.** HM-DEC-119's commission is answered and **the edges are not the problem**: both are late by thirty to thirty-six milliseconds, which is group delay, identical at both ends, and cancels for a length — what survives is three to seven milliseconds, under one hop, and does not grow as marks shorten. **The window is where the speeds live or die.** The tracker picks it from the speed it currently believes, so told ten words a minute it uses fifty milliseconds, which at thirty is longer than the dit: runs merge and the start error goes from thirty milliseconds to a hundred and seventy. Told twenty-five or more it uses twenty, and both twenty-five and thirty then read better than at the forty it acquires with. **A decoder that has not yet found the speed is running the window least able to find it.** Acquiring short alone was rejected: it trades the weak-and-slow reach that is this project's best-proven capability for speeds it cannot yet read. Iterating from the locked speed was rejected because at thirty there is no lock to iterate from. The selection needs no new judgement — a clean dit-or-dah cluster inside the ratio band is already the test. | The failure is asymmetric: too long merges runs and destroys the signal, too short only costs sensitivity. | HM-DEC-122 |
| 2026-08-18 | **HM-DEC-116 is not live and is blocked on a trace of the coupling between the two passes.** Built exactly as ruled, it meets its own acceptance — the handover test, two-station tone-finding, and the app transcript test all pass — and **it costs the callsign on a real off-air capture**, where the settled pass falls from `VA3VRR` to placeholders. It is a feedback loop rather than a tuning problem: the estimator adopting the settled classes changes state the settled pass reads back, and three narrowings — suppressing null adoption, removing the drop on a lost clock, confining adoption to the boundaries — left it intact while disabling adoption restored the callsign. Net thirteen failures either way, one synthetic test swapped for one real one, and **a real capture outranks a synthetic one** (HM-DEC-091). The settled pass takes one thing from the estimator, the dit hint; if that is the whole coupling the classes can be handed forward without touching what the dit derives from. **Marked blocked rather than left live**, so no future session ships a ruling that breaks a real decode. | A ruling that says do this while doing it breaks a real capture is a trap, and this repository's history is conventions surviving because nobody treated them as questions. | HM-DEC-121 |
| 2026-08-18 | **The refusal floor is 14 in the decoder's own margin units, superseding the 17 of HM-DEC-117's interim.** Swept rather than reasoned: 17, 15, 14 and 13 never invent a character at any level, 12 begins inventing at minus two decibels — HM-DEC-097's named case at 0.44 of the message invented. **Fourteen and thirteen are identical on every measured number and both read the message perfectly down to one decibel**, so the further of the two from the cliff is taken. Seventeen cost four decibels of reach and bought nothing, since every floor from seventeen to thirteen has the same worst invented share, which is none. | The first time this number was reasoned it was four decibels out; the second time it was measured and it was not. | HM-DEC-120 |
| 2026-08-18 | **HM-DEC-112 is superseded: the gate's own mark length is accurate and no half-amplitude correction is applied to the clock fit. Only the fitted dit-or-dah boundary survives.** Measured through Hamlet's own detector rather than through an offline filter: the gate reads 100–110 ms for a true 100, 45–50 for a true 48, 40–45 for a true 40, accurate to within one hop at every speed, while half amplitude reads 80–90, 30–35 and 25 — wrong by 15, 30 and 37 percent and worse the shorter the mark. **The shed is a fixed 15–20 milliseconds whatever the speed, which is the analysis window and not the transmitter's fall time**: a Goertzel window rounds the top of a short mark, so its width six decibels below its own apex is far less than its base. The evidence for HM-DEC-112 was taken with a different analysis window from the one Hamlet decodes through, and the correction carried that window's shape into the clock — shipping all three parts takes the suite from 13 failures to 29 and silences 30 words a minute entirely. **What survives**: `ClassifyMark` cuts between the two measured mark clusters rather than at two dits, fitted per signal and seeded on percentiles — 1.73 dits for a textbook fist, 1.58 for one sending dahs at two and a half. **Commissioned**: characterise this detector's envelope at the edges for synthesized dits of known length at 12, 25 and 30 words a minute, which decides whether the answer is a shorter edge window, sub-hop interpolation, or nothing. | A measurement taken through one instrument is not a fact about another, and 25 words a minute decoding exactly while 30 collapses is a correction working by accident. | HM-DEC-119 |
| 2026-08-18 | **The first spot refresh waits for the radio to say where it is.** Startup loaded spots from the remembered band before the radio was connected, so RBN was filtered and the skimmer watch scoped to a band the operator might not be on — seen once on the training radio, self-correcting on the first band change. **An empty panel asserts nothing and a wrong-band panel asserts something false**, which is the distinction §0.0 exists to draw, and the cost of waiting is two seconds of empty on a screen just opened. Marking the panel as remembered was rejected: it still calls somebody else's service about a band nobody is on, which HM-DEC-024 commits Hamlet not to do. | A display that is briefly wrong is still a display that is wrong, and the alternative costs nothing anybody will notice. | HM-DEC-118 |
| 2026-08-18 | **The refusal floor is measured rather than translated again.** Seventeen was reasoned from the seventeen decibel offset between broadband and in-filter measurement and was expected to bite at HM-DEC-097's own nought decibel line. It bites at about five: untouched to six, 0.94 correct at five, 0.61 at four, 0.11 at three, silence at two. **Four decibels of reach were given up and the arithmetic that gave them up was Claude's.** The property the ruling wanted holds — the worst invented share across the whole sweep is zero — so the floor stays in force while the number is established by sweeping candidate floors and finding where the invented share actually crosses zero. **Ruling again from a second translation was rejected**: the first was four decibels out and the harness to measure it already exists. | A number that decides what the display asserts is worth one test rather than a second guess. | HM-DEC-117 |
| 2026-08-18 | **The streaming pass adopts the settled pass's fitted gap classes for the current sender, and uses dit multiples only until those classes exist.** HM-DEC-115 is amended for the streaming path, not withdrawn: applying it there directly was measured and broke `NothingIsInventedAtTheHandover` and tone-finding on a two-station recording, because fitting three classes needs a history long enough to hold word gaps and a rolling window cannot tell one sender from two. **The classes have to belong to a sender before they are useful**, the tracker already knows when the sender changed, and the settled pass already fits per signal — so they are handed forward and reset on a tracker switch or a lost clock. Before the first fit the tip uses multiples and firms up when the classes arrive, which is what a provisional tip is for. | The two passes disagreeing about where the words are is a worse fault than either being wrong alone. | HM-DEC-116 |
| 2026-08-18 | **Word and character gaps are classified by clustering the gaps themselves and never by multiples of the dit, because real operators send Farnsworth.** HM-DEC-048 said this and the code did not do it. Measured on a strong off-air ARRL bulletin at S4, tone 501 Hz, 43 dB claimed: the gaps fall into three clean heaps at **40 ms, 240 ms and 500 ms while the dit is 57 ms** — the element gap is *shorter* than a dit and the character gap is six times the element gap rather than three. Nothing about 1:3:7 survives contact with a traffic net, and a decoder that assumes it splits every word in the wrong place while getting every character right. Cutting on the measured heaps instead reads **`AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAGE P`** from the same audio, every character correct after acquisition. | The letters were never the problem; the spaces were, and the spaces are where a transcript stops being a transcript. | HM-DEC-115 |
| 2026-08-18 | **A loud clean signal is decoded whole or it is a defect: at 15 dB or better in the passband with a steady fist, the decoder emits the message with no strangers and no placeholders.** The easy tiers and `exchange-easy` stop being ratchets and become pass or fail. **Ratchets were right while the audio was unproved and are wrong now that it is**: a ratchet on a proved fixture records that the decoder is still wrong without ever requiring it to stop being wrong, and `exchange-easy` has been read at 100% by the reference and at ten characters of eighteen by Hamlet for two sessions under that arrangement. A bar phased by speed was rejected, because it would licence twenty-five words a minute to stay broken by design and that is the speed on the air. **A red suite saying this signal is loud and clean and we cannot read it is the correct state of the world.** | Six years of copying by ear a machine should find easy, and "better" with no floor under it has been the answer for four sessions. | HM-DEC-114 |
| 2026-08-18 | **Every session commits and pushes to `main`, and no session creates or works on a branch without Tim's ruling.** Three consecutive sessions built the honest CW detection, the fixture rebuild, the generator fix, the scanner and the entire UI work order on `feature/honest-cw-detection` — about twenty commits — while `main` stood still. He ran his radio against `main` for an evening, reported a shipped feature as missing, and was right. **No ruling ever authorized a branch**: a session invented one and every session after inherited the invention as settled, while four reports named it without one of them treating it as a question. Branch-per-work-order merged at the end was rejected, because a session judging its own work merge-worthy is a session grading itself. The next session brings the branch back to `main`. | §9.5 already said a decision not in the record is not made; what it lacked was anywhere for the omission to be noticed, and a report naming a branch in passing is not a question being asked. | HM-DEC-113 |
| 2026-08-18 | **Element edges for the clock fit are taken at half amplitude, and the detection bandwidth follows the locked speed afterwards as separate work.** A gate threshold at the cluster midpoint sits about eight decibels below the mark level, so it catches the rising and falling skirts: **every mark is measured too long and every gap too short, by the detector's own rise time.** Negligible at eleven words a minute and ruinous at twenty-five. Measured on the 00:31 capture, the midpoint reads dit 64.9 ms and ratio 2.41 at twelve hertz — **below the floor, so the clock is rejected and nothing is emitted** — and dit 45.5 at forty, while half amplitude reads dit 44.5 to 47.8 and ratio 2.89 to 3.10 across the whole range. It is one root cause behind `ACleanSignalDecodesExactly(25)`, the `fast-easy` hold-out, and the reference returning every element as its own character at speed. **Widening the bandwidth alone was rejected**: it costs sensitivity where it is most needed and only shrinks the bias. Bandwidth following speed is real and comes second, measured separately, so the improvement is attributable. HM-DEC-105 already put half amplitude in the settled pass and HM-OPEN-023 keeps it out of the tone survey, which asks a different question; the clock fit is the third path and was ruled by neither. | The published dit 106 and dah 283 were a fifty millisecond window's artifact rather than the station, and the same error was still sitting inside the decoder. | HM-DEC-112 |
| 2026-08-18 | **A provenance label carries its age.** A capture sidecar wrote `frequency 14028000 Hz (read from the radio)` beside `band 40 m` while the operator was on 7.025400 — the value was read, sixty seconds and two tunings earlier, and the label asserted a freshness it did not have. The telemetry event written at the same instant carried the right frequency, so one file had two paths to the same fact and only one was correct. **Degrading the label past a threshold was rejected** as needing a per-field rule that the sweep makes moot, and leaving it to the sweep was rejected because a label asserting freshness it lacks is §0.0 broken whether or not the mechanism behind it usually works. | A value's age is part of what the value asserts, and this one asserted it while sixty seconds out of date. | HM-DEC-111 |
| 2026-08-18 | **A band's jump spot is the first "CW main street" block in the cited conventions, and `BandPlan` retires.** Band edges and CW segments were measured and both derive exactly from the privileges file — the edges from `97.301(b)`, the segments from the union of Data-carrying ranges in `97.305(c)`, to the hertz — **which corrects HM-OPEN-005's own claim** that the segments are convention rather than regulation and do not align with privilege boundaries. Only the jump spots needed a rule, since five of seven already were that block. 40 m moves 7.030 to 7.028 and 30 m moves 10.110, which matched no cited source at all, to 10.103. **The QRP watering hole was rejected** as moving all seven and aiming a beginner at a narrower slice than a segment's main street; **keeping the numbers as editorial was rejected** because it makes an unsourceable number permanent in a file whose purpose is citation. The neighborhood file is not the source: its Morse rows fall short at the top of every band, by 10 kHz on 17 m to 230 kHz on 10 m. | Two band plans in one tree is the state §0 exists to prevent, and the uncited one had the friendlier name. | HM-DEC-110 |
| 2026-08-18 | **The frequency is swept on the session poll, amending HM-DEC-050 for a third field.** That ruling says nothing the radio volunteers is polled for, and Mode and FilterSelection are already recorded exceptions for exactly this failure: a broadcast missed while the app is starting leaves the model holding a frequency the radio is not on, with nothing to correct it until the dial is next turned. The frequency was the one left out. Its age also meant something different from every other field's, reading as a link going quiet when it was a link with nothing to report, and once swept that separate rule is not needed. **On-demand reads at the two moments the value is reasoned from were rejected as sufficient**: they close the consequences seen and not the cause, and anything reading the frequency in future gets the old behavior back silently. | The band on screen derives from this reading, and the band scopes what RBN is filtered to and what the skimmer watch listens for — so a wrong one makes "nobody heard you" a defect wearing the clothes of an answer. | HM-DEC-109 |
| 2026-08-17 | **Confidence gains a third measurement — how far the gap that ended a character sat from the boundary it was judged against — and the worst of the three still wins. Extends HM-DEC-048 and supersedes nothing.** The settled pass was showing characters at full strength whose elements were clean and whose boundary decision was marginal: a lone dah spells T, a lone dit spells E, produced where the pass divides characters in the wrong place, and the timing margin of a dah that really is a dah is one, so the old model could not see the fault at all. Two wrong letters in eight at full strength is one in four, and on a callsign he is about to answer the operator could act on it. **Dimming everything until strangers reach zero was rejected** as treating the symptom by discarding what was right, and leaving it alone was rejected in the report before it was proposed. | The fault is a boundary decision, so the missing measurement is of the boundary; and nothing here raises a confidence, it only finds one more way to lower it. | HM-DEC-108 |
| 2026-08-17 | **Tuning writes are their own category and the scanner is governed by §0.2.1: never while transmitting, the starting frequency restored by any exit route, confined to a band-plan segment the operator configures in a file he edits, aborting on a touched dial or an unanswered read, and refusing to start before rig state is populated.** Reading the radio is unrestricted and moving the dial is not: a scan takes the tuning knob out of the hand of the person sitting at it, repeatedly and unasked. Frequencies come from configured data and never from a model's memory (§0). **Proposed by Claude; ratified by Tim's commit of this file**, on the precedent of HM-DEC-009. | The operator must always be able to tell where his radio was left and why, and silence from the link is a stop rather than a licence to keep moving. | HM-DEC-107 |
| 2026-08-17 | **A session's report is four sections — what Claude did, what Tim should expect, what we should do next, what is blocking us — written to `OUTPUT.md` at the repo root and not only to the terminal.** Supersedes HM-DEC-096 on the report's headings and nothing else; §12.1's four-part test for what a session may record itself is untouched, and a recorded entry now appears in full inside section one while everything handed back appears in section four. Reports were being read off photographs of a scrollback buffer, which is a transcription step between a measurement and the person who has to rule on it. | The split that matters is between what a session settled and what it is handing back, and a report he has to photograph is a report he reads less carefully. | HM-DEC-106 |
| 2026-08-17 | **The generator learns to join segments, and the two unadjudicated tests are adjudicated rather than deleted.** Concatenating a station at one speed with a second station answering at another speed and another pitch is not overhead: **clock loss, the retained previous clock, tracker switching and the speed-change annotation have no committed test at all**, having been built on rulings alone. One fixture proves all four, and it is the situation an answered call actually produces. Segments are generated complete and joined across a gap, never mid-character, so the seam is a signal and not an artifact to be learned. | Deleting a test to clear a number destroys the evidence the number was supposed to summarize. | HM-DEC-104 |
| 2026-08-17 | **Twenty-five words per minute is generated and covered, and the old fixture retires once replaced.** The one old fixture that fails is the only fast-CW coverage the repository has, so retiring it silently narrows what Hamlet is claimed to handle. Speed is a generator parameter and the gate governs the result exactly as it does the rest. New failures are expected and wanted: at 25 WPM the window's thirty-element floor binds rather than its two-and-a-half-second one, and no test has ever exercised that path. **Thirty-five was rejected** as scope invented at a test bench, since nothing has yet been decoded above twenty. | A claim about speed with no evidence behind it is the same defect as a decode with no signal behind it. | HM-DEC-103 |
| 2026-08-17 | **Nothing is diagnosed against audio that has not itself been proved.** The settled pass currently reads worse than the provisional tip on the five-decibel fading tier, which is the opposite of why the settled pass exists — but the reference scores only 52–53% on that same tier, so the fixture has proved nothing about Hamlet yet. Re-measured after HM-DEC-101, and investigated only if the gap survives on sound audio. HM-OPEN-017's labelled-approximation fallback stays available and is not taken on this evidence. | A defect measured against suspect audio is a phantom, and chasing one costs a session and teaches nothing. | HM-DEC-102 |
| 2026-08-17 | **A fixture the reference cannot read is a generator defect, and the control for the generator is the real recording.** Three fixtures were held out by the scoring gate, including a tight fist at the easy tier — while the reference reads the *real* tight fist at high confidence off capture 013347. The synthesis is therefore wrong and is fixed against the measured parameters of that capture until the reference scores it as well as it scores the audio. **Lowering the gate to admit them was rejected outright** (§12.5). The same defect is carried as the leading hypothesis for the `MVRR` shortfall, since both symptoms appear on the one fist whose gaps are shorter than its dits. | The gate exists to make fixtures falsifiable, and the first thing it falsified was a fixture. | HM-DEC-101 |
| 2026-08-17 | **Chat deliveries are a single scaffolded zip and a pasteable Claude Code prompt, and nothing else** — restating §9.1 because this conversation broke it repeatedly, presenting loose documents one at a time and twice asking Tim to hand-edit a file. It covers governance and record files exactly as it covers source: a delivery is extracted over the repo root, never placed by hand. Snippets, fragments and "add this line" are not deliveries. | Tim extracts and commits; every file he has to place or patch himself is a step Claude was supposed to have taken, and a chance for the tree to diverge from what Claude believes it wrote. | HM-DEC-100 |
| 2026-08-17 | **Every prompt and every work order opens with `PROJECT: Hamlet`, and a session's first action is verifying that gate against the tree rather than against the prompt's own claim.** No gate line, or a gate naming another project: the session says so and stops, without adapting or partially applying anything. Tim runs several projects and the failure this prevents is a session confidently editing a repository it was never written for — which no test catches, because the tests it runs are the wrong project's. Claude's deliveries carry the gate too; one written without it is defective and redone. | A misdirected prompt is the one class of error where being capable makes the damage worse. | HM-DEC-099 |
| 2026-08-17 | **An automated transmit cycle is built and exercised into a dummy load only, and reaching an antenna is a separate ruling taken afterwards.** §0.2's first sentence stands unamended: nothing transmits unattended. Auto-CQ is automated repeating transmission and cannot be reconciled with that sentence by argument, so it is proved against a load first — every stop condition watched to fire, including the USB link pulled mid-cycle — and the on-air question is re-opened once there is something to reason from. HM-DEC-008 unchanged. Governance had already named the abort this needs: `0x17` with `0xFF`, which is also the keying method's documented stop code. | Reasoning about an interlock is not seeing it work, and the cost of being wrong lands on somebody else's band. | HM-DEC-098 |
| 2026-08-17 | **The CW decoder refuses below 0 dB SNR rather than copying into the band where it is half wrong.** Measured on the sensitivity sweep: from 18 dB down to 0 dB the whole message returns with nothing wrong; at −1 dB one character in five is wrong; at −2 dB it emits a full message of which **44% is invented**; below −3 dB it already refuses. A trained ear copies to roughly 0 dB, so refusing there meets the stated goal — decode almost anything the operator can hear — rather than falling short of it. **A degraded label was rejected as a substitute**: marking a message does not make a plausible wrong callsign on screen any less actionable. | §0.0's practical test decides it one way — at −1 dB the operator can act and be wrong, and at −2 dB nearly half of what he acts on was never on the air. | HM-DEC-097 |
| 2026-08-17 | **A work unit is five or six ordered phases, every report ends with `RECORDED` / `NEEDS A RULING` / `STATE`, and one narrow class of conclusion becomes the session's to record rather than Tim's** — decided one way by a governing principle, superseding nothing, weighing no trade-off, reproduced in full so it can be overridden. Anything touching §0.0, §0.1, §0.2, transmit or what the display asserts stays Tim's without exception, and the attribution rule is untouched. **Tim does not edit files**; a delivery requiring him to hand-patch anything has failed §9.1. Full text in `SESSION_PROTOCOL.md`, summary in §12. Measured on the parent project: the same model on the same repository went from a five-minute session producing three fixes to consecutive forty-seven and fifty-one minute sessions producing six phases, ninety-one tests and a live defect found and fixed. | A session's authority is exactly as wide as the plan it was given, and a queue that mixes the conclusions needing judgement with the ones that could not have gone another way buries the ones that need him. | HM-DEC-096 |
| 2026-08-17 | **A note is chosen by how it is keyed and never by how loud it is, the operator's own transmission is not evidence about anybody else, and a sender's gaps are classified by clustering that sender's own gaps.** **On a branch and not merged**: the measurements hold and eleven synthetic-fixture tests regressed (HM-OPEN-016). The old detector was wrong on all three real recordings, including one answer that is neither the loudest thing nor the real one nor the configured pitch. **The brief's own hypothesis was tested and does not hold**: the keyed station is present more of the time than the carrier, so duty cycle separates nothing. What does is whether the mark lengths are two clusters or one smear. He is transmitting for eighteen of thirty seconds and what is audible between his own elements is not an element. A muted receiver is quiet and an empty file is zero, a hundred and fifty decibels apart. | Six years of copying by ear, and a station answering a call that produced one character. | HM-DEC-095 |
| 2026-08-17 | **The scope frame is three header bytes, both counts are BCD, and nothing state-dependent runs before the radio has answered anything.** **Two stacked bugs and the first was not the suspected one**: field 1 is a fixed zero the parser read as the part order, so every order was nought and every part of every sweep was discarded. Then the base: `0x11` is eleven, and the arithmetic proves it without the manual, since 475 points cannot be carried in seventeen parts of fifty bytes. **The fixtures were built from the same misunderstanding as the parser**, which is why they all passed for months. One `Populated` gate now precedes anything state-dependent, after three separate faults raced the same poll sweep. Below the fold is not off the edge, and a meter and a recording answer different questions. | 2,740 parts in, 2,740 thrown away, and a suite that was green throughout. | HM-DEC-094 |
| 2026-08-17 | **Every stage of the scope path is counted, and no session may report the waterfall working without a nonzero frame count from a connected radio.** It has been reported working three times and has never drawn a pixel from a radio; none of those claims was checkable because nothing counted. Received, parsed, rejected with its first reason, and delivered, **on the display and not only in a log**, because "band is quiet" and "nothing has ever arrived" paint the same picture. Reading eliminated two candidates: the composition root is right and the renderer marshals properly. **Closes HM-OPEN-013** with Tim's citation for `1A 05 0074`, read so a precondition becomes a measurement and never an errand (`SHACK_FACTS.md`). | No radio was connectable, so the measurement was built and nothing was fixed blind. | HM-DEC-093 |
| 2026-08-17 | **Hamlet asks the radio for its spectrum instead of advising about it, and a display is subject to §0.0 exactly as a sentence is.** The panel read `27 11`, found it off, and named two menu settings as the cause **having read neither**; both were already correct and the operator walked to the radio for nothing. `27 11` is send/read and tier one, so it is attempted and its answer recorded. Three states get three sentences, and what cannot be read is said to be unread. **A waterfall asserts things and is harder to catch than a sentence**: it draws what was measured, an empty band renders empty, and an axis is a claim. The link now reports commands sent, answered and unanswered. **Supersedes HM-DEC-067** on the point it got wrong: one of the two switches is a command Hamlet can send. | An evening spent checking two settings that were right, because the app asserted a cause it had never looked at. | HM-DEC-092 |
| 2026-08-17 | **Recordings made on the air are permanent fixtures, and a capture header names the radio's frequency rather than Hamlet's idea of it.** **The seven synthetic fixtures all passed while the decoder was deaf on the air**, which is the whole argument: two real recordings did in an afternoon what they could not do at all. They assert what was measured and what §0.0 forbids, never a transcript, because nobody knows what that station sent. The tone is now found at 28.6 and 18.8 dB and **the characters still do not resolve**; the reason is diagnosed, and a fix that works on real audio but makes the decoder confidently wrong on a fade was measured and deliberately not shipped (HM-OPEN-012). | 1,732 characters out of band noise, and a real station nobody could read. | HM-DEC-091 |
| 2026-08-17 | **A keyed signal is measured while it is keyed, nothing is emitted without a tone to emit it from, and a capture that cannot prove it is fresh is not written.** **The cause was time, not frequency**: the decoder was already narrowband, and both the reported ratio and the located pitch were averages over the ninety-six percent of a recording in which a station answering a call is silent. Both become held peaks, and a threshold calibrated from measured noise and the decoder's own working limit. The emission gate is safe only because the measurement under it was fixed first, and it costs a measured decibel of reach, stated rather than buried. One guarded speed read by every surface. | A signal fifty decibels out of the noise reported as minus nought point six, and seventeen hundred characters out of half a minute of band noise. | HM-DEC-090 |
| 2026-08-16 | **Hamlet does not offer a send the operator is not licensed to make, and a restored widget has to be somewhere they can see it.** The privilege refusal becomes a readiness precondition reading the band map's own data, settled before the radio is blamed, naming the nearest frequency the license does cover. **Supersedes HM-DEC-065**: an unknown class now refuses rather than warning, and what that ruling protected is kept, because the guard is the operator's to switch off and nobody is locked out of their own transmitter. **The canvas always came back; it came back off the edge of the screen**, so anything entirely out of view is rescued and the canvas says it did it. | The one place grey may mean you cannot do this, and a workspace rebuilt by hand every evening. | HM-DEC-089 |
| 2026-08-16 | **The decoder measures the noise beside the tone, integrates over the element, keeps what it heard, and says what it can see when it decodes nothing.** **The measurement corrected the brief**: the decoder was already narrowband, so the stated cause was not the cause. Each candidate was swept one at a time and two were rejected on evidence, including one that broke speed tracking for a decibel. **Reads down to −5.0 dB against −3.0 before, and returns the message perfectly where it used to get one character wrong every run.** Thirty seconds of exactly what the decoder heard writes to a WAV with the rig state beside it. The two audio paths are named, the Windows side is read where it can be read and named where it cannot, and everything reported is a measurement of the audio and never a claim about a station. | Six years of copying signals by ear that produce nothing on screen, and no way to tell whether any change helped. | HM-DEC-088 |
| 2026-08-16 | **The top strip becomes one row.** Bands beside the readout rather than above it, the privilege line and the way to your places on one line under them, and the hint about the scroll wheel retires once it has been used. Band card widths scale down together, since **the ratio is the meaning and not the size**. About 150 px given back on every screen, short of the 200 asked for: the rest is the readout's own drawn height, which is the app's anchor and not this session's to shrink. | A third of the window height spent on a band, a frequency, a mode and a hint, above a canvas that is where the work happens. | HM-DEC-141 |
| 2026-08-16 | **A control's resting look says press me, grey is reserved for genuinely unavailable, and every binding in the window must resolve or the build fails.** Seventeen canvas controls were dead because their bindings cast an items control's data context to the wrong type: Avalonia yields null on a failed cast, and **a button with a null command is indistinguishable from a disabled one.** Nothing failed. So the window is now built headless in a test and any unresolved binding fails it, which found two more nobody had reported. The resting style is fixed once for the whole application rather than a fourth time per screen. **Dragging never worked**: the frame walked the logical tree for a canvas that is only a visual ancestor. A pointer was driven this time. | Three narrow fixes to the same fault in three sessions, and a canvas nobody could move a widget on. | HM-DEC-087 |
| 2026-08-15 | **The panels become widgets on a canvas the operator arranges.** The top strip stays put and cannot be closed or moved, widening HM-DEC-021's rig-display exemption to the whole strip. Free placement with snapping and never a grid; presets above the canvas rather than in a menu, named by activity, and **a preset is a starting point and never a document**, so pressing one loads a fresh copy every time. Nobody ever starts on an empty canvas. Saving is one action from where you are, into `layouts.json` beside the profile. **Some widgets arrive on their own**, phrasebook first, and one the operator has moved is theirs from then on. **A widget that is not out still carries its news**, with nothing lost while it is away. | One column in the order things were built, and the operator scrolling past the ten they were not using to reach the two they were. | HM-DEC-086 |
| 2026-08-15 | **A transmission is one state, from the press to the last dah, and the send controls change once each way.** The third attempt at the blinking, and the first two are recorded because both looked right: sampling the transmit line, then latching on the send call, which returns in thirteen milliseconds while the radio keys for eighteen seconds. **Handing the message over is not the transmission.** The duration is arithmetic and known before the first dit, and the transmit line may only extend it, **never shorten it**, because sampled four times a second it shows a second and a half of apparent quiet inside a real CQ. An operator stop ends it immediately. The reported seconds become the transmission rather than the handover, and how the end was established is recorded beside them. | The panel strobed through every send, the record said a hundredth of a second for an eighteen-second call, and that figure reached the screen as "the radio keyed for 0 seconds". | HM-DEC-085 |
| 2026-08-15 | **Hamlet changes the radio, and never shows a rig control. The writes ruling HM-DEC-050 deferred.** **Settings are consequences of intent, never things the operator operates**, and no screen may carry a control corresponding one-to-one with a radio setting. Three tiers, and the tier is the safety design rather than a prompt on everything: receive side done and mentioned, what others hear offered, and the one that keys gated like a send. No byte written that is not cited; read before write and read back after; announced, undoable, and unknown stays unknown. The list comes from live state, says what is already right, and says what it could not read. | Six years, and two evenings lost to a receive gain at 42 percent and a wide-open filter that Hamlet could read and could not change. | HM-DEC-084 |
| 2026-08-15 | **Sending gets no look of its own, and the notice about the back of the radio is deleted.** Both Tim's, superseding work from two rulings ago. You cannot send while sending, so grey is correct and the status text does the rest; the latch stands and only its color goes. The notice is not retired on evidence but gone, because HM-DEC-082 answers its question with a measurement instead of a caveat. The per-message teaching lines are proposed for cutting and not cut. | A state that needs its own color to be understood has not been explained; and a sentence with a number in it beats a paragraph admitting ignorance. | HM-DEC-083 |
| 2026-08-15 | **After every send Hamlet reports what happened link by link and names the link that failed. This is the question the application exists to answer.** Five links, four of them machine-checkable and needing nobody else to cooperate: command acknowledged, radio keyed, power made (`15 11`, added here), power into a real load, somebody copied it. **A station making no power and a band with nobody listening are different facts and used to look identical: silence.** Every number is measured or absent, an unread link is not a failed link, a percentage and never a wattage, the skimmer count says what it actually measures, and nothing diagnoses the station. | Six years of not knowing whether he was speaking into the void or on the air with nobody listening, which is what Hamlet was built for. | HM-DEC-082 |
| 2026-08-15 | **Hamlet reads the SWR meter during a send, cites its scale, and never says what is connected.** `15 12`, four cited points on p. 19-3, linear between them and refusing past 3 to 1. Unknown the moment the transmitter stops, because a resting radio has no standing wave ratio. Above 1.5 it gives the manual's own tuning advice (p. 11-2). **The notice about the back of the radio now retires once a send has measured something**, persisted, since a warning nobody reads teaches everything near it to be ignored. | A dummy load reads flat and an antenna reads low, which is suggestive and not evidence, and this is the one screen where a wrong answer means keying into the wrong thing. | HM-DEC-081 |
| 2026-08-15 | **The send buttons had no style at all and fell through to the theme's grey, and status messages now occupy reserved space rather than appearing.** Ready is filled amber, armed deeper amber, sending green, and only refused is pale and dimmed. The complaint was heard three times as a state bug; a passing test about style binding gave a false pass, because when a complaint is about appearance the check is a screenshot. The reserved-space rule is a layout standard: nothing may move when a value the radio reports several times a second changes. | Their ordinary, fully-enabled appearance was itself grey, so a working button and a refused one looked identical and the operator assumed the worse. | HM-DEC-080 |
| 2026-08-15 | **Grey means refused and nothing else, and the confirming press guards what the operator wrote rather than what Hamlet wrote.** Two durable rules. Armed and sending are active states drawn at full strength; only a readiness refusal may look disabled, and a refusal always prints its reason. Unedited text sends on one press, edited text takes two, reverting disarms, and there is a way back out. **Supersedes HM-DEC-059's two-press default**, keeping the toggle as an option that is off. Sending is latched on the operation rather than sampled from the transmit line, and the send itself is in the record by length and duration but never by its words. | Transmit works and the operator could not tell; he pressed, saw nothing, and concluded the button was broken, and he built this. | HM-DEC-079 |
| 2026-08-15 | **The send buttons were destroyed and rebuilt four times a second, which is why they were dead.** The rig monitor raises state every poll whether anything changed or not, `Rebuild` cleared and repopulated on every one, and a press and its release cannot land on the same control that no longer exists. The gate, the threading and the notification were all correct. Rebuilding now happens only when the offer changed, the command carries the gate rather than only the visual tree, the UI-thread seam is named and guarded, and the button's own state is written beside the engine's verdict, as an error when the two disagree. | The record described the engine and not what the operator saw, so a log saying Ready and a screen saying no could not be told apart. | HM-DEC-078 |
| 2026-08-15 | **Telemetry becomes a decision record: every decision point that can go more than one way names the branch taken and the state that determined it.** Outcome, a stable reason token, and `determinedBy` with provenance and age. Unknown, off, unsupported and stale stay four different things in the file and on screen. The rig state travels with connects, readiness and decoder transitions, with a delta heartbeat. Levels mean something. The decoder reports counts and rejections without allocating per character. A refusal emits with nobody pressing anything, and a "What Hamlet decided" window sits beside the rig one. Recorded in §8.1. | A live attempt failed with the buttons greyed out and 144 events said nothing about it; Hamlet logged what it did and never what it decided. | HM-DEC-077 |
| 2026-08-15 | **Hamlet follows where a contact has got to and says when it has lost the thread. The model only, no interface.** Lost is the default and was designed first, because a guide that silently keeps guessing sends somebody confidently to the wrong part of a ritual they have never performed. Transitions come only from what was sent and what resolved cleanly; a half-read word moves nothing, and evidence goes stale at four minutes. | Knowing which part of a contact you are in is most of what a beginner lacks, and a wrong answer about it is worse than none. | HM-DEC-076 |
| 2026-08-15 | **Hamlet watches the skimmer network for the operator's own callsign and says who heard him, answered or not. Closes FG-008.** Three states: a wait that says what is normal because that is where a beginner gives up, a heard state naming receivers and the strongest report, and a silence that says it is not proof nobody heard him. Reports kept from the first one. **The distance-led presentation is not built**: RBN gives no skimmer location and HM-DEC-038 forbids deriving one from a prefix, so that half waits on cited data (HM-OPEN-010). Reads only, and a test proves no telemetry event can be handed a callsign. | Six years licensed and one contact; this is the first honest answer he has had to "did that work", and it is worth nothing the moment it becomes encouragement rather than evidence. | HM-DEC-075 |
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
| Writes: receive side | `16 41` auto notch, `16 48` manual notch, `14 0D` notch position, `16 22` NB, `14 12` NB level, `16 40` NR, `14 06` NR level, `16 12` AGC (**`00 to 03`, 00=off**), `16 02` preamp, `11` attenuator (00/20), `14 02` RF gain, `14 03` squelch, `14 09` CW pitch, `14 01` AF, `14 0F` break-in delay | 19-3 |
| Writes: receive side, cont. | `1A 03` filter width, `16 56` DSP filter shape, `1A 05 0059` ACC/USB output, `1A 05 0060` ACC/USB AF level, `1A 05 0025` RF/SQL function | 19-4 |
| Writes: `1A 05 0061` | ACC/USB squelch gate. **On 19-5, not 19-4 with its neighbors** | 19-5 |
| Writes: keys the radio | `1C 01`: 00=tuner off, 01=tuner on, **02=start a tuning cycle, which transmits** | 19-7 |
| `16 65` IP+ | **WRITE ONLY. Reads "Send the IP+ function setting" where neighbors read "Send/read".** Excluded from the write table: a write that cannot be read back cannot be confirmed or undone (HM-DEC-084) | 19-4 |
| ALC / Vd / Id meters | `15 13`, `15 15`, `15 16`. Meaningful only while transmitting | 19-3 |
| Po meter | `15 11`; `00 00`=0%, `01 43`=50%, `02 13`=100%. **Only meaningful while transmitting** (HM-DEC-082). Not the same as `14 0A`, which is where the power control is set | 19-3 |
| SWR meter | `15 12`; `00 00`=1.0, `00 48`=1.5, `00 80`=2.0, `01 20`=3.0. **Only meaningful while transmitting** (HM-DEC-081). Matched at or below 1.5, and above that hold TUNER (p. 11-2) | 19-3, 11-2 |
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
| Decoder fixtures | `tests/fixtures/cw` — synthesized WAV, 8 kHz 16-bit mono, regenerated byte for byte from the request beside each one (HM-DEC-007, HM-DEC-048). **`tests/fixtures/cw/captured` holds recordings made on the air**, each with the sidecar written at the moment of capture, and they assert what was measured rather than a transcript nobody knows (HM-DEC-091) |
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
logging. A failed write is dropped and counted, never propagated. This binds
hardest on the decoder's own record, which runs on the audio thread: aggregate
over an interval and allocate nothing per character, because a decoder that
stutters to write its diagnostics has traded the thing for the record of it.

### 8.1 The record is a decision record — HM-DEC-077

> **Every decision point that can go more than one way emits an event naming the
> branch taken and the state that determined it. Significant events carry the
> rig state as Hamlet believed it at that moment.**

The fault this replaces, in one sentence: **Hamlet logged what it did and never
what it decided.** Every event was a completed action, so there was no record
anywhere of a thing Hamlet chose not to do or tried and failed at. A refusal is
an outcome. A failure is an outcome. Both are as loggable as success and more
useful, because success is the case nobody ever has to diagnose.

What follows, and is not to be re-argued:

- **Every outcome event carries the same three things.** `outcome` — proceeded,
  refused, failed or degraded. `reason` — a **stable machine token**, never a
  display string, because a display string gets reworded the next time somebody
  improves the copy and takes every comparison across sessions with it.
  `determinedBy` — the state values that decided it, each with its provenance
  and the age of the reading.
- **Unknown, off, unsupported and stale stay four different things**, in the
  file and on the screen. Refusing on unknown is correct (HM-DEC-050); refusing
  on off is something the operator can walk across the room and fix. A record
  that conflates them is worth nothing on the evening it is needed.
- **A row nobody read says unknown and never zero.**
- **Levels mean something.** A refusal is a warning, a failure is an error, a
  reconnect nobody asked for is a warning however well it went. Anything a
  person would want to find by scanning is not `info`.
- **A decision emits when it is made, not when a button is pressed.** A disabled
  control fires no handler, so a record that only listens to handlers cannot
  tell "Hamlet refused" from "Hamlet is broken" from "nobody pressed it".
- **The reason reaches the operator, not only the file.** A file somebody has to
  upload is the second line of defense; the screen is the first.
- **HM-DEC-018 holds without exception.** No callsign, no name, no location, no
  message content, no decoded text, no upload. The payload shapes have nowhere
  to put them, which is stronger than every call site remembering.

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

**Every delivery ships fully scaffolded in repo path structure**, as a **zip**
whose internal paths are relative to the repository root — **even for a single
file, and even for a governance or record file** (HM-DEC-100). Extract over
`C:\Source\Hamlet` and files land where they belong. No loose files, no
individually presented documents, no path instructions in prose. Tim extracts
and commits; he does not place files by hand, and he does not edit them
(§0.4).

The only thing Claude delivers outside the zip is a **Claude Code prompt he can
copy and paste**, gated per §0.3.1. Nothing else — no code snippets, no
fragments, no "add this to that file".

Files compile without modification. Too large for one response → split into
multiple complete files, still zipped. Ask for files by repo path, not by name.

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

### 9.5.1 One branch, and it is `main` — ABSOLUTE

**Every session commits to `main` and pushes to `main`.** No session creates a
branch, switches to one, or works on one. If a work order genuinely needs
isolation, the session says so and stops; Tim rules and names the branch.

This is written against a real failure. Three consecutive sessions worked on
`feature/honest-cw-detection` — the honest CW detection, the fixture rebuild,
the generator fix, the scanner, and the whole UI work order, roughly twenty
commits — while `main` stood still at the scope frame fix. Tim ran his radio
against `main` for an evening, reported a feature as unbuilt, and was right: the
code that built it had never reached the branch he was on. **No ruling ever
authorized a branch.** A session invented one, each session after it inherited
the invention as settled, and four reports said "pushed to
`feature/honest-cw-detection`" without one of them treating that as a question.

- The report names the branch in section 1, every time (§12.2).
- Uncommitted or unpushed work at the end of a session is stated in section 2,
  with the count.
- §9.5's rule already covered this and was not applied: **a decision that is not
  in `DECISIONS.md` is not made.** A convention nobody ruled on is not a
  convention, however many sessions have followed it.

### 9.5 Claude Code sessions

Reads the tree directly, edits in place, builds, commits (§7 format) without
per-file confirmation. **A decision that is not in `DECISIONS.md` is not
made** — if code needs a ruling no record contains, the session stops and
asks, exactly as it would for a missing file. Within an approved plan, act;
report once at the end of a work unit, in the format §12.3 requires.

### 9.6 The work order — ABSOLUTE, Claude Code sessions

A work order is a file, never pasted text (HM-DEC-135). It is
`WORK_INSTRUCTIONS.md` at the repository root, it arrives in the delivery zip like
any other file (§9.1), and it is committed.

The prompt Tim pastes is two lines and nothing else:

    PROJECT: Hamlet

    Read WORK_INSTRUCTIONS.md at the repository root and execute it.

**Both carry the gate and the session verifies both against `PROJECT_CARD.md`**
(§13, HM-DEC-099). A one-line prompt in the wrong repository would read that
project's work order and find a gate that agrees with itself, which is the one
failure §9's gate exists to prevent.

The file opens with `PROJECT:` and `ISSUED:`. It is overwritten whole per work
order, so a session opening one dated earlier than `OUTPUT.md` is holding work
already done: it says so and stops.

**Its phases are worked as numbered.** A phase is skipped only where the order
names it droppable, and an item the order names and leaves (§12.6) is not worked
instead. A session that departs from the numbering says so in section one.

**Every order carries the status instruction of §13 and states its phase count**
(HM-DEC-137). An order missing either is defective and is redone; the session
writes the status regardless.

**And every order carries `Asks still outstanding`**, the same queue the report
carries back (§12.2, HM-DEC-139). An order without it is defective in the same
way, and the session reconstructs the queue from `OPEN_ISSUES.md` and the recent
reports rather than treating the omission as an empty queue.

`WORK_INSTRUCTIONS.md` in, `OUTPUT.md` out (HM-DEC-106). Both at the root, both
committed, both in the tree the session is changing.

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


---

## 12. How work units are scoped and how sessions report

Full text in `SESSION_PROTOCOL.md`. Ratified as HM-DEC-096. What follows is the
part that must be true even if that file is not open.

**Why it exists.** A session's authority to act is exactly as wide as the plan
it was given. A prompt naming three defects produces a session that fixes three
defects and stops, correctly — that is compliance, not underperformance. And a
session with nowhere to put a conclusion hands everything back at one priority,
so the conclusions needing Tim's judgement are buried among the ones that could
not have gone another way. **When a session keeps stopping to ask about things
the plan plainly covers, the plan was too thin, and that is Tim's to fix rather
than the session's.**

### 12.1 What a session may record itself — narrows §0.4

A session may write to `DECISIONS.md` when, and only when, **all four** hold:

1. A governing principle in this file decides it and **the reasoning runs one
   way** — once the constraint is stated, no second answer survives.
2. It supersedes no existing ruling and acts against none.
3. **It is not a trade-off.** If it weighs two costs against each other, it is
   Tim's.
4. **The report reproduces the entry in full**, so Tim can override it.

The test is not "is this obvious." Obvious is a feeling, and in the parent
project it was wrong eleven times. The test is whether an alternative can be
**stated** that survives the governing principle. **The practical tell:** an
entry containing "on balance", "the cleaner option" or "we felt" has already
failed, because each is the sound of two costs being weighed.

**Anything touching §0.0, §0.0.1, §0.1, §0.2, transmit, or what the display
asserts is Tim's without exception** — those are exactly the places where a
one-way argument is most likely to be a blind spot rather than a proof.

The attribution rule is not part of this relaxation and stays absolute. An
entry written under this section says so on its face and cites the principle
that decided it.

### 12.2 Every report is four sections, written to `OUTPUT.md` — MANDATORY

**The report is written to `OUTPUT.md` at the repository root, overwriting it,
and the same text is printed to the session.** Tim pastes that file rather than
screenshotting a terminal, and a report that exists only in scrollback is a
report he has to photograph.

Four sections, in this order, no prose between them, no other headings:

**1. What Claude did.** The branch, named explicitly (§9.5.1 — it is `main`).
What was built, what was measured, what the numbers were. Any decision recorded under §12.1 appears here **with its id and full
text**, never summarized. If nothing was recorded, say so in one line.

**2. What Tim should expect.** What is now true of the app and the suite, what
he will see if he runs it, and **what will look wrong but is not** — a red
count that is the known baseline, a fixture held out on purpose, a capability
that is engine-side only. Build status, tests passing and failing with the
failing ones named, what was pushed and to which branch.

**3. What we should do next.** The work this session made possible or
necessary, in the order it should happen, one line each.

**4. What's blocking us.** Anything that stops the next step, and **every
question needing Tim's ruling** — each in `DECISIONS.md`'s own format, ruling
first, then reasoning, then what was rejected and why, no id assigned. Ordered
with the one blocking the most work first. Empty is a real answer.

**Section four ends with a standing heading, `Asks still outstanding`, and it is
present even when the queue is empty** (HM-DEC-139). It lists every ask from any
earlier session that Tim has not yet ruled on, each with the date it was first
made, what it is waiting on, and where the change it concerns already sits in the
tree. It is carried forward **verbatim** until he rules, and dropped by the report
that records the ruling. **A report without the heading is defective and is
redone**, and an empty queue says so in words, because an absent heading and an
empty queue are the same sight.

The work order carries the same list inbound (§9.6), for HM-DEC-137's reason: both
of this project's channels have failed in the field and one of the two will catch.
**An ask that was answered is dropped rather than carried**, since a queue holding
settled questions is worse than none.

**What belongs on it: a question handed back for a ruling in a session report, and
nothing else** (HM-DEC-140). An ask in a report has no other home, because the next
session overwrites `OUTPUT.md`; an entry in `OPEN_ISSUES.md` has an id, an owner and
a date already, and folding those in would bury the few real questions among twenty
that want a capture file or a manual page.

The first line of section 1 states the surface and the gate: which machine,
which project name the prompt claimed, and what in the tree confirmed it
(§0.3.1). A session on the development computer states there that **nothing in
its report is evidence about the radio** (`SHACK_FACTS.md`, HM-DEC-093).

The load-bearing split is between what a session settled and what it is handing
back. Section 1 is the first; section 4 is the second. **An unlabelled mixture
of them puts the triage back on Tim**, which is the cost this exists to remove.

### 12.3 Work units are written wide, in ordered phases — Tim's side

Five or six phases, each independently committable, ordered so each is
buildable when reached. **Every work order ends by requiring the report in
`OUTPUT.md`** per §12.2 — a plan that does not say so gets a report Tim has to
photograph. The plan **names the phase to drop** if the session
runs out of room, and the session **says it dropped one** rather than
half-building it. Every plan ends with what not to do next — "if you finish
every phase, stop and report; do not start the next work unit" — because a
session finishing early otherwise wanders into unscoped work. A phase that
cannot be built without a ruling says so in the plan, so the session raises it
first instead of discovering it at the phase boundary.

### 12.4 Marked assumptions

Where a value cannot be confirmed it goes in a data file under `/data` carrying
`source: guess` or `source: extrapolated` and a named `confirm` owner — never
omitted, never asserted. An unmarked wrong value is indistinguishable from a
right one; **a marked one is a question with an owner.** This is §0.0 applied
to the repository instead of the screen, and it is what makes proceeding
without ratification affordable rather than reckless.

### 12.5 A fixture built from the same misunderstanding as the code proves nothing

Two faults in this repository survived months of green tests that way: the
scope parser and the fixtures that certified it, and the CW noiseless fixtures
whose digital silence between elements no receiver ever produces
(HM-OPEN-018). **When a test passes and the instrument disagrees, suspect the
fixture.** Rebuilt fixtures land under new names, the old failures are
adjudicated one at a time with a recorded reason each, and a reference
implementation must score well on a fixture before that fixture is allowed to
judge Hamlet. A fixture the reference cannot decode is a bad fixture, not a
Hamlet failure.

### 12.6 Do not repair unrelated things on the way past

Name them in `OPEN_ISSUES.md` and leave them. A session that fixes what it
passed is a session whose diff nobody can review.

---

## 13. Reporting status to the panel — `ANNUNCIATOR.md`

Tim runs several projects at once, in separate sessions and separate windows. A
panel reads two small files from each repository and shows every project on one
screen, so he can see which window a clipboard belongs in before he pastes.

**The panel only knows what a session last wrote.** A project whose sessions never
write reads as dead while it is working — that has already happened elsewhere, an
hour of green tests behind a panel showing blocked, because nothing in that tree
told the session to write.

The full rules are in `ANNUNCIATOR.md` at the repository root. **The parts a
session must not have to go and look up are here**, because this file is read
automatically and a companion file is read only if something points at it.

### 13.1 Two files at the repository root

`PROJECT_CARD.md` — standing facts, five lines, **changed only by ruling**:
`PROJECT`, `ONE_LINE`, `REPO_PATH`, `REMOTE`, `TRUNK`.

`PROJECT_STATUS.md` — volatile state, **overwritten whole**, six lines:

| Field | Values |
|---|---|
| `STATE` | `PREPARING_PROMPT` · `ANSWERING_QUESTIONS` · `EXECUTING` · `COMPLETED` · `BLOCKED` — **one of those five words and nothing else** |
| `PHASE` | `n of m`, the phase reached and the total the prompt gave; `—` when no work order is running |
| `BALL` | `code` · `web` · `tim` · `unassigned` — who must act next. **`unassigned` means nobody has taken it** and is not a polite way of saying it is Tim's |
| `NEXT_PASTE` | `none`, or **what → where**: `OUTPUT.md -> Claude Web`. Name the destination even when it looks obvious; telling identical windows apart is the whole point |
| `UPDATED` | ISO 8601 **with a UTC offset, read from the clock**, never typed. A typed one has already claimed to be five minutes in the future |
| `NOTE` | One line, a caption rather than a record. Anything that must survive the session goes in the decision log and the report as usual |

**Both files are read as the leading run of `KEY: value` lines, stopping at the
first blank line, `---` or `#`.** So every value is one line and no value begins a
line with `#`. Prose goes below a `---`; nothing reads it.

**Never report branch, commit or working-tree state.** The panel reads those from
`.git` itself. A measured fact stays true when a session goes quiet and a typed one
does not.

### 13.2 When a session writes — supersedes HM-DEC-131 on the triggers

- when it starts work
- at each phase boundary
- when it stops for a ruling
- when it finishes
- **every ten minutes while `STATE: EXECUTING`**

**The ten-minute rule is the one that matters, and it is why HM-DEC-131's "at each
state transition and at no other time" no longer holds.** Phases here run an hour —
a fixture rebuild, a corpus re-measurement — and **a long phase and a dead session
look identical** without it. On that write, `NOTE` says what is happening *inside*
the phase — `Phase 3 — rebuilding the AR/IR fixtures, 4 of 11 rebuilt` — so there
is something moving to look at.

Outside those triggers, do not rewrite the file to say the same thing again: a
rewrite with an unchanged state is a heartbeat, and a heartbeat makes staleness
meaningless.

### 13.3 What is never written

- **No invented state or `BALL`.** If neither fits, it goes in `NOTE` or it is a
  decision ask.
- **No count or timestamp that was not measured.** `none`, `—` and `not run` are
  real answers; a plausible number is not (§12.4).
- **The card is not edited during a work order.** It holds standing facts; if one
  is wrong that is a ruling, not an edit.

### 13.3.1 Where this instruction lives, and who is bound by it — HM-DEC-137

**The status-write instruction is carried twice: here, and in every Claude Code
work order. A session writes the status whether or not the order it was handed
says so.**

Neither channel has held alone, and both have now failed in the field. A rule only
in this file is read once at the start and forgotten across a phase that runs an
hour, **which is precisely the phase the ten-minute write exists for**: three
consecutive sessions did not apply §13.2, and one of them said so in its own report
— §13 was read, and not applied. A rule only in the prompt is lost whenever a
prompt is written in a hurry, and every order delivered to this project had been
missing the closing line `ANNUNCIATOR.md` already required of it.

- **An order that does not carry the status instruction, or does not state its
  phase count, is defective and is redone** (§9.6). That is HM-DEC-099's shape: a
  prompt without its gate is defective too, because the failure it prevents is one
  the session cannot detect from inside.
- **A defective order does not excuse the session.** It writes the status on
  §13.2's triggers regardless, reports the missing line in section one, and gets
  on with the work.
- Nothing here changes what is written. HM-DEC-132's five triggers and six fields
  stand exactly as §13.1 and §13.2 have them.

### 13.4 What this software can do to the physical world

**HAZARD: Hamlet keys an Icom IC-7300 HF transmitter via CI-V, including an
unattended repeating cycle — dummy load only until §0.2's first sentence is
amended by separate ruling (HM-DEC-098, HM-DEC-008) — and it commands the
operator's VFO under §0.2.1.**

That line rules nothing. §0.2 and §0.2.1 rule it, and it is restated here because
the annunciator's card is five lines with no room for it, and a session that
learned where the repository is without learning that it can put a hundred watts on
somebody else's frequency has learned the wrong half.

**RATINGS: the cited machine limits are §4's and are not copied here** — command
`17` takes 30 characters, CW pitch runs 300–900 Hz in 5 Hz steps, the radio answers
at `94h`, the IF filter indexes 50 Hz to 3.6 kHz. Every one is page-cited to Full
Manual `A7292-4EX-6` and a second copy would be a second thing to drift (§0).

**The transmitter's power output is the one rating this file does not carry, and it
is not filled in here** (HM-OPEN-038). §4's header says "HF/50 MHz transceiver" and
names no wattage, and no page in that table cites one — partly because HM-DEC-074
and HM-DEC-082 rule that Hamlet reports power as a percentage of the radio's own
range and never as a wattage, so nothing in the application ever needed the figure.
A hazard line is a poor place to introduce this project's first uncited number
(§4, §12.4).
