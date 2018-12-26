#!/bin/sh

BUILD_SCRIPT="./.build_script/node_modules/upm-template-utils/index.js"

if [ ! -f $BUILD_SCRIPT ]; then
	mkdir -p "./.build_script/node_modules"
	cd .build_script
	npm install upm-template-utils --registry https://api.bintray.com/npm/unity/unity-staging --no-save --loglevel error >/dev/null
	cd ..
fi

if [ -z $1 ]; then
	node $BUILD_SCRIPT
else
	node $BUILD_SCRIPT $*
fi
