#!/bin/sh
# unit 471 - the per-condition MET-CER-SURE / coverage / WBE rows and the MET-INVENTED rows of a metrics printout, shortened.
# Usage: sh .run-unit/unit472-conditions.sh <suffix>
cd /c/Source/HamLet || exit 1
export LC_ALL=C
F=.run-unit/unit472-metrics-$1.txt
short() {
  sed -e "s/real HF, no CH-\* profile, SNR_2500 not measured, //" \
      -e "s/synthetic, no fading, shaped noise band (not shown to be CH-AWGN), //" \
      -e "s/ in the passband (not restated in the 2500 Hz reference)//" \
      -e "s/ (CW_SPEC.md[^)]*)//" -e "s/, inside TX-FARNS's 3 to 7//"
}
echo "== $1: condition | key | sure emitted | wrong or added | MET-CER-SURE | sent | MET-COVERAGE | words | wrong | MET-WBE"
grep -a -E "^ *condition \| (real|synthetic) \| (real HF|synthetic,)" "$F" | awk -F'|' 'NF==13' | cut -d'|' -f3-12 | short
echo "== $1: MET-INVENTED condition | key | invented | sure added | sure wrong | sent"
grep -a -E "^ *condition \| (real|synthetic) \| (real HF|synthetic,)" "$F" | awk -F'|' 'NF==11' | cut -d'|' -f3-8 | short
