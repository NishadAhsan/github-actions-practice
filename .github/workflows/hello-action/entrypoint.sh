#!/bin/sh -l

echo "Hello, $INPUT_MY_NAME! This is running inside a Docker container"
echo "greeting=Hello, $INPUT_MY_NAME" >> "$GITHUB_OUTPUT"