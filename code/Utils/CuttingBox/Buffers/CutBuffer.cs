using System.IO;
using Rising.Utils;

namespace ACuttingBox.Buffers;


public class CutBuffer
{
	internal List<Vertex> Vertex = new List<Vertex>( 32 );

	internal List<uint> Index = new List<uint>( 32 );

	public Vertex Default;

	public bool Indexed { get; private set; }

	public virtual void Clear()
	{
		Vertex.Clear();
		Index.Clear();
		Default = default( Vertex );
	}

	public virtual void Init( bool useIndexBuffer )
	{
		Indexed = useIndexBuffer;
		Clear();
	}

	//
	// Summary:
	//     Add a vertex
	public void Add( Vertex v )
	{
		Vertex.Add( v );
	}

	//
	// Summary:
	//     Add an index. This is relative to the top of the vertex buffer. So 0 is Vertex.Count.,
	//     1 is Vertex.Count -1
	public void AddIndex( int i )
	{
		AddRawIndex( Vertex.Count - i );
	}

	//
	// Summary:
	//     Add an index. This is relative to the top of the vertex buffer. So 0 is Vertex.Count.
	public void AddTriangleIndex( int a, int b, int c )
	{
		AddIndex( a );
		AddIndex( b );
		AddIndex( c );
	}

	//
	// Summary:
	//     Add an index. This is relative to the top of the vertex buffer. So 0 is Vertex.Count.
	public void AddRawIndex( int i )
	{
		Index.Add( (ushort)i );
	}

	public VertexBuffer GetVertexBuffer()
	{
		VertexBuffer vb = new();
		vb.Init( Indexed );
		if ( Indexed )
		{
			for ( int i = 0; i < Index.Count; i += 3 )
			{
				vb.AddTriangle( Vertex[(int)Index[i]], Vertex[(int)Index[i + 1]], Vertex[(int)Index[i + 2]] );
			}
		}
		else
		{
			for ( int i = 0; i < Vertex.Count; i++ )
			{
				vb.Add( Vertex[i] );
			}
			for ( int i = 0; i < Index.Count; i += 3 )
			{
				vb.AddTriangleIndex( (int)Index[i], (int)Index[i + 1], (int)Index[i + 2] );
			}
		}

		return vb;
	}



	public byte[] SendCutBuffer()
	{
		using MemoryStream ms = new();
		using BinaryWriter bw = new( ms );

		bw.Write( Vertex.Count );
		bw.Write( Index.Count );
		foreach ( Vertex v in Vertex )
		{
			bw.Write( v.Position );
			bw.Write( v.Color.ToColor() );
			bw.Write( v.Normal );
			bw.Write( v.TexCoord0 );
			bw.Write( v.TexCoord1 );
			bw.Write( v.TexCoord2 );
			bw.Write( v.TexCoord3 );
			bw.Write( v.Tangent );
		}

		foreach ( uint i in Index )
		{
			bw.Write( i );
		}
		return ms.ToArray();
	}

	public static CutBuffer ReceiveCutBuffer( byte[] CutBufferData )
	{
		CutBuffer buffer = new();
		using MemoryStream ms = new( CutBufferData );
		using BinaryReader br = new( ms );

		int vertexCount = br.ReadInt32();
		int indexCount = br.ReadInt32();
		buffer.Vertex.Clear();
		buffer.Index.Clear();
		for ( int i = 0; i < vertexCount; i++ )
		{
			buffer.Vertex.Add( new Vertex()
			{
				Position = br.ReadVector3(),
				Color = br.ReadColor(),
				Normal = br.ReadVector3(),
				TexCoord0 = br.ReadVector4(),
				TexCoord1 = br.ReadVector4(),
				TexCoord2 = br.ReadVector4(),
				TexCoord3 = br.ReadVector4(),
				Tangent = br.ReadVector4()
			} );
		}
		for ( int i = 0; i < indexCount; i++ )
		{
			buffer.Index.Add( br.ReadUInt32() );
		}

		return buffer;
	}
}

