using System.Collections.Generic;

namespace RedAlert2ModCode.Common.Utils;

public static class UnitVoiceConfig
{
    public static readonly Dictionary<string, List<string>> AlliedUnits = new()
    {
        ["AmericanSoldier"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/AmericanSoldier/Igiata.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/AmericanSoldier/Igiatc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/AmericanSoldier/Igiatf.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/AmericanSoldier/Igimoa.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/AmericanSoldier/Igimoc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/AmericanSoldier/Igimod.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/AmericanSoldier/Igimof.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/AmericanSoldier/Igisea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/AmericanSoldier/Igisec.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/AmericanSoldier/Igisee.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/AmericanSoldier/Igisef.mp3",
        },
        ["GrizzlyTank"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/GrizzlyTank/Vgraatb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/GrizzlyTank/Vgraatc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/GrizzlyTank/Vgraate.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/GrizzlyTank/Vgramoa.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/GrizzlyTank/Vgramoc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/GrizzlyTank/Vgramoe.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/GrizzlyTank/Vgramof.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/GrizzlyTank/Vgrasea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/GrizzlyTank/Vgraseb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/GrizzlyTank/Vgrasec.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/GrizzlyTank/Vgrased.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/GrizzlyTank/Vgrasee.mp3",
        },
        ["AlliesEngineer"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/Engineer/Ienamoc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Engineer/Ienasea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Engineer/Ienaseb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Engineer/Ienasec.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Engineer/Ienased.mp3",
        },
        ["Intruder"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/Intruder/Vintata.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Intruder/Vintatb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Intruder/Vintatd.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Intruder/Vintmob.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Intruder/Vintmoc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Intruder/Vintsea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Intruder/Vintseb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Intruder/Vintsec.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Intruder/Vintsed.mp3",
        },
        ["MirageTank"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/MirageTank/Vmirata.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/MirageTank/Vmiratb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/MirageTank/Vmiratc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/MirageTank/Vmirate.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/MirageTank/Vmirmoa.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/MirageTank/Vmirmoe.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/MirageTank/Vmirsea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/MirageTank/Vmirseb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/MirageTank/Vmirsed.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/MirageTank/Vmirsee.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/MirageTank/Vmirsef.mp3",
        },
        ["NightHawkChopper"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/NightHawk/Vblhata.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/NightHawk/Vblhatb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/NightHawk/Vblhatd.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/NightHawk/Vblhmoa.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/NightHawk/Vblhmoc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/NightHawk/Vblhmod.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/NightHawk/Vblhseb.mp3",
        },
        ["PrismTank"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/PrismTank/Vpriata.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/PrismTank/Vpriate.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/PrismTank/Vprimob.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/PrismTank/Vprimoc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/PrismTank/Vprimod.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/PrismTank/Vprisea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/PrismTank/Vpriseb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/PrismTank/Vprisee.mp3",
        },
        ["RocketSoldier"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/RocketSoldier/Irocatd.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/RocketSoldier/Irocmoa.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/RocketSoldier/Irocmoc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/RocketSoldier/Irocmod.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/RocketSoldier/Irocseb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/RocketSoldier/Irocsec.mp3",
        },
        ["Spy"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/Spy/Ispyata.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Spy/Ispyatb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Spy/Ispymoc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Spy/Ispymod.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Spy/Ispymoe.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Spy/Ispysea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Spy/Ispyseb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Spy/Ispysec.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Spy/Ispysed.mp3",
        },
        ["AlliedTransportShip"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/TransportShip/Vhoamoa.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/TransportShip/Vhoamob.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/TransportShip/Vhoamoc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/TransportShip/Vhoamoe.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/TransportShip/Vhoaseb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/TransportShip/Vhoasec.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/TransportShip/Vhoased.mp3",
        },
        ["ChronoMiner"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/ChronoMiner/Vchrgob.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/ChronoMiner/Vchrgoc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/ChronoMiner/Vchrhac.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/ChronoMiner/Vchrhad.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/ChronoMiner/Vchrmod.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/ChronoMiner/Vchrsea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/ChronoMiner/Vchrseb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/ChronoMiner/Vchrsed.mp3",
        },
        ["AlliesDogSoldier"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/DogSoldier/Idogatca.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/DogSoldier/Idogdiea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/DogSoldier/Idogfea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/DogSoldier/Idogfec.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/DogSoldier/Idogsela.mp3",
        },
        ["AircraftCarrier"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/AircraftCarrier/Vairatb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/AircraftCarrier/Vairmoa.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/AircraftCarrier/Vairmob.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/AircraftCarrier/Vairmoc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/AircraftCarrier/Vairmod.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/AircraftCarrier/Vairmoe.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/AircraftCarrier/Vairsea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/AircraftCarrier/Vairsec.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/AircraftCarrier/Vairsed.mp3",
        },
        ["Destroyer"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/Destroyer/Vwaaata.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Destroyer/Vwaaatb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Destroyer/Vwaaatc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Destroyer/Vwaamob.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Destroyer/Vwaamoc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Destroyer/Vwaamod.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Destroyer/Vwaamoe.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Destroyer/Vwaasea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Destroyer/Vwaaseb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Destroyer/Vwaasec.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Destroyer/Vwaased.mp3",
        },
        ["Dolphin"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/Dolphin/Vdolatta.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Dolphin/Vdolmova.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Dolphin/Vdolmovb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Dolphin/Vdolselb.mp3",
        },
        ["Ifv"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/IFV/Vifvatc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/IFV/Vifvmob.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/IFV/Vifvmoc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/IFV/Vifvmoe.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/IFV/Vifvsea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/IFV/Vifvseb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/IFV/Vifvsec.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/IFV/Vifvsee.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/IFV/Vifvsef.mp3",
        },
        ["GuardianGi"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/GuardianGI/Iggiate.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/GuardianGI/Iggimoc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/GuardianGI/Iggimoe.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/GuardianGI/Iggiseb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/GuardianGI/Iggisec.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/GuardianGI/Iggised.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/GuardianGI/Iggisee.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/GuardianGI/Iggisef.mp3",
        },
    };

