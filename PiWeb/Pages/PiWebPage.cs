﻿using Ooui;
using TabNoc.MyOoui.Interfaces.AbstractObjects;
using TabNoc.MyOoui.Interfaces.Enums;

namespace TabNoc.PiWeb.Pages
{
	internal class PiWebPage : StylableElement
	{
		public PiWebPage() : base("div")
		{
			AddStyling(StylingOption.MarginTop, 5);
			AddStyling(StylingOption.MarginLeft, 5);
			Heading heading = new Heading(1, "Willkommen auf PiWeb.");
			Heading heading2 = new Heading(4, "Aktuell gibt es folgende Projekte:");
			List uList = new List();
			ListItem listItem = new ListItem();
			listItem.AppendChild(new Anchor("/overview", "WateringWeb"));
			uList.AppendChild(listItem);

			//listItem = new ListItem();
			//listItem.AppendChild(new Anchor("/test", "Test"));
			//uList.AppendChild(listItem);

			AppendChild(heading);
			AppendChild(heading2);
			AppendChild(uList);

			//AppendChild(new TabNoc.MyOoui.UiComponents.Chart(400, 400));
		}
	}
}
