﻿using Sandbox;

namespace Voxel
{
	public partial class VoxelEntity : Prop
	{
		[Net]
		public string Model { get; set; }

		private string _lastModel;

		[Event.Tick]
		public void Tick()
		{
			if ( Model != "" && Model != _lastModel )
			{
				VoxelModel model = VoxelManager.Models[Model];

				SetModel( model.Model );
				SetupPhysicsFromOBB( PhysicsMotionType.Dynamic, model.Bounds.Mins, model.Bounds.Maxs );

				PhysicsBody.Mass = model.Volume;
				PhysicsBody.RebuildMass();

				_lastModel = Model;
			}

			//DebugOverlay.Axis( Position, Rotation, depthTest: false );
		}

		[ServerCmd( "spawn_voxel" )]
		public static void SpawnEntity( string model )
		{
			if ( ConsoleSystem.Caller == null )
				return;

			if ( !VoxelManager.Models.ContainsKey( model ) )
				return;

			SandboxPlayer pawn = ConsoleSystem.Caller.Pawn as SandboxPlayer;

			TraceResult trace = Trace.Ray( pawn.EyePos, pawn.EyePos + pawn.EyeRot.Forward * 5000.0f ).UseHitboxes().Ignore( pawn ).Run();

			VoxelEntity entity = new() { Model = model };

			entity.Position = trace.EndPos + trace.Normal;
		}
	}
}
