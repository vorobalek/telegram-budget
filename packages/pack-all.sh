#!/bin/bash

BASEDIR=$(dirname "$0")
SUFFIX="$(date -u +%Y%m%d%H%M)"

echo "$BASEDIR" && \

(rm -rf "$BASEDIR/vorobalek.common" || :) && \
dotnet pack "$BASEDIR/../../common/Common.sln" -o "$BASEDIR/vorobalek.common" -c Release --version-suffix "$SUFFIX" && \

(rm -rf "$BASEDIR/telegram.flow" || :) && \
dotnet pack "$BASEDIR/../../telegram-flow/Telegram.Flow.sln" -o "$BASEDIR/telegram.flow" -c Release --version-suffix "$SUFFIX" && \

(rm -rf "$BASEDIR/tracee" || :) && \
dotnet pack "$BASEDIR/../../tracee/Tracee.sln" -o "$BASEDIR/tracee" -c Release --version-suffix "$SUFFIX" && \

(rm -rf "$BASEDIR/tr.lplus" || :) && \
dotnet pack "$BASEDIR/../../trlplus/TR.LPlus.sln" -o "$BASEDIR/tr.lplus" -c Release --version-suffix "$SUFFIX"