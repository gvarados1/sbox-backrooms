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

	public static int GridX = 80;
	public static int GridY = 80;
	public bool[,] Grid { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Grid = new bool[GridX, GridY];
	}

	public TimeSince TimeSinceReset = 0;

	[Event.Tick.Server]
	public void Tick()
	{
		if(TimeSinceReset > 240 )
		{
			TimeSinceReset = 0 - Rand.Int( 30 );
			GenerateWorld();
		}
	}
	public void GenerateWorld()
	{
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

	public void PopulateWalls()
	{
		for ( int xx = 0; xx < GridX; xx++ )
		{
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

					// todo: move this prop placement elsewhere
					if(walls > 1)
					{
						if ( Rand.Int( 3 ) == 1 )
						{
							var prop = new Prop();
							prop.SetModel( "models/citizen_props/crate01.vmdl" );
							prop.Position = new Vector3( (float)xx * TileSize, (float)yy * TileSize, 0f );
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

	[ServerCmd( "proc_generate" )]
	public static void Generate()
	{
		foreach ( var generator in Entity.All.OfType<ProcGenManager>().ToArray() )
		{
			generator.GenerateWorld();
		}
	}

	[ServerCmd( "proc_clear" )]
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
