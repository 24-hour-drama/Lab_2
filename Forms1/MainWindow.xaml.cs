using Library;
using Library.Lab2;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Forms1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NightlightState newNightlight;
        NightlightController controller = new NightlightController();
        CheckBox chk_power; //Создаём ссылку заранее
        Slider sld_brightness;
        ComboBox cmb_colors;
        Label lbl_actuall_brightness;
        List<string> colors = new List<string> { "Warm white", "Cold white", "White" };
        string pre_txt = "";
        string post_txt = "";

        private string currentHoareTriple = "";
        public MainWindow()
        {
            InitializeComponent();

            OperationsListBox.Items.Add("Операция 1: Включение");
            OperationsListBox.Items.Add("Операция 2: Настройка яркости");
            OperationsListBox.Items.Add("Операция 3: Настройка цвета");
            OperationsListBox.Items.Add("Операция 4: Таймер");
            OperationsListBox.Items.Add("Операция 5: Выключение");

            newNightlight = new NightlightState(); //Создаём новый объект для новой формы

            if (cmbPresets != null)
            {
                cmbPresets.Items.Add(new ComboBoxItem { Content = "1. Max из двух", Tag = "Max" });
                cmbPresets.Items.Add(new ComboBoxItem { Content = "2. Последовательность присваиваний", Tag = "Sequence" });
                cmbPresets.Items.Add(new ComboBoxItem { Content = "3. Квадратное уравнение", Tag = "Quadratic" });
                cmbPresets.SelectedIndex = 0;
            }
        }

        // Плавное появление окна при запуске
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var root = (FrameworkElement)this.Content;
            root.Opacity = 0;
            var tr = new TranslateTransform(0, 30);
            root.RenderTransform = tr;

            var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500));
            var slide = new DoubleAnimation(30, 0, TimeSpan.FromMilliseconds(500));

            root.BeginAnimation(OpacityProperty, fade);
            tr.BeginAnimation(TranslateTransform.YProperty, slide);
        }

        // Переключение вкладок — плавное появление содержимого
        private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tc && tc.SelectedItem is TabItem tab)
            {
                if (tab.Content is UIElement content)
                {
                    AnimateIn(content);
                }
            }
        }

        // Плавное появление элемента (сдвиг справа + прозрачность)
        private void AnimateIn(UIElement el)
        {
            el.Opacity = 0;
            var tr = new TranslateTransform(30, 0);
            el.RenderTransform = tr;

            var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250));
            var slide = new DoubleAnimation(30, 0, TimeSpan.FromMilliseconds(250));

            el.BeginAnimation(OpacityProperty, fade);
            tr.BeginAnimation(TranslateTransform.XProperty, slide);
        }

        // Короткий пульс (мигание) элемента
        private void Pulse(UIElement el)
        {
            var anim = new DoubleAnimation(1, 0.3, TimeSpan.FromMilliseconds(120))
            {
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(3)
            };
            el.BeginAnimation(OpacityProperty, anim);
        }

        #region
        // -----------------НОЧНИК-----------------
        // ОСНОВА ДЛЯ ВЫБОРА ОПЕРАЦИИ ИЗ СПИСКА
        private void OperationsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Проверяем, что пользователь действительно что-то выбрал
            if (OperationsListBox.SelectedItem != null)
            {
                //Очистка дочерних элементов перед созданием новых (удаление детей, ха-ха)
                FuncControlsPanel.Children.Clear();

                // Получаем выбранный элемент
                var selectedItem = OperationsListBox.SelectedItem;

                // Если в ListBox хранятся просто строки (как мы добавили в конструкторе),
                // то приводим его к строке и выводим в label1.
                // Если у вас там сложные объекты, нужно будет обращаться к их свойствам.
                string operationName = selectedItem.ToString();

                // Выводим название операции в заголовок
                label1.Content = operationName;

                // Сбрасываем старые результаты при выборе новой операции
                labelPreconditionValue.Content = "";
                labelPostconditionValue.Content = "";
                labelResult.Content = "";

                // Сбрасываем цвета квадратиков в серый
                labelPreconditionColor.Background = Brushes.Gray;
                labelPostconditionColor.Background = Brushes.Gray;

                //Создание дочерних элементов при выборе опр. действия
                switch (OperationsListBox.SelectedIndex)
                {
                    case 0:
                        pre_txt = "Питание есть (chk_power.IsChecked = true)\n" +
                            "Свет выключен (NightlightState.isOn = false)";
                        post_txt = "Свет включен (NightlightState.isOn = true)\n" +
                            "Если яркость была 0 (выключен) – яркость 50 (NightlightState.Brightness = 50)\n" +
                            "Если яркость была настроена ранее — значение сохраняется (NightlightState.Brightness не изменяется)";
                        ControlsForOn();
                        break;
                    case 1:
                        pre_txt = "Свет включен (NightlightState.isOn = true)\n" +
                            "Новая яркость не равна старой (newBrightness != oldBrightness)";
                        post_txt = "Яркость изменена (NightlightState.Brightness = newBrightness)\n" +
                            "Если яркость была настроена на 0 – выключение (NightlightState.isOn = false)\n" +
                            "Если яркость была настроена не на 0 — состояние сохраняется (NightlightState.isOn = true)";
                        ControlsForSet();
                        break;
                    case 2:
                        pre_txt = "Свет включен (NightlightState.isOn = true)\n" +
                            "Новый цвет отличается от прежнего (newColor != oldColor)";
                        post_txt = "Цвет изменен (NightlightState.currentColor = newColor)\n" +
                            "Включенное состояние сохраняется (NightlightState.isOn = true)";
                        ControlsForColor();
                        break;
                    case 4:
                        pre_txt = "Свет включен (NightlightState.isOn = true)\n";
                        post_txt = "Свет выключен (NightlightState.isOn = false)\n" +
                            "Сохранение настроенной яркости (NightlightState.Brightness не изменяется)";
                        ControlsForOff();
                        break;
                    default:
                        break;
                }
            }

            AnimateIn(label1);
            AnimateIn(FuncControlsPanel);
        }

        /// <summary>
        /// Создание элементов управления для функции включения
        /// </summary>
        private void ControlsForOn()
        {
            chk_power = new CheckBox
            {
                Content = "Питание есть", //Корректировка чекбокса
                FontSize = 14
            };

            chk_power.Checked += chk_power_select;
            chk_power.Unchecked += chk_power_select;

            FuncControlsPanel.Children.Add(chk_power);

            Label lbl_is_off = new Label
            {
                Content = "Свет должен быть выключен",
                FontSize = 14
            };
            FuncControlsPanel.Children.Add(lbl_is_off);
        }

        private void chk_power_select(object sender, EventArgs e)
        {
            if (chk_power.IsChecked == true)
            {
                PreWork();
                labelPostconditionColor.Background = Brushes.Gray;
                labelPostconditionValue.Content = $"";
                labelResult.Content = "";
            }
            else
            {
                PreDontWork("Нет питания");
                labelPostconditionColor.Background = Brushes.Gray;
                labelPostconditionValue.Content = $"";
                labelResult.Content = "Операция недоступна";
                if (newNightlight.isOn)
                    controller.TurnOff(newNightlight);
            }
        }

        /// <summary>
        /// Реакция лейбла на движение ползунка
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sld_brightness_set(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = (Slider)sender;
            var parent = slider.Parent as Panel;
            if (parent == null) return;

            lbl_actuall_brightness.Content = $"{e.NewValue:0}%";

            if (!newNightlight.isOn)
            {
                PreDontWork("Ночник выключен");
                labelPostconditionColor.Background = Brushes.Gray;
                labelPostconditionValue.Content = $"";
                labelResult.Content = "Операция недоступна";
            }
            else if (sld_brightness.Value == newNightlight.Brightness)
            {
                PreDontWork("Яркость не изменена");
                labelPostconditionColor.Background = Brushes.Gray;
                labelPostconditionValue.Content = $"";
                labelResult.Content = "Операция недоступна";
            }
            else
            {
                PreWork();
                labelPostconditionColor.Background = Brushes.Gray;
                labelPostconditionValue.Content = $"";
                labelResult.Content = "";
            }
            ;
        }

        public void ControlsForColor()
        {
            Label lbl_is_on = new Label
            {
                Content = "Свет должен быть включен",
                FontSize = 14
            };
            FuncControlsPanel.Children.Add(lbl_is_on);

            var row = new Grid { Margin = new Thickness(0, 4, 0, 4) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Label lbl_color = new Label
            {
                Content = "Новый цвет:",
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            };
            Grid.SetColumn(lbl_color, 0);
            row.Children.Add(lbl_color);

            cmb_colors = new ComboBox();
            foreach (string s in colors)
                cmb_colors.Items.Add(s);

            int indColor;
            switch (newNightlight.currentColor)
            {
                case "warmwhite":
                    indColor = 0;
                    break;
                case "coldwhite":
                    indColor = 1;
                    break;
                case "white":
                    indColor = 2;
                    break;
                default:
                    indColor = 2;
                    break;
            }       
            cmb_colors.SelectedIndex = indColor;

            cmb_colors.SelectionChanged += cmb_colors_selectionChanged;
            Grid.SetColumn(cmb_colors, 1);
            row.Children.Add(cmb_colors);

            FuncControlsPanel.Children.Add(row);
        }

        private void cmb_colors_selectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!newNightlight.isOn)
            {
                PreDontWork("Ночник выключен");
                labelPostconditionColor.Background = Brushes.Gray;
                labelPostconditionValue.Content = $"";
                labelResult.Content = "Операция недоступна";
            }
            else if (cmb_colors.SelectedItem.ToString().ToLower().Replace(" ", "") == newNightlight.currentColor)
            {
                PreDontWork("Цвет не изменён");
                labelPostconditionColor.Background = Brushes.Gray;
                labelPostconditionValue.Content = $"";
                labelResult.Content = "Операция недоступна";
            }
            else
            {
                PreWork();
                labelPostconditionColor.Background = Brushes.Gray;
                labelPostconditionValue.Content = $"";
                labelResult.Content = "";
            }
        }

        /// <summary>
        /// Создание элементов управления для функции настройки яркости
        /// </summary>
        private void ControlsForSet()
        {
            Label lbl_is_on = new Label
            {
                Content = "Свет должен быть включен",
                FontSize = 14
            };
            FuncControlsPanel.Children.Add(lbl_is_on);

            var row = new Grid { Margin = new Thickness(0, 4, 0, 4) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Label lbl_brightness = new Label
            {
                Content = "Новая яркость:",
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            };
            Grid.SetColumn(lbl_brightness, 0);
            row.Children.Add(lbl_brightness);

            sld_brightness = new Slider
            {
                Minimum = 0,
                Maximum = 100,
                Value = newNightlight.Brightness,
                TickFrequency = 1,
                IsSnapToTickEnabled = true,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(sld_brightness, 1);
            row.Children.Add(sld_brightness);

            lbl_actuall_brightness = new Label
            {
                Content = $"{newNightlight.Brightness}%",
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                MinWidth = 48,
                Margin = new Thickness(8, 0, 0, 0)
            };
            Grid.SetColumn(lbl_actuall_brightness, 2);
            row.Children.Add(lbl_actuall_brightness);

            sld_brightness.ValueChanged += sld_brightness_set;

            FuncControlsPanel.Children.Add(row);
        }

        /// <summary>
        /// Создание элементов управления для функции выключения
        /// </summary>
        private void ControlsForOff()
        {
            Label lbl_is_on = new Label
            {
                Content = "Свет должен быть включен",
                FontSize = 14
            };
            FuncControlsPanel.Children.Add(lbl_is_on);
        }

        /// <summary>
        /// ОБРАБОТЧИК КНОПКИ "ВЫПОЛНИТЬ"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Run_Click(object sender, RoutedEventArgs e)
        {
            switch (OperationsListBox.SelectedIndex)
            {
                case 0:
                    Run_On();
                    break;
                case 1:
                    Run_Set();
                    break;
                case 2:
                    Run_Color();
                    break;
                case 4:
                    Run_Off();
                    break;
                default:
                    break;
            }

            Pulse(labelResult);
            Pulse(labelPreconditionColor);
            Pulse(labelPostconditionColor);
        }

        /// <summary>
        /// Обработчик для функции включения
        /// </summary>
        private void Run_On()
        {
            //Проверка Pre
            bool havePower = chk_power.IsChecked == true;
            int oldBrightness = newNightlight.Brightness;
            if (!havePower)
            {
                PreDontWork("нет питания");
                PostDontWork("свет не был вкюлчён");
                return;
            }
            else if (newNightlight.isOn)
            {
                PreDontWork("свет уже включен");
                PostDontWork("свет не был вкюлчён");
                return;
            }
            else PreWork();

            //Настройка значений
            try
            {
                controller.TurnOn(newNightlight);
            }
            catch (Exception ex)
            {
                PreDontWork(ex.Message);
                PostDontWork(ex.Message);
                return;
            }

            //Проверка Post
            if (!newNightlight.isOn) PostDontWork("свет не был вкюлчён");
            else PostWork();
        }

        private void Run_Color()
        {
            //Проверка Pre
            string newColor = cmb_colors.SelectedItem.ToString().ToLower().Replace(" ", "");
            string oldColor = newNightlight.currentColor;
            if (!newNightlight.isOn)
            {
                PreDontWork("свет выключен");
                PostDontWork("яркость не была настроена");
                return;
            }
            else if (newColor == oldColor)
            {
                PreDontWork("не был выбран новый цвет");
                PostDontWork("цвет не был настроен");
                return;
            }
            else PreWork();

            //Настройка значений
            try
            { controller.SetColor(newNightlight, newColor); }
            catch (Exception ex)
            {
                PreDontWork(ex.Message);
                PostDontWork(ex.Message);
                return;
            }

            //Проверка Post
            if (newColor != newNightlight.currentColor) PostDontWork("цвет не был настроен");
            else PostWork();
        }

        /// <summary>
        /// Обработчик для функции изменения яркости
        /// </summary>
        private void Run_Set()
        {
            //Проверка Pre
            int newBrightness = (int)sld_brightness.Value;
            int oldBrightness = newNightlight.Brightness;
            if (!newNightlight.isOn)
            {
                PreDontWork("свет выключен");
                PostDontWork("яркость не была настроена");
                return;
            }
            else if (newBrightness == oldBrightness)
            {
                PreDontWork("новая яркость равна старой");
                PostDontWork("яркость не была настроена");
                return;
            }
            else PreWork();

            //Настройка значений
            try
            { controller.SetBrightness(newNightlight, newBrightness); }
            catch (Exception ex)
            {
                PreDontWork(ex.Message);
                PostDontWork(ex.Message);
                return;
            }

            //Проверка Post
            if (newBrightness != newNightlight.Brightness) PostDontWork("яркость не была настроена");
            else PostWork();
        }

        /// <summary>
        /// Обработчик для функции включения
        /// </summary>
        private void Run_Off()
        {
            //Проверка Pre
            if (!newNightlight.isOn)
            {
                PreDontWork("свет уже выключен");
                PostDontWork("свет не был выкюлчён");
                return;
            }
            else PreWork();

            //Настройка значений
            try
            { controller.TurnOff(newNightlight); }
            catch (Exception ex)
            {
                PreDontWork(ex.Message);
                PostDontWork(ex.Message);
                return;
            }

            //Проверка Post
            if (newNightlight.isOn) PostDontWork("свет не был выкюлчён");
            else PostWork();
        }

        /// <summary>
        /// При невыполненном Pre
        /// </summary>
        /// <param name="msg">Сообщение об ошибке</param>
        private void PreDontWork(string msg)
        {
            labelPreconditionColor.Background = Brushes.Red;
            labelPreconditionValue.Content = $"Ошибка: {msg}.";
        }

        /// <summary>
        /// При невыполненном Post
        /// </summary>
        /// <param name="msg"></param>
        private void PostDontWork(string msg)
        {
            labelPostconditionColor.Background = Brushes.Red;
            labelPostconditionValue.Content = $"Ошибка: {msg}.";
            labelResult.Content = "Операция была отменена.";
        }

        /// <summary>
        /// При выполненном Pre
        /// </summary>
        private void PreWork()
        {
            labelPreconditionColor.Background = Brushes.Green;
            labelPreconditionValue.Content = $"Выполнено.";
        }

        /// <summary>
        /// При выполненном Post
        /// </summary>
        private void PostWork()
        {
            labelPostconditionColor.Background = Brushes.Green;
            labelPostconditionValue.Content = $"Выполнено.";
            labelResult.Content = "Операция успешно завершена!";
        }

        // ОБРАБОТЧИК КНОПКИ "КОНТРАКТ
        private void Contract_Click(object sender, RoutedEventArgs e)
        {
            if (OperationsListBox.SelectedItem != null)
                MessageBox.Show($"{OperationsListBox.SelectedItem.ToString()}\n\n\n" +
                    $"pre:\n{pre_txt}\n\npost:\n{post_txt}",
                            $"Контракт операции {OperationsListBox.SelectedItem.ToString()}",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
            else MessageBox.Show($"Пожалуйста, выберете сначала операцию",
                            $"Операция не выбрана",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
        }


        #endregion fff

        #region
        // -----------------КОНСТРУКТОР ФРАГМЕНТА-----------------

        // Выбор пресета из списка
        private void CmbPresets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPresets.SelectedItem is ComboBoxItem item && item.Tag is string tag && tag != "None")
            {
                pnlTrace.Children.Clear();
                lblWpResult.Content = "-";
                lblWpHuman.Text = "-";
                lblDefinedness.Text = "-";
                currentHoareTriple = "";

                switch (tag)
                {
                    case "Max":
                        txtCode.Text = "if (x1 >= x2) { max = x1 } else { max = x2 }";
                        txtPost.Text = "max > 100";
                        break;
                    case "Sequence":
                        txtCode.Text = "x = x + 10; y = x + 1";
                        txtPost.Text = "(y == x - 9) && (x > 15)";
                        break;
                    case "Quadratic":
                        txtCode.Text = "if (D >= 0) { root = (-b + sqrt(D)) / (2 * a) } else { root = -1 }";
                        txtPost.Text = "(D >= 0 && a * root * root + b * root + c == 0) || (D < 0 && root == -1)";
                        break;
                }
            }
        }

        // Обработчик кнопки «Рассчитать wp»

        private void CalcWp_Click(object sender, RoutedEventArgs e)
        {
        }

        // Обработчик кнопки «Показать триаду»
        private void ShowTriad_Click(object sender, RoutedEventArgs e)
        {
            // показать {Pre} P {Post}
        }

        private Expressions ParseExpression(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) throw new Exception("Пустое выражение");
            var tokens = Tokenize(input);
            int pos = 0;
            return ParseOr(tokens, ref pos);
        }

        private List<string> Tokenize(string input)
        {
            var pattern = @"(\d+\.?\d*|[a-zA-Z_]\w*|>=|<=|==|!=|&&|\|\||[+\-*/()!><=]||sqrt\([a-zA-Z_]\w*\))";
            var matches = Regex.Matches(input, pattern);
            return matches.Cast<Match>().Select(m => m.Value).Where(v => v.Trim().Length > 0).ToList();
        }

        private Expressions ParseOr(List<string> t, ref int p) { var left = ParseAnd(t, ref p); while (p < t.Count && t[p] == "||") { p++; left = new BinaryOperation(left, ParseAnd(t, ref p), "||"); } return left; }
        private Expressions ParseAnd(List<string> t, ref int p) { var left = ParseEquality(t, ref p); while (p < t.Count && t[p] == "&&") { p++; left = new BinaryOperation(left, ParseEquality(t, ref p), "&&"); } return left; }
        private Expressions ParseEquality(List<string> t, ref int p) { var left = ParseRelational(t, ref p); while (p < t.Count && (t[p] == "==" || t[p] == "!=")) { string op = t[p++]; left = new BinaryOperation(left, ParseRelational(t, ref p), op); } return left; }
        private Expressions ParseRelational(List<string> t, ref int p) { var left = ParseAdditive(t, ref p); while (p < t.Count && (t[p] == ">" || t[p] == "<" || t[p] == ">=" || t[p] == "<=")) { string op = t[p++]; left = new BinaryOperation(left, ParseAdditive(t, ref p), op); } return left; }
        private Expressions ParseAdditive(List<string> t, ref int p) { var left = ParseMultiplicative(t, ref p); while (p < t.Count && (t[p] == "+" || t[p] == "-")) { string op = t[p++]; left = new BinaryOperation(left, ParseMultiplicative(t, ref p), op); } return left; }
        private Expressions ParseMultiplicative(List<string> t, ref int p) { var left = ParseUnary(t, ref p); while (p < t.Count && (t[p] == "*" || t[p] == "/")) { string op = t[p++]; left = new BinaryOperation(left, ParseUnary(t, ref p), op); } return left; }

        private Expressions ParseUnary(List<string> t, ref int p)
        {
            if (p < t.Count && (t[p] == "-" || t[p] == "!" || t[p] == "abs" || t[p] == "sqrt"))
            {
                string op = t[p++];
                return new UnaryOperation(ParseUnary(t, ref p), op);
            }
            return ParsePrimary(t, ref p);
        }

        private Expressions ParsePrimary(List<string> t, ref int p)
        {
            if (p >= t.Count) throw new Exception("Неожиданный конец выражения");
            string token = t[p++];
            if (token == "(") 
            { var expr = ParseOr(t, ref p); 
                if (p < t.Count && t[p] == ")")  p++; 
                return expr; 
            }
            if (double.TryParse(token, out double num)) return new Constants(num);
            return new Variable(token);
        }

        private Statement ParseStatement(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) throw new Exception("Пустая строка");
            var statements = new List<Statement>();
            int i = 0;
            while (i < input.Length)
            {
                while (i < input.Length && char.IsWhiteSpace(input[i])) i++;
                if (i >= input.Length) break;

                if (input.Substring(i).StartsWith("if"))
                {
                    int start = i; int braceCount = 0; bool inIf = false;
                    while (i < input.Length)
                    {
                        if (input[i] == '{') { braceCount++; inIf = true; }
                        if (input[i] == '}') braceCount--;
                        i++;
                        if (inIf && braceCount == 0) break;
                    }
                    statements.Add(ParseIfStatement(input.Substring(start, i - start).Trim()));
                }
                else
                {
                    int start = i;
                    while (i < input.Length && input[i] != ';') i++;
                    string stmtStr = input.Substring(start, i - start).Trim();
                    if (!string.IsNullOrEmpty(stmtStr)) statements.Add(ParseAssignment(stmtStr));
                    i++;
                }
            }
            return statements.Count == 1 ? statements[0] : new Sequence(statements);
        }
        private Statement ParseIfStatement(string input)
        {
            var condMatch = Regex.Match(input, @"if\s*\((.*?)\)\s*\{(.*)\}");
            if (!condMatch.Success) throw new Exception("Ошибка синтаксиса if. Проверьте скобки");

            string condStr = condMatch.Groups[1].Value.Trim();
            string inner = condMatch.Groups[2].Value.Trim();
            var condition = ParseExpression(condStr);

            int elseIdx = inner.LastIndexOf("} {");
            Statement trueStmt, falseStmt;

            if (elseIdx > 0)
            {
                trueStmt = ParseStatement(inner.Substring(0, elseIdx).Trim());
                falseStmt = ParseStatement(inner.Substring(elseIdx + 8).TrimEnd('}', ' ').Trim());
            }
            else
            {
                trueStmt = ParseStatement(inner.TrimEnd('}', ' ').Trim());
                falseStmt = new Sequence(new List<Statement>());
            }

            return new IfStatement(condition, trueStmt is Sequence s1 ? s1.Statements : new List<Statement> { trueStmt }, falseStmt is Sequence s2 ? s2.Statements : new List<Statement> { falseStmt });
        }

        private Statement ParseAssignment(string input)
        {
            var parts = input.Split(new[] { ":=", "=" }, StringSplitOptions.None);

            if (parts.Length != 2) throw new Exception($"Неверный формат присваивания");
            return new Assignment(parts[0].Trim(), ParseExpression(parts[1].Trim()));
        }
        #endregion
    }
}