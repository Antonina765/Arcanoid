using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Loader;
using System.Text.Json;
using Arcanoid.Models;
using Arcanoid.Stage;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace Arcanoid.Game;

public class Game
{
    private readonly Stage.Stage _stage; 
    private readonly Window _mainWindow; 
    private readonly GameMenu _menu; 
    private Grid _mainGrid; 
    private Canvas _menuCanvas;
    
    private SpecialBall _specialBall;
    private Platform _platform;
    private GameStatistics _gameStats;
    private List<BonusItem> _activeBonuses = new List<BonusItem>();
    private List<ScoreMessage> _activeScoreMessages = new List<ScoreMessage>();

    private bool _isFullScreen = true; 
    private bool _isMenuOpen; 
    private int _shapeCount = 20;
    
    private readonly GameFileManager _fileManager; 
    private readonly GameInputHelder _inputHelder; 
    private readonly GameMenuActions _menuActions;
    private readonly DispatcherTimer _gameTimer;
    
    public Game(Window window) 
    { 
        _mainWindow = window;
        
        _mainWindow.WindowState = WindowState.FullScreen; 
        _mainWindow.Width = 1430; 
        _mainWindow.Height = 800;
        
        _mainWindow.SizeChanged += (sender, e) => AdjustMenuSize();
        
        var border = new Border 
        { 
            BorderBrush = Brushes.White, 
            BorderThickness = new Thickness(10), 
            Padding = new Thickness(10),
        };
        
        _stage = new Stage.Stage(); 
        _platform = new Platform(_stage.GameCanvas, 150, 20);
        _specialBall = new SpecialBall(_stage.GameCanvas, (int)_mainWindow.Width, (int)_mainWindow.Height, new List<int>{20}, 255, 0, 0, 255, 255,0);
        _gameStats = new GameStatistics();

        _gameTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(16)
        };
        _gameTimer.Tick += (sender, args) => GameUpdate();
        _gameTimer.Start();
        
        _menuCanvas = new Canvas 
        { 
            Background = Brushes.Transparent, 
            IsHitTestVisible = false, 
            HorizontalAlignment = HorizontalAlignment.Center, 
            VerticalAlignment = VerticalAlignment.Center
        };
        
        _fileManager = new GameFileManager(_mainWindow, _stage); 
        _inputHelder = new GameInputHelder(_stage, _platform, _specialBall, ToggleFullScreen, () => _isMenuOpen); 
        _menuActions = new GameMenuActions(_stage, _fileManager, _mainWindow, _shapeCount, ToggleMenu);
        
        _menu = new GameMenu(
            _menuCanvas, 
            _menuActions.StartGame, 
            _menuActions.SaveGame, 
            _menuActions.LoadGame, 
            _menuActions.Settings, 
            _menuActions.Pause, 
            _menuActions.Exit
            );

        _mainGrid = new Grid(); 
        _mainGrid.Children.Add(_stage.GameCanvas); 
        _mainGrid.Children.Add(_menuCanvas);
        
        // Создаем отдельный канвас для UI (например, для бонусов и статистики)
        Canvas uiCanvas = new Canvas
        {
            Width = _mainWindow.Width,
            Height = _mainWindow.Height,
            IsHitTestVisible = false // если не нужно обрабатывать ввод
        };

        // Добавляем статистику: в правом верхнем углу с использованием Canvas.SetRight и Canvas.SetTop
        uiCanvas.Children.Add(_gameStats.StatsDisplay);
        Canvas.SetRight(_gameStats.StatsDisplay, 5);
        Canvas.SetTop(_gameStats.StatsDisplay, 5);

        // Затем добавляем uiCanvas поверх основного канваса
        _mainGrid.Children.Add(uiCanvas);
        
