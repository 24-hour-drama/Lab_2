using Library;
using Library.Lab2;
using System;
using Xunit;

namespace Tests
{
    public class UnitTest1
    {
        private readonly NightlightController _controller = new();
        [Fact]
        public void SetBrightnessZero()
        {
            var state = new NightlightState { isOn = true, Brightness = 50 };

            _controller.SetBrightness(state, 0);

            Assert.Equal(0, state.Brightness);
            Assert.False(state.isOn);
        }

        [Fact]
        public void SetBrightnessHundred()
        {
            var state = new NightlightState { isOn = true, Brightness = 50 };

            _controller.SetBrightness(state, 100);

            Assert.Equal(100, state.Brightness);
            Assert.True(state.isOn);
        }

        [Fact]
        public void SetBrightness_OutOfRange_More()
        {
            var state = new NightlightState { isOn = true, Brightness = 50 };

            Assert.Throws<ArgumentOutOfRangeException>(
                () => _controller.SetBrightness(state, 101));
        }

        [Fact]
        public void SetBrightness_OutOfRange_Less()
        {
            var state = new NightlightState { isOn = true, Brightness = 50 };

            Assert.Throws<ArgumentOutOfRangeException>(
                () => _controller.SetBrightness(state, -1));

        }

        [Fact]
        public void TurnItOn()
        {
            var state = new NightlightState { isOn = false, Brightness = 0 };
            _controller.TurnOn(state);
            Assert.True(state.isOn);
            Assert.Equal(50, state.Brightness);
        }

        [Fact] 
        public void TurnOn()
        {
            var state = new NightlightState { isOn = true };
            Assert.Throws<InvalidOperationException>(() => _controller.TurnOn(state));
        }


        [Fact]
        public void TurnItOff()
        {
            var state = new NightlightState { isOn = true };
            _controller.TurnOff(state);
            Assert.False(state.isOn);
        }

        [Fact]
        public void TurnOff()
        {
            var state = new NightlightState { isOn = false };
            Assert.Throws<InvalidOperationException>(() => _controller.TurnOff(state));
        }

        [Fact]
        public void SetTimer_Min_DeactivatesTimer()
        {
            var state = new NightlightState
            {
                isOn = true,
                TimerMinutes = 30,
                isTimerActive = true
            };

            _controller.SetTimer(state, 0);

            Assert.Equal(0, state.TimerMinutes);
            Assert.False(state.isTimerActive);
        }

        [Fact]
        public void SetTimer_Max_ActivatesTimer()
        {
            var state = new NightlightState
            {
                isOn = true,

                TimerMinutes = 30,
                isTimerActive = true
            };

            _controller.SetTimer(state, 60);

            Assert.Equal(60, state.TimerMinutes);
            Assert.True(state.isTimerActive);
        }

        [Fact]
        public void SetTimer_OutOfRange_More()
        {
            var state = new NightlightState
            {
                isOn = true,
                TimerMinutes = 30,
                isTimerActive = true
            };

            Assert.Throws<ArgumentOutOfRangeException>(
                () => _controller.SetTimer(state, 61));
        }

        [Fact]
        public void SetTimer_OutOfRange_Less()
        {
            var state = new NightlightState
            {
                isOn = true,
                TimerMinutes = 30,
                isTimerActive = true
            };

            Assert.Throws<ArgumentOutOfRangeException>(
                () => _controller.SetTimer(state, -1));
        }

    }

    /// <summary>
    /// Тесты для класса Expressions
    /// </summary>
    public class ExpressionsTests
    {
        [Fact]
        public void Constant_Evaluate_ReturnsValue()
        {
            var c = new Constants(5);
            Assert.Equal(5, c.Evaluate(new Dictionary<string, double>()));
        }

        [Fact]
        public void Constant_Exchange_ReturnsSame()
        {
            var c = new Constants(5);
            var result = c.Exchange("x", new Constants(99));
            Assert.Same(c, result);
        }

        [Fact]
        public void Variable_Exchange_ReplacesIfNameMatches()
        {
            var v = new Variable("x");
            var result = v.Exchange("x", new Constants(42));
            Assert.IsType<Constants>(result);
            Assert.Equal(42, ((Constants)result).Value);
        }

