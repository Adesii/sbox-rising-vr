using ACuttingBox.Entities;

namespace Rising.Entities;

[HammerEntity]
public partial class ObjectSpawner : Entity
{
	[Net] public BaseCuttable testCube { get; set; }
	public void SpawnTestCube()
	{
		var ents = Entity.All.OfType<BaseCuttable>();
		foreach ( var item in ents )
		{
			if ( item.IsValid() && item.IsClientOnly && IsClient )
				item.Delete();
			if ( item.IsValid() && item.IsServer && IsServer )
				item.Delete();
		}

		if ( IsServer )
		{
			if ( testCube.IsValid() ) testCube.Delete();
			var cube = new BaseCuttable( "models/testing/suzane.vmdl" )
			{
				Transform = Transform,
				UseAttachments = true,
			};
			cube.GenerateAttachments();
			testCube = cube;
		}

	}


}
