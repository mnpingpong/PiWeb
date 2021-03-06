﻿using Ooui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TabNoc.MyOoui.Interfaces.AbstractObjects;
using TabNoc.MyOoui.Interfaces.Enums;
using TabNoc.MyOoui.UiComponents;
using TabNoc.MyOoui.UiComponents.FormControl;
using TabNoc.MyOoui.UiComponents.FormControl.InputGroups;
using TabNoc.MyOoui.UiComponents.FormControl.InputGroups.Components;
using TabNoc.PiWeb.DataTypes.WateringWeb.Manual;
using Button = TabNoc.MyOoui.HtmlElements.Button;

namespace TabNoc.PiWeb.Pages.WateringWeb.Manual
{
	internal class BatchPage : StylableElement
	{
		private const string NewJobString = "Neuen Job Anlegen";
		private readonly Button _appendToJobButton;
		private readonly BatchEntry _batch;
		private readonly MultiInputGroup _jobNameMultiInputGroup;
		private readonly Dropdown _jobSelectDropdown;
		private readonly Button _removeFromJobButton;

		public BatchPage(BatchEntry batch, ManualPage parent) : base("div")
		{
			const int labelSize = 100;
			_batch = batch;
			SetBorder(BorderKind.Rounded, StylingColor.Secondary);

			#region Initialize Grid

			Grid grid = new Grid(this);
			grid.AddStyling(StylingOption.MarginRight, 2);
			grid.AddStyling(StylingOption.MarginLeft, 2);
			grid.AddStyling(StylingOption.MarginTop, 4);
			grid.AddStyling(StylingOption.MarginBottom, 2);

			#endregion Initialize Grid

			#region JobName

			MultiInputGroup batchNameMultiInputGroup = new MultiInputGroup();
			batchNameMultiInputGroup.AppendLabel("BatchName", labelSize);
			StylableTextInput batchNameTextInput = batchNameMultiInputGroup.AppendTextInput("Name?", startText: batch.Name);
			batchNameMultiInputGroup.AppendValidation("", "Ein Batch-Auftrag mit diesem Namen existiert bereits", false);
			batchNameMultiInputGroup.AppendCustomElement(new Button(StylingColor.Success, asOutline: true, text: "Namen übernehmen", fontAwesomeIcon: "save"), false).Click += (sender, args) =>
			{
				if (batchNameTextInput.Value == "")
				{
					batchNameTextInput.SetValidation(false, true);
					return;
				}
				if (PageStorage<ManualData>.Instance.StorageData.BatchEntries.Any(entry => entry.Name == batchNameTextInput.Value))
				{
					if (batch.Name == batchNameTextInput.Value)
					{
						return;
					}
					else
					{
						batchNameTextInput.SetValidation(false, true);
					}
				}
				else
				{
					batchNameTextInput.SetValidation(true, false);
					foreach (BatchEntry entry in PageStorage<ManualData>.Instance.StorageData.JobEntries.SelectMany(
						entry => entry.BatchEntries.Where(batchEntry =>
							batchEntry.Name == batch.Name && batchEntry != batch)))
					{
						entry.Name = batchNameTextInput.Value;
					}
					batch.Name = batchNameTextInput.Value;
					parent.UpdateBatch();
				}
			};

			Button deleteJobButton = batchNameMultiInputGroup.AppendCustomElement(new Button(StylingColor.Danger, asOutline: true, text: "Batch-Auftrag Löschen", fontAwesomeIcon: "trash"), false);
			deleteJobButton.Click += (sender, args) =>
			{
				const string confirmMessage = "Wirklich Löschen";
				if (deleteJobButton.Text != confirmMessage)
				{
					deleteJobButton.Text = confirmMessage;
					return;
				}
				else
				{
					PageStorage<ManualData>.Instance.StorageData.BatchEntries.Remove(batch);
					List<JobEntry> removeList = new List<JobEntry>();

					foreach (JobEntry entry in PageStorage<ManualData>.Instance.StorageData.JobEntries.Where(entry =>
						entry.BatchEntries.Any(batchEntry => batchEntry.Name == batch.Name)))
					{
						entry.BatchEntries.RemoveAll(batchEntry => batchEntry.Name == batch.Name);
						if (entry.BatchEntries.Count == 0)
						{
							removeList.Add(entry);
						}
					}

					foreach (JobEntry jobEntry in removeList)
					{
						PageStorage<ManualData>.Instance.StorageData.JobEntries.Remove(jobEntry);
					}

					parent.UpdateBatch();
					parent.UpdateJobs();
				}
			};

			batchNameMultiInputGroup.AddStyling(StylingOption.MarginBottom, 2);
			grid.AddRow().AppendCollum(batchNameMultiInputGroup, autoSize: true);

			MultiInputGroup batchActionMultiInputGroup = new MultiInputGroup();
			batchActionMultiInputGroup.AppendLabel("Aktion", labelSize);
			batchActionMultiInputGroup.AppendLabel(batch.ToString());
			batchActionMultiInputGroup.AddStyling(StylingOption.MarginBottom, 1);
			grid.AddRow().AppendCollum(batchActionMultiInputGroup, autoSize: true);

			#endregion JobName

			#region ExecuteAction

			#region Init Container

			Container firstContainer = new Container();
			firstContainer.SetBorder(BorderKind.Rounded, StylingColor.Info);
			firstContainer.AddStyling(StylingOption.MarginTop, 3);
			firstContainer.AddStyling(StylingOption.MarginBottom, 1);
			firstContainer.AddStyling(StylingOption.PaddingTop, 3);
			firstContainer.AddStyling(StylingOption.PaddingBottom, 2);
			grid.AddRow().AppendCollum(firstContainer, autoSize: true);

			#endregion Init Container

			#region create Heading

			Heading firstHeading = new Heading(5, "Batch Ausführen ...");
			firstContainer.AppendChild(firstHeading);

			#endregion create Heading

			#region Override

			OverrideInputGroup overrideInputGroup = new OverrideInputGroup(100);
			firstContainer.AppendChild(overrideInputGroup);

			#endregion Override

			#region StartButton

			Button startButton = new Button(StylingColor.Success, true, text: "Einreihen!", fontAwesomeIcon: "plus-circle", asBlock: true);
			firstContainer.AppendChild(startButton);
			startButton.Click += (o, args) =>
			{
				startButton.IsDisabled = true;
				try
				{
					CreateBatchAction(batch, overrideInputGroup.Value);

					startButton.Text = "Wurde Eingereiht";
					Task.Run(() =>
					{
						System.Threading.Thread.Sleep(5000);
						startButton.Text = "Einreihen!";
						startButton.SetFontAwesomeIcon("plus-circle");
						return startButton.IsDisabled = false;
					});
				}
				catch (Exception)
				{
					startButton.Text = "Einreihen fehlgeschlagen";
					throw;
				}
			};
			firstContainer.AppendChild(startButton);
			startButton.AddStyling(StylingOption.MarginBottom, 2);

			#endregion StartButton

			#endregion ExecuteAction

			#region AddToJob

			#region Init Container

			Container secondContainer = new Container();
			secondContainer.SetBorder(BorderKind.Rounded, StylingColor.Info);
			secondContainer.AddStyling(StylingOption.MarginTop, 3);
			secondContainer.AddStyling(StylingOption.MarginBottom, 1);
			secondContainer.AddStyling(StylingOption.PaddingTop, 3);
			secondContainer.AddStyling(StylingOption.PaddingBottom, 2);
			grid.AddRow().AppendCollum(secondContainer, autoSize: true);

			#endregion Init Container

			#region create Heading

			Heading heading = new Heading(5, "... zu Job-Auftrag hinzufügen");
			secondContainer.AppendChild(heading);

			#endregion create Heading

			#region JobName Input

			_jobNameMultiInputGroup = new MultiInputGroup();
			_jobNameMultiInputGroup.AddStyling(StylingOption.MarginTop, 4);
			_jobNameMultiInputGroup.AppendLabel("Name für den Job-Auftrag:");
			StylableTextInput jobNameTextInput = _jobNameMultiInputGroup.AppendTextInput("Name?");
			_jobNameMultiInputGroup.AppendValidation("", "Es gibt bereits einen Job mit diesem Namen", false);
			_jobNameMultiInputGroup.IsHidden = true;
			secondContainer.AppendChild(_jobNameMultiInputGroup);

			#endregion JobName Input

			#region jobSelect

			MultiInputGroup jobSelectMultiInputGroup = new MultiInputGroup();
			jobSelectMultiInputGroup.AddStyling(StylingOption.MarginTop, 4);
			jobSelectMultiInputGroup.AppendLabel("Ziel Job:");
			_jobSelectDropdown = jobSelectMultiInputGroup.AppendCustomElement(new Dropdown(new Button(StylingColor.Secondary, true, text: "Bitte Wählen!"), DropdownDirection.DropDown), false);
			FillJobDropDown();
			jobSelectMultiInputGroup.AppendValidation("", "Dieser Batch-Auftrag ist bereits Bestandteil des Jobs", true);
			secondContainer.AppendChild(jobSelectMultiInputGroup);

			#endregion jobSelect

			#region Appent To Job or Create New

			_appendToJobButton = new Button(StylingColor.Success, true, text: "Zu Job-Auftrag hinzufügen", fontAwesomeIcon: "save", asBlock: true);
			_appendToJobButton.AddStyling(StylingOption.MarginTop, 2);
			_appendToJobButton.Click += (sender, args) =>
			{
				if (batchNameTextInput.Value == "")
				{
					jobNameTextInput.SetValidation(false, true);
					return;
				}
				if (_jobSelectDropdown.Button.Text == NewJobString)
				{
					if (PageStorage<ManualData>.Instance.StorageData.JobEntries.Any(entry => entry.Name == jobNameTextInput.Value) || jobNameTextInput.Value == "")
					{
						// Invalid entered Name
						jobNameTextInput.SetValidation(false, true);
					}
					else
					{
						// Create Job
						jobNameTextInput.SetValidation(true, false);
						PageStorage<ManualData>.Instance.StorageData.JobEntries.Add(new JobEntry(jobNameTextInput.Value, batch));
						parent.UpdateJobs();
						_jobNameMultiInputGroup.IsHidden = true;
						_jobSelectDropdown.Button.Text = jobNameTextInput.Value;

						_appendToJobButton.IsHidden = true;
						_removeFromJobButton.IsHidden = false;
					}
				}
				else
				{
					jobNameTextInput.SetValidation(false, false);

					// Add To Job
					PageStorage<ManualData>.Instance.StorageData.JobEntries.First(entry => entry.Name == _jobSelectDropdown.Button.Text).BatchEntries.Add(batch);
					parent.UpdateJobs();

					_appendToJobButton.IsHidden = true;
					_removeFromJobButton.IsHidden = false;
				}
			};
			secondContainer.AppendChild(_appendToJobButton);

			#endregion Appent To Job or Create New

			#region Remove From Job

			_removeFromJobButton = new Button(StylingColor.Danger, true, text: "Aus Job löschen", fontAwesomeIcon: "trash", asBlock: true);
			_removeFromJobButton.AddStyling(StylingOption.MarginTop, 2);
			_removeFromJobButton.IsHidden = true;
			_removeFromJobButton.Click += (sender, args) =>
			{
				if (PageStorage<ManualData>.Instance.StorageData.JobEntries.Any(entry => entry.Name == _jobSelectDropdown.Button.Text))
				{
					JobEntry jobEntry = PageStorage<ManualData>.Instance.StorageData.JobEntries.First(entry => entry.Name == _jobSelectDropdown.Button.Text);
					jobEntry.BatchEntries.RemoveAll(entry => entry.Name == _batch.Name);
					if (jobEntry.BatchEntries.Count == 0)
					{
						PageStorage<ManualData>.Instance.StorageData.JobEntries.Remove(jobEntry);
					}
				}
				_removeFromJobButton.IsHidden = true;
				_appendToJobButton.IsHidden = false;
				FillJobDropDown();
				parent.UpdateJobs();
			};
			secondContainer.AppendChild(_removeFromJobButton);

			#endregion Remove From Job

			#endregion AddToJob
		}

