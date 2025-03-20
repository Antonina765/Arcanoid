using Avalonia.Input;

namespace Arcanoid.Game;

public class GameInputHendler
{ 
    private readonly Game _game;
    public GameInputHendler(Game game) 
    { 
        _game = game;
    }

    public void HandleKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.P)
        {
            _game.ToggleFullScreen();
        }
        else if (e.Key == Key.M)
        {
            _game.MovementManager.StopMovement();
            _game.IsRunWithAcceleration = false;
            _game.IsRunWithoutAcceleration = false;
            _game.ToggleMenu();
        }
        else if (!_game.IsMenuOpen)
        {
            if (e.Key == Key.Space)
            {
                _game.MovementManager.StopMovement();
                _game.IsRunWithAcceleration = false;
                _game.IsRunWithoutAcceleration = false;
            }
            else if (e.Key == Key.S)
            {
                // Запускаем движение фигур с равномерной скоростью (без ускорения)
                _game.MovementManager.StartMovement(0);
                _game.IsRunWithAcceleration = true;
                _game.IsRunWithoutAcceleration = false;
            }
            else if (e.Key == Key.Z)
            {
                // При нажатии Z запускаем ускорение без остановки движения
                _game.MovementManager.StartMovement(1);
                _game.IsRunWithoutAcceleration = true;
            }
            else if (e.Key == Key.X)
            {
                // сбрасываем ускорение у всех фигур, оставляя их текущую скорость
                foreach (var shape in _game.Stage.Shapes)
                {
                    shape.Acceleration = 0;
                }
                _game.IsRunWithAcceleration = true;
                _game.IsRunWithoutAcceleration = false;
            }
        }
    }

}

