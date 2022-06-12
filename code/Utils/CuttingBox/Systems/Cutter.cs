using System.Numerics;
using System.Collections.Generic;
using static Rising.VrGame;
using System;
using ACuttingBox.Entities;
using ACuttingBox.Builders;
using ACuttingBox.Algorithms;
using ACuttingBox.Buffers;
using ACuttingBox.Properties;
using Rising.Utils;
using Rising.Util;
using System.Diagnostics;

namespace ACuttingBox.Systems;

public static class Cutter
{

	public const float minsize = 25f;
	public const int maxchunksize = 500000;
	public static async void CutObject( this BaseCuttable HitObject, Vector3 Plane, Vector3 Position = new(), float ForcePush = 0f )
	{
		Stopwatch sw = new();
		sw.Start();
		var firstCuttable = CreateCuttable( HitObject, Plane, Position );
		var secondCuttable = CreateCuttable( HitObject, Plane, Position );

		if ( HitObject.LastCuttingBox == null )
			HitObject.LastCuttingBox = CuttingBox.CreatePlaneCut()
				.WithModel( HitObject.Model )
				.WithNormal( Plane )
				.WithPoint( Position ).Create();
		else
		{
			HitObject.LastCuttingBox.Algorithm.PlaneNormal = Plane;
			HitObject.LastCuttingBox.Algorithm.PlanePoint = Position;
		}
		await HitObject.LastCuttingBox.Algorithm.Cut();
		var cutbox = HitObject.LastCuttingBox;



		//TODO: figure out if there is a better way than this
		firstCuttable.LastCuttingBox = new()
		{
			Algorithm = new()
			{
				OriginalCutBuffer = cutbox.Result[0],
				ModelProperties = cutbox.ModelProperties,
				Planes = cutbox.Algorithm.Planes.ToList(),
			}
		};
		secondCuttable.LastCuttingBox = new()
		{
			Algorithm = new()
			{
				OriginalCutBuffer = cutbox.Result[1],
				ModelProperties = cutbox.ModelProperties,
				Planes = cutbox.Algorithm.Planes.ToList(),
			}
		};
		///////////////////////////////////////////////////////

		var models = cutbox.Models;
		if ( models[0] != null && models[0].RenderBounds.Size.Length >= minsize )
		{
			firstCuttable.Model = models[0];
			firstCuttable.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			SendChunkedData( firstCuttable, 1, cutbox.Result[0], cutbox.HoleResult[0], cutbox.ModelProperties );
		}
		else
		{
			firstCuttable?.Delete();
		}
		if ( models[1] != null && models[1].RenderBounds.Size.Length >= minsize )
		{
			secondCuttable.Model = models[1];
			secondCuttable.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			SendChunkedData( secondCuttable, 2, cutbox.Result[1], cutbox.HoleResult[1], cutbox.ModelProperties );

		}
		else
		{
			secondCuttable?.Delete();
		}
		if ( HitObject.UseAttachments )
			if ( HitObject.Model.HasData<CuttableAttachment[]>() )
			{
				var attachments = HitObject.Model.GetData<CuttableAttachment[]>();

				var firstattachments = new List<CuttableAttachment>();
				var firstattachmentsPositions = new List<Transform>();
				var secondattachments = new List<CuttableAttachment>();
				var secondattachmentsPositions = new List<Transform>();
				for ( int i = 0; i < attachments.Length; i++ )
				{
					CuttableAttachment item = attachments[i];
					var poss = HitObject.Transform.PointToWorld( item.AttachPosition );
					var rott = HitObject.Transform.RotationToWorld( item.Angles.ToRotation() );
					if ( poss.Distance( firstCuttable.Transform.PointToWorld( cutbox.Centers[0] ) ) <= poss.Distance( secondCuttable.Transform.PointToWorld( cutbox.Centers[1] ) ) )
					{
						var physjoint = PhysicsJoint.CreateHinge( firstCuttable.PhysicsBody, new PhysicsBody( Map.Physics )
						{
							Position = poss,
							Rotation = rott,
						}, poss, item.Angles.Direction );
						if ( item.LimitAngles )
						{
							physjoint.MinAngle = item.MinimumAngle;
							physjoint.MaxAngle = item.MaximumAngle;
						}
						firstCuttable.PhysicsJoints.Add( physjoint );
						firstattachments.Add( item );
						firstattachmentsPositions.Add( new Transform( poss, rott ) );
					}
					else
					{
						var physjoint = PhysicsJoint.CreateHinge( secondCuttable.PhysicsBody, new PhysicsBody( Map.Physics )
						{
							Position = poss,
							Rotation = rott,
						}, poss, item.Angles.Direction );
						if ( item.LimitAngles )
						{
							physjoint.MinAngle = item.MinimumAngle;
							physjoint.MaxAngle = item.MaximumAngle;
						}
						secondCuttable.PhysicsJoints.Add( physjoint );
						secondattachments.Add( item );
						secondattachmentsPositions.Add( new Transform( poss, rott ) );
					}
				}
				firstCuttable.LastCuttingBox.Attachments = firstattachments.ToArray();
				secondCuttable.LastCuttingBox.Attachments = secondattachments.ToArray();
				firstCuttable.LastCuttingBox.AttachmentsPositions = firstattachmentsPositions.ToArray();
				secondCuttable.LastCuttingBox.AttachmentsPositions = secondattachmentsPositions.ToArray();

			}
			else if ( cutbox.Attachments != null )
			{

				var attachments = cutbox.Attachments;
				var attachmentsPositions = cutbox.AttachmentsPositions;

				var firstattachments = new List<CuttableAttachment>();
				var firstattachmentsPositions = new List<Transform>();
				var secondattachments = new List<CuttableAttachment>();
				var secondattachmentsPositions = new List<Transform>();

				for ( int i = 0; i < attachments.Length; i++ )
				{
					CuttableAttachment item = attachments[i];
					Transform pos = attachmentsPositions[i];
					if ( (pos.Position).Distance( firstCuttable.Transform.PointToWorld( cutbox.Centers[0] ) ) <= (pos.Position).Distance( secondCuttable.Transform.PointToWorld( cutbox.Centers[1] ) ) )
					{
						if ( !firstCuttable.PhysicsBody.IsValid() ) continue;
						var physjoint = PhysicsJoint.CreateHinge( firstCuttable.PhysicsBody, new PhysicsBody( Map.Physics )
						{
							Position = pos.Position,
							Rotation = HitObject.Rotation * item.Angles.ToRotation(),
						}, pos.Position, item.Angles.Direction );
						if ( item.LimitAngles )
						{
							physjoint.MinAngle = item.MinimumAngle;
							physjoint.MaxAngle = item.MaximumAngle;
						}
						firstCuttable.PhysicsJoints.Add( physjoint );
						firstattachments.Add( item );
						firstattachmentsPositions.Add( pos );
					}
					else
					{
						if ( !secondCuttable.PhysicsBody.IsValid() ) continue;
						var physjoint = PhysicsJoint.CreateHinge( secondCuttable.PhysicsBody, new PhysicsBody( Map.Physics )
						{
							Position = pos.Position,
							Rotation = HitObject.Rotation * item.Angles.ToRotation(),
						}, pos.Position, item.Angles.Direction );
						if ( item.LimitAngles )
						{
							physjoint.MinAngle = item.MinimumAngle;
							physjoint.MaxAngle = item.MaximumAngle;
						}
						secondCuttable.PhysicsJoints.Add( physjoint );
						secondattachments.Add( item );
						secondattachmentsPositions.Add( pos );
					}
				}
				firstCuttable.LastCuttingBox.Attachments = firstattachments.ToArray();
				secondCuttable.LastCuttingBox.Attachments = secondattachments.ToArray();
				firstCuttable.LastCuttingBox.AttachmentsPositions = firstattachmentsPositions.ToArray();
				secondCuttable.LastCuttingBox.AttachmentsPositions = secondattachmentsPositions.ToArray();
			}



		//TODO: figure out a better solution to make the object move apart from the cut
		var ForcePushInDirectionofplane = Plane.Normal * ForcePush;
		if ( firstCuttable.IsValid() && secondCuttable.IsValid() )
		{
			firstCuttable.Velocity = ForcePushInDirectionofplane;
			firstCuttable.Velocity += HitObject.Velocity;
			secondCuttable.Velocity = -ForcePushInDirectionofplane;
			secondCuttable.Velocity += HitObject.Velocity;
		}
		HitObject.Delete();

		Log.Debug( $"CuttingBox took: {sw.ElapsedMilliseconds}ms. to cut {cutbox.Algorithm.OriginalCutBuffer.Vertex.Count} Vertices", 3 );
	}

