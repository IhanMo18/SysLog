# SysLog

This solution includes a Blazor WebAssembly client (`SysLog.Client`) and an API server (`SysLog.Api`).

## Running both projects

Use the provided helper script to start the API server and Blazor client together.

On Unix-like systems:

```bash
./start_all.sh
```

On Windows PowerShell:

```powershell
./start_all.ps1
```

The script starts `SysLog.Api` in the background and then runs the Blazor client. When the client stops, the server process is terminated.

