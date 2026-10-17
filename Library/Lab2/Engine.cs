using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Library.Lab2
{
    /// <summary>
    /// Вычисление обратной предикатной трансформации для заданной программы и постусловия.
    /// </summary>
    public class Engine
    {
        /// <summary>
        /// Результат вычисления обратной предикатной трансформации (wp), 
        /// включающий итоговое выражение, пошаговый трейс и условия определённости.
        /// </summary>
        public class Result
        {
            public Expressions Final { get; set; }
            public List<string> TraceSteps { get; set; }
            public List<Expressions> DefinednessConditions { get; set; }
        }

        /// <summary>
        /// Вычисляет обратную предикатную трансформацию (wp) для заданной программы и постусловия.
        /// </summary>
        /// <param name="program"></param>
        /// <param name="postcondition"></param>
        /// <returns></returns>
        public Result Calculate(Statement program, Expressions postcondition)
        {
            // Для пошагового трейса
            var trace = new List<string>(); 
            Expressions current = CalculateRecursive(program, postcondition, trace);

            // Собираем все условия определённости (знаменатели != 0)
            var definedness = current.GetDefinednessConditions();

            // Добавляем их к итоговому wp через логическое И (&&)
            foreach (var cond in definedness)
            {
                current = new BinaryOperation(current, cond, "&&");
            }
            current = current.Simplify();

            return new Result
            {
                Final = current,
                TraceSteps = trace,
                DefinednessConditions = definedness
            };
        }

        /// <summary>
        /// Рекурсивный метод для вычисления обратной предикатной трансформации (wp) для различных типов операторов.
        /// </summary>
        /// <param name="program"></param>
        /// <param name="postcondition"></param>
        /// <param name="trace"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>

        public Expressions CalculateRecursive(Statement program, Expressions postcondition, List<string> trace)
        {
            switch (program)
            {
                case Assignment assignment:
                    // Для присваивания: wp(x := E, Q) = Q[E/x]
                    var substituted = postcondition.Exchange(assignment.VariableName, assignment.Value);
                    trace.Add($"После присваивания {assignment.VariableName} := {assignment.Value.ToMathString()}, wp = {substituted.ToMathString()}");
                    return substituted;

                case Sequence sequence:
                    // Для последовательности: wp(S1; S2, Q) = wp(S1, wp(S2, Q))
                    Expressions current = postcondition;

                    trace.Add($"[Sequence] Начинаем обратный проход по {sequence.Statements.Count} операторам");

                    foreach (var stmt in sequence.Statements.AsEnumerable().Reverse())
                    {
                        // 1. Запоминаем, какое условие было ДО обработки этого оператора
                        string conditionBefore = current.ToMathString();

                        // 2. Тянем условие через текущий оператор
                        current = CalculateRecursive(stmt, current, trace);

                        // 3. Добавляем в трейс промежуточный результат (требование методички!)
                        trace.Add($"После оператора «{stmt.ToMathString()}» промежуточное wp стало: {current.ToMathString()}");
                    }

                    trace.Add($"[Sequence] Итоговое wp после всей последовательности: {current.ToMathString()}");
                    return current;

                case IfStatement ifStmt:
                    
                    var Then = CalculateRecursive(new Sequence(ifStmt.TrueBlock), postcondition, trace);
                    var Else = CalculateRecursive(new Sequence(ifStmt.FalseBlock), postcondition, trace);
                    var condition = ifStmt.Condition;

                    // (C => wpThen) && (!C => wpElse)
                    var notCondition = new UnaryOperation(condition, "!");
                    var branch1 = new BinaryOperation(notCondition, Then, "||");
                    var branch2 = new BinaryOperation(condition, Else, "||");
                    var result = new BinaryOperation(branch1, branch2, "&&");

                    trace.Add($"После if-оператора с условием {{{condition.ToMathString()}}}, wp = {{{result.ToMathString()}}}");
                    return result;


                default:
                    throw new NotImplementedException("Неподдерживаемый тип оператора");
            }
        }

        
    }
}
