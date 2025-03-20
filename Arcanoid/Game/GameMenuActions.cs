using System;
using Avalonia.Controls;

namespace Arcanoid.Game;

public class GameMenuActions
{
    private readonly Game _game;

    public GameMenuActions(Game game)
    {
        _game = game;
    }

    public void StartGame()
    {
        _game.ShapeManager.ClearCanvas();
        _game.Start();
        _game.ToggleMenu();
    }

    public void Settings()
    {
        var settingsWindow = new SettingsWindow(_game.ShapeCount, OnShapeCountChanged);
        settingsWindow.ShowDialog(_game.MainWindow);
    }

    private void OnShapeCountChanged(int newCount)
    {
        _game.ShapeCount = newCount;
        Console.WriteLine("Количество фигур изменено на: " + _game.ShapeCount);
    }

    public void Pause()
    {
        Console.WriteLine("Игра на паузе или выход");
        _game.ToggleMenu();
    }

    public void Exit()
    {
        _game.MainWindow.Close();
    }

}
