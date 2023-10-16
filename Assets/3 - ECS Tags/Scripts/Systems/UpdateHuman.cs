using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct UpdateHuman : ISystem
{
	private ProfilerMarker m_Marker;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<SimulationSettings>();
		state.RequireForUpdate<SimulationECSTags>();

		m_Marker = new ProfilerMarker("UpdateHumanState");
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		m_Marker.Begin();

		float currentTime = (float)SystemAPI.Time.ElapsedTime;
		var settings = SystemAPI.GetSingleton<SimulationSettings>();

		var ecb = new EntityCommandBuffer(Allocator.Temp);

		foreach (var (localTransform, fovResults, movement, entity) in SystemAPI.Query<LocalTransform, AgentFOVResults, RefRW<AgentMovement>>()
					.WithAll<HumanState>().WithEntityAccess())
		{
			if (fovResults.CanSeeZombie)
			{
				movement.ValueRW.Direction = math.normalize(localTransform.Position - fovResults.NearestVisibleZombiePosition);
				movement.ValueRW.Speed = settings.PanicSpeed;

				ecb.RemoveComponent<CalmState>(entity);
				ecb.AddComponent(entity, new PanicState { PanicEndTime = currentTime + settings.PanicDuration });
			}
		}

		ecb.Playback(state.EntityManager);
		ecb.Dispose();

		m_Marker.End();
	}
}
