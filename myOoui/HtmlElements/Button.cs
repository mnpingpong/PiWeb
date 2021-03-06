﻿using System;
using TabNoc.MyOoui.Interfaces.AbstractObjects;
using TabNoc.MyOoui.Interfaces.Enums;

namespace TabNoc.MyOoui.HtmlElements
{
	public class Button : StylableElement
	{
		public Button(StylingColor buttonType = StylingColor.Primary, bool asOutline = false, ButtonSize size = ButtonSize.Normal, bool asBlock = false, string text = "", int widthInPx = -1, string fontAwesomeIcon = "") : base("button")
		{
			SetAttribute("type", "button");
			ClassName = $"btn btn-{(asOutline == true ? "outline-" : "")}{Enum.GetName(typeof(StylingColor), buttonType).ToLower()}";
			ClassName += (size == ButtonSize.Large ? " btn-lg" : size == ButtonSize.Small ? " btn-sm" : "");
			ClassName += (asBlock == true ? " btn-block" : "");
			if (string.IsNullOrWhiteSpace(text) == false)
			{
				Text = text;
			}

			if (widthInPx > 0)
			{
				Style.Width = widthInPx;
			}
			SetFontAwesomeIcon(fontAwesomeIcon);
		}

		public void SetFontAwesomeIcon(string fontAwesomeIcon)
		{
			if (fontAwesomeIcon != "")
			{
				InsertBefore(new MyIElement() { ClassName = "fas fa-" + fontAwesomeIcon + " mr-2" }, FirstChild);
			}
		}

		public enum ButtonSize
		{
			Small,
			Normal,
			Large
		}
	}
}
