"""Render Chop Wars' original 16-bar jungle groove using only the Python standard library.

Run from any directory: python3 Tools/generate_jungle_music.py
All notes and percussion are synthesized here; no third-party samples are used.
Tails wrap around the loop boundary to avoid a fade-out or gap on repeat.
"""

import array
import math
from pathlib import Path
import random
import sys
import wave


RATE = 22050
BEAT = 60 / 108
LENGTH = round(16 * 4 * BEAT * RATE)
mix = [0.0] * LENGTH
rng = random.Random(108)


def add(start_beat, duration, sound, gain=1.0):
    start = round(start_beat * BEAT * RATE)
    for index in range(round(duration * RATE)):
        mix[(start + index) % LENGTH] += sound(index / RATE) * gain


def note(beat, midi, duration, gain, bass=False):
    frequency = 440 * 2 ** ((midi - 69) / 12)

    def tone(t):
        attack = min(1, t / 0.006)
        release = min(1, max(0, duration - t) / 0.045)
        phase = math.tau * frequency * t
        if bass:
            return attack * release * math.exp(-t / 0.28) * (
                math.sin(phase) + 0.18 * math.sin(2 * phase))
        return attack * release * (
            math.sin(phase) * math.exp(-t / 0.22)
            + 0.28 * math.sin(3.99 * phase) * math.exp(-t / 0.07)
            + 0.09 * math.sin(9.98 * phase) * math.exp(-t / 0.025))

    add(beat, duration, tone, gain)


# C6 / Am7 / Fmaj7 / G6: four related colors, with variations across four phrases.
chords = [(48, 64, 67, 69), (45, 60, 64, 67), (41, 60, 64, 69), (43, 62, 67, 71)]
melodies = [
    [(0.5, 76), (1.25, 79), (2, 81), (3.5, 79)],
    [(0, 76), (1.5, 72), (2.75, 74)],
    [(0.5, 72), (1.25, 76), (2.5, 81), (3.25, 79)],
    [(0, 79), (1.5, 74), (2.5, 71), (3.5, 74)],
]

for bar in range(16):
    beat = bar * 4
    root, third, fifth, sixth = chords[bar % 4]
    for offset, midi in [(0, root), (1.75, root + 12), (2.5, root + 7), (3.5, root + 12)]:
        note(beat + offset, midi, 0.45, 0.24, bass=True)
    for offset, midi in [(0.75, third), (1.5, fifth), (2.75, sixth), (3.5, fifth)]:
        note(beat + offset, midi, 0.75, 0.10)
    for offset, midi in melodies[bar % 4]:
        if bar // 4 == 1 and offset == 0.5:
            midi -= 12
        note(beat + offset, midi, 0.9, 0.16 if bar // 4 != 2 else 0.12)
    if bar in (7, 15):
        note(beat + 3.75, 74 if bar == 7 else 67, 0.5, 0.075)

    for offset in (0, 2):
        add(beat + offset, 0.2, lambda t: min(1, t / 0.003) *
            math.sin(math.tau * (49 * t + 0.75 * (1 - math.exp(-30 * t)))) *
            math.exp(-t * 24), 0.28)
    for offset in (1, 3):
        add(beat + offset, 0.10, lambda t: min(1, t / 0.0015) * math.exp(-t * 65) *
            (math.sin(math.tau * 760 * t) + 0.3 * math.sin(math.tau * 1210 * t)), 0.075)
    for eighth in range(8):
        noise = [rng.uniform(-1, 1) for _ in range(round(0.055 * RATE) + 1)]
        add(beat + eighth / 2, 0.055, lambda t: noise[min(len(noise) - 1, int(t * RATE))] *
            min(1, t / 0.002) * math.exp(-t * 90), 0.035 if eighth % 2 else 0.025)

# A quiet, short echo adds space while retaining a mono, web-friendly asset.
dry = mix[:]
for beats, gain in [(0.75, 0.12), (1.5, 0.045)]:
    delay = round(beats * BEAT * RATE)
    for index, sample in enumerate(dry):
        mix[(index + delay) % LENGTH] += sample * gain

peak = max(abs(sample) for sample in mix)
scale = 0.72 / peak
pcm = array.array('h', (round(sample * scale * 32767) for sample in mix))
if sys.byteorder != 'little':
    pcm.byteswap()
output = Path(__file__).resolve().parents[1] / 'Assets/Resources/JungleGroove.wav'
with wave.open(str(output), 'wb') as audio:
    audio.setnchannels(1)
    audio.setsampwidth(2)
    audio.setframerate(RATE)
    audio.writeframes(pcm.tobytes())

rms = math.sqrt(sum((sample * scale) ** 2 for sample in mix) / LENGTH)
print(f'{output}: {LENGTH / RATE:.2f}s, peak=0.72, RMS={rms:.3f}, loop seam={abs(mix[0] - mix[-1]) * scale:.5f}')
