using System.IO;

namespace ACuttingBox.Properties;

[ModelDoc.GameData( "CuttableProperties" )]
public partial class CuttableProperties
{
	[ResourceType( "vmat" )]
	public string MaterialName { get; set; }

	[HideInEditor]
	public string ModelName { get; set; }

	public byte[] SendProperties()
	{
		using var ms = new MemoryStream();
		using var bw = new BinaryWriter( ms );

		bw.Write( MaterialName );
		bw.Write( ModelName );

		return ms.ToArray();
	}

	public static CuttableProperties ReceiveProperties( byte[] data )
	{
		using var ms = new MemoryStream( data );
		using var br = new BinaryReader( ms );

		return new CuttableProperties
		{
			MaterialName = br.ReadString(),
			ModelName = br.ReadString()
		};
	}


}
