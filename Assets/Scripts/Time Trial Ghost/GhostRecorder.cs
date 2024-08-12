using UnityEngine;
using System.IO;

public class GhostRecorder : MonoBehaviour
{
	public TimeTrialGhostSO ghost;
	private float timer;
	private float timeValue;
	private string filePath;

	private void Awake()
	{
		filePath = Path.Combine(Application.persistentDataPath, "ghostData.json");

		if (ghost.ghostType == GhostType.Record)
		{
			ghost.ResetData();
			timeValue = 0;
			timer = 0;
		}
	}

	void Update()
	{
		timer += Time.unscaledDeltaTime;
		timeValue += Time.unscaledDeltaTime;

		if (ghost.ghostType == GhostType.Record & timer >= 1 / ghost.recordingFrequency)
		{
			ghost.ghostData.timeStamp.Add(timeValue);
			ghost.ghostData.position.Add(this.transform.position);
			ghost.ghostData.rotation.Add(this.transform.rotation);

			timer = 0;
		}
	}

	public void SaveGhostData()
	{
		string json = JsonUtility.ToJson(ghost.ghostData, true);
		File.WriteAllText(filePath, json);
	}

	public void LoadGhostData()
	{
		if (File.Exists(filePath))
		{
			string json = File.ReadAllText(filePath);
			ghost.ghostData = JsonUtility.FromJson<TimeTrialGhostData>(json);
		}
	}
}