	private static void SendChunkedData( BaseCuttable cutobject, int debugnum, CutBuffer res, CutBuffer hole, CuttableProperties modelp )
	{
		Log.Debug( $"{debugnum}: {res} {hole} {modelp}", 4 );

		var cutbuffer = res.SendCutBuffer().Compress();
		var holebuffer = hole.SendCutBuffer().Compress();
		var modelproperties = modelp.SendProperties();
		if ( cutbuffer.Length + holebuffer.Length >= maxchunksize )
		{
			var chunkCutBuffer = cutbuffer.Chunk( maxchunksize ).ToArray();
			var chunkHoleBuffer = holebuffer.Chunk( maxchunksize ).ToArray();
			int amountofchunks = chunkCutBuffer.Length > chunkHoleBuffer.Length ? chunkCutBuffer.Length : chunkHoleBuffer.Length;
			Log.Debug( $"{debugnum}: Sending chunked CutBuffer, Amount of Chunks: {chunkCutBuffer.Length}", 4 );
			for ( int i = 0; i <= amountofchunks; i++ )
			{
				byte[] item = chunkCutBuffer.Length > i ? chunkCutBuffer[i] : null;
				byte[] item2 = chunkHoleBuffer.Length > i ? chunkHoleBuffer[i] : null;
				Log.Debug( $"{debugnum}: Sending Chunk {i}/{amountofchunks}", 4 );
				cutobject.ReceiveChunkedCutBuffer( item, item2, modelproperties, amountofchunks, i );
			}
		}
		else
		{
			Log.Debug( $"{debugnum}: Sending Non-chunked CutBuffer", 4 );
			cutobject.ReceiveCutBuffer( cutbuffer, holebuffer, modelproperties );
		}
	}

	private static BaseCuttable CreateCuttable( BaseCuttable HitObject, Vector3 Plane, Vector3 Position = new() )
	{
		var cuttable = new BaseCuttable
		{
			Transform = HitObject.Transform,
			ModelProperties = HitObject.ModelProperties,
		};

		return cuttable;
	}
}
