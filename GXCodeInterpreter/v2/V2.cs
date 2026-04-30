namespace GXCodeInterpreter;
class GXCodeProgram
{
    public static Stack<Scope> scopeStack = new();

    public static void Start(string[] args)
    {
        try
        {
            string? filePath = null;
            GXCodeHelper.DebuggingEnabled = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--debug")
                {
                    GXCodeHelper.DebuggingEnabled = true;
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("DEBUG MODE ENABLED");
                    Console.ResetColor();
                }
                else if (args[i] == "--gxc" && i + 1 < args.Length)
                {
                    filePath = args[i + 1];
                    i++;
                }
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new GXCodeInterpreterError("No file specified. Use --gxc <path> to specify the GXCode file to run.");
            }

            string content = File.ReadAllText(filePath);
            List<string> lines = GXCodeHelper.SplitCode(content);
            GXCodeEnvironment env = new(content, lines);

            scopeStack.Push(new Scope());

            Callstack cs = new();
            List<int> cs_ids = [];
            int lastCSID = -1;

            GXC_CS_ELEMENT? inMem = null;

            bool inMultiLineComment = false;

            // split the code and save into env.blocks
            for (int i = 0; i < env.Lines.Count; i++)
            {
                string line = env.Lines[i];
                int ri = i + 1;
                LineType type = GXCodeInterpreter.GetLineType(line, inMultiLineComment);

                GXCodeHelper.Debug($"Line {ri}: {line} (type: {type})");

                GXC_CS_ELEMENT? last = cs.CS.LastOrDefault();

                switch (type)
                {
                    case LineType.UNKNOWN:
                        throw new GXCIndeterminableLineError(line, ri, null);
                    case LineType.COMMENT:
                        continue;
                    case LineType.MULTILINE_COMMENT_INDICATOR:
                        inMultiLineComment = !inMultiLineComment;
                        continue;
                    case LineType.NEGLIGIBLE:
                        continue;
                    case LineType.NAMESPACE_DEFINITION:
                        if (env.Namespace != "")
                        {
                            throw new GXCMultipleNamespaceError(ri, null);
                        }
                        if (ri != 1)
                        {
                            throw new GXCWrongNamespaceDefinitionError(ri, null);
                        }
                        env.Namespace = GXCodeInterpreter.GetNS(line);
                        break;
                    case LineType.ENTRYPOINT_DEFINITION_START:
                        if (last is not null)
                        {
                            throw new GXCNestedEntrypointError(ri, last.GetType().Name, null);
                        }

                        bool hasEntrypoint = env.blocks.Values.Any(val => val is GXC_CS_ENTRYPOINT);
                        if (hasEntrypoint)
                        {
                            throw new GXCMultipleEntrypointError(ri, null);
                        }

                        GXC_CS_ENTRYPOINT n_entrypoint = new(lastCSID + 1);
                        env.blocks.Add(lastCSID + 1, n_entrypoint);
                        lastCSID += 1;

                        cs.CS.Add(n_entrypoint);
                        cs_ids.Add(n_entrypoint.ID);
                        break;
                    case LineType.IF_START:
                        if (last is null)
                        {
                            throw new GXCStrayBlockError(ri, typeof(GXC_CS_IF).Name, false, null);
                        }
                        else if (last is GXC_CS_CLASS)
                        {
                            throw new GXCStrayBlockError(ri, typeof(GXC_CS_IF).Name, true, null);
                        }

                        string IfCondition = GXCodeInterpreter.GetIfCondition(line);
                        GXC_CS_IF n_if = new(lastCSID + 1, IfCondition);
                        env.blocks.Add(lastCSID + 1, n_if);
                        lastCSID += 1;

                        cs.CS.Add(n_if);
                        cs_ids.Add(n_if.ID);

                        env.blocks[last.ID].Lines.Add($"[BLOCK {n_if.ID}]");
                        break;
                    case LineType.ELSE_IF_START:
                        if (last is null)
                        {
                            throw new GXCStrayBlockError(ri, typeof(GXC_CS_ELSE_IF).Name, false, null);
                        }
                        else if (last is GXC_CS_CLASS)
                        {
                            throw new GXCStrayBlockError(ri, typeof(GXC_CS_ELSE_IF).Name, true, null);
                        }
                        if (inMem is not GXC_CS_IF && inMem is not GXC_CS_ELSE_IF)
                        {
                            throw new GXCStrayElseIfError(ri, null);
                        }

                        string ElseIfCondition = GXCodeInterpreter.GetElseIfCondition(line);
                        GXC_CS_ELSE_IF n_else_if = new(lastCSID + 1, ElseIfCondition);
                        env.blocks.Add(lastCSID + 1, n_else_if);
                        lastCSID += 1;

                        cs.CS.Add(n_else_if);
                        cs_ids.Add(n_else_if.ID);

                        env.blocks[last.ID].Lines.Add($"[BLOCK {n_else_if.ID}]");
                        break;
                    case LineType.ELSE_START:
                        if (last is null)
                        {
                            throw new GXCStrayBlockError(ri, typeof(GXC_CS_ELSE).Name, false, null);
                        }
                        else if (last is GXC_CS_CLASS)
                        {
                            throw new GXCStrayBlockError(ri, typeof(GXC_CS_ELSE).Name, true, null);
                        }
                        if (inMem is not GXC_CS_IF && inMem is not GXC_CS_ELSE_IF)
                        {
                            throw new GXCStrayElseError(ri, null);
                        }

                        GXC_CS_ELSE n_else = new(lastCSID + 1);
                        env.blocks.Add(lastCSID + 1, n_else);
                        lastCSID += 1;
                        
                        cs.CS.Add(n_else);
                        cs_ids.Add(n_else.ID);

                        env.blocks[last.ID].Lines.Add($"[BLOCK {n_else.ID}]");
                        break;
                    case LineType.SWITCH_START:
                        if (last is null)
                        {
                            throw new GXCStrayBlockError(ri, typeof(GXC_CS_SWITCH).Name, false, null);
                        }
                        else if (last is GXC_CS_CLASS)
                        {
                            throw new GXCStrayBlockError(ri, typeof(GXC_CS_SWITCH).Name, true, null);
                        }

                        string switchVar = GXCodeInterpreter.GetSwitchVariable(line);
                        GXC_CS_SWITCH n_switch = new(lastCSID + 1, switchVar);
                        env.blocks.Add(lastCSID + 1, n_switch);
                        lastCSID += 1;

                        cs.CS.Add(n_switch);
                        cs_ids.Add(n_switch.ID);

                        env.blocks[last.ID].Lines.Add($"[BLOCK {n_switch.ID}]");
                        break;
                    case LineType.CASE_START:
                        if (last is null)
                        {
                            throw new GXCStrayBlockError(ri, typeof(GXC_CS_CASE).Name, false, null);
                        }
                        else if (last is GXC_CS_CLASS)
                        {
                            throw new GXCStrayBlockError(ri, typeof(GXC_CS_CASE).Name, true, null);
                        }
                        if (last is not GXC_CS_SWITCH)
                        {
                            throw new GXCStrayCaseError(ri, null);
                        }

                        string caseValue = GXCodeInterpreter.GetCaseValue(line);
                        GXC_CS_CASE n_case = new(lastCSID + 1, caseValue);
                        env.blocks.Add(lastCSID + 1, n_case);
                        lastCSID += 1;

                        cs.CS.Add(n_case);
                        cs_ids.Add(n_case.ID);

                        env.blocks[last.ID].Lines.Add($"[BLOCK {n_case.ID}]");
                        break;
                    case LineType.REPEAT_START:
                        if (last is null)
                        {
                            throw new GXCStrayBlockError(ri, typeof(GXC_CS_REPEAT).Name, false, null);
                        }
                        else if (last is GXC_CS_CLASS)
                        {
                            throw new GXCStrayBlockError(ri, typeof(GXC_CS_REPEAT).Name, true, null);
                        }

                        string repeatVar = GXCodeInterpreter.GetRepeatVariable(line);
                        GXC_CS_REPEAT n_repeat = new(lastCSID + 1, repeatVar);
                        env.blocks.Add(lastCSID + 1, n_repeat);
                        lastCSID += 1;

                        cs.CS.Add(n_repeat);
                        cs_ids.Add(n_repeat.ID);

                        env.blocks[last.ID].Lines.Add($"[BLOCK {n_repeat.ID}]");
                        break;
                    case LineType.ITERATE_START:
                        if (last is null)
                        {
                            throw new GXCStrayBlockError(ri, typeof(GXC_CS_ITERATE).Name, false, null);
                        }
                        else if (last is GXC_CS_CLASS)
                        {
                            throw new GXCStrayBlockError(ri, typeof(GXC_CS_ITERATE).Name, true, null);
                        }

                        string iterateVar = GXCodeInterpreter.GetIterateVariable(line);
                        GXC_CS_ITERATE n_iterate = new(lastCSID + 1, iterateVar);
                        env.blocks.Add(lastCSID + 1, n_iterate);
                        lastCSID += 1;

                        cs.CS.Add(n_iterate);
                        cs_ids.Add(n_iterate.ID);

                        env.blocks[last.ID].Lines.Add($"[BLOCK {n_iterate.ID}]");
                        break;
                    case LineType.WHILE_START:
                        if (last is null)
                        {
                            throw new GXCStrayBlockError(ri, typeof(GXC_CS_WHILE).Name, false, null);
                        }
                        else if (last is GXC_CS_CLASS)
                        {
                            throw new GXCStrayBlockError(ri, typeof(GXC_CS_WHILE).Name, true, null);
                        }

                        string whileCondition = GXCodeInterpreter.GetWhileCondition(line);
                        GXC_CS_WHILE n_while = new(lastCSID + 1, whileCondition);
                        env.blocks.Add(lastCSID + 1, n_while);
                        lastCSID += 1;
                        
                        cs.CS.Add(n_while);
                        cs_ids.Add(n_while.ID);

                        env.blocks[last.ID].Lines.Add($"[BLOCK {n_while.ID}]");
                        break;
                    case LineType.CLOSING:
                        if (last is null)
                        {
                            throw new GXCNothingToCloseError(ri, null);
                        }

                        inMem = last;
                        cs.CS.Remove(last);
                        cs_ids.Remove(last.ID);
                        break;
                    case LineType.BUILTIN_OPERATION:
                        if (last is null)
                        {
                            throw new GXCStrayBuiltinOperationError(ri, false, null);
                        }
                        if (last is GXC_CS_CLASS)
                        {
                            throw new GXCStrayBuiltinOperationError(ri, true, null);
                        }

                        env.blocks[last.ID].Lines.Add(line);
                        break;
                    case LineType.VARIABLE_DECLARATION:
                        if (last is null)
                        {
                            throw new GXCStrayVariableDeclarationError(ri, null);
                        }

                        env.blocks[last.ID].Lines.Add(line);
                        break;
                    case LineType.VARIABLE_ASSIGNMENT:
                        if (last is null)
                        {
                            throw new GXCStrayVariableAssignmentError(ri, null);
                        }

                        env.blocks[last.ID].Lines.Add(line);
                        break;
                    case LineType.VARIABLE_ARITHMETIC:
                        if (last is null)
                        {
                            throw new GXCStrayVariableArithmeticError(ri, null);
                        }

                        env.blocks[last.ID].Lines.Add(line);
                        break;
                }
            }

            // begin with the entrypoint block
            GXC_CS_ENTRYPOINT? entrypoint = env.blocks.Values.FirstOrDefault(val => val is GXC_CS_ENTRYPOINT) as GXC_CS_ENTRYPOINT ?? throw new GXCMissingEntrypointError(null);
            GXCodeInterpreter.ExecuteBlock(env, entrypoint);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Program ran without any errors");
            Console.ResetColor();
        }
        catch (GXCodeError e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error {e.Id}: {e.Message} {(e.LineNr == 0 ? "" : $"at line {e.LineNr}")} {(e.Block == null ? "" : $"of block {e.Block}")}");
            Console.ResetColor();
        }
        catch (GXCodeInterpreterError e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Interpreter Error: {e.Message}");
            Console.ResetColor();
        }
        catch (GXCodeBreak)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Program was ended intentionally");
            Console.ResetColor();
        }
    }
}