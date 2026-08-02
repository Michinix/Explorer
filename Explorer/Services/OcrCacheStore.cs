using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Explorer.Models;
using Microsoft.Data.Sqlite;

namespace Explorer.Services;

public sealed class OcrCacheStore : IDisposable
{
	private static readonly string DatabasePath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Explorer", "ocr-cache.db");

	private readonly SqliteConnection _connection;
	private readonly Lock _gate = new();

	public OcrCacheStore()
	{
		Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);

		_connection = new SqliteConnection(new SqliteConnectionStringBuilder
		{
			DataSource = DatabasePath
		}.ToString());

		_connection.Open();

		Execute("PRAGMA journal_mode=WAL;");
		Execute("PRAGMA synchronous=NORMAL;");
		Execute("""
		        CREATE TABLE IF NOT EXISTS OcrCache (
		            NormalizedPath TEXT PRIMARY KEY,
		            FullPath       TEXT    NOT NULL,
		            Length         INTEGER NOT NULL,
		            LastWriteTicks INTEGER NOT NULL,
		            Text           TEXT    NOT NULL
		        );
		        """);
	}

	public void Dispose()
	{
		lock (_gate)
		{
			_connection.Dispose();
		}
	}

	public OcrCacheEntry? TryGet(string filePath)
	{
		try
		{
			lock (_gate)
			{
				using var command = _connection.CreateCommand();

				command.CommandText =
					"SELECT Length, LastWriteTicks, Text FROM OcrCache WHERE NormalizedPath = $path;";
				command.Parameters.AddWithValue("$path", Normalize(filePath));

				using var reader = command.ExecuteReader();

				if (!reader.Read())
					return null;

				return new OcrCacheEntry(reader.GetInt64(0), reader.GetInt64(1), reader.GetString(2));
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
			return null;
		}
	}

	public void Set(string filePath, long length, long lastWriteTicks, string text)
	{
		try
		{
			lock (_gate)
			{
				using var command = _connection.CreateCommand();

				command.CommandText =
					"""
					INSERT INTO OcrCache (NormalizedPath, FullPath, Length, LastWriteTicks, Text)
					VALUES ($path, $fullPath, $length, $ticks, $text)
					ON CONFLICT (NormalizedPath) DO UPDATE SET
					    FullPath       = excluded.FullPath,
					    Length         = excluded.Length,
					    LastWriteTicks = excluded.LastWriteTicks,
					    Text           = excluded.Text;
					""";

				command.Parameters.AddWithValue("$path", Normalize(filePath));
				command.Parameters.AddWithValue("$fullPath", filePath);
				command.Parameters.AddWithValue("$length", length);
				command.Parameters.AddWithValue("$ticks", lastWriteTicks);
				command.Parameters.AddWithValue("$text", text);

				command.ExecuteNonQuery();
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
		}
	}

	public void Remove(string filePath)
	{
		try
		{
			lock (_gate)
			{
				using var command = _connection.CreateCommand();

				command.CommandText = "DELETE FROM OcrCache WHERE NormalizedPath = $path;";
				command.Parameters.AddWithValue("$path", Normalize(filePath));

				command.ExecuteNonQuery();
			}
		}
		catch (Exception ex)
		{
			Debug.WriteLine(ex.Message);
		}
	}

	private static string Normalize(string filePath)
	{
		return filePath.ToUpperInvariant();
	}

	private void Execute(string sql)
	{
		using var command = _connection.CreateCommand();

		command.CommandText = sql;
		command.ExecuteNonQuery();
	}
}