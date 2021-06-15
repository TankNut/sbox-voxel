using Sandbox;
using System;
using System.IO;

namespace Voxel
{
	class Kv6Loader : IVoxelLoader
	{
#pragma warning disable CS0649 // Damn console warnings
		private struct Kv6Header
		{
			public uint SizeX, SizeY, SizeZ;
			public float PivotX, PivotY, PivotZ;
			public uint BlockCount;

			public Vector3 Size => new Vector3( SizeX, SizeY, SizeZ );
			public Vector3 Pivot => new Vector3( PivotX, PivotY, PivotZ );
		}

		private struct Kv6Block
		{
			public uint Color;
			public ushort ZPos;
			public byte Visibility;
			public byte Lighting;
		}
#pragma warning restore CS0649

		public VoxelModel Load( Stream stream, float scale )
		{
			Kv6Header header = stream.ReadStructureFromStream<Kv6Header>();
			Kv6Block[] blocks = stream.ReadStructuresFromStream<Kv6Block>( header.BlockCount );

			_ = stream.ReadStructuresFromStream<uint>( header.SizeX );

			ushort[] xyOffset = stream.ReadStructuresFromStream<ushort>( header.SizeX * header.SizeY );

			int index = 0;

			VoxelBuilder builder = new();

			builder.Pivot = header.Pivot.WithZ( header.SizeZ - header.PivotZ - 1 );

			for ( int x = 0; x < header.SizeX; x++ )
				for ( int y = 0; y < header.SizeY; y++ )
					for ( int i = 0; i < xyOffset[(x * header.SizeY) + y]; i++ )
					{
						Kv6Block block = blocks[index];

						byte[] bytes = BitConverter.GetBytes( block.Color );

						builder.Set( x, (int)header.SizeY - y - 1, (int)header.SizeZ - block.ZPos - 1, new Color32( bytes[2], bytes[1], bytes[0] ) );

						index++;
					}

			return builder.Build( scale );
		}
	}
}
