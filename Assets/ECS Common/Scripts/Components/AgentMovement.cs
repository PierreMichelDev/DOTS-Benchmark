using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct AgentMovement : IComponentData
{
	public float3 Direction;
	public float Speed;
}