        [Fact]
        public void Variable_Exchange_KeepsIfNameDiffers()
        {
            var v = new Variable("x");
            var result = v.Exchange("y", new Constants(42));
            Assert.Same(v, result);
        }

        [Fact]
        public void Variable_Evaluate_UsesDictionary()
        {
            var v = new Variable("x");
            var dict = new Dictionary<string, double> { { "x", 7 } };
            Assert.Equal(7, v.Evaluate(dict));
        }

        [Fact]
        public void Variable_Evaluate_ThrowsIfUndefined()
        {
            var v = new Variable("x");
            Assert.Throws<Exception>(() => v.Evaluate(new Dictionary<string, double>()));
        }

        [Fact]
        public void BinaryOp_Addition_Evaluates()
        {
            var expr = new BinaryOperation(new Constants(2), new Constants(3), "+");
            Assert.Equal(5, expr.Evaluate(new Dictionary<string, double>()));
        }

        [Fact]
        public void BinaryOp_Division_AddsDefinednessCondition()
        {
            var expr = new BinaryOperation(new Constants(1), new Variable("x"), "/");
            var conds = expr.GetDefinednessConditions();
            Assert.Single(conds);
            // Условие: x != 0
            Assert.Equal("(x != 0)", conds[0].ToMathString());
        }

        [Fact]
        public void BinaryOp_BoolComparison_Works()
        {
            var expr = new BinaryOperation(new Constants(5), new Constants(3), ">");
            Assert.True(expr.EvaluateBool(new Dictionary<string, double>()));
        }

        [Fact]
        public void Simplify_AddZero_RemovesZero()
        {
            var expr = new BinaryOperation(new Variable("x"), new Constants(0), "+");
            var simplified = expr.Simplify();
            Assert.IsType<Variable>(simplified);
        }

        [Fact]
        public void Simplify_MultiplyByZero_ReturnsZero()
        {
            var expr = new BinaryOperation(new Variable("x"), new Constants(0), "*");
            var simplified = expr.Simplify();
            Assert.IsType<Constants>(simplified);
            Assert.Equal(0, ((Constants)simplified).Value);
        }

        [Fact]
        public void Simplify_FoldsConstants()
        {
            var expr = new BinaryOperation(new Constants(2), new Constants(3), "+");
            var simplified = expr.Simplify();
            Assert.IsType<Constants>(simplified);
            Assert.Equal(5, ((Constants)simplified).Value);
        }

        [Fact]
        public void UnaryMinus_OnConstant_Folds()
        {
            var expr = new UnaryOperation(new Constants(5), "-");
            var simplified = expr.Simplify();
            Assert.IsType<Constants>(simplified);
            Assert.Equal(-5, ((Constants)simplified).Value);
        }

        [Fact]
        public void UnaryNot_EvaluatesBool()
        {
            var expr = new UnaryOperation(new Constants(0), "!");
            Assert.True(expr.EvaluateBool(new Dictionary<string, double>()));
        }

        [Fact]
        public void Exchange_InBinaryOp_ReplacesDeeply()
        {
            //(x + 1) с заменой x -> 10
            var expr = new BinaryOperation(new Variable("x"), new Constants(1), "+");
            var replaced = expr.Exchange("x", new Constants(10));
            Assert.Equal(11, replaced.Evaluate(new Dictionary<string, double>()));
        }

        [Fact]
        public void Constant_ToHumanString_TrimsDecimalForIntegers()
        {
            Assert.Equal("7", new Constants(7).ToHumanString());
        }

        [Fact]
        public void Constant_ToHumanString_KeepsDecimal()
        {
            Assert.Equal("2.5", new Constants(2.5).ToHumanString());
        }

        [Fact]
        public void BinaryOp_ToHumanString_UsesRussianWords()
        {
            var expr = new BinaryOperation(new Variable("a"), new Variable("b"), "+");
            Assert.Equal("(a плюс b)", expr.ToHumanString());
        }

        [Fact]
        public void BinaryOp_ToMathString_WrapsInParentheses()
        {
            var expr = new BinaryOperation(new Constants(1), new Constants(2), "*");
            Assert.Equal("(1 * 2)", expr.ToMathString());
        }

        [Fact]
        public void BinaryOp_And_EvaluatesBool()
        {
            var expr = new BinaryOperation(new Constants(1), new Constants(0), "&&");
            Assert.False(expr.EvaluateBool(new Dictionary<string, double>()));
        }

