using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Profiling;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct ZombieSimulationUpdateSpatialMapSystemECSEnableable : ISystem
{
	private ProfilerMarker m_Marker;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<ZombieSimulationSettings>();
		state.RequireForUpdate<ZombieSimulationSpatialHashData>();
		state.RequireForUpdate<ZombieSimulationECSEnableable>();

		m_Marker = new ProfilerMarker("UpdateSpatialMap");
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		m_Marker.Begin();

		var spatialHash = SystemAPI.GetSingletonRW<ZombieSimulationSpatialHashData>();
		spatialHash.ValueRW.Reset();

		foreach (var (transform, entity) in SystemAPI.Query<LocalTransform>().WithAll<ZombieSimulationZombieStateEnableable>().WithEntityAccess())
		{
			spatialHash.ValueRW.AddEntity(entity, transform.Position, SpatialHashEntityType.Zombie);
		}

		foreach (var (transform, entity) in SystemAPI.Query<LocalTransform>().WithAll<ZombieSimulationHumanStateEnableable>().WithEntityAccess())
		{
			bool isInfected = SystemAPI.IsComponentEnabled<ZombieSimulationInfectedStateEnableable>(entity);
			spatialHash.ValueRW.AddEntity(entity, transform.Position, isInfected ? SpatialHashEntityType.Infected : SpatialHashEntityType.Human);
		}
		spatialHash.ValueRW.SortEntities();

		m_Marker.End();
	}
}
