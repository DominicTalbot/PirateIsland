using UnityEngine;

[System.Serializable]
public class MissionData
{
    public string missionName;

    public int reward;

    public int requiredCrew;

    public float duration;

    public Transform destinationPoint;

    public int requiredMainBuildingLevel;
}