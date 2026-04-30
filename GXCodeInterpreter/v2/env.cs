using System.Runtime.CompilerServices;

namespace GXCodeInterpreter;
public class Variable(string name, object value, string type, bool isConst)
{
    public string Name { get; init; } = name;
    public object Value { get; set; } = value;
    public string Type { get; init; } = type;
    public bool IsConstant { get; init; } = isConst;
}

public class Scope
{
    public Dictionary<string, Variable> Variables = new(StringComparer.OrdinalIgnoreCase);
    public Scope? Parent = null;
    
    public Scope(Scope? parent = null)
    {
        Parent = parent;
    }
    
    public void Set(string name, object value, string type, bool isConst = false)
    {
        for (var scope = this; scope != null; scope = scope.Parent)
        {
            if (scope.Variables.TryGetValue(name, out var existing))
            {
                existing.Value = value;
                return;
            }
        }

        Variables[name] = new Variable(name, value, type, isConst);
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