using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct ZombieSimulationCalmState : IComponentData
{
	public float NextDirectionChangeTime;
}