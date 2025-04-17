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
        
    private void OnTimerTick(object sender, EventArgs e)
    {
        // На каждом тике проверка столкновений и перемещение фигур
        for (int i = 0; i < _shapeManager.Shapes.Count; i++)
        {
            var shape = _shapeManager.Shapes[i];
            _shapeManager.CheckCollision(shape, i);
            shape.Move();
        }
    }
}