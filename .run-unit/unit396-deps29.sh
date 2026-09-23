cd /c/Source/HamLet
R=.run-unit
P1=$R/unit395-piece-1-2068f868.patch
P3=$R/unit395-piece-3-3e84ac74.patch
P12R=$R/unit396-piece-12-resolved.patch
P18=$R/unit396-piece-18-b48d1158.patch
P19=$R/unit396-piece-19-0f2089f3-nochoice.patch
sh $R/unit396-deps.sh $R/unit396-piece-29-efcd5242.patch "" "$P3" "$P12R" "$P19" "$P3 $P12R" "$P1 $P3 $P12R $P18"
