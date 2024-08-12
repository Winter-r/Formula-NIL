using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField] private CarLocomotionManager carLocomotionManager;
	[SerializeField] private float followSpeed = 5f;

	private CameraView currentView;
	private int cameraIndex = 0;

	private void Start()
	{
		GameObject.FindGameObjectWithTag("Player").TryGetComponent(out carLocomotionManager);
	}

	private void Update()
	{
		if (!carLocomotionManager) return;

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

		if (cameraIndex >= carLocomotionManager.cameraViews.Length)
		{
			cameraIndex = 0;
		}
	}

	private void HandleCameraFollow()
	{
		currentView = carLocomotionManager.cameraViews[cameraIndex];

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
		transform.LookAt(carLocomotionManager.podCameraLookAt);
		transform.position = Vector3.Lerp(transform.position, currentView.viewTransform.position, Time.deltaTime * followSpeed);
	}

	private void DefaultView()
	{
		transform.LookAt(carLocomotionManager.cameraLookAt);
		transform.position = Vector3.Lerp(transform.position, currentView.viewTransform.position, Time.deltaTime * followSpeed);
	}
}
