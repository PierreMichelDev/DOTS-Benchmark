using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Profiling;
using Unity.Rendering;
using UnityEngine;

[BurstCompile]
public partial struct UpdateInfectedState : ISystem
{
	private ProfilerMarker m_Marker;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<SimulationSettings>();
		state.RequireForUpdate<SimulationECSTags>();

		m_Marker = new ProfilerMarker("UpdateInfectedState");
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		m_Marker.Begin();

		float currentTime = (float)SystemAPI.Time.ElapsedTime;
		var settings = SystemAPI.GetSingleton<SimulationSettings>();

		var ecb = new EntityCommandBuffer(Allocator.Temp);
		foreach (var (infectedState, color, entity)
				in SystemAPI.Query<InfectedState, RefRW<URPMaterialPropertyBaseColor>>().WithEntityAccess())
		{
			if (currentTime > infectedState.IncubationEndTime)
			{
				ecb.RemoveComponent<HumanState>(entity);
				ecb.RemoveComponent<InfectedState>(entity);
				ecb.AddComponent(entity, new ZombieState());

				color.ValueRW.Value = settings.ZombieColor;

				//TODO: force calm state ?
			}
		}

		ecb.Playback(state.EntityManager);
		ecb.Dispose();

		m_Marker.End();
	}
}
