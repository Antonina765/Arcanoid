using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Arcanoid.Models;

public class BonusItem
{
    public BonusType Type { get; set; }
    public Rectangle Icon { get; private set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double FallSpeed { get; set; } = 2.0;

    public BonusItem(BonusType type, double startX, double startY)
    {
        Type = type;
        X = startX;
        Y = startY;

        // Простой значок – круг желтого цвета
        Icon = new Rectangle
        {
            Width = 30,
            Height = 30,
            Fill = IsPositive(type) ? Brushes.Green : Brushes.Red,
        };
        
        Canvas.SetLeft(Icon, X);
        Canvas.SetTop(Icon, Y);
    }

    public bool IsPositive(BonusType type)
    {
        return ((int)type) < 5;
    }
    public void Draw(Canvas canvas)
    {
        if (!canvas.Children.Contains(Icon))
            canvas.Children.Add(Icon);
        Canvas.SetLeft(Icon, X);
        Canvas.SetTop(Icon, Y);
    }

    public void UpdatePosition()
    {
        Y += FallSpeed;
        Canvas.SetTop(Icon, Y);
    }
}