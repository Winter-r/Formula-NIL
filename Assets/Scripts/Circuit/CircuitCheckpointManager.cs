using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CircuitCheckpointManager : MonoBehaviour
{
	public static CircuitCheckpointManager Instance { get; private set; }

	public event EventHandler OnPlayerCorrectCheckpoint;
	public event EventHandler<PenaltyEventArgs> OnPlayerWrongCheckpoint;
	public event EventHandler OnPlayerDisqualified;

	public class PenaltyEventArgs : EventArgs
	{
		public int missedCheckpoints;
		public int warnings;
	}

	public List<CheckpointSingle> checkpointList;
	public CheckpointSingle startFinishLine;
	public List<int> nextCheckpointIndexList;

	private int totalMissedCheckpoints = 0;
	private int numberOfWarnings;

	[HideInInspector] public float currentLapTime;
	[HideInInspector] public float bestLapTime;
	[HideInInspector] public float lastLapTime;
	[HideInInspector] public bool startClock;

	// Race UI
	private TMP_Text lapTimeText;
	private TMP_Text bestLapTimeText;
	private TMP_Text lastLapTimeText;

	public static event EventHandler OnLapStarted;
	public static event EventHandler OnLapComplete;

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
	}

	private void Start()
	{
		InitializeCheckpointLists();
	}

	private void Update()
	{
		lapTimeText = CarLocomotionManager.FindAndAssignComponent("Lap Time Text", lapTimeText);
		bestLapTimeText = CarLocomotionManager.FindAndAssignComponent("Best Lap Time Text", bestLapTimeText);
		lastLapTimeText = CarLocomotionManager.FindAndAssignComponent("Last Lap Time Text", lastLapTimeText);

		UpdateLapTimeUI();
		UpdateTimer();
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

			if (checkpointSingle.IsStartFinishLine)
			{
				startFinishLine = checkpointSingle;
			}
		}

		nextCheckpointIndexList = new List<int>();
		foreach (Transform carTransform in RaceManager.Instance.CarTransformsList)
		{
			nextCheckpointIndexList.Add(0);
		}
	}

	public void CarReachedCheckpoint(CheckpointSingle checkpointSingle, Transform carTransform)
	{
		int carIndex = RaceManager.Instance.CarTransformsList.IndexOf(carTransform);
		int nextCheckpointIndex = nextCheckpointIndexList[carIndex];
		int checkpointIndex = checkpointList.IndexOf(checkpointSingle);

		if (checkpointIndex == nextCheckpointIndex)
		{
			if (checkpointSingle.IsStartFinishLine)
			{
				if (startClock)
				{
					LapComplete();
				}
				else
				{
					startClock = true;
					StartTimer();
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
		OnPlayerCorrectCheckpoint?.Invoke(this, EventArgs.Empty);
		Debug.Log("Checkpoint reached!");
		nextCheckpointIndexList[carIndex] = (nextCheckpointIndexList[carIndex] + 1) % checkpointList.Count;
	}

	private void HandleMissedCheckpoint(int carIndex, int checkpointIndex, int nextCheckpointIndex)
	{
		int missedCheckpoints = checkpointIndex - nextCheckpointIndex;
		totalMissedCheckpoints += missedCheckpoints;

		if (missedCheckpoints == 1)
		{
			numberOfWarnings++;

			OnPlayerWrongCheckpoint?.Invoke(this, new PenaltyEventArgs
			{
				missedCheckpoints = missedCheckpoints,
				warnings = numberOfWarnings
			});

			if (numberOfWarnings >= 3)
			{
				numberOfWarnings = 0;
			}
		}
		else
		{
			OnPlayerWrongCheckpoint?.Invoke(this, new PenaltyEventArgs
			{
				missedCheckpoints = missedCheckpoints,
				warnings = numberOfWarnings
			});
		}

		Debug.Log($"Missed {missedCheckpoints} checkpoint(s)!");
		nextCheckpointIndexList[carIndex] = (checkpointIndex + 1) % checkpointList.Count;
	}

	private void CheckDisqualification(Transform carTransform)
	{
		if (totalMissedCheckpoints >= 10)
		{
			OnPlayerDisqualified?.Invoke(this, EventArgs.Empty);
			carTransform.gameObject.SetActive(false);
			Debug.Log("Player disqualified!");
		}
	}

	private void StartTimer()
	{
		currentLapTime = 0;
		bestLapTime = 0;
		lastLapTime = 0;
	}

	private void UpdateTimer()
	{
		if (startClock) currentLapTime += Time.deltaTime;
	}

	private void LapComplete()
	{
		lastLapTime = currentLapTime;

		if (currentLapTime < bestLapTime || bestLapTime == 0)
		{
			bestLapTime = currentLapTime;
		}

		currentLapTime = 0;

		OnLapComplete?.Invoke(this, EventArgs.Empty);
		Debug.Log("Lap Complete");
	}

	private string FormatTime(string type, float time)
	{
		int minutes = Mathf.FloorToInt(time / 60F);
		int seconds = Mathf.FloorToInt(time % 60F);
		int milliseconds = Mathf.FloorToInt((time * 1000F) % 1000F);
		return $"{type}:{minutes:00}:{seconds:00}.{milliseconds:000}";
	}

	private void UpdateLapTimeUI()
	{
		if (lapTimeText)
		{
			lapTimeText.text = FormatTime("Lap: ", currentLapTime);
		}

		if (bestLapTimeText)
		{
			bestLapTimeText.text = FormatTime("Best Lap: ", bestLapTime);
		}

		if (lastLapTimeText)
		{
			lastLapTimeText.text = FormatTime("Last Lap: ", lastLapTime);
		}
	}
}
