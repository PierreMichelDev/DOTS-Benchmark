using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Profiling;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct UpdateSpatialMapSystemECSTags : ISystem
{
	private ProfilerMarker m_Marker;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<SimulationSettings>();
		state.RequireForUpdate<SpatialHashData>();
		state.RequireForUpdate<SimulationECSTags>();

		m_Marker = new ProfilerMarker("UpdateSpatialMap");
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		m_Marker.Begin();

		var spatialHash = SystemAPI.GetSingletonRW<SpatialHashData>();
		spatialHash.ValueRW.Reset();

		foreach (var (transform, entity) in SystemAPI.Query<LocalTransform>().WithAll<ZombieState>().WithEntityAccess())
		{
			spatialHash.ValueRW.AddEntity(entity, transform.Position, SpatialHashEntityType.Zombie);
		}

		foreach (var (transform, entity) in SystemAPI.Query<LocalTransform>().WithAll<HumanState>().WithEntityAccess())
		{
			bool isInfected = SystemAPI.HasComponent<InfectedState>(entity);
			spatialHash.ValueRW.AddEntity(entity, transform.Position, isInfected ? SpatialHashEntityType.Infected : SpatialHashEntityType.Human);
		}
		spatialHash.ValueRW.SortEntities();

		m_Marker.End();
	}
}
