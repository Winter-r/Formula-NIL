using UnityEngine;

[System.Serializable]
public class CameraView
{
	public ViewType viewType;
	public Transform viewTransform;
}

public enum ViewType
{
	Regular,
	Far,
	Pod
}

public class CameraController : MonoBehaviour
{
	[SerializeField] private CarLocomotionManager carLocomotionManager;
	[SerializeField] private Transform cameraLookAt;
	[SerializeField] private Transform podCameraLookAt;
	[SerializeField] private CameraView[] cameraViews;
	[SerializeField] private float followSpeed = 5f;

	private CameraView currentView;
	private int cameraIndex = 0;

	private void Update()
	{
		if (InputManager.cameraCycleInput)
		{
			InputManager.cameraCycleInput = false;
			CycleCamera();
		}
	}

	private void FixedUpdate()
	{
		HandleCameraFollow();
	}

	private void CycleCamera()
	{
		cameraIndex++;

		if (cameraIndex >= cameraViews.Length)
		{
			cameraIndex = 0;
		}
	}

	private void HandleCameraFollow()
	{
		currentView = cameraViews[cameraIndex];

		switch (currentView.viewType)
		{
			case ViewType.Regular:
				HandleRegularView();
				break;
			case ViewType.Far:
				HandleFarView();
				break;
			case ViewType.Pod:
				HandlePodView();
				break;
		}
	}

	private void HandleRegularView()
	{
		DefaultView();
	}

	private void HandleFarView()
	{
		DefaultView();
	}

	private void HandlePodView()
	{
		transform.LookAt(podCameraLookAt);
		transform.position = Vector3.Lerp(transform.position, currentView.viewTransform.position, Time.deltaTime * followSpeed);
	}

	private void DefaultView()
	{
		transform.LookAt(cameraLookAt);
		transform.position = Vector3.Lerp(transform.position, currentView.viewTransform.position, Time.deltaTime * followSpeed);
	}
}
