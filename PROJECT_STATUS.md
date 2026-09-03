PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 5 of 7
WORK_INSTRUCTION: 233
BALL: claude
NEXT_PASTE: none
RULES_AT: HM-DEC-152 (2026-08-31)
UPDATED: 2026-09-03T12:52:00-04:00
NOTE: Task 5 built and green, 13 of 13 across the two sheet classes with the pre-existing sheet test untouched and still passing. The sheet gained three blocks. AUDIO PATH: device name, the rate the device reported beside the rate the file was written at, channel count, encoding and bit depth, whether the samples were real or synthesized, and the Windows capture gain and mute from CaptureHealth - every row measured or "unknown (not read)" and nothing defaulted. SLOT GEOMETRY: every 15-second boundary inside the window at its corrected UTC, each marked whole-transmission-inside or CUT SHORT, with wholeSlots in its own field saying in capitals when no whole transmission is in the audio at all - because nothing decoded and nothing decodable was captured are different statements. CENSUS: one row per slot with the five counts, the rate, the top Costas match counts, and the refusal verbatim. ShowDecodes now returns the Ft8Reception so Compose gets the census the press already produced rather than decoding twice. WasapiAudioSource gained ChannelCount and Encoding, deliberately not on IAudioSource because the training radio has neither. One bug caught by its own test: the key column pads to 11 and audioIsReal is exactly 11, so it ran into its own value - padding is now at least key length plus one, which changes no row written before this unit. Next: task 6.

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
