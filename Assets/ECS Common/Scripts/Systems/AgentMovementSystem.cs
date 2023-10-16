using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct AgentMovementSystem : ISystem
{
	private ProfilerMarker m_Marker;

	[BurstCompile]
	public void OnCreate(ref SystemState state)
	{
		state.RequireForUpdate<SimulationSettings>();

		m_Marker = new ProfilerMarker("AgentMovementSystem");
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		m_Marker.Begin();

		float deltaTime = SystemAPI.Time.DeltaTime;
		var settings = SystemAPI.GetSingleton<SimulationSettings>();

		foreach (var (movement, localTransform) in SystemAPI.Query<RefRW<AgentMovement>, RefRW<LocalTransform>>())
		{
			float3 nextPosition = localTransform.ValueRO.Position + movement.ValueRO.Direction * movement.ValueRO.Speed * deltaTime;
			bool canMove = true;
			if (nextPosition.x < settings.MinBounds.x || nextPosition.x > settings.MaxBounds.x)
			{
				movement.ValueRW.Direction.x *= -1.0f;
				canMove = false;
			}
			if (nextPosition.z < settings.MinBounds.z || nextPosition.z > settings.MaxBounds.z)
			{
				movement.ValueRW.Direction.z *= -1.0f;
				canMove = false;
			}

			if (canMove)
			{
				localTransform.ValueRW.Position = nextPosition;
			}
		}

		m_Marker.End();
	}
}
