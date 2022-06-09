using Sandbox;
using System;
using System.Linq;

namespace ProcGen;
partial class BackroomsPlayer : Player
{
	private DamageInfo lastDamage;

	public ClothingContainer Clothing = new();

	private AnimatedEntity fog_dome;

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

		Controller = new SlowWalkController();
		Animator = new StandardPlayerAnimator();
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

		SimulateAnimatorSounds();

		//
		// If you have active children (like a weapon etc) you should call this to 
		// simulate those too.
		//
		SimulateActiveChild( cl, ActiveChild );
	}

	public override void FrameSimulate( Client cl )
	{
		// dome to hard-cap the fog. Is this a hack?
		if ( fog_dome == null )
		{
			fog_dome = new AnimatedEntity();
			fog_dome.SetModel( "models/backrooms/game/fog_dome.vmdl" );
			fog_dome.Owner = Owner;
		}
		fog_dome.Position = CameraMode.Position;
		base.FrameSimulate( cl );
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

	TimeSince timeSinceLastFootstep = 0;

	public override float FootstepVolume() => Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 260f );
	public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( LifeState != LifeState.Alive )
			return;

		if ( !IsServer )
			return;

		if ( timeSinceLastFootstep < 0.2f )
			return;

		volume *= FootstepVolume();

		timeSinceLastFootstep = 0;

		var tr = Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;

		tr.Surface.DoFootstep( this, tr, foot, volume * 10 );
	}

	private TimeSince TimeSinceGroundedSound = 0f;
	private void SimulateAnimatorSounds()
	{
		if ( !IsClient ) return;

		using var _ = Prediction.Off();

		if ( Animator.HasEvent( "jump" ) && TimeSinceGroundedSound > .2f )
		{
			Sound.FromEntity( "footstep-concrete", this );
		}

		if ( Animator.HasEvent( "grounded" ) )
		{
			Sound.FromEntity( "footstep-concrete-land", this );
			TimeSinceGroundedSound = 0f;
		}
	}

	public override void PostCameraSetup( ref CameraSetup setup )
	{
		base.PostCameraSetup( ref setup );

		if ( setup.Viewer != null )
		{
			AddCameraEffects( ref setup );
		}
	}

	// walkbob
	float walkBob = 0;
	float lean = 0;
	float fov = 0;

	private void AddCameraEffects( ref CameraSetup setup )
	{
		var speed = Velocity.Length.LerpInverse( 0, 320 );
		var forwardspeed = Velocity.Normal.Dot( setup.Rotation.Forward );

		var left = setup.Rotation.Left;
		var up = setup.Rotation.Up;

		if ( GroundEntity != null )
		{
			walkBob += Time.Delta * 25.0f * speed;
		}

		setup.Position += up * MathF.Sin( walkBob ) * speed * 2;
		setup.Position += left * MathF.Sin( walkBob * 0.6f ) * speed * 1;

		// Camera lean
		lean = lean.LerpTo( Velocity.Dot( setup.Rotation.Right ) * 0.01f, Time.Delta * 15.0f );

		var appliedLean = lean;
		appliedLean += MathF.Sin( walkBob ) * speed * 0.3f;
		setup.Rotation *= Rotation.From( 0, 0, appliedLean );

		speed = (speed - 0.7f).Clamp( 0, 1 ) * 3.0f;

		fov = fov.LerpTo( speed * 20 * MathF.Abs( forwardspeed ), Time.Delta * 4.0f );

		setup.FieldOfView += fov;

	}
}
