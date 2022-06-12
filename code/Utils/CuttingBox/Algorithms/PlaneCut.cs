using System.Threading.Tasks;
using ACuttingBox.Buffers;
using ACuttingBox.Interfaces;
using ACuttingBox.Properties;
using MIConvexHull;

namespace ACuttingBox.Algorithms;

public class PlaneCut : CuttingAlgorithm
{
	public Vector3 PlaneNormal { get; set; }
	public Vector3 PlanePoint { get; set; }
	public List<Plane> Planes { get; set; } = new();

	public override async Task Cut()
	{
		Planes.Add( new Plane( PlaneNormal, PlanePoint ) );
		if ( OriginalModel != null && !OriginalModel.IsProcedural && !OriginalModel.IsError )
		{
			CutBuffer bm = new();
			bm.Init( true );
			if ( OriginalModel.TryGetData<CuttableProperties>( out var props ) )
			{
				ModelProperties = props;
			}
			var originalVerts = OriginalModel.GetVertices();
			var originalIndices = OriginalModel.GetIndices();

			foreach ( var item in originalVerts )
			{
				bm.Add( item );
			}
			var newVerts = new List<Vertex>( originalIndices.Length );
			for ( int i = 0; i < originalIndices.Length; i++ )
			{
				newVerts.Add( originalVerts[originalIndices[i]] );
			}
			bm.Vertex = newVerts;
			OriginalCutBuffer = bm;
		}
		await GameTask.RunInThreadAsync( () =>
		{
			GenerateMesh();
		} );

	}

	enum IsInFront
	{
		Positive,
		Negative
	}

	public void GenerateMesh()
	{
		Plane plane = new( PlanePoint, PlaneNormal );
		Planes.Add( plane );


		Vertex[] originalVerts;

		if ( OriginalCutBuffer != null )
		{
			originalVerts = OriginalCutBuffer.Vertex.ToArray();
		}
		else
		{
			return;
		}

		CutBuffer VB1 = new();
		CutBuffer VB2 = new();
		VB1.Init( true );
		VB2.Init( true );

		List<Vertex> edges1 = new();
		List<Vertex> edges2 = new();

		HandleVertexes( originalVerts, VB1, VB2, plane, edges1, edges2 );

		CutBuffer cutpart1 = new();
		cutpart1.Init( true );

		CutBuffer cutpart2 = new();
		cutpart2.Init( true );


		var hullpoints1 = HandleHull( VB1, cutpart1 );
		var hullpoints2 = HandleHull( VB2, cutpart2 );


		result = new List<CutBuffer> { VB1, VB2 };
		holeresult = new List<CutBuffer> { cutpart1, cutpart2 };
		HullPoints = new List<List<Vector3>> { hullpoints1, hullpoints2 };

		List<Model> stuff = new();

		if ( hullpoints1.Count > 0 )
		{
			var m = Model.Builder;
			m = m.AddCollisionHull( hullpoints1.ToArray() );
			stuff.Add( m.Create() );
		}
		else
		{
			stuff.Add( null );
		}


		if ( hullpoints2.Count > 0 )
		{
			var m2 = Model.Builder;
			m2 = m2.AddCollisionHull( hullpoints2.ToArray() );
			stuff.Add( m2.Create() );
		}
		else
		{
			stuff.Add( null );
		}

		ResultModels = stuff;
		return;
	}


	private void HandleVertexes( Vertex[] newVerts, CutBuffer VB1, CutBuffer VB2, Plane plane, List<Vertex> edges1, List<Vertex> edges2 )
	{
		for ( int i = 0; i < newVerts.Length; i += 3 )
		{
			Vertex v1 = newVerts[i];
			Vertex v2 = newVerts[i + 1];
			Vertex v3 = newVerts[i + 2];


			HandleCutTriangle( v1, v2, v3, plane, VB1, VB2, out var edge1, out var edge2 );

			edges1.AddRange( edge1 );
			edges2.AddRange( edge2 );
		}
	}

