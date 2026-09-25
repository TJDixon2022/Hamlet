# PHASE_OUTCOME.md

A reader test record for attempt-read.bat. The example below is fenced and must never
read as an attempt:

```
ATTEMPT: 2.3 | unit 99 | yes | executed | a format example inside a fence
```

## PHASE

PHASE: reader test
PHASE_SET: 2026-09-14
STEP: 2 | in progress | the step

---

## UNIT 1 - STEP 2

STEP: 2
APPROACH: poll the process tree
ADVANCED: no
ATTEMPT: 2.3 | unit 1 | no | executed | poll the process tree

## UNIT 2 - STEP 2

STEP: 2
ATTEMPT: 2.4 | unit 2 | yes | executed | a different criterion entirely

## UNIT 3 - STEP 2

STEP: 2
ATTEMPT: 2.3 | unit 3 | no | never ran | read the watchdog's own log instead | which carries a pipe, and a very long tail that must come back whole: measure CPU time across every process in the tree at sixty-second looks, compare it against the floor of one hundred milliseconds that 062 recorded, and count consecutive quiet looks from disk rather than from memory so that a restarted watcher resumes the count

## UNIT 4 - STEP 12

STEP: 12
ATTEMPT: 12.3 | unit 4 | no | executed | twelve point three must not read as two point three

## UNIT 5 - STEP 2

STEP: 2
ATTEMPT: 2.3 | unit 5 | yes | executed | kill by PID and release the lock
