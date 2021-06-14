using Sandbox;

namespace Kv6
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
				VoxelModel model = VoxelBuilder.Models[Model];

				SetModel( model.Model );
				SetupPhysicsFromOBB( PhysicsMotionType.Dynamic, model.Bounds.Mins, model.Bounds.Maxs );

				PhysicsBody.Mass = model.Bounds.Volume * model.Scale;
				PhysicsBody.RebuildMass();

				_lastModel = Model;
			}

			//DebugOverlay.Axis( Position, Rotation, depthTest: false );
		}

		[ServerCmd( "spawn_kv6" )]
		public static void SpawnEntity( string model )
		{
			if ( ConsoleSystem.Caller == null )
				return;

			if ( !VoxelBuilder.Models.ContainsKey( model ) )
				return;

			SandboxPlayer pawn = ConsoleSystem.Caller.Pawn as SandboxPlayer;

			TraceResult trace = Trace.Ray( pawn.EyePos, pawn.EyePos + pawn.EyeRot.Forward * 5000.0f ).UseHitboxes().Ignore( pawn ).Run();

			VoxelEntity entity = new() { Model = model };

			entity.Position = trace.EndPos + trace.Normal;
		}
	}
}
