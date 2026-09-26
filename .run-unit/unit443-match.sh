#!/bin/sh
# unit 443 - the three added lists side by side: 1fb0bad6 (439's 13), 42d5dbb9 (G1 alone), HEAD.
# A character matches across commits on recording, emitted text and time within 0.3 s.
cd /c/Source/HamLet || exit 1
pick() {
  grep -E "^\s*added \| " "$1" | grep -v "added | recording" | awk -F' [|] ' -v tag="$2" '{ print tag "\t" $2 "\t" $3 "\t" $5 "\t" $10 "\t" $6 "\t" $7 "\t" $11 "\t" $15 "\t" $16 "\t" $17 "\t" $22 "\t" $12 "\t" $28 }'
}
pick .run-unit/unit443-added-1fb0bad6.txt base > .run-unit/unit443-match-base.tmp
pick .run-unit/unit443-added-42d5dbb9.txt g1 > .run-unit/unit443-match-g1.tmp
pick .run-unit/unit443-added-head.txt head > .run-unit/unit443-match-head.tmp
echo "counts: base real $(grep -c "	real	" .run-unit/unit443-match-base.tmp), g1 real $(grep -c "	real	" .run-unit/unit443-match-g1.tmp), head real $(grep -c "	real	" .run-unit/unit443-match-head.tmp)"
echo "counts: base synthetic $(grep -c "	synthetic	" .run-unit/unit443-match-base.tmp), g1 synthetic $(grep -c "	synthetic	" .run-unit/unit443-match-g1.tmp), head synthetic $(grep -c "	synthetic	" .run-unit/unit443-match-head.tmp)"
cat .run-unit/unit443-match-base.tmp .run-unit/unit443-match-g1.tmp .run-unit/unit443-match-head.tmp | awk -F'\t' '
  { key = $2 "\t" $5; n[key]++; t[key, n[key]] = $4; g[key, n[key]] = $1; line[$1, key, $4] = $0 }
  END {
    for (k in n) {
      split(k, kk, "\t");
      # cluster by time within 0.3 s
      delete used;
      for (i = 1; i <= n[k]; i++) {
        if (used[i]) continue;
        s = g[k, i] "@" t[k, i]; used[i] = 1;
        for (j = i + 1; j <= n[k]; j++) {
          if (used[j]) continue;
          d = t[k, i] - t[k, j]; if (d < 0) d = -d;
          if (d <= 0.3) { s = s " " g[k, j] "@" t[k, j]; used[j] = 1 }
        }
        print kk[1] "\t" kk[2] "\t" s
      }
    }
  }' | sort
