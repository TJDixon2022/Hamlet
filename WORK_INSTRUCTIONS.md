STOP. Verify the project before reading any further.

PROJECT: Hamlet

Check the repository root:
  MUST EXIST:      SHACK_FACTS.md
  MUST EXIST:      src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
  MUST NOT EXIST:  CoreHMI.sln
  MUST NOT EXIST:  MURC.sln

If all four are not as stated, you are in the wrong repository.
REFUSE. Do not read the rest of this file, do not summarise it, do not
adapt it to whatever project you are actually in, and change nothing.
Reply with only: the path you are in, which checks failed, and
"wrong project — nothing done."

If all four hold, say "Hamlet confirmed" and continue.

---

# Work instruction 038 — FT8 reaches the screen

**ISSUED: 2026-08-28. A fresh order, not an amendment. Follows unit 037.**

**Eight tasks; task 8 is the drop. This is a long unit by instruction —
the 45-to-60-minute window is a floor against trivial units, not a ceiling.**

## Why this unit exists

**Unit 037 drew the Digital tab and every value on it is placeholder. The count
of real numbers on that screen is zero.**

The phase's goal is interpreting FT8 at ninety-nine percent, measured against
WSJT-X's decode list on WAVs captured from this operator's own IC-7300. Nothing
can be measured until audio reaches a decoder and decodes reach the screen. **This
unit builds that chain end to end.**

Two things land, paired deliberately: **an audio waterfall drawn from the radio,
and an FT8 decoder wrapped from `ft8_lib`.** The waterfall is the instrument that
says a signal was present when the decoder says nothing, which is the single most
useful thing to hold while a decoder is young.

## Verify this instruction against the tree

**Nothing here describes the tree.** Check every claim against the files and
report any mismatch, including where the work succeeded anyway.

**Everything below about the tree comes from unit 037's report, not from
measurement by this author.** Trust the tree over this order everywhere they
differ and list the differences.

From that report:

- `WorkspaceBoundary` holds three sibling grids — `CwWorkspace`,
  `DigitalWorkspace`, `VoiceWorkspace` — shown by `IsCwMode` / `IsDigitalMode` /
  `IsVoiceMode`, all in `MainWindow.axaml`.
- `CollapsiblePanel` exposes `Title`, `Summary`, `Family`, `IsExpanded` and
  `HeaderAction`.
- `WaterfallControl` takes `Source`, `BandLowHz`, `BandHighHz`, `Gain`,
  `TuneCommand`, and with a null source draws an honest empty state.
- `ModePalette.Digital` supplies `FillBrush` and `InkBrush`; a digital
  `PanelFamily` was added in 037, derived from it.
- The CW capture press binds `CaptureAudioCommand` inside the terminal template.
- `src/Hamlet.App/ViewModels/DigitalIdleText.cs` holds four idle strings that
  nothing reads yet.
- Window-building tests to satisfy: `BindingHealthTests`,
  `EveryResourceKeyResolvesTests`, `ClippingTests`, `TheTabOwnsTheWorkspaceTests`,
  `TheTabsAndTheWorkspacesTests`, `TheOperatingScreenIsLaidOutAsRuledTests`,
  `EachTabShowsItsOwnWorkspace`, `TheCanvasIsGoneTests`.
- App suite 509 of 509; engine 28 failing of 1852, untouched by 037.

**Record the failing counts from the tree before task 2**, because this unit adds
engine code and must be able to say what it changed.

## Rulings in force

**Transcribed in full with what was rejected. Do not re-argue either.**

**Tim's rulings, 2026-08-28:**

> **The digital waterfall is an FFT of the received audio**, taken from the same
> codec tap the CW decoder listens on.
>
> Rejected: the radio's scope stream, CI-V `27 00` — it is band-wide RF in
> kilohertz, and FT8 occupies a 50 Hz sliver of a 3 kHz audio passband, so the
> picture would show nothing useful and the slot grid would have nothing to align
> to. Rejected: the time-frequency array the decoder builds internally — it ties
> the picture to the decoder, so when the decoder is silent the waterfall goes
> dark too, and that is exactly the case worth seeing.

> **The waterfall and the wrapper ship in one delivery**, because a picture with
> no text cannot be judged and the operator wants the chain seen end to end.

> **The only measurement is against real data from the real radio.** Synthetic
> audio may exist as unit tests during development; it never appears in the
> phase's score.

> **The decoder is wrapped first and written in C# afterwards** — Tim believes a
> C# implementation from the beginning is best and accepted the wrap-first order
> on the condition that **if the wrapper costs more than a session or two of
> fighting native binaries and marshalling, it is abandoned and the C#
> implementation begins directly.** A wrapper that will not build cleanly is
> therefore a **finding**, not a failure: report it with what was tried.

> **The capture press keeps the last four complete slots, trimmed to UTC
> boundaries.**
>
> Rejected: the raw thirty seconds the CW press keeps — partial slots are
> unscoreable by WSJT-X, so a third of each file would be dead weight.