    public static readonly Dictionary<string, List<string>> SovietUnits = new()
    {
        ["Conscript"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/Conscript/Iconata.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Conscript/Iconatb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Conscript/Iconatc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Conscript/Iconatd.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Conscript/Iconmoa.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Conscript/Iconmob.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Conscript/Iconmoc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Conscript/Iconmod.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Conscript/Iconsea.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Conscript/Iconseb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Conscript/Iconsec.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Conscript/Iconfea.mp3",
        },
        ["RhinoTank"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/RhinoTank/Vgrsata.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/RhinoTank/Vgrsatc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/RhinoTank/Vgrsatd.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/RhinoTank/Vgrsmoa.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/RhinoTank/Vgrsmob.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/RhinoTank/Vgrsmoc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/RhinoTank/Vgrssea.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/RhinoTank/Vgrsseb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/RhinoTank/Vgrssec.mp3",
        },
        ["SovietEngineer"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/SovietEngineer/Iensata.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/SovietEngineer/Iensmoa.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/SovietEngineer/Iensmob.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/SovietEngineer/Iensmoc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/SovietEngineer/Ienssea.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/SovietEngineer/Iensseb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/SovietEngineer/Ienssec.mp3",
        },
        ["WarMiner"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/WarMiner/Vwaratb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/WarMiner/Vwarhab.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/WarMiner/Vwarhac.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/WarMiner/Vwarmoa.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/WarMiner/Vwarmoc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/WarMiner/Vwarmoe.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/WarMiner/Vwarsea.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/WarMiner/Vwarseb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/WarMiner/Vwarsec.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/WarMiner/Vwarsed.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/WarMiner/Vwarsee.mp3",
        },
        ["FlakTrack"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/FlakTrack/Vflaatd.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakTrack/Vflamoa.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakTrack/Vflamob.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakTrack/Vflasea.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakTrack/Vflaseb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakTrack/Vflasec.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakTrack/Vflased.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakTrack/Vflasee.mp3",
        },
        ["FlakSubmarine"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/FlakSubmarine/Vscoata.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakSubmarine/Vscoatc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakSubmarine/Vscomoa.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakSubmarine/Vscomoc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakSubmarine/Vscomoe.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakSubmarine/Vscosea.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakSubmarine/Vscoseb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakSubmarine/Vscosec.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakSubmarine/Vscosee.mp3",
        },
        ["V3Rocket"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/V3Rocket/Vv3lata.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/V3Rocket/Vv3latd.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/V3Rocket/Vv3lmoc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/V3Rocket/Vv3lmoe.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/V3Rocket/Vv3lsea.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/V3Rocket/Vv3lseb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/V3Rocket/Vv3lsec.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/V3Rocket/Vv3lsed.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/V3Rocket/Vv3lsee.mp3",
        },
        ["TyphoonSubmarine"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/TyphoonSubmarine/Vsubata.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TyphoonSubmarine/Vsubatc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TyphoonSubmarine/Vsubatd.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TyphoonSubmarine/Vsubmoa.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TyphoonSubmarine/Vsubmoc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TyphoonSubmarine/Vsubmod.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TyphoonSubmarine/Vsubmoe.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TyphoonSubmarine/Vsubsea.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TyphoonSubmarine/Vsubsec.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TyphoonSubmarine/Vsubsee.mp3",
        },
        ["SovietAttackDog"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/DogSoldier/Idogatca.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/DogSoldier/Idogdiea.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/DogSoldier/Idogfea.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/DogSoldier/Idogfec.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/DogSoldier/Idogsela.mp3",
        },
        ["SovietFlakTrooper"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/FlakTrooper/Iflaata.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakTrooper/Iflaatd.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakTrooper/Iflamoa.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakTrooper/Iflamob.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakTrooper/Iflamoc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakTrooper/Iflasea.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakTrooper/Iflasec.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/FlakTrooper/Iflased.mp3",
        },
        ["TerrorDrone"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/TerrorDrone/Vtermova.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TerrorDrone/Vtersela.mp3",
        },
        ["TeslaTrooper"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/TeslaTrooper/Itesatb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TeslaTrooper/Itesmoc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TeslaTrooper/Itesmoe.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TeslaTrooper/Itessec.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TeslaTrooper/Itessed.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TeslaTrooper/Itessee.mp3",
        },
        ["SovietTransportShip"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/SovietTransportShip/Vhosmoa.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/SovietTransportShip/Vhosmob.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/SovietTransportShip/Vhosmoe.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/SovietTransportShip/Vhosseb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/SovietTransportShip/Vhossec.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/SovietTransportShip/Vhossed.mp3",
        },
        ["Kirov"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/Kirov/Vkirata.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Kirov/Vkiratb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Kirov/Vkiratc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Kirov/Vkirdia.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Kirov/Vkirmoc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Kirov/Vkirseb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Kirov/Vkirsec.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Kirov/Vkirsed.mp3",
        },
    };

    public static readonly Dictionary<string, List<string>> YuriUnits = new();

    public static List<string> GetUnitVoices(string unitName, string faction = "Allied")
    {
        return faction switch
        {
            "Soviet" => SovietUnits.TryGetValue(unitName, out var voices) ? voices : new List<string>(),
            "Yuri" => YuriUnits.TryGetValue(unitName, out var voices) ? voices : new List<string>(),
            _ => AlliedUnits.TryGetValue(unitName, out var voices) ? voices : new List<string>(),
        };
    }

    public static bool HasUnitVoices(string unitName, string faction = "Allied")
    {
        var voices = GetUnitVoices(unitName, faction);
        return voices != null && voices.Count > 0;
    }
}