#!/bin/sh

# first, npm install -g nodemon && gem install terminal-notifier

# builds and runs whenever a gwyfpatcher/*.cs file changes
# nodemon --watch gwyfpatcher -e cs --exec "msbuild && cd gwyfpatcher/bin/Debug && mono gwyfpatcher.exe"

# we helper now
nodemon --watch gwyfhelper -e cs --exec "msbuild gwyfhelper && ./postbuildhelper.sh"

# mv gwyfpatcher/bin/Assembly-CSharp.dll gwyfpatcher/bin/Assembly-CSharp.original.dll
# mono packages/ILRepack.2.0.13/tools/ILRepack.exe /targetplatform:v2 /lib:gwyfpatcher/bin /out:gwyfpatcher/bin/Assembly-CSharp.dll gwyfpatcher/bin/Assembly-CSharp.original.dll deps/unity-python/IronPython-2.6.2/*.dll

# subl ~/Library/Logs/Unity/Player.log
# or on windows:
# _EXECNAME_Data_\output_log.txt
# %USERPROFILE%\AppData\LocalLow\CompanyName\ProductName\output_log.txt
