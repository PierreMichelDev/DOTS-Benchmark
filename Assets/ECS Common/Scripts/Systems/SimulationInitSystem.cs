using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[BurstCompile]
public partial struct SimulationInitSystem : ISystem
{
	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<SimulationSettings>();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		Random randomGenerator = new Random(1234);
		var settings = SystemAPI.GetSingleton<SimulationSettings>();

		float3 boundsSize = settings.MaxBounds - settings.MinBounds;
		int2 spatialGridSize = new int2((int)(boundsSize.x / settings.FOVDistance), (int)(boundsSize.z / settings.FOVDistance));
		state.EntityManager.CreateSingleton(new SpatialHashData(settings.MinBounds, settings.MaxBounds, spatialGridSize));

		int infectedCount = math.max((int)math.round(settings.AgentCount * settings.InfectedRatio), 1);
		var ecb = new EntityCommandBuffer(Allocator.Temp);
		for (int i = 0; i < settings.AgentCount; ++i)
		{
			bool isInfected = i < infectedCount;
			var newAgent = ecb.Instantiate(isInfected ? settings.InfectedPrefab : settings.AgentPrefab);
			ecb.SetComponent(newAgent, new LocalTransform() { Position = randomGenerator.NextFloat3(settings.MinBounds, settings.MaxBounds), Scale = 0.5f});

			float2 startDirection = randomGenerator.NextFloat2Direction();
			ecb.AddComponent(newAgent, new AgentMovement()
			{
				Direction = math.normalize(new float3(startDirection.x, 0.0f, startDirection.y)),
				Speed = settings.CalmSpeed
			});

			float4 startColor = isInfected ? settings.InfectedColor : settings.HealthyColor;
			ecb.AddComponent(newAgent, new URPMaterialPropertyBaseColor(){ Value = startColor });
		}

		ecb.Playback(state.EntityManager);
		ecb.Dispose();

		state.Enabled = false;
	}
}
