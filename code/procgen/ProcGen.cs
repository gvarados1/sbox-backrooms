using Sandbox;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ProcGen;

public struct ProcGrid
{
	public ProcGrid( int x, int y )
	{

	}
}

partial class ProcGenManager : Entity
{
	public static int TileSize = 128;

	public static int GridX = 50; // used to be 80x80
	public static int GridY = 60;
	public bool[,] Grid { get; set; }

	public bool IsGenerating = false;
	public float WallCount = 0;

	public override void Spawn()
	{
		base.Spawn();

		Grid = new bool[GridX, GridY];
	}

	public TimeSince TimeSinceReset = 0;

	[Event.Tick.Server]
	public void Tick()
	{
		//if(TimeSinceReset > 240 )
		//{
		//	TimeSinceReset = 0 - Rand.Int( 30 );
		//	GenerateWorld();
		//}

		if ( IsGenerating )
		{
			DebugOverlay.ScreenText( $"Generating World: {(int)(WallCount/19f)}%" ); //41
			DebugOverlay.ScreenText( $"Recent S&box updates makes this take forever. It used to be instant...", 1 );
		}
	}
	public void GenerateWorld()
	{
		WallCount = 0;
		IsGenerating = true;
		ClearWorld();
		ResetGrid();
		GenerateGrid();
		//AddPlayerTiles();
		FillGaps();
		RemoveIslands();
		AddPillars();
		//PopulateFloor();
		PopulateWalls();
		RespawnPlayers();
	}
	public void GenerateGrid()
	{
		for( int xx = 0; xx < GridX; xx++ )
		{
			for(int yy = 0; yy < GridY; yy++ )
			{
				Grid[xx, yy] = Rand.Int( 1 ) == 1 ? true : false;
			}
		}
		Log.Info( "Grid Generated!" );
	}

	public void AddPlayerTiles()
	{
		foreach ( var ply in Entity.All.OfType<Player>().ToArray())
		{
			var plyx = (int)Math.Round(ply.Position.x / (float)TileSize);
			var plyy = (int)Math.Round(ply.Position.y / (float)TileSize);

			Grid[plyx, plyy] = true;
		}
	}

	public void RespawnPlayers()
	{
		foreach ( var ply in Entity.All.OfType<Player>().ToArray() )
		{
			ply.Respawn();
		}
	}

	public void RemoveIslands()
	{
		for ( int xx = 0; xx < GridX; xx++ )
		{
			for ( int yy = 0; yy < GridY; yy++ )
			{
				if ( Grid[xx, yy] )
				{
					int neighbors = 4;
					// count neighbors
					if ( xx + 2 > GridX || !Grid[xx + 1, yy] )
					{
						neighbors--;
					}
					if ( xx - 1 < 0 || !Grid[xx - 1, yy] )
					{
						neighbors--;
					}
					if ( yy + 2 > GridY || !Grid[xx, yy + 1] )
					{
						neighbors--;
					}
					if ( yy - 1 < 0 || !Grid[xx, yy - 1] )
					{
						neighbors--;
					}
					if(neighbors <= 1 )
					{
						Grid[xx,yy] = false;
						//Log.Warning( neighbors );
					}
				}
			}
		}
		Log.Info( "Islands Removed!" );
	}

	public void FillGaps()
	{
		for ( int xx = 0; xx < GridX; xx++ )
		{
			for ( int yy = 0; yy < GridY; yy++ )
			{
				if ( !Grid[xx, yy] )
				{
					int neighbors = 4;
					// count neighbors
					if ( xx + 2 > GridX || !Grid[xx + 1, yy] )
					{
						neighbors--;
					}
					if ( xx - 1 < 0 || !Grid[xx - 1, yy] )
					{
						neighbors--;
					}
					if ( yy + 2 > GridY || !Grid[xx, yy + 1] )
					{
						neighbors--;
					}
					if ( yy - 1 < 0 || !Grid[xx, yy - 1] )
					{
						neighbors--;
					}
					if ( neighbors >= 3 )
					{
						Grid[xx, yy] = true;
						//Log.Warning( neighbors );
					}
				}
			}
		}
		Log.Info( "Gaps Filled!" );
	}

