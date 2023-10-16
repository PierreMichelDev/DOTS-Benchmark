using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct PanicStateEnableable : IComponentData, IEnableableComponent
{
	public float PanicEndTime;
}