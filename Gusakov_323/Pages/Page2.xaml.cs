using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gusakov_323.Pages
{
    /// <summary>
    /// Логика взаимодействия для Page2.xaml
    /// </summary>
    public partial class Page2 : Page
    {
        // Перечисление для выбора функции
        private enum FunctionType
        {
            Sh,     // sh(x) - гиперболический синус
            X2,     // x² - квадрат
            Exp     // eˣ - экспонента
        }
        public Page2()
        {
            InitializeComponent();
            // Установка значений по умолчанию
            txtX.Text = "2";
            txtY.Text = "3";
            rbExp.IsChecked = true; // Выбираем eˣ по умолчанию
        }
        // Валидация ввода: только цифры, минус и точка
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9.\-]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void btnCalculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка на пустые поля
                if (string.IsNullOrWhiteSpace(txtX.Text) ||
                    string.IsNullOrWhiteSpace(txtY.Text))
                {
                    ShowError("Все поля должны быть заполнены!", "Ошибка ввода");
                    return;
                }

                // Парсинг чисел
                if (!double.TryParse(txtX.Text.Replace('.', ','), out double x) ||
                    !double.TryParse(txtY.Text.Replace('.', ','), out double y))
                {
                    ShowError("Введите корректные ЧИСЛОВЫЕ значения!\n" +
                             "Используйте только цифры, минус и точку.", "Ошибка ввода");
                    return;
                }

                // Определяем выбранную функцию
                FunctionType selectedFunction = FunctionType.Exp; // по умолчанию
                if (rbSh.IsChecked == true)
                    selectedFunction = FunctionType.Sh;
                else if (rbX2.IsChecked == true)
                    selectedFunction = FunctionType.X2;
                else if (rbExp.IsChecked == true)
                    selectedFunction = FunctionType.Exp;

                // Вычисление функции
                string result = CalculatePiecewiseFunction(x, y, selectedFunction);
                txtResult.Text = result;

                // Обновление статуса
                if (result.Contains("∞"))
                {
                    txtStatus.Text = "Бесконечность";
                    txtStatus.Foreground = new SolidColorBrush(Colors.Orange);
                }
                else if (result.Contains("NaN") || result.Contains("не определен"))
                {
                    txtStatus.Text = "Не определено";
                    txtStatus.Foreground = new SolidColorBrush(Colors.Red);
                }
                else if (result.Contains("комплексное"))
                {
                    txtStatus.Text = "Комплексный результат";
                    txtStatus.Foreground = new SolidColorBrush(Colors.Purple);
                }
                else
                {
                    txtStatus.Text = $"OK: {GetFunctionName(selectedFunction)}";
                    txtStatus.Foreground = new SolidColorBrush(Colors.Green);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtX.Clear();
            txtY.Clear();
            txtResult.Clear();
            txtStatus.Text = "Очищено";
            txtStatus.Foreground = new SolidColorBrush(Colors.Gray);
        }

        /// <summary>
        /// Вычисление кусочной функции a
        /// </summary>
        private string CalculatePiecewiseFunction(double x, double y, FunctionType funcType)
        {
            try
            {
                // Вычисляем f(x) в зависимости от выбора
                double fx = CalculateFx(x, funcType);

                // Проверка на переполнение f(x)
                if (double.IsInfinity(fx) || double.IsNaN(fx))
                {
                    return fx > 0 ? "∞ (f(x) слишком большое)" : "-∞ (f(x) слишком маленькое)";
                }

                // Вычисляем произведение xy для определения условия
                double xy = x * y;

                // Базовое выражение (f(x) + y)² для всех случаев
                double sum = fx + y;

                // Проверка на переполнение суммы
                if (double.IsInfinity(sum) || double.IsNaN(sum))
                {
                    return "∞ (переполнение в сумме f(x)+y)";
                }

                double baseSquare = sum * sum;

                // Проверка на переполнение квадрата
                if (double.IsInfinity(baseSquare))
                {
                    return "∞ (переполнение при возведении в квадрат)";
                }

                double result;

                // Определяем условие по знаку xy
                if (xy > 0) // Условие 1: xy > 0
                {
                    // a = (f(x) + y)² - √(f(x)·y)

                    // Проверка на отрицательное значение под корнем
                    double underRoot = fx * y;
                    if (underRoot < 0)
                    {
                        return "Комплексное число (отрицательное под корнем)";
                    }

                    double sqrtValue = Math.Sqrt(underRoot);

                    // Проверка на переполнение корня
                    if (double.IsInfinity(sqrtValue) || double.IsNaN(sqrtValue))
                    {
                        return "∞ (переполнение в корне)";
                    }

                    result = baseSquare - sqrtValue;

                    txtStatus.Text = $"Условие 1: xy = {xy:F2} > 0";
                }
                else if (xy < 0) // Условие 2: xy < 0
                {
                    // a = (f(x) + y)² + √(|f(x)·y|)

                    double underRoot = Math.Abs(fx * y);
                    double sqrtValue = Math.Sqrt(underRoot);

                    // Проверка на переполнение корня
                    if (double.IsInfinity(sqrtValue) || double.IsNaN(sqrtValue))
                    {
                        return "∞ (переполнение в корне)";
                    }

                    result = baseSquare + sqrtValue;

                    txtStatus.Text = $"Условие 2: xy = {xy:F2} < 0";
                }
                else // Условие 3: xy = 0 (с учетом погрешности)
                {
                    // a = (f(x) + y)² + 1
                    result = baseSquare + 1;

                    txtStatus.Text = $"Условие 3: xy = 0";
                }

                // Проверка результата
                if (double.IsInfinity(result))
                {
                    return result > 0 ? "∞" : "-∞";
                }
                else if (double.IsNaN(result))
                {
                    return "NaN";
                }
                else if (Math.Abs(result) > 1e100)
                {
                    // Очень большое число, показываем в экспоненциальной форме
                    return result.ToString("E10") + " (очень большое)";
                }
                else if (Math.Abs(result) < 1e-100 && result != 0)
                {
                    // Очень маленькое число
                    return result.ToString("E10") + " (очень маленькое)";
                }
                else
                {
                    // Нормальный результат с 10 знаками после запятой
                    return result.ToString("F10");
                }
            }
            catch (OverflowException)
            {
                return "∞ (переполнение)";
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex.Message}";
            }
        }

        /// <summary>
        /// Вычисление f(x) в зависимости от выбранной функции
        /// </summary>
        private double CalculateFx(double x, FunctionType funcType)
        {
            switch (funcType)
            {
                case FunctionType.Sh:
                    // sh(x) = (e^x - e^(-x)) / 2
                    try
                    {
                        return Math.Sinh(x);
                    }
                    catch (OverflowException)
                    {
                        return x > 0 ? double.PositiveInfinity : double.NegativeInfinity;
                    }

                case FunctionType.X2:
                    // x²
                    try
                    {
                        return x * x;
                    }
                    catch (OverflowException)
                    {
                        return double.PositiveInfinity;
                    }

                case FunctionType.Exp:
                    // e^x
                    try
                    {
                        return Math.Exp(x);
                    }
                    catch (OverflowException)
                    {
                        return x > 0 ? double.PositiveInfinity : 0;
                    }

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Получение названия функции для отображения
        /// </summary>
        private string GetFunctionName(FunctionType funcType)
        {
            switch (funcType)
            {
                case FunctionType.Sh: return "sh(x)";
                case FunctionType.X2: return "x²";
                case FunctionType.Exp: return "eˣ";
                default: return "?";
            }
        }

        private void ShowError(string message, string title)
        {
            txtStatus.Text = "Ошибка!";
            txtStatus.Foreground = new SolidColorBrush(Colors.Red);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // Дополнительная валидация при потере фокуса
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                if (textBox.Text.StartsWith("."))
                    textBox.Text = "0" + textBox.Text;
                if (textBox.Text.EndsWith("."))
                    textBox.Text = textBox.Text.TrimEnd('.');
            }
        }
    }
}