	public void AddPillars()
	{
		for ( int xx = 0; xx < GridX; xx++ )
		{
			for ( int yy = 0; yy < GridY; yy++ )
			{
				if ( Grid[xx, yy] )
				{
					int neighbors = 4;
					// count neighbors
					if ( xx + 2 > GridX || !Grid[xx + 1, yy] )
					{
						neighbors--;
					}
					if ( xx - 1 < 0 || !Grid[xx - 1, yy] )
					{
						neighbors--;
					}
					if ( yy + 2 > GridY || !Grid[xx, yy + 1] )
					{
						neighbors--;
					}
					if ( yy - 1 < 0 || !Grid[xx, yy - 1] )
					{
						neighbors--;
					}
					if ( neighbors == 1)
					{
						Grid[xx, yy] = false;
						Log.Warning( neighbors );
					}
					if ( neighbors == 4 && Rand.Int( 5 ) == 1 )
					{
						Grid[xx, yy] = false;
						Log.Warning( neighbors );
					}
				}
			}
		}
		Log.Info( "Pillars Added!" );
	}

	public void PopulateFloor()
	{
		for ( int xx = 0; xx < GridX; xx++ )
		{
			for ( int yy = 0; yy < GridY; yy++ )
			{
				if(Grid[xx, yy])
				{
					var model = new ProcModel();
					model.Position = new Vector3( (float)xx * TileSize, (float)yy * TileSize, 0f );
					model.SetModel( "models/procgen/floor_tile_01a.vmdl" );
					model.SetupPhysicsFromModel( PhysicsMotionType.Static );
				}
			}
		}
		Log.Info( "Floor Populated!" );
	}

	public async void PopulateWalls()
	{
		for ( int xx = 0; xx < GridX; xx++ )
		{
			await Task.Delay( 1 );
			for ( int yy = 0; yy < GridY; yy++ )
			{
				if ( Grid[xx, yy] )
				{
					var lastWallAngle = 0;
					int walls = 0;
					// forward
					if(xx + 2 > GridX ){
						CreateWall( xx, yy, 0 );
						walls++;
					}
					else if(!Grid[xx + 1, yy] )
					{
						CreateWall( xx, yy, 0 );
						walls++;
					}
					// backwards
					if ( xx - 1 < 0 )
					{
						CreateWall( xx, yy, 180 );
						lastWallAngle = 180;
						walls++;
					}
					else if ( !Grid[xx - 1, yy] )
					{
						CreateWall( xx, yy, 180 );
						lastWallAngle = 180;
						walls++;
					}
					// left
					if ( yy + 2 > GridY )
					{
						CreateWall( xx, yy, 90 );
						lastWallAngle = 90;
						walls++;
					}
					else if ( !Grid[xx, yy + 1] )
					{
						CreateWall( xx, yy, 90 );
						lastWallAngle = 90;
						walls++;
					}
					// right
					if ( yy - 1 < 0 )
					{
						CreateWall( xx, yy, 270 );
						lastWallAngle = 270;
						walls++;
					}
					else if ( !Grid[xx, yy - 1] )
					{
						CreateWall( xx, yy, 270 );
						lastWallAngle = 270;
						walls++;
					}

					// todo: move this prop placement elsewhere and make it better
					if(walls > 1)
					{
						if ( Rand.Int( 3 ) == 1 )
						{
							var prop = new Prop();
							prop.SetModel( "models/sbox_props/wooden_crate/wooden_crate.vmdl" );
							prop.Position = new Vector3( (float)xx * TileSize, (float)yy * TileSize, 5f );
							prop.Rotation = Rotation.Random;
							prop.Velocity = Vector3.Random * 1000 * Rand.Float( 5 );
						}
						else if ( Rand.Int( 15 ) == 1 )
						{
							var prop = new Prop();
							prop.SetModel( "models/sbox_props/office_chair/office_chair.vmdl" );
							prop.Position = new Vector3( (float)xx * TileSize, (float)yy * TileSize, 30f );
							prop.Rotation = Rotation.Random;
							prop.Velocity = Vector3.Random * 1000 * Rand.Float( 4 );
						}
					}

					if ( walls == 1 )
					{
						if ( Rand.Int( 10 ) == 1 )
						{
							var wallExtention = CreateWall( xx, yy, lastWallAngle );
							wallExtention.SetModel( "models/procgen/backrooms_wall_extention_01a.vmdl" );
						}
					}
					if ( walls == 1 )
					{
						if ( Rand.Int( 40 ) == 1 )
						{
							var wallExtention = CreateWall( xx, yy, lastWallAngle );
							wallExtention.SetModel( "models/backrooms/office_desk/office_desk.vmdl" );
						}
					}
				}
			}
		}
		Log.Info( "Walls Populated!" );

		RespawnPlayers();
		await Task.DelaySeconds( 5 );
		IsGenerating = false;
	}

