using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct ZombieSimulationPanicStateEnableable : IComponentData, IEnableableComponent
{
	public float PanicEndTime;
}