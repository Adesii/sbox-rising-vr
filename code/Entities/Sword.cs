

using ACuttingBox.Entities;
using ACuttingBox.Properties;
using ACuttingBox.Systems;

namespace Rising.Entities;

public partial class Sword : ModelEntity
{
	[Net] public Vector3 StartPoint { get; set; }
	[Net] public Vector3 EndPoint { get; set; }
	public BaseCuttable CuttingObject { get; set; }

	[Net, Predicted] public Vector3 CuttingStart { get; set; }
	[Net, Predicted] public Vector3 CuttingEnd { get; set; }
	public override void Spawn()
	{
		base.Spawn();
		SetModel( "models/testing/sword.vmdl" );
		SetSwordSettings();
	}

	public void SetSwordSettings()
	{
		if ( IsServer )
		{
			var bladesettings = Model.GetData<CuttingBladeProperties>();
			StartPoint = bladesettings.StartBlade;
			EndPoint = bladesettings.EndBlade;

			LocalRotation = bladesettings.HoldAngles.ToRotation();
			LocalPosition = bladesettings.HoldPosition;

		}
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );


		var start = Transform.PointToWorld( StartPoint );
		var end = Transform.PointToWorld( EndPoint );
		var tr = Trace.Ray( start, end ).EntitiesOnly().WithTag( "cuttable" ).Run();
		DebugOverlay.TraceResult( tr );
		if ( !CuttingObject.IsValid() )
		{
			if ( tr.Hit )
			{
				CuttingObject = tr.Entity as BaseCuttable;
				CuttingStart = tr.HitPosition;
			}
		}
		else
		{
			if ( tr.Hit )
			{
				CuttingEnd = tr.HitPosition;
			}
			else if ( CuttingObject.IsValid() )
			{
				Plane plane = new( start, ComputeNormal( CuttingStart, CuttingEnd, start ) );
				if ( IsServer )
					CuttingObject.CutObject( CuttingObject.Transform.NormalToLocal( plane.Normal ), CuttingObject.Transform.PointToLocal( plane.Origin ), 100f );
				CuttingObject = null;
			}
		}
	}

	public Vector3 ComputeNormal( Vector3 p1, Vector3 p2, Vector3 p3 )
	{
		Vector3 v1 = p2 - p1;
		Vector3 v2 = p3 - p1;
		return Vector3.Cross( v1, v2 );
	}
}
