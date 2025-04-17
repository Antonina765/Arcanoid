using System;
using System.Collections.Generic;
using Arcanoid.Models;
using Avalonia.Controls;

namespace Arcanoid.Stage;

public class StageDataManager
    {
        private readonly Canvas _canvas;
        private readonly StageShapeManager _shapeManager;
        
        public StageDataManager(Canvas canvas, StageShapeManager shapeManager)
        {
            _canvas = canvas;
            _shapeManager = shapeManager;
        }
        
        public List<ShapeData> GetShapesData()
        {
            var shapesData = new List<ShapeData>();
            foreach (var shape in _shapeManager.Shapes)
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
            _canvas.Children.Clear();
            _shapeManager.Shapes.Clear();
            
            foreach (var data in shapesData)
            {
                DisplayObject shape = null;
                byte r1 = data.R1, g1 = data.G1, b1 = data.B1;
                byte r2 = data.R2, g2 = data.G2, b2 = data.B2;
                
                switch (data.ShapeType)
                {
                    case "CircleObject":
                        shape = new CircleObject(_canvas, 800, 800, data.Size, data.Size, r1, g1, b1, r2, g2, b2)
                        {
                            X = data.X,
                            Y = data.Y,
                            Speed = data.Speed,
                            AngleSpeed = data.AngleSpeed,
                            Acceleration = data.Acceleration
                        };
                        break;
                    case "RectangleObject":
                        shape = new RectangleObject(_canvas, 800, 800, data.Size, r1, g1, b1, r2, g2, b2)
                        {
                            X = data.X,
                            Y = data.Y,
                            Speed = data.Speed,
                            AngleSpeed = data.AngleSpeed,
                            Acceleration = data.Acceleration
                        };
                        break;
                    case "TriangleShape":
                        shape = new TriangleShape(_canvas, 900, 900, data.Size, r1, g1, b1, r2, g2, b2)
                        {
                            X = data.X,
                            Y = data.Y,
                            Speed = data.Speed,
                            AngleSpeed = data.AngleSpeed,
                            Acceleration = data.Acceleration
                        };
                        break;
                    case "TrapezoidObject":
                        shape = new TrapezoidObject(_canvas, 900, 900, data.Size, r1, g1, b1, r2, g2, b2)
                        {
                            X = data.X,
                            Y = data.Y,
                            Speed = data.Speed,
                            AngleSpeed = data.AngleSpeed,
                            Acceleration = data.Acceleration
                        };
                        break;
                    default:
                        Console.WriteLine("Unknown shape type: " + data.ShapeType);
                        break;
                }
                
                if (shape != null)
                {
                    shape.Draw();
                    _shapeManager.Shapes.Add(shape);
                }
            }
        }
    }