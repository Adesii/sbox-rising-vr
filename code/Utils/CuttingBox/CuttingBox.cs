using System;
using ACuttingBox.Buffers;
using ACuttingBox.Interfaces;
using ACuttingBox.Builders;
using ACuttingBox.Properties;

namespace ACuttingBox;

public class CuttingBox<T> where T : CuttingAlgorithm
{
	public T Algorithm;

	public List<CutBuffer> Result => Algorithm.result;
	public List<CutBuffer> HoleResult => Algorithm.holeresult;

	public List<Model> Models => Algorithm.ResultModels;

	public List<Vector3> Centers => Algorithm.ResultCenters;

	public CuttableProperties ModelProperties => Algorithm.ModelProperties;

	public CuttableAttachment[] Attachments { get; set; }
	public Transform[] AttachmentsPositions { get; set; }

	public CuttingBox()
	{
	}

	public CuttingBox( T algorithm )
	{
		this.Algorithm = algorithm;
	}

}

public static class CuttingBox
{
	public static PlaneCutBuilder CreatePlaneCut()
	{
		return new PlaneCutBuilder();
	}
}
