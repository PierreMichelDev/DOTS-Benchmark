using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Profiling;
using UnityEngine;

[BurstCompile]
public partial struct UpdatePanicStateEnableable : ISystem
{
	private ProfilerMarker m_Marker;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<SimulationSettings>();
		state.RequireForUpdate<SimulationECSEnableable>();

		m_Marker = new ProfilerMarker("UpdatePanicState");
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		m_Marker.Begin();

		float currentTime = (float)SystemAPI.Time.ElapsedTime;

		foreach (var (panicState, entity) in SystemAPI.Query<PanicStateEnableable>().WithEntityAccess())
		{
			if (currentTime > panicState.PanicEndTime)
			{
				SystemAPI.SetComponentEnabled<PanicStateEnableable>(entity, false);
				SystemAPI.SetComponentEnabled<CalmStateEnableable>(entity, true);
			}
		}

		m_Marker.End();
	}
}
