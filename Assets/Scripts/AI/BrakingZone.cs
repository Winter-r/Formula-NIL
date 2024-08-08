using UnityEngine;

public class BrakingZone : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		AiCarController aiCarController = other.GetComponent<AiCarController>();

		if (aiCarController)
		{
			aiCarController.shouldBrake = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		AiCarController aiCarController = other.GetComponent<AiCarController>();

		if (aiCarController)
		{
			aiCarController.shouldBrake = false;
		}
	}
}
