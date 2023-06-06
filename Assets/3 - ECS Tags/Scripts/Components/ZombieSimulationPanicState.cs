using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct ZombieSimulationPanicState : IComponentData
{
	public float PanicEndTime;
}