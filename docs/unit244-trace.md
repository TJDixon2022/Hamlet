# Unit 244, task 1 — what a capture fixture has to fit into

**Reading only.** Every claim below was read out of this tree tonight and carries
its file and line. Where the work instruction's description and the tree disagree,
the tree is what is written here.

---

## 1. What a decode looks like coming out of Hamlet today

Two types carry it, and the harness only ever touches one of them.

### `Ft8SlotResult` — `src/Ft8Sharp/Dsp/Ft8SlotDecoder.cs:270`

A `readonly record struct` with six members:

| field | line | what it is |
|---|---|---|
| `CandidateCount` | `:271` | places the sync search returned |
| `ParitySatisfiedCount` | `:272` | of those, how many reached a valid codeword |
| `ChecksumPassedCount` | `:273` | of those, how many carried their own checksum |
| `BecameTextCount` | `:274` | of those, how many became words |
| `DuplicateCount` | `:275` | of those, how many repeated a message already returned |
| `Messages` | `:276` | `IReadOnlyList<Ft8SlotMessage>`, unique, in first-decode order |

and one computed member, `Texts` at `:279`, which projects `Messages` down to the
message strings and **throws everything else away**.

### `Ft8SlotMessage` — `src/Ft8Sharp/Dsp/Ft8SlotDecoder.cs:242`

`Ft8SlotMessage(Ft8Candidate Candidate, Ft8CodewordResult Result)`, with three
accessors:

- `Text` — `:245`, the message
- `FrequencyHz(Ft8WaterfallGeometry geometry)` — `:248`, the lowest tone's frequency
- `TimeSeconds(Ft8WaterfallGeometry geometry)` — `:251`, when the first symbol began,
  in seconds from the start of the slot

### The four fields the plan wants, one at a time

`PHASE_PLAN.md:230` wants a fixture row of **message, frequency, dt and SNR**.

| field | available per message today? |
|---|---|
| **message** | **Yes.** `Ft8SlotMessage.Text`, `Ft8SlotDecoder.cs:245`. |
| **frequency** | **Yes**, but only from the message, not from the result's `Texts`. `Ft8SlotMessage.FrequencyHz(geometry)`, `:248`, computed off `Ft8Candidate.FrequencyHz`, `src/Ft8Sharp/Dsp/Ft8Candidate.cs:93`. It needs the `Ft8WaterfallGeometry` passed back in; the message does not carry it. |
| **dt** | **Yes**, on the same terms. `Ft8SlotMessage.TimeSeconds(geometry)`, `:251`, off `Ft8Candidate.TimeSeconds`, `Ft8Candidate.cs:103`. Resolution is the search's sub-block grid, not a continuous estimate. |
| **SNR** | **NO. Hamlet cannot produce a per-message SNR today, and this is a finding.** |

**On the SNR, plainly.** Nothing on `Ft8SlotMessage`, `Ft8SlotResult`,
`Ft8Candidate` or `Ft8CodewordResult` is a signal-to-noise ratio in decibels. The
nearest thing is `Ft8Candidate.Score` (`Ft8Candidate.cs:49`), which the type's own
remarks at `:37` describe as *small integers over tens of thousands of hypotheses*
— a sync correlation score, not a ratio, on no calibrated scale and in no unit.

The only decibel figure this tree produces about FT8 audio is
`SignalToNoise.DecibelsFor`, used at `Ft8LadderHarness.cs:265`, and it is **a
property of a slot the test synthesised, computed from the signal power and the
noise power the test itself mixed in.** It is the ladder's *delivered* ratio for
the whole slot. It is not measured from a received signal and it is not per
message: on a slot with two transmissions in it there is one number, and on a
real capture there is no number at all.

**Consequence for this unit.** The fixture format carries an SNR column because
WSJT-X emits one and the plan names it, and the reader parses it, but **Hamlet has
nothing to put in that column on the returning side.** `PHASE_PLAN.md` step 5 is
the step that owes it, and the work instruction says so too. Task 4's scoring is
therefore **message-only matching** — see section 2 — and the SNR column is
recorded, compared by nothing, and waiting for step 5.

