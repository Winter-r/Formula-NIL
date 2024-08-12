using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CheckpointUI : MonoBehaviour
{
	[SerializeField] private CircuitCheckpointManager checkpointManager;
	[SerializeField] PenaltyGraphic penaltyGraphic;
	[SerializeField] GameObject disqualificationGraphic;

	private void Start()
	{
		checkpointManager.OnPlayerCorrectCheckpoint += OnPlayerCorrectCheckpoint;
		checkpointManager.OnPlayerWrongCheckpoint += OnPlayerWrongCheckpoint;
		checkpointManager.OnPlayerDisqualified += OnPlayerDisqualified;

		HideGraphic(penaltyGraphic.graphic);
		HideGraphic(disqualificationGraphic);
	}

	private void OnPlayerCorrectCheckpoint(object sender, EventArgs e)
	{
		HideGraphic(penaltyGraphic.graphic);
	}

	private void OnPlayerWrongCheckpoint(object sender, CircuitCheckpointManager.PenaltyEventArgs e)
	{
		penaltyGraphic.alertText.text = "CORNER CUTTING!";

		if (e.missedCheckpoints >= 2)
		{
			penaltyGraphic.penaltyText.text = $"+{e.missedCheckpoints * 3} seconds added to final time!";
		}
		else if (e.missedCheckpoints == 1)
		{
			if (e.warnings >= 3)
			{
				penaltyGraphic.penaltyText.text = $"+{e.missedCheckpoints * 3} seconds added to final time!";
			}
			else
			{
				penaltyGraphic.penaltyText.text = "WARNING!";
			}
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