#!/bin/sh
# Prints each named file with a header. Usage: sh unit450-show.sh file...
for f in "$@"; do
  echo "=== $f"
  cat "$f"
done
