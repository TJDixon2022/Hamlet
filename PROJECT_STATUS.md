PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 2 of 7
WORK_INSTRUCTION: 237
BALL: claude
NEXT_PASTE: none
RULES_AT: HM-DEC-152 (2026-08-31)
UPDATED: 2026-09-03T15:43:57-04:00
NOTE: THE ARBITER'S READING IS CONFIRMED AND IT IS A REAL DEFECT. 17 tests over the conversion, 14 pass, 3 fail, and the failing rows are the point of the night: extensible-float32 mono put 0.999023438 in and got 0.496086100 out, stereo put 0.1875 in and got -0.01171875 out. Every PCM row and the plain IeeeFloat row is exact. Extensible-pcm16 and extensible-pcm32 pass too - correct by luck, because the switch on BitsPerSample happens to match. Two supporting measurements, both hardware-free: NAudio 2.2.1's WaveFormat.MarshalFromPtr hands back a WaveFormatExtensible with Encoding still Extensible when the OS declares WAVE_FORMAT_EXTENSIBLE, and WasapiCapture's constructors call MMDevice.get_AudioClient and AudioClient.get_MixFormat - read out of the constructor's IL, not from memory. So the format that reaches ReadSample is the device's own mix format, tag intact. The fault is WasapiAudioSource.cs:332, which asks only the top-level tag.

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
