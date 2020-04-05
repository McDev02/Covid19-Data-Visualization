using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.EventSystems;

public class UIController : MonoBehaviour
{
	[SerializeField] Slider dotScaleSlider;
	[SerializeField] Slider timelineSlider;
	[SerializeField] DataReader dataReader;
	[SerializeField] DataVisualizer dataVisualizer;
	[SerializeField] InfoBox infoBox;
	[SerializeField] Text dateLabel;

	Database db;
	[SerializeField] Camera camera;
	EventSystem eventSystem;

	private void Awake()
	{
		dataReader.OnDataLoaded -= OnDataLoaded;
		dataReader.OnDataLoaded += OnDataLoaded;

		eventSystem = EventSystem.current;

	}

	public void SelectCategory(int val)
	{
		var category = (DataCategory)val;
		infoBox.SelectCategory(category);
		dataVisualizer.SetCategory(category);
	}

	void OnDataLoaded(Database database)
	{
		db = database;
		InitializeUI();

		SelectCategory(0);
	}

	private void InitializeUI()
	{
		SetInfoBox(db.worldData.Location);

		timelineSlider.minValue = 0;
		timelineSlider.maxValue = db.dates.Count - 1;
		timelineSlider.wholeNumbers = true;
		timelineSlider.value = db.dates.Count - 1;

		dotScaleSlider.value = 1;
	}

	private void LateUpdate()
	{
		if (Input.GetMouseButtonDown(0) && !eventSystem.IsPointerOverGameObject())
		{
			RaycastHit hit;
			if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit))
			{
				var indicator = hit.transform.GetComponent<IndicatorObject>();
				if (indicator != null)
				{
					SetInfoBox(indicator.location);
				}
			}
			else
				SetInfoBox(db.worldData.Location);
		}
	}

	void SetInfoBox(Location location)
	{
		var date = db.dates[(int)timelineSlider.value];
		if (db.timelineDataByLocation.ContainsKey(location.ID))
			infoBox.UpdateInfo(db.timelineDataByLocation[location.ID], date.Timestamp);
		else
			infoBox.UpdateInfo(db.worldData, date.Timestamp);
	}

	public void OnTimelineSliderChanged()
	{
		dataVisualizer.SetTimeID((int)timelineSlider.value);
		var date = db.dates[(int)timelineSlider.value];
		infoBox.UpdateTime(date.Timestamp);

		dateLabel.text = date.ToString();
	}
	public void OnDotScaleSliderChanged()
	{
		dataVisualizer.SetDotScale(dotScaleSlider.value);
	}
}