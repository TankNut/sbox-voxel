using System.IO;

namespace Voxel
{
	public interface IVoxelLoader
	{
		public VoxelModel Load( Stream stream, float scale );
	}
}
