using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum RaceType
{
	TimeTrial,
	GrandPrix
}

public enum SectorState
{
	Purple,
	Green,
	Yellow
}

public class RaceManager : MonoBehaviour
{
	// Singleton
	public static RaceManager Instance { get; private set; }

	// Properties
	public List<Transform> CarTransformsList { get; private set; }
	public bool TrialStarted { get; private set; } = false;
	public bool IsTrainingAI = false;

	// Serialized Fields
	[Header("Race Settings")]
	public RaceType raceType;
	[SerializeField] private int numberOfLaps;
	[SerializeField] private int numberOfOpponents;

	[Header("Cars")]
	[SerializeField] private GameObject playerPrefab;
	[SerializeField] private GameObject ghostPrefab;
	[SerializeField] private List<string> carTags;

	[Header("UI")]
	[SerializeField] private List<Image> countdownLights;

	[Header("Spawn Points")]
	[SerializeField] private Transform timeTrialSpawnPoint;
	[SerializeField] private Transform spawnPointsParent;
	[HideInInspector] public List<Transform> spawnPoints = new();
	[Space(5)]

	// Checkpoints
	public List<CheckpointSingle> checkpointList;
	public Dictionary<Transform, int> nextCheckpointIndexDict;
	private int totalMissedCheckpoints;
	private int numberOfWarnings;

	// Sectors
	[SerializeField] private GameObject[] sectorTriggers = new GameObject[3];
	private SectorState[] sectorStates = new SectorState[3];

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
	public Dictionary<Transform, float> carCurrentTimes = new();
	public Dictionary<Transform, float> carPenaltyTimes = new();

	// Events
	public event EventHandler<Transform> OnCorrectCheckpoint;
	public event EventHandler<PenaltyEventArgs> OnWrongCheckpoint;
	public event EventHandler<Transform> OnPlayerDisqualified;
	public static event EventHandler OnLapStarted;
	public static event EventHandler OnLapComplete;

	public class PenaltyEventArgs : EventArgs
	{
		public Transform carTransform;
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

		InitializeSpawnPoints();
		InitializeCarTransformsList();
		InitializeCheckpointLists();
	}

	private void Start()
	{
		if (raceType == RaceType.TimeTrial)
		{
			currentTrialLapTime = 0;
			lastTrialLapTime = 0;
			bestTrialLapTime = PlayerPrefs.GetFloat("BestLapTime", 0);
		}

		foreach (Image light in countdownLights)
		{
			light.enabled = false;
		}

		StartCoroutine(StartCountdown());

		lapTimeText = CarLocomotionManager.FindAndAssignComponent("Current Time Text", lapTimeText);
		bestLapTimeText = CarLocomotionManager.FindAndAssignComponent("Best Lap Time Text", bestLapTimeText);
		lastLapTimeText = CarLocomotionManager.FindAndAssignComponent("Last Lap Time Text", lastLapTimeText);
	}

	private void Update()
	{
		UpdateTrialLapTimeUI();
		UpdateTrialTimer();

		foreach (Transform car in CarTransformsList)
		{
			Debug.Log("Car: " + car.name + " Next Checkpoint: " + nextCheckpointIndexDict[car]);
		}
	}

	public void InitializeCheckpointLists()
	{
		checkpointList = new List<CheckpointSingle>();
		Transform checkpointsTransform = transform.Find("Checkpoints");

		foreach (Transform checkpointSingleTransform in checkpointsTransform)
		{
			CheckpointSingle checkpointSingle = checkpointSingleTransform.GetComponent<CheckpointSingle>();
			checkpointSingle.SetCircuitCheckpoints(this);
			checkpointList.Add(checkpointSingle);
		}

		nextCheckpointIndexDict = new Dictionary<Transform, int>();

		foreach (Transform carTransform in CarTransformsList)
		{
			if (!nextCheckpointIndexDict.ContainsKey(carTransform))
			{
				nextCheckpointIndexDict.Add(carTransform, 0);
			}
		}

		// reset ai checkpoints to 0
		if (IsTrainingAI)
		{
			foreach (Transform carTransform in CarTransformsList)
			{
				nextCheckpointIndexDict[carTransform] = 0;
			}
		}
	}

	public void InitializeCarTransformsList()
	{
		CarTransformsList = new List<Transform>();
		Transform carContainerTransform = GameObject.FindGameObjectWithTag("CarContainer").transform;

		if (raceType == RaceType.TimeTrial)
		{
			GameObject playerCar = Instantiate(playerPrefab, timeTrialSpawnPoint.position, timeTrialSpawnPoint.rotation, carContainerTransform);
			GhostRunner.Instance.recordTarget = playerCar.transform;
		}

		if (raceType == RaceType.GrandPrix)
		{
			for (int i = 0; i < numberOfOpponents; i++)
			{
			}
		}

		for (int i = 0; i < carContainerTransform.childCount; i++)
		{
			Transform carTransform = carContainerTransform.GetChild(i);

			if (carTags.Contains(carTransform.tag) && !CarTransformsList.Contains(carTransform))
			{
				CarTransformsList.Add(carTransform);
			}
		}
	}

	public void InitializeSpawnPoints()
	{
		if (raceType == RaceType.GrandPrix)
		{
			spawnPoints.Clear();

			foreach (Transform spawnPoint in spawnPointsParent)
			{
				spawnPoints.Add(spawnPoint);
			}
		}
	}

	public void StartNewEpisode(Transform carTransform)
	{
		int randomSpawnPointIndex = UnityEngine.Random.Range(0, spawnPoints.Count);
		Transform randomSpawnPoint = spawnPoints[randomSpawnPointIndex];
		carTransform.SetPositionAndRotation(randomSpawnPoint.position, randomSpawnPoint.rotation);
		InitializeCheckpointLists();
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

		if (raceType == RaceType.GrandPrix)
		{
			StartGrandPrix();
		}
	}

