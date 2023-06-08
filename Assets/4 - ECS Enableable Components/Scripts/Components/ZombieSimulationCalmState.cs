using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct ZombieSimulationCalmStateEnableable : IComponentData, IEnableableComponent
{
	public float NextDirectionChangeTime;
}