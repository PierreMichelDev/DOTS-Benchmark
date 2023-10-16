using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct CalmState : IComponentData
{
	public float NextDirectionChangeTime;
}