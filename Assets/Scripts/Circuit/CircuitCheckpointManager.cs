using System;
using System.Collections.Generic;
using UnityEngine;

public class CircuitCheckpointManager : MonoBehaviour
{
	public event EventHandler OnPlayerCorrectCheckpoint;
	public event EventHandler<PenaltyEventArgs> OnPlayerWrongCheckpoint;
	public event EventHandler OnPlayerDisqualified;

	public class PenaltyEventArgs : EventArgs
	{
		public int missedCheckpoints;
		public int warnings;
	}

	private List<CheckpointSingle> checkpointList;
	private List<int> nextCheckpointIndexList;

	private int totalMissedCheckpoints = 0;
	private int numberOfWarnings;

	private void Start()
	{
		InitializeCheckpointLists();
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
}
