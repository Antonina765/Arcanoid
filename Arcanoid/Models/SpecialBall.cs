using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Controls.Shapes;

namespace Arcanoid.Models;

public class SpecialBall : CircleObject
{
    private double _savedSpeedX;
    private double _savedSpeedY;
    private double _savedAngleSpeed;
    public bool IsLaunched { get; set; }
    public SpecialBall(Canvas canvas, int maxX, int maxY, List<int> size,
        byte R1, byte G1, byte B1, byte R2, byte G2, byte B2)
        : base(canvas, maxX, maxY, size, size, R1, G1, B1, R2, G2, B2)
    {
        // Принудительно устанавливаем маленький размер (например, 20)
        Size = new List<int> { 20 };
        // Особое оформление (более толстая обводка)
        Shape.StrokeThickness = 3;
        // Можно добавить анимацию мерцания или другой эффект
        IsLaunched = false;
    }

    public void Launch()
    {
        Console.WriteLine("Запуск особого шарика");
        Speed = 7;
        // Выбираем угол от -135 до -45 градусов, чтобы шарик летел вверх 
        Random rand = new Random();
        double angle = - (3 * Math.PI / 4) + (rand.NextDouble() * (Math.PI / 2)); 
        if (Math.Sin(angle) > 0) 
        { 
            angle = -Math.Abs(angle);
        } 
        AngleSpeed = angle; 
        IsLaunched = true; 
        /*Console.WriteLine("Специальный шарик запущен");
        // При старте задаем случайный угол движения, например, от 45 до 135 градусов вверх
        AngleSpeed = new Random().NextDouble() * (Math.PI / 2) + (Math.PI / 4);
        IsLaunched = true;*/
    }

    /// <summary>
    /// Переопределяем метод движения. Используем ту же физику, что и в базовом методе,
    /// но НЕ отражаем нижнюю границу – позволяем шарика уйти вниз.
    /// </summary>
    public override void Move()
    {
        if (!IsLaunched)
            return;

        // Вычисление смещения (как в базовом методе)
        double speedX = Speed * Math.Cos(AngleSpeed);
        double speedY = Speed * Math.Sin(AngleSpeed);
        double accelerationX = Acceleration * Math.Cos(AngleAcceleration);
        double accelerationY = Acceleration * Math.Sin(AngleAcceleration);

        X += speedX;
        Y += speedY;

        speedX += accelerationX;
        speedY += accelerationY;
        Speed = Math.Sqrt(speedX * speedX + speedY * speedY);
        AngleSpeed = Math.Atan2(speedY, speedX);

        // Отражаем по горизонтали (левая и правая границы)
        if (X <= 0 || X >= Canvas.Bounds.Width - Shape.Width)
        {
            AngleSpeed = Math.PI - AngleSpeed;
            X = Math.Max(0, Math.Min(X, Canvas.Bounds.Width - Shape.Width));
        }
        // Отражаем по верхней границе
        if (Y <= 0)
        {
            AngleSpeed = -AngleSpeed;
            Y = 0;
        }
        // Ниже не делаем проверки для нижней границы —
        // именно по этому условию в игровом цикле будет выполнен ResetGame()

        Canvas.SetLeft(Shape, X);
        Canvas.SetTop(Shape, Y);
    }

    // Обработка столкновения с обычным шариком: удаляем обычный шарик и выполняем отскок
    public void HandleCollisionWith(DisplayObject normalBall)
    {
        if (normalBall == null)
            return;

        // Изменяем угол движения – простая инверсия вертикальной компоненты скорости
        AngleSpeed = -AngleSpeed;
        Speed *= 1.1;  // увеличиваем скорость по желанию

        // Удаление обычного шарика будет происходить из менеджера шаров (например, удаляем объект из списка и с канвы)
    }
    
    // Останавливает движение шарика без сброса флага IsLaunched
    public void Stop()
    {
        if (IsLaunched)
        {
            // Вычисляем текущие компоненты скорости на основе Speed и AngleSpeed
            double currentSpeedX = Speed * Math.Cos(AngleSpeed);
            double currentSpeedY = Speed * Math.Sin(AngleSpeed);

            // Сохраняем текущие скорости
            _savedSpeedX = currentSpeedX;
            _savedSpeedY = currentSpeedY;
            _savedAngleSpeed = AngleSpeed;
                
            // Останавливаем шарик, устанавливая скорость в 0
            Speed = 0;
            AngleSpeed = 0;
        }
    }
    
    // Возобновляет движение шарика с сохраненными скоростями
    public void Resume()
    {
        if (IsLaunched && Speed == 0)
        {
            // Рассчитываем восстановленную скорость по сохранённым компонентам
            double restoredSpeed = Math.Sqrt(_savedSpeedX * _savedSpeedX + _savedSpeedY * _savedSpeedY);
            if (restoredSpeed > 0)
            {
                Speed = restoredSpeed;
                AngleSpeed = Math.Atan2(_savedSpeedY, _savedSpeedX);
            }
            else
            {
                // Если сохранённые значения равны нулю, устанавливаем стандартные значения
                double angle = new Random().NextDouble() * (Math.PI / 2) + (Math.PI / 4);  // угол от 45 до 135 градусов
                Speed = 7;
                AngleSpeed = angle;
            }
        }
    }
    
    public override void Draw()
    {
        Canvas.SetLeft(Shape, X);
        Canvas.SetTop(Shape, Y);
    }
}