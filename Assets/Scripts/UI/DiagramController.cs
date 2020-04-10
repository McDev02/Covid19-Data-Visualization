using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

public class DiagramController : MonoBehaviour
{
	[SerializeField] Image barchartPrefab;
	[SerializeField] Transform barchartContainer;

	[SerializeField] UIDIagramLegend diagramLegendPrefab;
	[SerializeField] Transform legendContainer;
	[SerializeField] int legendEntryCount;
	UIDIagramLegend[] legendEntires;

	ComponentPool<Image> barchartPool;
	bool showDelta;
	[SerializeField] Text valueButtonLable;

	DataSet[] lastValues;
	DataCategory category;

	private void Awake()
	{
		barchartPool = new ComponentPool<Image>(barchartContainer, barchartPrefab);
		showDelta = false;

		GenerateLegend();
	}

	public void OnShowDeltaChanged()
	{
		showDelta = !showDelta;
		UpdateBarChart(lastValues);
	}

	void GenerateLegend()
	{
		legendEntires = new UIDIagramLegend[legendEntryCount];
		for (int i = 0; i < legendEntryCount; i++)
		{
			legendEntires[i] = Instantiate(diagramLegendPrefab, legendContainer, false);
		}
	}

	public void UpdateData(Dictionary<long, DataSet> data)
	{
		var values = data.Values.ToArray();

		UpdateBarChart(values);
	}

	public void SelectCategory(DataCategory category)
	{
		this.category = category;
		UpdateBarChart(lastValues);
	}

	private void UpdateLegend(float maxValue)
	{
		valueButtonLable.text = showDelta ? "Delta values" : "Absolute";

		for (int i = 0; i < legendEntryCount; i++)
		{
			int val = (int)(maxValue * ((legendEntryCount - i) / (float)legendEntryCount));
			legendEntires[i].label.text = GetShortNumber(val);
		}
	}

	private string GetShortNumber(int val)
	{
		if (val < 10000)
			return val.ToString();
		else if (val < 100000)
		{
			var v = (val / 1000f).ToString("0.#");
			return $"{v}k";
		}
		else if (val < 1000000)
		{
			var v = (val / 1000);
			return $"{v}k";
		}
		else
		{
			var v = (val / 1000000f).ToString("0.##");
			return $"{v}M";
		}
	}

	private void UpdateBarChart(DataSet[] values)
	{
		lastValues = values;
		barchartPool.Reset();
		float maxValue = 0;

		if (showDelta)
			values = GetDeltaValues(values);

		//Calculate max Value
		for (int i = 0; i < values.Length; i++)
		{
			int val = 0;
			switch (category)
			{
				case DataCategory.Confirmed:
					val = values[i].Confirmed;
					break;
				case DataCategory.Recovered:
					val = values[i].Recovered;
					break;
				case DataCategory.Deaths:
					val = values[i].Deaths;
					break;
				case DataCategory.Active:
					val = values[i].Active;
					break;
			}

			if (maxValue < val)
				maxValue = val;
		}

		//show Value
		for (int i = 0; i < values.Length; i++)
		{
			int val = 0;
			switch (category)
			{
				case DataCategory.Confirmed:
					val = values[i].Confirmed;
					break;
				case DataCategory.Recovered:
					val = values[i].Recovered;
					break;
				case DataCategory.Deaths:
					val = values[i].Deaths;
					break;
				case DataCategory.Active:
					val = values[i].Active;
					break;
			}

			var bar = barchartPool.Get(i);
			bar.fillAmount = val / maxValue;
		}

		UpdateLegend(maxValue);
	}

	private DataSet[] GetDeltaValues(DataSet[] values)
	{
		DataSet[] delta = new DataSet[values.Length - 1];
		for (int i = 0; i < values.Length - 1; i++)
		{
			var a = values[i];
			var b = values[i + 1];
			delta[i] = new DataSet()
			{
				Confirmed = b.Confirmed - a.Confirmed,
				Active = b.Active - a.Active,
				Deaths = b.Deaths - a.Deaths,
				Recovered = b.Recovered - a.Recovered
			};
		}

		return delta;
	}
}