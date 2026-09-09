PROTOCOL: 2
PROJECT: Hamlet
STATE: COMPLETED
TASK: 8 of 8
WORK_INSTRUCTION: 296 - FT4 decodes where a real station lands, not where Hamlet put it
BALL: tim
NEXT_PASTE: output.md -> Claude Web
RULES_AT: HM-DEC-160 (2026-09-08)
UPDATED: 2026-09-09T15:25:58-04:00
NOTE: 8 of 8, nothing dropped including the named drop candidate. Step 1 goes partial to done and its two must-pass criteria are met off grid for the first time: 106 of 106 at drawn placements, 0 wrong anywhere in 10800 sweep slots, both round trips and the ladder's 3200. The number: at -13 dB the placement-averaged rate goes 808 of 900 to 894 of 900 and the worst cell 19 of 36 to 34 of 36, at 66.3 to 151.3 ms a slot, and the deficit the tree guessed at two to three decibels measured 2.4. One file under src/Ft8Sharp changed and FT8's own defaults are asserted unmoved extent by extent. The sweep found that frequency oversampling above 2 destroys the decode and destroys the on-grid one first, because Ft8Monitor's frame is BlockSize times FrequencyOversampling long and at 4 it spans four FT4 symbols; the fix is the time axis. Step 2's criterion 4 lost its last fifteen-second sentence. One item wants a ruling - Ft4DeepSignalToNoise's candidate time bias is stale by one sub-block and its search absorbs the error at exactly its own limit, so Ft4Unit294SnrAgreementTests is left red naming it rather than weakened to green. Four items back, one wanting a ruling, none in the way of a criterion. validate-output exit 0, all seven rules. This UPDATED is read from the clock; earlier ones in this session were composed and ran ahead of it, which is recorded in the report.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.
