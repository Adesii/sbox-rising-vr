using Sandbox;

namespace Rising;

public partial class VrBaseHand : AnimatedEntity
{
	[Net] public VrBaseHand Other { get; set; }

	protected virtual string ModelPath => "";

	public bool GripPressed => InputHand.Grip > 0.5f;
	public bool TriggerPressed => InputHand.Trigger > 0.5f;

	public virtual Input.VrHand InputHand { get; }

	[Net] public ModelEntity HoldingEntity { get; set; }

	public override void Spawn()
	{
		SetModel( ModelPath );

		Position = InputHand.Transform.Position;
		Rotation = InputHand.Transform.Rotation;

		Transmit = TransmitType.Always;
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		Transform = InputHand.Transform;
		HoldingEntity?.FrameSimulate( cl );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Transform = InputHand.Transform;
		HoldingEntity?.Simulate( cl );
		Animate();
	}

	private void Animate()
	{
		SetAnimParameter( "Index", InputHand.GetFingerCurl( 1 ) );
		SetAnimParameter( "Middle", InputHand.GetFingerCurl( 2 ) );
		SetAnimParameter( "Ring", InputHand.GetFingerCurl( 3 ) );
		SetAnimParameter( "Thumb", InputHand.GetFingerCurl( 0 ) );
	}
}
