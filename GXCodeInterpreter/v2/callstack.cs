namespace GXCodeInterpreter;
public class Callstack ()
{
    public List<GXC_CS_ELEMENT> CS { get; set; } = [];
}

public class GXC_CS_ELEMENT(int id)
{
    public int ID = id;
    public List<string> Lines { get; set; } = [];
}
public class GXC_CS_ENTRYPOINT(int id) : GXC_CS_ELEMENT(id) {}
public class GXC_CS_IF(int id, string condition) : GXC_CS_ELEMENT(id)
{
    public string Condition { get; set; } = condition;
}
public class GXC_CS_ELSE_IF(int id, string condition) : GXC_CS_ELEMENT(id)
{
    public string Condition { get; set; } = condition;
}
public class GXC_CS_ELSE(int id) : GXC_CS_ELEMENT(id) {}
public class GXC_CS_SWITCH(int id, string var) : GXC_CS_ELEMENT(id)
{
    public string Variable { get; set; } = var;
}
public class GXC_CS_CASE(int id, string val) : GXC_CS_ELEMENT(id)
{
    public string Value { get; set; } = val;
}
public class GXC_CS_REPEAT(int id, string var) : GXC_CS_ELEMENT(id)
{
    public string Variable { get; set; } = var;
}
public class GXC_CS_ITERATE(int id, string var) : GXC_CS_ELEMENT(id)
{
    public string Variable { get; set; } = var;
}
public class GXC_CS_WHILE(int id, string condition) : GXC_CS_ELEMENT(id)
{
    public string Condition { get; set; } = condition;
}
public class GXC_CS_CLASS(int id) : GXC_CS_ELEMENT(id) {}
public class GXC_CS_METHOD(int id) : GXC_CS_ELEMENT(id) {}