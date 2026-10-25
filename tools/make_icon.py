"""Uygulama ikonunu (assets/AppIcon.png) çizer. Sadece Pillow gerekir: pip3 install pillow

Damlalık, uygulamadaki ikonla aynı çizgilerden oluşuyor (App.axaml içindeki DropperIcon).
Çalıştırmak için:  python3 tools/make_icon.py
"""
import math
import os

from PIL import Image, ImageDraw

SIZE = 1024
SCALE = 2  # daha pürüzsüz kenarlar için 2 kat büyük çizip küçültüyoruz
S = SIZE * SCALE

root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
out_path = os.path.join(root, "assets", "AppIcon.png")
os.makedirs(os.path.dirname(out_path), exist_ok=True)

# --- Arka plan: macOS ikon ızgarasına uygun yuvarlak köşeli kare, mordan koyu mora geçiş ---
inset = int(100 * SCALE)
radius = int(185 * SCALE)

gradient = Image.new("RGB", (S, S))
top, bottom = (146, 133, 255), (74, 63, 181)
pixels = gradient.load()
for y in range(S):
    t = y / (S - 1)
    row = tuple(int(top[i] + (bottom[i] - top[i]) * t) for i in range(3))
    for x in range(S):
        pixels[x, y] = row

mask = Image.new("L", (S, S), 0)
ImageDraw.Draw(mask).rounded_rectangle((inset, inset, S - inset, S - inset), radius=radius, fill=255)

img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
img.paste(gradient, (0, 0), mask)

# --- Damlalık: 24x24'lük çizim alanındaki noktalar, 45 derece döndürülüp ortalanıyor ---
k = 30 * SCALE            # 1 birim = 30 piksel
cx = cy = S / 2
width = int(2.1 * k)


def place(x, y):
    dx, dy = x - 12, y - 13
    c, s = math.cos(math.radians(45)), math.sin(math.radians(45))
    return (cx + (dx * c - dy * s) * k, cy + (dx * s + dy * c) * k)


def arc(center, r, start_deg, end_deg, steps=24):
    ccx, ccy = center
    return [
        (ccx + r * math.cos(math.radians(a)), ccy + r * math.sin(math.radians(a)))
        for a in [start_deg + (end_deg - start_deg) * i / steps for i in range(steps + 1)]
    ]


# Her biri tek bir çizgi dizisi
bulb = [(9, 8), (9, 4.5)] + arc((12, 4.5), 3, 180, 360) + [(15, 8)]
collar = [(6.5, 8), (17.5, 8)]
tube = [(9.5, 8), (9.5, 17.5), (12, 22), (14.5, 17.5), (14.5, 8)]
level = [(9.5, 14), (14.5, 14)]

draw = ImageDraw.Draw(img)
for shape in (bulb, collar, tube, level):
    pts = [place(x, y) for x, y in shape]
    draw.line(pts, fill="white", width=width, joint="curve")
    r = width / 2
    for px, py in (pts[0], pts[-1]):  # yuvarlak uçlar
        draw.ellipse((px - r, py - r, px + r, py + r), fill="white")

img = img.resize((SIZE, SIZE), Image.LANCZOS)
img.save(out_path)
print("yazıldı:", out_path)