        [Fact]
        public void BinaryOp_Or_EvaluatesBool()
        {
            var expr = new BinaryOperation(new Constants(0), new Constants(1), "||");
            Assert.True(expr.EvaluateBool(new Dictionary<string, double>()));
        }

        [Fact]
        public void UnaryAbs_FoldsConstant()
        {
            var expr = new UnaryOperation(new Constants(-3), "abs");
            var simplified = expr.Simplify();
            Assert.IsType<Constants>(simplified);
            Assert.Equal(3, ((Constants)simplified).Value);
        }

        [Fact]
        public void UnaryMinus_DoubleNegation_Cancels()
        {
            // -(-x) -> x
            var inner = new UnaryOperation(new Variable("x"), "-");
            var outer = new UnaryOperation(inner, "-");
            var simplified = outer.Simplify();
            Assert.IsType<Variable>(simplified);
            Assert.Equal("x", ((Variable)simplified).Name);
        }

        [Fact]
        public void Simplify_NestedConstants_Collapses()
        {
            // (2 + 3) * 4 -> 20
            var sum = new BinaryOperation(new Constants(2), new Constants(3), "+");
            var expr = new BinaryOperation(sum, new Constants(4), "*");
            var simplified = expr.Simplify();
            Assert.IsType<Constants>(simplified);
            Assert.Equal(20, ((Constants)simplified).Value);
        }

        [Fact]
        public void Exchange_OnNestedExpression_ReplacesAll()
        {
            // (x + x) с заменой x -> 5  =>  5 + 5 = 10
            var expr = new BinaryOperation(new Variable("x"), new Variable("x"), "+");
            var replaced = expr.Exchange("x", new Constants(5));
            Assert.Equal(10, replaced.Evaluate(new Dictionary<string, double>()));
        }
    }

    /// <summary>
    /// Тесты для класса Statement
    /// </summary>
    public class StatementTests
    {
        [Fact]
        public void Assignment_ToMathString_Formats()
        {
            var a = new Assignment("x", new Constants(5));
            Assert.Equal("x = 5", a.ToMathString());
        }

        [Fact]
        public void Sequence_ToMathString_JoinsWithSemicolon()
        {
            var seq = new Sequence(new List<Statement>
            {
                new Assignment("x", new Constants(1)),
                new Assignment("y", new Constants(2))
            });
            Assert.Equal("x = 1; y = 2", seq.ToMathString());
        }

        [Fact]
        public void IfStatement_WithElse_ToMathString()
        {
            var ifs = new IfStatement(
                new BinaryOperation(new Variable("x"), new Constants(0), ">"),
                new List<Statement> { new Assignment("y", new Constants(1)) },
                new List<Statement> { new Assignment("y", new Constants(0)) });

            string s = ifs.ToMathString();
            Assert.Contains("if", s);
            Assert.Contains("else", s);
        }

        [Fact]
        public void IfStatement_WithoutElse_ToMathString()
        {
            var ifs = new IfStatement(
                new BinaryOperation(new Variable("x"), new Constants(0), ">"),
                new List<Statement> { new Assignment("y", new Constants(1)) });

            string s = ifs.ToMathString();
            Assert.Contains("if", s);
            Assert.DoesNotContain("else", s);
        }

        [Fact]
        public void Assignment_ToHumanString_MatchesMathForm()
        {
            var a = new Assignment("y", new Constants(3));
            Assert.Equal("y = 3", a.ToHumanString());
        }

        [Fact]
        public void Sequence_ToHumanString_UsesRussianJoiner()
        {
            var seq = new Sequence(new List<Statement>
            {
                new Assignment("a", new Constants(1)),
                new Assignment("b", new Constants(2))
            });
            Assert.Contains("затем", seq.ToHumanString());
        }

        [Fact]
        public void EmptySequence_ToMathString_IsEmpty()
        {
            var seq = new Sequence(new List<Statement>());
            Assert.Equal("", seq.ToMathString());
        }

        [Fact]
        public void NestedSequence_ToMathString_Flattens()
        {
            var inner = new Sequence(new List<Statement>
            {
                new Assignment("a", new Constants(1)),
                new Assignment("b", new Constants(2))
            });
            var outer = new Sequence(new List<Statement>
            {
                inner,
                new Assignment("c", new Constants(3))
            });
            Assert.Equal("a = 1; b = 2; c = 3", outer.ToMathString());
        }
    }

