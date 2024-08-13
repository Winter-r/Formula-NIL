using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum RaceType
{
	TimeTrial,
	AIGrandPrix
}

public class RaceManager : MonoBehaviour
{
	public static RaceManager Instance { get; private set; }

	public List<Transform> CarTransformsList { get; private set; }
	public bool RaceStarted { get; private set; } = false;

	[Header("Race Settings")]
	[SerializeField] private RaceType raceType;
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
	[SerializeField] private List<Transform> grandPrixSpawnPoints;

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
			// Start
		}
	}

	private void StartTrial()
	{
		RaceStarted = true;
	}
}
