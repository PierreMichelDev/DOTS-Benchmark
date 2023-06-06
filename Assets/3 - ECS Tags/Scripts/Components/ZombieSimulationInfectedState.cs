using UnityEngine;
using Unity.Entities;

public struct ZombieSimulationInfectedState : IComponentData
{
	public float IncubationEndTime;
}