using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace Arcanoid
{
    public partial class MainWindow : Window
    {
        private Game.Game _game;

        public MainWindow()
        {
            InitializeComponent();
            _game = new Game.Game(this);
            // Не вызывайте _game.Start() здесь!
            this.Opened += MainWindow_Opened;
        }

        private void MainWindow_Opened(object? sender, EventArgs e)
        {
            // Вызываем Start() после того, как окно открылось и размеры вычислены
            _game.Start();
            // Отписываемся, чтобы Start вызвался только один раз
            this.Opened -= MainWindow_Opened;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}