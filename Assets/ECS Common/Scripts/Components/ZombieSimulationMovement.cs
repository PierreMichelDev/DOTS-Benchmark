using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct ZombieSimulationMovement : IComponentData
{
	public float3 Direction;
	public float Speed;
}