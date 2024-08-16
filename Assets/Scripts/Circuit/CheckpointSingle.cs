using UnityEngine;

public class CheckpointSingle : MonoBehaviour
{
	public bool IsStartFinishLine;

	private RaceManager raceManager;

	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent(out CarLocomotionManager player))
		{
			raceManager.CarReachedCheckpoint(this, other.transform);
		}
	}

	public void SetCircuitCheckpoints(RaceManager raceManager)
	{
		this.raceManager = raceManager;
	}
}
