using UnityEngine;

public class CheckpointSingle : MonoBehaviour
{
	public bool IsStartFinishLine;

	private CircuitCheckpointManager circuitCheckpoints;

	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent(out CarLocomotionManager player))
		{
			circuitCheckpoints.CarReachedCheckpoint(this, other.transform);
		}
	}

	public void SetCircuitCheckpoints(CircuitCheckpointManager circuitCheckpoints)
	{
		this.circuitCheckpoints = circuitCheckpoints;
	}
}
