
while (true)
{
    Console.WriteLine("Please enter an arithmetic expression.");
    string input = Console.ReadLine() ?? "";

    if (string.IsNullOrEmpty(input)) break;
    string rpn = ShuntingYard.ConvertToRpn(input);
    Console.WriteLine(RpnCalculator.ReadRpn(rpn));


}



class RpnCalculator
{
    public static string ReadRpn(string s)
    {
        char[] sp = new char[] { ' ', '\t' };
        for (; ; )
        {
            /*string? s = Console.ReadLine();
            if (s == null) break;*/
            Stack<string> tks = new Stack<string>
                (s.Split(sp, StringSplitOptions.RemoveEmptyEntries));
            if (tks.Count == 0) continue;
            try
            {
                double r = EvalRpn(tks);
                if (tks.Count != 0) throw new Exception();
                return r.ToString();
            }
            catch (Exception) { Console.WriteLine("error"); }
        }
    }

    private static double EvalRpn(Stack<string> tks)
    {
        string tk = tks.Pop();
        double x, y;
        if (!Double.TryParse(tk, out x))
        {
            y = EvalRpn(tks); x = EvalRpn(tks);
            if (tk == "+") x += y;
            else if (tk == "-") x -= y;
            else if (tk == "*") x *= y;
            else if (tk == "/") x /= y;
            else throw new Exception();
        }
        return x;
    }
}

class ShuntingYard
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

    private enum Associativity
    {
        Left,
        Right
    }

    private static readonly Dictionary<char, Associativity> AssociativityDictionary = new Dictionary<char, Associativity>()
    {
        {'+', Associativity.Left},
        {'-', Associativity.Left},
        {'*', Associativity.Left},
        {'/', Associativity.Left},
        {'^', Associativity.Right}
    };
    public static string ConvertToRpn(string input)
    {
        if (string.IsNullOrEmpty(input)) throw new ArgumentException("input is empty");

        var tokens = string.Concat(input.Where(c => !char.IsWhiteSpace(c))); //sanitize the input with LINQ

        var output = new List<char>();
        var operatorStack = new Stack<char>();
        var number = string.Empty;
        var enumerableInfixTokens = new List<string>();

        foreach (char c in tokens)
        {
            if (Operators.Contains(c) || "()".Contains(c))
            {
                if (number.Length > 0)
                {
                    enumerableInfixTokens.Add(number);
                    number = string.Empty;
                }
                enumerableInfixTokens.Add(c.ToString());
            }
            else if (Numbers.Contains(c))

            {
                number += c.ToString();
            }
            else
            {
                throw new Exception($"Unexpected character {c}");
            }
        }

        if (number.Length > 0)
        {
            enumerableInfixTokens.Add(number);
            number = string.Empty;
        }

        foreach (string token in enumerableInfixTokens)
        {
            if (IsNum(token))
            {
                AddToOutput(output, token.ToArray());
            }
            else if (token.Length == 1)
            {
                char c = token[0];
                if (Numbers.Contains(c)) AddToOutput(output, c);
                else if (Operators.Contains(c))
                {
                    if (operatorStack.Count > 0)
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
                else if (c == '(')
                {
                    operatorStack.Push(c);
                }
                else if (c == ')')
                {
                    bool leftFound = false;
                    while (operatorStack.Count > 0)
                    {
                        char peek = operatorStack.Peek();
                        if (peek != '(')
                        {
                            AddToOutput(output, operatorStack.Pop());
                        }
                        else
                        {
                            operatorStack.Pop();
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

        while (operatorStack.Count > 0)
        {
            char o = operatorStack.Pop();
            if ("()".Contains(o))
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

    public static void AddToOutput(List<char> output, params char[]? chars)
    {
        if (chars != null && chars.Length > 0)
        {
            foreach (char c in chars)
            {
                output.Add(c);
            }
            output.Add(' ');
        }
    }


    private static bool IsNum(string symbol)
    {
        return symbol.All(char.IsDigit);
    }


}

