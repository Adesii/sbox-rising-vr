using Rising.Entities;
using Sandbox;

namespace Rising;

public class VrRightHand : VrBaseHand
{
	protected override string ModelPath => "models/hands/handright.vmdl";
	public override Input.VrHand InputHand => Input.VR.RightHand;

	public override void Spawn()
	{
		base.Spawn();
		Log.Info( "VR Controller Right Spawned" );
		SetInteractsAs( CollisionLayer.RIGHT_HAND );

		var sword = new Sword()
		{
			Parent = this,
		};
		sword.SetSwordSettings();
		HoldingEntity = sword;
	}
}
