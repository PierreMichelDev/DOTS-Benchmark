using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public enum SpatialHashEntityType
{
	Human,
	Infected,
	Zombie
}

public struct SpatialHashEntityData
{
	public Entity Entity;
	public float3 Position;
	public SpatialHashEntityType Type;
}

[BurstCompile]
public struct SpatialHashComparer : IComparer<SpatialHashEntityData>
{
	public float3 MinBounds;
	public float3 MaxBounds;
	public int2 GridSize;

	[BurstCompile]
	public int Compare(SpatialHashEntityData lhs, SpatialHashEntityData rhs)
	{
		int lhsIndex = ComputeCellFlatIndex(ComputeCellIndex(lhs.Position));
		int rhsIndex = ComputeCellFlatIndex(ComputeCellIndex(rhs.Position));
		return lhsIndex - rhsIndex;
	}

	[BurstCompile]
	public readonly int2 ComputeCellIndex(float3 position)
	{
		float3 normalizedPosition = (position - MinBounds) / (MaxBounds - MinBounds);
		int2 cellIndex = new int2((int)(normalizedPosition.x * GridSize.x), (int)(normalizedPosition.z * GridSize.y));
		cellIndex = math.clamp(cellIndex, 0, GridSize - 1);
		return cellIndex;
	}

	[BurstCompile]
	public readonly int ComputeCellFlatIndex(int2 cellIndex)
	{
		return cellIndex.y * GridSize.x + cellIndex.x;
	}
}

[BurstCompile]
public struct ZombieSimulationSpatialHashData : IComponentData
{
	public ZombieSimulationSpatialHashData(float3 minBounds, float3 maxBounds, int2 gridSize)
	{
		int cellCount = gridSize.x * gridSize.y;
		MaxCellIndex = cellCount - 1;
		CellStartIndices = new NativeArray<int>(cellCount, Allocator.Persistent);
		Entities = new NativeList<SpatialHashEntityData>(Allocator.Persistent);
		Comparer = new SpatialHashComparer()
		{
			MinBounds = minBounds,
			MaxBounds = maxBounds,
			GridSize = gridSize
		};
	}

	[BurstCompile]
	public void Reset()
	{
		Entities.Clear();
	}

	[BurstCompile]
	public void AddEntity(Entity entity, float3 position, SpatialHashEntityType type)
	{
		Entities.Add(new SpatialHashEntityData()
		{
			Entity = entity,
			Position = position,
			Type = type
		});
	}

	public void SortEntities()
	{
		int currentCellIndex = -1;
		int bufferLength = Entities.Length;
		Entities.Sort(Comparer);
		for (int i = 0; i < bufferLength; ++i)
		{
			int newCellIndex = Comparer.ComputeCellFlatIndex(Comparer.ComputeCellIndex(Entities[i].Position));

			if (currentCellIndex != newCellIndex)
			{
				CellStartIndices[newCellIndex] = i;
			}

			currentCellIndex = newCellIndex;
		}
	}

	[BurstCompile]
	public readonly void LookupEntities(float3 position, float radius, ref NativeList<SpatialHashEntityData> entities)
	{
	}

	public NativeList<SpatialHashEntityData> Entities;
	public NativeArray<int> CellStartIndices;
	public SpatialHashComparer Comparer;
	public int MaxCellIndex;
}
