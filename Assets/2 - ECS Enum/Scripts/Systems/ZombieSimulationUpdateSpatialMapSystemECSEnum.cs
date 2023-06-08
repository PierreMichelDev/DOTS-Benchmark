using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct ZombieSimulationUpdateSpatialMapSystemECSEnum : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<ZombieSimulationSettings>();
		state.RequireForUpdate<ZombieSimulationSpatialHashData>();
		state.RequireForUpdate<ZombieSimulationECSEnum>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var spatialHash = SystemAPI.GetSingletonRW<ZombieSimulationSpatialHashData>();
		spatialHash.ValueRW.Reset();

		foreach (var (agentState, transform, entity) in SystemAPI.Query<ZombieSimulationAgentState, LocalTransform>().WithEntityAccess())
		{
			SpatialHashEntityType type = agentState.Health switch
			{
				ZombieSimulationAgentState.HealthState.Healthy => SpatialHashEntityType.Human,
				ZombieSimulationAgentState.HealthState.Infected => SpatialHashEntityType.Infected,
				ZombieSimulationAgentState.HealthState.Zombie => SpatialHashEntityType.Zombie,
				_ => SpatialHashEntityType.Human
			};
			spatialHash.ValueRW.AddEntity(entity, transform.Position, type);
		}
	}
}
