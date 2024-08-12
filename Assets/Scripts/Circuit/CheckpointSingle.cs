using UnityEngine;

public class CheckpointSingle : MonoBehaviour
{
	private CircuitCheckpointManager circuitCheckpoints;

	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent<CarLocomotionManager>(out CarLocomotionManager player))
		{
			circuitCheckpoints.CarReachedCheckpoint(this, other.transform);
		}
	}

	public void SetCircuitCheckpoints(CircuitCheckpointManager circuitCheckpoints)
	{
		this.circuitCheckpoints = circuitCheckpoints;
	}
}
