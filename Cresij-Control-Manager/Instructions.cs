using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cresij_Control_Manager
{
    class Instructions 
    {
        Dictionary<string, byte[]> Instruction = new Dictionary<string, byte[]>();
        
        public Instructions()
        {
            Instruction.Add("None", new byte[] {});
            Instruction.Add("MacAddress", new byte[] { 0X8B, 0xB9, 00, 03, 05, 08, 0x10 });
            Instruction.Add("SystemOn", new byte[] { 0X8B, 0xB9, 00, 04, 05, 02, 0xC0, 0xCB });
            Instruction.Add("Status", new byte[] { 0X8B, 0xB9, 00, 03, 05, 01, 09 });
            Instruction.Add("SystemOff", new byte[] { 0X8B, 0xB9, 00, 04, 05, 02, 0xC1, 0xCC });
            Instruction.Add("ComputerOn", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x1D, 0x28 });
            Instruction.Add("ComputerOff", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x1E, 0x29 });
            Instruction.Add("IsMachineOnline", new byte[] { 0x8B, 0xB9, 00, 03, 03, 01, 07 });
            /// <summary>
            /// MediaSignals
            /// </summary>
            Instruction.Add("Desktop", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x10, 0x1B });
            Instruction.Add("Laptop", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x11, 0x1C });
            Instruction.Add("DigitalBooth", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x12, 0x1D });
            Instruction.Add("DigitalEquipment", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x13, 0x1e });
            Instruction.Add("Dvd", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x14, 0x1f });
            Instruction.Add("TV", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x15, 0x20 });
            Instruction.Add("VideoCamera", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x16, 0x21 });
            Instruction.Add("RecordingDevice", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x19, 0x24});
            Instruction.Add("BlurayDvd", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x1a, 0x25 });
            Instruction.Add("ExternalHd", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x1d, 0x28 });
            /// <summary>
            /// LockControls
            /// </summary> 
            Instruction.Add("SystemLock", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x2c, 0x37 });
            Instruction.Add("SystemUnlock", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x2d, 0x36 });
            Instruction.Add("PodiumLock ", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x2e, 0x39 });
            Instruction.Add("PodiumUnlock", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x2f, 0x3A });
            Instruction.Add("ClassLock", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x5f, 0x70 });
            Instruction.Add("ClassUnlock", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x60, 0x6B });
            /// <summary>
            /// SoundControls
            /// </summary>
            Instruction.Add("GeneralVolumeDown", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x21, 0x2C });
            Instruction.Add("GeneralVolumeUp", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x20, 0x2B });
            Instruction.Add("GeneralVolumeMute", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x22, 0x2D });
            Instruction.Add("WiredMicDown", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x24, 0x2F });
            Instruction.Add("WiredMicUp", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x23, 0x2E });
            Instruction.Add("WiredMicMute", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x25, 0x30 });
            Instruction.Add("WirelessMicDown", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x74, 0x7f });
            Instruction.Add("WirelessMicUp", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x73, 0x7e });
            Instruction.Add("WirelessMicMute", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x75, 0x80 });
            /// <summary>
            /// Projector Controls
            /// </summary>
            Instruction.Add("ProjectorOn", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x33, 0x3e });
            Instruction.Add("ProjectorHDMI", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x34, 0x3f });
            Instruction.Add("ProjectorVideo", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x35, 0x40 });
            Instruction.Add("ProjectorOff", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x43, 0x4e });
            Instruction.Add("ProjectorVGA", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x44, 0x4f });
            Instruction.Add("ProjectorSleep", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x45, 0x50 });

            /// <summary>
            /// Screen and Curtain Controls
            /// </summary>    
            
            Instruction.Add("ScreenDown", new byte[] {  });
            Instruction.Add("ScreenRise", new byte[] {  });
            Instruction.Add("ScreenStop", new byte[] { });
            Instruction.Add("CurtainOpen", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x56, 0x61 });
            Instruction.Add("CurtainClose", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x76, 0x81 });
            Instruction.Add("CurtainStop", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x66, 0x71 });

            ///<summary>
            ///power Controls
            /// </summary>

            Instruction.Add("ProjectorPowerOn", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 01, 01, 0x15});
            Instruction.Add("ComputerPowerOn", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01,02,01, 0x16});
            Instruction.Add("OtherPowerOn", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 04, 01, 0x18 });
            Instruction.Add("AmplifierPowerOn", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 03, 01, 0x17 });
            Instruction.Add("ProjectorPowerOff", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 01, 00, 0x14 });
            Instruction.Add("ComputerPowerOff", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 02, 00, 0x15 });
            Instruction.Add("OtherPowerOff", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 04, 00, 0x17 });
            Instruction.Add("AmplifierPowerOff", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 03, 00, 0x16 });

            #region Strategy Instruction
            /// <summary>
            /// Instruction set for Strategy
            /// </summary>   

            Instruction.Add("SystemOnS", new byte[] { 0X8B, 0xB9, 00, 04, 05, 09, 0xC0, 0xD2 });
           
            Instruction.Add("StatusS", new byte[] { 0X8B, 0xB9, 00, 03, 05, 01, 09 });
            Instruction.Add("SystemOffS", new byte[] { 0X8B, 0xB9, 00, 04, 05, 09, 0xC1, 0xD3 });
            Instruction.Add("ComputerOnS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x1D, 0x2F });
            Instruction.Add("ComputerOffS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x1E, 0x30 });
            Instruction.Add("IsMachineOnlineS", new byte[] { 0x8B, 0xB9, 00, 03, 03, 01, 07 });
            /// <summary>
            /// MediaSignals
            /// </summary>
            Instruction.Add("DesktopS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x10, 0x22 });
            Instruction.Add("LaptopS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x11, 0x23 });
            Instruction.Add("DigitalBoothS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x12, 0x24 });
            Instruction.Add("DigitalEquipmentS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x13, 0x25 });
            Instruction.Add("DvdS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x14, 0x26 });
            Instruction.Add("TVS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x15, 0x27 });
            Instruction.Add("VideoCameraS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x16, 0x28 });
            Instruction.Add("RecordingDeviceS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x19, 0x2B });
            Instruction.Add("BlurayDvdS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x1a, 0x2C });
            Instruction.Add("ExternalHdS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x1d, 0x2F });
            /// <summary>
            /// LockControls
            /// </summary> 
            Instruction.Add("SystemLockS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x2c, 0x3E });
            Instruction.Add("SystemUnlockS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x2d, 0x3F });
            Instruction.Add("PodiumLockS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x2e, 0x40 });
            Instruction.Add("PodiumUnlockS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x2f, 0x41 });
            Instruction.Add("ClassLockS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x5f, 0x71 });
            Instruction.Add("ClassUnlockS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x60, 0x72 });
            /// <summary>
            /// SoundControls
            /// </summary>
            Instruction.Add("GeneralVolumeDownS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x21, 0x33 });
            Instruction.Add("GeneralVolumeUpS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x20, 0x32 });
            Instruction.Add("GeneralVolumeMuteS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x22, 0x34 });
            Instruction.Add("WiredMicDownS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x24, 0x35 });
            Instruction.Add("WiredMicUpS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x23, 0x36 });
            Instruction.Add("WiredMicMuteS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x25, 0x37 });
            Instruction.Add("WirelessMicDownS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x74, 0x86 });
            Instruction.Add("WirelessMicUpS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x73, 0x85 });
            Instruction.Add("WirelessMicMuteS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x75, 0x87 });
            /// <summary>
            /// Projector Controls
            /// </summary>
            Instruction.Add("ProjectorOnS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x33, 0x45 });
            Instruction.Add("ProjectorHDMIS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x34, 0x46 });
            Instruction.Add("ProjectorVideoS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x35, 0x47 });
            Instruction.Add("ProjectorOffS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x43, 0x55 });
            Instruction.Add("ProjectorVGAS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x44, 0x56 });
            Instruction.Add("ProjectorSleepS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x45, 0x57 });

            /// <summary>
            /// Screen and Curtain Controls
            /// </summary>    

            Instruction.Add("ScreenDownS", new byte[] { });
            Instruction.Add("ScreenRiseS", new byte[] { });
            Instruction.Add("ScreenStopS", new byte[] { });
            Instruction.Add("CurtainOpenS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x76, 0x88 });
            Instruction.Add("CurtainCloseS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x56, 0x68 });
            Instruction.Add("CurtainStopS", new byte[] { 0x8B, 0xB9, 00, 04, 05, 09, 0x66, 0x78 });

            ///<summary>
            ///power Controls
            /// </summary>

            Instruction.Add("ProjectorPowerOnS", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 01, 01, 0x15 });
            Instruction.Add("ComputerPowerOnS", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 02, 01, 0x16 });
            Instruction.Add("OtherPowerOnS", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 04, 01, 0x18 });
            Instruction.Add("AmplifierPowerOnS", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 03, 01, 0x17 });
            Instruction.Add("ProjectorPowerOffS", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 01, 00, 0x14 });
            Instruction.Add("ComputerPowerOffS", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 02, 00, 0x15 });
            Instruction.Add("OtherPowerOffS", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 04, 00, 0x17 });
            Instruction.Add("AmplifierPowerOffS", new byte[] { 0x8B, 0xB9, 00, 06, 05, 07, 01, 03, 00, 0x16 });

            ///<summary>
            ///EnvironmentControls
            /// </summary>
            Instruction.Add("PodiumLightOnS", new byte[] { });
            Instruction.Add("PodiumLightOffS", new byte[] { });
            Instruction.Add("ClassroomLightOnS", new byte[] { });
            Instruction.Add("ClassroomLightOffS", new byte[] { });
            Instruction.Add("PodiumCurtainOnS", new byte[] { });
            Instruction.Add("PodiumCurtainOffS", new byte[] { });
            Instruction.Add("ClassroomCurtainOnS", new byte[] { });
            Instruction.Add("ClassroomCurtainOffS", new byte[] { });
            Instruction.Add("ExhaustFanOnS", new byte[] { });
            Instruction.Add("ExhaustFanOffS", new byte[] { });
            Instruction.Add("FreshAirSystemOnS", new byte[] { });
            Instruction.Add("FreshAirSystemOffS", new byte[] { });
            Instruction.Add("Ac1OnS", new byte[] { });
            Instruction.Add("Ac1OffS", new byte[] { });
            Instruction.Add("Ac2OnS", new byte[] { });
            Instruction.Add("Ac2OffS", new byte[] { });
            Instruction.Add("Ac3OnS", new byte[] { });
            Instruction.Add("Ac3OffS", new byte[] { });
            Instruction.Add("Ac4OnS", new byte[] { });
            Instruction.Add("Ac4OffS", new byte[] { });
            #endregion
        }

        public byte[] GetValues(string key)
        {
            return Instruction[key];
        }
        
    }
  
}
