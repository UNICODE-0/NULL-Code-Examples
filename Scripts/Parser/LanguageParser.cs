using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Data;
using System.Linq;
using TMPro;
using System.Threading;

public class LanguageParser : MonoBehaviour
{
    private Instruction[] _AvalibleInstructions;

    [Range(0,1)]
    private float _ParseStatus; // 1.0 - parse complete, 0.0 - parse start
    public float parseStatus
    {
        get { return _ParseStatus; }
        private set { _ParseStatus = value; }
    }
    public bool isParsing { get; private set; }
    
    private void Awake() 
    {
        IDEManager.parser = this;        
    }
    private void Start() 
    {
        _AvalibleInstructions = new Instruction[]
        {
            new Instruction(InstructionText: "include",
            SyntaxTags: new string[]{"@Space", "@Symbol(')", "@OSpace", "@InclusionName", "#Include", "@OSpace", "@Symbol(')"}),

            new Instruction(InstructionText: "int", Type: "int",
            SyntaxTags: new string[]{"@Space", "@DefineWord", "#InitVar", "@OSpace", "@Symbol(=|;)" , "@OSpace", "@Number(int)", "#AssignValue", "@OSpace", "@Symbol(;)"}),
            
            new Instruction(InstructionText: "^int", Type: "int",
            SyntaxTags: new string[]{"@Space", "@DefineWord", "#InitVar", "#SetConst", "@OSpace", "@Symbol(=|;)" , "@OSpace", "@Number(int)", "#AssignValue", "@OSpace", "@Symbol(;)"}),

            new Instruction(InstructionText: "~int", Type: "int",
            SyntaxTags: new string[]{"@Space", "@DefineWord", "#InitVar", "#SetStatic", "@OSpace", "@Symbol(=|;)" , "@OSpace", "@Number(int)", "#AssignValue", "@OSpace", "@Symbol(;)"}),

            new Instruction(InstructionText: "@Varible(int)", Type: "int", 
            SyntaxTags: new string[]{"#UseVar", "@OSpace", "@Symbol(=)", "@OSpace", "@Number(int)", "#AssignValue", "@OSpace", "@Symbol(;)"} ),

            new Instruction(InstructionText: "float", Type: "float",
            SyntaxTags: new string[]{"@Space", "@DefineWord", "#InitVar", "@OSpace", "@Symbol(=|;)" , "@OSpace", "@Number(float)", "#AssignValue", "@OSpace", "@Symbol(;)"}),

            new Instruction(InstructionText: "^float", Type: "float",
            SyntaxTags: new string[]{"@Space", "@DefineWord", "#InitVar", "#SetConst", "@OSpace", "@Symbol(=|;)" , "@OSpace", "@Number(float)", "#AssignValue", "@OSpace", "@Symbol(;)"}),

            new Instruction(InstructionText: "~float", Type: "float",
            SyntaxTags: new string[]{"@Space", "@DefineWord", "#InitVar", "#SetStatic", "@OSpace", "@Symbol(=|;)" , "@OSpace", "@Number(float)", "#AssignValue", "@OSpace", "@Symbol(;)"}),

            new Instruction(InstructionText: "@Varible(float)", Type: "float", 
            SyntaxTags: new string[]{"#UseVar", "@OSpace", "@Symbol(=)", "@OSpace", "@Number(float)", "#AssignValue", "@OSpace", "@Symbol(;)"} ),

            new Instruction(InstructionText: "bool", Type: "bool",
            SyntaxTags: new string[]{"@Space", "@DefineWord", "#InitVar", "@OSpace", "@Symbol(=|;)" , "@OSpace", "@LogicalValue", "#AssignValue", "@OSpace", "@Symbol(;)"}),

            new Instruction(InstructionText: "^bool", Type: "bool",
            SyntaxTags: new string[]{"@Space", "@DefineWord", "#InitVar", "#SetConst", "@OSpace", "@Symbol(=|;)" , "@OSpace", "@LogicalValue", "#AssignValue", "@OSpace", "@Symbol(;)"}),

            new Instruction(InstructionText: "~bool", Type: "bool",
            SyntaxTags: new string[]{"@Space", "@DefineWord", "#InitVar", "#SetStatic", "@OSpace", "@Symbol(=|;)" , "@OSpace", "@LogicalValue", "#AssignValue", "@OSpace", "@Symbol(;)"}),

            new Instruction(InstructionText: "@Varible(bool)", Type: "bool", 
            SyntaxTags: new string[]{"#UseVar", "@OSpace", "@Symbol(=)", "@OSpace", "@LogicalValue", "#AssignValue", "@OSpace", "@Symbol(;)"} ),

            new Instruction(InstructionText: "if",
            SyntaxTags: new string[]{"@OSpace", "@Symbol([)", "@OSpace", "@LogicalValue", "#ProcessCondition", "@OSpace", "@Symbol(])",
            "@OSpace", "@OEscapeSymbol", "@OSpace", "@Symbol({)", "@ScopeText", "#ProcessIfStatement", "@Symbol(})"}),
            
            new Instruction(InstructionText: "while",
            SyntaxTags: new string[]{"@OSpace", "@Symbol([)", "@OSpace", "@LogicalValue", "#ProcessCondition", "@OSpace", "@Symbol(])",
            "@OSpace", "@OEscapeSymbol", "@OSpace", "@Symbol({)", "@ScopeText", "#ProcessWhileStatement", "@Symbol(})"}),

            new Instruction(InstructionText: "Print",
            SyntaxTags: new string[]{"@OSpace", "@Symbol(()", "@OSpace", "@Varible", "#PrintVar", "@OSpace", "@Symbol())", "@OSpace", "@Symbol(;)"}),
            
            new Instruction(InstructionText: "Move",
            SyntaxTags: new string[]{"@OSpace", "@Symbol(()", "@OSpace", "@Varible(float)", "#AddParam", "@OSpace",
            "@Symbol())", "@OSpace", "#Call(PMove)", "@Symbol(;)"}),

            new Instruction(InstructionText: "Rotate",
            SyntaxTags: new string[]{"@OSpace", "@Symbol(()", "@OSpace", "@Varible(float)", "#AddParam", "@OSpace",
            "@Symbol())", "@OSpace", "#Call(PRotateY)", "@Symbol(;)"})
        };

        CheckInstructionsSyntax(_AvalibleInstructions);
    }
    private string GetInstructionsTextRegEx(Instruction[] Instructions)
    {
        string RegEx = @"\b(";
        for (int i = 0; i < Instructions.Length; i++)
        {
            RegEx += Instructions[i].instructionText;
            if(i != Instructions.Length - 1) RegEx += "|";
        }
        RegEx += @")\b";

        return RegEx;
    }
    private void CheckInstructionsSyntax(Instruction[] Instructions)
    {
        foreach (var Instruction in Instructions)
        {
            string[] Tags = Instruction.syntaxTags;
            for (int i = 0; i < Tags.Length; i++)
            {
                switch (Tags[i])
                {
                    case "#InitVar":
                    if(i - 1 < 0 || Tags[i - 1] != "@DefineWord")
                    {
                        Debug.LogError("The @DefineWord tag is required before the #InitVar tag");
                    } 
                    break;    
                    case "#AssignValue":
                    if(i - 1 < 0 || (!Tags[i - 1].Contains("@Number") && Tags[i - 1] != "@LogicalValue"))
                    {
                        Debug.LogError("The @Number or @LogicalValue tag is required before the #AssignValue tag");
                    } 
                    break;
                    case "#ProcessCondition":
                    if(i - 1 < 0 || (Tags[i - 1] != "@LogicalValue" && Tags[i - 1] != "@Varible"))
                    {
                        Debug.LogError("The @Varible or @LogicalValue tag is required before the #ProcessCondition tag");
                    } 
                    break;
                    case "#AddParam":
                    if(i - 1 < 0 || !Tags[i - 1].Contains("@Varible"))
                    {
                        Debug.LogError("The @Varible tag is required before the #AddParam tag");
                    } 
                    break;
                }    
            }
        }
    }
    public List<Command> GetCommandsFromString(string TargetString)
    {
        _ParseStatus = 0;
        string ErrorText = "";
        int ErrorIndex = 0;

        isParsing = true;
        List<Command> Cmds = ParseStringToCommands(TargetString, 0, TargetString.Length, out ErrorText, out ErrorIndex, true, null);
        isParsing = false;

        return Cmds;
    }
    private List<Command> ParseStringToCommands(
    string TargetString, int StartIndex, int EndIndex, out string ErrorText, out int ErrorIndex, bool PrintError = true,List<string> ExistInclusions = null, Dictionary<string,string> ExistVaribles = null)
    {
        ErrorText = "Unexpected error";
        ErrorIndex = 0;

        List<string> Inclusions;
        if(ExistInclusions == null) Inclusions = new List<string>(); 
        else Inclusions = ExistInclusions;

        Dictionary<string,string> Varibles;
        if(ExistVaribles == null) Varibles = new Dictionary<string, string>();
        else Varibles = ExistVaribles;

        List<Command> OutputCommands = new List<Command>();

        int TargetStringLength = TargetString.Length;
        string Word = "";
        Instruction CurrentInstruction;
        for (int i = StartIndex; i < TargetStringLength && i < EndIndex; i++)
        {
            if(IDEManager.interpreter.CancellationToken.IsCancellationRequested) return null;

            _ParseStatus = i / (float)TargetStringLength;

            char CurrentChar = TargetString[i];

            bool IsInvisibleChar = 
            CurrentChar == ' '  || 
            CurrentChar == '\n' ||
            CurrentChar == '\t' ||
            CurrentChar == ''; // TMP_Input sometimes insert this symbol Instead of '\n' (bug?)

            bool IsSpecialChar =
            //TargetString[i] == ',' || 
            CurrentChar == '(' || 
            CurrentChar == ')' || 
            CurrentChar == '{' || 
            CurrentChar == '}' ||
            CurrentChar == '[' || 
            CurrentChar == ']' ||
            CurrentChar == '=';

            bool IsWordEndChar = IsInvisibleChar || IsSpecialChar;

            bool IsStringEnd = i == TargetStringLength - 1 || i == EndIndex - 1;

            if((int)CurrentChar == 13)
            {
                ErrorText = $"unresolved character with unicode number {(int)CurrentChar}";
                if(PrintError) IDEManager.console.EPrint("[L" +  GetCharLine(i, TargetString) + "] " + ErrorText, true, PrintSource.CodeEditor);
                ErrorIndex = i;
                return null;
            }

            if(IsWordEndChar || IsStringEnd)
            {
                if(IsStringEnd && !IsWordEndChar) Word += CurrentChar;
                if(IsSpecialChar && Word.Length == 0)
                {
                    Word += CurrentChar;
                }
                if(Word.Length == 0) continue;

                if(TryGetInstructionByWord(Word, out CurrentInstruction, Varibles))
                {
                    List<int> TagsLastHandleIndex = new List<int>();
                    int SyntaxTagСonfirmed = 0;
                    int InstructionReadStart = i;
                    int SyntaxTagRequired = CurrentInstruction.syntaxTags.Length;
                    
                    SyntaxHandleInfo SyntaxHandleInfo = new SyntaxHandleInfo();
                    List<string> HandledTagResults = new List<string>();
                    HandledTagResults.Add(Word);

                    Command CurrentCommand = new Command();
                    CurrentCommand.varibleType = CurrentInstruction.type;
                    CurrentCommand.callLine =  GetCharLine(i, TargetString);

                    OutputCommands.Add(CurrentCommand);
                    foreach (string SyntaxTag in CurrentInstruction.syntaxTags)
                    {
                        SyntaxHandleInfo = HandleSyntaxTag(SyntaxTag, Varibles, Inclusions, ref TargetString, InstructionReadStart, ref CurrentCommand, HandledTagResults.LastOrDefault(), TagsLastHandleIndex);
                        //Debug.Log(SyntaxHandleInfo.handledTagResult + " | " + SyntaxHandleInfo.handleResult);
                        if(SyntaxHandleInfo == null)
                        {
                            ErrorIndex = i;
                            return null;
                        }
                        if(SyntaxHandleInfo.handleResult)
                        {
                            if(SyntaxHandleInfo.InclusionVaribles != null)
                            {
                                foreach (var Varible in SyntaxHandleInfo.InclusionVaribles)
                                {
                                    if(Varibles.ContainsKey(Varible.Key))
                                    {
                                        ErrorText = $"A variable with name '{Varible.Key}' already exists.";
                                        int SyntaxErrorIndex = SyntaxHandleInfo.errorIndex;
                                        ErrorIndex = SyntaxErrorIndex == -1 ? i : SyntaxErrorIndex;
                                        if(PrintError) IDEManager.console.EPrint("[L" +  GetCharLine(ErrorIndex, TargetString) + "] " + ErrorText, true, PrintSource.CodeEditor); 

                                        return null;
                                    }
                                    else Varibles.Add(Varible.Key,Varible.Value);
                                }
                            } 

                            if(SyntaxHandleInfo.handledCommands != null)
                            {
                                foreach (var cmd in SyntaxHandleInfo.handledCommands)
                                {
                                    if(cmd.commandType != CommandType.Empty) OutputCommands.Add(cmd);
                                }
                            }

                            string HandledTagResult = SyntaxHandleInfo.handledTagResult;
                            if(HandledTagResult != null) HandledTagResults.Add(HandledTagResult);

                            InstructionReadStart = SyntaxHandleInfo.lastHandleIndex;
                            TagsLastHandleIndex.Add(SyntaxHandleInfo.lastHandleIndex);
                            SyntaxTagСonfirmed++;
                            if(SyntaxHandleInfo.endOfLine)
                            {
                                SyntaxTagRequired = SyntaxTagСonfirmed;
                                break;
                            }
                        } else break;
                    }

                    if(SyntaxTagСonfirmed == SyntaxTagRequired)
                    {
                        if(CurrentCommand.commandType == CommandType.Empty) OutputCommands.Remove(CurrentCommand);

                        if(CurrentInstruction.syntaxTags.Contains("#InitVar"))
                        {
                            if(Varibles.Keys.Contains(CurrentCommand.varibleName))
                            {
                                //Varibles[CurrentCommand.varibleName] = CurrentCommand.varibleType;
                                ErrorText = $"A variable with name '{CurrentCommand.varibleName}' already exists.";
                                int SyntaxErrorIndex = SyntaxHandleInfo.errorIndex;
                                ErrorIndex = SyntaxErrorIndex == -1 ? i : SyntaxErrorIndex;
                                if(PrintError) IDEManager.console.EPrint("[L" +  GetCharLine(ErrorIndex, TargetString) + "] " + ErrorText, true, PrintSource.CodeEditor);

                                return null;
                            }else
                            {
                                Varibles.Add(CurrentCommand.varibleName, CurrentCommand.varibleType);
                            }
                        }

                    } 
                    else
                    {
                        ErrorText = SyntaxHandleInfo.errorText;
                        int SyntaxErrorIndex = SyntaxHandleInfo.errorIndex;
                        ErrorIndex = SyntaxErrorIndex == -1 ? i : SyntaxErrorIndex;
                        if(PrintError)
                        {
                            IDEManager.console.EPrint("[L" +  GetCharLine(ErrorIndex, TargetString) + "] " + ErrorText, true, PrintSource.CodeEditor);
                        } 

                        return null;
                    }
                    i = SyntaxHandleInfo.lastHandleIndex - 1;
                } else
                {
                    ErrorText = "Unknown command: '"+ Word +"'.";
                    int WordStartIndex = i - (Word.Length + 1);
                    ErrorIndex = WordStartIndex;
                    if(PrintError) IDEManager.console.EPrint("[L" + GetCharLine(WordStartIndex, TargetString) +"] " + ErrorText, true, PrintSource.CodeEditor);

                    return null;
                }
                Word = "";
                continue;
            } else
            {
                Word += CurrentChar;
            } 
        }

        ErrorIndex = -1;
        return OutputCommands;
    }

