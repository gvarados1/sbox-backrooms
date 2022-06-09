
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace ProcGen;

/// <summary>
/// This is your game class. This is an entity that is created serverside when
/// the game starts, and is replicated to the client. 
/// 
/// You can use this to create things like HUDs and declare which player class
/// to use for spawned players.
/// </summary>
public partial class Backrooms : Sandbox.Game
{
	public Backrooms()
	{
		if ( IsServer )
		{
			Log.Info( "My Gamemode Has Created Serverside!" );

			// Create a HUD entity. This entity is globally networked
			// and when it is created clientside it creates the actual
			// UI panels. You don't have to create your HUD via an entity,
			// this just feels like a nice neat way to do it.
			new MinimalHudEntity();
		}

		if ( Host.IsClient )
		{
			Log.Info( "My Gamemode Has Created Clientside!" );
			InitPostProcess();
		}
	}

	// original postprocessing stolen from devultj :)
	public StandardPostProcess StandardPostProcess { get; set; }

	private float distanceLerp = 0f;
	protected void PushGlobalPPSettings()
	{
		var pp = StandardPostProcess;
		pp.ChromaticAberration.Enabled = true;
		//pp.ChromaticAberration.Offset = new Vector3( -0.0007f, -0.0007f, 0f );
		//pp.ChromaticAberration.Offset = new Vector3( -0.003f, -0.004f, 0.001f );
		pp.ChromaticAberration.Offset = new Vector3( -0.0015f, -0.002f, 0.0005f );

		pp.MotionBlur.Enabled = true;
		pp.MotionBlur.Scale = 0.05f;
		pp.MotionBlur.Samples = 5;

		pp.Vignette.Enabled = true;
		pp.Vignette.Color = Color.Black;
		pp.Vignette.Roundness = 1.5f;
		pp.Vignette.Intensity = 1.5f;

		pp.Saturate.Enabled = true;
		pp.Saturate.Amount = 0.95f;

		//pp.PaniniProjection.Enabled = true;
		//pp.PaniniProjection.Amount = 1.05f;

		pp.HueRotate.Enabled = true;
		pp.HueRotate.Angle = 355;

		pp.FilmGrain.Enabled = true;
		pp.FilmGrain.Intensity = .2f;//0.12f;

		//pp.ColorOverlay.Enabled = false;
		//pp.ColorOverlay.Amount = 0.1f;
		//pp.ColorOverlay.Color = new Color( 0.1f, 0.1f, 0.2f );
		//pp.ColorOverlay.Mode = StandardPostProcess.ColorOverlaySettings.OverlayMode.Additive;
	}

	protected async void InitPostProcess()
	{
		StandardPostProcess = new();

		PostProcess.Add( StandardPostProcess );

		await Task.Delay( 1000 );
		PlaySound( "fluorescent_hum" );
	}

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );

		camSetup.ZFar = 3000f;//5000f;
	}

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );
		var player = new BackroomsPlayer( cl );
		player.Respawn();

		cl.Pawn = player;
	}

	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();
		var procmanager = new ProcGenManager();
		procmanager.GenerateWorld();

		foreach ( var ply in Entity.All.OfType<BackroomsPlayer>().ToArray() )
		{
			ply.MoveToWall();
		}

		var ghost = new Ghost();
	}

	public override void FrameSimulate( Client cl )
	{
		Host.AssertClient();

		if ( !cl.Pawn.IsValid() ) return;

		// Block Simulate from running clientside
		// if we're not predictable.
		if ( !cl.Pawn.IsAuthority ) return;

		cl.Pawn?.FrameSimulate( cl );

		PushGlobalPPSettings();
	}
}
