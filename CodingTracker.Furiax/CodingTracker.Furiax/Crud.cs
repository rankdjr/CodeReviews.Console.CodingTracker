﻿using CodingTracker.Furiax.Model;
using Microsoft.Data.Sqlite;
using System.Globalization;
using ConsoleTableExt;
using System.Data;
using Microsoft.Win32;

namespace CodingTracker.Furiax
{
	internal class Crud
	{
		internal static void CreateTable(string connectionString)
		{
			using (var connection = new SqliteConnection(connectionString))
			{
				connection.Open();
				var command = connection.CreateCommand();
				command.CommandText = @"CREATE TABLE IF NOT EXISTS CodeTracker(
									Id INTEGER PRIMARY KEY AUTOINCREMENT,
									StartTime TEXT NOT NULL,
									EndTime TEXT NOT NULL,
									Duration TEXT)";
				command.ExecuteNonQuery();
				connection.Close();
			}
		}
		internal static void DeleteRecord(string connectionString)
		{
			Console.Clear();
			OverviewTable(connectionString);
			while (true)
			{
				
				int recordToDelete = UserInput.GetId("What record do you want to delete: ");
				using (var connection = new SqliteConnection(connectionString))
				{
					connection.Open();
					bool doesIdExist = Validation.CheckIfRecordExists(recordToDelete, connectionString);
					if (doesIdExist)
					{
						
						var command = connection.CreateCommand();
						command.CommandText = $"DELETE FROM CodeTracker WHERE id = '{recordToDelete}'";
						command.ExecuteNonQuery();
						Console.WriteLine($"The record with id {recordToDelete} has been deleted from the db");
						break;
					}
					else
					{
						Console.WriteLine("A record with that id has not been found");
					}
					connection.Close();
				}
			}	
		}
		internal static void UpdateRecord(string connectionString)
		{
			Console.Clear();
			OverviewTable(connectionString);
			while(true)
			{
				int recordToUpdate = UserInput.GetId("What record do you want to update: ");
				using(var connection = new SqliteConnection( connectionString))
				{
					connection.Open();
					bool doesIdExist = Validation.CheckIfRecordExists(recordToUpdate, connectionString);
					if (doesIdExist)
					{
						DateTime askNewStartTime = UserInput.GetStartDate("Enter a new value for StartDate");
						string newStartTime = askNewStartTime.ToString("dd/MM/yy HH:mm");
						DateTime askNewEndTime = UserInput.GetEndDate("Enter a new value for EndDate", askNewStartTime);
						string newEndTime = askNewEndTime.ToString("dd/MM/yy HH:mm");
						TimeSpan timeBetween = CalculateDuration(askNewStartTime, askNewEndTime);
						string duration = timeBetween.ToString(@"hh\:mm");
						var command = connection.CreateCommand();
						command.CommandText = $"UPDATE CodeTracker SET StartTime = '{newStartTime}', EndTime = '{newEndTime}', Duration = '{duration}' WHERE Id = '{recordToUpdate}'";
						command.ExecuteNonQuery();
						break;
					}
					else 
					{
						Console.WriteLine("A record with that id has not been found");
					}
					connection.Close();
				}
			}
            Console.WriteLine("Record succesfully updated");
        }
		internal static void OverviewTable(string connectionString)
		{
			Console.Clear();
			string sqlCommand = "SELECT * FROM CodeTracker";
			List<CodingSession> sessions = new List<CodingSession>();
			sessions = BuildList(connectionString, sqlCommand);
			PrintTable(connectionString, sessions);
		}
		internal static void InsertRecord(string connectionString)
		{
			Console.Clear();
			DateTime inputStartDate = UserInput.GetStartDate("Please enter the start time in the following format dd/mm/yy hh:mm");
			string startDate =inputStartDate.ToString("dd/MM/yy HH:mm");
			DateTime inputEndDate = UserInput.GetEndDate("Please enter the end time in the following format dd/mm/yy hh:mm", inputStartDate);
			string endDate = inputEndDate.ToString("dd/MM/yy HH:mm");
			TimeSpan timeBetween = CalculateDuration(inputStartDate, inputEndDate);
			string duration = timeBetween.ToString(@"hh\:mm");
			using (var connection = new SqliteConnection(connectionString))
			{
				connection.Open();
				var command = connection.CreateCommand();
				command.CommandText = $"INSERT INTO CodeTracker (StartTime, EndTime, Duration) VALUES ('{startDate}','{endDate}', '{duration}')";
				command.ExecuteNonQuery();
				connection.Close();
			}
            Console.WriteLine("Times succesfully added to database");
        }
		internal static TimeSpan CalculateDuration(DateTime startTime, DateTime endTime)
		{
			TimeSpan duration = endTime - startTime;
			return duration;
		}

