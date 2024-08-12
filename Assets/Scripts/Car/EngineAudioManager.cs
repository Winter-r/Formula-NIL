using System.Collections;
using UnityEngine;

public class EngineAudioManager : MonoBehaviour
{
	[Header("Starting Sound")]
	[SerializeField] private AudioSource startingSound;
	private bool isEngineRunning;

	[Header("Idle Sound")]
	[SerializeField] private AudioSource idleSound;
	[SerializeField] private float maxIdleVolume;

	[Header("Running Sound")]
	[SerializeField] private AudioSource runningSound;
	[SerializeField] private float maxRunningVolume;
	[SerializeField] private float maxRunningPitch;

	[Header("Reverse Sound")]
	[SerializeField] private AudioSource reverseSound;
	[SerializeField] private float maxReverseVolume;
	[SerializeField] private float maxReversePitch;

	[Header("Rev Limiter")]
	[SerializeField] private float revLimiterSound = 1f;
	[SerializeField] private float revLimiterFrequency = 3f;
	[SerializeField] private float revLimiterEngage = 0.8f;
	private float revLimiter;

	private float speedRatio;

	private CarLocomotionManager carLocomotionManager;

	private void Awake()
	{
		carLocomotionManager = GetComponent<CarLocomotionManager>();

		idleSound.volume = 0;
		runningSound.volume = 0;
		reverseSound.volume = 0;
	}

	private void Update()
	{
		float speedSign = 0;

		if (carLocomotionManager)
		{
			speedSign = Mathf.Sign(carLocomotionManager.carSpeedRatio);
			speedRatio = Mathf.Abs(carLocomotionManager.carSpeedRatio);
		}

		if (speedRatio > revLimiterEngage)
		{
			revLimiter = (Mathf.Sin(Time.time * revLimiterFrequency) + 1f) * revLimiterSound * (speedRatio - revLimiterEngage);
		}

		if (isEngineRunning)
		{
			idleSound.volume = Mathf.Lerp(0.1f, maxIdleVolume, speedRatio);

			if (speedSign > 0)
			{
				reverseSound.volume = 0;
				runningSound.volume = Mathf.Lerp(0.3f, maxRunningVolume, speedRatio);
				// runningSound.pitch = Mathf.Lerp(0.3f, maxRunningPitch, speedRatio);
			}
			else
			{
				runningSound.volume = 0;
				reverseSound.volume = Mathf.Lerp(0f, maxReverseVolume, speedRatio);
				reverseSound.pitch = Mathf.Lerp(0.2f, maxReversePitch, speedRatio);
			}
		}
		else
		{
			idleSound.volume = 0;
			runningSound.volume = 0;
		}
	}

	public IEnumerator StartEngine()
	{
		startingSound.Play();
		carLocomotionManager.engineStatus = 1;
		yield return new WaitForSeconds(0.6f);
		isEngineRunning = true;
		yield return new WaitForSeconds(0.4f);
		carLocomotionManager.engineStatus = 2;
	}
}
