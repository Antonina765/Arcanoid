using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Arcanoid.Models;

public class BonusItem
{
    public BonusType Type { get; set; }
    public Rectangle Icon { get; private set; }
    
    public TextBlock Label { get; private set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double FallSpeed { get; set; } = 2.0;

    private const double Size = 30;

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
        
        // Создаем текстовый блок с заглавной буквой бонуса.
        // Здесь, например, берем первую букву названия enum'а.
        string bonusSymbol = type.GetSymbol();
        Label = new TextBlock
        {
            Text = bonusSymbol,
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White
        };

        // Примерно центрируем букву внутри бонуса (подгоните отступ при необходимости)
        Canvas.SetLeft(Label, X + (Size - 16) / 2); // 16 — приближенная ширина символа
        Canvas.SetTop(Label, Y + (Size - 16) / 2);
    }

    public bool IsPositive(BonusType type)
    {
        return ((int)type) < 5;
    }
    
    private string GetBonusLetter(BonusType type)
    {
        string typeName = type.ToString();
        if (!string.IsNullOrEmpty(typeName))
            return typeName.Substring(0, 1).ToUpper(); // первая буква названия типа бонуса
        return "X";
    }
    
    public void Draw(Canvas canvas)
    {
        if (!canvas.Children.Contains(Icon))
            canvas.Children.Add(Icon);
        if (!canvas.Children.Contains(Label))
            canvas.Children.Add(Label);
        Canvas.SetLeft(Icon, X);
        Canvas.SetTop(Icon, Y);
        
        // Центрируем текст внутри бонуса
        Canvas.SetLeft(Label, X + (Size - 16) / 2);
        Canvas.SetTop(Label, Y + (Size - 16) / 2);
    }

    public void UpdatePosition()
    {
        Y += FallSpeed;
        Canvas.SetTop(Icon, Y);
        Canvas.SetTop(Label, Y + (Size - 16) / 2);
    }
}