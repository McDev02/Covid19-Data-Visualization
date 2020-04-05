using UnityEngine;
using System.Collections.Generic;
using System;

public class ComponentPool<T> where T : Component
{
	public Transform container;
	public T prefab;
	public bool worldPositionStays;

	List<T> pool = new List<T>();

	public ComponentPool(Transform transform, T indicatorPrefab, bool worldPositionStays = false)
	{
		container = transform;
		prefab = indicatorPrefab;
		this.worldPositionStays = worldPositionStays;
	}

	public T Get(int id)
	{
		T obj;
		if (pool.Count <= id)
		{
			obj = GameObject.Instantiate(prefab, container, worldPositionStays);
			pool.Add(obj);
		}
		else
			obj = pool[id];

		obj.transform.SetParent(container, worldPositionStays);
		obj.gameObject.SetActive(true);
		return obj;
	}

	internal void Reset()
	{
		for (int i = 0; i < pool.Count; i++)
		{
			pool[i].gameObject.SetActive(false);
		}
	}
}