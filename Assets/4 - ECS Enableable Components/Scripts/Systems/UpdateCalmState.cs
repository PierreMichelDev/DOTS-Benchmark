using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[BurstCompile]
public partial struct UpdateCalmStateEnableable : ISystem
{
	private ProfilerMarker m_Marker;
	private Random m_Random;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		m_Random = new Random(1234);
		state.RequireForUpdate<SimulationSettings>();
		state.RequireForUpdate<SimulationECSEnableable>();

		m_Marker = new ProfilerMarker("UpdateCalmState");
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		m_Marker.Begin();

		float currentTime = (float)SystemAPI.Time.ElapsedTime;
		var settings = SystemAPI.GetSingleton<SimulationSettings>();

		foreach (var (calmState, movement)
				in SystemAPI.Query<RefRW<CalmStateEnableable>, RefRW<AgentMovement>>())
		{
			if (currentTime > calmState.ValueRO.NextDirectionChangeTime)
			{
				float2 nextDirection = m_Random.NextFloat2Direction();
				movement.ValueRW.Direction = math.normalize(new float3(nextDirection.x, 0.0f, nextDirection.y));
				movement.ValueRW.Speed = settings.CalmSpeed;
				calmState.ValueRW.NextDirectionChangeTime = currentTime + m_Random.NextFloat(settings.MinDirectionChangeTime, settings.MaxDirectionChangeTime);
			}
		}

		m_Marker.End();
	}
}
