using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

public abstract class DataReader : MonoBehaviour
{
	public Database database;
	protected StringBuilder stringBuilder;

	public Action<Database> OnDataLoaded;

	protected virtual void Awake()
	{
		stringBuilder = new StringBuilder();
		database = new Database();

		Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
	}

	private void Start()
	{
		LoadData();
	}

	protected void CollectWorldData()
	{
		database.worldData = new LocationTimelineData();
		database.worldData.Location = new Location() { StateCountry = "World", ID = "World", Population = 7800000000 };
		database.worldData.Timeline = new Dictionary<long, DataSet>();

		var dates = database.dates;
		for (int i = 0; i < dates.Count; i++)
		{
			database.worldData.Timeline.Add(dates[i].Timestamp, new DataSet());
		}

		List<string> usedCountries = new List<string>();
		var keys = database.timelineDataByLocation.Keys.ToArray();

		for (int i = 0; i < keys.Length; i++)
		{
			var data = database.timelineDataByLocation[keys[i]];
			var country = data.Location.StateCountry;
			if (usedCountries.Contains(country))
			{
				if (string.IsNullOrEmpty(data.Location.StateProvince) && string.IsNullOrEmpty(data.Location.City))
					continue;

				for (int j = 0; j < dates.Count; j++)
				{
					var time = dates[j].Timestamp;
					if (data.Timeline.ContainsKey(time))
					{
						var cd = data.Timeline[time];
						var d = database.worldData.Timeline[time];
						d.Active += cd.Active;
						d.Confirmed += cd.Confirmed;
						d.Recovered += cd.Recovered;
						d.Deaths += cd.Deaths;
						database.worldData.Timeline[time] = d;
					}
				}
			}
			else
			{
				usedCountries.Add(country);
			}
		}
	}

	protected abstract void LoadData();

	protected int GetSafeIntFromArray(int[] arr, int id)
	{
		if (arr.Length > 0 && arr.Length > id)
			return arr[id];
		return 0;
	}

	protected string GetSafeString(string val)
	{
		if (string.IsNullOrEmpty(val))
			return "";
		return val;
	}

	protected float GetSafeFloat(string val)
	{
		if (string.IsNullOrEmpty(val))
			return 0;

		float value;
		if (float.TryParse(val, out value))
			return value;

		return 0;
	}

	protected int GetSafeInt(string val)
	{
		if (string.IsNullOrEmpty(val))
			return 0;

		int value;
		if (int.TryParse(val, out value))
			return value;

		return 0;
	}
}