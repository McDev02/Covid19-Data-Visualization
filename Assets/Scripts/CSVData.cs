
using System;
using System.Collections.Generic;

public enum DataCategory { Confirmed, Recovered, Deaths, Active }

public class CSVDate
{
	public int Day;
	public int Month;
	public int Year;
	public DateTime DateTime;

	public long Timestamp;

	public override string ToString()
	{
		return $"{Day} / {Month} / {Year}";
	}
}

public class DataSet
{
	public int Confirmed;
	public int Deaths;
	public int Recovered;
	public int Active;
}

public class Location
{
	public string City;
	public string StateProvince;
	public string StateCountry;
	public float Lat;
	public float Long;

	public long Population;

	public string ID;
}

public class LocationTimelineData
{
	public Location Location;
	public Dictionary<long, DataSet> Timeline;
}

public class LocationDataSet
{
	public Location Location;
	public DataSet Data;
}
