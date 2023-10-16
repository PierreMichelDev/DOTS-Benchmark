using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Profiling;
using Unity.Rendering;
using UnityEngine;

[BurstCompile]
public partial struct UpdateInfectedStateEnableable : ISystem
{
	private ProfilerMarker m_Marker;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<SimulationSettings>();
		state.RequireForUpdate<SimulationECSEnableable>();

		m_Marker = new ProfilerMarker("UpdateInfectedState");
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		m_Marker.Begin();

		float currentTime = (float)SystemAPI.Time.ElapsedTime;
		var settings = SystemAPI.GetSingleton<SimulationSettings>();

		foreach (var (infectedState, color, entity)
				in SystemAPI.Query<InfectedStateEnableable, RefRW<URPMaterialPropertyBaseColor>>().WithEntityAccess())
		{
			if (currentTime > infectedState.IncubationEndTime)
			{
				SystemAPI.SetComponentEnabled<HumanStateEnableable>(entity, false);
				SystemAPI.SetComponentEnabled<InfectedStateEnableable>(entity, false);
				SystemAPI.SetComponentEnabled<ZombieStateEnableable>(entity, true);

				color.ValueRW.Value = settings.ZombieColor;

				//TODO: force calm state ?
			}
		}

		m_Marker.End();
	}
}
