cd /c/Source/HamLet
git log --oneline -1
sh .run-unit/unit400-commit.sh "step 3 repair: clean-12wpm and clean-18wpm regenerated with a band of 0.02 under HM-DEC-127 and HM-OPEN-018; TheCleanRecordingsDecodeExactly 2 of 2 on the settled transcript, floors 37 of 37 and 13 of 13 unmoved, R49" "TASK 2 of 5" "task 2 committed, the repair; deciding task 3 against the clock" PHASE_OUTCOME.md tests/Hamlet.RadioEngine.Tests/Cw/CwFixtures.cs tests/fixtures/cw/clean-12wpm.wav tests/fixtures/cw/clean-18wpm.wav docs/phase-cw/unit400-harness.md 2>&1 | grep -v "^	\|^  (use\|^$\|^Untracked\|^Changes not\|^On branch\|^Your branch"
git status --short tests
grep -c "TASK 2, THE BAND" PHASE_OUTCOME.md
date +%H:%M:%S
