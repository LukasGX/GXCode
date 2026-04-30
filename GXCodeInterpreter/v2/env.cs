namespace GXCodeInterpreter;
public class Variable
{
    public string Name { get; init; }
    public object Value { get; set; }
    public string Type { get; init; }  // "str", "int", "bool", etc.
    
    public Variable(string name, object value, string type)
    {
        Name = name;
        Value = value;
        Type = type;
    }
}

public class Scope
{
    public Dictionary<string, Variable> Variables = new(StringComparer.OrdinalIgnoreCase);
    public Scope? Parent = null;
    
    public Scope(Scope? parent = null)
    {
        Parent = parent;
    }
    
    public void Set(string name, object value, string type)
    {
        for (var scope = this; scope != null; scope = scope.Parent)
        {
            if (scope.Variables.TryGetValue(name, out var existing))
            {
                existing.Value = value;
                return;
            }
        }

        // not found in any parent: create in current scope
        Variables[name] = new Variable(name, value, type);
    }
    
    public bool TryGet(string name, out object? value, out string? type)
    {
        for (var scope = this; scope != null; scope = scope.Parent)
        {
            if (scope.Variables.TryGetValue(name, out var variable))
            {
                value = variable.Value;
                type = variable.Type;
                return true;
            }
        }
        
        value = null;
        type = null;
        return false;
    }
    
    public bool HasType(string name, string expectedType)
    {
        return TryGet(name, out _, out var type) && type == expectedType;
    }
}

class GXCodeEnvironment(string code, List<string> lines)
{
    public string Code { get; set; } = code;
    public List<string> Lines { get; set; } = lines;
    public string Namespace { get; set; } = "";
    public Dictionary<int, GXC_CS_ELEMENT> blocks = new();
}