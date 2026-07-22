using System.Text.RegularExpressions;

namespace GXCodeInterpreter;

partial class GXCodeInterpreter
{
    public static void ExecuteBlock(GXCodeEnvironment env, GXC_CS_ELEMENT block, Scope? overrideScope = null)
    {
        if (block is not GXC_CS_INIT) GXCodeProgram.scopeStack.Push(new Scope(GXCodeProgram.scopeStack.Peek()));

        string blockName = block.GetType().ToString() + "#" + block.ID;
        
        Scope useScope = overrideScope ?? GXCodeProgram.scopeStack.Peek();

        if (block is GXC_CS_IF ifBlock)
        {
            GXCodeHelper.Debug($"Evaluating IF condition: {ifBlock.Condition}");
            bool isTrue = EvaluateConditions(ifBlock.Condition, blockName);
            if (!isTrue)
            {
                GXCodeHelper.Debug("Condition is false, skipping IF block");
                GXCodeProgram.scopeStack.Pop();
                return;
            }
        }
        else if (block is GXC_CS_ELSE_IF elseIfBlock)
        {
            GXCodeHelper.Debug($"Evaluating ELSE IF condition: {elseIfBlock.Condition}");
            bool isTrue = EvaluateConditions(elseIfBlock.Condition, blockName);
            if (!isTrue)
            {
                GXCodeHelper.Debug("Condition is false, skipping ELSE IF block");
                GXCodeProgram.scopeStack.Pop();
                return;
            }
        }
        else if (block is GXC_CS_SWITCH switchBlock)
        {
            if (!useScope.TryGet(switchBlock.Variable, out var switchVal, out var switchType))
            {
                throw new GXCodeInterpreterError($"Unknown variable {switchBlock.Variable} in switch statement");
            }

            bool caseMatched = false;
            foreach (var line in block.Lines)
            {
                if (line.StartsWith("[BLOCK "))
                {
                    int caseId = int.Parse(Regex.Match(line, @"^\s*\[BLOCK\s+([0-99999999999]+)\]\s*$").Groups[1].Value);
                    if (env.blocks[caseId] is GXC_CS_CASE caseBlock)
                    {
                        string caseValue = caseBlock.Value.Trim();
                        if ((caseValue.StartsWith("\"") && caseValue.EndsWith("\"") && caseValue.Substring(1, caseValue.Length - 2) == switchVal?.ToString()) ||
                            caseValue == switchVal?.ToString())
                        {
                            GXCodeHelper.Debug($"Switch case matched: {caseValue}");
                            ExecuteBlock(env, caseBlock);
                            caseMatched = true;
                            return;
                        }
                    }
                }
            }

            if (!caseMatched)
            {
                GXCodeHelper.Debug("No matching switch case found, skipping switch block");
                GXCodeProgram.scopeStack.Pop();
                return;
            }
        }
        else if (block is GXC_CS_REPEAT repeatBlock)
        {
            // repeatBlock.Variable can be an integer literal or a variable name
            string token = repeatBlock.Variable?.Trim() ?? "";

            if (!int.TryParse(token, out int iterations))
            {
                if (!useScope.TryGet(token, out var repeatVal, out var repeatType))
                {
                    throw new GXCodeInterpreterError($"Unknown variable {token} in repeat statement");
                }
                if (repeatType != "int")
                {
                    throw new GXCodeInterpreterError($"Repeat variable {token} must be of type int");
                }
                if (repeatVal is null)
                {
                    throw new GXCodeInterpreterError($"Variable {token} is null");
                }
                iterations = (int)repeatVal;
            }

            for (int i = 0; i < iterations; i++)
            {
                GXCodeHelper.Debug($"Repeat iteration {i + 1} of {iterations}");
                // create an iteration-local scope
                GXCodeProgram.scopeStack.Push(new Scope(useScope));
                ExecuteBlockBody(env, repeatBlock);
                GXCodeProgram.scopeStack.Pop();
            }
            GXCodeProgram.scopeStack.Pop();
            return;
        }
        else if (block is GXC_CS_ITERATE iterateBlock)
        {
            if (!useScope.TryGet(iterateBlock.Variable, out var iterateVal, out var iterateType))
            {
                throw new GXCodeInterpreterError($"Unknown variable {iterateBlock.Variable} in iterate statement");
            }
            if (iterateType != "str[]" && iterateType != "int[]" && iterateType != "dec[]" && iterateType != "bool[]")
            {
                throw new GXCodeInterpreterError($"Iterate variable {iterateBlock.Variable} must be an array");
            }

            IEnumerable<object> collection = iterateVal switch
            {
                List<string> sList => sList.Cast<object>(),
                List<int> iList => iList.Cast<object>(),
                List<decimal> dList => dList.Cast<object>(),
                List<bool> bList => bList.Cast<object>(),
                _ => throw new GXCodeInterpreterError($"Unsupported iterate variable type {iterateType}")
            };

            foreach (var item in collection)
            {
                GXCodeHelper.Debug($"Iterating item: {item}");
                GXCodeProgram.scopeStack.Push(new Scope(useScope));
                useScope.Set("element", item, iterateType.Substring(0, iterateType.Length - 2));
                ExecuteBlockBody(env, iterateBlock);
                GXCodeProgram.scopeStack.Pop();
            }
            return;
        }
        else if (block is GXC_CS_WHILE whileBlock)
        {
            while (EvaluateConditions(whileBlock.Condition, blockName))
            {
                GXCodeHelper.Debug("While condition is true, executing block");
                GXCodeProgram.scopeStack.Push(new Scope(useScope));
                ExecuteBlockBody(env, whileBlock);
                GXCodeProgram.scopeStack.Pop();
            }
            GXCodeHelper.Debug("While condition is false, exiting block");
            GXCodeProgram.scopeStack.Pop();
            return;
        }

        ExecuteBlockBody(env, block);
        GXCodeProgram.scopeStack.Pop();
    }

