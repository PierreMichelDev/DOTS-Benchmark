using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct UpdateHumanEnableable : ISystem
{
	private ProfilerMarker m_Marker;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<SimulationSettings>();
		state.RequireForUpdate<SimulationECSEnableable>();

		m_Marker = new ProfilerMarker("UpdateHumanState");
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		m_Marker.Begin();

		float currentTime = (float)SystemAPI.Time.ElapsedTime;
		var settings = SystemAPI.GetSingleton<SimulationSettings>();

		foreach (var (localTransform, fovResults, movement, entity) in
					SystemAPI.Query<LocalTransform, AgentFOVResults, RefRW<AgentMovement>>()
					.WithAll<HumanStateEnableable>().WithEntityAccess())
		{
			if (fovResults.CanSeeZombie)
			{
				movement.ValueRW.Direction = math.normalize(localTransform.Position - fovResults.NearestVisibleZombiePosition);
				movement.ValueRW.Speed = settings.PanicSpeed;

				SystemAPI.SetComponent(entity, new PanicStateEnableable{ PanicEndTime = currentTime + settings.PanicDuration});
				SystemAPI.SetComponentEnabled<CalmStateEnableable>(entity, false);
				SystemAPI.SetComponentEnabled<PanicStateEnableable>(entity, true);
			}
		}

		m_Marker.End();
	}
}
