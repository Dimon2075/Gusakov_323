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
    /// Логика взаимодействия для Page3.xaml
    /// </summary>
    

    public partial class Page3 : Page
    {
        // Класс для точки данных
        public class DataPoint
        {
            public double X { get; set; }
            public double Y { get; set; }
            public bool IsValid { get; set; }
            public string Note { get; set; }
        }
        private List<DataPoint> dataPoints = new List<DataPoint>();
        private double currentCoefficient = 0;
        private double currentCosSqrtA3 = 0;
        private double currentA = 0, currentB = 0, currentC = 0;

        // Для масштабирования графика
        private double minX, maxX, minY, maxY;
        private const double canvasWidth = 800;
        private const double canvasHeight = 400;
        private const double margin = 50; // Отступ от краев canvas

        public Page3()
        {
            InitializeComponent();
            // Установка значений по умолчанию
            txtX0.Text = "0.5";
            txtXk.Text = "2.5";
            txtDx.Text = "0.2";
            txtA.Text = "2";
            txtB.Text = "3";
            txtC.Text = "1";
        }

        // Валидация ввода: только цифры, минус и /точка
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9.\-]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        // Обработка ввода с клавиатуры (запрещаем пробел)
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void btnCalculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка на пустые поля
                if (string.IsNullOrWhiteSpace(txtX0.Text) ||
                    string.IsNullOrWhiteSpace(txtXk.Text) ||
                    string.IsNullOrWhiteSpace(txtDx.Text) ||
                    string.IsNullOrWhiteSpace(txtA.Text) ||
                    string.IsNullOrWhiteSpace(txtB.Text) ||
                    string.IsNullOrWhiteSpace(txtC.Text))
                {
                    ShowError("Все поля должны быть заполнены!", "Ошибка ввода");
                    return;
                }

                // Парсинг чисел
                if (!TryParseDouble(txtX0.Text, out double x0) ||
                    !TryParseDouble(txtXk.Text, out double xk) ||
                    !TryParseDouble(txtDx.Text, out double dx) ||
                    !TryParseDouble(txtA.Text, out double a) ||
                    !TryParseDouble(txtB.Text, out double b) ||
                    !TryParseDouble(txtC.Text, out double c))
                {
                    ShowError("Введите корректные ЧИСЛОВЫЕ значения!\n" +
                             "Используйте цифры, минус и запятую/точку.", "Ошибка ввода");
                    return;
                }

                // Сохраняем параметры
                currentA = a;
                currentB = b;
                currentC = c;

                // Проверка шага
                if (dx == 0)
                {
                    ShowError("Шаг dx не может быть равен нулю!", "Ошибка");
                    return;
                }

                // Проверка на отрицательные значения под корнем
                if (a < 0)
                {
                    ShowError("Параметр a должен быть ≥ 0 (под корнем a³)!", "Ошибка");
                    return;
                }

                // Очищаем предыдущие данные
                dataPoints.Clear();
                GraphCanvas.Children.Clear();

                // Вычисляем постоянную часть: cos(√(a³))
                double aCubed = a * a * a;
                double sqrtA3 = Math.Sqrt(aCubed);
                currentCosSqrtA3 = Math.Cos(sqrtA3);

                // Вычисляем коэффициент: 10⁻²·b·c = 0.01 * b * c
                currentCoefficient = 0.01 * b * c;

                // Выполняем табуляцию
                StringBuilder results = new StringBuilder();
                results.AppendLine("┌────────────┬──────────────┬─────────────┐");
                results.AppendLine("│     x      │      y       │  Примечание │");
                results.AppendLine("├────────────┼──────────────┼─────────────┤");

                int count = 0;
                double currentX = x0;
                int maxPoints = 500; // Ограничение для производительности

                // Определяем направление и выполняем цикл
                if (dx > 0)
                {
                    while (currentX <= xk + dx / 2 && count < maxPoints)
                    {
                        AddDataPoint(currentX, results);
                        currentX += dx;
                        count++;
                    }
                }
                else // dx < 0
                {
                    while (currentX >= xk + dx / 2 && count < maxPoints)
                    {
                        AddDataPoint(currentX, results);
                        currentX += dx;
                        count++;
                    }
                }

                results.AppendLine("└────────────┴──────────────┴─────────────┘");
                results.AppendLine($"\nПараметры: a = {a:F3}, b = {b:F3}, c = {c:F3}");
                results.AppendLine($"cos(√(a³)) = cos(√({aCubed:F3})) = {currentCosSqrtA3:F6}");
                results.AppendLine($"10⁻²·b·c = 0.01 · {b:F3} · {c:F3} = {currentCoefficient:F6}");
                results.AppendLine($"Всего точек: {count}");

                // Выводим результаты
                txtResults.Text = results.ToString();

                // Рисуем график
                DrawGraph();

                // Обновляем статус
                txtStatus.Text = $"Вычисление завершено. Получено {count} точек. " +
                                $"cos(√(a³)) = {currentCosSqrtA3:F4}, 10⁻²·b·c = {currentCoefficient:F4}";
                txtStatus.Foreground = new SolidColorBrush(Colors.Green);
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private void AddDataPoint(double x, StringBuilder results)
        {
            string note = "";
            double y;
            bool isValid = true;

            try
            {
                // Проверка деления на ноль
                if (Math.Abs(x) < 1e-15)
                {
                    y = double.NaN;
                    note = "деление на 0";
                    isValid = false;
                }
                else
                {
                    // y = (10⁻²·b·c)/x + cos(√(a³))·x
                    double term1 = currentCoefficient / x;
                    double term2 = currentCosSqrtA3 * x;
                    y = term1 + term2;

                    // Проверка на переполнение
                    if (double.IsInfinity(y) || double.IsNaN(y))
                    {
                        note = double.IsInfinity(y) ? "бесконечность" : "не определено";
                        isValid = false;
                    }
                }
            }
            catch (OverflowException)
            {
                y = double.NaN;
                note = "переполнение";
                isValid = false;
            }

            // Добавляем точку в список
            dataPoints.Add(new DataPoint
            {
                X = x,
                Y = isValid ? y : double.NaN,
                IsValid = isValid,
                Note = note
            });

            // Форматируем вывод
            if (!isValid)
            {
                if (note == "деление на 0")
                    results.AppendLine($"│ {x,10:F4} │ {"деление на 0",12} │ {note,-11} │");
                else
                    results.AppendLine($"│ {x,10:F4} │ {"NaN",12} │ {note,-11} │");
            }
            else
            {
                results.AppendLine($"│ {x,10:F4} │ {y,12:F6} │ {note,-11} │");
            }
        }

        private void DrawGraph()
        {
            // Очищаем canvas
            GraphCanvas.Children.Clear();

            if (dataPoints.Count == 0) return;

            // Находим валидные точки для масштабирования
            List<DataPoint> validPoints = dataPoints.FindAll(p => p.IsValid);

            if (validPoints.Count == 0)
            {
                // Нет валидных точек для отображения
                TextBlock noDataText = new TextBlock
                {
                    Text = "Нет валидных точек для отображения графика",
                    FontSize = 14,
                    Foreground = Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Canvas.SetLeft(noDataText, canvasWidth / 2 - 150);
                Canvas.SetTop(noDataText, canvasHeight / 2);
                GraphCanvas.Children.Add(noDataText);
                return;
            }

            // Находим минимумы и максимумы
            minX = double.MaxValue;
            maxX = double.MinValue;
            minY = double.MaxValue;
            maxY = double.MinValue;

            foreach (var p in validPoints)
            {
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }

            // Добавляем отступы (10% с каждой стороны)
            double xRange = maxX - minX;
            double yRange = maxY - minY;

            if (xRange < 1e-10) xRange = 1;
            if (yRange < 1e-10) yRange = 1;

            minX -= xRange * 0.1;
            maxX += xRange * 0.1;
            minY -= yRange * 0.1;
            maxY += yRange * 0.1;

            // Обновляем информацию о масштабе
            txtScaleInfo.Text = $"Масштаб: X:[{minX:F2}; {maxX:F2}], Y:[{minY:F2}; {maxY:F2}]";

            // Рисуем оси координат
            DrawAxes();

            // Рисуем сетку
            DrawGrid();

            // Рисуем точки и соединяем их линиями
            DrawPointsAndLines(validPoints);

            // Добавляем подписи осей
            AddAxisLabels();
        }

        private void DrawAxes()
        {
            // Ось X (горизонтальная)
            Line xAxis = new Line
            {
                X1 = margin,
                Y1 = canvasHeight - margin,
                X2 = canvasWidth - margin,
                Y2 = canvasHeight - margin,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            GraphCanvas.Children.Add(xAxis);

            // Ось Y (вертикальная)
            Line yAxis = new Line
            {
                X1 = margin,
                Y1 = margin,
                X2 = margin,
                Y2 = canvasHeight - margin,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            GraphCanvas.Children.Add(yAxis);

            // Стрелочки на осях
            Polygon xArrow = new Polygon
            {
                Points = new PointCollection
                {
                    new Point(canvasWidth - margin, canvasHeight - margin),
                    new Point(canvasWidth - margin - 10, canvasHeight - margin - 5),
                    new Point(canvasWidth - margin - 10, canvasHeight - margin + 5)
                },
                Fill = Brushes.Black
            };
            GraphCanvas.Children.Add(xArrow);

            Polygon yArrow = new Polygon
            {
                Points = new PointCollection
                {
                    new Point(margin, margin),
                    new Point(margin - 5, margin + 10),
                    new Point(margin + 5, margin + 10)
                },
                Fill = Brushes.Black
            };
            GraphCanvas.Children.Add(yArrow);
        }

        private void DrawGrid()
        {
            // Количество линий сетки
            int gridLines = 5;

            // Вертикальные линии сетки
            for (int i = 0; i <= gridLines; i++)
            {
                double x = margin + i * (canvasWidth - 2 * margin) / gridLines;

                Line gridLine = new Line
                {
                    X1 = x,
                    Y1 = margin,
                    X2 = x,
                    Y2 = canvasHeight - margin,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection { 2, 2 }
                };
                GraphCanvas.Children.Add(gridLine);

                // Подпись значения X
                double xValue = minX + i * (maxX - minX) / gridLines;
                TextBlock xLabel = new TextBlock
                {
                    Text = xValue.ToString("F2"),
                    FontSize = 9,
                    Foreground = Brushes.Gray
                };
                Canvas.SetLeft(xLabel, x - 15);
                Canvas.SetTop(xLabel, canvasHeight - margin + 5);
                GraphCanvas.Children.Add(xLabel);
            }

            // Горизонтальные линии сетки
            for (int i = 0; i <= gridLines; i++)
            {
                double y = canvasHeight - margin - i * (canvasHeight - 2 * margin) / gridLines;

                Line gridLine = new Line
                {
                    X1 = margin,
                    Y1 = y,
                    X2 = canvasWidth - margin,
                    Y2 = y,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection { 2, 2 }
                };
                GraphCanvas.Children.Add(gridLine);

                // Подпись значения Y
                double yValue = minY + i * (maxY - minY) / gridLines;
                TextBlock yLabel = new TextBlock
                {
                    Text = yValue.ToString("F2"),
                    FontSize = 9,
                    Foreground = Brushes.Gray
                };
                Canvas.SetLeft(yLabel, margin - 30);
                Canvas.SetTop(yLabel, y - 8);
                GraphCanvas.Children.Add(yLabel);
            }
        }

        private void DrawPointsAndLines(List<DataPoint> validPoints)
        {
            if (validPoints.Count == 0) return;

            // Создаем коллекцию точек для полилинии
            PointCollection polylinePoints = new PointCollection();

            foreach (var point in validPoints)
            {
                // Преобразуем координаты в координаты canvas
                double canvasX = margin + (point.X - minX) / (maxX - minX) * (canvasWidth - 2 * margin);
                double canvasY = (canvasHeight - margin) - (point.Y - minY) / (maxY - minY) * (canvasHeight - 2 * margin);

                // Добавляем точку в полилинию
                polylinePoints.Add(new Point(canvasX, canvasY));

                // Рисуем точку (кружок)
                Ellipse ellipse = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = Brushes.Red,
                    Stroke = Brushes.DarkRed,
                    StrokeThickness = 1,
                    ToolTip = $"x = {point.X:F4}\ny = {point.Y:F4}"
                };
                Canvas.SetLeft(ellipse, canvasX - 3);
                Canvas.SetTop(ellipse, canvasY - 3);
                GraphCanvas.Children.Add(ellipse);
            }

            // Рисуем линию, соединяющую точки
            if (polylinePoints.Count > 1)
            {
                Polyline polyline = new Polyline
                {
                    Points = polylinePoints,
                    Stroke = Brushes.Blue,
                    StrokeThickness = 2,
                    Opacity = 0.7
                };
                GraphCanvas.Children.Add(polyline);
            }
        }

        private void AddAxisLabels()
        {
            // Подпись оси X
            TextBlock xAxisLabel = new TextBlock
            {
                Text = "x",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(xAxisLabel, canvasWidth - margin + 5);
            Canvas.SetTop(xAxisLabel, canvasHeight - margin - 20);
            GraphCanvas.Children.Add(xAxisLabel);

            // Подпись оси Y
            TextBlock yAxisLabel = new TextBlock
            {
                Text = "y",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(yAxisLabel, margin + 5);
            Canvas.SetTop(yAxisLabel, margin - 25);
            GraphCanvas.Children.Add(yAxisLabel);

            // Название функции
            TextBlock funcLabel = new TextBlock
            {
                Text = $"y = 10⁻²·{currentB}·{currentC}/x + cos(√({currentA}³))·x",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C3483"))
            };
            Canvas.SetLeft(funcLabel, canvasWidth / 2 - 150);
            Canvas.SetTop(funcLabel, 10);
            GraphCanvas.Children.Add(funcLabel);
        }

        private void GraphCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            // Отображение координат мыши на графике
            Point mousePos = e.GetPosition(GraphCanvas);

            if (mousePos.X >= margin && mousePos.X <= canvasWidth - margin &&
                mousePos.Y >= margin && mousePos.Y <= canvasHeight - margin)
            {
                // Преобразуем координаты canvas в координаты графика
                double graphX = minX + (mousePos.X - margin) / (canvasWidth - 2 * margin) * (maxX - minX);
                double graphY = maxY - (mousePos.Y - margin) / (canvasHeight - 2 * margin) * (maxY - minY);

                txtCursorCoords.Text = $"x = {graphX:F4}, y = {graphY:F4}";
            }
            else
            {
                txtCursorCoords.Text = "";
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtX0.Clear();
            txtXk.Clear();
            txtDx.Clear();
            txtA.Clear();
            txtB.Clear();
            txtC.Clear();
            txtResults.Clear();
            GraphCanvas.Children.Clear();
            txtCursorCoords.Text = "";
            dataPoints.Clear();
            txtStatus.Text = "Поля очищены";
            txtStatus.Foreground = new SolidColorBrush(Colors.Gray);
            txtScaleInfo.Text = "Масштаб: автоматический";
        }

        private bool TryParseDouble(string input, out double result)
        {
            // Замена точки на запятую для корректного парсинга
            input = input?.Trim().Replace('.', ',');

            // Замена пустой строки на 0
            if (string.IsNullOrWhiteSpace(input))
            {
                result = 0;
                return true;
            }

            return double.TryParse(input, out result);
        }

        private void ShowError(string message, string title)
        {
            txtStatus.Text = "Ошибка!";
            txtStatus.Foreground = new SolidColorBrush(Colors.Red);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

    }
}
