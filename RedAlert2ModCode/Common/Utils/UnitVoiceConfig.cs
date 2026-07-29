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
        ["AmericanSoldierAttack"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/AmericanSoldier/Ig_attack.wav",
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
        ["BlackHawk"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/BlackHawk/Vbleata.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/BlackHawk/Vblemoa.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/BlackHawk/Vblemob.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/BlackHawk/Vblemoc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/BlackHawk/Vblesea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/BlackHawk/Vbleseb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/BlackHawk/Vblesec.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/BlackHawk/Vblesed.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/BlackHawk/Vblesef.mp3",
        },
        ["Sniper"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/Sniper/Isniata.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Sniper/Isniatc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Sniper/Isnimob.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Sniper/Isnimoe.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Sniper/Isnisea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Sniper/Isniseb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Sniper/Isnisec.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Sniper/Isnised.mp3",
        },
        ["SniperAttack"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/Sniper/Isniatta_attack.mp3",
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
        ["PrismTankAttack"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/PrismTank/Vpriata_attack.wav",
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
            "res://RedAlert2ModResources/audio/AlliedUnits/Spy/Ispymoc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Spy/Ispymod.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Spy/Ispymoe.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Spy/Ispysea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Spy/Ispyseb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Spy/Ispysec.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/Spy/Ispysed.mp3",
        },
        ["SpyCamouflage"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/Spy/Ispyatb_camouflage.mp3",
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
        ["IfvDeploy"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/IFV/Vifvtran-deploy.mp3",
        },
        ["IfvAttack"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/IFV/Vifvat2b_attack.mp3",
        },
        ["IfvRepair"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/IFV/Vifvrepa_repair.mp3",
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
        ["GuardianGiAttack"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/GuardianGI/Iggisef_attack.wav",
        },
        ["GuardianGiDeploy"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/GuardianGI/Iggisef_deploy.wav",
        },
        ["BattleFortress"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/BattleFortress/Vbatate.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/BattleFortress/Vbatmoa.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/BattleFortress/Vbatmob.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/BattleFortress/Vbatmod.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/BattleFortress/Vbatmoe.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/BattleFortress/Vbatsea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/BattleFortress/Vbatseb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/BattleFortress/Vbatsec.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/BattleFortress/Vbatsed.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/BattleFortress/Vbatsee.mp3",
        },
        ["BattleFortressDeploy"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/BattleFortress/Vbatsef_deploy.mp3",
        },
        ["TankDestroyer"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/TankDestroyer/Vtanatb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/TankDestroyer/Vtanatc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/TankDestroyer/Vtanate.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/TankDestroyer/Vtanmoa.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/TankDestroyer/Vtanmoc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/TankDestroyer/Vtansea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/TankDestroyer/Vtanseb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/TankDestroyer/Vtansed.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/TankDestroyer/Vtansee.mp3",
        },
        ["ChronoLegionnaire"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/ChronoLegionnaire/Ichratb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/ChronoLegionnaire/Ichratc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/ChronoLegionnaire/Ichratd.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/ChronoLegionnaire/Ichrfeb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/ChronoLegionnaire/Ichrsea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/ChronoLegionnaire/Ichrseb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/ChronoLegionnaire/Ichrsec.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/ChronoLegionnaire/Ichrsed.mp3",
        },
        ["ChronoLegionnaireAttack"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/ChronoLegionnaire/Ichratta_attack.mp3",
        },
        ["ChronoLegionnaireKill"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/ChronoLegionnaire/Ichrkill_kill.mp3",
        },
        ["SealCommandos"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseaatb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseaexc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseamoa.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseamob.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseamoc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseasea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseaseb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseased.mp3",
        },
        ["SealCommandosAttack"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/seal_attack_1.wav",
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/seal_attack_2.wav",
        },
        ["SealCommandosC4"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseaexa-c4.mp3",
        },
        ["ChronoCommandos"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseaatb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseaexc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseamoa.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseamob.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseamoc.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseasea.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseaseb.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseasec_chrono.mp3",
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseased.mp3",
        },
        ["ChronoCommandosAttack"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/seal_attack_1.wav",
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/seal_attack_2.wav",
        },
        ["ChronoCommandosC4"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseaexa-c4.mp3",
        },
        ["ChronoCommandosEnter"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/SealAndChronoCommandos/Iseasec_chrono.mp3",
        },
        ["GrandCannonRotate"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/GrandCannon/rotate_1.wav",
            "res://RedAlert2ModResources/audio/AlliedUnits/GrandCannon/rotate_2.wav",
            "res://RedAlert2ModResources/audio/AlliedUnits/GrandCannon/rotate_3.wav",
            "res://RedAlert2ModResources/audio/AlliedUnits/GrandCannon/rotate_4.wav",
        },
        ["GrandCannonAttack"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/GrandCannon/attack.wav",
        },
        ["Pillbox"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/Pillbox/allied_pillbox_1.wav",
            "res://RedAlert2ModResources/audio/AlliedUnits/Pillbox/allied_pillbox_2.wav",
            "res://RedAlert2ModResources/audio/AlliedUnits/Pillbox/allied_pillbox_3.wav",
            "res://RedAlert2ModResources/audio/AlliedUnits/Pillbox/allied_pillbox_4.wav",
        },
        ["PatriotMissile"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/AlliedUnits/PatriotMissile/patriot_missile.wav",
        },
        ["ForceShieldOn"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/CommonSFX/ForceShield/force_shield_on.wav",
        },
        ["ForceShieldOff"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/CommonSFX/ForceShield/force_shield_off.wav",
        },
        ["PowerOutage"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/CommonSFX/power_outage.wav",
        },
        ["ParatrooperPlane"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/CommonSFX/paratrooper/paratrooper_plane_move.wav",
        },
        ["Paratrooper"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/CommonSFX/paratrooper/paratrooper.wav",
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
        ["YuriSoldier"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/YuriUnits/YuriSoldier/Iiniate.mp3",
            "res://RedAlert2ModResources/audio/YuriUnits/YuriSoldier/Iinimoa.mp3",
            "res://RedAlert2ModResources/audio/YuriUnits/YuriSoldier/Iinimoc.mp3",
            "res://RedAlert2ModResources/audio/YuriUnits/YuriSoldier/Iinisea.mp3",
            "res://RedAlert2ModResources/audio/YuriUnits/YuriSoldier/Iiniseb.mp3",
            "res://RedAlert2ModResources/audio/YuriUnits/YuriSoldier/Iinisec.mp3",
            "res://RedAlert2ModResources/audio/YuriUnits/YuriSoldier/Iinisee.mp3",
        },
        ["YuriSoldierAttack"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/YuriUnits/YuriSoldier/yuri_soldier_attack.wav",
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
        ["DemolitionTruck"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/DemolitionTruck/Vdemata.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/DemolitionTruck/Vdematb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/DemolitionTruck/Vdematd.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/DemolitionTruck/Vdemate.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/DemolitionTruck/Vdemmoc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/DemolitionTruck/Vdemseb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/DemolitionTruck/Vdemsec.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/DemolitionTruck/Vdemsed.mp3",
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
        ["GiantSquid"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/GiantSquid/Vsqumova.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/GiantSquid/Vsqumovb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/GiantSquid/Vsqusela.mp3",
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
        ["Desolator"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/Desolator/Idesata.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Desolator/Idesatb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Desolator/Idesatd.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Desolator/Idesate.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Desolator/Idesatf.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Desolator/Idesmoa.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Desolator/Idesmoc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Desolator/Idesmod.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Desolator/Idesmoe.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Desolator/Idessea.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Desolator/Idesseb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/Desolator/Idessee.mp3",
        },
        ["DesolatorAttack"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/Desolator/Idesat1a_radiation.mp3",
        },
        ["DesolatorDeploy"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/Desolator/Idesat2a_deploy.mp3",
        },
        ["TerrorMan"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/TerrorMan/Iterata.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TerrorMan/Iteratb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TerrorMan/Iteratc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TerrorMan/Itermoa.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TerrorMan/Itermob.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TerrorMan/Itermoc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TerrorMan/Itersea.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TerrorMan/Iterseb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TerrorMan/Itersec.mp3",
        },
        ["CrazyIvan"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icraata.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icraatb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icramoa.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icramoc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icramod.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icrasea.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icraseb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icrasec.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icrased.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icrasee.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icrasef.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icraseg.mp3",
        },
        ["ChronoIvan"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icraata.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icraatb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icramoa.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icramoc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icramod.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icrasea.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icraseb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icrasec.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icrased.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icrasee.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icrasef.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/CrazyIvan/Icraseg.mp3",
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
        ["SpyPlaneEngine"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/SpyPlane/Vspylo3a_engine.mp3",
        },
        ["SpyPlaneSnap"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/SpyPlane/Vspysnap_snap.mp3",
        },
        ["ApocalypseTank"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/ApocalypseTank/Vapoatb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/ApocalypseTank/Vapoatd.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/ApocalypseTank/Vapoate.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/ApocalypseTank/Vapomoa.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/ApocalypseTank/Vaposea.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/ApocalypseTank/Vaposeb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/ApocalypseTank/Vaposec.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/ApocalypseTank/Vaposed.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/ApocalypseTank/Vaposee.mp3",
        },
        ["TeslaTank"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/TeslaTank/Vtesata.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TeslaTank/Vtesatb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TeslaTank/Vtesate.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TeslaTank/Vtesmoa.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TeslaTank/Vtesmoc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TeslaTank/Vtessea.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TeslaTank/Vtesseb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TeslaTank/Vtessec.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/TeslaTank/Vtessed.mp3",
        },
        ["TeslaTankAttack"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/TeslaTank/Vtesatta_attack.mp3",
        },
        ["DemolitionTruck"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/DemolitionTruck/Vdemata.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/DemolitionTruck/Vdematb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/DemolitionTruck/Vdematd.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/DemolitionTruck/Vdemate.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/DemolitionTruck/Vdemmoc.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/DemolitionTruck/Vdemseb.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/DemolitionTruck/Vdemsec.mp3",
            "res://RedAlert2ModResources/audio/SovietUnits/DemolitionTruck/Vdemsed.mp3",
        },
        ["ConscriptAttack"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/Conscript/Iconsea_attack.wav",
        },
        ["RhinoTankAttack"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/RhinoTank/Vgrsata_attack.wav",
        },
        ["ApocalypseTankAttack"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/ApocalypseTank/Vapoatb_attack.wav",
        },
        ["WarMinerAttack"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/WarMiner/Vwaratb_attack.wav",
        },
        ["SovietPillbox"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/SovietUnits/SovietPillbox/soviet_pillbox_1.wav",
            "res://RedAlert2ModResources/audio/SovietUnits/SovietPillbox/soviet_pillbox_2.wav",
            "res://RedAlert2ModResources/audio/SovietUnits/SovietPillbox/soviet_pillbox_3.wav",
            "res://RedAlert2ModResources/audio/SovietUnits/SovietPillbox/soviet_pillbox_4.wav",
        },
    };

    public static readonly Dictionary<string, List<string>> YuriUnits = new()
    {
        ["Yuri"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/YuriUnits/Yuri/Iyurata.mp3",
            "res://RedAlert2ModResources/audio/YuriUnits/Yuri/Iyuratd.mp3",
            "res://RedAlert2ModResources/audio/YuriUnits/Yuri/Iyurate.mp3",
            "res://RedAlert2ModResources/audio/YuriUnits/Yuri/Iyurmoa.mp3",
            "res://RedAlert2ModResources/audio/YuriUnits/Yuri/Iyurmoc.mp3",
            "res://RedAlert2ModResources/audio/YuriUnits/Yuri/Iyursea.mp3",
            "res://RedAlert2ModResources/audio/YuriUnits/Yuri/Iyursec.mp3",
            "res://RedAlert2ModResources/audio/YuriUnits/Yuri/Iyursed.mp3",
        },
        ["YuriAttack"] = new List<string>
        {
            "res://RedAlert2ModResources/audio/YuriUnits/Yuri/Iyurat1a_attack.mp3",
        },
    };

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

    public static void PlayUnitVoice(string unitName, string voiceKey, string faction = "Allied")
    {
        string fullKey = unitName + voiceKey.First().ToString().ToUpper() + voiceKey.Substring(1);
        var voices = GetUnitVoices(fullKey, faction);
        if (voices.Count > 0)
        {
            PlayVoice(voices[0]);
        }
    }

    public static void PlayRandomVoice(string unitName, string faction = "Allied")
    {
        var voices = GetUnitVoices(unitName, faction);
        if (voices.Count > 0)
        {
            int index = new System.Random().Next(voices.Count);
            PlayVoice(voices[index]);
        }
    }

    private static void PlayVoice(string path)
    {
        try
        {
            Godot.AudioStream? stream = Godot.ResourceLoader.Load<Godot.AudioStream>(path);
            if (stream != null)
            {
                var audioPlayer = new Godot.AudioStreamPlayer();
                audioPlayer.Name = $"UnitVoicePlayer_{System.Guid.NewGuid()}";
                audioPlayer.Stream = stream;
                audioPlayer.VolumeDb = -5;

                var root = Godot.Engine.GetMainLoop() as Godot.SceneTree;
                root?.Root.AddChild(audioPlayer);

                audioPlayer.Play();

                audioPlayer.Finished += () =>
                {
                    if (Godot.GodotObject.IsInstanceValid(audioPlayer))
                    {
                        audioPlayer.QueueFree();
                    }
                };
            }
        }
        catch { }
    }
}