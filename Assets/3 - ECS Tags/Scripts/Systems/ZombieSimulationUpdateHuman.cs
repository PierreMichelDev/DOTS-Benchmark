using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct ZombieSimulationUpdateHuman : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		float currentTime = (float)SystemAPI.Time.ElapsedTime;
		var settings = SystemAPI.GetSingleton<ZombieSimulationSettingsECSEnum>();

		float fovDistanceSq = settings.FOVDistance * settings.FOVDistance;
		float fovAngleCos = math.cos(math.radians(settings.FOVAngle));

		var ecb = new EntityCommandBuffer(Allocator.Temp);

		foreach (var (localTransform, movement, entity) in SystemAPI.Query<LocalTransform, RefRW<ZombieSimulationMovement>>()
					.WithAll<ZombieSimulationHumanState>().WithEntityAccess())
		{
			if (FindNearestZombie(ref state, localTransform.Position, movement.ValueRO.Direction, fovDistanceSq, fovAngleCos,
				out float3 nearestZombiePosition))
			{
				movement.ValueRW.Direction = math.normalize(localTransform.Position - nearestZombiePosition);
				movement.ValueRW.Speed = settings.PanicSpeed;

				ecb.RemoveComponent<ZombieSimulationCalmState>(entity);
				ecb.AddComponent(entity, new ZombieSimulationPanicState { PanicEndTime = currentTime + settings.PanicDuration });
			}
		}

		ecb.Playback(state.EntityManager);
		ecb.Dispose();
	}

	[BurstCompile]
	public bool FindNearestZombie(ref SystemState state, float3 position, float3 direction, float fovDistanceSq, float fovAngleCos,
									out float3 foundAgentPosition)
	{
		bool foundZombie = false;
		float bestDistanceSq = float.MaxValue;
		float3 bestPosition = float3.zero;

		foreach (var localTransform in SystemAPI.Query<LocalTransform>().WithAll<ZombieSimulationZombieState>())
		{
			float distanceSq = math.distancesq(localTransform.Position, position);
			if (distanceSq > fovDistanceSq) continue;

			float angleCos = math.dot(direction, math.normalize(localTransform.Position - position));
			if (angleCos < fovAngleCos) continue;

			if (distanceSq < bestDistanceSq)
			{
				foundZombie = true;
				bestPosition = localTransform.Position;
				bestDistanceSq = distanceSq;
			}
		}

		foundAgentPosition = bestPosition;
		return foundZombie;
	}
}
