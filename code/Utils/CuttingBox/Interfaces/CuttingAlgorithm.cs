using System.Threading.Tasks;
using ACuttingBox.Buffers;
using ACuttingBox.Entities;
using ACuttingBox.Properties;

namespace ACuttingBox.Interfaces;

public class CuttingAlgorithm
{
	public Model OriginalModel;
	public CutBuffer OriginalCutBuffer;

	public List<CutBuffer> result;
	public List<CutBuffer> holeresult;
	public List<List<Vector3>> HullPoints;

	public List<BaseCuttable> ResultCuttables;
	public List<Model> ResultModels;

	public List<BaseCuttable> Cuttables;

	public CuttableProperties ModelProperties { get; set; }
	public List<Vector3> ResultCenters { get; set; }

	public virtual async Task Cut()
	{
	}
}
