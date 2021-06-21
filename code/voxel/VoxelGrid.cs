using System.Collections.Generic;

namespace Voxel
{
	public class VoxelGrid
	{
		private int _sizeX, _sizeY, _sizeZ;
		private VoxelData[] _data;

		public VoxelGrid( int x, int y, int z )
		{
			_sizeX = x;
			_sizeY = y;
			_sizeZ = z;

			_data = new VoxelData[x * y * z];
		}

		public VoxelGrid Load( Dictionary<VoxelPosition, VoxelData> data, Vector3 offset )
		{
			foreach ( KeyValuePair<VoxelPosition, VoxelData> pair in data )
			{
				VoxelData voxelData = pair.Value;

				int x = voxelData.Position.x;
				int y = voxelData.Position.y;
				int z = voxelData.Position.z;

				Set( x - (int)offset.x, y - (int)offset.y, z - (int)offset.z, voxelData );
			}

			return this;
		}

		public VoxelData Get( int x, int y, int z )
		{
			if ( x < 0 || y < 0 || z < 0 || x >= _sizeX || y >= _sizeY || z >= _sizeZ )
				return default;

			return _data[(_sizeX * _sizeY * z) + (_sizeX * y) + x];
		}

		public VoxelData Get( int x, int y, int z, Vector3 offset ) => Get( x - (int)offset.x, y - (int)offset.y, z - (int)offset.z );
		public void Set( int x, int y, int z, VoxelData val ) => _data[(_sizeX * _sizeY * z) + (_sizeX * y) + x] = val;
	}
}
