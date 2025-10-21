using op_homework_snake_game.Data;
using op_homework_snake_game.Entities;
using op_homework_snake_game.Enums;

namespace op_homework_snake_game.Core;

public static class MapLoader
{
    private const string MapsDirectory = "Data/Maps";

    public static List<MapInfo> LoadMaps()
    {
        List<MapInfo> maps = [];
        if (!Directory.Exists(MapsDirectory))
            return maps;
        
        foreach (string mapDirectory in Directory.GetFiles(MapsDirectory))
        {
            try
            {
                var map = ParseMapMetadata(mapDirectory);
                if (map != null) 
                    maps.Add(map);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Broken metadata in map {mapDirectory}: {e.Message}");
            }
        }
        
        return maps;
    }

    public static MapInfo? ParseMapMetadata(string filePath)
    {
        var map = new MapInfo {FilePath = filePath};
        try
        {
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                if (!line.StartsWith("/")) break;
                var parts = line.Substring(1).Trim().Split(':', 2);
                if (parts.Length < 2) continue;
                var key = parts[0].Trim().ToLower();
                var value = parts[1].Trim();

                switch (key)
                {
                    case "name":
                        map.Name = value;
                        break;
                    case "allowedmodes":
                        map.AllowedModes = value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(v =>
                                Enum.TryParse<GameMode>(v.Trim(), true, out var mode) ? mode : GameMode.SinglePlayer)
                            .Distinct().ToList();
                        break;
                    default:
                        if (key.StartsWith("snake"))
                        {
                            var coords = value.Split(',');
                            if (coords.Length == 2 &&
                                int.TryParse(coords[0], out int x) &&
                                int.TryParse(coords[1], out int y))
                            {
                                map.SnakeStartPositions.Add(new Point(x, y));
                            }
                        }

                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(map.Name))
                map.Name = Path.GetFileNameWithoutExtension(filePath);

            if (map.AllowedModes.Count == 0)
                map.AllowedModes.Add(GameMode.SinglePlayer);

            return map;
        }
        catch
        {
            return null;
        }
    }
}