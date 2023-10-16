using UnityEngine;
using Unity.Entities;

public struct InfectedStateEnableable : IComponentData, IEnableableComponent
{
	public float IncubationEndTime;
}