	private void HandleCutTriangle( Vertex v1, Vertex v2, Vertex v3, Plane plane, CutBuffer vB1, CutBuffer vB2, out List<Vertex> edge1, out List<Vertex> edge2 )
	{
		IsInFront IsInFrontV1 = plane.IsInFront( v1.Position ) ? IsInFront.Positive : IsInFront.Negative;
		IsInFront IsInFrontV2 = plane.IsInFront( v2.Position ) ? IsInFront.Positive : IsInFront.Negative;
		IsInFront IsInFrontV3 = plane.IsInFront( v3.Position ) ? IsInFront.Positive : IsInFront.Negative;
		edge1 = new();
		edge2 = new();
		if ( IsInFrontV1 == IsInFrontV2 && IsInFrontV2 == IsInFrontV3 )
		{
			if ( IsInFrontV1 == IsInFront.Positive )
			{
				vB1.AddTriangle( v1, v2, v3 );

			}
			else
			{
				vB2.AddTriangle( v1, v2, v3 );
			}
		}
		else
		if ( IsInFrontV1 == IsInFrontV2 )
		{
			HandleVert( v1, v3, out var newV2, plane.Trace( new Ray( v1.Position, v1.Position - v3.Position ), true ) ?? 0 );
			HandleVert( v2, v3, out var newV1, plane.Trace( new Ray( v2.Position, v2.Position - v3.Position ), true ) ?? 0 );
			edge1.Add( newV1 );

			if ( IsInFrontV1 == IsInFront.Positive )
			{
				vB1.AddTriangle( v2, newV1, v1 );
				vB1.AddTriangle( newV1, newV2, v1 );

				vB2.AddTriangle( newV1, v3, newV2 );
			}
			else
			{
				vB2.AddTriangle( v2, newV1, v1 );
				vB2.AddTriangle( newV1, newV2, v1 );

				vB1.AddTriangle( newV1, v3, newV2 );
			}
		}
		else
		if ( IsInFrontV1 == IsInFrontV3 )
		{
			HandleVert( v2, v1, out var newV2, plane.Trace( new Ray( v1.Position, v1.Position - v2.Position ), true ) ?? 0 );
			HandleVert( v2, v3, out var newV1, plane.Trace( new Ray( v3.Position, v3.Position - v2.Position ), true ) ?? 0 );
			edge1.Add( newV1 );

			if ( IsInFrontV1 == IsInFront.Positive )
			{
				vB1.AddTriangle( v1, newV1, v3 );
				vB1.AddTriangle( v1, newV2, newV1 );

				vB2.AddTriangle( newV2, v2, newV1 );
			}
			else
			{
				vB2.AddTriangle( v1, newV1, v3 );
				vB2.AddTriangle( v1, newV2, newV1 );

				vB1.AddTriangle( newV2, v2, newV1 );
			}
		}
		else
		{


			HandleVert( v2, v1, out var newV2, plane.Trace( new Ray( v1.Position, v2.Position - v1.Position ), true ) ?? 0 );
			HandleVert( v3, v1, out var newV1, plane.Trace( new Ray( v1.Position, v3.Position - v1.Position ), true ) ?? 0 );
			edge1.Add( newV1 );
			edge1.Add( newV2 );

			if ( IsInFrontV3 == IsInFront.Positive )
			{
				vB1.AddTriangle( v2, v3, newV1 );
				vB1.AddTriangle( newV1, newV2, v2 );

				vB2.AddTriangle( newV1, v1, newV2 );
			}
			else
			{
				vB2.AddTriangle( v2, v3, newV1 );
				vB2.AddTriangle( newV1, newV2, v2 );

				vB1.AddTriangle( newV1, v1, newV2 );
			}
		}
	}

	private List<Vector3> HandleHull( CutBuffer VB2, CutBuffer cutpart2 )
	{
		var firstConvexHull = ConvexHull.Create( VB2.Vertex.ConvertAll<ConvexHullPos>( x => x ) );

		List<Vector3> hullpoints = new();

		if ( firstConvexHull.Outcome == ConvexHullCreationResultOutcome.Success )
			foreach ( var item in firstConvexHull.Result.Faces )
			{
				Vector3 normal = new( (float)item.Normal[0], (float)item.Normal[1], (float)item.Normal[2] );
				ConvexHullPos[] list = item.Vertices;


				Vector3 Center = GetCenter( list );
				if ( Planes.Any( pn => normal.Abs().Angle( pn.Normal.Abs() ) < 90 && pn.SnapToPlane( Center ).Distance( Center ) < 0.5f ) )
				{
					for ( int i = 0; i < list.Length; i += 3 )
					{
						Vertex vert1 = list[i];
						vert1.Normal = normal;
						vert1.Tangent = new Vector4( normal, -1f );
						Vertex vert2 = list[i + 1];
						vert2.Normal = normal;
						vert2.Tangent = new Vector4( normal, -1f );
						Vertex vert3 = list[i + 2];
						vert3.Normal = normal;
						vert3.Tangent = new Vector4( normal, -1f );
						cutpart2.AddTriangle( vert1, vert2, vert3 );
					}
				}

				hullpoints.AddRange( list.Select( x => x.Pos ) );
			}

		return hullpoints;
	}

	private Vector3 GetCenter( ConvexHullPos[] points )
	{
		Vector3 center = Vector3.Zero;
		foreach ( var point in points )
			center += point;
		return center / points.Length;
	}


	private void HandleVert( Vertex v1, Vertex v2, out Vertex newV, Vector3 position )
	{
		float v1v2lenght = (v2.Position - v1.Position).Length;
		float distance = (v1.Position - position).Length / v1v2lenght;

		newV = new()
		{
			Position = position,
			Normal = v1.Normal.LerpTo( v2.Normal, distance ),
			Tangent = v1.Tangent.LerpTo( v2.Tangent, distance ),
			TexCoord0 = v1.TexCoord0.LerpTo( v2.TexCoord0, distance ),
			TexCoord1 = v1.TexCoord1.LerpTo( v2.TexCoord1, distance ),
			TexCoord2 = v1.TexCoord2.LerpTo( v2.TexCoord2, distance ),
			Color = v1.Color
		};
	}

	struct ConvexHullPos : IVertex
	{

		public Vector3 Pos;

		double[] IVertex.Position => new double[] { Pos.x, Pos.y, Pos.z };


		public static implicit operator ConvexHullPos( Vertex point )
		{
			return new ConvexHullPos { Pos = point.Position };
		}

		public static implicit operator Vertex( ConvexHullPos point )
		{
			return new Vertex { Position = point.Pos };
		}


		public static implicit operator Vector3( ConvexHullPos point )
		{
			return point.Pos;
		}
		public static implicit operator ConvexHullPos( Vector3 point )
		{
			return new ConvexHullPos { Pos = point };
		}
	}
}
