using UnityEngine;
using Unity.Entities;

public struct ZombieSimulationZombieState : IComponentData
{
	public float NextAttackTime;
}