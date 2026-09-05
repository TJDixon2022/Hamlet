import os, struct, sys, datetime

appdata = os.environ.get('APPDATA')
print("APPDATA =", appdata)
root = os.path.join(appdata, 'Hamlet')
print("Hamlet data root =", root, "exists:", os.path.isdir(root))

def listdir(p, label):
    print()
    print("=== %s : %s ===" % (label, p))
    if not os.path.isdir(p):
        print("  DOES NOT EXIST")
        return []
    names = sorted(os.listdir(p))
    if not names:
        print("  EMPTY")
    for n in names:
        full = os.path.join(p, n)
        if os.path.isdir(full):
            print("  [dir]  %s" % n)
        else:
            st = os.stat(full)
            print("  %-48s %10d bytes  mtime %s" % (n, st.st_size,
                  datetime.datetime.utcfromtimestamp(st.st_mtime).strftime('%Y-%m-%d %H:%M:%S') + " UTC"))
    return names

listdir(root, "data root")
cap = os.path.join(root, 'captures')
listdir(cap, "captures")
dig = os.path.join(cap, 'digital')
names = listdir(dig, "captures/digital")
tel = os.path.join(root, 'telemetry')
listdir(tel, "telemetry")

def wavinfo(path):
    with open(path, 'rb') as f:
        data = f.read(4096)
    if data[0:4] != b'RIFF' or data[8:12] != b'WAVE':
        return "not a RIFF/WAVE file"
    pos = 12
    fmt = None
    datasize = None
    while pos + 8 <= len(data):
        cid = data[pos:pos+4]
        csz = struct.unpack('<I', data[pos+4:pos+8])[0]
        if cid == b'fmt ':
            fmt = struct.unpack('<HHIIHH', data[pos+8:pos+8+16])
        elif cid == b'data':
            datasize = csz
            break
        pos += 8 + csz + (csz & 1)
    if fmt is None:
        return "no fmt chunk in first 4096 bytes"
    tag, ch, rate, byterate, align, bits = fmt
    total = os.path.getsize(path)
    if datasize is None:
        datasize = total - 44
    dur = datasize / float(byterate) if byterate else 0.0
    return ("formatTag=%d channels=%d sampleRate=%d bitsPerSample=%d "
            "dataBytes=%d duration=%.3f s" % (tag, ch, rate, bits, datasize, dur))

for n in names:
    full = os.path.join(dig, n)
    if n.lower().endswith('.wav'):
        print()
        print("--- WAV header: %s ---" % n)
        try:
            print("  " + wavinfo(full))
        except Exception as e:
            print("  header read failed: %r" % (e,))

txts = [n for n in names if n.lower().endswith('.txt')]
if txts:
    latest = sorted(txts)[-1]
    print()
    print("=== VERBATIM SHEET: %s ===" % latest)
    with open(os.path.join(dig, latest), 'rb') as f:
        raw = f.read()
    for enc in ('utf-8-sig', 'utf-16', 'latin-1'):
        try:
            print(raw.decode(enc))
            print("(decoded as %s)" % enc)
            break
        except Exception:
            continue
