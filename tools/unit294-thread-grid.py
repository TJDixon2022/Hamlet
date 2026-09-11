"""Work instruction 294 task 2.

Adds the explicit SlotGrid argument to every existing caller of the three
methods that grew one. Every one of these call sites is an FT8 control, so
every one of them gets SlotGrid.Ft8 - which is what makes "FT8 unchanged" an
assertion in the tests rather than a claim in a report.

Paren-matched rather than regexed, because the calls nest:
Ft8ContactStates.Read(ledger.For(call)!, now) has a close paren inside it.
"""

import io
import sys

CALLS = [
    "Ft8ContactStates.Read(",
    "Ft8ContactStates.ColumnTextFor(",
    ".SlotsSinceHeard(",
    ".SlotsSinceSent(",
]


def close_of(text, open_index):
    """The index of the paren that closes the one at open_index."""
    depth = 0
    i = open_index
    while i < len(text):
        c = text[i]
        if c == '"':
            i += 1
            while i < len(text) and text[i] != '"':
                i += 2 if text[i] == "\\" else 1
        elif c == "(":
            depth += 1
        elif c == ")":
            depth -= 1
            if depth == 0:
                return i
        i += 1
    raise ValueError("unbalanced parentheses from index %d" % open_index)


def thread(path, grid):
    text = io.open(path, encoding="utf-8-sig", newline="").read()
    added = 0
    for call in CALLS:
        at = 0
        while True:
            found = text.find(call, at)
            if found < 0:
                break
            open_index = found + len(call) - 1
            end = close_of(text, open_index)
            text = text[:end] + ", " + grid + text[end:]
            added += 1
            at = end + len(grid) + 4
    if added:
        io.open(path, "w", encoding="utf-8-sig", newline="").write(text)
    print("%-78s %d" % (path, added))
    return added


if __name__ == "__main__":
    grid = sys.argv[1]
    total = sum(thread(p, grid) for p in sys.argv[2:])
    print("threaded %d call sites" % total)
