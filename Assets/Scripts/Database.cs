
using System.Collections.Generic;

public class Database
{
	public LocationTimelineData worldData;
	public Dictionary<string, LocationTimelineData> timelineDataByLocation;

	public List<CSVDate> dates;
	public List<Location> locations;
}