    private SyntaxHandleInfo HandleSyntaxTag(
    string SyntaxysTag, Dictionary<string,string> Varibles, List<string> ExistInclusions, ref string FromString, int StartPoint,
    ref Command InitCommand, string LastHandledResult, List<int> TagsHandledIndexes)
    {
        SyntaxHandleInfo syntaxHandleInfo = new SyntaxHandleInfo();

        if(SyntaxysTag.Contains(@"!MoveLHI")) // Move last handled index
        {
            string Number = "";
            for (int i = 9; i < SyntaxysTag.Length; i++)
            {
                if(SyntaxysTag[i] == ')' || i == SyntaxysTag.Length - 1) 
                {
                    break;
                }
                Number += SyntaxysTag[i];
            }

            try
            {
                int TagNumber = int.Parse(Number);
                syntaxHandleInfo.lastHandleIndex = TagsHandledIndexes[TagNumber];
                syntaxHandleInfo.handleResult = true;
                return syntaxHandleInfo;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                syntaxHandleInfo.errorText += "Internal error.";
                syntaxHandleInfo.handleResult = false;
                return syntaxHandleInfo;
            }
        }
        //--------------------------------------------------------
        string VaribleType = "";
        if(SyntaxysTag.Contains("@Varible"))
        {
            for (int i = 9; i < SyntaxysTag.Length - 1; i++)
            {
                VaribleType += SyntaxysTag[i];
            }
        }
        
        if(VaribleType != "") SyntaxysTag = SyntaxysTag.Substring(0,8);

        //--------------------------------------------------------
        string FunctionToCall = "";
        if(SyntaxysTag.Contains("#Call"))
        {
            for (int i = 6; i < SyntaxysTag.Length - 1; i++)
            {
                FunctionToCall += SyntaxysTag[i];
            }
        }
        
        if(FunctionToCall != "") SyntaxysTag = SyntaxysTag.Substring(0,5);

        //--------------------------------------------------------
        string NumberType = "";
        if(SyntaxysTag.Contains("@Number"))
        {
            for (int i = 8; i < SyntaxysTag.Length - 1; i++)
            {
                NumberType += SyntaxysTag[i];
            }
        }
        
        if(NumberType != "") SyntaxysTag = SyntaxysTag.Substring(0,7);

        //--------------------------------------------------------
        if(SyntaxysTag.Contains("@Symbol"))
        {
            List<Char> Symbols = new List<char>();
            
            for (int i = 7; i < SyntaxysTag.Length; i++)
            {
                if(SyntaxysTag[i] == '|' || i == SyntaxysTag.Length - 1) 
                {
                    Symbols.Add(SyntaxysTag[i - 1]); 
                }
            }
            syntaxHandleInfo.lastHandleIndex = StartPoint + 1;

            if(Symbols.Count > 1) syntaxHandleInfo.errorText ="One of the expected characters: ";
            else syntaxHandleInfo.errorText ="Character ";

            foreach (var Symbol in Symbols)
            {
                syntaxHandleInfo.errorText += "'" + Symbol + "'";
                if(FromString[StartPoint] == Symbol)
                {
                    syntaxHandleInfo.handledTagResult = Symbol.ToString();

                    if(Symbol == ';') syntaxHandleInfo.endOfLine = true;
                    syntaxHandleInfo.handleResult = true;
                    return syntaxHandleInfo;
                }
            }

            syntaxHandleInfo.errorText += " not found.";
            syntaxHandleInfo.handleResult = false;
            return syntaxHandleInfo;
        }
        //--------------------------------------------------------
        switch (SyntaxysTag)
        {
            case "@Space":
            int SpaceCount = 0;
            for (int i = StartPoint; i < FromString.Length;i++)
            {
                if(FromString[i] == ' ') SpaceCount++;
                else if(SpaceCount == 0)
                {
                    syntaxHandleInfo.lastHandleIndex = i;
                    syntaxHandleInfo.handleResult = false;
                    syntaxHandleInfo.errorText = "No space found.";
                    return syntaxHandleInfo;
                } 
                else
                {
                    syntaxHandleInfo.lastHandleIndex = i;
                    syntaxHandleInfo.handleResult = true;
                    return syntaxHandleInfo;
                } 
            }
            syntaxHandleInfo.lastHandleIndex = FromString.Length - 1;
            syntaxHandleInfo.handleResult = true;
            return syntaxHandleInfo;
            //--------------------------------------------------------
            case "@OSpace": // Optional space/spaces
            for (int i = StartPoint; i < FromString.Length;i++)
            {
                if(FromString[i] == ' ') continue;
                else
                {
                    syntaxHandleInfo.lastHandleIndex = i;
                    syntaxHandleInfo.handleResult = true;
                    return syntaxHandleInfo;
                }
            }
            syntaxHandleInfo.lastHandleIndex = FromString.Length - 1;
            syntaxHandleInfo.handleResult = true;
            return syntaxHandleInfo;
            //--------------------------------------------------------
            case "@OEscapeSymbol": // Optional escpae symbol/symbols
            for (int i = StartPoint; i < FromString.Length;i++)
            {
                if(FromString[i] == '\n' || FromString[i] == '\t') continue;
                else
                {
                    syntaxHandleInfo.lastHandleIndex = i;
                    syntaxHandleInfo.handleResult = true;
                    return syntaxHandleInfo;
                }
            }
            syntaxHandleInfo.lastHandleIndex = FromString.Length - 1;
            syntaxHandleInfo.handleResult = true;
            return syntaxHandleInfo;
            //--------------------------------------------------------
            case "@ScopeText":
            int ScopeEndCharCount = 0 , ScopeStartCharCount = 1;
            string Text = "";
            for (int i = StartPoint; i < FromString.Length;i++)
            {
                //Debug.Log(FromString[i] + " | " + ScopeStartCharCount + " | " + ScopeEndCharCount);
                if(FromString[i] == '{') ScopeStartCharCount++;
                if(FromString[i] == '}') ScopeEndCharCount++;
                if(ScopeEndCharCount == ScopeStartCharCount)
                {
                    syntaxHandleInfo.lastHandleIndex = i;
                    syntaxHandleInfo.handledTagResult = Text;
                    syntaxHandleInfo.handleResult = true;
                    return syntaxHandleInfo;
                } else Text += FromString[i];
                // Debug.Log(Text);
            }

            syntaxHandleInfo.handleResult = false;
            syntaxHandleInfo.errorText = "Scope is defined incorrectly.";
            return syntaxHandleInfo;
            //--------------------------------------------------------
            case "@DefineWord":
            string Word = "";

            for (int i = StartPoint; i < FromString.Length;i++)
            {
                bool IsWordEndCahr = FromString[i] == ' ' || 
                FromString[i] == ';' || 
                FromString[i] == '\n' || 
                FromString[i] == '=' || 
                FromString[i] == ')' || 
                FromString[i] == '}' || 
                FromString[i] == '{';
                if(IsWordEndCahr || i  == FromString.Length - 1)
                {
                    if(!IsWordEndCahr && i  == FromString.Length - 1) Word += FromString[i];

                    syntaxHandleInfo.lastHandleIndex = i;

                    foreach (var Instruction in _AvalibleInstructions)
                    {
                        if(Instruction.instructionText == Word)
                        {
                            syntaxHandleInfo.handleResult = false;
                            syntaxHandleInfo.errorText = "The keyword cannot be used as a definition word.";
                            return syntaxHandleInfo;
                        }
                    }

                    foreach (var Func in IDEManager.interpreter.interFunctions)
                    {
                        if(Func.name == Word)
                        {
                            syntaxHandleInfo.handleResult = false;
                            syntaxHandleInfo.errorText = "The function name cannot be used as a definition word.";
                            return syntaxHandleInfo;
                        }
                    }

                    if(Word.Length > 0 && Regex.IsMatch(Word, @"^[a-zA-Z_$][a-zA-Z_$0-9]*$"))
                    {
                        syntaxHandleInfo.handledTagResult = Word;

                        syntaxHandleInfo.handleResult = true;
                        return syntaxHandleInfo;
                    } else
                    {
                        syntaxHandleInfo.handleResult = false;
                        syntaxHandleInfo.errorText = "'" + Word + "'" + " word has an incorrect format.";
                        return syntaxHandleInfo;
                    }
                } else Word += FromString[i];
            }
            Debug.LogWarning("'@DefineWord' tag worked incorrectly");
            break;
            //--------------------------------------------------------
            case "@InclusionName":
            string InclusionName = "";

            for (int i = StartPoint; i < FromString.Length;i++)
            {
                bool IsWordEndCahr = (int)FromString[i] == 39; // ['] symbol

                if(IsWordEndCahr || i  == FromString.Length - 1)
                {
                    if(!IsWordEndCahr && i  == FromString.Length - 1) InclusionName += FromString[i];

                    syntaxHandleInfo.lastHandleIndex = i;

                    Dictionary<string,string> InclusionsData = IDEManager.inclusionsInfo.inclusionsData;
                    foreach (var Inclusion in InclusionsData)
                    {
                        if(Inclusion.Key == InclusionName)
                        {
                            syntaxHandleInfo.handledTagResult = InclusionName;

                            syntaxHandleInfo.handleResult = true;
                            return syntaxHandleInfo;
                        }
                    }

                    syntaxHandleInfo.handleResult = false;
                    syntaxHandleInfo.errorText = $"Unknown '{InclusionName}' include name.";
                    return syntaxHandleInfo;
                } else InclusionName += FromString[i];
            }
            Debug.LogWarning("'@InclusionName' tag worked incorrectly");
            break;
            case "@Number":
            string Expression = "";
            
            for (int i = StartPoint; i < FromString.Length;i++)
            {
                bool IsNumberEndChar = FromString[i] == ';' || FromString[i] == '\n';

                if(IsNumberEndChar || i == FromString.Length - 1)
                {
                    if(!IsNumberEndChar && i == FromString.Length - 1) Expression += FromString[i];

                    syntaxHandleInfo.lastHandleIndex = i;

                    Expression = ReplaceLogicalOperands(Expression);
                    if(Expression == null)
                    {
                        syntaxHandleInfo.handleResult = false;
                        syntaxHandleInfo.errorText = "Unknown expression.";
                        return syntaxHandleInfo;
                    }

                    try
                    {
                        object Result = ComputeExpression(Expression, Varibles);
                        string TextResult = Result.ToString();
                        bool IsWrongType = false;

                        switch (NumberType)
                        {
                            case "int":
                            IsWrongType = Result is not Int32;
                            break;
                            case "float":
                            IsWrongType = Result is Boolean; // because ComputedParam may be 0 or 1 or 2, and these numbers are int anf float at the same time.
                            break;
                            case "":
                            IsWrongType = true;
                            break;
                            default:
                            Debug.Log($"Unknown NumberType {NumberType}.");
                            break;
                        }

                        if(TextResult == "True" || TextResult == "False" || IsWrongType)
                        {
                            syntaxHandleInfo.handleResult = false;
                            syntaxHandleInfo.errorText = "Specified cast is not valid.";
                            return syntaxHandleInfo;
                        }

                        syntaxHandleInfo.handledTagResult = Expression;
                    }
                    catch (Exception ex)
                    {
                        syntaxHandleInfo.handleResult = false;
                        syntaxHandleInfo.errorText = ReplaceExceptionMessages(ex.Message);
                        return syntaxHandleInfo;
                    }

                    syntaxHandleInfo.handleResult = true;
                    return syntaxHandleInfo;
                } else Expression += FromString[i];
            }
            Debug.LogWarning("'@Number' tag worked incorrectly");
            break;
            //--------------------------------------------------------
            case "@LogicalValue":
            string LogicalExpression = "";
            
            for (int i = StartPoint; i < FromString.Length;i++)
            {
                bool IsNumberEndChar = FromString[i] == ';' || FromString[i] == '\n' || FromString[i] == '[' || FromString[i] == ']';

                if(IsNumberEndChar || i == FromString.Length - 1)
                {
                    if(!IsNumberEndChar && i == FromString.Length - 1) LogicalExpression += FromString[i];

                    syntaxHandleInfo.lastHandleIndex = i;
                    LogicalExpression = ReplaceLogicalOperands(LogicalExpression);
                    if(LogicalExpression == null)
                    {
                        syntaxHandleInfo.handleResult = false;
                        syntaxHandleInfo.errorText = "Unknown expression.";
                        return syntaxHandleInfo;
                    }

                    try
                    {                 
                        object Result = ComputeExpression(LogicalExpression, Varibles);
                        
                        string TextResult = Result.ToString();
                        if(TextResult != "True" && TextResult != "False")
                        {
                            syntaxHandleInfo.handleResult = false;
                            syntaxHandleInfo.errorText = "Specified cast is not valid.";
                            return syntaxHandleInfo;
                        }

                        syntaxHandleInfo.handledTagResult = LogicalExpression;
                    }
                    catch (Exception ex)
                    {
                        syntaxHandleInfo.handleResult = false;
                        syntaxHandleInfo.errorText = ReplaceExceptionMessages(ex.Message);
                        return syntaxHandleInfo;
                    }

                    syntaxHandleInfo.handleResult = true;
                    return syntaxHandleInfo;
                } else LogicalExpression += FromString[i];
            }
            Debug.LogWarning("'@LogicalValue' tag worked incorrectly");
            break;
            
            //--------------------------------------------------------
            case "#InitVar":
            syntaxHandleInfo.lastHandleIndex = StartPoint;
            if(LastHandledResult != null)
            {
                InitCommand.commandType = CommandType.Declaration;
                InitCommand.varibleName = LastHandledResult;
                InitCommand.expression = null;
                
                syntaxHandleInfo.handleResult = true;
                return syntaxHandleInfo;
            } 
            else
            {
                Debug.LogError("Unable to add command(tag: #InitVar): LastHandledResult is null");
                syntaxHandleInfo.handleResult = false;
                syntaxHandleInfo.errorText = "Unresolved variable name 'null'.";
                return syntaxHandleInfo;
            }
            //--------------------------------------------------------
            case "#SetStatic":
            syntaxHandleInfo.lastHandleIndex = StartPoint;

            InitCommand.isStaticVarible = true;
            
            syntaxHandleInfo.handleResult = true;
            return syntaxHandleInfo;
            //--------------------------------------------------------
            case "#SetConst":
            syntaxHandleInfo.lastHandleIndex = StartPoint;

            InitCommand.isConstVarible = true;
            
            syntaxHandleInfo.handleResult = true;
            return syntaxHandleInfo;
            //--------------------------------------------------------
            case "#Include":
            syntaxHandleInfo.lastHandleIndex = StartPoint;
            
            if(ExistInclusions.Contains(LastHandledResult))
            {
                syntaxHandleInfo.handleResult = false;
                syntaxHandleInfo.errorText = $"'{LastHandledResult}' already included";
                return syntaxHandleInfo;
            }

            string InclusionText = IDEManager.inclusionsInfo.inclusionsData[LastHandledResult];
            string InclusionSyntaxError;
            int InclusionErrorIndex;
            List<Command> CommandsFromInclude = ParseStringToCommands(InclusionText, 0, InclusionText.Length, out InclusionSyntaxError, out InclusionErrorIndex, PrintError: false);
            if(CommandsFromInclude == null)
            {
                syntaxHandleInfo.handleResult = false;
                syntaxHandleInfo.errorText = $"'{LastHandledResult}' include syntax error: " + InclusionSyntaxError;
                return syntaxHandleInfo;
            } else
            {
                Dictionary<string,string> VariblesFromInclusion = new Dictionary<string, string>();
                foreach (var Command in CommandsFromInclude)
                {
                    if(Command.commandType == CommandType.Declaration) VariblesFromInclusion.Add(Command.varibleName,Command.varibleType);
                }

                ExistInclusions.Add(LastHandledResult);

                InitCommand.commandType = CommandType.Empty;
                syntaxHandleInfo.InclusionVaribles = VariblesFromInclusion;
                syntaxHandleInfo.handledCommands = CommandsFromInclude;
                syntaxHandleInfo.handleResult = true;
                return syntaxHandleInfo;
            }
            //--------------------------------------------------------
            case "#AssignValue":
            syntaxHandleInfo.lastHandleIndex = StartPoint;
            if(LastHandledResult != null)
            {
                if(InitCommand.commandType == CommandType.Empty) InitCommand.commandType = CommandType.Assigment;
                InitCommand.expression = LastHandledResult;

                syntaxHandleInfo.handleResult = true;
                return syntaxHandleInfo;
            } 
            else
            {
                Debug.LogError("Unable to add command(tag: #AssignValue): LastHandledResult is null");
                syntaxHandleInfo.handleResult = false;
                syntaxHandleInfo.errorText = "Unresolved 'null' value.";
                return syntaxHandleInfo;
            }
            //--------------------------------------------------------
            case "@Varible":
            string Varible = "";

            for (int i = StartPoint; i < FromString.Length;i++)
            {
                bool IsWordEndCahr = FromString[i] == ' ' || FromString[i] == ';' || FromString[i] == '\n' || FromString[i] == '=' || FromString[i] == ')' || FromString[i] == ',';
                if(IsWordEndCahr || i  == FromString.Length - 1)
                {
                    if(!IsWordEndCahr && i  == FromString.Length - 1) Varible += FromString[i];

                    syntaxHandleInfo.lastHandleIndex = i;

                    // foreach (var Instruction in _AvalibleInstructions)
                    // {
                    //     if(Instruction.instructionText == Varible)
                    //     {
                    //         syntaxHandleInfo.handleResult = false;
                    //         syntaxHandleInfo.errorText = "The keyword cannot be used as a varible.";
                    //         return syntaxHandleInfo;
                    //     }
                    // }

                    // foreach (var Func in IDEManager.interpreter.interFunctions)
                    // {
                    //     if(Func.name == Varible)
                    //     {
                    //         syntaxHandleInfo.handleResult = false;
                    //         syntaxHandleInfo.errorText = "The function name cannot be used as a varible.";
                    //         return syntaxHandleInfo;
                    //     }
                    // }

                    if(FindVarible(Varible, Varibles, VaribleType))
                    {
                        syntaxHandleInfo.handledTagResult = Varible;

                        syntaxHandleInfo.handleResult = true;
                        return syntaxHandleInfo;
                    } else
                    {
                        syntaxHandleInfo.handleResult = false;
                        if(Varible == "(") syntaxHandleInfo.errorText = $"Variable not found."; // There may be an unexpected error output | TODO handle this situation
                        else if(VaribleType == "") syntaxHandleInfo.errorText = $"'{Varible}' varible doesn't exist in current context.";
                        else syntaxHandleInfo.errorText = $"'{Varible}' variable doesn't match the '{VaribleType}' type or doesn't exist in current context.";
                        return syntaxHandleInfo;
                    }
                } else Varible += FromString[i];
            }
            Debug.LogWarning("'@Varible' tag worked incorrectly");
            break;
            //--------------------------------------------------------
            case "#UseVar":
            syntaxHandleInfo.lastHandleIndex = StartPoint;
            if(LastHandledResult != null)
            {
                InitCommand.varibleName = LastHandledResult;

                syntaxHandleInfo.handleResult = true;
                return syntaxHandleInfo;
            } 
            else
            {
                Debug.LogError("Unable to add command(tag: #UseVar): LastHandledResult is null");
                syntaxHandleInfo.handleResult = false;
                syntaxHandleInfo.errorText = "Unresolved 'null' value.";
                return syntaxHandleInfo;
            }
            //--------------------------------------------------------
            case "#PrintVar":
            syntaxHandleInfo.lastHandleIndex = StartPoint;
            if(LastHandledResult != null)
            {
                InitCommand.varibleName = LastHandledResult;
                InitCommand.commandType = CommandType.Print;

                syntaxHandleInfo.handleResult = true;
                return syntaxHandleInfo;
            } 
            else
            {
                Debug.LogError("Unable to add command(tag: #PrintVar): LastHandledResult is null");
                syntaxHandleInfo.handleResult = false;
                syntaxHandleInfo.errorText = "Can't print 'null'.";
                return syntaxHandleInfo;
            }
            //--------------------------------------------------------
            case "#ProcessCondition":
            syntaxHandleInfo.lastHandleIndex = StartPoint;
            if(LastHandledResult != null)
            {
                InitCommand.expression = LastHandledResult;
                InitCommand.commandType = CommandType.Validate;

                syntaxHandleInfo.handleResult = true;
                return syntaxHandleInfo;
            } 
            else
            {
                Debug.LogError("Unable to add command(tag: #ProcessCondition): LastHandledResult is null");
                syntaxHandleInfo.handleResult = false;
                syntaxHandleInfo.errorText = "Can't process the condition of the if statement.";
                return syntaxHandleInfo;
            }
            //--------------------------------------------------------
            case "#ProcessIfStatement":
            syntaxHandleInfo.lastHandleIndex = StartPoint;
            syntaxHandleInfo.handledCommands = new List<Command>();

            if(LastHandledResult != null)
            {
                var ExistVaribles = new Dictionary<string, string>(Varibles);
                string ErrorFromScope = "";
                int ErrorIndex = 0;

                syntaxHandleInfo.handledCommands = 
                ParseStringToCommands(FromString, StartPoint - LastHandledResult.Length, StartPoint - 1, out ErrorFromScope, out ErrorIndex, PrintError: false, ExistInclusions, ExistVaribles);
                
                if(syntaxHandleInfo.handledCommands == null)
                {
                    syntaxHandleInfo.handleResult = false;
                    syntaxHandleInfo.errorText = ErrorFromScope;
                    syntaxHandleInfo.errorIndex = ErrorIndex;
                    return syntaxHandleInfo;
                } 

                InitCommand.commnadsToSkip = syntaxHandleInfo.handledCommands.Count;
                InitCommand.commandType = CommandType.IfStatement;

                syntaxHandleInfo.handleResult = true;
                return syntaxHandleInfo;
            } 
            else
            {
                Debug.LogError("Unable to add command(tag: #ProcessIfStatement): LastHandledResult is null");
                syntaxHandleInfo.handleResult = false;
                syntaxHandleInfo.errorText = "Can't process the if statement.";
                return syntaxHandleInfo;
            }
            //--------------------------------------------------------
            case "#ProcessWhileStatement":
            syntaxHandleInfo.lastHandleIndex = StartPoint;
            syntaxHandleInfo.handledCommands = new List<Command>();
            if(LastHandledResult != null)
            {
                var ExistVaribles = new Dictionary<string, string>(Varibles);
                string ErrorFromScope = "";
                int ErrorIndex = 0;

                syntaxHandleInfo.handledCommands = 
                ParseStringToCommands(FromString, StartPoint - LastHandledResult.Length, StartPoint - 1, out ErrorFromScope, out ErrorIndex, PrintError: false, ExistInclusions, ExistVaribles);

                if(syntaxHandleInfo.handledCommands == null)
                {
                    syntaxHandleInfo.handleResult = false;
                    syntaxHandleInfo.errorText = ErrorFromScope;
                    syntaxHandleInfo.errorIndex = ErrorIndex;
                    return syntaxHandleInfo;
                } 

                InitCommand.commnadsToSkip = syntaxHandleInfo.handledCommands.Count;
                InitCommand.commandType = CommandType.WhileStatement;

                syntaxHandleInfo.handleResult = true;
                return syntaxHandleInfo;
            } 
            else
            {
                Debug.LogError("Unable to add command(tag: #ProcessWhileStatement): LastHandledResult is null");
                syntaxHandleInfo.handleResult = false;
                syntaxHandleInfo.errorText = "Can't process the process while statement.";
                return syntaxHandleInfo;
            }
            //--------------------------------------------------------
            case "#AddParam":
            syntaxHandleInfo.lastHandleIndex = StartPoint;
            if(LastHandledResult != null)
            {
                InitCommand.funcParams.Add(LastHandledResult);

                syntaxHandleInfo.handleResult = true;
                return syntaxHandleInfo;
            } 
            else
            {
                Debug.LogError("Unable to add command(tag: #AddParam): LastHandledResult is null");
                syntaxHandleInfo.handleResult = false;
                syntaxHandleInfo.errorText = "The parameter cannot be used.";
                return syntaxHandleInfo;
            }
            //--------------------------------------------------------
            case "#Call":
            syntaxHandleInfo.lastHandleIndex = StartPoint;
            syntaxHandleInfo.handleResult = true;
            switch (FunctionToCall)
            {
                case "PMove":
                if(InitCommand.funcParams.Count > 0)
                {
                    InitCommand.commandType = CommandType.PMove;
                    return syntaxHandleInfo;
                } else break;
                case "PRotateY":
                if(InitCommand.funcParams.Count > 0)
                {
                    InitCommand.commandType = CommandType.PRotateY;
                    return syntaxHandleInfo;
                } else break;
            }

            Debug.LogError($"Unable to add command(tag: #Call{FunctionToCall})");
            syntaxHandleInfo.handleResult = false;
            syntaxHandleInfo.errorText = $"Can't call {FunctionToCall}.";
            return syntaxHandleInfo;
            //--------------------------------------------------------
            case "~DeleteLTR": // Delete last tag result
            syntaxHandleInfo.lastHandleIndex = StartPoint;
            if(LastHandledResult != null)
            {
                FromString = FromString.Remove(StartPoint - 1, 1).Insert(StartPoint - 1, " ");
                syntaxHandleInfo.handleResult = true;
                return syntaxHandleInfo;
            } 
            else
            {
                Debug.LogError("Unable to add command(tag: ~DeleteLTR): LastHandledResult is null");
                syntaxHandleInfo.handleResult = false;
                syntaxHandleInfo.errorText = "Internal error.";
                return syntaxHandleInfo;
            }
            //--------------------------------------------------------
            default:
            Debug.LogError("Unknown syntax tag: " + SyntaxysTag);
            break;
        }
        syntaxHandleInfo.lastHandleIndex = FromString.Length - 1;
        syntaxHandleInfo.handleResult = false;
        syntaxHandleInfo.errorText = "Unexpected line ending.";
        return syntaxHandleInfo;
    }
    public bool CheckSyntaxByInstruction(string TargetString, Instruction ByInstruction, out string ErrorText, Dictionary<string,string> Varibles = null)
    {
        if(Varibles == null) Varibles = new Dictionary<string, string>();

        int SyntaxTagСonfirmed = 0;
        int InstructionReadStart = 0;
        int SyntaxTagRequired = ByInstruction.syntaxTags.Length;
        
        List<Command> OutputCommands = new List<Command>();

        List<int> TagsLastHandleIndex = new List<int>();
        SyntaxHandleInfo SyntaxHandleInfo = new SyntaxHandleInfo();
        List<string> HandledTagResults = new List<string>();

        Command CurrentCommand = new Command();

        foreach (string SyntaxTag in ByInstruction.syntaxTags)
        {
            if(SyntaxTag[0] == '#')
            {
                Debug.LogWarning("Сommands for the interpreter are prohibited!");
                ErrorText = "Сommands for the interpreter are prohibited!";
                return false;
            }
            List<string> Includes = new List<string>();
            SyntaxHandleInfo = HandleSyntaxTag(SyntaxTag, Varibles, Includes, ref TargetString, InstructionReadStart, ref CurrentCommand, HandledTagResults.LastOrDefault(), TagsLastHandleIndex);
            if(SyntaxHandleInfo.handleResult)
            {
                SyntaxTagСonfirmed++;
                InstructionReadStart = SyntaxHandleInfo.lastHandleIndex;
                TagsLastHandleIndex.Add(SyntaxHandleInfo.lastHandleIndex);
                if(SyntaxHandleInfo.endOfLine)
                {
                    SyntaxTagRequired = SyntaxTagСonfirmed;
                    break;
                }
            } else break;
        }

        if(SyntaxTagСonfirmed == SyntaxTagRequired)
        {
            ErrorText = null;
            return true;
        } 
        else
        {
            ErrorText = SyntaxHandleInfo.errorText;
            return false;
        }
    }
    private string ReplaceFunctions(string Expression)
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
                foreach (var Func in Functions)
                {
                    //if(FuncText[FuncText.Length - 1] != ')') break;
                    
                    if(Regex.IsMatch(FuncText, String.Format(@"\b{0}\b", Func.name)))
                    {
                        for (int j = i; j < Expression.Length; j++)
                        {
                            FuncText += Expression[j];
                            if(Expression[j] != ')' && j == Expression.Length - 1) throw new Exception("Missing closing parenthesis.");
                            
                            if(Expression[j] == ')')
                            {
                                string TextInBrackets = FuncText.Substring(Func.name.Length);
                                foreach (var FuncInBrackets in Functions)
                                {
                                    if(Regex.IsMatch(TextInBrackets, String.Format(@"\b{0}\b", FuncInBrackets.name))) throw new Exception("Invalid parameter in the function");
                                }

                                foreach (var Instruction in _AvalibleInstructions)
                                {
                                    if(Regex.IsMatch(TextInBrackets, String.Format(@"\b{0}\b", Instruction.instructionText))) throw new Exception("Invalid parameter in the function");
                                }

                                List<string> Params = new List<string>();
                                string CurParam = "";
                                for (int t = 1; t < TextInBrackets.Length - 1; t++)
                                {
                                    if(TextInBrackets[t] == ',' || t == TextInBrackets.Length - 2)
                                    {
                                        if(t == TextInBrackets.Length - 2) CurParam += TextInBrackets[t];

                                        Params.Add(CurParam);
                                        CurParam = "";
                                    } else CurParam += TextInBrackets[t];
                                }
                                if(Func.acceptedValuesCount != Params.Count) throw new Exception($"Wrong accepted values count in function '{Func.name}'.");
                                for (int p = 0; p < Params.Count; p++)
                                {
                                    object ComputedParam = ComputeExpression(Params[p], null);
                                    switch (Func.acceptedValues[p])
                                    {
                                        case "int":
                                        if(ComputedParam is not Int32) throw new Exception("Specified cast is not valid.");
                                        break;
                                        case "float":
                                        if(ComputedParam is Boolean) throw new Exception("Specified cast is not valid."); // because ComputedParam may be 0 or 1 or 2, and these numbers are int anf float at the same time.
                                        break;
                                        case "bool":
                                        if(ComputedParam is not Boolean) throw new Exception("Specified cast is not valid.");
                                        break;
                                        default:
                                        Debug.LogError($"Unknown func accepted value type '{Func.acceptedValues[p]}'");
                                        break;
                                    }
                                }
                                
                                i = j;
                                break;
                            }
                        }

                        switch (Func.returnValue)
                        {
                            case "int":
                            Out = Out.Replace(FuncText,"1");
                            break;
                            case "float":
                            Out = Out.Replace(FuncText,"1.0");
                            break;
                            case "bool":
                            Out = Out.Replace(FuncText,"False");
                            break;
                            default:
                            Debug.LogError($"Unknown function {FuncText}");
                            break;
                        }
                        
                    }
                }

