using System;
using Avalonia.Input;

namespace Arcanoid.Game;

public class GameInputHelder
    {
        private readonly Stage.Stage _stage;
        private readonly Action _toggleFullScreen;
        private readonly Func<bool> _isMenuOpen;

        private bool _isRunWithAcceleration;
        private bool _isRunWithoutAcceleration;
        
        public GameInputHelder(Stage.Stage stage, Action toggleFullScreen, Func<bool> isMenuOpen)
        {
            _stage = stage;
            _toggleFullScreen = toggleFullScreen;
            _isMenuOpen = isMenuOpen;
        }
        
        public void HandleKeyDown(KeyEventArgs e, Action toggleMenu)
        {
            if (e.Key == Avalonia.Input.Key.P)
            {
                _toggleFullScreen();
            }
            else if (e.Key == Avalonia.Input.Key.M)
            {
                _stage.MovementManager.StopMovement();
                _isRunWithAcceleration = false;
                _isRunWithoutAcceleration = false;
                toggleMenu();
            }
            else if (!_isMenuOpen())
            {
                if (e.Key == Avalonia.Input.Key.Space)
                {
                    _stage.MovementManager.StopMovement();
                    _isRunWithAcceleration = false;
                    _isRunWithoutAcceleration = false;
                }
                else if (e.Key == Avalonia.Input.Key.S)
                {
                    // Запускаем движение с равномерной скоростью (без ускорения)
                    _stage.MovementManager.StartMovement(0);
                    _isRunWithAcceleration = true;
                    _isRunWithoutAcceleration = false;
                }
                else if (e.Key == Avalonia.Input.Key.Z)
                {
                    // Запускаем ускорение без остановки движения
                    _stage.MovementManager.StartMovement(1);
                    _isRunWithoutAcceleration = true;
                }
                else if (e.Key == Avalonia.Input.Key.X)
                {
                    // Сбрасываем ускорение у всех фигур
                    foreach (var shape in _stage.ShapeManager.Shapes)
                    {
                        shape.Acceleration = 0;
                    }
                    _isRunWithAcceleration = true;
                    _isRunWithoutAcceleration = false;
                }
            }
        }
    }