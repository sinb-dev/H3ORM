using System.Diagnostics;
using H3ORM.Library.Scaffolding;

namespace H3ORM.Tests;

public class ScaffoldDocumentTests
{
    [Fact]
    public void ScaffoldSourceDocument()
    {
        SqlServerScaffoldingSource source = new();
        var doc = source.GetDocument("H3ORM.Library.Models", "Person");
        File.WriteAllText("Person.cs", doc.GetContent());
        Assert.NotNull(doc);
    }

    [Fact]
    public void SimpleClassGeneration()
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
        const string classContent = @"using System.Text;
namespace Models;

public class Person
{
    public int Id { get; set; } = 0;
}";
        Assert.Equal(classContent, content);
    }
}
