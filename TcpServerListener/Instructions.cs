using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpServerListener
{
    public class Instructions
    {
        readonly Dictionary<string, byte[]> Instruction = new Dictionary<string, byte[]>();
        readonly Dictionary<string, string> projectorConfig = new Dictionary<string, string>();
        readonly Dictionary<int, string> BaudRateHex = new Dictionary<int, string>();
        public Instructions()
        {
            #region Instruction For Web Platform
            Instruction.Add("None", new byte[] { });
            Instruction.Add("MacAddress", new byte[] { 0X8B, 0xB9, 00, 03, 05, 08, 0x10 });
            Instruction.Add("SystemOnWeb", new byte[] { 0X8B, 0xB9, 00, 04, 05, 02, 0xC0, 0xCB });
            Instruction.Add("Status", new byte[] { 0X8B, 0xB9, 00, 03, 05, 01, 09 });
            Instruction.Add("SystemOffWeb", new byte[] { 0X8B, 0xB9, 00, 04, 05, 02, 0xC1, 0xCC });
            Instruction.Add("ComputerOnWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x1D, 0x28 });
            Instruction.Add("ComputerOffWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x1E, 0x29 });
            Instruction.Add("IsMachineOnline", new byte[] { 0x8B, 0xB9, 00, 03, 03, 01, 07 });
            /// <summary>
            /// MediaSignals
            /// </summary>
            Instruction.Add("DesktopWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x10, 0x1B });//电脑
            Instruction.Add("LaptopWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x11, 0x1C });//笔记本-HDMI
            Instruction.Add("DigitalBoothWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x12, 0x1D });//笔记本-VGA
            Instruction.Add("DigitalEquipmentWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x13, 0x1e });//数字设备
            Instruction.Add("DvdWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x14, 0x1f });
            Instruction.Add("TVWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x15, 0x20 });
            Instruction.Add("VideoCameraWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x16, 0x21 });
            Instruction.Add("RecordingDeviceWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x19, 0x24 });
            Instruction.Add("BlurayDvdWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x1a, 0x25 });
            //Instruction.Add("ExternalHdWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x1d, 0x28 });
            /// <summary>
            /// LockControls
            /// </summary> 
            Instruction.Add("SystemLockWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x2c, 0x37 });
            Instruction.Add("SystemUnlockWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x2d, 0x38 });
            Instruction.Add("PodiumLockWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x2e, 0x39 });
            Instruction.Add("PodiumUnlockWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x2f, 0x3A });
            Instruction.Add("ClassLockWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x5f, 0x70 });
            Instruction.Add("ClassUnlockWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x60, 0x6B });
            /// <summary>
            /// SoundControls
            /// </summary>
            Instruction.Add("VolumeWeb", new byte[] { 0x8B, 0xB9, 00, 05, 05, 07,06 });
            Instruction.Add("WiredMicVolumeWeb", new byte[] { 0x8B, 0xB9, 00, 05, 05, 07,07});
            Instruction.Add("WirelessMicVolumeWeb", new byte[] { 0x8B, 0xB9, 00, 05, 05, 07,08 });
            //Instruction.Add("GeneralVolumeDownWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x21, 0x2C });
            //Instruction.Add("GeneralVolumeUpWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x20, 0x2B });
            Instruction.Add("GeneralVolumeMuteWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x22, 0x2D });
            //Instruction.Add("WiredMicDownWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x24, 0x2F });
            //Instruction.Add("WiredMicUpWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x23, 0x2E });
            Instruction.Add("WiredMicMuteWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x25, 0x30 });
            //Instruction.Add("WirelessMicDownWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x74, 0x7f });
            //Instruction.Add("WirelessMicUpWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x73, 0x7e });
            Instruction.Add("WirelessMicMuteWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x75, 0x80 });

            ///<summary>
            ///Camera Sound COntrols
            /// </summary>
            Instruction.Add("StudentAudioWeb", new byte[] { 0x8B, 0xB9 ,00, 05, 05, 07, 02, 07, 0x1A });
            Instruction.Add("TeacherAudioWeb", new byte[] { 0x8B, 0xB9, 00, 05, 05, 07, 02, 08, 0x1B });
            /// <summary>
            /// Projector Controls
            /// </summary>
            Instruction.Add("ProjectorOnWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x33, 0x3e });
            Instruction.Add("ProjectorHDMIWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x34, 0x3f });
            Instruction.Add("ProjectorVideoWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x35, 0x40 });
            Instruction.Add("ProjectorOffWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x43, 0x4e });
            Instruction.Add("ProjectorVGAWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x44, 0x4f });
            Instruction.Add("ProjectorSleepWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x45, 0x50 });
            Instruction.Add("ReadProjectorConfig", new byte[] { 0x8B, 0xB9, 00, 03, 03, 02, 08 });

            /// <summary>
            /// Screen and Curtain Controls
            /// </summary>    

            Instruction.Add("ScreenDownWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x56, 0x61 }); //屏幕
            Instruction.Add("ScreenRiseWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x76, 0x81 });// 屏幕
            Instruction.Add("ScreenStopWeb", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x66, 0x71 }); //屏幕
            Instruction.Add("CurtainOpenWeb", new byte[] {});
            Instruction.Add("CurtainCloseWeb", new byte[] { });
            Instruction.Add("CurtainStopWeb", new byte[] {  });

            ///<summary>
            ///power Controls
            /// </summary>

            Instruction.Add("ProjectorPowerOnWeb", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 01, 01, 0x15 });
            Instruction.Add("ComputerPowerOnWeb", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 02, 01, 0x16 });
            Instruction.Add("OtherPowerOnWeb", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 04, 01, 0x18 });
            Instruction.Add("AmplifierPowerOnWeb", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 03, 01, 0x17 });
            Instruction.Add("ProjectorPowerOffWeb", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 01, 00, 0x14 });
            Instruction.Add("ComputerPowerOffWeb", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 02, 00, 0x15 });
            Instruction.Add("OtherPowerOffWeb", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 04, 00, 0x17 });
            Instruction.Add("AmplifierPowerOffWeb", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 03, 00, 0x16 });

            #endregion
            #region Strategy Instructions
            Instruction.Add("CloseStrategy", new byte[] { 0x8B, 0xB9, 00, 06, 05, 09, 01 });
            Instruction.Add("SystemOn", new byte[] { 0X8B, 0xB9, 00, 06, 05, 09, 02});
            Instruction.Add("ProjectorOn", new byte[] { 0x8B, 0xB9, 00, 06, 05, 09, 03 });
            Instruction.Add("ComputerOn", new byte[] { 0x8B, 0xB9, 00, 06, 05, 09, 04});
            Instruction.Add("SoundOn", new byte[] { 0x8B, 0xB9, 00, 06, 05, 09, 05 });
            Instruction.Add("PortPowerOn", new byte[] { 0x8B, 0xB9, 00, 06, 05, 09, 06 });
                    
            Instruction.Add("PanelOff", new byte[] { 0x8B, 0xB9, 00, 06, 05, 09, 0x2c });//lock
            Instruction.Add("PanelOn", new byte[] { 0x8B, 0xB9, 00, 06, 05, 09, 0x2d });//unlock
            Instruction.Add("ScreenOn", new byte[] { 0x8B, 0xB9, 00, 06, 05, 09, 0x56 });//down屏幕
            Instruction.Add("ScreenOff", new byte[] { 0x8B, 0xB9, 00, 06, 05, 09, 0x76 });//rise屏幕

            #endregion

            #region Projector Configurations
            projectorConfig.Add("ProjectorAutoOn", "00");
            projectorConfig.Add("ProjectorAutoOff", "00");
            projectorConfig.Add("ScreenAutoDrop", "00");
            projectorConfig.Add("ComputerAutoOn", "00");
            projectorConfig.Add("ComputerAutoOff", "00");
            projectorConfig.Add("ProjectorSwitchAuto", "00");
            projectorConfig.Add("ScreenLinkageOn", "00");
            projectorConfig.Add("ScreenLinkageOff", "00");
            projectorConfig.Add("VolumeMemoryOn", "00");
            projectorConfig.Add("BuzzerOn", "00");
            projectorConfig.Add("IODetectionOn", "00");
            projectorConfig.Add("ProjectorOffDelayMinute", "01"); //1-9
            projectorConfig.Add("Projector232Signal", "00");
            projectorConfig.Add("projector2Infrared", "00");
            projectorConfig.Add("IODetectionOff", "00");
            projectorConfig.Add("SwipeOn", "00");
            projectorConfig.Add("SwipeOff", "00");
            projectorConfig.Add("ProjectorOnDelaySecond", "01"); //1-20
            projectorConfig.Add("FingerPrintOn", "00");
            projectorConfig.Add("FingerPrintOff", "00");
            projectorConfig.Add("ComputerLinkageOff", "00");
            projectorConfig.Add("HdmiAudio", "00");
            projectorConfig.Add("SystemAlarm", "00");
            #endregion

            #region Strategy Instruction
            /// <summary>
            /// Instruction set for Strategy
            /// Instructions before protocol change
            /// </summary>   

            //Instruction.Add("SystemOnStrategy", new byte[] { 0X8B, 0xB9, 00, 04, 05, 09, 0xC0, 0xD2 });
            //Instruction.Add("StatusStrategy", new byte[] { 0X8B, 0xB9, 00, 03, 05, 01, 09 });
            //Instruction.Add("SystemOffStrategy", new byte[] { 0X8B, 0xB9, 00, 04, 05, 09, 0xC1, 0xD3 });
            //Instruction.Add("ComputerOnStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x1D, 0x2F });
            //Instruction.Add("ComputerOffStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x1E, 0x30 });
            //Instruction.Add("IsMachineOnlineStrategy", new byte[] { 0x8B, 0xB9, 00, 03, 03, 01, 07 });
            ///// <summary>
            ///// MediaSignals
            ///// </summary>
            //Instruction.Add("DesktopStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x10, 0x22 });
            //Instruction.Add("LaptopStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x11, 0x23 });
            //Instruction.Add("DigitalBoothStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x12, 0x24 });
            //Instruction.Add("DigitalEquipmentStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x13, 0x25 });
            //Instruction.Add("DvdStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x14, 0x26 });
            //Instruction.Add("TVStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x15, 0x27 });
            //Instruction.Add("VideoCameraStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x16, 0x28 });
            //Instruction.Add("RecordingDeviceStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x19, 0x2B });
            //Instruction.Add("BlurayDvdStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x1a, 0x2C });
            //Instruction.Add("ExternalHdStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x1d, 0x2F });
            ///// <summary>
            ///// LockControls
            ///// </summary> 
            //Instruction.Add("SystemLockStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x2c, 0x3E });
            //Instruction.Add("SystemUnlockStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x2d, 0x3F });
            //Instruction.Add("PodiumLockStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x2e, 0x40 });
            //Instruction.Add("PodiumUnlockStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x2f, 0x41 });
            //Instruction.Add("ClassLockStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x5f, 0x71 });
            //Instruction.Add("ClassUnlockStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x60, 0x72 });
            ///// <summary>
            ///// SoundControls
            ///// </summary>
            //Instruction.Add("GeneralVolumeDownStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x21, 0x33 });
            //Instruction.Add("GeneralVolumeUpStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x20, 0x32 });
            //Instruction.Add("GeneralVolumeMuteStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x22, 0x34 });
            //Instruction.Add("WiredMicDownStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x24, 0x36 });
            //Instruction.Add("WiredMicUpStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x23, 0x35 });
            //Instruction.Add("WiredMicMuteStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x25, 0x37 });
            //Instruction.Add("WirelessMicDownStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x74, 0x86 });
            //Instruction.Add("WirelessMicUpStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x73, 0x85 });
            //Instruction.Add("WirelessMicMuteStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x75, 0x87 });
            ///// <summary>
            ///// Projector Controls
            ///// </summary>
            //Instruction.Add("ProjectorOnStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x33, 0x45 });
            //Instruction.Add("ProjectorHDMIStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x34, 0x46 });
            //Instruction.Add("ProjectorVideoStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x35, 0x47 });
            //Instruction.Add("ProjectorOffStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x43, 0x55 });
            //Instruction.Add("ProjectorVGAStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x44, 0x56 });
            //Instruction.Add("ProjectorSleepStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x45, 0x57 });

            ///// <summary>
            ///// Screen and Curtain Controls
            ///// </summary>    

            //Instruction.Add("ScreenDownStrategy", new byte[] { });
            //Instruction.Add("ScreenRiseStrategy", new byte[] { });
            //Instruction.Add("ScreenStopStrategy", new byte[] { });
            //Instruction.Add("CurtainOpenStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x76, 0x88 });
            //Instruction.Add("CurtainCloseStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x56, 0x68 });
            //Instruction.Add("CurtainStopStrategy", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x66, 0x78 });

            /////<summary>
            /////power Controls
            ///// </summary>

            //Instruction.Add("ProjectorPowerOnStrategy", new byte[] { 0x8B, 0xB9, 00, 06, 05, 09, 01, 01, 01, 0x17 });
            //Instruction.Add("ComputerPowerOnStrategy",  new byte[] { 0x8B, 0xB9, 00, 06, 05, 09, 01, 02, 01, 0x18 });
            //Instruction.Add("OtherPowerOnStrategy",     new byte[] { 0x8B, 0xB9, 00, 06, 05, 09, 01, 04, 01, 0x1A });
            //Instruction.Add("AmplifierPowerOnStrategy", new byte[] { 0x8B, 0xB9, 00, 06, 05, 09, 01, 03, 01, 0x19 });
            //Instruction.Add("ProjectorPowerOffStrategy",new byte[] { 0x8B, 0xB9, 00, 06, 05, 09, 01, 01, 00, 0x16 });
            //Instruction.Add("ComputerPowerOffStrategy", new byte[] { 0x8B, 0xB9, 00, 06, 05, 09, 01, 02, 00, 0x17 });
            //Instruction.Add("OtherPowerOffStrategy",    new byte[] { 0x8B, 0xB9, 00, 06, 05, 09, 01, 04, 00, 0x19 });
            //Instruction.Add("AmplifierPowerOffStrategy",new byte[] { 0x8B, 0xB9, 00, 06, 05, 09, 01, 03, 00, 0x18 });

            /////<summary>
            /////EnvironmentControls
            ///// </summary>
            //Instruction.Add("PodiumLightOnStrategy", new byte[] { });
            //Instruction.Add("PodiumLightOffStrategy", new byte[] { });
            //Instruction.Add("ClassroomLightOnStrategy", new byte[] { });
            //Instruction.Add("ClassroomLightOffStrategy", new byte[] { });
            //Instruction.Add("PodiumCurtainOnStrategy", new byte[] { });
            //Instruction.Add("PodiumCurtainOffStrategy", new byte[] { });
            //Instruction.Add("ClassroomCurtainOnStrategy", new byte[] { });
            //Instruction.Add("ClassroomCurtainOffStrategy", new byte[] { });
            //Instruction.Add("ExhaustFanOnStrategy", new byte[] { });
            //Instruction.Add("ExhaustFanOffStrategy", new byte[] { });
            //Instruction.Add("FreshAirSystemOnStrategy", new byte[] { });
            //Instruction.Add("FreshAirSystemOffStrategy", new byte[] { });
            //Instruction.Add("Ac1OnStrategy", new byte[] { });
            //Instruction.Add("Ac1OffStrategy", new byte[] { });
            //Instruction.Add("Ac2OnStrategy", new byte[] { });
            //Instruction.Add("Ac2OffStrategy", new byte[] { });
            //Instruction.Add("Ac3OnStrategy", new byte[] { });
            //Instruction.Add("Ac3OffStrategy", new byte[] { });
            //Instruction.Add("Ac4OnStrategy", new byte[] { });
            //Instruction.Add("Ac4OffStrategy", new byte[] { });
            #endregion

            #region BaurRate
            BaudRateHex.Add(4800, "00");
            BaudRateHex.Add(9600, "01");
            BaudRateHex.Add(19200, "02");
            BaudRateHex.Add(38400, "03");
            BaudRateHex.Add(57600, "04");
            BaudRateHex.Add(76800, "05");
            BaudRateHex.Add(115200, "06");
            BaudRateHex.Add(1200, "07");
            BaudRateHex.Add(2400, "08");
            #endregion
        }

        public byte[] GetValues(string key)
        {
            return Instruction[key];
        }

        public string GetBaud(int key)
        {
            return BaudRateHex[key];
        }
    }
}
