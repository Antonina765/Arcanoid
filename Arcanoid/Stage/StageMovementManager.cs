using System;
using System.Linq;
using Avalonia.Threading;
using Arcanoid.Models;

namespace Arcanoid.Stage;

public class StageMovementManager
{
    private readonly StageShapeManager _shapeManager;
    private readonly DispatcherTimer _timer;
        
    public StageMovementManager(StageShapeManager shapeManager)
    {
        _shapeManager = shapeManager;
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _timer.Tick += OnTimerTick;
    }
        
    public void StartMovement(byte acc)
    {
        // Запуск движения для каждой фигуры с указанным параметром ускорения
        foreach (var shape in _shapeManager.Shapes)
        {
            shape.StartMovement(acc);
        }
        _timer.Start();
    }
        
    public void StopMovement()
    {
        _timer.Stop();
    }
        
    /// <summary>
    /// Метод просчитывает ближайшее время столкновения между всеми парами объектов.
    /// </summary>
    public double? PredictNextCollisionTime()
    {
        double? minTime = null;
        var shapes = _shapeManager.Shapes;
        for (int i = 0; i < shapes.Count; i++)
        {
            for (int j = i + 1; j < shapes.Count; j++)
            {
                double? t = StagePhysicsCalculator.PredictCollisionTime(shapes[i], shapes[j]);
                if (t.HasValue)
                {
                    if (!minTime.HasValue || t.Value < minTime.Value)
                        minTime = t;
                }
            }
        }
        return minTime;
    }
        
    private void OnTimerTick(object sender, EventArgs e)
    {
        for (int i = 0; i < _shapeManager.Shapes.Count; i++)
        {
            var shape = _shapeManager.Shapes[i];
            _shapeManager.CheckCollision(shape, i);
            shape.Move();
        }
            
        _shapeManager.RedrawCanvas();
        
        // Можно логировать прогноз времени следующего столкновения
        var nextCollisionIn = PredictNextCollisionTime();
        if (nextCollisionIn.HasValue)
        {
            Console.WriteLine($"Прогноз времени до следующего столкновения: {nextCollisionIn.Value:F2} сек.");
        }
    }
}