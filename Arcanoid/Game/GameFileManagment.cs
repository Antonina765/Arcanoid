using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia.Controls;
using Arcanoid.Models;
using Arcanoid.Stage;

namespace Arcanoid.Game;

public class GameFileManagment
{ 
    private readonly Window _mainWindow;
    private readonly StageDataManager _dataManager; 
    private const string SaveFileName = "save.json";

    public GameFileManagment(Window mainWindow, StageDataManager dataManager) 
    { 
        _mainWindow = mainWindow; 
        _dataManager = dataManager;
    }

    public async void SaveGame() 
    { 
        var dialog = new SaveFileDialog() 
        { 
            Title = "Сохранение игры", 
            DefaultExtension = "json", 
            InitialFileName = SaveFileName, 
            Filters = new List<FileDialogFilter> 
            { 
                new FileDialogFilter { Name = "JSON Files", Extensions = { "json" } }
            }
        };
        
        var result = await dialog.ShowAsync(_mainWindow); 
        if (!string.IsNullOrEmpty(result)) 
        { 
            var shapesData = _dataManager.GetShapesData(); 
            var json = JsonSerializer.Serialize(shapesData, new JsonSerializerOptions { WriteIndented = true }); 
            File.WriteAllText(result, json); 
            Console.WriteLine("Игра сохранена: " + result);
        }
        else 
        { 
            Console.WriteLine("Сохранение отменено.");
        }
    }

    public async void LoadGame() 
    { 
        var dialog = new OpenFileDialog 
        { 
            Title = "Загрузка игры", 
            AllowMultiple = false, 
            Filters = new List<FileDialogFilter> 
            { 
                new FileDialogFilter { Name = "JSON Files", Extensions = { "json" } }
            }
        };
        
        var result = await dialog.ShowAsync(_mainWindow); 
        if (result != null && result.Length > 0) 
        { 
            var filePath = result[0]; 
            if (File.Exists(filePath)) 
            { 
                var json = File.ReadAllText(filePath); 
                var shapesData = JsonSerializer.Deserialize<List<ShapeData>>(json); 
                _dataManager.LoadShapesData(shapesData); 
                Console.WriteLine("Игра загружена: " + filePath);
            }
            else 
            { 
                Console.WriteLine("Файл не найден.");
            }
        }
        else 
        {
            Console.WriteLine("Загрузка отменена.");
        }
    }
}





