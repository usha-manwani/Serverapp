using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyExecution
{
    class Instructions 
    {
        Dictionary<string, byte[]> Instruction = new Dictionary<string, byte[]>();
        
        public Instructions()
        {
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
            Instruction.Add("SystemLock", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x2c, 0x5 });
            Instruction.Add("SystemUnlock", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x2d, 0x36 });
            Instruction.Add("PodiumLock", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x2e, 0x39 });
            Instruction.Add("PodiumUnlock", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x2f, 0x40 });
            Instruction.Add("ClassLock", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x5f, 0x70 });
            Instruction.Add("ClassUnlock", new byte[] { 0x8B, 0xB9, 00, 04, 05, 02, 0x60, 0x71 });
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
            /// <summary>
            /// Power Controls
            /// </summary>
            Instruction.Add("ProjectorPowerOn", new byte[] {  });
            Instruction.Add("ProjectorPowerOff", new byte[] { });
            Instruction.Add("ComputerPowerOn", new byte[] { });
            Instruction.Add("ComputerPowerOff", new byte[] { });
            Instruction.Add("AmplifierPowerOn", new byte[] { });
            Instruction.Add("AmplifierPowerOff", new byte[] { });
            Instruction.Add("OtherPowerOn", new byte[] { });
            Instruction.Add("OtherPowerOff", new byte[] { });
        }

        public byte[] GetValues(string key)
        {
            return Instruction[key];
        }        
    }   
}
