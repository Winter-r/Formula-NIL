using System;
using System.Collections.Generic;
using UnityEngine;

public enum GhostType
{
	Record,
	Replay
}

[CreateAssetMenu]
public class TimeTrialGhostSO : ScriptableObject
{
	public TimeTrialGhostData ghostData;

	public GhostType ghostType;

	public float recordingFrequency;

	public void ResetData()
	{
		ghostData.timeStamp = new List<float>();
		ghostData.position = new List<Vector3>();
		ghostData.rotation = new List<Quaternion>();
	}
}

[Serializable]
public class TimeTrialGhostData
{
	public List<float> timeStamp;
	public List<Vector3> position;
	public List<Quaternion> rotation;
}