using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace Rising.Utils;

public static class BinaryExtensions
{
	public static void Write( this BinaryWriter writer, Vector4 v )
	{
		writer.Write( v.x );
		writer.Write( v.y );
		writer.Write( v.z );
		writer.Write( v.w );
	}
	public static Vector4 ReadVector4( this BinaryReader reader )
	{
		Vector4 v = new()
		{
			x = reader.ReadSingle(),
			y = reader.ReadSingle(),
			z = reader.ReadSingle(),
			w = reader.ReadSingle()
		};
		return v;
	}


	public static byte[] SendBoneBuffer( List<Bone> bones )
	{
		using var ms = new MemoryStream();
		using var bw = new BinaryWriter( ms );
		bw.Write( bones.Count );
		foreach ( var bone in bones )
		{
			bw.Write( bone.Name );
			bw.Write( bone.Position );
			bw.Write( bone.Rotation );
			bw.Write( bone.ParentName );
		}
		return ms.ToArray();
	}
	public static List<Bone> ReceiveBoneBuffer( this byte[] bones )
	{
		using var ms = new MemoryStream( bones );
		using var br = new BinaryReader( ms );
		int count = br.ReadInt32();
		List<Bone> bonesList = new();
		for ( int i = 0; i < count; i++ )
		{
			Bone bone = new()
			{
				Name = br.ReadString(),
				Position = br.ReadVector3(),
				Rotation = br.ReadRotation(),
				ParentName = br.ReadString()
			};
			bonesList.Add( bone );
		}
		return bonesList;
	}

	/* public static byte[] SendPhysicsBuffer( List<PhysicsBody> physicsbuffer )
	{
		using var ms = new MemoryStream();
		using var bw = new BinaryWriter( ms );
		bw.Write( physicsbuffer.Count );
		foreach ( var physics in physicsbuffer )
		{
			if ( !physics.IsValid() ) continue;
			physics.
			bw.Write( physics.Position );
			bw.Write( physics.Rotation );
			bw.Write( physics.Mass );
			bw.Write( physics.Velocity );
			bw.Write( physics.AngularVelocity );
		}
		return ms.ToArray();
	}

	public static List<PhysicsBody> ReceivePhysicsBuffer( this byte[] physicsbuffer )
	{
		using var ms = new MemoryStream( physicsbuffer );
		using var br = new BinaryReader( ms );
		int count = br.ReadInt32();
		List<PhysicsBody> physicsList = new();
		for ( int i = 0; i < count; i++ )
		{
			PhysicsBody physics = new( Map.Physics )
			{
				Position = br.ReadVector3(),
				Rotation = br.ReadRotation(),
				Mass = br.ReadSingle(),
				Velocity = br.ReadVector3(),
				AngularVelocity = br.ReadVector3()
			};
			physicsList.Add( physics );
		}
		return physicsList;
	} */
}
public static class RPCUtility
{
	public static byte[] Compress( this byte[] data )
	{
		using var stream = new MemoryStream();
		using var deflate = new DeflateStream( stream, CompressionLevel.Optimal );


		deflate.Write( data );
		deflate.Close();

		return stream.ToArray();
	}

	public static byte[] Decompress( this byte[] bytes )
	{
		using var outputStream = new MemoryStream();

		using ( var compressStream = new MemoryStream( bytes ) )
		{
			using var deflateStream = new DeflateStream( compressStream, CompressionMode.Decompress );
			deflateStream.CopyTo( outputStream );
		}

		return outputStream.ToArray();
	}
}