---

## 2. How `Ft8LadderHarness` compares a returned message with an expected one

### The harness's own comparison — `Ft8LadderHarness.cs:274`

```csharp
var decoded = result.Texts.Contains(sent, StringComparer.Ordinal);
var wrong = result.Texts
    .Where(t => !string.Equals(t, sent, StringComparison.Ordinal))
    .ToArray();
```

`sent` comes from `Ft8MessageDecoder.Decode(entry.Message).Text` at `:266` — the
library's own round trip of the bits that were transmitted.

**So the harness applies no normalisation at all.** Both sides of its comparison
are produced by the same library from the same message type, so both are already
in the library's own spelling and an ordinal equality is exactly right. There is
nothing to normalise away.

### Where normalisation actually lives — `ReferenceRecordings.cs:100`

`ReferenceRecording.Normalise(string)` is a `public static` on the record at
`tests/Ft8Sharp.Tests/Dsp/ReferenceRecordings.cs:100`, and its rule is documented
at `:48–69`:

1. The message is everything after the first `~`, which is where upstream's own
   print format — `"%02d%02d%02d %+05.1f %+4.2f %4.0f ~  %s\n"`, quoted at `:54` —
   puts it. *(That split is done by the caller, `ExpectedMessages` at `:86`;
   `Normalise` itself takes the text after the tilde.)*
2. Leading and trailing whitespace is removed.
3. **Where the remainder contains a run of two or more spaces, the text is what
   lies to the left of it** — some of those lists carry a trailing country
   annotation that upstream's `printf` does not emit. An FT8 message is
   single-space separated, so a run of two is an unambiguous boundary.

And `:64–67` states the limit in writing: **nothing else is stripped** — no
brackets, no case folding, and `RR73` and `RRR` stay different messages.

**The ruling this unit obeys.** `Normalise` is the normalisation, it is already
`public static`, and the fixture reader **calls it rather than re-implementing
it.** It is the right function for exactly the reason the work instruction gives:
the fixture reader is reading a decoder's printed output, which is the same job
`ExpectedMessages` does for upstream's lists, and a second copy of a
normalisation rule drifts silently.

---

## 3. The CW capture fixture precedent — `tests/fixtures/cw/captured/`

**Shape.** A `.wav` and a `.txt` of the same stem, side by side. Four adjudicated
pairs at `tests/fixtures/cw/captured/`, and 36 more under
`tests/fixtures/cw/captured/unadjudicated/` with a `MANIFEST.md` beside them.

**What the sidecar holds.** Reading `cw-2026-08-17-013347.txt`: whitespace-aligned
`key value` lines in blocks separated by blank lines, no header, no version, no
delimiter between the blocks. The first block is the capture's own identity:

```
captured   2026-08-17 01:33:47 UTC
audioSeen  19130400 samples
fingerprint 6c24bba8f351
seconds    30.0
sampleRate 48000
frequency  7030000 Hz
band       40 m
```

then a measurement block (`inputPeak`, `snrDb`, `elements`, `characters`), then a
verbatim dump of the rig's own state (`Frequency`, `Mode`, `FilterBandwidth`,
`SMeter`, …). **It is a state snapshot, not a truth list.** The decoded text is one
of its fields, not a set of rows, and there is no per-message structure anywhere in
it.

**Does anything verify the pairing?** **No.** Nine test files reach into
`tests/fixtures/cw/captured/` by composing a path and opening it — for instance
`tests/Hamlet.RadioEngine.Tests/Audio/TheWaterfallPictureIsLegibleTests.cs:52`,
which builds `relative + ".wav"`, and
`tests/Hamlet.App.Tests/ViewModels/CaseRosterSurvivesAnEveningTests.cs:82`. Every
one names the file it wants. **Nothing walks the folder asserting that each `.wav`
has a `.txt` or the reverse**, and nothing checks that the `.txt` is about the
`.wav` next to it.

