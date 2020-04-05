using UnityEngine;
using System.Collections.Generic;
using System;

public class DataVisualizer : MonoBehaviour
{
	[SerializeField] DataReader dataReader;
	Database db;

	[SerializeField] float radius = 1;
	[SerializeField] IndicatorObject indicatorPrefab;

	DataCategory visualizationData;

	float dotScaleFactor = 1;
	int timeID;

	public float maxConfirmedCount;
	public float maxRecoveredCount;
	public float maxDeathsCount;
	public float maxActiveCount;
	public float minSize, maxSize;

	ComponentPool<IndicatorObject> indicatorPool;

	private void Awake()
	{
		dataReader.OnDataLoaded -= OnDataLoaded;
		dataReader.OnDataLoaded += OnDataLoaded;

		indicatorPool = new ComponentPool<IndicatorObject>(transform, indicatorPrefab);
	}

	internal void SetCategory(DataCategory category)
	{
		visualizationData = category;
		UpdateVisualization();
	}

	void OnDataLoaded(Database database)
	{
		db = database;
		CalculateMaxInfectedCount();
	}

	public void UpdateVisualization()
	{
		UpdateCases(timeID, visualizationData);
	}

	void CalculateMaxInfectedCount()
	{
		maxConfirmedCount = 0;
		maxRecoveredCount = 0;
		maxDeathsCount = 0;
		maxActiveCount = 0;
		var dates = db.dates;
		var data = db.timelineDataByLocation.Values;

		foreach (var timeline in data)
		{
			var count = dates.Count;
			for (int i = 0; i < count; i++)
			{
				if (timeline.Timeline.ContainsKey(dates[i].Timestamp))
				{
					var d = timeline.Timeline[dates[i].Timestamp];

					if (maxConfirmedCount < d.Confirmed)
						maxConfirmedCount = d.Confirmed;
					if (maxRecoveredCount < d.Recovered)
						maxRecoveredCount = d.Recovered;
					if (maxDeathsCount < d.Deaths)
						maxDeathsCount = d.Deaths;
					if (maxActiveCount < d.Active)
						maxActiveCount = d.Active;
				}
			}
		}
	}

	internal void SetDotScale(float value)
	{
		dotScaleFactor = value;
		UpdateVisualization();
	}
	internal void SetTimeID(int value)
	{
		timeID = value;
		UpdateVisualization();
	}

	void UpdateCases(int t, DataCategory category)
	{
		var date = db.dates[t];
		var values = db.timelineDataByLocation.Values;

		float minValue = float.MaxValue;
		float maxValue = 0;

		//foreach (var timeline in values)
		//{
		//	var location = timeline.Location;
		//	var data = timeline.Timeline[date.Timestamp];
		//
		//	if (minValue > data.Confirmed && data.Confirmed > 0)
		//		minValue = data.Confirmed;
		//	if (maxValue < data.Confirmed)
		//		maxValue = data.Confirmed;
		//}
		minValue = 0;
		switch (category)
		{
			case DataCategory.Confirmed:
				maxValue = maxConfirmedCount;
				break;
			case DataCategory.Recovered:
				maxValue = maxRecoveredCount;
				break;
			case DataCategory.Deaths:
				maxValue = maxDeathsCount;
				break;
			case DataCategory.Active:
				maxValue = maxActiveCount;
				break;
			default:
				break;
		}

		indicatorPool.Reset();
		int i = 0;
		foreach (var timeline in values)
		{
			var location = timeline.Location;
			if (!timeline.Timeline.ContainsKey(date.Timestamp))
				continue;

			var data = timeline.Timeline[date.Timestamp];
			float value = 0;
			switch (category)
			{
				case DataCategory.Confirmed:
					value = data.Confirmed;
					break;
				case DataCategory.Recovered:
					value = data.Recovered;
					break;
				case DataCategory.Deaths:
					value = data.Deaths;
					break;
				case DataCategory.Active:
					value = data.Active;
					break;
				default:
					break;
			}

			if (value <= 0)
				continue;

			var indicator = indicatorPool.Get(i++);

			indicator.location = timeline.Location;
			indicator.transform.localPosition = GeoToVector(location.Lat, location.Long, radius);
			value = (value - minValue) / (maxValue - minValue);
			indicator.transform.localScale = Vector3.one * (dotScaleFactor * Mathf.Lerp(minSize, maxSize, Mathf.Sqrt(value)));
		}
	}

	Vector3 GeoToVector(float lat, float lon, float radius = 1)
	{
		lat *= Mathf.Deg2Rad;
		lon *= Mathf.Deg2Rad;

		Vector3 pos = Vector3.zero;
		pos.x = Mathf.Cos(lat) * Mathf.Cos(lon);
		pos.z = Mathf.Cos(lat) * Mathf.Sin(lon);
		pos.y = Mathf.Sin(lat);

		pos = pos.normalized * radius;

		return pos;
	}
}