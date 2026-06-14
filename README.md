# Sun

WPF + SQL Server exam-ready application for managing agents. The desktop app uses SQL Server through ADO.NET repositories; the database is created and seeded from `database/sqlserver` scripts.

## Run

```powershell
powershell -ExecutionPolicy Bypass -File "database\sqlserver\setup-db.ps1"
dotnet build "src\PopryzhenokAgents.sln"
dotnet run --project "src\PopryzhenokAgents.TestRunner\PopryzhenokAgents.TestRunner.csproj"
dotnet run --project "src\PopryzhenokAgents.App\PopryzhenokAgents.App.csproj"
```

Default SQL Server instance is `.\SQLEXPRESS`; set `SQLSERVER` or `EXAM_CONNECTION_STRING` for another instance.