**Is any hash of the audio recorded?** **Only as a 12-character fingerprint, and it
is not verified against anything.** The `fingerprint` line comes from
`MainWindowViewModel.Fingerprint` at
`src/Hamlet.App/ViewModels/MainWindowViewModel.cs:5763–5770`: `SHA256.HashData` over
the raw float samples, `Convert.ToHexString`, **`[..12]`**, lower-cased —
HM-DEC-090, *so two identical captures are visibly identical*. Three things follow.
It is **truncated to 48 bits**; it is over the **float sample array in memory**, not
the file's bytes, so it cannot be recomputed from the committed `.wav` without
reproducing the app's own decode of the header; and **no test recomputes it.** It
is a label a human can eyeball, and it was never built to be a check.

**Worth following, or worth diverging from?**

- **Follow the placement.** Sidecar of the same stem, beside the audio, committed
  together. It has held for forty units and it is the thing that makes a capture
  and its truth impossible to separate by accident.
- **Diverge on everything else, and there are three reasons.** (a) CW's sidecar has
  **no rows**; the plan's fixture is a *list of messages*, and a key-value snapshot
  has nowhere to put one. (b) CW's hash is truncated, over in-memory floats and
  unchecked; the plan's must-pass exit is that **a hash mismatch fails loudly**,
  which requires a full SHA-256 over the **file's bytes**, recomputable by anyone
  holding the `.wav`. (c) CW's sidecar has **no provenance and no version**, so
  nothing in it distinguishes a real capture's truth from a synthetic one — which
  is the single distinction this unit's format exists to make.

---

## 4. Does anything in this tree already hash a file?

**There is no shared helper. There are two private implementations and neither is
reachable from `Ft8Sharp.Tests`.** Search over `*.cs`, `*.py`, `*.bat`, `*.ps1`,
excluding `bin/` and `obj/`, for `SHA256`/`Sha256`/`sha256` returns exactly:

| where | what it hashes | usable here? |
|---|---|---|
| `src/Hamlet.App/ViewModels/MainWindowViewModel.cs:5769` | float samples in memory, truncated to 12 hex chars | No — `private static`, in the app, and truncated |
| `tests/Hamlet.RadioEngine.Tests/Audio/Unit233ScratchTraceTests.cs:571` | a file's bytes, full digest | No — `private static` in a different test project |
| `tests/Hamlet.RadioEngine.Tests/Audio/Unit233ScratchTraceTests.cs:365` | ditto, inline | No, same |
| `tests/Ft8Sharp.Tests/Encode/PeImage.cs:260` | a PE section body, unrelated | No |

**So: no helper. This unit writes one**, in the fixture code, over the file's raw
bytes, full 64-character lower-case hex. That is a new function rather than a
duplicated one — nothing existed to call.

---

## 5. `Available()` in `Ft8LadderHarness`

**Signature** — `tests/Ft8Sharp.Tests/Dsp/Ft8LadderHarness.cs:182`:

```csharp
internal static IReadOnlyList<Decoder> Available()
```

**Body** — `:184–185`: constructs one `Ft8SlotDecoder` and returns a single-element
array, `new Decoder("Ft8Sharp", samples => port.Decode(samples))`.

**What a `Decoder` entry is** — `:72`:

```csharp
internal sealed record Decoder(string Name, Func<float[], Ft8SlotResult> Decode);
```

Two members. `Name` is what the report prints. `Decode` takes `float[]` and returns
`Ft8SlotResult` — **samples in, result out, and nothing else is told to it**
(`:71`). The remarks at `:175–181` state that adding `Ft8Sharp.Deep` is one entry in
this method and one project reference, and that **nothing else in the file knows how
many decoders there are.**

**What that means for task 4.** A fixture scorer that iterates `Available()` and
emits one row per entry gains its second column when step 1 adds its entry, with no
change at the call site. The one thing it must not do is index `[0]` or assume a
count.

---

