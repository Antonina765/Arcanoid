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
    public List<DisplayObject> Shapes { get; private set; } = new List<DisplayObject>();
    
    private readonly DispatcherTimer _timer;
    
    public Stage()
    {
        GameCanvas = new Canvas
        {
            Background = Brushes.Black,
            Focusable = true,  // Чтобы канвас принимал клавиатурные события
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        // Устанавливаем фокус для получения событий клавиатуры
        GameCanvas.Focus();
        
        // Подписываемся на событие нажатия клавиш
        GameCanvas.KeyDown += OnKeyDown;
        
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _timer.Tick += OnTimerTick;
        
        //Shapes = new List<DisplayObject>();
    }
    
    private void OnKeyDown(object sender, Avalonia.Input.KeyEventArgs e)
    {
        switch(e.Key)
        {
            case Avalonia.Input.Key.Z:
                // При нажатии Z увеличиваем ускорение на каждой фигуре
                foreach (var shape in Shapes)
                {
                    // Повышаем ускорение. Подберите нужное значение инкремента (здесь, например, 0.5)
                    shape.Acceleration += 0.5;
                }
                break;

            case Avalonia.Input.Key.X:
                // При нажатии X сбрасываем ускорение, оставляя скорость неизменной
                foreach (var shape in Shapes)
                {
                    shape.Acceleration = 0;
                }
                break;
        }
    }
    
    private void OnTimerTick(object sender, EventArgs e)
    {
        foreach (var shape in Shapes)
        {
            shape.Move();
        }
    }
    
    /*public void ApplyBlurEffect()
    { }
    public void RemoveBlurEffect()
    { }*/

}