> **UTC is measured and displayed, never corrected.** One SNTP query, the offset
> shown, and trimming refuses while the offset is unknown.
>
> Rejected: trusting the PC clock silently — a drifted clock produces trimmed
> files that are quietly wrong. Rejected: disciplining an internal clock — two
> notions of time in one app, and sidecar timestamps that disagree with the
> file's own mtime.

> **Every static string on the Digital tab stays exactly as unit 037 wrote it
> until it is live against real data.** That includes the mode strip's
> `reading it · 9 messages this slot`, which this author proposed changing and Tim
> has held.

**Standing rulings this unit is bound by:**

- **HM-DEC-009** — never present a guess as a decode. FT8 makes this cheap: a
  message passes CRC-14 under LDPC or it never appears. **Nothing partial, nothing
  probable, nothing "likely" reaches the screen.**
- **HM-DEC-026** — simulated signals say so, derived from connection state.
- **HM-DEC-007** — decoder work is tested against WAV fixtures.
- **HM-DEC-004** — the project is GPL-3.0, chosen partly to permit this
  dependency. `ft8_lib` is MIT (Kārlis Goba); a GPL application consuming an MIT
  library is fine in the direction that matters. **Do not re-argue the license.**
- **§0.2** — no transmit of any kind in this unit.

**HM-OPEN-004 is answered by this unit** — wrap `ft8_lib` rather than shell out to
`jt9`. **Write the closing text into the report's decision section for Tim to
enter. Do not mint an id.**

## Status cadence

After each task, before the next, update `PROJECT_STATUS.md` — `STATE`,
`TASK: n of m`, `BALL`, `UPDATED` from the clock, `NOTE` saying what is moving
inside the task, not restating the task name. Same every ten minutes while a task
runs.

## The tasks

### Task 1 — trace before building

**Say what you find rather than confirming this list.**

- **The audio tap.** What captures codec audio today, at what sample rate, and how
  the CW decoder receives it. **Can a second consumer subscribe without disturbing
  the first?** Name the type and the seam. If the tap is single-consumer, say so —
  that shapes task 2.
- **Existing FFT.** What transform code already exists in the engine, from the CW
  tone work and the capture analysis. **Reuse beats writing a second one.**
- **`WaterfallControl.Source`.** Its exact type and the shape of a frame — bin
  count, value range, how a row is pushed. This is what task 3 must produce.
- **The digital view model.** Whether 037 created one or the panels bind to the
  main view model, and where a real decode list would live.
- **The capture path.** What `CaptureAudioCommand` does, where it writes, what the
  sidecar contains, and whether the ring buffer it reads can yield an
  arbitrary-length window rather than a fixed thirty seconds.
- **Toolchain.** Is there a C compiler on this machine that can build a Windows
  x64 native library, and can the tree reach `github.com` to fetch `ft8_lib`?
  **Answer this in task 1, not in task 5**, because it decides whether task 5 is
  possible at all.

Report before writing a line.

### Task 2 — an audio spectrum source in the engine

A new engine type producing overlapping FFT frames of the received audio, covering
**200 Hz to 3000 Hz**.

- **Subscribes to the existing tap; does not open a second capture.** If the tap
  will not carry two consumers, **stop and report** rather than restructuring the
  CW audio path — that path is mid-repair and is not this unit's to change.
- Frame rate and bin width chosen so FT8's 6.25 Hz tone spacing is visible.
  **State the numbers chosen and why**, in the code and in the report.
- **Pure and testable**: samples and an elapsed time in, frames out, no clock read
  below the pump, so a fixture produces identical frames every run (§5.4).
- Reuse the existing transform. If a new one is genuinely needed, say why.

**Acceptance:** a WAV fixture in, a deterministic frame sequence out, asserted by
test.

### Task 3 — the waterfall draws it, with the slot grid

Bind the Digital tab's `WaterfallControl` to the task 2 source, `BandLowHz` 200,
`BandHighHz` 3000.

- **Draw the 15-second UTC slot boundaries** across the waterfall as rules. That
  grid is what makes FT8 recognisable — signals start and stop on the lines, and
  anything that ignores them is not FT8.
- **The grid is drawn only when the clock offset is known** (task 4). Unknown
  offset means no lines and a header that says so, rather than lines in the wrong
  place. A grid drawn at a guessed boundary is HM-DEC-009 in the one place nobody
  would check it.
- **The header summary stops being static** and reports what is really being
  drawn.
- The empty state 037 found stays exactly as it is for the case where no audio is
  arriving.

**Acceptance:** with the training radio the panel shows what training audio
contains, labelled simulated per HM-DEC-026; with a real radio it shows the codec.

### Task 4 — the clock, measured and shown

One SNTP query on connect and periodically after. **Measure and display; never
correct anything.**

- The offset appears on the Digital tab — the mode strip's right end is the
  natural home, but **do not change any static string 037 wrote**; add, do not
  replace.
- **Amber past a threshold you state**, with the age of the measurement beside it.
- **Unknown is a real state and says so in words.** Never assume zero.
- Pure functions over an offset and an elapsed time, so every threshold is
  testable without a clock.

### Task 5 — the `ft8_lib` wrapper

