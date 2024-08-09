using UnityEngine;

[RequireComponent(typeof(CarLocomotionManager))]
public class AiCarController : MonoBehaviour
{
	private CarLocomotionManager carLocomotionManager;
	
	private void Awake()
	{
		carLocomotionManager = GetComponent<CarLocomotionManager>();
	}
}