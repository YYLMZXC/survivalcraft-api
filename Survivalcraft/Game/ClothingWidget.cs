using System.Xml.Linq;
using Engine;

namespace Game
{
	public class ClothingWidget : CanvasWidget
	{
		private GridPanelWidget m_inventoryGrid;

		private StackPanelWidget m_clothingStack;

		private ButtonWidget m_vitalStatsButton;

		private ButtonWidget m_sleepButton;

		private PlayerModelWidget m_innerClothingModelWidget;

		private PlayerModelWidget m_outerClothingModelWidget;

		private ComponentPlayer m_componentPlayer;

		public ClothingWidget(ComponentPlayer componentPlayer)
		{
			m_componentPlayer = componentPlayer;
			XElement node = ContentManager.Get<XElement>("Widgets/ClothingWidget");
			LoadContents(this, node);
			m_clothingStack = Children.Find<StackPanelWidget>("ClothingStack");
			m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
			m_vitalStatsButton = Children.Find<ButtonWidget>("VitalStatsButton");
			m_sleepButton = Children.Find<ButtonWidget>("SleepButton");
			m_innerClothingModelWidget = Children.Find<PlayerModelWidget>("InnerClothingModel");
			m_outerClothingModelWidget = Children.Find<PlayerModelWidget>("OuterClothingModel");
			for (int i = 0; i < 4; i++)
			{
				InventorySlotWidget inventorySlotWidget = new InventorySlotWidget();
				float y = float.PositiveInfinity;
				if (i == 0)
				{
					y = 68f;
				}
				if (i == 3)
				{
					y = 54f;
				}
				inventorySlotWidget.Size = new Vector2(float.PositiveInfinity, y);
				inventorySlotWidget.BevelColor = Color.Transparent;
				inventorySlotWidget.CenterColor = Color.Transparent;
				inventorySlotWidget.AssignInventorySlot(m_componentPlayer.ComponentClothing, i);
				inventorySlotWidget.HideEditOverlay = true;
				inventorySlotWidget.HideInteractiveOverlay = true;
				inventorySlotWidget.HideFoodOverlay = true;
				inventorySlotWidget.HideHighlightRectangle = true;
				inventorySlotWidget.HideBlockIcon = true;
				inventorySlotWidget.HideHealthBar = m_componentPlayer.Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true).WorldSettings.GameMode == GameMode.Creative;
				m_clothingStack.Children.Add(inventorySlotWidget);
			}
			int num = 10;
			for (int j = 0; j < m_inventoryGrid.RowsCount; j++)
			{
				for (int k = 0; k < m_inventoryGrid.ColumnsCount; k++)
				{
					InventorySlotWidget inventorySlotWidget2 = new InventorySlotWidget();
					inventorySlotWidget2.AssignInventorySlot(componentPlayer.ComponentMiner.Inventory, num++);
					m_inventoryGrid.Children.Add(inventorySlotWidget2);
					m_inventoryGrid.SetWidgetCell(inventorySlotWidget2, new Point2(k, j));
				}
			}
			m_innerClothingModelWidget.PlayerClass = componentPlayer.PlayerData.PlayerClass;
			m_innerClothingModelWidget.CharacterSkinTexture = m_componentPlayer.ComponentClothing.InnerClothedTexture;
			m_outerClothingModelWidget.PlayerClass = componentPlayer.PlayerData.PlayerClass;
			m_outerClothingModelWidget.OuterClothingTexture = m_componentPlayer.ComponentClothing.OuterClothedTexture;
		}

		public override void Update()
		{
			if (m_vitalStatsButton.IsClicked && m_componentPlayer != null)
			{
				m_componentPlayer.ComponentGui.ModalPanelWidget = new VitalStatsWidget(m_componentPlayer);
			}
			if (m_sleepButton.IsClicked && m_componentPlayer != null)
			{
				if (!m_componentPlayer.ComponentSleep.CanSleep(out var reason))
				{
					m_componentPlayer.ComponentGui.DisplaySmallMessage(reason, Color.White, blinking: false, playNotificationSound: false);
				}
				else
				{
					m_componentPlayer.ComponentSleep.Sleep(allowManualWakeup: true);
				}
			}
		}
	}
}
