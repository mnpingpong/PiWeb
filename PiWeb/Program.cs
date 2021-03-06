﻿using Ooui;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using TabNoc.MyOoui;
using TabNoc.MyOoui.Interfaces.AbstractObjects;
using TabNoc.MyOoui.Storage;
using TabNoc.MyOoui.UiComponents;
using TabNoc.PiWeb.DataTypes.WateringWeb.Channels;
using TabNoc.PiWeb.DataTypes.WateringWeb.History;
using TabNoc.PiWeb.DataTypes.WateringWeb.Settings;
using TabNoc.PiWeb.PagePublisher;
using TabNoc.PiWeb.PagePublisher.WateringWeb;

namespace TabNoc.PiWeb
{
	internal static class Program
	{
		public static void Error(string message, Exception ex)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("{0}: {1}", message, ex);
			Console.ResetColor();
			WriteLog("System", "Error", $"{message}: {ex}");
		}

		public static void WriteLog(string source, string status, string message, DateTime timestamp = default(DateTime))
		{
			if (timestamp == default(DateTime))
			{
				timestamp = DateTime.Now;
			}
#if DEBUG
			new HttpClient().PostAsync("http://localhost:5000/api/history",
				new StringContent(JsonConvert.SerializeObject(
					new HistoryElement(timestamp, source, status, message)), Encoding.UTF8, "application/json"));
#else
			new HttpClient().PostAsync($"http://{Dns.GetHostAddresses(Dns.GetHostEntry("172.18.0.2").HostName)[0].ToString()}:5000/api/history",
				new StringContent(JsonConvert.SerializeObject(
					new HistoryElement(timestamp, source, status, message)), Encoding.UTF8, "application/json"));
#endif
		}

		private static void Main(string[] args)
		{
#if DEBUG
			Console.WriteLine("Anwendung wird im Debug Modus gestartet!");
#else
			Console.WriteLine("Anwendung wird im Produktiv Modus gestartet!");
#endif
#if !DEBUG
			Console.WriteLine(Dns.GetHostEntry("172.18.0.2").HostName);
			Console.WriteLine(Dns.GetHostAddresses(Dns.GetHostEntry("172.18.0.2").HostName)[0].ToString());
#endif
			Logging.ErrorAction = Error;
			Logging.LogAction = (time, s, arg3, arg4) => WriteLog(s, arg3, arg4, time);
			UI.HeadHtml = "<script src=\"https://code.jquery.com/jquery-3.3.1.min.js\" crossorigin=\"anonymous\"></script>";
			UI.HeadHtml += "<script src=\"https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.3/umd/popper.min.js\" integrity=\"sha384-ZMP7rVo3mIykV+2+9J3UJ46jBk0WLaUAdn689aCwoqbBJiSnjAK/l8WvCWPIPm49\" crossorigin=\"anonymous\"></script>";
			UI.HeadHtml += "<script src=\"https://stackpath.bootstrapcdn.com/bootstrap/4.1.1/js/bootstrap.min.js\" integrity=\"sha384-smHYKdLADwkXOn1EmN1qk/HfnUcbVRZyYmZ4qpPea6sjB/pTJ0euyQp0Mk8ck+5T\" crossorigin=\"anonymous\"></script>";
			UI.HeadHtml += "<link rel=\"stylesheet\" href=\"https://stackpath.bootstrapcdn.com/bootstrap/4.1.1/css/bootstrap.min.css\" integrity=\"sha384-WskhaSGFgHYWDcbwN70/dfYBj47jz9qbsMId/iRN3ewGhXQFZCSftd1LZCfmhktB\" crossorigin=\"anonymous\">";

			UI.HeadHtml += "<link rel=\"stylesheet\" href=\"https://use.fontawesome.com/releases/v5.1.0/css/all.css\" integrity=\"sha384-lKuwvrZot6UHsBSfcMvOkWwlCMgc0TaWr+30HWe3a4ltaBwTZhyTEggF5tJv8tbt\" crossorigin=\"anonymous\">";

			BackendData.Setup(new Dictionary<string, BackendProperty>()
			{
				{ "Humidity", new BackendProperty("", false) },
				{ "Settings", new BackendProperty("", false) },
				{ "Manual", new BackendProperty("", false) },
				{ "Overview", new BackendProperty("", false) },
				{ "ManualActionExecution", new BackendProperty("", false) },
				{ "History", new BackendProperty("", false) },
				{ "Channels", new BackendProperty("", false) }
			});

			#region WateringWeb

			new SettingsPagePublisher("/settings").Publish();
			new ChannelsPagePublisher("/channels").Publish();
			new OverviewPagePublisher("/overview").Publish();
			new ManualPagePublisher("/manual").Publish();
			new HistoryPagePublisher("/history").Publish();

			UI.PublishJson("/settings/WeatherLocations.json", () =>
			{
				Assembly assembly = Assembly.GetExecutingAssembly();
				//Assembly.GetExecutingAssembly().GetManifestResourceNames();
				string resourceName = "TabNoc.PiWeb.Storage.external_WeatherLocations.WeatherLocations.json";

				using (Stream stream = assembly.GetManifestResourceStream(resourceName))
				using (StreamReader reader = new StreamReader(stream, Encoding.Default))
				{
					return reader.ReadToEnd();
				}
			});
			UI.PublishJson("/lib/bootstrap3-typeahead.min.js", () =>
			{
				Assembly assembly = Assembly.GetExecutingAssembly();
				//Assembly.GetExecutingAssembly().GetManifestResourceNames();
				string resourceName = "TabNoc.PiWeb.Storage.lib.bootstrap3-typeahead.min.js";

				using (Stream stream = assembly.GetManifestResourceStream(resourceName))
				using (StreamReader reader = new StreamReader(stream, Encoding.Default))
				{
					return reader.ReadToEnd();
				}
			});

			#endregion WateringWeb

			#region PiWeb

			new PiWebPublisher("/").Publish();

			#endregion PiWeb

			Logging.WriteLog("System", "OK", "PiWebSite wurde gestartet!");

			Console.ReadLine();
			PageStorage<ChannelsData>.Instance.Dispose();
			PageStorage<SettingsData>.Instance.Dispose();
		}
	}
}
