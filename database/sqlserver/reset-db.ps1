$ErrorActionPreference = 'Stop'
$server = if ($env:SQLSERVER) { $env:SQLSERVER } else { '.\SQLEXPRESS' }
sqlcmd -S $server -E -b -Q "IF DB_ID(N'PopryzhenokAgents') IS NOT NULL BEGIN ALTER DATABASE [PopryzhenokAgents] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [PopryzhenokAgents]; END"
& $PSScriptRoot\setup-db.ps1
