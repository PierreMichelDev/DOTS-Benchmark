using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct ZombieSimulationUpdateFOVSystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		var settings = SystemAPI.GetSingleton<ZombieSimulationSettingsECSEnum>();
		float fovDistanceSq = settings.FOVDistance * settings.FOVDistance;
		float fovAngleCos = math.cos(math.radians(settings.FOVAngle));

		foreach (var (fovResults, movement, localTransform, entity) in
				SystemAPI.Query<RefRW<ZombieSimulationAgentFOVResults>, ZombieSimulationMovement, LocalTransform>().WithEntityAccess())
		{
			fovResults.ValueRW.CanSeeZombie = false;
			fovResults.ValueRW.CanSeeHealthyHuman = false;
			fovResults.ValueRW.NearestVisibleHealthyEntity = Entity.Null;

			float bestHealthyDistanceSq = float.MaxValue;
			float bestZombieDistanceSq = float.MaxValue;

			//TODO: We could improve this by using a quadtree or something similar
			foreach (var (otherLocalTransform, otherAgentState, otherEntity) in SystemAPI.Query<LocalTransform, ZombieSimulationAgentState>().WithEntityAccess())
			{
				if (entity == otherEntity) continue;

				float distanceSq = math.distancesq(localTransform.Position, otherLocalTransform.Position);
				if (distanceSq > fovDistanceSq) continue;

				float angleCos = math.dot(movement.Direction, math.normalize(otherLocalTransform.Position - localTransform.Position));
				if (angleCos < fovAngleCos) continue;

				if (distanceSq < bestHealthyDistanceSq && otherAgentState.Health == ZombieSimulationAgentState.HealthState.Healthy)
				{
					fovResults.ValueRW.CanSeeHealthyHuman = true;
					fovResults.ValueRW.NearestVisibleHealthyEntity = otherEntity;
					fovResults.ValueRW.NearestVisibleHealthyPosition = otherLocalTransform.Position;
					bestHealthyDistanceSq = distanceSq;
				}

				if (distanceSq < bestZombieDistanceSq && otherAgentState.Health == ZombieSimulationAgentState.HealthState.Zombie)
				{
					fovResults.ValueRW.CanSeeZombie = true;
					fovResults.ValueRW.NearestVisibleZombiePosition = otherLocalTransform.Position;
					bestZombieDistanceSq = distanceSq;
				}
			}
		}
	}
}
