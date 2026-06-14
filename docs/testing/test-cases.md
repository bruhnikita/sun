# Test cases - agents

| ID | Scenario | Steps | Expected result |
|---|---|---|---|
| TC-01 | Open list | Start the app | Agent list opens, real imported records are visible, 10 records per page |
| TC-02 | Search | Enter part of agent name, email, or phone | Only matching agents remain; counter changes |
| TC-03 | Sort by name | Select name sort | Agents are ordered by title |
| TC-04 | Sort by discount | Select discount sort | Agents are ordered by calculated discount |
| TC-05 | Sort by priority | Select priority sort | Agents are ordered by Priority |
| TC-06 | Filter by type | Select an agent type | Only agents of selected type are shown; All types resets filter |
| TC-07 | Combined search/sort/filter | Apply filter, then search, then sort | All operations work together |
| TC-08 | Discount calculation | Inspect agents with different sales totals | Discount follows 0/5/10/20/25 percent thresholds |
| TC-09 | Highlight 25 percent | Find agent with discount >= 25 | Card background is light green |
| TC-10 | Mass priority update | Select rows, enter value, click update | Selected Priority values change |
| TC-11 | Add/edit/delete | Use toolbar buttons | Record is added, edited, or deletion is blocked when sales dependency exists |
| TC-12 | Reset data | Click reset | Initial imported demo records return |
