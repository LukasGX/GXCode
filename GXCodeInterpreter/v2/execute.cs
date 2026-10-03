using System.Text.RegularExpressions;

namespace GXCodeInterpreter;

partial class GXCodeInterpreter
{
    public static void ExecuteBlock(GXCodeEnvironment env, GXC_CS_ELEMENT block, Scope? overrideScope = null)
    {
        if (block is not GXC_CS_INIT) GXCodeProgram.scopeStack.Push(new Scope(GXCodeProgram.scopeStack.Peek()));

        string blockName = block.GetType().ToString() + "#" + block.ID;

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
            Variable? variable = GXCodeEnvironment.GetVariable(switchBlock.Variable)
                ?? throw new GXCodeInterpreterError($"Unknown variable {switchBlock.Variable} in switch statement");
            var switchValue = variable.Value;

            GXC_CS_DEFAULT? defaultBlock = null;

            bool caseMatched = false;
            foreach (var line in block.Lines)
            {
                if (line.StartsWith("[BLOCK "))
                {
                    int caseId = int.Parse(Regex.Match(line, @"^\s*\[BLOCK\s+([0-99999999999]+)\]\s*$").Groups[1].Value);
                    if (env.blocks[caseId] is GXC_CS_CASE caseBlock)
                    {
                        string caseValue = caseBlock.Value.Trim();
                        string[] caseValueSplit = [.. caseBlock.Value
                            .Split('|')
                            .Select(s => s.Trim())];

                        if (caseValueSplit.Length == 1)
                        {
                            if (caseValue.StartsWith('"') && caseValue.EndsWith('"'))
                                caseValue = caseValue[1..^1];

                            if (caseValue != switchValue?.ToString())
                                continue;
                        }
                        else
                        {
                            string switchString = switchValue?.ToString() ?? "";

                            bool matched = caseValueSplit.Any(s =>
                            {
                                s = s.Trim();

                                if (s.StartsWith('"') && s.EndsWith('"'))
                                    s = s[1..^1];

                                return s == switchString;
                            });

                            if (!matched)
                                continue;
                        }

                        GXCodeHelper.Debug($"Switch case matched: {caseValue}");
                        ExecuteBlock(env, caseBlock);
                        caseMatched = true;
                        return;
                    }
                    else if (env.blocks[caseId] is GXC_CS_DEFAULT dB)
                    {
                        defaultBlock = dB;
                    }
                }
            }

            if (defaultBlock is not null)
            {
                GXCodeHelper.Debug("Default case used");
                ExecuteBlock(env, defaultBlock);
                caseMatched = true;
                return;
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
                Variable? variable = GXCodeEnvironment.GetVariable(token)
                    ?? throw new GXCodeInterpreterError($"Unknown variable {token} in repeat statement");
                var repeatVal = variable.Value;
                var repeatType = variable.Type;

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
                GXCodeProgram.scopeStack.Push(new Scope(GXCodeProgram.scopeStack.Peek()));
                bool result = ExecuteBlockBody(env, repeatBlock);
                if (result) break;
                GXCodeProgram.scopeStack.Pop();
            }
            GXCodeProgram.scopeStack.Pop();
            return;
        }
        else if (block is GXC_CS_ITERATE iterateBlock)
        {
            Variable? variable = GXCodeEnvironment.GetVariable(iterateBlock.Variable)
                ?? throw new GXCodeInterpreterError($"Unknown variable {iterateBlock.Variable} in iterate statement");

            var iterateVal = variable.Value;
            var iterateType = variable.Type;

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
                Scope old = GXCodeProgram.scopeStack.Peek();
                GXCodeProgram.scopeStack.Push(new Scope(old));
                old.Set("element", item, iterateType.Substring(0, iterateType.Length - 2));
                bool result = ExecuteBlockBody(env, iterateBlock);
                if (result) break;
                GXCodeProgram.scopeStack.Pop();
            }
            return;
        }
        else if (block is GXC_CS_WHILE whileBlock)
        {
            while (EvaluateConditions(whileBlock.Condition, blockName))
            {
                GXCodeHelper.Debug("While condition is true, executing block");
                GXCodeProgram.scopeStack.Push(new Scope(GXCodeProgram.scopeStack.Peek()));
                bool result = ExecuteBlockBody(env, whileBlock);
                if (result) break;
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
    public static bool ExecuteBlockBody(GXCodeEnvironment env, GXC_CS_ELEMENT block)
    {
        for (int i = 0; i < block.Lines.Count; i++)
        {
            string line = block.Lines[i];
            ShortLineType type = GetShortLineType(line);
            GXCodeHelper.Debug($"Line {i + 1} of {block.GetType().Name}#{block.ID}: {line} (type: {type})");

            string blockName = $"{block.GetType().Name}#{block.ID}";
            string blockType = block.GetType().Name;
            int ri = i+1;

            switch (type)
            {
                case ShortLineType.UNKNOWN:
                    throw new GXCodeInterpreterError($"Undetected indeterminable line structure of {line}");
                case ShortLineType.BUILTIN_OPERATION:
                    bool? result = ExecuteBuiltinOperation(line, ri, block);
                    if (result == true) return false;
                    else if (result == null) return true;
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
                case ShortLineType.METHOD_CALL:
                    CallMethod(env, line, ri, blockName);
                    break;
                case ShortLineType.BLOCK_INDICATOR:
                    int nestedId = int.Parse(Regex.Match(line, @"^\s*\[BLOCK\s+([0-99999999999]+)\]\s*$").Groups[1].Value);
                    ExecuteBlock(env, env.blocks[nestedId]);
                    break;
            }
        }

        return false;
    }

    public static bool? ExecuteBuiltinOperation(string line, int lineNr, GXC_CS_ELEMENT block)
    {
        // out
        string outPattern = @"^\s*out\s+(.*);$";
        Match outMatch = Regex.Match(line, outPattern);

        if (outMatch.Success)
        {
            string output = outMatch.Groups[1].Value;

            Variable? variable = GXCodeEnvironment.GetVariable(output);

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
            else if (variable is not null)
            {
                Console.WriteLine(variable.Value);
            }
            else
            {
                throw new GXCUndeclaredVariableError(lineNr, output, null);
            }
            return false;
        }

        // shout
        string shoutPattern = @"^\s*shout\s+(.*);$";
        Match shoutMatch = Regex.Match(line, shoutPattern);

        if (shoutMatch.Success)
        {
            string output = shoutMatch.Groups[1].Value;

            Variable? variable = GXCodeEnvironment.GetVariable(output);

            if (variable is not null)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("[!] ");
                Console.WriteLine(variable.Value);
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("[!] ");
                Console.WriteLine(output.Trim('"'));
                Console.ResetColor();
            }
            return false;
        }

        // exit
        string exitPattern = @"^\s*exit;\s*$";
        if (Regex.IsMatch(line, exitPattern)) throw new GXCodeBreak();

        // continue
        string continuePattern = @"^\s*continue;\s*$";
        if (Regex.IsMatch(line, continuePattern))
        {
            if (block is GXC_CS_ITERATE || block is GXC_CS_REPEAT || block is GXC_CS_WHILE) return true;
        }

        // break
        string breakPattern = @"^\s*break;\s*$";
        if (Regex.IsMatch(line, breakPattern))
        {
            if (block is GXC_CS_ITERATE || block is GXC_CS_REPEAT || block is GXC_CS_WHILE) return null;
        }

        throw new GXCodeInterpreterError("Could not detect built-in operation");
    }
}