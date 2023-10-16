using UnityEngine;
using Unity.Entities;

public struct ZombieStateEnableable : IComponentData, IEnableableComponent
{
	public float NextAttackTime;
}