using System.Reflection;
using System.Text;
using Microsoft.Data.Sqlite;
namespace H3ORM.ORM;

public static class ORM
{
    static string connectionString = "";
    public static void SetConnectionString(string connStr) => connectionString = connStr;

    static void ExecuteStatement(string sql)
    {
        using SqliteConnection connection = new(connectionString);
        connection.Open();
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    public static bool Insert(object o)
    {
        Type type = o.GetType();
        StringBuilder sql = new();
        List<string> columns = new();
        List<string> values = new();
        Dictionary<string, object?> parameters = new();
        foreach (PropertyInfo propertyInfo in type.GetProperties())
        {
            columns.Add(propertyInfo.Name);
            values.Add($"@{propertyInfo.Name}");
            parameters.Add(propertyInfo.Name, propertyInfo.GetValue(o)?.ToString() ?? "");
        }
        sql.Append($"INSERT INTO {type.Name}");
        sql.Append($"(`{string.Join("`,`",columns)}`)");
        sql.Append("VALUES ");
        sql.Append($"({string.Join(",", values)})");
        ExecuteStatement(sql.ToString(), parameters);
        return true;
    }
    /// <summary>
    /// Creates a table of the given type
    /// </summary>
    /// <param name="type"></param>
    /// <returns>Returns true if table is created</returns>
    public static bool CreateTable(Type type)
    {
        string sql = GetCreateTableSQL(type);
        try
        {
            ExecuteStatement(sql);
            return true;
        } 
        catch (Exception e)
        {
            Console.WriteLine($"Could not create table for '{type.Name}'. Unhandled exception {e}");    
        }
        return false;
    }
    /// <summary>
    /// Generates CREATE TABLE sql by reflecting a type
    /// </summary>
    /// <param name="type">The type to reflect and create table</param>
    /// <returns>SQL Create Table Statement</returns>
    static string GetCreateTableSQL(Type type)
    {
        StringBuilder sql = new StringBuilder();
        sql.Append($@"CREATE TABLE IF NOT EXISTS ""{type.Name}"" (");
        
        PropertyInfo[] properties = type.GetProperties();
        string primaryKeyColumn = "";
        foreach (PropertyInfo info in properties)
        {
            //Look for primary key
            if (IsPrimaryKeyCandidate(info.Name))
            {
                primaryKeyColumn = info.Name;
            }
            string sqlDataType = ToSqlType(info.PropertyType);
            sql.Append(@$" ""{info.Name}"" {sqlDataType}, ");
        }

        sql.Append($@"    PRIMARY KEY({primaryKeyColumn} AUTOINCREMENT)
        );");
        return sql.ToString();
    }
    /// <summary>
    /// Examines property name and returns true if name seems like a primary key
    /// </summary>
    /// <param name="columnName">The name of the column or property</param>
    /// <returns>true if the column looks like a primary key, false if not</returns>
    static bool IsPrimaryKeyCandidate(string columnName)
    {
        return columnName.ToLower() == "id" ||
                columnName.StartsWith(columnName)
                && columnName.ToLower().EndsWith("id");
    }
    static string ToSqlType(Type type)
    {
        if (type == typeof(string))
            return "TEXT";
        else if (type == typeof(int))
            return "INTEGER";
        else if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
        {
            return "DECIMAL";
        }
        throw new NotImplementedException();
    }
}