using AsusFanControl.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsusFanControl.Service
{
    internal class AsusService
    {
        public AsusService()
        {
            AsusWinIO64.InitializeWinIo();
            fanCount = AsusWinIO64.HealthyTable_FanCounts();
            ChangeMode(FanMode.Manual);

        }

        ~AsusService()
        {
            AsusWinIO64.ShutdownWinIo();
        }

        private int fanCount;
        private FanMode mode;
        private byte currentSpeed;

        private void SetFanSpeed(byte value, byte fanIndex = 0)
        {
            if (currentSpeed == value)
                return;

            currentSpeed = value;
            AsusWinIO64.HealthyTable_SetFanIndex(fanIndex);
            AsusWinIO64.HealthyTable_SetFanPwmDuty(currentSpeed);
        }

        private void SetFanSpeeds(byte value)
        {
            for (byte fanIndex = 0; fanIndex < fanCount; fanIndex++)
            {
                SetFanSpeed(value, fanIndex);
            }
        }

        public void SetFanSpeeds(int percent)
        {
            var value = (byte)(percent / 100.0f * 255);
            SetFanSpeeds(value);
        }

        public int GetFanSpeed(byte fanIndex = 0)
        {
            AsusWinIO64.HealthyTable_SetFanIndex(fanIndex);
            var fanSpeed = AsusWinIO64.HealthyTable_FanRPM();
            return fanSpeed;
        }

        public List<int> GetFanSpeeds()
        {
            var fanSpeeds = new List<int>();

            for (byte fanIndex = 0; fanIndex < fanCount; fanIndex++)
            {
                var fanSpeed = GetFanSpeed(fanIndex);
                fanSpeeds.Add(fanSpeed);
            }

            return fanSpeeds;
        }

        public ulong Thermal_Read_Cpu_Temperature()
        {
            return AsusWinIO64.Thermal_Read_Cpu_Temperature();
        }

        public void ChangeMode(FanMode newMode)
        {
            mode = newMode;
            if (mode == FanMode.Manual)
            {
                AsusWinIO64.HealthyTable_SetFanTestMode((char)0x01);
            }
            else
            {
                AsusWinIO64.HealthyTable_SetFanTestMode((char)0x00);
            }
        }
    }
}
