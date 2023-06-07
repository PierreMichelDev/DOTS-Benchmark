using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct ZombieSimulationUpdateZombieState : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<ZombieSimulationSettings>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		float currentTime = (float)SystemAPI.Time.ElapsedTime;
		var settings = SystemAPI.GetSingleton<ZombieSimulationSettings>();

		float fovDistanceSq = settings.FOVDistance * settings.FOVDistance;
		float fovAngleCos = math.cos(math.radians(settings.FOVAngle));

		var ecb = new EntityCommandBuffer(Allocator.Temp);

		foreach (var (localTransform, movement, zombieState, entity) in
				SystemAPI.Query<LocalTransform, RefRW<ZombieSimulationMovement>, RefRW<ZombieSimulationZombieState>>().WithEntityAccess())
		{
			if (FindNearestHealthyHuman(ref state, localTransform.Position, movement.ValueRO.Direction, fovDistanceSq, fovAngleCos,
					out float3 nearestZombiePosition, out float foundAgentDistanceSq, out Entity foundAgentEntity))
			{
				if (SystemAPI.HasComponent<ZombieSimulationCalmState>(entity))
				{
					ecb.RemoveComponent<ZombieSimulationCalmState>(entity);
					ecb.AddComponent(entity, new ZombieSimulationChaseState());
				}

				movement.ValueRW.Direction = math.normalize(nearestZombiePosition - localTransform.Position);
				movement.ValueRW.Speed = settings.PanicSpeed;
				
				if (foundAgentDistanceSq < settings.AttackDistance * settings.AttackDistance && currentTime > zombieState.ValueRO.NextAttackTime)
				{
					ecb.SetComponent(foundAgentEntity, new URPMaterialPropertyBaseColor { Value = settings.InfectedColor });
					ecb.AddComponent(foundAgentEntity, new ZombieSimulationInfectedState { IncubationEndTime = currentTime + settings.IncubationTime });
					zombieState.ValueRW.NextAttackTime = currentTime + settings.AttackCooldown;
				}
			}
			else if (SystemAPI.HasComponent<ZombieSimulationChaseState>(entity))
			{
				ecb.RemoveComponent<ZombieSimulationChaseState>(entity);
				ecb.AddComponent(entity, new ZombieSimulationCalmState());
			}
		}

		ecb.Playback(state.EntityManager);
		ecb.Dispose();
	}

	[BurstCompile]
	public bool FindNearestHealthyHuman(ref SystemState state, float3 position, float3 direction, float fovDistanceSq, float fovAngleCos,
		out float3 foundAgentPosition, out float foundAgentDistanceSq, out Entity foundAgentEntity)
	{
		float bestDistanceSq = float.MaxValue;
		float3 bestPosition = float3.zero;
		Entity bestEntity = Entity.Null;

		foreach (var (localTransform, entity) in SystemAPI.Query<LocalTransform>()
				.WithAll<ZombieSimulationHumanState>().WithNone<ZombieSimulationInfectedState>().WithEntityAccess())
		{
			float distanceSq = math.distancesq(localTransform.Position, position);
			if (distanceSq > fovDistanceSq) continue;

			float angleCos = math.dot(direction, math.normalize(localTransform.Position - position));
			if (angleCos < fovAngleCos) continue;

			if (distanceSq < bestDistanceSq)
			{
				bestEntity = entity;
				bestPosition = localTransform.Position;
				bestDistanceSq = distanceSq;
			}
		}

		foundAgentPosition = bestPosition;
		foundAgentDistanceSq = bestDistanceSq;
		foundAgentEntity = bestEntity;
		return bestEntity != Entity.Null;
	}
}
