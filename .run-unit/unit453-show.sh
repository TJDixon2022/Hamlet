#!/bin/sh
# Prints each named file with a header. Usage: sh unit453-show.sh file...
for f in "$@"; do
  echo "=== $f"
  cat "$f"
done
