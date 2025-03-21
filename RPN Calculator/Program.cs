
using System.Globalization;

while (true)
{
    Console.WriteLine("Please enter an arithmetic expression.");
    string input = Console.ReadLine() ?? "";

    if (string.IsNullOrEmpty(input)) break;
    string rpn = ShuntingYard.ConvertToRpn(input);
    Console.WriteLine(RpnCalculator.ReadRpn(rpn));


}



internal static class RpnCalculator
{
    /// <summary>
    /// Reads an arithmetic string in space delimited RPN format (1 2 +) and returns the evaluation
    /// </summary>
    /// <param name="s">Space delimited RPN arithmetic string</param>
    /// <returns>String evaluation of the input arithmetic</returns>
    public static string ReadRpn(string s)
    {
        Console.WriteLine($"RPN string = {s}");
        char[] sp = [' ', '\t']; //Delimiters to split on


        Stack<string> tks = new Stack<string>
            (s.Split(sp, StringSplitOptions.RemoveEmptyEntries)); //Make a stack of strings from the input string
        try
        {
            double r = EvalRpn(tks); //Send stack to Eval and get back answer
            if (tks.Count != 0) throw new Exception(); //Token stack should be empty by here
            return r.ToString(CultureInfo.CurrentCulture); //Return total as string, using current culture
        }
        catch (Exception)
        {
            return "error"; //Stack not empty. Shame.
        }
    }
    /// <summary>
    /// Evaluates a stack of RPN tokens and returns an integer result
    /// </summary>
    /// <param name="tks">Stack of tokens (strings)</param>
    /// <returns>Integer representing the evaluated value of the RPN</returns>
    /// <exception cref="Exception">Throws exception if unknown token is found</exception>
    private static double EvalRpn(Stack<string> tks)
    {
        string tk = tks.Pop(); //Immediately take token off top
        if (!double.TryParse(tk, out var x)) //If we found a token, get to work
        {
            var y = EvalRpn(tks); x = EvalRpn(tks); //Recursive call to get next two numbers.
            if (tk == "+") x += y;
            else if (tk == "-") x -= y;
            else if (tk == "*") x *= y;
            else if (tk == "/") x /= y;
            else if (tk == "^") x = Math.Pow(x, y);
            else throw new Exception(); //Unknown symbol
        }
        return x; //Return the Number token or the final eval
    }
}

