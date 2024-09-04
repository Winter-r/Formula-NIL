using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CheckpointUI : MonoBehaviour
{
	[SerializeField] private PenaltyGraphic penaltyGraphic;
	[SerializeField] private GameObject disqualificationGraphic;

	private void Start()
	{
		RaceManager.Instance.OnWrongCheckpoint += OnPlayerWrongCheckpoint;
		RaceManager.Instance.OnPlayerDisqualified += OnPlayerDisqualified;

		HideGraphic(penaltyGraphic.graphic);
		HideGraphic(disqualificationGraphic);
	}

	private void OnPlayerWrongCheckpoint(object sender, RaceManager.PenaltyEventArgs e)
	{
		if (!e.carTransform.CompareTag("Player"))
		{
			return;
		}

		penaltyGraphic.alertText.text = "CORNER CUTTING!";

		if (e.penaltyTime > 0)
		{
			penaltyGraphic.penaltyText.text = $"+{e.penaltyTime} seconds added to final time!";
		}
		else
		{
			penaltyGraphic.penaltyText.text = "Warning!";
		}
	}

	private void OnPlayerDisqualified(object sender, Transform carTransform)
	{
		StartCoroutine(DisqualificationGraphic(disqualificationGraphic, 5f, carTransform));
	}

	private IEnumerator DisqualificationGraphic(GameObject graphic, float seconds, Transform carTransform)
	{
		graphic.SetActive(true);
		carTransform.GetComponent<CarLocomotionManager>().DisableCar();
		yield return new WaitForSeconds(seconds);
		graphic.SetActive(false);

		RaceManager.Instance.ResetCircuit();
	}

	private void HideGraphic(GameObject graphic)
	{
		graphic.SetActive(false);
	}
}

[Serializable]
public struct PenaltyGraphic
{
	public GameObject graphic;
	public TextMeshProUGUI alertText;
	public TextMeshProUGUI penaltyText;
}