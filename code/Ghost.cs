using Sandbox;
using System;
using System.Linq;

namespace ProcGen;

partial class Ghost : AnimatedEntity
{
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen/citizen.vmdl" );

		//DeleteAsync( 2 );
	}

	[Event.Tick.Server]
	public void Tick()
	{
		if ( Rand.Int( 120 ) == 0 )
		{
			TeleportToPlayer();
		}
	}

	public void TeleportToPlayer()
	{
		var target = Entity.All.OfType<Player>().OrderBy( x => Guid.NewGuid() ).FirstOrDefault();
		// surely there's a better way to do this
		var signx = Rand.Int( 1 ) == 0 ? -1 : 1;
		var signy = Rand.Int( 1 ) == 0 ? -1 : 1;

		ResetInterpolation();
		//Position = target.Position;

		Position = new Vector3( target.Position.x + signx * (600 + Rand.Int( 800 )), target.Position.y + signy * (600 + Rand.Int( 800 )), 0);
	}
}
