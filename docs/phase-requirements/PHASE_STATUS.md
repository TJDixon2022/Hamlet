PHASE: Hamlet meets the CW requirements
PHASE_SET: 2026-09-25
DESCRIPTION: CW_REQUIREMENTS.md and CW_SPEC.md at the repository root are the specification from here on. Sixty-eight requirements, sixty-five of them must-tier, and section T shows fourteen groups with no test at all. This phase traces what exists, builds the metrics the requirements are written in - invented characters, sure-character error, coverage, word-boundary error, acquisition time - and then meets the requirements group by group, highest tier first. Every unit recenters on those two documents. Judged by requirement ids and, at the end, by Tim at the radio.
CURRENT_STEP: 9
WORK_INSTRUCTION: 456 - fldigi's CW receiver, ported as the second decoder
STEP: 0 | done | Every CW test is traced - section T names, for each requirement, the test that proves it or the word none, and every existing CW test names the requirement it proves or is marked as proving none.
STEP: 1 | done | The metrics exist - MET-INVENTED, MET-CER-SURE, MET-COVERAGE and MET-WBE are computed over the corpus and reported per condition, so the requirements can be measured at all.
STEP: 2 | not started | The decoder stops printing wrong letters with confidence - MET-CER-SURE driven toward HM-REQ-010's one in a hundred, from the 54 sure-but-wrong characters unit 439 measured.
STEP: 3 | not started | The junk between words goes - MET-INVENTED driven toward HM-REQ-011's zero, from the 13 added characters unit 439 measured.
STEP: 4 | not started | The pitch is right - section J, and the acquisition requirements of section K that depend on it.
STEP: 5 | not started | The speed is right - section D, five to forty-five words a minute, acquired cold and tracked through a change.
STEP: 6 | not started | The text is right - sections H and I, the character table, the prosigns and the word boundaries.
STEP: 7 | not started | The conditions can be generated - sections E, F and G: channel, sender and interference profiles, which nothing in the tree produces today.
STEP: 8 | not started | The record and the tests are put right - last, not first: the decision log, the traceability table, the 43 tests that measure something other than what their requirement states, and the 63 requirements with no test.
