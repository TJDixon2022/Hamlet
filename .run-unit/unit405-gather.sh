cd /c/Source/HamLet/.run-unit
# Gathers the printer's rows from the task 1 runs into unit405-trace.txt.
{
  echo "unit 405 task 1 - TheSixRedsTraceTests, gathered $(date '+%Y-%m-%dT%H:%M:%S%z')"
  for f in trace-b trace-d trace-c trace-b12 trace-b-after
  do
    echo
    echo "===== unit405-$f.txt"
    grep -E "^\s+(B|B-read|C|C-span|C-char|D|D-mix|D-read|D-grid|D-lattice|D-first|D-settled) \|" "unit405-$f.txt"
  done
} > unit405-trace.txt
wc -l unit405-trace.txt
