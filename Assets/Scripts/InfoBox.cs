using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class InfoBox : MonoBehaviour
{
	[SerializeField] DiagramController diagramController;

	[SerializeField] Text Country;
	[SerializeField] Text State;
	[SerializeField] Text Population;

	[SerializeField] GameObject activeIndicatorActive;
	[SerializeField] GameObject activeIndicatorConfirmed;
	[SerializeField] GameObject activeIndicatorRecovered;
	[SerializeField] GameObject activeIndicatorDeaths;

	[SerializeField] Text HeadlineActive;
	[SerializeField] Text HeadlineConfirmed;
	[SerializeField] Text HeadlineRecovered;
	[SerializeField] Text HeadlineDeaths;

	[SerializeField] Text Active;
	[SerializeField] Text Confirmed;
	[SerializeField] Text Recovered;
	[SerializeField] Text Deaths;

	LocationTimelineData locationData;


	public void SelectCategory(DataCategory category)
	{
		diagramController.SelectCategory(category);

		activeIndicatorActive.SetActive(category == DataCategory.Active);
		activeIndicatorConfirmed.SetActive(category == DataCategory.Confirmed);
		activeIndicatorRecovered.SetActive(category == DataCategory.Recovered);
		activeIndicatorDeaths.SetActive(category == DataCategory.Deaths);
	}

	public void UpdateInfo(LocationTimelineData locationData, long date)
	{
		this.locationData = locationData;

		string format = "N0";
		var location = locationData.Location;
		DataSet dataSet;
		if (locationData.Timeline.ContainsKey(date))
			dataSet = locationData.Timeline[date];
		else
			dataSet = new DataSet();

		Country.text = location.StateCountry;
		State.text = location.StateProvince;
		Population.text = location.Population.ToString(format);

		Active.text = dataSet.Active.ToString(format);
		Confirmed.text = dataSet.Confirmed.ToString(format);
		Recovered.text = dataSet.Recovered.ToString(format);
		Deaths.text = dataSet.Deaths.ToString(format);

		var confirmed = (float)dataSet.Confirmed;

		HeadlineActive.text = $"Active {(confirmed <= 0 ? "0%" : (dataSet.Active / confirmed).ToString("0.00 % "))}";
		HeadlineConfirmed.text = $"Confirmed { (dataSet.Confirmed / (float)location.Population).ToString("0.00%")}";
		HeadlineRecovered.text = $"Recovered {(confirmed <= 0 ? "0%" : (dataSet.Recovered / confirmed).ToString("0.00%"))}";
		HeadlineDeaths.text = $"Deaths {(confirmed <= 0 ? "0%" : (dataSet.Deaths / confirmed).ToString("0.00%"))}";

		diagramController.UpdateData(locationData.Timeline);
	}

	internal void UpdateTime(long timestamp)
	{
		UpdateInfo(locationData, timestamp);
	}
}