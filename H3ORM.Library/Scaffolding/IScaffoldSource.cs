namespace H3ORM.Library.Scaffolding;

public interface IScaffoldSource
{
    ScaffoldDocument GetDocument(string ns, string classname);
}