                FuncStartIndex = i;
                FuncText = "";
            } else FuncText += Expression[i];
        }

        return Out;
    }
    private string ReplaceVaribles(string Expression, Dictionary<string,string> Varibles)
    {
        string Out = Expression;

        foreach (var Var  in Varibles)
        {
            string VarNameRegEx = String.Format(@"\b{0}\b", Var.Key);
            switch (Var.Value)
            {
                case "int":
                Out =  Regex.Replace(Out, VarNameRegEx, "1");
                break;
                case "float":
                Out =  Regex.Replace(Out, VarNameRegEx, "1.0");
                break;
                case "bool":
                Out =  Regex.Replace(Out, VarNameRegEx, "False");
                break;
                default:
                Debug.LogError($"Unknown varible {Var.Value}");
                break;
            }
        }
        return Out;
    }
    public int GetCharLine(int CharIndex, string TargetString)
    {
        int LinesCount = 1;
        for (int i = CharIndex; i >= 0; i--)
        {
            if(TargetString[i] == '\n')
            {
                LinesCount++;
            }
        }
        return LinesCount;
    }
    private bool FindVarible(string Name, Dictionary<string,string> Varibles, string VaribleType)
    {
        foreach (var VaribleName in Varibles)
        {
            if(VaribleName.Key == Name)
            {
                if(VaribleType == "") return true;
                else if(VaribleName.Value == VaribleType) return true;
            }
        }
        return false;
    }
    private bool TryGetInstructionByWord(string Word, out Instruction instruction, Dictionary<string,string> Varibles)
    {
        foreach (Instruction ins in _AvalibleInstructions)
        {
            List<string> InstructionVariants = new List<string>();
            switch (ins.instructionText)
            {
                case "@Varible":
                foreach (var Var in Varibles)
                {
                    InstructionVariants.Add(Var.Key);
                }
                break;
                case "@Varible(int)":
                foreach (var Var in Varibles)
                {
                    if(Var.Value == "int") InstructionVariants.Add(Var.Key);
                }
                break;
                case "@Varible(float)":
                foreach (var Var in Varibles)
                {
                    if(Var.Value == "float") InstructionVariants.Add(Var.Key);
                }
                break;
                case "@Varible(bool)":
                foreach (var Var in Varibles)
                {
                    if(Var.Value == "bool") InstructionVariants.Add(Var.Key);
                }
                break;
                default:
                InstructionVariants.Add(ins.instructionText);
                break;
            }
            
            foreach (var Variant in InstructionVariants)
            {
                if(Variant == Word)
                {
                    instruction = ins;
                    return true;
                }
            }
        }
        instruction = null;
        return false;
    }
    private string ReplaceLogicalOperands(string Expression)
    {
        string Exp = Expression;
        // change '==' to '=', because datatable use '=' to compare
        int EqualsSymbolCount = 0;
        for (int j = 0; j < Exp.Length; j++)
        {
            if(Exp[j] == '=')
            {
                EqualsSymbolCount++;
            }
            bool IsOperatorEnd = j + 1 >= Exp.Length || Exp[j + 1] != '=';
            if(EqualsSymbolCount == 2 && IsOperatorEnd)
            {
                Exp = Exp.Remove(j, 1);
                EqualsSymbolCount = 0;
            } else if(EqualsSymbolCount >= 1 && IsOperatorEnd)
            {
                return null;
            }
        }
        
        // also for 'not'
        Exp = Exp.Replace("!"," not ");
        
        return Exp;
    }
    private object ComputeExpression(string Expression, Dictionary<string,string> Varibles)
    {
        string ReplacedExpression;
        if(Varibles is null) ReplacedExpression = ReplaceFunctions(Expression);
        else ReplacedExpression = ReplaceFunctions(ReplaceVaribles(Expression, Varibles));

        object ComputedNumber = new DataTable().Compute(ReplacedExpression, null);
        if(ComputedNumber.ToString().Length == 0) throw new Exception( "'" + Expression +  "'" + " Number has an incorrect format or the expression cannot be evaluated.");
        int DataHash = ComputedNumber.GetHashCode();
        if(DataHash == 2146435072 || DataHash == -1048576) throw new Exception("Attempted to divide by zero.");
        return ComputedNumber;
    }
    string ReplaceExceptionMessages(string Message)
    {
        string TempMessage = Message;
        if(Message.Contains("Cannot find column") || Message.Contains("Missing operand") || Message.Contains("Cannot interpret token")) TempMessage = "Unknown expression.";
        else if(Message.Contains("Value was either too large or too small for a Single.")) TempMessage = "Value was either too large or too small for a Float.";
        else if(Message.Contains("Value is either too large or too small for Type 'Int32'")) TempMessage = "Value was either too large or too small for a Int.";
        else if(Message.Contains("Cannot perform '*' operation on System.Int32 and System.String.") || 
        Message.Contains("Cannot perform '*' operation on System.String and System.Int32.") ) TempMessage = "Value was either too large or too small";
        else if(Message.Contains("Cannot perform 'Mod' operation")) TempMessage = "It is impossible to compare.";
        else if(Message.Contains("Cannot perform")) TempMessage = "Cannot perform this operation";
        
        return TempMessage;
    }
    private class SyntaxHandleInfo
    {
        public bool handleResult { get; set; }
        public string errorText { get; set; }
        public int errorIndex { get; set; } = -1;
        public int lastHandleIndex { get; set; }
        public bool endOfLine { get; set; } = false;
        public string handledTagResult { get; set; } = null;
        public List<Command> handledCommands { get; set; }
        public Dictionary<string,string> InclusionVaribles { get; set; }
    }
}

