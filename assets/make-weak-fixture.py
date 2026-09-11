"""Make the one weak-signal fixture work instruction 314 task 5 asks for.

**IT USES THE AUTHOR'S OWN MODEM, IT DOES NOT REIMPLEMENT IT.** The convention
in `fixtures/README.md` is the thing the demodulator was built to match, and a
second implementation of it here would be a second convention that could drift
from the first. This imports `reference-modem.py` and calls it.

Run from this directory, because that module reads `varicode.csv` beside itself:

    python make-weak-fixture.py

**FACT-004: made on a machine with no radio, so the result is an indication and
never a finding.** The seed is fixed so the file can be made again byte for byte.
"""

import hashlib
import importlib.util
import json
import os
import wave

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))

# **-10 dB IN 2500 Hz, WHICH IS ABOUT +9 dB IN PSK31's OWN 31 Hz.** One rung
# below the weakest fixture that already exists, and still not weak-signal work:
# that wants real off-air audio, which only the operator can record.
SNR_DB_2500 = -10.0
CARRIER_HZ = 1000.0
SEED = 314

NAME = "psk31-snr-10db-1000hz.wav"


def load_modem():
    """The author's modem, imported rather than copied."""
    path = os.path.join(HERE, "reference-modem.py")

    spec = importlib.util.spec_from_file_location("reference_modem", path)
    module = importlib.util.module_from_spec(spec)

    # The module reads varicode.csv from the working directory.
    was = os.getcwd()
    os.chdir(HERE)

    try:
        spec.loader.exec_module(module)
    finally:
        os.chdir(was)

    return module


def main():
    modem = load_modem()

    with open(os.path.join(HERE, "fixtures", "qso-text.txt"), "rb") as handle:
        text = handle.read().decode("latin-1")

    rng = np.random.default_rng(SEED)

    bits = modem.bits_for(text)
    clean = modem.modulate(bits, CARRIER_HZ, rng=rng)
    noisy = modem.add_noise(clean, SNR_DB_2500, rng)

    out = os.path.join(HERE, "fixtures", NAME)

    modem.write_wav(out, noisy)

    with open(out, "rb") as handle:
        digest = hashlib.sha256(handle.read()).hexdigest()

    with wave.open(out, "rb") as handle:
        seconds = handle.getnframes() / handle.getframerate()

    entry = {
        "file": NAME,
        "sha256": digest,
        "seconds": round(seconds, 2),
        "carrier_hz": CARRIER_HZ,
        "note": (
            "AWGN, SNR -10 dB referenced to 2500 Hz, about +9 dB in PSK31's own "
            "31 Hz. Made by assets/make-weak-fixture.py with seed "
            + str(SEED)
            + " for work instruction 314 task 5. A measurement, not a gate: no "
            "character error rate is asserted against it."
        ),
        "text": text,
        "reference_cer": None,
        "reference_decode_len": None,
    }

    manifest_path = os.path.join(HERE, "fixtures", "manifest.json")

    with open(manifest_path, encoding="utf-8") as handle:
        manifest = json.load(handle)

    manifest = [row for row in manifest if row.get("file") != NAME]
    manifest.append(entry)

    with open(manifest_path, "w", encoding="utf-8", newline="\n") as handle:
        json.dump(manifest, handle, indent=2)
        handle.write("\n")

    print(NAME, digest, round(seconds, 2), "s")


if __name__ == "__main__":
    main()
