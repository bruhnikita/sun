# Test cases - agents

| ID | Scenario | Steps | Expected result |
|---|---|---|---|
| TC-01 | Database setup | Run `setup-db.ps1` | SQL Server database is created and seeded |
| TC-02 | Open list | Start the app after DB setup | Agent list opens from SQL Server, 10 records per page |
| TC-03 | Search | Enter part of agent name, email, or phone | Only matching agents remain; counter changes |
| TC-04 | Sort by name | Select name sort | Agents are ordered by title |
| TC-05 | Sort by discount | Select discount sort | Agents are ordered by calculated discount |
| TC-06 | Sort by priority | Select priority sort | Agents are ordered by Priority |
| TC-07 | Filter by type | Select an agent type | Only agents of selected type are shown; All types resets filter |
| TC-08 | Discount calculation | Inspect agents with different sales totals | Discount follows 0/5/10/20/25 percent thresholds |
| TC-09 | Highlight 25 percent | Find agent with discount >= 25 | Card background is light green |
| TC-10 | Mass priority update | Select rows, enter value, click update | Selected Priority values are updated in SQL Server and history rows are added |
| TC-11 | Add/edit agent | Use Add or Edit and save | Agent row is saved in SQL Server |
| TC-12 | Product sale history | Select agent and click History | ProductSale rows are displayed |
| TC-13 | Replace logo | Use logo replacement in edit form | Selected file is copied to resources/images and relative path is saved |
| TC-14 | Delete rule | Delete agent with ProductSale dependency | Deletion is blocked; agents without sales delete shops/priority history first |
