#!/bin/sh
# unit 439 (by hand) task 4 - writes docs/phase-requirements/metrics-baseline.md from TheRequirementsAreMeasuredTests' output
cd /c/Source/HamLet || exit 1
M=.run-unit/unit439h-metrics-measure.txt
OUT=docs/phase-requirements/metrics-baseline.md
lines() { grep -E "^\s*$1" $M | sed 's/^\s*//'; }
{
echo "# The requirement metrics, baseline at unit 439"
echo
echo "Work instruction 439, tasks 3 and 4 (PHASE_PLAN.md 1.1 to 1.3), run by hand outside the loop on"
echo "2026-09-25 at HEAD \`efdad2bc\`, which carries no change under \`src\` from the entry \`7734ecbe\`."
echo "Printed by \`TheRequirementsAreMeasuredTests\` (engine, 2 facts, 61 s) and copied here from its"
echo "output, \`.run-unit/unit439h-metrics-measure.txt\`. **Every real key is inferred** and disagreement"
echo "with one is not by itself proof the decoder is wrong (V-13). **Every synthetic key is exact** and no"
echo "synthetic case is ever sole evidence (12.5)."
echo
echo "## The definitions, quoted from \`CW_SPEC.md\` section 11"
echo
echo "- **MET-INVENTED** - (sure insertions + sure substitutions) / characters sent."
echo "- **MET-CER-SURE** - MET-CER over characters emitted as sure; dim and placeholders excluded, not counted wrong."
echo "- **MET-COVERAGE** - Sure characters emitted / characters sent."
echo "- **MET-WBE** - Word gaps inserted or deleted relative to truth / words sent. Scored on boundaries alone, separately from MET-CER."
echo
echo "How \`CwMetrics\` reads them: a character is one symbol, a prosign included, and characters sent"
echo "exclude spaces. The character alignment has the spaces taken out of both sides and is \`CwScorer\`'s"
echo "Levenshtein over the symbols. MET-CER-SURE's denominator is the sure characters emitted, which is"
echo "how HM-REQ-010 reads it (\"one in a hundred sure characters wrong\"), and its numerator is the sure"
echo "ones wrong or added. MET-COVERAGE is the ratio as written, which exceeds one where sure characters"
echo "are added; its sure-and-right count is printed beside it. The decoder emits two classes, sure"
echo "(\`High\`) and placeholder (\`Unreadable\`). A named character below high would be counted apart and"
echo "never as sure; there are none today, and no dim class is invented."
echo
echo "## Conditions"
echo
echo "\`CW_SPEC.md\` section 4 states a condition as a channel profile, a sender profile and an SNR in the"
echo "2500 Hz reference. **No real recording here has all three.** None was received on a named \`CH-*\`"
echo "channel, and none carries an SNR in the reference bandwidth. Each is grouped by its sender where"
echo "section 10 names the capture (013347 TX-TIGHT, 012403 TX-ITU, 004507 TX-FARNS), and by \"sender not"
echo "stated\" otherwise. The synthetic set is labeled as generated: no fading, a shaped noise band that is"
echo "not shown to be \`CH-AWGN\`, a known sender and an in-passband level that is not restated in the"
echo "reference. **So no number below is yet a measurement of a requirement at its own condition.** It is"
echo "the metric over the corpus the tree has."
echo
echo "## Totals"
echo
lines "total \|" | sed 's/^/    /'
echo
echo "## Per condition"
echo
echo "MET-INVENTED:"
echo
grep -E "^\s*condition \|" $M | sed "s/^\s*//" | awk -F"|" "NF==11" | sed "s/^/    /"
echo
echo "MET-CER-SURE, MET-COVERAGE, MET-WBE:"
echo
grep -E "^\s*condition \|" $M | sed "s/^\s*//" | awk -F"|" "NF==13" | sed "s/^/    /"
echo
echo "## Per recording"
echo
echo "MET-INVENTED, with the stray trace's old count of added named characters beside it:"
echo
grep -E "^\s*row \|" $M | sed 's/^\s*//' | awk -F'|' 'NF==14' | sed 's/^/    /'
echo
echo "MET-CER-SURE, MET-COVERAGE, MET-WBE and \`CwScorer.Kinds\`' boundary edits on the same stretches:"
echo
grep -E "^\s*row \|" $M | sed 's/^\s*//' | awk -F'|' 'NF==19' | sed 's/^/    /'
echo
echo "## How MET-WBE relates to \`CwScorer.Kinds\`"
echo
echo "\`Kinds\` counts space edits in the text alignment, where spaces are characters. There, the"
echo "tie-break can seat a letter against a space and count that one edit as a boundary. MET-WBE places"
echo "boundaries on the letters-only alignment, so no letter error can stand in for one. On the synthetic"
echo "set they agree exactly: 19 inserted and 30 deleted, against 19 spaces added and 30 missing. On the"
echo "real set MET-WBE counts 42 inserted and 15 deleted, while \`Kinds\` counts 55 spaces added and 5"
echo "missing. The requirement's number is MET-WBE (HM-REQ-082). \`Kinds\` remains the edit total's"
echo "breakdown."
echo
echo "## Reaching the metrics from another test type (1.4)"
echo
echo "\`CwMetrics\` is a public static class in the engine test assembly, like \`CwScorer\`."
echo "\`CwMetrics.Symbols\` takes any settled characters. \`CwMetrics.Align\` takes symbols, a key and its kind."
echo "\`Invented\`, \`SureErrors\`, \`Coverage\` and \`WordBoundaries\` each take that alignment, and"
echo "\`TheRequirementsAreMeasuredTests.Measure\` cuts a decode into the baseline's scored stretches first."
echo "Two types call all four: \`TheMetricsCountWhatAHandCountsTests\` on pairs known by construction, and"
echo "\`TheRequirementsAreMeasuredTests\` on every keyed recording."
} > $OUT
wc -l $OUT
