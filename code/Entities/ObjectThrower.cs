
using ACuttingBox.Entities;

namespace Rising.Entities;

[HammerEntity]
[Library( "prop_objectthrower" )]
[EditorModel( "materials/models/editor/point_aimat.vmdl" )]
public partial class ObjectThrower : Entity
{
	[Property, ResourceType( "vmdl" )]
	public string ModelToThrow { get; set; }

	[Property]
	public float ThrowForce { get; set; }

	[Property]
	public float MinThrowTime { get; set; } = 1;
	[Property]
	public float MaxThrowTime { get; set; } = 3;

	public BaseCuttable ThrownObject { get; set; }

	public TimeUntil NextThrow { get; set; }

	public float NextThrowTime { get; set; }


	[Event.Tick.Server]
	public void tick()
	{
		if ( NextThrow )
		{
			if ( ThrownObject.IsValid() )
			{
				ThrownObject.Delete();
			}
			ThrownObject = new BaseCuttable()
			{
				Model = Model.Load( ModelToThrow ),
				Transform = Transform
			};

			ThrownObject.Velocity = Rotation.Forward * ThrowForce * (ThrownObject.PhysicsBody.Mass / 35);
			ThrownObject.Rotation = Rotation.Random;
			ThrownObject.AngularVelocity = Angles.Random;
			ThrownObject.PhysicsBody.GravityScale = 0.5f;
			NextThrow = Rand.Float( MinThrowTime, MaxThrowTime );
		}
	}


}
