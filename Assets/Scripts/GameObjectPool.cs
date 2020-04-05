using UnityEngine;
using System.Collections.Generic;
using System;

public class GameObjectPool
{
	public Transform container;
	public GameObject prefab;

	List<GameObject> pool = new List<GameObject>();

	public GameObjectPool(Transform transform, GameObject indicatorPrefab)
	{
		container = transform;
		prefab = indicatorPrefab;
	}

	public GameObject Get(int id)
	{
		GameObject obj;
		if (pool.Count <= id)
		{
			obj = GameObject.Instantiate(prefab, container);
			pool.Add(obj);
		}
		else
			obj = pool[id];

		obj.transform.parent = container;
		obj.SetActive(true);
		return obj;
	}

	internal void Reset()
	{
		for (int i = 0; i < pool.Count; i++)
		{
			pool[i].SetActive(false);
		}
	}
}