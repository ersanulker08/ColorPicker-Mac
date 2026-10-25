"""DMG penceresinin arka plan resmini çizer (assets/dmg-background.png ve @2x).

Pencere 660x400 puan; uygulama simgesi solda, Applications kısayolu sağda, ortada ok.
Çalıştırmak için:  python3 tools/make_dmg_background.py   (Pillow gerekir)
"""
import os

from PIL import Image, ImageDraw, ImageFont

W, H = 660, 400
SCALE = 2
S = SCALE

root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
out_dir = os.path.join(root, "assets")
os.makedirs(out_dir, exist_ok=True)

# Simgelerin merkezleri (tools/dmg_settings.py ile aynı olmalı)
APP_CENTER = (170, 190)
APPS_CENTER = (490, 190)

ACCENT = (123, 108, 246)
TEXT_DARK = (38, 36, 66)
TEXT_MUTED = (110, 108, 140)


def font(size, index=0):
    for path in ("/System/Library/Fonts/HelveticaNeue.ttc", "/System/Library/Fonts/Helvetica.ttc"):
        try:
            return ImageFont.truetype(path, size * S, index=index)
        except OSError:
            continue
    return ImageFont.load_default()


img = Image.new("RGB", (W * S, H * S))
px = img.load()
top, bottom = (250, 249, 255), (232, 229, 252)
for y in range(H * S):
    t = y / (H * S - 1)
    row = tuple(int(top[i] + (bottom[i] - top[i]) * t) for i in range(3))
    for x in range(W * S):
        px[x, y] = row

d = ImageDraw.Draw(img)

# Simgelerin arkasına yumuşak yuvarlaklar
for cx, cy in (APP_CENTER, APPS_CENTER):
    r = 82 * S
    d.ellipse(((cx * S - r), (cy * S - r), (cx * S + r), (cy * S + r)), fill=(240, 238, 255))

# Ok: iki simgenin arasında
y = APP_CENTER[1] * S
x1, x2 = 262 * S, 398 * S
width = 7 * S
d.line((x1, y, x2 - 14 * S, y), fill=ACCENT, width=width)
d.ellipse((x1 - width // 2, y - width // 2, x1 + width // 2, y + width // 2), fill=ACCENT)
d.polygon(
    [(x2, y), (x2 - 30 * S, y - 22 * S), (x2 - 30 * S, y + 22 * S)],
    fill=ACCENT,
)


def centered(text, cy, fnt, fill):
    box = d.textbbox((0, 0), text, font=fnt)
    d.text(((W * S - (box[2] - box[0])) / 2, cy * S - (box[3] - box[1]) / 2), text, font=fnt, fill=fill)


centered("Drag ColorPicker into Applications", 52, font(24, 1), TEXT_DARK)
centered("ColorPicker'ı Applications klasörüne sürükle", 82, font(15), TEXT_MUTED)
centered("Not opening? See the README  /  Açılmıyorsa README'ye bak", 366, font(12), TEXT_MUTED)

img.save(os.path.join(out_dir, "dmg-background@2x.png"))
img.resize((W, H), Image.LANCZOS).save(os.path.join(out_dir, "dmg-background.png"))
print("yazıldı: assets/dmg-background.png ve assets/dmg-background@2x.png")
