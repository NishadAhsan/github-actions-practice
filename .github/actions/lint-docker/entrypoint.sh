#!/bin/sh
set -eu

dotnet format "$1" --verify-no-changes
