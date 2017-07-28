# gwyfpatcher

## Setup

1. Install dependencies

    - Install Mono: http://www.mono-project.com/download/

    - See deps/readme.txt

2. Build gwyfhelper

        msbuild gwyfhelper

3. Build gwyfpatcher

        msbuild gwyfpatcher

4. Patch your Assembly-CSharp.dll

        # windows
        managed_dll_path="C:\Program Files (x86)\Steam\steamapps\common\Golf With Your Friends\Golf With Your Friends_Data\Managed"
        # mac
        managed_dll_path="$HOME/Library/Application Support/Steam/SteamApps/common/Golf With Your Friends/Golf With Your Friends.app/Contents/Resources/Data/Managed/"

        cp gwyfpatcher/bin/Debug/gwyfpatcher.exe "$managed_dll_path"
        cp gwyfpatcher/bin/Debug/dnlib.dll "$managed_dll_path"
        cp gwyfhelper/bin/Debug/gwyfhelper.dll "$managed_dll_path"

        mono "$managed_dll_path/gwyfpatcher.exe"


## Developing

You shouldn't need to touch the patcher code.
Just run gwyfpatcher.exe whenever the game gets updated.

Work in the gwyfhelper project and build & copy gwyfhelper.dll to your game dir to test it.

On Mac, Unity logs are here:
`~/Library/Logs/Unity/Player.log`