    /// <summary>
    /// Тесты для класса Engine
    /// </summary>
    public class EngineTests
    {
        private readonly Engine _engine = new Engine();

        [Fact]
        public void Wp_Assignment_SubstitutesValue()
        {
            // wp(x := 5, x > 0) = 5 > 0
            var program = new Assignment("x", new Constants(5));
            var post = new BinaryOperation(new Variable("x"), new Constants(0), ">");

            var result = _engine.Calculate(program, post);

            // После подстановки: (5 > 0), упрощать нечего
            Assert.Equal("(5 > 0)", result.Final.ToMathString());
        }

        [Fact]
        public void Wp_Assignment_SubstitutesExpression()
        {
            // wp(x := x + 1, x > 5) = (x + 1) > 5
            var program = new Assignment("x",
                new BinaryOperation(new Variable("x"), new Constants(1), "+"));
            var post = new BinaryOperation(new Variable("x"), new Constants(5), ">");

            var result = _engine.Calculate(program, post);

            Assert.Equal("((x + 1) > 5)", result.Final.ToMathString());
        }

        [Fact]
        public void Wp_Sequence_GoesBackwards()
        {
            // Программа: x = x + 10; y = x + 1
            // Постусловие: y == 0
            // wp(y := x + 1, y == 0)  => (x + 1) == 0
            // wp(x := x + 10, (x + 1) == 0) => ((x + 10) + 1) == 0
            var program = new Sequence(new List<Statement>
            {
                new Assignment("x", new BinaryOperation(new Variable("x"), new Constants(10), "+")),
                new Assignment("y", new BinaryOperation(new Variable("x"), new Constants(1), "+"))
            });
            var post = new BinaryOperation(new Variable("y"), new Constants(0), "==");

            var result = _engine.Calculate(program, post);

            // Проверяем, что итоговое wp содержит подстановку 10
            string s = result.Final.ToMathString();
            Assert.Contains("10", s);
            Assert.DoesNotContain("y", s);
        }

        [Fact]
        public void Wp_IfStatement_ProducesConditionalFormula()
        {
            // Программа: if (x > 0) { y = 1 } else { y = -1 }
            // Постусловие: y == 1
            var program = new IfStatement(
                new BinaryOperation(new Variable("x"), new Constants(0), ">"),
                new List<Statement> { new Assignment("y", new Constants(1)) },
                new List<Statement> { new Assignment("y", new Constants(-1)) });

            var post = new BinaryOperation(new Variable("y"), new Constants(1), "==");

            var result = _engine.Calculate(program, post);

            string s = result.Final.ToMathString();
            // Должно содержать условие x > 0 и ветви
            Assert.Contains("x", s);
            Assert.Contains("&&", s);
            Assert.Contains("||", s);
        }

        [Fact]
        public void Wp_Division_AddsDefinednessCondition()
        {
            // wp(x := 1 / y, x > 0) должен добавить условие y != 0
            var program = new Assignment("x",
                new BinaryOperation(new Constants(1), new Variable("y"), "/"));
            var post = new BinaryOperation(new Variable("x"), new Constants(0), ">");

            var result = _engine.Calculate(program, post);

            // Условие y != 0 должно попасть в DefinednessConditions
            Assert.Contains(result.DefinednessConditions,
                c => c.ToMathString().Contains("y") && c.ToMathString().Contains("!="));
        }

        [Fact]
        public void Wp_ProvidesTraceSteps()
        {
            var program = new Sequence(new List<Statement>
            {
                new Assignment("x", new Constants(1)),
                new Assignment("y", new Constants(2))
            });
            var post = new BinaryOperation(new Variable("y"), new Constants(0), ">");

            var result = _engine.Calculate(program, post);

            Assert.NotEmpty(result.TraceSteps);
        }

        [Fact]
        public void Wp_NoDivision_HasNoExtraDefinedness()
        {
            var program = new Assignment("x", new Constants(5));
            var post = new BinaryOperation(new Variable("x"), new Constants(0), ">");

            var result = _engine.Calculate(program, post);

            Assert.Empty(result.DefinednessConditions);
        }
    }
}
