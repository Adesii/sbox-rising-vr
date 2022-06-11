using System;
using System.Text.Json.Serialization;

namespace ACuttingBox.Properties;

[ModelDoc.GameData( "CuttingBlade" )]
[ModelDoc.Axis( Origin = "start_origin" )]
[ModelDoc.Axis( Origin = "end_origin" )]
[ModelDoc.Axis( Origin = "holding_origin", Angles = "holding_angles" )]
public class CuttingBladeProperties
{
	[JsonPropertyName( "start_origin" )]
	public Vector3 StartBlade { get; set; }


	[JsonPropertyName( "end_origin" )]
	public Vector3 EndBlade { get; set; }


	[JsonPropertyName( "holding_origin" )]
	public Vector3 HoldPosition { get; set; }

	[JsonPropertyName( "holding_angles" )]
	public Angles HoldAngles { get; set; }
}
