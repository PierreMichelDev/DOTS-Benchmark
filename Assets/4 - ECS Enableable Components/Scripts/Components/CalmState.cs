using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct CalmStateEnableable : IComponentData, IEnableableComponent
{
	public float NextDirectionChangeTime;
}