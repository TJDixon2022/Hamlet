cd /c/Source/HamLet
# Decision 16 for piece 10: the target against each out piece alone on the kept state, then the chains.
R=.run-unit
T=$R/unit396-piece-10-4786c7e7.patch
for p in 5-9de394da 7-7fb89d5e 8-1bf4372d 9-44cf3fc8; do
  echo "== with piece $p alone"
  sh $R/unit396-dep.sh $T $R/unit395-piece-$p.patch 2>&1 | grep -v "^$"
done
echo "== with 7 then 8"
sh $R/unit396-dep.sh $T $R/unit395-piece-7-7fb89d5e.patch $R/unit395-piece-8-1bf4372d.patch 2>&1
echo "== with 7 then 9"
sh $R/unit396-dep.sh $T $R/unit395-piece-7-7fb89d5e.patch $R/unit395-piece-9-44cf3fc8.patch 2>&1
echo "== with 7 then 8 then 9"
sh $R/unit396-dep.sh $T $R/unit395-piece-7-7fb89d5e.patch $R/unit395-piece-8-1bf4372d.patch $R/unit395-piece-9-44cf3fc8.patch 2>&1
echo "== tree after"
git status --short -- src
