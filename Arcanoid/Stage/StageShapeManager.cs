using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Arcanoid.Models;

namespace Arcanoid.Stage;

public class StageShapeManager
{
    private readonly Stage _stage;

    public StageShapeManager(Stage stage) 
    { 
        _stage = stage;
    }
    
    public void AddRandomShapes(int count, int maxX, int maxY) 
    { 
        var random = new Random(); 
        Console.WriteLine($"{maxX} - {maxY}");
        
        for (int i = 0; i < count; i++) 
        { 
            // Создаем круг
            var (R1, G1, B1) = GetRandomBrush();
            var (R2, G2, B2) = GetRandomBrush(); 
            _stage.Shapes.Add(new CircleObject(
                _stage.GameCanvas, 
                maxX, maxY, 
                new List<int> { random.Next(20, 150) },
                new List<int> { random.Next(20, 150) },
                R1, G1, B1, R2, G2, B2)
            );

            /*// Создаем прямоугольник
            (R1, G1, B1) = GetRandomBrush(); 
            (R2, G2, B2) = GetRandomBrush(); 
            _stage.Shapes.Add(new RectangleObject(
                _stage.GameCanvas, 
                maxX, maxY, 
                new List<int> { random.Next(20, 150), random.Next(20, 150) },
                R1, G1, B1, R2, G2, B2)
            );

            // Создаем треугольник
            (R1, G1, B1) = GetRandomBrush(); 
            (R2, G2, B2) = GetRandomBrush(); 
            _stage.Shapes.Add(new TriangleShape(
                _stage.GameCanvas, 
                maxX, maxY, 
                new List<int> { random.Next(20, 70), random.Next(20, 70), random.Next(20, 70) },
                R1, G1, B1, R2, G2, B2)
            );

            // Создаем трапецию
            (R1, G1, B1) = GetRandomBrush(); 
            (R2, G2, B2) = GetRandomBrush(); 
            _stage.Shapes.Add(new TrapezoidObject(
                _stage.GameCanvas, 
                maxX, maxY, 
                new List<int> { random.Next(20, 70), random.Next(20, 70), random.Next(20, 70) },
                R1, G1, B1, R2, G2, B2)
            );*/
        }
    }

    public void ClearCanvas() 
    { 
        _stage.GameCanvas.Children.Clear(); 
        _stage.Shapes.Clear();
    }

    public static (byte, byte, byte) GetRandomBrush() 
    { 
        Random rand = new Random(); 
        byte r = (byte)rand.Next(256); 
        byte g = (byte)rand.Next(256); 
        byte b = (byte)rand.Next(256); 
        return (r, g, b);
    }
}
