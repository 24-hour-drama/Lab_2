using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Library.Lab2
{
    /// <summary>
    /// Базовый абстрактный класс для всех выражений.
    /// Содержит общие операции: подстановка, приведение к строкам, проверка определённости и вычисление.
    /// </summary>
    public abstract class Expressions
    {
        /// <summary>
        /// Выполняет замену переменной <paramref name="varName"/> на выражение <paramref name="replacement"/>.
        /// </summary>
        public abstract Expressions Exchange(string varName, Expressions replacement);
        /// <summary>
        /// Возвращает строковое представление выражения в математической нотации.
        /// </summary>
        public abstract string ToMathString();
        /// <summary>
        /// Возвращает человекочитаемое строковое представление выражения (на русском языке).
        /// </summary>
        public abstract string ToHumanString();
        /// <summary>
        /// Возвращает список условий определённости (например, проверки деления на ноль).
        /// </summary>
        public abstract List<Expressions> GetDefinednessConditions();
        /// <summary>
        /// Вычисляет численное значение выражения при заданных значениях переменных.
        /// </summary>
        public abstract double Evaluate(Dictionary<string, double> variables);
        /// <summary>
        /// Вычисляет логическое значение выражения (false при значении 0, иначе true).
        /// По умолчанию использует <see cref="Evaluate"/>.
        /// </summary>
        public virtual bool EvaluateBool(Dictionary<string, double> variables)
        {
            return Evaluate(variables) != 0;
        }
        /// <summary>
        /// Применяет упрощение выражения и возвращает более простую форму или сам объект по умолчанию.
        /// </summary>
        public virtual Expressions Simplify() => this;

    }
    public class Constants : Expressions
    {
        /// <summary>
        /// Числовое значение константы.
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Создаёт новую константу с указанным значением.
        /// </summary>
        /// <param name="value">Числовое значение константы.</param>
        public Constants(double value)
        {
            Value = value;
        }

        /// <summary>
        /// Замена переменной в константе не изменяет выражение — возвращается сам объект.
        /// </summary>
        public override Expressions Exchange(string varName, Expressions replacement)
        {
            return this;
        }

        /// <summary>
        /// Возвращает математическое представление константы.
        /// </summary>
        public override string ToMathString()
        {
            return Value.ToString();
        }

        /// <summary>
        /// Возвращает человекочитаемое представление константы (убирает дробную часть для целых значений).
        /// </summary>
        public override string ToHumanString()
        {
            if (Value == (int)Value) return ((int)Value).ToString();
            return Value.ToString();
        }

        /// <summary>
        /// Константа не вносит условий определённости — возвращается пустой список.
        /// </summary>
        public override List<Expressions> GetDefinednessConditions()
        {
            return new List<Expressions>();
        }

        /// <summary>
        /// Возвращает численное значение константы.
        /// </summary>
        public override double Evaluate(Dictionary<string, double> variables)
        {
            return Value;
        }

        /// <summary>
        /// Упрощение константы возвращает саму константу.
        /// </summary>
        public override Expressions Simplify()
        {
            return this;
        }
    }
    public class Variable : Expressions
    {
        /// <summary>
        /// Имя переменной.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Создаёт переменную с заданным именем.
        /// </summary>
        /// <param name="name">Имя переменной.</param>
        public Variable(string name)
        {
            Name = name;
        }
        /// <summary>
        /// Если имя переменной совпадает с <paramref name="varName"/>, возвращает выражение-замену, иначе — саму переменную.
        /// </summary>
        public override Expressions Exchange(string varName, Expressions replacement)
        {
            if (Name == varName)
                return replacement;
            return this;
        }
        /// <summary>
        /// Возвращает математическое имя переменной.
        /// </summary>
        public override string ToMathString()
        {
            return Name;
        }
        /// <summary>
        /// Возвращает человекочитаемое имя переменной (по умолчанию совпадает с именем).
        /// </summary>
        public override string ToHumanString()
        {
            return Name;
        }
        /// <summary>
        /// Переменная не накладывает дополнительных условий определённости.
        /// </summary>
        public override List<Expressions> GetDefinednessConditions()
        {
            return new List<Expressions>();
        }
        /// <summary>
        /// Возвращает значение переменной из словаря значений. Если переменная не найдена, выбрасывает исключение.
        /// </summary>
        public override double Evaluate(Dictionary<string, double> variables)
        {
            if (variables.TryGetValue(Name, out double value))
                return value;
            throw new Exception($"Переменная '{Name}' не определена.");
        }
    }
    public class BinaryOperation : Expressions
    {
        /// <summary>
        /// Левый операнд операции.
        /// </summary>
        public Expressions Left { get; }
        /// <summary>
        /// Правый операнд операции.
        /// </summary>
        public Expressions Right { get; }
        /// <summary>
        /// Строковое представление оператора (например, "+", "-", "*", "/", ">").
        /// </summary>
        public string Operator { get; }
        public BinaryOperation(Expressions left, Expressions right, string op)
        {
            Left = left;
            Right = right;
            Operator = op;
        }
        /// <summary>
        /// Возвращает новое выражение, в котором выполнена замена переменной во всех подвыражениях.
        /// </summary>
        public override Expressions Exchange(string varName, Expressions replacement)
        {
            return new BinaryOperation(Left.Exchange(varName, replacement), Right.Exchange(varName, replacement), Operator);
        }
        /// <summary>
        /// Возвращает математическое строковое представление операции с учётом скобок.
        /// </summary>
        public override string ToMathString()
        {
            return $"({Left.ToMathString()} {Operator} {Right.ToMathString()})";
        }
        /// <summary>
        /// Возвращает человекочитаемое строковое представление операции на русском языке.
        /// </summary>
        public override string ToHumanString()
        {
            string opHuman = Operator switch
            {
                "+" => "плюс",
                "-" => "минус",
                "*" => "умножить на",
                "/" => "разделить на",
                ">" => "строго больше",
                ">=" => "больше или равно",
                "<" => "строго меньше",
                "<=" => "меньше или равно",
                "==" => "равно",
                "!=" => "не равно",
                "&&" => "И",
                "||" => "ИЛИ",
                _ => Operator
            };
            return $"({Left.ToHumanString()} {opHuman} {Right.ToHumanString()})";
        }
        /// <summary>
        /// Возвращает дополнительные условия определённости (напр., проверка деления на ноль).
        /// </summary>
        public override List<Expressions> GetDefinednessConditions()
        {
            var conditions = new List<Expressions>();
            conditions.AddRange(Left.GetDefinednessConditions());
            conditions.AddRange(Right.GetDefinednessConditions());
            if (Operator == "/")
                conditions.Add(new BinaryOperation(Right, new Constants(0), "!="));
            return conditions;
        }
        /// <summary>
        /// Вычисляет численное значение бинарной операции для заданных значений переменных.
        /// Поддерживаются арифметические операторы: +, -, *, /.
        /// </summary>
        public override double Evaluate(Dictionary<string, double> variables)
        {
            double leftValue = Left.Evaluate(variables);
            double rightValue = Right.Evaluate(variables);
            return Operator switch
            {
                "+" => leftValue + rightValue,
                "-" => leftValue - rightValue,
                "*" => leftValue * rightValue,
                "/" => leftValue / rightValue,
                _ => throw new Exception($"Неизвестный оператор '{Operator}'."),
            };
        }

        /// <summary>
        /// Вычисляет логическое значение бинарной операции (для операторов сравнения и логики).
        /// </summary>
        public override bool EvaluateBool(Dictionary<string, double> variables)
        {
            double leftValue = Left.Evaluate(variables);
            double rightValue = Right.Evaluate(variables);

            return Operator switch
            {
                ">" => leftValue > rightValue,
                ">=" => leftValue >= rightValue,
                "<" => leftValue < rightValue,
                "<=" => leftValue <= rightValue,
                "==" => leftValue == rightValue,
                "!=" => leftValue != rightValue,
                "&&" => (leftValue != 0) && (rightValue != 0),
                "||" => (leftValue != 0) || (rightValue != 0),
                _ => throw new Exception($"Неизвестный логический оператор '{Operator}'."),
            };
        }

        /// <summary>
        /// Пытается упростить выражение: выполняет рекурсивное упрощение операндов и сворачивает константы.
        /// </summary>
        public override Expressions Simplify()
        {
            var left = Left.Simplify();
            var right = Right.Simplify();

            if (Operator == "+" && right is Constants rc && rc.Value == 0) return left;
            if (Operator == "+" && left is Constants lc && lc.Value == 0) return right;
            if (Operator == "-" && right is Constants rc2 && rc2.Value == 0) return left;
            if (Operator == "*" && right is Constants rc3 && rc3.Value == 1) return left;
            if (Operator == "*" && left is Constants lc2 && lc2.Value == 1) return right;
            if (Operator == "*" && right is Constants rc4 && rc4.Value == 0) return new Constants(0);
            if (Operator == "*" && left is Constants lc3 && lc3.Value == 0) return new Constants(0);

            if (left is Constants cl && right is Constants cr)
            {
                double result = Operator switch
                {
                    "+" => cl.Value + cr.Value,
                    "-" => cl.Value - cr.Value,
                    "*" => cl.Value * cr.Value,
                    "/" when cr.Value != 0 => cl.Value / cr.Value,
                    _ => double.NaN
                };
                if (!double.IsNaN(result))
                    return new Constants(result);
            }

            return new BinaryOperation(left, right, Operator);

        }
    }
    public class UnaryOperation : Expressions
    {
        /// <summary>
        /// Операнд унарной операции.
        /// </summary>
        public Expressions Operand { get; }
        /// <summary>
        /// Строковое представление оператора ("-", "abs", "!").
        /// </summary>
        public string Operator { get; }
        public UnaryOperation(Expressions operand, string op)
        {
            Operand = operand;
            Operator = op;
        }
        /// <summary>
        /// Возвращает новое выражение с выполненной заменой переменной внутри операнда.
        /// </summary>
        public override Expressions Exchange(string varName, Expressions replacement)
        {
            return new UnaryOperation(Operand.Exchange(varName, replacement), Operator);
        }
        /// <summary>
        /// Возвращает математическое строковое представление унарной операции.
        /// </summary>
        public override string ToMathString()
        {
            return $"({Operator}{Operand.ToMathString()})";
        }
        /// <summary>
        /// Возвращает человекочитаемое строковое представление унарной операции на русском языке.
        /// </summary>
        public override string ToHumanString()
        {
            string opHuman = Operator switch
            {
                "-" => "минус",
                "abs" => "модуль от",
                "!" => "не",
                "sqrt" => "корень из",
                _ => Operator
            };
            return $"({opHuman} {Operand.ToHumanString()})";
        }
        /// <summary>
        /// Возвращает условия определённости операнда (если есть).
        /// </summary>
        public override List<Expressions> GetDefinednessConditions()
        {
            return Operand.GetDefinednessConditions();
        }
        /// <summary>
        /// Вычисляет численное значение унарной операции для заданных значений переменных.
        /// </summary>
        public override double Evaluate(Dictionary<string, double> variables)
        {
            double operandValue = Operand.Evaluate(variables);
            return Operator switch
            {
                "-" => -operandValue,
                "abs" => Math.Abs(operandValue),
                "sqrt" => Math.Sqrt(operandValue),
                _ => throw new Exception($"Неподдерживаемый унарный оператор '{Operator}'."),
            };
        }

        /// <summary>
        /// Вычисляет логическое значение унарной операции (поддерживает оператор "!").
        /// </summary>
        public override bool EvaluateBool(Dictionary<string, double> variables)
        {
            if (Operator == "!")
                return !Operand.EvaluateBool(variables);
            return base.EvaluateBool(variables);
        }

        /// <summary>
        /// Пытается упростить унарную операцию: сворачивает константы и устраняет двойной унарный минус.
        /// </summary>
        public override Expressions Simplify()
        {
            var operand = Operand.Simplify();
            if (Operator == "-" && operand is Constants c)
                return new Constants(-c.Value);
            if (Operator == "abs" && operand is Constants c2)
                return new Constants(Math.Abs(c2.Value));
            if (Operator == "-" && operand is UnaryOperation unaryMinus && unaryMinus.Operator == "-")
                return unaryMinus.Operand;
            return new UnaryOperation(operand, Operator);

        }
    }
}
