using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
	[SerializeField] Camera camera;
	[SerializeField] Transform planet;

	[SerializeField] float rotationSpeed;
	[SerializeField] float zoomSpeed;

	[SerializeField] float minZoom;
	[SerializeField] float maxZoom;

	float deltaRotX;
	float deltaRotY;
	float deltaZoom;

	float fov;

	Vector3 lastMousePos;

	[SerializeField] float offset;
	Matrix4x4 originalMatrix;

	private void Start()
	{
		fov = camera.fieldOfView;
		lastMousePos = Input.mousePosition;

		originalMatrix = camera.projectionMatrix;
		UpdateMatrix(fov);
	}

	void UpdateMatrix(float newfov)
	{
		fov = newfov;
		camera.projectionMatrix = originalMatrix;

		var ratio = Screen.height / (float)Screen.width;
		var p = originalMatrix;
		p.m02 += offset;
		p[1, 1] = 1f / Mathf.Tan(fov / (2f * Mathf.Rad2Deg));
		p[0, 0] = p[1, 1] * ratio;
		camera.projectionMatrix = p;
	}

	void Update()
	{
		UpdateInput();

		planet.Rotate(Vector3.up, deltaRotX);
		planet.Rotate(camera.transform.right, deltaRotY, Space.World);

		if (Mathf.Abs(deltaZoom) > 0.01f)
			UpdateMatrix(Mathf.Clamp(fov + deltaZoom * zoomSpeed, minZoom, maxZoom));
		//	camera.fieldOfView = Mathf.Clamp(camera.fieldOfView + deltaZoom * zoomSpeed, minZoom, maxZoom);

		lastMousePos = Input.mousePosition;
	}

	void UpdateInput()
	{
		if (Input.GetMouseButton(1))
		{
			var diff = Input.mousePosition - lastMousePos;
			deltaRotX = rotationSpeed * -diff.x * Time.deltaTime;
			deltaRotY = rotationSpeed * diff.y * Time.deltaTime;
		}
		else
		{
			deltaRotX = deltaRotY = 0;
		}

		deltaZoom = -Input.mouseScrollDelta.y;

		var zoomFactor = camera.fieldOfView / maxZoom;

		deltaRotX *= zoomFactor;
		deltaRotY *= zoomFactor;
	}
}