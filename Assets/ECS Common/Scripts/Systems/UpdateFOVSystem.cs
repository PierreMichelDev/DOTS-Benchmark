using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Transforms;

[BurstCompile]
public partial struct UpdateFOVSystem : ISystem
{
	private ProfilerMarker m_Marker;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<SimulationSettings>();
		state.RequireForUpdate<SpatialHashData>();

		m_Marker = new ProfilerMarker("UpdateFOVSystem");
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		m_Marker.Begin();

		var settings = SystemAPI.GetSingleton<SimulationSettings>();
		float fovAngleCos = math.cos(math.radians(settings.FOVAngle));
		var spatialHash = SystemAPI.GetSingletonRW<SpatialHashData>();
		

		var job = new UpdateFOVJob
		{
			Settings = settings,
			SpatialHash = spatialHash.ValueRO,
			FovAngleCos = fovAngleCos
		};

		var jobHandle = job.ScheduleParallel(state.Dependency);
		state.Dependency = jobHandle;
		jobHandle.Complete();

		m_Marker.End();
	}

	[BurstCompile]
	public partial struct UpdateFOVJob : IJobEntity
	{
		[ReadOnly] public float FovAngleCos;
		[ReadOnly] public SimulationSettings Settings;
		[ReadOnly] public SpatialHashData SpatialHash;

		[BurstCompile]
		public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref AgentFOVResults fovResults,
			in AgentMovement movement, in LocalTransform localTransform)
		{
			fovResults.CanSeeZombie = false;
			fovResults.CanSeeHealthyHuman = false;
			fovResults.NearestVisibleHealthyEntity = Entity.Null;

			float bestHealthyDistanceSq = float.MaxValue;
			float bestZombieDistanceSq = float.MaxValue;

			int2 minCellIndex = SpatialHash.Comparer.ComputeCellIndex(localTransform.Position - Settings.FOVDistance);
			int2 maxCellIndex = SpatialHash.Comparer.ComputeCellIndex(localTransform.Position + Settings.FOVDistance);

			float radiusSq = Settings.FOVDistance * Settings.FOVDistance;
			for (int y = minCellIndex.y; y <= maxCellIndex.y; ++y)
			{
				int minCellFlatIndex = SpatialHash.Comparer.ComputeCellFlatIndex(new int2(minCellIndex.x, y));
				int maxCellFlatIndex = minCellFlatIndex + 3;
				int minIndex = SpatialHash.CellStartIndices[math.min(minCellFlatIndex, SpatialHash.MaxCellIndex)];
				int maxIndex = SpatialHash.CellStartIndices[math.min(maxCellFlatIndex, SpatialHash.MaxCellIndex)];
				for (int i = minIndex; i < maxIndex; ++i)
				{
					SpatialHashEntityData entityData = SpatialHash.Entities[i];
					float distance = math.distancesq(localTransform.Position, entityData.Position);
					if (distance <= radiusSq)
					{
						float angleCos = math.dot(movement.Direction, math.normalize(entityData.Position - localTransform.Position));
						if (angleCos < FovAngleCos) continue;

						float distanceSq = math.distancesq(localTransform.Position, entityData.Position);
						if (distanceSq < bestHealthyDistanceSq && entityData.Type == SpatialHashEntityType.Human)
						{
							fovResults.CanSeeHealthyHuman = true;
							fovResults.NearestVisibleHealthyEntity = entityData.Entity;
							fovResults.NearestVisibleHealthyPosition = entityData.Position;
							bestHealthyDistanceSq = distanceSq;
						}

						if (distanceSq < bestZombieDistanceSq && entityData.Type == SpatialHashEntityType.Zombie)
						{
							fovResults.CanSeeZombie = true;
							fovResults.NearestVisibleZombiePosition = entityData.Position;
							bestZombieDistanceSq = distanceSq;
						}
					}
				}
			}
		}
	}
}
