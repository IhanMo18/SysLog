#!/usr/bin/env bash
set -e

# Start the API server in the background
dotnet run --project SysLog.Api &
server_pid=$!
# Ensure the server is stopped when the script exits
trap "kill $server_pid" EXIT

# Run the Blazor client
dotnet run --project SysLog.Client

