using System.Data.SqlClient;
using Exam.Core;

namespace Exam.Data;

public sealed class AgentRepository
{
    private readonly string connectionString;

    public AgentRepository(string? connectionString = null)
    {
        this.connectionString = connectionString ?? DatabaseSettings.BuildConnectionString("PopryzhenokAgents");
    }

    public List<Agent> Load()
    {
        const string sql = """
            SELECT
                a.ID,
                a.Title,
                a.AgentTypeID,
                at.Title AS AgentTypeTitle,
                ISNULL(a.Address, N'') AS Address,
                a.INN,
                ISNULL(a.KPP, N'') AS KPP,
                ISNULL(a.DirectorName, N'') AS DirectorName,
                a.Phone,
                ISNULL(a.Email, N'') AS Email,
                ISNULL(a.Logo, N'picture.png') AS Logo,
                a.Priority,
                ISNULL(s.SalesCount365, 0) AS SalesCount365,
                ISNULL(s.SalesAmount365, 0) AS SalesAmount365,
                CASE WHEN EXISTS (SELECT 1 FROM dbo.ProductSale ps WHERE ps.AgentID = a.ID) THEN 1 ELSE 0 END AS HasProductSale
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
            ) s
            ORDER BY a.Title;
            """;

        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(sql, connection);
        connection.Open();
        using var reader = command.ExecuteReader();
        var items = new List<Agent>();
        while (reader.Read())
        {
            items.Add(new Agent
            {
                Id = reader.GetInt32("ID"),
                Title = reader.GetString("Title"),
                AgentTypeId = reader.GetInt32("AgentTypeID"),
                Type = reader.GetString("AgentTypeTitle"),
                Address = reader.GetString("Address"),
                Inn = reader.GetString("INN"),
                Kpp = reader.GetString("KPP"),
                Director = reader.GetString("DirectorName"),
                Phone = CleanPrefix(reader.GetString("Phone"), "phone:"),
                Email = CleanPrefix(reader.GetString("Email"), "email:"),
                Logo = reader.GetString("Logo"),
                Priority = reader.GetInt32("Priority"),
                SalesCount365 = Convert.ToInt32(reader["SalesCount365"]),
                SalesAmount365 = Convert.ToDecimal(reader["SalesAmount365"]),
                HasProductSale = Convert.ToInt32(reader["HasProductSale"]) == 1
            });
        }
        return items;
    }

    public List<LookupItem> GetTypes()
    {
        const string sql = "SELECT ID, Title FROM dbo.AgentType ORDER BY Title;";
        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(sql, connection);
        connection.Open();
        using var reader = command.ExecuteReader();
        var result = new List<LookupItem>();
        while (reader.Read())
            result.Add(new LookupItem { Id = reader.GetInt32(0), Title = reader.GetString(1) });
        return result;
    }

    public List<SaleHistoryItem> GetSales(int agentId)
    {
        const string sql = """
            SELECT ps.SaleDate, p.Title, ps.ProductCount, CAST(ps.ProductCount AS decimal(18,2)) * p.MinCostForAgent AS Amount
            FROM dbo.ProductSale ps
            JOIN dbo.Product p ON p.ID = ps.ProductID
            WHERE ps.AgentID=@AgentID
            ORDER BY ps.SaleDate DESC;
            """;
        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@AgentID", agentId);
        connection.Open();
        using var reader = command.ExecuteReader();
        var result = new List<SaleHistoryItem>();
        while (reader.Read())
        {
            result.Add(new SaleHistoryItem
            {
                SaleDate = reader.GetDateTime(0),
                ProductTitle = reader.GetString(1),
                ProductCount = reader.GetInt32(2),
                Amount = reader.GetDecimal(3)
            });
        }
        return result;
    }

