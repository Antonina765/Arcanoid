using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using Arcanoid.Models;
using Avalonia.Layout;
using Avalonia.Threading;

namespace Arcanoid.Stage;

public class Stage
{
    public Canvas GameCanvas { get; private set; }
        
    public StageShapeManager ShapeManager { get; private set; }
    public StageMovementManager MovementManager { get; private set; }
    public StageDataManager DataManager { get; private set; }
    
    public Stage()
    {
        GameCanvas = new Canvas
        {
            Background = Brushes.Black
        };
            
        ShapeManager = new StageShapeManager(GameCanvas);
        MovementManager = new StageMovementManager(ShapeManager);
        DataManager = new StageDataManager(GameCanvas, ShapeManager);
    }
        
    // Статический метод для генерации случайного цвета
    public static (byte, byte, byte) GetRandomBrush()
    {
        Random rand = new Random();
        byte r = (byte)rand.Next(256);
        byte g = (byte)rand.Next(256);
        byte b = (byte)rand.Next(256);
        return (r, g, b);
    }
}