public class Instruction
{
    private string _InstructionText;
    public string instructionText
    {
        get { return _InstructionText; }
    }
    private string[] _SyntaxTags;
    public string[] syntaxTags
    {
        get { return _SyntaxTags; }
    }
    private string _Type = "";
    public string type
    {
        get { return _Type; }
    }
    private int _CommandsCount;
    public int commandsCount
    {
        get { return _CommandsCount; }
    }
    public Instruction(string InstructionText, string Type)
    {
        _InstructionText = InstructionText;
        _Type = Type;

        _CommandsCount = 0;
    }
    public Instruction(string InstructionText, string Type, string[] SyntaxTags)
    {
        _InstructionText = InstructionText;
        _SyntaxTags = SyntaxTags;
        _Type = Type;

        if(SyntaxTags is not null) _CommandsCount = GetCommandsCount(SyntaxTags);
        else _CommandsCount = 0;
    }
    public Instruction(string InstructionText, string[] SyntaxTags)
    {
        _InstructionText = InstructionText;
        _SyntaxTags = SyntaxTags;

        if(SyntaxTags is not null) _CommandsCount = GetCommandsCount(SyntaxTags);
        else _CommandsCount = 0;
    }
    int GetCommandsCount(string[] SyntaxTags)
    {
        int CommandsCount = 0;
        foreach (var SyntaxTag in SyntaxTags)
        {
            if(SyntaxTag[0] == '#') CommandsCount++;
        }
        return CommandsCount;
    }
}

