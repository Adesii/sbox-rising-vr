using System.IO;

namespace ACuttingBox.Properties;

[ModelDoc.GameData( "CuttableProperties" )]
public partial class CuttableProperties
{
	[ResourceType( "vmat" )]
	public string MaterialName { get; set; }

	internal CuttableProperties copy()
	{
		return new CuttableProperties
		{
			MaterialName = MaterialName
		};
	}

	public byte[] SendProperties()
	{
		using var ms = new MemoryStream();
		using var bw = new BinaryWriter( ms );

		bw.Write( MaterialName );

		return ms.ToArray();
	}

	public static CuttableProperties ReceiveProperties( byte[] data )
	{
		using var ms = new MemoryStream( data );
		using var br = new BinaryReader( ms );

		return new CuttableProperties
		{
			MaterialName = br.ReadString()
		};
	}


}
