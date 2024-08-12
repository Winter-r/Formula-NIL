using UnityEngine;

public class GhostPlayer : MonoBehaviour
{
	public TimeTrialGhostSO ghost;
	private int index1;
	private int index2;
	private float timer;

	private void Update()
	{
		timer += Time.deltaTime;

		if (ghost.ghostType == GhostType.Replay)
		{
			GetIndex();
			SetTransform();
		}
	}

	private void GetIndex()
	{
		for (int i = 0; i < ghost.ghostData.timeStamp.Count; i++)
		{
			if (ghost.ghostData.timeStamp[i] == timer)
			{
				index1 = i;
				index2 = i;

				return;
			}
			else if (ghost.ghostData.timeStamp[i] < timer && timer < ghost.ghostData.timeStamp[i + 1])
			{
				index1 = i;
				index2 = i + 1;

				return;
			}
		}

		index1 = ghost.ghostData.timeStamp.Count - 1;
		index2 = ghost.ghostData.timeStamp.Count - 1;
	}

	private void SetTransform()
	{
		if (index1 == index2)
		{
			transform.position = ghost.ghostData.position[index1];
			transform.rotation = ghost.ghostData.rotation[index1];
		}
		else
		{
			float interpolationFactor = (timer - ghost.ghostData.timeStamp[index1]) / (ghost.ghostData.timeStamp[index2] - ghost.ghostData.timeStamp[index1]);

			transform.position = Vector3.Lerp(ghost.ghostData.position[index1], ghost.ghostData.position[index2], interpolationFactor);
			transform.rotation = Quaternion.Lerp(ghost.ghostData.rotation[index1], ghost.ghostData.rotation[index2], interpolationFactor);
		}
	}
}