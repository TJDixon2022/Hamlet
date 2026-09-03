PROTOCOL: 2
PROJECT: Hamlet
STATE: WORKING
TASK: 4 of 7
WORK_INSTRUCTION: 236
BALL: claude
NEXT_PASTE: none
RULES_AT: HM-DEC-152 (2026-08-31)
UPDATED: 2026-09-03T14:53:24-04:00
NOTE: All five device branches are named, asserted and in the record, and not one of them carries a device. AudioDeviceChoiceReason - NothingToChooseFrom, OperatorsRemembered, LooksLikeRadio, SystemDefault, FirstInTheList - with ChooseWithReason beside the existing Choose, which is now that method with the reason dropped so there is one copy of the order rather than two that could drift. Measured branch table, printed before assertion: no devices -> NothingToChooseFrom; he chose the webcam last time -> OperatorsRemembered; his device is gone and the codec is here -> LooksLikeRadio; nothing remembered and the radio is plugged in -> LooksLikeRadio; NO RADIO IN THE LIST AND NOTHING REMEMBERED -> SystemDefault, which is the branch that matters and the shape of a morning listening to a laptop microphone; no radio, no default, nothing remembered -> FirstInTheList. DecoderStarted was EXTENDED rather than given an event of its own, with three optional parameters so every existing caller keeps compiling - one fact about one moment, and a separate line could be present when this one is missing. The written line: {"simulated":false,"sampleRate":48000,"pitchHz":600,"deviceChoice":"SystemDefault","looksLikeRadio":false,"captureDevicesOffered":3}, and the training radio writes all three as null rather than guessing. 19 of 19 green on the engine side, 8 of 8 on the app side including CallsignPrivacyTests still 4 of 4 with all five branches now walked. Next: task 5, BENCH_CHECK.md.

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
