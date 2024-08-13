using System;
using TarodevGhost;
using UnityEngine;

public class GhostRunner : MonoBehaviour
{
	public static GhostRunner Instance { get; private set; }

	public Transform recordTarget;
	[SerializeField] private GameObject ghostObject;
	[SerializeField, Range(1, 10)] private int recordInterval = 2;

	private ReplaySystem system;

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

		system = new ReplaySystem(this);
	}
	private void OnEnable()
	{
		CircuitCheckpointManager.OnLapStarted += OnLapStarted;
		CircuitCheckpointManager.OnLapComplete += OnLapComplete;
	}

	private void OnDisable()
	{
		CircuitCheckpointManager.OnLapStarted -= OnLapStarted;
		CircuitCheckpointManager.OnLapComplete -= OnLapComplete;
	}

	private void OnLapStarted(object sender, EventArgs e)
	{
		system.StartRun(recordTarget, recordInterval);
	}

	private void OnLapComplete(object sender, EventArgs e)
	{
		system.FinishRun();
		system.PlayRecording(RecordingType.Best, Instantiate(ghostObject));
	}
}