public enum CommandType
{
    Empty,
    Declaration,
    Assigment,
    Validate,
    IfStatement,
    WhileStatement,
    Repeat,
    Dispose,
    Print,
    PMove,
    PRotateY
}
public class Command 
{
    private CommandType _CommandType = CommandType.Empty;
    public CommandType commandType
    {
        get { return _CommandType; }
        set { _CommandType = value; }
    }
    private string _VaribleType = "";
    public string varibleType
    {
        get { return _VaribleType; }
        set { _VaribleType = value; }
    }
    private bool _IsStaticVarible = false;
    public bool isStaticVarible
    {
        get { return _IsStaticVarible; }
        set { _IsStaticVarible = value; }
    }
    private bool _IsConstVarible = false;
    public bool isConstVarible
    {
        get { return _IsConstVarible; }
        set { _IsConstVarible = value; }
    }
    
    private string _VaribleName = null;
    public string varibleName
    {
        get { return _VaribleName; }
        set { _VaribleName = value; }
    }
    private string _Expression = null;
    public string expression
    {
        get { return _Expression; }
        set { _Expression = value; }
    }
    private int _CallLine;
    public int callLine
    {
        get { return _CallLine; }
        set { _CallLine = value; }
    }
    private int _CommnadsToSkip;
    public int commnadsToSkip
    {
        get { return _CommnadsToSkip; }
        set { _CommnadsToSkip = value; }
    }
    private int _CommnadsToRepeat;
    public int commnadsToRepeat
    {
        get { return _CommnadsToRepeat; }
        set { _CommnadsToRepeat = value; }
    }
    private List<Command> _CommnadsToDispose;
    public List<Command> commnadsToDispose
    {
        get { return _CommnadsToDispose; }
        set { _CommnadsToDispose = value; }
    }
    private List<string> _FuncParams = new List<string>();
    public List<string> funcParams
    {
        get { return _FuncParams; }
        set { _FuncParams = value; }
    }
    
