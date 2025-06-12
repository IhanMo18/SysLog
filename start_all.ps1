$server = Start-Process "dotnet" "run --project SysLog.Api" -PassThru
try {
    dotnet run --project SysLog.Client
} finally {
    if ($server -ne $null) {
        Stop-Process -Id $server.Id
    }
}

