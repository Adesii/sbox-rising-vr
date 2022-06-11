using System.Collections.Generic;

namespace ACuttingBox.Entities;

public class BaseCuttableSO : RenderEntity
{
	public VertexBuffer bm;
	public Material mm;
	public Material pass;
	public SceneLayerType layer;



	public static Texture MaskBuffer { get; set; }
	public BaseCuttableSO( SceneWorld sceneWorld, VertexBuffer bm, Material m, SceneLayerType layer = SceneLayerType.Opaque )/*  : base( sceneWorld ) */
	{
		this.bm = bm;
		this.mm = m;
		this.layer = layer;

		pass = Material.Load( "materials/postprocess/passthrough.vmat" );

		MaskBuffer = Texture.CreateRenderTarget().WithSize( Screen.Size ).WithScreenFormat().Create();



	}

	public override void UpdateSceneObject( SceneObject obj )
	{
		base.UpdateSceneObject( obj );
		switch ( layer )
		{
			case SceneLayerType.Unknown:
				break;
			case SceneLayerType.Translucent:
				obj.Flags.IsTranslucent = true;
				obj.Flags.IsOpaque = false;
				break;
			case SceneLayerType.UI:
				break;
			case SceneLayerType.SunShadow:
				break;
			case SceneLayerType.Shadow:
				break;
			case SceneLayerType.EffectsTranslucent:
				obj.Flags.IsTranslucent = true;
				obj.Flags.IsOpaque = false;
				break;
			case SceneLayerType.EffectsOpaque:
				obj.Flags.IsTranslucent = false;
				obj.Flags.IsOpaque = true;
				break;
			case SceneLayerType.DepthPrepass:
				break;
			case SceneLayerType.Opaque:
				obj.Flags.IsTranslucent = false;
				obj.Flags.IsOpaque = true;
				break;
			case SceneLayerType.PostProcess:
				break;
		}

	}

	public override void DoRender( SceneObject obj )
	{
		base.DoRender( obj );

		if ( Render.Layer == layer && RenderingEnabled )
		{

			Render.CopyFrameBuffer();
			Render.CopyDepthBuffer();
			Render.Material = mm;
			Render.Attributes.Set( "Mask", MaskBuffer );
			//Render.Draw.ScreenQuad( new(), true );
			bm.Draw( mm );
		}
	}

	public bool RenderingEnabled { get; internal set; }

	/* public override void RenderSceneObject()
	{
		if ( bm == null ) return;
		if ( Render.Layer == layer )
		{



			using ( Render.RenderTarget( MaskBuffer ) )
			{
				Render.CopyFrameBuffer();
				Render.CopyDepthBuffer();
				RenderAttributes attributes = Render.Attributes;
				string k = "D_NO_UV";
				int value = 0;
				attributes.SetCombo( in k, in value );
				Render.Material = pass;
				Render.Draw.ScreenQuad( Render.Attributes, true );
			}
	Render.CopyFrameBuffer();
			Render.CopyDepthBuffer();
			Render.Material = mm;
			Render.Attributes.Set( "Mask", MaskBuffer );
			//Render.Draw.ScreenQuad( new(), true );
			bm.Draw( mm );
		}
	} */

}
