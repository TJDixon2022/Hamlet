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
| 1 | 2026-09-05T13:09 | 2026-09-05T13:47 | killed | unknown | killed by the watchdog: no status write within 12 min of the launch clock |
| phase | 2026-09-05T13:47 | 2026-09-05T13:47 | halted | 0 | the run could not take the session lock |
| 1 | 2026-09-05T14:05 | 2026-09-05T14:24 | complete | 10.390586 | ran unattended, 118 turns, 13 denied call(s) worked around, report valid |
| 2 | 2026-09-05T14:34 | 2026-09-05T15:15 | complete | 22.76733250000001 | ran unattended, 193 turns, 9 denied call(s) worked around, report valid |
| 3 | 2026-09-05T15:24 | 2026-09-05T16:04 | complete | 16.2019905 | ran unattended, 146 turns, 9 denied call(s) worked around, report valid |
| 4 | 2026-09-05T16:17 | 2026-09-05T17:12 | complete | 28.354318500000005 | ran unattended, 181 turns, 5 denied call(s) worked around, report valid |
| 5 | 2026-09-05T17:22 | 2026-09-05T18:10 | complete | 23.6036235 | ran unattended, 180 turns, 3 denied call(s) worked around, report valid |
| 6 | 2026-09-05T18:22 | 2026-09-05T19:16 | complete | 14.410884500000005 | ran unattended, 158 turns, 8 denied call(s) worked around, report valid |
| 7 | 2026-09-05T19:27 | 2026-09-05T20:07 | complete | 18.221650000000007 | ran unattended, 136 turns, 3 denied call(s) worked around, report valid |
| 8 | 2026-09-05T20:18 | 2026-09-05T21:02 | complete | 12.961496999999998 | ran unattended, 137 turns, 3 denied call(s) worked around, report valid |
| 9 | 2026-09-05T21:11 | 2026-09-05T21:17 | complete | 2.2865979999999997 | ran unattended, 42 turns, 3 denied call(s) worked around, report valid |
| 10 | 2026-09-05T21:27 | 2026-09-05T22:14 | complete | 48.272085 | ran unattended, 329 turns, 21 denied call(s) worked around, report valid |
| phase | 2026-09-05T22:15 | 2026-09-05T22:15 | halted | 197.4706 | stop 10: no progress in four consecutive units |
| 1 | 2026-09-06T18:22 | 2026-09-06T18:41 | complete | 10.629209499999998 | ran unattended, 162 turns, 13 denied call(s) worked around, report valid |
| 2 | 2026-09-06T18:51 | 2026-09-06T19:20 | complete | 19.011571000000014 | ran unattended, 190 turns, 15 denied call(s) worked around, report valid |
| 3 | 2026-09-06T19:28 | 2026-09-06T19:57 | complete | 18.9084725 | ran unattended, 169 turns, 15 denied call(s) worked around, report valid |
| 4 | 2026-09-06T20:04 | 2026-09-06T20:33 | complete | 15.8225235 | ran unattended, 157 turns, 10 denied call(s) worked around, report valid |
| 5 | 2026-09-06T20:43 | 2026-09-06T20:57 | killed | unknown | killed by the watchdog: no status write within 12 min of the launch clock |
| phase | 2026-09-06T20:58 | 2026-09-06T20:58 | halted | 64.3718 | the run could not take the session lock |
| 1 | 2026-09-06T22:28 | 2026-09-06T22:55 | complete | 14.266779999999997 | ran unattended, 153 turns, 9 denied call(s) worked around, report valid |
| 2 | 2026-09-06T23:03 | 2026-09-06T23:31 | complete | 17.7668375 | ran unattended, 179 turns, 14 denied call(s) worked around, report valid |
| 3 | 2026-09-06T23:40 | 2026-09-07T00:07 | complete | 18.74422600000001 | ran unattended, 203 turns, 5 denied call(s) worked around, report valid |
| 4 | 2026-09-07T00:13 | 2026-09-07T00:39 | complete | 15.008637499999999 | ran unattended, 170 turns, 14 denied call(s) worked around, report valid |
| 5 | 2026-09-07T00:48 | 2026-09-07T01:11 | complete | 13.1356565 | ran unattended, 159 turns, 13 denied call(s) worked around, report valid |
| 6 | 2026-09-07T01:19 | 2026-09-07T01:52 | complete | 20.201800000000006 | ran unattended, 189 turns, 12 denied call(s) worked around, report valid |
| 7 | 2026-09-07T02:01 | 2026-09-07T02:24 | complete | 13.141347000000003 | ran unattended, 165 turns, 8 denied call(s) worked around, report valid |
| 8 | 2026-09-07T02:33 | 2026-09-07T02:58 | failed | 12.819888500000001 | run-unit exit 4: 5 denied call(s), is_error=False, terminal=completed |
| phase | 2026-09-07T02:59 | 2026-09-07T02:59 | halted | 112.2652 | stop 7: validate-output refused the report (after 5 denied calls) |
| 1 | 2026-09-07T09:59 | 2026-09-07T10:23 | complete | 13.577231000000003 | ran unattended, 145 turns, 4 denied call(s) worked around, report valid |
| 2 | 2026-09-07T10:29 | 2026-09-07T10:47 | complete | 12.125338 | ran unattended, 145 turns, 12 denied call(s) worked around, report valid |
| 3 | 2026-09-07T10:54 | 2026-09-07T11:23 | complete | 16.319568000000007 | ran unattended, 170 turns, 6 denied call(s) worked around, report valid |
| 4 | 2026-09-07T11:33 | 2026-09-07T12:05 | complete | 24.625143 | ran unattended, 224 turns, 12 denied call(s) worked around, report valid |
| 5 | 2026-09-07T12:17 | 2026-09-07T12:44 | complete | 15.337653000000003 | ran unattended, 151 turns, 8 denied call(s) worked around, report valid |
| phase | 2026-09-07T12:49 | 2026-09-07T12:49 | halted | 81.9849 | stop 4: the arbiter declared a decision the owner's |
| 1 | 2026-09-07T13:20 | 2026-09-07T13:45 | complete | 16.066289 | ran unattended, 163 turns, 5 denied call(s) worked around, report valid |
| 2 | 2026-09-07T13:53 | 2026-09-07T14:15 | complete | 14.203481999999997 | ran unattended, 151 turns, 4 denied call(s) worked around, report valid |
| phase | 2026-09-07T14:19 | 2026-09-07T14:19 | halted | 30.2698 | stop 4: the arbiter declared a decision the owner's |
| 1 | 2026-09-09T08:34 | 2026-09-09T09:12 | complete | 33.30902650000002 | ran unattended, 219 turns, 25 denied call(s) worked around, report valid |
| 2 | 2026-09-09T09:20 | 2026-09-09T10:00 | complete | 34.79690249999999 | ran unattended, 303 turns, 24 denied call(s) worked around, report valid |
| 3 | 2026-09-09T10:07 | 2026-09-09T10:32 | complete | 17.942532499999995 | ran unattended, 201 turns, 13 denied call(s) worked around, report valid |
| 4 | 2026-09-09T10:40 | 2026-09-09T11:16 | complete | 34.540593 | ran unattended, 271 turns, 15 denied call(s) worked around, report valid |
| 5 | 2026-09-09T11:25 | 2026-09-09T12:10 | complete | 41.09455549999999 | ran unattended, 255 turns, 10 denied call(s) worked around, report valid |
| 6 | 2026-09-09T12:21 | 2026-09-09T13:14 | complete | 38.9557145 | ran unattended, 286 turns, 10 denied call(s) worked around, report valid |
| 7 | 2026-09-09T13:20 | 2026-09-09T13:53 | complete | 18.452825499999992 | ran unattended, 173 turns, 13 denied call(s) worked around, report valid |
| 8 | 2026-09-09T14:07 | 2026-09-09T15:26 | complete | 26.83501999999999 | ran unattended, 225 turns, 5 denied call(s) worked around, report valid |
| phase | 2026-09-09T15:28 | 2026-09-09T15:28 | halted | 245.9271 | stop 10: no progress in four consecutive units |
| 1 | 2026-09-11T11:10 | 2026-09-11T12:05 | complete | 24.949754000000002 | ran unattended, 263 turns, 17 denied call(s) worked around, report valid |
| 2 | 2026-09-11T12:11 | 2026-09-11T12:58 | complete | 19.498643 | ran unattended, 171 turns, 9 denied call(s) worked around, report valid |
| phase | 2026-09-11T13:07 | 2026-09-11T13:07 | halted | 44.4484 | stop 4: the arbiter declared a decision the owner's |
| 1 | 2026-09-11T13:30 | 2026-09-11T14:28 | complete | 22.346655000000002 | ran unattended, 219 turns, 13 denied call(s) worked around, report valid |
| 2 | 2026-09-11T14:36 | 2026-09-11T15:13 | complete | 14.774045999999997 | ran unattended, 162 turns, 6 denied call(s) worked around, report valid |
| phase | 2026-09-11T16:08 | 2026-09-11T16:08 | halted | 50.7568 | the run could not take the session lock |
| 1 | 2026-09-11T17:42 | 2026-09-11T17:55 | killed | unknown | killed by the watchdog: no status write within 12 min of the launch clock |
| phase | 2026-09-11T17:55 | 2026-09-11T17:55 | halted | 0 | the run could not take the session lock |
| 1 | 2026-09-11T18:09 | 2026-09-11T18:58 | complete | 54.71674749999996 | ran unattended, 379 turns, 25 denied call(s) worked around, report valid |
| phase | 2026-09-11T19:00 | 2026-09-11T19:00 | halted | 54.7167 | backstop: 1 iterations, no stop condition fired |
| 1 | 2026-09-11T19:26 | 2026-09-11T20:15 | complete | 47.65027650000001 | ran unattended, 344 turns, 17 denied call(s) worked around, report valid |
| phase | 2026-09-11T20:15 | 2026-09-11T20:15 | halted | 47.6503 | backstop: 1 iterations, no stop condition fired |
| 1 | 2026-09-11T20:56 | 2026-09-11T22:09 | complete | 48.23846000000003 | ran unattended, 385 turns, 24 denied call(s) worked around, report valid |
| 2 | 2026-09-11T22:15 | 2026-09-11T22:52 | complete | 33.364847 | ran unattended, 296 turns, 16 denied call(s) worked around, report valid |
| phase | 2026-09-11T22:53 | 2026-09-11T22:53 | halted | 81.6033 | backstop: 2 iterations, no stop condition fired |
| 1 | 2026-09-11T23:12 | 2026-09-12T00:00 | complete | 48.38649749999994 | ran unattended, 319 turns, 17 denied call(s) worked around, report valid |
| phase | 2026-09-12T00:01 | 2026-09-12T00:01 | halted | 48.3865 | backstop: 1 iterations, no stop condition fired |
| 1 | 2026-09-12T00:14 | 2026-09-12T00:32 | complete | 9.140988 | ran unattended, 122 turns, 9 denied call(s) worked around, report valid |
| phase | 2026-09-12T00:33 | 2026-09-12T00:33 | halted | 9.1410 | backstop: 1 iterations, no stop condition fired |
| 1 | 2026-09-12T10:58 | 2026-09-12T11:41 | killed | unknown | killed by the watchdog: no status write within 12 min of the launch clock |
| phase | 2026-09-12T11:41 | 2026-09-12T11:41 | halted | 0 | the run could not take the session lock |
| 1 | 2026-09-12T12:44 | 2026-09-12T13:53 | complete | 70.46303749999996 | ran unattended, 408 turns, 13 denied call(s) worked around, report valid |
| phase | 2026-09-12T13:54 | 2026-09-12T13:54 | halted | 70.4630 | backstop: 1 iterations, no stop condition fired |
| 1 | 2026-09-12T14:13 | 2026-09-12T15:16 | complete | 32.58145499999999 | ran unattended, 297 turns, 7 denied call(s) worked around, report valid |
| 2 | 2026-09-12T15:22 | 2026-09-12T15:43 | complete | 10.719323 | ran unattended, 149 turns, 15 denied call(s) worked around, report valid |
| phase | 2026-09-12T15:44 | 2026-09-12T15:44 | halted | 32.5815 | stop 3: a ruling is wanted - judged, not counted |
| 1 | 2026-09-12T16:09 | 2026-09-12T16:29 | complete | 9.115458999999998 | ran unattended, 170 turns, 10 denied call(s) worked around, report valid |
| 2 | 2026-09-12T16:35 | 2026-09-12T17:29 | complete | 28.656047000000004 | ran unattended, 300 turns, 8 denied call(s) worked around, report valid |
| 3 | 2026-09-12T17:36 | 2026-09-12T18:07 | complete | 15.901997 | ran unattended, 193 turns, 5 denied call(s) worked around, report valid |
| phase | 2026-09-12T18:08 | 2026-09-12T18:08 | halted | 37.7715 | stop 3: a ruling is wanted - judged, not counted |
| 1 | 2026-09-12T19:21 | 2026-09-12T20:06 | complete | 20.433159000000003 | ran unattended, 191 turns, 7 denied call(s) worked around, report valid |
| 2 | 2026-09-12T20:12 | 2026-09-12T21:02 | complete | 24.163439500000006 | ran unattended, 216 turns, 6 denied call(s) worked around, report valid |
| 3 | 2026-09-12T21:07 | 2026-09-12T21:30 | complete | 11.540931500000001 | ran unattended, 151 turns, 8 denied call(s) worked around, report valid |
| 4 | 2026-09-12T21:36 | 2026-09-12T21:59 | complete | 10.092021 | ran unattended, 109 turns, 9 denied call(s) worked around, report valid |
| 5 | 2026-09-12T22:06 | 2026-09-12T22:38 | complete | 12.401394500000002 | ran unattended, 148 turns, 8 denied call(s) worked around, report valid |
| 6 | 2026-09-12T22:44 | 2026-09-12T23:40 | killed | unknown | killed by the watchdog: no status write within 12 min of the launch clock |
| phase | 2026-09-12T23:41 | 2026-09-12T23:41 | halted | 78.6309 | the run could not take the session lock |
| 1 | 2026-09-13T09:19 | 2026-09-13T09:55 | killed | unknown | killed by the watchdog: no status write within 12 min of the launch clock |
| phase | 2026-09-13T09:55 | 2026-09-13T09:55 | halted | 0 | the run could not take the session lock |
| 1 | 2026-09-13T18:20 | 2026-09-13T18:32 | complete | 5.3819905000000015 | ran unattended, 69 turns, 8 denied call(s) worked around, report valid |
| phase | 2026-09-13T18:33 | 2026-09-13T18:33 | halted | 0 | stop 3: a ruling is wanted on one of the three - judged, not counted |
| 1 | 2026-09-13T21:01 | 2026-09-13T21:33 | complete | 13.254202499999996 | ran unattended, 165 turns, 16 denied call(s) worked around, report valid |
| 2 | 2026-09-13T21:41 | 2026-09-13T22:06 | complete | 9.6476085 | ran unattended, 135 turns, 4 denied call(s) worked around, report valid |
| 3 | 2026-09-13T22:13 | 2026-09-13T22:40 | complete | 8.573785499999998 | ran unattended, 114 turns, 4 denied call(s) worked around, report valid |
| 4 | 2026-09-13T22:53 | 2026-09-13T23:31 | complete | 13.8218565 | ran unattended, 196 turns, 10 denied call(s) worked around, report valid |
| 5 | 2026-09-13T23:40 | 2026-09-14T00:11 | complete | 13.679649000000001 | ran unattended, 218 turns, 13 denied call(s) worked around, report valid |
| 6 | 2026-09-14T00:19 | 2026-09-14T00:39 | complete | 7.633490500000001 | ran unattended, 119 turns, 6 denied call(s) worked around, report valid |
| 7 | 2026-09-14T00:45 | 2026-09-14T01:12 | complete | 9.589631500000003 | ran unattended, 122 turns, 2 denied call(s) worked around, report valid |
| 8 | 2026-09-14T01:21 | 2026-09-14T01:52 | complete | 12.794684999999996 | ran unattended, 203 turns, 5 denied call(s) worked around, report valid |
| 9 | 2026-09-14T02:00 | 2026-09-14T02:28 | complete | 10.5068985 | ran unattended, 186 turns, 8 denied call(s) worked around, report valid |
| 10 | 2026-09-14T02:39 | 2026-09-14T03:07 | complete | 11.855571999999999 | ran unattended, 136 turns, 3 denied call(s) worked around, report valid |
| phase | 2026-09-14T03:08 | 2026-09-14T03:08 | halted | 99.5018 | stop 3: a ruling is wanted on one of the three - judged, not counted |
| 1 | 2026-09-14T08:25 | 2026-09-14T09:27 | complete | 27.369619999999998 | ran unattended, 298 turns, 13 denied call(s) worked around, report valid |
| phase | 2026-09-14T09:29 | 2026-09-14T09:29 | halted | 0 | stop 3: a ruling is wanted on one of the three - judged, not counted |
| 1 | 2026-09-14T11:03 | 2026-09-14T11:07 | complete | 1.7553294999999998 | ran unattended, 30 turns, 4 denied call(s) worked around, report valid |
| phase | 2026-09-14T11:58 | 2026-09-14T11:58 | halted | 15.0457 | the run could not take the session lock |
| phase | 2026-09-14T13:03 | 2026-09-14T13:03 | halted | 0 | stop 3: a ruling is wanted on one of the three - judged, not counted |
| phase | 2026-09-18T22:18 | 2026-09-18T22:18 | halted | 11.7896 | the run could not take the session lock |
| 1 | 2026-09-19T11:00 | 2026-09-19T11:43 | complete | 14.782491499999999 | ran unattended, 136 turns, 10 denied call(s) worked around, report valid |
| 2 | 2026-09-19T11:49 | 2026-09-19T12:42 | complete | 17.930809499999995 | ran unattended, 140 turns, 6 denied call(s) worked around, report valid |
| phase | 2026-09-19T14:09 | 2026-09-19T14:09 | halted | 50.6441 | stop 4: the arbiter declared a decision the owner's |
| phase | 2026-09-19T17:11 | 2026-09-19T17:11 | halted | 0 | the run could not take the session lock |
| 1 | 2026-09-19T17:10 | 2026-09-19T18:14 | complete | 28.044364500000007 | ran unattended, 209 turns, 21 denied call(s) worked around, report valid |
| 2 | 2026-09-19T18:25 | 2026-09-19T20:45 | complete | 57.7013725 | ran unattended, 338 turns, 12 denied call(s) worked around, report valid |
| 3 | 2026-09-19T20:55 | 2026-09-19T22:28 | complete | 39.397907 | ran unattended, 240 turns, 9 denied call(s) worked around, report valid |
| 4 | 2026-09-19T22:38 | 2026-09-19T23:54 | complete | 41.152798999999995 | ran unattended, 283 turns, 6 denied call(s) worked around, report valid |
| phase | 2026-09-20T00:00 | 2026-09-20T00:00 | halted | 166.2965 | stop 4: the arbiter declared a decision the owner's |
| 1 | 2026-09-20T09:33 | 2026-09-20T09:39 | complete | 2.0802184999999995 | ran unattended, 45 turns, 8 denied call(s) worked around, report valid |
| phase | 2026-09-20T09:44 | 2026-09-20T09:44 | halted | 2.0802 | stop 4: the arbiter declared a decision the owner's |
| 1 | 2026-09-20T16:38 | 2026-09-20T17:23 | complete | 19.234482500000006 | ran unattended, 207 turns, 6 denied call(s) worked around, report valid |
| phase | 2026-09-20T17:30 | 2026-09-20T17:30 | halted | 19.2345 | refused: the decision block named no step and no criterion |
| 1 | 2026-09-20T19:35 | 2026-09-20T20:25 | complete | 16.45659 | ran unattended, 156 turns, 14 denied call(s) worked around, report valid |
| 2 | 2026-09-20T20:34 | 2026-09-20T21:36 | complete | 15.745063499999992 | ran unattended, 137 turns, 2 denied call(s) worked around, report valid |
| 3 | 2026-09-20T21:44 | 2026-09-20T22:33 | failed | 14.387669500000003 | run-unit exit 4: 3 denied call(s), is_error=False, terminal=completed |
| phase | 2026-09-20T22:34 | 2026-09-20T22:34 | halted | 32.2017 | stop 7: validate-output refused the report (after 3 denied calls) |
| 1 | 2026-09-21T08:08 | 2026-09-21T09:59 | complete | 11.124889999999997 | ran unattended, 127 turns, 7 denied call(s) worked around, report valid |
| phase | 2026-09-21T10:00 | 2026-09-21T10:00 | halted | 0 | stop 3: a ruling is wanted on one of the three - judged, not counted |
| 1 | 2026-09-21T10:41 | 2026-09-21T11:30 | complete | 22.841067 | ran unattended, 175 turns, 4 denied call(s) worked around, report valid |
| phase | 2026-09-21T11:31 | 2026-09-21T11:31 | halted | 0 | stop 3: a ruling is wanted on one of the three - judged, not counted |
| 1 | 2026-09-21T11:51 | 2026-09-21T12:57 | complete | 24.232530499999992 | ran unattended, 198 turns, 17 denied call(s) worked around, report valid |
| phase | 2026-09-21T12:59 | 2026-09-21T12:59 | halted | 0 | stop 3: a ruling is wanted on one of the three - judged, not counted |
| 1 | 2026-09-21T13:51 | 2026-09-21T14:53 | complete | 34.28825450000001 | ran unattended, 225 turns, 11 denied call(s) worked around, report valid |
| 2 | 2026-09-21T15:06 | 2026-09-21T15:55 | complete | 23.973417499999996 | ran unattended, 191 turns, 14 denied call(s) worked around, report valid |
| 3 | 2026-09-21T16:06 | 2026-09-21T17:05 | complete | 38.310687500000014 | ran unattended, 263 turns, 9 denied call(s) worked around, report valid |
| 4 | 2026-09-21T17:14 | 2026-09-21T18:16 | complete | 22.815561999999986 | ran unattended, 186 turns, 7 denied call(s) worked around, report valid |
| 5 | 2026-09-21T18:28 | 2026-09-21T19:16 | complete | 24.00254899999999 | ran unattended, 217 turns, 3 denied call(s) worked around, report valid |
| 6 | 2026-09-21T19:28 | 2026-09-21T20:21 | complete | 23.4784715 | ran unattended, 188 turns, 9 denied call(s) worked around, report valid |
| 7 | 2026-09-21T20:33 | 2026-09-21T20:56 | failed | 4.7155615 | run-unit exit 4: 4 denied call(s), is_error=True, terminal=api_error |
| phase | 2026-09-21T20:57 | 2026-09-21T20:57 | halted | 166.8690 | stop 6: denied 4 and could not complete - is_error True, terminal api_error |
| phase | 2026-09-21T21:25 | 2026-09-21T21:25 | failure | 0 | STOPPED, AND A STOP IS FAILURE - refused: ADVANCES named no step and criterion, and no unit or criterion it unblocks |
| 1 | 2026-09-22T08:13 | 2026-09-22T09:34 | complete | 10.913310000000006 | ran unattended, 121 turns, 4 denied call(s) worked around, report valid |
| phase | 2026-09-22T09:48 | 2026-09-22T09:48 | failure | 10.9133 | STOPPED, AND A STOP IS FAILURE - refused: ADVANCES named no step and criterion, and no unit or criterion it unblocks |
| 1 | 2026-09-22T10:22 | 2026-09-22T11:41 | complete | 43.3960615 | ran unattended, 310 turns, 13 denied call(s) worked around, report valid |
| phase | 2026-09-22T11:53 | 2026-09-22T11:53 | ending | 43.3961 | ENDED - the arbiter raised one of the three for the owner. stop 4: the arbiter declared a decision the owner's |
| 1 | 2026-09-22T13:14 | 2026-09-22T14:08 | complete | 11.630765599999995 | ran unattended, 219 turns, 5 denied call(s) worked around, report valid |
| 2 | 2026-09-22T14:15 | 2026-09-22T14:58 | complete | 12.394016599999993 | ran unattended, 255 turns, 6 denied call(s) worked around, report valid |
| phase | 2026-09-22T14:59 | 2026-09-22T14:59 | ending | 11.6308 | ENDED - a ruling is wanted on one of the three. stop 3: a ruling is wanted on one of the three - judged, not counted |
| phase | 2026-09-22T17:26 | 2026-09-22T17:26 | failure | 0 | STOPPED, AND A STOP IS FAILURE - stop 11: nothing was launched - run exit 2, and output.md was written after this unit was launched |
| phase | 2026-09-22T18:01 | 2026-09-22T18:01 | ending | 0 | ENDED - nothing is left but the owner-s verdict. stop 1: the phase is waiting on the owner's verdict - criteria 5.1 |
| phase | 2026-09-22T19:18 | 2026-09-22T19:18 | failure | 0 | STOPPED, AND A STOP IS FAILURE - stop 11: nothing was launched - run exit 2, and output.md was written after this unit was launched |
| 1 | 2026-09-22T20:06 | 2026-09-22T20:51 | complete | 8.7401892 | ran unattended, 191 turns, 15 denied call(s) worked around, report valid |
| 2 | 2026-09-22T21:00 | 2026-09-22T21:49 | complete | 3.499198799999999 | ran unattended, 93 turns, 3 denied call(s) worked around, report valid |
| 3 | 2026-09-22T22:03 | 2026-09-22T23:01 | complete | 4.033674599999999 | ran unattended, 114 turns, 6 denied call(s) worked around, report valid |
| phase | 2026-09-22T23:03 | 2026-09-22T23:03 | ending | 12.2394 | ENDED - a ruling is wanted on one of the three. stop 3: a ruling is wanted on one of the three - judged, not counted |
| 1 | 2026-09-22T23:29 | 2026-09-23T00:27 | complete | 5.8675254 | ran unattended, 147 turns, 9 denied call(s) worked around, report valid |
| 2 | 2026-09-23T00:37 | 2026-09-23T01:36 | complete | 8.2814368 | ran unattended, 236 turns, 8 denied call(s) worked around, report valid |
| 3 | 2026-09-23T01:43 | 2026-09-23T02:19 | complete | 5.770728200000002 | ran unattended, 156 turns, 10 denied call(s) worked around, report valid |
| 3 | 2026-09-23T02:20 | 2026-09-23T02:20 | note | none - not a run | no advance - step 4 criterion 2 flipped, and the state judge did not find it honestly met: no |
| 4 | 2026-09-23T02:31 | 2026-09-23T03:02 | complete | 6.718268799999998 | ran unattended, 131 turns, 8 denied call(s) worked around, report valid |
| 5 | 2026-09-23T03:13 | 2026-09-23T03:44 | complete | 3.5265805999999995 | ran unattended, 91 turns, 8 denied call(s) worked around, report valid |
| 5 | 2026-09-23T03:45 | 2026-09-23T03:45 | note | none - not a run | blocker-clear - cleared a blocker: the two clean synthetics, the third floor test, red since 8e3ee277 and step 3's repair under R53, without which step 3 criteria 3.4 and 3.5 cannot be met; set names #25, #26, #31 and #32 |
| 6 | 2026-09-23T04:05 | 2026-09-23T04:44 | complete | 4.036833199999999 | ran unattended, 100 turns, 6 denied call(s) worked around, report valid |
| 6 | 2026-09-23T04:45 | 2026-09-23T04:45 | note | none - not a run | no advance - step 3 criterion 3 was met and is met |
| 7 | 2026-09-23T04:56 | 2026-09-23T05:28 | complete | 2.9067488000000004 | ran unattended, 90 turns, 4 denied call(s) worked around, report valid |
| phase | 2026-09-23T05:29 | 2026-09-23T05:29 | ending | 37.1080 | ENDED - nothing is left but the owner-s verdict. stop 1: the phase is waiting on the owner's verdict - criteria 5.1 |
| 1 | 2026-09-23T08:10 | 2026-09-23T08:58 | complete | 5.784130800000002 | ran unattended, 111 turns, 9 denied call(s) worked around, report valid |
| 1 | 2026-09-23T08:59 | 2026-09-23T08:59 | note | none - not a run | no advance - step 3 criterion 6 was unmet and is unmet |
| phase | 2026-09-23T09:32 | 2026-09-23T09:32 | failure | 0 | STOPPED, AND A STOP IS FAILURE - refused: ADVANCES named no step and criterion, and no unit or criterion it unblocks |
| 1 | 2026-09-23T09:35 | 2026-09-23T10:11 | complete | 2.381359200000001 | ran unattended, 85 turns, 8 denied call(s) worked around, report valid |
| 1 | 2026-09-23T10:12 | 2026-09-23T10:12 | note | none - not a run | blocker-clear - cleared a blocker: criterion 3.6 |
| 2 | 2026-09-23T10:15 | 2026-09-23T12:23 | complete | 6.859046 | ran unattended, 166 turns, 13 denied call(s) worked around, report valid |
| 3 | 2026-09-23T12:28 | 2026-09-23T13:29 | complete | 13.461637400000008 | ran unattended, 220 turns, 14 denied call(s) worked around, report valid |
| 3 | 2026-09-23T13:30 | 2026-09-23T13:30 | note | none - not a run | no advance - step 3 criterion 6 was unmet and is unmet |
| 4 | 2026-09-23T13:34 | 2026-09-23T14:55 | complete | 9.276435599999996 | ran unattended, 179 turns, 7 denied call(s) worked around, report valid |
| 4 | 2026-09-23T14:56 | 2026-09-23T14:56 | note | none - not a run | no advance - step 3 criterion 6 was unmet and is unmet |
| 5 | 2026-09-23T15:02 | 2026-09-23T15:45 | complete | 6.751054999999997 | ran unattended, 145 turns, 15 denied call(s) worked around, report valid |
| 5 | 2026-09-23T15:46 | 2026-09-23T15:46 | note | none - not a run | no advance - step 3 criterion 6 was unmet and is unmet |
| 1 | 2026-09-23T16:06 | 2026-09-23T17:11 | complete | 9.123549399999998 | ran unattended, 186 turns, 5 denied call(s) worked around, report valid |
| 2 | 2026-09-23T17:15 | 2026-09-23T17:58 | complete | 4.7063060000000005 | ran unattended, 116 turns, 7 denied call(s) worked around, report valid |
| phase | 2026-09-23T17:59 | 2026-09-23T17:59 | ending | 13.8298 | ENDED - nothing is left but the owner-s verdict. stop 1: the phase is waiting on the owner's verdict - criteria 5.1 |
| phase | 2026-09-23T18:42 | 2026-09-23T18:42 | ending | 0 | ENDED - nothing is left but the owner-s verdict. stop 1: the phase is waiting on the owner's verdict - criteria 5.1 |
| 1 | 2026-09-23T18:44 | 2026-09-23T19:19 | failed | 3.9468494000000005 | run-unit exit 4: 5 denied call(s), is_error=False, terminal=completed |
| phase | 2026-09-23T19:21 | 2026-09-23T19:21 | failure | 0 | STOPPED, AND A STOP IS FAILURE - stop 7: validate-output refused the report (after 5 denied calls) |
| 1 | 2026-09-23T20:45 | 2026-09-23T21:27 | complete | 6.900385599999997 | ran unattended, 166 turns, 15 denied call(s) worked around, report valid |
| phase | 2026-09-23T21:28 | 2026-09-23T21:28 | ending | 0 | ENDED - a ruling is wanted on one of the three. stop 3: a ruling is wanted on one of the three - judged, not counted |
