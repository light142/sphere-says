using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TelemetryEvent
{
    public string event_at;
    public string event_type;
    public string event_category;
    public string ip_address;
    public string player_id;
    public string session_id;
    public string game_reference;
    public int game_level;
    public string game_mode;
    public string game_color;
    public string correct_game_color; // The correct color that should have been selected
    public List<string> game_sequence;
    public List<string> game_player_input;
    public int retry_count;
    public List<string> error_messages;

    public TelemetryEvent()
    {
        event_at = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        game_sequence = new List<string>();
        game_player_input = new List<string>();
        retry_count = 0;
        error_messages = new List<string>();
    }

    public TelemetryEvent(string eventType, string eventCategory, string sessionId, string gameMode, int gameLevel, string gameReference = null) : this()
    {
        event_type = eventType;
        event_category = eventCategory;
        session_id = sessionId;
        game_mode = gameMode;
        game_level = gameLevel;
        game_reference = gameReference; // Only set if provided, otherwise null
        ip_address = GetLocalIPAddress();
        player_id = GetPlayerId();
    }

    public void SetColor(Color color)
    {
        game_color = ColorToString(color);
    }

    public void SetSequence(List<Color> sequence)
    {
        game_sequence.Clear();
        foreach (Color color in sequence)
        {
            game_sequence.Add(ColorToString(color));
        }
    }

    public void SetPlayerInput(List<Color> playerInput)
    {
        game_player_input.Clear();
        foreach (Color color in playerInput)
        {
            game_player_input.Add(ColorToString(color));
        }
    }
    
    public void SetCorrectColor(Color correctColor)
    {
        correct_game_color = ColorToString(correctColor);
    }

    private string ColorToString(Color color)
    {
        if (color == Color.red) return "red";
        if (color == Color.blue) return "blue";
        if (color == Color.green) return "green";
        if (color == Color.yellow) return "yellow";
        return "unknown";
    }

    private string GetLocalIPAddress()
    {
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
        }
        catch
        {
            // Fallback to localhost if network detection fails
        }
        return "127.0.0.1";
    }


    private string GetPlayerId()
    {
        // Get or create a persistent player ID
        string playerId = PlayerPrefs.GetString("SphereGame_PlayerId", "");
        
        if (string.IsNullOrEmpty(playerId))
        {
            // Generate new player ID and store it
            playerId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString("SphereGame_PlayerId", playerId);
            PlayerPrefs.Save();
        }
        
        return playerId;
    }

    private string GenerateGameReference()
    {
        // Generate a unique game reference using timestamp and random component
        return $"game_{DateTime.UtcNow:yyyyMMddHHmmss}_{UnityEngine.Random.Range(1000, 9999)}";
    }

    public void IncrementRetryCount()
    {
        retry_count++;
    }

    public void AddErrorMessage(string error)
    {
        if (!string.IsNullOrEmpty(error))
        {
            error_messages.Add($"{DateTime.UtcNow:HH:mm:ss} - {error}");
        }
    }
}
