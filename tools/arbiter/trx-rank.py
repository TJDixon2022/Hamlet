"""trx-rank.py - rank the tests in a TRX by wall clock.

    python tools/arbiter/trx-rank.py <file.trx> [top-n]

WHY THIS EXISTS. Work instruction 250 had to name the twenty slowest tests in
each suite. The console logger prints a total and nothing else, and the console
logs in this tree are UTF-16, so grepping them as UTF-8 finds nothing and
reports zero - which is how a suite came to have no total in four consecutive
reports. The TRX carries a duration per test and is the only place the
per-test cost is written down.

Python rather than a .bat: this session's shell refuses powershell and refuses
.bat directly, and python runs. It prints, it changes nothing.
"""

import sys
import xml.etree.ElementTree as ET

NS = "{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}"


def seconds(text):
    """TRX durations are HH:MM:SS.fffffff."""
    if not text:
        return 0.0
    h, m, s = text.split(":")
    return int(h) * 3600 + int(m) * 60 + float(s)


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        return 2
    path = sys.argv[1]
    top = int(sys.argv[2]) if len(sys.argv) > 2 else 20
    # A third argument names a substring: print only the tests whose name holds
    # it, with their durations, instead of the ranking. That is how a gate-set
    # candidate's cost is read out of a run that has already happened.
    wanted = sys.argv[3] if len(sys.argv) > 3 else None

    root = ET.parse(path).getroot()
    rows = []
    for r in root.iter(NS + "UnitTestResult"):
        rows.append(
            (
                seconds(r.get("duration")),
                r.get("testName", ""),
                r.get("outcome", ""),
            )
        )

    times = root.find(NS + "Times")
    wall = None
    if times is not None:
        # start/finish are ISO 8601 with an offset.
        from datetime import datetime

        s = datetime.fromisoformat(times.get("start"))
        f = datetime.fromisoformat(times.get("finish"))
        wall = (f - s).total_seconds()

    total = sum(t for t, _, _ in rows)
    rows.sort(key=lambda x: -x[0])

    print(f"file      : {path}")
    print(f"tests     : {len(rows)}")
    print(f"sum of    : {total:.1f} s of measured per-test time")
    if wall is not None:
        print(f"wall clock: {wall:.1f} s (run start to finish, TRX Times)")
    outcomes = {}
    for _, _, o in rows:
        outcomes[o] = outcomes.get(o, 0) + 1
    print("outcomes  : " + ", ".join(f"{k}={v}" for k, v in sorted(outcomes.items())))
    print()

    if wanted:
        picked = [r for r in rows if wanted in r[1]]
        print(f"matching \"{wanted}\": {len(picked)}, {sum(t for t, _, _ in picked):.2f} s together")
        for t, name, outcome in picked:
            print(f"     {t:8.2f} s  {name}  [{outcome}]")
        return 0

    head = sum(t for t, _, _ in rows[:top])
    print(f"top {top} = {head:.1f} s = {100.0 * head / total:.1f}% of measured test time", end="")
    if wall:
        print(f", {100.0 * head / wall:.1f}% of the wall clock")
    else:
        print()
    print()
    for i, (t, name, outcome) in enumerate(rows[:top], 1):
        flag = "" if outcome == "Passed" else f"  [{outcome}]"
        print(f"{i:3d}  {t:8.2f} s  {name}{flag}")

    fails = [r for r in rows if r[2] not in ("Passed", "NotExecuted")]
    if fails:
        print()
        print(f"not passed ({len(fails)}):")
        for t, name, outcome in fails:
            print(f"     {t:8.2f} s  {name}  [{outcome}]")
    return 0


if __name__ == "__main__":
    sys.exit(main())
