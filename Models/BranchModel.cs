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
		#region " Private "
		private int? _ahead;
		private int? _behind;
		#endregion

		#region " Public "
		public string Name { get; set; }
		public string DisplayName => $"{this.Name} {this.Status}";
		public string OnlyName { get; set; }
		public int? Ahead
		{
			get => _ahead;
			set
			{
				_ahead = value;
				OnPropertyChanged(nameof(Status));
			}
		}
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
		#endregion

		public BranchModel(Branch branch)
		{
			this.Name = branch.FriendlyName;
			this.OnlyName = branch.FriendlyName.Replace("origin/", "");
			this.Ahead = branch.TrackingDetails.AheadBy;
			this.Behind = branch.TrackingDetails.BehindBy;
		}

		//public BranchModel(Branch branch, Repository repo) : this(branch)
		//{
		//	if (branch.IsRemote && repo.Head != branch)
		//	{
		//		var div = repo.ObjectDatabase.CalculateHistoryDivergence(branch.Tip, repo.Head.Tip);

		//		this.Ahead = div.AheadBy;
		//		this.Behind = div.BehindBy;
		//	}
		//	else
		//	{
		//		this.Ahead = branch.TrackingDetails.AheadBy;
		//		this.Behind = branch.TrackingDetails.BehindBy;
		//	}
		//}

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
