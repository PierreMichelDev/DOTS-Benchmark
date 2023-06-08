using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct ZombieSimulationUpdateSpatialMapSystemECSEnableable : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<ZombieSimulationSettings>();
		state.RequireForUpdate<ZombieSimulationSpatialHashData>();
		state.RequireForUpdate<ZombieSimulationECSEnableable>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
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
	}
}