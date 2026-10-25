#!/usr/bin/env bash
# ColorPicker.app ve ColorPicker.dmg üretir (dist/ klasörüne).
#
# Kullanım:   ./tools/build-app.sh [sürüm]        örn.  ./tools/build-app.sh 1.0.0
# İşlemci:    varsayılan osx-arm64 (Apple Silicon). Intel için:  RID=osx-x64 ./tools/build-app.sh
set -euo pipefail

cd "$(dirname "$0")/.."

APP_NAME="ColorPicker"
EXECUTABLE="ColorPickerApp"          # .csproj'daki derleme adı
BUNDLE_ID="com.ersanulker08.colorpicker"
VERSION="${1:-1.0.0}"
RID="${RID:-osx-arm64}"

DIST="dist"
APP="$DIST/$APP_NAME.app"

rm -rf "$DIST"
mkdir -p "$DIST"

echo "==> 1/5  Yayınlanıyor ($RID, .NET dahil)"
dotnet publish ColorPickerApp.csproj -c Release -r "$RID" --self-contained true -o "$DIST/publish"

echo "==> 2/5  .app paketi hazırlanıyor"
mkdir -p "$APP/Contents/MacOS" "$APP/Contents/Resources"
cp -R "$DIST/publish/." "$APP/Contents/MacOS/"
chmod +x "$APP/Contents/MacOS/$EXECUTABLE"

echo "==> 3/5  İkon oluşturuluyor"
ICONSET="$DIST/AppIcon.iconset"
mkdir -p "$ICONSET"
for size in 16 32 128 256 512; do
    sips -z "$size" "$size" assets/AppIcon.png --out "$ICONSET/icon_${size}x${size}.png" > /dev/null
    sips -z "$((size * 2))" "$((size * 2))" assets/AppIcon.png --out "$ICONSET/icon_${size}x${size}@2x.png" > /dev/null
done
iconutil -c icns "$ICONSET" -o "$APP/Contents/Resources/AppIcon.icns"
rm -rf "$ICONSET"

cat > "$APP/Contents/Info.plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>               <string>$APP_NAME</string>
    <key>CFBundleDisplayName</key>        <string>$APP_NAME</string>
    <key>CFBundleIdentifier</key>         <string>$BUNDLE_ID</string>
    <key>CFBundleExecutable</key>         <string>$EXECUTABLE</string>
    <key>CFBundleIconFile</key>           <string>AppIcon</string>
    <key>CFBundlePackageType</key>        <string>APPL</string>
    <key>CFBundleShortVersionString</key> <string>$VERSION</string>
    <key>CFBundleVersion</key>            <string>$VERSION</string>
    <key>LSMinimumSystemVersion</key>     <string>13.0</string>
    <key>NSHighResolutionCapable</key>    <true/>
    <key>NSPrincipalClass</key>           <string>NSApplication</string>
</dict>
</plist>
PLIST

echo "==> 4/5  İmzalanıyor (ad-hoc)"
codesign --force --deep --sign - "$APP"

echo "==> 5/5  DMG oluşturuluyor"
STAGING="$DIST/dmg"
mkdir -p "$STAGING"
cp -R "$APP" "$STAGING/"
ln -s /Applications "$STAGING/Applications"
hdiutil create -volname "$APP_NAME" -srcfolder "$STAGING" -ov -format UDZO "$DIST/$APP_NAME-$VERSION.dmg" > /dev/null
rm -rf "$STAGING" "$DIST/publish"

# Release derlemesi editörün (VS Code) paket listesini değiştirir ve Program.cs'te
# ".WithDeveloperTools" hatası gösterir. Debug ayarına geri dönüyoruz.
dotnet restore ColorPickerApp.csproj > /dev/null

echo
echo "Hazır:"
echo "  $APP"
echo "  $DIST/$APP_NAME-$VERSION.dmg"
