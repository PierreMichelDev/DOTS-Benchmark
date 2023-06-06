using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct ZombieSimulationZombieState : IComponentData
{
	public float3 NearestVisibleHealthyPosition;
	public bool CanSeeHealthyHuman;
	public Entity NearestVisibleHealthyEntity;

	public float NextAttackTime;
}