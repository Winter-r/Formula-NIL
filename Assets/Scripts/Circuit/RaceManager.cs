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
	public TimeTrialGhostSO ghost;
	public List<Image> countdownLights;
	// list of tags
	public List<string> carTags;
	public bool raceStarted { get; private set; } = false;
	public GameObject playerPrefab;
	public GameObject ghostPrefab;
	public Transform timeTrialSpawn;

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

		Instantiate(playerPrefab, timeTrialSpawn.position, timeTrialSpawn.rotation, carContainerTransform);
		Instantiate(ghostPrefab, timeTrialSpawn.position, timeTrialSpawn.rotation, carContainerTransform);

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
		float randomWaitTime = Random.Range(0.2f, 3f);
		yield return new WaitForSeconds(randomWaitTime);

		// Turn off all lights
		foreach (Image light in countdownLights)
		{
			light.enabled = false;
		}

		StartRace();
	}

	private void StartRace()
	{
		raceStarted = true;
		// Implement race start logic, such as enabling car controls
		Debug.Log("Race Started!");
	}
}
