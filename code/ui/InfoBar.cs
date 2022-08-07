using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace ProcGen;

internal class InfoBar : Panel
{
	public Label Header, Info;

	public InfoBar()
	{
		Header = Add.Label( string.Empty, "header" );
		Info = Add.Label( string.Empty, "info" );
	}

	public override void Tick()
	{
		base.Tick();

		var manager = ProcGenManager.Current;
		if ( !manager.IsValid ) return;

		//Header.Text = "- " + manager.RoundInfo;
		//Info.Text = manager.RoundName;
	}

}