    // Execute the lines inside a block (helper extracted to avoid accidental recursion)
    public static void ExecuteBlockBody(GXCodeEnvironment env, GXC_CS_ELEMENT block)
    {
        for (int i = 0; i < block.Lines.Count; i++)
        {
            string line = block.Lines[i];
            ShortLineType type = GetShortLineType(line);
            GXCodeHelper.Debug($"Line {i + 1} of {block.GetType().Name}#{block.ID}: {line} (type: {type})");

            string blockName = $"{block.GetType().Name}#{block.ID}";
            int ri = i+1;

            switch (type)
            {
                case ShortLineType.UNKNOWN:
                    throw new GXCodeInterpreterError($"Undetected indeterminable line structure of {line}");
                case ShortLineType.BUILTIN_OPERATION:
                    ExecuteBuiltinOperation(line, ri);
                    break;
                case ShortLineType.INSTANCE_DECLARATION:
                    DeclareInstance(line, ri, blockName, env);
                    break;
                case ShortLineType.STR_DECLARATION:
                    DeclareStr(line, ri, blockName);
                    break;
                case ShortLineType.CONST_STR_DECLARATION:
                    DeclareConstStr(line, ri, blockName);
                    break;
                case ShortLineType.INT_DECLARATION:
                    DeclareInt(line, ri, blockName);
                    break;
                case ShortLineType.CONST_INT_DECLARATION:
                    DeclareConstInt(line, ri, blockName);
                    break;
                case ShortLineType.DEC_DECLARATION:
                    DeclareDec(line, ri, blockName);
                    break;
                case ShortLineType.CONST_DEC_DECLARATION:
                    DeclareConstDec(line, ri, blockName);
                    break;
                case ShortLineType.BOOL_DECLARATION:
                    DeclareBool(line, ri, blockName);
                    break;
                case ShortLineType.CONST_BOOL_DECLARATION:
                    DeclareConstBool(line, ri, blockName);
                    break;
                case ShortLineType.REX_DECLARATION:
                    DeclareRex(line, ri, blockName);
                    break;
                case ShortLineType.CONST_REX_DECLARATION:
                    DeclareConstRex(line, ri, blockName);
                    break;
                case ShortLineType.ARRAY_DECLARATION:
                    DeclareArray(line, ri, blockName);
                    break;
                case ShortLineType.CONST_ARRAY_DECLARATION:
                    DeclareConstArray(line, ri, blockName);
                    break;
                case ShortLineType.DICT_DECLARATION:
                    DeclareDict(line, ri, blockName);
                    break;
                case ShortLineType.CONST_DICT_DECLARATION:
                    DeclareConstDict(line, ri, blockName);
                    break;
                case ShortLineType.VARIABLE_ASSIGNMENT:
                    AssignVariable(line, ri, blockName);
                    break;
                case ShortLineType.VARIABLE_ARITHMETIC:
                    PerformVariableArithmetic(line, ri, blockName);
                    break;
                case ShortLineType.INCREMENT:
                    IncrementVariable(line, ri, blockName);
                    break;
                case ShortLineType.DECREMENT:
                    DecrementVariable(line, ri, blockName);
                    break;
                case ShortLineType.BLOCK_INDICATOR:
                    int nestedId = int.Parse(Regex.Match(line, @"^\s*\[BLOCK\s+([0-99999999999]+)\]\s*$").Groups[1].Value);
                    ExecuteBlock(env, env.blocks[nestedId]);
                    break;
            }
        }
    }

    public static void ExecuteBuiltinOperation(string line, int lineNr)
        {
            // out
            string outPattern = @"^\s*out\s+(.*);$";
            Match outMatch = Regex.Match(line, outPattern);

            if (outMatch.Success)
            {
                string output = outMatch.Groups[1].Value;

                if (output.StartsWith('"') && output.EndsWith('"'))
                {
                    Console.WriteLine(output.Trim('"'));
                }
                else if (
                    int.TryParse(output, out _) ||
                    decimal.TryParse(output, out _) ||
                    bool.TryParse(output, out _)
                    // ignoring rex for now
                )
                {
                    Console.WriteLine(output);
                }
                else if (GXCodeProgram.scopeStack.Peek().TryGet(output, out object? variableValue, out var type))
                {
                    Console.WriteLine(variableValue);
                }
                else
                {
                    throw new GXCUndeclaredVariableError(lineNr, output, null);
                }
                return;
            }

            // shout
            string shoutPattern = @"^\s*shout\s+(.*);$";
            Match shoutMatch = Regex.Match(line, shoutPattern);

            if (shoutMatch.Success)
            {
                string output = shoutMatch.Groups[1].Value;

                if (GXCodeProgram.scopeStack.Peek().TryGet(output, out object? variableValue, out var type))
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write("[!] ");
                    Console.WriteLine(variableValue);
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write("[!] ");
                    Console.WriteLine(output.Trim('"'));
                    Console.ResetColor();
                }
                return;
            }

            // exit
            string exitPattern = @"^\s*exit;\s*$";
            if (Regex.IsMatch(line, exitPattern)) throw new GXCodeBreak();

            throw new GXCodeInterpreterError("Could not detect built-in operation");
        }
}