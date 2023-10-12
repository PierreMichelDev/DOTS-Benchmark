using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Profiling;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct ZombieSimulationUpdateSpatialMapSystemECSEnum : ISystem
{
	private ProfilerMarker m_Marker;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<ZombieSimulationSettings>();
		state.RequireForUpdate<ZombieSimulationSpatialHashData>();
		state.RequireForUpdate<ZombieSimulationECSEnum>();

		m_Marker = new ProfilerMarker("UpdateSpatialMap");
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		m_Marker.Begin();

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
		spatialHash.ValueRW.SortEntities();

		m_Marker.End();
	}
}
