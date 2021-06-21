namespace Voxel
{
	public class FlatGrid3<T>
	{
		private int _sizeX, _sizeY, _sizeZ;
		private T[] _data;

		public FlatGrid3( int x, int y, int z )
		{
			_sizeX = x;
			_sizeY = y;
			_sizeZ = z;

			_data = new T[x * y * z];
		}

		public T Get( int x, int y, int z )
		{
			if ( x < 0 || y < 0 || z < 0 || x >= _sizeX || y >= _sizeY || z >= _sizeZ )
				return default;

			return _data[(_sizeX * _sizeY * z) + (_sizeX * y) + x];
		}

		public T Get( int x, int y, int z, Vector3 offset ) => Get( x - (int)offset.x, y - (int)offset.y, z - (int)offset.z );
		public void Set( int x, int y, int z, T val ) => _data[(_sizeX * _sizeY * z) + (_sizeX * y) + x] = val;
	}
}
