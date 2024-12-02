using System;
using System.Data.SQLite;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string stickyNotesFolder = Path.Combine(localAppData, "Packages");
            string[] candidateFolders = Directory.GetDirectories(stickyNotesFolder, "Microsoft.MicrosoftStickyNotes_*");

            if (candidateFolders.Length == 0)
            {
                Console.WriteLine("Sticky Notes folder not found.");
                return;
            }

            string stickyNotesDbPath = Path.Combine(candidateFolders[0], "LocalState", "plum.sqlite");

            if (!File.Exists(stickyNotesDbPath))
            {
                Console.WriteLine($"Sticky Notes database not found at: {stickyNotesDbPath}");
                return;
            }

            string connectionString = $"Data Source={stickyNotesDbPath};Version=3;";

            Dictionary<string, string> notesDictionary = new Dictionary<string, string>();

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Connected to SQLite database.");

                string query = "SELECT Id, Text FROM Note";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        Console.WriteLine("Reading data...");
                        while (reader.Read())
                        {
                            string id = reader.GetString(0); // Column 0: Id
                            string note = reader.GetString(1); // Column 1: Text
                            notesDictionary[id] = note;
                        }
                    }
                }

                connection.Close();
                Console.WriteLine("Connection closed.");
            }

            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string outputPath = Path.Combine(userPath, "notes.json");

            string jsonOutput = JsonSerializer.Serialize(notesDictionary, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(outputPath, jsonOutput);

            Console.WriteLine($"Notes have been written to {outputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
