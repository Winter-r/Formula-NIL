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
	public RaceType raceType;
	[SerializeField] private int numberOfLaps;
	[SerializeField] private int numberOfOpponents;

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
	[SerializeField] private Dictionary<Transform, int> nextCheckpointIndexDict;
	private int totalMissedCheckpoints;
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
	private Dictionary<Transform, int> carPositions = new();
	[SerializeField] private List<BracketUI> positionsBracketsUI;

	[Serializable]
	public class BracketUI
	{
		public TMP_Text racerName;
		public TMP_Text racerPosition;
	}

	private float bestLapTime;
	private float playerLastLapTime;

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
		currentTrialLapTime = 0;
		lastTrialLapTime = 0;
		bestTrialLapTime = PlayerPrefs.GetFloat("BestLapTime", 0);

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

		UpdateRacingPositions();

		Debug.Log(nextCheckpointIndexDict[CarTransformsList[0]]);
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

		nextCheckpointIndexDict = new Dictionary<Transform, int>();

		foreach (Transform carTransform in CarTransformsList)
		{
			nextCheckpointIndexDict.Add(carTransform, 0);
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

	private void UpdateRacingPositions()
	{
		foreach (Transform racer in CarTransformsList)
		{
			int position = 1;

			foreach (Transform otherRacer in CarTransformsList)
			{
				if (racer == otherRacer) continue;

				if (carCurrentLaps[racer] > carCurrentLaps[otherRacer])
				{
					position++;
				}
				else if (carCurrentLaps[racer] == carCurrentLaps[otherRacer])
				{
					if (carCurrentTimes[racer] < carCurrentTimes[otherRacer])
					{
						position++;
					}
				}
			}

			carPositions[racer] = position;
		}

		// sort the cars by their positions
		CarTransformsList.Sort((a, b) => carPositions[a].CompareTo(carPositions[b]));

		UpdatePositionUI();
	}

	private void UpdatePositionUI()
	{
		if (raceType == RaceType.TimeTrial)
		{
			// disable the brackets UI
			foreach (BracketUI bracket in positionsBracketsUI)
			{
				SetBracketVisibility(bracket, false);
			}
		}
		else
		{
			// Grand Prix Mode
			int maxVisibleBrackets = Mathf.Min(positionsBracketsUI.Count, CarTransformsList.Count);
			for (int i = 0; i < maxVisibleBrackets; i++)
			{
				Transform carTransform = CarTransformsList[i];
				int position = carPositions[carTransform];
				BracketUI bracket = positionsBracketsUI[i];

				bracket.racerName.text = carTransform.name;
				bracket.racerPosition.text = position + GetOrdinalSuffix(position);
				SetBracketVisibility(bracket, true);
			}

			// Hide unused brackets
			for (int i = maxVisibleBrackets; i < positionsBracketsUI.Count; i++)
			{
				SetBracketVisibility(positionsBracketsUI[i], false);
			}
		}
	}

	private void SetBracketVisibility(BracketUI bracket, bool isVisible)
	{
		// Custom method to handle showing/hiding UI elements in your custom class
		bracket.racerName.gameObject.SetActive(isVisible);
		bracket.racerPosition.gameObject.SetActive(isVisible);
	}

	private string GetOrdinalSuffix(int number)
	{
		if (number % 100 >= 11 && number % 100 <= 13)
		{
			return "th";
		}
		switch (number % 10)
		{
			case 1: return "st";
			case 2: return "nd";
			case 3: return "rd";
			default: return "th";
		}
	}

	public void CarReachedCheckpoint(CheckpointSingle checkpointSingle, Transform carTransform)
	{
		int nextCheckpointIndex = nextCheckpointIndexDict[carTransform];
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

		if (raceType == RaceType.AIGrandPrix)
		{
			if (carCurrentLaps[carTransform] == numberOfLaps)
			{
				carCurrentTimes[carTransform] += carPenaltyTimes[carTransform];
			}
		}
	}

	private void HandleMissedCheckpoint(Transform carTransform, int checkpointIndex, int nextCheckpointIndex)
	{
		if (raceType == RaceType.TimeTrial)
		{
			DisqualifyPlayer(carTransform);
			return;
		}

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

		OnPlayerWrongCheckpoint?.Invoke(this, new PenaltyEventArgs
		{
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
		OnPlayerDisqualified?.Invoke(this, EventArgs.Empty);
		carTransform.GetComponent<CarLocomotionManager>().ResetCar();
		Debug.Log($"{carTransform.name} disqualified!");
	}

	public void ResetCircuit()
	{
		if (raceType == RaceType.TimeTrial)
		{
			ResetTimeTrial();
		}
		else if (raceType == RaceType.AIGrandPrix)
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

		InitializeCheckpointLists();

		foreach (Transform carTransform in CarTransformsList)
		{
			carTransform.gameObject.SetActive(true);
			carTransform.SetPositionAndRotation(timeTrialSpawnPoint.position, timeTrialSpawnPoint.rotation);
		}

		GhostRunner.Instance.ResetGhost();

		StartCoroutine(StartCountdown());
	}

	private void ResetGrandPrix()
	{
		RaceStarted = false;
		bestLapTime = 0;
		playerLastLapTime = 0;
		totalMissedCheckpoints = 0;
		carCurrentLaps.Clear();
		carCurrentTimes.Clear();
		carPenaltyTimes.Clear();
		nextCheckpointIndexDict.Clear();
		InitializeCheckpointLists();

		foreach (Transform carTransform in CarTransformsList)
		{
			carTransform.SetPositionAndRotation(grandPrixSpawnPoints[UnityEngine.Random.Range(0, grandPrixSpawnPoints.Count)].position, grandPrixSpawnPoints[UnityEngine.Random.Range(0, grandPrixSpawnPoints.Count)].rotation);
		}
	}

	private void UpdateTrialTimer()
	{
		if (startTrialClock) currentTrialLapTime += Time.deltaTime;
	}

	private void LapComplete(Transform carTransform)
	{
		if (raceType == RaceType.AIGrandPrix)
		{
			if (carCurrentLaps[carTransform] == numberOfLaps)
			{
				carCurrentTimes[carTransform] += carPenaltyTimes[carTransform];
			}

			if (carTransform.CompareTag("Player"))
			{
				playerLastLapTime = carCurrentTimes[carTransform];
			}

			if (carCurrentTimes[carTransform] < bestLapTime || bestLapTime == 0)
			{
				bestLapTime = carCurrentTimes[carTransform];
			}

			carCurrentLaps[carTransform]++;
			carCurrentTimes[carTransform] = 0;
		}
		else if (raceType == RaceType.TimeTrial)
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
