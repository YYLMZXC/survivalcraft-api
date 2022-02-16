using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Engine;

namespace Game
{
	public class CommunityContentScreen : Screen
	{
		private enum Order
		{
			ByRank,
			ByTime
		}

		private ListPanelWidget m_listPanel;

		private LinkWidget m_moreLink;

		private LabelWidget m_orderLabel;

		private ButtonWidget m_changeOrderButton;

		private LabelWidget m_filterLabel;

		private ButtonWidget m_changeFilterButton;

		private ButtonWidget m_downloadButton;

		private ButtonWidget m_deleteButton;

		private ButtonWidget m_moreOptionsButton;

		private object m_filter;

		private Order m_order;

		private double m_contentExpiryTime;

		private Dictionary<string, IEnumerable<object>> m_itemsCache = new Dictionary<string, IEnumerable<object>>();

		public CommunityContentScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/CommunityContentScreen");
			LoadContents(this, node);
			m_listPanel = Children.Find<ListPanelWidget>("List");
			m_orderLabel = Children.Find<LabelWidget>("Order");
			m_changeOrderButton = Children.Find<ButtonWidget>("ChangeOrder");
			m_filterLabel = Children.Find<LabelWidget>("Filter");
			m_changeFilterButton = Children.Find<ButtonWidget>("ChangeFilter");
			m_downloadButton = Children.Find<ButtonWidget>("Download");
			m_deleteButton = Children.Find<ButtonWidget>("Delete");
			m_moreOptionsButton = Children.Find<ButtonWidget>("MoreOptions");
			m_listPanel.ItemWidgetFactory = delegate(object item)
			{
				CommunityContentEntry communityContentEntry = item as CommunityContentEntry;
				if (communityContentEntry != null)
				{
					XElement node2 = ContentManager.Get<XElement>("Widgets/CommunityContentItem");
					ContainerWidget containerWidget = (ContainerWidget)Widget.LoadWidget(this, node2, null);
					containerWidget.Children.Find<RectangleWidget>("CommunityContentItem.Icon").Subtexture = ExternalContentManager.GetEntryTypeIcon(communityContentEntry.Type);
					containerWidget.Children.Find<LabelWidget>("CommunityContentItem.Text").Text = communityContentEntry.Name;
					containerWidget.Children.Find<LabelWidget>("CommunityContentItem.Details").Text = string.Format("{0} {1}", new object[2]
					{
						ExternalContentManager.GetEntryTypeDescription(communityContentEntry.Type),
						DataSizeFormatter.Format(communityContentEntry.Size)
					});
					containerWidget.Children.Find<StarRatingWidget>("CommunityContentItem.Rating").Rating = communityContentEntry.RatingsAverage;
					containerWidget.Children.Find<StarRatingWidget>("CommunityContentItem.Rating").IsVisible = communityContentEntry.RatingsAverage > 0f;
					containerWidget.Children.Find<LabelWidget>("CommunityContentItem.ExtraText").Text = communityContentEntry.ExtraText;
					return containerWidget;
				}
				XElement node3 = ContentManager.Get<XElement>("Widgets/CommunityContentItemMore");
				ContainerWidget containerWidget2 = (ContainerWidget)Widget.LoadWidget(this, node3, null);
				m_moreLink = containerWidget2.Children.Find<LinkWidget>("CommunityContentItemMore.Link");
				m_moreLink.Tag = item as string;
				return containerWidget2;
			};
			m_listPanel.SelectionChanged += delegate
			{
				if (m_listPanel.SelectedItem != null && !(m_listPanel.SelectedItem is CommunityContentEntry))
				{
					m_listPanel.SelectedItem = null;
				}
			};
		}

		public override void Enter(object[] parameters)
		{
			m_filter = string.Empty;
			m_order = Order.ByRank;
			PopulateList(null);
		}

		public override void Update()
		{
			CommunityContentEntry communityContentEntry = m_listPanel.SelectedItem as CommunityContentEntry;
			m_downloadButton.IsEnabled = communityContentEntry != null;
			m_deleteButton.IsEnabled = UserManager.ActiveUser != null && communityContentEntry != null && communityContentEntry.UserId == UserManager.ActiveUser.UniqueId;
			m_orderLabel.Text = GetOrderDisplayName(m_order);
			m_filterLabel.Text = GetFilterDisplayName(m_filter);
			if (m_changeOrderButton.IsClicked)
			{
				List<Order> items = EnumUtils.GetEnumValues(typeof(Order)).Cast<Order>().ToList();
				DialogsManager.ShowDialog(null, new ListSelectionDialog("Select Sort Order", items, 60f, (object item) => GetOrderDisplayName((Order)item), delegate(object item)
				{
					m_order = (Order)item;
					PopulateList(null);
				}));
			}
			if (m_changeFilterButton.IsClicked)
			{
				List<object> list = new List<object>();
				list.Add(string.Empty);
				foreach (ExternalContentType item in from ExternalContentType t in EnumUtils.GetEnumValues(typeof(ExternalContentType))
					where ExternalContentManager.IsEntryTypeDownloadSupported(t)
					select t)
				{
					list.Add(item);
				}
				if (UserManager.ActiveUser != null)
				{
					list.Add(UserManager.ActiveUser.UniqueId);
				}
				DialogsManager.ShowDialog(null, new ListSelectionDialog("Select Items Filter", list, 60f, (object item) => GetFilterDisplayName(item), delegate(object item)
				{
					m_filter = item;
					PopulateList(null);
				}));
			}
			if (m_downloadButton.IsClicked && communityContentEntry != null)
			{
				DownloadEntry(communityContentEntry);
			}
			if (m_deleteButton.IsClicked && communityContentEntry != null)
			{
				DeleteEntry(communityContentEntry);
			}
			if (m_moreOptionsButton.IsClicked)
			{
				DialogsManager.ShowDialog(null, new MoreCommunityLinkDialog());
			}
			if (m_moreLink != null && m_moreLink.IsClicked)
			{
				PopulateList((string)m_moreLink.Tag);
			}
			if (base.Input.Back || Children.Find<BevelledButtonWidget>("TopBar.Back").IsClicked)
			{
				ScreensManager.SwitchScreen("Content");
			}
			if (base.Input.Hold.HasValue && base.Input.HoldTime > 2f && base.Input.Hold.Value.Y < 20f)
			{
				m_contentExpiryTime = 0.0;
				Task.Delay(250).Wait();
			}
		}

