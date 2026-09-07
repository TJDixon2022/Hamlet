PROTOCOL: 2
PROJECT: Hamlet
STATE: RUNNING
TASK: 1 of 5
WORK_INSTRUCTION: 270 - after his last transmission, the screen still says where the contact stands
BALL: claude
NEXT_PASTE: output.md -> Claude Web
RULES_AT: HM-DEC-158 (2026-09-07)
UPDATED: 2026-09-07T12:19:30-04:00
NOTE: PROJECT GATE PASSED - SHACK_FACTS.md and Cw/CwProbabilisticDecoder.cs both present, CoreHMI.sln and MURC.sln both absent. Reading the tree for the six trace questions before a line is written: PlaceRow confirmed at MainWindowViewModel.cs:7826 with the contact assignment at :7840 and exactly two callers (:7787 the decoder's door, :7872 AddDecodeRowForTests); ContactTextFor at :7901; the one RecordSent call site at :8474; the seam unit 269 used at :8489-8490. Ft8MessageFields is (To, From, Payload) and the app already forwards to Ft8MessageSplit from Ft8Vocabulary.cs:57 and DecodedFilter.cs:86, so question 3 is looking like the reachable route rather than the fallback - to be written up with quotations before task 2 starts. Root version reads 1.12.123 and will go to 1.12.124.

---

Written by a Claude Code session per CLAUDE.md 13 and ANNUNCIATOR.md.

PROTOCOL names which protocol this header is written against. The long form,
STATUS_PROTOCOL.md, lives in the annunciator repository and is not in this
one, so nothing here can check conformance to it -- the field says what the
file was written to, not that anybody validated it.
