using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using Arcanoid.Models;

namespace Arcanoid.Stage;

public class StageMovementManager
{
    private readonly Stage _stage;
    private readonly DispatcherTimer _timer;
    public List<DisplayObject> Shapes { get; private set; } = new List<DisplayObject>();
    
    public StageMovementManager(Stage stage)
    {
        _stage = stage;
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _timer.Tick += OnTimerTick;
    }

    private void OnTimerTick(object sender, EventArgs e)
    {
        for (var i = 0; i < Shapes.Count; i++)
        {
            var shape = Shapes[i];
            CheckCollision(shape,i);
            shape.Move();
        }
        
    }

     private void CheckCollision(DisplayObject shape,int idx)
        {
            for (int i = 0; i < Shapes.Count; i++)
            {
                if (i == idx) continue;
                if (IsColliding(Shapes[i], shape))
                {
                    //StopMovement();
                    HandleCollision(Shapes[i], shape);
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

        
        private void HandleCollision(DisplayObject shape1, DisplayObject shape2)
        {
            if (shape1 is CircleObject && shape2 is CircleObject)
            {
                double mass1 = 1;//shape1.Size[0];
                double mass2 = 1;//shape2.Size[0];

                // Векторы скорости
                double v1x = shape1.Speed * Math.Cos(shape1.AngleSpeed);
                double v1y = shape1.Speed * Math.Sin(shape1.AngleSpeed);

                double v2x = shape2.Speed * Math.Cos(shape2.AngleSpeed);
                double v2y = shape2.Speed * Math.Sin(shape2.AngleSpeed);

                // Вектор нормали между центрами кругов
                double nx = shape2.X - shape1.X;
                double ny = shape2.Y - shape1.Y;
                double distance = Math.Sqrt(nx * nx + ny * ny);

                if (distance == 0) return;  // Предотвращаем деление на ноль

                nx /= distance;
                ny /= distance;

                // Проверяем, движутся ли круги навстречу друг другу (внутренняя часть скалярного произведения)
                double relativeVelocityX = v2x - v1x;
                double relativeVelocityY = v2y - v1y;
                double dotProduct = relativeVelocityX * nx + relativeVelocityY * ny;

                if (dotProduct > 0)
                {
                    return;  // Круги уже движутся в разные стороны, не обрабатываем столкновение
                }

                // Проекции скоростей на нормаль
                double p1 = v1x * nx + v1y * ny;
                double p2 = v2x * nx + v2y * ny;

                // Новые скорости по закону сохранения импульса
                double p1After = (p1 * (mass1 - mass2) + 2 * mass2 * p2) / (mass1 + mass2);
                double p2After = (p2 * (mass2 - mass1) + 2 * mass1 * p1) / (mass1 + mass2);

                // Обновляем компоненты скоростей с учетом коллизии
                v1x += (p1After - p1) * nx;
                v1y += (p1After - p1) * ny;

                v2x += (p2After - p2) * nx;
                v2y += (p2After - p2) * ny;

                // Обновляем скорости и углы
                shape1.Speed = Math.Sqrt(v1x * v1x + v1y * v1y);
                shape1.AngleSpeed = Math.Atan2(v1y, v1x);

                shape2.Speed = Math.Sqrt(v2x * v2x + v2y * v2y);
                shape2.AngleSpeed = Math.Atan2(v2y, v2x);
                
            }
        }
        
    public void StartMovement(byte acceleration)
    {
        foreach (var shape in _stage.Shapes)
        {
            shape.StartMovement(acceleration);
        }
        _timer.Start();
    }

    public void StopMovement()
    {
        _timer.Stop();
    }
    
}

