using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Voxel
{
	public struct VoxelModel
	{
		public Model Model;
		public BBox Bounds;
		public float Scale;
		public float Volume;
	}

	public static class VoxelManager
	{
		public static readonly Dictionary<string, VoxelModel> Models = new();
		public static readonly Dictionary<uint, IVoxelLoader> Loaders = new();

		public static void RegisterModel( string name, VoxelModel model )
		{
			Models[name] = model;
		}

		public static void RegisterLoader( string signature, IVoxelLoader loader )
		{
			Log.Info( $"[Voxel] Registering loader: {loader.GetType()} for file signature {signature}" );

			Loaders[BitConverter.ToUInt32( Encoding.Default.GetBytes( signature ), 0 )] = loader;
		}

		public static void Load()
		{
			RegisterLoader( "Kvxl", new Kv6Loader() );
			RegisterLoader( "VOX ", new MagicaLoader() );

			LoadFromFolder( "voxel/weapons" );

			LoadFromFile( "voxel/props/cp.kv6" );
			LoadFromFile( "voxel/props/intel.kv6" );

			LoadFromFolder( "voxel/monu" );

			VoxelBuilder builder = new();

			builder.Set( 0, 0, 0, new Color32( 255, 0, 0 ) );
			builder.Set( 1, 0, 0, new Color32( 255, 0, 0 ) );
			builder.Set( 0, 1, 0, new Color32( 0, 255, 0 ) );
			builder.Set( 1, 1, 0, new Color32( 0, 255, 0 ) );

			builder.Set( 0, 0, 10, new Color32( 0, 0, 255 ) );
			builder.Set( 1, 0, 10, new Color32( 0, 0, 255 ) );
			builder.Set( 0, 1, 10, new Color32( 0, 0, 255 ) );
			builder.Set( 1, 1, 10, new Color32( 0, 0, 255 ) );

			RegisterModel( "test", builder.Build() );
		}

		public static void LoadFromFolder( string folder )
		{
			foreach ( var file in FileSystem.Mounted.FindFile( folder, "*" ) )
				LoadFromFile( $"{folder}/{file}" );
		}

		public static void LoadFromFile( string filename )
		{
			string name = Path.GetFileNameWithoutExtension( filename );

			Log.Info( $"[Voxel] Loading model: {name} from {filename}" );

			Stream stream = FileSystem.Mounted.OpenRead( filename );

			uint signature = stream.ReadStructureFromStream<uint>();

			if ( !Loaders.TryGetValue( signature, out IVoxelLoader loader ) )
			{
				Log.Error( $"Unknown file signature: {signature}" );

				return;
			}

			Log.Info( $"[Voxel] Matching signature found for {loader.GetType()}" );

			Models[name] = loader.Load( stream );
		}
	}
}
