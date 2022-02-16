using System.Collections.Generic;
using System.Xml.Linq;
using Engine;

namespace Game
{
	public class RecipaediaDescriptionScreen : Screen
	{
		private BlockIconWidget m_blockIconWidget;

		private LabelWidget m_nameWidget;

		private ButtonWidget m_leftButtonWidget;

		private ButtonWidget m_rightButtonWidget;

		private LabelWidget m_descriptionWidget;

		private LabelWidget m_propertyNames1Widget;

		private LabelWidget m_propertyValues1Widget;

		private LabelWidget m_propertyNames2Widget;

		private LabelWidget m_propertyValues2Widget;

		private int m_index;

		private IList<int> m_valuesList;

		public RecipaediaDescriptionScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/RecipaediaDescriptionScreen");
			LoadContents(this, node);
			m_blockIconWidget = Children.Find<BlockIconWidget>("Icon");
			m_nameWidget = Children.Find<LabelWidget>("Name");
			m_leftButtonWidget = Children.Find<ButtonWidget>("Left");
			m_rightButtonWidget = Children.Find<ButtonWidget>("Right");
			m_descriptionWidget = Children.Find<LabelWidget>("Description");
			m_propertyNames1Widget = Children.Find<LabelWidget>("PropertyNames1");
			m_propertyValues1Widget = Children.Find<LabelWidget>("PropertyValues1");
			m_propertyNames2Widget = Children.Find<LabelWidget>("PropertyNames2");
			m_propertyValues2Widget = Children.Find<LabelWidget>("PropertyValues2");
		}

		public override void Enter(object[] parameters)
		{
			int item = (int)parameters[0];
			m_valuesList = (IList<int>)parameters[1];
			m_index = m_valuesList.IndexOf(item);
			UpdateBlockProperties();
		}

		public override void Update()
		{
			m_leftButtonWidget.IsEnabled = m_index > 0;
			m_rightButtonWidget.IsEnabled = m_index < m_valuesList.Count - 1;
			if (m_leftButtonWidget.IsClicked || base.Input.Left)
			{
				m_index = MathUtils.Max(m_index - 1, 0);
				UpdateBlockProperties();
			}
			if (m_rightButtonWidget.IsClicked || base.Input.Right)
			{
				m_index = MathUtils.Min(m_index + 1, m_valuesList.Count - 1);
				UpdateBlockProperties();
			}
			if (base.Input.Back || base.Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
			{
				ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
			}
		}

		private Dictionary<string, string> GetBlockProperties(int value)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			int num = Terrain.ExtractContents(value);
			Block block = BlocksManager.Blocks[num];
			if (block.DefaultEmittedLightAmount > 0)
			{
				dictionary.Add("Luminosity", block.DefaultEmittedLightAmount.ToString());
			}
			if (block.FuelFireDuration > 0f)
			{
				dictionary.Add("Fuel Value", block.FuelFireDuration.ToString());
			}
			dictionary.Add("Is Stackable", (block.MaxStacking > 1) ? ("Yes (up to " + block.MaxStacking + ")") : "No");
			dictionary.Add("Is Flammable", (block.FireDuration > 0f) ? "Yes" : "No");
			if (block.GetNutritionalValue(value) > 0f)
			{
				dictionary.Add("Nutrition", block.GetNutritionalValue(value).ToString());
			}
			if (block.GetRotPeriod(value) > 0)
			{
				dictionary.Add("Max Storage Time", $"{(float)(2 * block.GetRotPeriod(value)) * 60f / 1200f:0.0} days");
			}
			if (block.DigMethod != 0)
			{
				dictionary.Add("Digging Method", block.DigMethod.ToString());
				dictionary.Add("Digging Resilience", block.DigResilience.ToString());
			}
			if (block.ExplosionResilience > 0f)
			{
				dictionary.Add("Explosion Resilience", block.ExplosionResilience.ToString());
			}
			if (block.GetExplosionPressure(value) > 0f)
			{
				dictionary.Add("Explosive Power", block.GetExplosionPressure(value).ToString());
			}
			bool flag = false;
			if (block.GetMeleePower(value) > 1f)
			{
				dictionary.Add("Melee Power", block.GetMeleePower(value).ToString());
				flag = true;
			}
			if (block.GetMeleePower(value) > 1f)
			{
				dictionary.Add("Melee Hit Ratio", $"{100f * block.GetMeleeHitProbability(value):0}%");
				flag = true;
			}
			if (block.GetProjectilePower(value) > 1f)
			{
				dictionary.Add("Projectile Power", block.GetProjectilePower(value).ToString());
				flag = true;
			}
			if (block.ShovelPower > 1f)
			{
				dictionary.Add("Shoveling", block.ShovelPower.ToString());
				flag = true;
			}
			if (block.HackPower > 1f)
			{
				dictionary.Add("Hacking", block.HackPower.ToString());
				flag = true;
			}
			if (block.QuarryPower > 1f)
			{
				dictionary.Add("Quarrying", block.QuarryPower.ToString());
				flag = true;
			}
			if (flag && block.Durability > 0)
			{
				dictionary.Add("Durability", block.Durability.ToString());
			}
			if (block.DefaultExperienceCount > 0f)
			{
				dictionary.Add("Experience Orbs", block.DefaultExperienceCount.ToString());
			}
			if (block is ClothingBlock)
			{
				ClothingData clothingData = ClothingBlock.GetClothingData(Terrain.ExtractData(value));
				dictionary.Add("Can Be Dyed", clothingData.CanBeDyed ? "Yes" : "No");
				dictionary.Add("Armor Protection", $"{(int)(clothingData.ArmorProtection * 100f)}%");
				dictionary.Add("Armor Durability", clothingData.Sturdiness.ToString());
				dictionary.Add("Insulation", $"{clothingData.Insulation:0.0} clo");
				dictionary.Add("Movement Speed", $"{clothingData.MovementSpeedFactor * 100f:0}%");
			}
			return dictionary;
		}

		private void UpdateBlockProperties()
		{
			if (m_index < 0 || m_index >= m_valuesList.Count)
			{
				return;
			}
			int value = m_valuesList[m_index];
			int num = Terrain.ExtractContents(value);
			Block block = BlocksManager.Blocks[num];
			m_blockIconWidget.Value = value;
			m_nameWidget.Text = block.GetDisplayName(null, value);
			m_descriptionWidget.Text = block.GetDescription(value);
			m_propertyNames1Widget.Text = string.Empty;
			m_propertyValues1Widget.Text = string.Empty;
			m_propertyNames2Widget.Text = string.Empty;
			m_propertyValues2Widget.Text = string.Empty;
			Dictionary<string, string> blockProperties = GetBlockProperties(value);
			int num2 = 0;
			foreach (KeyValuePair<string, string> item in blockProperties)
			{
				if (num2 < blockProperties.Count - blockProperties.Count / 2)
				{
					LabelWidget propertyNames1Widget = m_propertyNames1Widget;
					propertyNames1Widget.Text = propertyNames1Widget.Text + item.Key + ":\n";
					LabelWidget propertyValues1Widget = m_propertyValues1Widget;
					propertyValues1Widget.Text = propertyValues1Widget.Text + item.Value + "\n";
				}
				else
				{
					LabelWidget propertyNames2Widget = m_propertyNames2Widget;
					propertyNames2Widget.Text = propertyNames2Widget.Text + item.Key + ":\n";
					LabelWidget propertyValues2Widget = m_propertyValues2Widget;
					propertyValues2Widget.Text = propertyValues2Widget.Text + item.Value + "\n";
				}
				num2++;
			}
		}
	}
}
