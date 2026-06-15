# Sun

Готовое WPF-приложение с SQL Server для учёта агентов. Приложение работает с базой через ADO.NET-репозитории, а база данных создаётся и заполняется скриптами из папки `database/sqlserver`.

## Быстрый запуск

```powershell
powershell -ExecutionPolicy Bypass -File "database\sqlserver\setup-db.ps1"
dotnet build "src\PopryzhenokAgents.sln"
dotnet run --project "src\PopryzhenokAgents.TestRunner\PopryzhenokAgents.TestRunner.csproj"
dotnet run --project "src\PopryzhenokAgents.App\PopryzhenokAgents.App.csproj"
```

По умолчанию используется экземпляр SQL Server `.\SQLEXPRESS`. Для другого экземпляра задай переменную `SQLSERVER` или полную строку подключения `EXAM_CONNECTION_STRING`.

## Что есть в проекте

- Просмотр, поиск, сортировка, фильтрация и постраничный вывод агентов.
- Добавление, редактирование, удаление и замена логотипа агента.
- Массовое изменение приоритета выбранных агентов.
- Расчёт суммы продаж за последние 365 дней и скидки агента.
- Подсветка агентов со скидкой 25%.
- Просмотр истории продаж продукции.
- Работа с магазинами, историей приоритетов и зависимостями от продаж.
- Документация, тест-кейсы, ERD и диаграмма вариантов использования в папке `docs`.
