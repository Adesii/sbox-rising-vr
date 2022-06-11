using System.IO;
using Rising.Utils;

namespace ACuttingBox.Buffers;

public static class CutBufferExtensions
{
	public static void AddTriangle( this CutBuffer self, Vertex a, Vertex b, Vertex c )
	{
		self.Add( a );
		self.Add( b );
		self.Add( c );

		if ( self.Indexed )
		{
			self.AddTriangleIndex( 3, 2, 1 );
		}
	}




}

