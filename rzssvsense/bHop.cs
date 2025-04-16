using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swed64;
using System.Runtime.InteropServices;
using rzssvsense;
using System.Security.Cryptography.X509Certificates;


namespace rzssvsense
{
    public class bHop
    {
        static public void BHop()
        { 
            
            Swed swed = new Swed("cs2");

            const int SpaceBar = 0x20;

            const uint Standing = 65665;
            const uint Crouching = 65667;

            const uint PlusJump = 65537;
            const uint MinusJump = 16777472;

            IntPtr client = swed.GetModuleBase("client.dll");
            IntPtr ForceJumpAddress = client + 0x186CD60;

            while (true)
            {
                IntPtr PlayerPawnAddress = swed.ReadPointer(client, 0x1874050);
                uint fFlag = swed.ReadUInt(PlayerPawnAddress, 0x3EC);

                if (GetAsyncKeyState(SpaceBar) < 0)
                {
                    if (fFlag == Standing || fFlag == Crouching)
                    {
                        Thread.Sleep(1);
                        swed.WriteUInt(ForceJumpAddress, PlusJump);
                    }
                    else
                    {
                        swed.WriteUInt(ForceJumpAddress, MinusJump);
                    }
                }
                Thread.Sleep(5);
            }


            [DllImport("user32.dll")]
            static extern short GetAsyncKeyState(int vKey);
        }
    }
}
