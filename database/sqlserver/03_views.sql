USE [PopryzhenokAgents];
GO
CREATE OR ALTER VIEW dbo.vAgentList AS
SELECT
    a.*,
    at.Title AS AgentTypeTitle,
    ISNULL(s.SalesCount365, 0) AS SalesCount365,
    ISNULL(s.SalesAmount365, 0) AS SalesAmount365,
    CASE
        WHEN ISNULL(s.SalesAmount365, 0) < 10000 THEN 0
        WHEN ISNULL(s.SalesAmount365, 0) < 50000 THEN 5
        WHEN ISNULL(s.SalesAmount365, 0) < 150000 THEN 10
        WHEN ISNULL(s.SalesAmount365, 0) < 500000 THEN 20
        ELSE 25
    END AS DiscountPercent
FROM dbo.Agent a
JOIN dbo.AgentType at ON at.ID = a.AgentTypeID
OUTER APPLY (
    SELECT
        SUM(ps.ProductCount) AS SalesCount365,
        SUM(CAST(ps.ProductCount AS decimal(18,2)) * p.MinCostForAgent) AS SalesAmount365
    FROM dbo.ProductSale ps
    JOIN dbo.Product p ON p.ID = ps.ProductID
    WHERE ps.AgentID = a.ID
      AND ps.SaleDate >= DATEADD(DAY, -365, CAST(GETDATE() AS date))
) s;
GO

