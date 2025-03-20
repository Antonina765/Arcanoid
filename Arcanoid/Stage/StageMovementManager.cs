using System;
using System.Linq;
using Avalonia.Threading;
using Arcanoid.Models;

namespace Arcanoid.Stage;

public class StageMovementManager
{
    private readonly Stage _stage;
    private readonly DispatcherTimer _timer;
   
    
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
        foreach (var shape in _stage.Shapes)
        {
            shape.Move();
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

