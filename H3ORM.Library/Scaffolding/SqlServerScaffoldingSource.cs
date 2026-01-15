using System.Data;
using Microsoft.Data.SqlClient;

namespace H3ORM.Library.Scaffolding;

public class SqlServerScaffoldingSource : IScaffoldSource
{
    public ScaffoldDocument GetDocument(string ns, string classname)
    {
        ScaffoldDocument document = new();
        document.Namespace = ns;
        document.ClassName = classname;
        using SqlConnection connection = new SqlConnection(File.ReadAllText("connection_string.txt"));
        connection.Open();

        SqlCommand command = connection.CreateCommand();
        command.CommandText = $"SELECT [Column_Name], [Data_Type] FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{classname}'";
        SqlDataReader reader = command.ExecuteReader();
        
        while (reader.Read())
        {
            ScaffoldProperty property = new();
            property.Name = reader.GetString(0);
            switch (reader.GetString(1))
            {
                case "varchar":
                    property.DataType = "string";
                    break;
                case "int": 
                    property.DataType = "int";
                    break;
            }
            document.Properties.Add(property);
        }
        return document;
    }
}