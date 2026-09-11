# PSK31 transcript corpus - for step 3, read the conversation

`corpus.json`: eight transcripts, 32 lines, written 2026-09-11 by the web thread as
the fixture step 3 is proved against. **They are written, not recorded.** Each carries
the lines as they would arrive on one channel and, per line, what a correct parser
must conclude: who sent it, who it is for, what kind of line it is, whether it hands
the turn over, and **whether the parser may be certain**.

What they exercise: the textbook exchange; a ragchew with the report buried in prose
and `KN` as the turnover; an exchange with no report at all; a full QSO between two
other stations; garbled callsigns and a damaged RST that must come out as unknown; a
line with no callsign; `CQ DX` with a qualifier the parser reports and does not judge;
an exchange that never closes; a portable suffix, lowercase, and `5NN`.

`unknown_rate_expected` per transcript is the fraction of lines a correct parser is
expected to mark uncertain - 0.2 on the garbled one, 0 elsewhere. The parser's actual
rate is a number to report; a higher rate is a finding, not a failure, and a lower one
on the garbled transcript is a parser asserting what the text does not support.

Name and QTH appear in the lines and are never in the expected fields, because §R3
says they are for reading, not for state.
