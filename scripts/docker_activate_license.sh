#!/usr/bin/env bash

set -e

xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' \
  $UNITY_EXE \
    -batchmode \
    -nographics \
    -logFile /dev/stdout \
    -quit \
    -serial "$UNITY_SERIAL" \
    -username "$UNITY_USERNAME" \
    -password "$UNITY_PASSWORD"