using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Controls.Shapes;

namespace Arcanoid.Models;

public class SpecialBall : CircleObject
{
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
        Console.WriteLine("Специальный шарик запущен");
        // При старте задаем случайный угол движения, например, от 45 до 135 градусов вверх
        AngleSpeed = new Random().NextDouble() * (Math.PI / 2) + (Math.PI / 4);
        IsLaunched = true;
    }

    public override void Move()
    {
        if (!IsLaunched)
            return;

        Console.WriteLine("Движется особый шарик");
        // Выполняем стандартное перемещение с отскоком от краев (рефлексия от платформы обрабатывается отдельно)
        base.Move();
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
    
    public override void Draw()
    {
        Canvas.SetLeft(Shape, X);
        Canvas.SetTop(Shape, Y);
    }
}