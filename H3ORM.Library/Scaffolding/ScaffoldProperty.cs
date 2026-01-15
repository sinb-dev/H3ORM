namespace H3ORM.Library.Scaffolding;

public class ScaffoldProperty
{
    public string DataType { get; set; } = "";
    public string Name { get; set; } = "";
    public string AccessModifier { get; set; } = "public";
    public string DefaultValue { get; set; } = "";
    public bool IsNullable { get; set; } = false;

    public override string ToString()
    {
        string defaultValue = DefaultValue;
        if (DataType == "string")
        {
            defaultValue = $"\"{DefaultValue}\"";
        }
        if (string.IsNullOrEmpty(defaultValue))
        {
            if (IsNullable) defaultValue = "null";
            else defaultValue = "\"\"";
        }
            
        return $"    {AccessModifier} {DataType} {Name} {{ get; set; }} = {defaultValue};";
    }
}