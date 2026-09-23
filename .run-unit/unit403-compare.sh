#!/bin/sh
# Work instruction 403: compare the two builds - the printer lines and the 37 capture rows.
cd /c/Source/HamLet/.run-unit
grep -E "^ (transcript|characters|elements|winning|tone)" unit403-head.txt > unit403-cmp-head.txt
grep -E "^ (transcript|characters|elements|winning|tone)" unit403-before.txt > unit403-cmp-before.txt
cmp unit403-cmp-head.txt unit403-cmp-before.txt && echo "printer lines identical, HEAD and a902cdf8"
grep -E " characters against a floor of " unit403-entry-captures.txt | sed 's/^ *//' | sort > unit403-rows-head.txt
grep -E " characters against a floor of " unit403-before-captures.txt | sed 's/^ *//' | sort > unit403-rows-before.txt
grep -E " characters against a floor of " unit403-before-captures-run1.txt | sed 's/^ *//' | sort > unit403-rows-before-run1.txt
wc -l unit403-rows-head.txt unit403-rows-before.txt unit403-rows-before-run1.txt
diff unit403-rows-head.txt unit403-rows-before.txt > unit403-rows-diff.txt
echo "rows diff HEAD against a902cdf8 rc=$?, lines $(wc -l < unit403-rows-diff.txt)"
cmp unit403-rows-before.txt unit403-rows-before-run1.txt && echo "a902cdf8 rows identical across its two runs"
grep -E "Total tests|Passed:|Failed:" unit403-before-captures.txt
