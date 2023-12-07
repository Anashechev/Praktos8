using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;

public interface ITypingTest
{
    void StartTyping();
}

public class User
{
    public string Name { get; set; }
    public int CharactersPerMinute { get; set; }
    public double CharactersPerSecond { get; set; }
    public TimeSpan TimeTaken { get; set; }
}

public static class Leaderboard
{
    private static List<User> records;

    static Leaderboard()
    {
        records = new List<User>();
    }

    public static void AddRecord(User user)
    {
        records.Add(user);
    }

    public static void SaveLeaderboard(string fileName)
    {
        string json = JsonSerializer.Serialize(records);
        File.WriteAllText(fileName, json);
    }

    public static void LoadLeaderboard(string fileName)
    {
        if (File.Exists(fileName))
        {
            string json = File.ReadAllText(fileName);
            records = JsonSerializer.Deserialize<List<User>>(json);
        }
    }

    public static void DisplayLeaderboard()
    {
        Console.WriteLine("Список лидеров:");
        Console.WriteLine("Ник\t\tCPM\tCPS\tВремя");
        foreach (var user in records)
        {
            Console.WriteLine($"{user.Name}\t\t{user.CharactersPerMinute}\t{user.CharactersPerSecond}\t{user.TimeTaken.TotalSeconds} сек");
        }
    }
}

public class TypingTest : ITypingTest
{
    private string text;
    private Stopwatch stopwatch;
    private User user;

    public TypingTest()
    {
        text = "Пикать дазла в мид плохая идея, а акса в лес неплохо.";
        stopwatch = new Stopwatch();
    }

    public void StartTyping()
    {
        Console.Write("Ваш ник: ");
        string name = Console.ReadLine();

        user = new User { Name = name };

        Console.WriteLine($"Текст для написания:\n{text}");
        Console.WriteLine("У вас есть 1 минута чтобы закончить писать этот текст.");

        Timer timer = new Timer(TimerCallback, null, 60000, Timeout.Infinite);

        stopwatch.Start();

        string input = Console.ReadLine();

        while (!string.Equals(input, text, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Неверный текст, попробуйте ещё раз.");
            input = Console.ReadLine();
        }

        stopwatch.Stop();
        timer.Dispose();

        CalculateResults();
        Leaderboard.AddRecord(user);

        Console.WriteLine("Молодец!");
        Leaderboard.DisplayLeaderboard();
    }

    private void TimerCallback(object state)
    {
        Console.WriteLine("\nВремя закончилось! Время написания текста не может превышать 1 минуту.");
        Environment.Exit(0);
    }

    private void CalculateResults()
    {
        int totalCharacters = text.Length;
        double totalTimeSeconds = stopwatch.Elapsed.TotalSeconds;

        double charactersPerSecond = totalCharacters / totalTimeSeconds;
        int charactersPerMinute = (int)(charactersPerSecond * 60);

        user.CharactersPerMinute = charactersPerMinute;
        user.CharactersPerSecond = charactersPerSecond;
        user.TimeTaken = stopwatch.Elapsed;
    }
}

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Leaderboard.LoadLeaderboard("leaderboard.json");

            do
            {
                TypingTest typingTest = new TypingTest();
                typingTest.StartTyping();

                Console.Write("Хотите пройти тест еще раз? (y/n): ");
            } while (Console.ReadLine()?.ToLower() == "y");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            Leaderboard.SaveLeaderboard("leaderboard.json");
        }
    }
}