	public static readonly List<String> WallModels = new List<String>()
	{
		"models/procgen/backrooms_wall_tile_01a.vmdl",
		"models/procgen/backrooms_wall_tile_01a.vmdl",
		"models/procgen/backrooms_wall_tile_01a.vmdl",
		"models/procgen/backrooms_wall_tile_01a.vmdl",
		"models/procgen/backrooms_wall_tile_01a.vmdl",
		"models/procgen/backrooms_wall_tile_01a.vmdl",
		"models/procgen/backrooms_wall_tile_01a.vmdl",
		"models/procgen/backrooms_wall_tile_01a.vmdl",
		"models/procgen/backrooms_wall_tile_01a.vmdl",
		"models/procgen/backrooms_wall_tile_01a.vmdl",
		"models/procgen/backrooms_wall_tile_01a.vmdl",
		"models/procgen/backrooms_wall_tile_01a.vmdl",
		"models/procgen/backrooms_wall_tile_01a.vmdl",
		"models/procgen/backrooms_wall_tile_01a.vmdl",
		"models/procgen/backrooms_wall_tile_01a.vmdl",
		"models/procgen/backrooms_wall_tile_01a.vmdl",
		"models/procgen/backrooms_wall_tile_01a.vmdl",
		"models/procgen/backrooms_wall_tile_01a.vmdl",
		"models/procgen/backrooms_wall_tile_01a.vmdl",
		"models/procgen/backrooms_wall_tile_01a.vmdl",

		"models/procgen/backrooms_wall_tile_02a.vmdl",
		"models/procgen/backrooms_wall_tile_02a.vmdl",
		"models/procgen/backrooms_wall_tile_02a.vmdl",
		"models/procgen/backrooms_wall_tile_02a.vmdl",
		"models/procgen/backrooms_wall_tile_02a.vmdl",
		"models/procgen/backrooms_wall_tile_02a.vmdl",
		"models/procgen/backrooms_wall_tile_02a.vmdl",
		"models/procgen/backrooms_wall_tile_02a.vmdl",

		"models/procgen/backrooms_wall_tile_03a.vmdl",
		"models/procgen/backrooms_wall_tile_03a.vmdl",
		"models/procgen/backrooms_wall_tile_03a.vmdl",
		"models/procgen/backrooms_wall_tile_03a.vmdl",

		"models/procgen/backrooms_wall_tile_04a.vmdl"
	};


	public ProcModel CreateWall(int xx, int yy, int dir)
	{
		WallCount++;
		var model = new ProcModel();
		model.Position = new Vector3( (float)xx * TileSize, (float)yy * TileSize, 0f );
		model.Rotation = new Angles( 0, dir, 0 ).ToRotation();
		model.SetModel( WallModels.OrderBy( x => Guid.NewGuid() ).FirstOrDefault() );
		model.SetupPhysicsFromModel( PhysicsMotionType.Static );
		return model;
	}

	public void ResetGrid()
	{
		Grid = new bool[GridX, GridY];
		Log.Info( "Grid Reset!" );
	}

	[ConCmd.Server( "proc_generate" )]
	public static void Generate()
	{
		foreach ( var generator in Entity.All.OfType<ProcGenManager>().ToArray() )
		{
			generator.GenerateWorld();
		}
	}

	[ConCmd.Server( "proc_clear" )]
	public static void ClearWorld()
	{
		foreach( var model in Entity.All.OfType<ProcModel>().ToArray())
		{
			model.Delete();
		}

		foreach ( var model in Entity.All.OfType<Prop>().ToArray() )
		{
			model.Delete();
		}
		Log.Info( "World Cleared!" );
	}
}
