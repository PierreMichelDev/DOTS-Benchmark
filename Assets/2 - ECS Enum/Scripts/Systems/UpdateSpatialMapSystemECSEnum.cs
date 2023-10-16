using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Profiling;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct UpdateSpatialMapSystemECSEnum : ISystem
{
	private ProfilerMarker m_Marker;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<SimulationSettings>();
		state.RequireForUpdate<SpatialHashData>();
		state.RequireForUpdate<SimulationECSEnum>();

		m_Marker = new ProfilerMarker("UpdateSpatialMap");
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		m_Marker.Begin();

		var spatialHash = SystemAPI.GetSingletonRW<SpatialHashData>();
		spatialHash.ValueRW.Reset();

		foreach (var (agentState, transform, entity) in SystemAPI.Query<AgentState, LocalTransform>().WithEntityAccess())
		{
			SpatialHashEntityType type = agentState.Health switch
			{
				AgentState.HealthState.Healthy => SpatialHashEntityType.Human,
				AgentState.HealthState.Infected => SpatialHashEntityType.Infected,
				AgentState.HealthState.Zombie => SpatialHashEntityType.Zombie,
				_ => SpatialHashEntityType.Human
			};
			spatialHash.ValueRW.AddEntity(entity, transform.Position, type);
		}
		spatialHash.ValueRW.SortEntities();

		m_Marker.End();
	}
}
