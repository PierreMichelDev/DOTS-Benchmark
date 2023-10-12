using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Profiling;
using UnityEngine;

[BurstCompile]
public partial struct ZombieSimulationUpdatePanicStateEnableable : ISystem
{
	private ProfilerMarker m_Marker;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<ZombieSimulationSettings>();
		state.RequireForUpdate<ZombieSimulationECSEnableable>();

		m_Marker = new ProfilerMarker("UpdatePanicState");
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		m_Marker.Begin();

		float currentTime = (float)SystemAPI.Time.ElapsedTime;

		foreach (var (panicState, entity) in SystemAPI.Query<ZombieSimulationPanicStateEnableable>().WithEntityAccess())
		{
			if (currentTime > panicState.PanicEndTime)
			{
				SystemAPI.SetComponentEnabled<ZombieSimulationPanicStateEnableable>(entity, false);
				SystemAPI.SetComponentEnabled<ZombieSimulationCalmStateEnableable>(entity, true);
			}
		}

		m_Marker.End();
	}
}
