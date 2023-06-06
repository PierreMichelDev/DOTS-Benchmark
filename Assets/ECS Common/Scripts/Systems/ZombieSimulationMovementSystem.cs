using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct ZombieSimulationMovementSystem : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		float deltaTime = SystemAPI.Time.DeltaTime;
		var settings = SystemAPI.GetSingleton<ZombieSimulationSettingsECSEnum>();

		foreach (var (movement, localTransform) in SystemAPI.Query<RefRW<ZombieSimulationMovement>, RefRW<LocalTransform>>())
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
	}
}
