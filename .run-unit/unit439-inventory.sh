# prints file|class|method|attribute for every test method ([...Fact] or [...Theory]) in the given files
for f in "$@"; do
awk -v F="$f" '
/^[[:space:]]*(public |internal )?(sealed |static |abstract |partial )*class [A-Za-z0-9_]+/ { match($0,/class [A-Za-z0-9_]+/); cls=substr($0,RSTART+6,RLENGTH-6) }
/^[[:space:]]*\[[A-Za-z]*(Fact|Theory)/ { pend=1; match($0,/\[[A-Za-z]*/); attr=substr($0,RSTART+1,RLENGTH-1); next }
pend && /\(/ && /(public|private|internal)/ { l=$0; sub(/\(.*/,"",l); n=split(l,a,/[[:space:]]+/); print F"|"cls"|"a[n]"|"attr; pend=0 }
' "$f"
done
