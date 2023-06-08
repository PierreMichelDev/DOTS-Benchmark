using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[BurstCompile]
public partial struct ZombieSimulationUpdateCalmStateEnableable : ISystem
{
	private Random m_Random;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		m_Random = new Random(1234);
		state.RequireForUpdate<ZombieSimulationSettings>();
		state.RequireForUpdate<ZombieSimulationECSEnableable>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		float currentTime = (float)SystemAPI.Time.ElapsedTime;
		var settings = SystemAPI.GetSingleton<ZombieSimulationSettings>();

		foreach (var (calmState, movement)
				in SystemAPI.Query<RefRW<ZombieSimulationCalmStateEnableable>, RefRW<ZombieSimulationMovement>>())
		{
			if (currentTime > calmState.ValueRO.NextDirectionChangeTime)
			{
				float2 nextDirection = m_Random.NextFloat2Direction();
				movement.ValueRW.Direction = math.normalize(new float3(nextDirection.x, 0.0f, nextDirection.y));
				movement.ValueRW.Speed = settings.CalmSpeed;
				calmState.ValueRW.NextDirectionChangeTime = currentTime + m_Random.NextFloat(settings.MinDirectionChangeTime, settings.MaxDirectionChangeTime);
			}
		}
	}
}
