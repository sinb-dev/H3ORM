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
                    if (property.PropertyType == typeof(string))
                    {
                        property.SetValue(instance, dataReader.GetString(dataReaderColumn));
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        property.SetValue(instance, dataReader.GetInt32(dataReaderColumn));
                    }
                }
                list.Add(instance);
            }
        }
        return list;
    }
    static void ExecuteStatement(string sql)
    {
        using SqliteConnection connection = new(connectionString);
        connection.Open();
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
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


    public static List<T> Select<T>()
    {
        Type type = typeof(T);
        StringBuilder sql = new();
        PropertyInfo[] properties = type.GetProperties();
        string propertyList = string.Join("`,`",properties.Select(p => p.Name).ToList());
        sql.Append($"SELECT `{propertyList}` FROM {type.Name}");
        return ExecuteQuery<T>(sql.ToString());

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