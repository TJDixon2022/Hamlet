# RUN_LEDGER.md

Append-only. One line per unit. **No line is ever rewritten** - a run
that went wrong gets a second line saying so, not a corrected first one.
Written by `tools\arbiter\ledger.bat`. GROKBOT.md section 3: this is what
the owner reads instead of watching.

| Unit | Started | Ended | Exit | Cost | What section 3 led with |
|---|---|---|---|---|---|
| 1 | 2026-08-31T15:13 | 2026-08-31T15:51 | killed | unknown | killed by the watchdog: no status write within 12 min of the launch clock |
| phase | 2026-08-31T15:51 | 2026-08-31T15:51 | halted | 0 | the run could not take the session lock |
| 1 | 2026-08-31T16:48 | 2026-08-31T17:07 | complete | 7.78417 | ran unattended, 109 turns, 18 denied call(s) worked around, report valid |
| phase | 2026-08-31T17:08 | 2026-08-31T17:08 | halted | 0 | stop 3: a ruling is wanted - judged, not counted |
| 1 | 2026-08-31T18:27 | 2026-08-31T18:46 | complete | 7.888783000000001 | ran unattended, 88 turns, 6 denied call(s) worked around, report valid |
| 2 | 2026-08-31T18:54 | 2026-08-31T19:13 | complete | 7.1544835 | ran unattended, 91 turns, 12 denied call(s) worked around, report valid |
| phase | 2026-08-31T19:14 | 2026-08-31T19:14 | halted | 15.0433 | stop 10: no progress in two consecutive units |
| 1 | 2026-08-31T19:40 | 2026-08-31T21:34 | complete | 5.859767999999999 | ran unattended, 120 turns, 12 denied call(s) worked around, report valid |
| 1 | 2026-09-01T08:11 | 2026-09-01T09:22 | complete | 5.7809905 | ran unattended, 101 turns, 6 denied call(s) worked around, report valid |
| 1 | 2026-09-01T10:38 | 2026-09-01T11:08 | failed | 14.175986499999999 | run-unit exit 4: 7 denied call(s), is_error=False, terminal=completed |
| 1 | 2026-09-01T11:22 | 2026-09-01T11:58 | complete | 20.2745845 | ran unattended, 153 turns, 5 denied call(s) worked around, report valid |
| 1 | 2026-09-01T13:19 | 2026-09-01T14:06 | failed | 26.849704999999993 | run-unit exit 4: 7 denied call(s), is_error=False, terminal=completed |
| 1 | 2026-09-01T14:21 | 2026-09-01T14:46 | failed | 12.318004499999994 | run-unit exit 4: 9 denied call(s), is_error=False, terminal=completed |
| 1 | 2026-09-01T16:13 | 2026-09-01T16:37 | failed | 11.122426499999998 | run-unit exit 4: 23 denied call(s), is_error=False, terminal=completed |
| 1 | 2026-09-01T17:55 | 2026-09-01T18:20 | failed | 14.4652225 | run-unit exit 4: 6 denied call(s), is_error=False, terminal=completed |
| 1 | 2026-09-01T19:41 | 2026-09-01T20:10 | failed | 16.1859135 | run-unit exit 4: 14 denied call(s), is_error=False, terminal=completed |
| 1 | 2026-09-01T20:57 | 2026-09-01T21:40 | failed | 23.139314500000008 | run-unit exit 4: 10 denied call(s), is_error=False, terminal=completed |
| 1 | 2026-09-01T23:13 | 2026-09-01T23:50 | complete | 23.093651500000004 | ran unattended, 209 turns, 20 denied call(s) worked around, report valid |
| 1 | 2026-09-02T08:40 | 2026-09-02T09:21 | complete | 25.309084000000006 | ran unattended, 195 turns, 15 denied call(s) worked around, report valid |
| 1 | 2026-09-02T09:53 | 2026-09-02T10:42 | complete | 34.8549455 | ran unattended, 232 turns, 15 denied call(s) worked around, report valid |
| 1 | 2026-09-02T11:04 | 2026-09-02T12:07 | complete | 33.250277499999996 | ran unattended, 251 turns, 21 denied call(s) worked around, report valid |
| 2 | 2026-09-02T12:22 | 2026-09-02T12:59 | complete | 21.796497500000005 | ran unattended, 200 turns, 11 denied call(s) worked around, report valid |
| phase | 2026-09-02T13:00 | 2026-09-02T13:00 | halted | 55.0468 | stop 10: no progress in two consecutive units |
| 1 | 2026-09-02T13:21 | 2026-09-02T13:54 | complete | 19.396669499999998 | ran unattended, 165 turns, 11 denied call(s) worked around, report valid |
| 1 | 2026-09-02T14:10 | 2026-09-02T14:30 | complete | 9.470564999999999 | ran unattended, 139 turns, 17 denied call(s) worked around, report valid |
| 1 | 2026-09-02T14:38 | 2026-09-02T15:46 | complete | 20.84393249999999 | ran unattended, 188 turns, 24 denied call(s) worked around, report valid |
| 2 | 2026-09-02T15:56 | 2026-09-02T16:44 | complete | 23.08400750000001 | ran unattended, 183 turns, 23 denied call(s) worked around, report valid |
| 3 | 2026-09-02T16:55 | 2026-09-02T17:45 | complete | 24.927057 | ran unattended, 171 turns, 13 denied call(s) worked around, report valid |
| phase | 2026-09-02T17:47 | 2026-09-02T17:47 | halted | 43.9279 | stop 3: a ruling is wanted - judged, not counted |
| 1 | 2026-09-02T18:13 | 2026-09-02T19:05 | complete | 24.656555999999984 | ran unattended, 244 turns, 22 denied call(s) worked around, report valid |
| 2 | 2026-09-02T19:13 | 2026-09-02T19:41 | complete | 17.553821499999994 | ran unattended, 155 turns, 15 denied call(s) worked around, report valid |
| 3 | 2026-09-02T19:50 | 2026-09-02T20:41 | complete | 22.329669 | ran unattended, 219 turns, 17 denied call(s) worked around, report valid |
| 4 | 2026-09-02T20:55 | 2026-09-02T21:43 | complete | 14.170606000000003 | ran unattended, 141 turns, 17 denied call(s) worked around, report valid |
| phase | 2026-09-02T21:44 | 2026-09-02T21:44 | halted | 64.5401 | stop 3: a ruling is wanted - judged, not counted |
| 1 | 2026-09-02T22:34 | 2026-09-02T23:55 | complete | 24.5806055 | ran unattended, 229 turns, 16 denied call(s) worked around, report valid |
| 2 | 2026-09-03T00:04 | 2026-09-03T00:51 | killed | unknown | killed by the watchdog: no status write within 25 min of the launch clock |
| phase | 2026-09-03T00:52 | 2026-09-03T00:52 | halted | 24.5806 | the run could not take the session lock |
| 1 | 2026-09-03T08:10 | 2026-09-03T08:57 | complete | 14.241551499999991 | ran unattended, 205 turns, 20 denied call(s) worked around, report valid |
| phase | 2026-09-03T09:03 | 2026-09-03T09:03 | halted | 14.2416 | stop 4: the arbiter declared a decision the owner's |
| 1 | 2026-09-03T11:48 | 2026-09-03T12:39 | complete | 28.663566500000023 | ran unattended, 252 turns, 22 denied call(s) worked around, report valid |
| 2 | 2026-09-03T12:49 | 2026-09-03T13:26 | complete | 10.705526500000003 | ran unattended, 142 turns, 12 denied call(s) worked around, report valid |
| 3 | 2026-09-03T13:37 | 2026-09-03T14:24 | complete | 17.785245 | ran unattended, 213 turns, 28 denied call(s) worked around, report valid |
| 4 | 2026-09-03T14:36 | 2026-09-03T15:25 | complete | 22.773395999999998 | ran unattended, 247 turns, 10 denied call(s) worked around, report valid |
| 5 | 2026-09-03T15:34 | 2026-09-03T16:41 | complete | 25.283956999999994 | ran unattended, 250 turns, 17 denied call(s) worked around, report valid |
| phase | 2026-09-03T16:42 | 2026-09-03T16:42 | halted | 105.2117 | stop 10: no progress in four consecutive units |
| 1 | 2026-09-04T19:23 | 2026-09-04T19:31 | complete | 3.6478070000000002 | ran unattended, 78 turns, 25 denied call(s) worked around, report valid |
| phase | 2026-09-04T19:33 | 2026-09-04T19:33 | halted | 0 | stop 3: a ruling is wanted - judged, not counted |
| 1 | 2026-09-04T20:49 | 2026-09-04T21:11 | complete | 13.183756000000011 | ran unattended, 168 turns, 16 denied call(s) worked around, report valid |
| 2 | 2026-09-04T21:17 | 2026-09-04T21:46 | complete | 15.484151499999996 | ran unattended, 157 turns, 5 denied call(s) worked around, report valid |
| 3 | 2026-09-04T21:53 | 2026-09-04T22:25 | complete | 18.422151 | ran unattended, 192 turns, 11 denied call(s) worked around, report valid |
| phase | 2026-09-04T22:26 | 2026-09-04T22:26 | halted | 47.0902 | stop 2: budget exhausted - spent 47.0902 of 40.00 |
| 1 | 2026-09-04T23:31 | 2026-09-05T00:15 | complete | 22.1820815 | ran unattended, 181 turns, 6 denied call(s) worked around, report valid |
| 2 | 2026-09-05T00:25 | 2026-09-05T01:21 | complete | 30.093965499999992 | ran unattended, 211 turns, 9 denied call(s) worked around, report valid |
| 3 | 2026-09-05T01:32 | 2026-09-05T02:48 | killed | unknown | killed by the watchdog: no status write within 12 min of the launch clock |
| phase | 2026-09-05T02:48 | 2026-09-05T02:48 | halted | 52.2761 | the run could not take the session lock |
| 1 | 2026-09-05T12:02 | 2026-09-05T12:35 | killed | unknown | killed by the watchdog: no status write within 12 min of the launch clock |
| phase | 2026-09-05T12:36 | 2026-09-05T12:36 | halted | 0 | the run could not take the session lock |
