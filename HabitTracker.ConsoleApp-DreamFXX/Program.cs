using System;
using System.Globalization;
using System.Collections.Generic;
using System.Data.SQLite;

internal class Program
{
    static string connectionString = @"Data Source=HabitTracker-ConsoleApp.db";

    static void Main(string[] args)
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand(); // Naváže spojení a proměnnou která bude nosit zadané příkazy.

            // Příkazy do sql cmd + @->dlouhé stringy se znaky jinak nepoužitelnými.
            tableCmd.CommandText = 
                @"CREATE TABLE IF NOT EXISTS kratom_doses(
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date Text,
                    Time Text,
                    Dose INTEGER
                    )"; 

            // Primary Key - Main Identifier of the record / Increment - Every add to database the id will create itself

            tableCmd.ExecuteNonQuery(); // // Execute the command and shows rows of the db. -> ENTER

            connection.Close();
        }
        
        GetUserInput();
    }

    static void GetUserInput()
    {
        Console.Clear();

        bool closeApp = false;
        while (closeApp == false)
        {
            

            Console.WriteLine("Vítej v monitoringu dávkování!");
            Console.WriteLine("\nHLAVNÍ MENU");
            Console.WriteLine("0 -> Uložit změny a zavřít Aplikaci\n");
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("1 -> Zobrazit všechny zápisy dávkování");
            Console.WriteLine("2 -> Vložit svou poslední dávku");
            Console.WriteLine("3 -> Smazat vybraný zápis");
            Console.WriteLine("4 -> Upravit uložený zápis");
            Console.WriteLine("------------------------------------------");

            string command = Console.ReadLine();

            switch (command)
            {
                case "0":
                    Console.WriteLine("\nHezký den vám přeje dose Monitoring! - Habit Tracker App by DreamFX.");
                    closeApp = true;
                    Environment.Exit(1);
                    break;
                case "1":
                    ViewAllRecords();
                    break;
                case "2":
                    AddRecord();
                    break;
                case "3":
                    DeleteRecod();
                    break;
                case "4":
                    ChangeRecord();
                    break;
                default:
                    Console.WriteLine("\n\nZadaná možnost neexistuje. Zkuste to prosím znovu (0 - 4).\n\n");
                    break;
            }
        }
    }

    private static void ViewAllRecords()
    {
        Console.Clear();

        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = "SELECT * FROM kratom_doses";

            List<KratomDoses> tableData = new();

            SQLiteDataReader reader = tableCmd.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    tableData.Add(
                        new KratomDoses
                        {
                            Id = reader.GetInt32(0),
                            Date = reader.GetString(1),
                            Time = reader.GetString(2),
                            Dose = reader.GetInt32(3)
                        });
                }
            }
            else
            {
                Console.WriteLine("\n\nMáš prázdnou databázi dávek! Začni tím, že si začneš jednotlivé dávky zapisovat.\n\n");
            }

            connection.Close();


            Console.WriteLine("------------CONSUMED Grams LIST-----------\n");
            foreach (var dw in tableData)
            {
                Console.WriteLine($"{dw.Id} -> {dw.Date} in {dw.Time}h // {dw.Dose}grams.");
            }
            Console.WriteLine("------------------------------------------\n");
        }

        
    }

    private static void AddRecord()
    {
        string date = GetDate();
        string time = GetTime();
        
        int dose = GetDoseInput("\n\nZadejte počet gramů poslední dávky Kratomu.\n\n");

        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open(); 
            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = $"INSERT INTO kratom_doses(date, time, dose) VALUES('{date}', '{time}', {dose})";

            tableCmd.ExecuteNonQuery(); // Potrvdí příkaz v sql cmd line.

            connection.Close();
        }
    }

    internal static void ChangeRecord()
    {
        // Console.Clear();
        ViewAllRecords();

        var recordId = GetDoseInput("\n\nZadej ID číslo zápisu, který chceš upravit.\n\n");

        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM kratom_doses WHERE Id = {recordId})";

            int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (checkQuery == 0)
            {
                Console.WriteLine($"\n\nZápis s ID '{recordId}' neexistuje, zadejte ID existujícího zápisu.\n\n");
                connection.Close();
                ChangeRecord();
            }

            string date = GetDate();
            string time = GetTime();

            int dose = GetDoseInput("\n\nZadej počet gramů, které obsahovala tvá poslední dávka.\n\n");

            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = $"UPDATE kratom_doses SET date = '{date}', time = '{time}', dose = {dose} WHERE Id = {recordId}";

            tableCmd.ExecuteNonQuery();

            connection.Close();
        }


    }

    private static void DeleteRecod()
    {
        Console.Clear();
        ViewAllRecords();

        var recordId = GetDoseInput("Zadej Id číslo záznamu, který si přeješ smazat.");


        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = $"DELETE from kratom_doses WHERE Id = '{recordId}'";

            int rowCount = tableCmd.ExecuteNonQuery();

            if(rowCount == 0)
            {
                Console.WriteLine("Zápis s číslem {recordId} neexistuje, zkus to znovu.");
                DeleteRecod();
            }
        }

        Console.WriteLine($"Zápis s ID {recordId} byl úspěšně smazán. Stiskni ENTER pro návrat do MENU.");
        Console.ReadLine(); // ??
        GetUserInput();
    }



    internal static string GetTime()
    {
        Console.WriteLine("\n\nZadej čas konzumace poslední dnešní dávky.\n0 -> MENU\n");
        Console.Write("Formát zápisu -> hh:mm - ");

        string timeInput = Console.ReadLine();

        if (timeInput == "0") GetUserInput();

        return timeInput;
    }

    internal static string GetDate()
    {
        var dateOnly = DateOnly.FromDateTime(DateTime.Today).ToString();
        return dateOnly;
    }

    internal static int GetDoseInput(string message)
    {
        Console.WriteLine(message);

        string doseInput = Console.ReadLine();

        if (doseInput == "0") GetUserInput();

        int intDoseInput = Convert.ToInt32(doseInput); // Konverze z čísla na string.

        return intDoseInput;
    }
}

public class KratomDoses
{
    public int Id { get; set; }
    public string Time { get; set; }
    public string Date { get; set; }
    public int Dose { get; set; }
}