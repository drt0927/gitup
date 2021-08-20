using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gitup.Models
{
	public class BranchModel : INotifyPropertyChanged
	{
		public string Name { get; set; }
		private int? _ahead;
		public int? Ahead
		{
			get => _ahead;
			set
			{
				_ahead = value;
				OnPropertyChanged(nameof(Status));
			}
		}
		private int? _behind;
		public int? Behind
		{
			get => _behind;
			set
			{
				_behind = value;
				OnPropertyChanged(nameof(Status));
			}
		}

		public string Status
		{
			get
			{
				string status = string.Empty;
				if (Ahead != null && Ahead != 0)
				{
					status += $" ↑{Ahead}";
				}

				if (Behind != null && Behind != 0)
				{
					status += $" ↓{Behind}";
				}

				return status;
			}
		}

		public BranchModel(Branch branch)
		{
			this.Name = branch.FriendlyName;
			this.Ahead = branch.TrackingDetails.AheadBy;
			this.Behind = branch.TrackingDetails.BehindBy;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
