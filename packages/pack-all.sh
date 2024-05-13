#!/bin/bash

BASEDIR=$(dirname "$0")
SUFFIX="$(date -u +%Y%m%d%H%M)"

echo "$BASEDIR" && \

(rm -rf vorobalek.common || :) && \
dotnet pack "$BASEDIR/../../common/Common.sln" -o vorobalek.common -c Release --version-suffix "$SUFFIX" && \

(rm -rf telegram.flow || :) && \
dotnet pack "$BASEDIR/../../telegram-flow/Telegram.Flow.sln" -o telegram.flow -c Release --version-suffix "$SUFFIX" && \

(rm -rf tracee || :) && \
dotnet pack "$BASEDIR/../../tracee/Tracee.sln" -o tracee -c Release --version-suffix "$SUFFIX" && \

(rm -rf tr.lplus || :) && \
dotnet pack "$BASEDIR/../../trlplus/TR.LPlus.sln" -o tr.lplus -c Release --version-suffix "$SUFFIX"