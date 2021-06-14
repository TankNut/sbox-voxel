using Sandbox;
using System;
using System.IO;
using System.Linq;

namespace Kv6
{
	class Kv6Loader
	{
		private static readonly byte[] _signature = new byte[] { 0x4b, 0x76, 0x78, 0x6c };

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

		[Event.Hotload]
		public static void LoadAll()
		{
			LoadFromFolder( "kv6/weapons", 1.0f );

			LoadFromFile( "kv6/props/cp.kv6", 8.0f );
			LoadFromFile( "kv6/props/intel.kv6", 4.0f );

			VoxelBuilder builder = new();

			builder.Set( 0, 0, 0, new Color32( 255, 0, 0 ) );
			builder.Set( 1, 0, 0, new Color32( 255, 0, 0 ) );
			builder.Set( 0, 1, 0, new Color32( 0, 255, 0 ) );
			builder.Set( 1, 1, 0, new Color32( 0, 255, 0 ) );

			builder.Set( 0, 0, 10, new Color32( 0, 0, 255 ) );
			builder.Set( 1, 0, 10, new Color32( 0, 0, 255 ) );
			builder.Set( 0, 1, 10, new Color32( 0, 0, 255 ) );
			builder.Set( 1, 1, 10, new Color32( 0, 0, 255 ) );

			builder.Build( "test", 8.0f );
		}

		private static void LoadFromFolder( string folder, float scale )
		{
			foreach ( var file in FileSystem.Mounted.FindFile( folder, "*.kv6" ) )
				LoadFromFile( $"{folder}/{file}", scale );
		}

		private static void LoadFromFile( string filename, float scale )
		{
			string name = Path.GetFileNameWithoutExtension( filename );

			Log.Info( $"Loading model: {name}" );

			Stream stream = FileSystem.Mounted.OpenRead( filename );

			Assert.True( stream.ReadByteArrayFromStream( stream.Position, 4 ).SequenceEqual( _signature ) );

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

			builder.Build( name, scale );
		}
	}
}
