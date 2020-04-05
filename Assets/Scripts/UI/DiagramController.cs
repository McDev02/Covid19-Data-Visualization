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

	DataSet[] lastValues;
	DataCategory category;

	private void Awake()
	{
		barchartPool = new ComponentPool<Image>(barchartContainer, barchartPrefab);
		GenerateLegend();
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

		UpdateLegend(maxValue);

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
	}
}