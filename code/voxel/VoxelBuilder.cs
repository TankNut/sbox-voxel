using Sandbox;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Voxel
{
	public class VoxelBuilder
	{
		public ImageFormat ColorFormat;
		public Vector3 Pivot { get; set; }

		public Vector3 Mins { get; private set; } = new Vector3( float.MaxValue );
		public Vector3 Maxs { get; private set; } = new Vector3( float.MinValue );

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

			public override int GetHashCode() => HashCode.Combine( x, y, z );

			public override bool Equals( object obj ) => obj is VoxelPosition other && Equals( other );

			public bool Equals( VoxelPosition other )
			{
				return x == other.x && y == other.y && z == other.z;
			}
		}

		[StructLayout( LayoutKind.Sequential )]
		private struct VoxelVertex
		{
			public Vector3 Position;
			public Vector3 Normal;
			public Vector3 Tangent;
			public Vector2 TexCoord;
			public Color Color;

			public VoxelVertex( Vector3 position, Vector3 normal, Vector3 tangent, Vector2 texCoord, Color color )
			{
				Position = position;
				Normal = normal;
				Tangent = tangent;
				TexCoord = texCoord;
				Color = color;
			}

			public static readonly VertexAttribute[] Layout =
			{
				new VertexAttribute(VertexAttributeType.Position, VertexAttributeFormat.Float32, 3),
				new VertexAttribute(VertexAttributeType.Normal, VertexAttributeFormat.Float32, 3),
				new VertexAttribute(VertexAttributeType.Tangent, VertexAttributeFormat.Float32, 3),
				new VertexAttribute(VertexAttributeType.TexCoord, VertexAttributeFormat.Float32, 2),
				new VertexAttribute(VertexAttributeType.Color, VertexAttributeFormat.Float32, 4)
			};
		}

		private struct VoxelData
		{
			public Color Color;
			public VoxelPosition Position;
		}

		public bool Exists( int x, int y, int z )
		{
			return _data.ContainsKey( new VoxelPosition( x, y, z ) );
		}

		public void Set( int x, int y, int z, Color color )
		{
			VoxelPosition position = new( x, y, z );

			Mins = new Vector3( Math.Min( Mins.x, x ), Math.Min( Mins.y, y ), Math.Min( Mins.z, z ) );
			Maxs = new Vector3( Math.Max( Maxs.x, x ), Math.Max( Maxs.y, y ), Math.Max( Maxs.z, z ) );

			_data[position] = new VoxelData()
			{
				Color = color,
				Position = position
			};
		}

		private List<VoxelVertex> GenerateMesh()
		{
			List<VoxelVertex> vertices = new();

			foreach ( KeyValuePair<VoxelPosition, VoxelData> pair in _data )
			{
				VoxelData voxelData = pair.Value;

				int x = voxelData.Position.x;
				int y = voxelData.Position.y;
				int z = voxelData.Position.z;

				Rotation rot = Rotation.Identity;

				var f = rot.Forward * 0.5f;
				var l = rot.Left * 0.5f;
				var u = rot.Up * 0.5f;

				Vector3 position = new Vector3( x, y, z ) - Pivot;

				if ( !Exists( x + 1, y, z ) )
					CreateQuad( vertices, new Ray( position + f, f.Normal ), l, u, voxelData.Color );
				if ( !Exists( x - 1, y, z ) )
					CreateQuad( vertices, new Ray( position - f, -f.Normal ), l, -u, voxelData.Color );

				if ( !Exists( x, y + 1, z ) )
					CreateQuad( vertices, new Ray( position + l, l.Normal ), -f, u, voxelData.Color );
				if ( !Exists( x, y - 1, z ) )
					CreateQuad( vertices, new Ray( position - l, -l.Normal ), f, u, voxelData.Color );

				if ( !Exists( x, y, z + 1 ) )
					CreateQuad( vertices, new Ray( position + u, u.Normal ), f, l, voxelData.Color );
				if ( !Exists( x, y, z - 1 ) )
					CreateQuad( vertices, new Ray( position - u, -u.Normal ), f, -l, voxelData.Color );
			}

			return vertices;
		}

		private static void CreateQuad( List<VoxelVertex> vertices, Ray origin, Vector3 width, Vector3 height, Color color )
		{
			Vector3 normal = origin.Direction;
			Vector4 tangent = new Vector4( width.Normal, 1 );

			VoxelVertex a = new VoxelVertex( origin.Origin - width - height, normal, tangent, new Vector2( 0, 0 ), color );
			VoxelVertex b = new VoxelVertex( origin.Origin + width - height, normal, tangent, new Vector2( 1, 0 ), color );
			VoxelVertex c = new VoxelVertex( origin.Origin + width + height, normal, tangent, new Vector2( 1, 1 ), color );
			VoxelVertex d = new VoxelVertex( origin.Origin - width + height, normal, tangent, new Vector2( 0, 1 ), color );

			vertices.Add( a );
			vertices.Add( b );
			vertices.Add( c );

			vertices.Add( c );
			vertices.Add( d );
			vertices.Add( a );
		}

		public VoxelModel Build()
		{
			VoxelManager.Log( $"Pivot: {Pivot}", LogLevel.Debug, "Builder" );
			VoxelManager.Log( $"Blocks: {_data.Count}", LogLevel.Debug, "Builder" );

			Vector3 mins = Mins - Pivot - new Vector3( 0.5f );
			Vector3 maxs = Maxs - Pivot + new Vector3( 0.5f );

			List<VoxelVertex> vertices = GenerateMesh();

			VoxelManager.Log( $"Vertices: {vertices.Count}", LogLevel.Debug, "Builder" );

			Mesh mesh = new Mesh( Material.Load( "materials/default/vertex_color.vmat" ) );

			mesh.CreateVertexBuffer<VoxelVertex>( vertices.Count, VoxelVertex.Layout, vertices.ToArray() );
			mesh.SetBounds( mins, maxs );

			ModelBuilder builder = new ModelBuilder();

			BBox box = new BBox( mins, maxs );

			builder.AddMesh( mesh );
			builder.AddCollisionBox( box.Size * 0.5f, box.Center );
			builder.WithMass( _data.Count );

			return new VoxelModel()
			{
				Model = builder.Create(),
				Bounds = box,
				Volume = _data.Count,
				Vertices = vertices.Count
			};
		}
	}
}
