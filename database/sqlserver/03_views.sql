CREATE OR ALTER VIEW dbo.vAgentList AS SELECT a.*, at.Title AS AgentTypeTitle, 0 AS Sales365, 0 AS DiscountPercent FROM dbo.Agent a JOIN dbo.AgentType at ON at.ID=a.AgentTypeID;
GO