- **Fetch `ft8_lib` from `github.com/kgoba/ft8_lib` and build it for Windows
  x64.** Vendor the source under `third_party/` with its MIT `LICENSE` and a
  `NOTICE` crediting Kārlis Goba, per §4's rules on third-party material.
- **Do not download a prebuilt binary from anywhere.** A native DLL of unknown
  provenance in this tree is not acceptable at any convenience.
- A thin P/Invoke boundary behind an engine-side interface, so the C# decoder of a
  later unit substitutes without the callers changing.
- **The entry point takes samples and a sample rate and returns decodes.** Each
  decode carries at minimum: message text, SNR, time offset, audio frequency.
- **Tested against a WAV fixture** (HM-DEC-007), and against `ft8_lib`'s own test
  vectors where they port.

**If the toolchain answer from task 1 makes this impossible, do not improvise.**
Report exactly what was tried and where it stopped. **That report is the evidence
that decides whether the C# implementation starts instead**, and it is worth more
than a half-working interop layer.

### Task 6 — slots assembled from live audio

Buffer the audio into **15-second slots aligned to UTC quarter-minutes**, and hand
each completed slot to the decoder.

- **Alignment comes from task 4's offset.** Unknown offset means no slots are cut
  and the tab says why.
- A slot that arrives short — the app started mid-slot — is discarded rather than
  decoded, and the count of discarded slots is observable (§0.0.1).
- Decoding runs off the audio thread. **Never block capture.**

### Task 7 — decodes reach the decoded-text panel

The four static rows are replaced by real decodes, newest first, grouped by slot.

- Columns stay exactly as 037 built them: `utc snr dt hz message`. **The message
  column is never truncated.**
- **The message text is what the decoder returned, unaltered.** No parsing, no
  interpretation, no callsign extraction in this unit.
- **`What people are saying` stays entirely static** — its parser is a later unit,
  and a plain-English line derived from nothing would be the prime directive
  broken in the friendliest possible voice.
- The panel's summary reports the real slot time and the real count.
- **A slot that decoded nothing shows the slot with nothing in it**, not a blank
  panel. Silence is a result.

### Task 8 — the capture press, wired *(the drop candidate)*

The press already on the waterfall header keeps **the last four complete slots,
trimmed to UTC boundaries**, with a sidecar beside it.

- Enough audio is retained to yield four whole slots after trimming.
- **The press refuses, in words, while the clock offset is unknown.** An untrimmed
  or wrongly trimmed file is unscoreable against WSJT-X and would poison the
  corpus this phase is measured on.
- The sidecar carries the rig state the CW sidecar carries, plus the clock offset,
  the slot boundaries used, and the decodes Hamlet produced for those slots — so a
  later comparison against WSJT-X has both answers in one place.
- File naming distinguishes these from the CW captures.

**Dropped whole if time runs out, and the report says so.**

## Parked — do not touch, do not raise

The CW decoder and everything in unit 036's residue — the phantoms, the tracker,
admission, the sheet's two spans, the carried asks. The scanner and the calling
cycle. `CHANGELOG.md`. The missing `DECISIONS.md` records including HM-DEC-086's
supersession. The phrasebook and the recent-places row. The prefix table and the
plain-English parser — later units, both.

**Both halves are required: do not touch them, and do not raise them.**

A parked item that genuinely blocks a task is raised once, and says it was parked.

## What not to do

Standing prohibitions are `CLAUDE.md`'s and are not retyped. Unit-specific:

- **No transmit. Nothing keys the radio.** §0.2, no exception in this unit.
- **Do not write any rig setting.** Not the mode, not the filter, not the dial.
  Tim sets the radio to USB-D at an FT8 frequency by hand. Hamlet may **read** and
  report what it sees, and may say plainly that the radio is not set for digital,
  but it changes nothing.
- **Do not edit the CW decoder or the CW markup.** If the audio tap must be
  restructured to carry a second consumer, stop and report.
- **Do not change any static string unit 037 wrote**, including the mode strip's
  status. Add beside them; replace only where a task above says a value becomes
  real.
- **Do not synthesise signals to fill the waterfall.**
- **Do not download a prebuilt native binary.**
- **Do not score anything against synthetic audio.** Synthetic fixtures are unit
  tests; they are not evidence about the decoder's yield.
- **Do not parse, interpret or extract callsigns from decoded messages.**
- **Do not write a colour literal.** `ModePalette` owns the language.
- **Do not mint a decision id.**

## Committing, pushing, reporting

Commit and push each task before starting the next; name the branch; a refused
push is reported as refused, with the reason.

Report per `CLAUDE_CODE.md` §8 to `output.md` at the repository root, overwritten
and printed. **Read the file's own section count and follow it.**

**The section that says what the owner should expect leads with this: on the
Digital tab, the waterfall now draws the radio's own audio with the FT8 slot grid
across it, and the decoded text panel shows what the decoder actually read.**

**The section that reports measurements leads with the answer to the question this
unit was commissioned to ask: does `ft8_lib` build and decode through P/Invoke on
this machine, yes or no, and what did it cost.**

**If you finish every task, stop and report. Do not start the next unit.**
