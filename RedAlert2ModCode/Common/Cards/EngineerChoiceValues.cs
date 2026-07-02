using System.Collections.Generic;
using MegaCrit.Sts2.Core.Localization;
using RedAlert2ModCode.UI;

using EngineerChoice = RedAlert2ModCode.UI.EngineerChoiceScreen.EngineerChoice;

namespace RedAlert2ModCode.Common.Cards;

public static class EngineerChoiceValues
{
	public static EngineerChoice CaptureOilDerrick => new()
	{
		Type = EngineerChoiceScreen.ChoiceType.CaptureOilDerrick,
		Title = new LocString("card_keywords", "engineer_choice.capture_oil_derrick.title"),
		Description = new LocString("card_keywords", "engineer_choice.capture_oil_derrick.description"),
		Weight = 8
	};

	public static EngineerChoice RepairBuilding => new()
	{
		Type = EngineerChoiceScreen.ChoiceType.RepairBuilding,
		Title = new LocString("card_keywords", "engineer_choice.repair_building.title"),
		Description = new LocString("card_keywords", "engineer_choice.repair_building.description"),
		Weight = 10
	};

	public static EngineerChoice CaptureAirfield => new()
	{
		Type = EngineerChoiceScreen.ChoiceType.CaptureAirfield,
		Title = new LocString("card_keywords", "engineer_choice.capture_airfield.title"),
		Description = new LocString("card_keywords", "engineer_choice.capture_airfield.description"),
		Weight = 4
	};

	public static EngineerChoice CaptureHospital => new()
	{
		Type = EngineerChoiceScreen.ChoiceType.CaptureHospital,
		Title = new LocString("card_keywords", "engineer_choice.capture_hospital.title"),
		Description = new LocString("card_keywords", "engineer_choice.capture_hospital.description"),
		Weight = 1
	};

	public static EngineerChoice CaptureWorkshop => new()
	{
		Type = EngineerChoiceScreen.ChoiceType.CaptureWorkshop,
		Title = new LocString("card_keywords", "engineer_choice.capture_workshop.title"),
		Description = new LocString("card_keywords", "engineer_choice.capture_workshop.description"),
		Weight = 1
	};

	public static EngineerChoice CaptureTechOutpost => new()
	{
		Type = EngineerChoiceScreen.ChoiceType.CaptureTechOutpost,
		Title = new LocString("card_keywords", "engineer_choice.capture_tech_outpost.title"),
		Description = new LocString("card_keywords", "engineer_choice.capture_tech_outpost.description"),
		Weight = 1
	};

	public static EngineerChoice RepairBridge => new()
	{
		Type = EngineerChoiceScreen.ChoiceType.RepairBridge,
		Title = new LocString("card_keywords", "engineer_choice.repair_bridge.title"),
		Description = new LocString("card_keywords", "engineer_choice.repair_bridge.description"),
		Weight = 5
	};

	public static List<EngineerChoice> AllChoices => new()
	{
		CaptureOilDerrick,
		RepairBuilding,
		CaptureAirfield,
		CaptureHospital,
		CaptureWorkshop,
		CaptureTechOutpost,
		RepairBridge
	};
}