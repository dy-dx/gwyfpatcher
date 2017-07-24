#!/bin/sh
helper_path="gwyfhelper/bin/Debug/gwyfhelper.dll"
managed_dll_path="$HOME/Library/Application Support/Steam/SteamApps/common/Golf With Your Friends/Golf With Your Friends.app/Contents/Resources/Data/Managed/"
app_path="$HOME/Library/Application Support/Steam/SteamApps/common/Golf With Your Friends/Golf With Your Friends.app"

pkill -f "Golf With Your Friends"
cp "$helper_path" "$managed_dll_path" && open "$app_path"
open steam://run/431240