		internal static void InsertStopwatchRecord(string connectionString, DateTime startTime, DateTime stopTime, string start, string stop)
		{
			TimeSpan timeBetween = CalculateDuration(startTime, stopTime);
			string duration = timeBetween.ToString(@"hh\:mm");
			if (timeBetween.Minutes <= 0)
			{
                Console.WriteLine("Session was less then a minute and therefore not stored");
            }
			else
			{
				using (var connection = new SqliteConnection(connectionString))
				{
					connection.Open();
					var command = connection.CreateCommand();
					command.CommandText = $"INSERT INTO CodeTracker (StartTime, EndTime, Duration) VALUES ('{start}', '{stop}', '{duration}')";
					command.ExecuteNonQuery();
					connection.Close();
				}
				Console.WriteLine("Record added to database");
			}
        }
		internal static void CreateReport(string connectionString)
		{
			Console.Clear();
			string sqlCommand = "SELECT * FROM CodeTracker";
			List<CodingSession> sessions = new List<CodingSession>();
			sessions = BuildList(connectionString, sqlCommand);
			int count = 0;
			TimeSpan totalTime = TimeSpan.Zero;
			foreach (var session in sessions)
			{
					totalTime = totalTime + session.Duration;
					count++;
			}
			TimeSpan averageTime = totalTime/count;

				Console.WriteLine($"You spend a total of {totalTime.ToString()} time on coding, divided over {count} sessions.");
                Console.WriteLine($"On average you spend {averageTime} per session.");
		}

		internal static void GoalStatus(string connectionString, TimeSpan goalTime)
		{
			Console.Clear();
			using( var connection = new SqliteConnection(connectionString))
			{ 
				var monday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek+(int)DayOfWeek.Monday);
                connection.Open();
				var command = connection.CreateCommand();
				command.CommandText = $"SELECT * FROM CodeTracker WHERE 'StartTime' >= '{monday}'";
				List<CodingSession> sessions = new List<CodingSession>();
				SqliteDataReader reader = command.ExecuteReader();
				while (reader.Read())
				{
					sessions.Add(new CodingSession {
						Id = reader.GetInt32(0),
						StartTime = DateTime.ParseExact(reader.GetString(1), "dd/MM/yy HH:mm", new CultureInfo("nl-BE")),
						EndTime = DateTime.ParseExact(reader.GetString(2), "dd/MM/yy HH:mm", new CultureInfo("nl-BE")),
						Duration = TimeSpan.ParseExact(reader.GetString(3), @"hh\:mm", new CultureInfo("nl-BE"))
					});
				}
				TimeSpan totalTimeCodedThisWeek = TimeSpan.Zero;
				foreach (var session in sessions)
				{
					totalTimeCodedThisWeek =+ session.Duration;
				}
				int daysLeft = 7 - (int)DateTime.Today.DayOfWeek +1;
				if (goalTime <= totalTimeCodedThisWeek)
					Console.WriteLine($"Goal achieved, you coded {totalTimeCodedThisWeek.TotalHours} hours this week, while the goal was {goalTime.TotalHours} hours");
				else
				{
					Console.WriteLine($"Progress: {totalTimeCodedThisWeek.TotalHours}/{goalTime.TotalHours} hours \nCode for another {goalTime.TotalHours - totalTimeCodedThisWeek.TotalHours} hours to reach the weekly goal");
					Console.WriteLine($"To achieve the goal you need to code atleast {(goalTime.TotalHours - totalTimeCodedThisWeek.TotalHours) / daysLeft} hours each day");
				}
				connection.Close();
            }
		}
		internal static List<CodingSession> BuildList(string connectionString, string sqlcommand)
		{
			List<CodingSession> sessions = new List<CodingSession>();
			using (var connection = new SqliteConnection(connectionString))
			{
				connection.Open();
				var command = connection.CreateCommand();
				command.CommandText = sqlcommand;

				SqliteDataReader reader = command.ExecuteReader();
				if (reader.HasRows)
				{
					while (reader.Read())
					{
						sessions.Add(new CodingSession
						{
							Id = reader.GetInt32(0),
							StartTime = DateTime.ParseExact(reader.GetString(1), "dd/MM/yy HH:mm", new CultureInfo("nl-BE")),
							EndTime = DateTime.ParseExact(reader.GetString(2), "dd/MM/yy HH:mm", new CultureInfo("nl-BE")),
							Duration = TimeSpan.ParseExact(reader.GetString(3), @"hh\:mm", new CultureInfo("nl-BE"))
						});
					}
				}
				else
				{
					Console.WriteLine("Database is empty");
				}
				connection.Close();
			}
			return sessions;
		}
		internal static void PrintTable(string connectionString, List<CodingSession> sessions) 
		{
			ConsoleTableBuilder
				.From(sessions)
				.WithTitle("Code Tracker")
				.ExportAndWriteLine();
		}
	}
}
