using H3ORM.Library.Scaffolding;

public class TestScaffoldSource : IScaffoldSource
{
    public ScaffoldDocument GetDocument(string ns, string classname)
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
        return doc;
    }
}