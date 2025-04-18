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

namespace Arcanoid.Game;

public class Game
{
        private readonly Stage.Stage _stage;
        private readonly Window _mainWindow;
        private readonly GameMenu _menu;
        private Grid _mainGrid;
        private Canvas _menuCanvas;
        
        private bool _isFullScreen = true;
        private bool _isMenuOpen;
        private int _shapeCount = 20;
        
        private readonly GameFileManager _fileManager;
        private readonly GameInputHelder _inputHelder;
        private readonly GameMenuActions _menuActions;
        
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
            _menuCanvas = new Canvas
            {
                Background = Brushes.Transparent,
                IsHitTestVisible = false,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            _fileManager = new GameFileManager(_mainWindow, _stage);
            _inputHelder = new GameInputHelder(_stage, ToggleFullScreen, () => _isMenuOpen);
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
            
            border.Child = _mainGrid;
            _mainWindow.Content = border;
            _mainWindow.KeyDown += OnKeyDown;
        }
        
        public void Start()
        {
            _stage.ShapeManager.AddRandomShapes(_shapeCount, (int)_mainWindow.Width, (int)_mainWindow.Height);
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