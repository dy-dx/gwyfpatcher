#!/bin/sh

# first, npm install -g nodemon

# builds and runs whenever a gwyfpatcher/*.cs file changes
nodemon --watch gwyfpatcher -e cs --exec "msbuild && cd gwyfpatcher/bin/Debug && mono gwyfpatcher.exe"
