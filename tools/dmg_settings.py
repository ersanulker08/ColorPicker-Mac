# dmgbuild ayarları: DMG penceresinin görünümü. tools/build-app.sh tarafından kullanılır.
# Değerler -D ile dışarıdan gelir: app_path, icon_path, background_path
import os

app_path = defines["app_path"]          # noqa: F821  (dmgbuild bu değişkeni kendisi verir)
app_name = os.path.basename(app_path)

format = "UDZO"
size = None

files = [app_path]
symlinks = {"Applications": "/Applications"}

icon = defines["icon_path"]             # noqa: F821  DMG diskinin simgesi
background = defines["background_path"]  # noqa: F821

window_rect = ((200, 120), (660, 432))  # (konum), (genişlik, yükseklik). 432 = 400 (arka plan) + 32 (başlık çubuğu)
icon_size = 128
text_size = 13

icon_locations = {
    app_name: (170, 190),
    "Applications": (490, 190),
}

show_status_bar = False
show_tab_view = False
show_toolbar = False
show_pathbar = False
show_sidebar = False
default_view = "icon-view"
include_icon_view_settings = "auto"
arrange_by = None
