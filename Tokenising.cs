namespace GrafRaknare;

public class ExpressionParser
{
    public object[] ExpressionFormatter(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return Array.Empty<object>();
        }

        expression = expression.Trim();
        if (IsWrappedEntirely(expression))
        {
            expression = expression[1..^1];
        }

        return ParseExpression(expression, 0, out _).ToArray();
    }

    private List<object> ParseExpression(string expression, int startIndex, out int nextIndex)
    {
        List<object> tokens = new List<object>();
        int index = startIndex;

        while (index < expression.Length)
        {
            if (char.IsWhiteSpace(expression[index]))
            {
                index++;
                continue;
            }

            if (expression[index] == ')')
            {
                nextIndex = index + 1;
                return tokens;
            }

            object token;
            if (expression[index] == '(')
            {
                List<object> nestedTokens = ParseExpression(expression, index + 1, out index);
                if (expression[index - 1] != ')')
                {
                    throw new ArgumentException("No matching closing parenthesis found.", nameof(expression));
                }

                token = nestedTokens.ToArray();
            }
            else if (IsUnaryMinus(expression, index))
            {
                int tokenStart = index;
                index = ReadSignedAtom(expression, index);
                token = expression[tokenStart..index].Replace(" ", string.Empty);
            }
            else if (IsOperator(expression[index]))
            {
                tokens.Add(expression[index].ToString());
                index++;
                continue;
            }
            else
            {
                int tokenStart = index;
                index = ReadAtom(expression, index);
                token = expression[tokenStart..index].Replace(" ", string.Empty);
            }

            AddImplicitMultiplicationIfNeeded(tokens, token);
            tokens.Add(token);
        }

        nextIndex = index;
        return tokens;
    }

    private static int ReadAtom(string expression, int startIndex)
    {
        int index = startIndex;
        bool isNumber = char.IsDigit(expression[index]) || expression[index] == '.';

        while (index < expression.Length)
        {
            char character = expression[index];
            if (isNumber ? char.IsDigit(character) || character == '.' : char.IsLetter(character) || character == '_')
            {
                index++;
                continue;
            }

            break;
        }

        int exponentStart = index;
        while (index < expression.Length && char.IsWhiteSpace(expression[index]))
        {
            index++;
        }

        if (index < expression.Length && expression[index] == '^')
        {
            int exponentIndex = index + 1;
            while (exponentIndex < expression.Length && char.IsWhiteSpace(expression[exponentIndex]))
            {
                exponentIndex++;
            }

            if (exponentIndex < expression.Length
                && (char.IsDigit(expression[exponentIndex])
                    || char.IsLetter(expression[exponentIndex])
                    || expression[exponentIndex] == '.'))
            {
                index = ReadBasicAtom(expression, exponentIndex);
            }
            else
            {
                index = exponentStart;
            }
        }
        else
        {
            index = exponentStart;
        }

        return index;
    }

    private static int ReadBasicAtom(string expression, int startIndex)
    {
        int index = startIndex;
        while (index < expression.Length
            && (char.IsLetterOrDigit(expression[index])
                || expression[index] == '.'
                || expression[index] == '_'))
        {
            index++;
        }

        return index;
    }

    private static int ReadSignedAtom(string expression, int startIndex)
    {
        int index = startIndex;
        if (expression[index] == '-')
        {
            index++;
        }

        while (index < expression.Length
            && (char.IsDigit(expression[index]) || expression[index] == '.'))
        {
            index++;
        }

        return index;
    }

    private static bool IsUnaryMinus(string expression, int index)
    {
        if (expression[index] != '-')
        {
            return false;
        }

        if (index + 1 >= expression.Length)
        {
            return false;
        }

        if (index == 0)
        {
            return char.IsDigit(expression[index + 1]) || expression[index + 1] == '.';
        }

        char previous = expression[index - 1];
        return previous is '(' or '+' or '-' or '*' or '/' or '^'
            && (char.IsDigit(expression[index + 1]) || expression[index + 1] == '.');
    }

    private static void AddImplicitMultiplicationIfNeeded(List<object> tokens, object token)
    {
        if (tokens.Count == 0 || !StartsOperand(token))
        {
            return;
        }

        object previousToken = tokens[^1];
        if (StartsOperand(previousToken))
        {
            tokens.Add("*");
        }
    }

    private static bool StartsOperand(object token)
    {
        return token is object[] || (token is string text && !IsOperator(text[0]));
    }

    private static bool IsOperator(char character)
    {
        return character is '+' or '-' or '*' or '/' or '^';
    }

    private static bool IsWrappedEntirely(string expression)
    {
        if (expression.Length < 2 || expression[0] != '(' || expression[^1] != ')')
        {
            return false;
        }

        int depth = 0;
        for (int index = 0; index < expression.Length; index++)
        {
            if (expression[index] == '(')
            {
                depth++;
            }
            else if (expression[index] == ')')
            {
                depth--;
                if (depth == 0 && index != expression.Length - 1)
                {
                    return false;
                }
            }
        }

        if (depth != 0)
        {
            throw new ArgumentException("No matching closing parenthesis found.", nameof(expression));
        }

        return true;
    }
}