		public void FillJobDropDown()
		{
			_jobSelectDropdown.Clear();
			foreach (JobEntry jobEntry in PageStorage<ManualData>.Instance.StorageData.JobEntries)
			{
				bool active = jobEntry.BatchEntries.Any(batchEntry => batchEntry.Name == _batch.Name);
				StylableAnchor entry = _jobSelectDropdown.AddEntry(jobEntry.Name, active);
				entry.Click += (sender, args) =>
				{
					if (active)
					{
						_appendToJobButton.IsHidden = true;
						_removeFromJobButton.IsHidden = false;
					}
					else
					{
						_appendToJobButton.IsHidden = false;
						_removeFromJobButton.IsHidden = true;
					}
					_jobSelectDropdown.Button.Text = jobEntry.Name;
					_jobNameMultiInputGroup.IsHidden = true;
				};
			}

			_jobSelectDropdown.AddDivider();
			_jobSelectDropdown.AddEntry(NewJobString).Click += (sender, args) =>
			{
				_jobSelectDropdown.Button.Text = NewJobString;
				_jobNameMultiInputGroup.IsHidden = false;

				_appendToJobButton.IsHidden = false;
				_removeFromJobButton.IsHidden = true;
			};
		}

		private static void CreateBatchAction(BatchEntry batch, int durationOverride)
		{
			PageStorage<ManualActionExecutionData>.Instance.StorageData.Name = batch.Name;
			PageStorage<ManualActionExecutionData>.Instance.StorageData.ExecutionList = new List<ManualActionExecutionData.ManualActionExecution>()
			{
				new ManualActionExecutionData.ManualActionExecution(batch.ChannelId, batch.Duration, batch.ActivateMasterChannel, durationOverride)
			};
			PageStorage<ManualActionExecutionData>.Instance.Save();

			PageStorage<ManualActionExecutionData>.Instance.StorageData.Name = "";
			PageStorage<ManualActionExecutionData>.Instance.StorageData.ExecutionList = null;
		}
	}
}
