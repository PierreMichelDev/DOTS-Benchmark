using UnityEngine;
using Unity.Entities;

public struct ZombieSimulationZombieStateEnableable : IComponentData, IEnableableComponent
{
	public float NextAttackTime;
}