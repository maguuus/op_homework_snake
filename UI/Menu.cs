using op_homework_snake_game.Core;
using op_homework_snake_game.Enums;

namespace op_homework_snake_game.UI;

public class Menu
{
    private readonly HighscoreManager _highscoreManager;

    private readonly string[] _mainMenuItems =
    {
        "Single player",
        "Multiplayer",
        "Top players",
        "Exit"
    };
    public Menu()
    {
        _highscoreManager = new HighscoreManager();
    }

    public void Start()
    {
        int selectedIndex = 0;
        while (true)
        {
            Console.Clear();
            DrawMenu("SNAKE GAME", _mainMenuItems, selectedIndex);
            var key = Console.ReadKey();
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                case ConsoleKey.W:
                    selectedIndex = (selectedIndex - 1 +  _mainMenuItems.Length) % _mainMenuItems.Length;
                    break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                    selectedIndex = (selectedIndex + 1) % _mainMenuItems.Length;
                    break;
                case ConsoleKey.Escape:
                    Console.Clear();
                    return;
                case ConsoleKey.Enter:
                    HandleSelection(selectedIndex);
                    break;
            }
        }
    }

    private void DrawMenu(string title, string[] items, int selectedIndex)
    {
        int menuWidth = items.Max(item => item.Length) + 4;
        int leftIndent = (Console.WindowWidth - menuWidth) / 2;
        int topIndent = Console.WindowHeight / 3;
        Console.SetCursorPosition(leftIndent, topIndent - 2);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(title.PadLeft((menuWidth - title.Length) / 2));
        Console.ResetColor();

        for (int i = 0; i < items.Length; i++)
        {
            Console.SetCursorPosition(leftIndent, topIndent + i);
            if (i == selectedIndex)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                Console.Write($"> {items[i].PadRight(menuWidth)}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write($" {items[i].PadRight(menuWidth)}");
            }
            Console.ResetColor();
        }
        Console.ForegroundColor = ConsoleColor.Green;
        string hint1 = "Use WASD/↑↓ arrows to navigate, Enter to select";
        string hint2 = "Press ESC to exit";
        int hint1X = leftIndent + (menuWidth - hint1.Length) / 2;
        int hint2X = leftIndent + (menuWidth - hint2.Length) / 2;
        Console.SetCursorPosition(hint1X, topIndent + items.Length + 1);
        Console.Write(hint1);
        Console.SetCursorPosition(hint2X, topIndent + items.Length + 2);
        Console.Write(hint2);
    }

    private void HandleSelection(int selectedIndex)
    {
        switch (selectedIndex)
        {
            case 0:
                StartGame(GameMode.SinglePlayer);
                break;
            case 1:
                StartGame(GameMode.MultiPlayer);
                break;
            case 2:
                ShowHighscore();
                break;
            case 3:
                Console.Clear();
                Environment.Exit(0);
                break;
        }
    }

    private void StartGame(GameMode gameMode)
    {
        var maps = MapLoader.LoadMaps();
        var availableMaps = maps.Where(map => map.AllowedModes.Contains(gameMode)).ToList();
        var selectedPath = " ";
        if (availableMaps.Count != 0)
        {
            int selectedIndex = 0;
            ConsoleKey key;
            do {
                Console.Clear();
                string title = "==== Select map ====";
                int titleX = (Console.WindowWidth - title.Length) / 2;
                int titleY = Console.WindowHeight / 2;
                Console.SetCursorPosition(titleX, titleY);
                Console.Write(title);
                int startY = titleY + 2;
                for (int i = 0; i < availableMaps.Count; i++)
                {
                    Console.ResetColor();
                    string line = $"{i + 1}. {availableMaps[i].Name}";
                    int x = (Console.WindowWidth - line.Length - 4) / 2;
                    int y = startY + i;
                    Console.SetCursorPosition(x, y);
                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.Write($"> {line}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.Write($"  {line}");
                        Console.ResetColor();
                    }
                }
                Console.ForegroundColor = ConsoleColor.Green;
                string hint1 = "Use WASD/↑↓ arrows to navigate, Enter to select";
                string hint2 = "Press ESC to exit";
                int hint1X = (Console.WindowWidth - hint1.Length) / 2;
                int hint2X = (Console.WindowWidth - hint2.Length) / 2;
                Console.SetCursorPosition(hint1X, startY + availableMaps.Count + 2);
                Console.Write(hint1);
                Console.SetCursorPosition(hint2X, startY + availableMaps.Count + 3);
                Console.Write(hint2);
                Console.ResetColor();
                
                key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W:
                        selectedIndex = (selectedIndex - 1 + availableMaps.Count) % availableMaps.Count;
                        break;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                        selectedIndex = (selectedIndex + 1) % availableMaps.Count;
                        break;
                    case ConsoleKey.Escape:
                        Console.Clear();
                        return;
                }
            } while (key != ConsoleKey.Enter);
            selectedPath = availableMaps[selectedIndex].FilePath;
            
        }
        var game = new Game(_highscoreManager, gameMode, Path.GetFileNameWithoutExtension(selectedPath));
        game.Start();
    }

    private void ShowHighscore()
    {
        Console.Clear();
        var singlePlayerHighscore =  _highscoreManager.GetHighscore(GameMode.SinglePlayer);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"🏆 TOP PLAYERS (Single player) 🏆");
        Console.WriteLine($"=================");
        Console.ResetColor();
        if (singlePlayerHighscore.Count == 0)
        {
            Console.WriteLine("No highscore yet!");
            Console.WriteLine("Be the first to set a record!");
        }
        else
        {
            for (int i = 0; i < singlePlayerHighscore.Count; i++)
            {
                var entry = singlePlayerHighscore[i];
                Console.ForegroundColor = i switch
                {
                    0 => ConsoleColor.Yellow,
                    1 => ConsoleColor.DarkGray,
                    2 => ConsoleColor.DarkYellow,
                    _ => ConsoleColor.White
                };
                Console.Write($"{i + 1, 2}. ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{entry.PlayerName, -15}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{entry.Score, 5} points ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"(Length: {entry.SnakeLength, 2}) ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"{entry.Time:dd.MM.yyyy}");
            }
        }
        var multiPlayerHighscore =  _highscoreManager.GetHighscore(GameMode.MultiPlayer);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"🏆 TOP PLAYERS (Multi player) 🏆");
        Console.WriteLine($"=================");
        Console.ResetColor();
        if (multiPlayerHighscore.Count == 0)
        {
            Console.WriteLine("No highscore yet!");
            Console.WriteLine("Be the first to set a record!");
        }
        else
        {
            for (int i = 0; i < multiPlayerHighscore.Count; i++)
            {
                var entry = multiPlayerHighscore[i];
                Console.ForegroundColor = i switch
                {
                    0 => ConsoleColor.Yellow,
                    1 => ConsoleColor.DarkGray,
                    2 => ConsoleColor.DarkYellow,
                    _ => ConsoleColor.White
                };
                Console.Write($"{i + 1, 2}. ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{entry.PlayerName, -15}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{entry.Score, 5} points ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"(Length: {entry.SnakeLength, 2}) ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"{entry.Time:dd.MM.yyyy}");
            }
        }
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write("Press any to return...");
        Console.ReadKey(true);
    }

    public string GetPlayerName(int score, int snakeLength, GameMode gameMode, int winner)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(gameMode == GameMode.SinglePlayer
            ? $"Congratulations!"
            : $"Congratulations, Player {winner + 1}!");
        Console.ResetColor();
        Console.WriteLine($"You achieved {score} points with a snake length of {snakeLength}!");
        Console.WriteLine($"Mode: {gameMode}");
        Console.WriteLine($"This qualifies for the top {_highscoreManager.GetHighscore(gameMode).Count + 1} players!");
        Console.WriteLine();
        Console.WriteLine("Enter your name (3-15 characters, letters and numbers only): ");
        Console.WriteLine("Press ESC to cancel and use 'Anonymous'");
        Console.WriteLine("Name: ");
        return GetSafeInput(3, 15) ?? "Anonymous";
    }

    private string? GetSafeInput(int minLength, int maxLength)
    {
        string input = string.Empty;
        int left = Console.CursorLeft;
        int top = Console.CursorTop;
        while (true)
        {
            var key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    string trimmed = input.Trim();
                    if (trimmed.Length >= minLength && trimmed.Length <= maxLength)
                    {
                        return trimmed;
                    }
                    if (trimmed.Length == 0)
                    {
                        return null;
                    }
                    Console.SetCursorPosition(0, top + 1);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Name must be between {minLength} and {maxLength} characters.");
                    Console.ResetColor();
                    Console.SetCursorPosition(left, top);
                    Console.Write(new string(' ', input.Length));
                    Console.SetCursorPosition(left, top);
                    input = string.Empty;
                    break;
                case ConsoleKey.Escape:
                    Console.WriteLine();
                    return null;
                case ConsoleKey.Backspace:
                    if (input.Length > 0)
                    {
                        input = input[..^1];
                        Console.Write("\b \b");
                    } 
                    break;
                default:
                    if (char.IsLetterOrDigit(key.KeyChar) && input.Length < maxLength)
                    {
                        input += key.KeyChar;
                        Console.Write(key.KeyChar);
                    }
                    break;
            }
        }
    }
}