## 6. Can `tests/Ft8Sharp.Tests` read a committed WAV at all today?

**Yes, and the reader already exists** — `WavFile`, at
`tests/Ft8Sharp.Tests/Encode/WavFile.cs:35`, `internal static`, namespace
`Ft8Sharp.Tests.Encode`.

- `WavFile.Read(string path)` → `Contents(SampleRate, BitsPerSample, ChannelCount, HeaderBytes, short[] Samples)`, `:52` and `:45`.
- `WavFile.Write(string path, ReadOnlySpan<float> samples, int sampleRate)`, `:203`, writes the canonical 44-byte mono 16-bit header — which is how this unit can produce a committed example capture without inventing a writer.
- `WavFile.Quantise(float)`, `:243`, is upstream's own clamp-and-half-before-truncate.

**What it refuses** (`Parse`, `:63` onward), each with a named exception: shorter
than 44 bytes `:65`; not `RIFF`/`WAVE`/`fmt ` `:71–73`; a `fmt` chunk that is not
16 bytes `:77`; audio format other than 1 `:85`; **channels other than 1** `:93`;
**bits per sample other than 16** `:103`; no `data` chunk anywhere `:146`; a `data`
chunk longer than the bytes that follow it `:155`.

**Two consequences worth stating.**

1. **Sample rate is read, never checked** (`:101`). `WavFile` will happily hand back
   a 48 kHz file. The FT8 decode path is built on
   `Ft8WaterfallGeometry.DefaultSampleRate`, which `Ft8LadderHarness.cs:64` binds to
   `Rate`, so **the fixture format has to state the sample rate and the scorer has
   to refuse a capture that is not at the rate the decoder expects.** Nothing in
   `WavFile` will catch that for it.
2. **Every committed WAV in this repository today is CW's, 48 kHz, and none is
   FT8's.** `git ls-files "*.wav"` returns **80 files**, all under
   `tests/fixtures/cw/`, and **0** matching `ft8`. Per `SHACK_FACTS.md` FACT-004
   that is the expected state on this machine and is not a finding.

`ReferenceRecording.ReadSamples` at `ReferenceRecordings.cs:121` shows the scaling
the decoder wants: one float per sixteen-bit count, **divided by 32768.0f**,
described at `:117` as upstream's own `load_wav` scaling. A fixture scorer reads a
committed WAV the same way, or it is analysing a differently-scaled signal.

---

## Mismatches between the work instruction and the tree

Checked one by one. **All of the instruction's claims hold**, with two refinements
and one confirmation:

| claim | finding |
|---|---|
| `Ft8LadderHarness.cs` exists, `Available()` "at about `:182`" | **Holds exactly.** `:182`. |
| `ReferenceRecordings.cs` reads from `C:\Source\ft8_lib` at run time and copies nothing | **Holds.** `:146`, and the ruling is stated at `:8–14`. Untouched by this unit. |
| CW precedent is `tests/fixtures/cw/captured/`, `.wav` + `.txt`, `MANIFEST.md` under `unadjudicated/` | **Holds.** |
| `tools/score-fixtures/score-fixtures.py` exists and is CW's | **Holds.** Its `FIXTURES` constant points at `tests/fixtures/cw/receiver`, and it scores with `cwdecoder.py`. Nothing in it is reusable here. |
| No FT8 capture `.wav` is committed anywhere | **Holds.** 80 committed WAVs, all CW. |
| `HM-OPEN-067` carries the ladder's figures | **Holds** for unit 221's figures — `OPEN_ISSUES.md:223`, 13 of 306 at −21.001 dB. It does **not** yet carry unit 243's reproduction of them; task 6 adds that. |
| Root version `1.12.46` | **Holds.** `Directory.Build.props:145`. |
| `Ft8Sharp` `0.10.7` | **Holds.** `src/Ft8Sharp/Directory.Build.props:396`. |
| `PHASE_PLAN.md:53` still reads *and it reads WSJT-X* | **Still there.** Reported once, per the arbiter's decision, and not touched. |
