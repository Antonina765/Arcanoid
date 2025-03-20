using System.Collections.Generic;
using Arcanoid.Models;

namespace Arcanoid.Stage;

public class StageDataManager
{
    private readonly Stage _stage;

    public StageDataManager(Stage stage) 
    { 
        _stage = stage;
    }
    
    public List<ShapeData> GetShapesData() 
    { 
        var shapesData = new List<ShapeData>(); 
        foreach (var shape in _stage.Shapes) 
        { 
            var data = new ShapeData 
            { 
                ShapeType = shape.GetType().Name, 
                X = (int)shape.X, 
                Y = (int)shape.Y, 
                Speed = shape.Speed, 
                AngleSpeed = shape.AngleSpeed, 
                Acceleration = shape.Acceleration, 
                R1 = shape.r1, 
                G1 = shape.g1, 
                B1 = shape.b1, 
                R2 = shape.r2, 
                G2 = shape.g2, 
                B2 = shape.b2, 
                Size = shape.Size
            }; 
            shapesData.Add(data);
        } 
        return shapesData;
    }
    
    public void LoadShapesData(List<ShapeData> shapesData) 
    { 
        // Предварительно очищаем канву и список фигур
        _stage.GameCanvas.Children.Clear(); 
        _stage.Shapes.Clear();
        
        foreach (var data in shapesData) 
        { 
            DisplayObject shape = null; 
            byte r1 = data.R1, g1 = data.G1, b1 = data.B1; 
            byte r2 = data.R2, g2 = data.G2, b2 = data.B2;
            
            switch (data.ShapeType) 
            { 
                case "CircleObject": 
                    shape = new CircleObject(_stage.GameCanvas, 800, 800, data.Size, data.Size, r1, g1, b1, r2, g2, b2); 
                    break;
                
                case "RectangleObject": 
                    shape = new RectangleObject(_stage.GameCanvas, 800, 800, data.Size, r1, g1, b1, r2, g2, b2); 
                    break;
                
                case "TriangleShape": 
                    shape = new TriangleShape(_stage.GameCanvas, 900, 900, data.Size, r1, g1, b1, r2, g2, b2); 
                    break;
                
                case "TrapezoidObject": 
                    shape = new TrapezoidObject(_stage.GameCanvas, 900, 900, data.Size, r1, g1, b1, r2, g2, b2); 
                    break;
            }

            if (shape != null) 
            { 
                // Устанавливаем сохранённые значения
                shape.X = data.X; 
                shape.Y = data.Y; 
                shape.Speed = data.Speed; 
                shape.AngleSpeed = data.AngleSpeed; 
                shape.Acceleration = data.Acceleration; 
                shape.Draw(); 
                _stage.Shapes.Add(shape);
            }
        }
    }
}
