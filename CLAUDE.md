# Ham Manager — Project Instructions and Decision Record

**Project:** Ham Manager. A C# MVVM desktop application that controls an Icom
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
| `data/vendor/` | Pinned snapshots of documents cited from outside this repository (see §4) |
| `tools/` | The repo-listing and get-files scripts — the chat-session workflow |

---

## 0.0 Prime directive — the reason everything below exists

> **Never present a guess as a decode.** What Ham Manager shows on screen is
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
> tell whether the fault is in the signal, the radio, or Ham Manager itself.**

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

---

## 1. Decision log

Every ruling, most recent first. Detailed records live in `DECISIONS.md`;
this table is the index.

| Date | Decision | Why | Ref |
|---|---|---|---|
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
| 2026-08-12 | **Governance established** — this file, `OPEN_ISSUES.md`, `DECISIONS.md`, `tools/`, ids `HM-OPEN-###` / `HM-DEC-###`, repo `C:\Source\HamManager`, GitHub `TJDixon2022/HamManager`. Carried from Tim's simulator project. | The rules carried are the ones learned by failing there: scaffolded delivery, the canonical collection script, the repo listing, never editing a file not pulled this session. | HM-DEC-001 |

---

## 2. What this project is

Four phases, each delivering something usable on the air:

| Phase | Delivers | Exit criterion |
|---|---|---|
| 1 | CW terminal: connect, key CW from typed text via the radio's keyer, decode received CW to text. Foundation: solution, `RadioEngine`, CI-V serial, audio capture, crude scope display | Call CQ; read the reply on screen |
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
| Solution layout | `HamManager.sln` at root; `src/` and `tests/` solution folders. Engine: `src/HamManager.RadioEngine`. Shell: `src/HamManager.App`. Tests: `tests/HamManager.RadioEngine.Tests` |
| Project settings | `Nullable` enabled, `ImplicitUsings` enabled, `TreatWarningsAsErrors` true — all new projects |
| Test framework | xUnit, no mocking framework; seams are hand-rolled interfaces (`IRig`, `IAudioSource`, `FakeRig`) |
| Audio | NAudio (WASAPI) |
| FFT | `<<<FILL IN — FftSharp vs Math.NET, decide when the audio pipeline is scaffolded>>>` |
| CI | GitHub Actions, build + engine tests, added once the solution exists |
| Data files | `/data` at repo root; vendored citations in `data/vendor/` |

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
| `feat` | New behaviour |
| `fix` | Corrected behaviour |
| `test` | Tests only |
| `docs` | Documentation, governance, records |
| `refactor` | No behavioural change |
| `chore` | Tooling, dependencies |

| Scope | Covers |
|---|---|
| `engine` | `src/HamManager.RadioEngine` |
| `app` | `src/HamManager.App` |
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
file. Extract over `C:\Source\HamManager` and files land where they belong.
No loose files, no path instructions in prose.

Files compile without modification. Too large for one response → split into
multiple complete files. Ask for files by repo path, not by name.

### 9.2 Every delivery is presented as four lists — ABSOLUTE

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

### 9.3 The repo listing — RUN THIS FIRST, chat sessions

`tools/repo-listing/get-listing.bat` produces `repo_listing.txt`: every file,
its size, its modified date, the commit, and whether the tree is dirty.
Default repo root: `C:\Source\HamManager`.

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

## 10. What Claude does at the start of a session

1. Read this file, `OPEN_ISSUES.md` and `DECISIONS.md`.
2. Establish the surface (§9.0).
3. In chat: ask for `repo_listing.txt` if the session will touch code and
   none is held, then request the files the work needs, whole folders.
4. Do not begin work on the strength of a summary. If it is not in a file,
   it was not decided.
