﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iDash
{
    class Constants
    {
        public static int IRacing = 0;
        public static int Raceroom = 1;
        public static int Assetto = 2;
        public static int RFactor = 3;
        public static int None = 4;

        public static object[] RFactorTelemetryData = new object[]
        {
            "speed.kmh.shared",
            "gear.int.shared",
            "engineRPM.float.shared",
            "engineWaterTemp.float.shared",
            "engineOilTemp.float.shared",
            "clutchRPM.float.shared",
            "unfilteredThrottle.float.shared",
            "unfilteredBrake.float.shared",
            "unfilteredSteering.float.shared",
            "unfilteredClutch.float.shared",
            "steeringArmForce.float.shared",
            "fuel.float.shared",
            "engineMaxRPM.float.shared",
            "driverName.byte[].vehicle",
            "vehicleName.byte[].vehicle",
            "totalLaps.short.vehicle",
            "sector.sbyte.vehicle",
            "finishStatus.sbyte.vehicle",
            "lapDist.float.vehicle",
            "pathLateral.float.vehicle",
            "trackEdge.float.vehicle",
            "bestSector1.time.vehicle",
            "bestSector2.time.vehicle",
            "bestLapTime.time.vehicle",
            "lastSector1.time.vehicle",
            "lastSector2.time.vehicle",
            "lastLapTime.time.vehicle",
            "curSector1.time.vehicle",
            "curSector2.time.vehicle",
            "numPitstops.short.vehicle",
            "numPenalties.short.vehicle",
            "isPlayer.byte.vehicle",
            "control.sbyte.vehicle",
            "inPits.byte.vehicle",
            "place.byte.vehicle",
            "vehicleClass.byte[].vehicle",
            "timeBehindNext.time.vehicle",
            "lapsBehindNext.int.vehicle",
            "timeBehindLeader.time.vehicle",
            "lapsBehindLeader.int.vehicle",
            "lapStartET.time.vehicle"
        };

        public static object[] RaceRoomTelemetryData = new object[] {
            "EngineRps.Single",
            "MaxEngineRps.Single",
            "FuelPressure.Single",
            "FuelLeft.Single",
            "FuelCapacity.Single",
            "EngineWaterTemp.Single",
            "EngineOilTemp.Single",
            "EngineOilPressure.Single",
            "CarSpeed.kmh",
            "NumberOfLaps.Int32",
            "CompletedLaps.Int32",
            "LapTimeBestSelf.time",
            "LapTimePreviousSelf.time",
            "LapTimeCurrentSelf.time",
            "Position.Int32",
            "NumCars.Int32",
            "Gear.Int32",
            "Temperature.FrontLeft_Left",
            "Temperature.FrontLeft_Center",
            "Temperature.FrontLeft_Right",
            "Temperature.FrontRight_Left",
            "Temperature.FrontRight_Center",
            "Temperature.FrontRight_Right",
            "Temperature.RearLeft_Left",
            "Temperature.RearLeft_Center",
            "Temperature.RearLeft_Right",
            "Temperature.RearRight_Left",
            "Temperature.RearRight_Center",
            "Temperature.RearRight_Right",
            "NumPenalties.Int32",
            "CarCgLocation.Vector3_X",
            "CarCgLocation.Vector3_Y",
            "CarCgLocation.Vector3_Z",
            "CarOrientation.Orientation_Pitch",
            "CarOrientation.Orientation_Yaw",
            "CarOrientation.Orientation_Roll",
            "LocalAcceleration.Vector_X",
            "LocalAcceleration.Vector_Y",
            "LocalAcceleration.Vector_Z",
            "DrsAvailable.Int32",
            "DrsEngaged.Int32",
            "Padding1.Int32",
            "Player.PlayerData_Velocity",
            "EventIndex.Int32",
            "SessionType.Int32",
            "SessionPhase.Int32",
            "SessionIteration.Int32",
            "ControlType.Int32",
            "ThrottlePedal.Single",
            "BrakePedal.Single",
            "ClutchPedal.Single",
            "BrakeBias.Single",
            "TirePressure.FrontLeft_Left",
            "TirePressure.FrontLeft_Center",
            "TirePressure.FrontLeft_Right",
            "TirePressure.FrontRight_Left",
            "TirePressure.FrontRight_Center",
            "TirePressure.FrontRight_Right",
            "TirePressure.RearLeft_Left",
            "TirePressure.RearLeft_Center",
            "TirePressure.RearLeft_Right",
            "TirePressure.RearRight_Left",
            "TirePressure.RearRight_Center",
            "TirePressure.RearRight_Right",
            "TireWearActive.Int32",
            "TireType.Int32",
            "BrakeTemperatures.FrontLeft",
            "BrakeTemperatures.FrontRight",
            "BrakeTemperatures.RearLeft",
            "BrakeTemperatures.RearRight",
            "FuelUseActive.Int32",
            "SessionTimeRemaining.time",
            "LapTimeBestLeader.time",
            "LapTimeBestLeaderClass.time",
            "LapTimeDeltaSelf.time",
            "LapTimeDeltaLeader.time",
            "LapTimeDeltaLeaderClass.time",
            "TimeDeltaFront.time",
            "TimeDeltaBehind.time",
            "PitWindowStatus.Int32",
            "PitWindowStart.Int32",
            "PitWindowEnd.Int32",
            "CutTrackWarnings.Int32",
            "Flags.Yellow",
            "Flags.Blue",
            "Flags.Black"
        };

        public static object[] IRacingTelemetryData = new object[] {
            "AirDensity.float",
            "AirPressure.float",
            "AirTemp.float",
            "Alt.float",
            "Brake.float",
            "BrakeRaw.float",
            "CamCameraNumber.int",
            "CamCameraState.bitfield",
            "CamCarIdx.int",
            "CamGroupNumber.int",
            "Clutch.float",
            "CpuUsageBG.float",
            "DCDriversSoFar.int",
            "DCLapStatus.int",
            "DisplayUnits.int",
            "DriverMarker.bool",
            "EngineWarnings.bitfield",
            "EnterExitReset.int",
            "FogLevel.float",
            "FrameRate.float",
            "FuelLevel.float",
            "FuelLevelPct.float",
            "FuelPress.float",
            "FuelUsePerHour.float",
            "Gear.int",
            "IsDiskLoggingActive.bool",
            "IsDiskLoggingEnabled.bool",
            "IsInGarage.bool",
            "IsOnTrack.bool",
            "IsOnTrackCar.bool",
            "IsReplayPlaying.bool",
            "Lap.int",
            "LapBestLap.int",
            "LapBestLapTime.time",
            "LapBestNLapLap.int",
            "LapBestNLapTime.time",
            "LapCurrentLapTime.time",
            "LapDeltaToBestLap.time",
            "LapDeltaToBestLap_DD.float",
            "LapDeltaToBestLap_OK.bool",
            "LapDeltaToOptimalLap.time",
            "LapDeltaToOptimalLap_DD.float",
            "LapDeltaToOptimalLap_OK.bool",
            "LapDeltaToSessionBestLap.time",
            "LapDeltaToSessionBestLap_DD.float",
            "LapDeltaToSessionBestLap_OK.bool",
            "LapDeltaToSessionLastlLap.time",
            "LapDeltaToSessionLastlLap_DD.float",
            "LapDeltaToSessionLastlLap_OK.bool",
            "LapDeltaToSessionOptimalLap.time",
            "LapDeltaToSessionOptimalLap_DD.float",
            "LapDeltaToSessionOptimalLap_OK.bool",
            "LapDist.float",
            "LapDistPct.float",
            "LapLasNLapSeq.int",
            "LapLastLapTime.time",
            "LapLastNLapTime.time",
            "Lat.double",
            "LatAccel.float",
            "Lon.double",
            "LongAccel.float",
            "ManifoldPress.float",
            "OilLevel.float",
            "OilPress.float",
            "OilTemp.float",
            "OnPitRoad.bool",
            "Pitch.float",
            "PitchRate.float",
            "PitOptRepairLeft.time",
            "PitRepairLeft.float",
            "PitSvFlags.bitfield",
            "PitSvFuel.float",
            "PitSvLFP.float",
            "PitSvLRP.float",
            "PitSvRFP.float",
            "PitSvRRP.float",
            "PlayerCarClassPosition.int",
            "PlayerCarPosition.int",
            "RaceLaps.int",
            "RadioTransmitCarIdx.int",
            "RadioTransmitFrequencyIdx.int",
            "RadioTransmitRadioIdx.int",
            "RelativeHumidity.float",
            "ReplayFrameNum.int",
            "ReplayFrameNumEnd.int",
            "ReplayPlaySlowMotion.bool",
            "ReplayPlaySpeed.int",
            "ReplaySessionNum.int",
            "ReplaySessionTime.double",
            "Roll.float",
            "RollRate.float",
            "RPM.float",
            "SessionFlags.bitfield",
            "SessionLapsRemain.int",
            "SessionNum.int",
            "SessionState.int",
            "SessionTime.dtime",
            "SessionTimeRemain.dtime",
            "SessionUniqueID.int",
            "ShiftGrindRPM.float",
            "ShiftIndicatorPct.float",
            "ShiftPowerPct.float",
            "Skies.int",
            "Speed.kmh",
            "SteeringWheelAngle.float",
            "SteeringWheelAngleMax.float",
            "SteeringWheelPctDamper.float",
            "SteeringWheelPctTorque.float",
            "SteeringWheelPctTorqueSign.float",
            "SteeringWheelPctTorqueSignStops.float",
            "SteeringWheelPeakForceNm.float",
            "SteeringWheelTorque.float",
            "Throttle.float",
            "ThrottleRaw.float",
            "TrackTemp.float",
            "TrackTempCrew.float",
            "VelocityX.float",
            "VelocityY.float",
            "VelocityZ.float",
            "VertAccel.float",
            "Voltage.float",
            "WaterLevel.float",
            "WaterTemp.float",
            "WeatherType.int",
            "WindDir.float",
            "WindVel.float",
            "Yaw.float",
            "YawNorth.float",
            "YawRate.float",
            "CFrideHeight.float",
            "CFshockDefl.float",
            "CFshockVel.float",
            "CFSRrideHeight.float",
            "CRrideHeight.float",
            "CRshockDefl.float",
            "CRshockVel.float",
            "dcABS.float",
            "dcAntiRollFront.float",
            "dcAntiRollRear.float",
            "dcBoostLevel.float",
            "dcBrakeBias.float",
            "dcBrakeBias.float",
            "dcDiffEntry.float",
            "dcDiffExit.float",
            "dcDiffMiddle.float",
            "dcEngineBraking.float",
            "dcEnginePower.float",
            "dcFuelMixture.float",
            "dcRevLimiter.float",
            "dcThrottleShape.float",
            "dcTractionControl.float",
            "dcTractionControl2.float",
            "dcTractionControlToggle.bool",
            "dcWeightJackerLeft.float",
            "dcWeightJackerRight.float",
            "dcWingFront.float",
            "dcWingRear.float",
            "dpFNOMKnobSetting.float",
            "dpFUFangleIndex.float",
            "dpFWingAngle.float",
            "dpFWingIndex.float",
            "dpLrWedgeAdj.float",
            "dpPSSetting.float",
            "dpQtape.float",
            "dpRBarSetting.float",
            "dpRFTruckarmP1Dz.float",
            "dpRRDamperPerchOffsetm.float",
            "dpRrPerchOffsetm.float",
            "dpRrWedgeAdj.float",
            "dpRWingAngle.float",
            "dpRWingIndex.float",
            "dpRWingSetting.float",
            "dpTruckarmP1Dz.float",
            "dpWedgeAdj.float",
            "LFbrakeLinePress.float",
            "LFcoldPressure.float",
            "LFpressure.float",
            "LFrideHeight.float",
            "LFshockDefl.float",
            "LFshockVel.float",
            "LFspeed.float",
            "LFtempCL.float",
            "LFtempCM.float",
            "LFtempCR.float",
            "LFtempL.float",
            "LFtempM.float",
            "LFtempR.float",
            "LFwearL.float",
            "LFwearM.float",
            "LFwearR.float",
            "LRbrakeLinePress.float",
            "LRcoldPressure.float",
            "LRpressure.float",
            "LRrideHeight.float",
            "LRshockDefl.float",
            "LRshockVel.float",
            "LRspeed.float",
            "LRtempCL.float",
            "LRtempCM.float",
            "LRtempCR.float",
            "LRtempL.float",
            "LRtempM.float",
            "LRtempR.float",
            "LRwearL.float",
            "LRwearM.float",
            "LRwearR.float",
            "RFbrakeLinePress.float",
            "RFcoldPressure.float",
            "RFpressure.float",
            "RFrideHeight.float",
            "RFshockDefl.float",
            "RFshockVel.float",
            "RFspeed.float",
            "RFtempCL.float",
            "RFtempCM.float",
            "RFtempCR.float",
            "RFtempL.float",
            "RFtempM.float",
            "RFtempR.float",
            "RFwearL.float",
            "RFwearM.float",
            "RFwearR.float",
            "RRbrakeLinePress.float",
            "RRcoldPressure.float",
            "RRpressure.float",
            "RRrideHeight.float",
            "RRshockDefl.float",
            "RRshockVel.float",
            "RRspeed.float",
            "RRtempCL.float",
            "RRtempCM.float",
            "RRtempCR.float",
            "RRtempL.float",
            "RRtempM.float",
            "RRtempR.float",
            "RRwearL.float",
            "RRwearM.float",
            "RRwearR.float",
            "CarIdxClassPosition.aint",
            "CarIdxEstTime.afloat",
            "CarIdxF2Time.afloat",
            "CarIdxGear.aint",
            "CarIdxLap.aint",
            "CarIdxLapDistPct.afloat",
            "CarIdxOnPitRoad.abool",
            "CarIdxPosition.aint",
            "CarIdxRPM.afloat",
            "CarIdxSteer.afloat",
            "CarIdxTrackSurface.aint"
        };

        public static object[] AssettoTelemetryData = new object[] {
            "PacketId.Int32.physics",
            "Gas.Single.physics",
            "Brake.Single.physics",
            "Fuel.Single.physics",
            "Gear.Int32.physics",
            "Rpms.Int32.physics",
            "SteerAngle.Single.physics",
            "SpeedKmh.kmh.physics",
            "Velocity.Single[].physics",
            "AccG.Single[].physics",
            "WheelSlip.Single[].physics",
            "WheelLoad.Single[].physics",
            "WheelsPressure.Single[].physics",
            "WheelAngularSpeed.Single[].physics",
            "TyreWear.Single[].physics",
            "TyreDirtyLevel.Single[].physics",
            "TyreCoreTemperature.Single[].physics",
            "CamberRad.Single[].physics",
            "SuspensionTravel.Single[].physics",
            "Drs.Single.physics",
            "TC.Single.physics",
            "Heading.Single.physics",
            "Pitch.Single.physics",
            "Roll.Single.physics",
            "CgHeight.Single.physics",
            "CarDamage.Single[].physics",
            "NumberOfTyresOut.Int32.physics",
            "PitLimiterOn.Int32.physics",
            "Abs.Single.physics",
            "PacketId.Int32.graphics",
            "Status.AC_STATUS.graphics",
            "Session.AC_SESSION_TYPE.graphics",
            "CurrentTime.time.graphics",
            "LastTime.time.graphics",
            "BestTime.time.graphics",
            "Split.String.graphics",
            "CompletedLaps.Int32.graphics",
            "Position.Int32.graphics",
            "iCurrentTime.Int32.graphics",
            "iLastTime.Int32.graphics",
            "iBestTime.Int32.graphics",
            "SessionTimeLeft.Single.graphics",
            "DistanceTraveled.Single.graphics",
            "IsInPit.Int32.graphics",
            "CurrentSectorIndex.Int32.graphics",
            "LastSectorTime.Int32.graphics",
            "NumberOfLaps.Int32.graphics",
            "TyreCompound.String.graphics",
            "ReplayTimeMultiplier.Single.graphics",
            "NormalizedCarPosition.Single.graphics",
            "CarCoordinates.Single[].graphics",
            "SMVersion.String.static",
            "ACVersion.String.static",
            "NumberOfSessions.Int32.static",
            "NumCars.Int32.static",
            "CarModel.String.static",
            "Track.String.static",
            "PlayerName.String.static",
            "PlayerSurname.String.static",
            "PlayerNick.String.static",
            "SectorCount.Int32.static",
            "MaxTorque.Single.static",
            "MaxPower.Single.static",
            "MaxRpm.Int32.static",
            "MaxFuel.Single.static",
            "SuspensionMaxTravel.Single[].static",
            "TyreRadius.Single[].static",
        };
    }
}
