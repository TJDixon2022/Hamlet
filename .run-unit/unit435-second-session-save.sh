#!/bin/sh
# unit 435, the second session - keep the task 1 trace draft outside the build, then put back the files this session changed, as HEAD and the launcher left them.
cd /c/Source/HamLet || exit 1
git diff -- tests/Hamlet.RadioEngine.Tests/Cw/WhatTheOpeningHeardTests.cs > .run-unit/unit435-second-session-trace-draft.patch
git diff -- Directory.Build.props PHASE_OUTCOME.md > .run-unit/unit435-second-session-record-draft.patch
wc -l .run-unit/unit435-second-session-trace-draft.patch .run-unit/unit435-second-session-record-draft.patch
git restore -- tests/Hamlet.RadioEngine.Tests/Cw/WhatTheOpeningHeardTests.cs Directory.Build.props PHASE_OUTCOME.md
git diff --stat
