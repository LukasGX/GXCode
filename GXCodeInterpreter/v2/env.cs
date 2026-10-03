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
}

public class GXCodeEnvironment(string code, List<string> lines)
{
    public string Code { get; set; } = code;
    public List<string> Lines { get; set; } = lines;
    public string Namespace { get; set; } = "";
    public Dictionary<int, GXC_CS_ELEMENT> blocks = new();

    public static Variable? GetVariable(string name)
    {
        foreach (Scope scope in GXCodeProgram.scopeStack)
        {
            if (scope.Variables.TryGetValue(name, out Variable? variable))
                return variable;
        }

        return null;
    }

    [Obsolete("Avoid overrideScope use")]
    public static Variable? GetVariable(string name, Scope overrideScope)
    {
        foreach (var key in overrideScope.Variables.Keys)
        {
            Console.WriteLine(key);
        }

        if (overrideScope.Variables.TryGetValue(name, out Variable? variable))
            return variable;
        else return null;
    }
}