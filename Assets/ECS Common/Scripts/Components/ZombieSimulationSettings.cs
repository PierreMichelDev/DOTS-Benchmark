using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct ZombieSimulationSettings : IComponentData
{
	public float3 MinBounds;
	public float3 MaxBounds;
	public int AgentCount;
	public float InfectedRatio;
	public Entity AgentPrefab;
	public Entity InfectedPrefab;

	public float MinDirectionChangeTime;
	public float MaxDirectionChangeTime;
	public float IncubationTime;
	public float PanicDuration;

	public float FOVDistance;
	public float FOVAngle;

	public float AttackDistance;
	public float AttackCooldown;

	public float CalmSpeed;
	public float PanicSpeed;
	public float ChaseSpeed;

	public float4 HealthyColor;
	public float4 InfectedColor;
	public float4 ZombieColor;
}