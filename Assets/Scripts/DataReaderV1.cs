using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

public class DataReaderV1 : DataReader
{
	public class CSVTimetableData
	{
		public Location Location;
		public int[] Values;
	}

	const string FILENAME_CONFIRMED = "time_series_covid19_confirmed_global.csv";
	const string FILENAME_RECOVERED = "time_series_covid19_recovered_global.csv";
	const string FILENAME_DEATHS = "time_series_covid19_deaths_global.csv";

	protected override void LoadData()
	{
		database.dates = ReadDates();
		var confirmed = LoadAndReadTimetable(FILENAME_CONFIRMED);
		var recovered = LoadAndReadTimetable(FILENAME_RECOVERED);
		var deaths = LoadAndReadTimetable(FILENAME_DEATHS);

		CollectLocations(new List<CSVTimetableData>[] { confirmed.Values.ToList(), recovered.Values.ToList(), deaths.Values.ToList() });

		//For each country
		int count = database.locations.Count;
		database.timelineDataByLocation = new Dictionary<string, LocationTimelineData>();

		for (int i = 0; i < count; i++)
		{
			var data = new LocationTimelineData();
			var location = database.locations[i].ID;

			int[] confirmedData, recoveredData, deathsData;

			if (confirmed.ContainsKey(location))
				confirmedData = confirmed[location].Values;
			else
				confirmedData = new int[0];

			if (recovered.ContainsKey(location))
				recoveredData = recovered[location].Values;
			else
				recoveredData = new int[0];

			if (deaths.ContainsKey(location))
				deathsData = deaths[location].Values;
			else
				deathsData = new int[0];

			data.Location = database.locations[i];
			data.Timeline = CombineTimelineData(confirmedData, recoveredData, deathsData);
			database.timelineDataByLocation.Add(location, data);
		}

		CollectWorldData();

		OnDataLoaded?.Invoke(database);
	}

	private Dictionary<long, DataSet> CombineTimelineData(int[] confirmedData, int[] recoveredData, int[] deathsData)
	{
		Dictionary<long, DataSet> timeline = new Dictionary<long, DataSet>();

		//if (database.dates.Count != Mathf.Max(Mathf.Max(confirmedData.Length, recoveredData.Length), deathsData.Length)
		//|| confirmedData.Length != recoveredData.Length || confirmedData.Length != deathsData.Length)
		//	Debug.Log($"Data count does not match Dates: {database.dates.Count} Confirmed: {confirmedData.Length}, Recovered: {recoveredData.Length}, Deaths: {deathsData.Length}");

		int count = database.dates.Count;// Mathf.Max(Mathf.Max(confirmedData.Length, recoveredData.Length), deathsData.Length);
		for (int i = 0; i < count; i++)
		{
			timeline.Add(database.dates[i].Timestamp, new DataSet()
			{
				Confirmed = GetSafeIntFromArray(confirmedData, i),
				Recovered = GetSafeIntFromArray(recoveredData, i),
				Deaths = GetSafeIntFromArray(deathsData, i)
			});
		}

		return timeline;
	}

	void CollectLocations(List<CSVTimetableData>[] dataset)
	{
		database.locations = new List<Location>();

		var setCount = dataset.Length;
		for (int i = 0; i < setCount; i++)
		{
			var data = dataset[i];
			for (int j = 0; j < data.Count; j++)
			{
				var location = data[j].Location;
				bool contains = false;
				for (int k = 0; k < database.locations.Count; k++)
				{
					if (database.locations[k].ID == location.ID)
					{
						contains = true;
						break;
					}
				}
				if (!contains)
					database.locations.Add(location);
			}
		}

		Debug.Log($"{database.locations.Count} Locations found");
	}

	Dictionary<string, CSVTimetableData> LoadAndReadTimetable(string filename)
	{
		Dictionary<string, CSVTimetableData> data = new Dictionary<string, CSVTimetableData>();
		string[] lines;
		if (TryReadCSVFile(filename, out lines))
		{
			CSVTimetableData output;
			for (int i = 1; i < lines.Length; i++)
			{
				if (TryReadTimetableData(lines[i], out output))
				{
					data.Add(output.Location.ID, new CSVTimetableData()
					{
						Location = output.Location,
						Values = output.Values
					});
				}
			}
		}

		return data;
	}

	bool TryReadTimetableData(string line, out CSVTimetableData data)
	{
		data = new CSVTimetableData();

		line = FixQoutationContent(line);
		var split = SplitCSVLine(line);

		data.Location = new Location()
		{
			StateProvince = GetSafeString(split[0]),
			StateCountry = GetSafeString(split[1]),
			Lat = GetSafeFloat(split[2]),
			Long = GetSafeFloat(split[3])
		};

		data.Location.ID = $"{data.Location.City},{data.Location.StateCountry},{data.Location.StateProvince}";

		var count = split.Length;
		int[] values = new int[count - 4];
		for (int i = 4; i < count; i++)
		{
			try
			{
				values[i - 4] = int.Parse(split[i]);
			}
			catch (Exception e)
			{
				Debug.LogError(e.ToString());
			}
		}

		data.Values = values;
		return true;
	}

	string FixQoutationContent(string txt)
	{
		stringBuilder.Clear();

		var split = txt.Split('"');
		if (split.Length <= 2)
			return txt;

		for (int i = 0; i < split.Length; i++)
		{
			if (i % 2 == 1)
			{
				var content = split[i];
				content = content.Replace(",", "");
				stringBuilder.Append(content);
			}
			else
				stringBuilder.Append(split[i]);
		}

		return stringBuilder.ToString();
	}

	List<CSVDate> ReadDates()
	{
		List<CSVDate> dates = new List<CSVDate>();

		string[] lines;
		if (TryReadCSVFile(FILENAME_CONFIRMED, out lines))
		{
			var data = SplitCSVLine(lines[0]);

			for (int i = 4; i < data.Length; i++)
			{
				var dateData = SplitCSVDate(data[i]);
				CSVDate date = new CSVDate();

				try
				{
					date.Day = int.Parse(dateData[1]);
					date.Month = int.Parse(dateData[0]);
					date.Year = 2000 + int.Parse(dateData[2]);

					date.DateTime = new DateTime(date.Year, date.Month, date.Day);
					date.Timestamp = long.Parse(date.DateTime.ToString("yyyyMMddHHmmssffff"));

					dates.Add(date);
					//Debug.Log($"{date.Day}, {date.Month} , {date.Year}");
				}
				catch (System.Exception e)
				{
					Debug.LogError($"line {i}: {e.ToString()}");
				}
			}
		}

		return dates;
	}

	bool TryReadCSVFile(string filename, out string[] lines)
	{
		var path = Application.streamingAssetsPath;
		string filepath = Path.Combine(path, filename);

		filepath = filepath.Replace('\\', '/');
		Debug.Log($"Load timeline from path: {filepath}");

		try
		{
			lines = File.ReadAllLines(filepath);
			return true;
		}
		catch (System.Exception e)
		{
			lines = new string[0];
			Debug.LogError(e.ToString());
			return false;
		}
	}

	string[] SplitCSVLine(string line)
	{
		var split = line.Split(',');
		return split;
	}

	string[] SplitCSVDate(string line)
	{
		var split = line.Split('/');
		return split;
	}
}