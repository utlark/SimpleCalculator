using System;
using System.Linq;

namespace SimpleCalculator
{
    class Fraction
    {
        public long Numerator { get; set; }
        public long Denominator { get; set; }
        public Fraction()
        {
            Numerator = Denominator = 1;
        }
        public Fraction(long numerator, long denominator)
        {
            if (denominator == 0)
                throw new DivideByZeroException("Ошибка: знаменатель не может быть равен нулю.");

            Numerator = numerator;
            Denominator = denominator;                     
        }

        public override string ToString()
        {
            if (Denominator == 1)
                return $"{Numerator}";
            return $"{Numerator}/{Denominator}";
        }
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
        static public Fraction Addition(Fraction a, Fraction b)
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
        static public Fraction Subtraction(Fraction a, Fraction b)
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
        static public Fraction Multiplication(Fraction a, Fraction b)
        {
            checked
            {
                a.Numerator *= b.Numerator;
                a.Denominator *= b.Denominator;
                return Reduction(ZeroCheck(a));
            }
        }
        static public Fraction Division(Fraction a, Fraction b)
        {
            checked
            {
                a.Numerator *= b.Denominator;
                a.Denominator *= b.Numerator;
                return Reduction(ZeroCheck(a));
            }
        }
    }
}
