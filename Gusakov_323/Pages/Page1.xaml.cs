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
    /// Логика взаимодействия для Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        
        public Page1()
        {
            InitializeComponent();
            // Установка значений по умолчанию для демонстрации
            txtX.Text = "1";
            txtY.Text = "2";
            txtZ.Text = "3";
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
                    string.IsNullOrWhiteSpace(txtY.Text) ||
                    string.IsNullOrWhiteSpace(txtZ.Text))
                {
                    ShowError("Все поля должны быть заполнены!", "Ошибка ввода");
                    return;
                }

                // Парсинг чисел (только цифровые значения)
                if (!double.TryParse(txtX.Text.Replace('.', ','), out double x) ||
                    !double.TryParse(txtY.Text.Replace('.', ','), out double y) ||
                    !double.TryParse(txtZ.Text.Replace('.', ','), out double z))
                {
                    ShowError("Введите корректные ЧИСЛОВЫЕ значения!\n" +
                             "Используйте только цифры, минус и точку.", "Ошибка ввода");
                    return;
                }

                // Вычисление функции
                string result = CalculateFunction(x, y, z);
                txtResult.Text = result;

                // Обновление статуса
                if (result.Contains("∞"))
                {
                    txtStatus.Text = "Результат: бесконечность";
                    txtStatus.Foreground = new SolidColorBrush(Colors.Orange);
                }
                else if (result.Contains("NaN"))
                {
                    txtStatus.Text = "Результат: не определен";
                    txtStatus.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    txtStatus.Text = "OK";
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
            txtZ.Clear();
            txtResult.Clear();
            txtStatus.Text = "Очищено";
            txtStatus.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private string CalculateFunction(double x, double y, double z)
        {
            try
            {
                // ФОРМУЛА: t = (2cos(x - π/6)) / (0.5 + sin²y) * (1 + z²/(3 - z²/5))

                // Проверка на слишком большие числа
                if (Math.Abs(x) > 1e308 || Math.Abs(y) > 1e308 || Math.Abs(z) > 1e308)
                {
                    return "∞ (число слишком большое)";
                }

                // Шаг 1: Вычисляем cos(x - π/6)
                double arg = x - Math.PI / 6;
                double cosValue = Math.Cos(arg);

                // Проверка на переполнение cos
                if (double.IsNaN(cosValue) || double.IsInfinity(cosValue))
                {
                    return "∞ (ошибка в cos)";
                }

                // Шаг 2: 2 * cos(...)
                double numerator = 2 * cosValue;

                // Шаг 3: Вычисляем sin²y
                double sinValue = Math.Sin(y);
                double sinSquared = sinValue * sinValue;

                // Проверка на переполнение sin
                if (double.IsNaN(sinSquared) || double.IsInfinity(sinSquared))
                {
                    return "∞ (ошибка в sin)";
                }

                // Шаг 4: Знаменатель первой дроби
                double denominator1 = 0.5 + sinSquared;

                // Проверка деления на ноль
                if (Math.Abs(denominator1) < 1e-15)
                {
                    return "∞ (деление на ноль)";
                }

                // Шаг 5: Первая часть
                double part1 = numerator / denominator1;

                // Проверка на бесконечность после деления
                if (double.IsInfinity(part1) || double.IsNaN(part1))
                {
                    return part1 > 0 ? "∞" : "-∞";
                }

                // Шаг 6: Вторая часть - вычисляем z²
                double zSquared = z * z;

                // Проверка на переполнение z²
                if (double.IsInfinity(zSquared))
                {
                    return "∞ (z² слишком большое)";
                }

                // Шаг 7: Вычисляем знаменатель второй дроби: 3 - z²/5
                double denominator2 = 3 - (zSquared / 5);

                // Проверка деления на ноль во второй дроби
                if (Math.Abs(denominator2) < 1e-15)
                {
                    return "∞ (деление на ноль)";
                }

                // Шаг 8: Вычисляем z² / (3 - z²/5)
                double fraction2 = zSquared / denominator2;

                // Проверка на бесконечность
                if (double.IsInfinity(fraction2) || double.IsNaN(fraction2))
                {
                    return fraction2 > 0 ? "∞" : "-∞";
                }

                // Шаг 9: 1 + fraction2
                double part2 = 1 + fraction2;

                // Шаг 10: Финальный результат
                double result = part1 * part2;

                // Проверка финального результата
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
            catch (Exception)
            {
                return "Ошибка вычисления";
            }
        }

        private void ShowError(string message, string title)
        {
            txtStatus.Text = "Ошибка!";
            txtStatus.Foreground = new SolidColorBrush(Colors.Red);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
