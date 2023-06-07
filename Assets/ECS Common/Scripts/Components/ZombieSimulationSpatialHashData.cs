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

public struct SpatialHashCell
{
	public NativeList<SpatialHashEntityData> Entities;
}

[BurstCompile]
public struct ZombieSimulationSpatialHashData : IComponentData
{
	//TODO: rename since it doesn't really use a hash

	public ZombieSimulationSpatialHashData(float3 minBounds, float3 maxBounds, int2 gridSize)
	{
		MinBounds = minBounds;
		MaxBounds = maxBounds;
		GridSize = gridSize;

		int cellCount = gridSize.x * gridSize.y;
		Cells = new NativeArray<SpatialHashCell>(cellCount, Allocator.Persistent);
		for (int i = 0; i < cellCount; ++i)
		{
			Cells[i] = new SpatialHashCell()
			{
				Entities = new NativeList<SpatialHashEntityData>(Allocator.Persistent)
			};
		}
	}

	[BurstCompile]
	public void Reset()
	{
		int maxIndex = Cells.Length;
		for (int i = 0; i < maxIndex; ++i)
		{
			Cells[i].Entities.Clear();
		}
	}

	[BurstCompile]
	public void AddEntity(Entity entity, float3 position, SpatialHashEntityType type)
	{
		int2 cellIndex = ComputeCellIndex(position);
		int cellFlatIndex = ComputeCellFlatIndex(cellIndex);
		Cells[cellFlatIndex].Entities.Add(new SpatialHashEntityData()
		{
			Entity = entity,
			Position = position,
			Type = type
		});
	}

	[BurstCompile]
	public readonly void LookupEntities(float3 position, float radius, ref NativeList<SpatialHashEntityData> entities)
	{
		int2 minCellIndex = ComputeCellIndex(position - radius);
		int2 maxCellIndex = ComputeCellIndex(position + radius);

		float radiusSq = radius * radius;
		for (int y = minCellIndex.y; y <= maxCellIndex.y; ++y)
		{
			for (int x = minCellIndex.x; x <= maxCellIndex.x; ++x)
			{
				int2 cellIndex = new int2(x, y);
				int cellFlatIndex = ComputeCellFlatIndex(cellIndex);
				LookupCellEntities(position, radiusSq, Cells[cellFlatIndex].Entities, ref entities);
			}
		}
	}

	[BurstCompile]
	private readonly void LookupCellEntities(float3 position, float radiusSq, in NativeList<SpatialHashEntityData> cellEntities,
		ref NativeList<SpatialHashEntityData> entities)
	{
		int maxIndex = cellEntities.Length;
		for (int i = 0; i < maxIndex; ++i)
		{
			SpatialHashEntityData entityData = cellEntities[i];
			float distance = math.distancesq(position, entityData.Position);
			if (distance <= radiusSq)
			{
				entities.Add(entityData);
			}
		}
	}

	[BurstCompile]
	private readonly int2 ComputeCellIndex(float3 position)
	{
		float3 normalizedPosition = (position - MinBounds) / (MaxBounds - MinBounds);
		int2 cellIndex = new int2((int)(normalizedPosition.x * GridSize.x), (int)(normalizedPosition.z * GridSize.y));
		cellIndex = math.clamp(cellIndex, 0, GridSize - 1);
		return cellIndex;
	}

	[BurstCompile]
	private readonly int ComputeCellFlatIndex(int2 cellIndex)
	{
		return cellIndex.y * GridSize.x + cellIndex.x;
	}

	private float3 MinBounds;
	private float3 MaxBounds;
	private int2 GridSize;
	private NativeArray<SpatialHashCell> Cells;
}