using System.Diagnostics;
using H3ORM.Library.Scaffolding;

namespace H3ORM.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        ScaffoldDocument doc = new();
        doc.ClassName = "Person";
        doc.Namespace = "Models";
        doc.Usings = "using System.Text;";
        doc.Properties.Add( new()
        {
           Name = "Id",
           DataType = "int",
           DefaultValue = "0", 
        });

        string content = doc.GetContent();
        File.WriteAllText("Person.cs", content);
    }
}
