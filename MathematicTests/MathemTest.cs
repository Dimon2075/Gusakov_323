using Gusakov_323.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MathematicTests
{
    /// <summary>
    /// Тесты для математической функции из Практической работы №4.
    /// </summary>
    [TestClass]
    public class Funct1
    {
        /// <summary>
        /// Проверка работы функции при нулевых входных значениях.
        /// Ожидается, что результат начинается с определенной строки.
        /// </summary>
        [TestMethod]
        public void CalculateFunction_WithZeroValues_ReturnsExpectedResult()
        {
            var page = new Page1();
            double x = 0;
            double y = 0;
            double z = 0;

            // Вычисление функции
            string result = page.CalculateFunction(x, y, z);

            // Проверка, что результат начинается с ожидаемой части
            Assert.IsTrue(result.StartsWith("3,4641"), $"Фактический результат: {result}");
        }

        /// <summary>
        /// Проверка результата при очень больших числах.
        /// Ожидается, что результат будет содержать символ бесконечности.
        /// </summary>
        [TestMethod]
        public void CalculateFunction_WithLargeValues_ReturnsInfinity()
        {
            // Arrange
            var page = new Page1();

            // Используем бесконечность для теста
            double x = double.PositiveInfinity;
            double y = double.PositiveInfinity;
            double z = double.PositiveInfinity;

            // Выполнение функции
            string result = page.CalculateFunction(x, y, z);

            // Ожидается наличие символа бесконечности в результате
            string expectedSymbol = double.PositiveInfinity.ToString();
            Assert.IsTrue(result.Contains(expectedSymbol),
                $"Ожидалось, что результат '{result}' будет содержать символ бесконечности '{expectedSymbol}'");
        }

        /// <summary>
        /// Проверка обработки деления на ноль внутри функции.
        /// В результате ожидается символ бесконечности.
        /// </summary>
        [TestMethod]
        public void CalculateFunction_DivisionByZero_ReturnsInfinity()
        {
            var page = new Page1();

            // Arrange: z выбирается так, чтобы знаменатель был равен нулю
            double x = 0.0;
            double y = 0.0;
            double z = Math.Sqrt(15); // чтобы 3 - z^2/5 = 0

            // Выполнение
            string result = page.CalculateFunction(x, y, z);

            // Проверка, что результат содержит символ бесконечности
            Assert.IsTrue(result.Contains("∞"), $"Ожидалась бесконечность, но метод вернул: {result}");
        }

    }

    /// <summary>
    /// Тесты для метода вычисления в <see cref="Page2"/>.
    /// </summary>
    [TestClass]
    public class Funct2
    {
        /// <summary>
        /// Проверка метода для положительных значений X и Y.
        /// Ожидается, что результат не содержит ошибок или ошибок NaN.
        /// </summary>
        [TestMethod]
        public void CalculatePiecewiseFunction_PositiveXAndY_ReturnsExpectedResult()
        {
            var page = new Page2();

            // Исходные параметры
            double x = 2.0;
            double y = 3.0;

            // Вызов метода (предположим, что FunctionType.X2 — один из вариантов)
            var result = page.CalculatePiecewiseFunction(x, y, Page2.FunctionType.X2);

            // Проверки
            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.IsFalse(result.Contains("NaN"));
        }
    }

    /// <summary>
    /// Юнит-тесты для <see cref="Page3"/>.
    /// </summary>
    [TestClass]
    public class Funct3
    {
        /// <summary>
        /// Проверка метода вычисления с валидными входными данными.
        /// </summary>
        [TestMethod]
        public void TestCalculateWithValidInput()
        {
            // Создаем экземпляр страницы
            var page = new Page3();

            // Задаем тестовые входные данные
            page.txtX0.Text = "1";
            page.txtXk.Text = "2";
            page.txtDx.Text = "0.5";
            page.txtA.Text = "4";
            page.txtB.Text = "5";
            page.txtC.Text = "6";

            // Вызов обработчика
            page.btnCalculate_Click(null, null);

            // Проверка, что результаты заполнены
            Assert.IsFalse(string.IsNullOrEmpty(page.txtResults.Text), "Результаты должны быть заполнены");

            // Проверка, что в результате есть ожидаемые строки
            StringAssert.Contains(page.txtResults.Text, "a = 4");
            StringAssert.Contains(page.txtResults.Text, "b = 5");
            StringAssert.Contains(page.txtResults.Text, "c = 6");

            // Проверка, что график построен (например, есть точки)
            Assert.IsTrue(page.dataPoints.Count > 0, "Должны быть сгенерированы точки данных");
        }
    }
}