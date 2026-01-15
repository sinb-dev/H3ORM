using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace H3ORM.Library.Scaffolding;

public class ScaffoldDocument
{
    public string Usings { get; set; } = "";
    public string Namespace { get; set; } = "";
    public string ClassName { get; set; } = "";
    public List<ScaffoldProperty> Properties { get; set; } = new();
    public string GetContent()
    {
        string propertiesString = "";
        foreach (ScaffoldProperty property in Properties)
        {
            propertiesString += property.ToString()+"\n";
        }
        string documentString = $@"{Usings}
namespace {Namespace};

public class {ClassName}
{{
{propertiesString}
}}";
        return documentString;
    }

    const string ClassTemplate = @"
    {{using}}
    namespace {{namespace}};

    public class {{classname}}
    {
        {{properties}}
    }
    ";
    const string PropertyTemplate = @"{{}}";
}