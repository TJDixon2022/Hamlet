#!/bin/sh
# unit 466 - the saves with class and p: ours alone and the port alone; the arbitrated transcript in the harness; and live, with decode time.
# Usage: sh .run-unit/unit466-saves.sh <alone|arb|live> <suffix> "<n of 4>" [compare-suffix]
# Files: unit466-ours-<suffix>.txt, unit466-port-<suffix>.txt, unit466-arb-<suffix>.txt, unit466-arb-live-<suffix>.txt
cd /c/Source/HamLet || exit 1
export LC_ALL=C
R=.run-unit/unit466-run.sh
F=FullyQualifiedName~.WhereTheTwoReadingsMeetFact
case "$1" in
  alone)
    sh $R "save-alone-$2" engine 600 "$3" "Saving ours alone and the port alone, each character with class and p, all 35" "$F.EachDecodersCharactersWithClassAndP" --no-build
    grep -a "^ *save | ours | " .run-unit/unit466-save-alone-$2.txt | sed 's/^ *//' > .run-unit/unit466-ours-$2.txt
    grep -a "^ *save | port | " .run-unit/unit466-save-alone-$2.txt | sed 's/^ *//' > .run-unit/unit466-port-$2.txt
    wc -l .run-unit/unit466-ours-$2.txt .run-unit/unit466-port-$2.txt
    if [ -n "$4" ]; then
      cmp .run-unit/unit466-ours-$4.txt .run-unit/unit466-ours-$2.txt && echo "ours alone BYTE-IDENTICAL to $4"
      cmp .run-unit/unit466-port-$4.txt .run-unit/unit466-port-$2.txt && echo "port alone BYTE-IDENTICAL to $4"
    fi
    cmp .run-unit/unit465-text-before.txt .run-unit/unit466-ours-$2.txt && echo "ours alone byte-identical to 465's task 0 save"
    cmp .run-unit/unit465-port-before.txt .run-unit/unit466-port-$2.txt && echo "port alone byte-identical to 465's task 0 save"
    ;;
  arb)
    sh $R "save-arb-$2" engine 600 "$3" "Saving the arbitrated transcript in the harness, all 35 under their own rows" "$F.TheCorpusThroughTheArbiter" --no-build
    grep -a "^ *save | ours | " .run-unit/unit466-save-arb-$2.txt | sed 's/^ *//' > .run-unit/unit466-arb-$2.txt
    wc -l .run-unit/unit466-arb-$2.txt
    grep -a -c "harvest check | .* | same |" .run-unit/unit466-save-arb-$2.txt
    if [ -n "$4" ]; then
      cmp .run-unit/unit466-arb-$4.txt .run-unit/unit466-arb-$2.txt && echo "arbitrated harness BYTE-IDENTICAL to $4"
    fi
    cmp .run-unit/unit465-text-before.txt .run-unit/unit466-arb-$2.txt && echo "arbitrated harness byte-identical to 465's save of ours"
    ;;
  live)
    sh $R "save-live-$2" engine 600 "$3" "Saving the arbitrated transcript on the live path, all 35, and timing it" "$F.TheLivePathOverTheCorpus" --no-build
    grep -a "^ *save | ours | " .run-unit/unit466-save-live-$2.txt | sed 's/^ *//' > .run-unit/unit466-arb-live-$2.txt
    wc -l .run-unit/unit466-arb-live-$2.txt
    if [ -n "$4" ]; then
      cmp .run-unit/unit466-arb-live-$4.txt .run-unit/unit466-arb-live-$2.txt && echo "arbitrated live BYTE-IDENTICAL to $4"
    fi
    cmp .run-unit/unit465-text-before.txt .run-unit/unit466-arb-live-$2.txt && echo "arbitrated live byte-identical to 465's save of ours"
    grep -a "decode time |" .run-unit/unit466-save-live-$2.txt | sed 's/^ *//'
    ;;
esac
