# User guide - agents

1. Run `database/sqlserver/setup-db.ps1` to create and seed SQL Server database.
2. Start the app from `src/PopryzhenokAgents.App`.
3. Use the top search field to search by agent name, email, or phone.
4. Use sorting to order by name, discount, or priority.
5. Use filtering to select one agent type; `Все типы` shows all records.
6. The bottom counter shows how many records are displayed from the filtered total.
7. Use page buttons to move through the list; each page shows 10 records.
8. Discount is calculated from ProductSale rows for the last 365 days.
9. Agents with 25 percent discount are highlighted light green.
10. Select records and use the mass update panel to change priority in SQL Server.
11. Add/Edit opens a form with agent fields and logo replacement.
12. History opens ProductSale records for the selected agent.
13. Delete removes shops/priority history and blocks deletion when product sales exist.
14. Missing logos use `resources/picture.png`.
15. Use `Обновить` to reload records from SQL Server after external database changes.
