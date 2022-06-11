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
