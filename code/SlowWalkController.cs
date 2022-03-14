using Sandbox;

namespace ProcGen;
[Library]
public partial class SlowWalkController : WalkController
{
	public SlowWalkController()
	{
		SprintSpeed = 260f;
		WalkSpeed = 120f;
		DefaultSpeed = 170f;

		Duck = new Duck( this );
		Unstuck = new Unstuck( this );
	}
}
