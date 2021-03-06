using System.Threading.Tasks;
using ACuttingBox.Algorithms;
using ACuttingBox.Buffers;
using ACuttingBox.Entities;

namespace ACuttingBox.Builders;

public struct PlaneCutBuilder
{
	private PlaneCut cut;

	public PlaneCutBuilder()
	{
		cut = new PlaneCut();
	}

	public PlaneCutBuilder WithOriginalCuttable( BaseCuttable cuttable )
	{
		cut.OriginalCuttable = cuttable;
		return this;
	}

	public PlaneCutBuilder WithNormal( Vector3 normal )
	{
		this.cut.PlaneNormal = normal;
		return this;
	}

	public PlaneCutBuilder WithPoint( Vector3 point )
	{
		this.cut.PlanePoint = point;
		return this;
	}

	public PlaneCutBuilder WithModel( Model model )
	{
		this.cut.OriginalModel = model;
		return this;
	}

	public PlaneCutBuilder WithCutBuffer( CutBuffer cutBuffer )
	{
		this.cut.OriginalCutBuffer = cutBuffer;
		return this;
	}

	public PlaneCutBuilder WithPlanes( List<Plane> planes )
	{
		this.cut.Planes = planes;
		return this;
	}

	public async Task<CuttingBox<PlaneCut>> Cut()
	{
		await cut.Cut();
		var res = new CuttingBox<PlaneCut>( cut );
		return res;
	}

	public CuttingBox<PlaneCut> Create()
	{
		var res = new CuttingBox<PlaneCut>( cut );
		return res;
	}
}
