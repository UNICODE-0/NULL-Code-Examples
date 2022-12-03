using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class Interpreter : MonoBehaviour
{
    private const int MAX_COMMANDS_TO_PROCESS = 5000;
    private const float MAX_MEMORY_OVERFLOW = 2; // Multiplier

    private Task _CompileTask = null;
    private Task _ExecuteTask = null;
    private bool _IsExecute = false;
    private float _ExecutionTime = 0;
    private Coroutine _ExecutionTimer;
    private int _MemoryUsed = 0;
    public int memoryUsed
    {
        get { return _MemoryUsed; }
    }
    

    private CancellationTokenSource _TokenSource = new CancellationTokenSource();
    public CancellationTokenSource CancellationToken
    {
        get { return _TokenSource; }
        private set { _TokenSource = value; }
    }

    private InterFunction[] _InterFunctions;

    public InterFunction[] interFunctions
    {
        get { return _InterFunctions; }
    }
    
    private List<Command> Commands = null;
    private Dictionary<string, int> _IntVaribles = new Dictionary<string, int>();
    private Dictionary<string, float> _FloatVaribles = new Dictionary<string, float>();
    private Dictionary<string, bool> _BoolVaribles = new Dictionary<string, bool>();
    private List<string> _ConstVaribles = new List<string>();

    public delegate void CompileEnd(int MemoryUsed);
    public static event CompileEnd compileEnd;

    public string ReplaceWholeWord(string Target, string OldWord, string NewWord)
    {
        bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';
        System.Text.StringBuilder sb = null;
        int p = 0, j = 0;
        while (j < Target.Length && (j = Target.IndexOf(OldWord, j, StringComparison.Ordinal)) >= 0)
            if ((j == 0 || !IsWordChar(Target[j - 1])) &&
            (j + OldWord.Length == Target.Length || !IsWordChar(Target[j + OldWord.Length])))
            {
                sb ??= new  System.Text.StringBuilder();
                sb.Append(Target, p, j - p);
                sb.Append(NewWord);
                j += OldWord.Length;
                p = j;
            }
            else j++;
        if (sb == null) return Target;
        sb.Append(Target, p, Target.Length - p);
        return sb.ToString();
    }

    private void Awake() 
    {
        IDEManager.interpreter = this;
    }
    private void Start() 
    {
        _InterFunctions = new InterFunction[]
        {
            new InterFunction("fac","int", new String[]{"int"}),

            new InterFunction("rand","int", new String[]{"int","int"}),
            
            new InterFunction("getRotation","float", new String[]{}),

            new InterFunction("getPosition","float", new String[]{"int"}),
            
            new InterFunction("getTime","float", new String[]{}),

            new InterFunction("raycast","float", new String[]{"int","int"}),

            new InterFunction("sin","float", new String[]{"float"}),
             
            new InterFunction("cos","float", new String[]{"float"}),
             
            new InterFunction("tan","float", new String[]{"float"}),
 
            new InterFunction("lerp","float", new String[]{"float","float","float"}),
             
            new InterFunction("log","float", new String[]{"float","float"}),
 
            new InterFunction("toInt","int", new String[]{"float"}),
 
            new InterFunction("abs","float", new String[]{"float"}),

            new InterFunction("clamp","float", new String[]{"float","float","float"}),

            new InterFunction("pow","float", new String[]{"float","float"}),

            new InterFunction("sqrt","float", new String[]{"float"}),
            
            new InterFunction("round","int", new String[]{"float"}),
            
            new InterFunction("inRange","bool", new String[]{"float","float","float"})
        };
    }

    private void FixedUpdate() 
    {
        if(_IsExecute && Commands is not null)
        {
            ExecuteAsync();
        }
    }
    private IEnumerator ExecutionTimer()
    {
        while(_IsExecute)
        {
            yield return new WaitForSeconds(0.1f);
            _ExecutionTime += 0.1f;
            if(_ExecutionTime >= float.MaxValue - 1) _ExecutionTime = 0;
        }
    }
    public void StartExexute()
    {
        _IsExecute = true;
        _ExecutionTimer = StartCoroutine(ExecutionTimer());
    }
    public void StopExexute()
    {
        _IsExecute = false;
        _ExecutionTime = 0f;
        if(_ExecuteTask == null || _ExecuteTask.IsCompleted) ClearVaribles();

        RobotManager.ResetAll();
    }

    private string RaplaceVaribles(string Expression)
    {
        string Out = Expression;

        foreach (var IntVar  in _IntVaribles)
        {
            Out = ReplaceWholeWord(Out, IntVar.Key, IntVar.Value.ToString());
        }

        foreach (var FloatVar  in _FloatVaribles)
        {
            Out = ReplaceWholeWord(Out, FloatVar.Key, FloatVar.Value.ToString(CultureInfo.InvariantCulture));
        }

        foreach (var BoolVar  in _BoolVaribles)
        {
            Out = ReplaceWholeWord(Out, BoolVar.Key, BoolVar.Value.ToString());
        }

        return Out;
    }
    private string RaplaceFunctions(string Expression) // You can't call this fucntion alone, you need replaceVaribles -> replaceFunctions, because FuncText.Contains(Func.name) can find function name in a varible name
    {
        string Out = Expression;
        InterFunction[] Functions = IDEManager.interpreter.interFunctions;

        string FuncText = "";
        int FuncStartIndex = 0;
        for (int i = 0; i < Expression.Length;i++)
        {
            bool IsWordEndCahr = Expression[i] == '(' || Expression[i] == ' ' || Expression[i] == '+' || Expression[i] == '-' || Expression[i] == '*' || Expression[i] == '/';
            if(IsWordEndCahr)
            {
                foreach (var Func in _InterFunctions)
                {
                    //if(Regex.IsMatch(FuncText, String.Format(@"\b{0}\b", Func.name)))
                    if(FuncText.Contains(Func.name))
                    {
                        string TextInBrackets = "";
                        for (int j = i; j < Expression.Length; j++)
                        {
                            FuncText += Expression[j];
                            if(Expression[j] == ')')
                            {
                                TextInBrackets = FuncText.Substring(Func.name.Length);
                                TextInBrackets = TextInBrackets.Remove(TextInBrackets.Length - 1,1).Trim().Remove(0,1);
                                i = j;
                                break;
                            } 
                        }
                        string FuncDecRegEx = String.Format(@"\b{0}\b", FuncText);
                        // string ErrorText = "";
                        // if(IDEManager.parser.CheckSyntaxByInstruction(TextInBrackets,Func.instruction, out ErrorText))
                        // {
                        List<string> Params = new List<string>();
                        string CurParam = "";
                        for (int t = 0; t < TextInBrackets.Length; t++)
                        {
                            if(TextInBrackets[t] == ',' || t == TextInBrackets.Length - 1)
                            {
                                if(t == TextInBrackets.Length - 1) CurParam += TextInBrackets[t];

                                Params.Add(CurParam);
                                CurParam = "";
                            } else CurParam += TextInBrackets[t];
                        }
                        
                        string ErrorMessage = "";
                        if(Params.Count == Func.acceptedValuesCount)
                        {
                            List<object> ParamsValue = null;
                            switch (Func.name)
                            {
                                case "fac":

                                ParamsValue = TryGetParamsValue(Params, Func.acceptedValues, out ErrorMessage);
                                if(ParamsValue != null) Out = Out.Replace(FuncText,Factorial(Convert.ToInt32(ParamsValue[0])).ToString(CultureInfo.InvariantCulture));
                                else throw new Exception(ErrorMessage);

                                break;
                                case "rand":

                                ParamsValue = TryGetParamsValue(Params, Func.acceptedValues, out ErrorMessage);
                                if(ParamsValue != null) Out = Out.Replace(FuncText,Rand(Convert.ToInt32(ParamsValue[0]),Convert.ToInt32(ParamsValue[1])).ToString(CultureInfo.InvariantCulture));
                                else throw new Exception(ErrorMessage);

                                break;
                                case "getRotation":

                                Out = Out.Replace(FuncText, RobotManager.movement.GetYRotation().ToString(CultureInfo.InvariantCulture));

                                break;
                                case "getPosition":

                                ParamsValue = TryGetParamsValue(Params, Func.acceptedValues, out ErrorMessage);
                                if(ParamsValue != null) Out = Out.Replace(FuncText,RobotManager.movement.GetPosition(Convert.ToInt32(ParamsValue[0])).ToString(CultureInfo.InvariantCulture));
                                else throw new Exception(ErrorMessage);

                                break;
                                case "getTime":
                                
                                Out = Out.Replace(FuncText, _ExecutionTime.ToString(CultureInfo.InvariantCulture));

                                break;
                                case "sin":

                                ParamsValue = TryGetParamsValue(Params, Func.acceptedValues, out ErrorMessage);
                                if(ParamsValue != null) Out = Out.Replace(FuncText, Mathf.Sin(Convert.ToSingle(ParamsValue[0])).ToString(CultureInfo.InvariantCulture));
                                else throw new Exception(ErrorMessage);

                                break;
                                case "cos":

                                ParamsValue = TryGetParamsValue(Params, Func.acceptedValues, out ErrorMessage);
                                if(ParamsValue != null) Out = Out.Replace(FuncText, Mathf.Cos(Convert.ToSingle(ParamsValue[0])).ToString(CultureInfo.InvariantCulture));
                                else throw new Exception(ErrorMessage);

                                break;
                                case "tan":

                                ParamsValue = TryGetParamsValue(Params, Func.acceptedValues, out ErrorMessage);
                                if(ParamsValue != null) Out = Out.Replace(FuncText, Mathf.Tan(Convert.ToSingle(ParamsValue[0])).ToString(CultureInfo.InvariantCulture));
                                else throw new Exception(ErrorMessage);

                                break;
                                case "lerp":

                                ParamsValue = TryGetParamsValue(Params, Func.acceptedValues, out ErrorMessage);
                                if(ParamsValue != null) Out = Out.Replace(FuncText, Mathf.Lerp(Convert.ToSingle(ParamsValue[0]),Convert.ToSingle(ParamsValue[1]),Convert.ToSingle(ParamsValue[2])).ToString(CultureInfo.InvariantCulture));
                                else throw new Exception(ErrorMessage);

                                break;

                                case "toInt":

                                ParamsValue = TryGetParamsValue(Params, Func.acceptedValues, out ErrorMessage);
                                if(ParamsValue != null) Out = Out.Replace(FuncText, Convert.ToInt32(Convert.ToSingle(ParamsValue[0])).ToString(CultureInfo.InvariantCulture));
                                else throw new Exception(ErrorMessage);

                                break;

                                case "log":

                                ParamsValue = TryGetParamsValue(Params, Func.acceptedValues, out ErrorMessage);
                                if(ParamsValue != null) Out = Out.Replace(FuncText, Mathf.Log(Convert.ToSingle(ParamsValue[0]),Convert.ToSingle(ParamsValue[1])).ToString(CultureInfo.InvariantCulture));
                                else throw new Exception(ErrorMessage);

                                break;
                                case "round":

                                ParamsValue = TryGetParamsValue(Params, Func.acceptedValues, out ErrorMessage);
                                if(ParamsValue != null) Out = Out.Replace(FuncText, Mathf.RoundToInt(Convert.ToSingle(ParamsValue[0])).ToString(CultureInfo.InvariantCulture));
                                else throw new Exception(ErrorMessage);

                                break;
                                case "abs":

                                ParamsValue = TryGetParamsValue(Params, Func.acceptedValues, out ErrorMessage);
                                if(ParamsValue != null) Out = Out.Replace(FuncText, Mathf.Abs(Convert.ToSingle(ParamsValue[0])).ToString(CultureInfo.InvariantCulture));
                                else throw new Exception(ErrorMessage);

                                break;
                                case "clamp":

                                ParamsValue = TryGetParamsValue(Params, Func.acceptedValues, out ErrorMessage);
                                if(ParamsValue != null) Out = Out.Replace(FuncText, Mathf.Clamp(Convert.ToSingle(ParamsValue[0]),Convert.ToSingle(ParamsValue[1]),Convert.ToSingle(ParamsValue[2])).ToString(CultureInfo.InvariantCulture));
                                else throw new Exception(ErrorMessage);

                                break;
                                case "pow":

                                ParamsValue = TryGetParamsValue(Params, Func.acceptedValues, out ErrorMessage);
                                if(ParamsValue != null) Out = Out.Replace(FuncText, Mathf.Pow(Convert.ToSingle(ParamsValue[0]),Convert.ToSingle(ParamsValue[1])).ToString(CultureInfo.InvariantCulture));
                                else throw new Exception(ErrorMessage);

                                break;
                                case "sqrt":

                                ParamsValue = TryGetParamsValue(Params, Func.acceptedValues, out ErrorMessage);
                                if(ParamsValue != null) Out = Out.Replace(FuncText, Mathf.Sqrt(Convert.ToSingle(ParamsValue[0])).ToString(CultureInfo.InvariantCulture));
                                else throw new Exception(ErrorMessage);

                                break;
                                case "raycast":

                                ParamsValue = TryGetParamsValue(Params, Func.acceptedValues, out ErrorMessage);
                                if(ParamsValue != null) Out = Out.Replace(FuncText, RobotManager.movement.Raycast(Convert.ToInt32(ParamsValue[0]),Convert.ToInt32(ParamsValue[1])).ToString(CultureInfo.InvariantCulture));
                                else throw new Exception(ErrorMessage);

                                break;
                                case "inRange":

                                ParamsValue = TryGetParamsValue(Params, Func.acceptedValues, out ErrorMessage);
                                if(ParamsValue != null) Out = Out.Replace(FuncText, InRange(Convert.ToSingle(ParamsValue[0]),Convert.ToSingle(ParamsValue[1]),Convert.ToSingle(ParamsValue[2])).ToString(CultureInfo.InvariantCulture));
                                else throw new Exception(ErrorMessage);

                                break;
                                default:
                                Debug.LogError("Unknown func name!");
                                return Out;
                            }
                        }else throw new Exception($"Wrong accepted values count in function '{Func.name}'.");
                        // }else
                        // {
                        //     Debug.Log("CheckSyntaxByInstruction error: " + ErrorText);
                        //     throw new Exception($"Wrong '{Func.name}' function syntax.");
                        // } 
                    }
                }

                FuncStartIndex = i;
                FuncText = "";
            } else FuncText += Expression[i];
        }

        return Out;
    }

    private void Dispose(List<Command> CommandsToDispose)
    {
        for (int i = 0; i < CommandsToDispose.Count; i++)
        {
            Command Cmd = CommandsToDispose[i];
            if(Cmd.isStaticVarible || Cmd.commandType != CommandType.Declaration) continue;

            switch (Cmd.varibleType)
            {
                case "int":
                _IntVaribles.Remove(Cmd.varibleName);
                break;
                case "float":
                _FloatVaribles.Remove(Cmd.varibleName);
                break;
                case "bool":
                _BoolVaribles.Remove(Cmd.varibleName);
                break;
                default:
                Debug.LogError($"Unknown varible type '{Cmd.varibleType}'.");
                break;
            }
        }
    }
    private object ComputeExpression(string Expression)
    {
        object ComputedExpression = new DataTable().Compute(Expression, null);
        int DataHash = ComputedExpression.GetHashCode();
        if(DataHash == 2146435072 || DataHash == -1048576) throw new Exception("Attempted to divide by zero.");

        if(ComputedExpression.ToString().Length == 0) ComputedExpression = null;

        return ComputedExpression;
    }
    private bool IsParamsExist(List<string> Params, string[] Types, out string ErrorMessage)
    {
        if(Params.Count != Types.Length)
        {
            ErrorMessage = "Params count and Types count are diffrent.";
            Debug.LogError(ErrorMessage);
            return false;
        } 

        for (int i = 0; i < Params.Count; i++)
        {
            ErrorMessage = $"'{Params[i]}' variable doesn't exist in current context.";
            switch (Types[i])
            {
                case "int":
                if(!_IntVaribles.ContainsKey(Params[i])) return false;
                break;
                case "float":
                if(!_FloatVaribles.ContainsKey(Params[i])) return false;
                break;
                case "bool":
                if(!_BoolVaribles.ContainsKey(Params[i])) return false;
                break;
                default:
                ErrorMessage = $"Unknown param type {Types[i]}.";
                Debug.LogError(ErrorMessage);
                return false;
            }
        }
        
        ErrorMessage = "";
        return true;
    }
    private List<object> TryGetParamsValue(List<string> Params, string[] Types, out string ErrorMessage)
    {
        List<object> ParamsValue = new List<object>();
        for (int i = 0; i < Params.Count; i++)
        {
            object ComputedParam = ComputeExpression(Params[i]);
            ParamsValue.Add(ComputedParam);

            ErrorMessage = "Specified cast is not valid.";
            switch (Types[i])
            {
                case "int":
                if(ComputedParam is not Int32) return null;
                break;
                case "float":
                if(ComputedParam is Boolean) return null; // because ComputedParam may be 0 or 1 or 2, and these numbers are int anf float at the same time.
                break;
                case "bool":
                if(ComputedParam is not Boolean) return null;
                break;
                default:
                ErrorMessage = $"Unknown param type {Types[i]}.";
                Debug.LogError(ErrorMessage);
                return null;
            }
        }

        ErrorMessage = "";
        return ParamsValue;
    }
    private void ExecuteComands(List<Command> Commands)
    {
        List<string> StringToPrint = new List<string>();
        
        if(Commands != null)
        {
            int CommandsProcessed = 0;
            for (int i = 0; i < Commands.Count; i++)
            {
                if(_TokenSource.IsCancellationRequested) return;
                Command Cmd = Commands[i];
                try
                {
                    CommandsProcessed++;
                    if(CommandsProcessed > MAX_COMMANDS_TO_PROCESS) throw new Exception("You have exceeded the execution time.");

                    string Expression = Cmd.expression;
                    if(Expression != null)
                    {
                        Expression = RaplaceFunctions(RaplaceVaribles(Expression)); // VERY BAD PERFOMANCE
                    }

                    object ComputedNumber = ComputeExpression(Expression);
                    
                    string ErrorMessage;
                    switch (Cmd.commandType)
                    {
                        case CommandType.Declaration:
                            if(Cmd.isStaticVarible &&
                            (_IntVaribles.ContainsKey(Cmd.varibleName) ||
                            _FloatVaribles.ContainsKey(Cmd.varibleName) ||
                            _BoolVaribles.ContainsKey(Cmd.varibleName)) ) continue;
                            
                            if(Cmd.isConstVarible)
                            {
                                _ConstVaribles.Add(Cmd.varibleName);
                            }

                            switch (Cmd.varibleType)
                            {
                                case "int":
                                if(ComputedNumber == null && Cmd.isStaticVarible) _IntVaribles.Add(Cmd.varibleName, ComputedNumber is null ? 0 : int.Parse(ComputedNumber.ToString()));
                                else if(ComputedNumber == null) _IntVaribles.Add(Cmd.varibleName, 0);
                                //else if(ComputedNumber.ToString().Contains('E')) throw new Exception("Value was either too large or too small for a Int.");
                                else _IntVaribles.Add(Cmd.varibleName, int.Parse(ComputedNumber.ToString()));
                                break;
                                case "float":
                                if(ComputedNumber == null && Cmd.isStaticVarible) _FloatVaribles.Add(Cmd.varibleName, ComputedNumber is null ? 0.0f : float.Parse(ComputedNumber.ToString()));
                                else if(ComputedNumber == null) _FloatVaribles.Add(Cmd.varibleName, 0.0f);
                                else _FloatVaribles.Add(Cmd.varibleName, float.Parse(ComputedNumber.ToString()));
                                break;
                                case "bool":
                                if(ComputedNumber == null && Cmd.isStaticVarible) _BoolVaribles.Add(Cmd.varibleName, ComputedNumber is null ? (false) : bool.Parse(ComputedNumber.ToString()));
                                else if(ComputedNumber == null) _BoolVaribles.Add(Cmd.varibleName, false);
                                else _BoolVaribles.Add(Cmd.varibleName, bool.Parse(ComputedNumber.ToString()));
                                break;
                                default:
                                Debug.LogError("Unknown varible type.");
                                break;
                            }
                            break;
                        case CommandType.Assigment:
                            if(_ConstVaribles.Contains(Cmd.varibleName)) throw new Exception($"'{Cmd.varibleName}' variable is constant, you can't change the constant variables.");

                            switch (Cmd.varibleType)
                            {
                                case "int":
                                if(_IntVaribles.ContainsKey(Cmd.varibleName)) _IntVaribles[Cmd.varibleName] = int.Parse(ComputedNumber.ToString());
                                else throw new Exception($"'{Cmd.varibleName}' variable doesn't exist in current context.");
                                break;
                                case "float":
                                if(_FloatVaribles.ContainsKey(Cmd.varibleName)) _FloatVaribles[Cmd.varibleName] = float.Parse(ComputedNumber.ToString());
                                else throw new Exception($"'{Cmd.varibleName}' variable doesn't exist in current context.");
                                break;
                                case "bool":
                                if(_BoolVaribles.ContainsKey(Cmd.varibleName)) _BoolVaribles[Cmd.varibleName] = bool.Parse(ComputedNumber.ToString());
                                else throw new Exception($"'{Cmd.varibleName}' variable doesn't exist in current context.");
                                break;
                                default:
                                Debug.LogError("Unknown varible type.");
                                break;
                            }
                            break;
                        case CommandType.Validate:
                            bool ValidateCondition = bool.Parse(ComputedNumber.ToString());
                            if(ValidateCondition) i++;
                            break;
                        case CommandType.IfStatement:
                            bool IfStatementCondition = bool.Parse(ComputedNumber.ToString());
                            if(IfStatementCondition)
                            {
                                if(i + 1 <= Commands.Count - 1)
                                {
                                    int ScopeStartIndex = i;
                                    int DisposeCommandsCount = Cmd.commnadsToSkip;
                                    if(ScopeStartIndex + DisposeCommandsCount > Commands.Count - 1)
                                    {
                                        Debug.LogError("Can't dispose command by index grater than commands count");
                                        throw new Exception("Dispose init command failure.");
                                    }

                                    ScopeStartIndex++; // To not check 'IfStatementCondition' command

                                    List<Command> CommandsToDispose = new List<Command>();
                                    for (int j = 0; j < DisposeCommandsCount; j++)
                                    {
                                        Command CmdToDisp = Commands[ScopeStartIndex + j];
                                        if(CmdToDisp.commandType == CommandType.Declaration)
                                        {
                                            CommandsToDispose.Add(CmdToDisp);
                                        }
                                    }
                                    Commands.Insert(i + Cmd.commnadsToSkip + 1, new Command(CommandType.Dispose, CommandsToDispose, Cmd.callLine));
                                } 
                            } 
                            else i += Cmd.commnadsToSkip;
                            break;
                        case CommandType.WhileStatement:
                            bool WhileStatementCondition = bool.Parse(ComputedNumber.ToString());
                            if(WhileStatementCondition)
                            {
                                if(i + 1 <= Commands.Count - 1)
                                {
                                    int ScopeStartIndex = i;
                                    int DisposeCommandsCount = Cmd.commnadsToSkip;
                                    if(ScopeStartIndex + DisposeCommandsCount > Commands.Count - 1)
                                    {
                                        Debug.LogError("Can't dispose command by index grater than commands count");
                                        throw new Exception("Dispose init command failure.");
                                    }

                                    ScopeStartIndex++; // To not check 'WhileStatementCondition' command

                                    List<Command> CommandsToDispose = new List<Command>();
                                    for (int j = 0; j < DisposeCommandsCount; j++)
                                    {
                                        Command CmdToDisp = Commands[ScopeStartIndex + j];
                                        if(CmdToDisp.commandType == CommandType.Declaration)
                                        {
                                            CommandsToDispose.Add(CmdToDisp);
                                        }
                                    }
                                    int EndOfWhileIndex = i + Cmd.commnadsToSkip + 1;
                                    Commands.Insert(EndOfWhileIndex, new Command(CommandType.Repeat, CommandsToDispose, Cmd.commnadsToSkip, Cmd.expression, Cmd.callLine));
                                    Commands.Insert(EndOfWhileIndex, new Command(CommandType.Dispose, CommandsToDispose, Cmd.callLine));
                                } 
                            } 
                            else i += Cmd.commnadsToSkip;
                            break;
                        case CommandType.Dispose:
                            Dispose(Cmd.commnadsToDispose);
                            Commands.Remove(Cmd);
                            i--;
                            break;
                            case CommandType.Repeat:
                            bool RepeatCondition = bool.Parse(ComputedNumber.ToString());
                            if(RepeatCondition)
                            {
                                if(i - Cmd.commnadsToRepeat - 1 >= 0)
                                {
                                    Commands.Insert(i, new Command(CommandType.Dispose, Cmd.commnadsToDispose, Cmd.callLine));
                                    i -= Cmd.commnadsToRepeat + 1;
                                } 
                                else Debug.LogError("Can't repeat command with negative index.");
                            }
                            Commands.Remove(Cmd);
                            i--;
                            break;
                        case CommandType.Print:
                            string ValueToPrint = "Print error.";

                            if(_IntVaribles.ContainsKey(Cmd.varibleName)) ValueToPrint = _IntVaribles[Cmd.varibleName].ToString();
                            else if(_FloatVaribles.ContainsKey(Cmd.varibleName)) ValueToPrint = _FloatVaribles[Cmd.varibleName].ToString().Replace(',','.');
                            else if(_BoolVaribles.ContainsKey(Cmd.varibleName)) ValueToPrint =  _BoolVaribles[Cmd.varibleName].ToString();
                            else throw new Exception($"'{Cmd.varibleName}' variable doesn't exist in current context.");

                            StringToPrint.Add(ValueToPrint + "\n");
                            break;
                        case CommandType.PMove:
                            if(!IsParamsExist(Cmd.funcParams, new string[]{"float"}, out ErrorMessage))
                            {
                                throw new Exception(ErrorMessage);
                            }
                            RobotManager.movement.Move(_FloatVaribles[Cmd.funcParams[0]]);
                            break;
                        case CommandType.PRotateY:
                            if(!IsParamsExist(Cmd.funcParams, new string[]{"float"}, out ErrorMessage))
                            {
                                throw new Exception(ErrorMessage);
                            }
                            RobotManager.movement.RotateAroundY(_FloatVaribles[Cmd.funcParams[0]]);
                            break;
                        default:
                        Debug.Log(Cmd.callLine + " | " + Cmd.commandType);
                        Debug.LogError("Unknown command type.");
                        break;
                    }
                } catch (Exception ex)
                {
                    string Message = $"[L{Cmd.callLine}] ";
                    if(ex.Message.Contains("Cannot find column")) Message += "Unknown expression or invalid parametres.";
                    else if(ex.Message.Contains("An item with the same key has already been added.")) Message += $"A variable with name '{Cmd.varibleName}' already exists.";
                    else if(ex.Message.Contains("Index was outside the bounds of the array")) Message += "Wrong function syntax.";
                    else if(ex.Message.Contains("Value was either too large or too small for a Single.")) Message += "Value was either too large or too small for a Float.";
                    else if(ex.Message.Contains("Value was either too large or too small for an Int32.")) Message += "Value was either too large or too small for an Int.";
                    else if(ex.Message.Contains("Input string was not in a correct format.")) Message += "Specified cast is not valid.";
                    else if(ex.Message.Contains(" Cannot perform 'Mod' operation")) Message += "It is impossible to compare.";
                    else Message += ex.Message;

                    StopExexute();
                    IDEManager.console.EPrint(Message, true, PrintSource.CodeEditor);
                    IDEManager.console.EPrint("Error", true, PrintSource.InGameUI);
                    return;
                }

            }

            IDEManager.console.Clear();
            foreach (var String in StringToPrint)
            {
                IDEManager.console.Print(String, false);
            }
        } else
        {
            StopExexute();
            return;
        }
        
        if(_IsExecute) Dispose(Commands);
        else ClearVaribles();
    }
    public async void ExecuteAsync()
    {
        if(_ExecuteTask == null || _ExecuteTask.IsCompleted)
        {
            _ExecuteTask = Task.Run(() => 
                {
                    ExecuteComands(Commands);
                }
            , _TokenSource.Token);
    
            await _ExecuteTask;
        }
    }
    public void Compile()
    {
        StopExexute();

        Commands = IDEManager.parser.GetCommandsFromString(IDEManager.codeEditor.inputText);
        if(Commands != null)
        {
            _MemoryUsed = Command.GetUsedMemory(Commands);
            if(_MemoryUsed >= LevelManager.avalibleMemory * 2)
            {
                Commands = null;
                IDEManager.console.EPrint("Critical memory overflow!\n", true, PrintSource.CodeEditor);
            } else
            {
                IDEManager.console.Print("<color=#9FCB6A>       Compile succes!\n---------------------------\n</color>", true, PrintSource.CodeEditor);
            }

            IDEManager.console.Print($"Memory used: {_MemoryUsed} byte(s)", false, PrintSource.CodeEditor);
        }
    }
    public async void CompileAsync()
    {
        if(_CompileTask == null || _CompileTask.IsCompleted)
        {
            _CompileTask = Task.Run(() => 
                {
                    Compile();
                }
            , _TokenSource.Token);
    
            await _CompileTask;
        }
        
        compileEnd(_MemoryUsed);
        UIManager.instance.SetDefaultUIState();
    }
    private void OnDisable() 
    {
        _TokenSource.Cancel();
    }
    private void ClearVaribles()
    {
        _IntVaribles.Clear();
        _FloatVaribles.Clear();
        _BoolVaribles.Clear();

        _ConstVaribles.Clear();
    }
    // Functions realiations
    private int Factorial(int Num)
    {
        if(Num == 1 || Num == 0) return 1;
        else if(Num < 0) throw new Exception($"invalid input parameter {Num}.");
        else if(Num > 10) throw new Exception($"The maximum number to compute for a factorial is 10.");
        else 
        {
            return Num * Factorial(Num - 1);
        }
    }
    private int Rand(int MinValue, int MaxValue)
    {
        if(Math.Abs(MinValue - MaxValue) > 1000) throw new Exception($"The maximum range to randomize is 1000.");
        else if(MinValue > MaxValue) throw new Exception($"The range is specified incorrectly.");

        System.Random TempRand = new System.Random();
        return TempRand.Next(MinValue,MaxValue);
    }
    private bool InRange(float Value, float RangeStart, float RangeEnd)
    {
        if(Value >= RangeStart && Value <= RangeEnd)
        {
            return true;
        } else return false;
    }
}

public class InterFunction
{
    private string _Name;
    public string name
    {
        get { return _Name; }
    }
    private string _ReturnValue;
    public string returnValue
    {
        get { return _ReturnValue; }
    }
    private int _AcceptedValuesCount;
    public int acceptedValuesCount
    {
        get { return _AcceptedValuesCount; }
    }
    private string[] _AcceptedValues;
    public string[] acceptedValues
    {
        get { return _AcceptedValues; }
        set { _AcceptedValues = value; }
    }
    
    public InterFunction(string Name, string ReturnValue, string[] AcceptedValues)
    {
        _Name = Name;
        _ReturnValue = ReturnValue;
        _AcceptedValues = AcceptedValues;
        _AcceptedValuesCount = AcceptedValues.Length;
    }
    
}