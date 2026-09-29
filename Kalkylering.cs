namespace GrafRaknare;

public class Kalkylering
{
    // This method calculates the value of the expression represented by the tokenizedExpression array for a given x value.
    
    // remember here is where you add the Logic to Handle the exponents if you want to deal with more complex expressions like x^2 + 3*x - 5,
    // you will need to implement logic to handle the exponentiation operator '^' in the CalculateY method. 
    // This can be done by adding a new case in the operator handling section of the method, similar to how multiplication and division are handled.


    public double CheckExpression(object[] expression, double x)
    {
        return CalculateY(x, expression);
    }









    private double CalculateY(double x, object[] tokenizedExpression)
    {
        List<object> tokenList = new List<object>(tokenizedExpression);
        string currentOperator = "";
        double leftValue;
        double rightValue;
        for (int i = 0; i < tokenList.Count; i++)
        {
            if (tokenList[i] is object[] nestedExpression)
            {
                tokenList[i] = CalculateY(x, nestedExpression);
            }            
        }
        for (int i = 0; i < tokenList.Count; i++)
        {
            if (tokenList[i] is string token)
            {
                if (WhatOperator(token) == "*" || WhatOperator(token) == "/")
                {
                    currentOperator = token;
                    leftValue = GetValue(tokenList[i - 1], x);
                    rightValue = GetValue(tokenList[i + 1], x);
                    CalculateAndReplaceMultiOrDivision(tokenList, i, leftValue, rightValue, currentOperator);
                    i--; // Cause the list has changed it might skip the next operator.
                }
            }
        }
        for (int i = 0; i < tokenList.Count; i++)
        {
            if (tokenList[i] is string token)
            {
                if (WhatOperator(token) == "+" || WhatOperator(token) == "-")
                {
                    currentOperator = token;
                    leftValue = GetValue(tokenList[i - 1], x);
                    rightValue = GetValue(tokenList[i + 1], x);
                    CalculateAndReplacePlusOrMinus(tokenList, i, leftValue, rightValue, currentOperator);
                    i--; // Cause the list has changed it might skip the next operator.
                }
            }
        }



        return GetValue(tokenList[0], x);
    }

    private static void CalculateAndReplacePlusOrMinus(List<object> tokenList, int operatorIndex, double leftValue, double rightValue, string currentOperator)
    {
        double result = 0;
        if (currentOperator == "+")
        {
            result = leftValue + rightValue;
        }
        else if (currentOperator == "-")
        {
            result = leftValue - rightValue;
        }
        tokenList[operatorIndex - 1] = result;
        tokenList.RemoveAt(operatorIndex); // Remove operator
        tokenList.RemoveAt(operatorIndex); // Remove right operand
    }

    private static void CalculateAndReplaceMultiOrDivision(List<object> tokenList, int operatorIndex, double leftValue, double rightValue, string currentOperator)
    {
        double result = 0;
        if (currentOperator == "*")
        {
            result = leftValue * rightValue;
        }
        else if (currentOperator == "/")
        {
            result = leftValue / rightValue;
        }
        tokenList[operatorIndex - 1] = result;
        tokenList.RemoveAt(operatorIndex); // Remove operator
        tokenList.RemoveAt(operatorIndex); // Remove right operand
    }





    private static double GetValue(object token, double x)
    {
        if (token is string strToken)
        {
            if (strToken.Contains('^'))
            {
                string[] parts = strToken.Split('^');
                if (parts.Length == 2)
                {
                    return Math.Pow(GetValue(parts[0], x), GetValue(parts[1], x));
                }
            }
            else if (strToken == "x")
            {
                return x;
            }
            else if (double.TryParse(strToken, out double result))
            {
                return result;
            }
        }
        else if (token is double numToken)
        {
            return numToken;
        }
        throw new ArgumentException("Invalid token type");
    }



    private static string WhatOperator(object token)
    {
        if (token is string strToken)
        {
            if (strToken == "+" || strToken == "-" || strToken == "*" || strToken == "/")
            {
                return strToken;
            }
        }
        return " ";
    }
}