    static public int GetUsedMemory(Command Cmd)
    {
        if(Cmd.commandType != CommandType.Declaration) return 0;

        switch (Cmd.varibleType)
        {
            case "bool":
            return 1;
            case "int":
            return 2;
            case "float":
            return 4;
            default:
            Debug.LogError("Unknown varible type.");
            return 0;
        }
    }
    static public int GetUsedMemory(List<Command> Cmds)
    {
        int MemoryUsed = 0;
        foreach (var Cmd in Cmds)
        {
            MemoryUsed += GetUsedMemory(Cmd);
        }
        return MemoryUsed;
    }
    
    public Command(CommandType TypeOfCommand, string VaribleName, string VaribleType, string Expression)
    {
        _CommandType = TypeOfCommand;
        _VaribleName = VaribleName;
        _Expression = Expression;
        _VaribleType = VaribleType;
    }
    public Command(CommandType TypeOfCommand, List<Command> CommnadsToDispose, int CallLine)
    {
        _CommandType = TypeOfCommand;
        _CommnadsToDispose = new List<Command>(CommnadsToDispose);
        _CallLine = CallLine;
    }
    public Command(CommandType TypeOfCommand, List<Command> CommnadsToDispose, int CommnadsToRepeat, string Expression, int CallLine)
    {
        _CommandType = TypeOfCommand;
        _CommnadsToRepeat = CommnadsToRepeat;
        _Expression = Expression;
        _CallLine = CallLine;
        _CommnadsToDispose = new List<Command>(CommnadsToDispose);
    }
    public Command()
    {
  
    }
}
