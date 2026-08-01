using System.Collections.Generic;
using MegaCrit.Sts2.Core.Localization;
using RedAlert2ModCode.UI;

using Choice = RedAlert2ModCode.UI.ChoiceSelectionScreen.Choice;

namespace RedAlert2ModCode.Common.Cards;

public static class EngineerChoiceValues
{
	public static Choice CaptureOilDerrick => new()
	{
		Type = ChoiceSelectionScreen.ChoiceType.CaptureOilDerrick,
		Title = new LocString("card_keywords", "engineer_choice.capture_oil_derrick.title"),
		Description = new LocString("card_keywords", "engineer_choice.capture_oil_derrick.description"),
		Weight = 20
	};

	public static Choice RepairBuilding => new()
	{
		Type = ChoiceSelectionScreen.ChoiceType.RepairBuilding,
		Title = new LocString("card_keywords", "engineer_choice.repair_building.title"),
		Description = new LocString("card_keywords", "engineer_choice.repair_building.description"),
		Weight = 20
	};

	public static Choice CaptureAirfield => new()
	{
		Type = ChoiceSelectionScreen.ChoiceType.CaptureAirfield,
		Title = new LocString("card_keywords", "engineer_choice.capture_airfield.title"),
		Description = new LocString("card_keywords", "engineer_choice.capture_airfield.description"),
		Weight = 15
	};

	public static Choice CaptureHospital => new()
	{
		Type = ChoiceSelectionScreen.ChoiceType.CaptureHospital,
		Title = new LocString("card_keywords", "engineer_choice.capture_hospital.title"),
		Description = new LocString("card_keywords", "engineer_choice.capture_hospital.description"),
		Weight = 4
	};

	public static Choice CaptureWorkshop => new()
	{
		Type = ChoiceSelectionScreen.ChoiceType.CaptureWorkshop,
		Title = new LocString("card_keywords", "engineer_choice.capture_workshop.title"),
		Description = new LocString("card_keywords", "engineer_choice.capture_workshop.description"),
		Weight = 4
	};

	public static Choice CaptureTechOutpost => new()
	{
		Type = ChoiceSelectionScreen.ChoiceType.CaptureTechOutpost,
		Title = new LocString("card_keywords", "engineer_choice.capture_tech_outpost.title"),
		Description = new LocString("card_keywords", "engineer_choice.capture_tech_outpost.description"),
		Weight = 1
	};

	public static Choice RepairBridge => new()
	{
		Type = ChoiceSelectionScreen.ChoiceType.RepairBridge,
		Title = new LocString("card_keywords", "engineer_choice.repair_bridge.title"),
		Description = new LocString("card_keywords", "engineer_choice.repair_bridge.description"),
		Weight = 15
	};

	public static Choice SurveyMineField => new()
	{
		Type = ChoiceSelectionScreen.ChoiceType.SurveyMineField,
		Title = new LocString("card_keywords", "engineer_choice.survey_mine_field.title"),
		Description = new LocString("card_keywords", "engineer_choice.survey_mine_field.description"),
		Weight = 4
	};

	public static List<Choice> AllChoices => new()
	{
		CaptureOilDerrick,
		RepairBuilding,
		CaptureAirfield,
		CaptureHospital,
		CaptureWorkshop,
		CaptureTechOutpost,
		RepairBridge,
		SurveyMineField
	};
}