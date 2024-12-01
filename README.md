# Parsing arithmetic strings into Reverse Polish Notation (RPN) and evaluating. 

Written in C# and supports basic arithmetic symbols (+ - * / ( ) ^)

## How it works:
Steps:
- Prompt user for arithmetic string
- Parse arithmetic string into **tokens**, where each token is either an **Operator** or a number.
- Take list of tokens and order them into RPN by:
   - Moving operator tokens into a stack, while pushing numbers into output until all tokens read. Note that tokens pushed to output have a space in between.
   - If parentheses encountered, continue as normal until closing parentheses is enocuntered. Then, dump all tokens between open parentheses and closed to output.
   - Any leftover numbers pushed to output
   - Any leftover tokens pushed to output
- Take list of tokens and recursively evaluate them back-to-front (starting from last token, ending with first token).
   - Example: "2 2 + 3 *"
      - '*' - token: recurse to find x,y
         - return '3' and '+'
           - '+' - token: recurse to find x,y
              - return '2' and '2'
             - 2 + 2 = 4
           - return 4
        - 3 * 4 = 12
     - return 12
- Return result and log to console.

##Shortfalls:
- Nothing really stopping you from putting in so many symbols that the recursion stack overflows.
- Assumes user input string is in an infix format.
- No error correction on input. Catches unknown symbols, but does not try to fix them.
- Does not work with variables.
- It was really late when I got it working.
  
