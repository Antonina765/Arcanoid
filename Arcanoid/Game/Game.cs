using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Arcanoid.Models;
using Arcanoid.Stage;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Arcanoid.Game;

public class Game
{
    // Stage-объекты: контейнер и менеджеры
    public Stage.Stage Stage { get; private set; } 
    public StageMovementManager MovementManager { get; private set; }
    public StageShapeManager ShapeManager { get; private set; } 
    public StageDataManager DataManager { get; private set; }
    
    // UI
    public Window MainWindow { get; private set; } 
    private GameMenu _menu; 
    private Grid _mainGrid; 
    private Canvas _menuCanvas;

    // Вспомогательные классы
    private GameInputHendler _inputHandler; 
    private GameFileManagment _fileManager; 
    private GameMenuActions _menuActions;

    // Состояния игры
    public bool IsFullScreen { get; private set; } = true; 
    public bool IsRunWithAcceleration { get; set; } = false; 
    public bool IsRunWithoutAcceleration { get; set; } = false;
    public bool IsMenuOpen { get; set; } = false; 
    public int ShapeCount { get; set; } = 20; 
    public int MaxX { get; set; } 
    public int MaxY { get; set; }

    // Флаг спавна фигур (чтобы добавление происходило только один раз)
    private bool _shapesSpawned = false;

    public Game(Window window) 
    { 
        MainWindow = window; 
        MainWindow.WindowState = WindowState.FullScreen; 
        //MainWindow.Width = 2318; 
        //MainWindow.Height = 1449;
        
        Stage = new Stage.Stage();
        MovementManager = new StageMovementManager(Stage); 
        ShapeManager = new StageShapeManager(Stage); 
        DataManager = new StageDataManager(Stage);

        // Настройки UI для меню
        _menuCanvas = new Canvas
        { 
            Background = Brushes.Transparent, 
            IsHitTestVisible = false
        };

        // Инициализация помощников Game
        _fileManager = new GameFileManagment(MainWindow, DataManager); 
        _menuActions = new GameMenuActions(this); 
        _inputHandler = new GameInputHendler(this);

        // Создаем меню и передаем делегаты
        _menu = new GameMenu(
            _menuCanvas, 
            _menuActions.StartGame,    
            _fileManager.SaveGame,    
            _fileManager.LoadGame,
            _menuActions.Settings, 
            _menuActions.Pause, 
            _menuActions.Exit
            );

        _mainGrid = new Grid(); 
        _mainGrid.Children.Add(Stage.GameCanvas); 
        _mainGrid.Children.Add(_menuCanvas);
        
        var border = new Border 
        { 
            BorderBrush = Brushes.White, 
            BorderThickness = new Thickness(10), 
            Padding = new Thickness(10), 
            Child = _mainGrid
        };
        
       MainWindow.Content = border; 
       MainWindow.KeyDown += _inputHandler.HandleKeyDown;
       
       MainWindow.Opened += MainWindow_Opened;
    }

    private void MainWindow_Opened(object sender, EventArgs e)
    {
        // Используем размеры канвы, а не MainWindow
        MaxX = (int)Stage.GameCanvas.Bounds.Width;
        MaxY = (int)Stage.GameCanvas.Bounds.Height;
        Console.WriteLine($"Canvas size (MaxX x MaxY): {MaxX} x {MaxY}");
    
        // Если нужно, можно вызвать спавн фигур здесь или оставить вызов в Start()
        // ShapeManager.AddRandomShapes(ShapeCount, MaxX, MaxY);
        // _shapesSpawned = true;
    
        MainWindow.Opened -= MainWindow_Opened;
    }
    
    // Метод Start делегирует добавление фигур через менеджер фигур
    public void Start()
    {
        // Если размеры ещё не обновились, пробуем их получить
        if (MaxX <= 0 || MaxY <= 0)
        {
            MaxX = (int)Stage.GameCanvas.Bounds.Width;
            MaxY = (int)Stage.GameCanvas.Bounds.Height;
        }
    
        ShapeManager.ClearCanvas();
        ShapeManager.AddRandomShapes(ShapeCount, MaxX, MaxY);
    }
    public void ToggleFullScreen() 
    {
        if (IsFullScreen)
        {
            MainWindow.WindowState = WindowState.Normal;
        }
        else
        {
            MainWindow.WindowState = WindowState.FullScreen;
        }
        IsFullScreen = !IsFullScreen;
    }

    // Переключает видимость меню и вызывает эффекты Stage
    public void ToggleMenu() 
    { 
        if (IsMenuOpen) 
        { 
            _menuCanvas.IsHitTestVisible = false; 
            _menuCanvas.Children.Clear(); 
            //Stage.RemoveBlurEffect();
        }
        else 
        { 
            _menu.DrawMenu(); 
            _menuCanvas.IsHitTestVisible = true; 
           // Stage.ApplyBlurEffect();
        } 
        IsMenuOpen = !IsMenuOpen;
    }
}
