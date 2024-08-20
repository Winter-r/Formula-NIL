using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField] private CarLocomotionManager carLocomotionManager;
	[SerializeField] private float followSpeed = 5f;

	private CameraView currentView;
	private int cameraIndex = 0;

	private Vector3 velocity = Vector3.zero;

	private void Start()
	{
		GameObject.FindGameObjectWithTag("Player").TryGetComponent(out carLocomotionManager);
	}

	private void Update()
	{
		if (!carLocomotionManager) return;

		if (CarInputManager.cameraCycleInput)
		{
			CarInputManager.cameraCycleInput = false;
			CycleCamera();
		}
	}

	private void LateUpdate()
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
		Vector3 desiredPosition = currentView.viewTransform.position;
		transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, followSpeed * Time.deltaTime);
		transform.LookAt(carLocomotionManager.podCameraLookAt);
	}

	private void DefaultView()
	{
		Vector3 desiredPosition = currentView.viewTransform.position;
		transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, followSpeed * Time.deltaTime);
		transform.LookAt(carLocomotionManager.cameraLookAt);
	}
}
