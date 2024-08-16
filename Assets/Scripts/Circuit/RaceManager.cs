using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum RaceType
{
	TimeTrial,
	AIGrandPrix
}

public class RaceManager : MonoBehaviour
{
	// Singleton
	public static RaceManager Instance { get; private set; }

	// Properties
	public List<Transform> CarTransformsList { get; private set; }
	public bool TrialStarted { get; private set; } = false;
	public bool RaceStarted { get; private set; } = false;

	// Serialized Fields
	[Header("Race Settings")]
	[SerializeField] private RaceType raceType;
	[SerializeField] int numberOfLaps;
	[SerializeField] int numberOfOpponents;

	[Header("Cars")]
	[SerializeField] private GameObject playerPrefab;
	[SerializeField] private GameObject ghostPrefab;
	[SerializeField] private GameObject aiPrefab;
	[SerializeField] private List<string> carTags;

	[Header("UI")]
	[SerializeField] private List<Image> countdownLights;

	[Header("Spawn Points")]
	[SerializeField] private Transform timeTrialSpawnPoint;
	[Space(5)]
	[SerializeField] private List<Transform> grandPrixSpawnPoints;

	// Checkpoints
	private List<CheckpointSingle> checkpointList;
	private List<int> nextCheckpointIndexList;
	private int totalMissedCheckpoints = 0;
	private int numberOfWarnings;

	// Time Trial
	private float currentTrialLapTime;
	private float bestTrialLapTime;
	private float lastTrialLapTime;
	private bool startTrialClock;
	
	// Trial UI
	private TMP_Text lapTimeText;
	private TMP_Text bestLapTimeText;
	private TMP_Text lastLapTimeText;

	// GP
	private Dictionary<Transform, float> carCurrentTimes = new();
	private Dictionary<Transform, float> carPenaltyTimes = new();
	private Dictionary<Transform, int> carCurrentLaps = new();
	private float bestLapTime;
	private float playerLastLapTime;

	// Opponents List UI

	// Events
	public event EventHandler<PenaltyEventArgs> OnPlayerWrongCheckpoint;
	public event EventHandler OnPlayerDisqualified;
	public static event EventHandler OnLapStarted;
	public static event EventHandler OnLapComplete;

	public class PenaltyEventArgs : EventArgs
	{
		public int missedCheckpoints;
		public int warnings;
		public float penaltyTime;
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}

