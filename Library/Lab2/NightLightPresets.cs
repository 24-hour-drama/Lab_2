using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Lab2
{
    public static class NightLightPresets
    {
        /// <summary>
        /// Возвращает кортеж, содержащий программу выбора максимального значения между sensor_left и sensor_right,
        /// соответствующее логическое постусловие и человеко‑читаемое описание.    
        /// </summary>
        /// <remarks>Постусловие гарантирует, что brightness не меньше показаний любого из
        /// датчиков.</remarks>
        /// <returns>Кортеж (program, post, humanPost): program — программа (ветвление), присваивающая brightness максимум из
        /// sensor_left и sensor_right; post — логическое выражение brightness >= sensor_left && brightness >=
        /// sensor_right; humanPost — строка с кратким описанием на русском.</returns>
        public static (Statement program, Expressions post, string humanPost) GetSensorsMax()
        {

            var condition = new BinaryOperation(
                new Variable("sensor_left"),
                new Variable("sensor_right"), ">");
            var trueBlock = new List<Statement> {
            new Assignment("brightness", new Variable("sensor_left"))};
            var falseBlock = new List<Statement> {
            new Assignment("brightness", new Variable("sensor_right"))};
            var program = new IfStatement(condition, trueBlock, falseBlock);

            var post = new BinaryOperation(
                new BinaryOperation(new Variable("brightness"), new Variable("sensor_left"), ">="),
                new BinaryOperation(new Variable("brightness"), new Variable("sensor_right"), ">="),
                "&&");

            return (program, post, "Яркость не меньше показаний любого из датчиков");
        }

        /// <summary>
        /// Формирует программу для вычисления t_fade на основе дискриминанта квадратичного уравнения и сопутствующее
        /// логическое постусловие с пояснением.    
        /// </summary>
        /// <remarks>Программа вычисляет D = b^2 - 4ac, затем при D >= 0 присваивает t_fade = (-b +
        /// sqrt(D)) / (2a), иначе t_fade = -1. Постусловие эквивалентно (D >= 0 && poly == 0) || (D < 0 && t_fade ==
        /// -1).</remarks>
        /// <returns>Кортеж (program, postCondition, description): program — последовательность операторов, вычисляющая D и
        /// присваивающая t_fade; postCondition — логическое выражение, обеспечивающее корректность найденного корня или
        /// значение t_fade = -1 при D < 0; description — поясняющая строка.</returns>
        public static (Statement program, Expressions post, string) GetFadeCurve()
        {
            var bTimesB = new BinaryOperation(new Variable("brightness"), new Variable("brightness"), "*");
            var fourAC = new BinaryOperation(
                new BinaryOperation(new Variable("fade_a"), new Variable("fade_c"), "*"),
                new Constants(4), "*");
            var dExpr = new BinaryOperation(bTimesB, fourAC, "-");
            var assignD = new Assignment("D", dExpr);

            var condition = new BinaryOperation(new Variable("D"), new Constants(0), ">=");

            var minusB = new UnaryOperation(new Variable("b"), "-");
            var sqrtD = new UnaryOperation(new Variable("D"), "sqrt");
            var numerator = new BinaryOperation(minusB, sqrtD, "+");
            var denominator = new BinaryOperation(new Constants(2), new Variable("a"), "*");
            var tFadeExpr = new BinaryOperation(numerator, denominator, "/");
            var thenBlock = new List<Statement> { new Assignment("t_fade", tFadeExpr) };

            var elseBlock = new List<Statement> { new Assignment("t_fade", new Constants(-1)) };

            var ifStmt = new IfStatement(condition, thenBlock, elseBlock);

            var program = new Sequence(new List<Statement> { assignD, ifStmt });


            var dGeZero = new BinaryOperation(new Variable("D"), new Constants(0), ">=");
            var dLtZero = new BinaryOperation(new Variable("D"), new Constants(0), "<");


            var tSquared = new BinaryOperation(new Variable("t_fade"), new Variable("t_fade"), "*");
            var aTSq = new BinaryOperation(new Variable("a"), tSquared, "*");
           
            var bT = new BinaryOperation(new Variable("b"), new Variable("t_fade"), "*");
           
            var poly = new BinaryOperation(
                new BinaryOperation(aTSq, bT, "+"),
                new Variable("c"),
                "+"
            );
            var polyIsZero = new BinaryOperation(poly, new Constants(0), "==");

            var leftBranch = new BinaryOperation(dGeZero, polyIsZero, "&&");
            var rightBranch = new BinaryOperation(
                dLtZero,
                new BinaryOperation(new Variable("t_fade"), new Constants(-1), "=="),
                "&&"
            );
            var post = new BinaryOperation(leftBranch, rightBranch, "||");

            return (program, post, "Либо корень удовлетворяет уравнению, либо t_fade = -1 при отрицательном D");


        }
        /// <summary>
        /// Создаёт программу, выполняющую обмен значений threshold_old и threshold_new, и возвращает программу,
        /// постусловие и поясняющее сообщение.
        /// </summary>
        /// <remarks>Для обмена используется временная переменная "temp"; A и B — исходные значения
        /// порогов.</remarks>
        /// <returns>Кортеж (Statement, Expressions, string): программа, выполняющая обмен; постусловие, требующее threshold_old
        /// == B && threshold_new == A; и человекочитаемое сообщение на русском.</returns>
        public static (Statement program, Expressions post, string) GetCalibrationSwap()
        {
            var stmt1 = new Assignment("temp", new Variable("threshold_old"));
            var stmt2 = new Assignment("threshold_old", new Variable("threshold_new"));
            var stmt3 = new Assignment("threshold_new", new Variable("temp"));
            var program = new Sequence(new List<Statement> { stmt1, stmt2, stmt3 });

            
            var post = new BinaryOperation(
                new BinaryOperation(new Variable("threshold_old"), new Variable("B"), "=="),
                new BinaryOperation(new Variable("threshold_new"), new Variable("A"), "=="),
                "&&"
            );

            return (program, post, "Значения порогов (threshold_old и threshold_new) успешно поменялись местами (A и B — исходные значения).");
        }
    


    }
}
