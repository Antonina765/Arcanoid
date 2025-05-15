using System;
using Avalonia.Controls;

namespace Arcanoid.Game;
public class GameMenuActions
{
    private readonly Stage.Stage _stage;
    private readonly GameFileManager _fileManager;
    private readonly Window _mainWindow;
    private int _shapeCount;
    private readonly Action _toggleMenu;
    private readonly Action _initializeSpecialObjects;
    private readonly Game _game;
        
    public GameMenuActions(Stage.Stage stage, GameFileManager fileManager, Window mainWindow, int initialShapeCount, Action toggleMenu, Action initializeSpecialObjects)
    {
        _stage = stage;
        _fileManager = fileManager;
        _mainWindow = mainWindow;
        _shapeCount = initialShapeCount;
        _toggleMenu = toggleMenu;
        _initializeSpecialObjects = initializeSpecialObjects;
    }
        
    public void StartGame()
    {
        // Очищаем канвас и создаём новые фигуры
        _stage.ShapeManager.ClearShapes();
        _stage.ShapeManager.AddRandomShapes(_shapeCount, (int)_mainWindow.Width, (int)_mainWindow.Height);
        _initializeSpecialObjects();
        _toggleMenu();
    }
        
    public void Settings()
    {
        var settingsWindow = new SettingsWindow(_shapeCount, OnShapeCountChanged);
        settingsWindow.ShowDialog(_mainWindow);
    }
        
    public void Pause()
    {
        Console.WriteLine("Игра на паузе или выход");
        _toggleMenu();
    }
        
    public void Exit()
    {
        _mainWindow.Close();
    }
        
    public void SaveGame()
    {
        _fileManager.SaveGame();
    }
        
    public void LoadGame()
    {
        _fileManager.LoadGame();
    }
        
    private void OnShapeCountChanged(int newCount)
    {
        _shapeCount = newCount;
        Console.WriteLine($"Количество фигур изменено на: {_shapeCount}");
    }
}