internal static class ShuntingYard
{
    private static readonly string Operators = "+-*/^";
    private static readonly string Numbers = "1234567890";
    private static readonly Dictionary<char, int> PriorityDictionary = new Dictionary<char, int>()
    {
        {'^', 3},
        {'*', 2},
        {'/', 2},
        {'+', 1},
        {'-', 1},
        {'(', 0},
        {')', 0},
    };
    /// <summary>
    /// Enum to use in AssociativityDictionary
    /// </summary>
    private enum Associativity
    {
        Left,
        Right
    }
    /// <summary>
    /// Dictionary for symbol's association (right or left)
    /// </summary>
    private static readonly Dictionary<char, Associativity> AssociativityDictionary = new Dictionary<char, Associativity>()
    {
        {'+', Associativity.Left},
        {'-', Associativity.Left},
        {'*', Associativity.Left},
        {'/', Associativity.Left},
        {'^', Associativity.Right}
    };
    /// <summary>
    /// Converts an Infix arithmetic string to Postfix (RPN)
    /// </summary>
    /// <param name="input">The arithmetic input string</param>
    /// <returns>String of arithmetic in Postfix (RPN) format</returns>
    /// <exception cref="ArgumentException">Throws if input is empty or null</exception>
    /// <exception cref="Exception">Generic exceptions with message</exception>
    /// <exception cref="FormatException">Input string does not adhere to grammar</exception>
    public static string ConvertToRpn(string input)
    {
        if (string.IsNullOrEmpty(input)) throw new ArgumentException("input is empty");

        var tokens = string.Concat(input.Where(c => !char.IsWhiteSpace(c))); //sanitize the input with LINQ

        var output = new List<char>();//Final output
        var operatorStack = new Stack<char>();
        var number = string.Empty;//Temporary buffer to handle multi-digit numbers
        var enumerableInfixTokens = new List<string>();//List to add infix tokens before translation

        foreach (char c in tokens) //For each character in the string
        {
            if (Operators.Contains(c) || "()".Contains(c)) //If character is an operator
            {
                if (number.Length > 0) //If we hit a symbol and number remaining in buffer, empty buffer into list as single token
                {
                    enumerableInfixTokens.Add(number);
                    number = string.Empty;
                }
                enumerableInfixTokens.Add(c.ToString());
            }
            else if (Numbers.Contains(c)) //If character is a number, convert to string and add to number buffer

            {
                number += c.ToString();
            }
            else
            {
                throw new Exception($"Unexpected character {c}"); //Not operator, not number. what is it?
            }
        }

        if (number.Length > 0)// If there are numbers left in buffer, add them to end
        {
            enumerableInfixTokens.Add(number);
        }


        foreach (string token in enumerableInfixTokens)//For each infix token
        {
            if (IsNum(token))//If token is a number, add to output
            {
                AddToOutput(output, token.ToArray());
            }
            else if (token.Length == 1)//If token has length of 1, can only be single digit or a symbol
            {
                char c = token[0];
                if (Numbers.Contains(c)) AddToOutput(output, c); //This should've been checked already, but just in case.
                else if (Operators.Contains(c))
                {
                    if (operatorStack.Count > 0) //For most operators, just follow the rules of Association and Priority before adding.
                    {
                        char peek = operatorStack.Peek();
                        if ((AssociativityDictionary[c] == Associativity.Left &&
                             PriorityDictionary[c] <= PriorityDictionary[peek]) ||
                            AssociativityDictionary[c] == Associativity.Right &&
                            PriorityDictionary[c] < PriorityDictionary[peek])
                        {
                            AddToOutput(output, operatorStack.Pop());
                        }
                    }
                    operatorStack.Push(c);
                }
                else if (c == '(') //Easy, just add
                {
                    operatorStack.Push(c);
                }
                else if (c == ')') //Less easy, keep adding. Note we don't ever add closing parenthesis to the output
                {
                    bool leftFound = false;
                    while (operatorStack.Count > 0)//While op stack not empty
                    {
                        char peek = operatorStack.Peek();
                        if (peek != '(')
                        {
                            AddToOutput(output, operatorStack.Pop()); //Add operators to output until we find the open parenthesis
                        }
                        else
                        {
                            operatorStack.Pop();//Pop out the open parenthesis, set found flag
                            leftFound = true;
                            break;
                        }
                    }

                    if (!leftFound)
                    {
                        throw new FormatException("Parenthesis mismatch");
                    }
                }
                else
                {
                    throw new FormatException($"Unrecognized character {c}");
                }
            }
            else
            {
                throw new Exception($"Token {token} is not numeric and has length > 1");
            }
        }

        while (operatorStack.Count > 0) //Almost done. If anything left in operator stack, add it to the end
        {
            char o = operatorStack.Pop();
            if ("()".Contains(o)) //Unless it's parenthesis. Then it's wrong.
            {
                throw new FormatException("Mismatched parenthesis. (Extra parentheses)");
            }
            else
            {
                AddToOutput(output, o);
            }
        }



        return new string(output.ToArray());
    }
    /// <summary>
    /// Add range of chars to output list of chars, unless null or empty. Helper function.
    /// </summary>
    /// <param name="output">Target output list of chars</param>
    /// <param name="chars">Input list of chars (nullable)</param>
    private static void AddToOutput(List<char> output, params char[]? chars)
    {
        if (chars is not { Length: > 0 }) return;
        output.AddRange(chars);
        output.Add(' '); // Add space after as delimiter. Useful for reading later.
    }

    /// <summary>
    /// Check if all characters of a given string are numbers (digits)
    /// </summary>
    /// <param name="symbol">input string</param>
    /// <returns>Boolean. True if all chars are digits. False otherwise.</returns>
    private static bool IsNum(string symbol)
    {
        return symbol.All(char.IsDigit);
    }


}

