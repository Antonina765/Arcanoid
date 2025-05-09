using Avalonia.Controls;
using Avalonia.Media;

namespace Arcanoid.Models;

public class ScoreMessage
{
    public string Message { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double FallSpeed { get; set; } = 1.0;
    public TextBlock TextBlock { get; private set; }

    public ScoreMessage(string message, double startX, double startY)
    {
        Message = message;
        X = startX;
        Y = startY;
        TextBlock = new TextBlock
        {
            Text = message,
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 24
        };
    }

    public void Draw(Canvas canvas)
    {
        if (!canvas.Children.Contains(TextBlock))
            canvas.Children.Add(TextBlock);
        Canvas.SetLeft(TextBlock, X);
        Canvas.SetTop(TextBlock, Y);
    }

    public void Update()
    {
        Y += FallSpeed;
    }
}