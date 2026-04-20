#!/usr/bin/env bash
# deploy.sh — Build LUA-NAR and copy output to a KSP install
# Usage: ./deploy.sh /path/to/KerbalSpaceProgram

set -e

KSP_DIR="${1}"

if [ -z "$KSP_DIR" ]; then
    echo "Usage: ./deploy.sh /path/to/KerbalSpaceProgram"
    exit 1
fi

if [ ! -d "$KSP_DIR" ]; then
    echo "ERROR: KSP directory not found: $KSP_DIR"
    exit 1
fi

TARGET="$KSP_DIR/GameData/LUA-NAR"
PLUGINS="$TARGET/Plugins"
SCRIPTS="$TARGET/Scripts"

echo "=== LUA-NAR Deploy ==="
echo "KSP path : $KSP_DIR"
echo "Target   : $TARGET"
echo ""

dotnet build LUA-NAR.csproj -c Release

mkdir -p "$PLUGINS" "$SCRIPTS"

cp bin/Release/LUA-NAR.dll "$PLUGINS/"
cp Libs/MoonSharp.Interpreter.dll "$PLUGINS/"

for lua in GameData/LUA-NAR/Scripts/*.lua; do
    [ -f "$lua" ] || continue
    cp "$lua" "$SCRIPTS/"
    echo "Copied: $(basename $lua)"
done

echo ""
echo "=== Deploy complete ==="
echo "Files in $PLUGINS:"
ls -lh "$PLUGINS"