        border.Child = _mainGrid; 
        _mainWindow.Content = border; 
        _mainWindow.KeyDown += OnKeyDown;
        InitializeSpecialObjects();
    }
    
    public void Start() 
    { 
        _stage.ShapeManager.AddRandomShapes(20, (int)_mainWindow.Width, (int)_mainWindow.Height);

        ResetSpecialBallOnPlatform();
    }
    
    public void InitializeSpecialObjects()
    {
        // Используем размеры канвы, которые уже установлены
        double canvasWidth = _stage.GameCanvas.Bounds.Width;
        double canvasHeight = _stage.GameCanvas.Bounds.Height;
    
        // Создаем платформу и задаем её положение
        //_platform = new Platform(_stage.GameCanvas, 150, 20);
        // Обновляем позицию платформы, опираясь на реальные размеры
        _platform.X = (canvasWidth - _platform.Shape.Bounds.Width) / 2;
        _platform.Y = canvasHeight - _platform.Shape.Bounds.Height - 10;
        _platform.Draw();
    
        // Создаем специальный шарик – убедись, что он добавлен в ту же канву
        //_specialBall = new SpecialBall(_stage.GameCanvas, (int)canvasWidth, (int)canvasHeight, new List<int> { 20 }, 255, 0, 0, 255, 255, 0);
        ResetSpecialBallOnPlatform(); // метод, который установит шарик на платформе
    }

    // Метод обновления игрового цикла, который вызывается на каждом тике таймера
    public void GameUpdate() 
    { 
        // Обновление движения всех обычных шариков
        foreach (var ball in _stage.ShapeManager.Shapes) 
        { 
            ball.Move();
        }

        // Обновление специального шарика
        if (_specialBall.IsLaunched) 
        { 
            _specialBall.Move(); 
            if (_specialBall.Y >= _mainWindow.Bounds.Height) 
            { 
                _gameStats.Attempts--; 
                _gameStats.UpdateDisplay(); 
                ResetSpecialBallOnPlatform();
            }
        }
        
        // Проверка столкновений специального шарика с обычными шарами
        for (int i = _stage.ShapeManager.Shapes.Count - 1; i >= 0; i--) 
        { 
            var normalBall = _stage.ShapeManager.Shapes[i]; 
            if (DetectCircleCollision(_specialBall, normalBall)) 
            { 
                _specialBall.HandleCollisionWith(normalBall); 
                _stage.ShapeManager.RemoveShape(normalBall); 
                CreateScoreMessage(normalBall.X, normalBall.Y, "+10");
                
                if (new Random().NextDouble() < 0.3) 
                { 
                    CreateBonus(normalBall.X, normalBall.Y);
                }
            }
        }

        // Обновляем бонусы
        foreach (var bonus in _activeBonuses.ToArray()) 
        { 
            bonus.UpdatePosition(); 
            bonus.Draw(_menuCanvas); 
            if (DetectBonusPlatformCollision(bonus, _platform)) 
            { 
                ApplyBonusEffect(bonus.Type); 
                _activeBonuses.Remove(bonus); 
                _menuCanvas.Children.Remove(bonus.Icon);
            }
            else if (bonus.Y > _mainWindow.Bounds.Height) 
            { 
                _activeBonuses.Remove(bonus); 
                _menuCanvas.Children.Remove(bonus.Icon);
            }
        }

        // Обновляем сообщения о набранных очках
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

        // Проверка условия перехода на следующий уровень
        if (_stage.ShapeManager.Shapes.Count == 0) 
        { 
            NextLevel();
        }
        
        _stage.ShapeManager.RedrawCanvas();
    }
    
    // Вспомогательные методы для обнаружения столкновений:
    private bool DetectCircleCollision(DisplayObject a, DisplayObject b)
    {
        double dx = (a.X + a.Size[0] / 2) - (b.X + b.Size[0] / 2);
        double dy = (a.Y + a.Size[0] / 2) - (b.Y + b.Size[0] / 2);
        double distance = Math.Sqrt(dx * dx + dy * dy);
        return distance < ((double)a.Size[0] / 2 + (double)b.Size[0] / 2);
    }

    private bool DetectBonusPlatformCollision(BonusItem bonus, Platform platform)
    { 
        double platLeft = platform.X; 
        double platRight = platform.X + platform.Shape.Bounds.Width; 
        double platTop = platform.Y; 
        double platBottom = platform.Y + platform.Shape.Bounds.Height;
        
        double bonusLeft = bonus.X; 
        double bonusRight = bonus.X + bonus.Icon.Bounds.Width; 
        double bonusTop = bonus.Y; 
        double bonusBottom = bonus.Y + bonus.Icon.Bounds.Height;
        
        return bonusRight > platLeft && bonusLeft < platRight &&
               bonusBottom > platTop && bonusTop < platBottom;
    }

    // Здесь же реализуй методы:
    // CreateScoreMessage(double x, double y, string message), CreateBonus(double x, double y),
    // ApplyBonusEffect(BonusType type), NextLevel(), ResetSpecialBallOnPlatform() и т.д.
    // Эти методы отвечают за создание бонусов, сообщений и переход на следующий уровень.

    private void ResetSpecialBallOnPlatform()
    {
        // Устанавливаем специальный шарик на платформу в центре, сбрасываем флаг запуска
        _specialBall.IsLaunched = false;
        _specialBall.X = _platform.X + (_platform.Shape.Bounds.Width - _specialBall.Shape.Bounds.Width) / 2;
        _specialBall.Y = _platform.Y - _specialBall.Shape.Bounds.Height;
        _specialBall.Draw();
    }

    // Пример для создания сообщения с очками:
    private void CreateScoreMessage(double x, double y, string text) 
    { 
        var scoreMsg = new ScoreMessage(text, x, y); 
        _activeScoreMessages.Add(scoreMsg); 
        scoreMsg.Draw(_menuCanvas);
    }

    // Пример создания бонуса
    private void CreateBonus(double x, double y)
    {
        // Случайный выбор бонуса, здесь можно улучшить логику
        var randomBonus = (BonusType)new Random().Next(0, 10);
        var bonus = new BonusItem(randomBonus, x, y); 
        _activeBonuses.Add(bonus); 
        bonus.Draw(_menuCanvas);
    }

    // Пример применения эффекта бонуса
    private void ApplyBonusEffect(BonusType type)
    {
        switch (type)
        {
            case BonusType.IncreaseSpecialBallSpeed: 
                _specialBall.Speed *= 1.2; 
                break;
            case BonusType.DecreaseSpecialBallSpeed: 
                _specialBall.Speed *= 0.8; 
                break;
            case BonusType.IncreasePlatformWidth: 
                // Увеличить ширину платформы и обновить отрисовку
                _platform.Shape.Width += 20; 
                break;
            case BonusType.DecreasePlatformWidth: 
                _platform.Shape.Width = Math.Max(50, _platform.Shape.Width - 20); 
                break;
            case BonusType.ExtraPoints: 
                _gameStats.AddScore(50); 
                break;
            case BonusType.ExtraAttempt: 
                _gameStats.Attempts++; 
                _gameStats.UpdateDisplay(); 
                break;
            // Другие бонусы…
            default: 
                break;
        } 
        _gameStats.UpdateDisplay();
    }

    // Пример перехода на следующий уровень
    private void NextLevel()
    {
        _gameStats.Level++;
        // Изменяем сложность: увеличиваем число шариков, повышаем скорость и т.д.
        _stage.ShapeManager.AddRandomShapes(20 + (_gameStats.Level * 5), (int)_mainWindow.Width, (int)_mainWindow.Height);
        _gameStats.UpdateDisplay();
    }
    
    private void OnKeyDown(object sender, KeyEventArgs e) 
    { 
        _inputHelder.HandleKeyDown(e, ToggleMenu);
    }
    
    private void ToggleFullScreen() 
    { 
        if (_isFullScreen) 
        { 
            _mainWindow.WindowState = WindowState.Normal;
        }
        else 
        { 
            _mainWindow.WindowState = WindowState.FullScreen;
        } 
        
        _isFullScreen = !_isFullScreen; 
        AdjustMenuSize(); //полдстраиваем меню после смены режима
    }
    
    private void ToggleMenu() 
    { 
        if (_isMenuOpen) 
        { 
            _menuCanvas.IsHitTestVisible = false; 
            _menuCanvas.Children.Clear();
        }
        else 
        { 
            _menu.DrawMenu(); 
            AdjustMenuSize(); 
            _menuCanvas.IsHitTestVisible = true;
        } 
        _isMenuOpen = !_isMenuOpen;
    }
    
    /// <summary>
    /// Метод для адаптации размеров меню к текущим размерам окна.
    /// Меню устанавливается равным 80% от размеров окна, но не более 600х400.
    /// </summary>
    private void AdjustMenuSize() 
    { 
        // Получаем фактические размеры окна через Bounds
        double windowWidth = _mainWindow.Bounds.Width; 
        double windowHeight = _mainWindow.Bounds.Height;
        
        // Расчитываем новый размер меню (80% от окна, максимум 600x400)
        double newWidth = Math.Min(windowWidth * 0.8, 600); 
        double newHeight = Math.Min(windowHeight * 0.8, 400);
        
        _menuCanvas.Width = newWidth; 
        _menuCanvas.Height = newHeight;

        // Если в меню есть вложенные элементы, обновляем их размеры (учитывая, например, отступы)
        foreach (var child in _menuCanvas.Children) 
        { 
            if (child is Control control) 
            { 
                control.Width = newWidth - 20; 
                control.Height = newHeight - 20;
            }
        }
    }
}