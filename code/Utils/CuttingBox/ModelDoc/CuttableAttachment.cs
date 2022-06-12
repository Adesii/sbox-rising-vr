using System.Text.Json.Serialization;

namespace ACuttingBox.Properties;

[ModelDoc.GameData( "CuttableAttachment", AllowMultiple = true )]
[ModelDoc.Axis( Origin = "cut_origin", Angles = "cut_angles_hinge" )]
[ModelDoc.HingeJoint( MinAngle = "min_angle", MaxAngle = "max_angle", EnableLimit = "enable_limit", Origin = "cut_origin", Angles = "cut_angles_hinge", Attachment = "cut_attach_parent" )]
public class CuttableAttachment
{
	[JsonPropertyName( "cut_origin" )]
	public Vector3 AttachPosition { get; set; }

	[JsonPropertyName( "cut_attach_parent" )]
	public string AttachPoint { get; set; }
	[JsonPropertyName( "cut_angles_hinge" )]
	public Angles Angles { get; set; }

	/// <summary>
	/// Whether the angle limit should be enabled or not.
	/// </summary>
	[JsonPropertyName( "enable_limit" )]
	public bool LimitAngles { get; set; }
	[JsonPropertyName( "min_angle" ), MinMax( -179, 179 )]
	public float MinimumAngle { get; set; }
	[JsonPropertyName( "max_angle" ), MinMax( -179, 179 )]
	public float MaximumAngle { get; set; }
}
