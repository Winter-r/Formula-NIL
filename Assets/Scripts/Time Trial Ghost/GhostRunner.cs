using System;
using TarodevGhost;
using UnityEngine;

public class GhostRunner : MonoBehaviour
{
	public static GhostRunner Instance { get; private set; }

	public Transform recordTarget;
	[SerializeField] private GameObject ghostPrefab;
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
		RaceManager.OnLapStarted += OnLapStarted;
		RaceManager.OnLapComplete += OnLapComplete;
	}

	private void OnDisable()
	{
		RaceManager.OnLapStarted -= OnLapStarted;
		RaceManager.OnLapComplete -= OnLapComplete;
	}

	private void OnLapStarted(object sender, EventArgs e)
	{
		system.PlayRecording(RecordingType.Best, Instantiate(ghostPrefab));
		system.StartRun(recordTarget, recordInterval, 300);
	}

	private void OnLapComplete(object sender, EventArgs e)
	{
		system.FinishRun();
	}

	public void ResetGhost()
	{
		system.StopReplay();
		Destroy(GameObject.Find("F1-Car (Ghost)(Clone)"));
	}
}

