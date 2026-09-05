"""Rewrite a file with CRLF line endings, in place.

    python tools/arbiter/to-crlf.py <file>

WHY. cmd.exe resumes a running batch file by byte offset, and a .bat written
with LF endings can send a running loop into the middle of a line. Every other
.bat in tools/arbiter is CRLF; gate-set.bat was written LF by the session that
created it. This is the one-line fix, kept because the same thing will happen
again the next time a session writes a .bat with a file-editing tool.
"""

import sys


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        return 2
    path = sys.argv[1]
    with open(path, "rb") as f:
        data = f.read()
    data = data.replace(b"\r\n", b"\n").replace(b"\n", b"\r\n")
    with open(path, "wb") as f:
        f.write(data)
    print(f"{path}: {data.count(b'\r\n')} CRLF lines, {len(data)} bytes")
    return 0


if __name__ == "__main__":
    sys.exit(main())
