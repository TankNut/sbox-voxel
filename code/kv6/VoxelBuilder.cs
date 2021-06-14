using Sandbox;
using System;
using System.Collections.Generic;

namespace Kv6
{
	public struct VoxelModel
	{
		public Model Model;
		public BBox Bounds;
		public float Scale;
	}

	public class VoxelBuilder
	{
		public static readonly Dictionary<string, VoxelModel> Models = new();

		public ImageFormat ColorFormat;
		public Vector3 Pivot { get; set; }

		private Dictionary<VoxelPosition, VoxelData> _data = new();

		private struct VoxelPosition
		{
			public int x;
			public int y;
			public int z;

			public Vector3 Vector => new Vector3( x, y, z );

			public VoxelPosition( int x, int y, int z )
			{
				this.x = x;
				this.y = y;
				this.z = z;
			}
		}

		private struct VoxelData
		{
			public Color32 Color;
			public VoxelPosition Position;
		}

		public bool Exists( int x, int y, int z )
		{
			return _data.ContainsKey( new VoxelPosition( x, y, z ) );
		}

		public void Set( int x, int y, int z, Color32 color )
		{
			VoxelPosition position = new( x, y, z );

			_data[position] = new VoxelData()
			{
				Color = color,
				Position = position
			};
		}

		private Tuple<VertexBuffer, Vector3, Vector3> GenerateMesh( float scale )
		{
			VertexBuffer buffer = new();
			Vector3 mins = new Vector3( float.MaxValue );
			Vector3 maxs = new Vector3( float.MinValue );

			buffer.Init( true );

			foreach ( KeyValuePair<VoxelPosition, VoxelData> pair in _data )
			{
				VoxelData voxelData = pair.Value;

				int x = voxelData.Position.x;
				int y = voxelData.Position.y;
				int z = voxelData.Position.z;

				mins.x = Math.Min( mins.x, x );
				mins.y = Math.Min( mins.y, y );
				mins.z = Math.Min( mins.z, z );

				maxs.x = Math.Max( maxs.x, x );
				maxs.y = Math.Max( maxs.y, y );
				maxs.z = Math.Max( maxs.z, z );

				Rotation rot = Rotation.Identity;

				var f = rot.Forward * scale * 0.5f;
				var l = rot.Left * scale * 0.5f;
				var u = rot.Up * scale * 0.5f;

				Vector3 position = new Vector3( x, y, z ) - Pivot;

				position *= scale;

				if ( !Exists( x + 1, y, z ) )
					AddQuad( buffer, new Ray( position + f, f.Normal ), l, u, voxelData.Color );
				if ( !Exists( x - 1, y, z ) )
					AddQuad( buffer, new Ray( position - f, -f.Normal ), l, -u, voxelData.Color );

				if ( !Exists( x, y + 1, z ) )
					AddQuad( buffer, new Ray( position + l, l.Normal ), -f, u, voxelData.Color );
				if ( !Exists( x, y - 1, z ) )
					AddQuad( buffer, new Ray( position - l, -l.Normal ), f, u, voxelData.Color );

				if ( !Exists( x, y, z + 1 ) )
					AddQuad( buffer, new Ray( position + u, u.Normal ), f, l, voxelData.Color );
				if ( !Exists( x, y, z - 1 ) )
					AddQuad( buffer, new Ray( position - u, -u.Normal ), f, -l, voxelData.Color );
			}

			return Tuple.Create( buffer, mins, maxs );
		}

		private static void AddQuad( VertexBuffer buffer, Ray origin, Vector3 width, Vector3 height, Color32 color )
		{
			buffer.Default.Normal = origin.Direction;
			buffer.Default.Tangent = new Vector4( width.Normal, 1 );
			buffer.Default.Color = color;

			buffer.Add( origin.Origin - width - height );
			buffer.Add( origin.Origin + width - height );
			buffer.Add( origin.Origin + width + height );
			buffer.Add( origin.Origin - width + height );

			buffer.AddTriangleIndex( 4, 3, 2 );
			buffer.AddTriangleIndex( 2, 1, 4 );
		}

		public void Build( string name, float scale )
		{
			Material material = Material.Load( "materials/default/vertex_color.vmat" );

			var tuple = GenerateMesh( scale );

			VertexBuffer buffer = tuple.Item1;

			Vector3 mins = tuple.Item2 - Pivot - new Vector3( 0.5f );
			Vector3 maxs = tuple.Item3 - Pivot + new Vector3( 0.5f );

			Models[name] = new VoxelModel()
			{
				Model = buffer.CreateModel( "test", material ),
				Bounds = new BBox( mins * scale, maxs * scale ),
				Scale = scale
			};
		}
	}
}
