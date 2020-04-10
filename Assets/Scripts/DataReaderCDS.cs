using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

[ExecuteInEditMode]
public class DataReaderCDS : DataReader
{
	//V1, before early April
	//enum CSVColumns
	//{
	//	city, county, state, country, population, latitude, longitude, url, aggregate, tz, cases, deaths, recovered, active, tested, growthFactor, date
	//}
	//V2, since early April
	enum CSVColumns
	{
		name, level, city, county, state, country, population, latitude, longitude, url, aggregate, tz, cases, deaths, recovered, active, tested, growthFactor, date
	}

	public class CSVTimetableData
	{
		public Location Location;
		public DataSet data;

		public CSVDate date;
	}

#if UNITY_EDITOR
	[SerializeField] public bool autoDownload;
	[SerializeField] DateTime lastDownloadDate;
	static DataReaderCDS instance;
	protected override void Awake()
	{
		base.Awake();
		instance = this;

		if (autoDownload)
		{
			//downlaod new data each 2 hours
			var diff = DateTime.Now - lastDownloadDate;
			if (diff.Hours >= 2)
				DownloadTimelineData();
		}
	}
#endif
	const string FILENAME = "timeseries.csv";

	[UnityEditor.MenuItem("COVID-19/Download Timeline Data from CDS")]
	static void DownloadTimelineData()
	{
		if (instance == null)
			Debug.LogError("Method can only be called from the main scene where a DataReaderCDS component is available!");
		else
			instance.StartCoroutine(TryDownloadFile("https://coronadatascraper.com/timeseries.csv"));
	}

	static IEnumerator TryDownloadFile(string url)
	{
		UnityWebRequest www = new UnityWebRequest(url);
		www.downloadHandler = new DownloadHandlerBuffer();
		yield return www.SendWebRequest();

		if (www.isNetworkError || www.isHttpError)
		{
			Debug.Log(www.error);
		}
		else
		{
			var content = www.downloadHandler.text;

			//Write File
			var path = Application.streamingAssetsPath;
			string filepath = Path.Combine(path, FILENAME);
			filepath = filepath.Replace('\\', '/');

			File.WriteAllText(filepath, content);
			instance.lastDownloadDate = DateTime.Now;
			Debug.Log($"{filepath} successfully updated");
		}
	}

