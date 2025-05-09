using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Arcanoid.Models;
using Avalonia.Controls;
using Avalonia.Media;

namespace Arcanoid.Stage;

public class StageShapeManager
{ 
    public List<DisplayObject> Shapes { get; private set; } = new List<DisplayObject>(); 
    private readonly Canvas _canvas;
    
    public StageShapeManager(Canvas canvas) 
    { 
        _canvas = canvas;
    }
    
    public void RedrawCanvas() 
    { 
        _canvas.InvalidateVisual();
    }
    
    public void AddRandomShapes(int count, int maxX, int maxY) 
    { 
        var random = new Random(); 
        Console.WriteLine($"{maxX} - {maxY}");
        
        for (int i = 0; i < count; i++) 
        { 
            var (R1, G1, B1) = Stage.GetRandomBrush(); 
            var (R2, G2, B2) = Stage.GetRandomBrush();
            
            var shape = new CircleObject(
                _canvas, 
                maxX, 
                maxY, 
                new List<int> { random.Next(50, 100) },
                new List<int> { random.Next(50, 100) },
                R1, G1, B1, R2, G2, B2
                );

            Shapes.Add(shape);
        }
    }
    
    public void ClearShapes() 
    { 
        _canvas.Children.Clear(); 
        Shapes.Clear();
    }
    
    public void CheckCollision(DisplayObject shape, int idx) 
    { 
        for (int i = 0; i < Shapes.Count; i++) 
        { 
            if (i == idx) continue; 
            if (IsColliding(Shapes[i], shape)) 
            { 
                HandleCollision(Shapes[i], shape); 
                ResolveOverlap(Shapes[i], shape); 
                Console.WriteLine("Collision detected! Game stopped."); 
                return;
            }
        }
    }
    
    private bool IsColliding(DisplayObject shape1, DisplayObject shape2) 
    { 
        var dx = shape1.X + shape1.Size[0] / 2 - (shape2.X + shape2.Size[0] / 2); 
        var dy = shape1.Y + shape1.Size[0] / 2 - (shape2.Y + shape2.Size[0] / 2); 
        var distance = Math.Sqrt(dx * dx + dy * dy); 
        return distance <= (double)shape1.Size[0] / 2 + (double)shape2.Size[0] / 2;
    }
    
    private void ResolveOverlap(DisplayObject s1, DisplayObject s2) 
    { 
        double dx = s2.X - s1.X; 
        double dy = s2.Y - s1.Y; 
        double distance = Math.Sqrt(dx * dx + dy * dy);
        
        double radius1 = (double)s1.Size[0] / 2; 
        double radius2 = (double)s2.Size[0] / 2; 
        double overlap = (radius1 + radius2) - distance; 
        
        if (overlap > 0 && distance > 0) 
        { 
            double nx = dx / distance; 
            double ny = dy / distance; 
            s1.X -= (overlap * nx) / 2; 
            s1.Y -= (overlap * ny) / 2; 
            s2.X += (overlap * nx) / 2; 
            s2.Y += (overlap * ny) / 2;
        }
    }
    
    private void HandleCollision(DisplayObject shape1, DisplayObject shape2) 
    { 
        if (shape1 is CircleObject && shape2 is CircleObject) 
        { 
            double mass1 = 1; 
            double mass2 = 1;
            
            double originalSpeed1 = shape1.Speed; 
            double originalSpeed2 = shape2.Speed;
            
            double v1x = originalSpeed1 * Math.Cos(shape1.AngleSpeed); 
            double v1y = originalSpeed1 * Math.Sin(shape1.AngleSpeed); 
            double v2x = originalSpeed2 * Math.Cos(shape2.AngleSpeed); 
            double v2y = originalSpeed2 * Math.Sin(shape2.AngleSpeed);
            
            double nx = shape2.X - shape1.X; 
            double ny = shape2.Y - shape1.Y; 
            double distance = Math.Sqrt(nx * nx + ny * ny);
            
            if (distance == 0) return;
            
            nx /= distance; 
            ny /= distance;
            
            double relativeVelocityX = v2x - v1x; 
            double relativeVelocityY = v2y - v1y; 
            double dotProduct = relativeVelocityX * nx + relativeVelocityY * ny;
            
            if (dotProduct > 0) 
            { 
                return;
            }
            
            double p1 = v1x * nx + v1y * ny; 
            double p2 = v2x * nx + v2y * ny; 
            double p1After = p2; 
            double p2After = p1;
            
            v1x += (p1After - p1) * nx; 
            v1y += (p1After - p1) * ny; 
            v2x += (p2After - p2) * nx; 
            v2y += (p2After - p2) * ny;
            
            shape1.AngleSpeed = Math.Atan2(v1y, v1x); 
            shape2.AngleSpeed = Math.Atan2(v2y, v2x);
            
            shape1.Speed = originalSpeed1; 
            shape2.Speed = originalSpeed2;
        }
    }
    
    public void RemoveShape(DisplayObject shape)
    {
        if (Shapes.Contains(shape))
        {
            Shapes.Remove(shape);
            if (_canvas.Children.Contains(shape.Shape))
            {
                _canvas.Children.Remove(shape.Shape);
            }
        }
    }
}