		InitializeCarTransformsList();
	}

	private void Start()
	{
		foreach (Image light in countdownLights)
		{
			light.enabled = false;
		}

		StartCoroutine(StartCountdown());
		InitializeCheckpointLists();

		lapTimeText = CarLocomotionManager.FindAndAssignComponent("Lap Time Text", lapTimeText);
		bestLapTimeText = CarLocomotionManager.FindAndAssignComponent("Best Lap Time Text", bestLapTimeText);
		lastLapTimeText = CarLocomotionManager.FindAndAssignComponent("Last Lap Time Text", lastLapTimeText);
	}

	private void Update()
	{
		UpdateTrialLapTimeUI();
		UpdateTrialTimer();
	}

	private void InitializeCheckpointLists()
	{
		checkpointList = new List<CheckpointSingle>();
		Transform checkpointsTransform = transform.Find("Checkpoints");

		foreach (Transform checkpointSingleTransform in checkpointsTransform)
		{
			CheckpointSingle checkpointSingle = checkpointSingleTransform.GetComponent<CheckpointSingle>();
			checkpointSingle.SetCircuitCheckpoints(this);
			checkpointList.Add(checkpointSingle);
		}

		nextCheckpointIndexList = new List<int>();
		foreach (Transform carTransform in CarTransformsList)
		{
			nextCheckpointIndexList.Add(0);
		}
	}

	private void InitializeCarTransformsList()
	{
		CarTransformsList = new List<Transform>();

		Transform carContainerTransform = GameObject.FindGameObjectWithTag("CarContainer").transform;

		GameObject playerCar = Instantiate(playerPrefab, timeTrialSpawnPoint.position, timeTrialSpawnPoint.rotation, carContainerTransform);

		if (raceType == RaceType.TimeTrial)
		{
			GhostRunner.Instance.recordTarget = playerCar.transform;
		}

		if (raceType == RaceType.AIGrandPrix)
		{
			for (int i = 0; i < numberOfOpponents; i++)
			{
				// Randomise the AI spawn points but ensure no cars have the same spawn point
				int randomIndex = UnityEngine.Random.Range(0, grandPrixSpawnPoints.Count);
				Instantiate(aiPrefab, grandPrixSpawnPoints[randomIndex].position, grandPrixSpawnPoints[randomIndex].rotation, carContainerTransform);
				grandPrixSpawnPoints.RemoveAt(randomIndex);
			}
		}

		for (int i = 0; i < carContainerTransform.childCount; i++)
		{
			if (carTags.Contains(carContainerTransform.GetChild(i).tag))
			{
				CarTransformsList.Add(carContainerTransform.GetChild(i));
			}
		}
	}

	private IEnumerator StartCountdown()
	{
		// Show each light one by one
		for (int i = 0; i < countdownLights.Count; i++)
		{
			countdownLights[i].enabled = true;
			yield return new WaitForSeconds(1f); // Wait 1 second between each light
		}

		// Wait for a random time between 0.2 to 3 seconds
		float randomWaitTime = UnityEngine.Random.Range(0.2f, 3f);
		yield return new WaitForSeconds(randomWaitTime);

		// Turn off all lights
		foreach (Image light in countdownLights)
		{
			light.enabled = false;
		}

		if (raceType == RaceType.TimeTrial)
		{
			StartTrial();
		}
		else if (raceType == RaceType.AIGrandPrix)
		{
			StartRace();
		}
	}

	private void StartTrial()
	{
		TrialStarted = true;

		currentTrialLapTime = 0;
		bestTrialLapTime = 0;
		lastTrialLapTime = 0;
	}

	private void StartRace()
	{
		RaceStarted = true;

		foreach (Transform car in CarTransformsList)
		{
			carCurrentTimes.Add(car, 0);
			carCurrentLaps.Add(car, 0);
			carPenaltyTimes.Add(car, 0);
		}
	}

	public void CarReachedCheckpoint(CheckpointSingle checkpointSingle, Transform carTransform)
	{
		int carIndex = CarTransformsList.IndexOf(carTransform);
		int nextCheckpointIndex = nextCheckpointIndexList[carIndex];
		int checkpointIndex = checkpointList.IndexOf(checkpointSingle);

		if (checkpointIndex == nextCheckpointIndex)
		{
			if (checkpointSingle.IsStartFinishLine)
			{
				if (startTrialClock)
				{
					LapComplete(carTransform);
				}
				else
				{
					startTrialClock = true;
				}

				OnLapStarted?.Invoke(this, EventArgs.Empty);
			}

			HandleCorrectCheckpoint(carIndex);
		}
		else if (checkpointIndex > nextCheckpointIndex)
		{
			HandleMissedCheckpoint(carIndex, checkpointIndex, nextCheckpointIndex);
		}
		else
		{
			Debug.Log("Checkpoint already passed.");
		}

		CheckDisqualification(carTransform);
	}

	private void HandleCorrectCheckpoint(int carIndex)
	{
		Transform racer = CarTransformsList[carIndex];
		Debug.Log("Checkpoint reached!");
		nextCheckpointIndexList[carIndex] = (nextCheckpointIndexList[carIndex] + 1) % checkpointList.Count;

		if (raceType == RaceType.AIGrandPrix)
		{
			if (carCurrentLaps[racer] == numberOfLaps)
			{
				carCurrentTimes[racer] += carPenaltyTimes[racer];
			}
		}
	}

	private void HandleMissedCheckpoint(int carIndex, int checkpointIndex, int nextCheckpointIndex)
	{
		Transform racer = CarTransformsList[carIndex];
		int missedCheckpoints = checkpointIndex - nextCheckpointIndex;
		totalMissedCheckpoints += missedCheckpoints;

		if (raceType == RaceType.TimeTrial)
		{
			DisqualifyPlayer(racer);
			return;
		}

		if (missedCheckpoints >= 2)
		{
			carPenaltyTimes[racer] = missedCheckpoints * 3;
		}
		else if (missedCheckpoints == 1)
		{
			numberOfWarnings++;

			if (numberOfWarnings >= 3)
			{
				carPenaltyTimes[racer] = missedCheckpoints * 3;
			}
		}

		OnPlayerWrongCheckpoint?.Invoke(this, new PenaltyEventArgs
		{
			missedCheckpoints = missedCheckpoints,
			warnings = numberOfWarnings,
			penaltyTime = carPenaltyTimes[racer]
		});

		Debug.Log($"Missed {missedCheckpoints} checkpoint(s)!");
		nextCheckpointIndexList[carIndex] = (checkpointIndex + 1) % checkpointList.Count;
	}

	private void CheckDisqualification(Transform carTransform)
	{
		if (totalMissedCheckpoints >= 10)
		{
			DisqualifyPlayer(carTransform);
		}
	}

	private void DisqualifyPlayer(Transform carTransform)
	{
		OnPlayerDisqualified?.Invoke(this, EventArgs.Empty);
		carTransform.gameObject.SetActive(false);
		Debug.Log($"{carTransform.name} disqualified!");
	}

	private void UpdateTrialTimer()
	{
		if (startTrialClock) currentTrialLapTime += Time.deltaTime;
	}

	private void LapComplete(Transform carTransform)
	{
		if (raceType == RaceType.AIGrandPrix)
		{
			carCurrentLaps[carTransform]++;

			if (carTransform.CompareTag("Player"))
			{
				playerLastLapTime = carCurrentTimes[carTransform];
			}

			if (carCurrentTimes[carTransform] < bestLapTime || bestLapTime == 0)
			{
				bestLapTime = carCurrentTimes[carTransform];
			}

			carCurrentTimes[carTransform] = 0;
		}
		else if (raceType == RaceType.TimeTrial)
		{
			lastTrialLapTime = currentTrialLapTime;

			if (currentTrialLapTime < bestTrialLapTime || bestTrialLapTime == 0)
			{
				bestTrialLapTime = currentTrialLapTime;
			}

			currentTrialLapTime = 0;

			OnLapComplete?.Invoke(this, EventArgs.Empty);
		}

		Debug.Log("Lap Complete");
	}

	private string FormatTime(string type, float time)
	{
		int minutes = Mathf.FloorToInt(time / 60F);
		int seconds = Mathf.FloorToInt(time % 60F);
		int milliseconds = Mathf.FloorToInt((time * 1000F) % 1000F);
		return $"{type}:{minutes:00}:{seconds:00}.{milliseconds:000}";
	}

	private void UpdateTrialLapTimeUI()
	{
		if (lapTimeText)
		{
			lapTimeText.text = FormatTime("Lap: ", currentTrialLapTime);
		}

		if (bestLapTimeText)
		{
			bestLapTimeText.text = FormatTime("Best Lap: ", bestTrialLapTime);
		}

		if (lastLapTimeText)
		{
			lastLapTimeText.text = FormatTime("Last Lap: ", lastTrialLapTime);
		}
	}
}
