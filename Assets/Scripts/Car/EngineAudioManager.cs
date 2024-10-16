using UnityEngine;
using FMODUnity;
using System.Collections;

public class EngineAudioManager : MonoBehaviour
{
	private CarLocomotionManager carLocomotionManager;
	[SerializeField] private StudioEventEmitter engineSoundEmitter;

	private void Awake()
	{
		carLocomotionManager = GetComponent<CarLocomotionManager>();

		carLocomotionManager.OnGearChanged += PlayGearChangeSound;
	}

	private void Update()
	{
		float speed = carLocomotionManager.carSpeedRatio;
		float maxRPM = carLocomotionManager.redLine;
		float minRPM = carLocomotionManager.idleRpm;

		float rpm = Mathf.Lerp(minRPM, maxRPM, speed);

		engineSoundEmitter.SetParameter("RPM", rpm);
	}

	public IEnumerator StartEngine()
	{
		carLocomotionManager.engineStatus = 1;
		yield return new WaitForSeconds(1f);
		carLocomotionManager.engineStatus = 2;
	}

	private void PlayGearChangeSound(int currentGear)
	{
		// TODO: Play gear change sound
	}
}
