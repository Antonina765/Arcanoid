using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using Arcanoid.Models;
using Avalonia.Controls;

namespace Arcanoid.Stage;

public class StageMovementManager
{
    private readonly StageShapeManager _shapeManager;
    private readonly DispatcherTimer _timer;
    private readonly Window _mainWindow; 
    private Canvas _menuCanvas;
    private Game.Game _game;
    private SpecialBall _specialBall;
    private Platform _platform;
    private GameStatistics _gameStats;
    private List<BonusItem> _activeBonuses = new List<BonusItem>();
    private List<ScoreMessage> _activeScoreMessages = new List<ScoreMessage>();
        
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
        
        // Останавливаем все объекты, устанавливая их скорость и ускорение в ноль
        foreach (var shape in _shapeManager.Shapes)
        {
            shape.Speed = 0;
            shape.Acceleration = 0;
        }
        
        // Останавливаем специальный шарик, если он существует и инициализирован
        if (_specialBall != null)
        {
            _specialBall.Speed = 0;
        }
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
        if (!_game.GameStarted)
        {
            _shapeManager.RedrawCanvas();
            return;
        }
        
        for (int i = 0; i < _shapeManager.Shapes.Count; i++)
        {
            var shape = _shapeManager.Shapes[i];
            _shapeManager.CheckCollision(shape, i);
            shape.Move();
        }
            
        // 2. Обновляем особый шарик, если он запущен.
        if (_specialBall.IsLaunched)
        {
            _specialBall.Move();

            // Проверка столкновения особого шарика с платформой:
            if (_game.DetectPlatformCollision(_specialBall, _platform))
            {
                Console.WriteLine("Отскок от платформы!");
                // Принудительно устанавливаем позицию особого шарика непосредственно над платформой
                _specialBall.Y = _platform.Y - _specialBall.Shape.Height;
                // Отражаем вертикальную компоненту, чтобы шарик полетел вверх
                _specialBall.AngleSpeed = -Math.Abs(_specialBall.AngleSpeed);
                // Дополнительно можно добавить небольшой импульс, изменив скорость, если нужно
            }
            
            // Если особый шарик касается нижней границы
            if (_specialBall.Y + _specialBall.Shape.Height >= _mainWindow.Bounds.Height)
            {
                _gameStats.Attempts--;
                _gameStats.UpdateDisplay();
                // Если попытки закончились, вызываем GameOver(), иначе перезапускаем уровень.
                if (_gameStats.Attempts <= 0)
                {
                    _game.GameOver();
                }
                else
                {
                    _game.ResetGame();
                }
                return;
            }
        }
        
        // 3. Проверяем столкновения особого шарика с обычными шарами.
        if (_specialBall.IsLaunched)
        {
            for (int i = _shapeManager.Shapes.Count - 1; i >= 0; i--)
            {
                var normalBall = _shapeManager.Shapes[i];
                if (_game.DetectCircleCollision(_specialBall, normalBall))
                {
                    _specialBall.HandleCollisionWith(normalBall);
                    _shapeManager.RemoveShape(normalBall);
                    
                    // Всегда создаём сообщение и прибавляем 10 очков.
                    _game.CreateScoreMessage(normalBall.X, normalBall.Y, "+10");
                    _gameStats.AddScore(10);
    
                    // Бонус генерируется случайно (30% шанс).
                    if (new Random().NextDouble() < 0.3)
                    {
                        _game.CreateBonus(normalBall.X, normalBall.Y);
                    }
                }
            }
        }

        // 4. Обновляем бонусы.
        foreach (var bonus in _activeBonuses.ToArray())
        {
            bonus.UpdatePosition();
            bonus.Draw(_menuCanvas);
            if (_game.DetectBonusPlatformCollision(bonus, _platform))
            {
                _game.ApplyBonusEffect(bonus.Type);
                _activeBonuses.Remove(bonus);
                _menuCanvas.Children.Remove(bonus.Icon);
            }
            else if (bonus.Y > _mainWindow.Bounds.Height)
            {
                _activeBonuses.Remove(bonus);
                _menuCanvas.Children.Remove(bonus.Icon);
            }
        }

        // 5. Обновляем сообщения о набранных очках.
        foreach (var msg in _activeScoreMessages.ToArray())
        {
            msg.Update();
            msg.Draw(_menuCanvas);
            if (msg.Y > _mainWindow.Bounds.Height)
            {
                _activeScoreMessages.Remove(msg);
                _menuCanvas.Children.Remove(msg.TextBlock);
            }
        }

        // 6. Если обычные шары закончились — переходим на следующий уровень.
        if (_shapeManager.Shapes.Count == 0)
        {
            _game.NextLevel();
        }
        
        _shapeManager.RedrawCanvas();
    }
}