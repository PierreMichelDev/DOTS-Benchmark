using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Transforms;

[BurstCompile]
public partial struct ZombieSimulationUpdateFOVSystem : ISystem
{
	private ProfilerMarker m_Marker;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<ZombieSimulationSettings>();
		state.RequireForUpdate<ZombieSimulationSpatialHashData>();

		m_Marker = new ProfilerMarker("UpdateFOVSystem");
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		m_Marker.Begin();

		var settings = SystemAPI.GetSingleton<ZombieSimulationSettings>();
		float fovAngleCos = math.cos(math.radians(settings.FOVAngle));

		NativeList<SpatialHashEntityData> spatialHashResult = new(Allocator.Temp);
		var spatialHash = SystemAPI.GetSingletonRW<ZombieSimulationSpatialHashData>();

		foreach (var (fovResults, movement, localTransform, entity) in
				SystemAPI.Query<RefRW<ZombieSimulationAgentFOVResults>, ZombieSimulationMovement, LocalTransform>().WithEntityAccess())
		{
			spatialHashResult.Clear();
			fovResults.ValueRW.CanSeeZombie = false;
			fovResults.ValueRW.CanSeeHealthyHuman = false;
			fovResults.ValueRW.NearestVisibleHealthyEntity = Entity.Null;

			float bestHealthyDistanceSq = float.MaxValue;
			float bestZombieDistanceSq = float.MaxValue;

			int2 minCellIndex = spatialHash.ValueRO.Comparer.ComputeCellIndex(localTransform.Position - settings.FOVDistance);
			int2 maxCellIndex = spatialHash.ValueRO.Comparer.ComputeCellIndex(localTransform.Position + settings.FOVDistance);

			float radiusSq = settings.FOVDistance * settings.FOVDistance;
			for (int y = minCellIndex.y; y <= maxCellIndex.y; ++y)
			{
				int minCellFlatIndex = spatialHash.ValueRO.Comparer.ComputeCellFlatIndex(new int2(minCellIndex.x, y));
				int maxCellFlatIndex = minCellFlatIndex + 3;
				int minIndex = spatialHash.ValueRO.CellStartIndices[math.min(minCellFlatIndex, spatialHash.ValueRO.MaxCellIndex)];
				int maxIndex = spatialHash.ValueRO.CellStartIndices[math.min(maxCellFlatIndex, spatialHash.ValueRO.MaxCellIndex)];
				for (int i = minIndex; i < maxIndex; ++i)
				{
					SpatialHashEntityData entityData = spatialHash.ValueRO.Entities[i];
					float distance = math.distancesq(localTransform.Position, entityData.Position);
					if (distance <= radiusSq)
					{
						float angleCos = math.dot(movement.Direction, math.normalize(entityData.Position - localTransform.Position));
						if (angleCos < fovAngleCos) continue;

						float distanceSq = math.distancesq(localTransform.Position, entityData.Position);
						if (distanceSq < bestHealthyDistanceSq && entityData.Type == SpatialHashEntityType.Human)
						{
							fovResults.ValueRW.CanSeeHealthyHuman = true;
							fovResults.ValueRW.NearestVisibleHealthyEntity = entityData.Entity;
							fovResults.ValueRW.NearestVisibleHealthyPosition = entityData.Position;
							bestHealthyDistanceSq = distanceSq;
						}

						if (distanceSq < bestZombieDistanceSq && entityData.Type == SpatialHashEntityType.Zombie)
						{
							fovResults.ValueRW.CanSeeZombie = true;
							fovResults.ValueRW.NearestVisibleZombiePosition = entityData.Position;
							bestZombieDistanceSq = distanceSq;
						}
					}
				}
			}
		}
		m_Marker.End();
	}
}
