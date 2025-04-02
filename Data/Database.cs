// Database.cs
using System;
using System.Collections.Generic;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;

namespace HelloAvalonia.Data;

public static class Database
{
    private const string DbFile = "rdv.db";

    public static void Initialize()
    {
        using var connection = new SqliteConnection($"Data Source={DbFile}");
        connection.Open();

        string createRdvTable = @"
            CREATE TABLE IF NOT EXISTS RendezVous (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nom TEXT NOT NULL,
                Date TEXT NOT NULL,
                Heure TEXT NOT NULL,
                Description TEXT
            );";

        string createUserTable = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL
            );";

        connection.Execute(createRdvTable);
        connection.Execute(createUserTable);
    }

    public static void AjouterRdv(string nom, DateTime date, string heure, string description)
    {
        using var connection = new SqliteConnection($"Data Source={DbFile}");
        connection.Execute(
            "INSERT INTO RendezVous (Nom, Date, Heure, Description) VALUES (@Nom, @Date, @Heure, @Description)",
            new { Nom = nom, Date = date.ToString("yyyy-MM-dd"), Heure = heure, Description = description });
    }

    public static List<string> ChargerTousLesRdvs()
    {
        using var connection = new SqliteConnection($"Data Source={DbFile}");
        var result = connection.Query<(string Nom, string Date, string Heure, string Description)>(
            "SELECT Nom, Date, Heure, Description FROM RendezVous");

        var rdvs = new List<string>();
        foreach (var r in result)
        {
            rdvs.Add($"\ud83d\udcc5 {DateTime.Parse(r.Date):dd/MM/yyyy} | \ud83d\udd52 {r.Heure} | \ud83d\udcac {r.Nom} : {r.Description}");
        }
        return rdvs;
    }

    public static void SupprimerRdv(string nom, string date, string heure)
    {
        using var connection = new SqliteConnection($"Data Source={DbFile}");
        connection.Execute("DELETE FROM RendezVous WHERE Nom = @Nom AND Date = @Date AND Heure = @Heure",
            new { Nom = nom, Date = date, Heure = heure });
    }

    public static void AddUser(string username, string passwordHash)
    {
        using var connection = new SqliteConnection($"Data Source={DbFile}");
        connection.Execute(
            "INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash)",
            new { Username = username, PasswordHash = passwordHash });
    }

    public static bool UserExists(string username)
    {
        using var connection = new SqliteConnection($"Data Source={DbFile}");
        return connection.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM Users WHERE Username = @Username",
            new { Username = username }) > 0;
    }

    public static bool ValidateLogin(string username, string passwordHash)
    {
        using var connection = new SqliteConnection($"Data Source={DbFile}");
        return connection.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM Users WHERE Username = @Username AND PasswordHash = @PasswordHash",
            new { Username = username, PasswordHash = passwordHash }) > 0;
    }
}
