global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using SandboxEditor;
global using System;
global using System.Collections.Generic;
global using System.ComponentModel;
global using System.Linq;
using ACuttingBox.Entities;
using ACuttingBox.Systems;
using Rising.Entities;

namespace Rising;

public partial class VrGame : Game
{
	public static VrGame Instance => Current as VrGame;
	[Net] public ObjectSpawner tester { get; set; }



	public VrGame()
	{
		if ( IsServer )
		{
			//_ = new ExampleHudEntity();
			Global.TickRate = 120;
		}

		if ( IsClient )
		{
			//PostProcess.Add( new MaterialPostProcess( "materials/cutview_postprocess.vmat" ) );
		}
	}

	[Event.Entity.PostSpawn]
	public void PostSpawn()
	{
		tester = FindByName( "@test" ) as ObjectSpawner;
	}

	public static void SpawnTestCube()
	{
		if ( !Instance.tester.IsValid() )
			Instance.tester = FindByName( "@test" ) as ObjectSpawner;
		Instance.tester.SpawnTestCube();
	}

	[ConCmd.Server()]
	public static void CutTestCube( int entid = 0, Vector3 plane = new() )
	{
		if ( entid == 0 )
		{
			Instance.tester.testCube = Entity.All.OfType<BaseCuttable>().First() as BaseCuttable;
		}
		else
		{
			Instance.tester.testCube = Entity.All.OfType<BaseCuttable>().FirstOrDefault( x => x.NetworkIdent == entid ) as BaseCuttable;
		}
		Instance.tester.testCube.CutObject( plane );
	}


	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );
		var player = new VrPlayer();
		client.Pawn = player;

		player.Respawn();
	}

}