	private void StartTrial()
	{
		TrialStarted = true;
	}

	private void StartGrandPrix()
	{
		// Start
	}

	public void CarReachedCheckpoint(CheckpointSingle checkpointSingle, Transform carTransform)
	{
		int nextCheckpointIndex = nextCheckpointIndexDict[carTransform];
		int checkpointIndex = checkpointList.IndexOf(checkpointSingle);

		if (checkpointIndex == nextCheckpointIndex)
		{
			if (checkpointSingle.IsStartFinishLine)
			{
				if (raceType == RaceType.TimeTrial)
				{
					if (startTrialClock)
					{
						LapComplete();
					}
					else
					{
						startTrialClock = true;
					}

					OnLapStarted?.Invoke(this, EventArgs.Empty);
				}
			}

			HandleCorrectCheckpoint(carTransform);
		}
		else if (checkpointIndex > nextCheckpointIndex)
		{
			HandleMissedCheckpoint(carTransform, checkpointIndex, nextCheckpointIndex);
		}
		else
		{
			Debug.Log("Checkpoint already passed.");
		}

		CheckDisqualification(carTransform);
	}

	private void HandleCorrectCheckpoint(Transform carTransform)
	{
		Debug.Log("Checkpoint reached!");
		nextCheckpointIndexDict[carTransform] = (nextCheckpointIndexDict[carTransform] + 1) % checkpointList.Count;
		OnCorrectCheckpoint?.Invoke(this, carTransform);
	}

	private void HandleMissedCheckpoint(Transform carTransform, int checkpointIndex, int nextCheckpointIndex)
	{
		if (raceType == RaceType.TimeTrial)
		{
			DisqualifyPlayer(carTransform);
			return;
		}

		if (checkpointIndex == nextCheckpointIndex) return;

		int missedCheckpoints = checkpointIndex - nextCheckpointIndex;
		totalMissedCheckpoints += missedCheckpoints;

		if (missedCheckpoints >= 2)
		{
			carPenaltyTimes[carTransform] = missedCheckpoints * 3;
		}
		else if (missedCheckpoints == 1)
		{
			numberOfWarnings++;

			if (numberOfWarnings >= 3)
			{
				carPenaltyTimes[carTransform] = missedCheckpoints * 3;
			}
		}

		OnWrongCheckpoint?.Invoke(this, new PenaltyEventArgs
		{
			carTransform = carTransform,
			missedCheckpoints = missedCheckpoints,
			warnings = numberOfWarnings,
			penaltyTime = carPenaltyTimes[carTransform]
		});

		Debug.Log($"Missed {missedCheckpoints} checkpoint(s)!");
		nextCheckpointIndexDict[carTransform] = (checkpointIndex + 1) % checkpointList.Count;
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
		if (carTransform.CompareTag("Player"))
		{
			OnPlayerDisqualified?.Invoke(this, carTransform);
		}
		else
		{
			if (IsTrainingAI)
			{
				return;
			}
			else
			{
				carTransform.GetComponent<CarLocomotionManager>().DisableCar();
			}
		}
	}

	public void ResetCircuit()
	{
		if (raceType == RaceType.TimeTrial)
		{
			ResetTimeTrial();
		}
		else
		{
			ResetGrandPrix();
		}
	}

	private void ResetTimeTrial()
	{
		TrialStarted = false;
		startTrialClock = false;
		currentTrialLapTime = 0;
		nextCheckpointIndexDict.Clear();
		checkpointList.Clear();
		totalMissedCheckpoints = 0;
		numberOfWarnings = 0;
		GhostRunner.Instance.ResetGhost();

		InitializeCheckpointLists();

		foreach (Transform carTransform in CarTransformsList)
		{
			carTransform.SetPositionAndRotation(timeTrialSpawnPoint.position, timeTrialSpawnPoint.rotation);
			carTransform.GetComponent<CarLocomotionManager>().EnableCar();
		}

		StartCoroutine(StartCountdown());
	}

	private void ResetGrandPrix()
	{
		// Reset
	}

	private void UpdateTrialTimer()
	{
		if (startTrialClock) currentTrialLapTime += Time.deltaTime;
	}

	private void LapComplete()
	{
		if (raceType == RaceType.TimeTrial)
		{
			lastTrialLapTime = currentTrialLapTime;

			if (currentTrialLapTime < bestTrialLapTime || bestTrialLapTime == 0)
			{
				bestTrialLapTime = currentTrialLapTime;
				PlayerPrefs.SetFloat("BestLapTime", bestTrialLapTime);
			}

			currentTrialLapTime = 0;

			OnLapComplete?.Invoke(this, EventArgs.Empty);
		}

		Debug.Log("Lap Complete");
	}

	private string FormatTime(string type, float time)
	{
		int minutes = Mathf.FloorToInt(time / 60f);
		int seconds = Mathf.FloorToInt(time % 60f);
		int milliseconds = Mathf.FloorToInt(time * 1000f % 1000F);
		return $"{type}: {minutes:00}:{seconds:00}.{milliseconds:000}";
	}

	private void UpdateTrialLapTimeUI()
	{
		if (lapTimeText)
		{
			lapTimeText.text = FormatTime("Current", currentTrialLapTime);
		}

		if (bestLapTimeText)
		{
			bestLapTimeText.text = FormatTime("Best", bestTrialLapTime);
		}

		if (lastLapTimeText)
		{
			lastLapTimeText.text = FormatTime("Last", lastTrialLapTime);
		}
	}
}
