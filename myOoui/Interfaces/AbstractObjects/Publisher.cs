﻿using Ooui;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TabNoc.MyOoui.Interfaces.Enums;
using TabNoc.MyOoui.UiComponents;

namespace TabNoc.MyOoui.Interfaces.AbstractObjects
{
	public abstract class Publisher : Publishable
	{
		protected readonly string PublishPath;

		protected Publisher(string publishPath) : base(publishPath)
		{
			PublishPath = publishPath;
		}

		protected abstract NavigationBar CreateNavigationBar();

		protected abstract Element CreatePage();

		protected abstract void Initialize();

		protected override Element PopulateAppElement()
		{
			Stopwatch stopwatchMain = new Stopwatch();
			stopwatchMain.Start();
			Div testDiv = new Div();
			//Grid grid = new Grid();

			Loading loading = new Loading();
			//grid.AppendChild(loading);
			testDiv.AppendChild(loading);

			NavigationBar navigationBar = CreateNavigationBar();
			if (navigationBar != null)
			{
				navigationBar.AddStyling(StylingOption.MarginBottom, 3);
				//grid.AddRow().AppendCollum(navigationBar);
				testDiv.AppendChild(navigationBar);
			}

			Stopwatch stopwatchCreatePage = new Stopwatch();

			Element WaitForInitAndCreatePage()
			{
				try
				{
					Initialize();
				}
				catch (Exception e)
				{
					Error(e, "Beim initialisieren der Seite ist auf dem Server ein Fehler aufgetreten.", loading);
					throw;
				}
				try
				{
					return CreatePage();
				}
				catch (Exception e)
				{
					Error(e, "Beim erstellen der Seite ist auf dem Server ein Fehler aufgetreten.", loading);
					throw;
				}
			}

			Task.Run((Func<Element>)WaitForInitAndCreatePage).ContinueWith(task =>
			{
				testDiv.AppendChild(task.Result);
				stopwatchCreatePage.Stop();
				Console.WriteLine($"\t\tSended Website Content: {testDiv.OuterHtml.Length}Byte\r\n\t\tElapsedTime from CreatePage: {stopwatchCreatePage.ElapsedMilliseconds}ms");
				testDiv.RemoveChild(loading);
			});

			stopwatchMain.Stop();
			Console.WriteLine($"\tSended Website Content: {testDiv.OuterHtml.Length}Byte\r\n\tElapsedTime in PopulateAppElement: {stopwatchMain.ElapsedMilliseconds}ms");
			stopwatchCreatePage.Start();
			return testDiv;
		}

		private void Error(Exception exception, string msg, Loading loading)
		{
			loading.ShowError(exception, msg);
			Console.WriteLine("\r\n\r\n");
			Console.WriteLine(msg);
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(exception.ToString());
			Console.ResetColor();
			Console.WriteLine("\r\n\r\n");
		}
	}
}
