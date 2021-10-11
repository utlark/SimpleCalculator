using System;
using System.Collections.Generic;

namespace SimpleCalculator
{
    class Calculator
    {
        private static bool IsUnary(char c)
        {
            return '+' == c || '-' == c;
        }

        private static bool IsDelimeter(char c)
        {
            return '+' == c || '-' == c || '*' == c || '/' == c || '(' == c || ')' == c;
        }

        private static byte Type(string s)
        {
            if (s.Length == 1 && IsDelimeter(s[0]) || s == "-(" || s == "+(")
                return 0;
            return 1;
        }

        private static byte Priority(string s)
        {
            switch (s)
            {
                case "*":
                case "/":
                    return 3;

                case "+":
                case "-":
                    return 2;

                case "(":
                case "-(":
                case "+(":
                case ")":
                    return 1;
                default:
                    return 0;
            }
        }

        private static Queue<string> GetTokens(string raw)
        {
            Queue<string> tokens = new Queue<string>();
            if (raw.Length > 0)
            {
                bool isUnary = false;
                raw = raw.Replace("·", "*").Replace("×", "*").Replace("⋅", "*").Replace("∙", "*").Replace("∗", "*");
                raw = raw.Replace("∶", "/").Replace(":", "/").Replace("÷", "/").Replace("∕", "/").Replace("⁄", "/");
                raw = raw.Replace("＋", "+").Replace("−", "-").Replace("－", "-");
                raw = raw.Replace(',', '.').Replace(" ", "");

                for (int i = 0; i < raw.Length;)
                    if (IsUnary(raw[i]) && i == 0 && raw.Length > 1 ||
                        IsUnary(raw[i]) && i > 0 && i < raw.Length - 1 && IsDelimeter(raw[i - 1]) && raw[i - 1] != ')' && (char.IsDigit(raw[i + 1]) || raw[i + 1] == '('))
                    {                        
                        isUnary = true;
                        i++;
                    }
                    else if (char.IsDigit(raw[i]))
                    {
                        string temp = null;

                        if (isUnary)
                            temp += raw[i - 1];
                       
                        while (i < raw.Length && !IsDelimeter(raw[i]))
                        {
                            if (char.IsDigit(raw[i]) || raw[i] == '.' && !(raw[i - 1] == '.'))
                                temp += raw[i];
                            else                               
                                throw new ArgumentException($"Ошибка: выражение записано неправильно.\nНа позиции: {i + 1}");
                            i++;
                        }

                        tokens.Enqueue(temp);
                        isUnary = false;
                    }
                    else if (IsDelimeter(raw[i]))
                    {
                        if (!isUnary)
                            tokens.Enqueue("" + raw[i]);
                        else
                            tokens.Enqueue("" + raw[i - 1] + raw[i]);
                        isUnary = false;
                        i++;
                    }
                    else if (raw[i] == '=' && i == raw.Length - 1)
                    {
                        i++;
                    }
                    else
                    {
                        throw new ArgumentException($"Ошибка: неожиданный символ -> {raw[i]}\nНа позиции: {i + 1}");
                    }
            }
            else
            {
                throw new ArgumentException("Ошибка: пустое выражение.");
            }
            return tokens;
        }

        private static void Eval(Stack<Fraction> numbers, Stack<string> operators)
        {
            if (numbers.Count >= 2 && operators.Count >= 1)
            {
                Fraction right = numbers.Pop();
                Fraction left = numbers.Pop();
                switch (operators.Pop())
                {
                    case "+":
                        numbers.Push(Fraction.Addition(left, right));
                        break;
                    case "-":
                        numbers.Push(Fraction.Subtraction(left, right));
                        break;
                    case "*":
                        numbers.Push(Fraction.Multiplication(left, right));
                        break;
                    case "/":
                        numbers.Push(Fraction.Division(left, right));
                        break;
                    case "(":
                        throw new ArgumentException("Ошибка: потеряны закрывающиеся скобки.");
                    default:
                        throw new ArgumentException("Ошибка: выражение записано неправильно.\nОшибка в операторах.");
                }
            }
            else
            {
                throw new ArgumentException("Ошибка: выражение записано неправильно.\nНесоответствие количества операторов и чисел.");
            }
        }       

        public static Fraction GetEval(string expression)
        {
            try
            {
                Stack<Fraction> numbers = new Stack<Fraction>();
                Stack<string> operators = new Stack<string>();
                Queue<string> tokens = GetTokens(expression);

                while (tokens.Count > 0)
                    if (Type(tokens.Peek()) == 1)
                    {                       
                        numbers.Push(Fraction.Parse(tokens.Dequeue()));
                    }
                    else if (Type(tokens.Peek()) == 0)
                    {
                        if (tokens.Peek().Contains("("))
                        {
                            operators.Push(tokens.Dequeue());
                        }
                        else if (tokens.Peek().Equals(")"))
                        {
                            tokens.Dequeue();
                            while (operators.Count > 0 && !operators.Peek().Contains("("))
                                Eval(numbers, operators);

                            if (operators.Peek().Equals("-("))
                            {
                                Console.WriteLine(0);
                                Fraction fraction = numbers.Pop();
                                fraction.Numerator *= -1;
                                numbers.Push(fraction);
                            }

                            if (operators.Count > 0 && operators.Peek().Contains("("))
                                operators.Pop();
                            else
                                throw new ArgumentException("Ошибка: потеряны открывающиеся скобки.");
                        }
                        else
                        {
                            while (operators.Count > 0 && Priority(operators.Peek()) >= Priority(tokens.Peek()))
                                Eval(numbers, operators);
                            operators.Push(tokens.Dequeue());
                        }
                    }

                while (operators.Count > 0)
                    Eval(numbers, operators);

                if (numbers.Count == 1 && operators.Count == 0)
                    return numbers.Pop();
                throw new ArgumentException("Ошибка: выражение записано неправильно.\nНесоответствие количества операторов и чисел.");
            }
            catch (OverflowException)
            {
                throw new OverflowException("Ошибка: вычисления вышли за доступные границы.");
            }
        }

        public static void GetInfo()
        {
            Console.WriteLine("\nПоддерживаемые операции: '+', '-', '*', '/', '(', ')'.\n" +
                "Поддерживаемые записи: '-5', '1/2', '0,5', '0.5'.\n" +
                "Примеры записи: '2+2*2', '(2+2)*2', '1/2*-2', '1,2*1.5'.\n");
        }
    }

    class Program
    {
        static public void ColorPrint(string message, ConsoleColor color = ConsoleColor.Red)
        {
            ConsoleColor temp = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = temp;
        }
        static void Main()
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
                        Calculator.GetInfo();
                    }
                    else
                    {
                        Fraction fraction = Calculator.GetEval(expression);
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