using UnityEngine;
using Unity.Entities;

public struct ZombieSimulationInfectedStateEnableable : IComponentData, IEnableableComponent
{
	public float IncubationEndTime;
}