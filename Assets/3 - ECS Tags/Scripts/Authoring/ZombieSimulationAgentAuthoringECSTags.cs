using UnityEngine;
using Unity.Entities;

public class ZombieSimulationAgentAuthoringECSTags : MonoBehaviour
{
	class Baker : Baker<ZombieSimulationAgentAuthoringECSTags>
	{
		public override void Bake(ZombieSimulationAgentAuthoringECSTags authoring)
		{
			var entity = GetEntity(TransformUsageFlags.Dynamic);
			//
		}
	}
}
