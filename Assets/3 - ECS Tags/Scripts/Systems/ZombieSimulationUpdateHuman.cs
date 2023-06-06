using System;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public partial struct ZombieSimulationUpdateHuman : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		//TODO: Check FOV for zombies
		
	}
}
