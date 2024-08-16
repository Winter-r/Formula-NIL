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
		RaceManager.Instance.OnPlayerWrongCheckpoint += OnPlayerWrongCheckpoint;
		RaceManager.Instance.OnPlayerDisqualified += OnPlayerDisqualified;

		HideGraphic(penaltyGraphic.graphic);
		HideGraphic(disqualificationGraphic);
	}

	private void OnPlayerWrongCheckpoint(object sender, RaceManager.PenaltyEventArgs e)
	{
		penaltyGraphic.alertText.text = "CORNER CUTTING!";

		if (e.penaltyTime > 0)
		{
			penaltyGraphic.penaltyText.text = $"+{e.penaltyTime} seconds added to final time!";
		}
		else
		{
			penaltyGraphic.penaltyText.text = "Warning!";
		}

		StartCoroutine(ShowGraphic(penaltyGraphic.graphic, 2f));
	}

	private void OnPlayerDisqualified(object sender, EventArgs e)
	{
		// TODO: Disqualify the player
		StartCoroutine(ShowGraphic(disqualificationGraphic, 5f));
	}

	private IEnumerator ShowGraphic(GameObject graphic, float seconds)
	{
		graphic.SetActive(true);
		yield return new WaitForSeconds(seconds);
		graphic.SetActive(false);
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