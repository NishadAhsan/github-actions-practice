#!/bin/sh
set -eu

dotnet format "$1" --verify-no-changes
echo "result=Formatting verification passed" >> "$GITHUB_OUTPUT"
