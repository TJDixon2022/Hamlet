# -*- coding: utf-8 -*-
"""Set one or more header keys in a status file, in place.

WHY IT EXISTS

    PROJECT_STATUS.md is 260 KB of carried notes and cannot be read whole by
    an editing tool, so the header keys at the top of it have to be rewritten
    by a program rather than by hand. Writing them by hand is also what
    status-check.py says every fault in these two files came from.

    It touches only the line whose key matches, leaves every other byte of the
    file alone, and never invents a value: the caller supplies each one.

Usage:

    python tools/unit228/status-set.py <file> KEY=value [KEY=value ...]
"""
import io
import sys


def main(argv):
    if len(argv) < 3:
        sys.stderr.write(__doc__)
        return 2

    path = argv[1]
    text = io.open(path, encoding='utf-8').read()
    lines = text.split('\n')

    for pair in argv[2:]:
        if '=' not in pair:
            sys.stderr.write('not a KEY=value pair: %s\n' % pair)
            return 2

        key, value = pair.split('=', 1)

        # A long NOTE does not fit comfortably on a command line, so it is
        # passed as a file. The leading @ is what says so.
        if value.startswith('@'):
            value = io.open(value[1:], encoding='utf-8').read().strip()

        found = False

        for i, line in enumerate(lines):
            if line.startswith(key + ':'):
                lines[i] = key + ': ' + value
                found = True
                break

        if not found:
            sys.stderr.write('key not found: %s\n' % key)
            return 1

        print('    %-20s %s' % (key, value[:70]))

    io.open(path, 'w', encoding='utf-8', newline='\n').write('\n'.join(lines))
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv))
