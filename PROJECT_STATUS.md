PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 7 of 7
WORK_INSTRUCTION: 237
BALL: claude
NEXT_PASTE: none
RULES_AT: HM-DEC-152 (2026-08-31)
UPDATED: 2026-09-03T16:17:50-04:00
NOTE: Root moved 1.12.40 to 1.12.41 under HM-DEC-150 with an entry in the props log, Ft8Sharp unmoved at 0.10.7 because no file under src/Ft8Sharp/ changed. Gates off TRX Counters, one at a time, never two at once: Ft8Sharp 523 passed 0 failed 1 skipped in 5m19s, failing set EMPTY; the RadioEngine Audio set 117 of 117 in 59s, failing set EMPTY; Hamlet.App channel 9 of 9 including VersionTests at the new version, failing set EMPTY - that is the ONE Hamlet.App.Tests invocation of the whole unit. the RadioEngine channel set 38 of 38 in 13m43s against the 7m38s its own record predicts, failing set EMPTY. All five green. Attribution 254 paths, 41 under Hamlet, five of them this unit's, reduction explicitly NOT claimed. Attribution already read: 254 paths since 2828ab6, 41 of them under Hamlet, five of those this unit's. BENCH_CHECK.md corrected in three places - a new subsection on what the sound card said it was speaking with the encoding row as a lookup table, plus two existing bullets that were wrong in a way that would have cost him the morning: a null-level slot now has a fourth meaning, and "the problem is downstream of the sound card" was false before 1.12.41. Every line tagged measured or inherited, and the subsection says plainly that this is very probably NOT what happened to him. TASK 5 TAKEN, NOT DROPPED, AND IT CHANGES THE CONCLUSION. This machine has 2 active capture endpoints and BOTH declare IeeeFloat 32-bit, 2 channels, 48000 Hz, with no subformat at all - so Windows here is handing over WAVE_FORMAT_IEEE_FLOAT and not WAVE_FORMAT_EXTENSIBLE. The defect was real and is measured and is repaired, but it would NOT have been triggered by either of these two endpoints, so the silent morning is NOT explained by it. Whether the radio's own USB codec was among the two is unknown: HM-DEC-018 forbids reading the name. THE SENTENCE, WITH REAL NUMBERS: Goba's three busiest recordings gave 47 rows through the float path, 24 through an Extensible-float device buffer BEFORE the fix and all 47 AFTER, at 48000 and at 44100 alike. So the failure was never silence - half the band came through, scrambled, which is worth saying because it does NOT on its own explain a table with nothing in it. Every other format gives 47 either way; 8-bit PCM gives 46 and no recording matches exactly, reported and not gated. FIXED AND WATCHED FAILING FIRST - 3 failed then 17 of 17 pass. THE ARBITER'S READING IS CONFIRMED AND IT IS A REAL DEFECT. 17 tests over the conversion, 14 pass, 3 fail, and the failing rows are the point of the night: extensible-float32 mono put 0.999023438 in and got 0.496086100 out, stereo put 0.1875 in and got -0.01171875 out. Every PCM row and the plain IeeeFloat row is exact. Extensible-pcm16 and extensible-pcm32 pass too - correct by luck, because the switch on BitsPerSample happens to match. Two supporting measurements, both hardware-free: NAudio 2.2.1's WaveFormat.MarshalFromPtr hands back a WaveFormatExtensible with Encoding still Extensible when the OS declares WAVE_FORMAT_EXTENSIBLE, and WasapiCapture's constructors call MMDevice.get_AudioClient and AudioClient.get_MixFormat - read out of the constructor's IL, not from memory. So the format that reaches ReadSample is the device's own mix format, tag intact. The fault is WasapiAudioSource.cs:332, which asks only the top-level tag.

---

## What this file is

Volatile state, overwritten whole at each write. `PROTOCOL: 2` names
`STATUS_PROTOCOL.md`, which is not in this repository - the header is read as
which protocol this is written against, not as conformance anything here can
check.

Branch, commit and working-tree state are never reported here. The panel reads
those from `.git` itself.

Prose below the `---` is free. Nothing reads it, and no key of this format's own
may appear below the rule.
