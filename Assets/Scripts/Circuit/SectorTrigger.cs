using UnityEngine;

public class SectorTrigger : MonoBehaviour
{
	[SerializeField] private int sectorIndex;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			RaceManager.Instance.CarReachedSector(sectorIndex);
		}
	}
}
