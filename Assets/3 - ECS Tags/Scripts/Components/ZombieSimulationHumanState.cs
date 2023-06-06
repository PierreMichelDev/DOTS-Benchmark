using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct ZombieSimulationHumanState : IComponentData
{
	public float3 NearestVisibleZombiePosition;
	public bool CanSeeZombie;
}