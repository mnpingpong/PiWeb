﻿using System.Collections.Generic;
using TabNoc.Ooui.Interfaces.AbstractObjects;

namespace TabNoc.Ooui.Storage.Channels
{
	public class ChannelsData : PageData
	{
		public bool Enabled;
		public List<ChannelData> Channels;
		public ChannelData MasterChannel;

		public new static ChannelsData CreateNew() => new ChannelsData
		{
			Channels = new List<ChannelData>(),
			MasterChannel = ChannelData.CreateNew(0, true),
			Enabled = true,
			Valid = true
		};
	}
}