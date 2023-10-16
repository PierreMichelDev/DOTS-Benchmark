using UnityEngine;
using Unity.Entities;

public struct InfectedState : IComponentData
{
	public float IncubationEndTime;
}