		private void PopulateList(string cursor)
		{
			string text = string.Empty;
			if (SettingsManager.CommunityContentMode == CommunityContentMode.Strict)
			{
				text = "1";
			}
			if (SettingsManager.CommunityContentMode == CommunityContentMode.Normal)
			{
				text = "0";
			}
			string text2 = ((m_filter is string) ? ((string)m_filter) : string.Empty);
			string text3 = ((m_filter is ExternalContentType) ? m_filter.ToString() : string.Empty);
			string text4 = m_order.ToString();
			string cacheKey = text2 + "\n" + text3 + "\n" + text4 + "\n" + text;
			m_moreLink = null;
			if (string.IsNullOrEmpty(cursor))
			{
				m_listPanel.ClearItems();
				m_listPanel.ScrollPosition = 0f;
				if (m_contentExpiryTime != 0.0 && Time.RealTime < m_contentExpiryTime && m_itemsCache.TryGetValue(cacheKey, out var value))
				{
					foreach (object item in value)
					{
						m_listPanel.AddItem(item);
					}
					return;
				}
			}
			CancellableBusyDialog busyDialog = new CancellableBusyDialog("Retrieving Content", autoHideOnCancel: false);
			DialogsManager.ShowDialog(null, busyDialog);
			//TO DO:fix search
			CommunityContentManager.List(cursor, text2, text3, text, text4,"", busyDialog.Progress, delegate(List<CommunityContentEntry> list, string nextCursor)
			{
				DialogsManager.HideDialog(busyDialog);
				m_contentExpiryTime = Time.RealTime + 300.0;
				while (m_listPanel.Items.Count > 0 && !(m_listPanel.Items[m_listPanel.Items.Count - 1] is CommunityContentEntry))
				{
					m_listPanel.RemoveItemAt(m_listPanel.Items.Count - 1);
				}
				foreach (CommunityContentEntry item2 in list)
				{
					m_listPanel.AddItem(item2);
				}
				if (list.Count > 0 && !string.IsNullOrEmpty(nextCursor))
				{
					m_listPanel.AddItem(nextCursor);
				}
				m_itemsCache[cacheKey] = new List<object>(m_listPanel.Items);
			}, delegate(Exception error)
			{
				DialogsManager.HideDialog(busyDialog);
				DialogsManager.ShowDialog(null, new MessageDialog("Error", error.Message, "OK", null, null));
			});
		}

		private void DownloadEntry(CommunityContentEntry entry)
		{
			string userId = ((UserManager.ActiveUser != null) ? UserManager.ActiveUser.UniqueId : string.Empty);
			CancellableBusyDialog busyDialog = new CancellableBusyDialog($"Downloading {entry.Name}", autoHideOnCancel: false);
			DialogsManager.ShowDialog(null, busyDialog);
			CommunityContentManager.Download(entry.Address, entry.Name, entry.Type, userId, busyDialog.Progress, delegate
			{
				DialogsManager.HideDialog(busyDialog);
			}, delegate(Exception error)
			{
				DialogsManager.HideDialog(busyDialog);
				DialogsManager.ShowDialog(null, new MessageDialog("Error", error.Message, "OK", null, null));
			});
		}

		private void DeleteEntry(CommunityContentEntry entry)
		{
			if (UserManager.ActiveUser == null)
			{
				return;
			}
			DialogsManager.ShowDialog(null, new MessageDialog("Are you sure?", "The link will be deleted from the server.", "Yes", "No", delegate(MessageDialogButton button)
			{
				if (button == MessageDialogButton.Button1)
				{
					CancellableBusyDialog busyDialog = new CancellableBusyDialog($"Deleting {entry.Name}", autoHideOnCancel: false);
					DialogsManager.ShowDialog(null, busyDialog);
					CommunityContentManager.Delete(entry.Address, UserManager.ActiveUser.UniqueId, busyDialog.Progress, delegate
					{
						DialogsManager.HideDialog(busyDialog);
						DialogsManager.ShowDialog(null, new MessageDialog("Link Deleted", "It will stop appearing in the listings in a few minutes.", "OK", null, null));
					}, delegate(Exception error)
					{
						DialogsManager.HideDialog(busyDialog);
						DialogsManager.ShowDialog(null, new MessageDialog("Error", error.Message, "OK", null, null));
					});
				}
			}));
		}

		private static string GetFilterDisplayName(object filter)
		{
			if (filter is string)
			{
				if (!string.IsNullOrEmpty((string)filter))
				{
					return "Your Items Only";
				}
				return "All Items";
			}
			if (filter is ExternalContentType)
			{
				return ExternalContentManager.GetEntryTypeDescription((ExternalContentType)filter);
			}
			throw new InvalidOperationException("Invalid filter.");
		}

		private static string GetOrderDisplayName(Order order)
		{
			switch (order)
			{
			case Order.ByRank:
				return "Top Ranked";
			case Order.ByTime:
				return "Recently Added";
			default:
				throw new InvalidOperationException("Invalid order.");
			}
		}
	}
}
