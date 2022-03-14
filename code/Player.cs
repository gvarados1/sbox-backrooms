using Sandbox;
using System;
using System.Linq;

namespace ProcGen;
partial class BackroomsPlayer : Player
{
	private DamageInfo lastDamage;

	public Clothing.Container Clothing = new();

	public BackroomsPlayer()
	{
		//asa
	}

	public BackroomsPlayer( Client cl ) : this()
	{
		// Load clothing from client data
		Clothing.LoadFromClient( cl );
	}

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		//
		// Use WalkController for movement (you can make your own PlayerController for 100% control)
		//
		Controller = new SlowWalkController();
		

		//
		// Use StandardPlayerAnimator  (you can make your own PlayerAnimator for 100% control)
		//
		Animator = new StandardPlayerAnimator();

		//
		// Use ThirdPersonCamera (you can make your own Camera for 100% control)
		//
		CameraMode = new FirstPersonCamera();

		Clothing.DressEntity( this );

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		base.Respawn();

		MoveToWall();
	}

	public virtual void MoveToWall()
	{
		// teleport to a random spot in the backrooms
		var spawnpoint = Entity.All
								.OfType<ProcModel>()               // get all SpawnPoint entities
								.OrderBy( x => Guid.NewGuid() )     // order them by random
								.FirstOrDefault();                  // take the first one

		if ( spawnpoint == null )
		{
			Log.Warning( $"Couldn't find spawnpoint for {this}!" );
			return;
		}

		Transform = spawnpoint.Transform;
	}

	/// <summary>
	/// Called every tick, clientside and serverside.
	/// </summary>
	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		//
		// If you have active children (like a weapon etc) you should call this to 
		// simulate those too.
		//
		SimulateActiveChild( cl, ActiveChild );
	}

	public override void OnKilled()
	{
		base.OnKilled();

		EnableDrawing = false;
		BecomeRagdollOnClient( Velocity, lastDamage.Flags, lastDamage.Position, lastDamage.Force, GetHitboxBone( lastDamage.HitboxIndex ) );
		CameraMode = new SpectateRagdollCamera();
	}
	public override void TakeDamage( DamageInfo info )
	{
		if ( GetHitboxGroup( info.HitboxIndex ) == 1 )
		{
			info.Damage *= 10.0f;
		}

		lastDamage = info;

		TookDamage( lastDamage.Flags, lastDamage.Position, lastDamage.Force );

		base.TakeDamage( info );
	}

	[ClientRpc]
	public void TookDamage( DamageFlags damageFlags, Vector3 forcePos, Vector3 force )
	{
	}

}
