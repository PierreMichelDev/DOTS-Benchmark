using UnityEngine;
using Unity.Entities;

public struct ZombieState : IComponentData
{
	public float NextAttackTime;
}