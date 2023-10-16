using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct AgentFOVResults : IComponentData
{
	public float3 NearestVisibleHealthyPosition;
	public float3 NearestVisibleZombiePosition;
	public bool CanSeeHealthyHuman;
	public bool CanSeeZombie;
	public Entity NearestVisibleHealthyEntity;
}