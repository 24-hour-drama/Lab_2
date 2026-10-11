using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Lab2
{
    /// <summary>
    /// Абстрактный базовый класс для всех операторов (Присваивание, ветвление, последовательность).
    /// </summary>
    public abstract class Statement
    {
        /// <summary>
        /// Возвращает строковое представление выражения в математической нотации.
        /// </summary>
        /// <returns></returns>
        public abstract string ToMathString();

        /// <summary>
        /// Возвращает строковое представление выражения для отчета.
        /// </summary>
        /// <returns></returns>
        public abstract string ToHumanString();

    }

    public class Assignment : Statement
    {
        /// <summary>
        /// Имя переменной, которой присваивается значение.
        /// </summary>
        public string VariableName { get; set; }

        /// <summary>
        /// Значение, которое присваивается переменной.
        /// </summary>
        public Expressions Value { get; set; }


        /// <summary>
        /// Создает новый объект присваивания с указанным именем переменной и значением.
        /// </summary>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        public Assignment(string variableName, Expressions value)
        {
            VariableName = variableName;
            Value = value;
        }

        /// <summary>
        /// Возвращает строковое представление выражения в математической нотации.
        /// </summary>
        /// <returns></returns>
        public override string ToMathString()
        {
            return $"{VariableName} = {Value.ToMathString()}";
        }

        /// <summary>
        /// Возвращает строковое представление выражения для отчета.
        /// </summary>
        /// <returns></returns>
        public override string ToHumanString()
        {
            return $"{VariableName} = {Value.ToHumanString()}";
        }
    }

    public class IfStatement : Statement
    {
        /// <summary>
        /// Условие ветвления.
        /// </summary>
        public Expressions Condition { get; set; }
        /// <summary>
        /// Блок операторов, выполняемых при истинности условия.
        /// </summary>
        public List<Statement> TrueBlock { get; set; }
        /// <summary>
        /// Блок операторов, выполняемых при ложности условия (необязательный).
        /// </summary>
        public List<Statement> FalseBlock { get; set; }

        /// <summary>
        /// Создает новый объект ветвления с указанным условием и блоками операторов.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="trueBlock"></param>
        /// <param name="falseBlock"></param>
        public IfStatement(Expressions condition, List<Statement> trueBlock, List<Statement> falseBlock = null)
        {
            Condition = condition;
            TrueBlock = trueBlock ?? new List<Statement>();
            FalseBlock = falseBlock ?? new List<Statement>();
        }

        /// <summary>
        /// Возвращает строковое представление выражения в математической нотации.
        /// </summary>
        /// <returns></returns>
        public override string ToMathString()
        {
            var trueBlockStr = string.Join("; ", TrueBlock.Select(s => s.ToMathString()));
            var falseBlockStr = FalseBlock != null ? string.Join("; ", FalseBlock.Select(s => s.ToMathString())) : "";
            if (FalseBlock.Count == 0 || FalseBlock == null)
            {
                return $"if ({Condition.ToMathString()}) {{ {trueBlockStr} }}";
            }
            return $"if ({Condition.ToMathString()}) {{ {trueBlockStr} }} else {{ {falseBlockStr} }}";
        }

        /// <summary>
        /// Возвращает строковое представление выражения для отчета.
        /// </summary>
        /// <returns></returns>
        public override string ToHumanString()
        {
            var trueBlockStr = string.Join(" ", TrueBlock.Select(s => s.ToHumanString()));
            var falseBlockStr = FalseBlock != null ? string.Join(" ", FalseBlock.Select(s => s.ToHumanString())) : "";
            return $"Если ({Condition.ToHumanString()}) {{ {trueBlockStr} }} иначе {{ {falseBlockStr} }}";
        }
    }

    public class Sequence : Statement
    {
        /// <summary>
        /// Список операторов, выполняемых последовательно.
        /// </summary>
        public List<Statement> Statements { get; set; }
        /// <summary>
        /// Создает новый объект последовательности с указанным списком операторов.
        /// </summary>
        /// <param name="statements"></param>
        public Sequence(List<Statement> statements)
        {
            Statements = statements ?? new List<Statement>();
        }
        /// <summary>
        /// Возвращает строковое представление выражения в математической нотации.
        /// </summary>
        /// <returns></returns>
        public override string ToMathString()
        {
            return string.Join("; ", Statements.Select(s => s.ToMathString()));
        }
        /// <summary>
        /// Возвращает строковое представление выражения для отчета.
        /// </summary>
        /// <returns></returns>
        public override string ToHumanString()
        {
            return string.Join(", затем ", Statements.Select(s => s.ToHumanString()));
        }
    }
}
