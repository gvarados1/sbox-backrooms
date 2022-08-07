using Sandbox.UI;

namespace ProcGen;

public class HudRootPanel : RootPanel
{
	public static HudRootPanel Current;

	public HudRootPanel()
	{
		Current = this;

		StyleSheet.Load( "/resource/styles/hud.scss" );
		SetTemplate( "/resource/templates/hud.html" );

		AddChild<ChatBox>();
		AddChild<VoiceList>();
		AddChild<VoiceSpeaker>();
	}
}