    public void Save(Agent agent)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            if (agent.Id == 0)
                Insert(connection, transaction, agent);
            else
                Update(connection, transaction, agent);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void BulkUpdatePriority(IEnumerable<int> agentIds, int priority)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var id in agentIds.Distinct())
            {
                Execute(connection, transaction, "UPDATE dbo.Agent SET Priority=@Priority WHERE ID=@ID;", new SqlParameter("@Priority", priority), new SqlParameter("@ID", id));
                Execute(connection, transaction,
                    "INSERT INTO dbo.AgentPriorityHistory(AgentID, ChangeDate, PriorityValue) VALUES(@ID, GETDATE(), @Priority);",
                    new SqlParameter("@ID", id), new SqlParameter("@Priority", priority));
            }
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public DeleteResult Delete(IEnumerable<Agent> agents)
    {
        var result = new DeleteResult();
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var agent in agents)
            {
                var hasSales = Convert.ToInt32(Scalar(connection, transaction,
                    "SELECT COUNT(*) FROM dbo.ProductSale WHERE AgentID=@ID;",
                    new SqlParameter("@ID", agent.Id))) > 0;
                if (hasSales)
                {
                    result.Blocked++;
                    result.Messages.Add($"Удаление агента '{agent.Title}' запрещено: есть реализации продукции.");
                    continue;
                }

                Execute(connection, transaction, "DELETE FROM dbo.Shop WHERE AgentID=@ID;", new SqlParameter("@ID", agent.Id));
                Execute(connection, transaction, "DELETE FROM dbo.AgentPriorityHistory WHERE AgentID=@ID;", new SqlParameter("@ID", agent.Id));
                Execute(connection, transaction, "DELETE FROM dbo.Agent WHERE ID=@ID;", new SqlParameter("@ID", agent.Id));
                result.Deleted++;
            }
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
        return result;
    }

    private void Insert(SqlConnection connection, SqlTransaction transaction, Agent agent)
    {
        const string sql = """
            INSERT INTO dbo.Agent(Title, AgentTypeID, Address, INN, KPP, DirectorName, Phone, Email, Logo, Priority)
            OUTPUT INSERTED.ID
            VALUES(@Title, @AgentTypeID, @Address, @INN, @KPP, @DirectorName, @Phone, @Email, @Logo, @Priority);
            """;
        agent.Id = Convert.ToInt32(Scalar(connection, transaction, sql,
            new SqlParameter("@Title", agent.Title),
            new SqlParameter("@AgentTypeID", agent.AgentTypeId),
            new SqlParameter("@Address", agent.Address),
            new SqlParameter("@INN", string.IsNullOrWhiteSpace(agent.Inn) ? "0000000000" : agent.Inn),
            new SqlParameter("@KPP", string.IsNullOrWhiteSpace(agent.Kpp) ? DBNull.Value : agent.Kpp),
            new SqlParameter("@DirectorName", agent.Director),
            new SqlParameter("@Phone", agent.Phone),
            new SqlParameter("@Email", agent.Email),
            new SqlParameter("@Logo", string.IsNullOrWhiteSpace(agent.Logo) ? "picture.png" : agent.Logo),
            new SqlParameter("@Priority", agent.Priority)));
    }

    private void Update(SqlConnection connection, SqlTransaction transaction, Agent agent)
    {
        const string sql = """
            UPDATE dbo.Agent
            SET Title=@Title,
                AgentTypeID=@AgentTypeID,
                Address=@Address,
                INN=@INN,
                KPP=@KPP,
                DirectorName=@DirectorName,
                Phone=@Phone,
                Email=@Email,
                Logo=@Logo,
                Priority=@Priority
            WHERE ID=@ID;
            """;
        Execute(connection, transaction, sql,
            new SqlParameter("@ID", agent.Id),
            new SqlParameter("@Title", agent.Title),
            new SqlParameter("@AgentTypeID", agent.AgentTypeId),
            new SqlParameter("@Address", agent.Address),
            new SqlParameter("@INN", string.IsNullOrWhiteSpace(agent.Inn) ? "0000000000" : agent.Inn),
            new SqlParameter("@KPP", string.IsNullOrWhiteSpace(agent.Kpp) ? DBNull.Value : agent.Kpp),
            new SqlParameter("@DirectorName", agent.Director),
            new SqlParameter("@Phone", agent.Phone),
            new SqlParameter("@Email", agent.Email),
            new SqlParameter("@Logo", string.IsNullOrWhiteSpace(agent.Logo) ? "picture.png" : agent.Logo),
            new SqlParameter("@Priority", agent.Priority));
    }

    private static string CleanPrefix(string value, string prefix) =>
        value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? value[prefix.Length..].Trim() : value;

    private static void Execute(SqlConnection connection, SqlTransaction transaction, string sql, params SqlParameter[] parameters)
    {
        using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddRange(parameters);
        command.ExecuteNonQuery();
    }

    private static object Scalar(SqlConnection connection, SqlTransaction transaction, string sql, params SqlParameter[] parameters)
    {
        using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddRange(parameters);
        return command.ExecuteScalar()!;
    }
}

