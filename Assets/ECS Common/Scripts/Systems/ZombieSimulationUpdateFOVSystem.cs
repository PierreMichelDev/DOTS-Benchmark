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

			spatialHash.ValueRO.LookupEntities(localTransform.Position, settings.FOVDistance, ref spatialHashResult);
			foreach (var resultData in spatialHashResult)
			{
				float angleCos = math.dot(movement.Direction, math.normalize(resultData.Position - localTransform.Position));
				if (angleCos < fovAngleCos) continue;

				float distanceSq = math.distancesq(localTransform.Position, resultData.Position);
				if (distanceSq < bestHealthyDistanceSq && resultData.Type == SpatialHashEntityType.Human)
				{
					fovResults.ValueRW.CanSeeHealthyHuman = true;
					fovResults.ValueRW.NearestVisibleHealthyEntity = resultData.Entity;
					fovResults.ValueRW.NearestVisibleHealthyPosition = resultData.Position;
					bestHealthyDistanceSq = distanceSq;
				}

				if (distanceSq < bestZombieDistanceSq && resultData.Type == SpatialHashEntityType.Zombie)
				{
					fovResults.ValueRW.CanSeeZombie = true;
					fovResults.ValueRW.NearestVisibleZombiePosition = resultData.Position;
					bestZombieDistanceSq = distanceSq;
				}
			}
		}
		m_Marker.End();
	}
}
