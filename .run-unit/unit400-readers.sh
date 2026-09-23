cd /c/Source/HamLet
# Usage: sh .run-unit/unit400-readers.sh <tag> "TASK n of 5" "<note prefix>"
# The three fixture-reading types, --no-build, one invocation each.
sh .run-unit/unit400-floors.sh fixtures-$1 600 "FullyQualifiedName~Hamlet.RadioEngine.Tests.Cw.CwFixtureTests" "$2" "$3 CwFixtureTests whole running" --no-build
grep -E "^\s+(Passed|Failed) " .run-unit/unit400-fixtures-$1.txt | sed -E "s/^\s+//" | sed -E "s/Hamlet.RadioEngine.Tests.Cw.//" | sed -E "s/ \[.*//" | sort
sh .run-unit/unit400-floors.sh cleanreads-$1 300 "FullyQualifiedName~TheCleanReadsStayCleanTests" "$2" "$3 TheCleanReadsStayCleanTests running" --no-build
grep -E "^\s+(Passed|Failed) " .run-unit/unit400-cleanreads-$1.txt | sed -E "s/^\s+//" | sed -E "s/Hamlet.RadioEngine.Tests.Cw.//" | sed -E "s/ \[.*//" | sort
sh .run-unit/unit400-floors.sh survey-$1 300 "FullyQualifiedName~TheSurveyAlreadyUsesAShortWindowTests" "$2" "$3 TheSurveyAlreadyUsesAShortWindowTests running" --no-build
grep -E "^\s+(Passed|Failed) " .run-unit/unit400-survey-$1.txt | sed -E "s/^\s+//" | sed -E "s/Hamlet.RadioEngine.Tests.Cw.//" | sed -E "s/ \[.*//" | sort
date +%H:%M:%S
