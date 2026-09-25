#!/bin/sh
# unit 432 - copy unit 430's pitch and opening scripts and unit 431's same, renamed.
cd /c/Source/HamLet/.run-unit || exit 1
sed "s/unit430/unit432/g; s/unit 430/unit 432/g" unit430-pitch.sh > unit432-pitch.sh
sed "s/unit430/unit432/g; s/unit 430/unit 432/g" unit430-opening.sh > unit432-opening.sh
echo copied
