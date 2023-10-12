using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Profiling;
using UnityEngine;

[BurstCompile]
public partial struct ZombieSimulationUpdatePanicState : ISystem
{
	private ProfilerMarker m_Marker;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<ZombieSimulationSettings>();
		state.RequireForUpdate<ZombieSimulationECSTags>();

		m_Marker = new ProfilerMarker("UpdatePanicState");
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		m_Marker.Begin();

		float currentTime = (float)SystemAPI.Time.ElapsedTime;

		var ecb = new EntityCommandBuffer(Allocator.Temp);
		foreach (var (panicState, entity) in SystemAPI.Query<ZombieSimulationPanicState>().WithEntityAccess())
		{
			if (currentTime > panicState.PanicEndTime)
			{
				ecb.RemoveComponent<ZombieSimulationPanicState>(entity);
				ecb.AddComponent(entity, new ZombieSimulationCalmState());
			}
		}

		ecb.Playback(state.EntityManager);
		ecb.Dispose();

		m_Marker.End();
	}
}
