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
		public int Volume;
		public int Vertices;
	}

	public enum LogLevel
	{
		Error = 0,
		Warning = 1,
		Info = 2,
		Debug = 3
	}

	public static class VoxelManager
	{
		public static readonly Dictionary<string, VoxelModel> Models = new();
		public static readonly Dictionary<uint, IVoxelLoader> Loaders = new();

		[ClientVar( "voxel_debug", Saved = true )]
		public static int Debug { get; set; } = 0;

		public static void Log( object obj, LogLevel level, string realm = "Voxel" ) => Log( obj.ToString(), level, realm );
		public static void Log( string str, LogLevel level, string realm = "Voxel" )
		{
			if ( Debug < (int)level )
				return;

			switch ( level )
			{
				case LogLevel.Error:
					Sandbox.Log.Error( $"[{realm}-{Enum.GetName( typeof( LogLevel ), level )}] {str}" );
					break;
				case LogLevel.Warning:
					Sandbox.Log.Warning( $"[{realm}-{Enum.GetName( typeof( LogLevel ), level )}] {str}" );
					break;
				default:
					Sandbox.Log.Info( $"[{realm}-{Enum.GetName( typeof( LogLevel ), level )}] {str}" );
					break;
			}
		}

		public static void RegisterModel( string name, VoxelModel model )
		{
			Log( $"Model registered: {name}", LogLevel.Info );

			Models[name] = model;
		}

		public static void RegisterLoader( string signature, IVoxelLoader loader )
		{
			Log( $"Registering loader: {loader.GetType()} for file signature {signature}", LogLevel.Debug );

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

			builder.Set( 0, 0, 0, new Color( 1, 0, 0 ) );
			builder.Set( 1, 0, 0, new Color( 1, 0, 0 ) );
			builder.Set( 0, 1, 0, new Color( 0, 1, 0 ) );
			builder.Set( 1, 1, 0, new Color( 0, 1, 0 ) );

			builder.Set( 0, 0, 9, new Color( 0, 0, 1 ) );
			builder.Set( 1, 0, 9, new Color( 0, 0, 1 ) );
			builder.Set( 0, 1, 9, new Color( 0, 0, 1 ) );
			builder.Set( 1, 1, 9, new Color( 0, 0, 1 ) );

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

			Log( $"Loading model {name} from {filename}", LogLevel.Info );

			Stream stream = FileSystem.Mounted.OpenRead( filename );

			uint signature = stream.ReadStructureFromStream<uint>();

			if ( !Loaders.TryGetValue( signature, out IVoxelLoader loader ) )
			{
				Log( $"{filename}: Unknown file signature {signature}", LogLevel.Error );

				return;
			}

			Log( $"Matching signature found for {loader.GetType()}", LogLevel.Debug );

			RegisterModel( name, loader.Load( stream ) );
		}
	}
}
