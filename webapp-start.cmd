start "PreProcessor" cmd /k "cd processors\PreProcessor\PreProcessor && dotnet run"
start "Processor" cmd /k "cd processors\Processor\Processor && dotnet run"
start "PostProcessor" cmd /k "cd processors\PostProcessor\PostProcessor && dotnet run"
start "HealthMonitor" cmd /k "cd HealthMonitor\HealthMonitor && dotnet run"