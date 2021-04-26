using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleCalculator
{
    class Fraction
    {
        public long Numerator { get; set; }
        public long Denominator { get; set; }
        public Fraction()
        {

        }
        public Fraction(long numerator, long denominator)
        {
            Numerator = numerator;
            if (denominator == 0)
                throw new DivideByZeroException("Ошибка: знаменатель не может быть равен нулю.");
            Denominator = denominator;
        }
        public override string ToString() => $"{Numerator}/{Denominator}";

        static public Fraction Parse(string s)
        {
            checked
            {
                Fraction fraction = new Fraction();
                if (s.Contains('.'))
                {
                    try
                    {
                        long integer = long.Parse(s.Remove(s.IndexOf("."), s.Length - s.IndexOf(".")));
                        long fractional = long.Parse(s.Remove(0, s.IndexOf(".") + 1));
                        double power = Math.Pow(10, fractional.ToString().Length);

                        fraction.Numerator = fractional + (integer * (long)power);
                        fraction.Denominator = (long)power;
                        fraction = Reduction(fraction);
                    }
                    catch
                    {
                        throw new ArgumentException("Ошибка: неверно записана десятичная дробь.");
                    }
                }
                else
                {
                    fraction.Numerator = long.Parse(s);
                    fraction.Denominator = 1;
                }
                return fraction;
            }
        }
        static public long GCF(long a, long b)
        {
            while (b != 0)
            {
                long temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
        static public long LCM(long a, long b)
        {
            return (a / GCF(a, b)) * b;
        }
        static public double GetDouble(Fraction a)
        {
            return (a.Numerator * 1.0) / a.Denominator;
        }
        static public Fraction Reduction(Fraction a)
        {
            long gcf = GCF(a.Numerator, a.Denominator);
            a.Numerator /= gcf;
            a.Denominator /= gcf;

            if (a.Denominator < 0)
            {
                a.Numerator *= -1;
                a.Denominator *= -1;
            }
            return a;
        }
        static private Fraction ZeroCheck(Fraction a)
        {
            if (a.Denominator == 0)
                throw new DivideByZeroException($"Ошибка: попытка деления на нуль - {a}");
            return a;
        }

        static public Fraction Addition(Fraction b, Fraction a)
        {
            checked
            {
                long lcm = LCM(a.Denominator, b.Denominator);

                a.Numerator *= (lcm / a.Denominator);
                b.Numerator *= (lcm / b.Denominator);
                a.Numerator += b.Numerator;
                a.Denominator = lcm;
                return Reduction(ZeroCheck(a));
            }
        }
        static public Fraction Subtraction(Fraction b, Fraction a)
        {
            checked
            {
                long lcm = LCM(a.Denominator, b.Denominator);

                a.Numerator *= (lcm / a.Denominator);
                b.Numerator *= (lcm / b.Denominator);
                a.Numerator -= b.Numerator;
                a.Denominator = lcm;
                return Reduction(ZeroCheck(a));
            }
        }
        static public Fraction Multiplication(Fraction b, Fraction a)
        {
            checked
            {
                a.Numerator *= b.Numerator;
                a.Denominator *= b.Denominator;
                return Reduction(ZeroCheck(a));
            }
        }
        static public Fraction Division(Fraction b, Fraction a)
        {
            checked
            {
                a.Numerator *= b.Denominator;
                a.Denominator *= b.Numerator;
                return Reduction(ZeroCheck(a));
            }
        }
    }
    class Calculator
    {
        static readonly Stack<Fraction> numbers = new Stack<Fraction>();
        static readonly Stack<Token> operators = new Stack<Token>();
        static readonly Queue<Token> tokens = new Queue<Token>();

        enum TokenType
        {
            Delimeter,
            Brackets,
            Number
        }
        class Token
        {
            public TokenType Type { get; }
            public string Value { get; }
            public int Priority { get; }
            public override string ToString() => $"{Type}: {Value}: {Priority}";
            public Token(TokenType type, string value, int priority)
            {
                Type = type;
                Value = value;
                Priority = priority;
            }
        }
       
        static private void Calculate()
        {
            if (numbers.Count >= 2 && operators.Count >= 1)
                switch (operators.Pop().Value)
                {
                    case "+":
                        numbers.Push(Fraction.Addition(numbers.Pop(), numbers.Pop()));
                        break;
                    case "-":
                        numbers.Push(Fraction.Subtraction(numbers.Pop(), numbers.Pop()));
                        break;
                    case "*":
                        numbers.Push(Fraction.Multiplication(numbers.Pop(), numbers.Pop()));
                        break;
                    case "/":
                        numbers.Push(Fraction.Division(numbers.Pop(), numbers.Pop()));
                        break;
                    case "(":
                        throw new ArgumentException("Ошибка: потеряны закрывающиеся скобки.");
                    default:
                        throw new ArgumentException("Ошибка: выражение записано неправильно.");
                }
            else
                throw new ArgumentException("Ошибка: выражение записано неправильно.");
        }
        static private int Priority(char c)
        {
            switch (c)
            {
                case '*':
                case '/':
                    return 3;

                case '+':
                case '-':
                    return 2;

                case '=':
                case '(':
                case ')':
                    return 1;
                default:
                    return 0;
            }
        }
        static private bool IsDelimeter(char c)
        {
            char[] delimeters = { '+', '-', '*', '/', '(', ')', '=' };
            if (delimeters.Contains(c))
                return true;
            return false;
        }
        static private void GetTokens(string s)
        {
            if (s.Length > 0)
            {
                int i = 0;
                tokens.Clear();
                string temp = null;
                bool isUnary = false;
                char[] unaryOperators = { '+', '-' };
                s = s.Replace("·", "*").Replace("×", "*").Replace("⋅", "*").Replace("∙", "*").Replace("∗", "*");
                s = s.Replace("∶", "/").Replace(":", "/").Replace("÷", "/").Replace("∕", "/").Replace("⁄", "/");
                s = s.Replace("＋", "+").Replace("−", "-").Replace("－", "-");
                s = s.Replace(',', '.').Replace(" ", "");

                while (i < s.Length)
                {
                    if (unaryOperators.Contains(s[i]) && i == 0 && s.Length > 1 && char.IsDigit(s[i + 1]) ||
                        unaryOperators.Contains(s[i]) && i > 0 && i < s.Length - 1 && IsDelimeter(s[i - 1]) && s[i - 1] != ')' && char.IsDigit(s[i + 1]))
                    {
                        isUnary = true;
                        i++;
                    }
                    else if (IsDelimeter(s[i]))
                    {
                        if (s[i] == '(' && (i == 0 || IsDelimeter(s[i - 1])) ||
                            s[i] == ')' && i > 0 && i == s.Length - 1 && (char.IsDigit(s[i - 1]) || IsDelimeter(s[i - 1]) && s[i - 1] != '(') ||
                            s[i] == ')' && i > 0 && i < s.Length - 1 && (char.IsDigit(s[i - 1]) || IsDelimeter(s[i - 1]) && s[i - 1] != '(') && IsDelimeter(s[i + 1]))
                            tokens.Enqueue(new Token(TokenType.Brackets, s[i].ToString(), Priority(s[i])));
                        else if (s[i] == '=' && s.IndexOf('=') != s.Length - 1)
                            throw new ArgumentException("Ошибка: знак равенства не в конце выражения.");
                        else if (s[i] != '=')
                            tokens.Enqueue(new Token(TokenType.Delimeter, s[i].ToString(), Priority(s[i])));
                        i++;
                    }
                    else if (char.IsDigit(s[i]))
                    {
                        if (isUnary)
                            temp += s[i - 1];

                        while (i < s.Length && !IsDelimeter(s[i]))
                        {
                            if (char.IsDigit(s[i]) || s[i] == '.' && !(s[i - 1] == '.'))
                                temp += s[i];
                            else
                                throw new ArgumentException("Ошибка: выражение записано неправильно.");
                            i++;
                        }

                        tokens.Enqueue(new Token(TokenType.Number, temp, 0));
                        isUnary = false;
                        temp = null;
                    }
                    else
                    {
                        throw new ArgumentException("Ошибка: неправильный символ -> " + s[i]);
                    }
                }
            }
            else
            {
                throw new ArgumentException("Ошибка: пустое выражение.");
            }
        }

        static public void CalculatorInfo()
        {
            Console.WriteLine("\nПоддерживаемые операции: '+', '-', '*', '/', '(', ')'.\n" +
                "Поддерживаемые записи: '-5', '1/2', '0,5', '0.5'.\n" +
                "Примеры записи: '2+2*2', '(2+2)*2', '1/2*-2', '1,2*1.5'.\n");
        }
        static public Fraction Solve(string expression)
        {
            try
            {
                GetTokens(expression);
                numbers.Clear();
                operators.Clear();

                while (tokens.Count > 0)
                {
                    if (tokens.Peek().Type == TokenType.Number)
                    {
                        numbers.Push(Fraction.Parse(tokens.Dequeue().Value));
                    }
                    else if (tokens.Peek().Type == TokenType.Delimeter)
                    {
                        if (operators.Count == 0)
                        {
                            operators.Push(tokens.Dequeue());
                        }
                        else
                        {
                            if (operators.Peek().Priority >= tokens.Peek().Priority)
                                while (operators.Count > 0 && operators.Peek().Priority >= tokens.Peek().Priority)
                                    Calculate();
                            operators.Push(tokens.Dequeue());
                        }
                    }
                    else if (tokens.Peek().Type == TokenType.Brackets)
                    {
                        if (tokens.Peek().Value == "(")
                        {
                            operators.Push(tokens.Dequeue());
                        }
                        else if (tokens.Dequeue().Value == ")")
                        {
                            while (operators.Count > 0 && operators.Peek().Value != "(")
                                Calculate();

                            if (operators.Count > 0 && operators.Peek().Value == "(")
                                operators.Pop();
                            else
                                throw new ArgumentException("Ошибка: потеряны открывающиеся скобки.");
                        }
                    }
                }

                while (operators.Count > 0)
                    Calculate();

                if (numbers.Count == 1 && operators.Count == 0)
                    return numbers.Pop();
                throw new ArgumentException("Ошибка: выражение записано неправильно.");
            }
            catch (OverflowException)
            {
                throw new OverflowException("Ошибка: вычисления вышли за доступные границы.");
            }
        }
    }

    class Program
    {
        static public void ColorPrint(string s, ConsoleColor c = ConsoleColor.Red)
        {
            ConsoleColor temp = Console.ForegroundColor;
            Console.ForegroundColor = c;
            Console.WriteLine(s);
            Console.ForegroundColor = temp;
        }
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                string expression = null;
                foreach (string a in args)
                    expression += a;

                try
                {
                    Fraction fraction = Calculator.Solve(expression);
                    string answer = $"Ответ: {fraction} = {Fraction.GetDouble(fraction)}";
                    ColorPrint(answer, ConsoleColor.Green);
                }
                catch (Exception e)
                {
                    ColorPrint(e.Message);
                }               
            }
            else
            {
                bool stop = false;
                while (!stop)
                {
                   try
                    {
                        Console.Clear();
                        Console.WriteLine("Простой калькулятор.\n I-Получить справку.\n");
                        Console.Write("Введите команду: ");

                        string expression = Console.ReadLine();
                        if (expression.ToLower() == "i")
                        {
                            Calculator.CalculatorInfo();
                        }
                        else
                        {
                            Fraction fraction = Calculator.Solve(expression);
                            string answer = $"Ответ: {fraction} = {Fraction.GetDouble(fraction)}";
                            ColorPrint(answer, ConsoleColor.Green);
                        }
                    }                    
                    catch (Exception e)
                    {
                        ColorPrint(e.Message);
                    }                    

                    Console.Write("\nВведите 'N' для завершения программы: ");
                    if (Console.ReadLine().ToLower() == "n")
                        stop = true;
                }
            }
        }
    }
}