using ACuttingBox.Entities;
using ACuttingBox.Systems;
using Rising.Utils;
using Sandbox;

namespace Rising;

partial class VrPlayer : Player
{
	[Net, Local, Predicted] public VrLeftHand LeftHand { get; set; }
	[Net, Local, Predicted] public VrRightHand RightHand { get; set; }

	public static BaseCuttableSO StencilSphere { get; set; }



	private void CreateHands()
	{
		DeleteHands();

		LeftHand = new() { Owner = this };
		RightHand = new() { Owner = this };

		LeftHand.Other = RightHand;
		RightHand.Other = LeftHand;
	}

	private void DeleteHands()
	{
		LeftHand?.Delete();
		RightHand?.Delete();
	}

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		if ( Client.IsUsingVr )
		{
			Controller = new VrWalkController();
			Animator = new VrPlayerAnimator();
			CameraMode = new VrCamera();
		}
		else
		{
			Controller = new WalkController();
			Animator = new StandardPlayerAnimator();
			CameraMode = new FirstPersonCamera();
		}

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		CreateHands();

		if ( Client.IsUsingVr )
			SetBodyGroup( "Hands", 1 ); // Hide hands

		base.Respawn();
	}
	Material m;

	public void CreateStencilSphere()
	{
		if ( !IsClient ) return;
		if ( StencilSphere != null )
		{
			StencilSphere.Delete();
		}
		/* StencilSphere = new SceneCustomObject( Map.Scene )
		{
			Bounds = new BBox( -Vector3.Zero * 50, Vector3.One * 50 ) + Position,
			RenderOverride = RenderStuff,

		}; */
		m = Material.Load( "materials/cutview_stencil_other.vmat" );
		var spherebuffer = new VertexBuffer();
		spherebuffer.Init( true );

		spherebuffer = AddCube( spherebuffer, 30f );

		/* var mesh = new Mesh( m );
		mesh.CreateBuffers( spherebuffer );
		Model mmm = Model.Builder.AddMesh( mesh ).Create();
		StencilSphere = new SceneObject( Map.Scene, mmm, Transform ); */

		StencilSphere = new BaseCuttableSO( Map.Scene, spherebuffer, m, SceneLayerType.Translucent );

		StencilSphere.RenderingEnabled = cutview;
	}

	public VertexBuffer AddCube( VertexBuffer self, float Radius )
	{
		var f = Rotation.Identity.Forward * Radius * 0.5f;
		var l = Rotation.Identity.Left * Radius * 0.5f;
		var u = Rotation.Identity.Up * Radius * 0.5f;
		self.AddQuad( new Ray( new Vector3() - f, f.Normal ), l, u );
		self.AddQuad( new Ray( new Vector3() + f, -f.Normal ), l, -u );
		self.AddQuad( new Ray( new Vector3() - l, l.Normal ), -f, u );
		self.AddQuad( new Ray( new Vector3() + l, -l.Normal ), f, u );
		self.AddQuad( new Ray( new Vector3() - u, u.Normal ), f, l );
		self.AddQuad( new Ray( new Vector3() + u, -u.Normal ), f, -l );


		return self;
	}

	[Event.Hotload]
	private void hot()
	{
		CreateStencilSphere();
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		CreateStencilSphere();
	}



	public override void Simulate( Client cl )
	{
		base.Simulate( cl );
		SimulateActiveChild( cl, ActiveChild );

		CheckRotate();
		SetVrAnimProperties();



		if ( !RightHand.IsValid() || !LeftHand.IsValid() || !Client.IsUsingVr )
		{
			NonVRSimulate( cl );
			return;
		}

		LeftHand.Simulate( cl );
		RightHand.Simulate( cl );


		if ( RightHand.InputHand.ButtonA.WasPressed )
		{
			VrGame.SpawnTestCube();
		}
	}

	Vector3 startpoint;
	Vector3 endpoint;

	bool cutview = false;
	private void NonVRSimulate( Client cl )
	{
		if ( Input.Pressed( InputButton.Use ) )
		{
			VrGame.SpawnTestCube();
		}
		if ( Input.Pressed( InputButton.Menu ) && IsClient )
		{
			cutview = !cutview;
			StencilSphere.RenderingEnabled = cutview;
			Event.Run( "StencilToggle", cutview );

		}
		if ( Input.Pressed( InputButton.PrimaryAttack ) )
		{
			startpoint = EyePosition + EyeRotation.Forward * 10;
		}

		if ( Input.Down( InputButton.PrimaryAttack ) )
		{
			endpoint = EyePosition + EyeRotation.Forward * 10;
			var eyes = EyePosition;
			Vector3 normal = (startpoint - endpoint).Cross( eyes - startpoint );
			DebugOverlay.Circle( EyePosition + EyeRotation.Forward * 100, Rotation.LookAt( normal, EyeRotation.Forward ), 10, Color.Orange, 1 );
		}
		if ( Input.Released( InputButton.PrimaryAttack ) && IsServer )
		{
			endpoint = EyePosition + EyeRotation.Forward * 10;
			var eyes = EyePosition;

			Vector3 normal = (startpoint - endpoint).Cross( eyes - startpoint );
			Plane idk = new( EyePosition, normal );
			var tr = Trace.Sphere( 50, EyePosition, EyePosition + EyeRotation.Forward * 100 ).Ignore( this ).EntitiesOnly().RunAll();
			if ( tr != null )
				for ( int i = 0; i < tr.Length; i++ )
				{
					TraceResult ent = tr[i];
					if ( ent.Hit )
					{
						if ( ent.Entity is BaseCuttable cuttable && idk.GetDistance( cuttable.Position ) < 25f )
						{
							cuttable.CutObject( cuttable.Transform.NormalToLocal( normal ), cuttable.Transform.PointToLocal( ent.HitPosition ), 100 );
						}
					}
				}
		}
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		LeftHand?.FrameSimulate( cl );
		RightHand?.FrameSimulate( cl );

		if ( IsClient )
		{

			StencilSphere.Rotation = EyeRotation;
			StencilSphere.Position = EyePosition + EyeRotation.Forward * 20;
			StencilSphere.RenderBounds = new BBox( -Vector3.Zero * 5000, Vector3.One * 5000 )/*  + StencilSphere.Position */;
		}
	}

	public void SetVrAnimProperties()
	{
		if ( LifeState != LifeState.Alive )
			return;

		if ( !Input.VR.IsActive )
			return;

		SetAnimParameter( "b_vr", true );
		var leftHandLocal = Transform.ToLocal( LeftHand.GetBoneTransform( 0 ) );
		var rightHandLocal = Transform.ToLocal( RightHand.GetBoneTransform( 0 ) );

		var handOffset = Vector3.Zero;
		SetAnimParameter( "left_hand_ik.position", leftHandLocal.Position + (handOffset * leftHandLocal.Rotation) );
		SetAnimParameter( "right_hand_ik.position", rightHandLocal.Position + (handOffset * rightHandLocal.Rotation) );

		SetAnimParameter( "left_hand_ik.rotation", leftHandLocal.Rotation * Rotation.From( 0, 0, 180 ) );
		SetAnimParameter( "right_hand_ik.rotation", rightHandLocal.Rotation );

		float height = Input.VR.Head.Position.z - Position.z;
		SetAnimParameter( "duck", 1.0f - ((height - 32f) / 32f) ); // This will probably need tweaking depending on height
	}

	private TimeSince timeSinceLastRotation;
	private void CheckRotate()
	{
		if ( !IsServer )
			return;

		const float deadzone = 0.2f;
		const float angle = 45f;
		const float delay = 0.25f;

		float rotate = Input.VR.RightHand.Joystick.Value.x;

		if ( timeSinceLastRotation > delay )
		{
			if ( rotate > deadzone )
			{
				Transform = Transform.RotateAround(
					Input.VR.Head.Position.WithZ( Position.z ),
					Rotation.FromAxis( Vector3.Up, -angle )
				);

				timeSinceLastRotation = 0;
			}
			else if ( rotate < -deadzone )
			{
				Transform = Transform.RotateAround(
					Input.VR.Head.Position.WithZ( Position.z ),
					Rotation.FromAxis( Vector3.Up, angle )
				);

				timeSinceLastRotation = 0;
			}
		}

		if ( rotate > -deadzone && rotate < deadzone )
		{
			timeSinceLastRotation = 10;
		}
	}

	public override void OnKilled()
	{
		base.OnKilled();
		EnableDrawing = false;
		DeleteHands();
	}
}
