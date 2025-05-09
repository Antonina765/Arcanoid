using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Arcanoid.Models;

public class GameStatistics
{
    public int Level { get; set; } = 1;
    public int Attempts { get; set; } = 3;
    public int Score { get; set; } = 0;
    public TextBlock StatsDisplay { get; set; }

    public GameStatistics()
    {
        StatsDisplay = new TextBlock
        {
            Foreground = new SolidColorBrush(Colors.White),
            FontSize = 20,
            HorizontalAlignment = HorizontalAlignment.Right, // чтобы текст выровнялся справа
            VerticalAlignment = VerticalAlignment.Top
            // Расположение можно задать через Canvas.SetLeft/Top при добавлении к основному канву
        };
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        StatsDisplay.Text = $"Уровень: {Level}   Попытки: {Attempts}   Счёт: {Score}";
    }

    public void AddScore(int points)
    {
        Score += points;
        UpdateDisplay();
    }
}