using System.Text.Json;
using System.Text.Json.Serialization;
using op_homework_snake_game.Data;
using op_homework_snake_game.Enums;

namespace op_homework_snake_game.UI;

public class HighscoreManager
{
    private const string DataDirectory = "Data";
    private const string ScoresDirectory = "Data/Scores";
    private const string SinglePlayerHighscoreFile =  "Data/Scores/highscore_single.json";
    private const string MultiPlayerHighscoreFile =  "Data/Scores/highscore_multi.json";
    private const int MaxEntries = 10;
    private readonly List<HighscoreEntry> _singlePlayerHighscore;
    private readonly List<HighscoreEntry> _multiPlayerHighscore;
    private readonly JsonSerializerOptions _jsonOptions;

    public HighscoreManager()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
        try
        {
            if (!Directory.Exists(DataDirectory))
                Directory.CreateDirectory(DataDirectory);
            if (!Directory.Exists(ScoresDirectory))
                Directory.CreateDirectory(ScoresDirectory);
            if (!File.Exists(SinglePlayerHighscoreFile))
                File.Create(SinglePlayerHighscoreFile).Close();
            if (!File.Exists(MultiPlayerHighscoreFile))
                File.Create(MultiPlayerHighscoreFile).Close();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error creating data directories: {e}");
        }
        
        _singlePlayerHighscore = new List<HighscoreEntry>();
        _multiPlayerHighscore = new List<HighscoreEntry>();
        LoadHighscore(SinglePlayerHighscoreFile, _singlePlayerHighscore);
        LoadHighscore(MultiPlayerHighscoreFile, _multiPlayerHighscore);
    }

    private void LoadHighscore(string filename, List<HighscoreEntry> highscore)
    {
        highscore.Clear();
        if (!File.Exists(filename))
            return;
        try
        {
            var json = File.ReadAllText(filename);
            var loadedHighscore = JsonSerializer.Deserialize<List<HighscoreEntry>>(json, _jsonOptions);
            if (loadedHighscore != null)
            {
                highscore.AddRange(loadedHighscore);
                highscore.Sort((a, b) =>
                {
                    int scoreComp = b.Score.CompareTo(a.Score);
                    if (scoreComp != 0)
                        return scoreComp;
                    return a.Time.CompareTo(b.Time);
                });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error loading {filename}: {e.Message}");
        }
    }

    private void SaveHighscore(GameMode gameMode)
    {
        var filename = gameMode == GameMode.SinglePlayer ? SinglePlayerHighscoreFile : MultiPlayerHighscoreFile;
        var highscore = GetHighscore(gameMode);
        try
        {
            var json = JsonSerializer.Serialize(highscore, _jsonOptions);
            File.WriteAllText(filename, json);
            Console.WriteLine($"Highscore saved to {filename}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error saving highscore: {e.Message}");
        }
    }

    public bool IsHighscore(int score, GameMode gameMode)
    {
        var highscore = GetHighscoreList(gameMode);
        return highscore.Count < MaxEntries || score > highscore.Last().Score;
    }

    public void AddScore(string playerName, int score, int snakeLength, GameMode gameMode)
    {
        var highscore = GetHighscoreList(gameMode);
        var entry = new HighscoreEntry
        {
            PlayerName = playerName,
            Score = score,
            Time = DateTime.Now,
            SnakeLength = snakeLength
        };
        highscore.Add(entry);
        highscore.Sort((a, b) =>
        {
            int scoreComp = b.Score.CompareTo(a.Score);
            if (scoreComp != 0)
                return scoreComp;
            return a.Time.CompareTo(b.Time);
        });
        
        if (highscore.Count > MaxEntries)
        {
            highscore.RemoveAt(highscore.Count - 1);
        }
        SaveHighscore(gameMode);
    }

    public List<HighscoreEntry> GetHighscore(GameMode gameMode)
    {
        return [..GetHighscoreList(gameMode)];
    }

    private List<HighscoreEntry> GetHighscoreList(GameMode gameMode)
    {
        return gameMode == GameMode.SinglePlayer ?  _singlePlayerHighscore : _multiPlayerHighscore;
    }
    

    public int GetMinimumTopScore(GameMode gameMode)
    {
        var highscore = GetHighscoreList(gameMode);
        return highscore.Count > 0 ? highscore.Last().Score : 0;
    }
}