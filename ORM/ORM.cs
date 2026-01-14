using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.Data.Sqlite;
namespace H3ORM.ORM;

public static class ORM
{
    static string connectionString = "";
    public static void SetConnectionString(string connStr) => connectionString = connStr;
    public static List<T> ExecuteQuery<T>(string sql)
    {
        using SqliteConnection connection = new(connectionString);
        connection.Open();
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        SqliteDataReader dataReader = command.ExecuteReader();

        List<T> list = new();
        while (dataReader.Read())
        {
            T? instance = (T?) Activator.CreateInstance(typeof(T));
            if (instance != null)
            {
                PropertyInfo[] properties = instance.GetType().GetProperties();
                int dataReaderColumn = 0;
                foreach (PropertyInfo property in properties)
                {
                    Type sqlFieldType = dataReader.GetFieldType(dataReaderColumn);
                    if (property.PropertyType != sqlFieldType)
                        throw new Exception($"Property {property.Name} does not match datatype {dataReader.GetFieldType(dataReaderColumn)}");
                    
                    if (dataReader.IsDBNull(dataReaderColumn))
                        continue;
                    
                    if (property.PropertyType == typeof(string))
                    {
                        property.SetValue(instance, dataReader.GetString(dataReaderColumn));
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        property.SetValue(instance, dataReader.GetInt32(dataReaderColumn));
                    }
                    else if (property.PropertyType == typeof(long))
                    {
                        property.SetValue(instance, dataReader.GetInt64(dataReaderColumn));
                    }
                    dataReaderColumn++;
                }
                list.Add(instance);
            }
        }
        return list;
    }
    static object? ExecuteStatement(string sql, Dictionary<string, object?>? parameters = null)
    {
        using SqliteConnection connection = new(connectionString);
        connection.Open();
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        if (parameters != null)
        {
            foreach ((string column, object? value) in parameters)
            {
                command.Parameters.AddWithValue(column, value);
            }
        }
        return command.ExecuteScalar();
    }

    public static bool Insert(object o)
    {
        Type type = o.GetType();
        StringBuilder sql = new();
        List<string> columns = new();
        List<string> values = new();
        Dictionary<string, object?> parameters = new();
        PropertyInfo? primaryKeyProperty = null;
        foreach (PropertyInfo propertyInfo in type.GetProperties())
        {
            if (IsPrimaryKeyCandidate(propertyInfo.Name) && primaryKeyProperty == null)
            {
                primaryKeyProperty = propertyInfo;
                continue; //Avoid setting primary key, let DB set it.
            }
            columns.Add(propertyInfo.Name);
            values.Add($"@{propertyInfo.Name}");
            parameters.Add(propertyInfo.Name, propertyInfo.GetValue(o)?.ToString() ?? "");
        }
        sql.Append($"INSERT INTO {type.Name}");
        sql.Append($"(`{string.Join("`,`",columns)}`)");
        sql.Append(" VALUES ");
        sql.Append($"({string.Join(",", values)})");
        ExecuteStatement(sql.ToString(), parameters);

        if (primaryKeyProperty != null)
        {
            if (ExecuteStatement("SELECT last_insert_rowid()") is long lastInsertId)
            {
                primaryKeyProperty.SetValue(o, lastInsertId);
            }
        }
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

    public static List<T> Select<T>(string where)
    {
        Type type = typeof(T);
        StringBuilder sql = new();
        PropertyInfo[] properties = type.GetProperties();
        List<string> columns = properties.Select(p => p.Name).ToList();

        string propertyList = string.Join("`,`", columns);
        sql.Append($"SELECT `{propertyList}` FROM {type.Name} ");
        sql.Append($"WHERE {where}");

        return ExecuteQuery<T>(sql.ToString());
    }
    public static List<T> Select<T>(Expression<Func<T, bool>> expr)
    {
        Type type = typeof(T);
        StringBuilder sql = new();
        PropertyInfo[] properties = type.GetProperties();
        List<string> columns = properties.Select(p => p.Name).ToList();

        string propertyList = string.Join("`,`", columns);
        sql.Append($"SELECT `{propertyList}` FROM {type.Name} ");
        sql.Append(Where(expr));
        return ExecuteQuery<T>(sql.ToString());
    }
    public static string Where(string str)
    {
        return $"WHERE {str}";
    }
    public static string Where<T>(Expression<Func<T, bool>> expr)
    {
        string sql = "WHERE ";
        if (expr.Body is BinaryExpression binaryExpression)
        {
            sql += binaryExpression.Left;
            if (binaryExpression.NodeType == ExpressionType.Equal)
            {
                sql += "=";
            } 
            else if (binaryExpression.NodeType == ExpressionType.NotEqual)
            {
                sql += "!=";   
            }
            else if (binaryExpression.NodeType == ExpressionType.LessThan)
            {
                sql += "<";   
            }
            sql += binaryExpression.Right;
        }

        return sql;
    }
    public static void Update(object record)
    {
        StringBuilder sql = new();
        Type type = record.GetType();
        Dictionary<string, object?> parameters = new();
        PropertyInfo? primaryKeyProperty = null;
        sql.Append($"UPDATE {type.Name} SET ");
        foreach (PropertyInfo propertyInfo in type.GetProperties())
        {
            if (IsPrimaryKeyCandidate(propertyInfo.Name) && primaryKeyProperty == null)
                primaryKeyProperty = propertyInfo;
            sql.Append($"`{propertyInfo.Name}`=@{propertyInfo.Name},");
            parameters.Add(propertyInfo.Name, propertyInfo.GetValue(record));
        }
        sql = new(sql.ToString().Trim(',', ' '));
        if (primaryKeyProperty == null)
            throw new Exception($"Cannot update: Missing primary key on type {type.Name}");
    
        sql.Append($"WHERE {primaryKeyProperty.Name}=@Id");
        ExecuteStatement(sql.ToString(), parameters);
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
        else if (type == typeof(int) || type == typeof(long) || type == typeof(short))
            return "INTEGER";
        else if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
        {
            return "DECIMAL";
        }
        throw new NotImplementedException();
    }
}