using System;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Controls.Shapes;

namespace Arcanoid.Models;

public class Platform : DisplayObject
{
    public Platform(Canvas canvas, double width, double height)
        : base(canvas, (int)canvas.Bounds.Width, (int)canvas.Bounds.Height)
    {
        // Рисуем платформу как прямоугольник
        Shape = new Rectangle
        {
            Width = width,
            Height = height,
            Fill = new SolidColorBrush(Colors.White)
        };

        canvas.Children.Add(Shape);

        // Начальное положение: центр по нижней границе
        X = (canvas.Bounds.Width - width) / 2;
        Y = canvas.Bounds.Height - height - 10;
        Draw();
    }

    public override void Draw()
    {
        Canvas.SetLeft(Shape, X);
        Canvas.SetTop(Shape, Y);
    }

    public void MoveLeft(double distance)
    {
        X = Math.Max(0, X - distance);
        Draw();
    }

    public void MoveRight(double distance)
    {
        X = Math.Min(Canvas.Bounds.Width - Shape.Bounds.Width, X + distance);
        Draw();
    }
}
