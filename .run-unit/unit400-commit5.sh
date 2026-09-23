cd /c/Source/HamLet
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo end
sh .run-unit/unit400-transmit.sh
sh .run-unit/unit400-commit.sh "unit400 ticks: 3.5 ticked with its R53 qualification at repair commit 7d1ffde6, all three floor tests green at the exit of every commit from it, both lines green with no red on an assertion; 3.1 gains #25, #26, #31, #32 and #30, #33, the set at 37 green and 14 red-open; 1.3 gains the two clean synthetics green 2 of 2; 3.4 untouched; doc section 6" "TASK 4 of 5" "task 4: ticks committed; adding the 400 item lines to PARKED.md" PHASE_PLAN.md docs/phase-cw/unit400-harness.md 2>&1 | grep -v "^	\|^  (use\|^$\|^Untracked\|^Changes not\|^On branch\|^Your branch"
sh .run-unit/unit400-commit.sh "unit400 PARKED: 400 items 1 to 5 - CwSensitivityTests red outside the set, section 5 mismatches, the app line dispatcher losses, the confident-mistakes cases under the settled transcript, the temporary writer and git rm" "TASK 4 of 5" "task 4: PARKED committed; writing output.md" docs/phase-cw/PARKED.md 2>&1 | grep -v "^	\|^  (use\|^$\|^Untracked\|^Changes not\|^On branch\|^Your branch"
git log --oneline -8
date +%H:%M:%S
