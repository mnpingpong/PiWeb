﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TabNoc.MyOoui;
using TabNoc.MyOoui.Interfaces.AbstractObjects;
using TabNoc.MyOoui.Interfaces.Enums;
using TabNoc.MyOoui.Storage;
using TabNoc.MyOoui.UiComponents;
using TabNoc.MyOoui.UiComponents.FormControl;
using TabNoc.PiWeb.DataTypes.WateringWeb.History;
using Button = TabNoc.MyOoui.HtmlElements.Button;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace TabNoc.PiWeb.Pages.WateringWeb.History
{
	internal class HistoryPage : StylableElement
	{
		public HistoryPage() : base("div")
		{
			AddStyling(StylingOption.MarginRight, 5);
			AddStyling(StylingOption.MarginLeft, 5);
			AddStyling(StylingOption.PaddingRight, 5);
			AddStyling(StylingOption.PaddingLeft, 5);
			//Container wrappingContainer = new Container(this);
			Grid grid = new Grid(this);

			Dropdown messageKindDropdown = new Dropdown(new Button(StylingColor.Light, false, text: "Quelle auswählen!"));
			Row headerRow = new Grid(grid.AddRow()).AddRow();

			headerRow.AppendCollum(messageKindDropdown, 6);
			messageKindDropdown.AddStyling(StylingOption.MarginBottom, 4);

			// Table historyTable = new Table(new List<string>() { "Zeitpunkt", "Status", "Quelle", "Meldung" }, CreateHistoryTableContent(), true);
			Table<DateTime> historyTable = new Table<DateTime>(new List<string>() { "Zeitpunkt", "Status", "Quelle", "Meldung" }, FetchEntries, FetchSearchEntries, FetchAmount, PrimaryCellConverter, true, 14);
			grid.AddRow().AppendCollum(historyTable);

			messageKindDropdown.AddEntry("Alles").Click += (sender, args) =>
			{
				historyTable.SetFilter("", "", true);
				messageKindDropdown.Button.Text = "Alles anzeigen";
			};
			messageKindDropdown.AddDivider();
			messageKindDropdown.AddEntry("System").Click += (sender, args) =>
			{
				historyTable.SetFilter("Quelle", "System", true);
				messageKindDropdown.Button.Text = "System-Meldungen anzeigen";
			};
			messageKindDropdown.AddEntry("Automatikbetrieb").Click += (sender, args) =>
			{
				historyTable.SetFilter("Quelle", "Automatic", true);
				messageKindDropdown.Button.Text = "Automatikbetrieb-Meldungen anzeigen";
			};
			messageKindDropdown.AddEntry("Handbetrieb").Click += (sender, args) =>
			{
				historyTable.SetFilter("Quelle", "Manual", true);
				messageKindDropdown.Button.Text = "Handbetrieb-Meldungen anzeigen";
			};

			historyTable.SetCellValueColor("Status", "OK", StylingColor.Success);
			historyTable.SetCellValueColor("Status", "Warn", StylingColor.Warning);
			historyTable.SetCellValueColor("Status", "Warnung", StylingColor.Warning);
			historyTable.SetCellValueColor("Status", "Error", StylingColor.Danger);
			historyTable.SetCellValueColor("Status", "Fehler", StylingColor.Danger);

			Button refreshButton = headerRow.AppendCollum(new Button(StylingColor.Primary, true, Button.ButtonSize.Normal, false, "Inhalt Aktualisieren! "), 2, false);
			refreshButton.Click += (sender, args) =>
			{
				refreshButton.IsDisabled = true;
				historyTable.RefreshBeginningTableContent();
				refreshButton.IsDisabled = false;
				if (refreshButton.Text.Contains(" !"))
				{
					refreshButton.Text = refreshButton.Text.Replace(" !", "! ");
				}
				else
				{
					refreshButton.Text = refreshButton.Text.TrimEnd(' ', '!') + " !";
				}
			};

			historyTable.EndUpdate();
		}

		private Task<int> FetchAmount()
		{
			string url = PageStorage<BackendData>.Instance.StorageData.GetUrl("History");

			if (url != "")
			{
				return new HttpClient().GetAsync($"{url}/amount").ContinueWith(task => JsonConvert.DeserializeObject<int>(task.EnsureResultSuccessStatusCode().Result.Content.ReadAsStringAsync().Result));
			}
			else
			{
				return Task.Run(() => PageStorage<HistoryData>.Instance.StorageData.HistoryElements.Count);
			}
		}

		private Task<List<(DateTime, List<string>)>> FetchEntries(DateTime primaryKey, int takeAmount)
		{
			string url = PageStorage<BackendData>.Instance.StorageData.GetUrl("History");

			if (url != "")
			{
				return new HttpClient()
					.GetAsync(HttpExtensions.GetQueryString(url, "range", ("primaryKey", primaryKey), ("takeAmount", takeAmount)))
					.ContinueWith(task => JsonConvert
						.DeserializeObject<List<HistoryElement>>(task.EnsureResultSuccessStatusCode().Result.Content.ReadAsStringAsync().Result)
						.Select(historyElement => (TimeStamp: historyElement.TimeStamp,
							new List<string>()
							{
								historyElement.Status,
								historyElement.Source,
								historyElement.Message
							})).ToList());
			}
			else
			{
				if (primaryKey == default(DateTime))
				{
					return Task.Run(() => PageStorage<HistoryData>.Instance.StorageData.HistoryElements.Take(takeAmount)
						.Select(historyElement => (historyElement.TimeStamp,
							new List<string>()
							{
								historyElement.Status,
								historyElement.Source,
								historyElement.Message
							})).ToList());
				}
				else
				{
					return Task.Run(() =>
						PageStorage<HistoryData>.Instance.StorageData.HistoryElements
							.SkipWhile(element => element.TimeStamp != primaryKey).Take(takeAmount)
							.Select(historyElement => (historyElement.TimeStamp,
								new List<string>()
								{
									historyElement.Status,
									historyElement.Source,
									historyElement.Message
								})).ToList());
				}
			}
		}

		private Task<List<(DateTime, List<string>)>> FetchSearchEntries(string searchstring, int collumn, int amount)
		{
			string url = PageStorage<BackendData>.Instance.StorageData.GetUrl("History");

			if (url != "")
			{
				return new HttpClient().GetAsync($"{url}/search?searchstring={searchstring.Replace(".", "%").Replace(":", "%")}&collumn={collumn}&amount={amount}")
					.ContinueWith(task => JsonConvert
						.DeserializeObject<List<HistoryElement>>(task.EnsureResultSuccessStatusCode().Result.Content.ReadAsStringAsync().Result)
						.Select(historyElement => ((TimeStamp: historyElement.TimeStamp,
							new List<string>()
							{
								historyElement.Status,
								historyElement.Source,
								historyElement.Message
							}))).ToList());
			}
			else
			{
				return Task.Run(() => PageStorage<HistoryData>.Instance.StorageData.HistoryElements
					.Where(element => GetElementCollumn(element, collumn).Contains(searchstring)).Take(amount).Select(historyElement =>
						(historyElement.TimeStamp, new List<string>() { historyElement.Status, historyElement.Source, historyElement.Message }))
					.ToList());
			}
		}

		private string GetElementCollumn(HistoryElement element, int collumn)
		{
			switch (collumn)
			{
				case 0:
					return PrimaryCellConverter(element.TimeStamp);

				case 1:
					return element.Status;

				case 2:
					return element.Source;

				case 3:
					return element.Message;

				default:
					throw new IndexOutOfRangeException();
			}
		}

		private string PrimaryCellConverter(DateTime primaryKey)
		{
			return primaryKey.ToShortDateString() + " " + primaryKey.ToLongTimeString();
		}
	}
}
