using System.Text.Json;
using System.Text.Json.Serialization;

namespace SnakeGame;

public class HighscoreEntry
{
    public string PlayerName { get; set; } = string.Empty;
    public int Score { get; set; }
    public DateTime Time { get; set; }
    public int SnakeLength { get; set; }
}

public class HighscoreManager
{
    private const string HighscoreFile =  "highscore.json";
    private const int MaxEntries = 10;
    private readonly List<HighscoreEntry> _highscore;    
    private readonly JsonSerializerOptions _jsonOptions;

    public HighscoreManager()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
        _highscore = new  List<HighscoreEntry>();
        LoadHighscore();
    }

    public void LoadHighscore()
    {
        _highscore.Clear();
        if (!File.Exists(HighscoreFile))
        {
            Console.WriteLine($"File {HighscoreFile} does not exist. Starting with empty leaderboard.");
            return;
        }

        try
        {
            var json = File.ReadAllText(HighscoreFile);
            var loadedHighscore = JsonSerializer.Deserialize<List<HighscoreEntry>>(json, _jsonOptions);
            if (loadedHighscore != null)
            {
                _highscore.AddRange(loadedHighscore);
                _highscore.Sort((a, b) =>
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
            Console.WriteLine($"Error loading highscore: {e.Message}");
        }
    }

    public void SaveHighscore()
    {
        try
        {
            var json = JsonSerializer.Serialize(_highscore, _jsonOptions);
            // var lines = _highscore.Select(x => $"{x.PlayerName} | {x.Score} | {x.Time:dd.MM.yyyy} | {x.SnakeLength}");
            File.WriteAllText(HighscoreFile, json);
            Console.WriteLine($"Highscore saved to {HighscoreFile}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error saving highscore: {e.Message}");
        }
    }

    public bool IsHighscore(int score)
    {
        return _highscore.Count < MaxEntries || score > _highscore.Last().Score;
    }

    public void AddScore(string playerName, int score, int snakeLength)
    {
        var entry = new HighscoreEntry
        {
            PlayerName = playerName,
            Score = score,
            Time = DateTime.Now,
            SnakeLength = snakeLength
        };
        _highscore.Add(entry);
        _highscore.Sort((a, b) =>
        {
            int scoreComp = b.Score.CompareTo(a.Score);
            if (scoreComp != 0)
                return scoreComp;
            return a.Time.CompareTo(b.Time);
        });
        
        if (_highscore.Count > MaxEntries)
        {
            _highscore.RemoveAt(_highscore.Count - 1);
        }
        SaveHighscore();
    }

    public List<HighscoreEntry> GetHighscore()
    {
        return new List<HighscoreEntry>(_highscore);
    }

    public int GetMinimumTopScore()
    {
        return _highscore.Count > 0 ? _highscore.Last().Score : 0;
    }
}