using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ACuttingBox.Algorithms;
using ACuttingBox.Buffers;
using ACuttingBox.Properties;
using ACuttingBox.Systems;
using MIConvexHull;
using Rising.Util;
using Rising.Utils;
using Sandbox;

namespace ACuttingBox.Entities;

[HammerEntity]
[Model]
[Library( "prop_cuttable" )]
public partial class BaseCuttable : ModelEntity
{

	public const string HoleMaterialName = "materials/cutpart.vmat";
	public CuttableProperties ModelProperties { get; set; }


	[Net, Property]
	public bool StartPhysics { get; set; } = true;

	[Net, Property]
	public bool UseAttachments { get; set; } = true;

	public BaseCuttableSO CutViewObject { get; set; }
	public SceneObject CutViewModel { get; set; }

	public CuttingBox<PlaneCut> LastCuttingBox { get; set; }

	public List<PhysicsJoint> PhysicsJoints { get; set; } = new();

	public BaseCuttable()
	{
	}

	public BaseCuttable( string modelName ) : base( modelName )
	{
	}

	public override void Spawn()
	{
		base.Spawn();
		if ( !StartPhysics )
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		else
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
	}

	public void GenerateAttachments()
	{
		if ( Model.HasData<CuttableAttachment[]>() )
		{
			var attachments = Model.GetData<CuttableAttachment[]>();
			foreach ( var item in PhysicsJoints )
			{
				item.Remove();
			}

			foreach ( var item in attachments )
			{
				var poss = Transform.PointToWorld( item.AttachPosition );
				var rott = Transform.RotationToWorld( item.Angles.ToRotation() );
				var physjoint = PhysicsJoint.CreateHinge( PhysicsBody, new PhysicsBody( Map.Physics )
				{
					Position = poss,
					Rotation = rott,
				}, poss, item.Angles.Direction );
				if ( item.LimitAngles )
				{
					physjoint.MinAngle = item.MinimumAngle;
					physjoint.MaxAngle = item.MaximumAngle;
				}
				PhysicsJoints.Add( physjoint );
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ( CutViewObject.IsValid() )
		{
			CutViewObject.Delete();
		}
		if ( CutViewModel.IsValid() )
		{
			CutViewModel.Delete();
		}
		foreach ( var joint in PhysicsJoints )
		{
			joint.Remove();
		}
	}
	[Event.Tick.Server]
	public void ServerDebug()
	{
		if ( Rising.Util.Debug.Level > 4 )
			foreach ( var item in PhysicsJoints )
			{
				DebugOverlay.Axis( item.Body2.Position, Rotation * item.Point1.LocalRotation );
			}
	}
	[Event.Frame]
	public void showplanes()
	{
		if ( CutViewObject.IsValid() )
		{
			CutViewObject.RenderBounds = Model.Bounds/*  + Position */;
			CutViewObject.Transform = Transform.WithScale( Transform.Scale + 0.05f );
			CutViewObject.RenderDirty();
		}
		if ( CutViewModel.IsValid() )
		{
			CutViewModel.Transform = Transform.WithScale( Transform.Scale /* + 0.05f  */);
		}
		if ( SceneObject.IsValid() )
		{
			SceneObject.Flags.IsTranslucent = false;
			SceneObject.Flags.IsOpaque = true;
		}

	}

	IEnumerable<byte> _cutbufferData;
	IEnumerable<byte> _cutholeData;
	byte[] _completecutbufferData;
	byte[] _completecutholeData;
	byte[] _modelData;


	[ClientRpc]
	public void ReceiveChunkedCutBuffer( byte[] cutbuffer, byte[] holeBuffer, byte[] ModelPropertiesData, int chunkAmount, int currentchunk )
	{
		if ( currentchunk == 0 )
		{
			_cutbufferData = cutbuffer;
			_cutholeData = holeBuffer;
			_modelData = ModelPropertiesData;
		}
		else
		{
			if ( cutbuffer != null )
				_cutbufferData = _cutbufferData.Concat( cutbuffer );
			if ( holeBuffer != null )
				_cutholeData = _cutholeData.Concat( holeBuffer );
		}
		if ( currentchunk == chunkAmount )
		{
			_completecutbufferData = _cutbufferData.ToArray();
			_completecutholeData = _cutholeData.ToArray();
			FinalizeCutBuffer();
		}

	}
	[ClientRpc]
	public void ReceiveCutBuffer( byte[] cutbuffer, byte[] holeBuffer, byte[] ModelPropertiesData )
	{
		_completecutbufferData = cutbuffer;
		_completecutholeData = holeBuffer;
		_modelData = ModelPropertiesData;

		FinalizeCutBuffer();
	}

	public void FinalizeCutBuffer()
	{
		CutBuffer c = CutBuffer.ReceiveCutBuffer( _completecutbufferData.Decompress() );
		CutBuffer h = CutBuffer.ReceiveCutBuffer( _completecutholeData.Decompress() );
		ModelProperties = CuttableProperties.ReceiveProperties( _modelData );

		Log.Debug( $"Received cutbuffer with {c.Vertex.Count} cutpoints and {h.Vertex.Count} holepoints", 4 );

		var mb = Model.Builder;
		var mainmesh = new Mesh( Material.Load( ModelProperties.MaterialName ) );
		mainmesh.CreateBuffers( c.GetVertexBuffer() );
		mb = mb.AddMesh( mainmesh );

		var holemesh = new Mesh( Material.Load( HoleMaterialName ) );
		if ( h.Vertex.Count > 0 )
		{
			holemesh.CreateBuffers( h.GetVertexBuffer() );
			mb = mb.AddMesh( holemesh );
		}

		Model = mb.Create();
		Log.Debug( $"Created model with {Model.MeshCount} meshes", 4 );
	}

	public void CreateNewSO( VertexBuffer bm )
	{
		if ( CutViewObject.IsValid() )
		{
			CutViewObject.Delete();
			(Local.Pawn as Rising.VrPlayer).CreateStencilSphere(); //TODO: Remove this once i find a better way to do this
		}
		CutViewObject = new BaseCuttableSO( Map.Scene, bm, Material.Load( "materials/cutview_stencil.vmat" ), SceneLayerType.Opaque )
		{
			RenderingEnabled = RenderStencil
		};


		Log.Info( "Cuttable: Created CutViewObject" );
	}
	public static bool RenderStencil = false;



	[Event( "StencilToggle" )]
	public async void ToggleStencil( bool val )
	{
		RenderStencil = val;
		if ( CutViewObject.IsValid() )
		{
			await GameTask.NextPhysicsFrame();
			CutViewObject.RenderingEnabled = RenderStencil;
		}
	}


	[ConCmd.Server]
	public static void DeleteEnt( int id )
	{
		var ent = FindByIndex( id );
		if ( ent.IsValid() )
			ent.Delete();
	}
}