	protected override IEnumerator LoadData()
	{
		var csvData = LoadAndReadTimetable(FILENAME);
		database.dates = ReadDates(csvData);

		CollectLocations(csvData);

		var csvDataByLocation = new Dictionary<string, List<CSVTimetableData>>();

		//Seperate data by Location
		for (int i = 0; i < csvData.Count; i++)
		{
			var d = csvData[i];
			List<CSVTimetableData> list;
			if (!csvDataByLocation.ContainsKey(d.Location.ID))
			{
				list = new List<CSVTimetableData>();
				list.Add(d);
				csvDataByLocation.Add(d.Location.ID, list);
			}
			else
			{
				list = csvDataByLocation[d.Location.ID];
				list.Add(d);
				csvDataByLocation[d.Location.ID] = list;
			}
		}
		yield return null;

		//Sort Data by Time (Might not be necessary)
		var keys = csvDataByLocation.Keys.ToArray();
		for (int i = 0; i < keys.Length; i++)
		{
			var list = csvDataByLocation[keys[i]];
			list.Sort((a, b) => a.date.Timestamp.CompareTo(b.date.Timestamp));
			csvDataByLocation[keys[i]] = list;
		}

		//Fix missing Data
		for (int i = 0; i < keys.Length; i++)
		{
			var list = csvDataByLocation[keys[i]];
			for (int j = 1; j < list.Count; j++)
			{
				var prev = list[j - 1].data;
				var entry = list[j];
				var cur = entry.data;
				if (cur.Confirmed < prev.Confirmed)
					cur.Confirmed = prev.Confirmed;
				if (cur.Recovered < prev.Recovered)
					cur.Recovered = prev.Recovered;
				if (cur.Deaths < prev.Deaths)
					cur.Deaths = prev.Deaths;

				cur.Active = cur.Confirmed - (cur.Recovered + cur.Deaths);
			}
			csvDataByLocation[keys[i]] = list;
		}
		yield return null;

		int yieldCounter = 0;

		//For each country
		database.timelineDataByLocation = new Dictionary<string, LocationTimelineData>();
		int count = database.locations.Count;
		for (int i = 0; i < count; i++)
		{
			var data = new LocationTimelineData();
			var location = database.locations[i].ID;

			if (!csvDataByLocation.ContainsKey(location))
			{
				Debug.LogError($"Location data not found of {location}");
				continue;
			}
			data.Timeline = new Dictionary<long, DataSet>();
			var csvTimetableData = csvDataByLocation[location];

			// AddMissingEntries
			int c = 0;
			CSVTimetableData lastData = null;// new CSVTimetableData();
											 //lastData.data = new DataSet();
			for (int j = 0; j < database.dates.Count; j++)
			{
				var d = csvTimetableData[c];
				var d2 = database.dates[j];
				if (d2.Timestamp == d.date.Timestamp)
				{
					if (csvTimetableData.Count > c + 1)
						c++;
					lastData = d;
					data.Timeline.Add(d.date.Timestamp, d.data);
				}
				else if (lastData != null)
				{
					data.Timeline.Add(d2.Timestamp, lastData.data);
				}

				if (yieldCounter++ > 2000)
				{
					yieldCounter = 0;
					yield return null;
				}
			}

			//for (int t = 0; t < csvTimetableData.Count; t++)
			//{
			//	var d = csvTimetableData[t];
			//	if (data.Timeline.ContainsKey(d.date.Timestamp))
			//		Debug.LogError($"Timeline of Location ({location}) already contains date {d.date}");
			//	//else
			//	data.Timeline.Add(d.date.Timestamp, d.data);
			//}

			data.Location = database.locations[i];
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

	void CollectLocations(List<CSVTimetableData> data)
	{
		database.locations = new List<Location>();

		for (int i = 0; i < data.Count; i++)
		{
			var location = data[i].Location;
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

		Debug.Log($"{database.locations.Count} Locations found");
	}

	List<CSVTimetableData> LoadAndReadTimetable(string filename)
	{
		List<CSVTimetableData> data = new List<CSVTimetableData>();
		string[] lines;

		if (TryReadCSVFile(filename, out lines))
		{
			CSVTimetableData output;
			for (int i = 1; i < lines.Length; i++)
			{
				if (TryReadTimetableData(lines[i], out output))
					data.Add(output);
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
			City = GetSafeString(split[(int)CSVColumns.city]),
			StateProvince = GetSafeString(split[(int)CSVColumns.county]) + GetSafeString(split[(int)CSVColumns.state]),
			StateCountry = GetSafeString(split[(int)CSVColumns.country]),
			Population = GetSafeInt(split[(int)CSVColumns.population]),
			Lat = GetSafeFloat(split[(int)CSVColumns.latitude]),
			Long = GetSafeFloat(split[(int)CSVColumns.longitude])
		};

		data.Location.ID = $"{data.Location.City},{data.Location.StateCountry},{data.Location.StateProvince}";

		data.data = new DataSet()
		{
			Confirmed = GetSafeInt(split[(int)CSVColumns.cases]),
			Deaths = GetSafeInt(split[(int)CSVColumns.deaths]),
			Recovered = GetSafeInt(split[(int)CSVColumns.recovered]),
			Active = GetSafeInt(split[(int)CSVColumns.active])
		};

		data.date = ReadDate(split[(int)CSVColumns.date]);

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

	List<CSVDate> ReadDates(List<CSVTimetableData> data)
	{
		List<CSVDate> dates = new List<CSVDate>();

		for (int i = 0; i < data.Count; i++)
		{
			var d = data[i].date;
			bool found = false;
			for (int j = 0; j < dates.Count; j++)
			{
				if (dates[j].Timestamp == d.Timestamp)
				{
					found = true;
					break;
				}
			}
			if (!found)
				dates.Add(d);
		}

		//Sort
		dates.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));

		return dates;
	}

	CSVDate ReadDate(string txt)
	{
		CSVDate date = new CSVDate();
		var split = txt.Split('-');
		if (split.Length != 3)
		{
			Debug.LogError($"Date could not be read! {txt}");
			return date;
		}

		date.Day = int.Parse(split[2]);
		date.Month = int.Parse(split[1]);
		date.Year = int.Parse(split[0]);

		date.DateTime = new DateTime(date.Year, date.Month, date.Day);
		date.Timestamp = long.Parse(date.DateTime.ToString("yyyyMMddHHmmssffff"));

		return date;
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