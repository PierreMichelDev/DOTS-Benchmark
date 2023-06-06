using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct ZombieSimulationAgentState : IComponentData
{
	public enum MovementState
	{
		Calm,
		Panic,
		Chase
	}

	public enum HealthState
	{
		Healthy,
		Infected,
		Zombie
	}

	public MovementState Movement;
	public HealthState Health;
	public float NextDirectionChangeTime;
	public float PanicEndTime;
	public float IncubationEndTime;
	public float NextAttackTime;
}