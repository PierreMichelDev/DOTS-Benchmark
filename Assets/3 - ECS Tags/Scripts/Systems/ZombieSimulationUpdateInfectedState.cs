using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Profiling;
using Unity.Rendering;
using UnityEngine;

[BurstCompile]
public partial struct ZombieSimulationUpdateInfectedState : ISystem
{
	private ProfilerMarker m_Marker;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<ZombieSimulationSettings>();
		state.RequireForUpdate<ZombieSimulationECSTags>();

		m_Marker = new ProfilerMarker("UpdateInfectedState");
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		m_Marker.Begin();

		float currentTime = (float)SystemAPI.Time.ElapsedTime;
		var settings = SystemAPI.GetSingleton<ZombieSimulationSettings>();

		var ecb = new EntityCommandBuffer(Allocator.Temp);
		foreach (var (infectedState, color, entity)
				in SystemAPI.Query<ZombieSimulationInfectedState, RefRW<URPMaterialPropertyBaseColor>>().WithEntityAccess())
		{
			if (currentTime > infectedState.IncubationEndTime)
			{
				ecb.RemoveComponent<ZombieSimulationHumanState>(entity);
				ecb.RemoveComponent<ZombieSimulationInfectedState>(entity);
				ecb.AddComponent(entity, new ZombieSimulationZombieState());

				color.ValueRW.Value = settings.ZombieColor;

				//TODO: force calm state ?
			}
		}

		ecb.Playback(state.EntityManager);
		ecb.Dispose();

		m_Marker.End();
	}
}
