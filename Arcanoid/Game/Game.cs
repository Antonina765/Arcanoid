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
    public bool GameStarted { get; set; } = false;
    
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
            Interval = TimeSpan.FromSeconds(0.03)
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
        _inputHelder = new GameInputHelder(_stage, _platform, _specialBall, ToggleFullScreen, () => _isMenuOpen, this); 
        _menuActions = new GameMenuActions(_stage, _fileManager, _mainWindow, _shapeCount, ToggleMenu, InitializeSpecialObjects);
        
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
    
        // Явно добавляем платформу в канвас, если её там ещё нет
        if (!_stage.GameCanvas.Children.Contains(_platform.Shape))
        {
            _stage.GameCanvas.Children.Add(_platform.Shape);
        }
        // Создаем специальный шарик – убедись, что он добавлен в ту же канву
        //_specialBall = new SpecialBall(_stage.GameCanvas, (int)canvasWidth, (int)canvasHeight, new List<int> { 20 }, 255, 0, 0, 255, 255, 0);
        ResetSpecialBallOnPlatform(); // метод, который установит шарик на платформе
    }

    // Метод обновления игрового цикла, который вызывается на каждом тике таймера
    public void GameUpdate() 
    { 
        if (!GameStarted) 
        {
            _stage.ShapeManager.RedrawCanvas();
            return;
        }
        // Обновление движения всех обычных шариков
        foreach (var ball in _stage.ShapeManager.Shapes) 
        { 
            ball.Move();
        }

        _stage.ShapeManager.ResolveNormalBallCollisions();
        
        // Обновление специального шарика
        if (_specialBall.IsLaunched) 
        { 
            _specialBall.Move(); 
            
            // Проверяем столкновение с платформой
            if (DetectPlatformCollision(_specialBall, _platform))
            {
                Console.WriteLine("Отскок от платформы!");
                // Устанавливаем шарик точно над платформой и отражаем его вверх
                _specialBall.Y = _platform.Y - _specialBall.Shape.Height;
                _specialBall.AngleSpeed = -Math.Abs(_specialBall.AngleSpeed);
            }
            
            // Если особый шарик касается нижней границы,
            // уменьшаем количество попыток и перезапускаем игру.
            if (_specialBall.Y >= _mainWindow.Bounds.Height) 
            { 
                _gameStats.Attempts--; 
                _gameStats.UpdateDisplay(); 
                ResetGame(); 
                // Прекращаем дальнейшее выполнение, чтобы дать время перезапустить игру
                return;
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
                _gameStats.AddScore(10);
                
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
    public bool DetectCircleCollision(DisplayObject a, DisplayObject b)
    {
        double dx = (a.X + a.Size[0] / 2) - (b.X + b.Size[0] / 2);
        double dy = (a.Y + a.Size[0] / 2) - (b.Y + b.Size[0] / 2);
        double distance = Math.Sqrt(dx * dx + dy * dy);
        return distance < ((double)a.Size[0] / 2 + (double)b.Size[0] / 2);
    }

    public void ResetGame()
    {
        // 1. Проверяем, если попытки закончились, завершаем игру.
        if (_gameStats.Attempts <= 0)
        {
            Console.WriteLine("Попытки исчерпаны. Игра завершается.");
            _mainWindow.Close(); // или Application.Current.Exit();
            return;
        }
        
        // 2. Очистка обычных шаров, бонусов и сообщений.
        _stage.ShapeManager.ClearShapes();
        _menuCanvas.Children.Clear();  // Если бонусы и сообщения рисуются на отдельном канве

        // 3. Перезапуск уровня: добавляем новые обычные шары в зависимости от уровня.
        int newShapeCount = 20 + (_gameStats.Level * 5);
        _stage.ShapeManager.AddRandomShapes(newShapeCount, (int)_mainWindow.Width, (int)_mainWindow.Height);

        // 4. Сброс платформы:
        // Вычисляем положение платформы – горизонтально по центру, с небольшим отступом от низа.
        double canvasWidth = _stage.GameCanvas.Bounds.Width;
        double canvasHeight = _stage.GameCanvas.Bounds.Height;
        _platform.X = (canvasWidth - _platform.Shape.Width) / 2;
        _platform.Y = canvasHeight - _platform.Shape.Height - 10;
        _platform.Draw();
    
        // Убедимся, что графический элемент платформы добавлен в GameCanvas
        if (!_stage.GameCanvas.Children.Contains(_platform.Shape))
        {
            _stage.GameCanvas.Children.Add(_platform.Shape);
        }

        // 5. Сброс особого шарика на платформу:
        ResetSpecialBallOnPlatform();
        if (!_stage.GameCanvas.Children.Contains(_specialBall.Shape))
        {
            _stage.GameCanvas.Children.Add(_specialBall.Shape);
        }
        
        // 6. Вывод информационного сообщения в консоль
        Console.WriteLine("Попытка потеряна. Игра перезапущена.");
        GameStarted = false;
    }

    public bool DetectBonusPlatformCollision(BonusItem bonus, Platform platform)
    {
        const double tolerance = 20.0;
        
        double bonusLeft = Canvas.GetLeft(bonus.Icon);
        double bonusTop = Canvas.GetTop(bonus.Icon);
        double bonusBottom = bonusTop + bonus.Icon.Height;
        double bonusCenterX = bonusLeft + bonus.Icon.Width / 2;
        
        double platformLeft = Canvas.GetLeft(platform.Shape);
        double platformTop = Canvas.GetTop(platform.Shape);
        double platformRight = platformLeft + platform.Shape.Width;
    
        // Если бонус "прилипает" к платформе: его нижняя граница в пределах 10 пикселей от верхней границы платформы
        // и горизонтально центр бонуса находится внутри платформы.
        if (bonusBottom >= platformTop && bonusBottom <= platformTop + tolerance)
        {
            if(bonusCenterX >= platformLeft && bonusCenterX <= platformRight)
                return true;
        }
    
        return false;
    }
    
    private void ResetSpecialBallOnPlatform()
    {
        // Устанавливаем специальный шарик на платформу в центре, сбрасываем флаг запуска
        _specialBall.IsLaunched = false;
        _specialBall.X = Canvas.GetLeft(_platform.Shape) + (_platform.Shape.Width - _specialBall.Shape.Width) / 2;
        _specialBall.Y = Canvas.GetTop(_platform.Shape) - _specialBall.Shape.Height;
        _specialBall.Draw();
        
        if (!_stage.GameCanvas.Children.Contains(_specialBall.Shape))
        {
            _stage.GameCanvas.Children.Add(_specialBall.Shape);
        }
    }

    // Пример для создания сообщения с очками:
    public void CreateScoreMessage(double x, double y, string text) 
    { 
        var scoreMsg = new ScoreMessage(text, x, y); 
        _activeScoreMessages.Add(scoreMsg); 
        scoreMsg.Draw(_menuCanvas);
    }

    // Пример создания бонуса
    public void CreateBonus(double x, double y)
    {
        // Случайный выбор бонуса, здесь можно улучшить логику
        var randomBonus = (BonusType)new Random().Next(0, 10);
        var bonus = new BonusItem(randomBonus, x, y); 
        _activeBonuses.Add(bonus); 
        bonus.Draw(_menuCanvas);
    }

    // Пример применения эффекта бонуса
    public void ApplyBonusEffect(BonusType type)
    {
        switch (type)
        {
            case BonusType.IncreaseSpecialBallSpeed:
                Console.WriteLine("Скорость шарика увеличилась");
                _specialBall.Speed *= 1.2; 
                break;
            case BonusType.DecreaseSpecialBallSpeed: 
                Console.WriteLine("Скорость шарика уменьшилась");
                _specialBall.Speed *= 0.8; 
                break;
            case BonusType.IncreasePlatformWidth: 
                Console.WriteLine("Ширина платформы увеличилась");
                // Увеличить ширину платформы и обновить отрисовку
                _platform.Shape.Width += 20; 
                break;
            case BonusType.DecreasePlatformWidth: 
                Console.WriteLine("Ширина платформы уменьшилась");
                _platform.Shape.Width = Math.Max(50, _platform.Shape.Width - 20); 
                break;
            case BonusType.ExtraPoints: 
                Console.WriteLine("Дополнительные очки +50");
                _gameStats.AddScore(50); 
                break;
            case BonusType.ExtraAttempt: 
                Console.WriteLine("Дополнительная попытка");
                _gameStats.Attempts++; 
                _gameStats.UpdateDisplay(); 
                break;
           case BonusType.MinusAttempt:
                Console.WriteLine("Минус попытка");
               _gameStats.Attempts--;
               _gameStats.UpdateDisplay(); 
               break;
           case BonusType.MinusPoints:
                Console.WriteLine("Вычитание очков -50");
                _gameStats.AddScore(-50); 
               break;
           case BonusType.ExtraBall:
                Console.WriteLine("Дополнительный шарик на 10 секунд");
                CreateExtraSpecialBall(10);
               break;
            default: 
                break;
        } 
        _gameStats.UpdateDisplay();
    }
    
    private void CreateExtraSpecialBall(int durationInSeconds)
    {
        // Создаем новый особый шарик с теми же начальными параметрами
        SpecialBall extraBall = new SpecialBall(_stage.GameCanvas, (int)_mainWindow.Width, (int)_mainWindow.Height, new List<int>{20}, 255, 0, 0, 255, 255,0);
        extraBall.Launch();
        _stage.ShapeManager.Shapes.Add(extraBall);

        // Запускаем таймер, чтобы удалить extraBall через durationInSeconds секунд
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(durationInSeconds) };
        timer.Tick += (s, e) =>
        {
            timer.Stop();
            _stage.ShapeManager.RemoveShape(extraBall);
            _stage.GameCanvas.Children.Remove(extraBall.Shape);
            Console.WriteLine("Дополнительный особый шарик удален по истечении времени.");
        };
        timer.Start();
    }
    
    public void NextLevel()
    {
        _gameStats.Level++;
        _gameStats.UpdateDisplay();

        // Перегенерируем обычные шары
        _stage.ShapeManager.ClearShapes();
        int newShapeCount = 20 + (_gameStats.Level * 5);
        _stage.ShapeManager.AddRandomShapes(newShapeCount, (int)_mainWindow.Width, (int)_mainWindow.Height);

        // Сброс платформы
        double canvasWidth = _stage.GameCanvas.Bounds.Width;
        double canvasHeight = _stage.GameCanvas.Bounds.Height;
        _platform.X = (canvasWidth - _platform.Shape.Width) / 2;
        _platform.Y = canvasHeight - _platform.Shape.Height - 10;
        _platform.Draw();
        if (!_stage.GameCanvas.Children.Contains(_platform.Shape))
        {
            _stage.GameCanvas.Children.Add(_platform.Shape);
        }

        // Сброс специального шарика на платформу
        ResetSpecialBallOnPlatform();
    
        // Игра ждёт нажатия клавиши F для запуска нового уровня
        GameStarted = false;
    
        Console.WriteLine("Переход на следующий уровень. Нажмите F для старта.");
    }

    public void GameOver()
    {
        GameStarted = false;
    
        // Очистим канва для отрисовки оверлея (если используется отдельный канвас для UI)
        _menuCanvas.Children.Clear();
    
        // Создаём прозрачный оверлей (Grid) – он будет перекрывать игровой канвас
        var overlayGrid = new Grid
        {
            Width = _mainWindow.Width,
            Height = _mainWindow.Height,
            Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)) // полупрозрачный тёмный фон
        };

        // Текст сообщения "Игра завершена"
        var gameOverText = new TextBlock
        {
            Text = "Игра завершена",
            FontSize = 32,
            Foreground = Brushes.Red,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 50)
        };

        // Кнопка "Начать заново"
        var restartButton = new Button
        {
            Content = "Начать заново",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        // При нажатии на кнопку вызывается метод для перезапуска игры
        restartButton.Click += (sender, e) =>
        {
            RestartGame();
            // Убираем оверлей из канвы
            _menuCanvas.Children.Remove(overlayGrid);
        };

        // Добавляем текст и кнопку в оверлей (Grid)
        overlayGrid.Children.Add(gameOverText);
        overlayGrid.Children.Add(restartButton);

        // Добавляем оверлей на _menuCanvas (предполагается, что _menuCanvas расположен поверх игрового канвы)
        _menuCanvas.Children.Add(overlayGrid);
    }

    public void RestartGame()
    {
        // Сбрасываем статистику игры
        _gameStats.Attempts = 3;
        _gameStats.Score = 0;
        _gameStats.Level = 1;
        _gameStats.UpdateDisplay();

        // Очищаем обычные шары
        _stage.ShapeManager.ClearShapes();
        // Очищаем дополнительные элементы UI (например, для бонусов и сообщений)
        _menuCanvas.Children.Clear();

        // Переставляем платформу в нижнюю часть канвы (отцентровав её)
        double canvasWidth = _stage.GameCanvas.Bounds.Width;
        double canvasHeight = _stage.GameCanvas.Bounds.Height;
        _platform.X = (canvasWidth - _platform.Shape.Width) / 2;
        _platform.Y = canvasHeight - _platform.Shape.Height - 10;
        _platform.Draw();
        if (!_stage.GameCanvas.Children.Contains(_platform.Shape))
        {
            _stage.GameCanvas.Children.Add(_platform.Shape);
        }

        // Сбрасываем особый шарик на платформу – он должен быть неподвижным, 
        // чтобы движение запускалось только после нажатия F
        ResetSpecialBallOnPlatform();
        if (!_stage.GameCanvas.Children.Contains(_specialBall.Shape))
        {
            _stage.GameCanvas.Children.Add(_specialBall.Shape);
        }

        // Перегенерируем обычные шары
        int newShapeCount = 20 + (_gameStats.Level * 5);
        _stage.ShapeManager.AddRandomShapes(newShapeCount, (int)_mainWindow.Width, (int)_mainWindow.Height);

        // Переводим игру в состояние ожидания – до нажатия F объекты не двигаются
        GameStarted = false;

        Console.WriteLine("Игра сброшена. Для начала игры нажмите F.");
    }

    public bool DetectPlatformCollision(SpecialBall ball, Platform platform)
    {
        double ballBottom = ball.Y + ball.Shape.Height;
        double platformTop = platform.Y;
        double platformLeft = platform.X;
        double platformRight = platform.X + platform.Shape.Width;
        double ballCenter = ball.X + ball.Shape.Width / 2;
    
        const double tolerance = 10.0;
        // Если нижняя часть шарика касается верхней части платформы (с некоторым допуском)
        if (ballBottom >= platformTop && ballBottom <= platformTop + tolerance)
        {
            if (ballCenter >= platformLeft && ballCenter <= platformRight)
                return true;
        }
        return false;
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
            GameStarted = false;
            //_stage.MovementManager.StopMovement();
            _specialBall.Stop(); // Явно останавливаем специальный шарик

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